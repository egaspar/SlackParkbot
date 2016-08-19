using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
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

		private TimeSpan m_reminderTime = TimeSpan.FromMinutes(2);
		private TimeSpan m_snoozeTime = TimeSpan.FromMinutes(1);

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

			[Description("Go Driving")]
			GoDriving,

			[Description("Go Transit")]
			GoTransit,

			Go,
			Status
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

				if (message.subtype != null && message.subtype.Equals("bot_message"))
				{
					return;
				}

				if (IsPCommand(message.text))
				{
					ParkedUser user = new ParkedUser();

					if (m_parkedUsers.ContainsKey(message.user))
					{
						m_parkedUsers.Remove(message.user);
					}
					user.UserID = message.user;
					user.TimeIn = DateTime.Now;
					user.FillInDetailsFromSlack();
					user.Channel = message.channel;
					user.Duration = message.text;

					m_parkedUsers.Add(user.UserID, user);

					SlackMessage testMessage2 = new SlackMessage
					{
						Channel = message.channel,
						Text = $"Hi {user.UserName}! You parked at {user.TimeIn.ToShortTimeString()} on a {user.Duration} parking spot. I will send you a reminder {m_reminderTime.ToString("%m")} minutes before your time runs out.",
						IconEmoji = Emoji.HeavyCheckMark,
						Username = BotName
					};
					m_postClient.Post(testMessage2);
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
							sb.AppendLine($"{user.Value.UserName}\t{user.Value.TimeIn.ToShortTimeString()}\t{user.Value.Duration}");
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
						List<String> channelList = new List<string>();
						foreach (KeyValuePair<string, ParkedUser> parkeduser in m_parkedUsers)
						{
							channelList.Add(parkeduser.Value.Channel);
						}

						ParkedUser user = m_parkedUsers[message.user];
						user.AlertTime = DateTime.Now;
						user.UserID = message.user;
						user.FillInDetailsFromSlack();

						SlackMessage alertMessage = new SlackMessage
						{
							Text = $"Parking Inspectors have been spotted in the area at {user.AlertTime.ToShortTimeString()} by {user.UserName}",
							IconEmoji = Emoji.Warning,
							Username = BotName
						};
						m_postClient.PostToChannels(alertMessage, channelList);
					}
				}

				if (message.text.Equals(EnumHelper.EnumDescription(CommandEnum.GoDriving), StringComparison.OrdinalIgnoreCase) ||
					message.text.Equals(EnumHelper.EnumDescription(CommandEnum.GoTransit), StringComparison.OrdinalIgnoreCase) ||
					message.text.Equals(CommandEnum.Go.ToString(), StringComparison.OrdinalIgnoreCase)
					)
				{
					string travelMode = "Driving";
					string extraQueries = string.Empty;

					if (message.text.Equals(EnumHelper.EnumDescription(CommandEnum.GoTransit), StringComparison.OrdinalIgnoreCase))
					{
						travelMode = "Transit";
						extraQueries = $"&timeType=Departure&dateTime={DateHelper.Now.ToShortTimeString().Replace(" ", "")}";
					}

					SlackAttachment map = new SlackAttachment();
					map.ImageUrl = $"http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/Routes/{travelMode}?waypoint.1={SRCoords}&waypoint.2={UserMapDict[message.user]}&maxSolutions=2&mapLayer=TrafficFlow&dcl=1&key=Auz3F4FC3_a4nAFl5yUGTlhfwnu1lgRirsrSN-kelovjPLP5w1FnJ0HkBI0yVz7k{extraQueries}"; ;
					map.Text = $"{travelMode} traffic data if leaving at {DateHelper.Now.ToShortTimeString()}:";

					List<SlackAttachment> attachments = new List<SlackAttachment>();
					attachments.Add(map);

					SlackMessage testMessage = new SlackMessage
					{
						Channel = message.channel,
						IconEmoji = Emoji.VerticalTrafficLight,
						Username = BotName,
						Attachments = attachments
					};
					m_postClient.Post(testMessage);
				}

				if (message.text.Equals(CommandEnum.Status.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					if (!m_parkedUsers.ContainsKey(message.user))
					{
						return;
					}

					ParkedUser user = m_parkedUsers[message.user];

					TimeSpan timediff;
					IsTimeExpired(user.TimeIn, CalculateDuration(user.Duration), out timediff);

					string text = $"Hi {user.UserName}! You parked in a {user.Duration} spot at {user.TimeIn.ToShortTimeString()}. You have {Math.Round(timediff.TotalMinutes, MidpointRounding.AwayFromZero)} minutes remaining.";

					SlackMessage testMessage = new SlackMessage
					{
						Channel = message.channel,
						IconEmoji = Emoji.InformationSource,
						Username = BotName,
						Text = text
					};
					m_postClient.Post(testMessage);
				}
			};

			System.Timers.Timer aTimer = new System.Timers.Timer();
			aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
			aTimer.Interval = 5000;
			aTimer.Enabled = true;
		}

		private bool IsPCommand(string text)
		{
			Regex reg = new Regex("([0-9]+P[0-9]+|[0-9]+P|P[0-9]+)");

			return reg.IsMatch(text);
		}

		#endregion Executor

		#region Timer Methods

		// Specify what you want to happen when the Elapsed event is raised.
		private void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			// check if any car park is expiring
			// loop through each ParkedUser
			foreach (var user in m_parkedUsers)
			{
				ParkedUser parkedUser = user.Value;

				if (parkedUser.ExpiredSent && parkedUser.ReminderSent)
				{
					continue;
				}

				string text;
				TimeSpan durationRegisteredByUser = CalculateDuration(parkedUser.Duration);
				TimeSpan timeDifference;

				bool isTimeExpired = IsTimeExpired(parkedUser.TimeIn, durationRegisteredByUser, out timeDifference);
				if (isTimeExpired && !parkedUser.ExpiredSent)
				{
					text = $"Your parking time has just expired";

					// post message in user's parkbot
					SlackMessage reminderMessage = new SlackMessage
					{
						Channel = parkedUser.Channel,
						Text = text,
						IconEmoji = Emoji.Notebook,
						Username = BotName
					};
					m_postClient.Post(reminderMessage);

					parkedUser.ExpiredSent = true;
				}

				bool isReminderRequired = IsReminderRequired(parkedUser.TimeIn, durationRegisteredByUser, out timeDifference);
				if (isReminderRequired && !parkedUser.ReminderSent)
				{
					text = $"Your parking time will expire in about {m_reminderTime.ToString("%m")} minutes. You better go move your car!";

					// post message in user's parkbot
					SlackMessage reminderMessage = new SlackMessage
					{
						Channel = parkedUser.Channel,
						Text = text,
						IconEmoji = Emoji.Notebook,
						Username = BotName
					};
					m_postClient.Post(reminderMessage);

					parkedUser.ReminderSent = true;
				}
			}
		}

		private TimeSpan CalculateDuration(string parkingDurationInPNotation)
		{
			int index = parkingDurationInPNotation.IndexOf("P");

			string hourPart = parkingDurationInPNotation.Substring(0, index);
			string minutesPart = parkingDurationInPNotation.Substring(index + 1, parkingDurationInPNotation.Length - (index + 1));

			TimeSpan duration = TimeSpan.Zero;
			if (hourPart.Length > 0)
			{
				int hours = ConvertNice.ToInteger(hourPart, 0);
				duration += TimeSpan.FromHours(hours);
			}

			if (minutesPart.Length > 0)
			{
				int minutes = ConvertNice.ToInteger(minutesPart, 0);
				duration += TimeSpan.FromMinutes(minutes);
			}

			return duration;
		}

		private bool IsTimeExpired(DateTime startTime, TimeSpan duration, out TimeSpan timeDifference)
		{
			DateTime now = DateHelper.Now;
			DateTime expiryTime = startTime.Add(duration);

			timeDifference = expiryTime.Subtract(now);
			long ticksDifference = expiryTime.Ticks - now.Ticks;

			return ticksDifference <= 0;
		}

		private bool IsReminderRequired(DateTime startTime, TimeSpan duration, out TimeSpan timeLeft)
		{
			DateTime now = DateHelper.Now;
			DateTime expiryTimeLessReminderTime = startTime.Add(duration).Subtract(m_reminderTime);

			TimeSpan timeDifference = expiryTimeLessReminderTime.Subtract(now);
			timeLeft = startTime.Add(duration).Subtract(now);

			long ticksDifference = expiryTimeLessReminderTime.Ticks - now.Ticks;

			return ticksDifference <= 0;
		}

		#endregion Timer Methods
	}
}