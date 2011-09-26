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
		const string AppRecordKey = "BargElectroLinesGroup";
		Document acadDocument;
		Database acadCurDb;
		
		public ElectroLines()
		{
			acadDocument = Application.DocumentManager.MdiActiveDocument;
			acadCurDb = acadDocument.Database;
		}
		
		/// <summary>
		/// Метод добавляет к линейным примитивам XRecord с именем группы
		/// </summary>
		[CommandMethod("AddLinesToGroup")]
		public void AddLinesToGroup()
		{
			Editor editor = acadDocument.Editor;
			//Спрашиваем имя группы (если уже есть группы - выводим как опции запроса)
			string GroupName = AskForGroup(false, FindGroups().Keys.ToList());
			if (GroupName != null)
			{
				using (Transaction acadTrans = acadCurDb.TransactionManager.StartTransaction())
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
					PromptSelectionResult acadSelSetPrompt = acadDocument.Editor.GetSelection(acadSelectionOptions, acadSelFilter);
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
								Entity entity = acadTrans.GetObject(selectedObj.ObjectId, OpenMode.ForWrite) as Entity;
								if (entity!=null)
								{
									// Проверяем, есть ли у объекта словарь? Если нет - создаём новый
									if (entity.ExtensionDictionary==ObjectId.Null)
									{
										entity.CreateExtensionDictionary();
									}
									using (DBDictionary dict = acadTrans.GetObject(
										entity.ExtensionDictionary, OpenMode.ForWrite, false) as DBDictionary)
									{
										//Готовим данные с именем группы для записи в XRecord
										TypedValue data = new TypedValue((int)DxfCode.Text, GroupName);
										ResultBuffer buffer = new ResultBuffer(data);
										//Проверяем, есть ли запись словаря, закреплённая (мной) за плагином
										if (dict.Contains(AppRecordKey))
										{
											// Если запись уже есть - получаем XRecord, и добавляем туда новую группу
											// (проверяем, естественно, вдруг такая уже есть)
											Xrecord xrecord = acadTrans.GetObject(
												dict.GetAt(AppRecordKey), OpenMode.ForWrite) as Xrecord;
											if (!xrecord.Data.AsArray().Contains(data))
											{
												buffer = xrecord.Data;
												buffer.Add(data);
												xrecord.Data = buffer;
											}
										}
										else
										{
											// Словаря нет - создаем запись словаря и XRecord
											Xrecord xrecord = new Xrecord();
											xrecord.Data = buffer;
											dict.SetAt(AppRecordKey, xrecord);
											acadTrans.AddNewlyCreatedDBObject(xrecord, true);
										}
									}
								}
							}
						}
					}
					acadTrans.Commit();
				}
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
			// Словарь для хранения групп и примитивов, принадлежащих им
			SortedDictionary<string, List<ObjectId>> Groups = FindGroups();
			using (Transaction acadTrans = acadCurDb.TransactionManager.StartTransaction())
			{
				// Итерируем по группам и объектам групп, получаем длины объектов и записываем в словарь длин
				foreach (KeyValuePair<string, List<ObjectId>> kvp in Groups)
				{
					foreach (ObjectId objectid in kvp.Value)
					{
						if (objectid != null)
						{
							Curve entity = acadTrans.GetObject(objectid, OpenMode.ForRead) as Curve;
							if (entity!=null)
							{
								if (GroupLenghts.ContainsKey(kvp.Key))
								{
									GroupLenghts[kvp.Key] += entity.GetDistanceAtParameter(entity.EndParam);
								}
								else
								{
									GroupLenghts.Add(kvp.Key, entity.GetDistanceAtParameter(entity.EndParam));
								}
							}
						}
					}
				}
			}
			// Выводим на консоль список длин с их суммарными длинами
			foreach (KeyValuePair<string, double> kvp in GroupLenghts)
			{
				acadDocument.Editor.WriteMessage("\nГруппа {0}, длина: {1}", kvp.Key, kvp.Value);
			}
		}

		/// <summary>
		/// Возвращает XRecord в словаре recordKey объекта в режиме для чтения
		/// </summary>
		/// <param name="recordKey">Ключ словаря, который должен содержать XRecord</param>
		/// <param name="objectid">ObjectID объекта, у которого мы получаем XRecord</param>
		/// <param name="transaction">Используемая транзакция</param>
		/// <returns>XRecord в режиме ForRead</returns>
		Xrecord getXrecord(string recordKey, ObjectId objectid, Transaction transaction)
		{
			// Получаем входной объект как Entity
			Entity entity = transaction.GetObject(objectid, OpenMode.ForRead) as Entity;
			if (entity != null)
			{
				// Проверяем, есть ли словарь у объекта
				if (entity.ExtensionDictionary != ObjectId.Null)
				{
					// 
					using (DBDictionary dict = transaction.GetObject(
						entity.ExtensionDictionary, OpenMode.ForRead) as DBDictionary)
					{
						if (dict.Contains(recordKey))
						{
							return transaction.GetObject(dict.GetAt(recordKey), OpenMode.ForRead) as Xrecord;
						}
					}
				}
			}
			return null;
		}
		
		[CommandMethod("SelectGroup")]
		public void SelectGroup()
		{
			Editor editor = acadDocument.Editor;
			SortedDictionary<string, List<ObjectId>> groups = FindGroups();
			string group = AskForGroup(true, groups.Keys.ToList());
			if (group != null)
			{
				editor.SetImpliedSelection(groups[group].ToArray());
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
			if (groups.Count == 0)
			{
				prmptKeywordOpt.Keywords.Add("Гр.1");
			}
			else
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
			PromptResult result = acadDocument.Editor.GetKeywords(prmptKeywordOpt);
			if (result.Status == PromptStatus.OK)
			{
				if (result.StringResult == "")
				{
					acadDocument.Editor.WriteMessage("\nНеобходимо ввести наименование группы!");
					return null;
				}
				return result.StringResult;
			}
			else
			{
				return null;
			}
		}
		
		/// <summary>
		/// Метод ищет по всем объектам чертежа информацию по группам
		/// </summary>
		/// <returns>Сортированный словарь, содержащий список групп, и списки ObjectId, относящихся к ним объектов</returns>
		SortedDictionary<string, List<ObjectId>> FindGroups()
		{
			SortedDictionary<string, List<ObjectId>> groups = new SortedDictionary<string, List<ObjectId>>();
			using (Transaction transaction = acadCurDb.TransactionManager.StartTransaction())
			{
				BlockTableRecord btr = (BlockTableRecord)transaction.GetObject(acadCurDb.CurrentSpaceId, OpenMode.ForRead);
				var entities = from ObjectId entity in btr 
					where transaction.GetObject(entity, OpenMode.ForRead) is Entity 
					select entity;
				foreach (ObjectId entity in entities)
				{
					Xrecord record = getXrecord(AppRecordKey, entity, transaction);
					if (record != null)
					{
						ResultBuffer buffer = record.Data;
						//Проходим по каждому значению в XRecord
						foreach (TypedValue recordValue in buffer)
						{
							//Проверяем, была ли у нас такая группа?
							if (groups.ContainsKey(recordValue.Value.ToString()))
							{
								groups[recordValue.Value.ToString()].Add(entity);
							}
							else
							{
								groups.Add(recordValue.Value.ToString(), new List<ObjectId>(){entity});
							}
						}
					}
				}
			}
			return groups;
		}
		
		List<string> GetPrimitivesGroups(Xrecord xrecord)
		{
			List<string> groups = new List<string>();
			if (xrecord != null)
			{
				ResultBuffer buffer = record.Data;
				//Проходим по каждому значению в XRecord
				foreach (TypedValue recordValue in buffer)
				{
					//Проверяем, была ли у нас такая группа?
					if (groups.ContainsKey(recordValue.Value.ToString()))
					{
						groups[recordValue.Value.ToString()].Add(entity);
					}
					else
					{
						groups.Add(recordValue.Value.ToString(), new List<ObjectId>(){entity});
					}
				}
			}
		}
		
		[CommandMethod("DeleteGroupLine", CommandFlags.UsePickSet)]
		public void DeleteGroupLine()
		{
			Editor editor = acadDocument.Editor;
			PromptSelectionResult selectionResult = editor.SelectImplied();
			SelectionSet selectionSet;
			List<string> groups = new List<string>();
			
			
			string group = AskForGroup(true, );
			
			if (selectionResult.Status == PromptStatus.OK)
			{
				selectionSet = selectionResult.Value;
			}
			else
			{
				
			}
		}
	}
}