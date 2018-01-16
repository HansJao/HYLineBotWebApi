using Line.Messaging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Line.Messaging.Webhooks;
using System.Net;
using System.IO;

namespace HYLineWebApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        static LineMessagingClient lineMessagingClient;
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            //lineMessagingClient = new LineMessagingClient("VO8CJj2Uwn2h5Mjm4884whpRKOXonme17QnbPQXatFKIDckf33rFM8jL+8Qv0hCPY0unc80NrZiWKR/Ut4qv1gSuRUAYdXZwMhctijKzqsVRbVD3Vm1STrcdMQzzu0QKeTjd/5pFDHF6jc9w35OKbwdB04t89/1O/w1cDnyilFU=");
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // // POST api/values
        // [HttpPost]
        // public void Post([FromBody]string value)
        // {
        // }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

         public ValuesController()
        {
            lineMessagingClient = new LineMessagingClient("VO8CJj2Uwn2h5Mjm4884whpRKOXonme17QnbPQXatFKIDckf33rFM8jL+8Qv0hCPY0unc80NrZiWKR/Ut4qv1gSuRUAYdXZwMhctijKzqsVRbVD3Vm1STrcdMQzzu0QKeTjd/5pFDHF6jc9w35OKbwdB04t89/1O/w1cDnyilFU=");
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [HttpPost]
        public async Task<HttpResponseMessage> Post(HttpRequestMessage request)
        {
            var postData = string.Empty;
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                postData = reader.ReadToEnd();
            }
            var events =  WebhookEventParser.Parse(postData);
           // var events = await request.GetWebhookEventsAsync("c1f910527e6456141087387d2ce2b783");
            //lineMessagingClient = new LineMessagingClient("VO8CJj2Uwn2h5Mjm4884whpRKOXonme17QnbPQXatFKIDckf33rFM8jL+8Qv0hCPY0unc80NrZiWKR/Ut4qv1gSuRUAYdXZwMhctijKzqsVRbVD3Vm1STrcdMQzzu0QKeTjd/5pFDHF6jc9w35OKbwdB04t89/1O/w1cDnyilFU=");
            LinebotApp app = new LinebotApp(lineMessagingClient);
            await app.RunAsync(events);
            var response = new HttpResponseMessage(HttpStatusCode.OK);//test
            return response;
        }      
    }
}
