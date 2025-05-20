using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Harris.GPC
{

	internal class DListNode<T>
	{
		public T m_data;
		public DListNode<T> m_prev;
		public DListNode<T> m_next;

		public DListNode(T data)
		{
			m_data = data;
			m_prev = null;
			m_next = null;
		}

		public void SetData(T data)
		{
			m_data = data;
		}

		//Insert a new node with given data after the this node
		public void InsertAfter(T data)
		{
			InsertAfter(new DListNode<T>(data));
		}

		//Insert a new node after the this node
		public void InsertAfter(DListNode<T> newnode) //5 7 null //5 6 7
		{
			newnode.m_prev = this;
			newnode.m_next = m_next;

			//left node always exists
			m_next = newnode;

			//if right node exists make right node previous = newnode
			if (newnode.m_next != null)
				newnode.m_next.m_prev = newnode;
		}
	}

	internal class DLinkedList<T> : IEnumerable<T>
	{
		public DListNode<T> m_head;
		public DListNode<T> m_tail;
		public int m_count;
		public T Zero { get; set; }

		//Indexer
		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= m_count)
				{
					return Zero;
				}

				DListIterator<T> itr = GetIterator();
				itr.Start();
				for (int i = 1; i <= index; i++)
					itr.Forth();

				return itr.Item();
			}

			set
			{
				if (index >= 0 && index < m_count)
				{
					DListIterator<T> itr = GetIterator();
					itr.Start();
					for (int i = 1; i <= index; i++)
						itr.Forth();

					itr.m_node.SetData(value);
				}
			}
		}

		public DLinkedList(T Zero)
		{
			m_head = null;
			m_tail = null;
			m_count = 0;
			this.Zero = Zero;
		}

		//This function will be called by treec class where T == Tree<T2>
		public void Append(T data)
		{
			Append(new DListNode<T>(data));
		}

		public void Append(DListNode<T> node)
		{
			if (m_head == null)
			{
				m_head = m_tail = node;
			}

			//insert new node at end of list and update the tail node
			else
			{
				m_tail.InsertAfter(node);
				m_tail = m_tail.m_next;
			}
			m_count++;
		}

		//Append range of values
		public void Append(params T[] values)
		{
			foreach (T v in values)
				Append(v);
		}

		public void Prepend(T data)
		{
			Prepend(new DListNode<T>(data));
		}

		public void Prepend(DListNode<T> newnode) // 5 => 4 5
		{
			newnode.m_next = m_head;

			newnode.m_next.m_prev = newnode;

			//update head node
			m_head = newnode;

			//update tail node if needed
			if (m_tail == null)
				m_tail = m_head;

			m_count++;
		}

		//Prepend range of values
		public void Prepend(params T[] values)
		{
			for (int i = values.Length - 1; i >= 0; i--)
				Prepend(values[i]);
		}


		public void RemoveHead()  //5 6 7=> 6 7
		{
			DListNode<T> node = null;
			if (m_head != null)
			{
				node = m_head.m_next;
				m_head = node;

				if (m_head == null)
					m_tail = null;

				m_count--;
			}
		}

		public void RemoveTail() //5 6 7 => 5 6
		{
			DListNode<T> node = m_head;

			if (m_head != null)
			{
				//only one node exixts?
				if (m_head == m_tail)
				{
					m_head = m_tail = null;
				}

				//more than one node exists
				else
				{
					while (node.m_next != m_tail)
						node = node.m_next;

					//make tail point to the node before the current tail
					m_tail = node;
					node.m_next = null;
				}
				m_count--;
			}
		}

		public DListIterator<T> GetIterator()
		{
			return new DListIterator<T>(this, m_head);
		}

		//Inserts a node after the iterator
		public void Insert(DListIterator<T> iterator, T data)  //5 7 => 5 6 7, //5 => 5 7
		{
			//Make sure the iterator belongs to this list
			if (iterator.m_list != this)
				return;

			if (iterator.m_node != null)
			{
				iterator.m_node.InsertAfter(data);

				//if data was inserted after the tail then uoda
				if (iterator.m_node == m_tail)
					m_tail = iterator.m_node.m_next;

				m_count++;

			}

			else
			{
				// if the iterator is invalid, just append the data
				Append(data);
			}
		}

		public void Remove(DListIterator<T> iterator)
		{
			DListNode<T> node = m_head;

			// if the iterator doesn’t belong to this list, do nothing.
			if (iterator.m_list != this)
				return;

			// if node is invalid, do nothing.
			if (iterator.m_node == null)
				return;

			if (iterator.m_node == m_head)
			{
				// move the iterator forward and delete the head.
				iterator.Forth();
				RemoveHead();
			}

			else
			{
				// scan forward through the list until you find 
				// the node prior to the node you want to remove 
				while (node.m_next != iterator.m_node)  //50n, 60it => //50n x60, nullit				//50n, 60it, 70	=> //50n, x60, 70it
					node = node.m_next;
				// move the iterator forward. 
				iterator.Forth();
				// if the node you are deleting is the tail, 
				// update the tail node. 
				if (node.m_next == m_tail)
				{
					m_tail = node;
				}

				// re-link the list.
				node.m_next = iterator.m_node;
				node.m_next.m_prev = node;
			}

			m_count--;
		}

		public override string ToString()
		{
			string result = string.Empty;

			DListIterator<T> itr = GetIterator();

			for (itr.Start(); itr.Valid(); itr.Forth())
			{
				result += itr.Item() + ", ";
			}

			return result;
		}

		public void printReverse()
		{
			string result = string.Empty;

			//Create iterator
			DListIterator<T> itr = GetIterator();

			for (itr.End(); itr.Valid(); itr.Back())
			{
				Console.Write(itr.Item() + ", ");
			}
		}

		public Array<T> ToArray()
		{
			Array<T> arr = new Array<T>(Zero);

			for (int i = 0; i < m_count; i++)
				arr.Add(this[i]);

			return arr;
		}

		public IEnumerator<T> GetEnumerator()
		{

			DListIterator<T> itr = GetIterator();

			for (itr.Start(); itr.Valid(); itr.Forth())
			{
				yield return itr.Item();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}


	internal class DListIterator<T>
	{
		public DListNode<T> m_node;
		public DLinkedList<T> m_list;

		public DListIterator(DLinkedList<T> list = null, DListNode<T> node = null)
		{
			m_list = list;
			m_node = node;
		}

		//move iterator to start node
		public void Start()
		{
			if (m_list != null)
				m_node = m_list.m_head;
		}

		//move iterator to end node
		public void End()
		{
			if (m_list != null)
				m_node = m_list.m_tail;
		}

		//move iterator to next node
		public void Forth()
		{
			if (m_node != null)
				m_node = m_node.m_next;
		}

		//move iterator to previous node
		public void Back()
		{
			if (m_node != null)
				m_node = m_node.m_prev;

			if (m_node == null)
				Console.WriteLine("previous node is null!");
		}

		//Get the data of current node
		public T Item()
		{
			return m_node.m_data;
		}

		public bool Valid()
		{
			return (m_node != null);
		}
	}
}
