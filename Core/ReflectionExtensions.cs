using System.Reflection;

namespace GoogleChromePortable.Core
{
    static class ReflectionExtensions
    {
        public static TDelegate CreateDelegate<TDelegate>(this MethodInfo mi, object target = null)
        {
            return (TDelegate)(object)mi.CreateDelegate(typeof(TDelegate), target);
        }
    }
}
