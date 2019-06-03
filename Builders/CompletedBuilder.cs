using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bornium.Injectable.Builders
{
    public class CompletedBuilder : ConstructableType
    {
        public object Instance { get; }
        public InjectableAttribute Injectable { get; }

        public CompletedBuilder(Object instance, String name = null) : base(instance.GetType(), new HashSet<ParameterInfo>())
        {
            Instance = instance;
            Injectable = new InjectableAttribute(name);
        }

        public override object Construct(Injector injector)
        {
            return Instance;
        }

        public override InjectableAttribute GetAttribute()
        {
            return Injectable;
        }
    }
}