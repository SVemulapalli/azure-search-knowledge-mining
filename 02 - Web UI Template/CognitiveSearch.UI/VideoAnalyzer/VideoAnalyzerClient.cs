// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CognitiveSearch.UI
{
    public class VideoAnalyzerClient
    {
        private IConfiguration _configuration { get; set; }

        private string apiUrl { get; set; }
        private string apiKey { get; set; }
        private string accountId { get; set; }
        private string accountLocation { get; set; }
      

        public VideoAnalyzerClient(IConfiguration configuration)
        {
            try
            {
                _configuration = configuration;

                apiUrl = "https://api.videoindexer.ai";
                apiKey = _configuration.GetSection("AVAM_Api_Key")?.Value;
                accountId = _configuration.GetSection("AVAM_Account_Id")?.Value;
                accountLocation = _configuration.GetSection("AVAM_Account_Location")?.Value;

            }
            catch (Exception e)
            {
                // If you get an exception here, most likely you have not set your
                // credentials correctly in appsettings.json
                throw new ArgumentException(e.Message.ToString());
            }
        }


        public async Task<string> GetAccessToken(string videoId)
        {
            // TLS 1.2 (or above) is required to send requests
            var httpHandler = new SocketsHttpHandler();
            httpHandler.SslOptions.EnabledSslProtocols |= SslProtocols.Tls12;
            var client = new HttpClient(httpHandler);

            var getAccountsRequest = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}/auth/{accountLocation}/Accounts/{accountId}/Videos/{videoId}/AccessToken");
            getAccountsRequest.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
            getAccountsRequest.Headers.Add("x-ms-client-request-id", Guid.NewGuid().ToString()); // Log the request id so you can include it in case you wish to report an issue for API errors or unexpected API behavior
            var result = await client.SendAsync(getAccountsRequest);
            Console.WriteLine("Response id to log: " + result.Headers.GetValues("x-ms-request-id").FirstOrDefault()); // Log the response id so you can include it in case you wish to report an issue for API errors or unexpected API behavior
            string accessToken = (await result.Content.ReadAsStringAsync()).Trim('"'); // The access token is returned as JSON value surrounded by double-quotes

            accessToken = $"https://www.videoindexer.ai/embed/player/{accountId}/{videoId}?location={accountLocation}&accessToken={accessToken}";        

            return accessToken;
        }
    }
}
