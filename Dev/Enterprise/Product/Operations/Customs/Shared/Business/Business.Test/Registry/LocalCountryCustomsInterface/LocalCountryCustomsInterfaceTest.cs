using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(LocalCountryCustomsInterface))]
	sealed class LocalCountryCustomsInterfaceTest : RegistryBusinessObjectTemplateTestCase<LocalCountryCustomsInterface>
	{
		public void TestEquals()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", new LocalCountryCustomsInterface(), new LocalCountryCustomsInterface());
				AssertNotEquals("Null", null, new LocalCountryCustomsInterface());
				AssertNotEquals("Values are all different", new LocalCountryCustomsInterface
				{
					SubmissionType = DeclarationApplicationCodeList.Codes.Builtin,
					RecipientID = "123",
					InterfaceType = LocalCountryCustomsInterfaceTypeCodeList.Codes.WiseTechCustoms
				}, new LocalCountryCustomsInterface
				{
					SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced,
					RecipientID = "456",
					InterfaceType = LocalCountryCustomsInterfaceTypeCodeList.Codes.eAdaptor
				});
				AssertEquals("Values are all same", new LocalCountryCustomsInterface
				{
					SubmissionType = DeclarationApplicationCodeList.Codes.Builtin,
					RecipientID = "123",
					InterfaceType = LocalCountryCustomsInterfaceTypeCodeList.Codes.WiseTechCustoms
				}, new LocalCountryCustomsInterface
				{
					SubmissionType = DeclarationApplicationCodeList.Codes.Builtin,
					RecipientID = "123",
					InterfaceType = LocalCountryCustomsInterfaceTypeCodeList.Codes.WiseTechCustoms
				});
			});
		}

		public void TestValidateRecipientID_RecipientIDMustBeAtLeast3Chars()
		{
			var testCompany = Factory.New<GlbCompany>();
			testCompany.GC_Code = "DE1";
			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			Factory.Save();

			CombineAssertions(() =>
			{
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
				customsInterface.RecipientID = "1";
				AssertNoError("ABMInterface not activated, CurrentFallBackLevel null, invalid RecipientID", customsInterface.RecipientIDInfo, RecipientIDMustBeAtLeast3CharsError);

				customsInterface.CurrentFallbackLevel = new FallbackLevel(testCompany, null, null);
				customsInterface.RecipientID = "1";
				AssertHasError("ABMInterface not activated, CurrentFallBackLevel set, invalid RecipientID", customsInterface.RecipientIDInfo, RecipientIDMustBeAtLeast3CharsError);

				customsInterface.RecipientID = "123";
				AssertNoError("ABMInterface not activated, CurrentFallBackLevel set, valid RecipientID", customsInterface.RecipientIDInfo, RecipientIDMustBeAtLeast3CharsError);

				CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABMInterfaceActivated");
				customsInterface.RecipientID = "1";
				AssertNoError("ABMInterface activated, CurrentFallBackLevel set, invalid RecipientID", customsInterface.RecipientIDInfo, RecipientIDMustBeAtLeast3CharsError);
			});
		}

		public void TestValidateRecipientID_MandatoryIfABMInterfaceNotActivated()
		{
			var testCompany = Factory.New<GlbCompany>();
			testCompany.GC_Code = "DE1";
			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			Factory.Save();

			CombineAssertions(() =>
			{
				customsInterface.RecipientID = string.Empty;
				AssertNoError("ABMInterface not activated, CurrentFallBackLevel null", customsInterface.RecipientIDInfo, RecipientIDMustBeAtLeast3CharsError);

				customsInterface.CurrentFallbackLevel = new FallbackLevel(testCompany, null, null);
				customsInterface.RecipientID = string.Empty;
				AssertHasError("ABMInterface not activated, CurrentFallBackLevel set", customsInterface.RecipientIDInfo, RecipientIDMustBeAtLeast3CharsError);

				CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABMInterfaceActivated");
				customsInterface.RecipientID = string.Empty;
				AssertNoError("ABMInterface Activated, CurrentFallBackLevel set", customsInterface.RecipientIDInfo, RecipientIDMustBeAtLeast3CharsError);
			});
		}

		public void TestRecipientIDMaxLength()
		{
			customsInterface.RecipientID = "123456789012345678901234567890123456";
			AssertEquals("123456789012345678901234567890123456", customsInterface.RecipientID);
		}

		public void TestRecipientID_ReadOnly()
		{
			var testCompany = Factory.New<GlbCompany>();
			testCompany.GC_Code = "DE1";
			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("ABMInterface not activated, CurrentFallBackLevel null", false, customsInterface.RecipientIDInfo.ReadOnly);

				customsInterface.CurrentFallbackLevel = new FallbackLevel(testCompany, null, null);
				AssertEquals("ABMInterface not activated, CurrentFallBackLevel set", false, customsInterface.RecipientIDInfo.ReadOnly);

				CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABMInterfaceActivated");
				AssertEquals("ABMInterface activated, CurrentFallBackLevel set", true, customsInterface.RecipientIDInfo.ReadOnly);

				CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
				AssertEquals("ABMInterface not activated, CurrentFallBackLevel set", false, customsInterface.RecipientIDInfo.ReadOnly);

				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("ABMInterface not activated, CurrentFallBackLevel set, submissionType BLT", true, customsInterface.RecipientIDInfo.ReadOnly);
				AssertEquals("ABMInterface not activated, CurrentFallBackLevel set, submissionType BLT, RecipientID should be empty", "", customsInterface.RecipientID);
			});
		}

		public void TestValidateSubmissionType()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(customsInterface.SubmissionTypeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(customsInterface.SubmissionTypeInfo, "XXX", DeclarationApplicationCodeList.Codes.Builtin);
		}

		public void TestValidateInterfaceType_DefaultValue()
		{
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals($"Default value of InterfaceType should be {LocalCountryCustomsInterfaceTypeCodeList.Codes.WiseTechCustoms}" +
			$"when SubmissionType is {DeclarationApplicationCodeList.Codes.Interfaced}",
			customsInterface.InterfaceType, LocalCountryCustomsInterfaceTypeCodeList.Codes.WiseTechCustoms);

			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals($"Default value of InterfaceType should be " + ZString.Empty +
			$"when SubmissionType is {DeclarationApplicationCodeList.Codes.Builtin}",
			customsInterface.InterfaceType, ZString.Empty);
		}

		public void TestValidateInterfaceType_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("SubmissionType is ITF", false, customsInterface.InterfaceTypeInfo.ReadOnly);
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("SubmissionType is BLT", true, customsInterface.InterfaceTypeInfo.ReadOnly);
			});
		}

		public void TestValidateInterfaceType()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(customsInterface.InterfaceTypeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(customsInterface.InterfaceTypeInfo, "XXX", LocalCountryCustomsInterfaceTypeCodeList.Codes.eAdaptor);
		}

		public void TestSubmissionTypeList()
		{
			CombineAssertions(() =>
			{
				var customsInterface = new LocalCountryCustomsInterface();
				var actualCodes = customsInterface.SubmissionTypeList.CodesAsString;

				AssertEquals("No Fallback Company - All Applies", "BTH, BIT, ITF, BLT", actualCodes);

				AssertSubmissionTypeForCountryCode("ITF Country", Core.Constants.CountryCodes.Latvia, "ITF");
				AssertSubmissionTypeForCountryCode("Integrated Country", Core.Constants.CountryCodes.SouthAfrica, "BTH, BIT, ITF, BLT");
				AssertSubmissionTypeForCountryCode("Development Country", Core.Constants.CountryCodes.Belgium, "BTH, BIT, ITF, BLT");
			});
		}

		public void TestSubmissionTypeListWhenDeclarationSubmissionProviderExists()
		{
			var mockProvider = new Mock<Integration.Customs.IDeclarationSubmissionProvider>();
			mockProvider.Setup(x => x.IsInterfaceOnlySupported()).Returns(false);

			var mockHandle = new Mock<ObjectHandle>();
			mockHandle.Setup(x => x.GetObject()).Returns(mockProvider.Object);
			var hashTable = new Hashtable
			{
				{ "AE", mockHandle.Object }
			};

			using var substitute = ObjectFactory.Substitute("DeclarationSubmissionProviders", hashTable);
			AssertSubmissionTypeForCountryCode("DeclarationSubmissionProvider exists", Core.Constants.CountryCodes.UnitedArabEmirates, "BTH, BIT, ITF, BLT");
		}

		public void TestSubmissionTypeListWhenDeclarationSubmissionProviderNotExists() => AssertSubmissionTypeForCountryCode("DeclarationSubmissionProvider not exists", Core.Constants.CountryCodes.UnitedArabEmirates, "ITF");

		void AssertSubmissionTypeForCountryCode(string countryType, string countryCode, string expectedCodes)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = $"C{countryCode}";
			company.GC_RN_NKCountryCode = countryCode;
			Factory.Save();

			var fallbackLevel = new FallbackLevel(company, null, null);
			var customsInterface = new LocalCountryCustomsInterface()
			{
				CurrentFallbackLevel = fallbackLevel
			};
			var actualCodes = customsInterface.SubmissionTypeList.CodesAsString;
			AssertEquals(countryType, expectedCodes, actualCodes);
		}

		protected override bool RequiresFallbackLevel => false;

		protected override bool RequiresFactory => false;

		protected override LocalCountryCustomsInterface GetBusinessObjectToClone()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
			return customsInterface;
		}

		protected override LocalCountryCustomsInterface GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		LocalCountryCustomsInterface customsInterface;
		protected override void SetUp()
		{
			base.SetUp();
			customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
		}

		const string RecipientIDMustBeAtLeast3CharsError = "Recipient ID must be at least 3 characters long.";
	}
}
