using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bornium.Injectable
{
    public abstract class ConstructableType
    {
        public Type Constructable { get; set; }
        public HashSet<Type> MissingDependencies { get; set; }
        public Dictionary<String,Type> MissingNamedDependencies { get; set; }

        protected ConstructableType(Type constructable, HashSet<ParameterInfo> missingDependencies)
        {
            Constructable = constructable;
            MissingDependencies = new HashSet<Type>(missingDependencies
                .Where(p => p.GetCustomAttributes(typeof(InjectableAttribute), false).Length == 0)
                .Select(p =>
                {
                    if (TypeIsANonInheritedCollection(p.ParameterType))
                        return typeof(IEnumerable<>).MakeGenericType(p.ParameterType.GetGenericArguments()[0]);
                    return p.ParameterType;
                })
                .ToList());

            MissingNamedDependencies = missingDependencies
                .Where(p => p.GetCustomAttributes(typeof(InjectableAttribute),false).Length > 0)
                .Select(p => Tuple.Create(p.GetCustomAttribute<InjectableAttribute>().Name,p.ParameterType))
                .ToDictionary(t => t.Item1, t => t.Item2);
        }

        public abstract object Construct(Injector injector);

        public abstract InjectableAttribute GetAttribute();

        public bool DependenciesFullfilled()
        {
            return MissingDependencies.Count == 0 && MissingNamedDependencies.Count == 0;
        }
        
        protected object InstanceFromParameterInfo(ParameterInfo t, Injector injector)
        {
            if (HasInjectableAttribute(t))
                return GetNamedInstance(t, injector);
            if (TypeIsANonInheritedCollection(t.ParameterType))
                return GetParameterAsIEnumerable(t, injector);
           
            return GetInstance(t, injector);
        }

        private static object GetInstance(ParameterInfo t, Injector injector)
        {
            return injector.Get(t.ParameterType);
        }

        private static object GetNamedInstance(ParameterInfo t, Injector injector)
        {
            return injector.Get(t.GetCustomAttribute<InjectableAttribute>().Name);
        }

        private static object GetParameterAsIEnumerable(ParameterInfo t, Injector injector)
        {
            return t.ParameterType.GetConstructor(new []{typeof(IEnumerable<>).MakeGenericType(t.ParameterType.GetGenericArguments()[0])}).Invoke(new []{injector.GetList(t.ParameterType.GetGenericArguments()[0])});
        }

        private static bool HasInjectableAttribute(ParameterInfo t)
        {
            return t.GetCustomAttribute<InjectableAttribute>() != null;
        }

        private static bool TypeIsANonInheritedCollection(Type t)
        {
            return t.GetInterface(nameof(IEnumerable)) != null && t.BaseType.GetInterface(nameof(IEnumerable)) == null;
        }
    }
}