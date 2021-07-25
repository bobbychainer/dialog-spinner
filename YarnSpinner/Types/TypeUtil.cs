using System;

namespace Yarn
{
    /// <summary>
    /// Contains utility methods for working with types and their methods.
    /// </summary>
    internal static class TypeUtil
    {
        internal static System.Delegate GetMethod<TResult>(System.Func<Value, Value, TResult> f) => f;

        internal static System.Delegate GetMethod<T>(System.Func<Value, T> f) => f;

        internal static System.Delegate GetMethod<T>(System.Func<T> f) => f;

        internal static IType FindImplementingTypeForMethod(IType type, string methodName)
        {
            if (type is null)
            {
                throw new System.ArgumentNullException(nameof(type));
            }

            if (string.IsNullOrEmpty(methodName))
            {
                throw new System.ArgumentException($"'{nameof(methodName)}' cannot be null or empty.", nameof(methodName));
            }

            var currentType = type;

            // Walk up the type hierarchy, looking for a type that
            // implements a method by this name
            while (currentType != null)
            {

                if (currentType.Methods != null && currentType.Methods.ContainsKey(methodName))
                {
                    return currentType;
                }

                currentType = currentType.Parent;
            }

            return null;
        }

        internal static string GetCanonicalNameForMethod(IType implementingType, string methodName)
        {
            if (implementingType is null)
            {
                throw new System.ArgumentNullException(nameof(implementingType));
            }

            if (string.IsNullOrEmpty(methodName))
            {
                throw new System.ArgumentException($"'{nameof(methodName)}' cannot be null or empty.", nameof(methodName));
            }

            return $"{implementingType.Name}.{methodName}";
        }

        internal static void GetNamesFromCanonicalName(string canonicalName, out string typeName, out string methodName)
        {
            if (string.IsNullOrEmpty(canonicalName))
            {
                throw new System.ArgumentException($"'{nameof(canonicalName)}' cannot be null or empty.", nameof(canonicalName));
            }

            var components = canonicalName.Split(new[] { '.' }, 2);

            if (components.Length != 2)
            {
                throw new System.ArgumentException($"Invalid canonical method name {canonicalName}");
            }

            typeName = components[0];
            methodName = components[1];
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="subType"/> is
        /// equal to <paramref name="parentType"/>, or if <paramref
        /// name="parentType"/> exists in <paramref name="subType"/>'s type
        /// hierarchy.
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="subType"></param>
        /// <returns></returns>
        internal static bool IsSubType(IType parentType, IType subType)
        {
            if (subType == BuiltinTypes.Undefined && parentType == BuiltinTypes.Any) {
                // Special case: the undefined type is always a subtype of
                // the Any type, because ALL types are a subtype of the Any
                // type.
                return true;
            }

            if (subType == BuiltinTypes.Undefined) {
                // The subtype is undefined. Assume that it is not a
                // subtype of parentType.
                return false;
            }

            var currentType = subType;

            while (currentType != null) {
                // TODO: this is a strict object comparison; a more
                // sophisticated type unification might be better
                if (currentType == parentType) {
                    return true;
                }
                currentType = currentType.Parent;
            }
            return false;
        }
    }
}
