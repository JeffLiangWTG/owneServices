using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeCreditorOverrideLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobTypeList()
		{
			AssertEquals("The JobTypeList should contain 'ALL' as the first element", "ALL", Lookups.JobTypeList[0].Code);
			Assert("The JobType 'ALL' should be JobInvoicingConsumerType", Lookups.JobTypeList[0] is JobInvoicingConsumerType);
			AssertEquals("The JobTypeList should contain 'SHP'", true, Lookups.JobTypeList.ContainsCode("SHP"));
			AssertEquals("The JobTypeList should contain 'CLL'", true, Lookups.JobTypeList.ContainsCode("CLL"));
			AssertEquals("The JobTypeList should contain 'CSH'", true, Lookups.JobTypeList.ContainsCode("CSH"));
			AssertEquals("The JobTypeList should contain 'BRK'", true, Lookups.JobTypeList.ContainsCode("BRK"));
		}

		public void TestDirectionList()
		{
			AssertEquals("DirectionList.Count", 5, Lookups.DirectionList.Count);
			AssertEquals("The DirectionList should contain 'All' as the first element", "ALL", Lookups.DirectionList[0].Code);
			AssertEquals("The DirectionList should contain 'Import'", true, Lookups.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Import));
			AssertEquals("The DirectionList should contain 'Export'", true, Lookups.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Export));
			AssertEquals("The DirectionList should contain 'Domestic'", true, Lookups.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Domestic));
			AssertEquals("The DirectionList should contain 'Other'", true, Lookups.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Other));
		}

		public void TestTransportModeList()
		{
			AssertEquals("TransportModeList.Count", 8, Lookups.TransportModeList.Count);
			AssertEquals("The TransportModeList should contain 'ALL' as the first element", "ALL", Lookups.TransportModeList[0].Code);
			AssertEquals("The TransportModeList should contain 'AIR'", true, Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Air));
			AssertEquals("The TransportModeList should contain 'SEA'", true, Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Sea));
			AssertEquals("The TransportModeList should contain 'ROA'", true, Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Road));
			AssertEquals("The TransportModeList should contain 'RAI'", true, Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Rail));
		}

		public void TestDefaultingRuleList()
		{
			CreditorOverride.ACC_JobType = "";
			AssertEquals("DefaultingRuleList.Count", 1, Lookups.DefaultingRuleList.Count);
			AssertEquals("The DefaultingRuleList should contain 'SCA'", true, Lookups.DefaultingRuleList.ContainsCode(Constants.ChargeCreditorDefaultingRule.DefaultingRule));

			CreditorOverride.ACC_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			AssertEquals("DefaultingRuleList.Count", 1, Lookups.DefaultingRuleList.Count);
			AssertEquals("The DefaultingRuleList should contain 'SCA'", true, Lookups.DefaultingRuleList.ContainsCode(Constants.ChargeCreditorDefaultingRule.DefaultingRule));

			CreditorOverride.ACC_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			AssertEquals("DefaultingRuleList.Count", 1, Lookups.DefaultingRuleList.Count);
			AssertEquals("The DefaultingRuleList should contain 'SCA'", true, Lookups.DefaultingRuleList.ContainsCode(Constants.ChargeCreditorDefaultingRule.DefaultingRule));

			CreditorOverride.ACC_JobType = "DMY";
			AssertEquals("DefaultingRuleList.Count", 1, Lookups.DefaultingRuleList.Count);
			AssertEquals("The DefaultingRuleList should contain 'SCA'", true, Lookups.DefaultingRuleList.ContainsCode(Constants.ChargeCreditorDefaultingRule.DefaultingRule));
		}

		public void TestCreditorRoleList()
		{
			AssertContainsExactElementsInAnyOrder
			(
				new[] { DocAddressTypes.Codes.OverseasAgent },
				Lookups.CreditorRoleList.GetAllCodes()
			);
		}

		public void TestPaymentTermList()
		{
			CombineAssertions("PaymentTermList should be ALL, CCX, PPD", () =>
			{
				AssertEquals
				(
					"First element",
					$"{AccChargeCreditorOverrideLookups.PaymentTermAdditionalCodes.All} (Any Payment Term)",
					$"{Lookups.PaymentTermList[0].Code} ({Lookups.PaymentTermList[0].Description})"
				);

				AssertContainsExactElementsInAnyOrder
				(
					"PaymentTermList",
					new[] { AccChargeCreditorOverrideLookups.PaymentTermAdditionalCodes.All, Constants.PaymentType.Collect, Constants.PaymentType.Prepaid },
					Lookups.PaymentTermList.GetAllCodes()
				);
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany chargeCompany = Factory.NewWithValidTestData<GlbCompany>();
			AccChargeCode testChargeCode = Factory.New<AccChargeCode>();
			testChargeCode.AC_GC = chargeCompany.PK;
			CreditorOverride = testChargeCode.CreditorOverrides.AddNew();
			Lookups = CreditorOverride.Lookups;
		}
		AccChargeCreditorOverrideLookups Lookups;
		AccChargeCreditorOverride CreditorOverride;

		#endregion
	}
}
