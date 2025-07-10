#if NETFRAMEWORK
using System.Web.Http;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
using WebAppEnvironment = Enterprise.ZArchitecture.Web.Business.WebAppEnvironment;
#elif NET
using Enterprise.Services.ServiceHost.NetCore;
using Enterprise.ZArchitecture.Web.Business;
using Microsoft.AspNetCore.Http;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
#if NETFRAMEWORK
	[RoutePrefix("api/reconciliation")]
#elif NET
	[Route("api/reconciliation")]
#endif
	public class AccountingReconciliationController : ControllerBase
	{
		readonly IAPReconciliationService reconciliationService;

#if NETFRAMEWORK

		public AccountingReconciliationController() : this(new APReconciliationService())
		{
		}

		public AccountingReconciliationController(IAPReconciliationService reconciliationService)
		{
			WebAppEnvironment.Setup();
			this.reconciliationService = reconciliationService;
		}

#elif NET
		public AccountingReconciliationController(IAPReconciliationService reconciliationService, IHttpContextAccessor httpContextAccessor)
		{
			this.reconciliationService = reconciliationService;
			WebAppEnvironment.Setup(httpContextAccessor.HttpContext);
		}
#endif

		[Route("getaccruals")]
		[HttpPost]
		public IActionResult GetAccruals([FromBody] GetAccrualsRequest request)
		{
			ValidateChargeAccrualsRequest(request);
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				using (Db.DisposableActionForDbConnection())
				using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
				{
					var accrualGroupsForReconciliation = new List<JobDetailsWithAccrualsAndRelatedJobs>();

					var chargeAccrualGroups = reconciliationService.GetChargeAccruals(request.JobParentsInfo, request.CompanyPK)
						.OfType<AccrualsForAPReconciliationWithConsolInfo<Charge>>()
						.Select(ConvertChargeToAccrualDetails);
					accrualGroupsForReconciliation.AddRange(chargeAccrualGroups);

					var consolCostsAccrualGroups = reconciliationService.GetConsolCostAccruals(request.JobParentsInfo, request.CompanyPK)
						.Select(ConvertConsolCostsToAccrualDetails);
					accrualGroupsForReconciliation.AddRange(consolCostsAccrualGroups);

					var accrualsWithRelatedJobs = ConvertAccrualsWithRelatedJobs(accrualGroupsForReconciliation, request.JobParentsInfo);

					return Ok(accrualsWithRelatedJobs);
				}
			}
			catch (Exception ex)
			{
#if NETFRAMEWORK
				return InternalServerError(ex);
#elif NET
				return new ObjectResult(ex) { StatusCode = StatusCodes.Status500InternalServerError };
#endif
			}
		}

		void ValidateChargeAccrualsRequest(GetAccrualsRequest request)
		{
			if (request == null)
			{
				ModelState.AddModelError(nameof(GetAccrualsRequest), (NoResString)"Request body cannot be null.");
			}
			else if (request.CompanyPK == Guid.Empty)
			{
				ModelState.AddModelError(nameof(request.CompanyPK), (NoResString)"CompanyPK cannot be an empty GUID.");
			}
			else if (request.JobParentsInfo == null || !request.JobParentsInfo.Any())
			{
				ModelState.AddModelError(nameof(request.JobParentsInfo), (NoResString)"At least one JobParentInfo is required.");
			}
		}

		internal JobDetailsWithAccrualsAndRelatedJobs ConvertChargeToAccrualDetails(AccrualsForAPReconciliationWithConsolInfo<Charge> chargeAccruals)
		{
			return new JobDetailsWithAccrualsAndRelatedJobs
			{
				JobParentId = chargeAccruals.JobParentId,
				JobParentTableCode = chargeAccruals.JobParentTableCode,
				AccrualType = chargeAccruals.AccrualType,
				JobNumber = chargeAccruals.JobNumber,
				ConsolPk = chargeAccruals.ConsolPk,
				IsRelatedJob = chargeAccruals.IsRelatedJob,
				Accruals = chargeAccruals.Accruals.Select(accrual => new AccrualDetails
				{
					LineIdentifier = accrual.PK.ToGuid(),
					LineType = APReconciliationLineTypes.JobCharge,
					ChargeCode = accrual.ChargeCode?.AC_Code,
					ChargeDesc = accrual.JR_Desc,
					OSCurrency = accrual.JR_RX_NKCostCurrency,
					OSCostAmount = accrual.JR_OSCostAmt,
					CostExchangeRate = accrual.JR_OSCostExRate,
					LocalCurrency = accrual.Company?.LocalCurrency?.Code,
					LocalCostAmount = accrual.JR_LocalCostAmt,
					Creditor = accrual.CostAccount?.OH_Code.Trim(),
					CostTaxID = accrual.CostGSTRate?.AT_Code,
					CostTaxAmount = accrual.JR_Calc_OSCostGSTAmt,
				}).ToList(),
			};
		}

		internal JobDetailsWithAccrualsAndRelatedJobs ConvertConsolCostsToAccrualDetails(AccrualsForAPReconciliation<JobConsolCost> consolCostAccruals)
		{
			return new JobDetailsWithAccrualsAndRelatedJobs
			{
				JobParentId = consolCostAccruals.JobParentId,
				JobParentTableCode = consolCostAccruals.JobParentTableCode,
				AccrualType = consolCostAccruals.AccrualType,
				JobNumber = consolCostAccruals.JobNumber,
				Accruals = consolCostAccruals.Accruals.Where(acc => !acc.IsPosted).Select(accrual => new AccrualDetails
				{
					LineIdentifier = accrual.PK.ToGuid(),
					LineType = APReconciliationLineTypes.ConsolCost,
					ChargeCode = accrual.ChargeCode?.AC_Code,
					ChargeDesc = accrual.ChargeCodeDescription,
					OSCurrency = accrual.E6_RX_NKCurrency,
					OSCostAmount = accrual.E6_OSCostAmount,
					CostExchangeRate = accrual.E6_ExchangeRate,
					LocalCurrency = accrual.Company?.LocalCurrency?.Code,
					LocalCostAmount = accrual.E6_LocalCostAmount,
					Creditor = accrual.Creditor?.OH_Code.Trim(),
					CostTaxID = accrual.TaxRate?.AT_Code,
					CostTaxAmount = accrual.E6_OSGSTAmount_Calc,
				}).ToList(),
			};
		}

		internal IEnumerable<JobDetailsWithAccrualsAndRelatedJobs> ConvertAccrualsWithRelatedJobs(
			IEnumerable<JobDetailsWithAccrualsAndRelatedJobs> accruals, List<JobParentInfo> jobParentInfos)
		{
			var parentJobsList = accruals.Where(x => !x.IsRelatedJob).ToList();

			var missingParentJobs = jobParentInfos
				.Where(request => !parentJobsList.Any(x => x.JobParentId == request.ParentId))
				.Select(request => new JobDetailsWithAccrualsAndRelatedJobs
				{
					JobParentId = request.ParentId,
					JobParentTableCode = request.ParentTableCode,
					AccrualType = AccrualSourceTypes.Consol,
					Accruals = new List<AccrualDetails>(),
				});

			parentJobsList.AddRange(missingParentJobs);

			var accrualsList = accruals.Where(x => x.AccrualType == AccrualSourceTypes.Job && x.ConsolPk != null && x.IsRelatedJob).ToList();

			foreach (var result in parentJobsList)
			{
				var relatedJobs = accrualsList
					.Where(accrual => accrual.ConsolPk == result.JobParentId)
					.Select(accrual => new JobDetails
					{
						JobId = accrual.JobParentId,
						JobNumber = accrual.JobNumber,
						ParentTableCode = accrual.JobParentTableCode,
						Accruals = accrual.Accruals,
					})
					.GroupBy(job => new { job.JobId, job.JobNumber })
					.Select(group => group.First())
					.ToList();

				if (relatedJobs.Any())
				{
					result.RelatedJobs = result.RelatedJobs == null
						? relatedJobs
						: result.RelatedJobs.Concat(relatedJobs).ToList();
				}
			}

			return parentJobsList;
		}

