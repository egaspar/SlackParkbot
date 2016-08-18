using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Hosting.Self;
using ServiceStack.Text;
using SlackAPI;

namespace Slackbot_Traffic
{
	class Program
	{
		static void Main(string[] args)
		{
			Slack slack = new Slack();
			slack.Run();
		}
		
	}
}
