
// C# code to implement priority-queue
// using array implementation of
// binary heap
using System;
using System.Collections.Generic;
using UnityEngine;
 
class PriorityQueue<T> : IOpenList<T> {
 
    Pair<T,float> []H = new Pair<T,float>[64];
    public int size {get => head+1;}
    int head =-1;
    private int length = 64;

    int adjust(int c, int i) {
		while (c <= i) {
			c *= 2;
		}
		return c;
	}

	void resize(int idx) {
		if (length <= idx) {
            int oldSize = length;
			int newSize = adjust(oldSize, idx);
            Debug.LogWarning("Resized : "+ newSize);
			Pair<T,float> []newTab = new Pair<T,float>[newSize];
            for (int i=0; i<oldSize; i++)
                newTab[i] = H[i];
			H = newTab;
            length = newSize;
		}
	}

    public bool Exist(Predicate<T> p){
        for(int i=0;i<size;i++)
            if(p(H[i].Elem))return true;
        return false;
    }

    public int Find(Predicate<T> p){
        for(int i=0;i<size;i++)
             if(p(H[i].Elem))return i;
        return -1;
    }

    public T FindItem(Predicate<T> p)
    {
        for (int i = 0; i < size; i++)
            if (p(H[i].Elem)) return H[i].Elem;
        return default(T);
    }

    // Function to return the index of the
    // parent node of a given node
    int parent(int i)
    {
        return (i - 1) / 2;
    }
    
    // Function to return the index of the
    // left child of the given node
    int leftChild(int i)
    {
        return ((2 * i) + 1);
    }
    
    // Function to return the index of the
    // right child of the given node
    int rightChild(int i)
    {
        return ((2 * i) + 2);
    }
    
    // Function to shift up the
    // node in order to maintain
    // the heap property
    void shiftUp(int i)
    {
        while (i > 0 && H[parent(i)].ElemB > H[i].ElemB)
        {
            
            // Swap parent and current node
            swap(parent(i), i);
        
            // Update i to parent of i
            i = parent(i);
        }
    }
    
    // Function to shift down the node in
    // order to maintain the heap property
    void shiftDown(int i)
    {
        int maxIndex = i;
        
        // Left Child
        int l = leftChild(i);
        
        if (l < size && H[l].ElemB < H[maxIndex].ElemB)
        {
            maxIndex = l;
        }
        
        // Right Child
        int r = rightChild(i);
        
        if (r < size && H[r].ElemB < H[maxIndex].ElemB)
        {
            maxIndex = r;
        }
        
        // If i not same as maxIndex
        if (i != maxIndex)
        {
            swap(i, maxIndex);
            shiftDown(maxIndex);
        }
    }
    
    // Function to insert a
    // new element in
    // the Binary Heap
    public void Enqueue(T elem, float p)
    {
        resize(++head);
        H[head] = new Pair<T,float>(elem,p);
        
        // Shift Up to maintain
        // heap property
        shiftUp(head);
    }
    
    // Function to extract
    // the element with
    // maximum priority (minimal value)
    public T Dequeue()
    {
        T result = H[0].Elem;
        
        // Replace the value
        // at the root with
        // the last leaf
        swap(0,head--);
        
        // Shift down the replaced
        // element to maintain the
        // heap property
        shiftDown(0);
        return result;
    }
    
    // Function to change the priority
    // of an element
    public void changePriority(T item, float p)    {
        int i = Find(listItem => listItem.Equals(item));
        float oldp = H[i].ElemB;
        H[i].ElemB = p;
        
        if (p < oldp)
        {
            shiftUp(i);
        }
        else
        {
            shiftDown(i);
        }
    }
    
    // Function to get value of
    // the current minimum element
    float getMin()
    {
        return H[0].ElemB;
    }
    
    // Function to remove the element
    // located at given index
    void Remove(int i)
    {
        H[i].ElemB = getMin() + 1;
        
        // Shift the node to the root
        // of the heap
        shiftUp(i);
        
        // Extract the node
        Dequeue();
    }
    
    void swap(int i, int j)
    {
        Pair<T, float> temp = new Pair<T, float>(H[i]);
        H[i].Elem = H[j].Elem;
        H[i].ElemB = H[j].ElemB;
        H[j].Elem = temp.Elem;
        H[j].ElemB = temp.ElemB;
    }

    public override String ToString(){
        Queue<int> toVisit = new Queue<int>();
        toVisit.Enqueue(0);
        String res = "";
        int curr=0;
        while(toVisit.Count>0 && curr<size){
            curr = toVisit.Dequeue();
            if(leftChild(curr)<size)toVisit.Enqueue(leftChild(curr));
            if(rightChild(curr)<size)toVisit.Enqueue(rightChild(curr));
            res += H[curr].ToString()+" ,";
        }
        return res;
    }

    public float FirstPrio(){
        return H[0].ElemB;
    }

    public T First()
    {
        return H[0].Elem;
    }

    public bool Contains(T elem)
    {
        return Exist(curr => curr.Equals(elem));
    }

}
 
// This code is inspired by GeeksForGeeks and was contributed by Amit Katiyar 