#nullable enable
		public class AccrualDetails : IReconciliationLineIdentifier
		{
			public Guid LineIdentifier { get; set; }
			public APReconciliationLineTypes LineType { get; set; }
			public string? ChargeCode { get; set; }
			public string? ChargeDesc { get; set; }
			public string? OSCurrency { get; set; }
			public decimal OSCostAmount { get; set; }
			public decimal CostExchangeRate { get; set; }
			public string? LocalCurrency { get; set; }
			public decimal LocalCostAmount { get; set; }
			public string? Creditor { get; set; }
			public string? CostTaxID { get; set; }
			public decimal CostTaxAmount { get; set; }
		}

		public class GetAccrualsRequest
		{
			public List<JobParentInfo> JobParentsInfo { get; set; } = [];

			public Guid CompanyPK { get; set; }
		}

		public class JobDetails
		{
			public ZGuid? JobId { get; set; }
			public string? JobNumber { get; set; }
			public string? ParentTableCode { get; set; }
			public IEnumerable<AccrualDetails>? Accruals { get; set; }
		}

		public class JobDetailsWithAccrualsAndRelatedJobs
		{
			public ZGuid JobParentId { get; set; }
			public string JobParentTableCode { get; set; } = string.Empty;
			public AccrualSourceTypes AccrualType { get; set; }
			public string JobNumber { get; set; } = string.Empty;
			public ZGuid? ConsolPk { get; set; }
			public bool IsRelatedJob { get; set; }
			public List<AccrualDetails>? Accruals { get; set; } = [];
			public List<JobDetails>? RelatedJobs { get; set; }
		}
	}
}
