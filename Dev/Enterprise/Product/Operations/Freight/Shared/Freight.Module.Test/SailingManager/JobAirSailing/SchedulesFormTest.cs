using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(SchedulesForm))]
	sealed class SchedulesFormTest : ZFormBasherTest
	{
		#region Save Button

		public void TestSaveButton()
		{
			var sailing = Factory.NewWithValidTestData<BaseJobSailing>();
			var sailings = new BaseJobSailingCollection(Factory) { sailing };
			var criteria = new BulkCopyCriteriaForTest(sailing.PK);

			Factory.Save();

			criteria.ConsolDetails.GenerateConsols(sailings);
			var criteriaConsol = criteria.ConsolDetails.CreatedConsols[0];

			Assert("Pre-condition", !criteriaConsol.IsInDatabase);
			AssertNoErrors("Expected no errors on the created Consol's MAWB number so we should be able to save", criteriaConsol.JK_MasterBillNumInfo);

			using (var schedulesForm = new SchedulesForm(criteria))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				schedulesForm.Show();

				Assert("Pre-condition", schedulesForm.SaveButtonForTest.Visible);
				schedulesForm.SaveButtonForTest.PerformClick();

				Assert(!schedulesForm.SaveButtonForTest.Visible);
				Assert("Expected to have saved consol", criteriaConsol.IsInDatabase);
				AssertNull("Expected no error message on saving", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			BaseJobSailing sailing = Factory.New<BaseJobSailing>();
			BulkCopyCriteria bulkCopyCriteria = new BulkCopyCriteriaForTest(sailing.PK);

			return new SchedulesForm(bulkCopyCriteria);
		}

		#endregion
	}
}
