using System.Collections;
using System.Runtime.CompilerServices;

namespace System
{
    public class Pair<T1, T2> {
        public Pair(T1 elem, T2 priority){
            Elem = elem; Priority = priority;
        }

        public Pair(Pair<T1,T2> p)
        {
            Elem = p.Elem; Priority = p.Priority;
        }
        
        public T1 Elem;
        public T2 Priority;

        public override String ToString(){
            return "<"+Elem+","+Priority+">";
        }
    }
}