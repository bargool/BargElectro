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
		public double PathCostFromRoot {get; set;}
//		int hitCounter;
//		public int HitCounter {get
//			{ return hitCounter; }
//		}
		
		protected List<Node> childs;
		public List<Node> Childs {get
			{ return childs; }
		}
		protected List<Line> edgesToChilds;
		public List<Line> EdgesToChilds {get
			{ return edgesToChilds; }
		}
		protected Line parentEdge;
		public Line ParentEdge {get
			{ return parentEdge; }
		}
		protected Node parent;
		public Node Parent {get
			{ return parent; }
		}
//		bool isLeaf;
		public bool IsLeaf {
			get{ return childs.Count == 0; }
		}
		protected Point3d point;
		public Point3d Point {
			get	{ return point; }
		}
		
		public Node()
		{
		}
		
		public Node(Point3d p, Line parentEdge, Node parentNode, double pathCostFR)
		{
			point = p;
			PathCostFromRoot = pathCostFR;
			this.parentEdge = parentEdge;
			this.parent = parentNode;
		}
		
		public virtual void GetChildsAndEdges(Line[] edges)
		{
			childs = new List<Node>();
			edgesToChilds = new List<Line>();
			foreach (Line edge in edges)
			{
				if (edge.StartPoint.IsEqualTo(this.point)&&!edge.EndPoint.IsEqualTo(parent.Point))
				{
					childs.Add(new Node(edge.EndPoint, edge, this, this.PathCostFromRoot+edge.Length));
					edgesToChilds.Add(edge);
				}
				if (edge.EndPoint.IsEqualTo(this.point)&&!edge.StartPoint.IsEqualTo(parent.Point))
				{
					childs.Add(new Node(edge.StartPoint, edge, this, this.PathCostFromRoot+edge.Length));
					edgesToChilds.Add(edge);
				}
			}
		}
		
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
		
//		List<Node> GetChilds(Line[] lines)
//		{
//			
//		}
//		
//		List<Edge> GetEdges()
//		{
//			
//		}
	}
}
