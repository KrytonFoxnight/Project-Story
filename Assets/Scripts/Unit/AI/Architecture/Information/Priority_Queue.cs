using System;
using System.Collections;
using System.Collections.Generic;

public class Priority_Queue<T> where T : IComparable<T>
{
    private List<T> data;

	public Priority_Queue()
	{
		this.data = new List<T>();
	}

	public void Enqueue(T item)
	{
		data.Add(item);
		int idx = data.Count - 1;
		while(idx > 0)
		{
			int parent_idx = (idx - 1) / 2;
			if(data[idx].CompareTo(data[parent_idx]) <= 0) break;

			T tmp = data[idx];
			data[idx] = data[parent_idx];
			data[parent_idx] = tmp;
			idx = parent_idx;
		}
	}

	public T Dequeue()
	{
		int idx = data.Count - 1;
		T item = data[0];
		data[0] = data[idx];
		data.RemoveAt(idx);

		--idx;
		int parent_idx = 0;

		while(true)
		{
			int cur_idx = parent_idx * 2 + 1;
			if(cur_idx > idx) break;

			// 왼쪽, 오른쪽 노드 중 가장 큰 값을 교환할 것으로 선정
			int right_idx = cur_idx + 1;
			if(right_idx <= idx && data[right_idx].CompareTo(data[cur_idx]) > 0)
			{
				cur_idx = right_idx;
			}

			// 부모의 score가 자식의 score보다 이미 큰 경우 heapify가 필요 없으므로 break
			if(data[parent_idx].CompareTo(data[cur_idx]) >= 0) break;

			// Max Heapify 진행
			T tmp = data[parent_idx];
			data[parent_idx] = data[cur_idx];
			data[cur_idx] = tmp;

			// 하위 노드로 Max Heapify를 수행
			parent_idx = cur_idx;
		}
		return item;
	}
	
	public T Peek()
	{
		return data[0];
	}

	public int Count()
	{
		return data.Count;
	}
}
