using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HConsigneeTest : TestCaseWithFactory
	{
		public void TestID()
		{
			bill.ABL_ConsigneeRegNo = "ID";
			AssertEquals("ID", consignee.ID);

			bill.ABL_ConsigneeRegNoType = "PAS";
			AssertEquals("NOID", consignee.ID);
		}

		public void TestName()
		{
			bill.ABL_ConsigneeName = "Name";
			AssertEquals("Name", consignee.Name);
		}

		public void TestChineseName()
		{
			bill.ABL_ConsigneeLocalName = "LocalName";
			AssertEquals("LocalName", consignee.ChineseName);
		}

		public void TestTypeCode()
		{
			CombineAssertions(() =>
			{
				bill.ABL_ConsigneeRegNoType = "VAT";
				AssertEquals("58", consignee.TypeCode);
				bill.ABL_ConsigneeRegNoType = "PAS";
				AssertEquals("53", consignee.TypeCode);
				bill.ABL_ConsigneeRegNoType = "PID";
				AssertEquals("174", consignee.TypeCode);
				bill.ABL_ConsigneeRegNoType = "Oth";
				AssertEquals(ZString.Empty, consignee.TypeCode);
			});
		}

		public void TestCustomsControlID()
		{
			AssertNullOrEmpty(consignee.CustomsControlID);
		}

		public void TestPaymentOnAccountBusinessID()
		{
			AssertNullOrEmpty(consignee.PaymentOnAccountBusinessID);
		}

		public void TestRoleCode()
		{
			AssertNullOrEmpty(consignee.RoleCode);
		}

		public void TestSubBoxID()
		{
			AssertNullOrEmpty(consignee.SubBoxID);
		}

		public void TestAddress()
		{
			AssertType<N5101HConsigneeAddress>(consignee.Address);
		}

		public void TestLPCOAuthorizedParty()
		{
			AssertNull(consignee.LPCOAuthorizedParty);
		}

		public void TestCommunications()
		{
			AssertNull(consignee.Communications);
		}

		public void TestContactName()
		{
			AssertNullOrEmpty(consignee.ContactName);
		}

		public void TestMainManufacturer()
		{
			AssertNullOrEmpty(consignee.MainManufacturer);
		}

		public void TestUndertakeCode()
		{
			AssertNullOrEmpty(consignee.UndertakeCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			bill = Factory.NewWithValidTestData<AsycudaBill>();
			consignee = new N5101HConsignee(bill);
		}

		AsycudaBill bill;
		N5101HConsignee consignee;
	}
}
