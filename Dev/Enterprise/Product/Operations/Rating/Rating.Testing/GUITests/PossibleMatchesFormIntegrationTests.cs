using System.Linq;
using Enterprise.Integration.Accounting;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Rating.Module;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.Testing.GUITests
{
	public class PossibleMatchesFormIntegrationTests : BaseRatingIntegrationTest
	{
		public void TestPossibleMatchesForm_ActivatesOpenedModuleForm_ShouldShowAMessageAboutIt()
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AUSYD", "USLAX", commodity: "XXX");

			Factory.Save();

			var controller = new ClientRatesController();
			using (var clientRateModuleForm = controller.ShowEditForm(clientRate))
			{
				var wrapper = new PossibleMatchesWrapper(CostSell.Revenue);
				wrapper.PossibleMatches.Add(rateEntry);
				using (var form = new PossibleMatchesForm(wrapper))
				{
					form.Show();

					var possibleMatchesGrid = form.FindAll<PossibleMatchesGrid>().Single();
					AssertNull("Before grid row is double clicked, grid should have no reference form.", possibleMatchesGrid.LastShownZForm);

					// double click grid while a module form of rate type has been opened.
					possibleMatchesGrid.InnerGrid.PerformMouseDownForTest(0, 2);

					AssertEquals(
						"The message should indicate that the tariffs and rates form is already open in the background.",
						"The tariffs and rates form for the selected rate is already opened in background. Please close this form, edit existing rates or add new rates and re-autorate.",
						UnitTestUserNotification.Instance.LastMessage.Text
					);

					var openedModuleForm = new ClientRatesController().GetOpenedForm(clientRate);
					AssertNotNull("Should refer to existing opened module form.", openedModuleForm);
					AssertSame("Grid should hold reference to the module form.", openedModuleForm, possibleMatchesGrid.LastShownZForm);
				}
			}
		}

		public void TestPossibleMatchesForm_ShowsNewModuleForm_ShouldNotShowMessage()
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AUSYD", "USLAX", commodity: "XXX");

			Factory.Save();

			var moduleForm = new ClientRatesController().GetOpenedForm(clientRate);
			AssertNull("Pre-condition: no client rate form is shown", moduleForm);

			var wrapper = new PossibleMatchesWrapper(CostSell.Revenue);
			wrapper.PossibleMatches.Add(rateEntry);
			using (var form = new PossibleMatchesForm(wrapper))
			{
				form.Show();

				var possibleMatchesGrid = form.FindAll<PossibleMatchesGrid>().Single();
				AssertNull("Before grid row is double clicked, grid should have no reference form.", possibleMatchesGrid.LastShownZForm);

				// double click grid when NO module form of rate type has been opened.
				possibleMatchesGrid.InnerGrid.PerformMouseDownForTest(0, 2);

				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				var openedModuleForm = new ClientRatesController().GetOpenedForm(clientRate);
				AssertNotNull("Should refer to newly opened module form.", openedModuleForm);
				AssertSame("Grid should hold reference to the module form.", openedModuleForm, possibleMatchesGrid.LastShownZForm);

				openedModuleForm.Close();
			}
		}
	}
}
