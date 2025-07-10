using System;
using System.Linq.Expressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.NewService.Controllers;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.OData.ModelBuilder;

namespace CargoWise.RefDbRepo.Staging.NewService
{
	public static class ModelConfig
	{
		public static void RegisterModels(ODataConventionModelBuilder builder)
		{
			Argument.NotNull(builder, nameof(builder));

			builder.EntitySet<RefCusTariff>().HasKey(x => x.ZZ1_PK);
			builder.EntitySet<RefCusTariffNationalCode>().HasKey(x => x.ZZW_PK);
			builder.EntitySet<RefCusTariffUOM>().HasKey(x => x.ZZ8_PK);
			builder.EntitySet<RefCusTariffAttribute>().HasKey(x => x.ZZ3_PK);
			builder.EntitySet<RefCusTariffAttributeName>().HasKey(x => x.ZY6_PK);
			builder.EntitySet<RefCusTariffBRCharacteristic>().HasKey(x => x.ZB1_PK);
			builder.EntitySet<RefCusTariffBRCharacteristicAttribute>().HasKey(x => x.ZB3_PK);
			builder.EntitySet<RefCusTariffBRCharacteristicValue>().HasKey(x => x.ZB2_PK);
			builder.EntitySet<RefCusRate>().HasKey(x => x.ZZ2_PK);
			builder.EntitySet<RefCusRateUOM>().HasKey(x => x.ZXG_PK);
			builder.EntitySet<RefCusRateType>().HasKey(x => x.ZZR_PK);
			builder.EntitySet<RefCusRateTypeLanguage>().HasKey(x => x.ZXT_PK);
			builder.EntitySet<RefCusTariffRelationship>().HasKey(x => x.ZZH_PK);
			builder.EntitySet<RefCusTariffType>().HasKey(x => x.ZZI_PK);
			builder.EntitySet<RefCusTariffTypeLanguage>().HasKey(x => x.ZXK_PK);
			builder.EntitySet<RefCusConditionType>().HasKey(x => x.ZX2_PK);
			builder.EntitySet<RefCusConditionCode>().HasKey(x => x.ZY7_PK);
			builder.EntitySet<RefCusConditionValueType>().HasKey(x => x.ZX4_PK);
			builder.EntitySet<RefCusExcludedTradeGroup>().HasKey(x => x.ZZC_PK);
			builder.EntitySet<RefCusTradeGroup>().HasKey(x => x.ZZA_PK);
			builder.EntitySet<RefCusApplicability>().HasKey(x => x.ZZT_PK);
			builder.EntitySet<RefCusTariffAdditionalCode>().HasKey(x => x.ZY2_PK);
			builder.EntitySet<RefCusTariffAdditionalCodeCategory>().HasKey(x => x.ZY3_PK);
			builder.EntitySet<RefCusTariffAdditionalCodeLanguage>().HasKey(x => x.ZY4_PK);

			builder.EntitySet<RefCusNomenclatureGroup>().HasKey(x => x.ZZ5_PK);
			builder.EntitySet<RefCusNomenclatureGroupNote>().HasKey(x => x.ZZL_PK);
			builder.EntitySet<RefCusCondition>().HasKey(x => x.ZX1_PK);
			builder.EntitySet<RefCusConditionLanguage>().HasKey(x => x.ZXJ_PK);
			builder.EntitySet<RefCusConditionValue>().HasKey(x => x.ZX3_PK);
			builder.EntitySet<RefCarrierCode>().HasKey(x => x.ZZ4_PK);
			builder.EntitySet<RefCarrierCodeAttribute>().HasKey(x => x.ZZG_PK);
			builder.EntitySet<RefCarrierCodeLanguage>().HasKey(x => x.ZCL_PK);
			builder.EntitySet<RefCusCodeType>().HasKey(x => x.ZZK_PK);
			builder.EntitySet<RefCusCodeTypeLanguage>().HasKey(x => x.ZXI_PK);
			builder.EntitySet<RefCusCodeList>().HasKey(x => x.ZZD_PK);
			builder.EntitySet<RefCusCodeListAttribute>().HasKey(x => x.ZZE_PK);
			builder.EntitySet<RefCusCodeListAttributeName>().HasKey(x => x.ZXE_PK);
			builder.EntitySet<RefCusCodeListAttributeNameLanguage>().HasKey(x => x.ZXH_PK);
			builder.EntitySet<RefCusCodeListLanguage>().HasKey(x => x.ZXA_PK);
			builder.EntitySet<RefCusMapType>().HasKey(x => x.ZZP_PK);
			builder.EntitySet<RefCusMap>().HasKey(x => x.ZZM_PK);
			builder.EntitySet<RefVessel>().HasKey(x => x.RV_PK);
			builder.EntitySet<RefVesselZZ>().HasKey(x => x.ZZO_PK);
			builder.EntitySet<RefVesselArrival>().HasKey(x => x.ZYA_PK);
			builder.EntitySet<RefExchangeRateZZ>().HasKey(x => x.ZZN_PK);
			builder.EntitySet<RefCarrierVesselPivot>().HasKey(x => x.ZZQ_PK);
			builder.EntitySet<RefCusRateCode>().HasKey(x => x.ZY1_PK);
			builder.EntitySet<RefCusRateCodeLanguage>().HasKey(x => x.ZXC_PK);
			builder.EntitySet<RefCusPreference>().HasKey(x => x.ZZS_PK);
			builder.EntitySet<RefCusTariffLanguage>().HasKey(x => x.ZX7_PK);
			builder.EntitySet<RefCusNomenclatureLanguage>().HasKey(x => x.ZX8_PK);
			builder.EntitySet<RefCusPreferenceLanguage>().HasKey(x => x.ZX9_PK);
			builder.EntitySet<RefCusCodeOrAttributeTransportMode>().HasKey(x => x.ZZU_PK);
			builder.EntitySet<RefCusNomenclatureGroupType>().HasKey(x => x.ZZ9_PK);
			builder.EntitySet<RefCusProcedure>().HasKey(x => x.ZZ6_PK);
			builder.EntitySet<RefCusProcedureAttribute>().HasKey(x => x.ZXB_PK);
			builder.EntitySet<RefCusTaxOrFee>().HasKey(x => x.ZZF_PK);
			builder.EntitySet<RefCusTaxOrFeeType>().HasKey(x => x.ZX0_PK);
			builder.EntitySet<RefCusVATApplicability>().HasKey(x => x.ZX5_PK);
			builder.EntitySet<RefCusTradeGroupCountry>().HasKey(x => x.ZZB_PK);
			builder.EntitySet<RefCusTradeGroupLanguage>().HasKey(x => x.ZXD_PK);
			builder.EntitySet<RefCountryStates>().HasKey(x => x.RW_PK);
			builder.EntitySet<RefTimeZoneSet>().HasKey(x => x.R3_PK);
			builder.EntitySet<RefTimeZone>().HasKey(x => x.R2_PK);
			builder.EntitySet<RefTimeZoneRule>().HasKey(x => x.R4_PK);
			builder.EntitySet<RefUNLOCO>().HasKey(x => x.RL_PK);
			builder.EntitySet<RefUNLOCOUtcOffset>().HasKey(x => x.RLO_PK);
			builder.EntitySet<RefUNLOCOPortMapping>().HasKey(x => x.RLM_PK);
			builder.EntitySet<RefUNLOCORelatedPort>().HasKey(x => x.RLR_PK);
			builder.EntitySet<RefCusRuling>().HasKey(x => x.ZZX_PK);
			builder.EntitySet<RefCusRulingConfig>().HasKey(x => x.ZZY_PK);
			builder.EntitySet<RefCurrency>().HasKey(x => x.RX_PK);
			builder.EntitySet<RefCountry>().HasKey(x => x.RN_PK);
			builder.EntitySet<RefLocoMap>().HasKey(x => x.RY_PK);
			builder.EntitySet<RefLanguageText>().HasKey(x => x.RLT_PK);
			builder.EntitySet<RefCusCodeListAttributeUserView>().HasKey(x => x.ZZE_PK);
			builder.EntitySet<RefCusCodeListUserView>().HasKey(x => x.ZZD_PK);
			builder.EntitySet<RefAccTaxRate>().HasKey(x => x.ZAT_PK);
			builder.EntitySet<RefAccTaxRateUserView>().HasKey(x => x.ZAT_PK);
			builder.EntitySet<RefHarbourRate>().HasKey(x => x.ZXF_PK);
			builder.EntitySet<RefCusAUNexdocECMCode>().HasKey(x => x.ZY5_PK);
			builder.EntitySet<RefShippingLine>().HasKey(x => x.RSL_PK);
			builder.EntitySet<RefShippingLineMessagingRequirement>().HasKey(x => x.RSR_PK);
			builder.EntitySet<RefShippingLineEBLProvider>().HasKey(x => x.RSE_PK);
			builder.EntitySet<RefShippingLineMessagingRequirementType>().HasKey(x => x.RST_PK);
			builder.EntitySet<UNDGSubstance>().HasKey(x => x.DG_PK);
			builder.EntitySet<UNDGAttribute>().HasKey(x => x.DA_PK);
			builder.EntitySet<UNDGReference>().HasKey(x => x.DR_PK);
			builder.EntitySet<UNDGSubstanceADR>().HasKey(x => x.ADR_PK);
			builder.EntitySet<UNDGSubstanceRID>().HasKey(x => x.RID_PK);
			builder.EntitySet<UNDGSubstanceADN>().HasKey(x => x.ADN_PK);
			builder.EntitySet<UNDGSubstanceJTT>().HasKey(x => x.JTT_PK);
			builder.EntitySet<UNDGSubstanceCFR>().HasKey(x => x.CFR_PK);
			builder.EntitySet<UNDGAttributeZZ>().HasKey(x => x.DAZ_PK);
			builder.EntitySet<UNDGCommonData>().HasKey(x => x.DC_PK);
			builder.EntitySet<RefCusConditionTypeLanguage>().HasKey(x => x.ZXW_PK);
			builder.EntitySet<RefCusConditionCodeLanguage>().HasKey(x => x.ZY8_PK);
			builder.EntitySet<RefCusConditionValueTypeLanguage>().HasKey(x => x.ZXX_PK);
			builder.EntitySet<RefCusProcedureLanguage>().HasKey(x => x.ZXV_PK);
			builder.EntitySet<RefCusTaxOrFeeLanguage>().HasKey(x => x.ZXU_PK);
			builder.EntitySet<RefAirline>().HasKey(x => x.RM_PK);
			builder.EntitySet<RefPortPolygon>().HasKey(x => x.RPP_PK);
			builder.EntitySet<RefApplicationAttribute>().HasKey(x => x.RAA_PK);
			builder.EntitySet<RefApplicationAttribute>(RefApplicationAttributeDefaultUpdateController.ControllerName + ControllerSuffix).HasKey(x => x.RAA_PK);
			builder.EntitySet<RefApplicationAttributeType>().HasKey(x => x.RAT_PK);
			builder.EntitySet<QRTZ_JOB_DETAILS>().HasKey(x => x.JOB_PK);
			builder.EntitySet<UNDGCountryReference>().HasKey(x => x.DCR_PK);
			builder.EntitySet<UNDGCountryReferencePivot>().HasKey(x => x.DCP_PK);
			builder.EntitySet<StmNote>().HasKey(x => x.ST_PK);
			builder.EntitySet<RefStlScript>().HasKey(x => x.STL_PK);
			builder.EntitySet<RefCusConfiguration>().HasKey(x => x.ZZJ_PK);
			builder.EntitySet<RefAirlineCommodityCode>().HasKey(x => x.RAC_PK);
			builder.EntitySet<RefAirlineProductCode>().HasKey(x => x.RAR_PK);
			builder.EntitySet<RefAirlineProductCodeCommodityCodePivot>().HasKey(x => x.RPC_PK);
			builder.EntitySet<RefCusQuota>().HasKey(x => x.ZXQ_PK);
			builder.EntitySet<RefFacility>().HasKey(x => x.RFT_PK);
			builder.EntitySet<RefFacilityLocalCode>().HasKey(x => x.RFL_PK);
			builder.EntitySet<RefStlFieldMapping>().HasKey(x => x.SFM_PK);
			builder.EntitySet<SourceData>().HasKey(x => x.SDA_PK);
			builder.EntitySet<SourceDataUserView>().HasKey(x => x.SDA_PK);
			builder.EntitySet<RefErrorReport>().HasKey(x => x.RER_PK);
			builder.EntitySet<ProcessorStatus>().HasKey(x => x.PRC_PK);
			builder.EntitySet<RefMessagingBussPackageInfo>().HasKey(x => x.ZMP_PK);
			builder.EntitySet<RefMessagingBussCarrierInfo>().HasKey(x => x.ZMC_PK);
			builder.EntitySet<RefMessagingBussPackageVersion>().HasKey(x => x.ZMV_PK);
			builder.EntitySet<RefDocOrgCusCode>().HasKey(x => x.DOC_PK);
			builder.EntitySet<RefSysConfigType>().HasKey(x => x.ZRT_PK);
			builder.EntitySet<RefSysConfig>().HasKey(x => x.ZRC_PK);
			builder.EntitySet<RefComplianceList>().HasKey(x => x.RCL_PK);
			builder.EntitySet<RefClient>().HasKey(x => x.RCT_PK);
			builder.EntitySet<RefMaterial>().HasKey(x => x.RMC_PK);
			builder.EntitySet<RefDamage>().HasKey(x => x.RFM_PK);
			builder.EntitySet<RefMRComponentCode>().HasKey(x => x.RCC_PK);
			builder.EntitySet<RefCusProfileType>().HasKey(x => x.XXX_PK);
			builder.EntitySet<RefCusProfile>().HasKey(x => x.XX0_PK);
			builder.EntitySet<RefCusProfileAttribute>().HasKey(x => x.XXY_PK);
			builder.EntitySet<RefCusProfileQuestion>().HasKey(x => x.XQ2_PK);
			builder.EntitySet<RefCusProfileQuestionAnswerList>().HasKey(x => x.XQ4_PK);
			builder.EntitySet<RefCusProfileQuestionAnswerListLanguage>().HasKey(x => x.XAL_PK);
			builder.EntitySet<RefCusProfileQuestionAttribute>().HasKey(x => x.XQ3_PK);
			builder.EntitySet<RefCusProfileQuestionLanguage>().HasKey(x => x.XQL_PK);
			builder.EntitySet<RefCusProfileQuestionPathway>().HasKey(x => x.XQP_PK);
			builder.EntitySet<RefRepairCode>().HasKey(x => x.RRC_PK);
			builder.EntitySet<RefUnitSection>().HasKey(x => x.RUS_PK);
			builder.EntitySet<RefAccElectronicProcessingFee>().HasKey(x => x.EPF_PK);
			builder.EntitySet<RefEquipmentGrade>().HasKey(x => x.REG_PK);
			builder.EntitySet<RefGlbReleaseNote>().HasKey(x => x.ZGF_PK);
			builder.EntitySet<RefComplianceCommodityAlert>().HasKey(x => x.RCR_PK);
			builder.EntitySet<UNDGVersion>().HasKey(x => x.DV_PK);
			builder.EntitySet<RefAccessorial>().HasKey(x => x.ASI_PK);
			builder.RegisterGeometryType();
			builder.RegisterQrtzJobDetails();
		}

