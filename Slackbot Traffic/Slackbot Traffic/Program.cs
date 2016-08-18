using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlackAPI;

namespace Slackbot_Traffic
{
	class Program
	{
		static void Main(string[] args)
		{
			Slack slack = new Slack();
			slack.Run();
			Console.WriteLine("Press <Enter> to exit");
			Console.ReadLine();
		}
		
	}
}
