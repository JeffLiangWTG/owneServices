using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HConsignorTest : TestCaseWithFactory
	{
		public void TestID()
		{
			bill.ABL_ShipperRegNo = "ID";
			AssertEquals("ID", consignor.ID);

			bill.ABL_ShipperRegNoType = "PAS";
			AssertEquals("NOID", consignor.ID);
		}

		public void TestName()
		{
			bill.ABL_ShipperName = "Name";
			AssertEquals("Name", consignor.Name);
		}

		public void TestChineseName()
		{
			bill.ABL_ShipperLocalName = "LocalName";
			AssertEquals("LocalName", consignor.ChineseName);
		}

		public void TestTypeCode()
		{
			CombineAssertions(() =>
			{
				bill.ABL_ShipperRegNoType = "VAT";
				AssertEquals("58", consignor.TypeCode);
				bill.ABL_ShipperRegNoType = "PAS";
				AssertEquals("53", consignor.TypeCode);
				bill.ABL_ShipperRegNoType = "PID";
				AssertEquals("174", consignor.TypeCode);
				bill.ABL_ShipperRegNoType = "Oth";
				AssertEquals(ZString.Empty, consignor.TypeCode);
			});
		}

		public void TestCustomsControlID()
		{
			AssertNullOrEmpty(consignor.CustomsControlID);
		}

		public void TestPaymentOnAccountBusinessID()
		{
			AssertNullOrEmpty(consignor.PaymentOnAccountBusinessID);
		}

		public void TestRoleCode()
		{
			AssertNullOrEmpty(consignor.RoleCode);
		}

		public void TestSubBoxID()
		{
			AssertNullOrEmpty(consignor.SubBoxID);
		}

		public void TestAddress()
		{
			AssertType<N5101HConsignorAddress>(consignor.Address);
		}

		public void TestLPCOAuthorizedParty()
		{
			AssertNull(consignor.LPCOAuthorizedParty);
		}

		public void TestCommunications()
		{
			AssertNull(consignor.Communications);
		}

		public void TestContactName()
		{
			AssertNullOrEmpty(consignor.ContactName);
		}

		public void TestMainManufacturer()
		{
			AssertNullOrEmpty(consignor.MainManufacturer);
		}

		public void TestUndertakeCode()
		{
			AssertNullOrEmpty(consignor.UndertakeCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			bill = Factory.NewWithValidTestData<AsycudaBill>();
			consignor = new N5101HConsignor(bill);
		}

		AsycudaBill bill;
		N5101HConsignor consignor;
	}
}
