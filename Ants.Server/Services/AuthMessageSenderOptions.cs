using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ants.Server.Services
{
    public class AuthMessageSenderOptions
    {
        public string SendGridFromAddress { get; set; }
        public string SendGridUser { get; set; }
        public string SendGridKey { get; set; }
    }
}
