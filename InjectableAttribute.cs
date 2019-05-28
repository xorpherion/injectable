using System;

namespace Bornium.Injectable
{
    [AttributeUsage(
        AttributeTargets.Method |
        AttributeTargets.Constructor |
        AttributeTargets.Parameter)]
    public class InjectableAttribute : Attribute
    {
        public InjectableAttribute(string name = null)
        {
            Name = name;
        }

        public String Name { get; set; }
    }
}