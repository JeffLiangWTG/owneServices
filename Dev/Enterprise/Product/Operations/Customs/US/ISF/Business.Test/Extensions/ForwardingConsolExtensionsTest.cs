using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ForwardingConsolExtensionsTest : TestCaseWithFactory
	{
		public void TestUpperCaseAMSBill()
		{
			var consol = Factory.New<ForwardingConsol>();
			var ams = consol.Numbers.AddNew();
			ams.CE_EntryNum = "AMS00001";
			ams.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			AssertEquals("ISF house bill", "AMS00001", consol.GetUpperCaseAMSBill());
			ams.CE_EntryNum = "ams00001";
			AssertEquals("ISF house bill", "AMS00001", consol.GetUpperCaseAMSBill());
		}
	}
}
