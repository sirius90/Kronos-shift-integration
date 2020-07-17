﻿// <copyright file="Employees.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SwapShift.FetchApprovals.SwapShiftData
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// This class models the Employees.
    /// </summary>
    [XmlRoot(ElementName = "Employees")]
    public class Employees
    {
        /// <summary>
        /// Gets or sets the PersonIdentity.
        /// </summary>
        [XmlElement(ElementName = "PersonIdentity")]
#pragma warning disable CA2227 // Collection properties should be read only
        public List<PersonIdentity> PersonIdentity { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}