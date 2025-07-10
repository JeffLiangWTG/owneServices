using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.Staging.NewService.Controllers
{
	public partial class RefApplicationAttributeUpdateController : StagingDataController<RefApplicationAttribute>
	{
		[InternalDataSetActionFilter]
		public override IQueryable<RefApplicationAttribute> Get()
		{
			var result = base.Get();
			return RefApplicationAttributeProcessor.GetWithoutSecretContent(result);
		}

		[InternalDataSetActionFilter]
		public override IQueryable<RefApplicationAttribute> Get(Guid key)
		{
			var result = base.Get(key);
			return RefApplicationAttributeProcessor.GetWithoutSecretContent(result);
		}

		[ActivatorUtilitiesConstructor]
		public RefApplicationAttributeUpdateController(IAuthorizationHelper authorizationHelper, IRefDbRepoCrypto refDbRepoCrypto) : base(authorizationHelper)
		{
			this.refDbRepoCrypto = refDbRepoCrypto;
		}

		public override IActionResult Post(RefApplicationAttribute data)
		{
			var result = PreCheckAndProcessRefApplicationAttribute(data);
			if (result != null)
			{
				return result;
			}
			return base.Post(data);
		}

		public override IActionResult Put([FromODataUri] Guid key, [FromBody] RefApplicationAttribute data)
		{
			var result = PreCheckAndProcessRefApplicationAttribute(data);
			if (result != null)
			{
				return result;
			}
			return base.Put(key, data);
		}

		public override IActionResult Patch([FromODataUri] Guid key, [FromBody] RefApplicationAttribute data)
		{
			var result = PreCheckAndProcessRefApplicationAttribute(data);
			if (result != null)
			{
				return result;
			}
			return base.Patch(key, data);
		}

		IActionResult PreCheckAndProcessRefApplicationAttribute(RefApplicationAttribute data)
		{
			if (!IsJobGroupValid(data))
			{
				return BadRequest($"Invalid job group: {data.RAA_JobGroup}");
			}
			RefApplicationAttributeProcessor.EncryptCredentialValue(data, refDbRepoCrypto);
			return null;
		}

		bool IsJobGroupValid(RefApplicationAttribute data)
		{
			var quartzController = new QRTZ_JOB_DETAILSUpdateController(null) { ControllerContext = base.ControllerContext };
			return QuartzJobGroupHelper.IsValidJobGroup(quartzController, data.RAA_JobGroup, data.RAA_ConfigFilePath);
		}

		readonly IRefDbRepoCrypto refDbRepoCrypto;
	}
}
