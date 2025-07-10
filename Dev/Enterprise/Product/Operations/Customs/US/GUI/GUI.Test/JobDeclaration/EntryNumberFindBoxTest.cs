using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.US.GUI
{
	sealed class EntryNumberFindBoxTest : TestCaseWithDummy
	{
		public void TestEmbeddedPopupType()
		{
			using (var testFindBox = new EntryNumberFindBox())
			using (var popup = testFindBox.CreateEmbeddedPopupInternal(new DummyFilterGridModule()))
			{
				AssertEquals("CustomsEmbeddedModulePopup has been created", typeof(CustomsEmbeddedModulePopup), popup.GetType());
			}
		}
	}
}
