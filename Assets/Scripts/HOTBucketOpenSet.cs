using System;
using Priority_Queue;

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
            TPriority FirstPrio { get; }
            int Count { get; }    */

    private SimplePriorityQueue<T,float> HotQueue;

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

    public float FirstPrio()
    {
        return HotQueue.FirstPrio;
    }

    public T First()
    {
        return HotQueue.First;
    }
}
