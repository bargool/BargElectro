/*
 * User: aleksey.nakoryakov
 * Date: 27.09.2011
 * Time: 10:45
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

namespace BargElectro
{
	/// <summary>
	/// Класс представляет собой абстракцию для работы с примитивом, содержащим информацию по группам
	/// </summary>
	public class GroupObject: IEquatable<GroupObject>
	{
		const string AppRecordKey = "BargElectroLinesGroup"; //Зарезервированное имя словаря для списка групп
		List<string> GroupList; //Список групп, к которым принадлежит объект
		ObjectId id; //ObjectId объекта entity, а не xrecord
		public ObjectId objectid
		{
			get {return id;}
		}
		Transaction transaction;
		bool hasGroup; // Имеет ли группы? TODO: Необходимо удалить
		public bool HasGroup
		{
			get {return GroupList.Count != 0;}
		}
		public GroupObject()
		{
			throw new ArgumentNullException("ObjectId and Transaction have to be as argument");
		}
		
		public GroupObject(ObjectId id, Transaction trans)
		{
			this.id = id;
			this.transaction = trans;
			GroupList = ReadGroups();
			hasGroup = GroupList != null;
		}
		
		/// <summary>
		/// Для инициализации считывает группы объекта из Xrecord
		/// </summary>
		/// <returns>Список групп, считанный из Xrecord</returns>
		private List<string> ReadGroups()
		{
			List<string> groups = new List<string>();
			Entity entity = transaction.GetObject(id, OpenMode.ForRead) as Entity;
			if (entity != null)
			{
				// Проверяем, есть ли словарь у объекта
				if (entity.ExtensionDictionary != ObjectId.Null)
				{
					//TODO: доделать обработку exception, как-то можно узнать удален ли словарь?
					try
					{
						using (DBDictionary dict = transaction.GetObject(
							entity.ExtensionDictionary, OpenMode.ForRead, false, false) as DBDictionary)
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
					catch (Autodesk.AutoCAD.Runtime.Exception ex)
					{
						
					}
				}
			}
			return null;
		}
		
		/// <summary>
		/// Запись групп в Xrecord объекта, необходимо вызывать каждый раз,
		/// когда происходят изменения
		/// </summary>
		public void WriteGroups()
		{
			Entity entity = transaction.GetObject(id, OpenMode.ForWrite) as Entity;
			if (entity!=null)
			{
				if (hasGroup)
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
				else
				{
					if (entity.ExtensionDictionary!=ObjectId.Null)
					{
						using (DBDictionary dict = (DBDictionary)transaction.GetObject(entity.ExtensionDictionary, OpenMode.ForWrite))
						{
							if (dict.Contains(AppRecordKey))
							{
							    dict.Remove(AppRecordKey);
							    if (dict.Count==0)
							    {
							    	dict.Erase();
							    }
							}
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Возвращает список групп объекта
		/// </summary>
		/// <returns>Список групп</returns>
		public List<string> GetGroups()
		{
			return this.GroupList;
		}
		
		/// <summary>
		/// Добавление группы к объекту
		/// </summary>
		/// <param name="group">Имя группы, которую надо добавить</param>
		public void AddGroup(string group)
		{
			if (group == null) return;
			if (hasGroup)
			{
				if (!GroupList.Contains(group))
				{
					GroupList.Add(group);
				}
			}
			else
			{
				GroupList = new List<string>();
				GroupList.Add(group);
				hasGroup = true;
			}
		}
		
		/// <summary>
		/// Удаление группы из объекта
		/// </summary>
		/// <param name="group">Имя группы для удаления</param>
		public void DeleteGroup(string group)
		{
			if ((group != null)&&(GroupList.Contains(group)))
			{
				GroupList.Remove(group);
				hasGroup = GroupList.Count != 0;
			}
		}
		
		/// <summary>
		/// Замена одной группы в объекте на другую
		/// </summary>
		/// <param name="groupFrom">Заменяемая группа</param>
		/// <param name="groupTo">Группа, на которую меняем,
		/// если уже есть - просто удаляем группу groupFrom</param>
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
		
		/// <summary>
		/// Проверяет, относится ли данный GroupObject к конкретной группе
		/// </summary>
		/// <param name="group">Группа, принадлежность к которой проверяем</param>
		/// <returns>Принадлежит или нет?</returns>
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
		
		/// <summary>
		/// Реализация интерфейса IEquatable
		/// </summary>
		/// <param name="other">другой объект для сравнения</param>
		/// <returns>Равны объекты или нет</returns>
		public bool Equals(GroupObject other)
		{
			if (this.objectid == other.objectid)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		/// <summary>
		/// Заготовка для обработки исключений внутри класса
		/// </summary>
		/// <param name="ex">исключение для обработки</param>
		void ExceptionHandling(Autodesk.AutoCAD.Runtime.Exception ex)
		{
			
		}
	}
}
