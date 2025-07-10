using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class RetrospectiveQuotaRequestMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAction()
	{
		var sendingObjectParent = new BaseMessageSendingObjectParent(Factory.New<JobDeclaration>());
		var sendingObject = new RetrospectiveQuotaRequestMessageSendingObject(Factory.New<CusEntryHeader>());
		var propertyInfo = sendingObject.ActionInfo;

		CombineAssertions(() =>
		{
			sendingObject.Action = ZString.Empty;
			AssertHasErrorContaining("Empty", propertyInfo, MandatoryValidation.MustBeEntered);

			sendingObject.Action = "ASD";
			AssertNoErrorContaining("Not empty", propertyInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining("Invalid code", propertyInfo, ListValidation.InvalidCodeError);

			sendingObject.Action = RetrospectiveQuotaRequestMessageSendingObject.PLRetrospectiveSchema.ActionType;
			AssertNoErrorContaining("Valid input", propertyInfo, ListValidation.InvalidCodeError);
		});
	}

	public void TestCheckEntryNumber()
	{
		var sendingObjectParent = new BaseMessageSendingObjectParent(Factory.New<JobDeclaration>());
		var sendingObject = new RetrospectiveQuotaRequestMessageSendingObject(Factory.New<CusEntryHeader>());

		CombineAssertions(() =>
		{
			sendingObject.EntryNumber = ZString.Empty;
			AssertHasErrorContaining("Empty", sendingObject.EntryNumberInfo, MandatoryValidation.MustBeEntered);

			sendingObject.EntryNumber = "123";
			AssertNoErrorContaining("Not empty", sendingObject.EntryNumberInfo, MandatoryValidation.MustBeEntered);
		});
	}
}
