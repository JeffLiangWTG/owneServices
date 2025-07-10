using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class USMailRefHintExtensionTest : BaseExtensionTest<HintExtension>
	{
		public void TestAllCaptions()
		{
			var hintExtension = new USMailRefHintExtension();
			hintExtension.Initialize(new GenericExtendedControl());

			AssertEquals("Mail Ref.", hintExtension.ShortCaption);
			AssertEquals("Mail Reference", hintExtension.Caption);
			AssertEquals("Mail Reference of the consignment.", hintExtension.Description);
		}
	}
}
