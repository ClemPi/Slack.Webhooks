using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;
using System.Net.Http.Formatting;

namespace Slack.Webhooks
{
    public class SlackClient
    {
        private readonly Uri _webhookUri;

        private const string VALID_HOST = "hooks.slack.com";
        private const string POST_SUCCESS = "ok";
        private static JsonMediaTypeFormatter formatter;

        public SlackClient(string webhookUrl)
        {
            if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out _webhookUri))
                throw new ArgumentException("Please enter a valid Slack webhook url");
           
            if (_webhookUri.Host != VALID_HOST)
                throw new ArgumentException("Please enter a valid Slack webhook url");
            formatter = new JsonMediaTypeFormatter();
            formatter.SerializerSettings = new  JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private string SerializePayload(SlackMessage slackMessage) {
            return JsonConvert.SerializeObject(slackMessage, new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }


        public virtual bool Post(SlackMessage slackMessage) {
            var client = new HttpClient ();
            var pl = SerializePayload (slackMessage);
            Console.WriteLine (pl);
            var response = client.PostAsync<SlackMessage> (_webhookUri.ToString(), slackMessage, formatter).Result;
            return response.IsSuccessStatusCode;
        }

        public bool PostToChannels(SlackMessage message, IEnumerable<string> channels)
		{
			return channels.DefaultIfEmpty(message.Channel)
					.Select(message.Clone)
					.Select(Post).All(r => r);
		}
        public Task<HttpResponseMessage> PostAsync(SlackMessage slackMessage)
        {
            var client = new HttpClient ();
            var response = client.PostAsync<SlackMessage> (_webhookUri.ToString(), slackMessage, formatter);
            return response;
        }


        public IEnumerable<Task<HttpResponseMessage>> PostToChannelsAsync(SlackMessage message, IEnumerable<string> channels)
        {
            return channels.DefaultIfEmpty(message.Channel)
                                .Select(message.Clone)
                                .Select(PostAsync);
        }
    }
     
}