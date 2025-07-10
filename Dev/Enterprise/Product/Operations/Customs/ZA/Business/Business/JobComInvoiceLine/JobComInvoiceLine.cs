using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Customs.Universal.Constants;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;

namespace Enterprise.Customs.ZA.Business
{
	[SystemDefinedValues]
	[DependentBusinessObject(typeof(JobComInvoiceHeader), "InvoiceLines")]
	public partial class JobComInvoiceLine : AutoZAJobComInvoiceLine
		, IUltimateDistributee
		, Integration.Customs.ZA.IJobComInvoiceLine
		, IUniversalRateCalcData
		, IChangeOfOwnershipLineDetails
		, IDA63ValueRecalculationParent
		, IAdditionalLineTariffDetailParent
		, ICusCodeDataTypeSupporter
		, IAddInfoWithSyncProperty
		, IInvoiceLineInformation
	{
		#region Constants

		#region Schema

		public new class Schema : AutoZAJobComInvoiceLine.Schema
		{
			public const string JI_Calc_ATV = "JI_Calc_ATV";
			public const string JI_Calc_CIF_C = "JI_Calc_CIF_C";
			public const string JI_Calc_ActualPrice = "JI_Calc_ActualPrice";
			public const string JI_CEI_Description = "JI_CEI_Description";

			public const string TariffFromRelatedImportBOE = "TariffFromRelatedImportBOE";
			public const string CustomsQtyFromRelatedImportBOE = "CustomsQtyFromRelatedImportBOE";
			public const string CustomsQtyUQFromRelatedImportBOE = "CustomsQtyUQFromRelatedImportBOE";
			public const string CustomsValueFromRelatedImportBOE = "CustomsValueFromRelatedImportBOE";

			public const string CustomsDutyFromRelatedImportBOE = "CustomsDutyFromRelatedImportBOE";
			public const string VATFromRelatedImportBOE = "VATFromRelatedImportBOE";
			public const string ConversionFactorFromRelatedImportBOE = "ConversionFactorFromRelatedImportBOE";

			public const string RelatedImportBOENumberString = "RelatedImportBOENumberString";

			public const string RefundRebateCode = "RefundRebateCode";
			public const string RefundRebateValue = "RefundRebateValue";
			public const string TradeAgreementCode = "TradeAgreementCode";
			public const string LinkedEntryLineNumber = "LinkedEntryLineNumber";

			public const string JI_Colour = "JI_Colour";
			public const string JI_EngineCapacity = "JI_EngineCapacity";
			public const string JI_EngineNumber = "JI_EngineNumber";
			public const string JI_Make = "JI_Make";
			public const string JI_VehicleFormat = "JI_VehicleFormat";
			public const string JI_VehicleType = "JI_VehicleType";
			public const string JI_VIN = "JI_VIN";
			public const string JI_YearOfManufacture = "JI_YearOfManufacture";
		}

		#endregion

		#endregion

		#region Constructor

		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Related BOs

		#region Classification
		public new CusClassification Classification
		{
			get { return (CusClassification)base.Classification; }
		}
		#endregion

		#region TariffView

		public override ZString UniversalTariffType => UniversalReferenceConstants.CusTariffCode.Schedule1Part1;

		internal TariffView ImportTariff
		{
			get { return JI_ImportTariff.IsEmpty ? null : Factory.GetCusTariff(UniversalReferenceConstants.CusTariffCode.Schedule1Part1, JI_ImportTariff, EffectiveAssessmentDate); }
		}

		internal TariffView Schedule1Part2ATariff => Factory.GetValue(ref schedule1Part2ATariffCached, () =>
					{
						return CusLineTariffDetails
							.Select(x => x.UniversalTariff)
							.FirstOrDefault(x => x != null && x.ZZ1_ZZI_TariffTypeCode == UniversalReferenceConstants.CusTariffCode.Schedule1Part2A);
					});

		CachedProperty<TariffView> schedule1Part2ATariffCached;

		#endregion

		#region ImportBOEntry

		public CusEntryHeader ImportBOEntry
		{
			get
			{
				if (fImportBOEntry == null || fImportBOEntry.IsDeleted || fImportBOEntry.MovementReferenceNumber != JI_PreviousEntryNumber)
				{
					if (JI_PreviousEntryNumber.IsEmpty)
					{
						fImportBOEntry = null;
					}
					else
					{
						fImportBOEntry = GetEntryHeader(false, ZAJobMessageTypeList.Codes.Import) ??
										 GetEntryHeader(false, ZAJobMessageTypeList.Codes.ExBond) ??
										 GetEntryHeader(true, ZAJobMessageTypeList.Codes.ImportByExternalBroker);
					}
				}
				return fImportBOEntry;
			}
		}
		CusEntryHeader fImportBOEntry;

		CusEntryHeader GetEntryHeader(ZBool currentCompany, ZString messageType)
		{
			var branches = GlbCompany.CurrentCompany.Branches.GetPKs();
			var branchSql = (currentCompany && branches.Any() ? " AND " + JobDeclaration.Schema.JE_GB + " IN(SELECT Value FROM @Branches) " : "");
			var sql = string.Format(CultureInfo.InvariantCulture, $@"
{CusEntryHeaderSchema.Constants.PK} IN
(
	SELECT TOP 1 {CusEntryHeaderSchema.Constants.PK}
	FROM {CusEntryHeaderSchema.Constants.SqlSchemaName}.{CusEntryHeaderSchema.Constants.TableName} AS HEADER
	INNER JOIN (
		SELECT {CusEntryNumSchema.Constants.CE_ParentID}
		FROM {CusEntryNumSchema.Constants.SqlSchemaName}.{CusEntryNumSchema.Constants.TableName}
		WHERE {CusEntryNumSchema.Constants.CE_EntryType} = @EntryType
		AND {CusEntryNumSchema.Constants.CE_EntryNum} = @EntryNumber
		AND {CusEntryNumSchema.Constants.CE_RN_NKCountryCode} = @CountryCode
	) AS ENTRYNUM
	ON HEADER.{CusEntryHeaderSchema.Constants.PK} = ENTRYNUM.{CusEntryNumSchema.Constants.CE_ParentID}
	INNER JOIN (
		SELECT {JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc}, {JobDeclarationSchema.Constants.PK}
		FROM {JobDeclarationSchema.Constants.SqlSchemaName}.{JobDeclarationSchema.Constants.TableName}
		WHERE {JobDeclarationSchema.Constants.JE_MessageType} = @MessageType
		{branchSql}
	) AS DECL ON DECL.{JobDeclarationSchema.Constants.PK} = HEADER.{CusEntryHeaderSchema.Constants.CH_JE}
	ORDER BY {JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc} DESC
)");

			var @params = new ZSqlParameterCollection();
			@params.Add("@EntryType", CusEntryNumberTypes.Standard.MovementReferenceNumber, CusEntryNumSchema.CE_EntryType);
			@params.Add("@EntryNumber", JI_PreviousEntryNumber, CusEntryNumSchema.CE_EntryNum);
			@params.Add("@CountryCode", Core.Constants.CountryCodes.SouthAfrica, CusEntryNumSchema.CE_RN_NKCountryCode);
			@params.Add("@MessageType", messageType, JobDeclarationSchema.JE_MessageType);
			if (currentCompany && branches.Any())
			{
				@params.Add(ZSqlParameter.New("@Branches", branches.ToArray(), JobDeclarationSchema.JE_GB, true));
			}

			var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			query.AddFilterAndZSQLParameterCollection(sql, @params);

			return Factory.LoadTop1<CusEntryHeader>(query);
		}

		#endregion

		#region ImportBOEntryLine

		public CusEntryLine ImportBOEntryLine
		{
			get
			{
				if (fImportBOEntryLine == null && ImportBOEntry != null)
				{
					fImportBOEntryLine = (CusEntryLine)ImportBOEntry.MergedLines.FindByLineNumber(JI_PreviousEntryLineNumber);
				}
				return fImportBOEntryLine;
			}
		}
		CusEntryLine fImportBOEntryLine;

		#endregion

		#endregion

		#region Part/Classification/Tariff BO field overrides

		#region JI_Tariff

		[MaxLength(10)]
		public override ZString JI_Tariff
		{
			get
			{
				if (IsDeclarationBuiltIn)
				{
					return base.JI_Tariff;
				}
				else
				{
					return TariffFormatter.Format(base.JI_Tariff);
				}
			}
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_Tariff))
				{
					var oldValue = JI_Tariff;
					base.JI_Tariff = value;
					if (!IsCopying && oldValue != JI_Tariff)
					{
						if (IsDeclarationBuiltIn)
						{
							SetDefaultTaxOrFeeFromTariff();
							DefaultPreference();
							JI_Description = UniversalTariff?.FullTariffDescription(EffectiveAssessmentDate, false, false).Left(JobComInvoiceLine.Schema.JI_DescriptionMaxLength) ?? ZString.Empty;
							using (SuspendUOMDefaulting())
							{
								ClearAndDefaultCusLineTariffDetails(null);
							}
							DefaultUOMFromTariffAndCusLineTariffDetailsIfNeeded();
							UpdateINTChargeIfNeeded();
						}
						CusLineTariffDetails.MarkAsNeedingValidation();
					}
					if (!IsDiamondProcessingRequired)
					{
						ClearDiamondProcessingFields();
					}
				}
			}
		}

		#endregion

		#region Tariff Description

		public ZString AdditionalDescriptionForTariffDescription
		{
			get
			{
				var additionalDescription = new ZStringBuilder();
				addFieldDescriptionToTariffDescription(additionalDescription, "VIN", JI_VIN);
				addFieldDescriptionToTariffDescription(additionalDescription, "Engine No.", JI_EngineNumber);
				addFieldDescriptionToTariffDescription(additionalDescription, "Make", JI_Make);
				addFieldDescriptionToTariffDescription(additionalDescription, "Model", JI_Model);
				addFieldDescriptionToTariffDescription(additionalDescription, "Veh. Format", JI_VehicleFormat);
				addFieldDescriptionToTariffDescription(additionalDescription, "Veh. Type", JI_VehicleType);
				addFieldDescriptionToTariffDescription(additionalDescription, "Colour", JI_Colour);
				addFieldDescriptionToTariffDescription(additionalDescription, "YoM", JI_YearOfManufacture);
				return additionalDescription.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		void addFieldDescriptionToTariffDescription(ZStringBuilder description, ZString fieldName, ZString fieldValue)
		{
			if (!fieldValue.IsEmpty)
			{
				description.AppendFormat("{0}; {1}", fieldName, fieldValue);
			}
		}

		#endregion

		#region JI_PartNo

		public override ZString JI_PartNo
		{
			get { return base.JI_PartNo; }
			set
			{
				var oldValue = JI_PartNo;
				base.JI_PartNo = value;
				DefaultBrandNameFromProductOwnerIfPossible();
				if (!IsCopying && oldValue != JI_PartNo)
				{
					DefaultOwnerProductDataIfNeeded();
				}
			}
		}

		#endregion

		#region JI_BrandName

		void DefaultBrandNameFromProductOwnerIfPossible()
		{
			if (JI_BrandName.IsEmpty && SupplierPart != null)
			{
				JI_BrandName = SupplierPart.OP_Brand;
			}
		}

		#endregion

		#region JI_PartAttrib

		public override ZString JI_PartAttrib1
		{
			get { return base.JI_PartAttrib1; }
			set
			{
				SettingPartAttrib(ref settingJI_PartAttrib1InProgres, () =>
				{
					var oldValue = JI_PartAttrib1;
					base.JI_PartAttrib1 = value;
					var newValue = JI_PartAttrib1;
					if (!IsCopying && oldValue != newValue)
					{
						PartSyncManager.Refresh();
						DefaultOwnerPartAttribIfNotEmpty(Importer, OrgMiscServ.Schema.OM_IMPartAttrib1Name, OrgMiscServ.Schema.OM_IMPartAttrib1Type, JI_PartAttrib1);
						PopulateJI_VINFromVINPartAttribute(1, newValue);
					}
				});
			}
		}

		public override ZString JI_PartAttrib2
		{
			get { return base.JI_PartAttrib2; }
			set
			{
				SettingPartAttrib(ref settingJI_PartAttrib2InProgres, () =>
				{
					var oldValue = JI_PartAttrib2;
					base.JI_PartAttrib2 = value;
					var newValue = JI_PartAttrib2;
					if (!IsCopying && oldValue != newValue)
					{
						PartSyncManager.Refresh();
						DefaultOwnerPartAttribIfNotEmpty(Importer, OrgMiscServ.Schema.OM_IMPartAttrib2Name, OrgMiscServ.Schema.OM_IMPartAttrib2Type, JI_PartAttrib2);
						PopulateJI_VINFromVINPartAttribute(2, newValue);
					}
				});
			}
		}

		public override ZString JI_PartAttrib3
		{
			get { return base.JI_PartAttrib3; }
			set
			{
				SettingPartAttrib(ref settingJI_PartAttrib3InProgres, () =>
				{
					var oldValue = JI_PartAttrib3;
					base.JI_PartAttrib3 = value;
					var newValue = JI_PartAttrib3;
					if (!IsCopying && oldValue != newValue)
					{
						PartSyncManager.Refresh();
						DefaultOwnerPartAttribIfNotEmpty(Importer, OrgMiscServ.Schema.OM_IMPartAttrib3Name, OrgMiscServ.Schema.OM_IMPartAttrib3Type, JI_PartAttrib3);
						PopulateJI_VINFromVINPartAttribute(3, newValue);
					}
				});
			}
		}

		bool settingJI_PartAttrib1InProgres;
		bool settingJI_PartAttrib2InProgres;
		bool settingJI_PartAttrib3InProgres;
		void SettingPartAttrib(ref bool settingPartAttribInProgress, Action setPartAttrib)
		{
			if (!settingPartAttribInProgress)
			{
				try
				{
					settingPartAttribInProgress = true;
					setPartAttrib();
				}
				finally
				{
					settingPartAttribInProgress = false;
				}
			}
		}

		#endregion

		protected override ZString ClassificationDetailsForGenericWrapperCore
		{
			get
			{
				ZString result = base.ClassificationDetailsForGenericWrapperCore;
				// TODO: ToBeChecked: Logic Changed with the removal of AdditionalDuties in AddInfo, Need re-implementation #Victor 20160322
				if (!JI_PrimaryPreference.IsEmpty)
				{
					result += " / " + JI_PrimaryPreference;
				}

				return result;
			}
		}

		protected override void SetTariffEtcDataFromProductsPivotCore(BaseCusClassPartPivot pivot)
		{
			base.SetTariffEtcDataFromProductsPivotCore(pivot);

			var pivot1 = (CusClassPartPivot)pivot;
			JI_CountryOfOrigin = pivot1.CI_RN_NKCountryOfOrigin;
			JI_PrimaryPreference = pivot1.CI_PrimaryPreference;
			JI_ROOCert = pivot1.CI_ROOCert;
			JI_NewUsed = pivot1.CI_NewUsed.IsEmpty ? new ZString(GoodsTypeList.Codes.N) : pivot1.CI_NewUsed;
			JI_EngineCapacity = (pivot1.CI_EngineCapacity >= short.MinValue && pivot1.CI_EngineCapacity <= short.MaxValue) ? (short)pivot1.CI_EngineCapacity : ZShort.Zero;
			JI_VehicleFormat = (pivot1.CI_VehicleFormat.Length <= JI_VehicleFormatMaxLength) ? pivot1.CI_VehicleFormat.Left(JI_VehicleFormatMaxLength) : ZString.Empty;
			JI_VehicleType = pivot1.CI_VehicleType;
			JI_Colour = pivot1.CI_Colour;

			ClearAndDefaultCusLineTariffDetails(null);

			if (pivot1.CusLineTariffDetails.Count > 0)
			{
				var shipmentType = CusProcedure?.ZZ6_ShipmentType ?? ZString.Empty;
				var concession = CusProcedure?.ZZ6_Concession ?? ZString.Empty;
				var concessionTypeList = ConcessionTariffTypes;
				var shipmentTypeList = GetShipmentTariffTypes(shipmentType);

				CusLineTariffDetails.RemoveAndDeleteAll();

				foreach (var tariff in pivot1.CusLineTariffDetails.Where(x => !x.BZ_Tariff.IsEmpty))
				{
					var isConcessionTariff = concessionTypeList.Contains(tariff.BZ_Type);
					if (CusProcedure == null ||
						(isConcessionTariff && !concession.IsEmpty && concession == tariff.BZ_Type.Left(1)) ||
						shipmentTypeList.Contains(tariff.BZ_Type) ||
						(!isConcessionTariff && shipmentTypeList.Length == 0))
					{
						CusLineTariffDetails.AddNew(tariff.BZ_Type, tariff.BZ_Tariff);
					}
				}
			}
		}

		ZString[] GetShipmentTariffTypes(ZString shipmentType)
		{
			var result = Array.Empty<ZString>();
			if (shipmentType.Contains("IMP") || shipmentType.Contains("EXW"))
			{
				result = ImportShipmentTariffTypes;
			}
			if (shipmentType.Contains("EXP"))
			{
				result = ExportShipmentTariffTypes;
			}
			return result;
		}

		protected ZString[] ConcessionTariffTypes => new ZString[] { "3P1", "3P2", "4P1", "4P2", "4P3", "4P4", "4P5", "4P6", "5P1", "5P2", "5P3", "5P4", "5P5", "6P1", "6P2", "6P3", "6P4", "6P5" };
		protected ZString[] ImportShipmentTariffTypes => new ZString[] { "12A", "12B", "13A", "13B", "13C", "13D", "13E", "15A", "15B", "17A", "2P1", "2P2", "2P3" };
		protected ZString[] ExportShipmentTariffTypes => new ZString[] { "1P6" };

		#endregion

		#region Overrides

		protected override BaseCustomsQuantityConverter GetCustomsQuantityConverter()
		{
			return new ZACustomsQuantityConverter(this, (ZPropertyInfoDecimal)JI_CustomsQuantityInfo, (ZPropertyInfoString)JI_CustomsUnitQtyInfo);
		}

		protected override Customs.Business.BondedWarehouseTransactionLine GetNewBondedWarehouseTransactionLine()
		{
			return new BondedWarehouseTransactionLine(this);
		}

		protected override bool IsLookupsCachedInBase
		{
			get { return false; }
		}

		public override ZWeight EffectiveGrossWeight => new ZWeight(JI_Weight, JI_WeightUQ);

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JI_PrimaryPreference = ZString.Empty;
			JI_NewUsed = GoodsTypeList.Codes.N;
			JI_TakeUpInTradeStatistics = true;
		}

		#endregion

		#region JI_Calc_DutyAmount

		public override ZDecimal JI_Calc_DutyAmount
		{
			get
			{
				// TODO: ToBeChecked: Logic Changed with the removal of AdditionalDuties in AddInfo, Need re-implementation #Victor 20160322
				// TODO: ToBeChecked: Currently Using Default getting Method, which is ending into to CusEntryLine GetDutyAmountCore(), the consideration of RebatedDury will be taken back into consideration when necessary. #Victor 20160427
				var entryLine = CusEntryLine;
				return (entryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(entryLine.GetDutyAmount()).Amount;
			}
		}

		#endregion

		#region JI_JZ

		[LightValidationTestExempt]
		public override ZGuid JI_JZ
		{
			get { return base.JI_JZ; }
			set
			{
				base.JI_JZ = value;
				if (InvoiceHeader != null)
				{
					if (!IsExport)
					{
						ClearInvoiceLineValuesIfSame(InvoiceHeader.JZ_ROOType, JobComInvoiceLine.Schema.JI_PrimaryPreference);
					}
					UpdateValuationMarkupIfNeeded();
				}
			}
		}

		#endregion

		public override ZString JI_CountryOfOrigin
		{
			get { return GetEffectiveValueToReturn(base.JI_CountryOfOrigin, JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin); }
			set
			{
				var oldValue = JI_CountryOfOrigin;
				base.JI_CountryOfOrigin = GetEffectiveValueToSet(value, JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin);
				if (!IsCopying && oldValue != JI_CountryOfOrigin && IsDeclarationBuiltIn)
				{
					DefaultPreference();
					CusLineTariffDetails.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JI_InvoiceUQ
		{
			get
			{
				return base.JI_InvoiceUQ;
			}
			set
			{
				if (!SetterSuspender.IsSetterSuspended(nameof(JI_InvoiceUQ)))
				{
					base.JI_InvoiceUQ = value;

					if (!IsCopying && Declaration != null)
					{
						Declaration.MarkAsNeedingValidation();
					}
				}
			}
		}

		#region JI_CustomsUnitQty

		public override ZString JI_CustomsUnitQty
		{
			get { return JI_InvoiceUQ == "NX" ? JI_InvoiceUQ : base.JI_CustomsUnitQty; }
			set
			{
				var oldValue = JI_CustomsUnitQty;
				base.JI_CustomsUnitQty = value;
				if (oldValue != JI_CustomsUnitQty)
				{
					DA63NeedsRecalculation = true;
				}
			}
		}

		protected override bool GetJI_CustomsUnitQtyInfoReadOnly() => false;

		internal bool customsUnitQtyAutoSet;

		internal ZBool CustomsUOMAutoPopulated
		{
			get
			{
				_ = GetDefaultCustomsQtyUnits(UniversalTariff, CusLineTariffDetails.Cast<CusLineTariffDetail>());
				fCustomsUnitQtyAutoSet = customsUnitQtyAutoSet
				|| customsSecondUnitQtyAutoSet
				|| customsThirdUnitQtyAutoSet;

				return fCustomsUnitQtyAutoSet;
			}
		}
		ZBool fCustomsUnitQtyAutoSet;

		protected override bool GetJI_CustomsQuantityReadOnly()
		{
			return JI_CustomsUnitQty.IsEmpty;
		}

		#endregion

		#region JI_CustomsQuantity

		[DecimalPlaces(2)]
		public override ZDecimal JI_CustomsQuantity
		{
			get { return JI_InvoiceUQ == "NX" ? JI_InvoiceQuantity : base.JI_CustomsQuantity; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsQuantity))
				{
					var oldValue = JI_CustomsQuantity;
					base.JI_CustomsQuantity = value;
					if (oldValue != JI_CustomsQuantity && IsDA63)
					{
						DA63NeedsRecalculation = true;
					}

					if (oldValue != JI_CustomsQuantity && value < 0.01m)
					{
						MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#region CustomsUQ
		public override ZString CustomsUQ
		{
			get { return UniversalTariff?.GetSpecificUOM(UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType) ?? ZString.Empty; }
		}
		#endregion

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy() => new UniversalTariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>(defaultFirstUnitOnly: true);

		#region GetNewValidation
		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			JobComInvoiceLineValidation result = null;
			var declaration = Declaration;
			if (declaration != null)
			{
				if (declaration.IsImport)
				{
					result = new ImportJobComInvoiceLineValidation(this);
				}
				else if (declaration.IsExport)
				{
					result = new ExportJobComInvoiceLineValidation(this);
				}
			}
			return result ?? new JobComInvoiceLineValidation(this);
		}
		#endregion

		#region GetNewLookups
		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
		{
			return new JobComInvoiceLineLookups(this);
		}
		#endregion

		#region JI_CC
		public override ZGuid JI_CC
		{
			get { return base.JI_CC; }
			set
			{
				bool needsLoading = JI_CC != value;
				base.JI_CC = value;
				if (needsLoading)
				{
					CusClassification classification = Classification;
					if (classification != null)
					{
						foreach (ZPropertyInfo info in ZPropertyInfoHash)
						{
							if (info.HasSetter && CargoWise.Schema.Schema.GetPrefixFromColumnName(info.Name) == "UZ")
							{
								if (classification.ZPropertyInfoHash.ContainsKey(info.Name))
								{
									IZType classificationValue = (IZType)classification[info.Name];
									info.Value = classificationValue;
								}
							}
						}
					}
				}
			}
		}
		#endregion

		#region JI_Procedure

		protected override ZString GetProcedureCodeCore()
		{
			var entryInstruction = EntryInstruction;
			return entryInstruction == null ? ZString.Empty : entryInstruction.CEI_Style;
		}

		[MaxLength(4)]
		[ResourceStringData("ADCD9616-A235-4E55-98D7-F11BB794CDF0", ShortCaption = "CPC/PPC", Caption = "Current/Previous Procedure", FullDescription = "Current and Previous Procedure Codes")]
		public override ZString JI_Procedure
		{
			get { return base.JI_Procedure; }
			set
			{
				var oldValue = JI_Procedure;
				var oldProcedure = CusProcedure;
				base.JI_Procedure = value.Left(4);
				if (!IsCopying && oldValue != JI_Procedure)
				{
					ClearAndDefaultCusLineTariffDetails(oldProcedure);
					DA63NeedsRecalculation = IsDA63;
				}
				if (Declaration != null)
				{
					Declaration.Validation.ValidateJE_MergeBy();
					Declaration.MarkAsNeedingValidation();
				}
			}
		}

		public bool JI_Procedure_ReadOnly
		{
			get { return EntryInstruction?.IsDeleted ?? ZBool.True; }
		}

		public ZBool IsExcise => (CusProcedure?.ZZ6_Group ?? ZString.Empty) == UniversalReferenceConstants.RefCusProcedureGroup.Excise;

		public ZBool AreRooDetailsIncomplete => IsImport && JI_PrimaryPreference != UniversalReferenceConstants.PrimaryPreference.Standard && !TradeAgreementCode.IsEmpty && JI_ROOCert.IsEmpty;
		
		#endregion

		#region TariffFormatter

		protected override TariffFormatter TariffFormatter
		{
			get
			{
				return new NumberOnlyTariffFormatter();
			}
		}

		#endregion

		#region JI_CustomsSecondQuantity

		[DecimalPlaces(2)]
		[ResourceStringData("InvoiceLinesUserControl|11FD565B-88A1-4A71-94E8-C7170A93F015", Caption = "Additional Qty 1", FullDescription = "Additional Quantity 1")]
		public override ZDecimal JI_CustomsSecondQuantity
		{
			get { return base.JI_CustomsSecondQuantity; }
			set { base.JI_CustomsSecondQuantity = value; }
		}

		#endregion

		#region JI_CustomsSecondUnitQty

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.AdditionalUnitCodeList))]
		[ResourceStringData("InvoiceLinesUserControl|D4FBD5C2-2705-48C1-AC1A-19D73D1EAD53", ShortCaption = "UQ", Caption = "Unit of Quantity", FullDescription = "Additional Unit of Quantity 1")]
		public override ZString JI_CustomsSecondUnitQty
		{
			get { return base.JI_CustomsSecondUnitQty; }
			set { base.JI_CustomsSecondUnitQty = value; }
		}

		public bool JI_CustomsSecondUnitQty_ReadOnly => false;

		internal bool customsSecondUnitQtyAutoSet;

		#endregion

		#region JI_CustomsThirdQuantity

		[DecimalPlaces(2)]
		[ResourceStringData("InvoiceLinesUserControl|A027EFDA-8FDD-478F-959F-8592D5B1B168", Caption = "Additional Qty 2", FullDescription = "Additional Quantity 2")]
		public override ZDecimal JI_CustomsThirdQuantity
		{
			get { return base.JI_CustomsThirdQuantity; }
			set { base.JI_CustomsThirdQuantity = value; }
		}

		#endregion

		#region JI_CustomsThirdUnitQty

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.AdditionalUnitCodeList))]
		[ResourceStringData("InvoiceLinesUserControl|5D02EA80-81A6-4B72-95A7-C191DB2AB403", ShortCaption = "UQ", Caption = "Unit of Quantity", FullDescription = "Additional Unit of Quantity 2")]
		public override ZString JI_CustomsThirdUnitQty
		{
			get { return base.JI_CustomsThirdUnitQty; }
			set { base.JI_CustomsThirdUnitQty = value; }
		}

		public bool JI_CustomsThirdUnitQty_ReadOnly => false;

		internal bool customsThirdUnitQtyAutoSet;

		#endregion

		#region JI_BondedWhsQuantity

		[DecimalPlaces(0)]
		[ResourceStringData("InvoiceLinesUserControl|08CE8D4B-B4A4-4C50-8E02-896150767A68", Caption = "Countable Qty", FullDescription = "Countable Quantity")]
		public override ZDecimal JI_BondedWhsQuantity
		{
			get { return base.JI_BondedWhsQuantity; }
			set { base.JI_BondedWhsQuantity = value; }
		}

		#endregion

		#region JI_BondedWhsUnitQty

		[ResourceStringData("InvoiceLinesUserControl|DDC43B34-5F85-4D5B-ACCB-35B37F95A042", ShortCaption = "UQ", Caption = "Unit of Quantity", FullDescription = "Countable Unit of Quantity")]
		public override ZString JI_BondedWhsUnitQty
		{
			get { return base.JI_BondedWhsUnitQty; }
			set { base.JI_BondedWhsUnitQty = value; }
		}

		#endregion

		#region JI_Weight
		public override ZDecimal JI_Weight
		{
			get { return base.JI_Weight; }
			set
			{
				var oldValue = JI_Weight;
				base.JI_Weight = value;
				if (oldValue != JI_Weight && CusEntryLine != null && CusEntryLine.Header != null)
				{
					CusEntryLine.Header.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JI_CL
		public override ZGuid JI_CL
		{
			get { return base.JI_CL; }
			set
			{
				var oldValue = JI_CL;
				base.JI_CL = value;
				if (oldValue != JI_CL && CusEntryLine != null && CusEntryLine.Header != null)
				{
					CusEntryLine.Header.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JI_CEI

		[ReadOnlyMember(nameof(JI_CEI_ReadOnly))]
		[ResourceStringData("ED95690E-7500-4858-97F2-033683D7A657", ShortCaption = "Entry Ins.", Caption = "Entry Instruction", FullDescription = "Entry Instruction Customs Procedure Code")]
		public override ZGuid JI_CEI
		{
			get { return base.JI_CEI; }
			set
			{
				var oldValue = JI_CEI;
				var oldProcedure = CusProcedure;
				base.JI_CEI = value;
				if (!IsCopying && JI_CEI != oldValue)
				{
					if (!IsAutomaticSplitByBondAmountCEISetterSuspended)
					{
						using (SuspendCusLineTariffDetailDefaulting())
						{
							SetDefaultPreviousProcedureCode();
						}
						ClearAndDefaultCusLineTariffDetails(oldProcedure);
						DA63NeedsRecalculation = IsDA63;
						JI_TargetEntryLineNumber = ZShort.Zero;
						JI_CEI_DescriptionInfo.RefreshBinding();
						InvoiceHeader?.MarkAsNeedingValidation();
						CusLineTariffDetails.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected bool JI_CEI_ReadOnly => !(InvoiceHeader?.IsAttachedToPersistentDeclaration ?? false);

		#endregion

		#region JI_CustomsValue

		public override ZDecimal JI_CustomsValue => Factory.GetValue(ref cachedJI_CustomsValue, () =>
					{
						var result = ZDecimal.Zero;
						var overridenCustomsValue = CustomsValueOverrideMoney;
						if (overridenCustomsValue != null)
						{
							result = CurrencyConverter.ConvertExact(overridenCustomsValue, LocalCurrency).Amount;
						}
						else
						{
							result = JI_Calc_ActualPrice - JI_Calc_IntellectualValue + JI_Calc_ValuationMarkup;
						}
						return result;
					});

		CachedProperty<ZDecimal> cachedJI_CustomsValue;

		#endregion

		#region CustomsValueOverride

		internal Money CustomsValueOverrideMoney
		{
			get
			{
				Money result = null;
				var overrideCurrency = CustomsValueCurrencyOverride;
				if (overrideCurrency != null && !JI_CustomsValueOverride.IsEmpty)
				{
					result = new Money(JI_CustomsValueOverride, overrideCurrency);
				}
				return result;
			}
		}

		#endregion

		#region JI_PreviousEntryNumber

		[ResourceStringData("E4564016-BCB3-473B-BD4D-2B77C6C14DC4", Caption = "Previous MRN")]
		public override ZString JI_PreviousEntryNumber
		{
			get { return GetEffectiveValueToReturnFromEntryInstruction(base.JI_PreviousEntryNumber, CusEntryInstruction.Schema.CEI_PreviousMRN); }
			set
			{
				value = GetEffectiveValueToSetFromEntryInstruction(value, CusEntryInstruction.Schema.CEI_PreviousMRN);
				var oldValue = JI_PreviousEntryNumber;
				base.JI_PreviousEntryNumber = value;
				if (oldValue != JI_PreviousEntryNumber && IsDA63)
				{
					fImportBOEntryLine = null;
					DA63NeedsRecalculation = true;
				}
			}
		}

		#endregion

		#region JI_PreviousEntryNumber

		[ResourceStringData("91957A73-E2B5-4D34-B5D1-D719C7F4B38B", Caption = "VAT")]
		public override ZString JI_ZZF_NKTaxType
		{
			get { return base.JI_ZZF_NKTaxType; }
			set { base.JI_ZZF_NKTaxType = value; }
		}

		#endregion

		#region DA63

		#region JI_PreviousEntryLineNumber

		[ResourceStringData("C5E0D891-66A4-4E4E-B8ED-646BA5CB860B", Caption = "Previous MRN Line")]
		public override ZShort JI_PreviousEntryLineNumber
		{
			get { return base.JI_PreviousEntryLineNumber; }
			set
			{
				var oldValue = JI_PreviousEntryLineNumber;
				base.JI_PreviousEntryLineNumber = value > 9999 ? ZShort.Zero : value;
				if (oldValue != JI_PreviousEntryLineNumber && IsDA63)
				{
					fImportBOEntryLine = null;
					DA63NeedsRecalculation = true;
				}
			}
		}

		#endregion

		#region JI_ImportTariff

		[BusinessObjectMaxLengthTestExclude]
		public override ZString JI_ImportTariff
		{
			get { return base.JI_ImportTariff; }
			set
			{
				var oldValue = JI_ImportTariff;
				var newTariff = TariffFormatter.Format(value).Left(JI_ImportTariffInfo.MaxLength);
				base.JI_ImportTariff = newTariff;
				if (oldValue != JI_ImportTariff)
				{
					(JI_ImportCustomsQtyUQ, JI_ImportCustomsQty2UQ, JI_ImportCustomsQty3UQ) = GetDefaultCustomsQtyUnits(ImportTariff, ImportBOEntryLine?.RandomLine.CusLineTariffDetails.Cast<CusLineTariffDetail>() ?? Enumerable.Empty<CusLineTariffDetail>());
				}
			}
		}

		#endregion

		#region DA63 Additional Duties

		[ChildEditable]
		public DA63AdditionalDutyCollection DA63AdditionalDuties
		{
			get
			{
				if (da63AdditionalDuties == null)
				{
					da63AdditionalDuties = new DA63AdditionalDutyCollection(this);
					da63AdditionalDuties.Load();
					da63AdditionalDuties.AutoPopulateIfNeeded();
					RegisterEditableChildObject(da63AdditionalDuties);
				}
				return da63AdditionalDuties;
			}
		}
		DA63AdditionalDutyCollection da63AdditionalDuties;

		internal void RepopulateDA63AdditionalDuties()
		{
			DA63AdditionalDuties.RemoveAndDeleteAll();
			DA63AdditionalDuties.AutoPopulateIfNeeded();
		}

		#endregion

		public ZDecimal JI_ImportSch1P2BPaid => DA63AdditionalDuties.S1P2BDuty?.CY_Value ?? ZDecimal.Zero;

		public ZDecimal JI_ImportPenalty => DA63AdditionalDuties.Penalties.Sum(x => x.CY_Value);

		public ZDecimal JI_ImportProvisionalPayment => DA63AdditionalDuties.ProvisionalPayments.Sum(x => x.CY_Value);

		#endregion

		protected override ZDecimal GetJI_Calc_CIF()
		{
			Declaration?.AddInvoiceChargesFetchHintsIfNeeded();
			Money result = new Money(JI_Calc_FOB, LinePriceRefCurrency);
			result = CurrencyConverter.Add(result, JI_OverseasFreight);
			result = CurrencyConverter.Add(result, JI_OverseasInsurance);
			result = CurrencyConverter.ConvertExact(result, LinePriceRefCurrency);
			return result.Amount;
		}

		protected override ZDecimal GetJI_Calc_FOB()
		{
			Declaration?.AddInvoiceChargesFetchHintsIfNeeded();
			return CurrencyConverter.ConvertExact(new Money(JI_Calc_ActualPrice, LocalCurrency), LinePriceRefCurrency).Amount;
		}

		protected override ZDecimal GetGSTVATAmountCore()
		{
			var entryLine = CusEntryLine;
			return (entryLine == null) ? ZDecimal.Zero : GetAmountApportionedFromCusEntryLine(entryLine.Fees.GetAmount(JI_ZZF_NKTaxType)).Amount;
		}

		public ZDecimal GetGSTVATAmountIncludingLCOnly()
		{
			var entryLine = CusEntryLine;
			return (entryLine == null) ? ZDecimal.Zero : GetAmountApportionedFromCusEntryLine(entryLine.Fees.GetAmountIncludingLCOnly(JI_ZZF_NKTaxType)).Amount;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Permits))]
		[ResourceStringData("CCB6AF43-0A80-4C20-A597-6B3CA7105EDB", Caption = "Permit Number")]
		public override ZString JI_PermitNumber
		{
			get { return base.JI_PermitNumber; }
			set { base.JI_PermitNumber = value; }
		}

		protected override BaseCusClassPartPivot[] GetPivotsByTypeAndOwnerSupplier(BaseCusClassPartPivot[] pivots)
		{
			if (IsMiscellaneous & pivots.Length > 0)
			{
				var provider = GetClassificationTypeProvider();
				return GetMatches(pivots, provider, provider.HTBCode);
			}

			return base.GetPivotsByTypeAndOwnerSupplier(pivots);
		}

		protected override ZString GetPartPivotTypeCore()
		{
			return ClassificationTypeList.Codes.HTB;
		}

		public override void UpdateDetailsFromPivotOnPartChangeCore()
		{
			base.UpdateDetailsFromPivotOnPartChangeCore();
			PopulateVINPartAttributeFromJI_VIN();
		}

		protected override void ASNRefereshDataCountrySpecific(IEnumerable<ZString> refreshOptions, BaseCusClassPartPivot pivot)
		{
			base.ASNRefereshDataCountrySpecific(refreshOptions, pivot);
			if (refreshOptions.Contains(DefaultOptions.Codes.CountryOfOrigin))
			{
				JI_CountryOfOrigin = pivot.CI_RN_NKCountryOfOrigin;
			}
		}

		[MaxLength(CusVehicle.Schema.CVH_VehicleIdentificationNumberMaxLength)]
		[ResourceStringData("8A016A27-001E-4892-8C8D-B3689585E237", Caption = "VIN")]
		public ZString JI_VIN
		{
			get { return FirstVehicle.CVH_VehicleIdentificationNumber; }
			set
			{
				var vehicle = FirstVehicle;
				if (vehicle.CVH_VehicleIdentificationNumber != value)
				{
					CheckMaximumLength(JI_VINInfo, value);
					var oldValue = vehicle.CVH_VehicleIdentificationNumber;
					vehicle.CVH_VehicleIdentificationNumber = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJI_VIN();
					}
					if (oldValue != vehicle.CVH_VehicleIdentificationNumber && !IsCopying)
					{
						PopulateVINPartAttributeFromJI_VIN();
					}
					JI_VINInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo JI_VINInfo => GetZPropertyInfo(Schema.JI_VIN);

		internal int VINPartAttributeIndex
		{
			get
			{
				var result = 0;
				var importer = Importer;
				var product = Part;
				if (importer != null && product != null && importer.PartAttributeManager.HasVINForProduct(product))
				{
					result = importer.PartAttributeManager.VINPartAttribute.Index;
				}
				return result;
			}
		}

		internal ZPropertyInfo VINPartAttributeInfo
		{
			get
			{
				switch (VINPartAttributeIndex)
				{
					case 1:
						return JI_PartAttrib1Info;
					case 2:
						return JI_PartAttrib2Info;
					case 3:
						return JI_PartAttrib3Info;
					default:
						return null;
				}
			}
		}

		void PopulateJI_VINFromVINPartAttribute(int partAttributeIndex, ZString value)
		{
			if (VINPartAttributeIndex == partAttributeIndex && JI_VIN != value)
			{
				JI_VIN = value;
			}
		}

		void PopulateVINPartAttributeFromJI_VIN()
		{
			var value = JI_VIN;
			var vinPartAttributeInfo = VINPartAttributeInfo;
			if (vinPartAttributeInfo != null && (ZString)vinPartAttributeInfo.Value != value && value.Length <= vinPartAttributeInfo.MaxLength)
			{
				vinPartAttributeInfo.Value = value;
			}
		}

		#region New Owner Part

		public JobComInvoiceLinePartSynchronisationManager NewOwnerProductSyncManager
		{
			get { return newOwnerProductSyncManager; }
		}
		JobComInvoiceLinePartSynchronisationManager newOwnerProductSyncManager;

		public override ZGuid JI_OP_NewOwnerProduct
		{
			get { return base.JI_OP_NewOwnerProduct; }
			set
			{
				var oldValue = JI_OP_NewOwnerProduct;
				base.JI_OP_NewOwnerProduct = value;
				if (!IsCopying && oldValue != JI_OP_NewOwnerProduct)
				{
					if (NewOwnerProductSyncManager != null)
					{
						NewOwnerProductSyncManager.ReloadPart = true;
					}
				}
			}
		}

		public OrgSupplierPart NewOwnerProduct
		{
			get
			{
				OrgSupplierPart result = null;
				if (newOwnerProductSyncManager == null)
				{
					ErrorReporter.ReportOnce("NewOwnerProductSyncManagerNullAccess", "NewOwnerProductSyncManager has not been initialised");
				}
				else
				{
					if (NewOwnerProductSyncManager.Part != null && !NewOwnerProductSyncManager.Part.IsDeleted)
					{
						result = (OrgSupplierPart)NewOwnerProductSyncManager.Part;
					}
				}
				return result;
			}
		}

		[RelatedBusinessObject(nameof(NewOwnerProduct))]
		[ResourceStringData("Enterprise.Customs.ZA.Business.JobComInvoiceLine|UZ_NewOwnerPartNo", Caption = "Owner Product")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.NewOwnerProducts))]
		public override ZString JI_NewOwnerPartNo
		{
			get { return base.JI_NewOwnerPartNo; }
			set
			{
				using (GetValidationSuspender())
				{
					bool reloadNeeded = (JI_NewOwnerPartNo != value);
					base.JI_NewOwnerPartNo = value;
					if (reloadNeeded && !IsCopying)
					{
						NewOwnerProductSyncManager.Refresh();
						if (JI_NewOwnerPartNo.IsEmpty)
						{
							JI_NewOwnerPartAttrib1 = ZString.Empty;
							JI_NewOwnerPartAttrib2 = ZString.Empty;
							JI_NewOwnerPartAttrib3 = ZString.Empty;
							JI_NewOwnerSerialNum = ZString.Empty;
						}
					}
				}
				if (!IsCopying)
				{
					Validation.ValidateJI_NewOwnerPartNo();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.JobComInvoiceLine|UZ_NewOwnerPartAttrib1", Caption = "Owner Part Attrib. 1", ShortCaption = "Own Part Attrib. 1")]
		[ReadOnlyMember(nameof(NewOwnerProductIsNotSpecified))]
		public override ZString JI_NewOwnerPartAttrib1
		{
			get { return base.JI_NewOwnerPartAttrib1; }
			set { base.JI_NewOwnerPartAttrib1 = value; }
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.JobComInvoiceLine|UZ_NewOwnerPartAttrib2", Caption = "Owner Part Attrib. 2", ShortCaption = "Own Part Attrib. 2")]
		[ReadOnlyMember(nameof(NewOwnerProductIsNotSpecified))]
		public override ZString JI_NewOwnerPartAttrib2
		{
			get { return base.JI_NewOwnerPartAttrib2; }
			set { base.JI_NewOwnerPartAttrib2 = value; }
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.JobComInvoiceLine|UZ_NewOwnerPartAttrib3", Caption = "Owner Part Attrib. 3", ShortCaption = "Own Part Attrib. 3")]
		[ReadOnlyMember(nameof(NewOwnerProductIsNotSpecified))]
		public override ZString JI_NewOwnerPartAttrib3
		{
			get { return base.JI_NewOwnerPartAttrib3; }
			set { base.JI_NewOwnerPartAttrib3 = value; }
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.JobComInvoiceLine|UZ_NewOwnerSerialNum", Caption = "Owner Serial Number", ShortCaption = "Own Serial Num.")]
		[ReadOnlyMember(nameof(NewOwnerProductIsNotSpecified))]
		public override ZString JI_NewOwnerSerialNum
		{
			get { return base.JI_NewOwnerSerialNum; }
			set { base.JI_NewOwnerSerialNum = value; }
		}

		protected bool NewOwnerProductIsNotSpecified
		{
			get { return JI_NewOwnerPartNo.IsEmpty; }
		}

		#endregion

		protected override ICusVehicleCollection<Customs.Business.CusVehicle, BaseJobComInvoiceLine> GetNewCusVehicleCollection() => new CusVehicleCollection<CusVehicle, JobComInvoiceLine>(this);

		public override VehicleRelationshipType VehicleRelationship => VehicleRelationshipType.One;

		public new CusVehicle FirstVehicle => (CusVehicle)base.FirstVehicle;

		#region Override AddInfo Properties

		[MaxLength(JI_EngineNumberMaxLength)]
		[ResourceStringData("98697894-6BAC-4129-B0D0-0198743DF570", Caption = "Engine No.")]
		public ZString JI_EngineNumber
		{
			get { return FirstVehicle.CVH_SerialNumber; }
			set
			{
				var vehicle = FirstVehicle;
				var oldValue = vehicle.CVH_SerialNumber;
				CheckMaximumLength(JI_EngineNumberInfo, value);
				vehicle.CVH_SerialNumber = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJI_EngineNumber();
				}
				JI_EngineNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JI_EngineNumberInfo => GetZPropertyInfo(Schema.JI_EngineNumber);

		const int JI_EngineNumberMaxLength = 20;

		[DecimalPlaces(2)]
		[ResourceStringData("963211F0-1F3F-424F-8EEF-D2249DC90900", ShortCaption = "Markup %", Caption = "Valuation Markup %", FullDescription = "Valuation Markup Percent")]
		public override ZDecimal JI_ValuationMarkup
		{
			get { return base.JI_ValuationMarkup; }
			set { base.JI_ValuationMarkup = value; }
		}

		[ResourceStringData("2d3d66e1-c178-4c90-9c2f-b8b37a6195b1", Caption = "Target Entry Line #", FullDescription = "Target Entry Line Number")]
		public override ZShort JI_TargetEntryLineNumber
		{
			get { return base.JI_TargetEntryLineNumber; }
			set { base.JI_TargetEntryLineNumber = value; }
		}

		[ResourceStringData("15F6DA6C-58B6-49B8-851C-B65D731DD7E6", Caption = "Take Up In Trade Statistics")]
		[BusinessObjectTestExclude()]
		public override ZBool JI_TakeUpInTradeStatistics
		{
			get
			{
				return IsExport ? base.JI_TakeUpInTradeStatistics : ZBool.False;
			}
			set { base.JI_TakeUpInTradeStatistics = value; }
		}

		[MaxLength(JI_VehicleFormatMaxLength)]
		[ResourceStringData("32D4A28D-03F1-41BC-9000-1F146689322E", ShortCaption = "Veh. Format", Caption = "Vehicle Format")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.VehicleFormats))]
		public ZString JI_VehicleFormat
		{
			get { return FirstVehicle.CVH_SupplyMethod; }
			set
			{
				var vehicle = FirstVehicle;
				var oldValue = vehicle.CVH_SupplyMethod;
				CheckMaximumLength(JI_VehicleFormatInfo, value);
				vehicle.CVH_SupplyMethod = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJI_VehicleFormat();
				}
				JI_VehicleFormatInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JI_VehicleFormatInfo => GetZPropertyInfo(Schema.JI_VehicleFormat);

		const int JI_VehicleFormatMaxLength = 3;

		[MaxLength(CusVehicle.Schema.CVH_CarTypeMaxLength)]
		[ResourceStringData("B5B9BC42-A123-4896-9E3C-9779DD6C5697", ShortCaption = "Veh. Type", Caption = "Vehicle Type")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.VehicleTypes))]
		public ZString JI_VehicleType
		{
			get { return FirstVehicle.CVH_CarType; }
			set
			{
				var vehicle = FirstVehicle;
				var oldValue = vehicle.CVH_CarType;
				CheckMaximumLength(JI_VehicleTypeInfo, value);
				vehicle.CVH_CarType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJI_VehicleType();
				}
				JI_VehicleTypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JI_VehicleTypeInfo => GetZPropertyInfo(Schema.JI_VehicleType);

		[ResourceStringData("777D86D5-E4A5-4E9A-9244-A28CB3877E94", Caption = "Goods Type")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.GoodsTypeList))]
		public override ZString JI_NewUsed
		{
			get { return base.JI_NewUsed; }
			set
			{
				var oldValue = JI_NewUsed;
				base.JI_NewUsed = value;
				if (!IsCopying && oldValue != JI_NewUsed)
				{
					CusLineTariffDetails.MarkAsNeedingValidation();
				}
			}
		}

		[BusinessObjectMaxLengthTestExclude]
		[MaxLength(JI_YearOfManufactureMaxLength)]
		[ResourceStringData("40F17B49-E390-46D6-83F5-D9405DA0AECA", ShortCaption = "YOM", Caption = "Year Of Manufacture")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.YearList))]
		public ZString JI_YearOfManufacture
		{
			get
			{
				var manufacturedDate = FirstVehicle.CVH_ManufacturedDate;
				if (manufacturedDate == ZDate.Empty)
				{
					return ZString.Empty;
				}
				else
				{
					return manufacturedDate.Year.ToString();
				}
			}
			set
			{
				var vehicle = FirstVehicle;
				var oldValue = vehicle.CVH_ManufacturedDate;
				if (JI_YearOfManufacture != value)
				{
					CheckMaximumLength(JI_YearOfManufactureInfo, value);
					if (value.IsEmpty || !Int32.TryParse(value, out var year))
					{
						vehicle.CVH_ManufacturedDate = ZDate.Empty;
					}
					else
					{
						vehicle.CVH_ManufacturedDate = new ZDate(year, 1, 1);
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateJI_YearOfManufacture();
					}
					JI_YearOfManufactureInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo JI_YearOfManufactureInfo => GetZPropertyInfo(Schema.JI_YearOfManufacture);

		const int JI_YearOfManufactureMaxLength = 4;

		[ResourceStringData("08CE8D4B-B4A4-4C60-8E03-896150767A68", Caption = "Customs Value", FullDescription = "Customs Value Override")]
		public override ZDecimal JI_CustomsValueOverride
		{
			get { return base.JI_CustomsValueOverride; }
			set
			{
				base.JI_CustomsValueOverride = value;
				if (JI_CustomsValueOverride.IsEmpty)
				{
					JI_RX_NKCustomsValueCurrencyOverride = ZString.Empty;
				}
				else if (JI_RX_NKCustomsValueCurrencyOverride.IsEmpty)
				{
					JI_RX_NKCustomsValueCurrencyOverride = LocalCurrency?.Code ?? ZString.Empty;
				}
			}
		}

		[LightValidationTestExempt]
		[RelatedBusinessObject(nameof(CustomsValueCurrencyOverride))]
		[ResourceStringData("DDC43B44-5F85-4D5B-BCCB-35B37F95A042", ShortCaption = "Curr", Caption = "Currency", FullDescription = "Customs Value Currency Override")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsValueCurrencyOverrideList))]
		public override ZString JI_RX_NKCustomsValueCurrencyOverride
		{
			get { return base.JI_RX_NKCustomsValueCurrencyOverride; }
			set { base.JI_RX_NKCustomsValueCurrencyOverride = value; }
		}

		[ResourceStringData("72B17F40-654D-4CC4-818D-88503D8D172A", Caption = "CO2")]
		public override ZDecimal JI_CO2Emission
		{
			get { return base.JI_CO2Emission; }
			set { base.JI_CO2Emission = value; }
		}

		[ResourceStringData("26801358-E451-41E8-98FC-D463B625F04E", Caption = "Engine CC")]
		public ZInt JI_EngineCapacity
		{
			get { return FirstVehicle.CVH_EngineCapacity; }
			set
			{
				var vehicle = FirstVehicle;
				var oldValue = vehicle.CVH_EngineCapacity;
				vehicle.CVH_EngineCapacity = (value > short.MaxValue && value < ZInt.Zero) ? ZShort.Zero : (short)value;
				vehicle.CVH_EngineCapacityUQ = "CC";
				if (!IsValidationSuspended)
				{
					Validation.ValidateJI_EngineCapacity();
				}
				JI_EngineCapacityInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JI_EngineCapacityInfo => GetZPropertyInfo(Schema.JI_EngineCapacity);

		[MaxLength(CusVehicle.Schema.CVH_ColorMaxLength)]
		[ResourceStringData("09B6D069-AA02-4FAC-B300-DFB7C6FE9CCE", Caption = "Color")]
		public ZString JI_Colour
		{
			get { return FirstVehicle.CVH_Color; }
			set
			{
				var vehicle = FirstVehicle;
				var oldValue = vehicle.CVH_Color;
				CheckMaximumLength(JI_ColourInfo, value);
				vehicle.CVH_Color = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJI_Colour();
				}
				JI_ColourInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JI_ColourInfo => GetZPropertyInfo(Schema.JI_Colour);

		[ResourceStringData("B5DF9664-A8F1-4516-A45E-99D41E953E60", Caption = "Commission No.")]
		public override ZString JI_CommissionNumber
		{
			get { return base.JI_CommissionNumber; }
			set { base.JI_CommissionNumber = value; }
		}

		[ResourceStringData("6B73E3FB-F557-4791-863A-375D1A44778C", Caption = "Model")]
		public override ZString JI_Model
		{
			get { return base.JI_Model; }
			set { base.JI_Model = value; }
		}

		[MaxLength(JI_MakeMaxLength)]
		[ResourceStringData("2753104D-CAFB-41A3-8494-7E4A4D96C4CA", Caption = "Make")]
		public ZString JI_Make
		{
			get { return FirstVehicle.CVH_ModelName; }
			set
			{
				var vehicle = FirstVehicle;
				var oldValue = vehicle.CVH_ModelName;
				CheckMaximumLength(JI_MakeInfo, value);
				vehicle.CVH_ModelName = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJI_Make();
				}
				JI_MakeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JI_MakeInfo => GetZPropertyInfo(Schema.JI_Make);

		const int JI_MakeMaxLength = 35;

		[ResourceStringData("E40D0EDB-BF75-437C-9BD3-7079A4622807", Caption = "Advance Payment Notification Number", ShortCaption = "APN No")]
		public override ZString JI_AdvancePaymentNo
		{
			get { return base.JI_AdvancePaymentNo; }
			set { base.JI_AdvancePaymentNo = value; }
		}

		[ResourceStringData("7E7A10D0-1F91-419E-8E4F-D3AC3A173F3D", Caption = "Conversion Factor")]
		public override ZDecimal JI_ConversionFactor
		{
			get => base.JI_ConversionFactor;
			set => base.JI_ConversionFactor = value;
		}

		#endregion

		public override bool CanConvertFromNetWeightToCustomsUnit(ZString customsUnit)
		{
			return customsUnit != QuantityCodeList.Codes.Volume_M3 && base.CanConvertFromNetWeightToCustomsUnit(customsUnit);
		}

		#endregion

		#region New Properties/Methods

		[ResourceStringData("EF3F2BE5-870B-4E2B-9711-D657A0A60625", Caption = "Linked Entry Line #", FullDescription = "Linked Entry Line Number")]
		public ZShort LinkedEntryLineNumber => CusEntryLine?.CL_LineNumber ?? ZShort.Zero;

		public ZPropertyInfo LinkedEntryLineNumberInfo => GetZPropertyInfo(Schema.LinkedEntryLineNumber);

		#region Procedure Category
		public ZString ProcedureCategory => EntryInstruction?.ProcedureCategory ?? ZString.Empty;

		public ZString PreviousProcedureCategory => PreviousProcedure?.ZZ6_Category ?? ZString.Empty;
		#endregion

		#region Apportioned Values from dbo.CusEntryLine

		#region CustomsValue

		public
#if DEBUG
		virtual
#endif
		ZDecimal ApportionedCustomsValue
		{
			get { return (CusEntryLine != null) ? GetAmountApportionedFromCusEntryLine(CusEntryLine.CL_CustomsValue).Amount : ZDecimal.Zero; }
		}

		#endregion

		#region DutySch1P2B

		public
#if DEBUG
		virtual
#endif
		ZDecimal ApportionedDutySch1P2B
		{
			get { return (CusEntryLine != null) ? GetAmountApportionedFromCusEntryLine(CusEntryLine.DutySch1P2B).Amount : ZDecimal.Zero; }
		}

		public ZDecimal GetDutySch1P2BAmountIncludingLCOnly()
		{
			var entryLine = CusEntryLine;
			return (entryLine == null) ? ZDecimal.Zero : GetAmountApportionedFromCusEntryLine(entryLine.GetDutySch1P2B(true)).Amount;
		}

		#endregion

		#region RefundRebateCode

		[MaxLength(35)]
		[List(nameof(RefundRebateTariffCollection))]
		[ResourceStringData("D6267E46-852D-443D-891E-EAACFC7CFC6C", Caption = "Refund/Rebate Code")]
		public ZString RefundRebateCode
		{
			get
			{
				var result = refundRebateCode;

				var firstRefundRebate = FirstTariffDetailForConcession;
				if (!firstRefundRebate?.BZ_Tariff.IsEmpty ?? false)
				{
					result = firstRefundRebate.BZ_Tariff;
				}

				return result;
			}
			set
			{
				var oldValue = RefundRebateCode;
				SetNonPersistentPropertyValue(RefundRebateCodeInfo, ref refundRebateCode, value);

				if (oldValue != refundRebateCode)
				{
					DefaultRefundRebateCode(value);
				}
			}
		}
		ZString refundRebateCode;

		public ZPropertyInfo RefundRebateCodeInfo
		{
			get { return GetZPropertyInfo(Schema.RefundRebateCode); }
		}

		public ZBool IsRefundRebateTariff
		{
			get
			{
				const string RefundRebateCodePrefix52203 = "52203";
				const string RefundRebateCodePrefix53600 = "53600";
				return RefundRebateCode.StartsWith(RefundRebateCodePrefix52203) || RefundRebateCode.StartsWith(RefundRebateCodePrefix53600);
			}
		}

		public ZBool IsRefundRebateType5P
		{
			get
			{
				const string RefundRebateTypePrefix5P = "5P";
				var firstRefundRebate = FirstTariffDetailForConcession;
				if (!firstRefundRebate?.BZ_Tariff.IsEmpty ?? false)
				{
					return firstRefundRebate.BZ_Type.StartsWith(RefundRebateTypePrefix5P);
				}
				return false;
			}
		}

		public ChildTariffViewCollection RefundRebateTariffCollection
		{
			get
			{
				var schedule = CusProcedure?.Concessions?.FirstNonSpecificTariffType(Factory) ?? ZString.Empty;
				return ChildTariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, schedule, EffectiveAssessmentDate, CusLineTariffDetailLookups.GetApplicableTariffDetail(this, FirstTariffDetailForConcession?.UniversalTariffType));
			}
		}

		void DefaultRefundRebateCode(ZString tariff)
		{
			var tariffDetail = FirstTariffDetailForConcession;
			if (tariffDetail != null)
			{
				tariffDetail.BZ_Tariff = tariff;
			}
			else
			{
				var candidate = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, tariff, EffectiveAssessmentDate);
				var hasRebateOrRefund = candidate?.Rates.Any(x => x.ZZ2_ZZR_RateTypeCode == Constants.RateTypes.Rebate || x.ZZ2_ZZR_RateTypeCode == Constants.RateTypes.Refund) ?? ZBool.False;
				if (hasRebateOrRefund)
				{
					CusLineTariffDetails.AddNew().BZ_Tariff = tariff;
				}
			}
		}

		public CusLineTariffDetail FirstTariffDetailForConcession
		{
			get
			{
				var schedule = CusProcedure?.Concessions?.FirstNonSpecificTariffType(Factory) ?? ZString.Empty;
				return CusLineTariffDetails.OfType<CusLineTariffDetail>().FirstOrDefault(x =>
				{
					return (x.UniversalTariffType?.ZZI_TariffType.Left(1) ?? ZString.Empty) == schedule;
				});
			}
		}

		#endregion

		#region RefundRebateValue

		[ResourceStringData("F477BEC3-A63C-4E55-8871-B2F58E00D6CE", Caption = "Refund/Rebate Value")]
		public ZString RefundRebateValue
		{
			get
			{
				var result = refundRebateValue;

				var firstRefundRebate = FirstTariffDetailForConcession;
				if (!firstRefundRebate?.FormulaSpecificValue.IsEmpty ?? false)
				{
					result = firstRefundRebate.FormulaSpecificValue;
				}

				return result;
			}
			set
			{
				var oldValue = RefundRebateValue;
				SetNonPersistentPropertyValue(RefundRebateValueInfo, ref refundRebateValue, value);

				if (oldValue != refundRebateValue)
				{
					var tariffDetail = FirstTariffDetailForConcession;
					if (tariffDetail != null)
					{
						tariffDetail.FormulaSpecificValue = value;
					}
				}
			}
		}
		ZString refundRebateValue;

		public ZPropertyInfo RefundRebateValueInfo
		{
			get { return GetZPropertyInfo(Schema.RefundRebateValue); }
		}

		public bool RefundRebateValue_ReadOnly
		{
			get { return FirstTariffDetailForConcession?.FormulaSpecificValueInfo.ReadOnly ?? true; }
		}

		#endregion

		#endregion

		#region JI_PrimaryPreference

		public ZString TradeAgreementLabel => Res.GetString("F48A4483-8CBF-4B1E-824E-C862342A1A21", "Trade Agreement: {0}", TradeAgreementDescription);

		public ZString TradeAgreementDescription => TradeAgreement.Value;

		[ResourceStringData("B4F93BF2-F507-4581-B0AB-9AB0FA8B9CBA", Caption = "Trade Agreement")]
		public ZString TradeAgreementCode => TradeAgreement.Key;

		public KeyValuePair<ZString, ZString> TradeAgreement => UniversalReferenceDataHelper.GetTradeGroupByPreference(UniversalTariff, AllApplicableRatesSelectionCriteria);

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ROOTypeOrPreferenceList))]
		public override ZString JI_PrimaryPreference
		{
			get
			{
				return IsExport ? base.JI_PrimaryPreference : GetEffectiveValueToReturn(base.JI_PrimaryPreference, JobComInvoiceHeader.Schema.JZ_ROOType);
			}
			set
			{
				var oldValue = JI_PrimaryPreference;
				base.JI_PrimaryPreference = IsExport ? value : GetEffectiveValueToSet(value, JobComInvoiceHeader.Schema.JZ_ROOType);

				if (!IsCopying && oldValue != JI_PrimaryPreference)
				{
					CusLineTariffDetails.MarkAsNeedingValidation();

					if (IsExport)
					{
						if (JI_PrimaryPreference == InvoiceHeader.JZ_ROOType && JI_ROOCert.IsEmpty)
						{
							JI_ROOCert = InvoiceHeader.JZ_ROOCert;
						}
					}
					else
					{
						ClearRulesOfOriginCertificateIfApplicable();
					}

					JI_ROOCertInfo.RefreshBinding();
				}

				JI_ConcessionOrder = JI_PrimaryPreference == UniversalReferenceConstants.PrimaryPreference.PreferentialQuota ? JI_ConcessionOrder = UniversalReferenceConstants.ConcessionOrder.Quota : ZString.Empty;
			}
		}

		#endregion

		#region DA63

		#region RecalculateDA63Values

		public void RecalculateDA63Values()
		{
			if (DA63NeedsRecalculation)
			{
				var newTariff = TariffFromRelatedImportBOE;
				newTariff = newTariff.IsEmpty ? JI_Tariff : newTariff;
				JI_ImportTariff = newTariff;
				if (JI_ImportTariff == JI_Tariff)
				{
					JI_ImportCustomsQty = JI_CustomsQuantity;
				}

				RepopulateDA63AdditionalDuties();
				if (ImportBOEntryLine != null)
				{
					var factor = ZDecimal.Zero;
					if (JI_ImportCustomsQtyUQ == CustomsQtyUQFromRelatedImportBOE && !CustomsQtyFromRelatedImportBOE.IsEmpty)
					{
						factor = JI_ImportCustomsQty / CustomsQtyFromRelatedImportBOE;
					}

					JI_ImportCustomsValue = CustomsValueFromRelatedImportBOE * factor;
					JI_ImportDutyPaid = CustomsDutyFromRelatedImportBOE * factor;
					JI_ImportVATPaid = ZDecimal.Zero;

					foreach (var additionalDuty in DA63AdditionalDuties.Cast<DA63AdditionalDuty>())
					{
						additionalDuty.CY_Value = additionalDuty.OriginValue * factor;
					}

					JI_ConversionFactor = ConversionFactorFromRelatedImportBOE;
				}
			}
			DA63NeedsRecalculation = false;
		}

		#endregion

		public ZBool IsDA63 => IsDA63Core;

		protected virtual ZBool IsDA63Core => (CusProcedure?.Concessions?.FirstNonSpecificTariffType(Factory) ?? ZString.Empty) == UniversalReferenceConstants.Schedule._5;

		public ZBool IsDA63WithOriginalEntry => IsDA63 && ImportBOEntryLine != null;

		#region JI_ImportCustomsQtyUQ

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.AdditionalUnitCodeList))]
		public override ZString JI_ImportCustomsQtyUQ
		{
			get => base.JI_ImportCustomsQtyUQ;
			set => base.JI_ImportCustomsQtyUQ = value;
		}

		#endregion

		#region JI_ImportCustomsQty2

		[ResourceStringData("B25ED0CE-828A-49A7-A578-928E4A670CFB|UZ_ImportCustomsQty2", Caption = "Additional Qty 1")]
		public override ZDecimal JI_ImportCustomsQty2
		{
			get => base.JI_ImportCustomsQty2;
			set => base.JI_ImportCustomsQty2 = value;
		}

		#endregion

		#region JI_ImportCustomsQty2UQ

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.AdditionalUnitCodeList))]
		public override ZString JI_ImportCustomsQty2UQ
		{
			get => base.JI_ImportCustomsQty2UQ;
			set => base.JI_ImportCustomsQty2UQ = value;
		}

		#endregion

		#region JI_ImportCustomsQty3

		[ResourceStringData("81D71084-EB29-4B25-B01D-BC3FDB6137A4|UZ_ImportCustomsQty3", Caption = "Additional Qty 2")]
		public override ZDecimal JI_ImportCustomsQty3
		{
			get => base.JI_ImportCustomsQty3;
			set => base.JI_ImportCustomsQty3 = value;
		}

		#endregion

		#region JI_ImportCustomsQty3UQ

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.AdditionalUnitCodeList))]
		public override ZString JI_ImportCustomsQty3UQ
		{
			get => base.JI_ImportCustomsQty3UQ;
			set => base.JI_ImportCustomsQty3UQ = value;
		}

		#endregion

		#region RelatedImportBOENumberString

		public ZString RelatedImportBOENumberString
		{
			get { return (ImportBOEntryLine == null) ? Res.GetString("481A8803-8DEA-483B-9543-8F13EDD1CFFC", "Original Entry not found, all DA63 values must be manually entered") : string.Empty; }
		}

		public ZPropertyInfo RelatedImportBOENumberStringInfo
		{
			get { return GetZPropertyInfo(Schema.RelatedImportBOENumberString); }
		}

		#endregion

		#region TariffFromRelatedImportBOE

		public ZString TariffFromRelatedImportBOE
		{
			get { return IsDA63WithOriginalEntry ? ImportBOEntryLine.CL_AdValoremTariff : ZString.Empty; }
		}

		public ZPropertyInfo TariffFromRelatedImportBOEInfo
		{
			get { return GetZPropertyInfo(Schema.TariffFromRelatedImportBOE); }
		}

		#endregion

		#region CustomsQtyFromImportBOE

		public ZDecimal CustomsQtyFromRelatedImportBOE
		{
			get { return IsDA63WithOriginalEntry ? ImportBOEntryLine.CustomsQuantity : ZDecimal.Zero; }
		}

		public ZPropertyInfo CustomsQtyFromRelatedImportBOEInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsQtyFromRelatedImportBOE); }
		}

		#endregion

		#region CustomsQtyUQFromImportBOE

		public ZString CustomsQtyUQFromRelatedImportBOE
		{
			get { return IsDA63WithOriginalEntry ? ImportBOEntryLine.CustomsUnitQty : ZString.Empty; }
		}

		public ZPropertyInfo CustomsQtyUQFromRelatedImportBOEInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsQtyUQFromRelatedImportBOE); }
		}

		#endregion

		#region CustomsValueFromRelatedImportBOE

		public ZDecimal CustomsValueFromRelatedImportBOE
		{
			get { return IsDA63WithOriginalEntry ? ImportBOEntryLine.CustomsValue.Amount : ZDecimal.Zero; }
		}

		public ZPropertyInfo CustomsValueFromRelatedImportBOEInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsValueFromRelatedImportBOE); }
		}

		#endregion

		#region CustomsDutyFromRelatedImportBOE

		public ZDecimal CustomsDutyFromRelatedImportBOE
		{
			get { return IsDA63WithOriginalEntry ? ImportBOEntryLine.GetCalcFeeValues().CustomsDutiesSchedule1P1andSchedule2 : ZDecimal.Zero; }
		}

		public ZPropertyInfo CustomsDutyFromRelatedImportBOEInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsDutyFromRelatedImportBOE); }
		}

		#endregion

		#region VATFromRelatedImportBOE

		public ZDecimal VATFromRelatedImportBOE
		{
			get { return IsDA63WithOriginalEntry ? ImportBOEntryLine.GetCalcFeeValues().ValueAddedTax : ZDecimal.Zero; }
		}

		public ZPropertyInfo VATFromRelatedImportBOEInfo
		{
			get { return GetZPropertyInfo(Schema.VATFromRelatedImportBOE); }
		}

		#endregion

		#region ConversionFactorFromRelatedImportBOE

		public ZDecimal ConversionFactorFromRelatedImportBOE
		{
			get { return IsDA63WithOriginalEntry ? ImportBOEntryLine.ConversionFactor : ZDecimal.Zero; }
		}

		public ZPropertyInfo ConversionFactorFromRelatedImportBOEInfo
		{
			get { return GetZPropertyInfo(Schema.ConversionFactorFromRelatedImportBOE); }
		}

		#endregion

		#endregion

		#region CustomsValueInLocalCurrency

		public ZDecimal CustomsValueInLocalCurrency
		{
			get { return JI_CustomsValue; }
		}

		#endregion

		#region GetTariffDescription
		protected override ZString GetTariffDescription(ZString tariffCode)
		{
			return "";
		}
		#endregion

		#region EffectiveCountryOfOrigin
		public new
