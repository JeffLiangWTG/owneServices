using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Test
{
	internal class ExternalRequestInfoTemplateDetailsTest : TestCaseWithFactory
	{
		public void TestFields()
		{
			var factory = new BusinessObjectFactory();
			var bO = factory.NewWithValidTestData<ExternalRequestInfoTemplate>();
			bO.RIT_Code = "XXX";
			bO.RIT_Description = "XXX DESC";
			bO.RIT_IsActive = true;
			bO.RIT_IsSystem = true;
			bO.RIT_JobType = ExternalRequestTypeJobTypes.Codes.SPL;

			factory.Save();

			using (var form = new ExternalRequestInfoTemplateForm(bO))
			{
				form.Show();

				var codeTextBox = form.FindSingle<ZTextBox>("RIT_CodeTextBox");
				var descriptionTextBox = form.FindSingle<ZTextBox>("RIT_DescriptionTextBox");
				var jobTypeDropEdit = form.FindSingle<ZDropEdit>("RIT_JobTypeDropEdit");
				var isActiveCheckBox = form.FindSingle<ZCheckBox>("RIT_IsActiveCheckBox");

				AssertEquals("XXX", codeTextBox.Text);
				AssertEquals("XXX DESC", descriptionTextBox.Text);
				AssertEquals(ExternalRequestTypeJobTypes.Codes.SPL, jobTypeDropEdit.Text);
				AssertEquals(true, isActiveCheckBox.Checked);
				AssertEquals(true, jobTypeDropEdit.ReadOnly);
			}
		}

		public void TestJobType_IsEditable_WhenNotSaved()
		{
			var factory = new BusinessObjectFactory();
			var bO = factory.NewWithValidTestData<ExternalRequestInfoTemplate>();
			bO.RIT_Code = "XXX";
			bO.RIT_Description = "XXX DESC";
			bO.RIT_IsActive = true;
			bO.RIT_IsSystem = true;
			bO.RIT_JobType = ExternalRequestTypeJobTypes.Codes.SPL;

			using (var form = new ExternalRequestInfoTemplateForm(bO))
			{
				form.Show();

				var jobTypeDropEdit = form.FindSingle<ZDropEdit>("RIT_JobTypeDropEdit");

				AssertEquals(false, jobTypeDropEdit.ReadOnly);
			}
		}
	}
}
