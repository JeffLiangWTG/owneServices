using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Rating.GUI.Test
{
	public class CreateAirlineAndAssignIATACodeCommandTest : AssignIATACodeCommandTestBase
	{
		#region Enabled

		public void TestIsEnabled_WhenWiseRateIsNotAir_CommandIsNotEnabled()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var entryView = CreateWiseEntryView(orgHeader, "XX", isAir: false);

			var command = new CreateAirlineAndAssignIATACodeCommand(shouldCreateAirline: true) as IWiseRatesCommand;

			CombineAssertions("Pre Conditions", () =>
			{
				AssertEquals("WiseEntryView IATA should be empty", false, string.IsNullOrWhiteSpace(entryView.RefCarrier.IATACode));
				Assert("WiseEntryView TI_OH_TransportProvider should be empty", entryView.TI_OH_TransportProvider.IsEmpty);
			});

			AssertEquals(false, command.IsEnabled(entryView));
		}

		public void TestIsEnabled_WhenWiseRateDoesNotHaveIATACode_CommandIsNotEnabled()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var entryView = CreateWiseEntryView(org, iataCode: string.Empty);

			var command = new CreateAirlineAndAssignIATACodeCommand(shouldCreateAirline: true) as IWiseRatesCommand;

			AssertEquals("IATA Code should be empty", string.Empty, entryView.RefCarrier.IATACode);
			AssertEquals("Command should be disabled", false, command.IsEnabled(entryView));
		}

		public void TestIsEnabled_WhenAirlineExistsWithIATACode_CommandIsNotEnabled()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX");
			CreateRefAirline("XX");

			var command = new CreateAirlineAndAssignIATACodeCommand(shouldCreateAirline: true) as IWiseRatesCommand;

			AssertEquals(false, command.IsEnabled(entryView));
		}

		public void TestIsEnabled_WhenCarrierAlreadyFound_CommandIsNotEnabled()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX", carrierFound: true);

			var command = new CreateAirlineAndAssignIATACodeCommand(shouldCreateAirline: true) as IWiseRatesCommand;
			AssertEquals(false, command.IsEnabled(entryView));
		}

		public void TestIsEnabled()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX");

			var command = new CreateAirlineAndAssignIATACodeCommand(shouldCreateAirline: true) as IWiseRatesCommand;

			Assert(command.IsEnabled(entryView));
		}

		#endregion

		#region Assign

		public void TestAssign_WhenUserDoesNotWantToCreateNewAirLine_ShouldNotAssign()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CR1";
			org.OH_IsShippingProvider = true;
			org.OH_IsAirLine = false;
			org.MiscServ.OM_RM_Airline = ZGuid.Empty;

			var entryView = CreateWiseEntryView(org, "XX");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			var command = new CreateAirlineAndAssignIATACodeCommandForTest(org, shouldCreateAirline: true);

			var assignmentResult = command.Assign(entryView, null);
			AssertEquals("Assignment result should be false when user cancels airline creation", false, assignmentResult);
		}

		public void TestAssign_WhenAirlineIsNotSaved_ShouldNotAssign()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CR1";
			org.OH_IsShippingProvider = true;
			org.OH_IsAirLine = false;
			org.MiscServ.OM_RM_Airline = ZGuid.Empty;
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			var command = new CreateAirlineAndAssignIATACodeCommandForTest(org, shouldCreateAirline: true);

			var assignmentResult = command.Assign(entryView, null);

			org.Reload();
			var airline = CreateAirlineAndAssignIATACodeCommand.GetAirline("XX");

			AssertEquals("Assignment should not happen", false, assignmentResult);
			AssertEquals("Carrier should not be an Airline", false, org.OH_IsAirLine);
			AssertNull("No airline should be created", airline);
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

			var airline = CreateANewAirline("XX");

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				if (form is RefAirlineForm refAirlineForm)
				{
					refAirlineForm.FireSaveButton();
					refAirlineForm.Close();
				}
			});

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var command = new CreateAirlineAndAssignIATACodeCommandForTest(org, shouldCreateAirline: true, airline: airline);

			Env.Security.OrganisationModify.IsAllowed = false;

			command.Assign(entryView, null);

			AssertEquals(typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestAssign_WhenUserHasNoRefAirlineAccess_ShouldPromptForUserLogin()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CR1";
			org.OH_IsShippingProvider = true;
			org.OH_IsAirLine = false;
			org.MiscServ.OM_RM_Airline = ZGuid.Empty;
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX");

			var airline = CreateANewAirline("XX");
			var command = new CreateAirlineAndAssignIATACodeCommandForTest(org, shouldCreateAirline: true, airline: airline);

			Env.Security.OrganisationModify.IsAllowed = true;
			Env.Security.RefAirlineModify.IsAllowed = false;

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			command.Assign(entryView, null);

			AssertEquals(typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestAssign_WhenUserDoNotConfirmTheAssignment_ShouldNotAssign()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CR1";
			org.OH_IsShippingProvider = true;
			org.OH_IsAirLine = false;
			org.MiscServ.OM_RM_Airline = ZGuid.Empty;
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX");

			var airline = CreateANewAirline("XX");

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var refAirlineForm = (RefAirlineForm)form;
				refAirlineForm.FireSaveButton();
				refAirlineForm.Close();
			});

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var command = new CreateAirlineAndAssignIATACodeCommandForTest(org, shouldCreateAirline: true, airline);

			var assignmentResult = command.Assign(entryView, null);

			org.Reload();
			var createdAirline = CreateAirlineAndAssignIATACodeCommand.GetAirline("XX");

			AssertEquals("Assignment should not happen", false, assignmentResult);
			AssertEquals("Carrier should not be an Airline", false, org.OH_IsAirLine);
			AssertNull(org.MiscServ.Airline);

			AssertNotNull("Airline should be created", createdAirline);
			AssertEquals("Created Airline IATA Code", "XX", createdAirline.RM_TwoCharacterCode);
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

			airline = CreateANewAirline("XX");

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var refAirlineForm = (RefAirlineForm)form;
				refAirlineForm.FireSaveButton();
				refAirlineForm.Close();
			});

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var command = new CreateAirlineAndAssignIATACodeCommandForTest(org, shouldCreateAirline: true, airline);

			var assignmentResult = command.Assign(entryView, null);

			org.Reload();
			var createdAirline = CreateAirlineAndAssignIATACodeCommand.GetAirline("XX");

			AssertNotNull("Airline should be created", createdAirline);
			AssertEquals("Created Airline IATA Code", "XX", createdAirline.RM_TwoCharacterCode);

			AssertEquals(false, assignmentResult);
			AssertEquals("An airline profile has already been assigned to the carrier CR1. Please choose another one.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestAssign_WhenMultipleOrganizationHavingSameIATACode_ShouldAskForOneToMap_UserSelectsOne()
		{
			var airline = CreateRefAirline("YY", "0XX");

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "CR1";
			carrier1.OH_FullName = "CR FULLNAME 1";
			carrier1.OH_IsShippingProvider = true;
			carrier1.OH_IsAirLine = true;
			carrier1.MiscServ.OM_RM_Airline = airline.PK;

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_Code = "CR2";
			carrier2.OH_FullName = "CR FULLNAME 2";
			carrier2.OH_IsShippingProvider = true;
			carrier2.OH_IsAirLine = true;
			carrier2.MiscServ.OM_RM_Airline = airline.PK;

			Factory.Save();

			var mockedDialogService = new Mock<IDialogService>();
			mockedDialogService
				.Setup
				(
					d => d.SelectSingleOrganization
					(
						It.IsAny<IEnumerable<string>>(),
						"Multiple Organizations have been found assigned with Carrier 'YY'. Please select which Organization to be populated as the Carrier to the job."
					)
				)
				.Returns("CR2 - CR FULLNAME 2");
			var command = new CreateAirlineAndAssignIATACodeCommandForTest(orgHeader: null, dialogService: mockedDialogService.Object);
			var assignedOrganization = command.Assign("YY", null);

			AssertNotNull(nameof(assignedOrganization), assignedOrganization);
			AssertEquals("CR2 code should have been assigned", "CR2", assignedOrganization.OH_Code);
		}

		public void TestAssign_WhenMultipleOrganizationHavingSameIATACode_ShouldAskForOneToMap_UserDoesNotSelectOne()
		{
			var airline = CreateRefAirline("YY", "0XX");

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "CR1";
			carrier1.OH_FullName = "CR FULLNAME 1";
			carrier1.OH_IsShippingProvider = true;
			carrier1.OH_IsAirLine = true;
			carrier1.MiscServ.OM_RM_Airline = airline.PK;

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_Code = "CR2";
			carrier2.OH_FullName = "CR FULLNAME 2";
			carrier2.OH_IsShippingProvider = true;
			carrier2.OH_IsAirLine = true;
			carrier2.MiscServ.OM_RM_Airline = airline.PK;

			Factory.Save();

			var mockedDialogService = new Mock<IDialogService>();
			mockedDialogService
				.Setup(
					d => d.SelectSingleOrganization(
						It.IsAny<IEnumerable<string>>(),
						It.IsAny<string>()))
				.Returns((string)null);
			var command = new CreateAirlineAndAssignIATACodeCommandForTest(orgHeader: null, dialogService: mockedDialogService.Object);
			var assignedOrganization = command.Assign("YY", null);

			AssertNull(nameof(assignedOrganization), assignedOrganization);
		}

		public void TestAssign()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CR1";
			org.OH_IsShippingProvider = true;
			org.OH_IsAirLine = false;
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX");

			var airline = CreateANewAirline("XX");
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "07";

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var refAirlineForm = (RefAirlineForm)form;
				refAirlineForm.FireSaveButton();
				refAirlineForm.Close();
			});

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var command = new CreateAirlineAndAssignIATACodeCommandForTest(org, shouldCreateAirline: true, airline);

			var assignmentResult = command.Assign(entryView, null);

			org.Reload();
			var createdAirline = CreateAirlineAndAssignIATACodeCommand.GetAirline("XX");

			AssertEquals("Assignment should happen", true, assignmentResult);
			AssertEquals("After assignment, Carrier should be an Airline", true, org.OH_IsAirLine);
			AssertEquals("After assignment, Carrier should be assigned to an Airline profile", "07", org.MiscServ.Airline.RM_EagleAddedAirlinePrefixOrAccountingCode);

			AssertNotNull("Airline should be created", createdAirline);
			AssertEquals("Created Airline IATA Code", "XX", createdAirline.RM_TwoCharacterCode);
		}

		public void TestAssign_WhenMultipleAirlinesFound_ShouldShowErrorMessageAndStop()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CR1";
			org.OH_IsShippingProvider = true;
			org.OH_IsAirLine = false;
			Factory.Save();

			var airline1 = CreateANewAirline("XX");
			airline1.Factory.Save();
			var airline2 = CreateANewAirline("XX");
			// new values to avoid conflicts
			airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "002";
			airline2.RM_AirlineName1 = "Another name";
			airline2.Factory.Save();

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
			{
				var refAirlineForm = (RefAirlineForm)form;
				refAirlineForm.FireSaveButton();
				refAirlineForm.Close();
			});

			var command = new CreateAirlineAndAssignIATACodeCommandForTest(org, shouldCreateAirline: true, airline1);
			var entryView = CreateWiseEntryView(org, "XX");
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var assignmentResult = command.Assign(entryView, null);
			AssertEquals("Expected the assignment to fail when multiple airlines are found.", false, assignmentResult);

			AssertEquals(
				"Expected error message when multiple airlines are found.",
				"More than one airline profile with IATA Code XX found.",
				UnitTestUserNotification.Instance.LastMessage.Text
			);
		}

		public void TestAssign_WhenNewAirlineDoesNotHaveThreeNumericCode_ConfirmMessageShows_UserSelectsOKToCompleteTheAirline()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CR1";
			org.OH_IsShippingProvider = true;
			org.OH_IsAirLine = false;
			Factory.Save();

			var airline = CreateANewAirline("XX");
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = ZString.Empty;

			int formCallCount = 0;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
			{
				formCallCount++;
				if (formCallCount > 1)
				{
					// assign numeric code to airline the 2nd time showing RefAirlineForm
					airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "007";
				}
				var refAirlineForm = (RefAirlineForm)form;
				refAirlineForm.FireSaveButton();
				refAirlineForm.Close();
			});

			var command = new CreateAirlineAndAssignIATACodeCommandForTest(org, shouldCreateAirline: true, airline);
			var entryView = CreateWiseEntryView(org, "XX");
			// say OK to the warning about airline having no numeric code
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			var assignmentResult = command.Assign(entryView, null);
			AssertEquals("The assignment result should be true", true, assignmentResult);

			var expectedMessages = new[]
			{
				"No airline profile with IATA code XX is found. Would you like to create one to continue Autorating?",
				"Do you wish to edit the Airline record to assign a Numeric Code? If Cancel, Autorating will not continue.",
				"During the operation the IATA code XX will be assigned to the carrier CR1. Are you sure you want to proceed?",
				"IATA Code XX is now assigned to Carrier CR1."
			};

			var actualMessages = UnitTestUserNotification.Instance.PreviousMessages
				.Select(x => x.Text)
				.Where(x => !string.IsNullOrEmpty(x))
				.ToArray();
			AssertContainsExactElementsInAnyOrder("The messages shown should match the expected messages", expectedMessages, actualMessages);

			AssertEquals(
				"The last message text should be 'IATA Code XX is now assigned to Carrier CR1.'",
				"IATA Code XX is now assigned to Carrier CR1.",
				UnitTestUserNotification.Instance.LastMessage.Text
			);
		}

		public void TestAssign_WhenNewAirlineDoesNotHaveThreeNumericCode_ConfirmMessageShows_UserSelectsCancel()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CR1";
			org.OH_IsShippingProvider = true;
			org.OH_IsAirLine = false;
			Factory.Save();

			var entryView = CreateWiseEntryView(org, "XX");

			var airline = CreateANewAirline("XX");
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = ZString.Empty;

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
			{
				var refAirlineForm = (RefAirlineForm)form;
				refAirlineForm.FireSaveButton();
				refAirlineForm.Close();
			});

			var command = new CreateAirlineAndAssignIATACodeCommandForTest(org, shouldCreateAirline: true, airline);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			// Say Cancel to the warning about airline having no numeric code
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

			var assignmentResult = command.Assign(entryView, null);
			AssertEquals("The assignment result should be false when the airline has no numeric code", false, assignmentResult);
			AssertEquals("The airline should not be deleted when it remains", false, airline.IsDeleted);
		}

		RefAirline CreateANewAirline(string twoCharCode)
		{
			var factory = new BusinessObjectFactory();
			var airline = factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = twoCharCode;
			airline.RM_AirlineName1 = "Virgin AU";
			airline.RM_AddressLine1 = "O'riordan St";
			airline.RM_AirlineCity = "Alexandria";
			airline.RM_AirlineCountry = "Australia";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "007";

			return airline;
		}

		#endregion
	}
}