#if DEBUG
		virtual
#endif
		ZString EffectiveCountryOfOrigin
		{
			get
			{
				var country = base.EffectiveCountryOfOrigin;
				return country.IsEmpty && (Declaration?.IsImport ?? false) ? new ZString("ZN") : country;
			}
		}
		#endregion

		#region EffectiveAssessmentDate

		public override ZDateTime EffectiveAssessmentDate => CusEntryInstruction.GetEffectiveAssessmentDate(EntryInstruction, Factory);

		#endregion

		#region CusLineTariffDetail

		[ChildEditable(true)]
		[BusinessObjectTestExclude]
		[UniversalCopyCollectionEntity(CusLineTariffDetailSchema.Constants.TableName, CusLineTariffDetailSchema.Constants.BZ_ParentID)]
		public new ICusLineTariffDetailCollection<CusLineTariffDetail> CusLineTariffDetails => (CusLineTariffDetailCollection<CusLineTariffDetail>)base.CusLineTariffDetails;

		protected override ICusLineTariffDetailCollection<Customs.Business.CusLineTariffDetail> GetCusLineTariffDetails() => new CusLineTariffDetailCollection<CusLineTariffDetail>(this);

		protected override void CusLineTariffDetailsCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			DefaultUOMFromTariffAndCusLineTariffDetailsIfNeeded();
		}

		protected override bool SupportsAdditionalTariffs => true;

		#endregion

		public TariffView ProcedureMeasureTariff => Factory.GetValue(ref procedureMeasureTariffCached, () =>
					{
						TariffView result = null;
						var concession = CusProcedure?.Concessions?.FirstNonSpecificTariffType(Factory) ?? ZString.Empty;
						if (!concession.IsEmpty)
						{
							var details = CusLineTariffDetails?.OrderBy(x => x.BZ_Type).FirstOrDefault(x => x.BZ_Type.Left(1) == concession && !x.UniversalTariff.IsPayableDuty());
							if (details != null)
							{
								result = details.UniversalTariff;
							}
						}
						else if (Declaration != null && Declaration.IsExport)
						{
							var lineTariff = CusLineTariffDetails?.FirstOrDefault();
							if (lineTariff != null && lineTariff.BZ_Type.StartsWith(UniversalReferenceConstants.Schedule._6))
							{
								result = lineTariff.UniversalTariff;
							}
						}
						return result;
					});

		CachedProperty<TariffView> procedureMeasureTariffCached;

		public bool IsMiscellaneous => InvoiceHeader?.JobDeclaration?.IsMiscellaneous ?? false;

		#region JI_Calc_ATV

		[DecimalPlaces(0)]
		public ZDecimal JI_Calc_ATV
		{
			get
			{
				ZDecimal result = 0m;
				var applicableTaxRate = new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.SouthAfrica, JI_ZZF_NKTaxType, EffectiveAssessmentDate)?.ZZF_Value ?? ZDecimal.Zero;
				if (!applicableTaxRate.IsEmpty)
				{
					result = JI_Calc_GSTVATAmount / applicableTaxRate;
				}
				return result.RoundUsingCustomsValueRule();
			}
		}

		public ZPropertyInfo JI_Calc_ATVInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_ATV); }
		}

		#endregion

		#region JI_Calc_ActualPrice

		public ZDecimal JI_Calc_ActualPrice => Factory.GetValue(ref cachedJI_Calc_ActualPrice, () =>
					{
						var result = 0m;
						var currencyConverter = CurrencyConverter;
						if (currencyConverter != null)
						{
							var linePriceRefCurrency = LinePriceRefCurrency;
							var resultMoney = new Money(JI_LinePrice, linePriceRefCurrency);
							var lineDiscountAmount = Charges.GetTotal(currencyConverter, linePriceRefCurrency, (charge) => charge.IsDiscount && !charge.J7_IsIncludedInITOT)
								+ ApportionedCharges.GetTotal(currencyConverter, linePriceRefCurrency, (charge) => charge.IsDiscount && !charge.J7_IsIncludedInITOT);
							result = (resultMoney.Amount - lineDiscountAmount) * (InvoiceHeader?.JZ_Calc_ConversionFactor ?? 0m);
						}
						return result;
					});

		CachedProperty<ZDecimal> cachedJI_Calc_ActualPrice;

		public ZPropertyInfo JI_Calc_ActualPriceInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_ActualPrice); }
		}

		#endregion

		#region JI_CEI_Description

		[ResourceStringData("4B49EDAB-F359-4EA2-9655-6177DF98F043", ShortCaption = "Entry Ins. Desc.", Caption = "Entry Ins. Description", FullDescription = "Entry Instruction Description")]
		public ZString JI_CEI_Description => EntryInstruction?.CEI_Description ?? ZString.Empty;

		public ZPropertyInfo JI_CEI_DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JI_CEI_Description); }
		}

		#endregion

		#region JI_CEI Related Methods

		void SetDefaultPreviousProcedureCode()
		{
			var ppcList = Lookups.Procedures;
			if (ppcList != null && ppcList.Count > 0)
			{
				JI_Procedure = ((CodeDescriptionPair)ppcList[0]).Code;
			}
			else
			{
				JI_Procedure = string.Empty;
			}
		}

		public ZString JI_ProcedureCode
		{
			get { return ProcedureCode; }
		}

		#endregion

		public ZDecimal JI_Calc_ValuationMarkup
		{
			get
			{
				var markupPercentage = IsImport ? JI_ValuationMarkup : ZDecimal.Zero;
				return (JI_Calc_ActualPrice - JI_Calc_IntellectualValue) * markupPercentage / 100m;
			}
		}

		public ZDecimal JI_Calc_IntellectualValue => JI_Calc_IntellectualValue_Core * (InvoiceHeader?.JZ_Calc_ConversionFactor ?? 0m);

		public ZDecimal JI_Calc_IntellectualValue_Core
		{
			get
			{
				var result = ZDecimal.Zero;

				var currencyConverter = CurrencyConverter;
				if (currencyConverter != null)
				{
					var intellectualValue = Charges.GetTotal(CurrencyConverter, LinePriceRefCurrency, (charge) =>
					{
						return charge.J7_ChargeType == InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue
							&& !charge.J7_IsDutiable
							&& charge.J7_IsIncludedInITOT;
					});
					result = intellectualValue;
				}
				return result;
			}
		}

		Money GetAmountInApportionedChargesCurrency(Money apportionedCharge)
		{
			var targetCurrency = ApportionedCharges.GetCommonApportionedChargesCurrencyOrInvoiceHeaderCurrency(InvoiceHeader.Invoice_Currency);
			if (targetCurrency != null && targetCurrency.Code != apportionedCharge.Currency.Code)
			{
				return CurrencyConverter.ConvertExact(apportionedCharge, targetCurrency, roundToDestinationCurrencyDecimals: false); // watch out for "converting" Yen to Yen, because rounding to zero decimal places will occur!
			}
			else
			{
				return apportionedCharge;
			}
		}

		#region DutiableChargesIncludedInLines

		public virtual Money DutiableChargesIncludedInLines
		{
			get
			{
				Money result = CurrencyConverter.Add(Charges.GetCharge(true, true), ApportionedCharges.GetCharge(true, true));
				return CurrencyConverter.Subtract(result, GetCharge(new MessageChargeKey(CustomsChargeTypeList.Codes.Discount, true, true, true)));
			}
		}

		#endregion

		#region DutiableChargesNotIncludedInLines

		public virtual Money DutiableChargesNotIncludedInLines
		{
			get
			{
				Money result = CurrencyConverter.Add(Charges.GetCharge(true, false), ApportionedCharges.GetCharge(true, false));
				result = CurrencyConverter.Subtract(result, GetCharge(new MessageChargeKey(CustomsChargeTypeList.Codes.Discount, true, true, false)));
				return GetAmountInApportionedChargesCurrency(result);
			}
		}

		#endregion

		#region NonDutiableChargesIncludedInLinesExcludingFreightAndInsurance

		public virtual Money NonDutiableChargesIncludedInLinesExcludingFreightAndInsurance
		{
			get
			{
				Money result = CurrencyConverter.Add(Charges.GetCharge(false, true), ApportionedCharges.GetCharge(false, true));
				result = CurrencyConverter.Subtract(result, OverseasFreightIncludedInLines);
				result = CurrencyConverter.Subtract(result, OverseasInsuranceIncludedInLines);
				result = CurrencyConverter.Subtract(result, GetCharge(new MessageChargeKey(CustomsChargeTypeList.Codes.Discount, false, false, true)));
				result = CurrencyConverter.Subtract(result, GetCharge(new ChargeCodeChargeKey(InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue, false, false)));
				return result;
			}
		}

		#endregion

		#region NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance

		public virtual Money NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance
		{
			get
			{
				Money result = CurrencyConverter.Add(Charges.GetCharge(false, false), ApportionedCharges.GetCharge(false, false));
				result = CurrencyConverter.Subtract(result, OverseasFreightNotIncludedInLines);
				result = CurrencyConverter.Subtract(result, OverseasInsuranceNotIncludedInLines);
				result = CurrencyConverter.Subtract(result, GetCharge(new MessageChargeKey(CustomsChargeTypeList.Codes.Discount, false, false, false)));
				return GetAmountInApportionedChargesCurrency(result);
			}
		}

		#endregion

		#region OverseasFreightIncludedInLines

		public virtual Money OverseasFreightIncludedInLines
		{
			get
			{
				Money result = Money.Empty;

				if (InvoiceHeader != null)
				{
					var chargeCodeKey = InvoiceHeader.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight)?.ChargeCodeChargeKey;
					if (chargeCodeKey != null)
					{
						result = GetCharge(new MessageChargeKey(chargeCodeKey.ChargeCode, chargeCodeKey.IsDutiable, chargeCodeKey.IsVATible, true));
					}
				}
				return result;
			}
		}

		#endregion

		#region OverseasFreightNotIncludedInLines

		public virtual Money OverseasFreightNotIncludedInLines
		{
			get
			{
				Money result = Money.Empty;

				if (InvoiceHeader != null)
				{
					var chargeCodeKey = InvoiceHeader.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight)?.ChargeCodeChargeKey;
					if (chargeCodeKey != null)
					{
						result = GetCharge(new MessageChargeKey(chargeCodeKey.ChargeCode, chargeCodeKey.IsDutiable, chargeCodeKey.IsVATible, false));
					}
				}
				return GetAmountInApportionedChargesCurrency(result);
			}
		}

		#endregion

		#region OverseasInsuranceIncludedInLines

		public virtual Money OverseasInsuranceIncludedInLines
		{
			get
			{
				Money result = Money.Empty;

				if (InvoiceHeader != null)
				{
					var chargeCodeKey = InvoiceHeader.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance)?.ChargeCodeChargeKey;
					if (chargeCodeKey != null)
					{
						result = GetCharge(new MessageChargeKey(chargeCodeKey.ChargeCode, chargeCodeKey.IsDutiable, chargeCodeKey.IsVATible, true));
					}
				}
				return result;
			}
		}

		#endregion

		#region OverseasInsuranceNotIncludedInLines

		public virtual Money OverseasInsuranceNotIncludedInLines
		{
			get
			{
				Money result = Money.Empty;

				if (InvoiceHeader != null)
				{
					var chargeCodeKey = InvoiceHeader.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance)?.ChargeCodeChargeKey;
					if (chargeCodeKey != null)
					{
						result = GetCharge(new MessageChargeKey(chargeCodeKey.ChargeCode, chargeCodeKey.IsDutiable, chargeCodeKey.IsVATible, false));
					}
				}
				return GetAmountInApportionedChargesCurrency(result);
			}
		}

		#endregion

		#region JI_Tariff Related Methods

		bool IsDeclarationBuiltIn
		{
			get
			{
				return Declaration != null && !Declaration.IsDeclarationIntegrated;
			}
		}

		internal IEnumerable<ZString> DistinctAdditionalUOMsFromAllValidTariffs => GetDistinctAdditionalUOMsFromAllValidTariffs(UniversalTariff, CusLineTariffDetails.Cast<CusLineTariffDetail>());

		IEnumerable<ZString> GetDistinctAdditionalUOMsFromAllValidTariffs(TariffView tariff, IEnumerable<CusLineTariffDetail> cusLineTariffDetails)
		{
			var distinctAdditionalUOMs = new List<ZString>();
			var tariff1P1StatisticalQty = tariff?.GetSpecificUOM(UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType) ?? ZString.Empty;
			var validTariffDetails = cusLineTariffDetails.Where(x => x.IsValidTariffTypeForDefaultingUOM && !x.BZ_UQ1.IsEmpty).OrderBy(x => x.BZ_Type).ToArray();

			distinctAdditionalUOMs.Add(tariff?.GetSpecificUOM(UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType) ?? ZString.Empty);
			distinctAdditionalUOMs.AddRange(validTariffDetails.Select(x => x.BZ_UQ1));
			distinctAdditionalUOMs.Add(tariff?.GetSpecificUOM(UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType) ?? ZString.Empty);

			var validTariffDetailsUniversalTariff = validTariffDetails.Select(x => x.UniversalTariff).WhereNotNull().ToArray();
			distinctAdditionalUOMs.AddRange(validTariffDetailsUniversalTariff.Select(x => x.GetSpecificUOM(UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType)));
			distinctAdditionalUOMs.AddRange(validTariffDetailsUniversalTariff.Select(x => x.GetSpecificUOM(UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType)));

			return distinctAdditionalUOMs.Where(x => !x.IsEmpty && x != tariff1P1StatisticalQty).Distinct();
		}

		internal void DefaultUOMFromTariffAndCusLineTariffDetailsIfNeeded()
		{
			if (!IsUOMDefaultingSuspended)
			{
				(JI_CustomsUnitQty, JI_CustomsSecondUnitQty, JI_CustomsThirdUnitQty) = GetDefaultCustomsQtyUnits(UniversalTariff, CusLineTariffDetails.Cast<CusLineTariffDetail>());
			}
		}

		(ZString CustomsUnitQty, ZString CustomsSecondUnitQty, ZString CustomsThirdUnitQty) GetDefaultCustomsQtyUnits(TariffView tariff, IEnumerable<CusLineTariffDetail> cusLineTariffDetails)
		{
			var customsUnitQty = tariff?.GetSpecificUOM(UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType) ?? ZString.Empty;

			var additionalUOMs = GetDistinctAdditionalUOMsFromAllValidTariffs(tariff, cusLineTariffDetails);
			var customsSecondUnitQty = additionalUOMs.FirstOrDefault();
			var customsThirdUnitQty = additionalUOMs.Count() > 1 ? additionalUOMs.ElementAt(1) : ZString.Empty;

			customsUnitQtyAutoSet = !customsUnitQty.IsEmpty;
			customsSecondUnitQtyAutoSet = !customsSecondUnitQty.IsEmpty;
			customsThirdUnitQtyAutoSet = !customsThirdUnitQty.IsEmpty;

			return (customsUnitQty, customsSecondUnitQty, customsThirdUnitQty);
		}

		public bool IsStandardTradeAgreement
		{
			get { return JI_PrimaryPreference == UniversalReferenceConstants.PrimaryPreference.Standard; }
		}

		#region DiamondProcessing
		public bool IsDiamondProcessingRequired
		{
			get
			{
				return UniversalTariff?.Attributes.Any(x => x.ZZ3_Name == UniversalReferenceConstants.TariffAttributes.Diamond) ?? false;
			}
		}

		void ClearRulesOfOriginCertificateIfApplicable()
		{
			if (IsStandardTradeAgreement && !base.JI_ROOCert.IsEmpty)
			{
				base.JI_ROOCert = ZString.Empty;
			}
		}

		void ClearDiamondProcessingFields()
		{
			JI_DiamondBeneficiaryLicense = "";
			JI_DiamondDealerLicense = "";
			JI_TemporaryExportExemption = "";
			JI_DiamondLevyValue = 0;
			JI_DiamondProducerRegistration = "";
			JI_DiamondProducerExemption = "";
			JI_ElectionsExemptionsLevy = "";
			JI_KimberleyCertificate = "";
			JI_TemporaryBuyersPermit = "";
		}
		#endregion

		public void DefaultPreference()
		{
			if (!JI_CountryOfOrigin.IsEmpty && !JI_Tariff.IsEmpty && Declaration.IsImport)
			{
				PreferenceListHelper.DefaultPreference(Lookups.ROOTypeOrPreferenceList, JI_PrimaryPreferenceInfo);
			}
			else if (JI_CountryOfOrigin == Core.Constants.CountryCodes.SouthAfrica)
			{
				JI_PrimaryPreference = GetEffectiveValueToReturn(base.JI_PrimaryPreference, JobComInvoiceHeader.Schema.JZ_ROOType);
			}
		}

		public virtual Money Discount
		{
			get
			{
				Money result = GetCharge(new MessageChargeKey(CustomsChargeTypeList.Codes.Discount, false, false, false));
				return result;
			}
		}

		public virtual ZDecimal DiscountInInvoiceCurrency
		{
			get
			{
				return CurrencyConverter.ConvertRounded(Discount, LinePriceRefCurrency).Amount;
			}
		}

		public virtual Money DiscountNotIncludedInLines => GetCharge(new MessageChargeKey(CustomsChargeTypeList.Codes.Discount, false, false, false));

		public virtual ZString DutyFormulaDescription
		{
			get
			{
				var result = ZString.Empty;

				var cusTariff = UniversalTariff;
				var cusEntryLine = CusEntryLine;
				if (cusTariff != null && cusEntryLine != null)
				{
					var selectedRate = cusTariff.GetApplicableRate(AllApplicableRatesSelectionCriteria);

					if (selectedRate != null)
					{
						result = selectedRate.RateFormulaDescription;
					}
				}
				return result;
			}
		}

		#endregion

		#region IsCustomsApprovedExporterRequired

		public ZBool IsCustomsApprovedExporterRequired
		{
			get
			{
				var declaration = Declaration;
				return declaration != null
					&& declaration.IsExport
					&& !declaration.SupplierCustomsApprovedExporterCode.IsEmpty
					&& Lookups.CusApprovedExporterList.ContainsCode(JI_PrimaryPreference);
			}
		}

		#endregion

		#region JI_ROOCert

		[ReadOnlyMember(nameof(JI_ROOCert_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.ZA.Business.JobComInvoiceLine|UZ_ROOCert", Caption = "Rules Of Origin Certificate", ShortCaption = "ROO Certificate")]
		public override ZString JI_ROOCert
		{
			get
			{
				var result = base.JI_ROOCert;
				if (!IsExport)
				{
					var invoiceHeaderROOType = InvoiceHeader?.JZ_ROOType ?? ZString.Empty;
					if (!IsExport && !IsStandardTradeAgreement || invoiceHeaderROOType == JI_PrimaryPreference)
					{
						result = GetEffectiveValueToReturn(base.JI_ROOCert, JobComInvoiceHeader.Schema.JZ_ROOCert);
					}
					else if (IsStandardTradeAgreement)
					{
						result = ZString.Empty;
					}
				}
				return result;
			}
			set
			{
				if (!IsExport)
				{
					value = GetEffectiveValueToSet(value, JobComInvoiceHeader.Schema.JZ_ROOCert);
				}

				base.JI_ROOCert = value;
			}
		}

		bool JI_ROOCert_ReadOnly
		{
			get { return IsStandardTradeAgreement; }
		}

		#endregion

		void SetDefaultTaxOrFeeFromTariff()
		{
			JI_ZZF_NKTaxType = UniversalTariff?.ZZ1_ZZF_NKTaxOrFeeCode ?? ZString.Empty;
		}

		#endregion

		void ClearInvoiceLineValuesIfSame(IZType invoiceValue, string invoiceLineFieldName)
		{
			IZType invoiceLineValue = (IZType)base[invoiceLineFieldName];

			if (invoiceLineValue.Equals(invoiceValue))
			{
				using (SuspendEffectiveValue(invoiceLineFieldName, invoiceValue))
				{
					base[invoiceLineFieldName] = invoiceLineValue.Default;
				}

				ZPropertyInfo infoToRefresh = base.ZPropertyInfoHash[invoiceLineFieldName];
				if (infoToRefresh != null)
				{
					infoToRefresh.RefreshBinding();
				}
			}
		}

		#region Fetch Hints

		protected override ZArchitecture.Business.EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobComInvoiceLineFetchStrategy(this);
		}

		class JobComInvoiceLineFetchStrategy : Customs.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
		{
			public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine)
				: base(invoiceLine)
			{
			}

			protected override void FetchForLoadCore()
			{
				Factory.AddFetchHint(CusLineTariffDetailSchema.BZ_ParentID, BusinessObject.PK);
				base.FetchForLoadCore();
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				if (invoiceLine.SupportInvoiceLineRefs)
				{
					Factory.AddFetchHint(JobComInvLineRefsSchema.JG_JI, invoiceLine.PK);
				}
			}

			JobComInvoiceLine invoiceLine
			{
				get { return BusinessObject as JobComInvoiceLine; }
			}
		}

		#endregion

		#region Suspend CusLineTariffDetail Defaulting

		internal bool IsCusLineTariffDetailDefaultingSuspended
		{
			get { return cusLineTariffDetailDefaultingSuspenderIndex > 0; }
		}

		internal IDisposable SuspendCusLineTariffDetailDefaulting()
		{
			return new CusLineTariffDetailDefaultingSuspender(this);
		}

		int cusLineTariffDetailDefaultingSuspenderIndex;

		class CusLineTariffDetailDefaultingSuspender : IDisposable
		{
			public CusLineTariffDetailDefaultingSuspender(JobComInvoiceLine invoiceLine)
			{
				this.invoiceLine = invoiceLine;
				this.invoiceLine.cusLineTariffDetailDefaultingSuspenderIndex++;
			}

			readonly JobComInvoiceLine invoiceLine;

			#region IDisposable Members

			public void Dispose()
			{
				invoiceLine.cusLineTariffDetailDefaultingSuspenderIndex--;
			}

			#endregion
		}
		#endregion

		#region Suspend Automatic Split By Bond Amount

		internal bool IsAutomaticSplitByBondAmountCEISetterSuspended
		{
			get { return automaticSplitByBondAmountCEISetterSuspenderIndex > 0; }
		}

		internal IDisposable SuspendAutomaticSplitByBondAmountCEISetter()
		{
			return new AutomaticSplitByBondAmountCEISetterSuspender(this);
		}

		int automaticSplitByBondAmountCEISetterSuspenderIndex;

		class AutomaticSplitByBondAmountCEISetterSuspender : IDisposable
		{
			public AutomaticSplitByBondAmountCEISetterSuspender(JobComInvoiceLine invoiceLine)
			{
				this.invoiceLine = invoiceLine;
				this.invoiceLine.automaticSplitByBondAmountCEISetterSuspenderIndex++;
			}

			readonly JobComInvoiceLine invoiceLine;

			#region IDisposable Members

			public void Dispose()
			{
				invoiceLine.automaticSplitByBondAmountCEISetterSuspenderIndex--;
			}

			#endregion
		}
		#endregion

		#region Implementation

		protected override Customs.Business.ProcedureRegimeDecider GetNewProcedureRegimeDecider() => new ProcedureRegimeDecider();
		void DefaultOwnerProductDataIfNeeded()
		{
			var partNo = JI_PartNo;
			if (JI_NewOwnerPartNo.IsEmpty && !partNo.IsEmpty)
			{
				var owner = EntryInstruction?.Owner;
				if (owner != null)
				{
					var loadResults = JobComInvoiceLinePartSynchronisationManager.LoadResults(Factory, TypeOfPartUsed, partNo, owner, null, IsDrawback, IsForExportSectionOfDrawback, IsForImportSectionOfDrawback, InvoiceHeader.IsExport);
					var newOwnerPart = (OrgSupplierPart)loadResults.BestMatchingProduct;
					if (newOwnerPart != null)
					{
						JI_NewOwnerPartNo = partNo;
					}
					if (!JI_NewOwnerPartNo.IsEmpty)
					{
						var importerMiscServ = Importer?.MiscServ;
						var ownerPartAttributeDetails = GetOwnerPartAttributeDetails(owner.MiscServ).ToArray();
						if (importerMiscServ != null && ownerPartAttributeDetails.Length > 0)
						{
							DefaultOwnerPartAttribIfNeeded(importerMiscServ.OM_IMPartAttrib1NameMultilingual, importerMiscServ.OM_IMPartAttrib1Type, JI_PartAttrib1, ownerPartAttributeDetails);
						}
					}
				}
			}
		}

		IEnumerable<OwnerPartAttributeDetail> GetOwnerPartAttributeDetails(OrgMiscServ miscServ)
		{
			if (miscServ != null)
			{
				var detail = GetOwnerPartAttributeDetail(miscServ, 1);
				if (detail != null)
				{
					yield return detail;
				}
				detail = GetOwnerPartAttributeDetail(miscServ, 2);
				if (detail != null)
				{
					yield return detail;
				}
				detail = GetOwnerPartAttributeDetail(miscServ, 3);
				if (detail != null)
				{
					yield return detail;
				}
			}
		}

		OwnerPartAttributeDetail GetOwnerPartAttributeDetail(OrgMiscServ orgMiscServ, int attributeNo)
		{
			OwnerPartAttributeDetail result = null;

			if (attributeNo == 1)
			{
				result = OwnerPartAttributeDetail.New(orgMiscServ.OM_IMPartAttrib1NameMultilingual, orgMiscServ.OM_IMPartAttrib1Type, attributeNo, JI_NewOwnerPartAttrib1Info);
			}
			else if (attributeNo == 2)
			{
				result = OwnerPartAttributeDetail.New(orgMiscServ.OM_IMPartAttrib2NameMultilingual, orgMiscServ.OM_IMPartAttrib2Type, attributeNo, JI_NewOwnerPartAttrib2Info);
			}
			else if (attributeNo == 3)
			{
				result = OwnerPartAttributeDetail.New(orgMiscServ.OM_IMPartAttrib3NameMultilingual, orgMiscServ.OM_IMPartAttrib3Type, attributeNo, JI_NewOwnerPartAttrib3Info);
			}

			return result;
		}

		class OwnerPartAttributeDetail
		{
			OwnerPartAttributeDetail() { }

			public static OwnerPartAttributeDetail New(ZString name, ZString type, int position, ZPropertyInfo info)
			{
				return !name.IsEmpty && !type.IsEmpty ? new OwnerPartAttributeDetail() { Name = name, Type = type, Info = info, Position = position } : null;
			}

			public ZString Name { get; private set; }
			public ZString Type { get; private set; }
			public ZPropertyInfo Info { get; private set; }

			public int Position { get; private set; }
		}

		void DefaultOwnerPartAttribIfNotEmpty(OrgHeader org, string partAttribNameField, string partAttribTypeField, ZString partAttribValue)
		{
			if (!partAttribValue.IsEmpty)
			{
				var miscServ = org?.MiscServ;
				if (miscServ != null)
				{
					var partAttribName = (ZString)(miscServ.ZPropertyInfoHash.GetPropertySafe(partAttribNameField)?.Value ?? ZString.Empty);
					var partAttribType = (ZString)(miscServ.ZPropertyInfoHash.GetPropertySafe(partAttribTypeField)?.Value ?? ZString.Empty);
					if (!partAttribName.IsEmpty && !partAttribType.IsEmpty)
					{
						var ownerPartAttributeDetails = GetOwnerPartAttributeDetails(EntryInstruction?.Owner?.MiscServ).ToArray();
						var ownerPartAttributeDetail = ownerPartAttributeDetails.FirstOrDefault(x => x.Name.EqualsIgnoringCase(partAttribName) && x.Type.EqualsIgnoringCase(partAttribType));
						if (ownerPartAttributeDetail != null)
						{
							SetOwnerPartAttributeIfPossible(ownerPartAttributeDetail.Info, ownerPartAttributeDetail.Position, partAttribValue);
						}
					}
				}
			}
		}

		void DefaultOwnerPartAttribIfNeeded(ZString partAttribName, ZString partAttribType, ZString partAttribValue, OwnerPartAttributeDetail[] ownerPartAttributeDetails)
		{
			if (!partAttribName.IsEmpty && !partAttribType.IsEmpty)
			{
				var ownerPartAttributeDetail = ownerPartAttributeDetails.FirstOrDefault(x => x.Name.EqualsIgnoringCase(partAttribName) && x.Type.EqualsIgnoringCase(partAttribType));
				if (ownerPartAttributeDetail != null)
				{
					SetOwnerPartAttributeIfPossible(ownerPartAttributeDetail.Info, ownerPartAttributeDetail.Position, partAttribValue);
				}
			}
		}

		void SetOwnerPartAttributeIfPossible(ZPropertyInfo info, int position, ZString partAttribValue)
		{
			var partRelation = NewOwnerProduct?.RelatedOrganisations?.FindByOrganisationAndRelationship(EntryInstruction?.Owner, OrgPartRelation.RelationshipTypes.Owner);
			if (partRelation == null || UsePartAttrib(partRelation, position))
			{
				info.Value = partAttribValue;
			}
		}

		bool UsePartAttrib(OrgPartRelation partRelation, int position)
		{
			var info = partRelation.ZPropertyInfoHash.GetPropertySafe(OrgPartRelation.Schema.OU_UsePartAttrib1.Substring(0, OrgPartRelation.Schema.OU_UsePartAttrib1.Length - 1) + position) as ZPropertyInfoBool;
			return info != null && info.Value;
		}

		internal bool IsCopyingInternal => IsCopying;

		protected override void InitialisePartSyncManager()
		{
			base.InitialisePartSyncManager();
			if (newOwnerProductSyncManager == null)
			{
				newOwnerProductSyncManager = new JobComInvoiceLinePartSynchronisationManager(new InvoiceLineNewOwnerPartDetails(this));
			}
		}

		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);
		protected override ZString CustomsCountryCodeCore
		{
			get { return Core.Constants.CountryCodes.SouthAfrica; }
		}

		internal MessageDataProviderKeyFactor MessageKeyFactor => Factory.GetValue(ref messageKeyFactorCached, () =>
					{
						return CusEntryLine.CreateMessageKeyFactor(Declaration, this);
					});

		CachedProperty<MessageDataProviderKeyFactor> messageKeyFactorCached;

		void ClearAndDefaultCusLineTariffDetails(RefCusProcedure oldProcedure)
		{
			ClearAndDefaultCusLineTariffDetails(oldProcedure, CusProcedure);
		}

		internal void ClearAndDefaultCusLineTariffDetails(RefCusProcedure oldProcedure, RefCusProcedure newProcedure)
		{
			if (!IsCusLineTariffDetailDefaultingSuspended)
			{
				var oldKey = oldProcedure == null ? ZString.Empty : ZString.Format("{0}_{1}", oldProcedure.ZZ6_Category, oldProcedure.ZZ6_Concession);
				var newKey = newProcedure == null ? ZString.Empty : ZString.Format("{0}_{1}", newProcedure.ZZ6_Category, newProcedure.ZZ6_Concession);
				if (oldKey != newKey)
				{
					using (SuspendCusLineTariffDetailDefaulting())
					{
						CusLineTariffDetails.RemoveAndDeleteAll();
						refundRebateCode = ZString.Empty;
						refundRebateValue = ZString.Empty;

						DefaultCusLineTariffDetails(newProcedure, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, JI_Tariff, EffectiveAssessmentDate, "1", JI_NewUsed);
						if (IsExport && newProcedure != null && !newProcedure.ZZ6_Concession.IsEmpty)
						{
							if (!CusLineTariffDetails.Cast<CusLineTariffDetail>().Any(x => x.BZ_Type.StartsWith(newProcedure.ZZ6_Concession, StringComparison.Ordinal)))
							{
								DefaultCusLineTariffDetailsForExports(newProcedure);
							}
						}
					}
				}
			}
		}

		internal void DefaultCusLineTariffDetailsForExports(RefCusProcedure procedure)
		{
			var concessionCode =
				procedure?.Concessions?.FirstNonSpecificTariffType(procedure.Factory)
					.Left(RefCusTariffType.Schema.ZZI_TariffTypeMaxLength);

			var query = new ZQuery(RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica);
			query.AddToFilter(RefCusTariffTypeSchema.ZZI_TariffType, SQLComparisonOperator.StartsWith, concessionCode);
			query.OrderBy = RefCusTariffTypeSchema.Constants.ZZI_TariffType;

			RefCusTariffType tariffDetail = Factory.LoadTop1<RefCusTariffType>(query);
			if (tariffDetail != null)
			{
				CusLineTariffDetails.AddNew(tariffDetail.ZZI_TariffType, "");
			}
		}

		internal void DefaultCusLineTariffDetails(RefCusProcedure procedure, ZString tariffType, ZString tariffCode, ZDateTime assessmentDate, ZString onlyAddTariffTypeGreaterThan, ZString newUsed)
		{
			var newAddedTariffDetailsThatNeedToBeProcessed = new List<CusLineTariffDetail>(DefaultCusLineTariffDetailsCore(procedure, tariffType, tariffCode, assessmentDate, onlyAddTariffTypeGreaterThan, newUsed));
			while (newAddedTariffDetailsThatNeedToBeProcessed.Count > 0)
			{
				var tariffDetail = newAddedTariffDetailsThatNeedToBeProcessed[0];
				newAddedTariffDetailsThatNeedToBeProcessed.Remove(tariffDetail);
				var type = tariffDetail.BZ_Type;
				var tariff = tariffDetail.BZ_Tariff;
				if (Factory.GetCusTariff(tariffType, tariffCode, assessmentDate) != null)
				{
					newAddedTariffDetailsThatNeedToBeProcessed.AddRange(DefaultCusLineTariffDetailsCore(procedure, type, tariff, assessmentDate, onlyAddTariffTypeGreaterThan, newUsed));
				}
			}
		}

		IEnumerable<CusLineTariffDetail> DefaultCusLineTariffDetailsCore(RefCusProcedure procedure, ZString tariffType, ZString tariffCode, ZDateTime assessmentDate, ZString onlyAddTariffTypeGreaterThan, ZString newUsed)
		{
			var result = new List<CusLineTariffDetail>();
			if (procedure != null && !tariffCode.IsEmpty)
			{
				foreach (var pair in procedure.GetValidRefCusTariffSortedDictionary(tariffType, tariffCode, assessmentDate).Where(x => x.Key.ZZI_TariffType > onlyAddTariffTypeGreaterThan && CanAddType(x.Key, procedure, newUsed)))
				{
					var tariffDetail = CusLineTariffDetails.AddNew();
					using (tariffDetail.SuspendCusLineTariffDetailDefaulting())
					{
						tariffDetail.BZ_Type = pair.Key.ZZI_TariffType;
						if (pair.Value.Count == 1)
						{
							tariffDetail.BZ_Tariff = pair.Value[0].ZZ1_TariffCode.Left(tariffDetail.BZ_TariffInfo.MaxLength);
							result.Add(tariffDetail);
						}
					}
				}
			}
			return result;
		}

		bool CanAddType(RefCusTariffType cusTariffType, RefCusProcedure procedure, ZString newUsed)
		{
			var result = cusTariffType.IsPayableDutyExcludingAntiDumping();
			if (!result)
			{
				var tariffTypePrefix = cusTariffType.ZZI_TariffType.Left(1);
				result = !CusLineTariffDetails.Cast<CusLineTariffDetail>().Any(x => x.BZ_Type.Left(1) == tariffTypePrefix);
			}

			if (result && cusTariffType.ZZI_TariffType == UniversalReferenceConstants.CusTariffCode.Schedule1Part3D)
			{
				result = (newUsed.IsEmpty || newUsed == GoodsTypeList.Codes.N);
			}

			if (result && cusTariffType.ZZI_TariffType == UniversalReferenceConstants.CusTariffCode.Schedule1Part8)
			{
				result = procedure.ZZ6_ProcedureCode == UniversalReferenceConstants.ProcedureCodes._10 && procedure.ZZ6_PreviousProcedureCode == UniversalReferenceConstants.ProcedureCodes._00;
			}

			return result;
		}

		#region Suspend Effective Value

		internal IDisposable SuspendEffectiveValue(string fieldName, IZType invoiceValue)
		{
			EffectiveValueSuspender holder;
			if (!EffectiveValueSuspenders.TryGetValue(fieldName, out holder))
			{
				holder = new EffectiveValueSuspender(this, fieldName) { InvoiceValue = invoiceValue };
				EffectiveValueSuspenders.Add(fieldName, holder);
			}
			return holder;
		}

		Dictionary<string, EffectiveValueSuspender> EffectiveValueSuspenders
		{
			get { return effectiveValueSuspenders ?? (effectiveValueSuspenders = new Dictionary<string, EffectiveValueSuspender>()); }
		}
		Dictionary<string, EffectiveValueSuspender> effectiveValueSuspenders;

		class EffectiveValueSuspender : IDisposable
		{
			public EffectiveValueSuspender(JobComInvoiceLine invoiceLine, string fieldName)
			{
				this.invoiceLine = invoiceLine;
				this.fieldName = fieldName;
			}

			public IZType InvoiceValue { get; set; }

			readonly JobComInvoiceLine invoiceLine;
			readonly string fieldName;

			#region IDisposable Members

			public void Dispose()
			{
				invoiceLine.EffectiveValueSuspenders.Remove(fieldName);
			}

			#endregion
		}
		#endregion

		T GetEffectiveValueToReturn<T>(T baseValue, string fieldNameInJobComInvoiceHeader) where T : IZType
		{
			T result = baseValue;

			if (result.IsEmpty && !string.IsNullOrEmpty(fieldNameInJobComInvoiceHeader))
			{
				var invoice = InvoiceHeader;
				if (invoice != null)
				{
					result = (T)invoice[fieldNameInJobComInvoiceHeader];
				}
			}

			return result;
		}

		T GetEffectiveValueToReturnFromEntryInstruction<T>(T baseValue, string fieldNameInEntryInstruction) where T : IZType
		{
			T result = baseValue;

			if (result.IsEmpty && !string.IsNullOrEmpty(fieldNameInEntryInstruction))
			{
				var instruction = EntryInstruction;
				if (instruction != null)
				{
					result = (T)instruction[fieldNameInEntryInstruction];
				}
			}

			return result;
		}

		T GetEffectiveValueToSet<T>(T valuePassed, string fieldNameInJobComInvoiceHeader) where T : IZType
		{
			T result = valuePassed;

			if (!string.IsNullOrEmpty(fieldNameInJobComInvoiceHeader))
			{
				var invoice = InvoiceHeader;

				if (invoice != null && invoice[fieldNameInJobComInvoiceHeader].Equals(valuePassed))
				{
					result = (T)valuePassed.Default;
				}
			}

			return result;
		}

		T GetEffectiveValueToSetFromEntryInstruction<T>(T valuePassed, string fieldNameInEntryInstruction) where T : IZType
		{
			T result = valuePassed;

			if (!string.IsNullOrEmpty(fieldNameInEntryInstruction))
			{
				var instruction = EntryInstruction;
				if (instruction != null && instruction[fieldNameInEntryInstruction].Equals(valuePassed))
				{
					result = (T)valuePassed.Default;
				}
			}

			return result;
		}

		#region Decaulting From InvoiceHeader

		internal void UpdateINTChargeIfNeeded()
		{
			if (IsImport)
			{
				var tariffNeedsIntellectualValue = UniversalTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributes.IntellectualValue) ?? false;
				var containsINTCharges = Charges.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue);
				if (tariffNeedsIntellectualValue && !containsINTCharges)
				{
					Charges.AddNew(InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue);
				}
				else if (!tariffNeedsIntellectualValue && containsINTCharges)
				{
					var chargeCodeChargeKey = IncoTermAndChargeFactory.GetCharge(InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue)?.ChargeCodeChargeKey;
					if (chargeCodeChargeKey != null)
					{
						Charges.ClearCharge(chargeCodeChargeKey);
					}
				}
			}
		}

		void UpdateValuationMarkupIfNeeded()
		{
			if (IsImport)
			{
				JI_ValuationMarkup = InvoiceHeader?.JZ_ValuationMarkup ?? ZDecimal.Zero;
			}
		}

		internal void UpdateValuationMarkupIfNeeded(ZDecimal oldValue)
		{
			if (IsImport && (JI_ValuationMarkup == oldValue || JI_ValuationMarkup.IsEmpty))
			{
				JI_ValuationMarkup = InvoiceHeader?.JZ_ValuationMarkup ?? ZDecimal.Zero;
			}
		}

		#endregion

		#endregion

		#region IUltimateDistributee Members

		ZDecimal IUltimateDistributee.DutyPercent
		{
			// TODO: ToBeChecked: Logic Changed with the removal of AdditionalDuties in AddInfo, Need re-implementation #Victor 20160322
			get { return CusEntryLine != null ? CusEntryLine.CL_DutyPercent : ZDecimal.Zero; }
		}

		DutyTaxEntryFee IUltimateDistributee.LineDutyTaxEntryFeeItems => Factory.GetValue(ref lineDutyTaxEntryFeeItemsCached, GetLineDutyTaxEntryFeeItems);

		CachedProperty<DutyTaxEntryFee> lineDutyTaxEntryFeeItemsCached;

		// TODO: Will be replaced with GenericLandedCostingConfig
		DutyTaxEntryFee GetLineDutyTaxEntryFeeItems()
		{
			var result = new DutyTaxEntryFee();
			var entryLine = CusEntryLine;
			if (entryLine != null)
			{
				result[CustomsDisbursementChargeCode.TotalDuty] = GetAmountApportionedFromCusEntryLine(entryLine.GetDutyAmountForLandedCosting()).Amount;
			}
			return result;
		}

		#endregion

		#region IUniversalRateCalcData Members

		DateTime IUniversalRateCalcData.DateOfValuation
		{
			get { return EffectiveAssessmentDate.ToDateTime(); }
		}

		decimal IUniversalRateCalcData.ValueForDuty
		{
			get { return JI_CustomsValue; }
		}

		decimal IUniversalRateCalcData.CustomsValue
		{
			get { return JI_CustomsValue; }
		}

		IDictionary<string, decimal> IUniversalRateCalcData.UnitOfMeasureValueList
		{
			get
			{
				var unitOfMeasureValueList = new Dictionary<string, decimal>();
				UpdateDictionaryOrAddNew(unitOfMeasureValueList, JI_CustomsUnitQty, JI_CustomsQuantity);
				UpdateDictionaryOrAddNew(unitOfMeasureValueList, JI_CustomsSecondUnitQty, JI_CustomsSecondQuantity);
				UpdateDictionaryOrAddNew(unitOfMeasureValueList, JI_CustomsThirdUnitQty, JI_CustomsThirdQuantity);
				return unitOfMeasureValueList.ToDictionary(x => x.Key, x => ZArchitecture.Core.Utilities.Round(x.Value, 2));
			}
		}

		void UpdateDictionaryOrAddNew(IDictionary<string, decimal> result, ZString key, ZDecimal value)
		{
			if (!key.IsEmpty)
			{
				if (!result.ContainsKey(key))
				{
					result.Add(key, value);
				}
				else
				{
					result[key] += value;
				}
			}
		}

		IDictionary<string, decimal> IUniversalRateCalcData.CountrySpecificValueList { get; } = new Dictionary<string, decimal>();

		public IList<Tuple<string, string>> AdditionalInformationList { get; } = new List<Tuple<string, string>>();

		public IDictionary<string, string> MeursingExpressionList { get; } = new Dictionary<string, string>();

		#endregion

		#region IChangeOfOwnershipLineDetails

		Customs.Business.OrgSupplierPart IChangeOfOwnershipLineDetails.OwnerPart => NewOwnerProduct;
		ZPropertyInfo IChangeOfOwnershipLineDetails.NewOwnerProductCodeInfo => JI_NewOwnerPartNoInfo;
		ZPropertyInfo IChangeOfOwnershipLineDetails.NewOwnerPartAttribute1Info => JI_NewOwnerPartAttrib1Info;
		ZPropertyInfo IChangeOfOwnershipLineDetails.NewOwnerPartAttribute2Info => JI_NewOwnerPartAttrib2Info;
		ZPropertyInfo IChangeOfOwnershipLineDetails.NewOwnerPartAttribute3Info => JI_NewOwnerPartAttrib3Info;
		ZPropertyInfo IChangeOfOwnershipLineDetails.NewOwnerSerialNumberInfo => JI_NewOwnerSerialNumInfo;

		#endregion IChangeOfOwnershipLineDetails

		#region IAdditionalLineTariffDetailParent

		ICusLineTariffDetailCollection<Customs.Business.CusLineTariffDetail> IAdditionalLineTariffDetailParent.CusLineTariffDetails => CusLineTariffDetails;

		#endregion

		#region IDA63ValueRecalculationParent

		public ZBool DA63NeedsRecalculation
		{
			get { return IsDA63 && fDA63NeedsRecalculation; }
			set
			{
				fDA63NeedsRecalculation = value;
				if (DA63NeedsRecalculation && DA63NeedsRecalculationValueChanged != null)
				{
					DA63NeedsRecalculationValueChanged.Invoke(null, null);
				}
			}
		}

		ZBool fDA63NeedsRecalculation;

		public EventHandler DA63NeedsRecalculationValueChanged;

		#endregion

		#region ICusCodeDataTypeSupporter

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.DA63AdditionalDuty, typeof(DA63AdditionalDuty) }
			};
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region RateSelectionCriteria

		public IZZRateSelectionCriteria GetSpecificRateSelectionCriteria(ZString rateCode) => new RateSelectionCriteria(this, ZString.Empty, rateCode);

		public IZZRateSelectionCriteria AdValoremExciseRateSelectionCriteria => Factory.GetValue(ref adValoremExciseRateSelectionCriteria, GetAdValoremExciseRateSelectionCriteriaCore);

		CachedProperty<IZZRateSelectionCriteria> adValoremExciseRateSelectionCriteria;

		protected IZZRateSelectionCriteria GetAdValoremExciseRateSelectionCriteriaCore() => new RateSelectionCriteria(this, Constants.RateTypes.AdValoremExcise, ZString.Empty);

		public IZZRateSelectionCriteria AntiDumpingRateSelectionCriteria => Factory.GetValue(ref antiDumpingRateSelectionCriteria, GetAntiDumpingRateSelectionCriteriaCore);

		CachedProperty<IZZRateSelectionCriteria> antiDumpingRateSelectionCriteria;

		protected IZZRateSelectionCriteria GetAntiDumpingRateSelectionCriteriaCore() => new RateSelectionCriteria(this, Constants.RateTypes.AntiDumping, ZString.Empty);

		public IZZRateSelectionCriteria ExciseRateSelectionCriteria => Factory.GetValue(ref exciseRateSelectionCriteria, GetExciseRateSelectionCriteriaCore);

		CachedProperty<IZZRateSelectionCriteria> exciseRateSelectionCriteria;

		protected IZZRateSelectionCriteria GetExciseRateSelectionCriteriaCore() => new RateSelectionCriteria(this, Constants.RateTypes.Excise, ZString.Empty);

		public IZZRateSelectionCriteria RebateRateSelectionCriteria => Factory.GetValue(ref rebateRateSelectionCriteria, GetRebateRateSelectionCriteriaCore);

		CachedProperty<IZZRateSelectionCriteria> rebateRateSelectionCriteria;

		protected IZZRateSelectionCriteria GetRebateRateSelectionCriteriaCore() => new RateSelectionCriteria(this, Constants.RateTypes.Rebate, ZString.Empty);

		protected override IZZRateSelectionCriteria GetDutyRateSelectionCriteriaCore() => new RateSelectionCriteria(this, Constants.RateTypes.Duty, ZString.Empty);

		protected override IZZRateSelectionCriteria GetAllApplicableRatesSelectionCriteriaCore() => new RateSelectionCriteria(this, ZString.Empty, ZString.Empty);

		public class RateSelectionCriteria : RateSelectionCriteria<JobComInvoiceLine>
		{
			public RateSelectionCriteria(JobComInvoiceLine invoiceLine, ZString rateType, ZString rateCode)
				: base(invoiceLine, rateType, rateCode)
			{
			}

			protected override ZString GetConcessionOrder(JobComInvoiceLine invoiceLine) => ZString.Empty;
		}

		#endregion

		void IAddInfoWithSyncProperty.EnableSynchronization() { }

		IZType IAddInfoWithSyncProperty.GetAddInfoValue(IZType data, Type addInfoValueType)
			=> (IZType)Activator.CreateInstance(addInfoValueType, data);

		ZPropertyInfo IAddInfoWithSyncProperty.AddInfoProperty => JI_AddInfoInfo;

		#region IInvoiceLineInformation

		ZShort IInvoiceLineInformation.InvoiceLineNumber => JI_LineNo;

		ZShort IInvoiceLineInformation.RelatedDeclarationLineNumber => LinkedEntryLineNumber;

		ZString IInvoiceLineInformation.ProductCode => !JI_PartNo.IsEmpty ? JI_PartNo : "N/A";

		ZDecimal IInvoiceLineInformation.Quantity => JI_InvoiceQuantity.Round(4);

		ZString IInvoiceLineInformation.QuantityUnit => JI_InvoiceUQ;

		ZDecimal IInvoiceLineInformation.PriceDetails => JI_InvoiceQuantity != 0 ? ((ZDecimal)(JI_LinePrice / JI_InvoiceQuantity)).Round(4) : 0m;

		ZDecimal IInvoiceLineInformation.ItemAmount => JI_LinePrice;

		ZDecimal IInvoiceLineInformation.RateDetails => JI_ValuationMarkup;

		ZString IInvoiceLineInformation.BrandName => JI_BrandName;

		ZString IInvoiceLineInformation.CommercialInvoiceItemDescription => JI_Description;

		IEnumerable<IInvoiceLineChargeInformation> IInvoiceLineInformation.InvoiceLineChargeInformations
		{
			get
			{
				var result = new List<IInvoiceLineChargeInformation>();
				result.AddRange(Charges.Select(charge => new InvoiceLineChargeWrapper(charge)));
				result.AddRange(ApportionedCharges.Select(charge => new InvoiceLineChargeWrapper(charge)));
				return result;
			}
		}

		#endregion

		#region Concurrency Handling

		protected override void OnConcurrencyExceptionCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
			if (!IsDeleted)
			{
				base.OnConcurrencyExceptionCore(propertyRecords);
			}
		}

		protected override void OnConcurrencyExceptionAfterMergeCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
			if (!IsDeleted)
			{
				base.OnConcurrencyExceptionAfterMergeCore(propertyRecords);
			}
		}

		#endregion

		public static bool ZAAddInvoiceDetailsToCUSDECMessageEnabled => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);

		public static bool IsMigratedAddInfoProperties(string propertyName)
		{
			switch (propertyName)
			{
				case JobComInvoiceLine.Schema.JI_Colour:
				case JobComInvoiceLine.Schema.JI_EngineCapacity:
				case JobComInvoiceLine.Schema.JI_EngineNumber:
				case JobComInvoiceLine.Schema.JI_Make:
				case JobComInvoiceLine.Schema.JI_VehicleFormat:
				case JobComInvoiceLine.Schema.JI_VehicleType:
				case JobComInvoiceLine.Schema.JI_VIN:
				case JobComInvoiceLine.Schema.JI_YearOfManufacture:
					return true;
			}
			return false;
		}
	}
}
