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
		List<string> GroupList; //Список групп, к которым принадлежит объект
		ObjectId objectid; //ObjectId объекта entity, а не xrecord
		
		public GroupObject()
		{
		}
		
		public List<string> GetGroups()
		{
			return this.GroupList;
		}
		
//		public void SetGoups(List<string> groups)
//		{
//			
//		}
		
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
			return true;
		}
	}
}
