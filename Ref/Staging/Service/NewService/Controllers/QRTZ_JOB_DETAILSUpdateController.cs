using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Staging.Common;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;

namespace CargoWise.RefDbRepo.Staging.NewService.Controllers
{
	public partial class QRTZ_JOB_DETAILSUpdateController
	{
		[ODataEnableQuery]
		public override IQueryable<QRTZ_JOB_DETAILS> Get()
		{
			var qrtzJobs = base.Get();
			Argument.NotNull(qrtzJobs, nameof(qrtzJobs));
			foreach (var qrtzJob in qrtzJobs)
			{
				QRTZ_JOB_DETAILSHelper.SetCalculatedProperties(qrtzJob);
			}
			return qrtzJobs;
		}

		[ODataEnableQuery]
		public override IQueryable<QRTZ_JOB_DETAILS> Get([FromODataUri] Guid key)
		{
			var qrtzJobs = base.Get(key);
			Argument.NotNull(qrtzJobs, nameof(qrtzJobs));
			foreach (var qrtzJob in qrtzJobs)
			{
				QRTZ_JOB_DETAILSHelper.SetCalculatedProperties(qrtzJob);
			}
			return qrtzJobs;
		}

		// We do not use these endpoints. I commented them out instead of deleting them because in case we need them in the future,
		// we should be aware that these endpoints have vulnerabilities : if attackers post a OS script as a job and quartz will execute it,
		// compromising the whole system and network. For info can be obtained in the See 3.1 and 3.4 https://wisetechglobal.sharepoint.com/sites/ISMS/InfoSec/Forms/AllItems.aspx?id=%2Fsites%2FISMS%2FInfoSec%2F60%20Penetration%20Testing%2F20%20Reports%2FReference%20Data%2F202403%5FWiseTech%20Global%20%2D%201081%20%2D%20Reference%20Data%20Web%20Application%20and%20API%20Penetration%20Test%20Report%20v1%2Epdf&parent=%2Fsites%2FISMS%2FInfoSec%2F60%20Penetration%20Testing%2F20%20Reports%2FReference%20Data

		public override IActionResult Post(QRTZ_JOB_DETAILS data)
		{
			throw new NotImplementedException();
		}

		public override IActionResult Put([FromODataUri] Guid key, [FromBody] QRTZ_JOB_DETAILS data)
		{
			throw new NotImplementedException();
		}

		public override IActionResult Patch([FromODataUri] Guid key, [FromBody] QRTZ_JOB_DETAILS data)
		{
			throw new NotImplementedException();
		}
	}
}
