#if NET
using Enterprise.Accounting.Business.JobInvoicing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ObjectFactory = CargoWise.Application.ObjectFactory;
#elif NETFRAMEWORK
using CargoWise.Application;
#endif
using System;
using System.Net;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests;

class AccountingPayablesControllerTest_PostDraftInvoice_WithoutDTO : AccountingPayablesControllerTest_PostDraftInvoice
{
	protected override PosterConfigurationDTO InitDummyConfigurationDTO() => null;
}

class AccountingPayablesControllerTest_PostDraftInvoice_WithDTO : AccountingPayablesControllerTest_PostDraftInvoice
{
	protected override PosterConfigurationDTO InitDummyConfigurationDTO() => new PosterConfigurationDTO { };
}

abstract class AccountingPayablesControllerTest_PostDraftInvoice : TestCaseWithFactory
{
	public void TestPostDraftInvoice_DraftInvoiceNotExist()
	{
		var dummyPK = Guid.NewGuid();
		AssertNull(Factory.Load<AccDraftInvoiceHeader>(dummyPK));

		using (ObjectFactory.Substitute(new Mock<IInvoiceSecurityChecker>(MockBehavior.Strict).Object))
		using (ObjectFactory.Substitute(new Mock<IAPReconciliationPoster>(MockBehavior.Strict).Object))
		{
			Controller.PostDraftInvoice(dummyPK, null).AssertResultContains(HttpStatusCode.OK, "false");
		}
	}

	public void TestPostDraftInvoice_WithoutSecurity()
	{
		var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
		draftInvoice.AIH_TransactionType = TransactionTypes.CreditNote;
		Factory.Save();

		var mockIInvoiceSecurityChecker = new Mock<IInvoiceSecurityChecker>(MockBehavior.Strict);
		mockIInvoiceSecurityChecker
			.Setup(x => x.GetBasicSecuritySettings(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote))
			.Returns(new BasicSecuritySettings
			{
				New = GetDummySecurity(false)
			});

		using (ObjectFactory.Substitute(mockIInvoiceSecurityChecker.Object))
		using (ObjectFactory.Substitute(new Mock<IAPReconciliationPoster>(MockBehavior.Strict).Object))
		{
			Controller.PostDraftInvoice(draftInvoice.PK.ToGuid(), DummyConfigurationDTO).AssertResultContains(HttpStatusCode.OK, "false");
		}

		mockIInvoiceSecurityChecker.Verify(
			x => x.GetBasicSecuritySettings(It.IsAny<string>(), It.IsAny<string>())
			, Times.Exactly(1)
		);
	}

	public void TestPostDraftInvoice_ConvertingValidationError()
	{
		var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
		draftInvoice.AIH_TransactionType = TransactionTypes.CreditNote;
		Factory.Save();

		var mockIInvoiceSecurityChecker = MockPostSecurity(TransactionTypes.CreditNote);
		var mockIInvoiceConverter = MockIAPReconciliationPoster_PostFromDraftInvoice<APCreditNote>(draftInvoice
			, outputInvoice: null
			, ("Dummy Validation Error Msg", "Dummy Validation Error Caption")
			, default);

		using (ObjectFactory.Substitute(mockIInvoiceSecurityChecker.Object))
		using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
		{
			Controller.PostDraftInvoice(draftInvoice.PK.ToGuid(), DummyConfigurationDTO).AssertResultContains(HttpStatusCode.OK, "false");
		}

		mockIInvoiceSecurityChecker.Verify(
			x => x.GetBasicSecuritySettings(It.IsAny<string>(), It.IsAny<string>())
			, Times.Exactly(1)
		);
		mockIInvoiceConverter.Verify(
			x => x.PostFromDraftInvoice<APCreditNote>(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<PosterConfigurationDTO>(), out It.Ref<(string, string)>.IsAny, out It.Ref<(string, string)>.IsAny)
			, Times.Exactly(1)
		);
	}

