/*
 * Created by SharpDevelop.
 * User: aleksey
 * Date: 25.10.2011
 * Time: 0:12
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace DepthFirstSearch
{
	/// <summary>
	/// Description of Edge.
	/// </summary>
	public class Edge
	{
		Point3d beginNode;
		public Point3d BeginNode {
			get { return beginNode; }
		}
		Point3d endNode;
		public Point3d EndNode {
			get { return endNode; }
		}
		Line line;
		
		public Edge()
		{
		}
		public Edge(Point3d begin, Point3d end, Line l)
		{
			beginNode = begin;
			endNode = end;
			line = l;
		}
	}
}
