using CargoWise.EntityFramework.Testing;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.GUI.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Telematics.GUI.Test.Forms
{
	class TelPreDriveChecklistTemplateFormTest : TestCaseWithFactory
	{
		public void TestDoesNotAllowNew()
		{
			AssertEquals(false, ((IPostingButtonsProvider)form).AllowNew);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<TelPreDriveChecklistTemplateHeader>();
			form = new TelPreDriveChecklistTemplateForm(header);
			form.Show();
		}

		protected override void TearDown()
		{
			form.Hide();
			form.Dispose();
			form = null;

			base.TearDown();
		}

		TelPreDriveChecklistTemplateHeader header;
		TelPreDriveChecklistTemplateForm form;
	}
}
