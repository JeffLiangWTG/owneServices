using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.Test
{
	public class AssignIATACodeCommandTest : AssignIATACodeCommandTestBase
	{
		#region Enabled

		public void TestIsEnabled()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX");

			CreateRefAirline("XX");

			var command = new CreateAirlineAndAssignIATACodeCommand() as IWiseRatesCommand;
			AssertEquals(true, command.IsEnabled(entryView));
		}

		public void TestIsEnabled_WhenWiseRateIsNotAir_CommandIsNotEnabled()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX", isAir: false);
			CreateRefAirline("XX");

			var command = new CreateAirlineAndAssignIATACodeCommand() as IWiseRatesCommand;

			CombineAssertions("Pre Conditions", () =>
			{
				AssertEquals("WiseEntryView IATA should be empty", false, string.IsNullOrWhiteSpace(entryView.RefCarrier.IATACode));
				AssertEquals("WiseEntryView TI_OH_TransportProvider should be empty", true, entryView.TI_OH_TransportProvider.IsEmpty);
			});

			AssertEquals(false, command.IsEnabled(entryView));
		}

		public void TestIsEnabled_WhenWiseRateDoesNotHaveIATACode_CommandIsNotEnabled()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var entryView = CreateWiseEntryView(org, string.Empty);

			var command = new CreateAirlineAndAssignIATACodeCommand() as IWiseRatesCommand;
			AssertEquals(false, command.IsEnabled(entryView));
		}

		public void TestIsEnabled_WhenNoRefAirlineWithIataCode_CommandIsNotEnabled()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX");

			var command = new CreateAirlineAndAssignIATACodeCommand() as IWiseRatesCommand;
			AssertEquals(false, command.IsEnabled(entryView));
		}

		public void TestIsEnabled_WhenCarrierAlreadyFound_CommandIsNotEnabled()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX", carrierFound: true);

			CreateRefAirline("XX");

			var command = new CreateAirlineAndAssignIATACodeCommand() as IWiseRatesCommand;
			AssertEquals(false, command.IsEnabled(entryView));
		}

		#endregion

		#region Assign

		public void TestAssign()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CR1";
			org.OH_IsShippingProvider = true;
			org.OH_IsAirLine = false;
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX");

			CreateRefAirline("XX", "00X");

			UnitTestUserNotification.Instance.AddYesAnswer();
			var cmd = new CreateAirlineAndAssignIATACodeCommandForTest(org);

			var assignmentResult = cmd.Assign(entryView, null);

			org.Reload();

			AssertEquals("Assignment Should Happen", true, assignmentResult);
			AssertEquals("After assignment, Carrier should be an Airline", true, org.OH_IsAirLine);
			AssertEquals("After assignment, Carrier should be assigned to an Airline profile", "00X", org.MiscServ.Airline.RM_EagleAddedAirlinePrefixOrAccountingCode);
		}

		public void TestAssign_WhenUserDoNotConfirmTheAssignment_ThenShouldNotAssign()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CR1";
			org.OH_IsShippingProvider = true;
			org.OH_IsAirLine = false;
			org.MiscServ.OM_RM_Airline = ZGuid.Empty;
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX");

			CreateRefAirline("XX", "00X");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var cmd = new CreateAirlineAndAssignIATACodeCommandForTest(org);

			var assignmentResult = cmd.Assign(entryView, null);

			org.Reload();

			AssertEquals("Assignment should not happen", false, assignmentResult);
			AssertEquals("Carrier should not be an Airline", false, org.OH_IsAirLine);
			AssertNull(org.MiscServ.Airline);
		}

		public void TestAssign_WhenUserHasNoOrgEditAccess_ShouldPromptForUserLogin()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CR1";
			org.OH_IsShippingProvider = true;
			org.OH_IsAirLine = false;
			org.MiscServ.OM_RM_Airline = ZGuid.Empty;
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX");

			CreateRefAirline("XX", "00X");

			UnitTestUserNotification.Instance.AddYesAnswer();
			var cmd = new CreateAirlineAndAssignIATACodeCommandForTest(org);

			Env.Security.OrganisationModify.IsAllowed = false;

			cmd.Assign(entryView, null);

			AssertEquals(typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestAssign_WhenSelectedOrganizationAlreadyAssignedAirlineProfile_ShouldNotAssign()
		{
			var airline = CreateRefAirline("YY", "0XX");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CR1";
			org.OH_IsShippingProvider = true;
			org.OH_IsAirLine = true;
			org.MiscServ.OM_RM_Airline = airline.PK;
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX");

			CreateRefAirline("XX", "00X");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var cmd = new CreateAirlineAndAssignIATACodeCommandForTest(org);

			var assignmentResult = cmd.Assign(entryView, null);

			org.Reload();

			AssertEquals(false, assignmentResult);
			AssertEquals("An airline profile has already been assigned to the carrier CR1. Please choose another one.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion
	}
}
