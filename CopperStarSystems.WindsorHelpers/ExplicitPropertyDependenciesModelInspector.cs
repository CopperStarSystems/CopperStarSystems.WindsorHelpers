// --------------------------------------------------------------------------------------
// <copyright file="ExplicitPropertyDependenciesModelInspector.cs" company="Copper Star Systems, LLC">
//    Copyright 2016 Copper Star Systems, LLC. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Core;
using Castle.Core.Internal;
using Castle.MicroKernel;
using Castle.MicroKernel.ModelBuilder;
using CopperStarSystems.WindsorHelpers.CustomAttributes;

namespace CopperStarSystems.WindsorHelpers
{
    public class ExplicitPropertyDependenciesModelInspector : IContributeComponentModelConstruction
    {
        public void ProcessModel(IKernel kernel, ComponentModel model)
        {
            InspectProperties(model);
        }

        protected void InspectProperties(ComponentModel model)
        {
            if (!ModelIsConfiguredForInspection(model))
                return;

            var targetType = GetModelType(model);
            var properties = GetProperties(model, targetType);

            if (!InjectablePropertiesExist(properties))
                return;

            AddPropertyDependenciesToModel(model, properties);
        }

        static Type GetModelType(ComponentModel model)
        {
            return model.Implementation;
        }

        static ICollection<PropertyDependencyFilter> GetPropertyFilters(ComponentModel model)
        {
            return StandardPropertyFilters.GetPropertyFilters(model, false);
        }

        static bool InjectablePropertiesExist(PropertyInfo[] properties)
        {
            return properties.Length > 0;
        }

        static bool ModelIsConfiguredForInspection(ComponentModel model)
        {
            return model.InspectionBehavior != PropertiesInspectionBehavior.None;
        }

        static bool PropertyFiltersExist(ICollection<PropertyDependencyFilter> filters)
        {
            return filters != null;
        }

        void AddAllPropertyDependenciesToModel(ComponentModel model, PropertyInfo[] properties)
        {
            properties.ForEach(p => model.AddProperty(BuildDependency(p, true)));
        }

        void AddFilteredPropertyDependenciesToModel(ComponentModel model, ICollection<PropertyDependencyFilter> filters,
            PropertyInfo[] properties)
        {
            foreach (var dependencies in
                filters.Select(filter => filter.Invoke(model, properties, BuildDependency))
                    .Where(dependencies => dependencies != null))
                foreach (var dependency in dependencies)
                    model.AddProperty(dependency);
        }

        void AddPropertyDependenciesToModel(ComponentModel model, PropertyInfo[] properties)
        {
            var filters = GetPropertyFilters(model);
            if (PropertyFiltersExist(filters))
                AddFilteredPropertyDependenciesToModel(model, filters, properties);
            else
                AddAllPropertyDependenciesToModel(model, properties);
        }

        PropertySet BuildDependency(PropertyInfo propertyInfo, bool isOptional)
        {
            var dependency = new DependencyModel(propertyInfo.Name, propertyInfo.PropertyType, isOptional);
            return new PropertySet(propertyInfo, dependency);
        }

        PropertyInfo[] GetProperties(ComponentModel model, Type targetType)
        {
            BindingFlags bindingFlags;
            if (model.InspectionBehavior == PropertiesInspectionBehavior.DeclaredOnly)
                bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            else
                bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            var properties = targetType.GetProperties(bindingFlags);
            return properties.Where(IsValidPropertyDependency).ToArray();
        }

        bool HasInjectPropertyAttribute(PropertyInfo propertyInfo)
        {
            return propertyInfo.HasAttribute<InjectPropertyAttribute>();
        }

        bool IsIndexedProperty(PropertyInfo propertyInfo)
        {
            var parameters = propertyInfo.GetIndexParameters();
            return parameters.Length != 0;
        }

        bool IsSettable(PropertyInfo propertyInfo)
        {
            return propertyInfo.CanWrite && (propertyInfo.GetSetMethod() != null);
        }

        bool IsValidPropertyDependency(PropertyInfo propertyInfo)
        {
            return IsSettable(propertyInfo) && (IsIndexedProperty(propertyInfo) == false) &&
                   HasInjectPropertyAttribute(propertyInfo);
        }
    }
}