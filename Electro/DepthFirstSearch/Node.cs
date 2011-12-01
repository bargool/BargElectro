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
		public int Index {get; set;}
		public Point3d Point {get; private set;}
		#endregion
		#region Constructors
		public Node()
			: this(new Point3d(0,0,0), 0) { }
			
		public Node(Point3d p, double pathCostFR)
			: this(p, pathCostFR, 0) { }
		
		public Node(Point3d p, double pathCostFR, int index)
		{
			Point = p;
			PathCostFromRoot = pathCostFR;
			this.Index = index;
		}
		#endregion
		
		//IEquatable
		public bool Equals(Node other)
		{
			if (this.Point.IsEqualTo(other.Point))
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
