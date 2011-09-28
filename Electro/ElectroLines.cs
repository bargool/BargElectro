//Microsoft
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Autodesk
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

[assembly: CommandClass(typeof(BargElectro.ElectroLines))]

namespace BargElectro
{
	public class ElectroLines
	{
		//TODO: Добавить возможность клика по линии с целью узнать, к каким линиям принадлежит
		//TODO: Переименование групп. Вначале замена одного имени на другое, затем вопрос, добавлять ли имя, если его не было?
		const string AppRecordKey = "BargElectroLinesGroup";
		Document dwg;
		Database CurrentDatabase;
		
		public ElectroLines()
		{
			dwg = Application.DocumentManager.MdiActiveDocument;
			CurrentDatabase = dwg.Database;
		}
		
		/// <summary>
		/// Метод добавляет к линейным примитивам XRecord с именем группы
		/// </summary>
		[CommandMethod("AddLinesToGroup")]
		public void AddLinesToGroup()
		{
			Editor editor = dwg.Editor;
			using (Transaction acadTrans = CurrentDatabase.TransactionManager.StartTransaction())
			{
				GroupsInformation groupsEntities = new GroupsInformation(acadTrans, CurrentDatabase);
				//Спрашиваем имя группы (если уже есть группы - выводим как опции запроса)
				string GroupName = AskForGroup(false, groupsEntities.GroupList);
				if (GroupName != null)
				{
					// Готовим опции для запроса элементов группы
					PromptSelectionOptions acadSelectionOptions = new PromptSelectionOptions();
					acadSelectionOptions.MessageForAdding = "\nУкажите объекты группы " + GroupName;
					//Выделять будем только линии и полилинии. Создаем фильтр
					TypedValue[] acadFilterValues = new TypedValue[4];
					acadFilterValues.SetValue(new TypedValue((int)DxfCode.Operator, "<OR"),0);
					acadFilterValues.SetValue(new TypedValue((int)DxfCode.Start, "LINE"),1);
					acadFilterValues.SetValue(new TypedValue((int)DxfCode.Start, "LWPOLYLINE"),2);
					acadFilterValues.SetValue(new TypedValue((int)DxfCode.Operator, "OR>"),3);
					SelectionFilter acadSelFilter = new SelectionFilter(acadFilterValues);
					//Используем фильтр для выделения
					PromptSelectionResult acadSelSetPrompt = dwg.Editor.GetSelection(acadSelectionOptions, acadSelFilter);
					//Если выбраны объекты - едем дальше
					if (acadSelSetPrompt.Status == PromptStatus.OK)
					{
						// Формируем коллекцию выделенных объектов
						SelectionSet acadSelectedObjects = acadSelSetPrompt.Value;
						// Проходим по каждому объекту в выделении
						foreach (SelectedObject selectedObj in acadSelectedObjects)
						{
							if (selectedObj!=null)
							{
								groupsEntities.AppendGroupToObject(selectedObj.ObjectId, GroupName);
							}
						}
					}
				}
				acadTrans.Commit();
			}
		}
		
		/// <summary>
		/// Команда выводит список групп и суммарную длину в плане
		/// </summary>
		[CommandMethod("GetLinesroup")]
		public void GetLinesGroup()
		{
			//SortedDictionary, в котором будут храниться номера групп и длины
			SortedDictionary<string, double> GroupLenghts = new SortedDictionary<string, double>();
			using (Transaction acadTrans = CurrentDatabase.TransactionManager.StartTransaction())
			{
				GroupsInformation groupsEntities = new GroupsInformation(acadTrans, CurrentDatabase);
				// Итерируем по группам и объектам групп, получаем длины объектов и записываем в словарь длин
				foreach (string group in groupsEntities.GroupList)
				{
					foreach (ObjectId objectid in groupsEntities.GetObjectsOfGroup(group))
					{
						if (objectid != null)
						{
							Curve entity = acadTrans.GetObject(objectid, OpenMode.ForRead) as Curve;
							if (entity!=null)
							{
								if (GroupLenghts.ContainsKey(group))
								{
									GroupLenghts[group] += entity.GetDistanceAtParameter(entity.EndParam);
								}
								else
								{
									GroupLenghts.Add(group, entity.GetDistanceAtParameter(entity.EndParam));
								}
							}
						}
					}
				}
			}
			// Выводим на консоль список длин с их суммарными длинами
			foreach (KeyValuePair<string, double> kvp in GroupLenghts)
			{
				dwg.Editor.WriteMessage("\nГруппа {0}, длина: {1}", kvp.Key, kvp.Value);
			}
		}

		[CommandMethod("SelectGroup")]
		public void SelectGroup()
			// FIXME: проверка на наличие групп в чертеже (падает, если задать несуществующую)
		{
			Editor editor = dwg.Editor;
//			SortedDictionary<string, List<ObjectId>> groups = FindGroups();
			using (Transaction transaction = CurrentDatabase.TransactionManager.StartTransaction())
			{
				GroupsInformation groupsEntities = new GroupsInformation(transaction, CurrentDatabase);
				string group = AskForGroup(true, groupsEntities.GroupList);
				if (group != null)
				{
					editor.SetImpliedSelection(groupsEntities.GetObjectsOfGroup(group).ToArray());
				}
			}
		}
		
