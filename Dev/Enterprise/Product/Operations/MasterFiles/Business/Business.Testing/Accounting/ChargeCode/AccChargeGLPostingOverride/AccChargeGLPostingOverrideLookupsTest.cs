using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeGLPostingOverrideLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobTypeList()
		{
			AssertEquals("The JobTypeList should contain 'ALL' as the first element", "ALL", Lookups.JobTypeList[0].Code);
			Assert("The JobType 'ALL' should be JobInvoicingConsumerType", Lookups.JobTypeList[0] is JobInvoicingConsumerType);
			var jobTypeListCount = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes().Count + 1;

			AssertEquals(jobTypeListCount, Lookups.JobTypeList.Count);
		}

		public void TestDirectionList()
		{
			AssertEquals("DirectionList.Count", 5, Lookups.DirectionList.Count);
			AssertEquals("The DirectionList should contain 'ALL' as the first element", "ALL", Lookups.DirectionList[0].Code);
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
			AssertEquals("The TransportModeList should contain 'FSA'", true, Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.SeaAir));
			AssertEquals("The TransportModeList should contain 'FAS'", true, Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.AirSea));
			AssertEquals("The TransportModeList should contain 'ROA'", true, Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Road));
			AssertEquals("The TransportModeList should contain 'RAI'", true, Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Rail));
			AssertEquals("The TransportModeList should contain 'COU'", true, Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Courier));
		}

		public void TestConsolContainerModeList()
		{
			AssertEquals("ConsolContainerModeList.Count", 15, Lookups.ConsolContainerModeList.Count);
			AssertEquals("The ConsolContainerModeList should contain 'ALL' as the first element", "ALL", Lookups.ConsolContainerModeList[0].Code);
			AssertEquals("The ConsolContainerModeList should contain 'LCL'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.LCL));
			AssertEquals("The ConsolContainerModeList should contain 'FCL'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.FCL));
			AssertEquals("The ConsolContainerModeList should contain 'GRP'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.Groupage));
			AssertEquals("The ConsolContainerModeList should contain 'BCN'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.BuyersConsol));
			AssertEquals("The ConsolContainerModeList should contain 'SCN'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.ShippersConsol));
			AssertEquals("The ConsolContainerModeList should contain 'LSE'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.Loose));
			AssertEquals("The ConsolContainerModeList should contain 'ULD'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.ULD));
			AssertEquals("The ConsolContainerModeList should contain 'BBK'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.BreakBulk));
			AssertEquals("The ConsolContainerModeList should contain 'BLK'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.Bulk));
			AssertEquals("The ConsolContainerModeList should contain 'LQD'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.Liquid));
			AssertEquals("The ConsolContainerModeList should contain 'ROR'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.RollOnRollOff));
			AssertEquals("The ConsolContainerModeList should contain 'LTL'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.LTL));
			AssertEquals("The ConsolContainerModeList should contain 'FTL'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.FTL));
			AssertEquals("The ConsolContainerModeList should contain 'OTH'", true, Lookups.ConsolContainerModeList.ContainsCode(Constants.ContainerModes.Other));
		}

		public void TestMasterPaymentTypeList()
		{
			AssertEquals("MasterPaymentTypeList.Count", 3, Lookups.MasterPaymentTypeList.Count);
			AssertEquals("The MasterPaymentTypeList should contain 'ALL' as the first element", "ALL", Lookups.MasterPaymentTypeList[0].Code);
			AssertEquals("The MasterPaymentTypeList should contain 'PPD'", true, Lookups.MasterPaymentTypeList.ContainsCode(Constants.PaymentType.Prepaid));
			AssertEquals("The MasterPaymentTypeList should contain 'CCX'", true, Lookups.MasterPaymentTypeList.ContainsCode(Constants.PaymentType.Collect));
		}

		public void TestHousePaymentTypeList()
		{
			AssertEquals("HousePaymentTypeList.Count", 3, Lookups.HousePaymentTypeList.Count);
			AssertEquals("The HousePaymentTypeList should contain 'ALL' as the first element", "ALL", Lookups.HousePaymentTypeList[0].Code);
			AssertEquals("The HousePaymentTypeList should contain 'PPD'", true, Lookups.HousePaymentTypeList.ContainsCode(Constants.PaymentType.Prepaid));
			AssertEquals("The HousePaymentTypeList should contain 'CCX'", true, Lookups.HousePaymentTypeList.ContainsCode(Constants.PaymentType.Collect));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var chargeCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testChargeCode = Factory.New<AccChargeCode>();
			testChargeCode.AC_GC = chargeCompany.PK;
			ChargeGLPostingOverride = testChargeCode.GLPostingOverrides.AddNew();
			Lookups = ChargeGLPostingOverride.Lookups;
		}
		AccChargeGLPostingOverrideLookups Lookups;
		AccChargeGLPostingOverride ChargeGLPostingOverride;

		#endregion
	}
}
