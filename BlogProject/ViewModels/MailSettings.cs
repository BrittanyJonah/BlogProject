using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalBlog.ViewModels
{
    /// <summary>
    /// Configure and Use SMTP server from Google
    /// </summary>
    public class MailSettings
    {
        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; } 
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
