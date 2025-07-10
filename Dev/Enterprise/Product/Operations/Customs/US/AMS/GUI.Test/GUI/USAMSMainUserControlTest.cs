using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Business;

namespace Enterprise.Customs.US.AMS.GUI.Testing
{
	class USAMSMainUserControlTest : TestCaseWithFactory
	{
		public void TestRemoveBH_IsOutboundCargoCheckBox()
		{
			var header = Factory.New<CusInBondHeader>();
			using (var form = new USAMSForm(header))
			{
				var bH_IsOutboundCargoCheckBox = form.Controls.Find("BH_IsOutboundCargoCheckBox", true);
				AssertEquals("BH_IsOutboundCargoCheckBox is not exsited", 0, bH_IsOutboundCargoCheckBox.Length);
			}
		}
	}
}
