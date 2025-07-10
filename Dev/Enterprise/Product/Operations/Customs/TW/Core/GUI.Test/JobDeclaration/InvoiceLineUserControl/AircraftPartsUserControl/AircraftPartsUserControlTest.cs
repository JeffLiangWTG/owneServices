using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class AircraftPartsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingMembers()
		{
			using (var control = new AircraftPartsUserControl())
			{
				var bingdingSource = control.BindingSource;
				var englishDescriptionLongTextControl = control.FindSingleOrDefault<LongTextControl>(c => c.Name == "EnglishDescriptionLongTextControl");
				var chineseDescriptionLongTextControl = control.FindSingleOrDefault<LongTextControl>(c => c.Name == "ChineseDescriptionLongTextControl");
				CombineAssertions(() =>
				{
					AssertEquals("EnglishDescriptionLongTextControl BindingMember", "FilteredInvoiceLines.JI_Description", bingdingSource.GetBindingMember(englishDescriptionLongTextControl));
					AssertEquals("ChineseDescriptionLongTextControl BindingMember", "FilteredInvoiceLines.JI_NDescription", bingdingSource.GetBindingMember(chineseDescriptionLongTextControl));
				});
			}
		}
	}
}
