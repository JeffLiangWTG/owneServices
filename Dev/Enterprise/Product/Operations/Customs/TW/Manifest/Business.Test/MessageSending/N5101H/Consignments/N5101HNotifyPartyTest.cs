using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HNotifyPartyTest : TestCaseWithFactory
	{
		public void TestID()
		{
			bill.ABL_NotifyPartyRegNo = "ID";
			AssertEquals("ID", notifyParty.ID);

			bill.ABL_NotifyPartyRegNoType = "PAS";
			AssertEquals("NOID", notifyParty.ID);
		}

		public void TestName()
		{
			bill.ABL_NotifyPartyName = "Name";
			AssertEquals("Name", notifyParty.Name);
		}

		public void TestChineseName()
		{
			bill.ABL_NotifyPartyLocalName = "LocalName";
			AssertEquals("LocalName", notifyParty.ChineseName);
		}

		public void TestTypeCode()
		{
			CombineAssertions(() =>
			{
				bill.ABL_NotifyPartyRegNoType = "VAT";
				AssertEquals("58", notifyParty.TypeCode);
				bill.ABL_NotifyPartyRegNoType = "PAS";
				AssertEquals("53", notifyParty.TypeCode);
				bill.ABL_NotifyPartyRegNoType = "PID";
				AssertEquals("174", notifyParty.TypeCode);
				bill.ABL_NotifyPartyRegNoType = "Oth";
				AssertEquals(ZString.Empty, notifyParty.TypeCode);
			});
		}

		public void TestCustomsControlID()
		{
			AssertNullOrEmpty(notifyParty.CustomsControlID);
		}

		public void TestPaymentOnAccountBusinessID()
		{
			AssertNullOrEmpty(notifyParty.PaymentOnAccountBusinessID);
		}

		public void TestRoleCode()
		{
			AssertNullOrEmpty(notifyParty.RoleCode);
		}

		public void TestSubBoxID()
		{
			AssertNullOrEmpty(notifyParty.SubBoxID);
		}

		public void TestAddress()
		{
			AssertType<N5101HNotifyPartyAddress>(notifyParty.Address);
		}

		public void TestLPCOAuthorizedParty()
		{
			AssertNull(notifyParty.LPCOAuthorizedParty);
		}

		public void TestCommunications()
		{
			AssertNull(notifyParty.Communications);
		}

		public void TestContactName()
		{
			AssertNullOrEmpty(notifyParty.ContactName);
		}

		public void TestMainManufacturer()
		{
			AssertNullOrEmpty(notifyParty.MainManufacturer);
		}

		public void TestUndertakeCode()
		{
			AssertNullOrEmpty(notifyParty.UndertakeCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			bill = Factory.NewWithValidTestData<AsycudaBill>();
			notifyParty = new N5101HNotifyParty(bill);
		}

		AsycudaBill bill;
		N5101HNotifyParty notifyParty;
	}
}
