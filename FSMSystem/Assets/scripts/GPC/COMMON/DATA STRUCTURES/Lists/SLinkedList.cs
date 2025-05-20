using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;

namespace Harris.GPC
{

	internal class SListNode<T>
	{
		public T m_data;
		public SListNode<T> m_next;

		public SListNode(T data)
		{
			m_data = data;
			m_next = null;
		}

		public void SetData(T data)
		{
			m_data = data;
		}

		//Insert a new node with given data after the this node
		public void InsertAfter(T data)
		{
			InsertAfter(new SListNode<T>(data));
		}

		//Insert a new node after the this node
		public void InsertAfter(SListNode<T> newnode)
		{
			newnode.m_next = m_next;
			m_next = newnode;
		}
	}

	internal class SLinkedList<T> : IEnumerable<T>
	{
		public SListNode<T> m_head;
		public SListNode<T> m_tail;
		int m_count;
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

				SListIterator<T> itr = GetIterator();
				itr.Start();
				for (int i = 1; i <= index; i++)
					itr.Forth();

				return itr.Item();
			}

			set
			{
				if (index >= 0 && index < m_count)
				{
					SListIterator<T> itr = GetIterator();
					itr.Start();
					for (int i = 1; i <= index; i++)
						itr.Forth();

					itr.m_node.SetData(value);
				}
			}
		}

		public SLinkedList(T Zero)
		{
			m_head = null;
			m_tail = null;
			m_count = 0;
			this.Zero = Zero;
		}

		public void Append(T data)
		{
			Append(new SListNode<T>(data));
		}

		public void Append(SListNode<T> node)
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
			Prepend(new SListNode<T>(data));
		}

		public void Prepend(SListNode<T> newnode)
		{
			newnode.m_next = m_head;

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
			for(int i = values.Length-1; i >= 0; i--)
				Prepend(values[i]);
		}

		public void RemoveHead()
		{
			SListNode<T> node = null;
			if (m_head != null)
			{
				node = m_head.m_next;
				m_head = node;

				if (m_head == null)
					m_tail = null;

				m_count--;
			}
		}

		public void RemoveTail()
		{
			SListNode<T> node = m_head;

			if (m_head != null)
			{
				if (m_head == m_tail)
				{
					m_head = m_tail = null;
				}

				else
				{ 
					while(node.m_next != m_tail)
						node = node.m_next;

					//make tail point to the node before the current tail
					m_tail = node;
					node.m_next = null;
				}
				m_count--;
			}
		}

		public SListIterator<T> GetIterator()
		{
			return new SListIterator<T> (this, m_head);
		}

		//Inserts a node after the iterator
		public void Insert(SListIterator<T> iterator, T data)
		{
			//Make sure the iterator belongs to this list
			if (iterator.m_list != this)
				return;

			if (iterator.m_node != null)
			{
				iterator.m_node.InsertAfter(data);

				// if the iterator is valid, then insert the node
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

		public void Remove(SListIterator<T> iterator)
		{
			SListNode<T> node = m_head;

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
			}

			m_count--;
		}

		public override string ToString()
		{
			string result = string.Empty;

			SListIterator<T> itr = GetIterator();

			for (itr.Start(); itr.Valid(); itr.Forth())
			{
				result += itr.Item() + ", ";
			}

			return result;
		}

		public IEnumerator<T> GetEnumerator()
		{
			
			SListIterator<T> itr = GetIterator();

			for (itr.Start(); itr.Valid(); itr.Forth())
			{
				yield return itr.Item();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public Array<T> ToArray()
		{
			Array<T> arr = new Array<T>(Zero);

			for (int i = 0; i < m_count; i++)
				arr.Add(this[i]);

			return arr;
		}
	}

	internal class SListIterator<T>
	{
		public SListNode<T> m_node;
		public SLinkedList<T> m_list;

		public SListIterator(SLinkedList<T> list = null, SListNode < T> node = null)
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

		//move iterator to next node
		public void Forth()
		{
			if (m_node != null)
				m_node = m_node.m_next;
		}

		//Get the data of current node
		public T Item()
		{
			return m_node.m_data;
		}

		public bool Valid()
		{
			return(m_node != null);	
		}
	}
}
