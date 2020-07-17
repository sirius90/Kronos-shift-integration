﻿// <copyright file="AppSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Shifts.Integration.API.Common
{
    using System;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// AppSettings class.
    /// </summary>
    public class AppSettings
    {
        private readonly IKeyVaultHelper keyVaultHelper;
        private IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettings" /> class.
        /// </summary>
        /// <param name="keyVaultHelper">KeyVaultHelper class object.</param>
        /// <param name="configuration">app settings configuration.</param>
        public AppSettings(IKeyVaultHelper keyVaultHelper, IConfiguration configuration)
        {
            if (keyVaultHelper is null)
            {
                throw new ArgumentNullException(nameof(keyVaultHelper));
            }

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            this.keyVaultHelper = keyVaultHelper;
            this.configuration = configuration;
            this.AccessTokenUri = "https://dev.api.tjx.com/gies/v1/oauth2/accesstoken?grant_type=client_credentials";//this.keyVaultHelper.GetSecretByUri(this.configuration["KeyVault"] + "secrets/" + this.configuration["AccessTokenUri"]);
            this.AuthorizationToken = "Basic Uko4OWR4dXVHODdKT3dBV3JyaGtQR1hKQVVQcmp0Sjk6aFI1ZlJjWkxjZUo2aWw2UQ=="; //this.keyVaultHelper.GetSecretByUri(this.configuration["KeyVault"] + "secrets/" + this.configuration["AuthorizationToken"]);

            this.StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=w3hatjpjfkyjs;AccountKey=ek4xarhnMjUl/eVH0zISyYvCnhExSxjREbEQQRQckDUaHe2aNi4SUaa0EN6TlN1SsrUUiBDDK59s59GTzaEB9A==;EndpointSuffix=core.windows.net"; // this.keyVaultHelper.GetSecretByUri(this.configuration["KeyVault"] + "secrets/" + this.configuration["StorageConnectionString"]);
            this.RedisCacheConfiguration = "Kronos-SHFT-INT-redis.redis.cache.windows.net:6380,password=Aa0Zmf3RXdlX58Ci5Yi5WsN5YGpzisMwoDbLlXMwAAQ=,ssl=True,abortConnect=False"; // this.keyVaultHelper.GetSecretByUri(this.configuration["KeyVault"] + "secrets/" + this.configuration["RedisCacheConfiguration"]);
            this.ClientSecret = "_-2~wsK-LV3XG_.g4UIT6sqIBP15cXE5My"; // this.keyVaultHelper.GetSecretByUri(this.configuration["KeyVault"] + "secrets/" + this.configuration["ClientSecret"]);
            this.WfmSuperUsername = "Import"; // this.keyVaultHelper.GetSecretByUri(this.configuration["KeyVault"] + "secrets/" + this.configuration["WfmSuperUsername"]);
            this.WfmSuperUserPassword = "WfmSuperUserPassword"; // this.keyVaultHelper.GetSecretByUri(this.configuration["KeyVault"] + "secrets/" + this.configuration["WfmSuperUserPassword"]);
        }

        /// <summary>
        /// Gets Azure table storage connectionstring.
        /// </summary>
        public string StorageConnectionString { get; }

        /// <summary>
        /// Gets RedisCacheConfiguration.
        /// </summary>
        public string RedisCacheConfiguration { get; }

        /// <summary>
        /// Gets ClientSecret.
        /// </summary>
        public string ClientSecret { get; }

        /// <summary>
        /// Gets or sets WfmSuperUsername.
        /// </summary>
        public string WfmSuperUsername { get; set; }

        /// <summary>
        /// Gets or sets WfmSuperUserPassword.
        /// </summary>
        public string WfmSuperUserPassword { get; set; }

        /// <summary>
        /// Gets or sets AccessTokenUri.
        /// </summary>
        public string AccessTokenUri { get; set; }

        /// <summary>
        /// Gets or sets AuthorizationToken.
        /// </summary>
        public string AuthorizationToken { get; set; }

        /// <summary>
        /// Gets ClientId.
        /// </summary>
        public string ClientId
        {
            get => this.configuration["ClientId"];
        }

        /// <summary>
        /// Gets TenantId.
        /// </summary>
        public string TenantId
        {
            get => this.configuration["TenantId"];
        }

        // ********************************* API CONFIG ************************************

        /// <summary>
        /// Gets GraphApiUrl.
        /// </summary>
#pragma warning disable CA1056 // Uri properties should not be strings
        public string GraphApiUrl => this.configuration["GraphApiUrl"];
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets GraphBetaApiUrl.
        /// </summary>
#pragma warning disable CA1056 // Uri properties should not be strings
        public string GraphBetaApiUrl => this.configuration["GraphBetaApiUrl"];
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets TeamDepartmentMapping.
        /// </summary>
        public string TeamDepartmentMapping => this.configuration.GetValue<string>("TeamDepartmentMapping");

        /// <summary>
        /// Gets UserToUserMapping.
        /// </summary>
        public string UserToUserMapping => this.configuration.GetValue<string>("UserToUserMapping");

        /// <summary>
        /// Gets ShiftStartDate.
        /// </summary>
        public string ShiftStartDate => this.configuration["ShiftStartDate"];

        /// <summary>
        /// Gets ShiftEndDate.
        /// </summary>
        public string ShiftEndDate => this.configuration["ShiftEndDate"];

        /// <summary>
        /// Gets ShiftTheme.
        /// </summary>
        public string ShiftTheme => this.configuration["ShiftTheme"];

        /// <summary>
        /// Gets Instance.
        /// </summary>
        public string Instance => this.configuration["Instance"];

        /// <summary>
        /// Gets Domain.
        /// </summary>
        public string Domain => this.configuration["Domain"];

        /// <summary>
        /// Gets KronosTimeZone.
        /// </summary>
        public string KronosTimeZone => this.configuration["KronosTimeZone"];

        /// <summary>
        /// Gets ShiftsTimeZone.
        /// </summary>
        public string ShiftsTimeZone => this.configuration["ShiftsTimeZone"];

        /// <summary>
        /// Gets ProcessNumberOfUsersInBatch.
        /// </summary>
        public string ProcessNumberOfUsersInBatch => this.configuration["ProcessNumberOfUsersInBatch"];

        /// <summary>
        /// Gets ProcessNumberOfOrgJobsInBatch.
        /// </summary>
        public string ProcessNumberOfOrgJobsInBatch => this.configuration["ProcessNumberOfOrgJobsInBatch"];

        /// <summary>
        /// Gets ShiftMappingEntity.
        /// </summary>
        public string ShiftMappingEntity => this.configuration.GetValue<string>("ShiftMappingEntity");

        /// <summary>
        /// Gets ShiftMappingEntity.
        /// </summary>
        public string SwapShiftMappingEntity => this.configuration.GetValue<string>("SwapShiftMappingEntity");

        /// <summary>
        /// Gets OpenShiftTheme.
        /// </summary>
        public string OpenShiftTheme => this.configuration["OpenShiftTheme"];

        /// <summary>
        /// Gets Kronos query name.
        /// </summary>
        public string KronosUserDetailsQuery => this.configuration["KronosUserDetailsQuery"];

        /// <summary>
        /// Gets excel content type.
        /// </summary>
        public string ExcelContentType => this.configuration["ExcelContentType"];

        /// <summary>
        /// Gets template name.
        /// </summary>
        public string KronosShiftUserMappingTemplateName => this.configuration["KronosShiftUserMappingTemplateName"];

        /// <summary>
        /// Gets template container name.
        /// </summary>
        public string TemplatesContainerName => this.configuration["TemplatesContainerName"];

        /// <summary>
        /// Gets shift tempalte name.
        /// </summary>
        public string KronosShiftTeamDeptMappingTemplateName => this.configuration["KronosShiftTeamDeptMappingTemplateName"];

        /// <summary>
        /// Gets the Kronos query date span date format.
        /// </summary>
        public string KronosQueryDateSpanFormat => this.configuration["KronosQueryDateSpanFormat"];

        // **************************************CONFIGURATION PROJECT*********************************************

        /// <summary>
        /// Gets CallbackPath.
        /// </summary>
        public string CallbackPath => this.configuration["CallbackPath"];

        /// <summary>
        /// Gets the IntegrationApiUrl.
        /// </summary>
#pragma warning disable CA1056 // Uri properties should not be strings
        public string IntegrationApiUrl => this.configuration["IntegrationApiUrl"];
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets Authority.
        /// </summary>
        public string Authority => this.configuration.GetValue<string>("Authority");

        /// <summary>
        /// Gets PostLogoutRedirectUri.
        /// </summary>
#pragma warning disable CA1056 // Uri properties should not be strings
        public string PostLogoutRedirectUri => this.configuration.GetValue<string>("PostLogoutRedirectUri");
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets ResponseType.
        /// </summary>
        public string ResponseType => this.configuration.GetValue<string>("ResponseType");

        /// <summary>
        /// Gets RedisCacheInstanceName.
        /// </summary>
        public string RedisCacheInstanceName => this.configuration.GetValue<string>("RedisCacheInstanceName");

        /// <summary>
        /// Gets the base address for the first time sync.
        /// </summary>
        public string BaseAddressFirstTimeSync => this.configuration["BaseAddressFirstTimeSync"];

        /// <summary>
        /// Gets the number of days in the past to sync.
        /// </summary>
        public string SyncFromPreviousDays => this.configuration["SyncFromPreviousDays"];

        /// <summary>
        /// Gets OrgJobPath.
        /// </summary>public string OrgJobPath => this.configuration["OrgJobPath"];
        public string SyncToNextDays => this.configuration["SyncToNextDays"];

        /// <summary>
        /// Gets the configuration for the polling delay in the sync functionality.
        /// </summary>
        public string SyncDelayForPolling => this.configuration["SyncDelayForPolling"];

        /// <summary>
        /// Gets security group name for managers.
        /// </summary>
        public string AdSecurityGroupName => this.configuration["SecurityGroupName"];

        /// <summary>
        /// Gets security group id for managers.
        /// </summary>
        public string AdSecurityGroupId => this.configuration["SecurityGroupId"];

        /// <summary>
        /// Gets the correctedDateSpanForOutboundCalls.
        /// </summary>
        public string CorrectedDateSpanForOutboundCalls => this.configuration["CorrectedDateSpanForOutboundCalls"];

        // *********************************************************************************

        /// <summary>
        /// This method can be used to set secret in key vault.
        /// </summary>
        /// <param name="secretName">secret name.</param>
        /// <param name="secretValue">secret value.</param>
        /// <returns>boolean result.</returns>
        public bool SetConfigToKeyVault(string secretName, string secretValue)
        {
            var val = this.keyVaultHelper.SetKeyVaultSecret(this.configuration["KeyVault"], secretName, secretValue);
            if (val.Equals(secretValue, StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }
    }
}