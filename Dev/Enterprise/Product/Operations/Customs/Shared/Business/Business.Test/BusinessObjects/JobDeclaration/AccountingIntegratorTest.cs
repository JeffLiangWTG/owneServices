using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AccountingIntegratorTest : TestCaseWithFactory
	{
		public void TestForInformationChargesIncludeEntryReference()
		{
			var testHelper = new InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();
			RatingDataRegistry.Instance.IncludeEntryHeaderReferenceInCustomsDisbursementCharges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var declaration = Factory.New<BaseJobDeclarationWithAccIntegrationSupport>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_PaidBy = MasterFiles.Business.Customs.PaidByCodeList.Codes.CLI;
			declaration.JE_DeclarationReference = "B00010000";
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_BGMReference = "B00010000/ENT1";
			entry1.EntryNumber = "ENT1";
			entry1.Charges.AddNew("VAT", 10m);
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_BGMReference = "B00010000/ENT2";
			entry2.EntryNumber = "ENT2";
			entry2.Charges.AddNew("VAT", 20m);
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMPORG";
			var jobHeader = new JobHeader.Loader(declaration).TryLoadOrCreate();
			jobHeader.JH_OA_LocalChargesAddr = importer.MainAddress.PK;
			Factory.Save();

			new AutoRatingStarter(declaration, null).ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue.With(billingType: BillingType.Invoicing));

			var job = new Job.Loader(declaration).Load();
			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));

			CombineAssertions(() =>
			{
				AssertEquals("Two charges", 2, charges.Length);
				AssertContains("Deferred Customs Charges (Information Only) - B00010000/ENT1", charges[0].JR_Desc);
				AssertContains("Deferred Customs Charges (Information Only) - B00010000/ENT2", charges[1].JR_Desc);
			});
		}

		public void TestHasExceptionDuringIntegration()
		{
			CombineAssertions("IntegrationNotSupported", () =>
			{
				var testDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				AssertEquals(false, testDeclaration.IsIntegrationWithAccountingSupported);
				var integrator = new InvoicePostingAccountingIntegrator();
				var result = integrator.IntegrateIfNecessary(new AccIntegrationDataProviderForExceptionTest(testDeclaration));
				AssertEquals(false, result.WasSuccessful);
				AssertEquals("", result.Message);
				AssertEquals(false, result.HasChanges);
				AssertEquals(false, integrator.HasExceptionDuringIntegration);
			});
			CombineAssertions("IntegrationSupported, no Exception", () =>
			{
				var testDeclaration = Factory.NewWithValidTestData<BaseJobDeclarationWithAccIntegrationSupport>();
				testDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;
				Factory.Save();
				AssertEquals(true, testDeclaration.IsIntegrationWithAccountingSupported);
				var integrator = new InvoicePostingAccountingIntegrator();
				var result = integrator.IntegrateIfNecessary(new JobDeclarationIAccIntegrationDataProvider(testDeclaration));
				AssertEquals(false, result.WasSuccessful);
				AssertEquals("", result.Message);
				AssertEquals(false, result.HasChanges);
				AssertEquals(false, integrator.HasExceptionDuringIntegration);
			});

			var testDeclarationForException = SetupAndAssertForExceptionTest();

			AssertExceptions("Random Exception", new AccIntegrationDataProviderForExceptionTest(testDeclarationForException), true);
			AssertExceptions(IAccIntegrationDataProviderExtensionMethods.ConcurrencyExceptionUserExplanation, new AccIntegrationDataProviderForZSaveConcurrencyExceptionTest(testDeclarationForException), false);
			AssertExceptions(IAccIntegrationDataProviderExtensionMethods.SaveExceptionUserExplanation, new AccIntegrationDataProviderForZSaveExceptionTest(testDeclarationForException), false);
			AssertExceptions("Customs Invoice Raise Exception", new AccIntegrationDataProviderForCustomsInvoiceRaiseExceptionTest(testDeclarationForException), false);
		}

		BaseJobDeclarationWithAccIntegrationSupport SetupAndAssertForExceptionTest()
		{
			var testDeclaration = Factory.NewWithValidTestData<BaseJobDeclarationWithAccIntegrationSupport>();
			testDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;
			testDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			var entry = testDeclaration.CustomsEntryHeaders.AddNew();
			entry.Charges.AddNew("VAT", 100m);
			new JobHeader.Loader(testDeclaration).TryLoadOrCreate();
			Factory.Save();
			AssertEquals(true, testDeclaration.IsIntegrationWithAccountingSupported);

			return testDeclaration;
		}

		void AssertExceptions(ZString expectedExceptionMessage, JobDeclarationIAccIntegrationDataProvider accIntegrationProvider, bool haserrorReported)
		{
			CombineAssertions("IntegrationSupported, " + expectedExceptionMessage, () =>
			{
				var integrator = new InvoicePostingAccountingIntegrator();
				var result = integrator.IntegrateIfNecessary(accIntegrationProvider);
				AssertEquals(false, result.WasSuccessful);
				AssertEquals(true, result.Message.Contains(expectedExceptionMessage));
				AssertEquals(false, result.HasChanges);
				AssertEquals(true, integrator.HasExceptionDuringIntegration);

				if (haserrorReported)
				{
					Assert(ErrorReporter.LastExceptionsReported().Any(x => x.Contains("Invoicing broken")));
				}
				else
				{
					Assert(!ErrorReporter.LastExceptionsReported().Any(x => x.Contains("Invoicing broken")));
				}
			});
			ErrorReporter.Clear();
		}
	}

	internal class AccIntegrationDataProviderForExceptionTest : JobDeclarationIAccIntegrationDataProvider
	{
		public AccIntegrationDataProviderForExceptionTest(BaseJobDeclaration declaration) : base(ChargePosterBehaviours.AutoRateDSB, declaration.GetFormalEntries().Select(x => x.PK), declaration.PK, declaration.IsIntegrationWithAccountingSupported, new BusinessObjectFactory())
		{
		}

		protected override void OnIntegratedWithAccountingSuccessfully()
		{
			throw new Exception("Random Exception");
		}
	}

	internal class AccIntegrationDataProviderForZSaveExceptionTest : JobDeclarationIAccIntegrationDataProvider
	{
		public AccIntegrationDataProviderForZSaveExceptionTest(BaseJobDeclaration declaration) : base(ChargePosterBehaviours.AutoRateDSB, declaration.GetFormalEntries().Select(x => x.PK), declaration.PK, declaration.IsIntegrationWithAccountingSupported, new BusinessObjectFactory())
		{
			this.declaration = declaration;
		}

		readonly BaseJobDeclaration declaration;

		protected override void OnIntegratedWithAccountingSuccessfully()
		{
			throw new ZSaveException(new ZDataException(new ApplicationException("Save Exception"), ((INeedRow)declaration).Row, Db.Connection), declaration.Factory);
		}
	}

	internal class AccIntegrationDataProviderForZSaveConcurrencyExceptionTest : JobDeclarationIAccIntegrationDataProvider
	{
		public AccIntegrationDataProviderForZSaveConcurrencyExceptionTest(BaseJobDeclaration declaration) : base(ChargePosterBehaviours.AutoRateDSB, declaration.GetFormalEntries().Select(x => x.PK), declaration.PK, declaration.IsIntegrationWithAccountingSupported, new BusinessObjectFactory())
		{
			this.declaration = declaration;
		}

		readonly BaseJobDeclaration declaration;

		protected override void OnIntegratedWithAccountingSuccessfully()
		{
			throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new ApplicationException("Save Concurrency Exception"), ((INeedRow)declaration).Row, Db.Connection), declaration.Factory);
		}
	}

	internal class AccIntegrationDataProviderForCustomsInvoiceRaiseExceptionTest : JobDeclarationIAccIntegrationDataProvider
	{
		public AccIntegrationDataProviderForCustomsInvoiceRaiseExceptionTest(BaseJobDeclaration declaration) : base(ChargePosterBehaviours.AutoRateDSB, declaration.GetFormalEntries().Select(x => x.PK), declaration.PK, declaration.IsIntegrationWithAccountingSupported, new BusinessObjectFactory())
		{
		}

		protected override void OnIntegratedWithAccountingSuccessfully()
		{
			throw new CustomsInvoiceRaiseException("Customs Invoice Raise Exception");
		}
	}
}
