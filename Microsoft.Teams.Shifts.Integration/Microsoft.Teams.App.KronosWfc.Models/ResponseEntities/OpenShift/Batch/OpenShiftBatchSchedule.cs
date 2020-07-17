﻿// <copyright file="OpenShiftBatchSchedule.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.OpenShift.Batch
{
    using System.Xml.Serialization;

    /// <summary>
    /// This class models the Schedule.
    /// </summary>
    [XmlRoot(ElementName = "Schedule")]
    public class OpenShiftBatchSchedule
    {
        /// <summary>
        /// Gets or sets the QueryDateSpan.
        /// </summary>
        [XmlAttribute(AttributeName = "QueryDateSpan")]
        public string QueryDateSpan { get; set; }

        /// <summary>
        /// Gets or sets the OrgJobPath.
        /// </summary>
        [XmlAttribute(AttributeName = "OrgJobPath")]
        public string OrgJobPath { get; set; }

        /// <summary>
        /// Gets or sets the ScheduleItems.
        /// </summary>
        [XmlElement(ElementName = "ScheduleItems")]
        public ScheduleItems ScheduleItems { get; set; }
    }
}