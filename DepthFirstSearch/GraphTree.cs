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
	/// Description of Tree.
	/// </summary>
	public class GraphTree
	{
		Node root;
		List<Node> nodes;
		List<Edge> edges;
		
		public GraphTree()
		{
		}
		
		public GraphTree(Node root, Line[] lines)
		{
			this.root = root;
			
		}
		
		void AddNode(Node node)
			//TODO: Что с индексом?!
		{
			node.Index = nodes.Count;
			nodes.Add(node);
		}
		
		void DeleteNode(Node node)
		{
			
		}
		
		void DeleteNode(int index)
		{
			
		}
		
		void AddEdge(Node beginnode, Node endnode)
		{
			
		}
		
		void AddEdgeAndNode(Node node, Line[] lines)
		{
			
		}
		
		void FindParent(Node node)
		{
			
		}
		
		void FindChilds(Node node)
		{
			
		}
		
		void FindEdgeToParent(Node node)
		{
			
		}
		
		void FindEdgesToChilds(Node node)
		{
			
		}
	}
}
