using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bornium.Injectable.Builders
{
    public class MethodBuilder : ConstructableType
    {
        public MethodInfo AnnotatedMethod { get; }

        public MethodBuilder(MethodInfo annotatedMethod) : base(annotatedMethod.ReturnType,new HashSet<ParameterInfo>(annotatedMethod.GetParameters().ToList()))
        {
            AnnotatedMethod = annotatedMethod;
        }

        public override object Construct(Injector injector)
        {
            return AnnotatedMethod.Invoke(CreateContainingObjectToCallMethodsOn(),GetMethodArguments(injector));
        }

        private object[] GetMethodArguments(Injector injector)
        {
            return AnnotatedMethod.GetParameters().Select(p => InstanceFromParameterInfo(p,injector)).ToArray();
        }

        private object CreateContainingObjectToCallMethodsOn()
        {
            return AnnotatedMethod.DeclaringType.GetConstructor(new Type[0]).Invoke(new object[0]);
        }

        public override InjectableAttribute GetAttribute()
        {
            return AnnotatedMethod.GetCustomAttribute<InjectableAttribute>();
        }

        
    }
}