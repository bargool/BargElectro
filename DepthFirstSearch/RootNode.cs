/*
 * User: Bargool
 * Date: 27.10.2011
 * Time: 17:40
 */
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace DepthFirstSearch
{
	/// <summary>
	/// Description of RootNode.
	/// </summary>
	public class RootNode : Node
	{
		public RootNode()
		{
		}
		
		public RootNode(Point3d p)
			: base(p, null, null, 0)
		{}
		
		public override void GetChildsAndEdges(Autodesk.AutoCAD.DatabaseServices.Line[] edges)
		{
			childs = new List<Node>();
			edgesToChilds = new List<Line>();
			foreach (Line edge in edges)
			{
				if (edge.StartPoint.IsEqualTo(this.point))
				{
					childs.Add(new Node(edge.EndPoint, edge, this, edge.Length));
					edgesToChilds.Add(edge);
				}
				if (edge.EndPoint.IsEqualTo(this.point))
				{
					childs.Add(new Node(edge.StartPoint, edge, this, edge.Length));
					edgesToChilds.Add(edge);
				}
			}
		}
	}
}
