using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWSpecialCodeCollection))]
	sealed class TWSpecialCodeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TWSpecialCodeCollection>
	{
		public void TestLoad()
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
			twSpecialCodeCollection.Load();
			AssertEquals("TWSpecialCodeCollection.Count", 2, twSpecialCodeCollection.Count);
			AssertNotNull(twSpecialCodeCollection.Cast<TWSpecialCode>().FirstOrDefault(x => x.Code == "SP99"));
			AssertNotNull(twSpecialCodeCollection.Cast<TWSpecialCode>().FirstOrDefault(x => x.Code == "NS99"));
		}

		protected override TWSpecialCodeCollection GetCollectionToTest() => new TWSpecialCodeCollection(Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection() => TWSpecialCodeTest.CreateTWSpecialCode(Factory, "XXX");
	}
}
