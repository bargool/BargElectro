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
	public class Node
	{
		public double PathCostFromRoot {get; set;}
		int hitCounter;
		public int HitCounter {get
			{ return hitCounter; }
		}
//		int nodeNumber;
//		public int NodeNumber {get
//			{
//				return nodeNumber;
//			}
//		}
		
//		List<Node> childs;
		List<Edge> edgesToChilds;
		Edge parentEdge;
		bool isLeaf;
		public bool IsLeaf {
			get{ return isLeaf; }
		}
		Point3d point;
		public Point3d Point {
			get	{ return point; }
		}
		
		public Node()
		{
		}
		
		public Node(Point3d p, Line[] lines, Line parentLine, double pathCostFR)
		{
			point = p;
			PathCostFromRoot = pathCostFR;
			
		}
		
		List<Node> GetChilds(Line[] lines)
		{
			
		}
		
		List<Edge> GetEdges()
		{
			
		}
	}
}
