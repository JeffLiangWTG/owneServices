using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	sealed class AccPOSConfigurationViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestChargeType_IsMandatory()
		{
			PlaceOfSupplyConfig.PSC_ChargeType = ZString.Empty;
			AssertHasError(PlaceOfSupplyConfig.PSC_ChargeTypeInfo, "Please enter a Charge Type.");

			PlaceOfSupplyConfig.PSC_ChargeType = AccPOSChargeTypeList.Codes.Cost;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_ChargeTypeInfo);
		}

		public void TestChargeType_IsOnLookup()
		{
			PlaceOfSupplyConfig.PSC_ChargeType = "XYZ";
			AssertHasError(PlaceOfSupplyConfig.PSC_ChargeTypeInfo, "Enter a valid Charge Type.");

			PlaceOfSupplyConfig.PSC_ChargeType = AccPOSChargeTypeList.Codes.Revenue;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_ChargeTypeInfo);
		}

		public void TestLedger_CanBeEmpty()
		{
			PlaceOfSupplyConfig.PSC_Ledger = LedgerTypes.AccountsPayable;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_LedgerInfo);

			PlaceOfSupplyConfig.PSC_Ledger = ZString.Empty;
			AssertNoErrors("Empty Ledger = 'ALL' ChargeType", PlaceOfSupplyConfig.PSC_LedgerInfo);
		}

		public void TestParentTableCode_CanBeEmpty()
		{
			PlaceOfSupplyConfig.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			AssertNoErrors("Enter a valid value.", PlaceOfSupplyConfig.PSC_ParentTableCodeInfo);

			PlaceOfSupplyConfig.PSC_ParentTableCode = ZString.Empty;
			AssertNoErrors("Enter a valid value.", PlaceOfSupplyConfig.PSC_ParentTableCodeInfo);
		}

		public void TestJobType_IsMandatory()
		{
			PlaceOfSupplyConfig.PSC_JobType = ZString.Empty;
			AssertHasError(PlaceOfSupplyConfig.PSC_JobTypeInfo, "Please enter a Job Type.");

			PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_JobTypeInfo);
		}

		public void TestJobType_IsOnLookup()
		{
			PlaceOfSupplyConfig.PSC_JobType = "XYZ";
			AssertHasError(PlaceOfSupplyConfig.PSC_JobTypeInfo, "Enter a valid Job Type.");

			PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_JobTypeInfo);
		}

		public void TestIncoTerm_CanBeEmpty()
		{
			var jobTypesWithIncoterms = new[]
			{
				JobInvoicingConsumerTypes.ShipmentCode,
				JobInvoicingConsumerTypes.BrokerageCode,
				JobInvoicingConsumerTypes.QuotedBookingCode,
				JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All
			};

			foreach (var jobType in PlaceOfSupplyConfig.Lookups.JobTypeList.GetAllCodes())
			{
				PlaceOfSupplyConfig.PSC_JobType = jobType;

				PlaceOfSupplyConfig.PSC_IncoTerm = Constants.IncoTerms.FreeOnBoard;
				if (jobTypesWithIncoterms.Contains(jobType))
				{
					AssertNoErrors(PlaceOfSupplyConfig.PSC_IncoTermInfo);
				}
				else
				{
					AssertHasError($"Job Type: {jobType} - no Incoterms for Job Types other than expected", PlaceOfSupplyConfig.PSC_IncoTermInfo, "Enter a valid Incoterms.");
				}

				PlaceOfSupplyConfig.PSC_IncoTerm = ZString.Empty;
				AssertNoErrors(PlaceOfSupplyConfig.PSC_IncoTermInfo);
			}
		}

		public void TestIncoTerm_IsOnLookup()
		{
			PlaceOfSupplyConfig.PSC_JobType = new AllJobsConsumerType().Code;

			PlaceOfSupplyConfig.PSC_IncoTerm = Constants.IncoTerms.FreeCarrierSeller;
			AssertNoErrors("Incoterms are available for All Job Types", PlaceOfSupplyConfig.PSC_IncoTermInfo);

			PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;

			PlaceOfSupplyConfig.PSC_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			AssertHasError("No Incoterms for Forwarding Consol", PlaceOfSupplyConfig.PSC_IncoTermInfo, "Enter a valid Incoterms.");

			PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.ShipmentCode;

			PlaceOfSupplyConfig.PSC_IncoTerm = "XYZ";
			AssertHasError(PlaceOfSupplyConfig.PSC_IncoTermInfo, "Enter a valid Incoterms.");

			PlaceOfSupplyConfig.PSC_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_IncoTermInfo);
		}

		public void TestServiceDirection_IsMandatory()
		{
			PlaceOfSupplyConfig.PSC_ServiceDirection = ZString.Empty;
			AssertHasError(PlaceOfSupplyConfig.PSC_ServiceDirectionInfo, "Please enter a Service Direction.");

			PlaceOfSupplyConfig.PSC_ServiceDirection = Constants.FreightShipmentDirection.Code.Import;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_ServiceDirectionInfo);
		}

		public void TestServiceDirection_IsOnLookup()
		{
			PlaceOfSupplyConfig.PSC_ServiceDirection = "XYZ";
			AssertHasError(PlaceOfSupplyConfig.PSC_ServiceDirectionInfo, "Enter a valid Service Direction.");

			PlaceOfSupplyConfig.PSC_ServiceDirection = Constants.FreightShipmentDirection.Code.Import;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_ServiceDirectionInfo);
		}

		public void TestTransportMode_IsMandatory()
		{
			PlaceOfSupplyConfig.PSC_TransportMode = ZString.Empty;
			AssertHasError(PlaceOfSupplyConfig.PSC_TransportModeInfo, "Please enter a Transport Mode.");

			PlaceOfSupplyConfig.PSC_TransportMode = Constants.TransportModes.Sea;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_TransportModeInfo);
		}

		public void TestTransportMode_IsOnLookup()
		{
			PlaceOfSupplyConfig.PSC_TransportMode = "XYZ";
			AssertHasError(PlaceOfSupplyConfig.PSC_TransportModeInfo, "Enter a valid Transport Mode.");

			PlaceOfSupplyConfig.PSC_TransportMode = Constants.TransportModes.Sea;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_TransportModeInfo);
		}

		public void TestTaxRegistrationType_CanBeEmpty()
		{
			PlaceOfSupplyConfig.PSC_TaxRegistrationType = AccPOSTaxRegistrationList.Codes.ForeignOrganization;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_TaxRegistrationTypeInfo);

			PlaceOfSupplyConfig.PSC_TaxRegistrationType = ZString.Empty;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_TaxRegistrationTypeInfo);
		}

		public void TestTaxRegistrationType_IsOnLookup()
		{
			PlaceOfSupplyConfig.PSC_TaxRegistrationType = "XYZ";
			AssertHasError(PlaceOfSupplyConfig.PSC_TaxRegistrationTypeInfo, "Enter a valid Tax Registration Type.");

			PlaceOfSupplyConfig.PSC_TaxRegistrationType = AccPOSTaxRegistrationList.Codes.ForeignOrganization;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_TaxRegistrationTypeInfo);
		}

		public void TestBranch_IsOnLookup()
		{
			var currentBranch = Env.CurrentCompany.ActiveBranches.First();

			PlaceOfSupplyConfig.PSC_NK_Branch = "XYZ";
			AssertHasError(PlaceOfSupplyConfig.PSC_NK_BranchInfo, "Enter a valid Branch.");

			PlaceOfSupplyConfig.PSC_NK_Branch = currentBranch.Code;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_NK_BranchInfo);
		}

		public void TestBranch_CanBeEmpty()
		{
			var currentBranch = Env.CurrentCompany.ActiveBranches.First();
			PlaceOfSupplyConfig.PSC_NK_Branch = currentBranch.Code;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_NK_BranchInfo);

			PlaceOfSupplyConfig.PSC_NK_Branch = ZString.Empty;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_NK_BranchInfo);
		}

		public void TestSupplyType_CanBeEmpty()
		{
			PlaceOfSupplyConfig.PSC_SupplyType = SupplyTypeClassificationCodes.LOC;
			AssertNoErrors("Enter a valid value.", PlaceOfSupplyConfig.PSC_SupplyTypeInfo);

			PlaceOfSupplyConfig.PSC_SupplyType = ZString.Empty;
			AssertNoErrors("Enter a valid value.", PlaceOfSupplyConfig.PSC_SupplyTypeInfo);
		}

		public void TestSupplyType_IsOnLookup()
		{
			PlaceOfSupplyConfig.PSC_SupplyType = "XYZ";
			AssertHasError(PlaceOfSupplyConfig.PSC_SupplyTypeInfo, "Enter a valid Supply Type.");

			PlaceOfSupplyConfig.PSC_SupplyType = SupplyTypeClassificationCodes.LOC;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_SupplyTypeInfo);
		}

		public void TestPlaceOfSupplyRule_IsMandatory()
		{
			PlaceOfSupplyConfig.PSC_PlaceOfSupplyRule = ZString.Empty;
			AssertHasError(PlaceOfSupplyConfig.PSC_PlaceOfSupplyRuleInfo, "Please enter a Place Of Supply Rule.");

			PlaceOfSupplyConfig.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.SupplierLocation;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_PlaceOfSupplyRuleInfo);
		}

		public void TestPlaceOfSupplyRule_IsOnList()
		{
			PlaceOfSupplyConfig.PSC_PlaceOfSupplyRule = "XYZ";
			AssertHasError(PlaceOfSupplyConfig.PSC_PlaceOfSupplyRuleInfo, "Enter a valid Place Of Supply Rule.");

			PlaceOfSupplyConfig.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.SupplierLocation;
			AssertNoErrors(PlaceOfSupplyConfig.PSC_PlaceOfSupplyRuleInfo);
		}

		public void TestIsDuplicateInCollection()
		{
			var collection = new AccPOSConfigurationCollection(GlbCompany.CurrentCompany);
			collection.Add(PlaceOfSupplyConfig);

			var duplicate = Factory.NewWithValidTestData<AccPOSConfiguration>();
			collection.Add(duplicate);
			duplicate.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.SupplierLocation;

			duplicate.PSC_TaxRegistrationType = AccPOSTaxRegistrationList.Codes.ForeignOrganization;
			AssertNoRowError(duplicate, "Another record already sets POS Configuration for the same Job parameters.");
			duplicate.PSC_TaxRegistrationType = PlaceOfSupplyConfig.PSC_TaxRegistrationType;
			AssertHasRowError(duplicate, "Another record already sets POS Configuration for the same Job parameters.");

			duplicate.PSC_ServiceDirection = Constants.FreightShipmentDirection.Code.Domestic;
			AssertNoRowError(duplicate, "Another record already sets POS Configuration for the same Job parameters.");
			duplicate.PSC_ServiceDirection = PlaceOfSupplyConfig.PSC_ServiceDirection;
			AssertHasRowError(duplicate, "Another record already sets POS Configuration for the same Job parameters.");

			duplicate.PSC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertNoRowError(duplicate, "Another record already sets POS Configuration for the same Job parameters.");
			duplicate.PSC_JobType = PlaceOfSupplyConfig.PSC_JobType;
			AssertHasRowError(duplicate, "Another record already sets POS Configuration for the same Job parameters.");

			duplicate.PSC_NK_Branch = Env.CurrentBranch.Code;
			AssertNoRowError(duplicate, "Another record already sets POS Configuration for the same Job parameters.");
			duplicate.PSC_NK_Branch = PlaceOfSupplyConfig.PSC_NK_Branch;
			AssertHasRowError(duplicate, "Another record already sets POS Configuration for the same Job parameters.");

			duplicate.PSC_JobType = JobInvoicingConsumerTypes.QuotedBookingCode;
			AssertNoRowError(duplicate, "Another record already sets POS Configuration for the same Job parameters.");
			duplicate.PSC_JobType = PlaceOfSupplyConfig.PSC_JobType;
			AssertHasRowError(duplicate, "Another record already sets POS Configuration for the same Job parameters.");

			duplicate.PSC_JobType = JobInvoicingConsumerTypes.WorkItemCode;
			AssertNoRowError(duplicate, "Another record already sets POS Configuration for the same Job parameters.");
			duplicate.PSC_JobType = PlaceOfSupplyConfig.PSC_JobType;
			AssertHasRowError(duplicate, "Another record already sets POS Configuration for the same Job parameters.");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			PlaceOfSupplyConfig = Factory.NewWithValidTestData<AccPOSConfiguration>();
		}
		AccPOSConfiguration PlaceOfSupplyConfig;

		#endregion
	}
}
