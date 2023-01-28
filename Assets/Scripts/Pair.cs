using System.Collections;
using System.Runtime.CompilerServices;

namespace System
{
    public class Pair<T1, T2> {
        public Pair(T1 elem, T2 elemb)
        {
            Elem = elem; ElemB = elemb;
        }

        public Pair(Pair<T1,T2> p)
        {
            Elem = p.Elem; ElemB = p.ElemB;
        }
        
        public T1 Elem;
        public T2 ElemB;

        public override String ToString(){
            return "<"+Elem+","+ElemB+">";
        }
    }
}