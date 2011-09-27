/*
 * Created by SharpDevelop.
 * User: aleksey.nakoryakov
 * Date: 27.09.2011
 * Time: 10:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

namespace BargElectro
{
	/// <summary>
	/// Description of GroupObject.
	/// </summary>
	public class GroupObject
	{
		const string AppRecordKey = "BargElectroLinesGroup";
		List<string> GroupList; //Список групп, к которым принадлежит объект
		public ObjectId objectid {get; set;} //ObjectId объекта entity, а не xrecord
		Transaction transaction;
		
		public GroupObject()
		{
			throw new ArgumentNullException("ObjectId and Transaction have to be as argument");
		}
		
		public GroupObject(ObjectId id, Transaction trans)
		{
			this.objectid = id;
			this.transaction = trans;
			GroupList = ReadGroups();
			if (GroupList == null)
			{
				GroupList = new List<string>();
			}
		}
		
		private List<string> ReadGroups()
		{
			List<string> groups = new List<string>();
			Entity entity = transaction.GetObject(objectid, OpenMode.ForRead) as Entity;
			if (entity != null)
			{
				// Проверяем, есть ли словарь у объекта
				if (entity.ExtensionDictionary != ObjectId.Null)
				{
					using (DBDictionary dict = transaction.GetObject(
						entity.ExtensionDictionary, OpenMode.ForRead) as DBDictionary)
					{
						if (dict.Contains(AppRecordKey))
						{
							Xrecord xrecord = transaction.GetObject(dict.GetAt(AppRecordKey), OpenMode.ForRead) as Xrecord;
							ResultBuffer buffer = xrecord.Data;
							foreach (TypedValue recordValue in buffer)
							{
								groups.Add(recordValue.Value.ToString());
							}
							if (groups.Count != 0)
							{
								return groups;
							}
						}
					}
				}
			}
			return null;
		}
		
		public void WriteGroups()
		{
			Entity entity = transaction.GetObject(objectid, OpenMode.ForWrite) as Entity;
			if (entity!=null)
			{
				// Проверяем, есть ли у объекта словарь? Если нет - создаём новый
				if (entity.ExtensionDictionary==ObjectId.Null)
				{
					entity.CreateExtensionDictionary();
				}
				using (DBDictionary dict = transaction.GetObject(
					entity.ExtensionDictionary, OpenMode.ForWrite, false) as DBDictionary)
				{
					//Готовим данные с именем группы для записи в XRecord
					ResultBuffer buffer = new ResultBuffer();
					foreach (string group in GroupList)
					{
						buffer.Add(new TypedValue((int)DxfCode.Text, group));
					}
					//Проверяем, есть ли запись словаря, закреплённая (мной) за плагином
					if (dict.Contains(AppRecordKey))
					{
						// Если запись уже есть - получаем XRecord, и перезаписываем группы
						Xrecord xrecord = transaction.GetObject(
							dict.GetAt(AppRecordKey), OpenMode.ForWrite) as Xrecord;
						xrecord.Data = buffer;
					}
					else
					{
						// Словаря нет - создаем запись словаря и XRecord
						Xrecord xrecord = new Xrecord();
						xrecord.Data = buffer;
						dict.SetAt(AppRecordKey, xrecord);
						transaction.AddNewlyCreatedDBObject(xrecord, true);
					}
				}
			}
		}
		
		public List<string> GetGroups()
		{
			return this.GroupList;
		}
		
		public void AddGroup(string group)
		{
			if ((group != null)&&(!GroupList.Contains(group)))
			{
				GroupList.Add(group);
			}
		}
		
		public void DeleteGroup(string group)
		{
			if ((group != null)&&(GroupList.Contains(group)))
			{
				GroupList.Remove(group);
			}
		}
		
		public void ChangeGroup(string groupFrom, string groupTo)
		{
			if ((groupFrom != null)&&(groupTo != null))
			{
				if (GroupList.Contains(groupFrom))
				{
					if (!GroupList.Contains(groupTo))
					{
						int index = GroupList.FindIndex(n => n == groupFrom);
						GroupList[index] = groupTo;
					}
					else
					{
						GroupList.Remove(groupFrom);
					}
				}
			}
		}
		
		public bool IsBelongToGroup(string group)
		{
			if (GroupList.Contains(group))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
