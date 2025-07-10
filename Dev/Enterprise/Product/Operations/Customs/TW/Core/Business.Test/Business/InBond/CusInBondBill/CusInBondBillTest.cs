using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondBill))]
	sealed class CusInBondBillTest : EnterpriseBusinessObjectTestCase
	{
		public void TestVoyageFlightNo()
		{
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			cusInBondHeader.BH_UniqueVoyageIdentifier = "XX1";
			var moveHeader = cusInBondHeader.MovementHeader;
			moveHeader.BM_ConveyanceNumber = "XX2";
			var bill = cusInBondHeader.Bills.AddNew();
			bill.B0_ShipmentType = Constants.CusInBondBill.ShipmentType.Import;
			AssertEquals("XX1", bill.VoyageFlightNo);
			bill.B0_ShipmentType = Constants.CusInBondBill.ShipmentType.Export;
			AssertEquals("XX2", bill.VoyageFlightNo);
		}

		public void TestB0_ReferenceID()
		{
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			var bill = cusInBondHeader.Bills.AddNew();
			AssertEquals(4, bill.B0_ReferenceIDInfo.MaxLength);
		}

		public void TestIsAirForMasterBill()
		{
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			var arrivalBill = cusInBondHeader.ArrivalBill;
			var movementBill = cusInBondHeader.MovementBill;
			CombineAssertions(() =>
			{
				AssertEquals(false, arrivalBill.IsAirForMasterBill);
				AssertEquals(true, movementBill.IsAirForMasterBill);
			});

			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			CombineAssertions(() =>
			{
				AssertEquals(true, arrivalBill.IsAirForMasterBill);
				AssertEquals(true, movementBill.IsAirForMasterBill);
			});

			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			CombineAssertions(() =>
			{
				AssertEquals(false, arrivalBill.IsAirForMasterBill);
				AssertEquals(true, movementBill.IsAirForMasterBill);
			});

			cusInBondHeader.MovementHeader.BM_ExportLadenOn = "Test ExportLadenOn";
			CombineAssertions(() =>
			{
				AssertEquals(false, arrivalBill.IsAirForMasterBill);
				AssertEquals(false, movementBill.IsAirForMasterBill);
			});
		}

		public void TestB0_MasterBillNumber()
		{
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			var arrivalBill = cusInBondHeader.ArrivalBill;
			var movementBill = cusInBondHeader.MovementBill;
			CombineAssertions(() =>
			{
				arrivalBill.B0_MasterBillNumber = "111-11111111";
				movementBill.B0_MasterBillNumber = "111-11111111";
				AssertEquals("111-11111111", arrivalBill.B0_MasterBillNumber);
				AssertEquals("11111111111", movementBill.B0_MasterBillNumber);
			});

			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			CombineAssertions(() =>
			{
				arrivalBill.B0_MasterBillNumber = "111-11111111";
				movementBill.B0_MasterBillNumber = "111-11111111";
				AssertEquals("11111111111", arrivalBill.B0_MasterBillNumber);
				AssertEquals("11111111111", movementBill.B0_MasterBillNumber);
			});

			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			CombineAssertions(() =>
			{
				arrivalBill.B0_MasterBillNumber = "111-11111111";
				movementBill.B0_MasterBillNumber = "111-11111111";
				AssertEquals("111-11111111", arrivalBill.B0_MasterBillNumber);
				AssertEquals("11111111111", movementBill.B0_MasterBillNumber);
			});

			cusInBondHeader.MovementHeader.BM_ExportLadenOn = "Test ExportLadenOn";
			CombineAssertions(() =>
			{
				arrivalBill.B0_MasterBillNumber = "111-11111111";
				movementBill.B0_MasterBillNumber = "111-11111111";
				AssertEquals("111-11111111", arrivalBill.B0_MasterBillNumber);
				AssertEquals("111-11111111", movementBill.B0_MasterBillNumber);
			});
		}

		public void TestHeader()
		{
			AssertEquals(typeof(CusInBondHeader), CusInBondBill.Header.GetType());
		}

		public void TestB0_ManifestQty_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(CusInBondBill.B0_ManifestQtyInfo);
			AssertEquals("Caption", "Total Package Qty", resourceStringData.Caption);
		}

		public void TestValidation()
		{
			CusInBondBill.B0_ShipmentType = Constants.CusInBondBill.ShipmentType.Import;
			AssertEquals(typeof(ArrivalBillValidation), CusInBondBill.Validation.GetType());
			CusInBondBill.B0_ShipmentType = Constants.CusInBondBill.ShipmentType.Export;
			AssertEquals(typeof(MovementBillValidation), CusInBondBill.Validation.GetType());
		}

		public void TestLookups()
		{
			AssertEquals(typeof(CusInBondBillLookups), CusInBondBill.Lookups.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusInBondHeader = Factory.New<CusInBondHeader>();
			CusInBondBill = CusInBondHeader.Bills.AddNew();
		}

		public CusInBondHeader CusInBondHeader;
		public CusInBondBill CusInBondBill;
	}
}
