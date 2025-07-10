using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccEInvoicingTemplateFileViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEmptyTemplateCodeIsNotAllowed()
		{
			var templateConfig = Factory.New<AccEInvoicingTemplateFileView>();
			templateConfig.ETF_TemplateCode = ZString.Empty;
			templateConfig.Validation.ValidateETF_TemplateCode();
			AssertEquals(true, templateConfig.HasErrors);
		}

		public void TestListValidationTransportMode()
		{
			var testCases = new[]
			{
				new { value = ZString.Empty, isOk = false },
				new { value = (ZString)"XXX", isOk = false },
				new { value = (ZString)"AIR", isOk = true },
				new { value = (ZString)"SEA", isOk = true },
			};

			var templateConfig = Factory.New<AccEInvoicingTemplateFileView>();

			foreach (var test in testCases)
			{
				templateConfig.ETF_TransportMode = test.value;
				templateConfig.Validation.ValidateETF_TransportMode();
				AssertEquals($"{test.value}", test.isOk, !templateConfig.HasErrors);
			}
		}

		public void TestListValidationJobType()
		{
			var testCases = new[]
			{
				new { value = ZString.Empty, isOk = false },
				new { value = (ZString)"XXX", isOk = false },
				new { value = (ZString)"SHP", isOk = true },
				new { value = (ZString)"ALL", isOk = true },
			};

			var templateConfig = Factory.New<AccEInvoicingTemplateFileView>();

			foreach (var test in testCases)
			{
				templateConfig.ETF_JobType = test.value;
				templateConfig.Validation.ValidateETF_JobType();
				AssertEquals($"{test.value}", test.isOk, !templateConfig.HasErrors);
			}
		}

		public void TestCheckForDuplicates()
		{
			var fakeGcPk = ZGuid.NewZGuid();

			var templateConfigCollection = new AccEInvoicingTemplateFileViewCollection(Factory, fakeGcPk);

			var templateConfig = templateConfigCollection.AddNew();

			templateConfig.ETF_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			templateConfig.ETF_TemplateCode = "XXX";
			templateConfig.ETF_Ledger = LedgerTypes.AccountsReceivable;
			templateConfig.ETF_JobType = "ALL";
			templateConfig.Validation.ValidateAll();
			AssertEquals(false, templateConfig.HasRowErrors);

			var templateConfigDup = templateConfigCollection.AddNew();
			templateConfigDup.ETF_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			templateConfigDup.ETF_Ledger = LedgerTypes.AccountsReceivable;
			templateConfigDup.ETF_JobType = "ALL";
			templateConfigDup.ETF_TemplateCode = "XXX";
			templateConfigDup.Validation.ValidateAll();
			AssertEquals(false, templateConfigDup.HasRowErrors);

			templateConfigDup.ETF_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			AssertRowDuplicationError(templateConfigDup);

			templateConfigDup.ETF_JobType = "SHP";
			templateConfigDup.Validation.ValidateETF_JobType();
			AssertEquals(false, templateConfigDup.HasRowErrors);
			templateConfigDup.ETF_JobType = "ALL";
			templateConfigDup.Validation.ValidateETF_JobType();
			AssertRowDuplicationError(templateConfigDup);
		}

		void AssertRowDuplicationError(AccEInvoicingTemplateFileView templateFileConfig)
		{
			AssertEquals(true, templateFileConfig.HasRowErrors);
			AssertEquals(AccEInvoicingTemplateFileViewValidation.IsDuplicateErrorString, templateFileConfig.RowErrors.First().Message);
		}
	}
}
