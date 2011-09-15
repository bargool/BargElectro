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
				string GroupName = pStringResult.StringResult;
				//Application.ShowAlertDialog("Наименование группы: " + pStringResult.StringResult);
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
								string RecordKey = "BargElectroLines";
								TypedValue[] data = new TypedValue[2];
								data.SetValue(new TypedValue((int)DxfCode.Text, "GroupLines"),0);
								data.SetValue(new TypedValue((int)DxfCode.Text, GroupName),1);
								ResultBuffer buffer = new ResultBuffer(data);
								
								if (entity.ExtensionDictionary==ObjectId.Null)
								{
									entity.CreateExtensionDictionary();
								}
								using (DBDictionary dict = acadTrans.GetObject(
									entity.ExtensionDictionary, OpenMode.ForWrite, false) as DBDictionary)
								{
									if (dict.Contains(RecordKey))
									{
										Xrecord xrecord = acadTrans.GetObject(
											dict.GetAt(RecordKey), OpenMode.ForWrite) as Xrecord;
										xrecord.Data = buffer;
									}
									else
									{
										Xrecord xrecord = new Xrecord();
										xrecord.Data = buffer;
										dict.SetAt(RecordKey, xrecord);
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
}
