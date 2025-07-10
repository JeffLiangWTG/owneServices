#if NETFRAMEWORK
using System.Web.Http;
using ControllerBase = System.Web.Http.ApiController;
using IActionResult = System.Web.Http.IHttpActionResult;
using WebAppEnvironment = Enterprise.ZArchitecture.Web.Business.WebAppEnvironment;
#elif NET
using System.Net.Mime;
using Enterprise.Services.ServiceHost.NetCore;
using Enterprise.ZArchitecture.Web.Business;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Res = Enterprise.Services.ServiceHost.NetCore.Res;
#endif
using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
#if NETFRAMEWORK
	[RoutePrefix("api/payables")]
#elif NET
	[Route("api/payables")]
#endif
	public class AccountingPayablesController : ControllerBase
	{
		readonly IAccountingPayablesService service;

#if NETFRAMEWORK

		public AccountingPayablesController() : this(new AccountingPayablesService())
		{
		}

		public AccountingPayablesController(IAccountingPayablesService service)
		{
			WebAppEnvironment.Setup();
			this.service = service;
		}

#elif NET
		public AccountingPayablesController(IAccountingPayablesService service, IHttpContextAccessor httpContextAccessor)
		{
			this.service = service;
			WebAppEnvironment.Setup(httpContextAccessor.HttpContext);
		}
#endif

		[Route("gettaxcode")]
		[HttpGet]
#if NET
		[Produces(MediaTypeNames.Application.Json)]
#endif
		public IActionResult GetTaxCode(string countryCode)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = service.GetTaxCode(countryCode);
				if (string.IsNullOrEmpty(result))
				{
					return Ok();
				}
				return Ok(result);
			}
		}

		[Route("getDefaultAddress/{orgHeaderPK}")]
		[HttpGet]
		public IActionResult GetDefaultAddress(Guid orgHeaderPK)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = new BusinessObjectFactory();
				var header = factory.Load<OrgHeader>(orgHeaderPK);

				if (header == null)
				{
					return NotFound();
				}

				return Ok(header.AddressForSendingAPDocuments.PK);
			}
		}

		[Route("getduedate")]
		[HttpGet]
		public IActionResult GetDueDate(DateTime invoiceDate, Guid companyPK, Guid creditorPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = service.GetDueDate(invoiceDate, companyPK, creditorPK);
				return Ok(result);
			}
		}

		[Route("reconciledraftinvoice/{draftInvoicePK}")]
		[HttpGet]
		public IActionResult ReconcileDraftInvoice(Guid draftInvoicePK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory();
				var draftInvoiceHeader = factory.Load<AccDraftInvoiceHeader>(draftInvoicePK);

				if (draftInvoiceHeader == null)
				{
					return Ok(FormatErrorMsg(Res.GetString("DD433AD1-9CF1-47AA-BB23-454F3B6ADADC", "Draft Transaction does not exist.")));
				}

				var result = Reconcile(draftInvoiceHeader
					, out (string Msg, string Caption) validationError);

				if (validationError != default)
				{
					return Ok(FormatErrorMsg(validationError.Msg));
				}

				if (result.Result != APReconciliationResultTypes.Success)
				{
					return Ok(FormatErrorMsg(result.FailureReason));
				}

				var groupedAccruals = result.ReconciliableAccruals
					.GroupBy(accrual => accrual.ParentId)
					.Select(accrualGroup => new
					{
						ParentId = accrualGroup.Key,
						Accruals = accrualGroup.ToList()
					});

				return Ok(groupedAccruals);
			}
		}

		[Route("getJobProfitTotals")]
		[HttpPost]
		public IActionResult GetJobProfitTotals([FromBody] GetJobProfitTotalsRequest request)
		{
			if (request == null)
			{
				ModelState.AddModelError(nameof(GetJobProfitTotalsRequest), (NoResString)"Request body cannot be null or undefined.");
			}
			else if (request.CompanyPK == Guid.Empty)
			{
				ModelState.AddModelError(nameof(request.CompanyPK), (NoResString)"CompanyPK cannot be an empty GUID.");
			}
			else if (request.JobParentInfo == null)
			{
				ModelState.AddModelError(nameof(request.JobParentInfo), (NoResString)"JobParentInfo cannot be null or undefined.");
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory();
				var filter = new ZQuery(JobHeaderSchema.JH_ParentID, request!.JobParentInfo!.ParentId);
				filter.AddToFilter(JobHeaderSchema.JH_ParentTableCode, request.JobParentInfo.ParentTableCode);
				filter.AddToFilter(JobHeaderSchema.JH_GC, request.CompanyPK);
				var jobHeader = factory.Load<Job>(filter).FirstOrDefault();

				if (jobHeader == null)
				{
					return NotFound();
				}

				var response = new GetJobProfitTotalsResponse
				{
					Cost = jobHeader.JH_TotalCost,
					Revenue = jobHeader.JH_TotalRevenue,
					Profit = jobHeader.JH_ProfitLoss,
					LocalDecimals = jobHeader.JH_LocalCurrencyDecimals,
				};

				return Ok(response);
			}
		}

		string FormatErrorMsg(string msg)
		{
			return $"error:{msg}";
		}

		[Route("postdraftinvoice/{draftInvoicePK}")]
		[HttpPost]
		public IActionResult PostDraftInvoice(Guid draftInvoicePK, [FromBody] PosterConfigurationDTO configurationDTO)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory();
				var draftInvoiceHeader = factory.Load<AccDraftInvoiceHeader>(draftInvoicePK);

				if (draftInvoiceHeader == null)
				{
					return Ok(false);
				}

				var checkPointForPost = ObjectFactory
					.Get<IInvoiceSecurityChecker>()
					.GetBasicSecuritySettings(LedgerTypes.AccountsPayable, draftInvoiceHeader.AIH_TransactionType)
					.New;
				if (!checkPointForPost.IsAllowed)
				{
					return Ok(false);
				}

				var invoice = ConvertToInvoice(draftInvoiceHeader
					, configurationDTO
					, out (string Msg, string Caption) validationError
					, out (string Msg, string Caption) reconciliationError);

				if (invoice == null || validationError != default || reconciliationError != default)
				{
					return Ok(false);
				}

				invoice.RunPreSaveValidation();
				if (invoice.HasErrors)
				{
					return Ok(false);
				}

				var factorySavedSuccessfully = false;
				invoice.Factory.Saved += (factory, savedSuccessfully) =>
				{
					factorySavedSuccessfully = savedSuccessfully;
#if DEBUG
					if (ShouldPostDraftInvoiceSavingFailed_ForTestOnly)
					{
						factorySavedSuccessfully = false;
					}
#endif
				};
				invoice.Factory.Save();

				return Ok(factorySavedSuccessfully);
			}
		}

