using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.Common.NZ;
	using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing;
	using NUnit.Framework;

	class ConsolCustomsChargesTest : TestCaseWithFactory
	{
		[TestDate(2021, 7, 1, 5, 0, 0)]
		public void TestEntryFeeGetsRightGSTAfter2021_07_01()
		{
			CombineAssertions(() =>
			{
				TaxOrFeeTestHelper.SetUp(Factory);

				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				Registry.EntryChargeTypeList list = new Registry.EntryChargeTypeList();
				ICustomsCharges customsCharges = new ConsolCustomsCharges(consol);
				CustomsCharge[] charges = customsCharges.GetCustomsCharges(null);
				AssertEquals(1, charges.Length);
				var chargeType = list[Registry.EntryChargeTypeList.Codes.EntryFee];
				AssertEquals(chargeType.Description, charges[0].Description);
				AssertEquals(16.16m, charges[0].Amount);
				AssertEquals(2.42m, charges[0].GST);

				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				charges = customsCharges.GetCustomsCharges(null);
				AssertEquals(1, charges.Length);
				chargeType = list[Registry.EntryChargeTypeList.Codes.EntryFee];
				AssertEquals(chargeType.Description, charges[0].Description);
				AssertEquals(12.48m, charges[0].Amount);
				AssertEquals(1.87m, charges[0].GST);
			});
		}

		[TestDate(2021, 6, 30, 23, 0, 0)]
		public void TestEntryFeeGetsRightGSTBefore2021_07_01()
		{
			CombineAssertions(() =>
			{
				TaxOrFeeTestHelper.SetUp(Factory);

				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				Registry.EntryChargeTypeList list = new Registry.EntryChargeTypeList();
				ICustomsCharges customsCharges = new ConsolCustomsCharges(consol);
				CustomsCharge[] charges = customsCharges.GetCustomsCharges(null);
				AssertEquals(1, charges.Length);
				var chargeType = list[Registry.EntryChargeTypeList.Codes.EntryFee];
				AssertEquals(chargeType.Description, charges[0].Description);
				AssertEquals(25.07m, charges[0].Amount);
				AssertEquals(3.76m, charges[0].GST);

				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				charges = customsCharges.GetCustomsCharges(null);
				AssertEquals(1, charges.Length);
				chargeType = list[Registry.EntryChargeTypeList.Codes.EntryFee];
				AssertEquals(chargeType.Description, charges[0].Description);
				AssertEquals(10.01m, charges[0].Amount);
				AssertEquals(1.50m, charges[0].GST);
			});
		}

		public void TestIsActive()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ICustomsCharges customsCharges = new ConsolCustomsCharges(consol);
			AssertEquals(false, customsCharges.IsActive);

			Common.CusEntryNumber entryNumber = consol.Numbers.AddNew();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = "123456789";
			entryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.Cleared;

			AssertEquals(true, customsCharges.IsActive);
		}
	}
}
