using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject.Components;
using Ninject.Infrastructure;
using Ninject.Planning.Bindings;
using Ninject.Planning.Bindings.Resolvers;

namespace Ninject.ContravariantBindingResolver
{
    public class ContravariantBindingResolver : NinjectComponent, IBindingResolver
    {
        /// <summary>
        /// Returns any bindings from the specified collection that match the specified service.
        /// </summary>
        public IEnumerable<IBinding> Resolve(Multimap<Type, IBinding> bindings, Type service)
        {
            if (!service.IsGenericType)
            {
                return Enumerable.Empty<IBinding>();
            }

            Type genericType = service.GetGenericTypeDefinition();
            Type[] genericArguments = genericType.GetGenericArguments();
            if (genericArguments.Length != 1 ||
                !genericArguments[0].GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant))
            {
                return Enumerable.Empty<IBinding>();
            }

            Type argument = service.GetGenericArguments().Single();
            return from kvp in bindings
                from value in kvp.Value
                let key = kvp.Key
                let genericArgument = key.GetGenericArguments().Single()
                where
                key.IsGenericType && key.GetGenericTypeDefinition() == genericType && genericArgument != argument &&
                genericArgument.IsAssignableFrom(argument)
                select value;
        }
    }
}