#if DEBUG

		public bool ShouldPostDraftInvoiceSavingFailed_ForTestOnly;
#endif

		InvoicingBase ConvertToInvoice(AccDraftInvoiceHeader draftInvoiceHeader
			, PosterConfigurationDTO configurationDTO
			, out (string Msg, string Caption) validationError
			, out (string Msg, string Caption) reconciliationError)
		{
			validationError = default;
			reconciliationError = default;
			return (string)draftInvoiceHeader.AIH_TransactionType switch
			{
				TransactionTypes.CreditNote
					=> ObjectFactory.Get<IAPReconciliationPoster>()
						.PostFromDraftInvoice<APCreditNote>(draftInvoiceHeader
							, configurationDTO
							, out validationError
							, out reconciliationError
						),
				TransactionTypes.Invoice
					=> ObjectFactory.Get<IAPReconciliationPoster>()
						.PostFromDraftInvoice<APInvoice>(draftInvoiceHeader
							, configurationDTO
							, out validationError
							, out reconciliationError
						),
				_ => null,
			};
		}

		APReconciliationProcessingResult Reconcile(AccDraftInvoiceHeader draftInvoiceHeader
			, out (string Msg, string Caption) validationError)
		{
			validationError = default;
			return (string)draftInvoiceHeader.AIH_TransactionType switch
			{
				TransactionTypes.CreditNote
					=> ObjectFactory.Get<IAPReconciliationPoster>()
						.ReconcileFromDraftInvoice<APCreditNote>(draftInvoiceHeader
							, out validationError
						),
				TransactionTypes.Invoice
					=> ObjectFactory.Get<IAPReconciliationPoster>()
						.ReconcileFromDraftInvoice<APInvoice>(draftInvoiceHeader
							, out validationError
						),
				_ => null,
			};
		}
	}
}
