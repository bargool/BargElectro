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
	/// Description of GroupsInformation.
	/// </summary>
	public class GroupsInformation:IEnumerable
		//TODO: проверить скорость открытия транзакций. что если использовать транзакции в каждом методе класса?
	{
		List<GroupObject> groupObjects;
		Transaction transaction;
		List<string> groupList;
		Database currentDatabase;
		public GroupsInformation()
		{
		}
		
		public GroupsInformation(Transaction transaction, Database currDatabase)
		{
			this.transaction = transaction;
			currentDatabase = currDatabase;
			groupObjects = new List<GroupObject>();
			groupList = new List<string>();
			GetGroupsInformation();
		}
		
		void GetGroupsInformation()
		{
			BlockTable bt = (BlockTable)transaction.GetObject(
				currentDatabase.BlockTableId, OpenMode.ForRead);
			BlockTableRecord btr = (BlockTableRecord)transaction.GetObject(
				bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
			var entities = from ObjectId entity in btr 
				where transaction.GetObject(entity, OpenMode.ForRead) is Entity 
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
					groupList.Sort();
				}
			}
		}
		
		public void AppendObjectToGroup(ObjectId objectid, string group)
			//TODO: Необходимо проверять, нету ли уже такого объекта в списке объектов?
		{
			
		}
		
		public void DeleteObjectFromGroup(ObjectId objectid, string group)
		{
			
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return groupObjects.GetEnumerator();
		}
		
		
	}
}
