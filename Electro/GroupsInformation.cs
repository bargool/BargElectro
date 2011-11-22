/*
 * Created by SharpDevelop.
 * User: aleksey.nakoryakov
 * Date: 27.09.2011
 * Time: 16:09
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

namespace BargElectro
{
	/// <summary>
	/// Класс для работы с объектами с группами
	/// </summary>
	public class GroupsInformation:IEnumerable
		//TODO: проверить скорость открытия транзакций. что если использовать транзакции в каждом методе класса?
	{
		List<GroupObject> groupObjects; // Список объектов с группами
		Transaction transaction;
		List<string> groupList; // Список групп
		
		public List<string> GroupList
		{
			get { return groupList; }
		}
		
		Database currentDatabase;
		public GroupsInformation()
		{
			throw new ArgumentNullException("Transaction and Database have to be as argument");
		}
		
		public GroupsInformation(Transaction transaction, Database currDatabase)
		{
			this.transaction = transaction;
			currentDatabase = currDatabase;
			groupObjects = new List<GroupObject>();
			groupList = new List<string>();
			GetGroupsInformation();
		}
		
		/// <summary>
		/// Итерирует чертеж и собирает информацию по группам
		/// </summary>
		void GetGroupsInformation()
		{
			BlockTable bt = (BlockTable)transaction.GetObject(
				currentDatabase.BlockTableId, OpenMode.ForRead);
			BlockTableRecord btr = (BlockTableRecord)transaction.GetObject(
				bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
			var entities = from ObjectId entity in btr 
				where transaction.GetObject(entity, OpenMode.ForRead, false) is Entity 
				select entity;
			foreach (ObjectId entity in entities)
			{
				GroupObject groupObj = new GroupObject(entity, transaction);
				if (groupObj.HasGroup)
				{
					groupObjects.Add(groupObj);
					foreach (string group in groupObj.GetGroups())
					{
						if (!groupList.Contains(group))
						{
							groupList.Add(group);
						}
					}
				}
			}
			groupList.Sort();
		}
		
		/// <summary>
		/// Добавляет группу к объекту
		/// </summary>
		/// <param name="objectid">id объекта для добавления группы</param>
		/// <param name="group">имя группы для добавления</param>
		public void AppendGroupToObject(ObjectId objectid, string group)
			//TODO: При добавлении новой группы не обновляется список групп (а надо ли это, ведь он считывается заново при каждом вызове комманд?)
		{
			GroupObject groupObj = new GroupObject(objectid, transaction);
			int index = groupObjects.IndexOf(groupObj);
			if (index == -1)
			{
				groupObj.AddGroup(group);
				groupObjects.Add(groupObj);
				index = groupObjects.Count-1;
			}
			else
			{
				groupObjects[index].AddGroup(group);
			}
			groupObjects[index].WriteGroups();
		}
		
		/// <summary>
		/// Удаление группы из объекта
		/// </summary>
		/// <param name="objectid">id объекта для удаления группы</param>
		/// <param name="group">имя группы для удаления</param>
		public void DeleteGroupFromObject(ObjectId objectid, string group)
		{
			GroupObject groupObj = new GroupObject(objectid, transaction);
			int index = groupObjects.IndexOf(groupObj);
			if (index != -1)
			{
				groupObjects[index].DeleteGroup(group);
				groupObjects[index].WriteGroups();
				if (!groupObjects[index].HasGroup)
				{
					groupObjects.RemoveAt(index);
				}
			}
		}
		
		/// <summary>
		/// Возвращает группы объекта
		/// </summary>
		/// <param name="objectid">id объекта для получения информации</param>
		/// <returns>список групп объекта</returns>
		public List<string> GetGroupsOfObject(ObjectId objectid)
		{
			if (objectid!=null)
			{
				GroupObject groupObj = new GroupObject(objectid, transaction);
				int index = groupObjects.IndexOf(groupObj);
				if (index!=-1)
				{
					return groupObjects[index].GetGroups();
				}
			}
			return null;
		}
		
		/// <summary>
		/// Возвращает объекты, принадлежащие группе
		/// </summary>
		/// <param name="group">Имя группы для поиска</param>
		/// <returns>Список ObjectId с группой</returns>
		public List<ObjectId> GetObjectsOfGroup(string group)
		{
			List<ObjectId> objectsOfGroup = new List<ObjectId>();
			if (group != null)
			{
				foreach (GroupObject groupObj in groupObjects)
				{
					if (groupObj.IsBelongToGroup(group))
					{
						objectsOfGroup.Add(groupObj.objectid);
					}
				}
			}
			return objectsOfGroup;
		}
		
		/// <summary>
		/// Переименование группы объекта
		/// </summary>
		/// <param name="objectid">id объекта для переименования</param>
		/// <param name="previousName">Первоначальное имя группы</param>
		/// <param name="newName">Новое имя группы</param>
		public void RenameGroupOfObject(ObjectId objectid, string previousName, string newName)
		{
			GroupObject groupObj = new GroupObject(objectid, transaction);
			if (groupObj.IsBelongToGroup(previousName))
			{
				int index = groupObjects.IndexOf(groupObj);
				groupObjects[index].ChangeGroup(previousName, newName);
				groupObjects[index].WriteGroups();
			}
		}
		
		/// <summary>
		/// Реализация IEnumerable
		/// </summary>
		/// <returns>Итератор</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return groupObjects.GetEnumerator();
		}
		
		
	}
}
