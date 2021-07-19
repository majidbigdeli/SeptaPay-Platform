using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SeptaPay.Platform.Accounting.Domain.Settings;

namespace SeptaPay.Platform.Accounting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountingController : ControllerBase
    {
        private readonly BaseAppSetting options;

        public AccountingController(IOptions<BaseAppSetting> options)
        {
            this.options = options.Value;
        }

        [HttpGet("Hello")]
        public string Hello()
        {
            return options.ConnectionString;
        }

    }
}
