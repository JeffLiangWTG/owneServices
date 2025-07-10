using System;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(MessagePreSendingValidation))]
sealed class MessagePreSendingValidationTest : MessagePreSendingValidationBaseTest<MessagePreSendingValidation>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new MessagePreSendingValidation(null));
	}

	public void TestValidate_MissingPhoneNumber_WorkPhone()
	{
		void SetupWorkPhoneData(GlbStaff user, string phoneNumber, bool publish)
		{
			user.GS_WorkPhone = phoneNumber;
			user.GS_PublishWorkPhone = publish;
		}

		TestValidate_MissingPhoneNumber(SetupWorkPhoneData);
	}

	public void TestValidate_MissingPhoneNumber_HomePhone()
	{
		void SetupHomePhoneData(GlbStaff user, string phoneNumber, bool publish)
		{
			user.GS_HomePhone = phoneNumber;
			user.GS_PublishHomePhone = publish;
		}

		TestValidate_MissingPhoneNumber(SetupHomePhoneData);
	}

	public void TestValidate_MissingPhoneNumber_MobilePhone()
	{
		void SetupMobilePhoneData(GlbStaff user, string phoneNumber, bool publish)
		{
			user.GS_MobilePhone = phoneNumber;
			user.GS_PublishMobilePhone = publish;
		}

		TestValidate_MissingPhoneNumber(SetupMobilePhoneData);
	}

	void TestValidate_MissingPhoneNumber(Action<GlbStaff, string, bool> setupPhoneData)
	{
		const string validNumber = "12345";
		const string emptyNumber = null;
		const bool isPublished = true;
		const bool isHidden = false;
		const string errorMessage = "The current user does not have any Phone number marked for publication in Staff data.";

		SetCredentials();

		TestPreSendErrorMessageForPhoneNumber(
			isImport: false,
			testCases:
			[
				(validNumber, isHidden, false),
				(validNumber, isPublished, false),
				(emptyNumber, isHidden, false)
			]);

		TestPreSendErrorMessageForPhoneNumber(
			isImport: true,
			testCases:
			[
				(validNumber, isHidden, true),
				(validNumber, isPublished, false),
				(emptyNumber, isHidden, true)
			]);

		void TestPreSendErrorMessageForPhoneNumber(bool isImport, (string, bool, bool)[] testCases)
		{
			declaration.JE_MessageType = isImport ? MessageTypeList.Codes.Import : MessageTypeList.Codes.Export;
			var validation = GetInstanceForTest();

			var user = GlbStaff.CurrentUser;

			CombineAssertions(isImport ? "Import" : "Export", () =>
			{
				foreach (var (phoneNumber, publish, shouldContainError) in testCases)
				{
					setupPhoneData(user, phoneNumber, publish);

					var notifications = new NotificationCollection();
					validation.Validate(notifications);

					var description = $"Phone number is {(publish ? "" : "not ")}published and {(string.IsNullOrEmpty(phoneNumber) ? "" : "not ")}empty";
					if (shouldContainError)
					{
						Assert($"{description}, Notification: exists", notifications.Contains(errorMessage));
					}
					else
					{
						Assert($"{description}, Notification: does not exist", !notifications.Contains(errorMessage));
					}
				}
			});
		}
	}

	public void TestValidate_MissingData()
	{
		const string expectedErrorMessage = "Declaration needs at least one of each 'Entry Instruction', 'Invoice Header', 'Invoice Line'.";
		SetCredentials();
		declaration = Factory.New<JobDeclaration>();
		var validation = GetInstanceForTest();
		var notifications = new NotificationCollection();

		CombineAssertions(() =>
		{
			validation.Validate(notifications);
			Assert("Missing Entry Instruction", notifications.Contains(expectedErrorMessage));

			notifications.Clear();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			validation.Validate(notifications);
			Assert("Missing Invoice", notifications.Contains(expectedErrorMessage));

			notifications.Clear();
			var invoice = declaration.Invoices.AddNew();
			validation.Validate(notifications);
			Assert("Missing Invoice Line", notifications.Contains(expectedErrorMessage));

			notifications.Clear();
			invoice.InvoiceLines.AddNew().JI_CEI = instruction.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			validation.Validate(notifications);
			Assert("Declaration contains 1 of each : Entry Instruction, Invoice, Invoice Line", !notifications.Contains(expectedErrorMessage));
		});
	}

	public void TestValidate_NotMerged()
	{
		const string errorMessage = "Declaration lacks entries.";
		SetCredentials();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew().JI_CEI = instruction.PK;
		var validation = GetInstanceForTest();
		var notifications = new NotificationCollection();

		CombineAssertions(() =>
		{
			validation.Validate(notifications);
			Assert("Not Merged", notifications.Contains(errorMessage));

			notifications.Clear();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Assert("Merged", !notifications.Contains(errorMessage));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew().JI_CEI = instruction.PK;
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
	}

	JobDeclaration declaration;

	protected override MessagePreSendingValidation GetInstanceForTest() => new(declaration);
}
