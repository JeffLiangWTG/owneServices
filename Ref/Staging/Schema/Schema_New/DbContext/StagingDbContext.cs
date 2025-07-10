using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public partial class StagingDbContext : DbContext
{
    public StagingDbContext(DbContextOptions<StagingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AutoSchema> AutoSchemas { get; set; }

    public virtual DbSet<DataChangeCapture> DataChangeCaptures { get; set; }

    public virtual DbSet<DataProcessingClone> DataProcessingClones { get; set; }

    public virtual DbSet<DataProcessingInformation> DataProcessingInformations { get; set; }

    public virtual DbSet<DataProcessingResult> DataProcessingResults { get; set; }

    public virtual DbSet<DataSourceInformation> DataSourceInformations { get; set; }

    public virtual DbSet<NamedEntityClassification> NamedEntityClassifications { get; set; }

    public virtual DbSet<ProcessData> ProcessData { get; set; }

    public virtual DbSet<ProcessorStatus> ProcessorStatuses { get; set; }

    public virtual DbSet<QRTZ_BLOB_TRIGGERS> QRTZ_BLOB_TRIGGERS { get; set; }

    public virtual DbSet<QRTZ_CALENDARS> QRTZ_CALENDARS { get; set; }

    public virtual DbSet<QRTZ_CRON_TRIGGERS> QRTZ_CRON_TRIGGERS { get; set; }

    public virtual DbSet<QRTZ_FIRED_TRIGGERS> QRTZ_FIRED_TRIGGERS { get; set; }

    public virtual DbSet<QRTZ_JOB_DETAILS> QRTZ_JOB_DETAILS { get; set; }

    public virtual DbSet<QRTZ_LOCKS> QRTZ_LOCKS { get; set; }

    public virtual DbSet<QRTZ_PAUSED_TRIGGER_GRPS> QRTZ_PAUSED_TRIGGER_GRPS { get; set; }

    public virtual DbSet<QRTZ_SCHEDULER_STATE> QRTZ_SCHEDULER_STATE { get; set; }

    public virtual DbSet<QRTZ_SIMPLE_TRIGGERS> QRTZ_SIMPLE_TRIGGERS { get; set; }

    public virtual DbSet<QRTZ_SIMPROP_TRIGGERS> QRTZ_SIMPROP_TRIGGERS { get; set; }

    public virtual DbSet<QRTZ_TRIGGERS> QRTZ_TRIGGERS { get; set; }

    public virtual DbSet<RefAccElectronicProcessingFee> RefAccElectronicProcessingFees { get; set; }

    public virtual DbSet<RefAccTaxRate> RefAccTaxRates { get; set; }

    public virtual DbSet<RefAccTaxRateUserView> RefAccTaxRateUserViews { get; set; }

    public virtual DbSet<RefAccessorial> RefAccessorials { get; set; }

    public virtual DbSet<RefAirline> RefAirlines { get; set; }

    public virtual DbSet<RefAirlineCommodityCode> RefAirlineCommodityCodes { get; set; }

    public virtual DbSet<RefAirlineProductCode> RefAirlineProductCodes { get; set; }

    public virtual DbSet<RefAirlineProductCodeCommodityCodePivot> RefAirlineProductCodeCommodityCodePivots { get; set; }

    public virtual DbSet<RefApplicationAttribute> RefApplicationAttributes { get; set; }

    public virtual DbSet<RefApplicationAttributeType> RefApplicationAttributeTypes { get; set; }

    public virtual DbSet<RefCarrierCode> RefCarrierCodes { get; set; }

    public virtual DbSet<RefCarrierCodeAttribute> RefCarrierCodeAttributes { get; set; }

    public virtual DbSet<RefCarrierCodeLanguage> RefCarrierCodeLanguages { get; set; }

    public virtual DbSet<RefCarrierVesselPivot> RefCarrierVesselPivots { get; set; }

    public virtual DbSet<RefClient> RefClients { get; set; }

    public virtual DbSet<RefComplianceCommodityAlert> RefComplianceCommodityAlerts { get; set; }

    public virtual DbSet<RefComplianceList> RefComplianceLists { get; set; }

    public virtual DbSet<RefCountry> RefCountries { get; set; }

    public virtual DbSet<RefCountryStates> RefCountryStates { get; set; }

    public virtual DbSet<RefCurrency> RefCurrencies { get; set; }

    public virtual DbSet<RefCusAUNexdocECMCode> RefCusAUNexdocECMCodes { get; set; }

    public virtual DbSet<RefCusApplicability> RefCusApplicabilities { get; set; }

    public virtual DbSet<RefCusCodeList> RefCusCodeLists { get; set; }

    public virtual DbSet<RefCusCodeListAttribute> RefCusCodeListAttributes { get; set; }

    public virtual DbSet<RefCusCodeListAttributeName> RefCusCodeListAttributeNames { get; set; }

    public virtual DbSet<RefCusCodeListAttributeNameLanguage> RefCusCodeListAttributeNameLanguages { get; set; }

    public virtual DbSet<RefCusCodeListAttributeUserView> RefCusCodeListAttributeUserViews { get; set; }

    public virtual DbSet<RefCusCodeListLanguage> RefCusCodeListLanguages { get; set; }

    public virtual DbSet<RefCusCodeListUserView> RefCusCodeListUserViews { get; set; }

    public virtual DbSet<RefCusCodeOrAttributeTransportMode> RefCusCodeOrAttributeTransportModes { get; set; }

    public virtual DbSet<RefCusCodeType> RefCusCodeTypes { get; set; }

    public virtual DbSet<RefCusCodeTypeLanguage> RefCusCodeTypeLanguages { get; set; }

    public virtual DbSet<RefCusCondition> RefCusConditions { get; set; }

    public virtual DbSet<RefCusConditionCode> RefCusConditionCodes { get; set; }

    public virtual DbSet<RefCusConditionCodeLanguage> RefCusConditionCodeLanguages { get; set; }

    public virtual DbSet<RefCusConditionLanguage> RefCusConditionLanguages { get; set; }

    public virtual DbSet<RefCusConditionType> RefCusConditionTypes { get; set; }

    public virtual DbSet<RefCusConditionTypeLanguage> RefCusConditionTypeLanguages { get; set; }

    public virtual DbSet<RefCusConditionValue> RefCusConditionValues { get; set; }

    public virtual DbSet<RefCusConditionValueType> RefCusConditionValueTypes { get; set; }

    public virtual DbSet<RefCusConditionValueTypeLanguage> RefCusConditionValueTypeLanguages { get; set; }

    public virtual DbSet<RefCusConfiguration> RefCusConfigurations { get; set; }

    public virtual DbSet<RefCusExcludedTradeGroup> RefCusExcludedTradeGroups { get; set; }

    public virtual DbSet<RefCusMap> RefCusMaps { get; set; }

    public virtual DbSet<RefCusMapType> RefCusMapTypes { get; set; }

    public virtual DbSet<RefCusNomenclatureGroup> RefCusNomenclatureGroups { get; set; }

    public virtual DbSet<RefCusNomenclatureGroupNote> RefCusNomenclatureGroupNotes { get; set; }

    public virtual DbSet<RefCusNomenclatureGroupType> RefCusNomenclatureGroupTypes { get; set; }

    public virtual DbSet<RefCusNomenclatureLanguage> RefCusNomenclatureLanguages { get; set; }

    public virtual DbSet<RefCusPreference> RefCusPreferences { get; set; }

    public virtual DbSet<RefCusPreferenceLanguage> RefCusPreferenceLanguages { get; set; }

    public virtual DbSet<RefCusProcedure> RefCusProcedures { get; set; }

    public virtual DbSet<RefCusProcedureAttribute> RefCusProcedureAttributes { get; set; }

    public virtual DbSet<RefCusProcedureLanguage> RefCusProcedureLanguages { get; set; }

    public virtual DbSet<RefCusProfile> RefCusProfiles { get; set; }

    public virtual DbSet<RefCusProfileAttribute> RefCusProfileAttributes { get; set; }

    public virtual DbSet<RefCusProfileQuestion> RefCusProfileQuestions { get; set; }

    public virtual DbSet<RefCusProfileQuestionAnswerList> RefCusProfileQuestionAnswerLists { get; set; }

    public virtual DbSet<RefCusProfileQuestionAnswerListLanguage> RefCusProfileQuestionAnswerListLanguages { get; set; }

    public virtual DbSet<RefCusProfileQuestionAttribute> RefCusProfileQuestionAttributes { get; set; }

    public virtual DbSet<RefCusProfileQuestionLanguage> RefCusProfileQuestionLanguages { get; set; }

    public virtual DbSet<RefCusProfileQuestionPathway> RefCusProfileQuestionPathways { get; set; }

    public virtual DbSet<RefCusProfileType> RefCusProfileTypes { get; set; }

    public virtual DbSet<RefCusQuota> RefCusQuota { get; set; }

    public virtual DbSet<RefCusRate> RefCusRates { get; set; }

    public virtual DbSet<RefCusRateCode> RefCusRateCodes { get; set; }

    public virtual DbSet<RefCusRateCodeLanguage> RefCusRateCodeLanguages { get; set; }

    public virtual DbSet<RefCusRateType> RefCusRateTypes { get; set; }

    public virtual DbSet<RefCusRateTypeLanguage> RefCusRateTypeLanguages { get; set; }

    public virtual DbSet<RefCusRateUOM> RefCusRateUOMs { get; set; }

    public virtual DbSet<RefCusRuling> RefCusRulings { get; set; }

    public virtual DbSet<RefCusRulingConfig> RefCusRulingConfigs { get; set; }

    public virtual DbSet<RefCusTariff> RefCusTariffs { get; set; }

    public virtual DbSet<RefCusTariffAdditionalCode> RefCusTariffAdditionalCodes { get; set; }

    public virtual DbSet<RefCusTariffAdditionalCodeCategory> RefCusTariffAdditionalCodeCategories { get; set; }

    public virtual DbSet<RefCusTariffAdditionalCodeLanguage> RefCusTariffAdditionalCodeLanguages { get; set; }

    public virtual DbSet<RefCusTariffAttribute> RefCusTariffAttributes { get; set; }

    public virtual DbSet<RefCusTariffAttributeName> RefCusTariffAttributeNames { get; set; }

    public virtual DbSet<RefCusTariffBRCharacteristic> RefCusTariffBRCharacteristics { get; set; }

    public virtual DbSet<RefCusTariffBRCharacteristicAttribute> RefCusTariffBRCharacteristicAttributes { get; set; }

    public virtual DbSet<RefCusTariffBRCharacteristicValue> RefCusTariffBRCharacteristicValues { get; set; }

    public virtual DbSet<RefCusTariffLanguage> RefCusTariffLanguages { get; set; }

    public virtual DbSet<RefCusTariffNationalCode> RefCusTariffNationalCodes { get; set; }

    public virtual DbSet<RefCusTariffRelationship> RefCusTariffRelationships { get; set; }

    public virtual DbSet<RefCusTariffType> RefCusTariffTypes { get; set; }

    public virtual DbSet<RefCusTariffTypeLanguage> RefCusTariffTypeLanguages { get; set; }

    public virtual DbSet<RefCusTariffUOM> RefCusTariffUOMs { get; set; }

    public virtual DbSet<RefCusTaxOrFee> RefCusTaxOrFees { get; set; }

    public virtual DbSet<RefCusTaxOrFeeLanguage> RefCusTaxOrFeeLanguages { get; set; }

    public virtual DbSet<RefCusTaxOrFeeType> RefCusTaxOrFeeTypes { get; set; }

    public virtual DbSet<RefCusTradeGroup> RefCusTradeGroups { get; set; }

    public virtual DbSet<RefCusTradeGroupCountry> RefCusTradeGroupCountries { get; set; }

    public virtual DbSet<RefCusTradeGroupLanguage> RefCusTradeGroupLanguages { get; set; }

    public virtual DbSet<RefCusVATApplicability> RefCusVATApplicabilities { get; set; }

    public virtual DbSet<RefDamage> RefDamages { get; set; }

    public virtual DbSet<RefDocOrgCusCode> RefDocOrgCusCodes { get; set; }

    public virtual DbSet<RefEquipmentGrade> RefEquipmentGrades { get; set; }

    public virtual DbSet<RefErrorReport> RefErrorReports { get; set; }

    public virtual DbSet<RefExchangeRateZZ> RefExchangeRateZZs { get; set; }

    public virtual DbSet<RefFacility> RefFacilities { get; set; }

    public virtual DbSet<RefFacilityLocalCode> RefFacilityLocalCodes { get; set; }

    public virtual DbSet<RefGlbReleaseNote> RefGlbReleaseNotes { get; set; }

    public virtual DbSet<RefHarbourRate> RefHarbourRates { get; set; }

    public virtual DbSet<RefLanguageText> RefLanguageTexts { get; set; }

    public virtual DbSet<RefLocoMap> RefLocoMaps { get; set; }

    public virtual DbSet<RefMRComponentCode> RefMRComponentCodes { get; set; }

    public virtual DbSet<RefMaterial> RefMaterials { get; set; }

    public virtual DbSet<RefMessagingBussCarrierInfo> RefMessagingBussCarrierInfos { get; set; }

    public virtual DbSet<RefMessagingBussPackageInfo> RefMessagingBussPackageInfos { get; set; }

    public virtual DbSet<RefMessagingBussPackageVersion> RefMessagingBussPackageVersions { get; set; }

    public virtual DbSet<RefPortPolygon> RefPortPolygons { get; set; }

    public virtual DbSet<RefRepairCode> RefRepairCodes { get; set; }

    public virtual DbSet<RefShippingLine> RefShippingLines { get; set; }

    public virtual DbSet<RefShippingLineEBLProvider> RefShippingLineEBLProviders { get; set; }

    public virtual DbSet<RefShippingLineMessagingRequirement> RefShippingLineMessagingRequirements { get; set; }

    public virtual DbSet<RefShippingLineMessagingRequirementType> RefShippingLineMessagingRequirementTypes { get; set; }

    public virtual DbSet<RefStlFieldMapping> RefStlFieldMappings { get; set; }

    public virtual DbSet<RefStlScript> RefStlScripts { get; set; }

    public virtual DbSet<RefSysConfig> RefSysConfigs { get; set; }

    public virtual DbSet<RefSysConfigType> RefSysConfigTypes { get; set; }

    public virtual DbSet<RefTimeZone> RefTimeZones { get; set; }

    public virtual DbSet<RefTimeZoneRule> RefTimeZoneRules { get; set; }

    public virtual DbSet<RefTimeZoneSet> RefTimeZoneSets { get; set; }

    public virtual DbSet<RefUNLOCO> RefUNLOCOs { get; set; }

    public virtual DbSet<RefUNLOCOPortMapping> RefUNLOCOPortMappings { get; set; }

    public virtual DbSet<RefUNLOCORelatedPort> RefUNLOCORelatedPorts { get; set; }

    public virtual DbSet<RefUNLOCOUtcOffset> RefUNLOCOUtcOffsets { get; set; }

    public virtual DbSet<RefUnitSection> RefUnitSections { get; set; }

    public virtual DbSet<RefVessel> RefVessels { get; set; }

    public virtual DbSet<RefVesselArrival> RefVesselArrivals { get; set; }

    public virtual DbSet<RefVesselZZ> RefVesselZZs { get; set; }

    public virtual DbSet<SourceData> SourceData { get; set; }

    public virtual DbSet<SourceDataUserView> SourceDataUserViews { get; set; }

    public virtual DbSet<StmNote> StmNotes { get; set; }

    public virtual DbSet<SubscriptionEvent> SubscriptionEvents { get; set; }

    public virtual DbSet<SubscriptionEventType> SubscriptionEventTypes { get; set; }

    public virtual DbSet<SubscriptionNotificationQueue> SubscriptionNotificationQueues { get; set; }

    public virtual DbSet<SystemData> SystemData { get; set; }

    public virtual DbSet<UNDGAttribute> UNDGAttributes { get; set; }

    public virtual DbSet<UNDGAttributeZZ> UNDGAttributeZZs { get; set; }

    public virtual DbSet<UNDGCommonData> UNDGCommonData { get; set; }

    public virtual DbSet<UNDGCountryReference> UNDGCountryReferences { get; set; }

    public virtual DbSet<UNDGCountryReferencePivot> UNDGCountryReferencePivots { get; set; }

    public virtual DbSet<UNDGReference> UNDGReferences { get; set; }

    public virtual DbSet<UNDGSubstance> UNDGSubstances { get; set; }

    public virtual DbSet<UNDGSubstanceADN> UNDGSubstanceADNs { get; set; }

    public virtual DbSet<UNDGSubstanceADR> UNDGSubstanceADRs { get; set; }

    public virtual DbSet<UNDGSubstanceCFR> UNDGSubstanceCFRs { get; set; }

    public virtual DbSet<UNDGSubstanceJTT> UNDGSubstanceJTTs { get; set; }

    public virtual DbSet<UNDGSubstanceRID> UNDGSubstanceRIDs { get; set; }

    public virtual DbSet<UNDGVersion> UNDGVersions { get; set; }

    public virtual DbSet<UniversalXmlSchema> UniversalXmlSchemas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AutoSchema>(entity =>
        {
            entity.HasKey(e => e.AS_PK)
                .HasName("PK_AutoSchema_AS_PK")
                .IsClustered(false);

            entity.Property(e => e.AS_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<DataChangeCapture>(entity =>
        {
            entity.HasKey(e => e.DCC_PK).HasName("PK_DataChangeCapture_DCC_PK");

            entity.Property(e => e.DCC_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DCC_EventTimeUTC).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<DataProcessingClone>(entity =>
        {
            entity.HasKey(e => e.DPC_PK).HasName("PK_DataProcessingClone_DPC_PK");

            entity.Property(e => e.DPC_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<DataProcessingInformation>(entity =>
        {
            entity.HasKey(e => e.DPI_ID)
                .HasName("PK_DataProcessingInformation_DPI_ID")
                .IsClustered(false);

            entity.Property(e => e.DPI_ID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DPI_Message).HasDefaultValueSql("('')");
            entity.Property(e => e.DPI_Status).HasDefaultValueSql("('QUE')");
        });

        modelBuilder.Entity<DataProcessingResult>(entity =>
        {
            entity.HasKey(e => e.DPR_PK).HasName("PK_DataProcessingResult_DPR_PK");

            entity.Property(e => e.DPR_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DPR_Status).HasDefaultValueSql("('QUE')");
            entity.Property(e => e.DPR_SubSource).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<DataSourceInformation>(entity =>
        {
            entity.HasKey(e => e.DSI_PK).HasName("PK_DataSourceInformation_DPR_PK");

            entity.Property(e => e.DSI_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DSI_SubSource).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<NamedEntityClassification>(entity =>
        {
            entity.Property(e => e.NEC_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<ProcessData>(entity =>
        {
            entity.Property(e => e.ID).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<ProcessorStatus>(entity =>
        {
            entity.ToTable("ProcessorStatus", tb => tb.HasTrigger("ProcessorStatus_Modify"));

            entity.Property(e => e.PRC_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.PRC_Status).HasDefaultValueSql("('PRS')");

            entity.HasOne(d => d.PRC_).WithMany(p => p.ProcessorStatuses)
                .HasPrincipalKey(p => new { p.SCHED_NAME, p.JOB_NAME, p.JOB_GROUP })
                .HasForeignKey(d => new { d.PRC_SchedName, d.PRC_JobName, d.PRC_JobGroup })
                .HasConstraintName("FK_ProcessorStatus_QrtzJobDetails");
        });

        modelBuilder.Entity<QRTZ_CRON_TRIGGERS>(entity =>
        {
            entity.HasOne(d => d.QRTZ_TRIGGERS).WithOne(p => p.QRTZ_CRON_TRIGGERS).HasConstraintName("FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS");
        });

        modelBuilder.Entity<QRTZ_JOB_DETAILS>(entity =>
        {
            entity.Property(e => e.JOB_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<QRTZ_SIMPLE_TRIGGERS>(entity =>
        {
            entity.HasOne(d => d.QRTZ_TRIGGERS).WithOne(p => p.QRTZ_SIMPLE_TRIGGERS).HasConstraintName("FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS");
        });

        modelBuilder.Entity<QRTZ_SIMPROP_TRIGGERS>(entity =>
        {
            entity.HasOne(d => d.QRTZ_TRIGGERS).WithOne(p => p.QRTZ_SIMPROP_TRIGGERS).HasConstraintName("FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS");
        });

        modelBuilder.Entity<QRTZ_TRIGGERS>(entity =>
        {
            entity.HasOne(d => d.QRTZ_JOB_DETAILS).WithMany(p => p.QRTZ_TRIGGERS)
                .HasPrincipalKey(p => new { p.SCHED_NAME, p.JOB_NAME, p.JOB_GROUP })
                .HasForeignKey(d => new { d.SCHED_NAME, d.JOB_NAME, d.JOB_GROUP })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS");
        });

        modelBuilder.Entity<RefAccElectronicProcessingFee>(entity =>
        {
            entity.Property(e => e.EPF_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.EPF_CountryCode)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.EPF_JobDirection).HasDefaultValueSql("('')");
            entity.Property(e => e.EPF_ValidFrom).HasDefaultValueSql("('1900-01-01')");
        });

        modelBuilder.Entity<RefAccTaxRate>(entity =>
        {
            entity.Property(e => e.ZAT_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZAT_EndDate).HasDefaultValueSql("('31 December 9999')");
            entity.Property(e => e.ZAT_RN_NKCountry).IsFixedLength();
            entity.Property(e => e.ZAT_RateDenominator).HasDefaultValueSql("((1))");
            entity.Property(e => e.ZAT_StartDate).HasDefaultValueSql("('1 January 1')");
        });

        modelBuilder.Entity<RefAccTaxRateUserView>(entity =>
        {
            entity.ToView("RefAccTaxRateUserView");

            entity.Property(e => e.ZAT_RN_NKCountry).IsFixedLength();
        });

        modelBuilder.Entity<RefAccessorial>(entity =>
        {
            entity.Property(e => e.ASI_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ASI_Code)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.ASI_Description).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefAirline>(entity =>
        {
            entity.Property(e => e.RM_PK).ValueGeneratedNever();
            entity.Property(e => e.RM_AccountingCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_AccountingSecondaryFlag).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_AddressLine1).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_AddressLine2).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_AirlineCity).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_AirlineCountry).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_AirlineName1).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_AirlineName2).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_AirlinePostalCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_AirlinePrefix).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_AirlinePrefixSecondaryFlag).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_AirlineState).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_ContactNameOCIIdentifier).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_ContactPhoneOCIIdentifier).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_EagleAddedAirlinePrefixOrAccountingCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_EmergencyContactName).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_EmergencyContactTitle).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_EmergencyTeletype).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.RM_LabelShortName).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_RN_NKAirlineCountry).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_ReservationsContactName).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_ReservationsContactTeletype).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_ReservationsContactTitle).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_ReservationsDeptTeletype).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_ThreeLetterCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_TwoCharacterCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RM_TypeOfOperationsCode).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefAirlineCommodityCode>(entity =>
        {
            entity.HasKey(e => e.RAC_PK)
                .HasName("PK_UX__RAC_PK")
                .IsClustered(false);

            entity.Property(e => e.RAC_PK).ValueGeneratedNever();
            entity.Property(e => e.RAC_AirlineID).HasDefaultValueSql("('')");
            entity.Property(e => e.RAC_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.RAC_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.RAC_SpecialHandlingCodes).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefAirlineProductCode>(entity =>
        {
            entity.HasKey(e => e.RAR_PK).IsClustered(false);

            entity.Property(e => e.RAR_PK).ValueGeneratedNever();
            entity.Property(e => e.RAR_AirlineID).HasDefaultValueSql("('')");
            entity.Property(e => e.RAR_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.RAR_Description).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefAirlineProductCodeCommodityCodePivot>(entity =>
        {
            entity.HasKey(e => e.RPC_PK).IsClustered(false);

            entity.Property(e => e.RPC_PK).ValueGeneratedNever();
            entity.Property(e => e.RPC_AirlineID).HasDefaultValueSql("('')");

            entity.HasOne(d => d.RPC_RARNavigation).WithMany(p => p.RefAirlineProductCodeCommodityCodePivots)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefAirlineProductCodeCommodityCodePivot_RefAirlineProductCode");
        });

        modelBuilder.Entity<RefApplicationAttribute>(entity =>
        {
            entity.ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("RefApplicationAttributeHistory", "dbo");
                        ttb
                            .HasPeriodStart("RAA_SysStartTime")
                            .HasColumnName("RAA_SysStartTime");
                        ttb
                            .HasPeriodEnd("RAA_SysEndTime")
                            .HasColumnName("RAA_SysEndTime");
                    }));

            entity.Property(e => e.RAA_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RAA_AttributeName).HasDefaultValueSql("('')");
            entity.Property(e => e.RAA_ConfigFilePath).HasDefaultValueSql("('')");
            entity.Property(e => e.RAA_JobGroup).HasDefaultValueSql("('')");
            entity.Property(e => e.RAA_Value).HasDefaultValueSql("('')");

            entity.HasOne(d => d.RAA_RAT_NKTypeNavigation).WithMany(p => p.RefApplicationAttributes)
                .HasPrincipalKey(p => p.RAT_Type)
                .HasForeignKey(d => d.RAA_RAT_NKType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefApplicationAttribute_ApplicationAttributeType");
        });

        modelBuilder.Entity<RefApplicationAttributeType>(entity =>
        {
            entity.ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("RefApplicationAttributeTypeHistory", "dbo");
                        ttb
                            .HasPeriodStart("RAT_SysStartTime")
                            .HasColumnName("RAT_SysStartTime");
                        ttb
                            .HasPeriodEnd("RAT_SysEndTime")
                            .HasColumnName("RAT_SysEndTime");
                    }));

            entity.Property(e => e.RAT_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RAT_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.RAT_Type).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCarrierCode>(entity =>
        {
            entity.Property(e => e.ZZ4_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<RefCarrierCodeAttribute>(entity =>
        {
            entity.Property(e => e.ZZG_PK).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.ZZG_ZZ4_CarrierCodeNavigation).WithMany(p => p.RefCarrierCodeAttributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCarrierCodeAttribute_RefCarrierCode");
        });

        modelBuilder.Entity<RefCarrierCodeLanguage>(entity =>
        {
            entity.Property(e => e.ZCL_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZCL_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.ZCL_ZX6_NKLanguage).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZCL_ZZ4_CarrierCodeNavigation).WithMany(p => p.RefCarrierCodeLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCarrierCodeLanguage_RefCarrierCode");
        });

        modelBuilder.Entity<RefCarrierVesselPivot>(entity =>
        {
            entity.Property(e => e.ZZQ_PK).ValueGeneratedNever();

            entity.HasOne(d => d.ZZQ_ZZ4Navigation).WithMany(p => p.RefCarrierVesselPivots)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCarrierVesselPivot_Carrier");
        });

        modelBuilder.Entity<RefClient>(entity =>
        {
            entity.Property(e => e.RCT_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<RefComplianceCommodityAlert>(entity =>
        {
            entity.Property(e => e.RCR_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RCR_AlertCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RCR_AlertDescription).HasDefaultValueSql("('')");
            entity.Property(e => e.RCR_AlertName).HasDefaultValueSql("('')");
            entity.Property(e => e.RCR_AlertType)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.RCR_CountryRegion)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.RCR_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.RCR_SourceURL).HasDefaultValueSql("('')");
            entity.Property(e => e.RCR_TradeDirection)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
        });

        modelBuilder.Entity<RefComplianceList>(entity =>
        {
            entity.Property(e => e.RCL_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RCL_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.RCL_LastUpdatedDate).HasDefaultValueSql("('2021-06-30')");
            entity.Property(e => e.RCL_ListCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RCL_ListDescription).HasDefaultValueSql("('')");
            entity.Property(e => e.RCL_ListName).HasDefaultValueSql("('')");
            entity.Property(e => e.RCL_ListPublisher).HasDefaultValueSql("('')");
            entity.Property(e => e.RCL_ListType).HasDefaultValueSql("('')");
            entity.Property(e => e.RCL_MainSourceURL).HasDefaultValueSql("('')");
            entity.Property(e => e.RCL_PublisherDescription).HasDefaultValueSql("('')");
            entity.Property(e => e.RCL_PublisherJurisdiction).HasDefaultValueSql("('')");
            entity.Property(e => e.RCL_SecondarySourceURL).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCountry>(entity =>
        {
            entity.HasKey(e => e.RN_PK).HasName("PK_UX__RN_PK");

            entity.Property(e => e.RN_PK).ValueGeneratedNever();
            entity.Property(e => e.RN_AddressFormattingRule).HasDefaultValueSql("('DEF')");
            entity.Property(e => e.RN_Code)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.RN_CountryDialingCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RN_Desc).HasDefaultValueSql("('')");
            entity.Property(e => e.RN_EconomicGrouping).HasDefaultValueSql("('')");
            entity.Property(e => e.RN_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.RN_IsoAlpha3Code).HasDefaultValueSql("('')");
            entity.Property(e => e.RN_IsoNumericUNM49Code).HasDefaultValueSql("('')");
            entity.Property(e => e.RN_PostcodeValidationRule).HasDefaultValueSql("('NVR')");
            entity.Property(e => e.RN_RX_NKAirWaybillCurrency).HasDefaultValueSql("('')");
            entity.Property(e => e.RN_RX_NKLocalCurrency).HasDefaultValueSql("('')");
            entity.Property(e => e.RN_StateProvinceValidationRule).HasDefaultValueSql("('NVR')");
            entity.Property(e => e.RN_ValidationStatus)
                .HasDefaultValueSql("('NAV')")
                .IsFixedLength();
        });

        modelBuilder.Entity<RefCountryStates>(entity =>
        {
            entity.HasKey(e => e.RW_PK)
                .HasName("PK_UX__RW_PK")
                .IsClustered(false);

            entity.Property(e => e.RW_PK).ValueGeneratedNever();
            entity.Property(e => e.RW_RN_NKCountryCode).IsFixedLength();
        });

        modelBuilder.Entity<RefCurrency>(entity =>
        {
            entity.HasKey(e => e.RX_PK).HasName("PK_UX__RX_PK");

            entity.Property(e => e.RX_PK).ValueGeneratedNever();
            entity.Property(e => e.RX_Code)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.RX_Desc).HasDefaultValueSql("('')");
            entity.Property(e => e.RX_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.RX_SubUnitName).HasDefaultValueSql("('')");
            entity.Property(e => e.RX_SubUnitRatio).HasDefaultValueSql("((100))");
            entity.Property(e => e.RX_Symbol).HasDefaultValueSql("('')");
            entity.Property(e => e.RX_UnitName).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCusAUNexdocECMCode>(entity =>
        {
            entity.Property(e => e.ZY5_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZY5_CommodityCode)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.ZY5_PackTypeCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ZY5_PreservationCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ZY5_ProductTypeCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ZY5_SupplementaryCode).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCusApplicability>(entity =>
        {
            entity.Property(e => e.ZZT_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZT_AdditionalCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZT_OrderNumber).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZZT_ZX1_ConditionsNavigation).WithMany(p => p.RefCusApplicabilities).HasConstraintName("FK_RefCusApplicability_RefCusCondition");

            entity.HasOne(d => d.ZZT_ZY2_AdditionalCodeNavigation).WithMany(p => p.RefCusApplicabilities).HasConstraintName("FK_RefCusApplicability_RefCusTariffAdditionalCode");

            entity.HasOne(d => d.ZZT_ZZ2_RateNavigation).WithMany(p => p.RefCusApplicabilities).HasConstraintName("FK_RefCusApplicability_RefCusRate");

            entity.HasOne(d => d.ZZT_ZZH_TariffRelationshipNavigation).WithMany(p => p.RefCusApplicabilities).HasConstraintName("FK_RefCusApplicability_RefCusTariffRelationship");
        });

        modelBuilder.Entity<RefCusCodeList>(entity =>
        {
            entity.Property(e => e.ZZD_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<RefCusCodeListAttribute>(entity =>
        {
            entity.Property(e => e.ZZE_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZE_EndDate).IsSparse();
            entity.Property(e => e.ZZE_StartDate).IsSparse();

            entity.HasOne(d => d.ZZE_ZZD_CodeListNavigation).WithMany(p => p.RefCusCodeListAttributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusCodeListAttribute_RefCusCodeList");
        });

        modelBuilder.Entity<RefCusCodeListAttributeName>(entity =>
        {
            entity.Property(e => e.ZXE_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXE_ColumnCaption).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXE_ValueDataType).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCusCodeListAttributeNameLanguage>(entity =>
        {
            entity.Property(e => e.ZXH_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXH_ColumnCaption).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXH_Description).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZXH_ZXE_CodeListAttributeNameNavigation).WithMany(p => p.RefCusCodeListAttributeNameLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusCodeListAttributeNameLanguage_ZXH_ZXE_CodeListAttributeName");
        });

        modelBuilder.Entity<RefCusCodeListAttributeUserView>(entity =>
        {
            entity.ToView("RefCusCodeListAttributeUserView");
        });

        modelBuilder.Entity<RefCusCodeListLanguage>(entity =>
        {
            entity.Property(e => e.ZXA_PK).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.ZXA_ZZD_CodeListNavigation).WithMany(p => p.RefCusCodeListLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusCodeListLanguage_RefCusCodeList");
        });

        modelBuilder.Entity<RefCusCodeListUserView>(entity =>
        {
            entity.ToView("RefCusCodeListUserView");
        });

        modelBuilder.Entity<RefCusCodeOrAttributeTransportMode>(entity =>
        {
            entity.Property(e => e.ZZU_PK).ValueGeneratedNever();

            entity.HasOne(d => d.ZZU_ZZD_CodeListNavigation).WithMany(p => p.RefCusCodeOrAttributeTransportModes).HasConstraintName("FK_RefCusCodeOrAttributeTransportMode_RefCusCodeList");

            entity.HasOne(d => d.ZZU_ZZE_AttributeNavigation).WithMany(p => p.RefCusCodeOrAttributeTransportModes).HasConstraintName("FK_RefCusCodeOrAttributeTransportMode_RefCusCodeListAttribute");
        });

        modelBuilder.Entity<RefCusCodeType>(entity =>
        {
            entity.Property(e => e.ZZK_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZK_IsReadonly).HasDefaultValueSql("((1))");
        });

        modelBuilder.Entity<RefCusCodeTypeLanguage>(entity =>
        {
            entity.Property(e => e.ZXI_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXI_Description).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZXI_ZZK_CodeTypeNavigation).WithMany(p => p.RefCusCodeTypeLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusCodeTypeLanguage_ZXI_ZZK_CodeType");
        });

        modelBuilder.Entity<RefCusCondition>(entity =>
        {
            entity.Property(e => e.ZX1_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZX1_AdditionalComment).HasDefaultValueSql("('')");
            entity.Property(e => e.ZX1_Comment).HasDefaultValueSql("('')");
            entity.Property(e => e.ZX1_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.ZX1_Severity)
                .IsFixedLength()
                .IsSparse();
            entity.Property(e => e.ZX1_Source).HasDefaultValueSql("('')");
            entity.Property(e => e.ZX1_StartDate).HasDefaultValueSql("('1900-01-01')");
            entity.Property(e => e.ZX1_ZX2_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZX1_ZZ1_TariffNavigation).WithMany(p => p.RefCusConditions).HasConstraintName("FK_RefCusCondition_RefCusTariff");

            entity.HasOne(d => d.ZX1_ZZ5_NomenclatureNavigation).WithMany(p => p.RefCusConditions).HasConstraintName("FK_RefCusCondition_RefCusNomenclatureGroup");
        });

        modelBuilder.Entity<RefCusConditionCode>(entity =>
        {
            entity.Property(e => e.ZY7_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<RefCusConditionCodeLanguage>(entity =>
        {
            entity.Property(e => e.ZY8_PK).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.ZY8_ZY7_ConditionCodeNavigation).WithMany(p => p.RefCusConditionCodeLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusConditionLanguageCode_RefCusConditionCode");
        });

        modelBuilder.Entity<RefCusConditionLanguage>(entity =>
        {
            entity.HasKey(e => e.ZXJ_PK).IsClustered(false);

            entity.Property(e => e.ZXJ_PK).ValueGeneratedNever();
            entity.Property(e => e.ZXJ_AdditionalComment).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXJ_Comment).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXJ_Source).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXJ_ZX6_NKLanguage).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZXJ_ZX1_ConditionNavigation).WithMany(p => p.RefCusConditionLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusConditionLanguage_RefCusCondition");
        });

        modelBuilder.Entity<RefCusConditionType>(entity =>
        {
            entity.Property(e => e.ZX2_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<RefCusConditionTypeLanguage>(entity =>
        {
            entity.Property(e => e.ZXW_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXW_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXW_ZX6_NKLanguage).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZXW_ZX2_ConditionTypeNavigation).WithMany(p => p.RefCusConditionTypeLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusConditionTypeLanguage_ZXW_ZX2_ConditionType");
        });

        modelBuilder.Entity<RefCusConditionValue>(entity =>
        {
            entity.Property(e => e.ZX3_PK).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.ZX3_ZX1_ConditionNavigation).WithMany(p => p.RefCusConditionValues)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusConditionValue_RefCusCondition");
        });

        modelBuilder.Entity<RefCusConditionValueType>(entity =>
        {
            entity.Property(e => e.ZX4_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<RefCusConditionValueTypeLanguage>(entity =>
        {
            entity.Property(e => e.ZXX_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXX_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXX_ZX6_NKLanguage).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZXX_ZX4_ValueTypeNavigation).WithMany(p => p.RefCusConditionValueTypeLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusConditionValueTypeLanguage_ZXX_ZX4_ValueType");
        });

        modelBuilder.Entity<RefCusConfiguration>(entity =>
        {
            entity.HasKey(e => e.ZZJ_PK).IsClustered(false);

            entity.Property(e => e.ZZJ_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZJ_RN_NKCustomsCountry).IsFixedLength();
            entity.Property(e => e.ZZJ_StartDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ZZJ_TariffDataSource).IsFixedLength();
        });

        modelBuilder.Entity<RefCusExcludedTradeGroup>(entity =>
        {
            entity.Property(e => e.ZZC_PK).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.ZZC_ZZT_ApplicabilityNavigation).WithMany(p => p.RefCusExcludedTradeGroups)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusExcludedTradeGroup_RefCusApplicability");
        });

        modelBuilder.Entity<RefCusMap>(entity =>
        {
            entity.Property(e => e.ZZM_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<RefCusMapType>(entity =>
        {
            entity.Property(e => e.ZZP_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZP_Direction).HasDefaultValueSql("('BTH')");
        });

        modelBuilder.Entity<RefCusNomenclatureGroup>(entity =>
        {
            entity.Property(e => e.ZZ5_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZ5_Value).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ5_ZZ9_NKNomenclatureGroupType).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCusNomenclatureGroupNote>(entity =>
        {
            entity.Property(e => e.ZZL_PK).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.ZZL_ZZ5_NomenclatureGroupNavigation).WithMany(p => p.RefCusNomenclatureGroupNotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusNomenclatureGroupNote_RefCusNomenclatureGroup");
        });

        modelBuilder.Entity<RefCusNomenclatureGroupType>(entity =>
        {
            entity.Property(e => e.ZZ9_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<RefCusNomenclatureLanguage>(entity =>
        {
            entity.Property(e => e.ZX8_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZX8_Description).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZX8_ZZ5_NomenclatureGroupNavigation).WithMany(p => p.RefCusNomenclatureLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusNomenclatureLanguage_RefCusNomenclatureGroup");
        });

        modelBuilder.Entity<RefCusPreference>(entity =>
        {
            entity.Property(e => e.ZZS_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<RefCusPreferenceLanguage>(entity =>
        {
            entity.Property(e => e.ZX9_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZX9_Description).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZX9_ZZS_PreferenceNavigation).WithMany(p => p.RefCusPreferenceLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusPreferenceLanguage_RefCusPreference");
        });

        modelBuilder.Entity<RefCusProcedure>(entity =>
        {
            entity.Property(e => e.ZZ6_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZ6_CalculateDuty).HasDefaultValueSql("((1))");
            entity.Property(e => e.ZZ6_CalculateVAT).HasDefaultValueSql("((1))");
            entity.Property(e => e.ZZ6_Category).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ6_Concession).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ6_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.ZZ6_Group).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ6_IntoInwardProcessing)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_IntoOutwardProcessing)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_IntoTemporaryExport)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_IntoTemporaryImport)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_IntoVATWarehouse)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_IntoWarehouse)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_IsGuaranteeConsumed)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_IsGuaranteeReleased)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_IsTransit)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_OutOfInwardProcessing)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_OutOfTemporaryExport)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_OutOfTemporaryImport)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_OutOfVATWarehouse)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_OutOfWarehouse)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_OutofOutwardProcessing)
                .HasDefaultValueSql("('N')")
                .IsFixedLength();
            entity.Property(e => e.ZZ6_PreviousProcedureCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ6_ShipmentType).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ6_StartDate).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<RefCusProcedureAttribute>(entity =>
        {
            entity.Property(e => e.ZXB_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXB_Name).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXB_Value).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZXB_ZZ6_ProcedureCodeNavigation).WithMany(p => p.RefCusProcedureAttributes).HasConstraintName("FK_RefCusProcedureAttribute_RefCusProcedure");
        });

        modelBuilder.Entity<RefCusProcedureLanguage>(entity =>
        {
            entity.Property(e => e.ZXV_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXV_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXV_ZX6_NKLanguage).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZXV_ZZ6_ProcedureNavigation).WithMany(p => p.RefCusProcedureLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusProcedureLanguage_ZXV_ZZ6_Procedure");
        });

        modelBuilder.Entity<RefCusProfile>(entity =>
        {
            entity.Property(e => e.XX0_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.XX0_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.XX0_QuestionCode).HasDefaultValueSql("('')");
            entity.Property(e => e.XX0_StartDate).HasDefaultValueSql("('1900-01-01')");
            entity.Property(e => e.XX0_TariffCode).HasDefaultValueSql("('')");
            entity.Property(e => e.XX0_XXX_NKProfileType).HasDefaultValueSql("('')");
            entity.Property(e => e.XX0_XXX_ZZI_NKTariffType).HasDefaultValueSql("('')");
            entity.Property(e => e.XX0_XXX_ZZI_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");
            entity.Property(e => e.XX0_XXX_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCusProfileAttribute>(entity =>
        {
            entity.Property(e => e.XXY_PK).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.XXY_XX0_ProfileNavigation).WithMany(p => p.RefCusProfileAttributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusProfileAttribute_RefCusProfile");
        });

        modelBuilder.Entity<RefCusProfileQuestion>(entity =>
        {
            entity.Property(e => e.XQ2_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.XQ2_AnswerMask).HasDefaultValueSql("('')");
            entity.Property(e => e.XQ2_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.XQ2_Note).HasDefaultValueSql("('')");
            entity.Property(e => e.XQ2_StartDate).HasDefaultValueSql("('1900-01-01')");
            entity.Property(e => e.XQ2_XXX_NKProfileType).HasDefaultValueSql("('')");
            entity.Property(e => e.XQ2_XXX_ZZI_NKTariffType).HasDefaultValueSql("('')");
            entity.Property(e => e.XQ2_XXX_ZZI_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");
            entity.Property(e => e.XQ2_XXX_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCusProfileQuestionAnswerList>(entity =>
        {
            entity.Property(e => e.XQ4_PK).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.XQ4_XQ2_QuestionNavigation).WithMany(p => p.RefCusProfileQuestionAnswerLists)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusProfileQuestionAnswerList_RefCusProfileQuestion");
        });

        modelBuilder.Entity<RefCusProfileQuestionAnswerListLanguage>(entity =>
        {
            entity.Property(e => e.XAL_PK).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.XAL_XQ4_QuestionAnswerNavigation).WithMany(p => p.RefCusProfileQuestionAnswerListLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusProfileQuestionAnswerListLanguage_XAL_XQ4_QuestionAnswer");
        });

        modelBuilder.Entity<RefCusProfileQuestionAttribute>(entity =>
        {
            entity.Property(e => e.XQ3_PK).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.XQ3_XQ2_QuestionNavigation).WithMany(p => p.RefCusProfileQuestionAttributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusProfileQuestionAttribute_RefCusProfileQuestion");
        });

        modelBuilder.Entity<RefCusProfileQuestionLanguage>(entity =>
        {
            entity.Property(e => e.XQL_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.XQL_Note).HasDefaultValueSql("('')");

            entity.HasOne(d => d.XQL_XQ2_QuestionNavigation).WithMany(p => p.RefCusProfileQuestionLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusProfileQuestionLanguage_XQL_XQ2_Question");
        });

        modelBuilder.Entity<RefCusProfileQuestionPathway>(entity =>
        {
            entity.Property(e => e.XQP_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.XQP_ConditionToProceedFormula).HasDefaultValueSql("('')");
            entity.Property(e => e.XQP_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.XQP_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.XQP_StartDate).HasDefaultValueSql("('1900-01-01')");
            entity.Property(e => e.XQP_XQ2_NKQuestionChild).HasDefaultValueSql("('')");
            entity.Property(e => e.XQP_XQ2_NKQuestionParent).HasDefaultValueSql("('')");
            entity.Property(e => e.XQP_XQ2_NKQuestionStartDateChild).HasDefaultValueSql("('1900-01-01')");
            entity.Property(e => e.XQP_XQ2_NKQuestionStartDateParent).HasDefaultValueSql("('1900-01-01')");
            entity.Property(e => e.XQP_XQ2_XXX_NKProfileType).HasDefaultValueSql("('')");
            entity.Property(e => e.XQP_XQ2_XXX_ZZI_NKTariffType).HasDefaultValueSql("('')");
            entity.Property(e => e.XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");
            entity.Property(e => e.XQP_XQ2_XXX_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");
            entity.Property(e => e.XQP_XQ2_ZZZ_NKDataGroupingChild).HasDefaultValueSql("('')");
            entity.Property(e => e.XQP_XQ2_ZZZ_NKDataGroupingParent).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCusProfileType>(entity =>
        {
            entity.Property(e => e.XXX_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.XXX_ZZI_NKTariffType).HasDefaultValueSql("('')");
            entity.Property(e => e.XXX_ZZI_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCusQuota>(entity =>
        {
            entity.Property(e => e.ZXQ_PK).ValueGeneratedNever();
            entity.Property(e => e.ZXQ_OrderNumber).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXQ_UnitOfMeasure).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXQ_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCusRate>(entity =>
        {
            entity.Property(e => e.ZZ2_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZ2_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.ZZ2_RX_NKCurrencyOverride).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ2_SelectorFormula).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ2_StartDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ZZ2_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZZ2_ZZ1_TariffNavigation).WithMany(p => p.RefCusRates).HasConstraintName("FK_RefCusRate_RefCusTariff");

            entity.HasOne(d => d.ZZ2_ZZW_TariffNationalCodeNavigation).WithMany(p => p.RefCusRates).HasConstraintName("FK_RefCusRate_RefCusTariffNationalCode");
        });

        modelBuilder.Entity<RefCusRateCode>(entity =>
        {
            entity.Property(e => e.ZY1_PK).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.ZY1_ZZR_RateTypeNavigation).WithMany(p => p.RefCusRateCodes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusRateCode_RefCusRateType");
        });

        modelBuilder.Entity<RefCusRateCodeLanguage>(entity =>
        {
            entity.Property(e => e.ZXC_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXC_Description).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZXC_ZY1_RateCodeNavigation).WithMany(p => p.RefCusRateCodeLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusRateCodeLanguage_RefCusRateCode");
        });

        modelBuilder.Entity<RefCusRateType>(entity =>
        {
            entity.Property(e => e.ZZR_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZR_CustomsValueFormula).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZR_RX_NKFormulaCurrency).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCusRateTypeLanguage>(entity =>
        {
            entity.Property(e => e.ZXT_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXT_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXT_ZX6_NKLanguage).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZXT_ZZR_RateTypeNavigation).WithMany(p => p.RefCusRateTypeLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusRateTypeLanguage_RefCusRateType");
        });

        modelBuilder.Entity<RefCusRateUOM>(entity =>
        {
            entity.Property(e => e.ZXG_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXG_UOM).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZXG_ZZ2_RateNavigation).WithMany(p => p.RefCusRateUOMs).HasConstraintName("FK_RefCusRateUOM_RefCusRate");
        });

        modelBuilder.Entity<RefCusRuling>(entity =>
        {
            entity.Property(e => e.ZZX_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZX_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.ZZX_RN_NKCountryCode).IsFixedLength();
            entity.Property(e => e.ZZX_RulingType).IsFixedLength();
            entity.Property(e => e.ZZX_StartDate).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<RefCusRulingConfig>(entity =>
        {
            entity.Property(e => e.ZZY_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZY_Value).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZZY_ZZX_CusRulingNavigation).WithMany(p => p.RefCusRulingConfigs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusRulingConfig_ZZY_ZZX_CusRuling");
        });

        modelBuilder.Entity<RefCusTariff>(entity =>
        {
            entity.Property(e => e.ZZ1_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZ1_CompositeKeyOnZZ5).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ1_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ1_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.ZZ1_StartDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ZZ1_ZZF_NKTaxOrFeeCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ1_ZZI_NKTariffType).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ1_ZZI_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ1_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCusTariffAdditionalCode>(entity =>
        {
            entity.Property(e => e.ZY2_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZY2_AdditionalCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ZY2_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.ZY2_IsMandatory).HasDefaultValueSql("((0))");
            entity.Property(e => e.ZY2_ParentAdditionalCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ZY2_ZY3_NKCategory).IsFixedLength();
            entity.Property(e => e.ZY2_ZY3_NKParentCategory)
                .HasDefaultValueSql("('')")
                .IsFixedLength();

            entity.HasOne(d => d.ZY2_ZZ1_TariffNavigation).WithMany(p => p.RefCusTariffAdditionalCodes).HasConstraintName("FK_RefCusTariffAdditionalCode_RefCusTariff");

            entity.HasOne(d => d.ZY2_ZZW_NationalCodeNavigation).WithMany(p => p.RefCusTariffAdditionalCodes).HasConstraintName("FK_RefCusTariffAdditionalCode_RefCusTariffNationalCode");
        });

        modelBuilder.Entity<RefCusTariffAdditionalCodeCategory>(entity =>
        {
            entity.Property(e => e.ZY3_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZY3_Category)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.ZY3_Description).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCusTariffAdditionalCodeLanguage>(entity =>
        {
            entity.Property(e => e.ZY4_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZY4_Description).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZY4_ZY2_TariffAdditionalCodeNavigation).WithMany(p => p.RefCusTariffAdditionalCodeLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusTariffAdditionalCodeLanguage_RefCusTariffAdditionalCode");
        });

        modelBuilder.Entity<RefCusTariffAttribute>(entity =>
        {
            entity.Property(e => e.ZZ3_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZ3_Name).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZ3_Value).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZZ3_ZZ1_TariffNavigation).WithMany(p => p.RefCusTariffAttributes).HasConstraintName("FK_RefCusTariffAttribute_RefCusTariff");

            entity.HasOne(d => d.ZZ3_ZZW_TariffNationalCodeNavigation).WithMany(p => p.RefCusTariffAttributes).HasConstraintName("FK_RefCusTariffAttribute_RefCusTariffNationalCode");
        });

        modelBuilder.Entity<RefCusTariffAttributeName>(entity =>
        {
            entity.HasKey(e => e.ZY6_PK).IsClustered(false);

            entity.HasIndex(e => new { e.ZY6_ZZZ_NKDataGrouping, e.ZY6_ZZI_NKTariffType, e.ZY6_Name }, "IX_RefCusTariffAttributeName_ZY6_ZZZ_NKDataGrouping_ZY6_ZZI_NKTariffType_ZY6_Name").IsClustered();

            entity.Property(e => e.ZY6_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZY6_ColumnCaption).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefCusTariffBRCharacteristic>(entity =>
        {
            entity.HasIndex(e => e.ZB1_ZZ1_Tariff, "IX_RefCusTariffBRCharacteristic_ZB1_ZZ1_Tariff").HasFilter("([ZB1_ZZ1_Tariff] IS NOT NULL)");

            entity.HasIndex(e => e.ZB1_ZZ5_Nomenclature, "IX_RefCusTariffBRCharacteristic_ZB1_ZZ5_Nomenclature").HasFilter("([ZB1_ZZ5_Nomenclature] IS NOT NULL)");

            entity.Property(e => e.ZB1_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZB1_CharacteristicType).HasDefaultValueSql("('')");
            entity.Property(e => e.ZB1_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.ZB1_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.ZB1_StartDate).HasDefaultValueSql("('1900-01-01')");
            entity.Property(e => e.ZB1_Style).HasDefaultValueSql("('')");
            entity.Property(e => e.ZB1_Text).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZB1_ZZ1_TariffNavigation).WithMany(p => p.RefCusTariffBRCharacteristics).HasConstraintName("FK_RefCusTariffBRCharacteristic_RefCusTariff");

            entity.HasOne(d => d.ZB1_ZZ5_NomenclatureNavigation).WithMany(p => p.RefCusTariffBRCharacteristics).HasConstraintName("FK_RefCusTariffBRCharacteristic_RefCusNomenclatureGroup");
        });

        modelBuilder.Entity<RefCusTariffBRCharacteristicAttribute>(entity =>
        {
            entity.Property(e => e.ZB3_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZB3_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.ZB3_Name).HasDefaultValueSql("('')");
            entity.Property(e => e.ZB3_Value).HasDefaultValueSql("('')");
            entity.Property(e => e.ZB3_ZB1_Characteristic).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZB3_ZB1_CharacteristicNavigation).WithMany(p => p.RefCusTariffBRCharacteristicAttributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusTariffBRCharacteristicAttribute_RefCusTariffBRCharacteristic");
        });

        modelBuilder.Entity<RefCusTariffBRCharacteristicValue>(entity =>
        {
            entity.Property(e => e.ZB2_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZB2_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.ZB2_Value).HasDefaultValueSql("('')");
            entity.Property(e => e.ZB2_ZB1_Characteristic).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZB2_ZB1_CharacteristicNavigation).WithMany(p => p.RefCusTariffBRCharacteristicValues)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusTariffBRCharacteristicValue_RefCusTariffBRCharacteristic");
        });

        modelBuilder.Entity<RefCusTariffLanguage>(entity =>
        {
            entity.Property(e => e.ZX7_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZX7_Description).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZX7_ZZ1_TariffNavigation).WithMany(p => p.RefCusTariffLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusTariffLanguage_RefCusTariff");
        });

        modelBuilder.Entity<RefCusTariffNationalCode>(entity =>
        {
            entity.Property(e => e.ZZW_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZW_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.ZZW_NationalCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZW_StartDate).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.ZZW_ZZ1_TariffNavigation).WithMany(p => p.RefCusTariffNationalCodes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusTariffNationalCode_RefCusTariff");
        });

        modelBuilder.Entity<RefCusTariffRelationship>(entity =>
        {
            entity.Property(e => e.ZZH_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZH_TariffCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZH_ZZI_NKTariffType).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZH_ZZI_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZZH_ZZ1_TariffNavigation).WithMany(p => p.RefCusTariffRelationships)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusTariffRelationship_RefCusTariff");
        });

        modelBuilder.Entity<RefCusTariffType>(entity =>
        {
            entity.Property(e => e.ZZI_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZI_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZI_TariffType).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZI_ZZ9_NKNomenclatureGroupType).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZI_ZZR_NKRateType).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZI_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZZI_ZZR_RateTypeNavigation).WithMany(p => p.RefCusTariffTypes)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RefCusTariffType_RefCusRateType");
        });

        modelBuilder.Entity<RefCusTariffTypeLanguage>(entity =>
        {
            entity.Property(e => e.ZXK_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXK_ZX6_NKLanguage).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZXK_ZZI_TariffTypeNavigation).WithMany(p => p.RefCusTariffTypeLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusTariffTypeLanguage_RefCusTariffType");
        });

        modelBuilder.Entity<RefCusTariffUOM>(entity =>
        {
            entity.Property(e => e.ZZ8_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZ8_EndDate).IsSparse();
            entity.Property(e => e.ZZ8_StartDate).IsSparse();
            entity.Property(e => e.ZZ8_ZZZ_NKDataGrouping).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZZ8_ZZ1_TariffNavigation).WithMany(p => p.RefCusTariffUOMs).HasConstraintName("FK_RefCusTariffUOM_RefCusTariff");

            entity.HasOne(d => d.ZZ8_ZZW_TariffNationalCodeNavigation).WithMany(p => p.RefCusTariffUOMs).HasConstraintName("FK_RefCusTariffUOM_RefCusTariffNationalCode");
        });

        modelBuilder.Entity<RefCusTaxOrFee>(entity =>
        {
            entity.Property(e => e.ZZF_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZF_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.ZZF_StartDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ZZF_ZX0_NKTaxOrFeeType).HasDefaultValueSql("('OTH')");
        });

        modelBuilder.Entity<RefCusTaxOrFeeLanguage>(entity =>
        {
            entity.Property(e => e.ZXU_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXU_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXU_ZX6_NKLanguage).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZXU_ZZF_TaxOrFeeNavigation).WithMany(p => p.RefCusTaxOrFeeLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusTaxOrFeeLanguage_ZXU_ZZF_TaxOrFee");
        });

        modelBuilder.Entity<RefCusTaxOrFeeType>(entity =>
        {
            entity.Property(e => e.ZX0_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<RefCusTradeGroup>(entity =>
        {
            entity.Property(e => e.ZZA_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZA_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.ZZA_StartDate).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<RefCusTradeGroupCountry>(entity =>
        {
            entity.Property(e => e.ZZB_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZB_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZB_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.ZZB_RN_NKTradeGroupCountryCode).IsFixedLength();
            entity.Property(e => e.ZZB_StartDate).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.ZZB_ZZA_TradeGroupNavigation).WithMany(p => p.RefCusTradeGroupCountries)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusTradeGroupCountry_RefCusTradeGroup");
        });

        modelBuilder.Entity<RefCusTradeGroupLanguage>(entity =>
        {
            entity.Property(e => e.ZXD_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXD_Description).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZXD_ZZA_TradeGroupNavigation).WithMany(p => p.RefCusTradeGroupLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefCusTradeGroupLanguage_RefCusTradeGroup");
        });

        modelBuilder.Entity<RefCusVATApplicability>(entity =>
        {
            entity.Property(e => e.ZX5_PK).ValueGeneratedNever();
            entity.Property(e => e.ZX5_AdditionalCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ZX5_Description).HasDefaultValueSql("(N'')");
            entity.Property(e => e.ZX5_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.ZX5_StartDate).HasDefaultValueSql("('1900-01-01')");
            entity.Property(e => e.ZX5_VATCategory).HasDefaultValueSql("('')");
            entity.Property(e => e.ZX5_ZZF_NKTaxOrFeeCode).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZX5_ZZ1_TariffNavigation).WithMany(p => p.RefCusVATApplicabilities).HasConstraintName("FK_RefCusVATApplicability_RefCusTariff");

            entity.HasOne(d => d.ZX5_ZZW_TariffNationalCodeNavigation).WithMany(p => p.RefCusVATApplicabilities).HasConstraintName("FK_RefCusVATApplicability_RefCusTariffNationalCode");
        });

        modelBuilder.Entity<RefDamage>(entity =>
        {
            entity.Property(e => e.RFM_PK).ValueGeneratedNever();
            entity.Property(e => e.RFM_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.RFM_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.RFM_Group).HasDefaultValueSql("('')");
            entity.Property(e => e.RFM_IsActive).HasDefaultValueSql("((1))");
        });

        modelBuilder.Entity<RefDocOrgCusCode>(entity =>
        {
            entity.Property(e => e.DOC_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DOC_CodeType)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.DOC_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.DOC_Direction).HasDefaultValueSql("('BTH')");
            entity.Property(e => e.DOC_DocumentType).HasDefaultValueSql("('')");
            entity.Property(e => e.DOC_LongLabel).HasDefaultValueSql("('')");
            entity.Property(e => e.DOC_Notes).HasDefaultValueSql("('')");
            entity.Property(e => e.DOC_RN_NKCodeCountry).HasDefaultValueSql("('')");
            entity.Property(e => e.DOC_RN_NKRegulatingCountry).HasDefaultValueSql("('')");
            entity.Property(e => e.DOC_ShortLabel).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefEquipmentGrade>(entity =>
        {
            entity.Property(e => e.REG_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.REG_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.REG_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.REG_IsActive).HasDefaultValueSql("((1))");
        });

        modelBuilder.Entity<RefErrorReport>(entity =>
        {
            entity.HasKey(e => e.RER_PK)
                .HasName("PK_RER_RefErrorReport")
                .IsClustered(false);

            entity.Property(e => e.RER_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RER_JobName).HasDefaultValueSql("('')");
            entity.Property(e => e.RER_Status)
                .HasDefaultValueSql("('QUE')")
                .IsFixedLength();

            entity.HasOne(d => d.RER_SDANavigation).WithMany(p => p.RefErrorReports).HasConstraintName("FK_RER_RefErrorReport_SourceData");
        });

        modelBuilder.Entity<RefExchangeRateZZ>(entity =>
        {
            entity.Property(e => e.ZZN_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZN_AsPublished).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZN_RN_NKCountry).IsFixedLength();
            entity.Property(e => e.ZZN_RX_NKExCurrency).IsFixedLength();
        });

        modelBuilder.Entity<RefFacility>(entity =>
        {
            entity.HasKey(e => e.RFT_PK).IsClustered(false);

            entity.Property(e => e.RFT_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RFT_Address1).HasDefaultValueSql("('')");
            entity.Property(e => e.RFT_Address2).HasDefaultValueSql("('')");
            entity.Property(e => e.RFT_BICCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RFT_City).HasDefaultValueSql("('')");
            entity.Property(e => e.RFT_Code)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.RFT_FacilityType)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.RFT_GeoLocation).HasDefaultValueSql("([geography]::STPointFromText('POINT EMPTY',(4326)))");
            entity.Property(e => e.RFT_IATACode).HasDefaultValueSql("('')");
            entity.Property(e => e.RFT_IHSGlobalPortId).HasDefaultValueSql("('')");
            entity.Property(e => e.RFT_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.RFT_Name).HasDefaultValueSql("('')");
            entity.Property(e => e.RFT_PostCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RFT_RL_NKLocationCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RFT_RN_NKCountryCode)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.RFT_SMDGCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RFT_State).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefFacilityLocalCode>(entity =>
        {
            entity.HasKey(e => e.RFL_PK).IsClustered(false);

            entity.Property(e => e.RFL_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RFL_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.RFL_RFT_NKFacilityCode).IsFixedLength();
            entity.Property(e => e.RFL_RN_NKCountryCode)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.RFL_Usage)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
        });

        modelBuilder.Entity<RefGlbReleaseNote>(entity =>
        {
            entity.Property(e => e.ZGF_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZGF_Category).HasDefaultValueSql("('')");
            entity.Property(e => e.ZGF_MinVersion).HasDefaultValueSql("('')");
            entity.Property(e => e.ZGF_RN_NKCountryForReleaseNote).HasDefaultValueSql("('')");
            entity.Property(e => e.ZGF_Section).HasDefaultValueSql("('C1U')");
            entity.Property(e => e.ZGF_Summary).HasDefaultValueSql("('')");
            entity.Property(e => e.ZGF_URL).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefHarbourRate>(entity =>
        {
            entity.Property(e => e.ZXF_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZXF_Commodity).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXF_EndDate).HasDefaultValueSql("('2079-06-06 23:59')");
            entity.Property(e => e.ZXF_Port).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXF_PortTaxType).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXF_RateFormula).HasDefaultValueSql("('')");
            entity.Property(e => e.ZXF_StartDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ZXF_Type).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefLanguageText>(entity =>
        {
            entity.HasKey(e => e.RLT_PK).HasName("PK_UX__RLT_PK");

            entity.Property(e => e.RLT_PK).ValueGeneratedNever();
        });

        modelBuilder.Entity<RefLocoMap>(entity =>
        {
            entity.Property(e => e.RY_PK).ValueGeneratedNever();
            entity.Property(e => e.RY_LocalPortCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RY_RL_NKLocoPort).HasDefaultValueSql("('')");
            entity.Property(e => e.RY_RN_NKCountryCode).IsFixedLength();
            entity.Property(e => e.RY_SystemUsage).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefMRComponentCode>(entity =>
        {
            entity.Property(e => e.RCC_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RCC_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.RCC_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.RCC_Group).HasDefaultValueSql("('')");
            entity.Property(e => e.RCC_IsActive).HasDefaultValueSql("((1))");
        });

        modelBuilder.Entity<RefMaterial>(entity =>
        {
            entity.Property(e => e.RMC_PK).ValueGeneratedNever();
            entity.Property(e => e.RMC_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.RMC_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.RMC_Group).HasDefaultValueSql("('')");
            entity.Property(e => e.RMC_IsActive).HasDefaultValueSql("((1))");
        });

        modelBuilder.Entity<RefMessagingBussCarrierInfo>(entity =>
        {
            entity.HasKey(e => e.ZMC_PK).IsClustered(false);

            entity.Property(e => e.ZMC_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZMC_CarrierCode)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.ZMC_CarrierName).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZMC_ZMP_PackageInfoNavigation).WithMany(p => p.RefMessagingBussCarrierInfos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefMessagingBussCarrierInfo_RefMessagingBussPackageInfo");
        });

        modelBuilder.Entity<RefMessagingBussPackageInfo>(entity =>
        {
            entity.HasKey(e => e.ZMP_PK).IsClustered(false);

            entity.Property(e => e.ZMP_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZMP_PackageName).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefMessagingBussPackageVersion>(entity =>
        {
            entity.HasKey(e => e.ZMV_PK).IsClustered(false);

            entity.Property(e => e.ZMV_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZMV_Version).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZMV_ZMP_PackageInfoNavigation).WithMany(p => p.RefMessagingBussPackageVersions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefMessagingBussPackageVersion_RefMessagingBussPackageInfo");
        });

        modelBuilder.Entity<RefPortPolygon>(entity =>
        {
            entity.HasKey(e => e.RPP_PK)
                .HasName("PK_UX_RPP_PK")
                .IsClustered(false);

            entity.Property(e => e.RPP_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RPP_SerializedPolygon).HasDefaultValueSql("([geography]::STPointFromText('POINT EMPTY',(4326)))");
        });

        modelBuilder.Entity<RefRepairCode>(entity =>
        {
            entity.Property(e => e.RRC_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RRC_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.RRC_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.RRC_Group).HasDefaultValueSql("('')");
            entity.Property(e => e.RRC_IsActive).HasDefaultValueSql("('')");
            entity.Property(e => e.RRC_ServiceType)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
        });

        modelBuilder.Entity<RefShippingLine>(entity =>
        {
            entity.Property(e => e.RSL_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RSL_CargoWiseOneCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RSL_CarrierName).HasDefaultValueSql("('')");
            entity.Property(e => e.RSL_EHubIds).HasDefaultValueSql("('')");
            entity.Property(e => e.RSL_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.RSL_StandardCarrierAlphaCode).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefShippingLineEBLProvider>(entity =>
        {
            entity.HasKey(e => e.RSE_PK).IsClustered(false);

            entity.HasIndex(e => e.RSE_RSL_ShippingLine, "IX_RefShippingLineEBLProvider_RSE_RSL_ShippingLine").IsClustered();

            entity.Property(e => e.RSE_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RSE_Name).HasDefaultValueSql("('')");

            entity.HasOne(d => d.RSE_RSL_ShippingLineNavigation).WithMany(p => p.RefShippingLineEBLProviders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefShippingLineEBLProvider_RefShippingLine");
        });

        modelBuilder.Entity<RefShippingLineMessagingRequirement>(entity =>
        {
            entity.Property(e => e.RSR_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RSR_RST_NKType)
                .HasDefaultValueSql("('')")
                .IsFixedLength();

            entity.HasOne(d => d.RSR_RSL_ShippingLineNavigation).WithMany(p => p.RefShippingLineMessagingRequirements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefShippingLineMessagingRequirement_RefShippingLine");
        });

        modelBuilder.Entity<RefShippingLineMessagingRequirementType>(entity =>
        {
            entity.Property(e => e.RST_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RST_Code)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.RST_Description).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefStlFieldMapping>(entity =>
        {
            entity.Property(e => e.SFM_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.SFM_FeatureCode)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
        });

        modelBuilder.Entity<RefStlScript>(entity =>
        {
            entity.Property(e => e.STL_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.STL_ActiveOn)
                .HasDefaultValueSql("('ALL')")
                .IsFixedLength();
            entity.Property(e => e.STL_AdditionalRefs).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_BillingReference1).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_BillingReference2).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_BillingReference3).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_BillingReference4).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_BranchCode).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_CompanyCode).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_CreatingUserCode).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_DataGranularity)
                .HasDefaultValueSql("('TRN')")
                .IsFixedLength();
            entity.Property(e => e.STL_DateType)
                .HasDefaultValueSql("('DTE')")
                .IsFixedLength();
            entity.Property(e => e.STL_FeatureCode)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.STL_FeatureName).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_FromClause).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_FunctionName).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_GuidReference).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_MaxCW1Version).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_MinCW1Version).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_ModuleName).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_PreparationScript).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_RoleName).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_TransactionCount).HasDefaultValueSql("('1')");
            entity.Property(e => e.STL_TransactionDateUtc).HasDefaultValueSql("('')");
            entity.Property(e => e.STL_UsedInBilling).HasDefaultValueSql("((1))");
            entity.Property(e => e.STL_WhereClause).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefSysConfig>(entity =>
        {
            entity.Property(e => e.ZRC_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZRC_StringValue).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZRC_ZRT_NKConfigCodeNavigation).WithMany(p => p.RefSysConfigs)
                .HasPrincipalKey(p => p.ZRT_ConfigCode)
                .HasForeignKey(d => d.ZRC_ZRT_NKConfigCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefSysConfig_RefSysConfigType");
        });

        modelBuilder.Entity<RefSysConfigType>(entity =>
        {
            entity.Property(e => e.ZRT_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<RefTimeZone>(entity =>
        {
            entity.ToTable("RefTimeZone", tb => tb.HasTrigger("RefTimeZone_Delete"));

            entity.Property(e => e.R2_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.R2_CivilianTimeZoneCode).HasDefaultValueSql("('')");
            entity.Property(e => e.R2_CivilianTimeZoneFullName).HasDefaultValueSql("('')");
            entity.Property(e => e.R2_MilitaryTimeZoneCode).HasDefaultValueSql("('')");
            entity.Property(e => e.R2_Type).HasDefaultValueSql("('')");

            entity.HasOne(d => d.R2_R3_TimeZoneSetNavigation).WithMany(p => p.RefTimeZones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefTimeZoneSet_R2_R3_TimeZoneSet");
        });

        modelBuilder.Entity<RefTimeZoneRule>(entity =>
        {
            entity.HasKey(e => e.R4_PK)
                .HasName("PK_RefTimeZoneRule_R4_PK")
                .IsClustered(false);

            entity.Property(e => e.R4_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.R4_DaylightSavingDayName).HasDefaultValueSql("('')");
            entity.Property(e => e.R4_DaylightSavingDayWeekDate).HasDefaultValueSql("('')");
            entity.Property(e => e.R4_DaylightSavingMonth).HasDefaultValueSql("('')");
            entity.Property(e => e.R4_StartOrEndRule).HasDefaultValueSql("('')");
            entity.Property(e => e.R4_TypeOfTime).HasDefaultValueSql("('')");

            entity.HasOne(d => d.R4_R2Navigation).WithMany(p => p.RefTimeZoneRules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefTimeZoneRule_RefTimeZone");
        });

        modelBuilder.Entity<RefTimeZoneSet>(entity =>
        {
            entity.HasKey(e => e.R3_PK).HasName("PK_RefTimeZoneSet_R3_PK");

            entity.Property(e => e.R3_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.R3_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.R3_TimeZoneSetName).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefUNLOCO>(entity =>
        {
            entity.HasKey(e => e.RL_PK)
                .HasName("PK_UX__RL_PK")
                .IsClustered(false);

            entity.Property(e => e.RL_PK).ValueGeneratedNever();
            entity.Property(e => e.RL_CoOrdinates).HasDefaultValueSql("('')");
            entity.Property(e => e.RL_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.RL_GeoLocation).HasDefaultValueSql("([geography]::STPointFromText('POINT EMPTY',(4326)))");
            entity.Property(e => e.RL_IATA).HasDefaultValueSql("('')");
            entity.Property(e => e.RL_IATARegionCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RL_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.RL_NameWithDiacriticals).HasDefaultValueSql("('')");
            entity.Property(e => e.RL_PortName).HasDefaultValueSql("('')");
            entity.Property(e => e.RL_RN_NKCountryCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RL_RW_RN_NKCountryCode).IsFixedLength();
        });

        modelBuilder.Entity<RefUNLOCOPortMapping>(entity =>
        {
            entity.Property(e => e.RLM_PK).ValueGeneratedNever();
        });

        modelBuilder.Entity<RefUNLOCORelatedPort>(entity =>
        {
            entity.HasKey(e => e.RLR_PK).IsClustered(false);

            entity.Property(e => e.RLR_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RLR_RL_NKRelatedPort).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefUNLOCOUtcOffset>(entity =>
        {
            entity.HasKey(e => e.RLO_PK).IsClustered(false);

            entity.Property(e => e.RLO_PK).ValueGeneratedNever();
            entity.Property(e => e.RLO_RL_NKCode).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefUnitSection>(entity =>
        {
            entity.Property(e => e.RUS_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RUS_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.RUS_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.RUS_Group).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<RefVessel>(entity =>
        {
            entity.Property(e => e.RV_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RV_CarrierCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RV_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.RV_CustomAttrib1).HasDefaultValueSql("('')");
            entity.Property(e => e.RV_CustomAttrib2).HasDefaultValueSql("('')");
            entity.Property(e => e.RV_CustomAttrib3).HasDefaultValueSql("('')");
            entity.Property(e => e.RV_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.RV_LloydsNumber)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.RV_MalaysiaVesselId).HasDefaultValueSql("('')");
            entity.Property(e => e.RV_MaritimeMobileServiceIdentity)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.RV_RN_NKCountryOfReg).HasDefaultValueSql("('')");
            entity.Property(e => e.RV_RadioCallSign).HasDefaultValueSql("('')");
            entity.Property(e => e.RV_ScreeningStatus)
                .HasDefaultValueSql("('NOT')")
                .IsFixedLength();
            entity.Property(e => e.RV_StatCode5)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.RV_StatusCode)
                .HasDefaultValueSql("('SRV')")
                .IsFixedLength();
            entity.Property(e => e.RV_VesselType)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
        });

        modelBuilder.Entity<RefVesselArrival>(entity =>
        {
            entity.Property(e => e.ZYA_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZYA_ArrivalPort).HasDefaultValueSql("('')");

            entity.HasOne(d => d.ZYA_ZZO_VesselNavigation).WithMany(p => p.RefVesselArrivals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefVesselArrival_RefVesselZZ");
        });

        modelBuilder.Entity<RefVesselZZ>(entity =>
        {
            entity.Property(e => e.ZZO_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ZZO_LloydsNumber).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZO_RN_NKCountryOfReg).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZO_RadioCallSign).HasDefaultValueSql("('')");
            entity.Property(e => e.ZZO_VesselType).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<SourceData>(entity =>
        {
            entity.HasKey(e => e.SDA_PK).IsClustered(false);

            entity.ToTable(tb => tb.HasTrigger("SourceData_Modify"));

            entity.Property(e => e.SDA_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.SDA_ContentText).HasDefaultValueSql("('')");
            entity.Property(e => e.SDA_ContentType).IsFixedLength();
            entity.Property(e => e.SDA_CreatedTime).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.SDA_FileHash).IsFixedLength();
            entity.Property(e => e.SDA_Source).IsFixedLength();
            entity.Property(e => e.SDA_Status)
                .HasDefaultValueSql("('QUE')")
                .IsFixedLength();
            entity.Property(e => e.SDA_SubSource).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<SourceDataUserView>(entity =>
        {
            entity.ToView("SourceDataUserView");

            entity.Property(e => e.SDA_Source).IsFixedLength();
            entity.Property(e => e.SDA_Status).IsFixedLength();
        });

        modelBuilder.Entity<StmNote>(entity =>
        {
            entity.Property(e => e.ST_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ST_Description).HasDefaultValueSql("('Terms and Conditions')");
            entity.Property(e => e.ST_ForceRead).HasDefaultValueSql("((1))");
            entity.Property(e => e.ST_NoteContext).HasDefaultValueSql("('ALL')");
            entity.Property(e => e.ST_NoteText).HasDefaultValueSql("('')");
            entity.Property(e => e.ST_NoteType)
                .HasDefaultValueSql("('INT')")
                .IsFixedLength();
        });

        modelBuilder.Entity<SubscriptionEvent>(entity =>
        {
            entity.Property(e => e.SSV_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<SubscriptionEventType>(entity =>
        {
            entity.Property(e => e.SST_PK).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<SubscriptionNotificationQueue>(entity =>
        {
            entity.Property(e => e.SNQ_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.SNQ_Status).HasDefaultValueSql("('QUE')");
        });

        modelBuilder.Entity<SystemData>(entity =>
        {
            entity.Property(e => e.SD_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.SD_Name).HasDefaultValueSql("('')");
            entity.Property(e => e.SD_Value).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<UNDGAttribute>(entity =>
        {
            entity.HasKey(e => e.DA_PK).IsClustered(false);

            entity.Property(e => e.DA_PK).ValueGeneratedNever();
            entity.Property(e => e.DA_Descriptor).HasDefaultValueSql("('')");
            entity.Property(e => e.DA_Index).HasDefaultValueSql("('')");
            entity.Property(e => e.DA_Language).HasDefaultValueSql("('')");
            entity.Property(e => e.DA_Type).HasDefaultValueSql("('')");

            entity.HasOne(d => d.DA_DGNavigation).WithMany(p => p.UNDGAttributes).HasConstraintName("FK_UNDGAttribute_UNDGSubstance");
        });

        modelBuilder.Entity<UNDGAttributeZZ>(entity =>
        {
            entity.Property(e => e.DAZ_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DAZ_Descriptor).HasDefaultValueSql("('')");
            entity.Property(e => e.DAZ_Index).HasDefaultValueSql("('')");
            entity.Property(e => e.DAZ_Language).HasDefaultValueSql("('')");
            entity.Property(e => e.DAZ_ParentCode).HasDefaultValueSql("('')");
            entity.Property(e => e.DAZ_Type).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<UNDGCommonData>(entity =>
        {
            entity.HasKey(e => e.DC_PK).IsClustered(false);

            entity.Property(e => e.DC_PK).ValueGeneratedNever();
            entity.Property(e => e.DC_Descriptor).HasDefaultValueSql("('')");
            entity.Property(e => e.DC_Index).HasDefaultValueSql("('')");
            entity.Property(e => e.DC_Language).HasDefaultValueSql("('')");
            entity.Property(e => e.DC_Type).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<UNDGCountryReference>(entity =>
        {
            entity.Property(e => e.DCR_PK).ValueGeneratedNever();
            entity.Property(e => e.DCR_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.DCR_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.DCR_RN_NKCountry).HasDefaultValueSql("('')");
            entity.Property(e => e.DCR_Type).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<UNDGCountryReferencePivot>(entity =>
        {
            entity.HasKey(e => e.DCP_PK).IsClustered(false);

            entity.HasIndex(e => e.DCP_DCR, "IX_UNDGCountryReferencePivot_DCP_DCR").IsClustered();

            entity.Property(e => e.DCP_PK).ValueGeneratedNever();
            entity.Property(e => e.DCP_Standard).HasDefaultValueSql("('')");
            entity.Property(e => e.DCP_UNNO).HasDefaultValueSql("('')");
            entity.Property(e => e.DCP_Variant).HasDefaultValueSql("('')");

            entity.HasOne(d => d.DCP_DCRNavigation).WithMany(p => p.UNDGCountryReferencePivots)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UNDGCountryReferencePivot_UNDGCountryReference");
        });

        modelBuilder.Entity<UNDGReference>(entity =>
        {
            entity.HasKey(e => e.DR_PK).IsClustered(false);

            entity.Property(e => e.DR_PK).ValueGeneratedNever();
            entity.Property(e => e.DR_Code).HasDefaultValueSql("('')");
            entity.Property(e => e.DR_Description).HasDefaultValueSql("('')");
            entity.Property(e => e.DR_RN_NKCountry).HasDefaultValueSql("('')");
            entity.Property(e => e.DR_Type).HasDefaultValueSql("('')");

            entity.HasOne(d => d.DR_DGNavigation).WithMany(p => p.UNDGReferences)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UNDGReference_UNDGSubstance");
        });

        modelBuilder.Entity<UNDGSubstance>(entity =>
        {
            entity.HasKey(e => e.DG_PK).IsClustered(false);

            entity.Property(e => e.DG_PK).ValueGeneratedNever();
            entity.Property(e => e.DG_CargoMaxAmtUQ).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_CargoPackAmtType).HasDefaultValueSql("('NLM')");
            entity.Property(e => e.DG_CargoPackIns).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_Class).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_CodedStow).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_DglPhrase).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_EMS).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_EXVector).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_EmergencyResponseGuide).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_ExceptedQuantityCode).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_ExpLim).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_FlashPoint).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_Hazards).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_IBCIns).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_IBCProv).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_IMOTankIns).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.DG_LQ2OrPaxMaxAmtType).HasDefaultValueSql("('NLM')");
            entity.Property(e => e.DG_LQ2OrPaxMaxAmtUQ).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_LQMaxAmtType).HasDefaultValueSql("('NLM')");
            entity.Property(e => e.DG_LQMaxAmtUQ).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_LQSpecProvIndex).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_MP).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_Markers).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_PG).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_PSN).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_PackIns).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_PackProv).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_PaxPackIns).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_Pointers).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_SpecialHandlingCodes).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_Standard).HasDefaultValueSql("('IMO')");
            entity.Property(e => e.DG_State).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_StowCat).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_SubLabel1).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_SubLabel2).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_TankProv).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_TechName).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_TreatAs).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_UNNO).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_UNTankIns).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_UlineEMS).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_UsrUSDOTShippingName).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_Variant).HasDefaultValueSql("('')");
            entity.Property(e => e.DG_Variation).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<UNDGSubstanceADN>(entity =>
        {
            entity.Property(e => e.ADN_PK).ValueGeneratedNever();
            entity.Property(e => e.ADN_CarriagePermittedDetails).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_Class).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_ClassificationCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_EquipmentDetails).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_ExceptedQuantityCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.ADN_LQ2MaxAmtUQ).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_LQMaxAmtUQ).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_Labels).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_LoadingSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_LoadingSpecialProvNote).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_OperationSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_OperationSpecialProvNote).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_PG).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_PSN).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_SpecialProvisions).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_UNNO).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_UnloadingSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_UnloadingSpecialProvNote).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_Variant).HasDefaultValueSql("('')");
            entity.Property(e => e.ADN_Ventilation).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<UNDGSubstanceADR>(entity =>
        {
            entity.Property(e => e.ADR_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ADR_ADRTankCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_ADRTankSpecProv).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_BulkSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_BulkTankIns).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_BulkTankSpecProv).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_Class).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_ClassificationCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_ExceptedQuantityCode).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_HazardIDNumber).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.ADR_LQ2MaxAmtUQ).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_LQMaxAmtUQ).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_Labels).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_LoadingSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_MixedPackingProv).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_OperationSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_PG).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_PSN).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_PackIns).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_PackProv).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_PackingSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_SpecialProvisions).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_TankVehicle).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_TransportCategory).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_UNNO).HasDefaultValueSql("('')");
            entity.Property(e => e.ADR_Variant).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<UNDGSubstanceCFR>(entity =>
        {
            entity.Property(e => e.CFR_PK).ValueGeneratedNever();
            entity.Property(e => e.CFR_BulkPackingInstructions).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_BulkPackingProvisions).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_CVL).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_CargoAirRailLimitType).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_CargoAirRailLimitUnit).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_EmergencyResponseGuide).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_ExceptedQuantity).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_GeneralStowage).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_IBCInstructions).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_IBCProvisions).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.CFR_LQMaxAmtUQ).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_MarinePollutant)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.CFR_PAXAirRailLimitType).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_PAXAirRailLimitUnit).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_PSN).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_PackingExceptions).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_PackingGroup).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_PackingInstructions).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_PackingProvisions).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_PassengerStowage).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_PoisonInhalationHazard)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.CFR_Prefix).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_PrimaryClass).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_ReportableQuantityUnit).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_SecondaryCargoAirRailLimitUnit).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_SecondaryClass).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_SecondaryPAXAirRailLimitUnit).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_SpecialProvisions).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_State)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.CFR_StowageCategory).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_StowageCodes).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_StowageIMDGCodes).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_TankInstructions).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_TankProvisions).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_TechnicalName)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.CFR_TertiaryClass)
                .HasDefaultValueSql("('')")
                .IsFixedLength();
            entity.Property(e => e.CFR_TreatAs).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_UNNO).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_Variant).HasDefaultValueSql("('')");
            entity.Property(e => e.CFR_Variation).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<UNDGSubstanceJTT>(entity =>
        {
            entity.Property(e => e.JTT_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.JTT_BulkSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_BulkTankIns).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_BulkTankSpecProv).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_Class).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_ClassificationCode).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_ExceptedQuantityCode).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_HazardIDNumber).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.JTT_LQ2MaxAmtUQ).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_LQMaxAmtUQ).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_Labels).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_LoadingSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_MixedPackingProv).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_OperationSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_PG).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_PSN).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_PackIns).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_PackProv).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_PackingSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_SpecialProvisions).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_TankCode).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_TankSpecProv).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_TankVehicle).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_TransportCategory).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_UNNO).HasDefaultValueSql("('')");
            entity.Property(e => e.JTT_Variant).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<UNDGSubstanceRID>(entity =>
        {
            entity.Property(e => e.RID_PK).ValueGeneratedNever();
            entity.Property(e => e.RID_BulkContainerTankIns).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_BulkContainerTankProv).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_CarriageBulkSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_CarriageLoadingSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_CarriagePackagesSpecialProv).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_Class).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_ClassificationCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_ColisExpressCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_ExceptedQuantityCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_HazardIDNumber).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_IBCIns).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.RID_LQ2MaxAmtUQ).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_LQMaxAmtUQ).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_Labels).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_MixedPackProv).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_PG).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_PSN).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_PackIns).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_PackProv).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_SpecialProvisions).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_TankCode).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_TankSpecProv).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_TransportCategory).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_UNNO).HasDefaultValueSql("('')");
            entity.Property(e => e.RID_Variant).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<UNDGVersion>(entity =>
        {
            entity.Property(e => e.DV_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DV_IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.DV_Name).HasDefaultValueSql("('')");
            entity.Property(e => e.DV_Standard).HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<UniversalXmlSchema>(entity =>
        {
            entity.HasKey(e => e.XSD_PK).IsClustered(false);

            entity.Property(e => e.XSD_PK).HasDefaultValueSql("(newid())");
            entity.Property(e => e.XSD_TimeStamp).HasDefaultValueSql("(getdate())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
