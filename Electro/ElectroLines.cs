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
		
		[CommandMethod("AddLinesToGroup")]
		public void AddLinesToGroup()
		{
			Editor editor = acadDocument.Editor;
			string GroupName = AskForGroup(false, FindGroups().Keys.ToList());
			if (GroupName != null)
			{
				using (Transaction acadTrans = acadCurDb.TransactionManager.StartTransaction())
				{
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
						SelectionSet acadSelectedObjects = acadSelSetPrompt.Value;
						foreach (SelectedObject selectedObj in acadSelectedObjects)
						{
							if (selectedObj!=null)
							{
								Entity entity = acadTrans.GetObject(selectedObj.ObjectId, OpenMode.ForWrite) as Entity;
								if (entity!=null)
								{
									TypedValue data = new TypedValue((int)DxfCode.Text, GroupName);
									ResultBuffer buffer = new ResultBuffer(data);
									
									if (entity.ExtensionDictionary==ObjectId.Null)
									{
										entity.CreateExtensionDictionary();
									}
									using (DBDictionary dict = acadTrans.GetObject(
										entity.ExtensionDictionary, OpenMode.ForWrite, false) as DBDictionary)
									{
										if (dict.Contains(AppRecordKey))
										{
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
		
		[CommandMethod("GetLinesroup")]
		public void GetLinesGroup()
		{
			//TODO: Переделать для итерации по базе и использования FindGroups!!
			//Dictionary, в котором будут храниться номера групп и длины
			Dictionary<string, double> GroupLenghts = new Dictionary<string, double>();
			using (Transaction acadTrans = acadCurDb.TransactionManager.StartTransaction())
			{
				//Выделять будем только линии и полилинии. Создаем фильтр
				TypedValue[] acadFilterValues = new TypedValue[4];
				acadFilterValues.SetValue(new TypedValue((int)DxfCode.Operator, "<OR"),0);
				acadFilterValues.SetValue(new TypedValue((int)DxfCode.Start, "LINE"),1);
				acadFilterValues.SetValue(new TypedValue((int)DxfCode.Start, "LWPOLYLINE"),2);
				acadFilterValues.SetValue(new TypedValue((int)DxfCode.Operator, "OR>"),3);
				SelectionFilter acadSelFilter = new SelectionFilter(acadFilterValues);
				//Выделяем ВСЕ линии и полилинии на незамороженных слоях
				SelectionSet selectionSet = acadDocument.Editor.SelectAll(acadSelFilter).Value;
				//Проходим по получившемуся выделению
				foreach (SelectedObject selectedObj in selectionSet)
				{
					if (selectedObj != null)
					{
						Curve entity = acadTrans.GetObject(selectedObj.ObjectId, OpenMode.ForRead) as Curve;
						if (entity!=null)
						{
							Xrecord record = getXrecord(AppRecordKey, selectedObj.ObjectId, acadTrans);
							if (record != null)
							{
								ResultBuffer buffer = record.Data;
								//Проходим по каждому значению в XRecord
								foreach (TypedValue recordValue in buffer)
								{
									//Проверяем, была ли у нас такая группа?
									if (GroupLenghts.ContainsKey(recordValue.Value.ToString()))
									{
										GroupLenghts[recordValue.Value.ToString()] += entity.GetDistanceAtParameter(entity.EndParam);
									}
									else
									{
										GroupLenghts.Add(recordValue.Value.ToString(), entity.GetDistanceAtParameter(entity.EndParam));
									}
								}
							}
						}
					}
				}
			}
			foreach (KeyValuePair<string, double> kvp in GroupLenghts)
			{
				acadDocument.Editor.WriteMessage("\nГруппа {0}, длина: {1}", kvp.Key, kvp.Value);
			}
		}

		/// <summary>
		/// Возвращает XRecord объекта в режиме для чтения
		/// </summary>
		/// <param name="recordKey">Ключ словаря, который должен содержать XRecord</param>
		/// <param name="objectid">ObjectID объекта, у которого мы получаем XRecord</param>
		/// <param name="transaction">Используемая транзакция</param>
		/// <returns>XRecord в режиме ForRead</returns>
		Xrecord getXrecord(string recordKey, ObjectId objectid, Transaction transaction)
		{
			Entity entity = transaction.GetObject(objectid, OpenMode.ForRead) as Entity;
			if (entity != null)
			{
				if (entity.ExtensionDictionary != ObjectId.Null)
				{
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
	}
}