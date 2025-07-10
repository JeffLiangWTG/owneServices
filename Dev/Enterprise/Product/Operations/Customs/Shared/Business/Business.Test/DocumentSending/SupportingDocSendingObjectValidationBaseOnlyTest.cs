using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing;

public sealed class SupportingDocSendingObjectValidationBaseOnlyTest : TestCaseWithFactory
{
	public void TestInvalidFileNameCharsForSending()
	{
		var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
		var sendingObject = SupportingDocSendingObject.New(declaration);
		AssertEquals("No invalid chars", 0, sendingObject.Validation.InvalidFileNameCharsForSending.Length);
	}

	public void TestCheckEDocFileName()
	{
		var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
		var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");

		var sendingObject = SupportingDocSendingObject.New(declaration);
		sendingObject.EDoc = eDoc.UniqueKey;
		sendingObject.Validation.ValidateEDoc();
		AssertNoError(sendingObject.EDocInfo, sendingObject.Validation.EDocContainInvalidCharError);

		var mockSendingObject = new TestSupportingDocSendingObject(declaration);
		mockSendingObject.SetShouldCheckFileNameInEdocField(true);
		mockSendingObject.EDoc = eDoc.UniqueKey;

		var mockSendingObjectValidation = new TestSupportingDocSendingObjectValidation(mockSendingObject);
		mockSendingObjectValidation.ValidateEDoc();
		AssertNoError(mockSendingObject.EDocInfo, mockSendingObject.Validation.EDocContainInvalidCharError);

		mockSendingObjectValidation = new TestSupportingDocSendingObjectValidation(mockSendingObject);
		mockSendingObjectValidation.SetInvalidFileNameCharsForSending(new[] { 'I' });
		mockSendingObjectValidation.ValidateEDoc();
		AssertHasErrorContaining(mockSendingObject.EDocInfo, mockSendingObject.Validation.EDocContainInvalidCharError);

		mockSendingObject.SetShouldCheckFileNameInEdocField(false);
		mockSendingObjectValidation.ValidateEDoc();
		AssertNoError(mockSendingObject.EDocInfo, mockSendingObject.Validation.EDocContainInvalidCharError);
	}

	public class TestSupportingDocSendingObject : SupportingDocSendingObject
	{
		public TestSupportingDocSendingObject(ISupportingDocObject supportingDocObject) : base(supportingDocObject)
		{
		}

		bool shouldCheckFileNameInEdocField;

		public override bool ShouldCheckFileNameInEdocField => shouldCheckFileNameInEdocField;

		public void SetShouldCheckFileNameInEdocField(bool value)
		{
			shouldCheckFileNameInEdocField = value;
		}
	}

	public class TestSupportingDocSendingObjectValidation : SupportingDocSendingObjectValidation
	{
		public TestSupportingDocSendingObjectValidation(AutoSupportingDocSendingObject parent) : base(parent)
		{
		}

		char[] invalidFileNameCharsForSending = System.Array.Empty<char>();

		public override char[] InvalidFileNameCharsForSending => invalidFileNameCharsForSending;

		public void SetInvalidFileNameCharsForSending(char[] value)
		{
			invalidFileNameCharsForSending = value;
		}
	}
}
