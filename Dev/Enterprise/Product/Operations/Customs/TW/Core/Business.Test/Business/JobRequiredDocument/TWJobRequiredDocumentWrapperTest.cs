using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWJobRequiredDocumentWrapper))]
	sealed class TWJobRequiredDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TWJobRequiredDocumentWrapper(orgHeader, requiredDocument);
		}

		public void TestArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TWJobRequiredDocumentWrapper(null, requiredDocument));
			AssertNoExceptionThrown(() => new TWJobRequiredDocumentWrapper(orgHeader, requiredDocument));
		}

		public void TestBoxNumber()
		{
			var wrapper = GetNewBusinessObject() as TWJobRequiredDocumentWrapper;
			AssertEquals("111", wrapper.BoxNumber);
		}

		public void TestDefaultOrg()
		{
			var wrapper = GetNewBusinessObject() as TWJobRequiredDocumentWrapper;
			AssertEquals("Proxy Org Name", wrapper.DefaultOrg.OH_FullName);
		}

		public void TestControlledPremisesID()
		{
			var wrapper = GetNewBusinessObject() as TWJobRequiredDocumentWrapper;
			AssertEquals("CCP01", wrapper.ControlledPremisesID);
		}

		public void TestAddresses()
		{
			var wrapper = GetNewBusinessObject() as TWJobRequiredDocumentWrapper;
			AssertEquals("10000台北巿台湾出口區園東街10號", wrapper.OrgAddressFormat);
			AssertEquals("10093臺北巿臺北代理出口區園東街6號", wrapper.DefaultOrgAddressFormat);
		}

		public void TestCustomsDistrictDescription()
		{
			var wrapper = GetNewBusinessObject() as TWJobRequiredDocumentWrapper;
			AssertEquals("基隆", wrapper.CustomsDistrictDescription);
		}

		public void TestCompanyName()
		{
			var wrapper = GetNewBusinessObject() as TWJobRequiredDocumentWrapper;
			AssertEquals("台湾股份有限公司", wrapper.OrgCompanyName);
			AssertEquals("代理股份有限公司", wrapper.DefaultOrgCompanyName);
		}

		public void TestUniformNumber()
		{
			var wrapper = GetNewBusinessObject() as TWJobRequiredDocumentWrapper;
			AssertEquals("", wrapper.UniformNumber);
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.PID, "PID01", "TW");
			AssertEquals("PID01", wrapper.UniformNumber);
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS01", "TW");
			AssertEquals("PAS01", wrapper.UniformNumber);
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT01", "TW");
			AssertEquals("VAT01", wrapper.UniformNumber);
		}

		public void TestPhone()
		{
			var wrapper = GetNewBusinessObject() as TWJobRequiredDocumentWrapper;
			AssertEquals("01", wrapper.OrgPhonePrefix);
			AssertEquals("PHONE1", wrapper.OrgPhone);
			AssertEquals("0t", wrapper.DefaultOrgPhonePrefix);
			AssertEquals(" PHONE", wrapper.DefaultOrgPhone);
			orgHeader.MainAddress.OA_Phone = "";
			AssertEquals("", wrapper.OrgPhonePrefix);
			AssertEquals("", wrapper.OrgPhone);
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateAndSetProxyOrganization();
			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Test tw";
			orgHeader.OH_RL_NKClosestPort = "TWTPE";
			orgHeader.MainAddress.Address1 = "Unit 2020";
			orgHeader.MainAddress.Address2 = "TAI WANG 110";
			orgHeader.MainAddress.City = "TPE";
			orgHeader.MainAddress.Postcode = "2020";
			orgHeader.MainAddress.OA_Phone = "00001PHONE1";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "TW";
			orgHeader.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("CCP", "CCP01", "TW");
			var zhTWtranslatedAddress1 = orgHeader.MainAddress.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "台湾股份有限公司";
			zhTWtranslatedAddress1.OTA_Address1 = "台湾出口區園東街10號";
			zhTWtranslatedAddress1.OTA_Address2 = string.Empty;
			zhTWtranslatedAddress1.OTA_City = "台北巿";
			zhTWtranslatedAddress1.OTA_PostCode = "10000";
			zhTWtranslatedAddress1.ClosestPort = "TW";
			requiredDocument = orgHeader.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocUsage = "BRK";
			requiredDocument.EQ_DocType = "POA";
			requiredDocument.EQ_RN_NKRelatedCountry = "TW";
			requiredDocument.Attributes.DeleteAll();
			var attrib = requiredDocument.Attributes.AddNew();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			attrib.D0_AttribDisplayValue = "A";
			attrib = requiredDocument.Attributes.AddNew();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
			attrib.D0_AttribDisplayValue = "111";
		}

		OrgHeader orgHeader;
		JobRequiredDocument requiredDocument;
	}
}
