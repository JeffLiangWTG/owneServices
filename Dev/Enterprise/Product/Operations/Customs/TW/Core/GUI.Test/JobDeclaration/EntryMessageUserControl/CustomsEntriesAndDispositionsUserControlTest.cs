using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class CustomsEntriesAndDispositionsUserControlTest : TestCaseWithFactory
	{
		public void TestCustomsResponseCodeGridColumns()
		{
			using (var form = new ZForm(Factory.New<JobDeclaration>()))
			{
				var userControl = new CustomsEntriesAndDispositionsUserControl();
				form.Controls.Add(userControl);
				form.Show();

				var customsResponseCodeGrid = userControl.FindSingle<ZGrid>("CustomsResponseCodeGrid");
				CombineAssertions(() =>
				{
					AssertNotNull("User control should have StatusKey", customsResponseCodeGrid.GetColumnStyle(CusDisposition.Schema.CDI_StatusKey));
					AssertNotNull("User control should have Status", customsResponseCodeGrid.GetColumnStyle(CusDisposition.Schema.CDI_Status));
					AssertNotNull("User control should have StatusDate", customsResponseCodeGrid.GetColumnStyle(CusDisposition.Schema.CDI_StatusDate));
					AssertNotNull("User control should have Status Description", customsResponseCodeGrid.GetColumnStyle(CusDisposition.Schema.StatusDescription));
				});
			}
		}

		public void TestCustomsResponseCodeGroupBoxCaption()
		{
			using (var form = new ZForm(Factory.New<JobDeclaration>()))
			{
				var userControl = new CustomsEntriesAndDispositionsUserControl();
				form.Controls.Add(userControl);
				form.Show();

				var customsResponseCodeGroupBox = userControl.FindSingle<ZGroupBox>("CustomsResponseCodeGroupBox");
				AssertEquals("Customs Response Code", customsResponseCodeGroupBox.CaptionResourceString.Caption);
			}
		}
	}
}
