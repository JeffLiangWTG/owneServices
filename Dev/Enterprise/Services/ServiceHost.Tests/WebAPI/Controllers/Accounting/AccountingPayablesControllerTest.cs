#if NET
using Enterprise.Accounting.Business.JobInvoicing;
using Microsoft.AspNetCore.Http;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost.Tests;

class AccountingPayablesControllerTest : TestCaseWithFactory
{
	public void TestGetTaxCode_Success()
	{
		var result = Controller.GetTaxCode("AU").GetResult();

		Assert(result.GetStatusCode() == (int)HttpStatusCode.OK);

		AssertNotNull(result.GetContent(HttpStatusCode.OK));
		AssertEquals("GST", result.GetValue(HttpStatusCode.OK));
	}

	public void TestGetDefaultAddress_WithValidOrganisation()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_Code = "TES01MLIN";
		var address1 = org.Addresses.AddNew(OrgAddressType.Payables, true);
		address1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		address1.OA_Address1 = "TEST01 ADDRESS TEST01";
		Factory.Save();

		var result =
			Controller.GetDefaultAddress(org.PK.ToGuid())
				.GetResult();

		Assert(result.GetStatusCode() == (int)HttpStatusCode.OK);

		AssertEquals(org.AddressForSendingAPDocuments.PK, address1.PK);
		AssertEquals(MessageHelper.DecorateAsResponse(HttpStatusCode.OK, $"{address1.PK}"), result.GetMessage(HttpStatusCode.OK));
	}

	public void TestGetDefaultAddress_WithInvalidOrganisation()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_Code = "TES01MLIN";
		var address1 = org.Addresses.AddNew(OrgAddressType.Payables, true);
		address1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		address1.OA_Address1 = "TEST01 ADDRESS TEST01";
		Factory.Save();

		var result =
			Controller.GetDefaultAddress(Guid.Empty)
				.GetResult();

		AssertEquals((int)HttpStatusCode.NotFound, result.GetStatusCode());
	}

	public void TestGetDueDate_Success()
	{
		var invoiceDate = new DateTime(2024, 3, 15);
		var result = Controller.GetDueDate(invoiceDate, Guid.NewGuid(), Guid.NewGuid()).GetResult();
		Assert(result.GetStatusCode() == (int)HttpStatusCode.OK);
		AssertNotNull(result.GetContent(HttpStatusCode.OK));
		AssertEquals(invoiceDate, result.GetValue(HttpStatusCode.OK));
	}

	public void TestGetJobProfitTotals_RequestIsNull()
	{
		var result = Controller.GetJobProfitTotals(null);
		var httpResponse = result.GetResult();

		AssertEquals((int)HttpStatusCode.BadRequest, httpResponse.GetStatusCode());
		AssertHelper.AssertModelState(nameof(GetJobProfitTotalsRequest), "Request body cannot be null or undefined.", result);
	}

	public void TestGetJobProfitTotals_CompanyPKIsEmpty()
	{
		var request = new GetJobProfitTotalsRequest
		{
			CompanyPK = Guid.Empty,
			JobParentInfo = new JobParentInfo { ParentId = Guid.NewGuid(), ParentTableCode = "JS" }
		};

		var result = Controller.GetJobProfitTotals(request);
		var httpResponse = result.GetResult();

		AssertEquals((int)HttpStatusCode.BadRequest, httpResponse.GetStatusCode());
		AssertHelper.AssertModelState(nameof(GetJobProfitTotalsRequest.CompanyPK), "CompanyPK cannot be an empty GUID.", result);
	}

	public void TestGetJobProfitTotals_JobParentInfoIsNull()
	{
		var request = new GetJobProfitTotalsRequest
		{
			CompanyPK = Guid.NewGuid(),
			JobParentInfo = null
		};

		var result = Controller.GetJobProfitTotals(request);
		var httpResponse = result.GetResult();

		AssertEquals((int)HttpStatusCode.BadRequest, httpResponse.GetStatusCode());
		AssertHelper.AssertModelState(nameof(GetJobProfitTotalsRequest.JobParentInfo), "JobParentInfo cannot be null or undefined.", result);
	}

	public void TestGetJobProfitTotals_JobNotFound()
	{
		var currentCompany = GlbCompany.CurrentCompany;
		var testCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "EDI");

		var testObjectCreator = new TestObjectCreator(Factory);
		var shipment = testObjectCreator.CreateShipment("S1");

		var job = testObjectCreator.CreateJob(shipment);

		AssertNotEquals("Companies are different", currentCompany, testCompany);

		Factory.Save();

		var request = new GetJobProfitTotalsRequest
		{
			CompanyPK = testCompany.PK.ToGuid(),
			JobParentInfo = new JobParentInfo
			{
				ParentId = job.JH_ParentID.ToGuid(),
				ParentTableCode = job.JH_ParentTableCode
			}
		};

		var httpResponse = Controller.GetJobProfitTotals(request)
			.GetResult();

		AssertEquals((int)HttpStatusCode.NotFound, httpResponse.GetStatusCode());
	}

	public void TestGetJobProfitTotals_ValidRequest()
	{
		var currentCompany = GlbCompany.CurrentCompany;

		var testObjectCreator = new TestObjectCreator(Factory);
		var shipment = testObjectCreator.CreateShipment("S1");

		var job = testObjectCreator.CreateJob(shipment);

		testObjectCreator.CreateCharge(job, osCostAmt: 80, osSellAmt: 100);

		Factory.Save();

		var request = new GetJobProfitTotalsRequest
		{
			CompanyPK = currentCompany.PK.ToGuid(),
			JobParentInfo = new JobParentInfo
			{
				ParentId = job.JH_ParentID.ToGuid(),
				ParentTableCode = job.JH_ParentTableCode
			}
		};

		var result = Controller.GetJobProfitTotals(request);
		var httpResponse = result.GetResult();
		var response = result.JsonResult<GetJobProfitTotalsResponse>(HttpStatusCode.OK);

		AssertEquals((int)HttpStatusCode.OK, httpResponse.GetStatusCode());
		AssertNotNull(httpResponse.GetContent(HttpStatusCode.OK));

		AssertEquals(80m, response.Cost);
		AssertEquals(100m, response.Revenue);
		AssertEquals(20m, response.Profit);
		AssertEquals(2, response.LocalDecimals);
	}

	public void TestReconcileDraftInvoice_APInvoice()
	{
		var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
		draftInvoice.AIH_TransactionType = TransactionTypes.Invoice;
		Factory.Save();
		AssertReconcileDraftInvoice<APInvoice>(draftInvoice);
	}

	public void TestReconcileDraftInvoice_APCreditNote()
	{
		var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
		draftInvoice.AIH_TransactionType = TransactionTypes.CreditNote;
		Factory.Save();
		AssertReconcileDraftInvoice<APCreditNote>(draftInvoice);
	}

	void AssertReconcileDraftInvoice<T>(AccDraftInvoiceHeader draftInvoice)
	where T : InvoicingBase
	{
		var parentId1 = new ZGuid("34893963-ba5d-4146-8a26-59ac193b6f6c");
		var parentId2 = new ZGuid("ee25f88e-c46f-4aa0-bc6e-0fd0cfc345f7");

		var dummyAccruals = new[] {
			new APReconciliationLine()
			{
				ParentId = parentId1,
				LineType = APReconciliationLineTypes.JobCharge,
				LineIdentifier = new ZGuid("A7A0E470-47F6-4699-AAF0-E05954FD3C17"),
				OSCurrency = "AUD" ,
				OSExTaxAmount = 100m ,
				LocalCurrency = "AUD",
				LocalExTaxAmount = 100m,
				CreditorPk = new ZGuid("F2884399-E2BF-4488-9A93-3D1CA0D651ED"),
				Description = "Dummy Desc 1",
				ExchangeRate = 1m
			},
			new APReconciliationLine()
			{
				ParentId = parentId1,
				LineType = APReconciliationLineTypes.ConsolCost,
				LineIdentifier = new ZGuid("729BBF4A-C3AF-43DF-BB07-BB4407D9567E"),
				OSCurrency = "USD",
				OSExTaxAmount = 200m,
				LocalCurrency = "AUD",
				LocalExTaxAmount = 100m,
				CreditorPk = new ZGuid("F2884399-E2BF-4488-9A93-3D1CA0D651ED"),
				Description = "Dummy Desc 2",
				ExchangeRate = 2m
			},
			new APReconciliationLine()
			{
				ParentId = parentId2,
				LineType = APReconciliationLineTypes.ConsolCost,
				LineIdentifier = new ZGuid("33333333-3333-3333-3333-333333333333"),
				OSCurrency = "USD",
				OSExTaxAmount = 300m,
				LocalCurrency = "AUD",
				LocalExTaxAmount = 150m,
				CreditorPk = new ZGuid("F2884399-E2BF-4488-9A93-3D1CA0D651ED"),
				Description = "Dummy Desc 3",
				ExchangeRate = 2m
			},
		};
		var mockIInvoiceConverter = MockIAPReconciliationPoster_ReconcileFromDraftInvoice<T>(draftInvoice
			, outputResult: new APReconciliationProcessingResult
			{
				Result = APReconciliationResultTypes.Success,
				FailureReason = "Dummy Fail Reason that would not be used since status is Success",
				ReconciliableAccruals = dummyAccruals
			}, default);

		using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
		{
			var result = Controller.ReconcileDraftInvoice(draftInvoice.PK.ToGuid()).GetResult();
			Assert(result.GetStatusCode() == (int)HttpStatusCode.OK);

			var json = result.GetMessage(HttpStatusCode.OK, isJson: true);
			var groupedAccruals = JsonConvert.DeserializeObject<GroupedAccruals[]>(json);
			AssertNotNull(groupedAccruals);
			AssertEquals(2, groupedAccruals.Length);

			AssertGroupedAccruals(groupedAccruals, parentId1, dummyAccruals);
			AssertGroupedAccruals(groupedAccruals, parentId2, dummyAccruals);
		}

		mockIInvoiceConverter.Verify(
			x => x.ReconcileFromDraftInvoice<T>(It.IsAny<AccDraftInvoiceHeader>(), out It.Ref<(string, string)>.IsAny)
			, Times.Exactly(1)
		);
	}

	public void TestReconcileDraftInvoice_DraftInvoiceNotExist()
	{
		var dummyPK = Guid.NewGuid();
		AssertNull(Factory.Load<AccDraftInvoiceHeader>(dummyPK));

		using (ObjectFactory.Substitute(new Mock<IInvoiceSecurityChecker>(MockBehavior.Strict).Object))
		using (ObjectFactory.Substitute(new Mock<IAPReconciliationPoster>(MockBehavior.Strict).Object))
		{
			Controller.ReconcileDraftInvoice(dummyPK).AssertResultContains(HttpStatusCode.OK, "error:Draft Transaction does not exist.");
		}
	}

	public void TestReconcileDraftInvoice_ConvertingValidationError()
	{
		var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
		draftInvoice.AIH_TransactionType = TransactionTypes.CreditNote;
		Factory.Save();

		var mockIInvoiceConverter = MockIAPReconciliationPoster_ReconcileFromDraftInvoice<APCreditNote>(draftInvoice
			, outputResult: null
			, ("Dummy Validation Error Msg", "Dummy Validation Error Caption"));

		using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
		{
			Controller.ReconcileDraftInvoice(draftInvoice.PK.ToGuid()).AssertResultContains(HttpStatusCode.OK, "error:Dummy Validation Error Msg");
		}

		mockIInvoiceConverter.Verify(
			x => x.ReconcileFromDraftInvoice<APCreditNote>(It.IsAny<AccDraftInvoiceHeader>(), out It.Ref<(string, string)>.IsAny)
			, Times.Exactly(1)
		);
	}

	public void TestReconcileDraftInvoice_ConvertingReconciliationError()
	{
		var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
		draftInvoice.AIH_TransactionType = TransactionTypes.CreditNote;
		Factory.Save();

		var mockIInvoiceConverter = MockIAPReconciliationPoster_ReconcileFromDraftInvoice<APCreditNote>(draftInvoice
			, outputResult: new APReconciliationProcessingResult
			{
				Result = APReconciliationResultTypes.Failed,
				FailureReason = "Dummy Fail Reason",
				ReconciliableAccruals = null
			}
			, default);

		using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
		{
			Controller.ReconcileDraftInvoice(draftInvoice.PK.ToGuid()).AssertResultContains(HttpStatusCode.OK, "error:Dummy Fail Reason");
		}

		mockIInvoiceConverter.Verify(
			x => x.ReconcileFromDraftInvoice<APCreditNote>(It.IsAny<AccDraftInvoiceHeader>(), out It.Ref<(string, string)>.IsAny)
			, Times.Exactly(1)
		);
	}

	Mock<IAPReconciliationPoster> MockIAPReconciliationPoster_ReconcileFromDraftInvoice<T>(AccDraftInvoiceHeader passingInDraftInvoiceHeader
		, APReconciliationProcessingResult outputResult
		, (string, string) outputValidationError)
	where T : InvoicingBase
	{
		var mockIInvoiceConverter = new Mock<IAPReconciliationPoster>(MockBehavior.Strict);
		mockIInvoiceConverter
			.Setup(x => x.ReconcileFromDraftInvoice<T>(
				It.Is<AccDraftInvoiceHeader>(x => x.PK == passingInDraftInvoiceHeader.PK)
				, out outputValidationError))
			.Returns(outputResult);
		return mockIInvoiceConverter;
	}

	void AssertGroupedAccruals(GroupedAccruals[] groupedAccruals, ZGuid parentId, IEnumerable<APReconciliationLine> expectedAccruals)
	{
		var group = groupedAccruals.FirstOrDefault(g => g.ParentId == parentId.ToString());
		AssertNotNull(group);

		var expectedIds = expectedAccruals.Where(a => a.ParentId == parentId).Select(a => a.LineIdentifier).ToList();
		var actualIds = group.Accruals.Select(a => a.LineIdentifier).ToList();

		AssertEquals(expectedIds.Count, actualIds.Count);
		AssertContainsExactElementsInAnyOrder(expectedIds, actualIds);
	}

	protected override void SetUp()
	{
		base.SetUp();
#if NET
		Controller = ControllerHelper.GetController<AccountingPayablesController, AccountingPayablesService, IHttpContextAccessor>();
#elif NETFRAMEWORK
		Controller = ControllerHelper.GetController<AccountingPayablesController>();
#endif
	}

	protected override void TearDown()
	{
#if NETFRAMEWORK
		Controller?.Dispose();
#endif
		base.TearDown();
	}

	AccountingPayablesController Controller { get; set; }

	class GroupedAccruals
	{
		public string ParentId { get; set; }
		public List<APReconciliationLine> Accruals { get; set; }
	}
}
