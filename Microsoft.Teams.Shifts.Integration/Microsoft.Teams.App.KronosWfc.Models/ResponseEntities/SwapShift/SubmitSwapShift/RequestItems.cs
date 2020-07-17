﻿// <copyright file="RequestItems.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SwapShift.SubmitSwapShift
{
    using System.Xml.Serialization;

    /// <summary>
    /// This class models the RequestItems.
    /// </summary>
    public class RequestItems
    {
        /// <summary>
        /// Gets or sets the EmployeeSwapShiftRequestItems.
        /// </summary>
        [XmlElement("EmployeeSwapShiftRequestItem")]
        public EmployeeSwapShiftRequestItem EmployeeSwapShiftRequestItems { get; set; }
    }
}