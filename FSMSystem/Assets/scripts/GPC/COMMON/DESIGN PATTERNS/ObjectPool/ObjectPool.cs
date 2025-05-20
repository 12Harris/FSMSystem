using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Harris.GPC
{

    public class ObjectPool<T> where T : MonoBehaviour
    {
        // list to hold the objects
        private List<T> objectsList = new List<T>();
        //counter keeps track of the number of objects in the pool
        private int counter = 0;
        // max objects allowed in the pool
        public int maxObjects = 20;

        // returns the number of objects in the pool
        public int getCount()
        {
            return counter ;
        }

        // method to get object from the pool
        public T getObj()
        {
            // declare item
            T objectItem;
            // check if pool has any objects
            // if yes, remove the first object and return it
            // also, decrease the count
            if (counter > 0)
            {
                objectItem = objectsList[0] ;
                objectsList.RemoveAt(0) ;
                counter--;
                return objectItem;
            }
           return null;
        }

        // method to return object to the pool
        // if counter is less than the max objects allowed, add object to the pool
        // also, increment counter
        public void releaseObj(T item)
        {
            if(counter < maxObjects)
            {
            objectsList.Add(item);
            counter++;
            }           
        }
        
    }
}