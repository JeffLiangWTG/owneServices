using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWSpecialCode))]
	public sealed class TWSpecialCodeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TWSpecialCode(Factory.New<ZZRefCusCodeListCombined>());
		}

		public void TestHumanReadableName()
		{
			var subLocation = CreateTWSpecialCode(Factory, "1234");
			AssertEquals("HumanReadableName", "Special Code: '1234'", subLocation.HumanReadableName);
		}

		public void TestProperties()
		{
			var twCountryCode = Core.Constants.CountryCodes.Taiwan;
			var specialCodesForExemptionOfControllingAgencies = Codes.SpecialCodesForExemptionOfControllingAgencies;
			var twca = "TWCA";
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType("TWCA", "Control Agency", twCountryCode);
			var controllingAgency = helper.CreateNewOrGetExistingCusCodeType(specialCodesForExemptionOfControllingAgencies, "Permit Exemption Codes", twCountryCode);
			controllingAgency.ZZK_IsReadonly = false;
			var controllingAgencyAttributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributes.ControllingAgency, "ControllingAgency", specialCodesForExemptionOfControllingAgencies, twCountryCode);
			controllingAgencyAttributeName.ZXE_ZZK_NKCodeTypeForValueList = twca;
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributes.Remarks, "Remarks", specialCodesForExemptionOfControllingAgencies, twCountryCode);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributes.Source, "Source", specialCodesForExemptionOfControllingAgencies, twCountryCode);
			helper.CreateCusCodeList(twCountryCode, twca, "SP", "科技部中部科學工業園區管理局", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(twCountryCode, twca, "NS", "科技部南部科學工業園區管理局", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListSP99 = helper.CreateCusCodeList(twCountryCode, specialCodesForExemptionOfControllingAgencies, "SP99", "SP99Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeListSP99.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.ControllingAgency, "SP");
			codeListSP99.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Remarks, "SPRemarks");
			codeListSP99.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Source, "SPSource");
			var codeListNS99 = helper.CreateCusCodeList(twCountryCode, specialCodesForExemptionOfControllingAgencies, "NS99", "NS99Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeListNS99.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.ControllingAgency, "NS");
			codeListNS99.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Remarks, "NSRemarks");
			codeListNS99.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Source, "NSSource");
			newFactory.Save();
			var twSpecialCodeCollection = new TWSpecialCodeCollection(Factory);
			twSpecialCodeCollection.Load(new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, "SP99"));
			AssertEquals(1, twSpecialCodeCollection.Count);
			var twSpecialCode = twSpecialCodeCollection.Cast<TWSpecialCode>().FirstOrDefault();
			AssertEquals("SP99", twSpecialCode.Code);
			AssertEquals("SP99Desc", twSpecialCode.Description);
			AssertEquals("SCECA", twSpecialCode.SC_CodeType);
			AssertEquals("Permit Exemption Codes", twSpecialCode.SC_CodeTypeDesc);
			AssertEquals("TW", twSpecialCode.SC_Country);
			AssertEquals("SP", twSpecialCode.SC_ControllingAgency);
			AssertEquals("科技部中部科學工業園區管理局", twSpecialCode.SC_ControllingAgencyDescription);
			AssertEquals("SPRemarks", twSpecialCode.SC_Remarks);
			AssertEquals("SPSource", twSpecialCode.SC_Source);
		}

		static ZZRefCusCodeListCombined CreateRefCusCodeListForTWSpecialCode(BusinessObjectFactory factory, ZString code, string description)
		{
			var cusCodeList = factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			cusCodeList.ZZD_CodeType = Codes.SpecialCodesForExemptionOfControllingAgencies;
			cusCodeList.ZZD_Code = code;
			cusCodeList.ZZD_Description = description;
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddYears(-10);
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddYears(10);
			return cusCodeList;
		}

		public static TWSpecialCode CreateTWSpecialCode(BusinessObjectFactory factory, ZString code, string description = "")
		{
			return new TWSpecialCode(CreateRefCusCodeListForTWSpecialCode(factory, code, description));
		}
	}
}
