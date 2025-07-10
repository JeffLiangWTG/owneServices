using System;
using System.Linq.Expressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public static class ModelConfig
	{
		public static IEdmModel GetEdmModel()
		{
			var builder = new ODataConventionModelBuilder();
			RegisterModels(builder);
			return builder.GetEdmModel();
		}

		public static void RegisterModels(ODataConventionModelBuilder builder)
		{
			Argument.NotNull(builder, nameof(builder));

			builder.EntitySet<RefCusTariff>().HasKey(x => x.ZZ1_PK).EnableDataSetFunctions().EnableCloneExpiredDependent();
			builder.EntitySet<RefCusTariffUOM>().HasKey(x => x.ZZ8_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusTariffAttribute>().HasKey(x => x.ZZ3_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusTariffAttributeName>().HasKey(x => x.ZY6_PK).EnableDataSetFunctions();

			builder.EntitySet<RefCusTariffBRCharacteristic>().HasKey(x => x.ZB1_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusTariffBRCharacteristicAttribute>().HasKey(x => x.ZB3_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusTariffBRCharacteristicValue>().HasKey(x => x.ZB2_PK).EnableDeleteOrExpire();

			builder.EntitySet<RefCusRate>().HasKey(x => x.ZZ2_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusRateType>().HasKey(x => x.ZZR_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusRateTypeLanguage>().HasKey(x => x.ZXT_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusRateUOM>().HasKey(x => x.ZXG_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusTariffRelationship>().HasKey(x => x.ZZH_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusNomenclatureGroupType>().HasKey(x => x.ZZ9_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusTariffType>().HasKey(x => x.ZZI_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusTariffTypeLanguage>().HasKey(x => x.ZXK_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCarrierCode>().HasKey(x => x.ZZ4_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCarrierCodeAttribute>().HasKey(x => x.ZZG_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCarrierCodeLanguage>().HasKey(x => x.ZCL_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefVessel>().HasKey(x => x.RV_PK).EnableDataSetFunctions();
			builder.EntitySet<RefVesselZZ>().HasKey(x => x.ZZO_PK).EnableDataSetFunctions();
			builder.EntitySet<RefVesselArrival>().HasKey(x => x.ZYA_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCarrierVesselPivot>().HasKey(x => x.ZZQ_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusNomenclatureGroup>().HasKey(x => x.ZZ5_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusNomenclatureGroupNote>().HasKey(x => x.ZZL_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefExchangeRateZZ>().HasKey(x => x.ZZN_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusMapType>().HasKey(x => x.ZZP_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusMap>().HasKey(x => x.ZZM_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusCodeList>().HasKey(x => x.ZZD_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusCodeListAttribute>().HasKey(x => x.ZZE_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusCodeListAttributeName>().HasKey(x => x.ZXE_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusCodeListAttributeNameLanguage>().HasKey(x => x.ZXH_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusCodeOrAttributeTransportMode>().HasKey(x => x.ZZU_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusCodeType>().HasKey(x => x.ZZK_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusCodeTypeLanguage>().HasKey(x => x.ZXI_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusProcedure>().HasKey(x => x.ZZ6_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusProcedureAttribute>().HasKey(x => x.ZXB_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusTradeGroup>().HasKey(x => x.ZZA_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusTradeGroupCountry>().HasKey(x => x.ZZB_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusTradeGroupLanguage>().HasKey(x => x.ZXD_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusTaxOrFee>().HasKey(x => x.ZZF_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusTaxOrFeeType>().HasKey(x => x.ZX0_PK).EnableDataSetFunctions();

			builder.EntitySet<RefCusTariffRule>().HasKey(x => x.ZZ1_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusTariffAttributeRule>().HasKey(x => x.ZZ3_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusTariffRelationshipRule>().HasKey(x => x.ZZH_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusTariffUOMRule>().HasKey(x => x.ZZ8_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusRateRule>().HasKey(x => x.ZZ2_PK).EnableDeleteOrExpire();
			builder.EntitySet<ClientRefDbVersionControl>().HasKey(x => x.CVC_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusConditionValueType>().HasKey(x => x.ZX4_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusPreference>().HasKey(x => x.ZZS_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusRateCode>().HasKey(x => x.ZY1_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusRateCodeLanguage>().HasKey(x => x.ZXC_PK).EnableDeleteOrExpire();

			builder.EntitySet<RefDataGrouping>().HasKey(x => x.ZZZ_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusCondition>().HasKey(x => x.ZX1_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusConditionLanguage>().HasKey(x => x.ZXJ_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusConditionValue>().HasKey(x => x.ZX3_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusConditionType>().HasKey(x => x.ZX2_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusConditionCode>().HasKey(x => x.ZY7_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusApplicability>().HasKey(x => x.ZZT_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusExcludedTradeGroup>().HasKey(x => x.ZZC_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusTariffNationalCode>().HasKey(x => x.ZZW_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusVATApplicability>().HasKey(x => x.ZX5_PK).EnableDeleteOrExpire();

			builder.EntitySet<UNDGSubstance>().HasKey(x => x.DG_PK).EnableDataSetFunctions();
			builder.EntitySet<UNDGCommonData>().HasKey(x => x.DC_PK).EnableDataSetFunctions();
			builder.EntitySet<UNDGAttribute>().HasKey(x => x.DA_PK).EnableDeleteOrExpire();
			builder.EntitySet<UNDGReference>().HasKey(x => x.DR_PK).EnableDeleteOrExpire();
			builder.EntitySet<UNDGSubstanceADR>().HasKey(x => x.ADR_PK).EnableDataSetFunctions();
			builder.EntitySet<UNDGSubstanceRID>().HasKey(x => x.RID_PK).EnableDataSetFunctions();
			builder.EntitySet<UNDGSubstanceADN>().HasKey(x => x.ADN_PK).EnableDataSetFunctions();
			builder.EntitySet<UNDGSubstanceJTT>().HasKey(x => x.JTT_PK).EnableDataSetFunctions();
			builder.EntitySet<UNDGSubstanceCFR>().HasKey(x => x.CFR_PK).EnableDataSetFunctions();
			builder.EntitySet<UNDGAttributeZZ>().HasKey(x => x.DAZ_PK).EnableDeleteOrExpire();

			builder.EntitySet<RefLanguageType>().HasKey(x => x.ZX6_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusTariffLanguage>().HasKey(x => x.ZX7_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusPreferenceLanguage>().HasKey(x => x.ZX9_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusNomenclatureLanguage>().HasKey(x => x.ZX8_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusCodeListLanguage>().HasKey(x => x.ZXA_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCountryStates>().HasKey(x => x.RW_PK).EnableDataSetFunctions();
			builder.EntitySet<RefAccTaxRate>().HasKey(x => x.ZAT_PK).EnableDataSetFunctions();
			builder.EntitySet<RefHarbourRate>().HasKey(x => x.ZXF_PK).EnableDataSetFunctions();
			builder.EntitySet<RefShippingLine>().HasKey(x => x.RSL_PK).EnableDataSetFunctions();
			builder.EntitySet<RefShippingLineMessagingRequirement>().HasKey(x => x.RSR_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefShippingLineEBLProvider>().HasKey(x => x.RSE_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefShippingLineMessagingRequirementType>().HasKey(x => x.RST_PK).EnableDataSetFunctions();

			builder.EntitySet<RefTimeZoneSet>().HasKey(x => x.R3_PK).EnableDataSetFunctions();
			builder.EntitySet<RefTimeZone>().HasKey(x => x.R2_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefTimeZoneRule>().HasKey(x => x.R4_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefUNLOCO>().HasKey(x => x.RL_PK).EnableDataSetFunctions();
			builder.EntitySet<RefUNLOCOPortMapping>().HasKey(x => x.RLM_PK).EnableDataSetFunctions();
			builder.EntitySet<RefUNLOCOUtcOffset>().HasKey(x => x.RLO_PK).GetWithOptimizedExpand().EnableDeleteOrExpire();
			builder.EntitySet<RefUNLOCORelatedPort>().HasKey(x => x.RLR_PK).GetWithOptimizedExpand().EnableDeleteOrExpire();
			builder.EntitySet<RefCusRuling>().HasKey(x => x.ZZX_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusRulingConfig>().HasKey(x => x.ZZY_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefLocoMap>().HasKey(x => x.RY_PK).GetWithOptimizedExpand().EnableDeleteOrExpire();
			builder.EntitySet<RefCountry>().HasKey(x => x.RN_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCurrency>().HasKey(x => x.RX_PK).EnableDataSetFunctions();
			builder.EntitySet<RefLanguageText>().HasKey(x => x.RLT_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusCodeListAttributeUserView>().HasKey(x => x.ZZE_PK);
			builder.EntitySet<RefCusCodeListUserView>().HasKey(x => x.ZZD_PK).EnableDataSetFunctions();
			builder.EntitySet<RefAccTaxRateUserView>().HasKey(x => x.ZAT_PK).EnableDataSetFunctions();
			builder.EntitySet<RefShippingLineUserView>().HasKey(x => x.RSL_PK).EnableDataSetFunctions();
			builder.EntitySet<RefStlScriptUserView>().HasKey(x => x.STL_PK).EnableDataSetFunctions();

			builder.EntitySet<DataSetChangeHistory>().HasKey(x => x.DCH_PK);
			builder.EntitySet<RefCusTariffAdditionalCode>().HasKey(x => x.ZY2_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusAUNexdocECMCode>().HasKey(x => x.ZY5_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusTariffAdditionalCodeCategory>().HasKey(x => x.ZY3_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusTariffAdditionalCodeLanguage>().HasKey(x => x.ZY4_PK).EnableDeleteOrExpire();

			builder.EntitySet<RefDocOrgCusCode>().HasKey(x => x.DOC_PK).EnableDataSetFunctions();

			builder.EntitySet<RefDataSetInformation>().HasKey(x => x.RDS_PK);
			builder.EntitySet<RefDataSetInformationDefinition>().HasKey(x => x.RDD_PK);
			builder.EntitySet<DataPushSubscription>().HasKey(x => x.DPS_PK);

			builder.EntitySet<RefCusConditionTypeLanguage>().HasKey(x => x.ZXW_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusConditionCodeLanguage>().HasKey(x => x.ZY8_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusConditionValueTypeLanguage>().HasKey(x => x.ZXX_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusProcedureLanguage>().HasKey(x => x.ZXV_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusTaxOrFeeLanguage>().HasKey(x => x.ZXU_PK).EnableDeleteOrExpire();

			builder.EntitySet<RefSysConfigType>().HasKey(x => x.ZRT_PK).EnableDataSetFunctions();
			builder.EntitySet<RefSysConfig>().HasKey(x => x.ZRC_PK).EnableDeleteOrExpire();

			builder.EntitySet<RefComplianceList>().HasKey(x => x.RCL_PK).EnableDataSetFunctions();
			builder.EntitySet<RefAirline>().HasKey(x => x.RM_PK).EnableDataSetFunctions();
			builder.EntitySet<RefPortPolygon>().HasKey(x => x.RPP_PK).EnableDataSetFunctions();

			builder.EntitySet<UNDGCountryReference>().HasKey(x => x.DCR_PK).EnableDataSetFunctions();
			builder.EntitySet<UNDGCountryReferencePivot>().HasKey(x => x.DCP_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefAirlineCommodityCode>().HasKey(x => x.RAC_PK).EnableDataSetFunctions();
			builder.EntitySet<RefAirlineProductCode>().HasKey(x => x.RAR_PK).EnableDataSetFunctions();
			builder.EntitySet<RefAirlineProductCodeCommodityCodePivot>().HasKey(x => x.RPC_PK).EnableDeleteOrExpire();
			builder.EntitySet<StmNote>().HasKey(x => x.ST_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefStlScript>().HasKey(x => x.STL_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusProcedureUserView>().HasKey(x => x.ZZ6_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusProcedureAttributeUserView>().HasKey(x => x.ZXB_PK);
			builder.EntitySet<RefCusConfiguration>().HasKey(x => x.ZZJ_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusQuota>().HasKey(x => x.ZXQ_PK).EnableDataSetFunctions();
			builder.EntitySet<RefClient>().HasKey(x => x.RCT_PK).EnableDataSetFunctions();

			builder.EntitySet<RefFacility>().HasKey(x => x.RFT_PK).EnableDataSetFunctions();
			builder.EntitySet<RefFacilityLocalCode>().HasKey(x => x.RFL_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefStlFieldMapping>().HasKey(x => x.SFM_PK).EnableDataSetFunctions();
			builder.EntitySet<UserAuthorization>().HasKey(x => x.UA_PK);

			builder.EntitySet<RefMessagingBussPackageInfo>().HasKey(x => x.ZMP_PK).EnableDataSetFunctions();
			builder.EntitySet<RefMessagingBussCarrierInfo>().HasKey(x => x.ZMC_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefMessagingBussPackageVersion>().HasKey(x => x.ZMV_PK).EnableDeleteOrExpire();

			builder.EntitySet<RefVesselUserView>().HasKey(x => x.RV_PK).EnableDataSetFunctions();
			builder.EntitySet<RefUNLOCOUserView>().HasKey(x => x.RL_PK).EnableDataSetFunctions();
			builder.EntitySet<RefPortPolygonUserView>().HasKey(x => x.RPP_PK).EnableDataSetFunctions();
			builder.EntitySet<RefMaterial>().HasKey(x => x.RMC_PK).EnableDataSetFunctions();
			builder.EntitySet<RefDamage>().HasKey(x => x.RFM_PK).EnableDataSetFunctions();
			builder.EntitySet<RefRepairCode>().HasKey(x => x.RRC_PK).EnableDataSetFunctions();
			builder.EntitySet<RefUnitSection>().HasKey(x => x.RUS_PK).EnableDataSetFunctions();
			builder.EntitySet<RefMRComponentCode>().HasKey(x => x.RCC_PK).EnableDataSetFunctions();

			builder.EntitySet<RefCusProfileType>().HasKey(x => x.XXX_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusProfile>().HasKey(x => x.XX0_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusProfileAttribute>().HasKey(x => x.XXY_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusProfileQuestion>().HasKey(x => x.XQ2_PK).EnableDataSetFunctions();
			builder.EntitySet<RefCusProfileQuestionAnswerList>().HasKey(x => x.XQ4_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusProfileQuestionAnswerListLanguage>().HasKey(x => x.XAL_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusProfileQuestionAttribute>().HasKey(x => x.XQ3_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusProfileQuestionLanguage>().HasKey(x => x.XQL_PK).EnableDeleteOrExpire();
			builder.EntitySet<RefCusProfileQuestionPathway>().HasKey(x => x.XQP_PK).EnableDataSetFunctions();

			builder.EntitySet<RefAccElectronicProcessingFee>().HasKey(x => x.EPF_PK).EnableDataSetFunctions();
			builder.EntitySet<RefEquipmentGrade>().HasKey(x => x.REG_PK).EnableDataSetFunctions();
			builder.EntitySet<RefGlbReleaseNote>().HasKey(x => x.ZGF_PK).EnableDataSetFunctions();
			builder.EntitySet<RefComplianceCommodityAlert>().HasKey(x => x.RCR_PK).EnableDataSetFunctions();
			builder.EntitySet<UNDGVersion>().HasKey(x => x.DV_PK).EnableDataSetFunctions();
			builder.EntitySet<RefAccessorial>().HasKey(x => x.ASI_PK).EnableDataSetFunctions();

			builder.RegisterGeometryType();
		}

		static EntitySetConfiguration<T> EntitySet<T>(this ODataConventionModelBuilder builder) where T : class
		{
			Argument.NotNull(builder, nameof(builder));
			return builder.EntitySet<T>(typeof(T).Name + ControllerSuffix);
		}

		static EntityTypeConfiguration<T> EnableDataSetFunctions<T>(this EntityTypeConfiguration<T> entityType) where T : class
		{
			Argument.NotNull(entityType, nameof(entityType));
			return entityType.EnableDataSetFunctions(typeof(T).Name + ControllerSuffix);
		}

		static EntityTypeConfiguration<T> GetWithOptimizedExpand<T>(this EntityTypeConfiguration<T> entityType) where T : class
		{
			Argument.NotNull(entityType, nameof(entityType));
			return entityType.GetWithOptimizedExpand(typeof(T).Name + ControllerSuffix);
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

			var portPolygonUserView = builder.EntityType<RefPortPolygonUserView>();
			portPolygonUserView.Ignore(p => p.RPP_SerializedPolygon);
			portPolygonUserView.ComplexProperty(p => p.RPP_SerializedPolygon_WKT);

			var unLoco = builder.EntityType<RefUNLOCO>();
			unLoco.Ignore(p => p.RL_GeoLocation);
			unLoco.ComplexProperty(p => p.RL_GeoLocation_WKT);

			var unLocoUserView = builder.EntityType<RefUNLOCOUserView>();
			unLocoUserView.Ignore(p => p.RL_GeoLocation);
			unLocoUserView.ComplexProperty(p => p.RL_GeoLocation_WKT);

			var facility = builder.EntityType<RefFacility>();
			facility.Ignore(p => p.RFT_GeoLocation);
			facility.ComplexProperty(p => p.RFT_GeoLocation_WKT);
		}

		const string ControllerSuffix = "Update";
	}
}
