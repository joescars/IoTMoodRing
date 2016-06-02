using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using System.Web;
using System.Net.Http.Headers;
using System.IO;
using System.Net;
using System.Configuration;

namespace ExecuterProgram
{


    class RuleExecuterEventProcessor : IEventProcessor
    {

        public RuleExecuterEventProcessor()
        {

        }
        async Task IEventProcessor.CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Executer Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason);
            if (reason == CloseReason.Shutdown)
                await context.CheckpointAsync();

        }

        Task IEventProcessor.OpenAsync(PartitionContext context)
        {
            Console.WriteLine("Executer Processor initialized.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);
            return Task.FromResult<object>(null);
        }



        async Task IEventProcessor.ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                string pictureResult = "", textResult = "";

                if (eventData == null) { return; }
                string message = Encoding.UTF8.GetString(eventData.GetBytes());

                if (string.IsNullOrEmpty(message)) { return; }

                JObject json_message;

                try
                {
                    json_message = JObject.Parse(message);
                }
                catch
                {
                    // invalid json
                    Console.WriteLine("Invalid JSON: " + message);
                    return;
                }

                // TODO: Work with the message
                



                if (!string.IsNullOrEmpty(json_message["Url"].ToString()))
                {
                    pictureResult = await MakeEmotionAnalyticsRequest(json_message["Url"].ToString());
                    Console.WriteLine("picture: " + pictureResult);
                    SendMessageToCloudAsync(pictureResult);
                }

                if (!string.IsNullOrEmpty(json_message["Text"].ToString()))
                {
                    //Console.WriteLine("MESSAGE: " + json_message["Text"].ToString());
                    textResult = await MakeTextAnalyticsRequest(json_message["Text"].ToString());
                    Console.WriteLine("text: " + textResult);
                    SendMessageToCloudAsync(textResult);
                }


                //send pictureResult;
                //send textResult;

            }
            // For the sake of the demo we are checkpointing with ever event batch
            await context.CheckpointAsync();


        }



        static public async Task<string> MakeEmotionAnalyticsRequest(string message)
        {
            //Emotion[] X = await CheckEmotion("http://video.ch9.ms/ch9/9c85/8f5eb88e-ee76-4b1e-926b-7abc98b59c85/S01E04_512.jpg"); //happiness
            //Emotion[] X = await CheckEmotion("http://firstplacelosers.com/wp-content/uploads/2013/11/anger.jpg");   //anger
            //Emotion[] X = await CheckEmotion("http://www.cuded.com/wp-content/uploads/2014/04/Sad-little-child.jpg");   //neutral
            return await CheckEmotion(message);   //neutral
        }

        static public async Task<string> CheckEmotion(string url)
        {
            string subscriptionKey = ConfigurationManager.AppSettings["FaceAPIKey"];
            EmotionServiceClient emotionServiceClient = new EmotionServiceClient(subscriptionKey);

            try
            {
                //
                // Detect the emotions in the URL
                //
                Emotion[] emotionResult = await emotionServiceClient.RecognizeAsync(url);

                float anger = 0;
                float happiness = 0;
                float neutral = 0;
                string emo = "";

                //Only process if we get a result
                if (emotionResult != null)
                {
                    float currentEmo = 0;
                    float newEmo = 0;

                    foreach (Emotion emotion in emotionResult)
                    {
                        newEmo = emotion.Scores.Anger;
                        if (newEmo > currentEmo)
                            emo = "angry";currentEmo = newEmo;

                        newEmo = emotion.Scores.Happiness;
                        if (newEmo > currentEmo)
                            emo = "happy"; currentEmo = newEmo;

                        newEmo = emotion.Scores.Neutral;
                        if (newEmo > currentEmo)
                            emo = "neutral"; currentEmo = newEmo;

                        newEmo = emotion.Scores.Fear;
                        if (newEmo > currentEmo)
                            emo = "fear"; currentEmo = newEmo;

                        newEmo = emotion.Scores.Sadness;
                        if (newEmo > currentEmo)
                            emo = "sadness"; currentEmo = newEmo;

                        newEmo = emotion.Scores.Surprise;
                        if (newEmo > currentEmo)
                            emo = "suprise"; currentEmo = newEmo;

                        newEmo = emotion.Scores.Disgust;
                        if (newEmo > currentEmo)
                            emo = "disgust"; currentEmo = newEmo;
                    }
                }
                //Console.WriteLine(emotionResult[0].Scores);

                //var led = "happy";

                //if (anger > happiness && anger > neutral)
                //{
                //    led = "angry";
                //}
                //else if (neutral > happiness && neutral > anger)
                //{
                //    led = "neutral";
                //}

                //Console.WriteLine(led);

                //blue neutral
                //red anger
                //green happy

                //return led;
                return emo;
            }
            catch (Exception exception)
            {

                return null;
            }

        }



        public static bool CheckURLValid(string source)
        {
            Uri uriResult;
            return Uri.TryCreate(source, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
        }

        static async Task<string> MakeTextAnalyticsRequest(string message)
        {
            string subscriptionKey = ConfigurationManager.AppSettings["TextAnalyticsAPIKey"];
            var uri = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            Console.WriteLine("message: " + message);

            // Create a unique ID for the request
            var id = Guid.NewGuid();

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                // Request body
                string postMessage = @"{" +
                    "'documents': [" +
                    "   {" +
                    "      'language': 'en'," +
                    "      'id': '" + id + "'," +
                    "      'text': " + JsonConvert.ToString(message) +
                    "    }" +
                    "  ]" +
                    "}";

                Console.WriteLine(postMessage);

                streamWriter.Write(postMessage);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Console.WriteLine("Message: " + message);
                Console.WriteLine("Result is: " + result);

                JObject json_result = JObject.Parse(result);
                var score = json_result["documents"][0]["score"].Value<float>();
                if (score >= 0.66f)
                {
                    //happy
                    return "happy";
                }
                else if (score <= 0.33f)
                {
                    return "angry";
                }
                else
                {
                    return "neutral";
                }
                //Console.WriteLine(json_result);

            }
        }
      
        private static async void SendMessageToCloudAsync(string myMessage)
        {
            await CommandService.SendDeviceToCloudMessageAsync(myMessage);

        }
    }
}
