using System;
using System.Collections;
using System.Collections.Generic;

namespace Bornium.Injectable
{
    public class Injector
    {
        Dictionary<Type,IList> instances = new Dictionary<Type, IList>();
        private Dictionary<String,object> Named { get; set; } = new Dictionary<string, object>();

        public void Add(Type t, object obj)
        {
            if(!instances.ContainsKey(t))
                instances.Add(t, CreateListFor(t));

            instances[t].Add(obj);
        }

        public void Add(Type[] types, object obj)
        {
            foreach (var type in types)
                Add(type,obj);
        }

        public void Add(String name, object obj)
        {
            Named.Add(name,obj);
        }

        private IList CreateListFor(Type type)
        {
            var listType = typeof(List<>);
            var constructedListType = listType.MakeGenericType(type);

            return Activator.CreateInstance(constructedListType) as IList;
        }

        public T Get<T>()
        {
            return (T) Get(typeof(T));
        }

        public object Get(Type t)
        {
            var list = GetList(t);
            if(list.Count > 1)
                Console.WriteLine("Ambiguous");
            return list[0];
        }

        public object Get(String name)
        {
            return Named[name];
        }
        
        public T Get<T>(String name)
        {
            return (T) Get(name);
        }
        
        public List<T> GetList<T>()
        {
            return (List<T>) GetList(typeof(T));
        }
        
        public IList GetList(Type t)
        {
            return instances[t];
        }
    }
}