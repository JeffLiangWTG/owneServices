using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using static Enterprise.Freight.Integration.Forwarding;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCountryData))]
	public class OrgCountryDataTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCountriesWithAviationSecuritySchemePerAddress()
		{
			AssertCurrentCompanyIsInCountryWithAviationSecuritySchemePerAddress("AU", true);
			AssertCurrentCompanyIsInCountryWithAviationSecuritySchemePerAddress("US", true);
			AssertCurrentCompanyIsInCountryWithAviationSecuritySchemePerAddress("JP", true);
			AssertCurrentCompanyIsInCountryWithAviationSecuritySchemePerAddress("HK", true);
			AssertCurrentCompanyIsInCountryWithAviationSecuritySchemePerAddress("SG", true);

			AssertCurrentCompanyIsInCountryWithAviationSecuritySchemePerAddress("NZ", false);

			AssertCurrentCompanyIsInCountryWithAviationSecuritySchemePerAddress("IT", true);
			AssertCurrentCompanyIsInCountryWithAviationSecuritySchemePerAddress("DE", true);
			AssertCurrentCompanyIsInCountryWithAviationSecuritySchemePerAddress("ES", true);
			AssertCurrentCompanyIsInCountryWithAviationSecuritySchemePerAddress("GB", true);
		}

		void AssertCurrentCompanyIsInCountryWithAviationSecuritySchemePerAddress(ZString country, bool expectedResult)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				AssertEquals(country, expectedResult, ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>().GetConfiguration().IsAddressLevelScheme);
			}
		}

		public void TestImpAddInfo()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = organisation.PK;
			countryData.OV_RN_NKClientCountryRelation = Constants.CountryCodes.UnitedStates;
			var impAddInfo = countryData.ImpAddInfo;
			AssertEquals(ObjectFactory.GetType<US.IOrgImpAddInfo>(), impAddInfo.GetType());
			AssertSame(impAddInfo, countryData.ImpAddInfo);
			AssertEquals(true, countryData.IsRegisteredEditableChildObject(impAddInfo));

			countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = organisation.PK;
			countryData.OV_RN_NKClientCountryRelation = Constants.CountryCodes._TemplateCountryName_;
			AssertExceptionThrown<NotSupportedException>(() => countryData.ImpAddInfo.GetType());
		}

		public void TestReadOnlySecurity()
		{
			const string countryWithoutAviationSecurityScheme = "MY";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryWithoutAviationSecurityScheme))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var data = org.CountryDataCollectionForThisCompany.AddNew();
				data.FillWithValidTestData();
				data.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;
				Factory.Save();

				bool oldValue = Env.Security.OrgConsignorModifyExporterScheme.IsAllowed;
				try
				{
					Env.Security.OrgConsignorModifyExporterScheme.IsAllowed = true;

					Assert("Access Allowed - Not ReadOnly", !data.OV_EXApprovalMethodInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_EXApprovalNumberInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_EXApprovedOrMajorExporterInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_EXExportPermissionDetailsInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_EXPermitNumberInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_CustomsEconomicGroupAddInfoInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_EXSiteInspectionDateInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_GS_NKReviewedByUserInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_ImportCustomsDefaultAddInfoInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_ImportEntryPaymentPreferenceInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_ImportQuarantinePaymentPreferenceInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_LastReviewedOnInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_OA_ApprovedLocationInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_OH_OrgHeaderInfo.ReadOnly);
					Assert("Access Allowed - Not ReadOnly", !data.OV_RN_NKClientCountryRelationInfo.ReadOnly);

					Assert("Always ReadOnly", data.OV_SystemCreateTimeUtcInfo.ReadOnly);
					Assert("Always ReadOnly", data.OV_SystemCreateUserInfo.ReadOnly);
					Assert("Always ReadOnly", data.OV_SystemLastEditTimeUtcInfo.ReadOnly);
					Assert("Always ReadOnly", data.OV_SystemLastEditUserInfo.ReadOnly);
				}
				finally
				{
					Env.Security.OrgConsignorModifyExporterScheme.IsAllowed = oldValue;
				}

				org = Factory.NewWithValidTestData<OrgHeader>();
				data = org.CountryDataCollectionForThisCompany.AddNew();
				data.FillWithValidTestData();
				Factory.Save();

				oldValue = Env.Security.OrgConsignorModifyExporterScheme.IsAllowed;
				try
				{
					Env.Security.OrgConsignorModifyExporterScheme.IsAllowed = false;

					Assert("Access Not Allowed - ReadOnly", data.OV_EXApprovalMethodInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_EXApprovalNumberInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_EXApprovedOrMajorExporterInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_EXExportPermissionDetailsInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_EXPermitNumberInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_CustomsEconomicGroupAddInfoInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_EXSiteInspectionDateInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_GS_NKReviewedByUserInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_ImportCustomsDefaultAddInfoInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_ImportEntryPaymentPreferenceInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_ImportQuarantinePaymentPreferenceInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_LastReviewedOnInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_OA_ApprovedLocationInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_OH_OrgHeaderInfo.ReadOnly);
					Assert("Access Not Allowed - ReadOnly", data.OV_RN_NKClientCountryRelationInfo.ReadOnly);

					Assert("Always ReadOnly", data.OV_SystemCreateTimeUtcInfo.ReadOnly);
					Assert("Always ReadOnly", data.OV_SystemCreateUserInfo.ReadOnly);
					Assert("Always ReadOnly", data.OV_SystemLastEditTimeUtcInfo.ReadOnly);
					Assert("Always ReadOnly", data.OV_SystemLastEditUserInfo.ReadOnly);
				}
				finally
				{
					Env.Security.OrgConsignorModifyExporterScheme.IsAllowed = oldValue;
				}
			}
		}

		public void TestAddressListUsesAddressesActive()
		{
			var org = Factory.New<OrgHeader>();
			var data = org.CountryDataCollectionForThisCompany.AddNew();
			var descriptor = data.OV_OA_ApprovedLocationInfo.PropertyDescriptor;
			var listAttribute = descriptor.Attributes[typeof(ListAttribute)] as ListAttribute;

			AssertNotNull(listAttribute);
			AssertEquals("OrgHeader.AddressesActive", listAttribute.ListDataSourceMember);
		}

		public void TestDefaultValues()
		{
			OrgHeader org = OrgHeader.New(Factory);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, org.CountryData.OV_RN_NKClientCountryRelation);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, org.CountryData.OV_RN_NKIssuingAuthorityCountry);
			AssertEquals(AviationSecuritySchemeMembershipEx.Codes.No, org.CountryData.OV_EXApprovedOrMajorExporter);
		}

		public void TestUSDefaultValues()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			var sgOrg = OrgHeader.New(Factory);
			AssertEquals(ZString.Empty, sgOrg.CountryData.OV_ImportCustomsDefaultAddInfo);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var org = OrgHeader.New(Factory);
			AssertEquals("Pre-Condition: Current Country Code should be US", "US", org.CountryData.OV_RN_NKClientCountryRelation);
			AssertEquals("ZO_OtherReconIndicator must default to NA when Organisation is created under US company", "<ImportCustomsDefaultAddInfo><OtherReconIndicator>NA</OtherReconIndicator></ImportCustomsDefaultAddInfo>", org.CountryData.OV_ImportCustomsDefaultAddInfo);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
			var orgPuertoRico = OrgHeader.New(Factory);
			AssertEquals("Pre-Condition: Current Country Code should be PR", "PR", orgPuertoRico.CountryData.OV_RN_NKClientCountryRelation);
			AssertEquals("ZO_OtherReconIndicator must default to NA when Organisation is created under US jurisdiction company", "<ImportCustomsDefaultAddInfo><OtherReconIndicator>NA</OtherReconIndicator></ImportCustomsDefaultAddInfo>", orgPuertoRico.CountryData.OV_ImportCustomsDefaultAddInfo);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Jamaica);
			var jmOrg = OrgHeader.New(Factory);
			AssertEquals(ZString.Empty, jmOrg.CountryData.OV_ImportCustomsDefaultAddInfo);
		}

		public void TestIsSavedByFactory()
		{
			OrgCountryData orgCountryData = Factory.New<OrgCountryData>();
			Assert("Can save with empty OV_OH_OrgHeader", orgCountryData.IsSavedByFactory);

			orgCountryData.OV_OH_OrgHeader = ZGuid.NewZGuid();
			Assert("Cannot save without OrgHeader", !orgCountryData.IsSavedByFactory);

			orgCountryData.OV_OH_OrgHeader = Factory.New<OrgHeader>().PK;
			Assert("Can save with OrgHeader", orgCountryData.IsSavedByFactory);

			orgCountryData.OrgHeader.OverrideIsSavedByFactory(false);
			Assert("Cannot save if OrgHeader is not in DB and is not saveable", !orgCountryData.IsSavedByFactory);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestOV_EXApprovalNumber_ReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var orgCountryData = Factory.New<OrgCountryData>();
				orgCountryData.OV_EXApprovedOrMajorExporter = "YES";
				Assert("Read/Write for YES", !orgCountryData.OV_EXApprovalNumberInfo.ReadOnly);

				orgCountryData.OV_EXApprovedOrMajorExporter = "NO";
				Assert("Read only for NO", orgCountryData.OV_EXApprovalNumberInfo.ReadOnly);
			}
		}

		public void TestRegionSpecificImpAddInfo()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = organisation.PK;
			countryData.OV_RN_NKClientCountryRelation = Constants.CountryCodes.UnitedKingdom;
			var regionSpecificImpAddInfo = countryData.RegionSpecificImpAddInfo;
			AssertEquals(ObjectFactory.GetType<EU.IOrgImpAddInfo>(), regionSpecificImpAddInfo.GetType());
			AssertSame(regionSpecificImpAddInfo, countryData.RegionSpecificImpAddInfo);
			AssertEquals(true, countryData.IsRegisteredEditableChildObject(regionSpecificImpAddInfo));

			countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = organisation.PK;
			countryData.OV_RN_NKClientCountryRelation = Constants.CountryCodes.Australia;
			AssertExceptionThrown<NotSupportedException>(() => countryData.RegionSpecificImpAddInfo.GetType());
		}
	}
}
