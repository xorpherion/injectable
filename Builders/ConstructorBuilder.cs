using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bornium.Injectable.Builders
{
    public class ConstructorBuilder : ConstructableType
    {
        public ConstructorInfo AnnotatedConstructor { get; }

        public ConstructorBuilder(ConstructorInfo annotatedConstructor): base(annotatedConstructor.DeclaringType,new HashSet<ParameterInfo>(annotatedConstructor.GetParameters().ToList()))
        {
            AnnotatedConstructor = annotatedConstructor;
        }

        public override object Construct(Injector injector)
        {
            return AnnotatedConstructor.Invoke(AnnotatedConstructor.GetParameters().Select(p => InstanceFromParameterInfo(p,injector)).ToArray());
        }
        
        public override InjectableAttribute GetAttribute()
        {
            return AnnotatedConstructor.GetCustomAttribute<InjectableAttribute>();
        }
    }
}