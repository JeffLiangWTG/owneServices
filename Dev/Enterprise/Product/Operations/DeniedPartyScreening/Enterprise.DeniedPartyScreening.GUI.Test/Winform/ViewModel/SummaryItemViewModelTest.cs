using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class SummaryItemViewModelTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var model = new SummaryItemViewModel(header, "Test Name", ScreeningStatusesList.Codes.Matched);
			AssertEquals("Test Name", model.Name);
			AssertEquals(ScreeningStatusesList.Codes.Matched, model.ScreeningStatus);
		}

		public void TestOpenPartyFormCommand()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var notLinkedVessel = Factory.New<ITransport>();
			notLinkedVessel.ParentType = typeof(ITransportParentCommon);
			notLinkedVessel.JW_Vessel = "notLinkedVessel";
			var person = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var model = new SummaryItemViewModel(header, "Test Name", ScreeningStatusesList.Codes.Matched);
			AssertNoExceptionThrown(() => model.OpenPartyFormCommand.Execute(null));
			using (var form = Application.OpenForms.OfType<BaseOrganisationsForm>().SingleOrDefault())
			{
				AssertNotNull(form);
			}

			model = new SummaryItemViewModel(jobDocAddress, "Test Name", ScreeningStatusesList.Codes.Matched);
			model.OpenPartyFormCommand.Execute(null);
			AssertEquals("Not able to show form for this entity.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			model = new SummaryItemViewModel(vessel, "Test Name", ScreeningStatusesList.Codes.Matched);
			AssertNoExceptionThrown(() => model.OpenPartyFormCommand.Execute(null));
			using (var form = Application.OpenForms.OfType<RefVesselForm>().SingleOrDefault())
			{
				AssertNotNull(form);
			}

			model = new SummaryItemViewModel(notLinkedVessel as BusinessObject, "Test Name", ScreeningStatusesList.Codes.Matched);
			model.OpenPartyFormCommand.Execute(null);
			AssertEquals("Not able to show form for this entity.", UnitTestUserNotification.Instance.LastMessage.Text);

			model = new SummaryItemViewModel(person, "Test Name", ScreeningStatusesList.Codes.Matched);
			model.OpenPartyFormCommand.Execute(null);
			AssertNoExceptionThrown(() => model.OpenPartyFormCommand.Execute(null));
			using (var form = Application.OpenForms.OfType<GlbPersonForm>().SingleOrDefault())
			{
				AssertNotNull(form);
			}
		}

		public void TestOpenPartyFormCommandForSpecificClient()
		{
			var header = Factory.NewWithValidTestData<OrgHeaderTest.OrgHeaderForTest>();
			Factory.Save();

			var model = new SummaryItemViewModel(header, "Test Name", ScreeningStatusesList.Codes.Matched);
			AssertNoExceptionThrown(() => model.OpenPartyFormCommand.Execute(null));
			using (var form = Application.OpenForms.OfType<BaseOrganisationsForm>().SingleOrDefault())
			{
				AssertNotNull(form);
			}
		}
	}
}
