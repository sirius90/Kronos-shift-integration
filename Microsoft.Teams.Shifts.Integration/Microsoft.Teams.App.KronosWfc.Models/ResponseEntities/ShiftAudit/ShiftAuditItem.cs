// <copyright file="PersonIdentity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Shifts.ShiftAudit
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// This class models the PersonIdentity.
    /// </summary>
    [Serializable]
    public class ShiftAuditItem
    {
        /// <summary>
        /// Gets or sets the WorkRule.
        /// </summary>
        [XmlAttribute]
        public string WorkRule { get; set; }

        /// <summary>
        /// Gets or sets the PersonNumber.
        /// </summary>
        [XmlAttribute]
        public string PersonNumber { get; set; }

        /// <summary>
        /// Gets or sets the Action.
        /// </summary>
        [XmlAttribute]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the SegmentEnd.
        /// </summary>
        [XmlAttribute]
        public string SegmentEnd { get; set; }

        /// <summary>
        /// Gets or sets the ShiftStart.
        /// </summary>
        [XmlAttribute]
        public string ShiftStart { get; set; }

        /// <summary>
        /// Gets or sets the SegmentStart.
        /// </summary>
        [XmlAttribute]
        public string SegmentStart { get; set; }

        /// <summary>
        /// Gets or sets the DataSource.
        /// </summary>
        [XmlAttribute]
        public string DataSource { get; set; }

        /// <summary>
        /// Gets or sets the SegmentType.
        /// </summary>
        [XmlAttribute]
        public string SegmentType { get; set; }

        /// <summary>
        /// Gets or sets the ShiftEnd.
        /// </summary>
        [XmlAttribute]
        public string ShiftEnd { get; set; }

        /// <summary>
        /// Gets or sets the ShiftEnd.
        /// </summary>
        [XmlAttribute]
        public string Job { get; set; }
    }
}