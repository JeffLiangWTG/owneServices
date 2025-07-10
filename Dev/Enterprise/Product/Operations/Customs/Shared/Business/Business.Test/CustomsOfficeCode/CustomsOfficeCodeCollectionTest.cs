using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CustomsOfficeCodeCollection))]
	public sealed class CustomsOfficeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGBOffices()
		{
			Factory.Save();
			var collection = new CustomsOfficeCodeCollection(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			collection.Load();
			var expectedOfficeCodes = new ZString[] { "DE004323", "DE003478" };
			AssertContainsExactElementsInAnyOrder(expectedOfficeCodes, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestLocalCountryOnlyCustomsOfficesWithRequiredRoles()
		{
			Factory.Save();
			var collectionWithoutRolesSpecified = CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Core.Constants.CountryCodes.Italy);
			collectionWithoutRolesSpecified.Load();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Confirm more than one office exists for Country", new[] { "IT008734", "IT009278" }, collectionWithoutRolesSpecified.Select(x => x.ZZD_Code));

				var collectionWithRolesSpecified = CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Core.Constants.CountryCodes.Italy, CustomsOfficeCodeTestHelper.EoriRegistrationAuthoritiesCodeType);
				collectionWithRolesSpecified.Load();
				AssertContainsExactElementsInAnyOrder("Specify roles", new[] { "IT009278" }, collectionWithRolesSpecified.Select(x => x.ZZD_Code));
			});
		}

		public void TestLocalCountryOnlyCustomsOfficesWithRequiredRoles_FilterBusinessObjectDefaults_CountryOrGrouping()
		{
			Factory.Save();
			var collection = CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Core.Constants.CountryCodes.Italy);
			AssertEquals(Core.Constants.CountryCodes.Italy, collection.FilterBusinessObjectDefaults[$"{Constants.ZZRefCusCodeListFilters.CountryOrGrouping}:Property"].Value);

			collection = CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, new ZString[] { Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes }, CustomsOfficeCodeTestHelper.EoriRegistrationAuthoritiesCodeType);
			AssertEquals(Core.Constants.CountryCodes.Italy, collection.FilterBusinessObjectDefaults[$"{Constants.ZZRefCusCodeListFilters.CountryOrGrouping}:Property"].Value);
			AssertEquals(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, collection.FilterBusinessObjectDefaults[$"{Constants.ZZRefCusCodeListFilters.CountryOrGrouping}:Property:1"].Value);
		}

		public void TestLocalCountryOnlyCustomsOfficesWithRequiredAttributes_SingleDataGroupingCode()
		{
			var cusofIT2 = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT009279", "ITALIAN OFFICE2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, CustomsOfficeCodeTestHelper.EoriRegistrationAuthoritiesCodeType);
			cusofIT2.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, bool.FalseString);

			var cusofIT3 = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT009280", "ITALIAN OFFICE3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, CustomsOfficeCodeTestHelper.EoriRegistrationAuthoritiesCodeType);
			cusofIT3.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, bool.TrueString);

			var cusofIT4 = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT009281", "ITALIAN OFFICE4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, CustomsOfficeCodeTestHelper.EoriRegistrationAuthoritiesCodeType);
			cusofIT4.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, bool.TrueString);
			Factory.Save();

			var attributesFilter = new Dictionary<ZString, ZString[]>
			{
				{ RefCusCodeListAttributeTypes.Codes.ROLE, new ZString[] { CustomsOfficeCodeTestHelper.EoriRegistrationAuthoritiesCodeType, CustomsOfficeCodeTestHelper.CompetentAuthorityOfEnquiryCodeType } },
				{ RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, new ZString[] { bool.TrueString } }
			};

			var zzCodeList1 = Factory.Load<ZZRefCusCodeListCombined>(customsOfficeTestHelper.CusofIT.PK);
			var zzCodeList2 = Factory.Load<ZZRefCusCodeListCombined>(cusofIT2.PK);
			var zzCodeList3 = Factory.Load<ZZRefCusCodeListCombined>(cusofIT3.PK);
			var zzCodeList4 = Factory.Load<ZZRefCusCodeListCombined>(cusofIT4.PK);
			var completeFilter = CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredAttributes(Factory, Core.Constants.CountryCodes.Italy, attributesFilter).CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals("no main office, has 'REG' role", false, zzCodeList1.MatchesFilter(completeFilter));
				AssertEquals("has main office with invalid value, has 'REG' role", false, zzCodeList2.MatchesFilter(completeFilter));
				AssertEquals("has main office with valid value, has 'REG' role", true, zzCodeList3.MatchesFilter(completeFilter));
				AssertEquals("has main office with valid value, has 'ENQ' role", true, zzCodeList4.MatchesFilter(completeFilter));
			});
		}

		public void TestLocalCountryOnlyCustomsOfficesWithRequiredAttributes_MultipleDataGroupingCode()
		{
			customsOfficeTestHelper.CusofIT.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, bool.TrueString);
			customsOfficeTestHelper.CusofDE.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, bool.TrueString);
			var cusofXI = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XI009278", "XI OFFICE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, CustomsOfficeCodeTestHelper.EoriRegistrationAuthoritiesCodeType);
			cusofXI.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, bool.TrueString);
			Factory.Save();

			var attributesFilter = new Dictionary<ZString, ZString[]>
			{
				{ RefCusCodeListAttributeTypes.Codes.ROLE, new ZString[] { CustomsOfficeCodeTestHelper.EoriRegistrationAuthoritiesCodeType, CustomsOfficeCodeTestHelper.CompetentAuthorityOfEnquiryCodeType } },
				{ RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, new ZString[] { bool.TrueString } }
			};

			var zzCodeList1 = Factory.Load<ZZRefCusCodeListCombined>(customsOfficeTestHelper.CusofIT.PK);
			var zzCodeList2 = Factory.Load<ZZRefCusCodeListCombined>(customsOfficeTestHelper.CusofDE.PK);
			var zzCodeList3 = Factory.Load<ZZRefCusCodeListCombined>(cusofXI.PK);
			var completeFilter = CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredAttributes(Factory, new ZString[] { Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.Germany }, attributesFilter).CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals("IT", true, zzCodeList1.MatchesFilter(completeFilter));
				AssertEquals("DE", true, zzCodeList2.MatchesFilter(completeFilter));
				AssertEquals("XI", false, zzCodeList3.MatchesFilter(completeFilter));
			});
		}

		public void TestGetCachedCollection()
		{
			Factory.Save();
			var zzCodeList1 = Factory.Load<ZZRefCusCodeListCombined>(customsOfficeTestHelper.CusofDE.PK);
			var zzCodeList2 = Factory.Load<ZZRefCusCodeListCombined>(customsOfficeTestHelper.CusofIT.PK);
			var list = CustomsOfficeCodeCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Germany, ZDateTime.Today);
			var completeFilter = list.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals("zzCodeList1, correct country", true, zzCodeList1.MatchesFilter(completeFilter));
				AssertEquals("zzCodeList2, invalid country", false, zzCodeList2.MatchesFilter(completeFilter));
				AssertSame("Cached", list, CustomsOfficeCodeCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Germany, ZDateTime.Today));
			});
		}

		public void TestGetCachedCollectionWithAtrributesFilter()
		{
			Factory.Save();
			var zzCodeList1 = Factory.Load<ZZRefCusCodeListCombined>(customsOfficeTestHelper.CusofDE.PK);
			var zzCodeList2 = Factory.Load<ZZRefCusCodeListCombined>(customsOfficeTestHelper.CusofIT.PK);
			var attributesFilter = new Dictionary<ZString, ZString[]>
			{
				{ RefCusCodeListAttributeTypes.Codes.ROLE, new ZString[] { CustomsOfficeCodeTestHelper.CompetentAuthorityOfEnquiryCodeType } }
			};
			var list = CustomsOfficeCodeCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Germany, ZDateTime.Today, attributesFilter);
			var completeFilter = list.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals("zzCodeList1, correct cusof", true, zzCodeList1.MatchesFilter(completeFilter));
				AssertEquals("zzCodeList2, invalid cusof", false, zzCodeList2.MatchesFilter(completeFilter));
				AssertSame("Cached", list, CustomsOfficeCodeCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Germany, ZDateTime.Today, attributesFilter));
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CustomsOfficeCodeCollection(Factory, GlbCompany.CurrentCompany.Country.Code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);

		protected override Type GetExpectedCollectionType() => typeof(CustomsOfficeCodeCollection);

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			customsOfficeTestHelper = new CustomsOfficeCodeTestHelper(helper);
		}

		UniversalReferenceTestDataHelper helper;
		CustomsOfficeCodeTestHelper customsOfficeTestHelper;
	}
}
