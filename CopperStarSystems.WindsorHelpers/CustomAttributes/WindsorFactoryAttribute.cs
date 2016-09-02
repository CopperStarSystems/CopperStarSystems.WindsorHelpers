// --------------------------------------------------------------------------------------
// <copyright file="WindsorFactoryAttribute.cs" company="Copper Star Systems, LLC">
//    Copyright 2016 Copper Star Systems, LLC. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------

using System;

namespace CopperStarSystems.WindsorHelpers.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class WindsorFactoryAttribute : Attribute
    {
    }
}