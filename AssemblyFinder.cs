using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bornium.Injectable
{
    public class AssemblyFinder
    {
        public  Assembly[] Assemblies { get; }
        private HashSet<string> loadedAssemblies = new HashSet<string>();

        public AssemblyFinder(params Assembly[] assemblies)
        {
            List<Assembly> start = new List<Assembly>(assemblies);
            start.Add(Assembly.GetEntryAssembly());

            Assemblies = GetAllAssemblies(start.ToArray());
        }

        private Assembly[] GetAllAssemblies(Assembly[] startAssemblies)
        {
            List<Assembly> result = new List<Assembly>(startAssemblies);
            
            result.AddRange(result.Select(GetAllReferencedAssemblies).SelectMany(a => a).ToArray());
            
            return result.ToArray();
        }

        private Assembly[] GetAllReferencedAssemblies(Assembly assembly)
        {
            loadedAssemblies.Add(assembly.GetName().ToString());
            
            var res = assembly.GetReferencedAssemblies().Where(name => !loadedAssemblies.Contains(name.ToString()))
                .Select(Assembly.Load).ToList();
            res.AddRange(res.Select(GetAllReferencedAssemblies).SelectMany(a=>a).ToArray());
            return res.ToArray();
        }
    }
}