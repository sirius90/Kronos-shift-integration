// <copyright file="TeamsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Shifts.Integration.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.Shifts.Encryption.Encryptors;
    using Microsoft.Teams.Shifts.Integration.API.Models.IntegrationAPI;
    using Microsoft.Teams.Shifts.Integration.API.Models.IntegrationAPI.Incoming;
    using Microsoft.Teams.Shifts.Integration.BusinessLogic.Models;
    using Microsoft.Teams.Shifts.Integration.BusinessLogic.Providers;
    using Microsoft.Teams.Shifts.Integration.BusinessLogic.ResponseModels;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This is the teams controller that is being used here.
    /// </summary>
    [Route("/v1/teams")]
    [ApiController]
    public class TeamsController : Controller
    {
        private readonly TelemetryClient telemetryClient;
        private readonly IConfigurationProvider configurationProvider;
        private readonly OpenShiftRequestController openShiftRequestController;
        private readonly SwapShiftController swapShiftController;
        private readonly Common.Utility utility;
        private readonly IUserMappingProvider userMappingProvider;
        private readonly IShiftMappingEntityProvider shiftMappingEntityProvider;
        private readonly IOpenShiftRequestMappingEntityProvider openShiftRequestMappingEntityProvider;
        private readonly IOpenShiftMappingEntityProvider openShiftMappingEntityProvider;
        private readonly ISwapShiftMappingEntityProvider swapShiftMappingEntityProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsController"/> class.
        /// </summary>
        /// <param name="telemetryClient">ApplicationInsights DI.</param>
        /// <param name="configurationProvider">ConfigurationProvider DI.</param>
        /// <param name="openShiftRequestController">OpenShiftRequestController DI.</param>
        /// <param name="swapShiftController">SwapShiftController DI.</param>
        /// <param name="utility">The common utility methods DI.</param>
        /// <param name="userMappingProvider">The user mapping provider DI.</param>
        /// <param name="shiftMappingEntityProvider">The shift entity mapping provider DI.</param>
        /// <param name="openShiftRequestMappingEntityProvider">The open shift request mapping entity provider DI.</param>
        /// <param name="openShiftMappingEntityProvider">The open shift mapping entity provider DI.</param>
        /// <param name="swapShiftMappingEntityProvider">The swap shift mapping entity provider DI.</param>
        public TeamsController(
            TelemetryClient telemetryClient,
            IConfigurationProvider configurationProvider,
            OpenShiftRequestController openShiftRequestController,
            SwapShiftController swapShiftController,
            Common.Utility utility,
            IUserMappingProvider userMappingProvider,
            IShiftMappingEntityProvider shiftMappingEntityProvider,
            IOpenShiftRequestMappingEntityProvider openShiftRequestMappingEntityProvider,
            IOpenShiftMappingEntityProvider openShiftMappingEntityProvider,
            ISwapShiftMappingEntityProvider swapShiftMappingEntityProvider)
        {
            this.telemetryClient = telemetryClient;
            this.configurationProvider = configurationProvider;
            this.openShiftRequestController = openShiftRequestController;
            this.swapShiftController = swapShiftController;
            this.utility = utility;
            this.userMappingProvider = userMappingProvider;
            this.shiftMappingEntityProvider = shiftMappingEntityProvider;
            this.openShiftRequestMappingEntityProvider = openShiftRequestMappingEntityProvider;
            this.openShiftMappingEntityProvider = openShiftMappingEntityProvider;
            this.swapShiftMappingEntityProvider = swapShiftMappingEntityProvider;
        }

        /// <summary>
        /// Method to update the Workforce Integration ID to the schedule.
        /// </summary>
        /// <returns>A unit of execution that contains the HTTP Response.</returns>
        [HttpGet]
        [Route("/api/teams/CheckSetup")]
        public async Task<HttpResponseMessage> CheckSetupAsync()
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();

            var isSetUpDone = await this.utility.IsSetUpDoneAsync().ConfigureAwait(false);

            // Check for all the setup i.e User to user mapping, team department mapping, user logged in to configuration web app
            if (isSetUpDone)
            {
                httpResponseMessage.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                httpResponseMessage.StatusCode = HttpStatusCode.InternalServerError;
            }

            return httpResponseMessage;
        }

        /// <summary>
        /// The method that will be called from Shifts.
        /// </summary>
        /// <param name="aadGroupId">The AAD Group Id for the Team.</param>
        /// <returns>An action result.</returns>
        [HttpPost]
        [Route("/v1/teams/{aadGroupId}/update")]
        public async Task<ActionResult> UpdateTeam([FromRoute] string aadGroupId)
        {
            var request = this.Request;
            var requestHeaders = request.Headers;
            Microsoft.Extensions.Primitives.StringValues passThroughValue = string.Empty;

            this.telemetryClient.TrackTrace("IncomingRequest, starts for method: UpdateTeam - " + DateTime.Now.ToString(CultureInfo.InvariantCulture));

            // Step 1 - Obtain the secret from the database.
            var configurationEntities = await this.configurationProvider.GetConfigurationsAsync().ConfigureAwait(false);
            var configurationEntity = configurationEntities?.FirstOrDefault();

            // Check whether Request coming from correct workforce integration is present and is equal to workforce integration id.
            // Request is valid for OpenShift and SwapShift request FLM approval when the value of X-MS-WFMRequest coming
            // from correct workforce integration is equal to current workforce integration id.
            var isRequestFromCorrectIntegration = requestHeaders.TryGetValue("X-MS-WFMPassthrough", out passThroughValue) &&
                                           string.Equals(passThroughValue, configurationEntity.WorkforceIntegrationId, StringComparison.Ordinal);

            // Step 2 - Create/declare the byte arrays, and other data types required.
            byte[] secretKeyBytes = Encoding.UTF8.GetBytes(configurationEntity?.WorkforceIntegrationSecret);

            // Step 3 - Extract the incoming request using the symmetric key, and the HttpRequest.
            var jsonModel = await DecryptEncryptedRequestFromShiftsAsync(
                secretKeyBytes,
                this.Request).ConfigureAwait(false);

            IntegrationApiResponseModel responseModel = new IntegrationApiResponseModel();
            ShiftsIntegResponse integrationResponse;
            List<ShiftsIntegResponse> responseModelList = new List<ShiftsIntegResponse>();
            string responseModelStr = string.Empty;

            var updateProps = new Dictionary<string, string>()
            {
                { "IncomingAadGroupId", aadGroupId },
            };

            // Check if payload is for open shift request.
            if (jsonModel.Requests.Any(x => x.Url.Contains("/openshiftrequests/", StringComparison.InvariantCulture)))
            {
                // Process payload for open shift request.
                responseModelList = await this.ProcessOpenShiftRequest(jsonModel, updateProps, aadGroupId, isRequestFromCorrectIntegration).ConfigureAwait(false);
            }

            // Check if payload is for swap shift request.
            else if (jsonModel.Requests.Any(x => x.Url.Contains("/swapRequests/", StringComparison.InvariantCulture)))
            {
                this.telemetryClient.TrackTrace("Teams Controller swapRequests " + JsonConvert.SerializeObject(jsonModel));

                // Process payload for swap shift request.
                responseModelList = await this.ProcessSwapShiftRequest(jsonModel, aadGroupId, isRequestFromCorrectIntegration).ConfigureAwait(true);
            }

            // Check if payload is for open shift.
            else if (jsonModel.Requests.Any(x => x.Url.Contains("/openshifts/", StringComparison.InvariantCulture)))
            {
                // Acknowledge with status OK for open shift as solution does not synchronize open shifts into Kronos, Kronos being single source of truth for front line manager actions.
                integrationResponse = ProcessOpenShiftAcknowledgement(jsonModel, updateProps);
                responseModelList.Add(integrationResponse);
            }

            // Check if payload is for shift.
            else if (jsonModel.Requests.Any(x => x.Url.Contains("/shifts/", StringComparison.InvariantCulture)))
            {
                // Acknowledge with status OK for shift as solution does not synchronize shifts into Kronos, Kronos being single source of truth for front line manager actions.
                integrationResponse = ProcessShiftAcknowledgement(jsonModel, updateProps);
                responseModelList.Add(integrationResponse);
            }

            responseModel.ShiftsIntegResponses = responseModelList;
            responseModelStr = JsonConvert.SerializeObject(responseModel);

            this.telemetryClient.TrackTrace("IncomingRequest, ends for method: UpdateTeam - " + DateTime.Now.ToString(CultureInfo.InvariantCulture));

            // Sends response back to Shifts.
            return this.Ok(responseModelStr);
        }

        /// <summary>
        /// This method will create the necessary acknowledgement response whenever Shift entities are created, or updated.
        /// </summary>
        /// <param name="jsonModel">The decrypted JSON payload.</param>
        /// <param name="updateProps">The type of <see cref="Dictionary{TKey, TValue}"/> that contains properties that are being logged to ApplicationInsights.</param>
        /// <returns>A type of <see cref="ShiftsIntegResponse"/>.</returns>
        private static ShiftsIntegResponse ProcessShiftAcknowledgement(RequestModel jsonModel, Dictionary<string, string> updateProps)
        {
            if (jsonModel.Requests.First(x => x.Url.Contains("/shifts/", StringComparison.InvariantCulture)).Body != null)
            {
                var incomingShift = JsonConvert.DeserializeObject<Shift>(jsonModel.Requests.First(x => x.Url.Contains("/shifts/", StringComparison.InvariantCulture)).Body.ToString());

                updateProps.Add("ShiftId", incomingShift.Id);
                updateProps.Add("UserIdForShift", incomingShift.UserId);
                updateProps.Add("SchedulingGroupId", incomingShift.SchedulingGroupId);

                var integrationResponse = GenerateResponse(incomingShift.Id, HttpStatusCode.OK, null, null);
                return integrationResponse;
            }
            else
            {
                var nullBodyShiftId = jsonModel.Requests.First(x => x.Url.Contains("/shifts/", StringComparison.InvariantCulture)).Id;
                updateProps.Add("NullBodyShiftId", nullBodyShiftId);

                // The outbound acknowledgement does not honor the null Etag, 502 Bad Gateway is thrown if so.
                // Checking for the null eTag value, from the attributes in the payload and generate a non-null value in GenerateResponse method.
                var integrationResponse = GenerateResponse(
                    nullBodyShiftId,
                    HttpStatusCode.OK,
                    null,
                    null);

                return integrationResponse;
            }
        }

        /// <summary>
        /// This method will generate the necessary response for acknowledging the open shift being created or changed.
        /// </summary>
        /// <param name="jsonModel">The decrypted JSON payload.</param>
        /// <param name="updateProps">The type of <see cref="Dictionary{TKey, TValue}"/> which contains various properties to log to ApplicationInsights.</param>
        /// <returns>A type of <see cref="ShiftsIntegResponse"/>.</returns>
        private static ShiftsIntegResponse ProcessOpenShiftAcknowledgement(RequestModel jsonModel, Dictionary<string, string> updateProps)
        {
            ShiftsIntegResponse integrationResponse;
            if (jsonModel.Requests.First(x => x.Url.Contains("/openshifts/", StringComparison.InvariantCulture)).Body != null)
            {
                var incomingOpenShift = JsonConvert.DeserializeObject<OpenShiftIS>(jsonModel.Requests.First().Body.ToString());

                updateProps.Add("OpenShiftId", incomingOpenShift.Id);
                updateProps.Add("SchedulingGroupId", incomingOpenShift.SchedulingGroupId);

                integrationResponse = GenerateResponse(incomingOpenShift.Id, HttpStatusCode.OK, null, null);
            }
            else
            {
                var nullBodyIncomingOpenShiftId = jsonModel.Requests.First(x => x.Url.Contains("/openshifts/", StringComparison.InvariantCulture)).Id;
                updateProps.Add("NullBodyOpenShiftId", nullBodyIncomingOpenShiftId);
                integrationResponse = GenerateResponse(nullBodyIncomingOpenShiftId, HttpStatusCode.OK, null, null);
            }

            return integrationResponse;
        }

        /// <summary>
        /// Generates the response for each outbound request.
        /// </summary>
        /// <param name="itemId">Id for response.</param>
        /// <param name="statusCode">HttpStatusCode for the request been processed.</param>
        /// <param name="eTag">Etag based on response.</param>
        /// <param name="error">Forward error to Shifts if any.</param>
        /// <returns>ShiftsIntegResponse.</returns>
        private static ShiftsIntegResponse GenerateResponse(string itemId, HttpStatusCode statusCode, string eTag, ResponseError error)
        {
            // The outbound acknowledgement does not honor the null Etag, 502 Bad Gateway is thrown if so.
            // Checking for the null eTag value, from the attributes in the payload.
            string responseEtag;
            if (string.IsNullOrEmpty(eTag))
            {
                responseEtag = GenerateNewGuid();
            }
            else
            {
                responseEtag = eTag;
            }

            var integrationResponse = new ShiftsIntegResponse()
            {
                Id = itemId,
                Status = (int)statusCode,
                Body = new Body
                {
                    Error = error,
                    ETag = responseEtag,
                },
            };

            return integrationResponse;
        }

        /// <summary>
        /// Generates the Guid for outbound call response.
        /// </summary>
        /// <returns>Returns newly generated GUID string.</returns>
        private static string GenerateNewGuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// This method will properly decrypt the encrypted payload that is being received from Shifts.
        /// </summary>
        /// <param name="secretKeyBytes">The sharedSecret from Shifts casted into a byte array.</param>
        /// <param name="request">The incoming request from Shifts UI that contains an encrypted payload.</param>
        /// <returns>A unit of execution which contains the RequestModel.</returns>
        private static async Task<RequestModel> DecryptEncryptedRequestFromShiftsAsync(byte[] secretKeyBytes, HttpRequest request)
        {
            string decryptedRequestBody = null;

            // Step 1 - using a memory stream for the processing of the request.
            using (MemoryStream ms = new MemoryStream())
            {
                await request.Body.CopyToAsync(ms).ConfigureAwait(false);
                byte[] encryptedRequestBytes = ms.ToArray();
                Aes256CbcHmacSha256Encryptor decryptor = new Aes256CbcHmacSha256Encryptor(secretKeyBytes);
                byte[] decryptedRequestBodyBytes = decryptor.Decrypt(encryptedRequestBytes);
                decryptedRequestBody = Encoding.UTF8.GetString(decryptedRequestBodyBytes);
            }

            // Step 2 - Parse the decrypted request into the correct model.
            return JsonConvert.DeserializeObject<RequestModel>(decryptedRequestBody);
        }

        /// <summary>
        /// Generate response to prevent actions.
        /// </summary>
        /// <param name="jsonModel">The request payload.</param>
        /// <param name="errorMessage">Error message to send while preventing action.</param>
        /// <returns>List of ShiftsIntegResponse.</returns>
        private static List<ShiftsIntegResponse> GenerateResponseToPreventAction(RequestModel jsonModel, string errorMessage)
        {
            List<ShiftsIntegResponse> shiftsIntegResponses = new List<ShiftsIntegResponse>();
            var integrationResponse = new ShiftsIntegResponse();
            foreach (var item in jsonModel.Requests)
            {
                ResponseError responseError = new ResponseError();
                responseError.Code = HttpStatusCode.BadRequest.ToString();
                responseError.Message = errorMessage;
                integrationResponse = GenerateResponse(item.Id, HttpStatusCode.BadRequest, null, responseError);
                shiftsIntegResponses.Add(integrationResponse);
            }

            return shiftsIntegResponses;
        }

        /// <summary>
        /// Process open shift requests outbound calls.
        /// </summary>
        /// <param name="jsonModel">Incoming payload for the request been made in Shifts.</param>
        /// <param name="updateProps">telemetry properties.</param>
        /// <returns>Returns list of ShiftIntegResponse for request.</returns>
        private async Task<List<ShiftsIntegResponse>> ProcessOpenShiftRequest(RequestModel jsonModel, Dictionary<string, string> updateProps, string teamsId, bool isRequestFromCorrectIntegration)
        {
            List<ShiftsIntegResponse> responseModelList = new List<ShiftsIntegResponse>();
            var requestBody = jsonModel.Requests.First(x => x.Url.Contains("/openshiftrequests/", StringComparison.InvariantCulture)).Body;

            if (requestBody != null)
            {
                var requestState = requestBody?["state"].Value<string>();

                switch (requestState)
                {
                    // The Open shift request is submitted in Shifts and is pending with manager for approval.
                    case ApiConstants.ShiftsPending:
                        {
                            var openShiftRequest = JsonConvert.DeserializeObject<OpenShiftRequestIS>(requestBody.ToString());
                            responseModelList = await this.ProcessOutboundOpenShiftRequestAsync(openShiftRequest, updateProps, teamsId).ConfigureAwait(false);
                        }

                        break;

                    // The Open shift request is approved by manager.
                    case ApiConstants.ShiftsApproved:
                        {
                            // The request is coming from intended workforce integration.
                            if (isRequestFromCorrectIntegration)
                            {
                                this.telemetryClient.TrackTrace($"Request coming from correct workforce integration is {isRequestFromCorrectIntegration} for OpenShiftRequest approval outbound call.");
                                responseModelList = await this.ProcessOpenShiftRequestApprovalAsync(jsonModel, updateProps).ConfigureAwait(false);
                            }

                            // Request is coming from either Shifts UI or from incorrect workforce integration.
                            else
                            {
                                this.telemetryClient.TrackTrace($"Request coming from correct workforce integration is {isRequestFromCorrectIntegration} for OpenShiftRequest approval outbound call.");
                                responseModelList = GenerateResponseToPreventAction(jsonModel, Resource.InvalidApproval);
                            }
                        }

                        break;

                    // The code below would be when there is a decline.
                    case ApiConstants.ShiftsDeclined:
                        {
                            // The request is coming from intended workforce integration.
                            if (isRequestFromCorrectIntegration)
                            {
                                this.telemetryClient.TrackTrace($"Request coming from correct workforce integration is {isRequestFromCorrectIntegration} for OpenShiftRequest decline outbound call.");
                                var integrationResponse = new ShiftsIntegResponse();
                                foreach (var item in jsonModel.Requests)
                                {
                                    integrationResponse = GenerateResponse(item.Id, HttpStatusCode.OK, null, null);
                                    responseModelList.Add(integrationResponse);
                                }
                            }

                            // Request is coming from either Shifts UI or from incorrect workforce integration.
                            else
                            {
                                this.telemetryClient.TrackTrace($"Request coming from correct workforce integration is {isRequestFromCorrectIntegration} for OpenShiftRequest decline outbound call.");
                                responseModelList = GenerateResponseToPreventAction(jsonModel, Resource.InvalidApproval);
                            }
                        }

                        break;
                }
            }

            return responseModelList;
        }

        /// <summary>
        /// Process swap shift requests outbound calls.
        /// </summary>
        /// <param name="jsonModel">Incoming payload for the request been made in Shifts.</param>
        /// <param name="aadGroupId">AAD Group id.</param>
        /// <returns>Returns list of ShiftIntegResponse for request.</returns>
        private async Task<List<ShiftsIntegResponse>> ProcessSwapShiftRequest(RequestModel jsonModel, string aadGroupId, bool isRequestFromCorrectIntegration)
        {
            List<ShiftsIntegResponse> responseModelList = new List<ShiftsIntegResponse>();
            var requestBody = jsonModel.Requests.First(x => x.Url.Contains("/swapRequests/", StringComparison.InvariantCulture)).Body;
            ShiftsIntegResponse integrationResponseSwap = null;
            try
            {
                // If the requestBody is not null - represents either a new Swap Shift request being created by FLW1,
                // FLW2 accepts or declines FLW1's request, or FLM approves or declines the Swap Shift request.
                // If the requestBody is null - represents FLW1's cancellation of the Swap Shift Request.
                if (requestBody != null)
                {
                    var requestState = requestBody?["state"].Value<string>();
                    var requestAssignedTo = requestBody?["assignedTo"].Value<string>();

                    var swapRequest = JsonConvert.DeserializeObject<SwapRequest>(requestBody.ToString());

                    // FLW1 has requested for swap shift, submit the request in Kronos.
                    if (requestState == ApiConstants.ShiftsPending && requestAssignedTo == ApiConstants.ShiftsRecipient)
                    {
                        integrationResponseSwap = await this.swapShiftController.SubmitSwapShiftRequestToKronosAsync(swapRequest, aadGroupId).ConfigureAwait(false);
                        responseModelList.Add(integrationResponseSwap);
                    }

                    // FLW2 has approved the swap shift, updates the status in Kronos to submitted and request goes to manager for approval.
                    else if (requestState == ApiConstants.ShiftsPending && requestAssignedTo == ApiConstants.ShiftsManager)
                    {
                        integrationResponseSwap = await this.swapShiftController.ApproveOrDeclineSwapShiftRequestToKronosAsync(swapRequest, aadGroupId).ConfigureAwait(false);
                        responseModelList.Add(integrationResponseSwap);
                    }

                    // FLW2 has declined the swap shift, updates the status in Kronos to refused.
                    else if (requestState == ApiConstants.Declined && requestAssignedTo == ApiConstants.ShiftsRecipient)
                    {
                        integrationResponseSwap = await this.swapShiftController.ApproveOrDeclineSwapShiftRequestToKronosAsync(swapRequest, aadGroupId).ConfigureAwait(false);
                        responseModelList.Add(integrationResponseSwap);
                    }

                    // Manager has declined the request in Kronos, which declines the request in Shifts also.
                    else if (requestState == ApiConstants.ShiftsDeclined && requestAssignedTo == ApiConstants.ShiftsManager)
                    {
                        // The request is coming from intended workforce integration.
                        if (isRequestFromCorrectIntegration)
                        {
                            this.telemetryClient.TrackTrace($"Request coming from correct workforce integration is {isRequestFromCorrectIntegration} for SwapShiftRequest decline outbound call.");
                            integrationResponseSwap = GenerateResponse(swapRequest.Id, HttpStatusCode.OK, swapRequest.ETag, null);
                            responseModelList.Add(integrationResponseSwap);
                        }

                        // Request is coming from either Shifts UI or from incorrect workforce integration.
                        else
                        {
                            this.telemetryClient.TrackTrace($"Request coming from correct workforce integration is {isRequestFromCorrectIntegration} for SwapShiftRequest decline outbound call.");
                            responseModelList = GenerateResponseToPreventAction(jsonModel, Resource.InvalidApproval);
                        }
                    }

                    // Manager has approved the request in Kronos.
                    else if (requestState == ApiConstants.ShiftsApproved && requestAssignedTo == ApiConstants.ShiftsManager)
                    {
                        // The request is coming from intended workforce integration.
                        if (isRequestFromCorrectIntegration)
                        {
                            this.telemetryClient.TrackTrace($"Request coming from correct workforce integration is {isRequestFromCorrectIntegration} for SwapShiftRequest approval outbound call.");
                            responseModelList = await this.ProcessSwapShiftRequestApprovalAsync(jsonModel, aadGroupId).ConfigureAwait(false);
                        }

                        // Request is coming from either Shifts UI or from incorrect workforce integration.
                        else
                        {
                            this.telemetryClient.TrackTrace($"Request coming from correct workforce integration is {isRequestFromCorrectIntegration} for SwapShiftRequest approval outbound call.");
                            responseModelList = GenerateResponseToPreventAction(jsonModel, Resource.InvalidApproval);
                        }
                    }

                    // There is a System decline with the Swap Shift Request
                    else if (requestState == ApiConstants.Declined && requestAssignedTo == ApiConstants.System)
                    {
                        var systemDeclineSwapReqId = jsonModel.Requests.First(x => x.Url.Contains("/swapRequests/", StringComparison.InvariantCulture)).Id;
                        ResponseError responseError = new ResponseError
                        {
                            Message = Resource.SystemDeclined,
                        };

                        integrationResponseSwap = GenerateResponse(systemDeclineSwapReqId, HttpStatusCode.OK, null, responseError);
                        responseModelList.Add(integrationResponseSwap);
                    }
                }
                else if (jsonModel.Requests.Any(c => c.Method == "DELETE"))
                {
                    // Code below handles the delete swap shift request.
                    var deleteSwapRequestId = jsonModel.Requests.First(x => x.Url.Contains("/swapRequests/", StringComparison.InvariantCulture)).Id;

                    // Logging to telemetry the incoming cancelled request by FLW1.
                    this.telemetryClient.TrackTrace($"The Swap Shift Request: {deleteSwapRequestId} has been declined by FLW1.");

                    var entityToCancel = await this.swapShiftMappingEntityProvider.GetKronosReqAsync(deleteSwapRequestId).ConfigureAwait(false);

                    // Updating the ShiftsStatus to Cancelled.
                    entityToCancel.ShiftsStatus = ApiConstants.SwapShiftCancelled;

                    // Updating the entity accordingly
                    await this.swapShiftMappingEntityProvider.AddOrUpdateSwapShiftMappingAsync(entityToCancel).ConfigureAwait(false);

                    integrationResponseSwap = GenerateResponse(deleteSwapRequestId, HttpStatusCode.OK, null, null);
                    responseModelList.Add(integrationResponseSwap);
                }
            }
            catch (Exception)
            {
                this.telemetryClient.TrackTrace("Teams Controller swapRequests responseModelList Exception" + JsonConvert.SerializeObject(responseModelList));
                throw;
            }

            this.telemetryClient.TrackTrace("Teams Controller swapRequests responseModelList" + JsonConvert.SerializeObject(responseModelList));

            return responseModelList;
        }

        /// <summary>
        /// This method processes the open shift request approval, and proceeds to update the Azure table storage accordingly with the Shifts status
        /// of the open shift request, and also ensures that the ShiftMappingEntity table is properly in sync.
        /// </summary>
        /// <param name="jsonModel">The decrypted JSON payload.</param>
        /// <param name="updateProps">A dictionary of string, string that will be logged to ApplicationInsights.</param>
        /// <returns>A unit of execution.</returns>
        private async Task<List<ShiftsIntegResponse>> ProcessOpenShiftRequestApprovalAsync(RequestModel jsonModel, Dictionary<string, string> updateProps)
        {
            List<ShiftsIntegResponse> responseModelList = new List<ShiftsIntegResponse>();
            ShiftsIntegResponse integrationResponse = null;

            var openShiftRequests = jsonModel?.Requests?.Where(x => x.Url.Contains("/openshiftrequests/", StringComparison.InvariantCulture));
            var finalOpenShiftObj = jsonModel?.Requests?.FirstOrDefault(x => x.Url.Contains("/openshifts/", StringComparison.InvariantCulture));
            var finalShiftObj = jsonModel?.Requests?.FirstOrDefault(x => x.Url.Contains("/shifts/", StringComparison.InvariantCulture));

            // Filter all the system declined requests.
            var autoDeclinedRequests = openShiftRequests.Where(c => c.Body != null && c.Body["state"].Value<string>() == ApiConstants.Declined && c.Body["assignedTo"].Value<string>() == ApiConstants.System).ToList();

            // Filter approved open shift request.
            var approvedOpenShiftRequest = openShiftRequests.Where(c => c.Body != null && c.Body["state"].Value<string>() == ApiConstants.ShiftsApproved && c.Body["assignedTo"].Value<string>() == ApiConstants.ShiftsManager).FirstOrDefault();

            var finalShift = JsonConvert.DeserializeObject<Shift>(finalShiftObj.Body.ToString());
            var finalOpenShiftRequest = JsonConvert.DeserializeObject<OpenShiftRequestIS>(approvedOpenShiftRequest.Body.ToString());
            var finalOpenShift = JsonConvert.DeserializeObject<OpenShiftIS>(finalOpenShiftObj.Body.ToString());

            updateProps.Add("NewShiftId", finalShift.Id);
            updateProps.Add("GraphOpenShiftRequestId", finalOpenShiftRequest.Id);
            updateProps.Add("GraphOpenShiftId", finalOpenShift.Id);

            // Step 1 - Create the Kronos Unique ID.
            var kronosUniqueId = this.utility.CreateUniqueId(finalShift);

            this.telemetryClient.TrackTrace("KronosHash-OpenShiftRequestApproval-TeamsController: " + kronosUniqueId);

            try
            {
                this.telemetryClient.TrackTrace("Updating entities-OpenShiftRequestApproval started: " + DateTime.Now.ToString(CultureInfo.InvariantCulture));

                // Step 1 - Get the temp shift record first by table scan against RowKey.
                var tempShiftRowKey = $"SHFT_PENDING_{finalOpenShiftRequest.Id}";
                var tempShiftEntity = await this.shiftMappingEntityProvider.GetShiftMappingEntityByRowKeyAsync(tempShiftRowKey).ConfigureAwait(false);

                // We need to check if the tempShift is not null because in the Open Shift Request controller, the tempShift was created
                // as part of the Graph API call to approve the Open Shift Request.
                if (tempShiftEntity != null)
                {
                    // Step 2 - Form the new shift record.
                    var shiftToInsert = new TeamsShiftMappingEntity()
                    {
                        RowKey = finalShift.Id,
                        KronosPersonNumber = tempShiftEntity.KronosPersonNumber,
                        KronosUniqueId = tempShiftEntity.KronosUniqueId,
                        PartitionKey = tempShiftEntity.PartitionKey,
                        AadUserId = tempShiftEntity.AadUserId,
                        ShiftStartDate = this.utility.UTCToKronosTimeZone(finalShift.SharedShift.StartDateTime),
                    };

                    // Step 3 - Save the new shift record.
                    await this.shiftMappingEntityProvider.SaveOrUpdateShiftMappingEntityAsync(shiftToInsert, shiftToInsert.RowKey, shiftToInsert.PartitionKey).ConfigureAwait(false);

                    // Step 4 - Delete the temp shift record.
                    await this.shiftMappingEntityProvider.DeleteOrphanDataFromShiftMappingAsync(tempShiftEntity).ConfigureAwait(false);

                    // Adding response for create new shift.
                    integrationResponse = GenerateResponse(finalShift.Id, HttpStatusCode.OK, null, null);
                    responseModelList.Add(integrationResponse);
                }
                else
                {
                    // We are logging to ApplicationInsights that the tempShift entity could not be found.
                    this.telemetryClient.TrackTrace(string.Format(CultureInfo.InvariantCulture, Resource.EntityNotFoundWithRowKey, tempShiftRowKey));
                }

                // Logging to ApplicationInsights the OpenShiftRequestId.
                this.telemetryClient.TrackTrace("OpenShiftRequestId = " + finalOpenShiftRequest.Id);

                // Find the open shift request for which we update the ShiftsStatus to Approved.
                var openShiftRequestEntityToUpdate = await this.openShiftRequestMappingEntityProvider.GetOpenShiftRequestMappingEntityByOpenShiftIdAsync(
                    finalOpenShift.Id,
                    finalOpenShiftRequest.Id).ConfigureAwait(false);

                openShiftRequestEntityToUpdate.ShiftsStatus = finalOpenShiftRequest.State;

                // Update the open shift request to Approved in the ShiftStatus column.
                await this.openShiftRequestMappingEntityProvider.SaveOrUpdateOpenShiftRequestMappingEntityAsync(openShiftRequestEntityToUpdate).ConfigureAwait(false);

                // Delete the open shift entity accordingly from the OpenShiftEntityMapping table in Azure Table storage as the open shift request has been approved.
                await this.openShiftMappingEntityProvider.DeleteOrphanDataFromOpenShiftMappingByOpenShiftIdAsync(finalOpenShift.Id).ConfigureAwait(false);

                // Adding response for delete open shift.
                integrationResponse = GenerateResponse(finalOpenShift.Id, HttpStatusCode.OK, null, null);
                responseModelList.Add(integrationResponse);

                // Adding response for approved open shift request.
                integrationResponse = GenerateResponse(finalOpenShiftRequest.Id, HttpStatusCode.OK, null, null);
                responseModelList.Add(integrationResponse);

                foreach (var declinedRequest in autoDeclinedRequests)
                {
                    this.telemetryClient.TrackTrace($"SystemDeclinedOpenShiftRequestId: {declinedRequest.Id}");
                    var declinedOpenShiftRequest = JsonConvert.DeserializeObject<OpenShiftRequestIS>(declinedRequest.Body.ToString());

                    // Update the status in Azure table storage.
                    var entityToUpdate = await this.openShiftRequestMappingEntityProvider.GetOpenShiftRequestMappingEntityByOpenShiftRequestIdAsync(
                        declinedRequest.Id).ConfigureAwait(false);

                    entityToUpdate.KronosStatus = declinedOpenShiftRequest.State;
                    entityToUpdate.ShiftsStatus = declinedOpenShiftRequest.State;

                    // Commit the change to the database.
                    await this.openShiftRequestMappingEntityProvider.SaveOrUpdateOpenShiftRequestMappingEntityAsync(entityToUpdate).ConfigureAwait(false);

                    this.telemetryClient.TrackTrace($"OpenShiftRequestId: {declinedOpenShiftRequest.Id}, assigned to: {declinedOpenShiftRequest.AssignedTo}, state: {declinedOpenShiftRequest.State}");

                    // Adding response for system declined open shift request.
                    integrationResponse = GenerateResponse(declinedOpenShiftRequest.Id, HttpStatusCode.OK, null, null);
                    responseModelList.Add(integrationResponse);
                }

                this.telemetryClient.TrackTrace("Updating entities-OpenShiftRequestApproval complete: " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    this.telemetryClient.TrackTrace($"Shift mapping has failed for {finalOpenShiftRequest.Id}: " + ex.InnerException.ToString());
                }

                this.telemetryClient.TrackTrace($"Shift mapping has resulted in some type of error with the following: {ex.StackTrace.ToString(CultureInfo.InvariantCulture)}");

                throw;
            }

            return responseModelList;
        }

        /// <summary>
        /// This method further processes the Swap Shift request approval.
        /// </summary>
        /// <param name="jsonModel">The decryped JSON payload from Shifts/MS Graph.</param>
        /// <param name="aadGroupId">The team ID for which the Swap Shift request has been approved.</param>
        /// <returns>A unit of execution that contains the type of <see cref="ShiftsIntegResponse"/>.</returns>
        private async Task<List<ShiftsIntegResponse>> ProcessSwapShiftRequestApprovalAsync(RequestModel jsonModel, string aadGroupId)
        {
            List<ShiftsIntegResponse> swapShiftsIntegResponses = new List<ShiftsIntegResponse>();
            ShiftsIntegResponse integrationResponse = null;
            var swapShiftApprovalRes = from requests in jsonModel.Requests
                                       group requests by requests.Url;

            var swapRequests = jsonModel.Requests.Where(c => c.Url.Contains("/swapRequests/", StringComparison.InvariantCulture));

            // Filter all the system declined requests.
            var autoDeclinedRequests = swapRequests.Where(c => c.Body != null && c.Body["state"].Value<string>() == ApiConstants.Declined && c.Body["assignedTo"].Value<string>() == ApiConstants.System).ToList();

            // Filter approved swap shift request.
            var approvedSwapShiftRequest = swapRequests.Where(c => c.Body != null && c.Body["state"].Value<string>() == ApiConstants.ShiftsApproved && c.Body["assignedTo"].Value<string>() == ApiConstants.ShiftsManager).FirstOrDefault();

            var swapShiftRequest = JsonConvert.DeserializeObject<SwapRequest>(approvedSwapShiftRequest.Body.ToString());

            var postedShifts = jsonModel.Requests.Where(x => x.Url.Contains("/shifts/", StringComparison.InvariantCulture) && x.Method == "POST").ToList();

            var deletedShifts = jsonModel.Requests.Where(x => x.Url.Contains("/shifts/", StringComparison.InvariantCulture) && x.Method == "DELETE").ToList();

            if (swapShiftRequest != null)
            {
                var newShiftFirst = JsonConvert.DeserializeObject<Shift>(postedShifts.First().Body.ToString());
                var newShiftSecond = JsonConvert.DeserializeObject<Shift>(postedShifts.Last().Body.ToString());

                // Step 1 - Create the Kronos Unique ID.
                var kronosUniqueIdFirst = this.utility.CreateUniqueId(newShiftFirst);
                var kronosUniqueIdSecond = this.utility.CreateUniqueId(newShiftSecond);

                try
                {
                    var userMappingRecord = await this.userMappingProvider.GetUserMappingEntityAsyncNew(
                                       newShiftFirst?.UserId,
                                       aadGroupId).ConfigureAwait(false);

                    // When getting the month partition key, make sure to take into account the Kronos Time Zone as well
                    var provider = CultureInfo.InvariantCulture;
                    var actualStartDateTimeStr = this.utility.CalculateStartDateTime(
                        newShiftFirst.SharedShift.StartDateTime.Date).ToString("M/dd/yyyy", provider);
                    var actualEndDateTimeStr = this.utility.CalculateEndDateTime(
                        newShiftFirst.SharedShift.EndDateTime.Date).ToString("M/dd/yyyy", provider);

                    // Create the month partition key based on the finalShift object.
                    var monthPartitions = Common.Utility.GetMonthPartition(actualStartDateTimeStr, actualEndDateTimeStr);
                    var monthPartition = monthPartitions?.FirstOrDefault();

                    // Create the shift mapping entity based on the finalShift object also.
                    var shiftEntity = this.utility.CreateShiftMappingEntity(newShiftFirst, userMappingRecord, kronosUniqueIdFirst);
                    await this.shiftMappingEntityProvider.SaveOrUpdateShiftMappingEntityAsync(
                        shiftEntity,
                        newShiftFirst.Id,
                        monthPartition).ConfigureAwait(false);

                    var userMappingRecordSec = await this.userMappingProvider.GetUserMappingEntityAsyncNew(
                                      newShiftSecond?.UserId,
                                      aadGroupId).ConfigureAwait(false);
                    integrationResponse = GenerateResponse(newShiftFirst.Id, HttpStatusCode.OK, null, null);
                    swapShiftsIntegResponses.Add(integrationResponse);

                    // When getting the month partition key, make sure to take into account the Kronos Time Zone as well
                    var actualStartDateTimeStrSec = this.utility.CalculateStartDateTime(
                        newShiftSecond.SharedShift.StartDateTime).ToString("M/dd/yyyy", provider);
                    var actualEndDateTimeStrSec = this.utility.CalculateEndDateTime(
                        newShiftSecond.SharedShift.EndDateTime).ToString("M/dd/yyyy", provider);

                    // Create the month partition key based on the finalShift object.
                    var monthPartitionsSec = Common.Utility.GetMonthPartition(actualStartDateTimeStrSec, actualEndDateTimeStrSec);
                    var monthPartitionSec = monthPartitionsSec?.FirstOrDefault();

                    // Create the shift mapping entity based on the finalShift object also.
                    var shiftEntitySec = this.utility.CreateShiftMappingEntity(newShiftSecond, userMappingRecordSec, kronosUniqueIdSecond);
                    await this.shiftMappingEntityProvider.SaveOrUpdateShiftMappingEntityAsync(
                        shiftEntitySec,
                        newShiftSecond.Id,
                        monthPartitionSec).ConfigureAwait(false);
                    integrationResponse = GenerateResponse(newShiftSecond.Id, HttpStatusCode.OK, null, null);
                    swapShiftsIntegResponses.Add(integrationResponse);

                    foreach (var delShifts in deletedShifts)
                    {
                        integrationResponse = GenerateResponse(delShifts.Id, HttpStatusCode.OK, null, null);
                        swapShiftsIntegResponses.Add(integrationResponse);
                    }

                    integrationResponse = GenerateResponse(approvedSwapShiftRequest.Id, HttpStatusCode.OK, swapShiftRequest.ETag, null);
                    swapShiftsIntegResponses.Add(integrationResponse);

                    foreach (var declinedRequest in autoDeclinedRequests)
                    {
                        this.telemetryClient.TrackTrace($"SystemDeclinedOpenShiftRequestId: {declinedRequest.Id}");
                        var declinedSwapShiftRequest = JsonConvert.DeserializeObject<SwapRequest>(declinedRequest.Body.ToString());

                        // Get the requests from storage.
                        var entityToUpdate = await this.swapShiftMappingEntityProvider.GetKronosReqAsync(
                            declinedRequest.Id).ConfigureAwait(false);

                        entityToUpdate.KronosStatus = declinedSwapShiftRequest.State;
                        entityToUpdate.ShiftsStatus = declinedSwapShiftRequest.State;

                        // Commit the change to the database.
                        await this.swapShiftMappingEntityProvider.AddOrUpdateSwapShiftMappingAsync(entityToUpdate).ConfigureAwait(false);

                        this.telemetryClient.TrackTrace($"OpenShiftRequestId: {declinedSwapShiftRequest.Id}, assigned to: {declinedSwapShiftRequest.AssignedTo}, state: {declinedSwapShiftRequest.State}");

                        // Adding response for system declined open shift request.
                        integrationResponse = GenerateResponse(declinedSwapShiftRequest.Id, HttpStatusCode.OK, declinedSwapShiftRequest.ETag, null);
                        swapShiftsIntegResponses.Add(integrationResponse);
                    }
                }
                catch (Exception ex)
                {
                    var exceptionProps = new Dictionary<string, string>()
                    {
                        { "NewFirstShiftId", newShiftFirst.Id },
                        { "NewSecondShiftId", newShiftSecond.Id },
                    };

                    this.telemetryClient.TrackException(ex, exceptionProps);
                    throw;
                }
            }

            return swapShiftsIntegResponses;
        }

        /// <summary>
        /// Process outbound open shift request.
        /// </summary>
        /// <param name="openShiftRequest">Open shift request payload.</param>
        /// <param name="updateProps">Telemetry properties.</param>
        /// <param name="teamsId">The Shifts team id.</param>
        /// <returns>Returns list of shiftIntegResponse.</returns>
        private async Task<List<ShiftsIntegResponse>> ProcessOutboundOpenShiftRequestAsync(
            OpenShiftRequestIS openShiftRequest,
            Dictionary<string, string> updateProps,
            string teamsId)
        {
            this.telemetryClient.TrackTrace($"{Resource.ProcessOutboundOpenShiftRequestAsync} starts at: {DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)}");

            List<ShiftsIntegResponse> responseModelList = new List<ShiftsIntegResponse>();
            ShiftsIntegResponse openShiftReqSubmitResponse;

            updateProps.Add("OpenShiftRequestId", openShiftRequest.Id);
            updateProps.Add("OpenShiftId", openShiftRequest.OpenShiftId);

            this.telemetryClient.TrackTrace(Resource.IncomingOpenShiftRequest, updateProps);

            // This code will be submitting the Open Shift request to Kronos.
            openShiftReqSubmitResponse = await this.openShiftRequestController.SubmitOpenShiftRequestToKronosAsync(openShiftRequest, teamsId).ConfigureAwait(false);
            responseModelList.Add(openShiftReqSubmitResponse);

            this.telemetryClient.TrackTrace($"{Resource.ProcessOutboundOpenShiftRequestAsync} ends at: {DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)}");
            return responseModelList;
        }
    }
}