using UnityEngine;
using System;
using System.Collections.Generic;

public static class ReflectionUtil
{

    public static void SearchType<T> (ref List<Type> result)
    {
        // Search through all of the assemblies to find any types that derive from AnimStateInfo.
        result.Clear();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; ++i)
        {
            var assemblyTypes = assemblies[i].GetTypes();
            for (int j = 0; j < assemblyTypes.Length; ++j)
            {
                // Must derive from type T;
                if (!typeof(T).IsAssignableFrom(assemblyTypes[j]))
                {
                    continue;
                }

                // Ignore abstract classes.
                if (assemblyTypes[j].IsAbstract)
                {
                    continue;
                }

                result.Add(assemblyTypes[j]);
            }
        }
    }
}
