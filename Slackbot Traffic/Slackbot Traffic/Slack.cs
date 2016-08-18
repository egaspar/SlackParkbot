using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slack.Webhooks;
using SAPI = SlackAPI;

namespace Slackbot_Traffic
{
	public class Slack
	{
		// used for posting messages
		// configure here: https://carspaceinvaders.slack.com/apps/A0F7XDUAZ-incoming-webhooks
		public const string WebHookURL = "https://hooks.slack.com/services/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";

		// this is the parkbot token, it only responds to private messages
		// configure here: https://carspaceinvaders.slack.com/services/B22DLAZA7
		private const string ParkbotToken = "xoxb-70462373491-3i42LI2lO6IPRlDOLzzcHfP5";

        // this token is for testing, I think it listens to all channels
        // configure here: https://api.slack.com/docs/oauth-test-tokens
        private const string TestToken = "xoxp-69743982737-70052309687-70509175045-060f534a15";


		public void Run()
		{
			SAPI.SlackSocketClient client = new SAPI.SlackSocketClient(ParkbotToken);
			
			// Connect the websocket to the Real Time Messaging API
			client.Connect((connected) => {
				//This is called once the client has emitted the RTM start command
				Console.WriteLine($"Emitted the RTM start command");
			}, () => {
				//This is called once the RTM client has connected to the end point
				Console.WriteLine($"RTM client has connected to the end point");
			});


			SlackClient postClient = new SlackClient(WebHookURL);

			// this will 
			client.OnMessageReceived += (message) =>
			{			
				// Parse, add if conditions, and respond as needed

				// EXAMPLE message
				if (message.subtype == null || !message.subtype.Equals("bot_message"))
				{
					//Handle each message as you receive them
					SlackMessage testMessage = new SlackMessage
					{
						Channel = message.channel,
						Text = $"Hi {message.user}! You typed {message.text}.",
						IconEmoji = Emoji.Recycle,
						Username = "Friendly Bot",
						
					};
					postClient.Post(testMessage);
				}
				// EXAMPLE message end


                // Example alter message. Any user who sends the ALERT command, post to all users that parking inspectors have been spotted
                if (message.subtype.Equals("ALERT") || !message.subtype.Equals("bot_message"))
                {
                    SlackMessage alertMessage = new SlackMessage
                    {


                    }



                }

			};

			while(true)
			{
				// add the logic to check the time and send a SlackMessage to someone here

				// also, keep a List<StarRezUsers> or something to keep the time and other info?
			}
		}
	}
}
