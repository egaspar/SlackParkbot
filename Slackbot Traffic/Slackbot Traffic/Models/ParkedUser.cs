using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slackbot_Traffic.Models
{
    internal class ParkedUser
    {
        public string UserID { get; set; }
        public string UserName { get; set; }

        public DateTime TimeParked { get; set; }
    }
}
