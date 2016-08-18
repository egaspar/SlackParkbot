using System;
using System.Net;
using Newtonsoft.Json;

namespace Slackbot_Traffic.Models
{
	internal class ParkedUser
	{
		public string UserID { get; set; }
		public string UserName { get; set; }

		public string Email { get; set; }

		public DateTime TimeIn { get; set; }

		public DateTime TimeOut { get; set; }

		public Slack.ParkingTimeEnum ParkingDuration { get; set; } = Slack.ParkingTimeEnum.Out;
		
		public void FillInDetailsFromSlack()
		{
			if (!string.IsNullOrEmpty(this.UserID))
			{
				using (WebClient client = new WebClient())
				{
						dynamic response = JsonConvert.DeserializeObject(client.DownloadString($"https://slack.com/api/users.info?token={Slack.TestToken}&user={this.UserID}"));
						this.Email = response.user.profile.email.ToString();
						this.UserName = response.user.profile.first_name.ToString();
				}
			}
		}
	}
}