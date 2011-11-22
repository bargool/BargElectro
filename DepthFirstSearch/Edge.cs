/*
 * Created by SharpDevelop.
 * User: aleksey
 * Date: 23.11.2011
 * Time: 1:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace DepthFirstSearch
{
	/// <summary>
	/// Description of Edge.
	/// </summary>
	public class Edge
	{
		public int BeginNodeIndex {get; private set;}
		public int EndNodeIndex {get; private set;}
		
		public Edge()
		{
		}
		
		public Edge(int beginNodeIndex, int endNodeIndex)
		{
			this.BeginNodeIndex = beginNodeIndex;
			this.EndNodeIndex = endNodeIndex;
		}
	}
}
