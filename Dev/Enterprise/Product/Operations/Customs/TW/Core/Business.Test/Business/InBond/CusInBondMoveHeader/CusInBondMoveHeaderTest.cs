using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeader))]
	sealed class CusInBondMoveHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsSeaMovement()
		{
			moveHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.SeaPackedSundryGoods;
			Assert(moveHeader.IsSeaMovement);
			moveHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.SeaContainer;
			Assert(moveHeader.IsSeaMovement);
			moveHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.SeaBulkGoods;
			Assert(moveHeader.IsSeaMovement);
			moveHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.SeaPassengerOrCREW;
			Assert(moveHeader.IsSeaMovement);
			moveHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.AirNotExpressDelivery;
			Assert(!moveHeader.IsSeaMovement);
			moveHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.AirPassengerOrCREW;
			Assert(!moveHeader.IsSeaMovement);
			moveHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.Other;
			Assert(!moveHeader.IsSeaMovement);
		}

		public void TestDestinationVisible()
		{
			moveHeader.BM_InBondEntryType = EntryTypeList.Codes.T1;
			Assert(moveHeader.DestinationVisible);
			moveHeader.BM_InBondEntryType = EntryTypeList.Codes.T2;
			Assert(!moveHeader.DestinationVisible);
			moveHeader.BM_InBondEntryType = EntryTypeList.Codes.T4;
			Assert(!moveHeader.DestinationVisible);
			moveHeader.BM_InBondEntryType = EntryTypeList.Codes.T5;
			Assert(!moveHeader.DestinationVisible);
			moveHeader.BM_InBondEntryType = EntryTypeList.Codes.T6;
			Assert(!moveHeader.DestinationVisible);
		}

		public void TestDestinationUNVisible()
		{
			moveHeader.BM_InBondEntryType = EntryTypeList.Codes.T1;
			Assert(!moveHeader.DestinationUNVisible);
			moveHeader.BM_InBondEntryType = EntryTypeList.Codes.T2;
			Assert(moveHeader.DestinationUNVisible);
			moveHeader.BM_InBondEntryType = EntryTypeList.Codes.T4;
			Assert(moveHeader.DestinationUNVisible);
			moveHeader.BM_InBondEntryType = EntryTypeList.Codes.T5;
			Assert(moveHeader.DestinationUNVisible);
			moveHeader.BM_InBondEntryType = EntryTypeList.Codes.T6;
			Assert(moveHeader.DestinationUNVisible);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CusInBondMoveHeaderValidation), moveHeader.Validation.GetType());
		}

		public void TestLookups()
		{
			AssertEquals(typeof(CusInBondMoveHeaderLookups), moveHeader.Lookups.GetType());
		}

		public void TestInBondMoveDetail()
		{
			var movementBill = header.MovementBill;
			var inBondMoveDetail = moveHeader.InBondMoveDetail;
			AssertSame(inBondMoveDetail, moveHeader.MovementDetails.FirstOrDefault());
			AssertEquals(movementBill.PK, inBondMoveDetail.B9_B0);
		}

		public void TestVessel()
		{
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Code = "VESSEL 1";
			refVessel.RV_LloydsNumber = "abcdef";
			refVessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;
			moveHeader.BM_ExportLadenOn = "VESSEL 1";
			var vessel = moveHeader.Vessel;
			AssertEquals(refVessel.PK, vessel.PK);
			AssertEquals("abcdef", vessel.RV_LloydsNumber);
			AssertEquals(Core.Constants.CountryCodes.Taiwan, vessel.RV_RN_NKCountryOfReg);
		}

		public void TestBM_ExportLadenOnDescription()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Code = "VESSEL 1";
			vessel1.RV_LloydsNumber = "8811928";
			vessel1.RV_RadioCallSign = "8811927";
			vessel1.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Code = "VESSEL 2";
			vessel2.RV_RadioCallSign = "8811924";
			vessel2.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;

			var vessel3 = Factory.New<RefVessel>();
			vessel3.RV_Code = "VESSEL 3";
			vessel3.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;

			moveHeader.BM_ExportLadenOn = "VESSEL 1";
			AssertEquals("8811928", moveHeader.BM_ExportLadenOnDescription);

			moveHeader.BM_ExportLadenOn = "VESSEL 2";
			AssertEquals("8811924", moveHeader.BM_ExportLadenOnDescription);

			moveHeader.BM_ExportLadenOn = "VESSEL 3";
			AssertEquals("NIL", moveHeader.BM_ExportLadenOnDescription);

			moveHeader.BM_ExportLadenOn = ZString.Empty;
			AssertEquals(ZString.Empty, moveHeader.BM_ExportLadenOnDescription);
		}

		public void TestBM_InBondEntryType()
		{
			moveHeader.BM_InBondEntryType = ZString.Empty;
			moveHeader.BM_RL_NKForeignDestPort = "AA";
			moveHeader.BM_ForeignDestPortKCode = "BB";
			AssertEquals("AA", moveHeader.BM_RL_NKForeignDestPort);
			AssertEquals("BB", moveHeader.BM_ForeignDestPortKCode);
			moveHeader.BM_InBondEntryType = "A";
			AssertNullOrEmpty(moveHeader.BM_RL_NKForeignDestPort);
			AssertNullOrEmpty(moveHeader.BM_ForeignDestPortKCode);
			moveHeader.BM_RL_NKForeignDestPort = "AA";
			moveHeader.BM_ForeignDestPortKCode = "BB";
			AssertEquals("AA", moveHeader.BM_RL_NKForeignDestPort);
			AssertEquals("BB", moveHeader.BM_ForeignDestPortKCode);
			moveHeader.BM_InBondEntryType = ZString.Empty;
			AssertNullOrEmpty(moveHeader.BM_RL_NKForeignDestPort);
			AssertNullOrEmpty(moveHeader.BM_ForeignDestPortKCode);
		}

		public void TestBM_ConveyanceNumber_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(moveHeader.BM_ConveyanceNumberInfo);
			AssertEquals("Caption", "Flight No/Voyage", resourceStringData.Caption);
		}

		#region Implementation
		CusInBondHeader header;
		CusInBondMoveHeader moveHeader;

		protected override void SetUp()
		{
			base.SetUp();
			header = CreateNewCusInBondHeader();
			moveHeader = CreateNewCusInBondMoveHeader(header);
		}

		CusInBondMoveHeader CreateNewCusInBondMoveHeader(Customs.Business.CusInBondHeader header)
		{
			var moveHeader = ((CusInBondHeader)header).MovementHeaders.AddNew();
			moveHeader.BM_BH = this.header.PK;
			return moveHeader;
		}

		CusInBondHeader CreateNewCusInBondHeader()
		{
			return Factory.New<CusInBondHeader>();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = CreateNewCusInBondHeader();
			return CreateNewCusInBondMoveHeader(header);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			header = CreateNewCusInBondHeader();
			return CreateNewCusInBondMoveHeader(header);
		}
		#endregion
	}
}
