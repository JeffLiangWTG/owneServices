using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class CityTownMultiCombinationControlTest : TestCase
	{
		public void TestGridGuidFindBoxGuidCacheIsDisabled()
		{
			using (ZForm testForm = new ZForm())
			{
				var multiControl = new CityTownMultiCombinationControl()
				{
					ControlType = FieldType.Guid,
					ModuleID = DummyModuleIDs.Dummy
				};
				testForm.Controls.Add(multiControl);

				testForm.Show();
				AssertType("CurrentEditor should be a ZGridGuidFindBox", typeof(CityTownFindBox), multiControl.CurrentEditor);

				var findBox = multiControl.CurrentEditor as CityTownFindBox;
				Assert("The ZGridGuidFindBox should has Guid Cache disabled", !findBox.IsGuidCacheEnabled);
			}
		}
	}
}
