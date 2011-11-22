/*
 * User: aleksey
 * Date: 25.10.2011
 * Time: 0:14
 */
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace DepthFirstSearch
{
	/// <summary>
	/// Класс описывает узлы дерева
	/// </summary>
	public class Node : IEquatable<Node>
	{
		#region Properties
		public double PathCostFromRoot {get; private set;}
		int hitCounter;
		public int HitCounter {get
			{ return hitCounter; }
		}
		
		public int index {get; set;}
		
		protected Point3d point;
		public Point3d Point {
			get	{ return point; }
		}
		#endregion
		#region Constructors
		public Node()
		{
		}
		
		public Node(Point3d p, double pathCostFR)
		{
			point = p;
			PathCostFromRoot = pathCostFR;
		}
		#endregion
		
		public bool Equals(Node other)
		{
			if (this.point.IsEqualTo(other.Point))
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
