using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(UpdateOpportunityDateForExchangeRateForm))]
	sealed class UpdateOpportunityDateForExchangeRateFormTest : ZFormBasherTest
	{
		#region Buttons

		[TestDate(2022, 2, 2)]
		public void TestUpdateDateButton()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_DateForExchangeRate = new ZDateTime(2022, 2, 2);
			var updateAction = new UpdateP8_DateForExchangeRateAction(opportunity);
			using (var form = new UpdateOpportunityDateForExchangeRateForm(updateAction))
			{
				form.Show();

				updateAction.Date = ZDateTime.Empty;

				form.UpdateDateButton.PerformClick();
				UnitTestUserNotification.Instance.ClearMessages();
				AssertEquals("Should not have updated date", new ZDateTime(2022, 2, 2), opportunity.P8_DateForExchangeRate);
				AssertEquals("Should not close form", false, form.IsDisposed);

				updateAction.Date = new ZDateTime(2022, 3, 3);
				UnitTestUserNotification.Instance.ClearMessages();
				form.UpdateDateButton.PerformClick();
				AssertEquals("Should have updated date", new ZDateTime(2022, 3, 3), opportunity.P8_DateForExchangeRate);
				AssertEquals("Should have closed form", true, form.IsDisposed);
			}
		}

		public void TestUseExistingDateButton()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_DateForExchangeRate = new ZDateTime(2022, 2, 2);
			var updateAction = new UpdateP8_DateForExchangeRateAction(opportunity);
			using (var form = new UpdateOpportunityDateForExchangeRateForm(updateAction))
			{
				form.Show();

				updateAction.Date = new ZDateTime(2033, 3, 3);

				form.UseExistingDateButton.PerformClick();
				AssertEquals("Should not have updated date", new ZDateTime(2022, 2, 2), opportunity.P8_DateForExchangeRate);
				AssertEquals("Should have closed form", true, form.IsDisposed);
			}
		}

		#endregion

		#region Form Caption

		public void TestFormVerb()
		{
			using (var form = GetNewFormForTest())
			{
				AssertEquals("", form.FormVerb);
			}
		}

		#endregion

		#region Event Handlers

		public void TestCloseOnEscape()
		{
			var opp = Factory.New<OrgOpportunity>();
			var action = new UpdateP8_DateForExchangeRateAction(opp);
			using (var form = new UpdateOpportunityDateForExchangeRateForm(action))
			{
				form.Show();

				KeySender.PostKeyDown(form, Keys.Escape);
				Application.DoEvents();

				AssertEquals(DialogResult.Cancel, form.DialogResult);
				AssertEquals(true, form.IsDisposed);
			}
		}

		#endregion

		#region Implementation

		UpdateOpportunityDateForExchangeRateForm GetNewFormForTest()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var updateAction = new UpdateP8_DateForExchangeRateAction(opportunity);
			return new UpdateOpportunityDateForExchangeRateForm(updateAction);
		}

		protected override Form GetFormToBashCore()
		{
			return GetNewFormForTest();
		}

		#endregion
	}
}
