/*
 * User: aleksey
 * Date: 25.10.2011
 * Time: 0:14
 */
using System;
using System.Collections.Generic;
using System.Linq;

using acad = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.EditorInput;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace DepthFirstSearch
{
	/// <summary>
	/// Description of Tree.
	/// </summary>
	public class GraphTree
	{
		Editor ed;
//		Node root;
		public List<Node> Nodes {get; private set;}
		public List<Edge> Edges {get; private set;}
		public Node FarestNode {get; private set;}
		
		public GraphTree()
		{
			throw new NotImplementedException();
		}
		
		public GraphTree(Point3d rootPoint, Line[] lines)
		{
			ed = acad.DocumentManager.MdiActiveDocument.Editor;
//			this.root = new Node(rootPoint, 0, -1);
			this.Nodes = new List<Node>();
			this.Edges = new List<Edge>();
			AddNode(new Node(rootPoint, 0));
			this.FarestNode = Nodes.First();
			int count = 0;
			do
			{
				AddChildEdgesAndNodes(Nodes[count], lines);
				count++;
			} while (count<Nodes.Count);
		}
		/// <summary>
		/// Добавляет узел
		/// </summary>
		/// <param name="node">Узел для добавления. Свойство Index будет переопределено
		/// в соотвествии с зарегистрированными узлами в графе</param>
		void AddNode(Node node)
		{
			node.Index = Nodes.Count != 0 ? Nodes.Last().Index+1 : 0;
			Nodes.Add(node);
			if (FarestNode!=null)
			{
				if (node.PathCostFromRoot>FarestNode.PathCostFromRoot)
				{
					FarestNode = node;
				}
			}
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
			Edges.Add(new Edge(beginnode.Index, endnode.Index, geometry));
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
//				ed.WriteMessage("\nОбрабатываем линию ({0}, {1}) - ({2}, {3})",
//				               line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y);
				if (line.StartPoint.IsEqualTo(node.Point)&&FindNode(line.EndPoint)==null)
				{
					AddNode(new Node(line.EndPoint, node.PathCostFromRoot+line.Length));
					AddEdge(node, Nodes.Last(), line.ObjectId);
				}
				if (line.EndPoint.IsEqualTo(node.Point)&&FindNode(line.StartPoint)==null)
				{
					AddNode(new Node(line.StartPoint, node.PathCostFromRoot+line.Length));
					AddEdge(node, Nodes.Last(), line.ObjectId);
				}
			}
		}

		Node FindNode(int index)
		{
			return Nodes.FirstOrDefault(n => n.Index==index);
		}
		
		Node FindNode(Point3d point)
		{
//			if (point.IsEqualTo(root.Point))
//			{
//				return root;
//			}
//			else
//			{
				return Nodes.FirstOrDefault(n => point.IsEqualTo(n.Point));
//			}
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
			return Edges.FirstOrDefault(n => n.EndNodeIndex == node.Index);
		}
		
		Edge[] FindEdgesToChilds(Node node)
		{
			return (from Edge edge in Edges
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
