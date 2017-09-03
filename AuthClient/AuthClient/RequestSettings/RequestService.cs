using System;
using System.Threading.Tasks;
using AuthClient.Controllers;
using AuthClient.RequestSettings.Exceptions;
using AuthClient.RequestSettings.Inerfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AuthClient.RequestSettings
{
    public class RequestService : IRequestService
    {
        private const string Defaultpath = "http://localhost:5000/UserService/";

        private readonly ILogger<RequestService> _logger;

        public RequestService(ILogger<RequestService> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> PostAsync<TResponse, TRequest>(
            string requestPath,
            TRequest data,
            IHeaderDictionary headers = null)
        {
            string responseText;

            var requestUrl = Defaultpath + requestPath;

            try
            {
                responseText = await RequestSender.SendRequestAsync(requestUrl, "Post", data, headers);
            }
            catch (Exception ex)
            {
                var msg = $"Error occured during request.  Message: {ex.Message}";

                _logger.LogError(msg);

                throw new Exception(msg);
            }

            var result = DeserializeResponse<TResponse>(responseText);

            return result;
        }

        public async Task<string> PostAsync(
           string requestPath,
           object data,
           IHeaderDictionary headers = null)
        {
            string responseText;

            var requestUrl = Defaultpath + requestPath;

            try
            {
                responseText = await RequestSender.SendRequestAsync(requestUrl, "Post", data, headers);
            }
            catch (WebRequestException ex)
            {
                var msg = $"Error occured during request. Url: {ex.Url}. Message: {ex.Message}";

                _logger.LogError(msg);

                throw new ServiceCommunicationException(msg);
            }

            return responseText;
        }
        private T DeserializeResponse<T>(string response)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<T>(response);

                return result;
            }
            catch (JsonException ex)
            {
                var msg = $"Cannot deserialize response into {typeof(T).Name}. Error message: {ex.Message}";

                _logger.LogError(msg);

                throw new Exception(msg);
            }
        }
    }
}
