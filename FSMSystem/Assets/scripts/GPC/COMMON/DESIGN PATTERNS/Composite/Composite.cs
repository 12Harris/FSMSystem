using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;

namespace Harris.GPC
{
    public interface IComponent <T>
    {
        void           Add(IComponent <T> c);
        IComponent <T> Remove(T s);
        IComponent <T> Find(T s);
        string         Display(int depth);
        T              Name {get; set;}
        IComponent <T> Start();
        void Update();
        static int Count{get;set;} = 0;
        int ID{get;set;}
        int Level{get;set;}
        void AssignLevels(int level);

        public static bool operator > (IComponent<T> lhs,IComponent<T> rhs) {

            return lhs.ID > rhs.ID;
        }

        public static bool operator < (IComponent<T> lhs,IComponent<T> rhs) {

            return lhs.ID < rhs.ID;
        }
    }

    [Serializable]
    // The Component (atomic)
    public class Component <T> : IComponent <T> {
        public T Name {get; set;}

        public int ID {get; set;} = 0;

        public static int Count{get;set;} = 1;

        public int Level{get;set;} = 0;

        public Component (T name)  {
            Name = name;
        }
        public void Add(IComponent <T> c) {
            Debug.Log("Cannot add to an item");
        }
        public IComponent <T>  Remove(T s) {
            Debug.Log("Cannot remove directly");
            return this;
        }
        public string Display(int depth) {
            return new String('-', depth) + Name+"\n";
        }
        public IComponent <T>  Find (T s) {
            if (s.Equals(Name))
            return this;
            else
            return null;
        }

        public IComponent <T> Start()
        {
            return this;
        }

        public virtual void Update()
        {
          
        }

        public void GenerateID(int id)
        {
            ID = id;
        }

        public void AssignLevels(int level)
        {
            Level = level;
        }
    }

    [Serializable]
     // The Composite
    public class Composite <T> : IComponent  <T> 
    {
        public List  <IComponent <T>> list;
        public T Name {get; set;}

        private static int ComponentCount = 0;
        public int ID{get;set;} = 0;

        public static int Count{get;set;} = 0;

        public static int CurrentID{get;set;} = 0;

        public int Level{get;set;} = 0;
        
        public Composite (T name) 
        {
            Name = name;
            list = new List<IComponent <T>> ( );
        }

        //ToDo: 2 add methods: 1 for adding composite,
        //another for adding atomic Component
        public void Add(IComponent  <T> c) 
        {
            list.Add(c);
        }

         IComponent <T> holder=null;
        // Finds the item from a particular point in the structure
        // and returns the composite from which it was removed
        // If not found, return the point as given
        public IComponent <T> Remove(T s)
        {
            holder = this;
            IComponent <T> p = holder.Find(s);
            if (holder!=null) 
            {
                (holder as Composite<T>).list.Remove(p);
                return holder;
            }
            else
                return this;
        }

        // Recursively looks for an item
        // Returns its reference or else null
        public IComponent <T>  Find (T s) 
        {
            holder = this;
            if (Name.Equals(s)) return this;
            IComponent <T> found=null;
            foreach (IComponent <T> c in list)
            {
                found = c.Find(s);
                if (found!=null)
                break;
            }
            return found;
        }

        public IComponent <T> Start()
        {
            IComponent <T> start=null;

            start = list[0].Start();

            return start;
        }

        public void GenerateID(int id)
        {
            ID = id;
            
            int i = id;

            Debug.Log("assigned id = " + ID);

            foreach (IComponent <T> component in list) {
                
                i++;

                if(component is Composite<T>)
                    (component as Composite<T>).GenerateID(i);
                else
                    (component as Component<T>).GenerateID(i);
                
            }

            Count = i;
            
        }

        public void AssignLevels(int level)
        {
            Level = level;

            Debug.Log("assigned level = " + level);

            foreach (IComponent <T> component in list) {
                component.AssignLevels(level+1);
            }

        }

        // Displays items in a format indicating their level in the composite structure
        public string Display(int depth)
        {
            StringBuilder s = new StringBuilder(new String('-', depth));
            s.Append("Set "+ Name +  " length :" + list.Count + "\n");
            foreach (IComponent <T> component in list) {
                s.Append(component.Display(depth + 2));
            }
            return s.ToString( );
        }

        public static void AddToArray(List<IComponent<T>> ls, IComponent<T> component)
        {
            ls.Add(component);

            if(component is Composite<T>)
            {
                foreach (IComponent<T> child in (component as Composite<T>).list) {
                    CurrentID++;
                    AddToArray(ls,child);
                }
            }
        }

        public static List<IComponent<T>> ToArray(Composite<T> root)
        {
            //IComponent<T>[] arr = new IComponent<T>(Count+1);

            List<IComponent<T>> ls = new List<IComponent<T>>();

            ls.Add(root);

            CurrentID = 1;

            foreach (IComponent <T> component in root.list) {
  
                AddToArray(ls, component);
            }

            return ls;
        }

        public virtual void Update()
        {
            
        }
    }
    public static class CompositePatternExample
    {
        public static void Run()
        {
                IComponent <string> album = new Composite<string> ("Album");
            IComponent <string> point = album;
            string [] s;
            string command, parameter;
            // Create and manipulate a structure
            StreamReader instream = new StreamReader("Composite.dat");

            do {
                string t = instream.ReadLine( );
                Console.WriteLine("\t\t\t\t"+t);
                s = t.Split( );
                    command = s[0];
                if (s.Length>1) parameter = s[1]; else parameter = null;
                switch (command) {
                case "AddSet" :
                    IComponent <string> c = new Composite <string> (parameter);
                    point.Add(c);
                    point = c;
                    break;
                case "AddPhoto" :
                    point.Add(new Component <string> (parameter));
                    break;
                case "Remove" :
                    point = point.Remove(parameter);
                    break;
                case "Find" :
                    point = album.Find(parameter);
                    break;
                case "Display" :
                    Console.WriteLine(album.Display(0));
                    break;
                case "Quit" :
                    break;
                }
            } while (!command.Equals("Quit"));

        }
        
    }
}



namespace Harris.Obsolete
{
    public abstract class Component
    {
        protected string PartName;
        public Component(string partName)
        {
            PartName = partName;
        }

        public abstract void Add(Component component);
        public abstract void Remove(Component component);

        public virtual void WireUp(int length, string gauge)
        {
            string wire = "";
            for (int i = 1; i < length; i++)
                wire += " ";
            wire += gauge;
            Console.WriteLine(wire + PartName);
        }
    }

    class Composite : Component
    {
        private List<Component> subComps = new List<Component>();

        public Composite(string partName) : base(partName) { }

        public override void Add(Component component)
        {
            subComps.Add(component);
        }

        public override void Remove(Component component)
        {
            subComps.Remove(component);
        }

        public override void WireUp(int length, string gauge)
        {
            base.WireUp(length, gauge);
            foreach (Component component in subComps)
                component.WireUp(length + 1, gauge);
        }
    }

    class Leaf : Component
    {
        public Leaf(string partName) : base(partName) { }

        public override void Add(Component component)
        {
            throw new Exception("Attaching Component to Leaf Not Allowed!");
        }
        public override void Remove(Component component)
        {
            throw new Exception("Removing Component from Leaf Not Allowed!");
        }

        public override void WireUp(int length, string gauge)
        {
            base.WireUp(length, gauge);
        }
    }

}
