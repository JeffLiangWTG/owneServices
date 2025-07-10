#if NETFRAMEWORK
using System.Web.Http;
using ControllerBase = System.Web.Http.ApiController;
using IActionResult = System.Web.Http.IHttpActionResult;
#elif NET
using Enterprise.Services.ServiceHost.NetCore;
using Microsoft.AspNetCore.Mvc;
#endif
using System;
using CargoWise.Data;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
#if NETFRAMEWORK
	[RoutePrefix("api/billing")]
#elif NET
	[Route("api/billing")]
#endif
	public class AccountingBillingController : ControllerBase
	{
		readonly IAccountingBillingService service;

#if NETFRAMEWORK
		public AccountingBillingController() : this(new AccountingBillingService())
		{
		}
#endif
		public AccountingBillingController(IAccountingBillingService service)
		{
			this.service = service;
		}

		[Route("createjobheader/{operationsJobPk}/{operationsJobTableCode}/{staffPk}/{branchPk}/{departmentPk}/{localClientAddressPk}")]
		[HttpPost]
		[HttpGet]
		public IActionResult CreateJobHeader(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk, Guid localClientAddressPk)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = service.CreateJobHeader(operationsJobPk, operationsJobTableCode, staffPk, branchPk, departmentPk, localClientAddressPk);
				return ToHttpActionResult(result);
			}
		}

		[Route("postrevenue/{operationsJobPk}/{operationsJobTableCode}/{staffPk}/{branchPk}/{departmentPk}")]
		[HttpPost]
		[HttpGet]
		public IActionResult PostRevenue(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = service.PostRevenue(operationsJobPk, operationsJobTableCode, staffPk, branchPk, departmentPk);
				return ToHttpActionResult(result);
			}
		}

		[Route("postcost/{operationsJobPk}/{operationsJobTableCode}/{staffPk}/{branchPk}/{departmentPk}")]
		[HttpPost]
		[HttpGet]
		public IActionResult PostCost(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = service.PostCost(operationsJobPk, operationsJobTableCode, staffPk, branchPk, departmentPk);
				return ToHttpActionResult(result);
			}
		}

		[Route("postoverseasagentcharges/{operationsJobPk}/{operationsJobTableCode}/{staffPk}/{branchPk}/{departmentPk}")]
		[HttpPost]
		[HttpGet]
		public IActionResult PostOverseasAgentCharges(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = service.PostOverseasAgentCharges(operationsJobPk, operationsJobTableCode, staffPk, branchPk, departmentPk);
				return ToHttpActionResult(result);
			}
		}

		[Route("splitapportionamount/{jobConsolCostPK}/{staffPk}/{branchPk}/{departmentPk}")]
		[HttpPost]
		[HttpGet]
		public IActionResult SplitApportionAmount(Guid jobConsolCostPK, Guid staffPk, Guid branchPk, Guid departmentPk)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = service.SplitApportionAmount(jobConsolCostPK, staffPk, branchPk, departmentPk);
				return ToHttpActionResult(result);
			}
		}

		IActionResult ToHttpActionResult(BillingActionResult result)
		{
			if (string.IsNullOrEmpty(result.ErrorMessage))
			{
				return Ok();
			}

			if (result.InputArgumentsHadError)
			{
				return BadRequest(result.ErrorMessage);
			}

			return Ok(result.ErrorMessage);
		}
	}
}
