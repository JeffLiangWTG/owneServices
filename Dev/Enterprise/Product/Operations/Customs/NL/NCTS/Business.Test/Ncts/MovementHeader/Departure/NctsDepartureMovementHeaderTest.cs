using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureMovementHeader))]
sealed class NctsDepartureMovementHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestGuarantees()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType<NctsGuaranteeCollection<NctsGuarantee>>(nctsHeader.MovementHeader.Guarantees);
	}

	public void TestLookups()
	{
		var departureMovement = GetNewBusinessObject(Factory);
		CombineAssertions(() =>
		{
			AssertType<NctsDepartureMovementHeaderLookups>("Lookups", departureMovement.Lookups);
			AssertType<NctsDepartureMovementHeaderLookups>("GetNewPhase4Lookups()",  departureMovement.GetType().GetMethod("GetNewPhase4Lookups", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod).Invoke(departureMovement,null));
			AssertType<NctsDepartureMovementHeaderLookups>("GetNewPhase5Lookups()", departureMovement.GetType().GetMethod("GetNewPhase5Lookups", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod).Invoke(departureMovement, null));
		});
	}

	public void TestNctsHeader()
	{
		var departureMovement = GetNewBusinessObject(Factory);
		AssertType<NctsHeader>(departureMovement.Header);
	}

	public void TestCusGoodsLocation()
	{
		var departureMovement = GetNewBusinessObject(Factory);
		AssertType<CusGoodsLocation>(departureMovement.GoodsLocation);
	}

	public void TestForceRegenerateLocalReferenceNumber()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var departureMovement = Factory.New<NctsDepartureMovementHeaderForTest>();
			departureMovement.BM_BH = nctsHeader.PK;
			departureMovement.ActivateForceRegenerateLocalReferenceNumber = true;
			departureMovement.BM_PaperlessInbondNum = ZString.Empty;
			departureMovement.BM_Phase = ZString.Empty;
			departureMovement.BM_MessageStatus = ZString.Empty;

			AssertEquals("There is no LRN yet so force should not be enabled (because the regular generation is active)", false, departureMovement.ForceRegenerateLocalReferenceNumberOverride);

			departureMovement.BM_PaperlessInbondNum = ZString.Empty;
			departureMovement.BM_Phase = ZString.Empty;
			departureMovement.BM_MessageStatus = "INV";

			AssertEquals("There is no LRN yet so force should not be enabled (because the regular generation is active)", false, departureMovement.ForceRegenerateLocalReferenceNumberOverride);

			departureMovement.BM_PaperlessInbondNum = ZString.Empty;
			departureMovement.BM_Phase = "015";
			departureMovement.BM_MessageStatus = "INV";

			AssertEquals("There is no LRN yet so force should not be enabled (because the regular generation is active)", false, departureMovement.ForceRegenerateLocalReferenceNumberOverride);

			departureMovement.BM_PaperlessInbondNum = "TestLRN";
			departureMovement.BM_Phase = "015";
			departureMovement.BM_MessageStatus = "INV";

			AssertEquals("All conditions are true to force generate the LRN", true, departureMovement.ForceRegenerateLocalReferenceNumberOverride);

			departureMovement.BM_PaperlessInbondNum = "TestLRN";
			departureMovement.BM_Phase = "015";
			departureMovement.BM_MessageStatus = "ERR";

			AssertEquals("All conditions are true to force generate the LRN", true, departureMovement.ForceRegenerateLocalReferenceNumberOverride);

			departureMovement.BM_PaperlessInbondNum = "TestLRN";
			departureMovement.BM_Phase = "015";
			departureMovement.BM_MessageStatus = "ACK";

			AssertEquals("Message status is not ERR or INV, so force generate should not be enabled", false, departureMovement.ForceRegenerateLocalReferenceNumberOverride);

			departureMovement.BM_PaperlessInbondNum = "TestLRN";
			departureMovement.BM_Phase = "014";
			departureMovement.BM_MessageStatus = "ERR";

			AssertEquals("Phase is not 015, so force generate should not be enabled", false, departureMovement.ForceRegenerateLocalReferenceNumberOverride);

			departureMovement.ActivateForceRegenerateLocalReferenceNumber = false;
			departureMovement.BM_PaperlessInbondNum = "TestLRN";
			departureMovement.BM_Phase = "015";
			departureMovement.BM_MessageStatus = "ERR";

			AssertEquals("All conditions are true to force generate the LRN but function is not activated", false, departureMovement.ForceRegenerateLocalReferenceNumberOverride);
		});
	}

	public void TestDefaultDepartureLocationCodeFromCusAuthorisationIfBlank()
	{
		var departureMovement = GetNewBusinessObject(Factory);
		departureMovement.IsSimplifiedNctsProcedure = true;

		var header = departureMovement.Header;
		header.Principal.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;

		var configuration = header.Configuration.LocationOfGoodsFromAuthorisationDefaulterConfiguration;
		CombineAssertions(() =>
		{
			AssertEquals("Pre-requisite: Configuration IsDefaultingEnabled", expected: true, configuration.IsDefaultingEnabled);
			AssertEquals("Pre-requisite: Configuration QualifierCode", CusGoodsLocationQualifierList.Codes.PostcodeAddress, configuration.QualifierCode);
			AssertEquals("Pre-requisite: Configuration TypeCode", CusGoodsLocationTypeList.Codes.AuthorizedPlace, configuration.TypeCode);
		});

		var cusAuthorisationUsage = departureMovement.CusAuthorizationUsages.AddNew();
		cusAuthorisationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		cusAuthorisationUsage.AGC_Number = "AR10001";
		cusAuthorisationUsage.AGC_OH_Owner = GlbCompany.CurrentCompany.OrgProxy.PK;

		var cusAuthorisationHeader = Factory.NewWithValidTestData<Customs.Business.CusAuthorisationHeader>();
		cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		cusAuthorisationHeader.CPH_Number = "AR10001";
		cusAuthorisationHeader.CPH_OH_PermitHolder = GlbCompany.CurrentCompany.OrgProxy.PK;

		var goodsLocation = departureMovement.GoodsLocation;

		var rule = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		rule.CPR_ValueFrom = "NL000001";
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, configuration.QualifierCode, configuration.TypeCode, rule.CPR_ValueFrom);

		ClearDepartureGoodsLocation(goodsLocation);

		var rule1 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		rule1.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		rule1.CPR_ValueFrom = "NL000002";
		_ = NCTSTestHelper.CreateLinkedAuthorisationRuleForTest(rule1, LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "NL000002");
		departureMovement.CustomsOffices.Single<NctsEuOfficeCode>(e => e.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture).CY_Data = "NL000002";
		AssertEquals("Departure Customs Office", "NL000002", departureMovement.DepartureCustomsOffice.OfficeCode);
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, configuration.QualifierCode, configuration.TypeCode, rule1.CPR_ValueFrom);

		ClearDepartureGoodsLocation(goodsLocation);

		var rule2 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		rule2.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		rule2.CPR_ValueFrom = "A001";
		_ = NCTSTestHelper.CreateLinkedAuthorisationRuleForTest(rule2, LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "NL000002");
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, ZString.Empty, ZString.Empty, ZString.Empty);
	}

	public void TestIsFallbackProcedure_Caption()
	{
		var departureMovement = GetNewBusinessObject(Factory);
		AssertEquals("Fallback procedure", DataBoundResourceStrings.GetDataForProperty(departureMovement.IsFallbackProcedureInfo).Caption);
	}

	public void TestFallbackReference_Attributes()
	{
		_ = AssertEntity<NctsDepartureMovementHeader>()
			.HasProperty(x => x.FallbackReference)
			.WithCaption("Fallback reference")
			.WithAttribute<ReadOnlyAttribute>(r => r.IsReadOnly)
			.WithMaxLength(70);
	}

	public void TestIsFallbackTime_Attributes()
	{
		_ = AssertEntity<NctsDepartureMovementHeader>()
			.HasProperty(x => x.FallbackTime)
			.WithCaption("Fallback time")
			.WithAttribute<ReadOnlyAttribute>(r => r.IsReadOnly);
	}

	public void TestFallbackEntryNumber_Attributes()
	{
		_ = AssertEntity<NctsDepartureMovementHeader>()
			.HasProperty(x => x.FallbackNumber)
			.WithMaxLength(10);
	}

	public void TestIsFallbackProcedure()
	{
		var departureMovement = GetNewBusinessObject(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("By default the fallback procedure should be false", false, departureMovement.IsFallbackProcedure);
			var config = FallbackConfigurationTestHelper.GetFallbackConfigurationWithDVASetValue(new ZDateTime(2025, 01, 29, 10, 04, 00));
			departureMovement.IsFallbackProcedure = true;
			AssertEquals("Setting the fallback procedure to true should set the system defined value", true, departureMovement.GetSystemDefinedValue<ZBool>("IsFallbackProcedure"));
			AssertEquals("Emergency reference", config.InvocationReason, departureMovement.FallbackReference);
			AssertEquals("Emergency date/time", config.Start, departureMovement.FallbackTime);
		});
	}

	public void TestIsFallbackProcedure_ReadOnly()
	{
		var departureMovement = GetNewBusinessObject(Factory);
		var config = new FallbackConfiguration();

		var past = ZDateTime.Now.AddDays(-10);
		var future = ZDateTime.Now.AddDays(10);

		CombineAssertions(() =>
		{
			config.Start = ZDateTime.Empty;
			config.End = ZDateTime.Empty;
			NLCustomsRegistry.Instance.FallbackConfiguration_DVA.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);
			AssertEquals("Both Start and End Dates are empty - Fallback procedure checkbox should be disabled", true, departureMovement.IsFallbackProcedureInfo.ReadOnly);

			config.Start = past;
			config.End = future;
			NLCustomsRegistry.Instance.FallbackConfiguration_DVA.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);
			AssertEquals("Start in past and End in future - Fallback procedure checkbox should be enabled", false, departureMovement.IsFallbackProcedureInfo.ReadOnly);

			config.Start = future;
			config.End = future;
			NLCustomsRegistry.Instance.FallbackConfiguration_DVA.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);
			AssertEquals("Start in future and End in future - Fallback procedure checkbox should be disabled", true, departureMovement.IsFallbackProcedureInfo.ReadOnly);

			config.Start = past;
			config.End = past;
			NLCustomsRegistry.Instance.FallbackConfiguration_DVA.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);
			AssertEquals("Start in past and End in past - Fallback procedure checkbox should be disabled", true, departureMovement.IsFallbackProcedureInfo.ReadOnly);

			config.Start = ZDateTime.Empty;
			config.End = future;
			NLCustomsRegistry.Instance.FallbackConfiguration_DVA.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);
			AssertEquals("End in Future but Start empty - Fallback procedure checkbox should be disabled", true, departureMovement.IsFallbackProcedureInfo.ReadOnly);

			config.Start = past;
			config.End = ZDateTime.Empty;
			NLCustomsRegistry.Instance.FallbackConfiguration_DVA.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);
			AssertEquals("Start in past but End empty - Fallback procedure checkbox should be disabled", true, departureMovement.IsFallbackProcedureInfo.ReadOnly);
		});
	}

	[TestDate(2025, 01, 29, 10, 04, 00)]
	public void TestFactorySavingFallbackNumber()
	{
		var departureMovement = GetNewBusinessObject(Factory);
		var config = FallbackConfigurationTestHelper.GetFallbackConfigurationWithDVASetValue(new ZDateTime(2025, 01, 29, 10, 04, 00));
		NLCustomsRegistry.Instance.FallbackConfiguration_DVA.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);
		departureMovement.IsFallbackProcedure = true;

		CombineAssertions(() =>
		{
			AssertEquals("Empty FallbackNumber", "", departureMovement.FallbackNumber);
			Factory.Save();
			AssertEquals("FallbackNumber", "2501000001", departureMovement.FallbackNumber);
		});
	}

	public void TestIsFallbackProcedureResetValues()
	{
		var departureMovement = GetNewBusinessObject(Factory);
		departureMovement.IsFallbackProcedure = true;
		departureMovement.FallbackReference = "Ref";
		departureMovement.FallbackTime = ZDateTime.Now;
		departureMovement.FallbackNumber = "123456";

		departureMovement.IsFallbackProcedure = false;
		CombineAssertions(() =>
		{
			AssertEquals("FallbackReference", ZString.Empty, departureMovement.FallbackReference);
			AssertEquals("FallbackTime", ZDateTime.Empty, departureMovement.FallbackTime);
			AssertEquals("FallbackEntryNumber", ZString.Empty, departureMovement.FallbackNumber);
		});
	}

	public void TestExportTransportModeFallbackOnInlandTransportMode() => CombineAssertions(() =>
	{
		var departureMovement = GetNewBusinessObject(Factory);
		departureMovement.BM_ExportTransportMode = "4";
		departureMovement.BM_InlandTransportMode = "1";
		AssertEquals("export 4, inland 1", "4", departureMovement.ExportTransportModeFallbackOnInlandTransportMode);
		departureMovement.BM_ExportTransportMode = "";
		AssertEquals("export '', inland 1", "1", departureMovement.ExportTransportModeFallbackOnInlandTransportMode);
	});

	public void TestCusAuthorizationUsages()
	{
		var departureMovement = GetNewBusinessObject(Factory);
		AssertType<CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsDepartureMovementHeader>>(departureMovement.CusAuthorizationUsages);
	}

	void ClearDepartureGoodsLocation(CusGoodsLocation goodsLocation)
	{
		goodsLocation.CGL_Qualifier = ZString.Empty;
		goodsLocation.CGL_Type = ZString.Empty;
		goodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
	}

	void AssertDepartureGoodsLocation(CusGoodsLocation goodsLocation, ZString qualifier, ZString type, ZString houseNumber)
	{
		CombineAssertions(() =>
		{
			AssertEquals("Goods Location Qualifier", qualifier, goodsLocation.CGL_Qualifier);
			AssertEquals("Goods Location Type", type, goodsLocation.CGL_Type);
			AssertEquals("Goods Location House Number", houseNumber, goodsLocation.CGL_AdditionalIdentifier);

			var displayText = new ZStringBuilder().AppendIfNotEmpty(qualifier).AppendIfNotEmpty(type).AppendIfNotEmpty(houseNumber).ToStringWithDelimiterBetweenAppends(";").TrimEnd(';');
			AssertContains("Goods Location Display Text", displayText, goodsLocation.DisplayText);
		});
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	NctsDepartureMovementHeader GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader;
	}

	public class NctsDepartureMovementHeaderForTest : NctsDepartureMovementHeader
	{
		public NctsDepartureMovementHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool ForceRegenerateLocalReferenceNumberOverride => base.ForceRegenerateLocalReferenceNumber;
	}
}
