// <copyright file="ScheduleReq.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.SchedulingAudit
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// This class models the ScheduleAudit.
    /// </summary>
    [Serializable]
    public class SchedulingAudit
    {
        /// <summary>
        /// Gets or sets the list of Employees.
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public List<PersonIdentity> Employees { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets the QueryDateSpan.
        /// </summary>
        [XmlAttribute]
        public string QueryDateSpan { get; set; }

        /// <summary>
        /// Gets or sets the Type.
        /// </summary>
        [XmlAttribute]
        public string Type { get; set; }

    }
}