		static EntitySetConfiguration<T> EntitySet<T>(this ODataConventionModelBuilder builder) where T : class
		{
			Argument.NotNull(builder, nameof(builder));
			return builder.EntitySet<T>(typeof(T).Name + ControllerSuffix);
		}

		static EntityTypeConfiguration<T> HasKey<T>(this EntitySetConfiguration<T> entitySet, Expression<Func<T, Guid>> getKey) where T : class
		{
			var entityType = entitySet.EntityType;
			entityType.HasKey(getKey);
			return entityType;
		}

		static void RegisterGeometryType(this ODataConventionModelBuilder builder)
		{
			var portPolygon = builder.EntityType<RefPortPolygon>();
			portPolygon.Ignore(p => p.RPP_SerializedPolygon);
			portPolygon.ComplexProperty(p => p.RPP_SerializedPolygon_WKT);

			var unLoco = builder.EntityType<RefUNLOCO>();
			unLoco.Ignore(p => p.RL_GeoLocation);
			unLoco.ComplexProperty(p => p.RL_GeoLocation_WKT);

			var facility = builder.EntityType<RefFacility>();
			facility.Ignore(p => p.RFT_GeoLocation);
			facility.ComplexProperty(p => p.RFT_GeoLocation_WKT);
		}

		static void RegisterQrtzJobDetails(this ODataConventionModelBuilder builder)
		{
			var jobDetail = builder.EntityType<QRTZ_JOB_DETAILS>();
			jobDetail.Property(x => x.CountryCode);
			jobDetail.Property(x => x.ProgramExePath);
			jobDetail.Property(x => x.ProgramArgs);
		}

		const string ControllerSuffix = "Update";
	}
}
