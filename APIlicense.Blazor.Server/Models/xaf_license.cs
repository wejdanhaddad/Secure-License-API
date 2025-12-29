using System;
using DevExpress.Xpo;
using DevExpress.Persistent.Base;

namespace APIlicense.Blazor.Server.Models
{
    [Persistent("xaf_license")] 
    [DefaultClassOptions]
    public class xaf_license : XPObject
    {
        public xaf_license(Session session) : base(session) { }

        private string licenseKey;
        private string type;
        private string client;
        private string product;
        private string category;
        private DateTime expirationDate;
        private bool isActive;

        [Size(255)]
        public string LicenseKey
        {
            get => licenseKey;
            set => SetPropertyValue(nameof(LicenseKey), ref licenseKey, value);
        }

        public string Type
        {
            get => type;
            set => SetPropertyValue(nameof(Type), ref type, value);
        }

        public string Client
        {
            get => client;
            set => SetPropertyValue(nameof(Client), ref client, value);
        }

        public string Product
        {
            get => product;
            set => SetPropertyValue(nameof(Product), ref product, value);
        }

        public string Category
        {
            get => category;
            set => SetPropertyValue(nameof(Category), ref category, value);
        }

        public DateTime ExpirationDate
        {
            get => expirationDate;
            set => SetPropertyValue(nameof(ExpirationDate), ref expirationDate, value);
        }

        public bool IsActive
        {
            get => isActive;
            set => SetPropertyValue(nameof(IsActive), ref isActive, value);
        }
    }
}
