using System;
using Priority_Queue;
using UnityEditor.Experimental.GraphView;

public class HOTBucketOpenSet<T> : IOpenList<T>
{
    /*	SimplePriorityQueue<T> Interface
         	void Enqueue(TItem node, TPriority priority);
            TItem Dequeue();
            void Clear();
            bool Contains(TItem node);
            void Remove(TItem node);
            void UpdatePriority(TItem node, TPriority priority);
            TItem First { get; }
            int Count { get; }    */

    private SimplePriorityQueue<T> HotQueue;
    public HOTBucketOpenSet() {
        HotQueue = new SimplePriorityQueue<T>();
    }
    public int size {get {return HotQueue.Count;}}

    public void changePriority(T item, float p)
    {
        HotQueue.UpdatePriority(item, p);
    }

    public T Dequeue()
    {
        return HotQueue.Dequeue();
    }

    public void Enqueue(T elem, float p)
    {
        HotQueue.Enqueue(elem, p);
    }

    public bool Contains(T node)
    {
        return HotQueue.Contains(node);
    }

}
