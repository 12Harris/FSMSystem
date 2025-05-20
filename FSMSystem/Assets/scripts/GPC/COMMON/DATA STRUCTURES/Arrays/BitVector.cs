using System;
// System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harris.GPC
{
	internal class BitVector
	{
		private Array<long> arr;

		public int Size {
			get
			{
				if (arr.getCount() > 0)
					return arr.getCount();
				return 0;
			}

		}

		public BitVector(int p_size)
		{
			arr = new Array<long>(0);
			Resize(p_size);
		}

		~BitVector()
		{
			arr.Clear();
		}

		public void Resize(int p_size)
		{
			if (p_size % 32 == 0)
			{
				p_size = p_size / 32;
			}
			else
			{
				p_size = (p_size / 32) + 1;
			}
			Array<long> arr2 = new Array<long>(0);
			for(int i = 0; i < p_size; i++)
				arr2.Add(0);

			int min;
			if (p_size < Size)
				min = p_size;
			else
				min = Size;

			int index;

			for(index = 0; index< min; index++)
				arr2[index] = arr[index];

			arr = arr2;
		}

		public bool this[int index]
		{
			get
			{
				int cell = index / 32;
				int bit = index % 32;
				long a = arr[cell] & ((1 << bit));
				bool b = Convert.ToBoolean(a >> bit);
				return b;
			}
		}

		public void Set(int index, bool value)
		{
			int cell = index / 32;
			int bit = index % 32;

			if (value == true)
			{
				arr[cell] = (arr[cell] | ((long)1 << bit));
			}
			else
			{
				arr[cell] = (arr[cell] & (~(1 << bit)));
			}
		}

		public void ClearAll()
		{
			int index;
			for (index = 0; index < Size; index++)
			{
				arr[index] = 0;
			}
		}

		public void SetAll()
		{
			int index;
			for (index = 0; index < Size; ++index)
			{
				arr[index] = 0xFFFFFFFF;//8 bits = 0xFF
			}
		}

		public override string ToString()
		{
			string binary = "";

			for (int i = 0; i < Size; i++)
			{
				binary += Convert.ToString(arr[i], 2);

				if(i < Size - 1)
					binary += " ";
			}
			return binary;
		}
	}
}
