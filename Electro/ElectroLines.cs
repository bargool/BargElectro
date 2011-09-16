using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

[assembly: CommandClass(typeof(Electro.ElectroLines))]

namespace Electro
{
	public class ElectroLines
	{
		const string AppRecordKey = "BargElectroLinesGroup";
		
		[CommandMethod("AddLinesToGroup")]
		public void AddLinesToGroup()
		{
			Document acadDocument = Application.DocumentManager.MdiActiveDocument;
			Database acadCurDb = acadDocument.Database;
			using (Transaction acadTrans = acadCurDb.TransactionManager.StartTransaction())
			{
				//Спрашиваем имя группы
				PromptStringOptions pStringOptions = new PromptStringOptions("\nВведите наименование группы: ");
				pStringOptions.AllowSpaces = false;
				pStringOptions.DefaultValue = "Гр.1";
				PromptResult pStringResult = acadDocument.Editor.GetString(pStringOptions);
				string GroupName = pStringResult.StringResult; //Имя группы
				PromptSelectionOptions acadSelectionOptions = new PromptSelectionOptions();
				acadSelectionOptions.MessageForAdding = "\nУкажите объекты группы";
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
								//data.SetValue(new TypedValue((int)DxfCode.Text, "GroupLine"),0);
								//data .SetValue (new TypedValue((int)DxfCode.Text, GroupName),0);
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
		
		[CommandMethod("GetLinesroup")]
		public void GetLinesGroup()
		{
			//Dictionary, в котором будут храниться номера групп и длины
			Dictionary<string, double> GroupLenghts = new Dictionary<string, double>();
			
			Document acadDocument = Application.DocumentManager.MdiActiveDocument;
			Database acadCurDb = acadDocument.Database;
			using (Transaction acadTrans = acadCurDb.TransactionManager.StartTransaction())
			{
				//Спрашиваем имя группы
//				PromptStringOptions pStringOptions = new PromptStringOptions("\nВведите наименование группы: ");
//				pStringOptions.AllowSpaces = false;
//				pStringOptions.DefaultValue = "Гр.1";
//				PromptResult pStringResult = acadDocument.Editor.GetString(pStringOptions);
//				string GroupName = pStringResult.StringResult; //Имя группы
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
							if (entity.ExtensionDictionary!=ObjectId.Null)
							{
								using (DBDictionary dict = acadTrans.GetObject(
									entity.ExtensionDictionary, OpenMode.ForRead, false) as DBDictionary)
								{
									if (dict.Contains(AppRecordKey))
									{
										//Получаем XRecord
										Xrecord xrecord = acadTrans.GetObject(
											dict.GetAt(AppRecordKey), OpenMode.ForRead) as Xrecord;
										ResultBuffer buffer = xrecord.Data;
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
				}
			}
			foreach (KeyValuePair<string, double> kvp in GroupLenghts)
			{
				acadDocument.Editor.WriteMessage("\nГруппа {0}, длина: {1}", kvp.Key, kvp.Value);
			}
		}
		
//		public ObjectIdCollection GetGroupPrimitives(string groupname, Transaction trans, Document acad, Database CurDb)
//		{
//			foreach (SelectedObject selectedObj in selectionSet)
//			{
//				if (selectedObj != null)
//				{
//					Curve entity = acadTrans.GetObject(selectedObj.ObjectId, OpenMode.ForRead) as Curve;
//					if (entity!=null)
//					{
//						if (entity.ExtensionDictionary!=ObjectId.Null)
//						{
//							using (DBDictionary dict = acadTrans.GetObject(
//								entity.ExtensionDictionary, OpenMode.ForRead, false) as DBDictionary)
//							{
//								if (dict.Contains(AppRecordKey))
//								{
//									//Получаем XRecord
//									Xrecord xrecord = acadTrans.GetObject(
//										dict.GetAt(AppRecordKey), OpenMode.ForRead) as Xrecord;
//									ResultBuffer buffer = xrecord.Data;
//									//Проходим по каждому значению в XRecord
//									foreach (TypedValue recordValue in buffer)
//									{
//										//Проверяем, была ли у нас такая группа?
//										if (GroupLenghts.ContainsKey(recordValue.Value.ToString()))
//										{
//											GroupLenghts[recordValue.Value.ToString()] += entity.GetDistanceAtParameter(entity.EndParam);
//										}
//										else
//										{
//											GroupLenghts.Add(recordValue.Value.ToString(), entity.GetDistanceAtParameter(entity.EndParam));
//										}
//									}
//								}
//							}
//						}
//					}
//				}
//			}
//			foreach (KeyValuePair<string, double> kvp in GroupLenghts)
//			{
//				acadDocument.Editor.WriteMessage("\nГруппа {0}, длина: {1}", kvp.Key, kvp.Value);
//			}
//		}
//		
		[CommandMethod("GetLines")]
		public void GetLines()
		{
			Document acadDocument = Application.DocumentManager.MdiActiveDocument;
			Database acadCurDb = acadDocument.Database;
			using (Transaction acadTrans = acadCurDb.TransactionManager.StartTransaction())
			{
				BlockTable blockTable = acadTrans.GetObject(acadCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
				BlockTableRecord blockTableRecord = acadTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
								
				var lines = from ObjectId line in blockTableRecord where acadTrans.GetObject(line, OpenMode.ForRead) is Line select (Line)acadTrans.GetObject(line, OpenMode.ForRead);

				foreach (Line line in lines)
				{
					acadDocument.Editor.WriteMessage("\nДлина: {0}", line.Length);
				}
			}
		}
	}
}