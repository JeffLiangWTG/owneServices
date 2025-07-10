using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

sealed class CompactDisplayModeBehaviourTest : TestCaseWithFactory
{
	public void TestUpdateBehaviour()
	{
		var nctsHeader = Factory.New<EU.NCTS.Business.NctsHeader>();
		using (var zDocAddressControl = new ZDocAddressControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals("base zDocAddressControl DisplayMode", ZDocAddressControlDisplayMode.ShowOverrideAndTabs, zDocAddressControl.DisplayMode);

				var behaviour = new CompactDisplayModeBehaviour();
				behaviour.UpdateBehaviour(zDocAddressControl, nctsHeader);
				AssertEquals("after zDocAddressControl DisplayMode", ZDocAddressControlDisplayMode.Compact, zDocAddressControl.DisplayMode);
			});
		}
	}
}
