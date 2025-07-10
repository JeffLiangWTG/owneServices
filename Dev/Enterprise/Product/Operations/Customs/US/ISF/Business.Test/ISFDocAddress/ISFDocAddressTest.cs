using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFDocAddress))]
	sealed class ISFDocAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsSocialSecurityNumberGovRegNumType()
		{
			var docAddress = Factory.New<ISFDocAddress>();
			docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			AssertEquals(false, docAddress.IsSocialSecurityNumberGovRegNumType);
			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			AssertEquals(true, docAddress.IsSocialSecurityNumberGovRegNumType);
		}

		public void TestE2_SocialSecurityNumberDetails()
		{
			var docAddress = Factory.New<ISFDocAddress>();
			docAddress.E2_AddressOverride = true;

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			AssertEquals("NOT SPECIFIED DOB:", docAddress.E2_SocialSecurityNumberDetails);
			docAddress.E2_SocialSecurityNumber = "996-99-5864";
			AssertEquals("996-99-5864 DOB:", docAddress.E2_SocialSecurityNumberDetails);
			docAddress.E2_SocialSecurityNumberDateOfBirth = new ZDate(1985, 3, 25);
			AssertEquals("996-99-5864 DOB:25MAR1985", docAddress.E2_SocialSecurityNumberDetails);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			docAddress.E2_SocialSecurityNumber = "996-99-5864";
			docAddress.E2_SocialSecurityNumberDateOfBirth = ZDate.Empty;
			AssertEquals("***-**-**** DOB:", docAddress.E2_SocialSecurityNumberDetails);

			docAddress.E2_SocialSecurityNumberDateOfBirth = new ZDate(1985, 3, 25);
			AssertEquals("***-**-**** DOB:25MAR1985", docAddress.E2_SocialSecurityNumberDetails);
		}

		public void TestSocialSecurityNumberData()
		{
			var docAddress = Factory.New<ISFDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			docAddress.E2_GovRegNum = "";
			AssertEquals(ZString.Empty, docAddress.E2_SocialSecurityNumber);
			AssertEquals(ZDateTime.Empty, docAddress.E2_SocialSecurityNumberDateOfBirth);
			docAddress.E2_GovRegNum = "996-99-586425MAR1980";
			AssertEquals(ZString.Empty, docAddress.E2_SocialSecurityNumber);
			AssertEquals(ZDateTime.Empty, docAddress.E2_SocialSecurityNumberDateOfBirth);
			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			AssertEquals(ZString.Empty, docAddress.E2_SocialSecurityNumber);

			docAddress.E2_GovRegNum = "996-99-586425MAR1980";
			AssertEquals("996-99-5864", docAddress.E2_SocialSecurityNumber);
			AssertEquals(new ZDateTime(1980, 3, 25), docAddress.E2_SocialSecurityNumberDateOfBirth);
		}

		public void TestSocialSecurityNumberForDisplay()
		{
			var docAddress = Factory.New<ISFDocAddress>();
			docAddress.E2_AddressOverride = true;

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			docAddress.E2_GovRegNum = "";
			AssertEquals(ZString.Empty, docAddress.SocialSecurityNumberForDisplay);
			docAddress.E2_GovRegNum = "996-99-586425MAR1980";
			AssertEquals(ZString.Empty, docAddress.SocialSecurityNumberForDisplay);
			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			AssertEquals(ZString.Empty, docAddress.SocialSecurityNumberForDisplay);
			docAddress.E2_GovRegNum = "123-45-6789";
			AssertEquals("123-45-6789", docAddress.SocialSecurityNumberForDisplay);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			docAddress.E2_GovRegNum = "996-99-586425MAR1980";
			AssertEquals("***-**-****", docAddress.SocialSecurityNumberForDisplay);
			docAddress.E2_GovRegNum = "123-45-6789";
			AssertEquals("***-**-****", docAddress.SocialSecurityNumberForDisplay);
			docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			AssertEquals(ZString.Empty, docAddress.SocialSecurityNumberForDisplay);
		}

		public void TestSocialSecurityNumberForDisplay_ReadOnly()
		{
			var docAddress = Factory.New<ISFDocAddress>();
			docAddress.E2_AddressOverride = true;

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			docAddress.E2_GovRegNum = "";
			AssertEquals(true, docAddress.SocialSecurityNumberForDisplayInfo.ReadOnly);
			docAddress.E2_GovRegNum = "996-99-586425MAR1980";
			AssertEquals(true, docAddress.SocialSecurityNumberForDisplayInfo.ReadOnly);
			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			AssertEquals(false, docAddress.SocialSecurityNumberForDisplayInfo.ReadOnly);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			docAddress.E2_GovRegNum = "996-99-586425MAR1980";
			AssertEquals(true, docAddress.SocialSecurityNumberForDisplayInfo.ReadOnly);
		}

		public void TestSocialSecurityOrGovRegNum()
		{
			var docAddress = Factory.New<ISFDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.TaxIDNumber;
			docAddress.E2_GovRegNum = "123-12-1234";
			AssertEquals("123-12-1234", docAddress.E2_SocialSecurityNumberOrGovRegNum);
			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			docAddress.E2_SocialSecurityNumber = "996-99-5864";
			docAddress.E2_SocialSecurityNumberDateOfBirth = new ZDate(1985, 3, 25);
			AssertEquals("996-99-5864 DOB:25MAR1985", docAddress.E2_SocialSecurityNumberOrGovRegNum);
		}

		public void TestCodeAndDescriptionAttributes()
		{
			AssertEquals("E2_CompanyName", CodePropertyAttribute.CodePropertyNameFromType(typeof(ISFDocAddress)));
			AssertEquals("E2_ShortAddress", DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeof(ISFDocAddress)));
		}

		public void TestAddress1ShownIfNoOrg()
		{
			var factory = new BusinessObjectFactory();
			var iSFAddress = factory.New<ISFDocAddress>();
			iSFAddress.E2_Address1 = "Address 1";
			AssertEquals("Address 1", iSFAddress.E2_ShortAddress);
		}

		public void TestAddress1ShownIfNoShortAddress()
		{
			var factory = new BusinessObjectFactory();
			var orgAddress = factory.New<OrgAddress>();
			orgAddress.OA_Code = "";
			var iSFAddress = factory.New<ISFDocAddress>();
			iSFAddress.E2_Address1 = "Address 1";
			AssertEquals("Address 1", iSFAddress.E2_ShortAddress);
		}

		public void TestShortAddressShown()
		{
			var factory = new BusinessObjectFactory();
			var orgAddress = factory.New<OrgAddress>();
			orgAddress.OA_Code = "Short Address";
			var iSFAddress = factory.New<ISFDocAddress>();
			iSFAddress.E2_Address1 = "Address 1";
			iSFAddress.E2_OA_Address = orgAddress.PK;
			AssertEquals("Short Address", iSFAddress.E2_ShortAddress);
		}

		public void TestRegNumForDucument()
		{
			var docAddress = Factory.New<ISFDocAddress>();
			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			docAddress.E2_GovRegNum = "123-45-7890";
			AssertEquals(ZString.Empty, docAddress.RegTypeAndNumForDocument);

			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			docAddress.E2_GovRegNum = "58-123456789";
			AssertEquals("EIN: 58-123456789", docAddress.RegTypeAndNumForDocument);

			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			docAddress.E2_GovRegNum = "111-23-2121";
			AssertEquals("CBN: 111-23-2121", docAddress.RegTypeAndNumForDocument);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (ISFDocAddress)base.GetNewBusinessObjectForDeleteTest(factory);
			result.E2_AddressOverride = true;
			return result;
		}
	}
}
