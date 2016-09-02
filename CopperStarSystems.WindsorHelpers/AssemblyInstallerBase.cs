// --------------------------------------------------------------------------------------
// <copyright file="AssemblyInstallerBase.cs" company="Copper Star Systems, LLC">
//    Copyright 2016 Copper Star Systems, LLC. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CopperStarSystems.WindsorHelpers.CustomAttributes;

namespace CopperStarSystems.WindsorHelpers
{
    public abstract class AssemblyInstallerBase : IWindsorInstaller
    {
        protected AssemblyInstallerBase(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        protected string AssemblyName { get; }

        protected IWindsorContainer Container { get; private set; }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            Container = container;
            InstallInternal();
        }

        protected virtual BasedOnDescriptor RegisterAssemblyTypeOverrides(BasedOnDescriptor assemblyRegistration)
        {
            return assemblyRegistration;
        }

        protected virtual void RegisterSingletons()
        {
            RegisterSingletonsByAttribute();
        }

        protected virtual void RegisterTransientOverrides()
        {
        }

        void InstallInternal()
        {
            RegisterSingletons();
            RegisterFactories();
            RegisterTransientOverrides();
            RegisterAllAssemblyClasses();
        }

        void RegisterAllAssemblyClasses()
        {
            var basedOnDescriptor =
                Classes.FromAssemblyNamed(AssemblyName).Pick().WithService.DefaultInterfaces().LifestyleTransient();
            Container.Register(RegisterAssemblyTypeOverrides(basedOnDescriptor));
        }

        void RegisterEachTypeAsFactory(IEnumerable<Type> types)
        {
            foreach (var type in types)
                Container.Register(Component.For(type).AsFactory());
        }

        void RegisterEachTypeAsSingleton(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                var implementingType =
                    ((WindsorSingletonAttribute) Attribute.GetCustomAttribute(type, typeof(WindsorSingletonAttribute)))
                        .ImplementingType;
                Container.Register(Component.For(type).ImplementedBy(implementingType).LifestyleSingleton());
            }
        }

        void RegisterFactories()
        {
            var types = AttributeHelper.GetInterfacesWithSpecificAttribute(AssemblyName, typeof(WindsorFactoryAttribute));
            RegisterEachTypeAsFactory(types);
        }

        void RegisterSingletonsByAttribute()
        {
            var types = AttributeHelper.GetInterfacesWithSpecificAttribute(AssemblyName,
                typeof(WindsorSingletonAttribute));
            RegisterEachTypeAsSingleton(types);
        }
    }
}