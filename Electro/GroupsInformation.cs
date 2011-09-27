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
using Autodesk.AutoCAD.DatabaseServices;

namespace BargElectro
{
	/// <summary>
	/// Description of GroupsInformation.
	/// </summary>
	public class GroupsInformation:IEnumerable
	{
		List<GroupObject> groupObjects;
		Transaction transaction;
		List<string> groupList;
		public GroupsInformation()
		{
		}
		
		public GroupsInformation(Transaction transaction)
		{
			
		}
		
		
		
		public void AddObjectToGroup(ObjectId objectid, string group)
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
