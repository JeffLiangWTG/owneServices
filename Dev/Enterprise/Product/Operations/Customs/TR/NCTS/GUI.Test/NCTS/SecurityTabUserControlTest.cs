using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NctsHeader = Enterprise.Customs.TR.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	class SecurityTabUserControlTest : TestCaseWithFactory
	{
		public void TestPlaceOfloadingControls()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			using (var control = new SecurityTabUserControl())
			{
				control.SetDataBinding(header, "");
				var placeOfUnloadingFindBox = control.FindSingleOrDefault<ZCodeFindBox>("PlaceOfUnloadingFindBox");
				var placeOfloadingFindBox = control.FindSingleOrDefault<ZCodeFindBox>("PlaceOfloadingFindBox");

				CombineAssertions(() =>
				{
					AssertEquals("PlaceOfloadingFindBox.Visible", true, placeOfloadingFindBox.Visible);
					AssertEquals("PlaceOfloadingFindBox.CodeBox.Size", ControlDpiScalingHelper.NewScaledSize(45, 20), placeOfloadingFindBox.CodeBox.Size);
					AssertEquals("PlaceOfUnloadingFindBox.Visible", true, placeOfUnloadingFindBox.Visible);
					AssertEquals("PlaceOfloadingFindBox.CodeBox.Size", ControlDpiScalingHelper.NewScaledSize(45, 20), placeOfUnloadingFindBox.CodeBox.Size);
				});
			}
		}
	}
}
