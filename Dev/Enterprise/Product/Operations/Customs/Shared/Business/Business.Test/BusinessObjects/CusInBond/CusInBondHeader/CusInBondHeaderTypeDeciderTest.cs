using System;
using System.Data;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInBondHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			AssertNull(typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertNull(typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForLoad_ErrorReporter()
		{
			var header = Factory.New<DummyCusInBondHeader>();
			header.BH_ApplicationCode = "Z!";
			CombineAssertions(() =>
			{
				AssertNull("Invalid Type", typeDecider.GetTypeForLoad(((INeedRow)header).Row, Factory));
				AssertEquals("ErrorReporter Key", "CusInBondHeader for Application Code 'Z!' is unknown", ErrorReporter.LastKeyReported);
				AssertEquals("ErrorReporter Message", "Cannot determine the CusInBondHeader object for Application Code 'Z!'", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}

		public void TestGetTypeForLoad_AMS()
		{
			var header = Factory.New<DummyCusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.AMS;
			AssertEquals(ObjectFactory.GetType<Integration.Customs.US.USAMS.ICusInBondHeader>(), typeDecider.GetTypeForLoad(((INeedRow)header).Row, Factory));
		}

		public void TestGetTypeForLoad_eManifest()
		{
			var header = Factory.New<DummyCusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			AssertEquals(ObjectFactory.GetType<Integration.Customs.US.eManifest.ICusInBondHeader>(), typeDecider.GetTypeForLoad(((INeedRow)header).Row, Factory));
		}

		public void TestGetTypeForLoad_InBond()
		{
			var header = Factory.New<DummyCusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			AssertEquals(ObjectFactory.GetType<Integration.Customs.US.InBond.ICusInBondHeader>(), typeDecider.GetTypeForLoad(((INeedRow)header).Row, Factory));
		}

		public void TestGetTypeForLoad_NCTS4()
		{
			var header = Factory.New<DummyCusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals(ObjectFactory.GetType<Integration.Customs.EU.NCTS.ICusInBondHeader>(), typeDecider.GetTypeForLoad(((INeedRow)header).Row, Factory));
		}

		public void TestGetTypeForLoad_NCTS5()
		{
			var header = Factory.New<DummyCusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals(ObjectFactory.GetType<Integration.Customs.EU.NCTS.ICusInBondHeader>(), typeDecider.GetTypeForLoad(((INeedRow)header).Row, Factory));
		}

		public void TestGetTypeForLoad_TRSPTS()
		{
			var header = Factory.New<DummyCusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.TRSPTS;
			AssertEquals(ObjectFactory.GetType<Integration.Customs.TR.ICusInBondSPTSHeader>(), typeDecider.GetTypeForLoad(((INeedRow)header).Row, Factory));
		}

		public void TestGetTypeForLoad_TWTranshipment()
		{
			var header = Factory.New<DummyCusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.TWTranshipment;
			AssertEquals(ObjectFactory.GetType<Integration.Customs.TW.ICusInBondHeader>(), typeDecider.GetTypeForLoad(((INeedRow)header).Row, Factory));
		}

		protected override void SetUp()
		{
			base.SetUp();
			typeDecider = new CusInBondHeaderTypeDecider();
		}
		CusInBondHeaderTypeDecider typeDecider;
	}

	class DummyCusInBondHeader : CusInBondHeader
	{
		public DummyCusInBondHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type BillTypeCore => typeof(DummyCusInBondBill);

		protected override Type MovementHeaderTypeCore => throw new NotImplementedException();

		protected override CusInBondMoveHeaderCollection GetMovementHeaders() => throw new NotImplementedException();

		protected override ICusInBondBillCollection GetNewBillsCollection() => new DummyCusInBondBillCollection(this);
	}

	class DummyCusInBondBill : CusInBondBill
	{
		public DummyCusInBondBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type MovementDetailType => null;
	}

	class DummyCusInBondBillCollection : CusInBondBillCollection<DummyCusInBondBill>
	{
		public DummyCusInBondBillCollection(CusInBondHeader master)
			: base(master)
		{
		}
	}
}
