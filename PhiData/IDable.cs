using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace PhiClient
{
    public interface IDable
    {
        int getID();
    }

    public class ID
    {
        public static E Find<E>(List<E> list, int id) where E : IDable
        {
            E elem = TryFind(list, id);

            if (elem == null)
            {
                throw new Exception("Entity with ID " + id + " not found in " + list.ToString());
            }
            return elem;
        }

        public static E TryFind<E>(List<E> list, int id) where E : IDable
        {
            foreach (E e in list)
            {
                if (e.getID() == id)
                {
                    return e;
                }
            }

            return default(E);
        }
    }
}
