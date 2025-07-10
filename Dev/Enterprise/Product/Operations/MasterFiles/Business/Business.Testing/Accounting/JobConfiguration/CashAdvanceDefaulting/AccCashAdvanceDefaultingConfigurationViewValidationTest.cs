using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCashAdvanceDefaultingConfigurationViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLedgerValidation()
		{
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			config.CAC_Ledger = LedgerTypes.CashBook;
			AssertHasError(config.CAC_LedgerInfo, "Enter a valid Ledger.");
			config.CAC_Ledger = LedgerTypes.AccountsReceivable;
			AssertNoErrors(config.CAC_LedgerInfo);
			config.CAC_Ledger = LedgerTypes.AccountsPayable;
			AssertNoErrors(config.CAC_LedgerInfo);
			config.CAC_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			config.CAC_Ledger = ZString.Empty;
			AssertHasError(config.CAC_LedgerInfo, "Please enter a Ledger.");
			config.CAC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			config.CAC_Ledger = ZString.Empty;
			AssertNoErrors("Empty Ledger = 'ALL'", config.CAC_LedgerInfo);
		}

		public void TestJobTypeValidation()
		{
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			config.CAC_JobType = ZString.Empty;
			AssertHasError(config.CAC_JobTypeInfo, "Please enter a Job Type.");
			config.CAC_JobType = "123";
			AssertHasError(config.CAC_JobTypeInfo, "Enter a valid Job Type.");
			config.CAC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertNoErrors(config.CAC_JobTypeInfo);
		}

		public void TestTransportModeValidation()
		{
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			config.CAC_TransportMode = ZString.Empty;
			AssertHasError(config.CAC_TransportModeInfo, "Please enter a Transport Mode.");
			config.CAC_TransportMode = "123";
			AssertHasError(config.CAC_TransportModeInfo, "Enter a valid Transport Mode.");
			config.CAC_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoErrors(config.CAC_TransportModeInfo);
		}

		public void TestServiceDirectionValidation()
		{
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			config.CAC_ServiceDirection = ZString.Empty;
			AssertHasError(config.CAC_ServiceDirectionInfo, "Please enter a Service Direction.");
			config.CAC_ServiceDirection = "123";
			AssertHasError(config.CAC_ServiceDirectionInfo, "Enter a valid Service Direction.");
			config.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Export;
			AssertNoErrors(config.CAC_ServiceDirectionInfo);
		}

		public void TestDefaultingOptionValidation()
		{
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			config.CAC_DefaultingOption = ZString.Empty;
			AssertHasError(config.CAC_DefaultingOptionInfo, "Please enter a Defaulting Charges.");
			config.CAC_DefaultingOption = "123";
			AssertHasError(config.CAC_DefaultingOptionInfo, "Enter a valid Defaulting Charges.");
			config.CAC_DefaultingOption = CashAdvanceDefaultingOption.All;
			AssertNoErrors(config.CAC_DefaultingOptionInfo);
		}

		public void TestParentTableCodeCanBeEmpty()
		{
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			config.CAC_ParentTableCode = ZString.Empty;
			AssertNoErrors("Please enter a valid value.", config.CAC_ParentTableCodeInfo);

			config.CAC_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertNoErrors("Please enter a valid value.", config.CAC_ParentTableCodeInfo);
		}

		public void TestIsDuplicate()
		{
			var collection = new AccCashAdvanceDefaultingConfigurationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			collection.Add(config);
			var duplicate = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			collection.Add(duplicate);

			duplicate.CAC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertNoRowError(duplicate, "Another record already sets Advance Payment Defaulting Configuration for the same Job parameters.");
			duplicate.CAC_JobType = config.CAC_JobType;
			AssertHasRowError(duplicate, "Another record already sets Advance Payment Defaulting Configuration for the same Job parameters.");

			duplicate.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Domestic;
			AssertNoRowError(duplicate, "Another record already sets Advance Payment Defaulting Configuration for the same Job parameters.");
			duplicate.CAC_ServiceDirection = config.CAC_ServiceDirection;
			AssertHasRowError(duplicate, "Another record already sets Advance Payment Defaulting Configuration for the same Job parameters.");

			duplicate.CAC_TransportMode = Core.Constants.TransportModes.Road;
			AssertNoRowError(duplicate, "Another record already sets Advance Payment Defaulting Configuration for the same Job parameters.");
			duplicate.CAC_TransportMode = config.CAC_TransportMode;
			AssertHasRowError(duplicate, "Another record already sets Advance Payment Defaulting Configuration for the same Job parameters.");

			duplicate.CAC_Ledger = LedgerTypes.AccountsPayable;
			AssertNoRowError(duplicate, "Another record already sets Advance Payment Defaulting Configuration for the same Job parameters.");
			duplicate.CAC_Ledger = config.CAC_Ledger;
			AssertHasRowError(duplicate, "Another record already sets Advance Payment Defaulting Configuration for the same Job parameters.");
		}
	}
}
