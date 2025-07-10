using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DefaultEPaymentReferenceValidationTest : TestCaseWithFactory
	{
		public void TestValidateReferenceType()
		{
			var referenceCollection = new DefaultEPaymentReferenceCollection();
			var reference = referenceCollection.AddNew();

			reference.ReferenceType = ZString.Empty;
			AssertHasError(reference.ReferenceTypeInfo, "Please enter a Reference Type.");

			reference.ReferenceType = "AAA";
			AssertHasError(reference.ReferenceTypeInfo, "Enter a valid Reference Type.");

			reference.ReferenceType = EPaymentReferenceTypes.FreeText;
			AssertNoErrors(reference.ReferenceTypeInfo);
		}

		public void TestValidateReference()
		{
			var referenceCollection = new DefaultEPaymentReferenceCollection();
			var reference = referenceCollection.AddNew();
			reference.ReferenceType = EPaymentReferenceTypes.FreeText;

			reference.Reference = "Test";
			AssertNoErrors(reference.ReferenceInfo);

			AssertNoExceptionThrown(() => reference.Reference = "012345678901234567890123456789");

			var ex = AssertExceptionThrown<MaxLengthExceededException>(() => reference.Reference = "012345678901234567890123456789X");
			AssertContains("The maximum length of this property is 30 characters, but 31 were entered.", ex.Message);
			ErrorReporter.Clear();
		}

		public void TestValidateProviderCode()
		{
			var referenceCollection = new DefaultEPaymentReferenceCollection();
			var reference = referenceCollection.AddNew();

			reference.ProviderCode = ZString.Empty;
			AssertHasError(reference.ProviderCodeInfo, "Please enter a Provider.");

			reference.ProviderCode = "AAA";
			AssertHasError(reference.ProviderCodeInfo, "Enter a valid Provider.");

			reference.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			AssertNoErrors(reference.ProviderCodeInfo);
		}

		public void TestDuplicateRow()
		{
			const string expectedError = "One Default Payment Reference is allowed for a Provider.";
			const string dummyProvider1 = "XXX";
			const string dummyProvider2 = "YYY";

			AssertNotEquals("Pre-condition", dummyProvider1, EPaymentProviderCodes.Codes.OFX);
			AssertNotEquals(dummyProvider2, EPaymentProviderCodes.Codes.OFX);

			var referenceCollection = new DefaultEPaymentReferenceCollection();
			var ref1 = referenceCollection.AddNew();
			var ref2 = referenceCollection.AddNew();
			var ref3 = referenceCollection.AddNew();

			ref1.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			ref1.ReferenceType = EPaymentReferenceTypes.PaymentReferenceNum;
			ref2.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			ref2.ReferenceType = EPaymentReferenceTypes.FreeText;
			ref3.ProviderCode = dummyProvider1;
			ref3.ReferenceType = EPaymentReferenceTypes.PaymentReferenceNum;
			referenceCollection.RunPreSaveValidation();

			AssertHasRowError(ref1, expectedError);
			AssertHasRowError(ref2, expectedError);
			AssertNoRowError(ref3, expectedError);

			ref2.ProviderCode = dummyProvider2;
			referenceCollection.RunPreSaveValidation();

			AssertNoRowErrors(ref1);
			AssertNoRowErrors(ref2);
			AssertNoRowErrors(ref3);
		}
	}
}