		/// <summary>
		/// Метод выводит запрос на имя группы, существующие группы выводятся в качетсве ключевых слов
		/// </summary>
		/// <param name="existing">запрос существующей группы, или новой</param>
		/// <param name="groups">список существующих групп</param>
		/// <returns>имя введённой группы</returns>
		string AskForGroup(bool existing, List<string> groups)
		{
			PromptKeywordOptions prmptKeywordOpt = new PromptKeywordOptions("\nВыберите группу: ");
			foreach (string group in groups)
			{
				prmptKeywordOpt.Keywords.Add(group);
			}
			prmptKeywordOpt.AllowNone = false;
			if (groups.Count != 0)
			{
				prmptKeywordOpt.Keywords.Default = groups[0];
			}
			if (existing)
			{
				prmptKeywordOpt.AllowArbitraryInput = false;
			}
			else
			{
				prmptKeywordOpt.Message = "\nВведите наименование группы: ";
				prmptKeywordOpt.AllowArbitraryInput = true;
			}
			PromptResult result = dwg.Editor.GetKeywords(prmptKeywordOpt);
			if (result.Status == PromptStatus.OK)
			{
				if (result.StringResult == "")
				{
					dwg.Editor.WriteMessage("\nНеобходимо ввести наименование группы!");
					return null;
				}
				return result.StringResult;
			}
			else
			{
				return null;
			}
		}
		
		[CommandMethod("DeleteGroupFromLine", CommandFlags.UsePickSet)]
		public void DeleteGroupFromLine()
		{
			Editor editor = dwg.Editor;
			PromptSelectionResult selectionResult = editor.SelectImplied();
			List<string> groupList = new List<string>();
			if (selectionResult.Status != PromptStatus.OK)
			{
				PromptSelectionOptions selectionOptions = new PromptSelectionOptions();
				selectionOptions.MessageForAdding = "\nУкажите объекты";
				selectionResult = editor.GetSelection(selectionOptions);
			}
			if (selectionResult.Status == PromptStatus.OK)
			{
				using (Transaction transaction = CurrentDatabase.TransactionManager.StartTransaction())
				{
					SelectionSet selectionSet = selectionResult.Value;
					GroupsInformation groupEntities = new GroupsInformation(transaction, CurrentDatabase);
					foreach (SelectedObject selectedObject in selectionSet)
					{
						foreach (string group in groupEntities.GetGroupsOfObject(selectedObject.ObjectId))
						{
							if (!groupList.Contains(group))
							{
								groupList.Add(group);
							}
						}
					}
					groupList.Sort();
					string groupName = AskForGroup(true, groupList);
					if (groupName!=null)
					{
						foreach (SelectedObject selectedObject in selectionSet)
						{
							groupEntities.DeleteGroupFromObject(selectedObject.ObjectId, groupName);
						}
					}
					transaction.Commit();
				}
			}
		}
		
		[CommandMethod("ChangeGroupOfLines", CommandFlags.UsePickSet)]
		public void ChangeGroupOfLines()
		{
			Editor editor = dwg.Editor;
			PromptSelectionResult selectionResult = editor.SelectImplied();
			List<string> groupList = new List<string>();
			if (selectionResult.Status != PromptStatus.OK)
			{
				PromptSelectionOptions selectionOptions = new PromptSelectionOptions();
				selectionOptions.MessageForAdding = "\nУкажите объекты";
				selectionResult = editor.GetSelection(selectionOptions);
			}
			if (selectionResult.Status == PromptStatus.OK)
			{
				using (Transaction transaction = CurrentDatabase.TransactionManager.StartTransaction())
				{
					SelectionSet selectionSet = selectionResult.Value;
					GroupsInformation groupEntities = new GroupsInformation(transaction, CurrentDatabase);
					foreach (SelectedObject selectedObject in selectionSet)
					{
						foreach (string group in groupEntities.GetGroupsOfObject(selectedObject.ObjectId))
						{
							if (!groupList.Contains(group))
							{
								groupList.Add(group);
							}
						}
					}
					groupList.Sort();
					string previousName = AskForGroup(true, groupList);
					if (previousName!=null)
					{
						string newName = AskForGroup(false, groupEntities.GroupList);
						if (newName!=null)
						{
							foreach (SelectedObject selectedObject in selectionSet)
							{
								groupEntities.RenameGroupOfObject(selectedObject.ObjectId, previousName, newName);
							}
						}
					}
					transaction.Commit();
				}
			}
		}
		[CommandMethod("GetGroupsOfObject", CommandFlags.UsePickSet)]
		public void GetGroupsOfObject()
		{
			Editor editor = dwg.Editor;
			PromptSelectionResult selectionResult = editor.SelectImplied();
			List<string> groupList = new List<string>();
			if (selectionResult.Status != PromptStatus.OK)
			{
				PromptSelectionOptions selectionOptions = new PromptSelectionOptions();
				selectionOptions.MessageForAdding = "\nУкажите объекты";
				selectionResult = editor.GetSelection(selectionOptions);
			}
			if (selectionResult.Status == PromptStatus.OK)
			{
				using (Transaction transaction = CurrentDatabase.TransactionManager.StartTransaction())
				{
					SelectionSet selectionSet = selectionResult.Value;
					GroupsInformation groupEntities = new GroupsInformation(transaction, CurrentDatabase);
					foreach (SelectedObject selectedObject in selectionSet)
					{
						foreach (string group in groupEntities.GetGroupsOfObject(selectedObject.ObjectId))
						{
							if (!groupList.Contains(group))
							{
								groupList.Add(group);
							}
						}
					}
					groupList.Sort();
				}
				editor.WriteMessage("\nГруппы, к которым принадлежат объекты: ");
				foreach (string group in groupList)
				{
					editor.WriteMessage("\n{0}", group);	
				}
			}
		}
	}
}