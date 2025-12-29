using System;

namespace LicenceAPIv2.Models
{
    public class LicenseRequest
    {
        public string Type { get; set; }
        public string Client { get; set; }
        public string Product { get; set; }
        public string Category { get; set; }
        public DateTime ExpirationDate { get; set; }
        public object ProductKey { get; internal set; }
    }
}
