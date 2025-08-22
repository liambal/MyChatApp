using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsN2
{
    internal class message
    {
        public string Sender { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"{Sender} [{Timestamp:HH:mm}]: {Text}";
        }
    }
}