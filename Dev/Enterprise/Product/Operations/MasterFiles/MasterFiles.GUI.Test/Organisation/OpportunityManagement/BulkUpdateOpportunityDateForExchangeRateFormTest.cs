using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(BulkUpdateOpportunityDateForExchangeRateForm))]
	sealed class BulkUpdateOpportunityDateForExchangeRateFormTest : ZFormBasherTest
	{
		#region InstructionsLabel

		[RequiresSTA]
		public void TestInstructionsLabel()
		{
			var opportunity1 = Factory.New<OrgOpportunity>();
			var opportunity2 = Factory.New<OrgOpportunity>();
			var opportunity3 = Factory.New<OrgOpportunity>();

			var updateAction = new BulkUpdateP8_DateForExchangeRateAction(new[] { opportunity1, opportunity2, opportunity3 });
			using (var form = new BulkUpdateOpportunityDateForExchangeRateForm(updateAction))
			{
				form.Show();

				AssertEquals(@"Updating the Exchange Rate Date for the 3 selected opportunity(s).
The total opportunity estimate values will be converted into the new currency as per date set.", form.InstructionsLabel.Text);
			}
		}

		#endregion

		#region Buttons

		[TestDate(2022, 2, 2)]
		public void TestUpdateDateButton()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_DateForExchangeRate = new ZDateTime(2022, 2, 2);
			Factory.Save();

			var updateAction = new BulkUpdateP8_DateForExchangeRateAction(new[] { opportunity });
			using (var form = new BulkUpdateOpportunityDateForExchangeRateForm(updateAction))
			{
				form.Show();

				updateAction.Date = ZDateTime.Empty;

				form.UpdateDateButton.PerformClick();
				var anotherFactory = new BusinessObjectFactory();
				var oppInOtherFactory = anotherFactory.Load<OrgOpportunity>(opportunity.PK);
				AssertEquals("Should not have updated date", new ZDateTime(2022, 2, 2), oppInOtherFactory.P8_DateForExchangeRate);
				AssertEquals("Should not close form", false, form.IsDisposed);

				updateAction.Date = new ZDateTime(2022, 3, 3);
				form.UpdateDateButton.PerformClick();
				anotherFactory = new BusinessObjectFactory();
				oppInOtherFactory = anotherFactory.Load<OrgOpportunity>(opportunity.PK);
				AssertEquals("Should have updated date", new ZDateTime(2022, 3, 3), oppInOtherFactory.P8_DateForExchangeRate);
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

		#region Implementation

		BulkUpdateOpportunityDateForExchangeRateForm GetNewFormForTest()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var updateAction = new BulkUpdateP8_DateForExchangeRateAction(new[] { opportunity });
			return new BulkUpdateOpportunityDateForExchangeRateForm(updateAction);
		}

		protected override Form GetFormToBashCore()
		{
			return GetNewFormForTest();
		}

		#endregion
	}
}
