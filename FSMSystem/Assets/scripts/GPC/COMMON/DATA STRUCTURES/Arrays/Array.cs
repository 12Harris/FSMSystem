using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.IO; // NEEDED FOR STREAMS

namespace Harris.GPC
{

	public class CustomObject : IComparable<CustomObject>
	{
		public int index;
		public string name;

		public CustomObject() { }

		public CustomObject(int index, string name)
		{
			this.index = index;
			this.name = name;
		}

		public override string ToString()
		{
			return index + ", " + name;
		}

		public int CompareTo(CustomObject other)
		{
			if (String.Compare(name ,other.name) < 0) return -1;
			if (String.Compare(name, other.name) == 0) return 0;
			return 1;
		}

	}

	internal class Array<T>
	{
		private List<T> arr;
		public T Zero { get; set; }

		//Indexer
		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= arr.Count)
				{
					return Zero;
				}
				return arr[index];
			}

			set
			{
				if (index >= 0 && index < arr.Count)
					arr[index] = value;
			}
		}

		public T Last { get => arr[arr.Count - 1]; }

		public Array(T zero)
		{
			arr = new List<T>();
			Zero = zero;
			//Console.WriteLine("Called Array Construtor");
			//OUTPUTFN = curDir + "\\Output\\output.txt";
		}

		public void Add(T elem)
		{
			arr.Add(elem);
		}

		public void Insert(T elem, int index)
		{
			arr.Insert(index, elem);
		}

		public void Remove(T elem)
		{
			arr.Remove(elem);
		}

		public void RemoveAt(int index)
		{
			if (arr.Count > index)
				arr.RemoveAt(index);
			else
				Console.WriteLine("error removing element");
		}

		public void AddValues(params T[] _elements)
		{
			foreach (T element in _elements)
				Add(element);
		}

		public int getCount()
		{
			return arr.Count;
		}

		public void Clear()
		{
			arr.Clear();
		}

		//Define an implicit operator from Array<T> to List<T>
		public static implicit operator List<T>(Array<T> array)
		{
			List<T> result = new List<T>();
			for (int i = 0; i < array.getCount(); i++)
			{
				result.Add(array[i]);
			}
			return result;
		}

		public void Show()
		{
			for (int i = 0; i < arr.Count; i++)
			{
				Console.Write(arr[i]);
				if (i < arr.Count - 1)
				{
					Console.Write(" ");
				}
				else
				{
					Console.WriteLine();
				}
			}
		}

		public override string ToString()
		{
			string result = string.Empty;
			for (int i = 0; i < arr.Count; i++)
			{
				result += arr[i].ToString();
				if (i < arr.Count - 1)
				{
					result += " ";
				}
			}
			return result;
		}

		public void readArray(string fileName)
		{
			arr.Clear();
			arr = ObjectSerializer.DeSerializeObject<List<T>>(fileName);
			Console.WriteLine(this);
		}

		public void writeArray(string fileName)
		{
			ObjectSerializer.SerializeObject(arr, fileName);
		}

		public List<T> GetArr()
		{
			return arr;
		}

		//Static method to copy array elements to other array
		public static void Copy(Array<T> sourceArray, int sourceIndex, Array<T> destArray, int destIndex, int length)
		{
			if (sourceIndex < 0 || (sourceIndex + length) > sourceArray.getCount())
			{
				throw new ArgumentOutOfRangeException();
			}

			for (int i = 0; i < length; i++)
				destArray.RemoveAt(destIndex);

			int insertIndex = destIndex;

			for (int i = sourceIndex; i < (sourceIndex + length); i++)
			{
				destArray.Insert(sourceArray[i], destIndex++);
			}

		}
	}
}
