using System.Collections.Generic;
using UnityEngine;

namespace Harris.GPC
{

    public class Event : ISubject
    {
        private List<IEventListener> elisteners = new List<IEventListener>();

        public void Attach (IObserver listener)
        {
            elisteners.Add(listener as IEventListener);
        }

        public void Detach (IObserver listener)
        {
            elisteners.Remove(listener as IEventListener);
        }

        public virtual void Notify()
        {
            for(int i = 0; i < elisteners.Count; i++)
            {
                elisteners[i].Update(this);
            }
        }
    }

    public interface IEventListener : IObserver
    {
    
    }
}