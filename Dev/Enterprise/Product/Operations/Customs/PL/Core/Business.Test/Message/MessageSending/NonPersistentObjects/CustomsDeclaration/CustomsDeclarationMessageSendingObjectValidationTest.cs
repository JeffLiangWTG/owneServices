using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CustomsDeclarationMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAction()
	{
		var sendingObjectParent = new BaseMessageSendingObjectParent(Factory.New<JobDeclaration>());
		var sendingObject = new CustomsDeclarationMessageSendingObject(Factory.New<CusEntryHeader>());
		var propertyInfo = sendingObject.ActionInfo;

		CombineAssertions(() =>
		{
			sendingObject.Action = ZString.Empty;
			AssertHasErrorContaining("Empty", propertyInfo, MandatoryValidation.MustBeEntered);

			sendingObject.Action = "ASD";
			AssertNoErrorContaining("Not empty", propertyInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining("Invalid code", propertyInfo, ListValidation.InvalidCodeError);

			sendingObject.Action = Constants.MessageSendingObjectActionCodes.ZC415;
			AssertNoErrorContaining("Valid input", propertyInfo, ListValidation.InvalidCodeError);
		});
	}

	public void TestCheckEntryNumberIE513()
	{
		var sendingObjectParent = new BaseMessageSendingObjectParent(Factory.New<JobDeclaration>());
		var sendingObject = new CustomsDeclarationMessageSendingObject(Factory.New<CusEntryHeader>());
		var propertyInfo = sendingObject.EntryNumberInfo;
		const string messageError = "Enter the 18 char long MRN number of declaration being amended.";

		CombineAssertions(() =>
		{
			sendingObject.Action = Constants.MessageSendingObjectActionCodes.CC513;
			sendingObject.EntryNumber = ZString.Empty;
			AssertHasError("Empty", propertyInfo, messageError);

			sendingObject.EntryNumber = "SD";
			AssertHasError("The EntryNumber length is less than 18", propertyInfo, messageError);

			sendingObject.EntryNumber = "ABCDEFGHIJKLMNOPQRQQQ";
			AssertHasError("The EntryNumber length is more than 18", propertyInfo, messageError);

			sendingObject.EntryNumber = "ABCDEFGHIJKLMNOPQR";
			AssertNoError("The EntryNumber length is 18", propertyInfo, messageError);
		});
	}

	public void TestCheckEntryNumber()
	{
		var sendingObject = new CustomsDeclarationMessageSendingObject(Factory.New<CusEntryHeader>());
		var propertyInfo = sendingObject.EntryNumberInfo;
		const string messageError = "Enter the MRN number of declaration being amended.";

		CombineAssertions(() =>
		{
			sendingObject.Action = Constants.MessageSendingObjectActionCodes.CC583;
			sendingObject.EntryNumber = ZString.Empty;
			AssertHasError("Empty", propertyInfo, messageError);

			sendingObject.EntryNumber = "ABCDEFGHIJKLMNOPQR";
			AssertNoError("The EntryNumber is not null", propertyInfo, messageError);

			const string actionThatDoesNotRequireEntryNum = Constants.MessageSendingObjectActionCodes.CC513;
			sendingObject.Action = actionThatDoesNotRequireEntryNum;
			sendingObject.EntryNumber = ZString.Empty;
			AssertNoError("MRN is not required: Empty", propertyInfo, messageError);
		});
	}
}