	public void TestPostDraftInvoice_ConvertingReconciliationError()
	{
		var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
		draftInvoice.AIH_TransactionType = TransactionTypes.CreditNote;
		Factory.Save();

		var mockIInvoiceSecurityChecker = MockPostSecurity(TransactionTypes.CreditNote);
		var mockIInvoiceConverter = MockIAPReconciliationPoster_PostFromDraftInvoice<APCreditNote>(draftInvoice
			, outputInvoice: null
			, default
			, ("Dummy Reconciliation Error Msg", "Dummy Reconciliation Error Caption"));

		using (ObjectFactory.Substitute(mockIInvoiceSecurityChecker.Object))
		using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
		{
			Controller.PostDraftInvoice(draftInvoice.PK.ToGuid(), DummyConfigurationDTO).AssertResultContains(HttpStatusCode.OK, "false");
		}

		mockIInvoiceSecurityChecker.Verify(
			x => x.GetBasicSecuritySettings(It.IsAny<string>(), It.IsAny<string>())
			, Times.Exactly(1)
		);
		mockIInvoiceConverter.Verify(
			x => x.PostFromDraftInvoice<APCreditNote>(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<PosterConfigurationDTO>(), out It.Ref<(string, string)>.IsAny, out It.Ref<(string, string)>.IsAny)
			, Times.Exactly(1)
		);
	}

	public void TestPostDraftInvoice_ConvertingResultIsNull()
	{
		var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
		draftInvoice.AIH_TransactionType = TransactionTypes.CreditNote;
		Factory.Save();

		var mockIInvoiceSecurityChecker = MockPostSecurity(TransactionTypes.CreditNote);
		var mockIInvoiceConverter = MockIAPReconciliationPoster_PostFromDraftInvoice<APCreditNote>(draftInvoice
			, outputInvoice: null
			, default
			, default);

		using (ObjectFactory.Substitute(mockIInvoiceSecurityChecker.Object))
		using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
		{
			Controller.PostDraftInvoice(draftInvoice.PK.ToGuid(), DummyConfigurationDTO).AssertResultContains(HttpStatusCode.OK, "false");
		}

		mockIInvoiceSecurityChecker.Verify(
			x => x.GetBasicSecuritySettings(It.IsAny<string>(), It.IsAny<string>())
			, Times.Exactly(1)
		);
		mockIInvoiceConverter.Verify(
			x => x.PostFromDraftInvoice<APCreditNote>(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<PosterConfigurationDTO>(), out It.Ref<(string, string)>.IsAny, out It.Ref<(string, string)>.IsAny)
			, Times.Exactly(1)
		);
	}

	public void TestPostDraftInvoice_ResultValidationError()
	{
		var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
		draftInvoice.AIH_TransactionType = TransactionTypes.CreditNote;
		Factory.Save();

		var mockIInvoiceSecurityChecker = MockPostSecurity(TransactionTypes.CreditNote);
		var invalidCreditNote = Factory.NewWithValidTestData<APCreditNote>();
		invalidCreditNote.AH_TransactionNum = string.Empty;
		AssertEquals("PreCondition", true, invalidCreditNote.HasErrors);

		var mockIInvoiceConverter = MockIAPReconciliationPoster_PostFromDraftInvoice(draftInvoice
			, outputInvoice: invalidCreditNote
			, default
			, default);

		using (ObjectFactory.Substitute(mockIInvoiceSecurityChecker.Object))
		using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
		{
			Controller.PostDraftInvoice(draftInvoice.PK.ToGuid(), DummyConfigurationDTO).AssertResultContains(HttpStatusCode.OK, "false");
		}

		mockIInvoiceSecurityChecker.Verify(
			x => x.GetBasicSecuritySettings(It.IsAny<string>(), It.IsAny<string>())
			, Times.Exactly(1)
		);
		mockIInvoiceConverter.Verify(
			x => x.PostFromDraftInvoice<APCreditNote>(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<PosterConfigurationDTO>(), out It.Ref<(string, string)>.IsAny, out It.Ref<(string, string)>.IsAny)
			, Times.Exactly(1)
		);
	}

