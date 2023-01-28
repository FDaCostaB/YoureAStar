using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PairList<T>
{
    public static IEnumerable<Pair<T,T>> uniquePair(List<T> list)
    {
        List<Pair<T,T>> res = new List<Pair<T, T>>();
        for(int i = 0; i< list.Count; i++)
        {
            for(int j = i+1; j< list.Count; j++)
            {
                yield return new Pair<T, T>(list[i], list[j]);
            }
        }
    }
}
