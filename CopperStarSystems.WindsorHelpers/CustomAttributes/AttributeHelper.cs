// --------------------------------------------------------------------------------------
// <copyright file="AttributeHelper.cs" company="Copper Star Systems, LLC">
//    Copyright 2016 Copper Star Systems, LLC. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CopperStarSystems.WindsorHelpers.CustomAttributes
{
    public static class AttributeHelper
    {
        public static IEnumerable<Type> GetInterfacesWithSpecificAttribute(string assemblyName, Type attributeType)
        {
            var assembly = Assembly.Load(assemblyName);
            return assembly.GetExportedTypes().Where(p => p.IsInterface && Attribute.IsDefined(p, attributeType));
        }
    }
}