	public void TestPostDraftInvoice_Invoice()
	{
		new AccountingPeriodTestHelper().SetupPeriods();
		AssertNotNull(TestObjectCreator.TestOrganisation);
		AssertNotNull(TestObjectCreator.FEADepartment);
		Factory.Save();

		var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
		draftInvoice.AIH_TransactionType = TransactionTypes.Invoice;
		Factory.Save();

		var mockIInvoiceSecurityChecker = MockPostSecurity(TransactionTypes.Invoice);
		var outputInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV00001", TestObjectCreator.AUD, 1m
			, TestObjectCreator.TestOrganisation) as APInvoice;
		var line = TestObjectCreator.CreateInvoiceLine(outputInvoice, TestObjectCreator.AUD
			, 1m, 100m, 0m, 100m);
		line.GenericCharge = TestObjectCreator.GLHeader1.PK;
		line.AL_AT = TestObjectCreator.FREEVAT.PK;
		line.AL_Desc = "Dummy Line";

		outputInvoice.RunPreSaveValidation();
		AssertEquals("PreCondition", false, outputInvoice.HasErrors);
		var mockIInvoiceConverter = MockIAPReconciliationPoster_PostFromDraftInvoice(draftInvoice
			, outputInvoice
			, default
			, default);

		using (ObjectFactory.Substitute(mockIInvoiceSecurityChecker.Object))
		using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
		{
			Controller.PostDraftInvoice(draftInvoice.PK.ToGuid(), DummyConfigurationDTO).AssertResultContains(HttpStatusCode.OK, "true");
		}

