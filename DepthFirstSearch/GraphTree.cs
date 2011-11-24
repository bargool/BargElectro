/*
 * User: aleksey
 * Date: 25.10.2011
 * Time: 0:14
 */
using System;
using System.Collections.Generic;
using System.Linq;

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
		List<Node> nodes = new List<Node>();
		List<Edge> edges = new List<Edge>();
		
		public GraphTree()
		{
		}
		
		public GraphTree(Node root, Line[] lines)
		{
			this.root = root;
			
		}
		/// <summary>
		/// Добавляет узел
		/// </summary>
		/// <param name="node">Узел для добавления. Свойство Index будет переопределено
		/// в соотвествии с зарегистрированными узлами в графе</param>
		void AddNode(Node node)
			//TODO: Что с индексом?!
		{
			node.Index = ++nodes[nodes.Count-1].Index;
			nodes.Add(node);
		}
		
		/// <summary>
		/// Удаление узла. Этот метод пока не нужен - пропускаем
		/// </summary>
		/// <param name="node"></param>
		void DeleteNode(Node node)
		{
			
		}
		
		/// <summary>
		/// Удаление узла. Этот метод пока не нужен - пропускаем
		/// </summary>
		/// <param name="index"></param>
		void DeleteNode(int index)
		{
			
		}
		
		/// <summary>
		/// Добавление грани между узлами
		/// </summary>
		/// <param name="beginnode">Начальный узел</param>
		/// <param name="endnode">Конечный узел</param>
		/// <param name="geometry">Объект, представляющий грань</param>
		void AddEdge(Node beginnode, Node endnode, ObjectId geometry)
		{
			edges.Add(new Edge(beginnode.Index, endnode.Index, geometry));
		}
		
		/// <summary>
		/// Добавление граней и узлов, исходящих из конкретного узла
		/// </summary>
		/// <param name="node">Узел для вычисления исходящих граней</param>
		/// <param name="lines">Набор линий для поиска граней</param>
		void AddChildEdgesAndNodes(Node node, Line[] lines)
		{
			//TODO: Обработка зацикливания
//			foreach (Edge edge in edges)
//			{
//				if (FindChilds(node).Length!=0)
//				{
//					throw new Exception("Похоже, зациклились");
//				}
//			}
			foreach (Line line in lines)
			{
				if (line.StartPoint.IsEqualTo(node.Point)&&FindNode(line.EndPoint)==null)
				{
					AddNode(new Node(line.EndPoint, node.PathCostFromRoot+line.Length));
					AddEdge(node, nodes.Last(), line);
//					childs.Add(new Node(edge.EndPoint, edge, this, this.PathCostFromRoot+edge.Length));
//					edgesToChilds.Add(edge);
				}
			}
		}

		Node FindNode(int index)
		{
			return nodes.DefaultIfEmpty(null).First(n => n.Index==index);
		}
		
		Node FindNode(Point3d point)
		{
			return nodes.DefaultIfEmpty(null).First(n => point.IsEqualTo(n.Point));
		}
		
		Node FindParent(Node node)
		{
			return FindEdgeToParent(node)!=null ? FindNode(FindEdgeToParent(node).BeginNodeIndex) : null;
		}
		
		Node[] FindChilds(Node node)
		{
			return 
				(from Edge edge in FindEdgesToChilds(node)
				 select FindNode(edge.EndNodeIndex)).ToArray();
		}
		
		Edge FindEdgeToParent(Node node)
		{
			return edges.DefaultIfEmpty(null).First(n => n.EndNodeIndex == node.Index);
		}
		
		Edge[] FindEdgesToChilds(Node node)
		{
			return (from Edge edge in edges
				where edge.BeginNodeIndex==node.Index
				select edge).ToArray();
		}
		
//		void GetChildsAndEdges(Line[] edges)
//		{
//			childs = new List<Node>();
//			edgesToChilds = new List<Edge>();
//			foreach (Line edge in edges)
//			{
//				if (edge.StartPoint.IsEqualTo(this.point)&&!edge.EndPoint.IsEqualTo(parent.Point))
//				{
//					childs.Add(new Node(edge.EndPoint, edge, this, this.PathCostFromRoot+edge.Length));
//					edgesToChilds.Add(edge);
//				}
//				if (edge.EndPoint.IsEqualTo(this.point)&&!edge.StartPoint.IsEqualTo(parent.Point))
//				{
//					childs.Add(new Node(edge.StartPoint, edge, this, this.PathCostFromRoot+edge.Length));
//					edgesToChilds.Add(edge);
//				}
//			}
//		}
	}
}
