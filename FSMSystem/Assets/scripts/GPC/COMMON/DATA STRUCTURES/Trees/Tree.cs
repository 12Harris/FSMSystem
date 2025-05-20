using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Harris.GPC
{
	using System.IO;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Runtime.CompilerServices;
	using System.Xml.Linq;

	//Function pointer for processing a tree
	internal delegate void ProcessTreeFunc<T>(Tree<T> tree, int level);

	internal class Tree<T>
	{
		public T m_data;
		public Tree<T> m_parent;

		//List of child trees
		public DLinkedList<Tree<T>> m_children;

		public int rootLevel = 0;

		private static FileStream fs;

		public Tree()
		{
			m_children = new DLinkedList<Tree<T>>(null);
			m_parent = null;
		}

		//Counts the number of nodes in a subtree and returns the result
		public int Count()
		{
			int c = 1;
			DListIterator<Tree<T>> itr = m_children.GetIterator();
			for (itr.Start(); itr.Valid(); itr.Forth())
				c += itr.Item().Count();
			return c;
		}

		//Counts the number of direct child nodes in a subtree and returns the result
		public int childCount()
		{
			int c = 0;
			DListIterator<Tree<T>> itr = m_children.GetIterator();
			for (itr.Start(); itr.Valid(); itr.Forth())
				c++;
			return c;
		}

		//PREORDER TREE TRAVERSAL (each subtree is processed before the next subtree is processed)

		/*Preorder(node )
			Process(node )
			For each child
				Preorder(child )
			end For
		end Preorder*/

		public void Preorder(Tree<T> node, ProcessTreeFunc<T> processTreeFunc)
		{
			if (processTreeFunc != null)
				processTreeFunc(node, rootLevel);

			//if this subtree has children then increase the level
			if (node.childCount() > 0)
				rootLevel++;

			DListIterator<Tree<T>> itr = node.m_children.GetIterator();

			bool childrenProcessed = false;

			for (itr.Start(); itr.Valid(); itr.Forth())
			{
				Preorder(itr.Item(), processTreeFunc);
				childrenProcessed = true;
			}

			if (childrenProcessed)
			{
				rootLevel--;
			}
		}

		/*Postorder(node )
			For each child
				Postorder(child )
			end For
				Process(node )
		end Postorder*/


		public void Postorder(Tree<T> node, ProcessTreeFunc<T> processTreeFunc)
		{

			DListIterator<Tree<T>> itr = node.m_children.GetIterator();

			for (itr.Start(); itr.Valid(); itr.Forth())
				Postorder(itr.Item(), processTreeFunc);

			if(processTreeFunc != null)
				processTreeFunc(node, rootLevel+1);

		}

		public void writeTreeToFile(string path)
		{
			if (!File.Exists(path))
			{
				// Create a file to write to.
				using (StreamWriter sw = File.CreateText(path))
				{
					writeTreeToFile(sw, path, 0);
				}
			}
		}


		public void writeTreeToFile(StreamWriter sw, string path, int level=0)
		{
	
			for (int i = 0; i < level; i++)
			{
				sw.Write("\t");
			}

			sw.WriteLine(m_data);

			DListIterator<Tree<T>> itr = m_children.GetIterator();

			for (itr.Start(); itr.Valid(); itr.Forth())
			{
				itr.Item().writeTreeToFile(sw, path,level + 1);
			}
		
		}

		public void printTree(int level)
		{
			for (int i = 0; i <  level; i++)
				Console.Write("\t");

			Console.WriteLine(m_data);

			DListIterator<Tree<T>> itr = m_children.GetIterator();

			for (itr.Start(); itr.Valid(); itr.Forth())
			{
				itr.Item().printTree(level+1);
			}
		}

		/*public void printTree(Tree<T> currentNode, int treeLevel, bool fancy)
		{
			for (int i = 0; i < treeLevel; i++)
				Console.Write("\t");

			if (fancy)
			{
				if (currentNode.m_parent == null || currentNode == currentNode.m_parent.m_children[currentNode.m_parent.childCount() - 1])
					Console.Write("\\-");

				else
					Console.Write("|-");
			}

			Console.WriteLine(currentNode.m_data);
		}*/

		public static void readTree(string path, ref Tree<string> g_Tree)
		{
			ParseTree(path,ref g_Tree, 0, 0);
		}

		static uint howManyTabs(string s)
		{
			return (uint)s.Count(ch => ch == '\t');
		}

		public static string tabsRemoved(string s)
		{
			string result = "";
			foreach (var ch in s)
			{
				if(ch != '\t')
					result += ch;
			}
			return result;
		}

		public static void ParseTree(string path, ref Tree<string> g_Tree, int level, int lineStart)
		{
			//Console.WriteLine("ParseTree called.");

			var lines = File.ReadLines(path);
			var linesList = lines.ToList();

			
			String line = "";
			int currentlineIndex = 0;
			while (currentlineIndex < lineStart)
			{
				currentlineIndex++;
			}

			while (currentlineIndex < linesList.Count && howManyTabs(linesList[currentlineIndex]) >= level)
			{
				//Console.WriteLine("How many tabs on " + line + ": " + howManyTabs(line));
				if (howManyTabs(linesList[currentlineIndex]) == level)
				{
					//Console.WriteLine("level = " + level + " value = " + linesList[currentlineIndex]);

					if (g_Tree == null)
					{
						//Console.WriteLine("gtree is null!");
						g_Tree = new Tree<string>();
						g_Tree.m_data = tabsRemoved(linesList[currentlineIndex]); ;
						ParseTree(path,ref g_Tree, level + 1, currentlineIndex + 1);
					}

					else
					{
						Tree<string> node = new Tree<string>();
						node.m_data = tabsRemoved(linesList[currentlineIndex]); ;
						TreeIterator<string> itr = new TreeIterator<string>(g_Tree);
						itr.AppendChild(node);
						ParseTree(path,ref node, level + 1, currentlineIndex + 1);
					}
				}
				currentlineIndex++;
			}
		}
	}

	internal class TreeIterator<T>
	{
		//current node
		public Tree<T> m_node;

		//current child in current node
		public DListIterator<Tree<T>> m_childitr;

		public TreeIterator(Tree<T> node = null)
		{
			m_node = node;
			ResetIterator();
		}

		public T Item()
		{
			return m_node.m_data;
		}

		//Call this whenever the m_node pointer is changed
		public void ResetIterator()
		{
			if (m_node != null)
			{
				m_childitr = m_node.m_children.GetIterator();

				//if (m_childitr.Item() == null)
					//Console.WriteLine("mchild itr is null!");
			}

			else
			{
				//invalidate the list that this iterator belongs to
				m_childitr.m_list = null;

				//invalidate the node that the iterator points to
				m_childitr.m_node = null;
			}
		}

		//Move the iterator to the root of the tree
		public void Root()
		{
			while (m_node.m_parent != null)
			{
				m_node = m_node.m_parent;
			}
			m_node.rootLevel = 0;
			ResetIterator();
		}

		//Move the iterator up one level, can possibly invalidate the iterator if it moves beyond root
		public void Up()
		{
			if(m_node!= null)
			{
				m_node = m_node.m_parent;
			}
			ResetIterator();
		}

		public void Down()
		{
			if (m_node != null)
			{
				m_node = m_childitr.Item();
				ResetIterator();
			}
		}

		//Child Iterator navigation functions
		public void ChildStart()
		{
			m_childitr.Start();
		}

		public void ChildForth()
		{
			m_childitr.Forth();
		}

		public void ChildBack()
		{
			m_childitr.Back();
		}

		public void ChildEnd()
		{
			m_childitr.End();
		}

		//Other functions
		public void AppendChild(Tree<T> node)
		{
			m_childitr.m_list.Append(node);
			node.m_parent = m_node;
		}

		public void PrependChild(Tree<T> node)
		{
			m_childitr.m_list.Prepend(node);
			node.m_parent = m_node;
		}

		public void InsertChildBefore(Tree<T> node)
		{
			ChildBack();
			m_childitr.m_list.Insert(m_childitr, node);
			node.m_parent = m_node;
		}

		public void InsertChildAfter(Tree<T> node)
		{
			m_childitr.m_list.Insert(m_childitr, node);
			node.m_parent = m_node;

			//Console.WriteLine("Iter value = " + m_childitr.Item());

		}

		public void RemoveChild()
		{
			m_childitr.m_list.Remove(m_childitr);
		}

		public bool ChildValid()
		{
			return m_childitr.Valid();
		}

		public Tree<T> ChildItem()
		{
			return m_childitr.Item();
		}

	}
}
