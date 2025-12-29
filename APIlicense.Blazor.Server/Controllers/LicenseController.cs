using APIlicense.Blazor.Server.Models;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.MultiTenancy;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using LicenceAPIv2.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace LicenceAPIv2.Controllers
{
    [ApiController]
    [Route("api/licenses")]
    public class LicenseController : ControllerBase
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;
        private readonly ITenantProvider _tenantProvider;

        public LicenseController(IObjectSpaceFactory objectSpaceFactory, ITenantProvider tenantProvider)
        {
            _objectSpaceFactory = objectSpaceFactory;
            _tenantProvider = tenantProvider;
        }

        [HttpPost("generate")]
        public IActionResult Generate([FromBody] LicenseRequest req)
        {
            _tenantProvider.TenantId = new Guid("11111111-1111-1111-1111-111111111111"); // à adapter

            using IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace<xaf_license>();
            var session = ((XPObjectSpace)objectSpace).Session;

            var license = new xaf_license(session)
            {
                LicenseKey = GenerateKey(req),
                Type = req.Type,
                Client = req.Client,
                Product = req.Product,
                Category = req.Category,
                ExpirationDate = req.ExpirationDate,
                IsActive = false
            };

            objectSpace.CommitChanges();
            return Ok(license);
        }

        [HttpGet("check/{key}")]
        public IActionResult Check(string key)
        {
            _tenantProvider.TenantId = new Guid("11111111-1111-1111-1111-111111111111");

            using IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace<xaf_license>();
            var session = ((XPObjectSpace)objectSpace).Session;

            var license = session.Query<xaf_license>().FirstOrDefault(l => l.LicenseKey == key);
            if (license == null) return NotFound("Licence introuvable.");

            var status = license.ExpirationDate < DateTime.UtcNow
                ? "Expirée"
                : license.IsActive ? "Active" : "Inactive";

            return Ok(new
            {
                license.LicenseKey,
                license.Client,
                license.Product,
                license.ExpirationDate,
                license.IsActive,
                Status = status
            });
        }

        [HttpPost("activate/{key}")]
        public IActionResult Activate(string key)
        {
            _tenantProvider.TenantId = new Guid("11111111-1111-1111-1111-111111111111");

            using IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace<xaf_license>();
            var session = ((XPObjectSpace)objectSpace).Session;

            var license = session.Query<xaf_license>().FirstOrDefault(l => l.LicenseKey == key);
            if (license == null) return NotFound("Licence introuvable.");
            if (license.ExpirationDate < DateTime.UtcNow) return BadRequest("Licence expirée.");
            if (license.IsActive) return BadRequest("Déjà activée.");

            license.IsActive = true;
            objectSpace.CommitChanges();
            return Ok("Licence activée avec succès.");
        }

        [HttpPost("deactivate/{key}")]
        public IActionResult Deactivate(string key)
        {
            _tenantProvider.TenantId = new Guid("11111111-1111-1111-1111-111111111111");

            using IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace<xaf_license>();
            var session = ((XPObjectSpace)objectSpace).Session;

            var license = session.Query<xaf_license>().FirstOrDefault(l => l.LicenseKey == key);
            if (license == null) return NotFound("Licence introuvable.");
            if (!license.IsActive) return BadRequest("Déjà désactivée.");

            license.IsActive = false;
            objectSpace.CommitChanges();
            return Ok("Licence désactivée.");
        }

        private string GenerateKey(LicenseRequest req)
        {
            var raw = $"{req.Client}-{req.Product}-{req.Type}-{DateTime.UtcNow.Ticks}";
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw))
                .Replace("=", "")
                .Replace("+", "")
                .Replace("/", "")
                .Substring(0, 20)
                .ToUpper();
        }
    }
}
