using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Test
{
	internal class ExternalRequestTypeDetailsTest : TestCaseWithFactory
	{
		public void TestFields()
		{
			var factory = new BusinessObjectFactory();

			var externalRequestInfoTemplate = factory.New<ExternalRequestInfoTemplate>();
			externalRequestInfoTemplate.RIT_Code = "A01";
			externalRequestInfoTemplate.RIT_Description = "A01 Desc";
			externalRequestInfoTemplate.RIT_JobType = ExternalRequestTypeJobTypes.Codes.ORD;
			externalRequestInfoTemplate.RIT_IsActive = ZBool.True;
			factory.Save();

			var bO = factory.NewWithValidTestData<ExternalRequestType>();
			bO.RQT_Code = "XXX";
			bO.RQT_Description = "XXX DESC";
			bO.RQT_IsActive = true;
			bO.RQT_IsSystem = true;
			bO.RQT_JobType = ExternalRequestTypeJobTypes.Codes.SBK;
			bO.RQT_Reviewer = DocAddressTypes.Codes.ControllingCustomer;
			bO.RQT_Assignee = DocAddressTypes.Codes.Manufacturer;
			bO.RQT_RequiredInDays = 10;
			bO.RQT_RIT_Template = externalRequestInfoTemplate.PK;

			factory.Save();

			using (var form = new ExternalRequestTypeForm(bO))
			{
				form.Show();

				var codeTextBox = form.FindSingle<ZTextBox>("RQT_CodeTextBox");
				var descriptionTextBox = form.FindSingle<ZTextBox>("RQT_DescriptionTextBox");
				var jobTypeDropEdit = form.FindSingle<ZDropEdit>("RQT_JobTypeDropEdit");
				var isActiveCheckBox = form.FindSingle<ZCheckBox>("RQT_IsActiveCheckBox");
				var formTypeGuidFindBox = form.FindSingle<ZGuidFindBox>("RQT_FormTypeGuiFindBox");
				var assigneeDropEdit = form.FindSingle<ZDropEdit>("RQT_AssigneeDropEdit");
				var reviewerDropEdit = form.FindSingle<ZDropEdit>("RQT_ReviewerDropEdit");
				var requiredInDaysCalcEdit = form.FindSingle<ZCalcEdit>("RQT_RequiredInDaysCalcEdit");

				AssertEquals("XXX", codeTextBox.Text);
				AssertEquals("XXX DESC", descriptionTextBox.Text);
				AssertEquals(ExternalRequestTypeJobTypes.Codes.SBK, jobTypeDropEdit.Text);
				AssertEquals(true, isActiveCheckBox.Checked);
				AssertEquals(externalRequestInfoTemplate.PK, formTypeGuidFindBox.Guid);
				AssertEquals("A01", formTypeGuidFindBox.CodeBox.Text);
				AssertEquals(DocAddressTypes.Codes.Manufacturer, assigneeDropEdit.Text);
				AssertEquals(DocAddressTypes.Codes.ControllingCustomer, reviewerDropEdit.Text);
				AssertEquals(10m, requiredInDaysCalcEdit.CalcValue);
			}
		}

		public void TestRQT_DescriptionTextBox_Should_Store_And_Display_Up_To_100_Characters()
		{
			using (var form = new ExternalRequestTypeDetails())
			{
				form.Show();

				var descriptionTextBox = (ZTextBox)form.Controls.Find("RQT_DescriptionTextBox", true)[0];

				var descriptionText = new string('A', 100);
				descriptionTextBox.Text = descriptionText;
				var expectedWidth = TextRenderer.MeasureText(descriptionText, descriptionTextBox.Font).Width;

				AssertEquals(100, descriptionTextBox.Text.Length);
				AssertGreaterThanOrEqualTo(expectedWidth, descriptionTextBox.Width);
			}
		}
	}
}
