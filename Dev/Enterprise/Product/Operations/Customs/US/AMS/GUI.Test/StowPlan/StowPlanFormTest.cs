using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.GUI.Testing
{
	[TestedType(typeof(StowPlanForm))]
	class StowPlanFormTest : ZFormBasherTest
	{
		public void TestShowDialog()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = Factory.NewWithValidTestData<RefVessel>().RV_FK;
			var shippingLine = new SeaShippingProviderCollection(Factory).AddNew();
			shippingLine.FillWithValidTestData();
			voyage.JV_OH_Line = shippingLine.PK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUMEL";
			origin.JA_E_DEP = ZDateTime.Today;
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USCHI";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(1);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			StowPlanForm.ShowDialog(voyage);
			AssertEquals("This schedule has errors, please fix and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			voyage.JV_VoyageFlight = "1233";
			StowPlanForm.ShowDialog(voyage);
			AssertEquals("This schedule has unsaved changes, would you like to save the changes and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			StowPlanForm.ShowDialog(voyage);
			Assert(voyage.IsInDatabase);
		}

		protected override Form GetFormToBashCore()
		{
			var voyage = Factory.New<JobVoyage>();
			return new StowPlanForm(new StowPlanSailingData(voyage));
		}
	}
}