		mockIInvoiceSecurityChecker.Verify(
			x => x.GetBasicSecuritySettings(It.IsAny<string>(), It.IsAny<string>())
			, Times.Exactly(1)
		);
		mockIInvoiceConverter.Verify(
			x => x.PostFromDraftInvoice<APInvoice>(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<PosterConfigurationDTO>(), out It.Ref<(string, string)>.IsAny, out It.Ref<(string, string)>.IsAny)
			, Times.Exactly(1)
		);
	}

	public void TestPostDraftInvoice_CreditNote()
	{
		new AccountingPeriodTestHelper().SetupPeriods();
		var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
		draftInvoice.AIH_TransactionType = TransactionTypes.CreditNote;
		Factory.Save();

		var mockIInvoiceSecurityChecker = MockPostSecurity(TransactionTypes.CreditNote);
		var outputInvoice = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "CRD00001", TestObjectCreator.AUD, 1m
			, TestObjectCreator.TestOrganisation) as APCreditNote;
		var line = TestObjectCreator.CreateInvoiceLine(outputInvoice, TestObjectCreator.AUD
			, 1m, 100m, 0m, 100m);
		line.GenericCharge = TestObjectCreator.GLHeader1.PK;
		line.AL_AT = TestObjectCreator.FREEVAT.PK;
		line.AL_Desc = "Dummy Line";

		outputInvoice.RunPreSaveValidation();
		AssertEquals("PreCondition", false, outputInvoice.HasErrors);
		var mockIInvoiceConverter = MockIAPReconciliationPoster_PostFromDraftInvoice(draftInvoice
			, outputInvoice
			, default
			, default);

		using (ObjectFactory.Substitute(mockIInvoiceSecurityChecker.Object))
		using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
		{
			Controller.PostDraftInvoice(draftInvoice.PK.ToGuid(), DummyConfigurationDTO).AssertResultContains(HttpStatusCode.OK, "true");
		}

		mockIInvoiceSecurityChecker.Verify(
			x => x.GetBasicSecuritySettings(It.IsAny<string>(), It.IsAny<string>())
			, Times.Exactly(1)
		);
		mockIInvoiceConverter.Verify(
			x => x.PostFromDraftInvoice<APCreditNote>(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<PosterConfigurationDTO>(), out It.Ref<(string, string)>.IsAny, out It.Ref<(string, string)>.IsAny)
			, Times.Exactly(1)
		);
	}

	public void TestPostDraftInvoice_FactorySavingFailed()
	{
		new AccountingPeriodTestHelper().SetupPeriods();
		var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
		draftInvoice.AIH_TransactionType = TransactionTypes.Invoice;
		Factory.Save();

		var mockIInvoiceSecurityChecker = MockPostSecurity(TransactionTypes.Invoice);
		var outputInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV00001", TestObjectCreator.AUD, 1m
			, TestObjectCreator.TestOrganisation) as APInvoice;
		var line = TestObjectCreator.CreateInvoiceLine(outputInvoice, TestObjectCreator.AUD
			, 1m, 100m, 0m, 100m);
		line.GenericCharge = TestObjectCreator.GLHeader1.PK;
		line.AL_AT = TestObjectCreator.FREEVAT.PK;
		line.AL_Desc = "Dummy Line";

		outputInvoice.RunPreSaveValidation();
		AssertEquals("PreCondition", false, outputInvoice.HasErrors);
		var mockIInvoiceConverter = MockIAPReconciliationPoster_PostFromDraftInvoice(draftInvoice
			, outputInvoice
			, default
			, default);

		using (ObjectFactory.Substitute(mockIInvoiceSecurityChecker.Object))
		using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
		{
			var controller = Controller;
			controller.ShouldPostDraftInvoiceSavingFailed_ForTestOnly = true;
			controller.PostDraftInvoice(draftInvoice.PK.ToGuid(), DummyConfigurationDTO).AssertResultContains(HttpStatusCode.OK, "false");
		}

		mockIInvoiceSecurityChecker.Verify(
			x => x.GetBasicSecuritySettings(It.IsAny<string>(), It.IsAny<string>())
			, Times.Exactly(1)
		);
		mockIInvoiceConverter.Verify(
			x => x.PostFromDraftInvoice<APInvoice>(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<PosterConfigurationDTO>(), out It.Ref<(string, string)>.IsAny, out It.Ref<(string, string)>.IsAny)
			, Times.Exactly(1)
		);
	}

	Mock<IAPReconciliationPoster> MockIAPReconciliationPoster_PostFromDraftInvoice<T>(AccDraftInvoiceHeader passingInDraftInvoiceHeader
		, T outputInvoice
		, (string, string) outputValidationError
		, (string, string) outputReconciliationError)
	where T : InvoicingBase
	{
		var mockIInvoiceConverter = new Mock<IAPReconciliationPoster>(MockBehavior.Strict);
		mockIInvoiceConverter
			.Setup(x => x.PostFromDraftInvoice<T>(
				It.Is<AccDraftInvoiceHeader>(x => x.PK == passingInDraftInvoiceHeader.PK)
				, DummyConfigurationDTO
				, out outputValidationError, out outputReconciliationError))
			.Returns(outputInvoice);
		return mockIInvoiceConverter;
	}

	Mock<IInvoiceSecurityChecker> MockPostSecurity(string transactionType)
	{
		var mockIInvoiceSecurityChecker = new Mock<IInvoiceSecurityChecker>(MockBehavior.Strict);
		mockIInvoiceSecurityChecker
			.Setup(x => x.GetBasicSecuritySettings(LedgerTypes.AccountsPayable, transactionType))
			.Returns(new BasicSecuritySettings
			{
				New = GetDummySecurity(true)
			});
		return mockIInvoiceSecurityChecker;
	}

	SecurityCheckpoint GetDummySecurity(bool isAllowed)
	{
		var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		var dummyNewSecurity = new SecurityCheckpoint("TS1", (NoResString)"Test Security", null, security.SecurityInstance);
		dummyNewSecurity.IsAllowed = isAllowed;
		return dummyNewSecurity;
	}

	protected override void SetUp()
	{
		base.SetUp();

#if NETFRAMEWORK
		Controller = ControllerHelper.GetController<AccountingPayablesController>();
#elif NET
		var services = new ServiceCollection();
		services.AddControllers();
		var serviceProvider = services.BuildServiceProvider();

		Controller = ControllerHelper.GetController<AccountingPayablesController, IAccountingPayablesService, IHttpContextAccessor>();
		Controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext
			{
				RequestServices = serviceProvider
			}
		};
#endif
	}

	protected override void TearDown()
	{
#if NETFRAMEWORK
		Controller?.Dispose();
#endif
		base.TearDown();
	}

	protected PosterConfigurationDTO DummyConfigurationDTO => (dummyConfigurationDTO ??= InitDummyConfigurationDTO());
	PosterConfigurationDTO dummyConfigurationDTO;

	protected abstract PosterConfigurationDTO InitDummyConfigurationDTO();

	AccountingPayablesController Controller { get; set; }

	TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
	TestObjectCreator testObjectCreator;
}
