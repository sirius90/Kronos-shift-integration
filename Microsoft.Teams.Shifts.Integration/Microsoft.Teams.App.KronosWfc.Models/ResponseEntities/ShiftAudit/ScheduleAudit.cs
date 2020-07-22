// <copyright file="ScheduleUpcoming.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Shifts.ShiftAudit
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// This class models the schedule.
    /// </summary>
    public class ScheduleAudit
    {
        /// <summary>
        /// Gets or sets the ScheduleAuditItems.
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public List<ShiftAuditItem> ScheduleAuditItems { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets the employees.
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public List<PersonIdentity> Employees { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets the Type.
        /// </summary>
        [XmlAttribute]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the queryDateSpan.
        /// </summary>
        [XmlAttribute]
        public string QueryDateSpan { get; set; }
    }
}