using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Harris.GPC
{
	internal class Sorter <T> where T : IComparable<T>
	{
		//public static void BubbleSort(T[] array)

		public static void BubbleSort(Array<T> arr)
		{
			int n = arr.getCount();
			bool swapped;

			do
			{
				swapped = false;

				for (int i = 0; i < n - 1; i++)
				{
					if (arr[i].CompareTo(arr[i + 1]) > 0)
					{
						//Swap(ref x, ref y);
						Swap(arr, i, i+1);
						swapped = true;
					}
				}

				n--;
			} while (swapped);
		}

		public static void SelectionSort(Array<T> arr)
		{
			for (int partIndex = arr.getCount() - 1; partIndex > 0; partIndex--)
			{
				int largestAt = 0;
				for (int i = 1; i <= partIndex; i++)
				{
					if (arr[i].CompareTo(arr[largestAt]) > 0)
					{
						largestAt = i;
					}
				}
				Swap(arr, largestAt, partIndex);
			}
		}

		public static void InsertionSort(Array<T> array)
		{
			for (int partIndex = 1; partIndex < array.getCount(); partIndex++)
			{
				T curUnsorted = array[partIndex];
				int i = 0;

				for (i = partIndex; i > 0 && (array[i - 1].CompareTo(curUnsorted) > 0); i--)
				{
					array[i] = array[i - 1];
				}

				array[i] = curUnsorted;
			}
		}

		public static void ShellSort(Array<T> array)
		{
			int gap = 1;
			while (gap < array.getCount() / 3)
				gap = 3 * gap + 1;

			while (gap >= 1)
			{
				for (int i = gap; i < array.getCount(); i++)
				{
					for (int j = i; j >= gap && (array[j].CompareTo(array[j - gap]) < 0); j -= gap)
					{
						Swap(array, j, j - gap);
					}

				}
				gap /= 3;
			}
		}

		//Makes use of local functions
		public static void MergeSort(Array<T> array,T Zero)
		{
			Array<T> aux = new Array<T>(Zero);
			for (int i = 0; i < array.getCount(); i++)
				aux.Add(Zero);

			Sort(0, array.getCount()-1);

			void Sort(int low, int high)
			{
				if (high <= low)
					return;
				
				int mid = (high + low) / 2;
				Sort(low, mid);
				Sort(mid+1, high);
				Merge(low, mid, high);
			}

			void Merge(int low, int mid, int high)
			{
				if (array[mid].CompareTo(array[mid + 1]) <= 0)
					return;
				int i = low;
				int j = mid + 1;

				//Array Copy(sourceArray, sourceIndex, destArray, destIndex, length);
				Array<T>.Copy(array, low, aux, low, high - low + 1);

				for (int k = low; k <= high; k++)
				{
					if (i > mid) array[k] = aux[j++];
					else if (j > high) array[k] = aux[i++];
					else if (aux[j].CompareTo(aux[i]) < 0)
						array[k] = aux[j++];
					else
						array[k] = aux[i++];
				}
			}
		}

		public static void QuickSort(Array<T> array, T Zero)
		{
			Sort(0, array.getCount() - 1);
			void Sort(int low, int high)
			{
				if(high <= low) 
					return;

				int j = Partition(low, high);
				Sort(low, j - 1);
				Sort(j + 1, high);

				int Partition(int low, int high)
				{
					int i = low;
					int j = high + 1;

					T pivot = array[low];
					while (true)
					{
						while (array[++i].CompareTo(pivot) < 0)
						{
							if (i == high)
								break;
						}

						while (pivot.CompareTo(array[--j]) < 0)
						{
							if(j == low)
								break;
						}

						if(i >= j) 
							break;

						Swap(array, i, j);
					}
					Swap(array, low, j);
					return j;
				}
			}

		}

		private static void Swap(Array<T> array, int i, int j)
		{
			if (i == j)
				return;
			T temp = array[i];
			array[i] = array[j];
			array[j] = temp;
		}
	}
}
