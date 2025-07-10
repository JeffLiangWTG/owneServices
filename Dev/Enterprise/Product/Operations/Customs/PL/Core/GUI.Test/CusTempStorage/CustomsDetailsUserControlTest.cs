using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.CusTempStorage;
using Enterprise.Customs.PL.GUI.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

class CustomsDetailsUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		var header = CusTempStorageJobHeader.New(Factory);
		using (var userControl = new CustomsDetailsUserControl())
		{
			using (var form = new ZForm(header))
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("EntryCustomsOfficeCodeFindBox", true, userControl.FindSingleOrDefault<ZCodeFindBox>("EntryCustomsOfficeCodeFindBox").Visible);
					AssertEquals("CustomsOfficeCodeFindBox", true, userControl.FindSingleOrDefault<ZCodeFindBox>("CustomsOfficeCodeFindBox").Visible);
					AssertEquals("PreviousRefTypeDropEdit", true, userControl.FindSingleOrDefault<ZDropEditWithFixedWidth>("PreviousRefTypeDropEdit").Visible);
					AssertEquals("PreviousRefNumTextBox", true, userControl.FindSingleOrDefault<ZTextBox>("PreviousRefNumTextBox").Visible);
					AssertEquals("NCTSFlagCheckBox", true, userControl.FindSingleOrDefault<ZCheckBox>("NCTSFlagCheckBox").Visible);
					AssertEquals("AdditionalInfoTextBox", true, userControl.FindSingleOrDefault<ZTextBox>("AdditionalInfoTextBox").Visible);
					AssertEquals("PresentationDateDateEdit", true, userControl.FindSingleOrDefault<ZDateEdit>("PresentationDateDateEdit").Visible);
				});
			}
		}
	}
}
