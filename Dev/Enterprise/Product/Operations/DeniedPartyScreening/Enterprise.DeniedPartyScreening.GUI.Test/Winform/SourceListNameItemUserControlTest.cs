
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class SourceListNameItemUserControlTest : TestCaseWithFactory
	{
		public void TestSetDataBinding()
		{
			using (var itemUserControl = new SourceListNameItemUserControlForTest())
			{
				var winModel = new SourceListNamesWinModel(new SourceListNamesModel(Factory, new[] { "AAA" }));
				itemUserControl.SetDataBinding(winModel.IncludedSourceListNames.Single(), "");

				AssertEquals("AAA", itemUserControl.OpenSourceListOnLinkLabel.Text);
			}
		}

		public void TestSetDataBinding_NoExceptionthrown()
		{
			using (var itemUserControl = new SourceListNameItemUserControlForTest())
			{
				AssertNoExceptionThrown(() => itemUserControl.SetDataBinding(null, ""));
			}
		}

		public void TestOpenComplianceListForm_WhenSourceComplianceListNotExists_ShowMessage()
		{
			using (var itemUserControl = new SourceListNameItemUserControlForTest())
			{
				UnitTestUserNotification.Instance.ClearMessages();

				var winModel = new SourceListNamesWinModel(new SourceListNamesModel(Factory, new[] { "AAA" }));
				itemUserControl.SetDataBinding(winModel.IncludedSourceListNames.Single(), "");
				itemUserControl.OpenSourceListLinkLabel_OnLinkClicked();

				AssertEquals("This record is currently unavailable via the Compliance Lists (Party Screening) module. Please raise a customer support incident if this issue persists.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenComplianceListForm_WhenSourceComplianceListExists()
		{
			var complianceList = CommonTestDataHelper.CreateComplianceList(Factory, "USFCU");
			Factory.Save();

			using (var itemUserControl = new SourceListNameItemUserControlForTest())
			{
				var winModel = new SourceListNamesWinModel(new SourceListNamesModel(Factory, new[] { (string)complianceList.RCL_ListCode }));
				itemUserControl.SetDataBinding(winModel.IncludedSourceListNames.Single(), "");
				itemUserControl.OpenSourceListLinkLabel_OnLinkClicked();

				using (var form = Application.OpenForms.OfType<RefComplianceListForm>().SingleOrDefault())
				{
					AssertNotNull(form);
					form.Close();
				}
			}
		}

		class SourceListNameItemUserControlForTest : SourceListNameItemUserControl
		{
			public ZLinkLabel OpenSourceListOnLinkLabel => OpenSourceListLinkLabel;

			public void OpenSourceListLinkLabel_OnLinkClicked() => OpenSourceListLinkLabel_LinkClicked(this, new LinkLabelLinkClickedEventArgs(null));
		}
	}
}
