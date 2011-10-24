/*
 * Created by SharpDevelop.
 * User: aleksey
 * Date: 25.10.2011
 * Time: 0:14
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace DepthFirstSearch
{
	/// <summary>
	/// Description of Node.
	/// </summary>
	public class Node
	{
		public double PathCostFromRoot {get; set;}
		int hitCounter;
		public int HitCounter {get
			{
				return hitCounter;
			}
		}
		int nodeNumber;
		public int NodeNumber {get
			{
				return nodeNumber;
			}
		}
		
		public Node()
		{
		}
	}
}
