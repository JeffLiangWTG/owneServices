using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class LegacyValueStripControlTest : TestCaseWithFactory
	{
		#region Properties

		public void TestIsMandatory()
		{
			using (var control = new LegacyValueStripControl())
			{
				control.IsMandatory = true;
				AssertEquals(false, control.DeleteButton.Visible);

				control.IsMandatory = false;
				AssertEquals(true, control.DeleteButton.Visible);
			}
		}

		#endregion

		#region ReadOnly

		public void TestReadOnly()
		{
			using (var control = new LegacyValueStripControl())
			{
				control.ReadOnly = true;
				AssertEquals(false, control.DeleteButton.Visible);

				control.ReadOnly = false;
				AssertEquals(true, control.DeleteButton.Visible);
			}
		}

		#endregion

		#region Delete Button

		public void TestDeleteButton()
		{
			var org = Factory.New<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var valueItem = opportunity.ValueItems.AddNew();

			using (var form = new ZForm(opportunity))
			using (var control = new LegacyValueStripControl())
			{
				form.Controls.Add(control);
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.DeleteButton.PerformClick();

				AssertEquals("Caption", "Delete all Legacy Estimate Values", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Text", "Are you sure you want to delete all estimate values for Legacy Value Analysis?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, opportunity.IsDeleted);
				AssertNotNull(control.Parent);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				control.DeleteButton.PerformClick();

				AssertEquals(false, opportunity.IsDeleted);
				AssertEquals(true, valueItem.IsDeleted);
				AssertNull(control.Parent);
			}
		}

		#endregion
	}
}
