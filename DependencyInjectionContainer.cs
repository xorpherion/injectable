using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bornium.Injectable.Builders;

namespace Bornium.Injectable
{
    public class DependencyInjectionContainer : IDisposable
    {
        List<ConstructableType> blueprints = new List<ConstructableType>();

        public Injector Injector { get; } = new Injector();

        public DependencyInjectionContainer(Assembly[] assemblies, params CompletedBuilder[] initialLoad)
        {
            blueprints.AddRange(initialLoad);
            FindBlueprints(assemblies);
            TryToConstructFromBlueprints();
        }

        private void FindBlueprints(Assembly[] assemblies)
        {
            var types = GetAllTypes(assemblies);

            var annotatedMethodsTask =
                Task.Factory.StartNew(() => GetAllInjectableAnnotatedMethods(GetAllDeclaredMethods(types)));

            var annotatedConstructorsTask =
                Task.Factory.StartNew(() => GetAllInjectableAnnotatedConstructors(GetAllConstructors(types)));

            Task.WaitAll(annotatedMethodsTask, annotatedConstructorsTask);

            var annotatedMethods = annotatedMethodsTask.Result;
            var annotatedConstructors = annotatedConstructorsTask.Result;

            foreach (var annotatedMethod in annotatedMethods)
                blueprints.Add(new MethodBuilder(annotatedMethod));


            foreach (var annotatedConstructor in annotatedConstructors)
                blueprints.Add(new ConstructorBuilder(annotatedConstructor));
        }

        private void TryToConstructFromBlueprints()
        {
            bool goOn = true;

            while (goOn)
            {
                goOn = false;
                foreach (var blueprint in new List<ConstructableType>(blueprints))
                {
                    if (blueprint.DependenciesFullfilled())
                    {
                        goOn = true;
                        
                        foreach (var type in ConstructBlueprintAndGetTypesOfInstance(blueprint))
                            foreach (var b in blueprints)
                            {
                                b.MissingDependencies.Remove(type);
                                if (!ThereAreStillMissingInstancesToConstructForCollectionOf(type))
                                    b.MissingDependencies.Remove(TypeAsCollection(type));
                                if (blueprint.GetAttribute().Name != null)
                                    b.MissingNamedDependencies.Remove(blueprint.GetAttribute().Name);
                            }
                    }
                }
            }
        }

        private bool ThereAreStillMissingInstancesToConstructForCollectionOf(Type type)
        {
            return blueprints.Select(b1 => GetAllImplementingTypesForType(b1.Constructable)).SelectMany(l => l).Where(t => t != typeof(Object)).Select(t => t.FullName).ToList().Contains(type.FullName);
        }

        private Type[] ConstructBlueprintAndGetTypesOfInstance(ConstructableType blueprint)
        {
            var instanceTypes = GetAllImplementingTypesForType(blueprint.Constructable);
            AddInstanceToInjector(blueprint, instanceTypes, blueprint.Construct(Injector));

            blueprints.Remove(blueprint);
            return instanceTypes;
        }

        private void AddInstanceToInjector(ConstructableType blueprint, Type[] instanceTypes, object constructedInstance)
        {
            Injector.Add(instanceTypes, constructedInstance);
            if (blueprint.GetAttribute().Name != null)
                Injector.Add(blueprint.GetAttribute().Name, constructedInstance);
        }

        private static Type TypeAsCollection(Type type)
        {
            return typeof(IEnumerable<>).MakeGenericType(type);
        }

        private static Type[] GetAllImplementingTypesForType(Type t)
        {
            List<Type> types = new List<Type>();
            var current = t;
            while (current != null)
            {
                types.Add(current);
                current = current.BaseType;
            }

            return new[] {t.GetInterfaces(), types.ToArray()}.SelectMany(a => a).ToArray();
        }

        private static ConstructorInfo[] GetAllInjectableAnnotatedConstructors(ConstructorInfo[] constructors)
        {
            return constructors.Where(m => m.GetCustomAttributes(typeof(InjectableAttribute),false).Length > 0).ToArray();
        }

        private static MethodInfo[] GetAllInjectableAnnotatedMethods(MethodInfo[] methods)
        {
            return methods.Where(m => m.GetCustomAttributes(typeof(InjectableAttribute),false).Length > 0).ToArray();
        }

        private static ConstructorInfo[] GetAllConstructors(Type[] types)
        {
            return types.Select(t => t.GetConstructors()).SelectMany(c => c).ToArray();
        }

        private static MethodInfo[] GetAllDeclaredMethods(Type[] types)
        {
            return types.Select(t => t.GetMethods()).SelectMany(m => m).ToArray();
        }

        private static Type[] GetAllTypes(Assembly[] res)
        {
            return res.Select(a => a.GetTypes()).SelectMany(t => t).ToArray();
        }

        public void Dispose()
        {
            Injector.GetList<IDisposable>().ForEach(d =>  d.Dispose());
        }
    }
}