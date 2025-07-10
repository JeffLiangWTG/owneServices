using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeGovtChargeCodeOverrideValidationTest : BusinessObjectValidationTestCase
	{
		public void TestACG_JobType()
		{
			govtChargeCodeOverride.ACG_JobType = "";
			govtChargeCodeOverride.Validation.ValidateACG_JobType();
			AssertHasErrors(govtChargeCodeOverride.ACG_JobTypeInfo);

			foreach (var jobType in govtChargeCodeOverride.Lookups.JobTypes.Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				govtChargeCodeOverride.ACG_JobType = jobType;
				govtChargeCodeOverride.Validation.ValidateACG_JobType();
				AssertNoErrors(govtChargeCodeOverride.ACG_JobTypeInfo);
			}

			govtChargeCodeOverride.ACG_JobType = "ZZZ";
			govtChargeCodeOverride.Validation.ValidateACG_JobType();
			AssertHasErrors(govtChargeCodeOverride.ACG_JobTypeInfo);
		}

		public void TestACG_CostSellAll()
		{
			govtChargeCodeOverride.ACG_CostSellAll = ZString.Empty;
			govtChargeCodeOverride.Validation.ValidateACG_CostSellAll();
			AssertHasError(govtChargeCodeOverride.ACG_CostSellAllInfo, "Please enter a Cost/Sell.");

			foreach (var costSellAll in govtChargeCodeOverride.Lookups.CostSellList.GetAllCodes())
			{
				govtChargeCodeOverride.ACG_CostSellAll = costSellAll;
				govtChargeCodeOverride.Validation.ValidateACG_CostSellAll();
				AssertNoErrors(govtChargeCodeOverride.ACG_CostSellAllInfo);
			}

			govtChargeCodeOverride.ACG_CostSellAll = "ZZZ";
			govtChargeCodeOverride.Validation.ValidateACG_CostSellAll();
			AssertHasError(govtChargeCodeOverride.ACG_CostSellAllInfo, "Enter a valid Cost/Sell.");
		}

		public void TestACG_ServiceDirection()
		{
			foreach (var jobType in govtChargeCodeOverride.Lookups.JobTypes.Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				govtChargeCodeOverride.ACG_JobType = jobType;

				if (!govtChargeCodeOverride.ACG_DirectionInfo.ReadOnly)
				{
					govtChargeCodeOverride.ACG_Direction = "";
					AssertEquals("ALL", govtChargeCodeOverride.ACG_Direction);
					govtChargeCodeOverride.Validation.ValidateACG_Direction();
					AssertNoErrors(govtChargeCodeOverride.ACG_DirectionInfo);
				}

				foreach (var direction in govtChargeCodeOverride.Lookups.DirectionList.Cast<CodeDescriptionPair>().Select(x => x.Code))
				{
					govtChargeCodeOverride.ACG_Direction = direction;

					govtChargeCodeOverride.Validation.ValidateACG_Direction();
					AssertNoErrors(govtChargeCodeOverride.ACG_DirectionInfo);
				}

				if (!govtChargeCodeOverride.ACG_DirectionInfo.ReadOnly)
				{
					govtChargeCodeOverride.ACG_Direction = "ZZZ";
					govtChargeCodeOverride.Validation.ValidateACG_Direction();
					AssertHasErrors(govtChargeCodeOverride.ACG_DirectionInfo);
				}
			}
		}

		public void TestACG_TransportMode()
		{
			foreach (var jobType in govtChargeCodeOverride.Lookups.JobTypes.Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				govtChargeCodeOverride.ACG_JobType = jobType;

				if (!govtChargeCodeOverride.ACG_TransportModeInfo.ReadOnly)
				{
					govtChargeCodeOverride.ACG_TransportMode = "";
					AssertEquals("ALL", govtChargeCodeOverride.ACG_TransportMode);
					govtChargeCodeOverride.Validation.ValidateACG_TransportMode();
					AssertNoErrors(govtChargeCodeOverride.ACG_TransportModeInfo);
				}

				foreach (var mode in govtChargeCodeOverride.Lookups.TransportModeList.Cast<CodeDescriptionPair>().Select(x => x.Code))
				{
					govtChargeCodeOverride.ACG_TransportMode = mode;

					govtChargeCodeOverride.Validation.ValidateACG_TransportMode();
					AssertNoErrors(govtChargeCodeOverride.ACG_TransportModeInfo);
				}

				if (!govtChargeCodeOverride.ACG_TransportModeInfo.ReadOnly)
				{
					govtChargeCodeOverride.ACG_TransportMode = "ZZZ";
					govtChargeCodeOverride.Validation.ValidateACG_TransportMode();
					AssertHasErrors(govtChargeCodeOverride.ACG_TransportModeInfo);
				}
			}
		}

		public void TestACG_GovtChargeCode()
		{
			govtChargeCodeOverride.ACG_GovtChargeCode = "";
			govtChargeCodeOverride.Validation.ValidateACG_GovtChargeCode();
			AssertHasErrors(govtChargeCodeOverride.ACG_GovtChargeCodeInfo);

			govtChargeCodeOverride.ACG_GovtChargeCode = "123";
			govtChargeCodeOverride.Validation.ValidateACG_GovtChargeCode();
			AssertNoErrors(govtChargeCodeOverride.ACG_GovtChargeCodeInfo);

			govtChargeCodeOverride.ACG_GovtChargeCode = "01234567890123456789";
			govtChargeCodeOverride.Validation.ValidateACG_GovtChargeCode();
			AssertNoErrors(govtChargeCodeOverride.ACG_GovtChargeCodeInfo);

			var overLengthExp = AssertExceptionThrown<MaxLengthExceededException>(() => govtChargeCodeOverride.ACG_GovtChargeCode = "123456789012345678901234");
			AssertContains("The maximum length of this property is 23 characters, but 24 were entered.", overLengthExp.Message);
			ErrorReporter.Clear();
		}

		public void TestCheckForDuplicates()
		{
			var govtChargeCodeConfigCollection = new AccChargeGovtChargeCodeOverrideCollection(Factory.NewWithValidTestData<AccChargeCode>());

			var govtChargeCodeOverride = govtChargeCodeConfigCollection.AddNew();
			govtChargeCodeOverride.ACG_CostSellAll = AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All;
			govtChargeCodeOverride.ACG_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			govtChargeCodeOverride.ACG_Direction = OrgConstants.ServiceDirection.Code.Export;
			govtChargeCodeOverride.ACG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			govtChargeCodeOverride.ACG_GovtChargeCode = "1234";

			govtChargeCodeOverride.Validation.ValidateAll();
			AssertNoErrors(govtChargeCodeOverride);

			var govtChargeCodeOverrideDup = govtChargeCodeConfigCollection.AddNew();
			govtChargeCodeOverrideDup.ACG_CostSellAll = AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All;
			govtChargeCodeOverrideDup.ACG_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			govtChargeCodeOverrideDup.ACG_Direction = OrgConstants.ServiceDirection.Code.Import;
			govtChargeCodeOverrideDup.ACG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			govtChargeCodeOverrideDup.ACG_GovtChargeCode = "1234";

			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertNoRowErrors(govtChargeCodeOverrideDup);

			govtChargeCodeOverrideDup.ACG_Direction = OrgConstants.ServiceDirection.Code.Export;
			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertHasRowError(govtChargeCodeOverrideDup, AccChargeGovtChargeCodeOverrideValidation.IsDuplicateErrorString);

			govtChargeCodeOverrideDup.ACG_CostSellAll = AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Cost;
			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertNoRowErrors(govtChargeCodeOverrideDup);

			govtChargeCodeOverrideDup.ACG_CostSellAll = AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All;
			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertHasRowError(govtChargeCodeOverrideDup, AccChargeGovtChargeCodeOverrideValidation.IsDuplicateErrorString);

			govtChargeCodeOverrideDup.ACG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertNoRowErrors(govtChargeCodeOverrideDup);

			govtChargeCodeOverrideDup.ACG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertHasRowError(govtChargeCodeOverrideDup, AccChargeGovtChargeCodeOverrideValidation.IsDuplicateErrorString);

			govtChargeCodeOverrideDup.ACG_JobType = JobInvoicingConsumerTypes.TransportBookingCode;
			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertNoRowErrors(govtChargeCodeOverrideDup);

			govtChargeCodeOverrideDup.ACG_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertHasRowError(govtChargeCodeOverrideDup, AccChargeGovtChargeCodeOverrideValidation.IsDuplicateErrorString);

			govtChargeCodeOverride.ACG_JobType = JobInvoicingConsumerTypes.BrokerageCode;
			govtChargeCodeOverrideDup.ACG_JobType = JobInvoicingConsumerTypes.BrokerageCode;
			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertHasRowError(govtChargeCodeOverrideDup, AccChargeGovtChargeCodeOverrideValidation.IsDuplicateErrorString);

			govtChargeCodeOverrideDup.ACG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertNoRowErrors(govtChargeCodeOverrideDup);

			govtChargeCodeOverride.ACG_JobType = JobInvoicingConsumerTypes.QuotedBookingCode;
			govtChargeCodeOverride.ACG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			govtChargeCodeOverrideDup.ACG_JobType = JobInvoicingConsumerTypes.QuotedBookingCode;
			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertHasRowError(govtChargeCodeOverrideDup, AccChargeGovtChargeCodeOverrideValidation.IsDuplicateErrorString);

			govtChargeCodeOverrideDup.ACG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertNoRowErrors(govtChargeCodeOverrideDup);

			govtChargeCodeOverride.ACG_JobType = JobInvoicingConsumerTypes.OneOffQuotationCode;
			govtChargeCodeOverrideDup.ACG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			govtChargeCodeOverrideDup.ACG_JobType = JobInvoicingConsumerTypes.OneOffQuotationCode;
			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertHasRowError(govtChargeCodeOverrideDup, AccChargeGovtChargeCodeOverrideValidation.IsDuplicateErrorString);

			govtChargeCodeOverrideDup.ACG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Rail;
			govtChargeCodeOverrideDup.Validation.ValidateAll();
			AssertNoRowErrors(govtChargeCodeOverrideDup);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			govtChargeCodeOverride = Factory.New<AccChargeGovtChargeCodeOverride>();
		}
		AccChargeGovtChargeCodeOverride govtChargeCodeOverride;

		#endregion
	}
}
