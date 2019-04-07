using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Anomaly.Detector.API
{
    public class Detection
    {

        const string latestPointDetectionUrl = "/anomalydetector/v1.0/timeseries/last/detect";
        const string batchDetectionUrl = "/anomalydetector/v1.0/timeseries/entire/detect";

        /// <summary>
        /// Detect anomalies in your time series data using the Anomaly Detector REST API
        /// </summary>
        /// <param name="subscriptionKey">The subscriptionKey string value with your valid subscription key</param>
        /// <param name="endpoint">Replace the endpoint URL with the correct one for your subscription. Your endpoint can be found in the Azure portal. For example: https://westus2.api.cognitive.microsoft.com</param>
        /// <param name="dataPath">The dataPath string with a path to the JSON formatted time series data.</param>
        /// <param name="latestPointDetectionUrl"></param>
        /// <param name="batchDetectionUrl"></param>
        /// <returns></returns>
        public Result AnomalyDetection(string subscriptionKey, string endpoint, string dataPath, string latestPointDetectionUrl, string batchDetectionUrl)
        {
            var requestData = File.ReadAllText(dataPath);
            Result result = new Result();
            result.Batchs = detectAnomaliesBatch(requestData, endpoint, subscriptionKey);
            result.LastestNomalies = detectAnomaliesLatest(requestData, endpoint, subscriptionKey);
            return result;
        }
        #region private

        private static async Task<string> RequestAPI(string baseAddress, string endpoint, string subscriptionKey, string requestData)
        {
            using (HttpClient client = new HttpClient { BaseAddress = new Uri(baseAddress) })
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                var content = new StringContent(requestData, Encoding.UTF8, "application/json");
                var res = await client.PostAsync(endpoint, content);
                if (res.IsSuccessStatusCode)
                {
                    return await res.Content.ReadAsStringAsync();
                }
                else
                {
                    return $"ErrorCode: {res.StatusCode}";
                }
            }
        }

        private static List<int> detectAnomaliesBatch(string requestData, string endpoint, string subscriptionKey)
        {
            List<int> batchResult = new List<int>();
            string requestResult = RequestAPI(
                endpoint,
                batchDetectionUrl,
                subscriptionKey,
                requestData).Result;

            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(requestResult);
            bool[] anomalies = jsonObj["isAnomaly"].ToObject<bool[]>();
            for (var i = 0; i < anomalies.Length; i++)
            {
                if (anomalies[i])
                {
                    batchResult.Add(i);
                }
            }
            return batchResult;
        }

        private static dynamic detectAnomaliesLatest(string requestData, string endpoint, string subscriptionKey)
        {
            string requesrtResult = RequestAPI(
                endpoint,
                latestPointDetectionUrl,
                subscriptionKey,
                requestData).Result;
            return Newtonsoft.Json.JsonConvert.DeserializeObject(requesrtResult);

        }
        #endregion 
        public class Result
        {
            public List<int> Batchs { get; set; }
            public string LastestNomalies { get; set; }
        }

    }
}
