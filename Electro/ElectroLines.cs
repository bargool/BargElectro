﻿//Microsoft
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Autodesk
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Runtime;
using acad = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(BargElectro.ElectroLines))]

namespace BargElectro
{
	public class ElectroLines
	{
		const string AppRecordKey = "BargElectroLinesGroup";
		Document dwg;
		Database CurrentDatabase;
		
		public ElectroLines()
		{
			dwg = acad.DocumentManager.MdiActiveDocument;
			CurrentDatabase = dwg.Database;
		}
		
		/// <summary>
		/// Команда добавляет к линейным примитивам XRecord с именем группы
		/// </summary>
		[CommandMethod("BEAdd")]
		public void AddLinesToGroup()
		{
			Editor editor = dwg.Editor;
			using (Transaction acadTrans = CurrentDatabase.TransactionManager.StartTransaction())
			{
				GroupsInformation groupsEntities = new GroupsInformation(acadTrans, CurrentDatabase);
				//Спрашиваем имя группы
				string GroupName = AskForGroup(true, groupsEntities.GroupList);
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
		[CommandMethod("BELen")]
		public void GetGroupLengths()
			//FIXME: Падает если удалить группы, а потом обратиться к ним. Если сделать audit - всё нормально.
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

		/// <summary>
		/// Команда запрашивает имя группы и выделяет примитивы, принадлежащие данной группе
		/// </summary>
		[CommandMethod("BESel")]
		public void SelectGroup()
		{
			Editor editor = dwg.Editor;
			using (Transaction transaction = CurrentDatabase.TransactionManager.StartTransaction())
			{
				GroupsInformation groupsEntities = new GroupsInformation(transaction, CurrentDatabase);
				string group = AskForGroup(false, groupsEntities.GroupList);
				if (group != null)
				{
					editor.SetImpliedSelection(groupsEntities.GetObjectsOfGroup(group).ToArray());
				}
			}
		}
		
		/// <summary>
		/// Метод выводит запрос на имя группы, существующие группы выводятся в качетсве ключевых слов
		/// </summary>
		/// <param name="CanAdd">запрос существующей группы, или новой</param>
		/// <param name="groups">список существующих групп</param>
		/// <returns>имя введённой группы</returns>
		string AskForGroup(bool CanAdd, List<string> groups)
		{
			if (CanAdd)
			{
				BargElectro.Windows.ListAddGroupsWindow win =
					new BargElectro.Windows.ListAddGroupsWindow(groups);
				if (true == win.ShowDialog())
				{
					return win.Group;
				}
			}
			else
			{
				BargElectro.Windows.ListSelectGroupsWindow win =
					new BargElectro.Windows.ListSelectGroupsWindow(groups);
				if (true == win.ShowDialog())
				{
					return win.Group;
				}
			}
			return null;
		}
		
		/// <summary>
		/// Команда удаляет информацию об указанной группе из примитива
		/// </summary>
		[CommandMethod("BEDel", CommandFlags.UsePickSet)]
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
					string groupName = AskForGroup(false, groupList);
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
		
		/// <summary>
		/// Команда заменяет (переименовывает группу) информацию о группе с одной на другую, если в примитиве присутсвует
		/// как первая, так и вторая группа - первая группа удаляется
		/// </summary>
		[CommandMethod("BEChGr", CommandFlags.UsePickSet)]
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
					string previousName = AskForGroup(false, groupList);
					if (previousName!=null)
					{
						string newName = AskForGroup(true, groupEntities.GroupList);
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
		
		/// <summary>
		/// Команда выводит в командную строку список групп выделенных примитивов
		/// </summary>
		[CommandMethod("BEGet")]
		public void GetGroupsOfObject()
		{
			Editor editor = dwg.Editor;
			List<string> groupList = null;
			PromptEntityOptions selectionOptions = new PromptEntityOptions("\nВыберите объект");
			selectionOptions.AllowNone = false;
			PromptEntityResult selectionResult = editor.GetEntity(selectionOptions);
			if (selectionResult.Status == PromptStatus.OK)
			{
				using (Transaction transaction = CurrentDatabase.TransactionManager.StartTransaction())
				{
					GroupsInformation groupEntities = new GroupsInformation(transaction, CurrentDatabase);
					groupList = groupEntities.GetGroupsOfObject(selectionResult.ObjectId);
				}
				if (groupList!=null)
				{
					editor.WriteMessage("\nГруппы, к которым принадлежат объекты: ");
					foreach (string group in groupList)
						editor.WriteMessage("\n{0}", group);	
				}
				else
				{
					editor.WriteMessage("\nОбъект не принадлежит никаким группам!");
				}
			}
		}
		
		[CommandMethod("BERen")]
		public void RenameGroup()
		{
			Editor ed = dwg.Editor;
			List<string> groupList = null;
			using (Transaction transaction = CurrentDatabase.TransactionManager.StartTransaction())
			{
				GroupsInformation groupEntities = new GroupsInformation(transaction, CurrentDatabase);
				groupList = groupEntities.GroupList;
				if (groupList.Count == 0)
				{
					ed.WriteMessage("\nВ чертеже нет групп!");
					return;
				}
				ed.WriteMessage("\nПереименовываем группу:");
				string oldGroupName = AskForGroup(false, groupList);
				ed.WriteMessage(oldGroupName);
				string newGroupName = AskForGroup(true, groupList);
				ed.WriteMessage(newGroupName);
				foreach (GroupObject go in groupEntities)
				{
					go.ChangeGroup(oldGroupName, newGroupName);
					go.WriteGroups();
				}
				transaction.Commit();
			}
		}
		[CommandMethod("BEDelAll")]
		public void DeleteAllGroupInformation()
		{
			Editor ed = dwg.Editor;
			PromptKeywordOptions pkOpts = new PromptKeywordOptions(
				"\nВы собираетесь удалить всю информацию о группах с чертежа. Точно? [Yes/No]", "Yes No");
			pkOpts.AllowArbitraryInput = false;
			pkOpts.AllowNone = false;
			PromptResult res = ed.GetKeywords(pkOpts);
			switch (res.StringResult)
			{
				case "Yes":
					ed.WriteMessage("\nВперёд!!!");
					using (Transaction tr = CurrentDatabase.TransactionManager.StartTransaction())
					{
						GroupsInformation groupEntities = new GroupsInformation(tr, CurrentDatabase);
						foreach (string group in groupEntities.GroupList)
						{
							foreach (ObjectId id in groupEntities.GetObjectsOfGroup(group))
							{
								groupEntities.DeleteGroupFromObject(id, group);
							}
						}
						tr.Commit();
					}
					break;
				case "No":
					ed.WriteMessage("\nФфух!");
					break;
			}
		}
		
		[CommandMethod("BELeader")]
		public void DrawGroupLeader()
		{
			Editor ed = dwg.Editor;
			PromptEntityOptions prmtEntityOpts = new PromptEntityOptions("Укажите линию");
			prmtEntityOpts.AllowNone = false;
			prmtEntityOpts.SetRejectMessage("Должна быть линия или полилиния!");
			prmtEntityOpts.AddAllowedClass(typeof(Line), true);
			prmtEntityOpts.AddAllowedClass(typeof(Polyline), true);
			PromptEntityResult entRes = ed.GetEntity(prmtEntityOpts);
			if (entRes.Status!= PromptStatus.OK)
			{
				return;
			}
			using (Transaction tr = CurrentDatabase.TransactionManager.StartTransaction())
			{
				GroupsInformation groupEntities = new GroupsInformation(tr, CurrentDatabase);
				List<string> groupList = groupEntities.GetGroupsOfObject(entRes.ObjectId);
				if (groupList == null)
				{
					ed.WriteMessage("За указанным объектом не значится никаких групп!");
					return;
				}
				PromptPointOptions pointOpts = new PromptPointOptions("\nУкажите точку вставки блока: ");
				PromptPointResult pointRes = ed.GetPoint(pointOpts);
				if (pointRes.Status!= PromptStatus.OK)
				{
					return;
				}
				BlockTable bt = (BlockTable)CurrentDatabase.BlockTableId.GetObject(OpenMode.ForRead);
				BlockTableRecord btrSpace = (BlockTableRecord)CurrentDatabase.CurrentSpaceId
					.GetObject(OpenMode.ForWrite);
				if (!bt.Has("group_vinoska"))
				{
				    ed.WriteMessage("\nВ файле не определён блок выноски!!");
				    return;
				}
				BlockTableRecord gleaderBtr = (BlockTableRecord)bt["group_vinoska"].GetObject(OpenMode.ForRead);
				BlockReference gleader = new BlockReference(pointRes.Value, gleaderBtr.ObjectId);
				btrSpace.AppendEntity(gleader);
				tr.AddNewlyCreatedDBObject(gleader, true);
				
				//Если блок аннотативный - добавляем в таблицу аннотативных масштабов блока текущий масштаб
				ObjectContextManager ocm = CurrentDatabase.ObjectContextManager;
				ObjectContextCollection occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
				if (gleaderBtr.Annotative == AnnotativeStates.True)
				{
					ObjectContexts.AddContext(gleader, occ.CurrentContext);
				}
				
				gleader.SetDatabaseDefaults();
				if (gleaderBtr.HasAttributeDefinitions)
				{
					var attDefs = gleaderBtr.Cast<ObjectId>()
						.Where(n => n.ObjectClass.Name == "AcDbAttributeDefinition")
						.Select(n => (AttributeDefinition)n.GetObject(OpenMode.ForRead));
					foreach (AttributeDefinition attdef in attDefs)
					{
						AttributeReference attref = new AttributeReference();
						attref.SetAttributeFromBlock(attdef, gleader.BlockTransform);
						gleader.AttributeCollection.AppendAttribute(attref);
						tr.AddNewlyCreatedDBObject(attref, true);
						if (gleaderBtr.Annotative == AnnotativeStates.True)
						{
							ObjectContexts.AddContext(attref, occ.CurrentContext);
						}
						int attCount = int.Parse(attref.Tag.Remove(0,10));
						if (attCount<=groupList.Count)
						{
							attref.TextString = groupList[attCount-1];
						}
					}
				}
				
				if (gleaderBtr.IsDynamicBlock)
				{
					DynamicBlockReferencePropertyCollection dynBRefColl = gleader.DynamicBlockReferencePropertyCollection;
					foreach (DynamicBlockReferenceProperty prop in dynBRefColl)
					{
						if (prop.PropertyName == "Lookup1")
						{
							prop.Value = prop.GetAllowedValues()[groupList.Count-1];
						}
					}
				}
				tr.Commit();
			}
		}
		
		[CommandMethod("BELongest")]
		public void GetLongestSegment()
		{
			Editor ed = dwg.Editor;
			PromptPointResult res = ed.GetPoint("Укажите корневую точку");
			if (res.Status!= PromptStatus.OK)
			{
				return;
			}
			using (Transaction tr = CurrentDatabase.TransactionManager.StartTransaction())
			{
				GroupsInformation groupsEntities = new GroupsInformation(tr, CurrentDatabase);
				string group = AskForGroup(false, groupsEntities.GroupList);
				if (group == null)
				{
					return;
				}
				
				List<ObjectId> groupLines = groupsEntities.GetObjectsOfGroup(group);
				var plineSegments = groupLines
					.Where(n => n.GetObject(OpenMode.ForRead) is Polyline)
					.SelectMany(n => {
					            	DBObjectCollection coll = new DBObjectCollection();
					            	((Polyline)n.GetObject(OpenMode.ForRead)).Explode(coll);
					            	return coll.Cast<DBObject>();
					            })
					.Concat(
						groupLines
						.Where(n => n.GetObject(OpenMode.ForRead) is Line)
						.Select(n => (DBObject)n.GetObject(OpenMode.ForRead)));
				Line[] lines = (from DBObject l in plineSegments
					where l is Line
					select l as Line).ToArray();
				DepthFirstSearch.GraphTree tree = new DepthFirstSearch.GraphTree(res.Value, lines);
				ed.WriteMessage("\nСамый длинный участок - {0}",tree.FarestNode.PathCostFromRoot);
			}
		}
	}
}