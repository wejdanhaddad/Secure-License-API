using DevExpress.ExpressApp.MultiTenancy;
using System;

    namespace APIlicense.Blazor.Server
    {
        public class StaticTenantProvider : ITenantProvider
        {
            // Implémentation avec getter et setter
            public Guid? TenantId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

            public string TenantName { get; set; } = "DefaultTenant";

            public object TenantObject { get; set; } = null;
        }
    }




