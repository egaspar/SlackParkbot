using Slack.Webhooks;
using Slackbot_Traffic.Models;
using System;
using System.Collections.Generic;
using SAPI = SlackAPI;

namespace Slackbot_Traffic
{
    public class Slack
    {
        #region Settings
        // used for posting messages
        // configure here: https://carspaceinvaders.slack.com/apps/A0F7XDUAZ-incoming-webhooks
        public const string WebHookURL = "https://hooks.slack.com/services/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";

        // this is the parkbot token, it only responds to private messages
        // configure here: https://carspaceinvaders.slack.com/services/B22DLAZA7
        private const string ParkbotToken = "xoxb-70462373491-J4akF2VoWI8Pv0K4zNpFbsyk";

        // this token is for testing, I think it listens to all channels
        // configure here: https://api.slack.com/docs/oauth-test-tokens
        private const string TestToken = "xoxp-69743982737-70052309687-70509175045-060f534a15";

        private readonly DateTime StartTime = new DateTime(1, 1, 1, 6, 0, 0);
        private readonly DateTime EndTime = new DateTime(1, 1, 1, 18, 0, 0);

        #endregion

        #region Declarations

        private List<ParkedUser> parkedUsers = new List<ParkedUser>();

        private SlackClient m_postClient;

        #endregion Declarations

        #region Constructor

        public Slack()
        {
            m_postClient = new SlackClient(WebHookURL);
        }

        #endregion Constructor

        #region Executor

        public void Run()
        {
            SAPI.SlackSocketClient parkbotClient = new SAPI.SlackSocketClient(ParkbotToken);

            // Connect the parkbot websocket to the Real Time Messaging API
            parkbotClient.Connect((connected) =>
            {
                //This is called once the client has emitted the RTM start command
                Console.WriteLine($"Parkbot RTM client has emitted the RTM start command");
            }, () =>
            {
                //This is called once the RTM client has connected to the end point
                Console.WriteLine($"Parkbot RTM client has connected to the end point");
            });

            parkbotClient.OnMessageReceived += (message) =>
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
                    m_postClient.Post(testMessage);
                }
                // EXAMPLE message end
            };

            while (true)
            {
                // add the logic to check the time and send a SlackMessage to someone here

                // also, keep a List<StarRezUsers> or something to keep the time and other info?
            }
        }

        #endregion Executor
    }
}