using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultEPaymentReference))]
	sealed class DefaultEPaymentReferenceTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestReferenceReadOnly()
		{
			var reference = CreateTestObject();

			reference.ReferenceType = EPaymentReferenceTypes.PaymentReferenceNum;
			AssertEquals(false, reference.ProviderCodeInfo.ReadOnly);
			AssertEquals(false, reference.ReferenceTypeInfo.ReadOnly);
			AssertEquals(true, reference.ReferenceInfo.ReadOnly);

			reference.ReferenceType = EPaymentReferenceTypes.FreeText;
			AssertEquals(false, reference.ProviderCodeInfo.ReadOnly);
			AssertEquals(false, reference.ReferenceTypeInfo.ReadOnly);
			AssertEquals(false, reference.ReferenceInfo.ReadOnly);
		}

		public void TestClearReferenceWhenChangeReferenceType()
		{
			var reference = CreateTestObject();

			reference.ReferenceType = EPaymentReferenceTypes.FreeText;
			reference.Reference = "Test";
			AssertNotNullOrEmpty("Pre-condition", reference.Reference);

			reference.ReferenceType = EPaymentReferenceTypes.PaymentReferenceNum;
			AssertNullOrEmpty(reference.Reference);

			reference.ReferenceType = EPaymentReferenceTypes.FreeText;
			reference.Reference = "Test";
			AssertNotNullOrEmpty("Pre-condition", reference.Reference);

			reference.ReferenceType = EPaymentReferenceTypes.InvoiceNumbers;
			AssertNullOrEmpty(reference.Reference);
		}

		public void TestPaymentProviderCodes()
		{
			var reference = CreateTestObject();
			var list = (reference.PaymentProviderCodes as CodeDescriptionPairList).Cast<CodeDescriptionPair>();

			AssertEquals(1, list.Count());
			AssertContainsExactElementsInAnyOrder(new string[] { "OFX - OFX" }, list.Select(x => x.CodeAndDescription));
		}

		public void TestPaymentReferenceTypeList()
		{
			var reference = CreateTestObject();
			var list = (reference.PaymentReferenceTypeList as CodeDescriptionPairList).Cast<CodeDescriptionPair>();

			AssertEquals(3, list.Count());
			AssertContainsExactElementsInAnyOrder(new string[] {
				"TXT - Free Text", "INV - Invoice Numbers", "PRN - Payment Reference Number" }, list.Select(x => x.CodeAndDescription));
		}

		public void TestProviderCode()
		{
			var reference = new DefaultEPaymentReference();
			AssertEquals(ZString.Empty, reference.ProviderCodeInfo.Value);

			reference.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			AssertEquals(EPaymentProviderCodes.Codes.OFX, reference.ProviderCodeInfo.Value);
		}

		public void TestReferenceType()
		{
			var reference = new DefaultEPaymentReference();
			AssertEquals(ZString.Empty, reference.ReferenceTypeInfo.Value);

			reference.ReferenceType = EPaymentReferenceTypes.FreeText;
			AssertEquals(EPaymentReferenceTypes.FreeText, reference.ReferenceTypeInfo.Value);
		}

		public void TestReference()
		{
			var reference = new DefaultEPaymentReference();
			AssertEquals(ZString.Empty, reference.ReferenceInfo.Value);

			reference.Reference = "TST";
			AssertEquals("TST", reference.ReferenceInfo.Value);
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => CreateTestObject();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => CreateTestObject();

		DefaultEPaymentReference CreateTestObject()
		{
			var defaultReason = new DefaultEPaymentReference(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			defaultReason.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			defaultReason.ReferenceType = EPaymentReferenceTypes.FreeText;
			defaultReason.Reference = "Test";
			return defaultReason;
		}
	}
}
