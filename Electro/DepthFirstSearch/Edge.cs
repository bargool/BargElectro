/*
 * Created by SharpDevelop.
 * User: aleksey
 * Date: 23.11.2011
 * Time: 1:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace DepthFirstSearch
{
	/// <summary>
	/// Description of Edge.
	/// </summary>
	public class Edge : IEquatable<Edge>
	{
		public int BeginNodeIndex {get; private set;}
		public int EndNodeIndex {get; private set;}
		public ObjectId EdgeGeometry {get; private set;}
		
		public Edge()
		{
		}
		
		public Edge(int beginNodeIndex, int endNodeIndex, ObjectId edgeGeometry)
		{
			this.BeginNodeIndex = beginNodeIndex;
			this.EndNodeIndex = endNodeIndex;
			this.EdgeGeometry = edgeGeometry;
		}
		
		public bool Equals(Edge other)
		{
			if (this.EdgeGeometry==other.EdgeGeometry)
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
