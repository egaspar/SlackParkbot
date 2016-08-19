using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Timers;
using System.Web;
using Slack.Webhooks;
using Slackbot_Traffic.Libraries;
using Slackbot_Traffic.Models;
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
		private const string ParkbotToken = "xoxb-70462373491-LcDHI0SeXyHyWfyMFeQ8LnOU";

		// this token is for testing, I think it listens to all channels
		// configure here: https://api.slack.com/docs/oauth-test-tokens
		public const string TestToken = "xoxp-69743982737-70052309687-70896054151-9504972333";

		private const string SRCoords = "-37.805783,144.944801";
		
		private readonly DateTime StartTime = new DateTime(1, 1, 1, 6, 0, 0);
		private readonly DateTime EndTime = new DateTime(1, 1, 1, 18, 0, 0);

		private const string BotName = "Parkbot";

		private readonly Dictionary<string, string> UserMapDict = new Dictionary<string, string>()
		{
			{ "U221J93L7", "-37.865379,144.971860" },
			{ "U21MUG0MD", "-37.756241,144.908088" },
			{ "U22CVHL04", "-37.813344,145.023207" }
		};
		
		#endregion Settings

		#region Declarations

		private Dictionary<string, ParkedUser> m_parkedUsers = new Dictionary<string, ParkedUser>(StringComparer.OrdinalIgnoreCase);

		private SlackClient m_postClient;

		#endregion Declarations

		#region Constructor

		public Slack()
		{
			m_postClient = new SlackClient(WebHookURL);
		}

		#endregion Constructor

		#region Commands

		internal enum ParkingTimeEnum
		{
			Out,

			[Description("1P")]
			OneHour = 1,

			[Description("2P")]
			TwoHours = 2,

			[Description("4P")]
			FourHours = 4
		}

		internal enum CommandEnum
		{
			List,
			Alert,
			Go
		}

		#endregion Commands

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
				if (message.subtype != null && message.subtype.Equals("bot_message"))
				{
					return;
				}

				//Handle each message as you receive them
				//SlackMessage testMessage = new SlackMessage
				//                {
				//                    Channel = message.channel,
				//                    Text = $"Hi {message.user}! You typed {message.text}.",
				//                    IconEmoji = Emoji.Recycle,
				//                    Username = "Friendly Bot",
				//                };
				//                m_postClient.Post(testMessage);
				// EXAMPLE message end

				ParkingTimeEnum parkingTimeRequest = EnumHelper.GetValueFromDescription<ParkingTimeEnum>(message.text);
				if (parkingTimeRequest != default(ParkingTimeEnum))
				{
					if (m_parkedUsers.ContainsKey(message.user))
					{
						ParkedUser user = m_parkedUsers[message.user];

						SlackMessage testMessage = new SlackMessage
						{
							Channel = message.channel,
							Text = $"Hi {user.UserName}! You parked at {user.TimeIn.ToShortTimeString()} for {StringHelper.CamelCaseToProperCaseWithSpace(user.ParkingDuration.ToString())}. I have updated your timer to {DateTime.Now.ToShortTimeString()} for {StringHelper.CamelCaseToProperCaseWithSpace(user.ParkingDuration.ToString())}.",
							IconEmoji = Emoji.Parking,
							Username = BotName
						};
						m_postClient.Post(testMessage);

						user.ParkingDuration = parkingTimeRequest;
						user.TimeIn = DateTime.Now;
						user.TimeOut = user.TimeIn.AddHours((int)parkingTimeRequest);
					}
					else
					{
						ParkedUser user = new ParkedUser();
						user.UserID = message.user;
						user.ParkingDuration = parkingTimeRequest;
						user.TimeIn = DateTime.Now;
						user.TimeOut = user.TimeIn.AddHours((int)parkingTimeRequest);
						user.FillInDetailsFromSlack();

						m_parkedUsers.Add(user.UserID, user);

						SlackMessage testMessage = new SlackMessage
						{
							Channel = message.channel,
							Text = $"Hi {user.UserName}! You parked at {user.TimeIn.ToShortTimeString()} for {StringHelper.CamelCaseToProperCaseWithSpace(user.ParkingDuration.ToString())}. I will send you a reminder 15 minutes before your time runs out.",
							IconEmoji = Emoji.HeavyCheckMark,
							Username = BotName
						};
						m_postClient.Post(testMessage);
					}
				}

				if (message.text.Equals(CommandEnum.List.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					string text = "No one's parked yet!";
					if (m_parkedUsers.Count > 0)
					{
						StringBuilder sb = new StringBuilder();
						sb.AppendLine("Currently parked users:");

						foreach (KeyValuePair<string, ParkedUser> user in m_parkedUsers)
						{
							sb.AppendLine($"{user.Value.UserName}\t{user.Value.TimeOut.ToShortTimeString()}");
						}

						text = sb.ToString();
					}

					SlackMessage testMessage = new SlackMessage
					{
						Channel = message.channel,
						Text = text,
						IconEmoji = Emoji.Notebook,
						Username = BotName
					};
					m_postClient.Post(testMessage);
				}

				if (message.text.Equals(CommandEnum.Alert.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					if (m_parkedUsers.Count > 0)
					{
						List<String> userlist = new List<string>();
						foreach (KeyValuePair<string, ParkedUser> parkeduser in m_parkedUsers)
						{
							userlist.Add(parkeduser.Value.Channel);
						}

						ParkedUser user = m_parkedUsers[message.user];
						user.AlertTime = DateTime.Now;

						SlackMessage alertMessage = new SlackMessage
						{
							Text = $"Parking Inspectors have been spotted in the area at {user.AlertTime.ToString()}",
							IconEmoji = Emoji.Warning,
						};
						m_postClient.PostToChannels(alertMessage, userlist);
					}
				}

				if (message.text.Equals(CommandEnum.Go.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					SlackAttachment map = new SlackAttachment();
					map.ImageUrl = $"http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/Routes/Driving?waypoint.1={SRCoords}&waypoint.2={UserMapDict[message.user]}&maxSolutions=2&mapLayer=TrafficFlow&dcl=1&key=Auz3F4FC3_a4nAFl5yUGTlhfwnu1lgRirsrSN-kelovjPLP5w1FnJ0HkBI0yVz7k";

					List<SlackAttachment> attachments = new List<SlackAttachment>();
					attachments.Add(map);

					SlackMessage testMessage = new SlackMessage
					{
						Channel = message.channel,
						Text = "Traffic data",
						IconEmoji = Emoji.VerticalTrafficLight,
						Username = BotName,
						Attachments = attachments
					};
					m_postClient.Post(testMessage);
				}
			};

			//while (true)
			//{
			//	// add the logic to check the time and send a SlackMessage to someone here

			//	// also, keep a List<StarRezUsers> or something to keep the time and other info?
			//}

			System.Timers.Timer aTimer = new System.Timers.Timer();
			aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
			aTimer.Interval = 5000;
			aTimer.Enabled = true;

			//Console.WriteLine("Press \'q\' to quit the sample.");
			//while (Console.Read() != 'q') ;
		}

		#endregion Executor

		#region Timer Methods

		// Specify what you want to happen when the Elapsed event is raised.
		private static void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			Console.WriteLine("Hello World!");

			// check if any car park is expiring

			Dictionary<string, ParkedUser> m_parkedUsers = new Dictionary<string, ParkedUser>();

			// loop through each ParkedUser
			foreach (var user in m_parkedUsers)
			{
				// check if user parking has expired
			}
		}

		//private TimeSpan CalculateDuration(string parkingTime)
		//{
		//	parkingTime.IndexOf("P");

		//}

		private bool IsTimeExpired(DateTime startTime, TimeSpan duration, out TimeSpan timeDifference)
		{
			DateTime now = DateHelper.Now;
			DateTime expiryTime = startTime.Add(duration);

			timeDifference = expiryTime.Subtract(now);

			return expiryTime >= now;
		}

		#endregion Timer Methods
	}
}