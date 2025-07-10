using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.US.GUI
{
	sealed class EntryNumberColumnStyleTest : TestCaseWithDummy
	{
		public void TestEmbeddedPopupType()
		{
			using (var userControl = new ZGridEntryNumberUserControl())
			using (var popup = userControl.CreateEmbeddedPopupInternal(new DummyFilterGridModule()))
			{
				AssertEquals("CustomsEmbeddedModulePopup has been created", typeof(CustomsEmbeddedModulePopup), popup.GetType());
			}
		}
	}
}
