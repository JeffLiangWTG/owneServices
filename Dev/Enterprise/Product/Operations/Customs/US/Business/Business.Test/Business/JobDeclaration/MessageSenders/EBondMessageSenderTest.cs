using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EBondMessageSenderTest : MainMessageSenderTest
	{
		public void TestPrepare()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "Insurance Agent");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "IAA", "UnitedStates Code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var declaration = GetDeclarationForSendMessage();
			declaration.US_InsuranceAgent = string.Empty;
			declaration.US_InsuranceDisposition = string.Empty;
			declaration.US_EnableENS = false;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			declaration.ActiveEntryHeaders.RemoveAndDeleteAll();

			var messageInitiator = (SendsMessagesToCustomsShutterUpperer)declaration.MessageInitiator;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_FullName = "Test User";
			staff.GS_LoginName = "TST";
			staff.GS_WorkPhone = string.Empty;
			staff.GS_EmailAddress = string.Empty;
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var messageSender = new EBondMessageSenderForTest(declaration);

				var expectedMessage = "The bond contact details must be entered and have the following details: contact name, email and phone.";

				messageSender.Prepare();
				AssertEquals("Bond contact detail is missing email and phone details", expectedMessage, messageInitiator.Warning);
			}

			staff.GS_WorkPhone = "+610451111221";
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var messageSender = new EBondMessageSenderForTest(declaration);

				var expectedMessage = "The bond contact details must be entered and have the following details: contact name, email and phone.";

				messageSender.Prepare();
				AssertEquals("Bond contact detail is missing email and phone details", expectedMessage, messageInitiator.Warning);
			}

			staff.GS_EmailAddress = "TestUser@anc.mail";
			staff.GS_WorkPhone = string.Empty;
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var messageSender = new EBondMessageSenderForTest(declaration);

				var expectedMessage = "The bond contact details must be entered and have the following details: contact name, email and phone.";

				messageSender.Prepare();
				AssertEquals("Bond contact detail is missing email and phone details", expectedMessage, messageInitiator.Warning);
			}

			staff.GS_WorkPhone = "+610451111221";
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var messageSender = new EBondMessageSenderForTest(declaration);

				messageSender.Prepare();

				var expectedMessage = "Please choose a valid Insurance Agent.";
				AssertEquals("Should be the message as US_InsuranceAgent is empty.", expectedMessage, messageInitiator.Warning);

				declaration.US_InsuranceAgent = "IAA";

				messageSender.Prepare();
				AssertNotEquals("Should not be the message as US_InsuranceAgent is valid.", expectedMessage, messageInitiator.Warning);

				var currentCompany = GlbCompany.CurrentCompany;

				expectedMessage = $"Please create a valid eBond Insurance Agent Credential for IAA on {currentCompany.CompanyName} - Brokerage.";
				AssertEquals("Should be the message as there is no valid credential for IAA.", expectedMessage, messageInitiator.Warning);

				var companyWrapper = GlbCompanyWrapper.GetWrapper<US.Business.GlbCompanyWrapper>(currentCompany);
				var extPassword = companyWrapper.PasswordCollection.AddNew();
				extPassword.GP_MailBoxID = "IAA";
				extPassword.GP_UserID = "TST";
				extPassword.GP_CurrentPassword = "012345";

				messageSender.Prepare();
				AssertNotEquals("Should not contains the message as there is a valid credential for IAA.", expectedMessage, messageInitiator.Warning);

				expectedMessage = "Enable Entry Summary must be selected to merge.";
				AssertEquals("Should be the message as there is no valid Entry Summary for sending message.", expectedMessage, messageInitiator.Warning);

				declaration.US_EnableENS = true;

				messageSender.Prepare();
				AssertEquals("Should not be the message as Enable Entry Summary is selected.", expectedMessage, messageInitiator.Warning);
			}
		}

		public void TestNotifications()
		{
			var declaration = GetDeclarationForSendMessage();

			var messageSender = new EBondMessageSenderForTest(declaration);
			var actualMessage = messageSender.GetNotifications()[0].Message;

			var expectedMessages = new[]
			{
					"Declaration B00001001: Please add a valid processing district port code at Customs -> Country or Region Specific -> United States of America -> Import -> ABI -> Processing -> District Port.",
					"Declaration B00001001: Please add a valid entry filer code at Customs -> Country or Region Specific -> United States of America -> Import -> ABI -> Entry Filer.",
					"Bond Amount: Bond Amount cannot be zero.",
					"Tariff: Tariff may not be empty",
					"SPI: There are SPIs that are valid for the selected tariff and/or country of origin. Please review and select a SPI from the list. Select N/A(Not Applicable) in the case that SPI does not apply to the invoice line.",
					"Country Of Origin: You have not entered a value.",
					"Bond Designation Code: Please enter a designation code.",
					"Bond Producer Acc No: You have not entered a value.",
					"Surety Code: You have not entered a value.",
					"Insurance Agent: The code you have selected is not in the list.",
					"Entry Type: Entry Type is mandatory for Entry Summary.",
			};

			CombineAssertions(() =>
			{
				foreach (var message in expectedMessages)
				{
					AssertContains(message, actualMessage);
				}
			});

			AssertNotContains("Bond Designation Code: Bond Designation Code is not Basic Bond.", actualMessage);
		}

		public void TestOverriddenProperties()
		{
			var declaration = GetDeclarationForSendMessage();
			var messageSender = new EBondMessageSenderForTest(declaration);
			AssertEquals("Credit check not required for eBond", false, messageSender.IsCreditCheckRequired);
			AssertEquals("Should notify user once message is sent", true, messageSender.ShouldNotifyUserOfASuccessfulSend);
			AssertEquals("eBond Request message has been generated", messageSender.SuccessfulSendNotification);
		}

		JobDeclaration GetDeclarationForSendMessage()
		{
			DeclarationTestHelper.SetProcessingDistrictPortCode(ZString.Empty);
			DeclarationTestHelper.SetEntryFilerCode(ZString.Empty);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = DeclarationTestHelper.GetDeclarationForeBond(Factory);

			declaration.US_SuretyCode = string.Empty;
			declaration.IOROrgPK = orgHeader.PK;
			declaration.DecEntryNumber = string.Empty;
			declaration.US_EntryType = string.Empty;
			declaration.US_BondAmount = 0;
			declaration.US_BondDesignationCode = string.Empty;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = string.Empty;
			invoiceLine.US_UC_NKCountryOfOrigin = string.Empty;
			invoiceLine.US_SPI = string.Empty;

			Factory.Save();

			return declaration;
		}

		protected override void SetupAndAssertCreditOnHoldTestCase(bool isCreditCheckPerformedExternally)
		{
			var declaration = GetDeclarationForSendMessage();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.CompanyData.OB_AROnCreditHold = true;
			declaration.Importer.CompanyData.OB_IsDebtor = true;

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Harry";
			staff.GS_EmailAddress = "test@mail.com";
			staff.GS_WorkPhone = "7788990";
			staff.GS_LoginName = "TST";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "Insurance Agent");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "IAA", "UnitedStates Code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var companyWrapper = GlbCompanyWrapper.GetWrapper<US.Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany);
				var extPassword = companyWrapper.PasswordCollection.AddNew();
				extPassword.GP_MailBoxID = "IAA";
				extPassword.GP_UserID = "TST";
				extPassword.GP_CurrentPassword = "012345";

				var messageSender = new EBondMessageSenderForTest(declaration);
				var actions = messageSender.Actions;
				actions[0].US_Paid = YesNoDefaultList.Codes.Yes;
				actions[0].US_SendMessage = true;
				actions[0].US_AcknowledgeAndSign = true;
				declaration.US_InsuranceAgent = "IAA";
				Factory.Save();

				SetupMainSender(messageSender);
				AssertEquals(true, messageSender.SendMessage());

				var notifier = messageSender.MessageInitiator as SendsMessagesToCustomsShutterUpperer;
				AssertEquals("No Credit Restriction Warning Exists", null, notifier.Warning);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		sealed class EBondMessageSenderForTest : EBondMessageSender
		{
			public EBondMessageSenderForTest(JobDeclaration declaration)
				: base(declaration, ImportMessageSendingMessageType.EBondRequest, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary)
			{
			}

			public MessageSendingNotificationCollection GetNotifications()
			{
				return base.Notifications;
			}

			public new bool Prepare()
			{
				return base.Prepare();
			}

			public new bool IsCreditCheckRequired => base.IsCreditCheckRequired;

			public new bool ShouldNotifyUserOfASuccessfulSend => base.ShouldNotifyUserOfASuccessfulSend;

			public new string SuccessfulSendNotification => base.SuccessfulSendNotification;
		}
	}
}
