using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harris.GPC
{

	//Function pointer for processing a tree
	internal delegate void ProcessBTreeFunc<T>(BinaryTree<T> tree, int level);

	internal class BinaryTree<T>
	{
		public T m_data;
		public BinaryTree<T> m_parent;
		public BinaryTree<T> m_left;
		public BinaryTree<T> m_right;

		public int rootLevel = 0;

		public BinaryTree()
		{
			m_parent = null;
			m_left = null;
			m_right = null;
		}

		//Counts the number of nodes in a subtree and returns the result
		public int Count()
		{
			int c = 1;
			if (m_left != null)
				c += m_left.Count();

			if (m_right != null)
				c += m_right.Count();
			return c;
		}

		//Counts the number of direct child nodes in a subtree and returns the result
		public int childCount()
		{
			int c = 0;
			if (m_left != null)
				c++;
			if (m_right != null)
				c++;
			return c;
		}

		public void Preorder(BinaryTree<T> node, ProcessBTreeFunc<T> processTreeFunc)
		{

			processTreeFunc(node, rootLevel);

			//if this subtree has children then increase the level
			if (node.childCount() > 0)
				rootLevel++;

			bool childrenProcessed = false;

			if(node.m_left != null)
			{
				Preorder(node.m_left, processTreeFunc);
				childrenProcessed = true;
			}

			if (node.m_right != null)
			{
				Preorder(node.m_right, processTreeFunc);
				childrenProcessed = true;
			}

			if (childrenProcessed)
			{
				rootLevel--;
			}
		}

		public void Inorder(BinaryTree<T> node, ProcessBTreeFunc<T> processTreeFunc)
		{

			//if this subtree has children then increase the level
			if (node.childCount() > 0)
				rootLevel++;

			bool childrenProcessed = false;

			if (node.m_left != null)
			{
				Inorder(node.m_left, processTreeFunc);
				childrenProcessed = true;
			}

			processTreeFunc(node, rootLevel);


			if (node.m_right != null)
			{
				Inorder(node.m_right, processTreeFunc);
				childrenProcessed = true;
			}

			if (childrenProcessed)
			{
				rootLevel--;
			}
		}

		public void Postorder(BinaryTree<T> node, ProcessBTreeFunc<T> processTreeFunc)
		{

			//if this subtree has children then increase the level
			if (node.childCount() > 0)
				rootLevel++;

			bool childrenProcessed = false;

			if (node.m_left != null)
			{
				Postorder(node.m_left, processTreeFunc);
				childrenProcessed = true;
			}

			if (node.m_right != null)
			{
				Postorder(node.m_right, processTreeFunc);
				childrenProcessed = true;
			}

			if (childrenProcessed)
			{
				rootLevel--;
			}

			processTreeFunc(node, rootLevel);

		}

		public void printTree(BinaryTree<T> currentNode, int treeLevel)
		{
			for (int i = 0; i < treeLevel; i++)
				Console.Write("  ");

			if (currentNode.m_parent == null || currentNode == currentNode.m_parent.m_right)
				Console.Write("\\-");

			else
				Console.Write("|-");

			Console.WriteLine(currentNode.m_data);
		}
	}

	internal class BTreeIterator<T>
	{
		//current node
		public BinaryTree<T> m_node;

		//current child in current node
		public BinaryTree<T> m_childitr;

		public BTreeIterator(BinaryTree<T> node = null)
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
			/*if (m_node != null)
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
			}*/
		}

		//Move the iterator to the root of the tree
		public void Root()
		{
			while (m_node.m_parent != null)
			{
				m_node = m_node.m_parent;
			}
			m_node.rootLevel = 0;
			//ResetIterator();
		}

		//Move the iterator up one level, can possibly invalidate the iterator if it moves beyond root
		public void Up()
		{
			if (m_node != null)
			{
				m_node = m_node.m_parent;
			}
			//ResetIterator();
		}

		public void Down()
		{
			if (m_node != null)
			{
				m_node = m_childitr;
				//ResetIterator();
			}
		}

		//Child Iterator navigation functions
		public void Left()
		{
			if(m_node.m_left != null)	
				m_childitr = m_node.m_left;
		}

		public void Right()
		{
			if (m_node.m_right != null)
				m_childitr = m_node.m_right;
		}


		//Other functions
		public void AppendChild(BinaryTree<T> node)
		{
			if (m_node.m_left == null)
				m_node.m_left = node;

			else if (m_node.m_right == null)
				m_node.m_right = node;

			node.m_parent = m_node;
		}	

		public void InsertLeft(BinaryTree<T> node)
		{
			if (m_node.m_left == null)
				m_node.m_left = node;
			node.m_parent = m_node;
		}

		public void InsertRight(BinaryTree<T> node)
		{
			if (m_node.m_right == null)
				m_node.m_right = node;
			node.m_parent = m_node;

		}

		public void RemoveLeft()
		{
			if (m_node.m_left != null)
				m_node.m_left = null;
		}

		public void RemoveRight()
		{
			if (m_node.m_right!= null)
				m_node.m_right = null;
		}

		public bool LeftValid()
		{
			return m_node.m_left != null;
		}

		public bool RightValid()
		{
			return m_node.m_right != null;
		}

		//Get the data of current child node
		public BinaryTree<T> ChildItem()
		{
			return m_childitr;
		}

	}
}
