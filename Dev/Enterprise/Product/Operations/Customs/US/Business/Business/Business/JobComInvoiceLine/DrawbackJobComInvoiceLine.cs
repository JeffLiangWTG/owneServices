using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public partial class JobComInvoiceLine : AutoJobComInvoiceLine
		, IDrawbackImportClaim
		, IDrawbackManufactureClaim
		, IDrawback7551ImportDocLine
		, IDrawback7551ExportDocLine
		, IACEDrawbackImportClaim
		, IACEDrawbackManufactureClaim
		, IACEDrawbackExportClaim
		, IACEDrawbackTFTEAClaim
		, IACEDrawbackTrackingNumberLine
	{
		#region Schema

		public new partial class Schema : AutoJobComInvoiceLine.Schema
		{
			public const string DRWImportQuantity = "DRWImportQuantity";
			public const string DRWImportUQ = "DRWImportUQ";
			public const string DRWExportQuantity = "DRWExportQuantity";
			public const string DRWExportUQ = "DRWExportUQ";
			public const string DeclaredVFD = "DeclaredVFD";
			public const string DutyPerUnit = "DutyPerUnit";
			public const string ExportValue = "ExportValue";
			public const string ClaimedDuty = "ClaimedDuty";
			public const string _99ClaimedDuty = "_99ClaimedDuty";
			public const string CalculatedDuty = "CalculatedDuty";
			public const string AdjClaimDuty = "AdjClaimDuty";
			public const string DeclaredTax = "DeclaredTax";
			public const string TaxPerUnit = "TaxPerUnit";
			public const string ClaimedTax = "ClaimedTax";
			public const string _99ClaimedTax = "_99ClaimedTax";
			public const string CalculatedTax = "CalculatedTax";
			public const string AdjClaimTax = "AdjClaimTax";
			public const string DeclaredMPF = "DeclaredMPF";
			public const string LineMPF = "LineMPF";
			public const string WeightedRatio = "WeightedRatio";
			public const string MPFWeightedRatio = "MPFWeightedRatio";
			public const string MPFPerUnit = "MPFPerUnit";
			public const string ClaimedMPF = "ClaimedMPF";
			public const string _99ClaimedMPF = "_99ClaimedMPF";
			public const string CalculatedMPF = "CalculatedMPF";
			public const string AdjClaimMPF = "AdjClaimMPF";
			public const string DeclaredHMF = "DeclaredHMF";
			public const string LineHMF = "LineHMF";
			public const string HMFPerUnit = "HMFPerUnit";
			public const string ClaimedHMF = "ClaimedHMF";
			public const string _99ClaimedHMF = "_99ClaimedHMF";
			public const string CalculatedHMF = "CalculatedHMF";
			public const string AdjClaimHMF = "AdjClaimHMF";
			public const string DeclaredOtherFees = "DeclaredOtherFees";
			public const string OtherFeesPerUnit = "OtherFeesPerUnit";
			public const string ClaimedOtherFees = "ClaimedOtherFees";
			public const string _99ClaimedOtherFees = "_99ClaimedOtherFees";
			public const string US_OH_DRWExporterOrDestroyer = "US_OH_DRWExporterOrDestroyer";
			public const string US_OA_DRWExporterOrDestroyer = "US_OA_DRWExporterOrDestroyer";
			public const string US_FormattedExportTariff = "US_FormattedExportTariff";
			public const string LineDuty = "LineDuty";
			public const string LineDutyRateDesc = "LineDutyRateDesc";
			public const string DRWAllowableQTY = "DRWAllowableQTY";
			public const string DRWGoodsValuePerUQ = "DRWGoodsValuePerUQ";
			public const string SubstitutedValuePerUnit = "SubstitutedValuePerUnit";
			public const string ExportTariffDescription = "ExportTariffDescription";
			public const string DRWImportQuantity2 = "DRWImportQuantity2";
			public const string DRWImportUQ2 = "DRWImportUQ2";
			public const string DRWAllowableQTY2 = "DRWAllowableQTY2";
			public const string DRWGoodsValuePerUQ2 = "DRWGoodsValuePerUQ2";
			public const string SubstitutedValuePerUnit2 = "SubstitutedValuePerUnit2";
			public const string DRWImportQuantity3 = "DRWImportQuantity3";
			public const string DRWImportUQ3 = "DRWImportUQ3";
			public const string DRWAllowableQTY3 = "DRWAllowableQTY3";
			public const string DRWGoodsValuePerUQ3 = "DRWGoodsValuePerUQ3";
			public const string SubstitutedValuePerUnit3 = "SubstitutedValuePerUnit3";
		}

		#endregion

		public bool IsDrawbackDeclaration
		{
			get { return IsDrawback; }
		}

		public bool IsACEDrawback
		{
			get { return Declaration?.IsACEDrawback ?? false; }
		}

		public bool Is7552
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.Is7552;
			}
		}

		public new IDrawbackEntryLine DrawbackImportEntryLine
		{
			get { return (IDrawbackEntryLine)base.DrawbackImportEntryLine; }
		}

		public override bool UseImportClassification
		{
			get { return IsDrawbackDeclaration ? IsForImportSectionOfDrawback && !IsForExportSectionOfDrawback : (IsExport ? !UseScheduleB : base.UseImportClassification); }
		}

		public override bool UseExportClassification
		{
			get { return IsDrawbackDeclaration ? IsForExportSectionOfDrawback : (IsExport ? UseScheduleB : base.UseExportClassification); }
		}

		public override ZBool US_DRWIsForImportSection
		{
			get { return Is7552 || base.US_DRWIsForImportSection; }
			set
			{
				if (!Is7552)
				{
					base.US_DRWIsForImportSection = value;
					DrawbackNAFTAs.SetReadOnlyIncludingChildren(!value);
				}

				SetDefaultAccountingMethod();
			}
		}

		internal void SetDefaultAccountingMethod()
		{
			if (IsACEDrawback && US_DRWIsForImportSection && US_DRWAccMethod.IsEmpty && Lookups.DrawbackAccountingCodes.ContainsCode(DrawbackAccountingMethodCodeList.Codes._00))
			{
				US_DRWAccMethod = DrawbackAccountingMethodCodeList.Codes._00;
			}
		}

		public override bool IsForImportSectionOfDrawback
		{
			get { return US_DRWIsForImportSection; }
		}

		public override bool IsForExportSectionOfDrawback
		{
			get { return US_DRWIsForExportSection; }
		}

		public bool isNotForExportSecionOfDrawbackDocument
		{
			get { return IsDrawbackDeclaration && !US_DRWIsForExportSection; }
		}

		public ZString US_DRWExportDestDescription
		{
			get
			{
				if (US_DRWExportDest.IsEmpty)
				{
					return Core.Constants.FindBoxMessages.NoneSelected;
				}
				else
				{
					var country = RefCountry.LoadFromCountryCode(Factory, US_DRWExportDest);
					if (country != null)
					{
						return country.RN_Desc;
					}
					else
					{
						switch (US_DRWExportDest)
						{
							case ForeignTradeZoneISOCode:
								return "FTZ";
							case ForeignDestinationISOCode:
								return "Petroleum Products";
							case OuterSpaceISOCode:
								return "Outer Space";
						}
					}
				}

				return Core.Constants.FindBoxMessages.InvalidSelection;
			}
		}
		internal const string ForeignTradeZoneISOCode = "FZ";
		internal const string ForeignDestinationISOCode = "FN";
		internal const string OuterSpaceISOCode = "FF";

		public bool isNotForImportSecionOfDrawbackDocument
		{
			get { return IsDrawbackDeclaration && !US_DRWIsForImportSection; }
		}

		public override ZString US_ImportEntryNo
		{
			get { return base.US_ImportEntryNo; }
			set
			{
				var oldValue = US_ImportEntryNo;
				if (oldValue != value && !IsCopying && IsDrawbackDeclaration)
				{
					var parser = new EntryLineCodeParser(value);
					var needRefresh = false;
					try
					{
						suspendDefaultDrawbackData = true;
						if (parser.IsCompleteCode)
						{
							if (oldValue != parser.EntryNumber)
							{
								base.US_ImportEntryNo = parser.EntryNumber;
								needRefresh = true;
							}

							if (US_DRWImportEntryLine != parser.LineNumber)
							{
								US_DRWImportEntryLine = parser.LineNumber;
								needRefresh = true;
							}
						}
						else
						{
							base.US_ImportEntryNo = value;
							needRefresh = true;
						}
					}
					finally
					{
						suspendDefaultDrawbackData = false;
					}

					if (needRefresh)
					{
						RefreshDrawbackImportEntryLineAndDefaultDrawbackData();
					}

					AddInfoValidation.ValidateUS_DRWCertOfManufacture();
				}
				else
				{
					base.US_ImportEntryNo = value;
				}
			}
		}

		public ZString FormattedDrawbackEntryNoOrCMDNumber
		{
			get { return US_ImportEntryNo.IsEmpty ? US_DRWCertOfManufacture : GetFormattedImportEntryNo(); }
		}

		internal ZString GetFormattedImportEntryNo()
		{
			var builder = new ZStringBuilder();
			builder.Append(US_ImportEntryNo.SubstringSafe(0, 3));
			builder.Append(US_ImportEntryNo.SubstringSafe(3, 7));
			builder.Append(US_ImportEntryNo.SubstringSafe(10, 1));
			return builder.ToStringWithDelimiterBetweenAppends("-");
		}

		internal ZString GetFormattedImportEntryNoWithLineItemNo()
		{
			var entryNo = US_ImportEntryNo;
			if (!entryNo.IsEmpty)
			{
				entryNo = GetFormattedImportEntryNo() + " / " + US_DRWImportEntryLine;
			}
			return entryNo;
		}

		public override ZString US_DRWCertOfManufacture
		{
			get { return base.US_DRWCertOfManufacture; }
			set
			{
				base.US_DRWCertOfManufacture = value;
				AddInfoValidation.ValidateUS_ImportEntryNo();
			}
		}

		public override ZInt DrawbackImportDeclarationLine
		{
			get { return US_DRWImportEntryLine; }
		}

		public override ZString DrawbackImportDeclarationNumber
		{
			get
			{
				if (drawbackImportDeclarationNumberCached == null)
				{
					drawbackImportDeclarationNumberCached = new CachedProperty<ZString>(Factory, () =>
					{
						var importEntryNo = US_ImportEntryNo;
						return importEntryNo.SubstringSafe(USAddInfoSchema.US_EntryFilerCode.MaxLength);
					});
				}
				return drawbackImportDeclarationNumberCached.Value;
			}
		}
		CachedProperty<ZString> drawbackImportDeclarationNumberCached;

		public override ZString US_ExportTariff
		{
			get { return base.US_ExportTariff; }
			set
			{
				var newTariff = TariffFormatter.Format(value).Left(US_ExportTariffInfo.MaxLength);
				base.US_ExportTariff = newTariff;
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ExportTariffs))]
		public ZString US_FormattedExportTariff
		{
			get { return TariffFormatter.DisplayFormat(US_ExportTariff); }
			set { US_ExportTariff = value; }
		}

		public ZPropertyInfo US_FormattedExportTariffInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_FormattedExportTariff, x => US_ExportTariffInfo); }
		}

		internal bool UseViewForDrawbackEntryLine
		{
			get
			{
				if (!useViewForDrawbackEntryLineCached.HasValue)
				{
					useViewForDrawbackEntryLineCached = DataRegistry.Business.USCustomsDataRegistry.Instance.UseViewForDrawbackEntryLine.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				}
				return useViewForDrawbackEntryLineCached.Value;
			}
		}
		bool? useViewForDrawbackEntryLineCached;

		protected override IBaseDrawbackEntryLine DrawbackImportEntryLineCore => UseViewForDrawbackEntryLine ? GetViewDrawbackImportEntryLineData() : GetCusEntryLineDrawbackImportEntryLineData();

		IDrawbackEntryLine GetCusEntryLineDrawbackImportEntryLineData() => CusEntryLineLoader.FindParentByDeclarationLineNumberAndEntryFilerCode(DrawbackImportDeclarationNumber, DrawbackImportDeclarationLine, DrawbackImportDeclarationEntryFilerCode);

		IDrawbackEntryLine GetViewDrawbackImportEntryLineData()
		{
			var importEntryNo = US_ImportEntryNo;
			var drawbackImportDeclarationLine = (ZShort)DrawbackImportDeclarationLine;

			DrawbackJobComInvoiceLineFetchHintHelper.AddFetchHintForUSImportEntryLineTable(Factory, importEntryNo, drawbackImportDeclarationLine);
			((IBusinessObjectFactoryInternals)Factory).RowFactory.ExecuteFetchHintsForTable(USImportEntryLineSchema.Constants.TableName);
			return DrawbackJobComInvoiceLineFetchHintHelper.GetViewDrawbackImportEntryLineData((x) => Factory.Load<USImportEntryLine>(x), importEntryNo, drawbackImportDeclarationLine);
		}

		public ZString DrawbackImportDeclarationEntryFilerCode
		{
			get
			{
				if (drawbackImportDeclarationEntryFilerCodeCached == null)
				{
					drawbackImportDeclarationEntryFilerCodeCached = new CachedProperty<ZString>(Factory, () =>
					{
						var importEntryNo = US_ImportEntryNo;
						return importEntryNo.Left(AddInfoJobDeclaration.Schema.US_EntryFilerCodeMaxLength);
					});
				}
				return drawbackImportDeclarationEntryFilerCodeCached.Value;
			}
		}
		CachedProperty<ZString> drawbackImportDeclarationEntryFilerCodeCached;

		public override ZInt US_DRWImportEntryLine
		{
			get { return base.US_DRWImportEntryLine; }
			set
			{
				if (base.US_DRWImportEntryLine != value)
				{
					base.US_DRWImportEntryLine = value;
					if (!IsCopying)
					{
						RefreshDrawbackImportEntryLineAndDefaultDrawbackData();
					}
				}
			}
		}

		public bool ShouldReCalculateDrawbackData => !IsReCalculatedDrawbackDataSuspended && !US_DRWOldData;

		bool IsReCalculatedDrawbackDataSuspended => suspendReCalculatedDrawbackDataIndex > 0;
		byte suspendReCalculatedDrawbackDataIndex;
		internal IDisposable SuspendReCalculatedDrawbackData() => new DisposableAction(() => suspendReCalculatedDrawbackDataIndex++, () => suspendReCalculatedDrawbackDataIndex--);
		public void RefreshDrawbackImportEntryLineAndDefaultDrawbackData()
		{
			RefreshDrawbackImportEntryLine();
			if (!suspendDefaultDrawbackData && !IsCopying && !DrawbackImportDeclarationNumber.IsEmpty && !DrawbackImportDeclarationLine.IsEmpty)
			{
				if (US_DRWOldData) // old data is marked in transformation; if users trigger this method then new data will be calculated.
				{
					US_DRWOldData = ZBool.False;
				}
				var drawbackImportEntryLine = DrawbackImportEntryLine;
				if (drawbackImportEntryLine != null)
				{
					using (SuspendReCalculatedDrawbackData())
					{
						shouldAddAdditionalImportTariffNumbers = true;
						DrawbackAdditionalImportTariffNumbers.RemoveAndDeleteAll();
						JI_Tariff = drawbackImportEntryLine.CL_AdValoremTariff;

						if (!drawbackImportEntryLine.CL_Description.IsEmpty && JI_Description.IsEmpty)
						{
							JI_Description = drawbackImportEntryLine.CL_Description.Left(JobComInvoiceLine.Schema.JI_DescriptionMaxLength);
						}

						US_DRWEntryDate = drawbackImportEntryLine.EntryDate;
						US_DRWPort = drawbackImportEntryLine.EntryPort.Left(AddInfo.Schema.US_DRWPortMaxLength);
						if (IsACEDrawback)
						{
							US_DRWQuarterlyHMF = drawbackImportEntryLine.IsFTZAdmission;
						}

						SetDefaultValeDRWImportQuantityAndUQ();

						if (this.IsACEDrawback)
						{
							DrawbackOtherFees.RemoveAndDeleteAll();
							var drawbackOtherFeeCodeList = Factory.GetCachedValue<DrawbackOtherFeeTypesList>();
							var feeCusCodeDataList = CusFeeCodeConstants.GetAccountingClassFeeCodeList(Factory);
							var shouldAddOtherFeeNotInListCode = false;

							foreach (var fee in drawbackImportEntryLine.Fees)
							{
								var feeTypeCode = fee.Code;
								if (feeCusCodeDataList.ContainsCode(feeTypeCode) && !CusFeeCodeConstants.OtherFeeCodesToExcludeForACEDrawback(feeTypeCode))
								{
									if (!drawbackOtherFeeCodeList.ContainsCode(feeTypeCode))
									{
										shouldAddOtherFeeNotInListCode = true;
									}
									else
									{
										DrawbackOtherFees.AddNewOrUpdate(feeTypeCode, ZDecimal.Zero);
									}
								}
							}

							if (shouldAddOtherFeeNotInListCode)
							{
								DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.OtherFee, ZDecimal.Zero);
							}
						}
					}
					Claims.DefaultOverrideData();
				}
				else
				{
					shouldAddAdditionalImportTariffNumbers = false;
				}
			}
		}
		bool suspendDefaultDrawbackData;
		bool shouldAddAdditionalImportTariffNumbers;

		void AddAdditionalImportTariffNumbers()
		{
			var drawbackImportEntryLine = DrawbackImportEntryLine;
			if (drawbackImportEntryLine != null)
			{
				var importQty = drawbackImportEntryLine.GetImportQuantityToDefault(GetImportUQToDefault());
				var parentLineOrThis = drawbackImportEntryLine.ParentLine ?? drawbackImportEntryLine;
				AddAdditionalTariffNumber(parentLineOrThis, importQty);
				parentLineOrThis.ChildSecondaryEntryLines.ForEach(l => AddAdditionalTariffNumber(l, importQty));
			}
		}

		void AddAdditionalTariffNumber(IDrawbackEntryLine entryLine, ZDecimal importQty)
		{
			var tariffNumber = entryLine.CL_AdValoremTariff.Left(DrawbackAdditionalImportTariffNumber.Schema.US_FormattedTariffMaxLength);
			if (!JI_Tariff.IsEmpty && !tariffNumber.IsEmpty && tariffNumber != JI_Tariff && !DrawbackAdditionalImportTariffNumbers.ContainsTariffNumber(tariffNumber))
			{
				var newSecondaryTariff = DrawbackAdditionalImportTariffNumbers.AddNew();
				using (newSecondaryTariff.GetValidationSuspender())
				{
					newSecondaryTariff.US_FormattedTariff = tariffNumber;

					if (IsACEDrawback)
					{
						if (!entryLine.CL_Description.IsEmpty && newSecondaryTariff.US_Description.IsEmpty)
						{
							newSecondaryTariff.US_Description = entryLine.CL_Description.Left(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_DescriptionMaxLength);
						}

						newSecondaryTariff.US_UQ1 = GetImportUQToDefault();
						newSecondaryTariff.US_UQ2 = GetImportSecondUQToDefault();
						newSecondaryTariff.US_UQ3 = GetImportThirdUQToDefault();

						if (!entryLine.US_SupLine && importQty > ZDecimal.Zero)
						{
							var customsValue = CustomsValueDeciderForInvoiceLine.GetCustomsValue(entryLine);
							newSecondaryTariff.US_ValuePerUnit1 = ((ZDecimal)(customsValue / importQty)).Round(4);
						}
					}
				}
			}
		}

		void UpdateDrawbackDetailsOnPartChange()
		{
			if (IsForExportSectionOfDrawback && IsForImportSectionOfDrawback)
			{
				OrgSupplierPart part = Part;
				if (part != null)
				{
					var importPivot = (CusClassPartPivot)part.GetUSPivots().GetImportMatch(Core.Constants.CountryCodes.UnitedStates, ZGuid.Empty, ZGuid.Empty);
					JI_Tariff = importPivot == null ? ZString.Empty : importPivot.TariffNumber;

					var partForExportClassification = part.OP_PartNum == JI_PartNo ? part : (OrgSupplierPart)part.LocalPart;
					if (partForExportClassification != null)
					{
						var exportPivot = (CusClassPartPivot)partForExportClassification.GetUSPivots().GetExportMatch(Core.Constants.CountryCodes.UnitedStates, true, ZGuid.Empty, ZGuid.Empty);
						if (exportPivot != null)
						{
							US_ExportTariff = exportPivot.TariffNumber.Left(AddInfoJobComInvoiceLine.Schema.US_ExportTariffMaxLength);
						}
					}
				}
			}
			else if (IsForExportSectionOfDrawback)
			{
				US_ExportTariff = JI_Tariff.Left(AddInfo.Schema.US_ExportTariffMaxLength);
				JI_Tariff = ZString.Empty;
			}
		}

		protected override ZString GetTariffDescription(ZString tariffCode)
		{
			var result = ZString.Empty;
			if (!IsImport)
			{
				result = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, UseScheduleB ? Constants.TariffTypes.ScheduleB : Constants.TariffTypes.Export, tariffCode, EffectiveDateForDutyRate)?.ZZ1_Description ?? ZString.Empty;
			}

			return result;
		}

		internal void AdjustPartsListPropertiesForDrawback(Customs.Business.OrgSupplierPartCollection partsList)
		{
			var declaration = Declaration;
			if (declaration != null && IsForExportSectionOfDrawback && !IsForImportSectionOfDrawback)
			{
				if (!declaration.JE_OH_Importer.IsEmpty)
				{
					if (partsList.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property1"))
					{
						partsList.FilterBusinessObjectDefaults.Remove("Importer/Supplier:Property1");
					}

					if (!partsList.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property2"))
					{
						partsList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property2", ImporterPK_Effective));
					}
				}
			}
		}

		public ZDecimal DrawbackDutyAmountOverriden
		{
			get { return drawbackDutyAmountOverriden; }
			set { drawbackDutyAmountOverriden = value; }
		}
		ZDecimal drawbackDutyAmountOverriden;

		public ZDecimal DrawbackTaxAmountOverriden
		{
			get { return drawbackTaxAmountOverriden; }
			set { drawbackTaxAmountOverriden = value; }
		}
		ZDecimal drawbackTaxAmountOverriden;

		#region JI_CustomsQuantity

		public override ZDecimal JI_CustomsQuantity
		{
			get { return base.JI_CustomsQuantity; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsQuantity))
				{
					ZDecimal oldValue = JI_CustomsQuantity;
					base.JI_CustomsQuantity = value;
					if (!IsCopying && oldValue != JI_CustomsQuantity)
					{
						ClearCustomsQuantityIfSameAsEffective(Schema.US_SupQty1, JI_CustomsQuantity, US_SupUQ1, JI_CustomsUnitQty, ImportSupTariff);

						if (!IsLineGroupingEnabled || TotalNoOfSequences == 1)
						{
							AIILine aiiLine = FirstAIILine;
							if (aiiLine != null)
							{
								aiiLine.US_CustomsQty = JI_CustomsQuantity;
								aiiLine.US_CustomsQtyInfo.RefreshBinding(oldValue);
							}
						}
					}
				}
			}
		}

		bool IsReferingToAnExistingDeclarationLine
		{
			get { return DrawbackImportEntryLine != null; }
		}

		#endregion

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public DrawbackAdditionalImportTariffNumberCollection DrawbackAdditionalImportTariffNumbers
		{
			get
			{
				if (drawbackAdditionalImportTariffNumbers == null)
				{
					drawbackAdditionalImportTariffNumbers = new DrawbackAdditionalImportTariffNumberCollection(this);
					drawbackAdditionalImportTariffNumbers.Load();
					RegisterEditableChildObject(drawbackAdditionalImportTariffNumbers);
				}
				return drawbackAdditionalImportTariffNumbers;
			}
		}
		DrawbackAdditionalImportTariffNumberCollection drawbackAdditionalImportTariffNumbers;

		internal HugeSequenceNumberGenerator AdditionalImportTariffSequenceGenerator
		{
			get { return fAdditionalImportTariffSequenceGenerator ?? (fAdditionalImportTariffSequenceGenerator = new HugeSequenceNumberGenerator(new DrawbackAdditionalTariffSequenceNumberHeader(() => new TypedEnumerable<ISequenceNumberLine>(DrawbackAdditionalImportTariffNumbers)))); }
		}
		HugeSequenceNumberGenerator fAdditionalImportTariffSequenceGenerator;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public DrawbackAdditionalExportTariffNumberCollection DrawbackAdditionalExportTariffNumbers
		{
			get
			{
				if (drawbackAdditionalExportTariffNumbers == null)
				{
					drawbackAdditionalExportTariffNumbers = new DrawbackAdditionalExportTariffNumberCollection(this);
					drawbackAdditionalExportTariffNumbers.Load();
					RegisterEditableChildObject(drawbackAdditionalExportTariffNumbers);
				}
				return drawbackAdditionalExportTariffNumbers;
			}
		}
		DrawbackAdditionalExportTariffNumberCollection drawbackAdditionalExportTariffNumbers;

		internal ShortSequenceNumberGenerator AdditionalExportTariffSequenceGenerator
		{
			get { return fAdditionalExportTariffSequenceGenerator ?? (fAdditionalExportTariffSequenceGenerator = new ShortSequenceNumberGenerator(new DrawbackAdditionalTariffSequenceNumberHeader(() => new TypedEnumerable<ISequenceNumberLine>(DrawbackAdditionalExportTariffNumbers)))); }
		}
		ShortSequenceNumberGenerator fAdditionalExportTariffSequenceGenerator;

		public ZString DrawbackImportEntry
		{
			get { return US_ImportEntryNo; }
		}

		public ZString DrawbackImportEntryPort
		{
			get { return US_DRWPort; }
		}

		public ZDate DrawbackImportEntryDate
		{
			get { return US_DRWEntryDate.Date; }
		}

		public ZString CMCDIndicator
		{
			get { return US_DRWCMCDIndicator; }
		}

		public ZDecimal DrawbackClaimDuty
		{
			get { return Is7552 ? ClaimedDuty : _99ClaimedDuty; }
		}

		public ZDecimal DrawbackClaimTax
		{
			get { return Is7552 ? ClaimedTax : _99ClaimedTax; }
		}

		public ZString CertificateOfManufactureNumber
		{
			get { return US_DRWCertOfManufacture; }
		}

		public ZString CertificateOfManufacturePort
		{
			get { return US_DRWPort; }
		}

		public ZDecimal DrawbackManufactureQuantity
		{
			get { return US_DRWExportQuantity; }
		}

		public ZString DrawbackManufactureUnitOfMeasure
		{
			get { return US_DRWExportUQ; }
		}

		public ZString DescriptionForBlock41
		{
			get { return JI_Description; }
		}

		ZString DrawbackDocDescription
		{
			get
			{
				var sb = new ZStringBuilder();
				sb.AppendIfNotEmpty(JI_Description);

				var invoiceNumber = ImpDeclInvoiceNumber;
				if (!invoiceNumber.IsEmpty)
				{
					sb.Append("Invoice No.: " + invoiceNumber);
				}

				var partNo = PartNo;
				if (!partNo.IsEmpty)
				{
					sb.Append("Part No.: " + partNo);
				}

				return sb.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public DrawbackNAFTACollection DrawbackNAFTAs
		{
			get
			{
				if (drawbackNAFTAs == null)
				{
					drawbackNAFTAs = new DrawbackNAFTACollection(this);
					drawbackNAFTAs.Load();
					RegisterEditableChildObject(drawbackNAFTAs);
					drawbackNAFTAs.SetReadOnlyIncludingChildren(!IsForImportSectionOfDrawback);
				}
				return drawbackNAFTAs;
			}
		}
		DrawbackNAFTACollection drawbackNAFTAs;

		public void SetDrawbackDefaultFromPreviousLine(JobComInvoiceLine previousLine)
		{
			US_DRWIsForImportSection = previousLine.US_DRWIsForImportSection;
			US_DRWIsForExportSection = previousLine.US_DRWIsForExportSection;
			if (US_DRWIsForImportSection)
			{
				US_ImportEntryNo = previousLine.US_ImportEntryNo;
				US_DRWCertOfManufacture = previousLine.US_DRWCertOfManufacture;
				US_DRWEntryDate = previousLine.US_DRWEntryDate;
				US_DRWPort = previousLine.US_DRWPort;
				US_DRWCMCDIndicator = previousLine.US_DRWCMCDIndicator;
				US_DRWDateRcvFrom = previousLine.US_DRWDateRcvFrom;
				US_DRWDateRcvTo = previousLine.US_DRWDateRcvTo;
				US_DRWDateUsedFrom = previousLine.US_DRWDateUsedFrom;
				US_DRWDateUsedTo = previousLine.US_DRWDateUsedTo;
			}
			if (US_DRWIsForExportSection)
			{
				US_DRWExportDate = previousLine.US_DRWExportDate;
				US_DRWExportAction = previousLine.US_DRWExportAction;
				US_DRWExportID = previousLine.US_DRWExportID;
				US_DRWExportDest = previousLine.US_DRWExportDest;
				US_UI_NKCarrierSCAC = previousLine.US_UI_NKCarrierSCAC;
			}
		}

		void CopyDrawbackDetails(JobComInvoiceLine invoiceLine)
		{
			foreach (var drawbackNAFTA in DrawbackNAFTAs)
			{
				invoiceLine.DrawbackNAFTAs.Add(drawbackNAFTA.Clone());
			}

			foreach (var additionalTariff in DrawbackAdditionalImportTariffNumbers)
			{
				invoiceLine.DrawbackAdditionalImportTariffNumbers.Add(additionalTariff.Clone());
			}
		}

		public virtual USCarrierCombined ExportingCarrier
		{
			get
			{
				USCarrierCombined result = null;
				if (!US_UI_NKCarrierSCAC.IsEmpty)
				{
					result = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, US_UI_NKCarrierSCAC));
				}
				return result;
			}
		}

		public ZString DrawbackEntryNoOrCMDNo
		{
			get { return US_ImportEntryNo.IsEmpty ? US_DRWCertOfManufacture : US_ImportEntryNo; }
		}

		public bool US_DRWQuantityUsed_ReadOnly
		{
			get { return !US_DRWIsForManufacturerSection; }
		}

		public bool US_DRWUQUsed_ReadOnly
		{
			get { return !US_DRWIsForManufacturerSection; }
		}

		public bool US_DRWDateOfManufacture_ReadOnly
		{
			get { return !US_DRWIsForManufacturerSection; }
		}

		public bool US_DRWDescrManufactured_ReadOnly
		{
			get { return !US_DRWIsForManufacturerSection; }
		}

		public bool US_DRWDescrUsed_ReadOnly
		{
			get { return !US_DRWIsForManufacturerSection; }
		}

		public bool US_DRWFactoryLocation_ReadOnly
		{
			get { return !US_DRWIsForManufacturerSection; }
		}

		public bool US_DRWManufRuleNo_ReadOnly
		{
			get { return !US_DRWIsForManufacturerSection; }
		}

		#region Overidden Properties

		#region US_DRWIsForExportSection

		public override ZBool US_DRWIsForExportSection
		{
			get { return base.US_DRWIsForExportSection; }
			set
			{
				base.US_DRWIsForExportSection = value;
				ExporterOrDestroyer.ReadOnly = !value;
				LocationOfDestruction.ReadOnly = !value;
				LocationOfMerchandise.ReadOnly = !value;
			}
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		public override ZString US_DRWUQUsed
		{
			get { return base.US_DRWUQUsed; }
			set { base.US_DRWUQUsed = value; }
		}

		public override ZBool US_DRWIsForManufacturerSection
		{
			get { return base.US_DRWIsForManufacturerSection; }
			set
			{
				base.US_DRWIsForManufacturerSection = value;

				if (!US_DRWIsForManufacturerSection)
				{
					US_DRWMafActInd = ZString.Empty;
					US_DRWQuantityUsed = ZDecimal.Zero;
					US_DRWUQUsed = ZString.Empty;
					US_DRWDateOfManufacture = ZDateTime.Empty;
					US_DRWDescrManufactured = ZString.Empty;
					US_DRWDescrUsed = ZString.Empty;
					US_DRWFactoryLocation = ZString.Empty;
					US_DRWMafTrkID = ZString.Empty;
					US_DRWManufRuleNo = ZString.Empty;
				}
			}
		}

		public override ZDecimal US_DRWSubstituted
		{
			get => base.US_DRWSubstituted;
			set
			{
				var oldValue = base.US_DRWSubstituted;
				base.US_DRWSubstituted = value;
				if (oldValue != value && US_DRWOldData)
				{
					US_DRWOldData = ZBool.False;
				}
				if (ShouldReCalculateDrawbackData)
				{
					Claims.DefaultOverrideData();
				}
				DrawbackOtherFees.RefreshBinding();
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|US_DRWClaimAmountOverriden_New", Caption = "Override")]
		public override ZBool US_DRWClaimAmountOverriden_New
		{
			get { return base.US_DRWClaimAmountOverriden_New; }
			set
			{
				var oldValue = US_DRWClaimAmountOverriden_New;
				base.US_DRWClaimAmountOverriden_New = value;
				var newValue = US_DRWClaimAmountOverriden_New;
				if (!IsCopying && oldValue != US_DRWClaimAmountOverriden_New)
				{
					RefreshUS_DRWAdValoremRate();
					if (newValue && US_DRWOldData)
					{
						US_DRWOldData = ZBool.False;
					}
					if (ShouldReCalculateDrawbackData)
					{
						Claims.DefaultOverrideData();
					}
					DrawbackOtherFees.RefreshBinding();
				}
			}
		}

		public void RefreshUS_DRWAdValoremRate()
		{
			if (IsDrawback)
			{
				var data = IsAdvaloremTariffForDrawbackClaimOverriden;
				RefreshUS_DRWAdValoremRateAndDutyRateDesc(data.value, data.importTariff);
			}
		}

		void RefreshUS_DRWAdValoremRateAndDutyRateDesc(bool isAdvaloremTariffForDrawbackClaimOverriden, USCTariff importTariff)
		{
			if (isAdvaloremTariffForDrawbackClaimOverriden)
			{
				if (!US_DRWCalcDutyWithAdValoremRate)
				{
					US_DRWCalcDutyWithAdValoremRate = true;
				}

				var rateAndDescriptionFromTariff = GetRateAndDescriptionFromTariff(importTariff);
				var adValoremRate = rateAndDescriptionFromTariff.AdValoremRate;
				var dutyRateDesc = rateAndDescriptionFromTariff.RateDescription;
				foreach (var additionalImportTariffNumber in DrawbackAdditionalImportTariffNumbers.Cast<DrawbackAdditionalImportTariffNumber>())
				{
					rateAndDescriptionFromTariff = GetRateAndDescriptionFromTariff(additionalImportTariffNumber.Tariff);
					var adValoremRateFromTariff = rateAndDescriptionFromTariff.AdValoremRate;
					var rateDescriptionFromTariff = rateAndDescriptionFromTariff.RateDescription;
					adValoremRate += adValoremRateFromTariff;
					dutyRateDesc = adValoremRateFromTariff > 0 ? (ZString)(dutyRateDesc + "+" + rateDescriptionFromTariff) : dutyRateDesc;
				}

				if (US_DRWAdValoremRate != adValoremRate)
				{
					US_DRWAdValoremRate = adValoremRate;
				}

				if (US_DRWLineDutyRateDesc != dutyRateDesc)
				{
					US_DRWLineDutyRateDesc = dutyRateDesc;
				}
			}
		}

		(ZDecimal AdValoremRate, ZString RateDescription) GetRateAndDescriptionFromTariff(USCTariff tariff)
		{
			var adValoremRate = ZDecimal.Zero;
			var rateDescription = ZString.Empty;

			if (tariff != null && tariff.UE_DutyComputationCode == ComputationCodeList.Codes.AdValorem)
			{
				adValoremRate = tariff.UE_Column1RateAdValorem * 100;
				rateDescription = adValoremRate.ToStringTrimZeros() + "%";
			}

			return (adValoremRate, rateDescription);
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|US_DRWAdValoremRate", Caption = "Ad-valorem Rate %")]
		public override ZDecimal US_DRWAdValoremRate
		{
			get { return base.US_DRWAdValoremRate; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWAdValoremRate))
				{
					var oldValue = US_DRWAdValoremRate;
					base.US_DRWAdValoremRate = value;
					if (!IsCopying && ShouldReCalculateDrawbackData && oldValue != US_DRWAdValoremRate)
					{
						Claims.DutyClaim.DefaultDutyRateDesc();
						Claims.DutyClaim.Default_99ClaimedDutyAndCalculatedAmount();
					}
				}
			}
		}

		[DecimalPlaces(4)]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|US_DRWQuantityUsed", Caption = "Quantity and UQ Used")]
		public override ZDecimal US_DRWQuantityUsed
		{
			get { return base.US_DRWQuantityUsed; }
			set { base.US_DRWQuantityUsed = value.Truncate(4); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|US_DRWImpTrkID", Caption = "Tracking ID", FullDescription = "Import Tracking Identification Number")]
		[ReadOnly(true)]
		public override ZString US_DRWImpTrkID
		{
			get { return base.US_DRWImpTrkID; }
			set { base.US_DRWImpTrkID = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|US_DRWMafTrkID", Caption = "Manufactured Tracking ID", ShortCaption = "Tracking ID", FullDescription = "Manufactured Tracking Identification Number")]
		[ReadOnly(true)]
		public override ZString US_DRWMafTrkID
		{
			get { return base.US_DRWMafTrkID; }
			set { base.US_DRWMafTrkID = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|US_DRWAccMethod", Caption = "Accounting Method", FullDescription = "Drawback Accounting Method Code")]
		[ReadOnlyMember(nameof(isNotForImportSecionOfDrawbackDocument))]
		public override ZString US_DRWAccMethod
		{
			get { return base.US_DRWAccMethod; }
			set { base.US_DRWAccMethod = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|US_DRWMafActInd", Caption = "Action Code")]
		public override ZString US_DRWMafActInd
		{
			get { return base.US_DRWMafActInd; }
			set { base.US_DRWMafActInd = value; }
		}

		public bool US_DRWMafActInd_ReadOnly
		{
			get { return !US_DRWIsForManufacturerSection; }
		}

		#endregion

		#region New Properties

		public ZString ExportTariffDescription
		{
			get { return GetTariffDescription(US_ExportTariff); }
		}

		public ZPropertyInfo ExportTariffDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ExportTariffDescription); }
		}

		#endregion

		#region Drawback Claims

		public DrawbackClaims Claims
		{
			get { return fClaims ?? (fClaims = new DrawbackClaims(this)); }
		}
		DrawbackClaims fClaims;

		internal ZBool IsAutoCalculated
		{
			get { return !US_DRWClaimAmountOverriden_New; }
		}

		public bool NoDrawbackAmounts
		{
			get
			{
				if (noClaimAmounts == null)
				{
					noClaimAmounts = new CachedProperty<bool>(Factory, () =>
					{
						return CalculatedDuty == 0 && CalculatedHMF == 0 && CalculatedMPF == 0 && CalculatedTax == 0 &&
							AdjClaimDuty == 0 && AdjClaimHMF == 0 && AdjClaimMPF == 0 && AdjClaimTax == 0 &&
							(DrawbackOtherFees.Count == 0 || DrawbackOtherFees.OfType<DrawbackOtherFee>().All(x => x.US_CalculatedAmount == 0 && x.US_AdjClaimAmount == 0));
					});
				}
				return noClaimAmounts.Value;
			}
		}
		CachedProperty<bool> noClaimAmounts;

		public override ZDecimal US_DRWWeightedRatio
		{
			get => base.US_DRWWeightedRatio;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWWeightedRatio))
				{
					var shouldReCalculateDrawbackData = ShouldReCalculateDrawbackData;
					var mpfWeightedRatio = shouldReCalculateDrawbackData ? MPFWeightedRatio : ZDecimal.Zero;
					var oldValue = US_DRWWeightedRatio;
					base.US_DRWWeightedRatio = value;
					if (!IsCopying && shouldReCalculateDrawbackData && oldValue != US_DRWWeightedRatio)
					{
						Claims.HMFClaim.DefaultOverrideData();
						if (mpfWeightedRatio != MPFWeightedRatio && Claims.MPFClaim.IsClaimable)
						{
							Claims.MPFClaim.DefaultOverrideData();
						}
					}
				}
			}
		}

		public override ZDecimal US_DRWMPFWeightedRatio
		{
			get => base.US_DRWMPFWeightedRatio;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWMPFWeightedRatio))
				{
					var oldValue = MPFWeightedRatio;
					base.US_DRWMPFWeightedRatio = value;
					if (!IsCopying && ShouldReCalculateDrawbackData && oldValue != MPFWeightedRatio)
					{
						Claims.MPFClaim.DefaultOverrideData();
					}
				}
			}
		}

		#region Weighted Ratio

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[DecimalPlaces(6)]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|WeightedRatio", Caption = "HMF Weighted Ratio")]
		public ZDecimal WeightedRatio
		{
			get { return Claims.HMFClaim.WeightedRatio; }
			set { Claims.HMFClaim.WeightedRatio = value; }
		}

		public ZPropertyInfo WeightedRatioInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.WeightedRatio, x => US_DRWWeightedRatioInfo); }
		}

		#endregion

		#region Claims Proxy Properties

		public override ZDecimal US_DRWLineDuty
		{
			get => base.US_DRWLineDuty;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWLineDuty))
				{
					var shouldReCalculateData = ShouldReCalculateDrawbackData;
					var oldValue = shouldReCalculateData ? LineDuty : ZDecimal.Zero;
					base.US_DRWLineDuty = value;
					if (!IsCopying && shouldReCalculateData && oldValue != US_DRWLineDuty)
					{
						Claims.DutyClaim.Default_99ClaimedDutyAndCalculatedAmount();
					}
				}
			}
		}

		public override ZDecimal US_DRWDeclaredTax
		{
			get => base.US_DRWDeclaredTax;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWDeclaredTax))
				{
					var oldValue = US_DRWDeclaredTax;
					base.US_DRWDeclaredTax = value;
					if (!IsCopying && ShouldReCalculateDrawbackData && oldValue != US_DRWDeclaredTax)
					{
						Claims.IRTaxClaim.Default_99ClaimedDutyAndCalculatedAmount();
					}
				}
			}
		}

		public override ZDecimal US_DRWDeclaredVFD
		{
			get => base.US_DRWDeclaredVFD;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWDeclaredVFD))
				{
					var oldValue = US_DRWDeclaredVFD;
					base.US_DRWDeclaredVFD = value;
					if (!IsCopying && ShouldReCalculateDrawbackData && oldValue != US_DRWDeclaredVFD)
					{
						Claims.DefaultGoodsValuePerUnit();
						Claims.DefaultGoodsValuePerUnit2();
						Claims.DefaultGoodsValuePerUnit3();
						Claims.DutyClaim.Default_99ClaimedDutyAndCalculatedAmount();
					}
				}
			}
		}

		public override ZDecimal US_DRWDeclaredHMF
		{
			get => base.US_DRWDeclaredHMF;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWDeclaredHMF))
				{
					var oldValue = US_DRWDeclaredHMF;
					base.US_DRWDeclaredHMF = value;
					if (!IsCopying && ShouldReCalculateDrawbackData && oldValue != US_DRWDeclaredHMF)
					{
						Claims.HMFClaim.Default_99ClaimedDutyAndCalculatedAmount();
					}
				}
			}
		}

		public override ZDecimal US_DRWDeclaredMPF
		{
			get => base.US_DRWDeclaredMPF;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWDeclaredMPF))
				{
					var oldValue = US_DRWDeclaredMPF;
					base.US_DRWDeclaredMPF = value;
					if (!IsCopying && ShouldReCalculateDrawbackData && oldValue != US_DRWDeclaredMPF)
					{
						Claims.MPFClaim.Default_99ClaimedDutyAndCalculatedAmount();
					}
				}
			}
		}

		public override ZDecimal US_DRWImportQuantity
		{
			get => base.US_DRWImportQuantity;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWImportQuantity))
				{
					var oldValue = US_DRWImportQuantity;
					base.US_DRWImportQuantity = value.Truncate(4);
					if (!IsCopying && ShouldReCalculateDrawbackData && oldValue != US_DRWImportQuantity)
					{
						Claims.DefaultGoodsValuePerUnit();
						Claims.Default_99ClaimedDutyAndCalculatedAmount();
						DrawbackOtherFees.Cast<DrawbackOtherFee>().ForEach(x => x.Claims.Default_99ClaimedDutyAndCalculatedAmount());
					}
				}
			}
		}

		public override ZString US_DRWImportUQ
		{
			get => base.US_DRWImportUQ;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWImportUQ))
				{
					base.US_DRWImportUQ = value;
				}
			}
		}

		public override ZDecimal US_DRWExportQuantity
		{
			get => base.US_DRWExportQuantity;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWExportQuantity))
				{
					var oldValue = US_DRWExportQuantity;
					base.US_DRWExportQuantity = value.Truncate(4);
					if (!IsCopying && ShouldReCalculateDrawbackData && oldValue != US_DRWExportQuantity)
					{
						Claims.Default_99ClaimedDutyAndCalculatedAmount();
						DrawbackOtherFees.Cast<DrawbackOtherFee>().ForEach(x => x.Claims.Default_99ClaimedDutyAndCalculatedAmount());
					}
				}
			}
		}

		public override ZString US_DRWExportUQ
		{
			get => base.US_DRWExportUQ;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWExportUQ))
				{
					base.US_DRWExportUQ = value;
				}
			}
		}

		public override ZDecimal US_DRWImportQuantity2
		{
			get => base.US_DRWImportQuantity2;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWImportQuantity2))
				{
					var oldValue = US_DRWImportQuantity2;
					base.US_DRWImportQuantity2 = value.Truncate(4);
					if (!IsCopying && ShouldReCalculateDrawbackData && oldValue != US_DRWImportQuantity2)
					{
						Claims.DefaultGoodsValuePerUnit2();
					}
				}
			}
		}

		public override ZString US_DRWImportUQ2
		{
			get => base.US_DRWImportUQ2;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWImportUQ2))
				{
					base.US_DRWImportUQ2 = value;
				}
			}
		}

		public override ZDecimal US_DRWImportQuantity3
		{
			get => base.US_DRWImportQuantity3;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWImportQuantity3))
				{
					var oldValue = US_DRWImportQuantity3;
					base.US_DRWImportQuantity3 = value.Truncate(4);
					if (!IsCopying && ShouldReCalculateDrawbackData && oldValue != US_DRWImportQuantity3)
					{
						Claims.DefaultGoodsValuePerUnit3();
					}
				}
			}
		}

		public override ZString US_DRWImportUQ3
		{
			get => base.US_DRWImportUQ3;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWImportUQ3))
				{
					base.US_DRWImportUQ3 = value;
				}
			}
		}

		public override ZDecimal US_DRWValuePerUQ
		{
			get => base.US_DRWValuePerUQ;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWValuePerUQ))
				{
					base.US_DRWValuePerUQ = value;
				}
			}
		}

		public override ZDecimal US_DRWValuePerUQ2
		{
			get => base.US_DRWValuePerUQ2;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWValuePerUQ2))
				{
					base.US_DRWValuePerUQ2 = value;
				}
			}
		}

		public override ZDecimal US_DRWValuePerUQ3
		{
			get => base.US_DRWValuePerUQ3;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWValuePerUQ3))
				{
					base.US_DRWValuePerUQ3 = value;
				}
			}
		}

		#region DRW Import Quantity

		[DecimalPlaces(4)]
		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DRWImportQuantity", Caption = "Import Quantity")]
		public ZDecimal DRWImportQuantity
		{
			get { return Claims.ImportQuantity; }
			set
			{
				var oldValue = DRWImportQuantity;
				Claims.ImportQuantity = value;
				if (!IsCopying && value.IsEmpty && oldValue != value)
				{
					DRWAllowableQTY = ZDecimal.Zero;
					DRWGoodsValuePerUQ = ZDecimal.Zero;
					SubstitutedValuePerUnit = ZDecimal.Zero;
				}
			}
		}

		void SetDefaultValeDRWImportQuantityAndUQ()
		{
			DRWImportUQ = GetImportUQToDefault();
			DRWExportUQ = DRWImportUQ;
			DRWImportQuantity = GetImportQuantityToDefault();

			if (IsACEDrawback)
			{
				DRWImportUQ2 = GetImportSecondUQToDefault();
				DRWImportQuantity2 = GetImportSecondQuantityToDefault();
				DRWImportUQ3 = GetImportThirdUQToDefault();
				DRWImportQuantity3 = GetImportThirdQuantityToDefault();
			}
		}

		ZString GetImportUQToDefault() => Factory.GetValue(ref importUQToDefaultCached, () => DrawbackImportEntryLine?.GetImportUQToDefault() ?? ZString.Empty);
		CachedProperty<ZString> importUQToDefaultCached;

		ZDecimal GetImportQuantityToDefault() => Factory.GetValue(ref importQuantityToDefaultCached, () => DrawbackImportEntryLine?.GetImportQuantityToDefault(US_DRWQuantityUsed > 0 ? US_DRWUQUsed : US_DRWExportUQ) ?? ZDecimal.Zero);
		CachedProperty<ZDecimal> importQuantityToDefaultCached;

		ZString GetImportSecondUQToDefault() => Factory.GetValue(ref importSecondUQToDefaultCached, () => DrawbackImportEntryLine?.GetImportSecondUQToDefault() ?? ZString.Empty);
		CachedProperty<ZString> importSecondUQToDefaultCached;

		ZDecimal GetImportSecondQuantityToDefault() => Factory.GetValue(ref importSecondQuantityToDefaultCached, () => DrawbackImportEntryLine?.GetImportSecondQuantityToDefault() ?? ZDecimal.Zero);
		CachedProperty<ZDecimal> importSecondQuantityToDefaultCached;

		ZString GetImportThirdUQToDefault() => Factory.GetValue(ref importThirdUQToDefaultCached, () => DrawbackImportEntryLine?.GetImportThirdUQToDefault() ?? ZString.Empty);
		CachedProperty<ZString> importThirdUQToDefaultCached;

		ZDecimal GetImportThirdQuantityToDefault() => Factory.GetValue(ref importThirdQuantityToDefaultCached, () => DrawbackImportEntryLine?.GetImportThirdQuantityToDefault() ?? ZDecimal.Zero);
		CachedProperty<ZDecimal> importThirdQuantityToDefaultCached;

		public ZPropertyInfo DRWImportQuantityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWImportQuantity, x => US_DRWImportQuantityInfo); }
		}

		#endregion

		#region DRW Import UQ

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUQList))]
		[BusinessObjectTestExclude]
		[MaxLength(ImportUQMaxLength)]
		public ZString DRWImportUQ
		{
			get { return Claims.ImportUQ; }
			set
			{
				Claims.ImportUQ = value;
				if (!defaultingDRWImportAndExportUQInProgress && DRWExportUQ != value)
				{
					try
					{
						defaultingDRWImportAndExportUQInProgress = true;
						DRWExportUQ = value;
					}
					finally
					{
						defaultingDRWImportAndExportUQInProgress = false;
					}
				}
			}
		}

		public ZPropertyInfo DRWImportUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWImportUQ, x => US_DRWImportUQInfo); }
		}

		const int ImportUQMaxLength = 3;

		#endregion

		#region DRW Allowable Quantity

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DRWAllowableQTY", Caption = "Allowable Quantity")]
		public ZDecimal DRWAllowableQTY
		{
			get { return Claims.AllowableQTY; }
			set
			{
				Claims.AllowableQTY = value;
			}
		}

		public ZPropertyInfo DRWAllowableQTYInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWAllowableQTY, x => US_DRWAllowQtyInfo); }
		}

		public bool DRWAllowableQTY_ReadOnly
		{
			get { return IsAutoCalculated || DRWImportQuantity.IsEmpty; }
		}

		#endregion

		#region DRW Goods Value Per Unit

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DRWGoodsValuePerUQ", Caption = "Goods Value per Unit", FullDescription = "Entered (Goods) Value per Unit")]
		public ZDecimal DRWGoodsValuePerUQ
		{
			get { return Claims.GoodsValuePerUnit; }
			set
			{
				Claims.GoodsValuePerUnit = value;
			}
		}

		public ZPropertyInfo DRWGoodsValuePerUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWGoodsValuePerUQ, x => US_DRWValuePerUQInfo); }
		}

		public bool DRWGoodsValuePerUQ_ReadOnly
		{
			get { return IsAutoCalculated || DRWImportQuantity.IsEmpty; }
		}

		public bool IsValuePerUQNotMatch
		{
			get
			{
				if (isValuePerUQNotMatchCached == null)
				{
					isValuePerUQNotMatchCached = new CachedProperty<bool>(Factory, () =>
					{
						ZDecimal sumDRWGoodsValuePerUQ = DRWGoodsValuePerUQ + (IncludeAdditionalImportTariffValuePerUnit ? DrawbackAdditionalImportTariffNumbers.Cast<DrawbackAdditionalImportTariffNumber>().Sum(x => x.US_ValuePerUnit1) : decimal.Zero);
						return Claims.DutyClaim.ValuePerUnitIncludingSecondaryLines.Truncate(4) != sumDRWGoodsValuePerUQ.Truncate(4);
					});
				}

				return isValuePerUQNotMatchCached.Value;
			}
		}
		CachedProperty<bool> isValuePerUQNotMatchCached;

		bool IncludeAdditionalImportTariffValuePerUnit => JI_Tariff.StartsWith("91", StringComparison.OrdinalIgnoreCase); //Is watch goods

		public string ValuePerUQNotMatchNotification
		{
			get
			{
				if (valuePerUQNotMatchNotificationCached == null)
				{
					valuePerUQNotMatchNotificationCached = new CachedProperty<string>(Factory, () =>
					{
						var format = "0.#####";
						var valuesToSum = new List<ZDecimal>();
						var goodsValuePerUQ = DRWGoodsValuePerUQ;
						if (!goodsValuePerUQ.IsEmpty)
						{
							valuesToSum.Add(goodsValuePerUQ);
						}

						if (IncludeAdditionalImportTariffValuePerUnit)
						{
							valuesToSum.AddRange(DrawbackAdditionalImportTariffNumbers
								.Cast<DrawbackAdditionalImportTariffNumber>()
								.Select(x => x.US_ValuePerUnit1).Where(x => !x.IsEmpty));
						}

						var isACE = IsACE;
						var strBuilder1 = new ZStringBuilder();
						if (valuesToSum.Count == 0)
						{
							strBuilder1.Append("0");
						}
						else if (valuesToSum.Count == 1)
						{
							strBuilder1.Append(valuesToSum[0].ToString(format, CultureInfo.InvariantCulture));
						}
						else
						{
							var total = ZDecimal.Zero;
							foreach (var value in valuesToSum)
							{
								strBuilder1.Append(value.ToString(format, CultureInfo.InvariantCulture));
								total += value;
							}

							strBuilder1 = new ZStringBuilder(strBuilder1.ToStringWithDelimiterBetweenAppends("+"));
							strBuilder1.Append("=");
							strBuilder1.Append((isACE ? total.Round(4) : total.Round(5).Round(4)).ToString(format, CultureInfo.InvariantCulture));
						}

						var strBuilder2 = new ZStringBuilder();
						if (DRWImportQuantity.IsEmpty)
						{
							strBuilder2.Append("0");
						}
						else
						{
							strBuilder2.Append(DeclaredVFD.ToString(format, CultureInfo.InvariantCulture));
							strBuilder2.Append("/");
							strBuilder2.Append(DRWImportQuantity.ToString(format, CultureInfo.InvariantCulture));
							strBuilder2.Append("=");
							strBuilder2.Append(Claims.DutyClaim.ValuePerUnitIncludingSecondaryLines.Truncate(4).ToString(format, CultureInfo.InvariantCulture));
						}
						var result = string.Format(CultureInfo.InvariantCulture, "Value Per Unit({0}) doesn't match Entered Value/Import Quantity({1}).", strBuilder1.ToString(), strBuilder2.ToString());
						return result;
					});
				}

				return valuePerUQNotMatchNotificationCached.Value;
			}
		}
		CachedProperty<string> valuePerUQNotMatchNotificationCached;

		#endregion

		#region DRW Substituted Value Per Unit

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|SubstitutedValuePerUnit", Caption = "Substituted per Unit", FullDescription = "Substituted Value per Unit")]
		public ZDecimal SubstitutedValuePerUnit
		{
			get { return Claims.SubstitutedValuePerUnit; }
			set { Claims.SubstitutedValuePerUnit = value; }
		}

		public ZPropertyInfo SubstitutedValuePerUnitInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SubstitutedValuePerUnit, x => US_DRWSubstitutedInfo); }
		}

		public bool SubstitutedValuePerUnit_ReadOnly
		{
			get { return IsAutoCalculated || DRWImportQuantity.IsEmpty; }
		}

		#endregion

		#region DRW Import Quantity 2

		[DecimalPlaces(4)]
		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DRWImportQuantity2", Caption = "Import Quantity")]
		public ZDecimal DRWImportQuantity2
		{
			get { return Claims.ImportQuantity2; }
			set
			{
				var oldValue = DRWImportQuantity2;
				Claims.ImportQuantity2 = value;
				if (oldValue != value && !IsCopying)
				{
					if (value.IsEmpty)
					{
						DRWAllowableQTY2 = ZDecimal.Zero;
						DRWGoodsValuePerUQ2 = ZDecimal.Zero;
						SubstitutedValuePerUnit2 = ZDecimal.Zero;
					}
				}
			}
		}

		public ZPropertyInfo DRWImportQuantity2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWImportQuantity2, x => US_DRWImportQuantity2Info); }
		}

		#endregion

		#region DRW Import UQ 2

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUQList))]
		[BusinessObjectTestExclude]
		[MaxLength(ImportUQMaxLength)]
		public ZString DRWImportUQ2
		{
			get { return Claims.ImportUQ2; }
			set
			{
				Claims.ImportUQ2 = value;
			}
		}

		public ZPropertyInfo DRWImportUQ2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWImportUQ2, x => US_DRWImportUQ2Info); }
		}

		#endregion

		#region DRW Allowable Quantity 2

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DRWAllowableQTY2", Caption = "Allowable Quantity")]
		public ZDecimal DRWAllowableQTY2
		{
			get { return Claims.AllowableQTY2; }
			set
			{
				Claims.AllowableQTY2 = value;
			}
		}

		public ZPropertyInfo DRWAllowableQTY2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWAllowableQTY2, x => US_DRWAllowQty2Info); }
		}

		public bool DRWAllowableQTY2_ReadOnly
		{
			get { return IsAutoCalculated || DRWImportQuantity2.IsEmpty; }
		}

		#endregion

		#region DRW Goods Value Per Unit 2

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DRWGoodsValuePerUQ2", Caption = "Goods Value per Unit", FullDescription = "Entered (Goods) Value per Unit")]
		public ZDecimal DRWGoodsValuePerUQ2
		{
			get { return Claims.GoodsValuePerUnit2; }
			set
			{
				Claims.GoodsValuePerUnit2 = value;
			}
		}

		public ZPropertyInfo DRWGoodsValuePerUQ2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWGoodsValuePerUQ2, x => US_DRWValuePerUQ2Info); }
		}

		public bool DRWGoodsValuePerUQ2_ReadOnly
		{
			get { return IsAutoCalculated || DRWImportQuantity2.IsEmpty; }
		}

		#endregion

		#region DRW Substituted Value Per Unit 2

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|SubstitutedValuePerUnit2", Caption = "Substituted per Unit", FullDescription = "Substituted Value per Unit")]
		public ZDecimal SubstitutedValuePerUnit2
		{
			get { return Claims.SubstitutedValuePerUnit2; }
			set
			{
				Claims.SubstitutedValuePerUnit2 = value;
			}
		}

		public ZPropertyInfo SubstitutedValuePerUnit2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.SubstitutedValuePerUnit2, x => US_DRWSubstituted2Info); }
		}

		public bool SubstitutedValuePerUnit2_ReadOnly
		{
			get { return IsAutoCalculated || DRWImportQuantity2.IsEmpty; }
		}

		#endregion

		#region DRW Import Quantity 3

		[DecimalPlaces(4)]
		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DRWImportQuantity3", Caption = "Import Quantity")]
		public ZDecimal DRWImportQuantity3
		{
			get { return Claims.ImportQuantity3; }
			set
			{
				var oldValue = DRWImportQuantity3;
				Claims.ImportQuantity3 = value;
				if (oldValue != value && !IsCopying)
				{
					if (value.IsEmpty)
					{
						DRWAllowableQTY3 = ZDecimal.Zero;
						DRWGoodsValuePerUQ3 = ZDecimal.Zero;
						SubstitutedValuePerUnit3 = ZDecimal.Zero;
					}
				}
			}
		}

		public ZPropertyInfo DRWImportQuantity3Info
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWImportQuantity3, x => US_DRWImportQuantity3Info); }
		}

		#endregion

		#region DRW Import UQ 3

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUQList))]
		[BusinessObjectTestExclude]
		[MaxLength(ImportUQMaxLength)]
		public ZString DRWImportUQ3
		{
			get { return Claims.ImportUQ3; }
			set
			{
				Claims.ImportUQ3 = value;
			}
		}

		public ZPropertyInfo DRWImportUQ3Info
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWImportUQ3, x => US_DRWImportUQ3Info); }
		}

		#endregion

		#region DRW Allowable Quantity 3

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DRWAllowableQTY3", Caption = "Allowable Quantity")]
		public ZDecimal DRWAllowableQTY3
		{
			get { return Claims.AllowableQTY3; }
			set
			{
				Claims.AllowableQTY3 = value;
			}
		}

		public ZPropertyInfo DRWAllowableQTY3Info
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWAllowableQTY3, x => US_DRWAllowQty3Info); }
		}

		public bool DRWAllowableQTY3_ReadOnly
		{
			get { return IsAutoCalculated || DRWImportQuantity3.IsEmpty; }
		}

		#endregion

		#region DRW Goods Value Per Unit 3

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DRWGoodsValuePerUQ3", Caption = "Goods Value per Unit", FullDescription = "Entered (Goods) Value per Unit")]
		public ZDecimal DRWGoodsValuePerUQ3
		{
			get { return Claims.GoodsValuePerUnit3; }
			set
			{
				Claims.GoodsValuePerUnit3 = value;
			}
		}

		public ZPropertyInfo DRWGoodsValuePerUQ3Info
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWGoodsValuePerUQ3, x => US_DRWValuePerUQ3Info); }
		}

		public bool DRWGoodsValuePerUQ3_ReadOnly
		{
			get { return IsAutoCalculated || DRWImportQuantity3.IsEmpty; }
		}

		#endregion

		#region DRW Substituted Value Per Unit 3

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|SubstitutedValuePerUnit3", Caption = "Substituted per Unit", FullDescription = "Substituted Value per Unit")]
		public ZDecimal SubstitutedValuePerUnit3
		{
			get { return Claims.SubstitutedValuePerUnit3; }
			set
			{
				Claims.SubstitutedValuePerUnit3 = value;
			}
		}

		public ZPropertyInfo SubstitutedValuePerUnit3Info
		{
			get { return GetWrappedZPropertyInfo(Schema.SubstitutedValuePerUnit3, x => US_DRWSubstituted3Info); }
		}

		public bool SubstitutedValuePerUnit3_ReadOnly
		{
			get { return IsAutoCalculated || DRWImportQuantity3.IsEmpty; }
		}

		#endregion

		#region DRW Export Quantity

		[DecimalPlaces(4)]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DRWExportQuantity", Caption = "Export Quantity")]
		public ZDecimal DRWExportQuantity
		{
			get { return Claims.ExportQuantity; }
			set
			{
				Claims.ExportQuantity = value;
			}
		}

		public ZPropertyInfo DRWExportQuantityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWExportQuantity, x => US_DRWExportQuantityInfo); }
		}

		#endregion

		#region DRW Export UQ

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUQList))]
		[BusinessObjectTestExclude]
		[MaxLength(ImportUQMaxLength)]
		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public ZString DRWExportUQ
		{
			get { return Claims.ExportUQ; }
			set
			{
				Claims.ExportUQ = value;
				if (!defaultingDRWImportAndExportUQInProgress && DRWImportUQ != value)
				{
					try
					{
						defaultingDRWImportAndExportUQInProgress = true;
						DRWImportUQ = value;
					}
					finally
					{
						defaultingDRWImportAndExportUQInProgress = false;
					}
				}
			}
		}

		public ZPropertyInfo DRWExportUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DRWExportUQ, x => US_DRWExportUQInfo); }
		}

		#endregion

		bool defaultingDRWImportAndExportUQInProgress;

		#region Duty Claim

		#region Declared Duty

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DeclaredVFD", Caption = "Entered Value")]
		public ZDecimal DeclaredVFD
		{
			get { return Claims.DutyClaim.DeclaredAmount; }
			set
			{
				var oldValue = DeclaredVFD;
				Claims.DutyClaim.DeclaredAmount = value;
				DeclaredVFDInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DeclaredVFDInfo
		{
			get { return GetZPropertyInfo(Schema.DeclaredVFD); }
		}

		#endregion

		#region Duty Per Unit

		public ZDecimal DutyPerUnit
		{
			get { return Claims.DutyClaim.PerUnit; }
		}

		public ZPropertyInfo DutyPerUnitInfo
		{
			get { return GetZPropertyInfo(Schema.DutyPerUnit); }
		}

		#endregion

		#region Export Value

		public ZDecimal ExportValue
		{
			get { return Claims.DutyClaim.ExportValue; }
		}

		public ZPropertyInfo ExportValueInfo
		{
			get { return GetZPropertyInfo(Schema.ExportValue); }
		}

		#endregion

		#region Line Duty

		[ReadOnlyMember(nameof(IsLineDutyReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|LineDuty", Caption = "Line Duty")]
		public ZDecimal LineDuty
		{
			get { return Claims.DutyClaim.LineDuty; }
			set
			{
				var oldValue = LineDuty;
				Claims.DutyClaim.LineDuty = value;
				LineDutyInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo LineDutyInfo
		{
			get { return GetZPropertyInfo(Schema.LineDuty); }
		}

		bool IsLineDutyReadOnly
		{
			get { return IsAutoCalculated || US_DRWCalcDutyWithAdValoremRate; }
		}

		public override ZString US_DRWLineDutyRateDesc
		{
			get => base.US_DRWLineDutyRateDesc;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWLineDutyRateDesc))
				{
					base.US_DRWLineDutyRateDesc = value;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|US_DRWCalcDutyWithAdValoremRate", Caption = "Calculate Duty using Ad-valorem Rate")]
		public override ZBool US_DRWCalcDutyWithAdValoremRate
		{
			get { return base.US_DRWCalcDutyWithAdValoremRate; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_DRWCalcDutyWithAdValoremRate))
				{
					var oldValue = US_DRWCalcDutyWithAdValoremRate;
					base.US_DRWCalcDutyWithAdValoremRate = value;
					var newValue = US_DRWCalcDutyWithAdValoremRate;
					if (!IsCopying && oldValue != newValue)
					{
						if (!newValue && !US_DRWAdValoremRate.IsEmpty)
						{
							US_DRWAdValoremRate = ZDecimal.Zero;
						}
						else if (ShouldReCalculateDrawbackData)
						{
							Claims.DutyClaim.DefaultDutyRateDesc();
							Claims.DutyClaim.Default_99ClaimedDutyAndCalculatedAmount();
						}
					}
				}
			}
		}
		#endregion

		#region Line Duty Rate Description

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[BusinessObjectTestExclude]
		[MaxLength(30)]
		public ZString LineDutyRateDesc
		{
			get { return Claims.DutyClaim.LineDutyRateDesc; }
			set
			{
				var oldValue = LineDutyRateDesc;
				Claims.DutyClaim.LineDutyRateDesc = value;
				LineDutyRateDescInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo LineDutyRateDescInfo
		{
			get { return GetZPropertyInfo(Schema.LineDutyRateDesc); }
		}

		#endregion

		#region Duty Rate

		internal ZDecimal DutyRate
		{
			get { return Claims.DutyClaim.DutyRate; }
		}

		#endregion

		#region Claimed Duty

		public ZDecimal ClaimedDuty
		{
			get { return Claims.DutyClaim.ClaimedDuty; }
		}

		public ZPropertyInfo ClaimedDutyInfo
		{
			get { return GetZPropertyInfo(Schema.ClaimedDuty); }
		}

		#endregion

		#region 99% Duty

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public ZDecimal _99ClaimedDuty
		{
			get { return Claims.DutyClaim._99ClaimedDuty; }
			set { Claims.DutyClaim._99ClaimedDuty = value; }
		}

		public ZPropertyInfo _99ClaimedDutyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema._99ClaimedDuty, x => USI_DRW99ClaimedDutyInfo); }
		}

		#endregion

		#region Calculated Duty

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public ZDecimal CalculatedDuty
		{
			get { return Claims.DutyClaim.CalculatedAmount; }
			set
			{
				var oldValue = CalculatedDuty;
				Claims.DutyClaim.CalculatedAmount = value;
				CalculatedDutyInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CalculatedDutyInfo
		{
			get { return GetZPropertyInfo(Schema.CalculatedDuty); }
		}

		#endregion

		#region Adjusted Claim Duty
		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public ZDecimal AdjClaimDuty
		{
			get { return Claims.DutyClaim.AdjClaimAmount; }
			set
			{
				var oldValue = AdjClaimDuty;
				Claims.DutyClaim.AdjClaimAmount = value;
				AdjClaimDutyInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AdjClaimDutyInfo
		{
			get { return GetZPropertyInfo(Schema.AdjClaimDuty); }
		}
		#endregion

		#endregion

		#region IR Tax Claim

		#region Declated Tax

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DeclaredTax", Caption = "IR Tax")]
		public ZDecimal DeclaredTax
		{
			get { return Claims.IRTaxClaim.DeclaredAmount; }
			set
			{
				var oldValue = DeclaredTax;
				Claims.IRTaxClaim.DeclaredAmount = value;
				DeclaredTaxInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DeclaredTaxInfo
		{
			get { return GetZPropertyInfo(Schema.DeclaredTax); }
		}

		#endregion

		#region Per unit

		public ZDecimal TaxPerUnit
		{
			get { return Claims.IRTaxClaim.PerUnit; }
		}

		public ZPropertyInfo TaxPerUnitInfo
		{
			get { return GetZPropertyInfo(Schema.TaxPerUnit); }
		}

		#endregion

		#region Claimed Tax

		public ZDecimal ClaimedTax
		{
			get { return Claims.IRTaxClaim.ClaimedDuty; }
		}

		public ZPropertyInfo ClaimedTaxInfo
		{
			get { return GetZPropertyInfo(Schema.ClaimedTax); }
		}

		#endregion

		#region 99% claimed tax

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public ZDecimal _99ClaimedTax
		{
			get { return Claims.IRTaxClaim._99ClaimedDuty; }
			set { Claims.IRTaxClaim._99ClaimedDuty = value; }
		}

		public ZPropertyInfo _99ClaimedTaxInfo
		{
			get { return GetWrappedZPropertyInfo(Schema._99ClaimedTax, x => USI_DRW99ClaimedTaxInfo); }
		}

		#endregion

		#region Calculated Tax

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public ZDecimal CalculatedTax
		{
			get { return Claims.IRTaxClaim.CalculatedAmount; }
			set
			{
				var oldValue = CalculatedTax;
				Claims.IRTaxClaim.CalculatedAmount = value;
				CalculatedTaxInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CalculatedTaxInfo
		{
			get { return GetZPropertyInfo(Schema.CalculatedTax); }
		}

		#endregion

		#region Adjusted Claim Tax
		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public ZDecimal AdjClaimTax
		{
			get { return Claims.IRTaxClaim.AdjClaimAmount; }
			set
			{
				var oldValue = AdjClaimTax;
				Claims.IRTaxClaim.AdjClaimAmount = value;
				AdjClaimTaxInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AdjClaimTaxInfo
		{
			get { return GetZPropertyInfo(Schema.AdjClaimTax); }
		}
		#endregion

		#endregion

		#region MPF Claim

		#region Declared MPF

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DeclaredMPF", Caption = "Total Entry MPF")]
		public ZDecimal DeclaredMPF
		{
			get { return Claims.MPFClaim.DeclaredAmount; }
			set
			{
				var oldValue = DeclaredMPF;
				Claims.MPFClaim.DeclaredAmount = value;
				DeclaredMPFInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DeclaredMPFInfo
		{
			get { return GetZPropertyInfo(Schema.DeclaredMPF); }
		}

		#endregion

		#region Line MPF

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|LineMPF", Caption = "Weighted MPF")]
		public ZDecimal LineMPF
		{
			get { return Claims.MPFClaim.LineAmount; }
		}

		public ZPropertyInfo LineMPFInfo
		{
			get { return GetZPropertyInfo(Schema.LineMPF); }
		}

		#endregion

		#region Per unit

		public ZDecimal MPFPerUnit
		{
			get { return Claims.MPFClaim.PerUnit; }
		}

		public ZPropertyInfo MPFPerUnitInfo
		{
			get { return GetZPropertyInfo(Schema.MPFPerUnit); }
		}

		#endregion

		#region Claimed MPF

		public ZDecimal ClaimedMPF
		{
			get { return Claims.MPFClaim.ClaimedDuty; }
		}

		public ZPropertyInfo ClaimedMPFInfo
		{
			get { return GetZPropertyInfo(Schema.ClaimedMPF); }
		}

		#endregion

		#region 99% claimed MPF

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public ZDecimal _99ClaimedMPF
		{
			get { return Claims.MPFClaim._99ClaimedDuty; }
			set { Claims.MPFClaim._99ClaimedDuty = value; }
		}

		public ZPropertyInfo _99ClaimedMPFInfo
		{
			get { return GetWrappedZPropertyInfo(Schema._99ClaimedMPF, x => USI_DRW99ClaimedMPFInfo); }
		}

		#endregion

		#region Calculated MPF

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public ZDecimal CalculatedMPF
		{
			get { return Claims.MPFClaim.CalculatedAmount; }
			set
			{
				var oldValue = CalculatedMPF;
				Claims.MPFClaim.CalculatedAmount = value;
				CalculatedMPFInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CalculatedMPFInfo
		{
			get { return GetZPropertyInfo(Schema.CalculatedMPF); }
		}

		#endregion

		#region Adjusted Claim MPF
		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public ZDecimal AdjClaimMPF
		{
			get { return Claims.MPFClaim.AdjClaimAmount; }
			set
			{
				var oldValue = AdjClaimMPF;
				Claims.MPFClaim.AdjClaimAmount = value;
				AdjClaimMPFInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AdjClaimMPFInfo
		{
			get { return GetZPropertyInfo(Schema.AdjClaimMPF); }
		}
		#endregion

		#region MPF Weighted Ratio

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[DecimalPlaces(6)]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|MPFWeightedRatio", Caption = "MPF Weighted Ratio")]
		public ZDecimal MPFWeightedRatio
		{
			get
			{
				var result = Claims.MPFClaim.WeightedRatio;
				if (result.IsEmpty && Claims.MPFClaim.IsClaimable)
				{
					result = WeightedRatio;
				}
				return result;
			}
			set
			{
				if (value == WeightedRatio)
				{
					value = ZDecimal.Zero;
				}

				Claims.MPFClaim.WeightedRatio = value;
			}
		}

		public ZPropertyInfo MPFWeightedRatioInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.MPFWeightedRatio, x => US_DRWMPFWeightedRatioInfo); }
		}

		#endregion

		#endregion

		#region HMF Claim

		#region Quarterly HMF

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public override ZBool US_DRWQuarterlyHMF
		{
			get { return base.US_DRWQuarterlyHMF; }
			set { base.US_DRWQuarterlyHMF = value; }
		}

		#endregion

		#region Declared HMF

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DeclaredHMF", Caption = "Total Entry HMF")]
		public ZDecimal DeclaredHMF
		{
			get { return Claims.HMFClaim.DeclaredAmount; }
			set
			{
				var oldValue = DeclaredHMF;
				Claims.HMFClaim.DeclaredAmount = value;
				DeclaredHMFInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DeclaredHMFInfo
		{
			get { return GetZPropertyInfo(Schema.DeclaredHMF); }
		}

		#endregion

		#region Line HMF

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|LineHMF", Caption = "Weighted HMF")]
		public ZDecimal LineHMF
		{
			get { return Claims.HMFClaim.LineAmount; }
		}

		public ZPropertyInfo LineHMFInfo
		{
			get { return GetZPropertyInfo(Schema.LineHMF); }
		}

		#endregion

		#region Per unit

		public ZDecimal HMFPerUnit
		{
			get { return Claims.HMFClaim.PerUnit; }
		}

		public ZPropertyInfo HMFPerUnitInfo
		{
			get { return GetZPropertyInfo(Schema.HMFPerUnit); }
		}

		#endregion

		#region Claimed HMF

		public ZDecimal ClaimedHMF
		{
			get { return Claims.HMFClaim.ClaimedDuty; }
		}

		public ZPropertyInfo ClaimedHMFInfo
		{
			get { return GetZPropertyInfo(Schema.ClaimedHMF); }
		}

		#endregion

		#region 99% claimed HMF

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public ZDecimal _99ClaimedHMF
		{
			get { return Claims.HMFClaim._99ClaimedDuty; }
			set { Claims.HMFClaim._99ClaimedDuty = value; }
		}

		public ZPropertyInfo _99ClaimedHMFInfo
		{
			get { return GetWrappedZPropertyInfo(Schema._99ClaimedHMF, x => USI_DRW99ClaimedHMFInfo); }
		}

		#endregion

		#region Calculated HMF

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public ZDecimal CalculatedHMF
		{
			get { return Claims.HMFClaim.CalculatedAmount; }
			set
			{
				var oldValue = CalculatedHMF;
				Claims.HMFClaim.CalculatedAmount = value;
				CalculatedHMFInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CalculatedHMFInfo
		{
			get { return GetZPropertyInfo(Schema.CalculatedHMF); }
		}

		#endregion

		#region Adjusted Claim HMF
		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public ZDecimal AdjClaimHMF
		{
			get { return Claims.HMFClaim.AdjClaimAmount; }
			set
			{
				var oldValue = AdjClaimHMF;
				Claims.HMFClaim.AdjClaimAmount = value;
				AdjClaimHMFInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AdjClaimHMFInfo
		{
			get { return GetZPropertyInfo(Schema.AdjClaimHMF); }
		}
		#endregion

		#endregion

		#region Other fees Claim

		#region Declared Other fees

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackJobComInvoiceLine|DeclaredOtherFees", Caption = "Other fees")]
		public ZDecimal DeclaredOtherFees
		{
			get { return Claims.OtherFeesClaim.DeclaredAmount; }
			set
			{
				var oldValue = DeclaredOtherFees;
				Claims.OtherFeesClaim.DeclaredAmount = value;
				DeclaredOtherFeesInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DeclaredOtherFeesInfo
		{
			get { return GetZPropertyInfo(Schema.DeclaredOtherFees); }
		}

		#endregion

		#region Per unit

		public ZDecimal OtherFeesPerUnit
		{
			get { return Claims.OtherFeesClaim.PerUnit; }
		}

		public ZPropertyInfo OtherFeesPerUnitInfo
		{
			get { return GetZPropertyInfo(Schema.OtherFeesPerUnit); }
		}

		#endregion

		#region Claimed Other Fees

		public ZDecimal ClaimedOtherFees
		{
			get { return Claims.OtherFeesClaim.ClaimedDuty; }
		}

		public ZPropertyInfo ClaimedOtherFeesInfo
		{
			get { return GetZPropertyInfo(Schema.ClaimedOtherFees); }
		}

		#endregion

		#region 99% Claimed Other Fees

		public ZDecimal _99ClaimedOtherFees
		{
			get { return Claims.OtherFeesClaim._99ClaimedDuty; }
		}

		public ZPropertyInfo _99ClaimedOtherFeesInfo
		{
			get { return GetZPropertyInfo(Schema._99ClaimedOtherFees); }
		}

		#endregion

		#endregion

		internal ZString ImpDeclInvoiceNumber
		{
			get
			{
				var declaredLine = DrawbackImportEntryLine;
				return declaredLine != null ? declaredLine.InvoiceNumber : US_ImpDecInvoiceNum;
			}
		}

		internal ZString PartNo
		{
			get
			{
				var declaredLine = DrawbackImportEntryLine;
				var partNoFromDeclaredLine = declaredLine != null ? declaredLine.PartNo : ZString.Empty;

				return !partNoFromDeclaredLine.IsEmpty ? partNoFromDeclaredLine : JI_PartNo;
			}
		}

		#endregion

		public bool US_DRWCalcDutyWithAdValoremRate_ReadOnly
		{
			get { return !US_DRWClaimAmountOverriden_New || JI_Tariff.IsEmpty; }
		}

		public bool US_DRWAdValoremRate_ReadOnly
		{
			get { return !US_DRWClaimAmountOverriden_New || !US_DRWCalcDutyWithAdValoremRate; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public DrawbackOtherFeeCollection DrawbackOtherFees
		{
			get
			{
				if (fDrawbackOtherFees == null)
				{
					fDrawbackOtherFees = new DrawbackOtherFeeCollection(this);
					fDrawbackOtherFees.Load();
					RegisterEditableChildObject(fDrawbackOtherFees);
				}
				return fDrawbackOtherFees;
			}
		}
		DrawbackOtherFeeCollection fDrawbackOtherFees;

		#endregion

		#region Drawback Notice Of Intent

		public override ZString US_DRWTENo
		{
			get { return GetEffectiveValueToReturn(base.US_DRWTENo, ZString.Empty, ZString.Empty, JobDeclaration.Schema.US_DRWTENo); }
			set { base.US_DRWTENo = GetEffectiveValueToSet(value, ZString.Empty, ZString.Empty, JobDeclaration.Schema.US_DRWTENo); }
		}

		public override ZString US_DRWIntendedPortOfExport
		{
			get { return GetEffectiveValueToReturn(base.US_DRWIntendedPortOfExport, ZString.Empty, ZString.Empty, JobDeclaration.Schema.US_DRWIntendedPortOfExport); }
			set { base.US_DRWIntendedPortOfExport = GetEffectiveValueToSet(value, ZString.Empty, ZString.Empty, JobDeclaration.Schema.US_DRWIntendedPortOfExport); }
		}

		public override ZString US_UI_NKCarrierSCAC
		{
			get { return GetEffectiveValueToReturn(base.US_UI_NKCarrierSCAC, ZString.Empty, ZString.Empty, JobDeclaration.Schema.US_UI_NKCarrierSCAC); }
			set { base.US_UI_NKCarrierSCAC = GetEffectiveValueToSet(value, ZString.Empty, ZString.Empty, JobDeclaration.Schema.US_UI_NKCarrierSCAC); }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Organisations))]
		public ZGuid US_OH_DRWExporterOrDestroyer
		{
			get { return ExporterOrDestroyer.OrganisationPK; }
			set
			{
				var oldValue = US_OH_DRWExporterOrDestroyer;
				ExporterOrDestroyer.OrganisationPK = value;
				US_OH_DRWExporterOrDestroyerInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo US_OH_DRWExporterOrDestroyerInfo
		{
			get { return GetZPropertyInfo(Schema.US_OH_DRWExporterOrDestroyer); }
		}

		[List(nameof(ExporterOrDestroyer) + "." + nameof(JobDocAddress.Organisation) + "." + nameof(OrgHeader.Address_List))]
		public ZGuid US_OA_DRWExporterOrDestroyer
		{
			get { return ExporterOrDestroyer.E2_OA_Address; }
			set
			{
				var oldValue = US_OA_DRWExporterOrDestroyer;
				ExporterOrDestroyer.E2_OA_Address = value;
				US_OA_DRWExporterOrDestroyerInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo US_OA_DRWExporterOrDestroyerInfo
		{
			get { return GetZPropertyInfo(Schema.US_OA_DRWExporterOrDestroyer); }
		}

		public bool US_OA_DRWExporterOrDestroyer_ReadOnly
		{
			get { return ExporterOrDestroyer.Organisation == null; }
		}

		public JobDocAddress ExporterOrDestroyer
		{
			get
			{
				if (fExporterOrDestroyer == null || fExporterOrDestroyer.IsDeleted)
				{
					fExporterOrDestroyer = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.DrawbackExporterOrDestroyer);
					if (fExporterOrDestroyer.OrganisationPK.IsEmpty && !fExporterOrDestroyer.E2_AddressOverride && Declaration is JobDeclaration declaration)
					{
						fExporterOrDestroyer.OrganisationPK = declaration.JE_OH_Importer;
					}
					fExporterOrDestroyer.AdditionalValidation = InvoiceLineDocAddressValidation(fExporterOrDestroyer);
				}
				return fExporterOrDestroyer;
			}
		}
		JobDocAddress fExporterOrDestroyer;

		public JobDocAddressValidation InvoiceLineDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new InvoiceLineJobDocAddressValidation(addressToValidate);
		}

		public JobDocAddress LocationOfMerchandise
		{
			get
			{
				if (fLocationOfMerchandise == null || fLocationOfMerchandise.IsDeleted)
				{
					fLocationOfMerchandise = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.DrawbackLocationOfMerchandise);
				}
				return fLocationOfMerchandise;
			}
		}
		JobDocAddress fLocationOfMerchandise;

		public JobDocAddress LocationOfDestruction
		{
			get
			{
				if (fLocationOfDestruction == null || fLocationOfDestruction.IsDeleted)
				{
					fLocationOfDestruction = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.DrawbackLocationOfDestruction);
				}
				return fLocationOfDestruction;
			}
		}
		JobDocAddress fLocationOfDestruction;

		#endregion

		#region IDrawback7551DocLine Members

		ZString IDrawback7551DocLine.DrawbackExportQtyAndUnit
		{
			get { return new ZStringBuilder().Append(DRWExportQuantity.ToString("#.#####", CultureInfo.CurrentCulture)).Append(DRWExportUQ).ToStringWithDelimiterBetweenAppends(" "); }
		}

		ZString IDrawback7551DocLine.DrawbackExportUQ
		{
			get { return DRWExportUQ; }
		}

		#endregion

		#region IDrawbackManufDocLine Members

		ZString IDrawback7551ManufDocLine.DrawbackManufQtyAndUQ
		{
			get { return new ZStringBuilder().Append(US_DRWQuantityUsed.ToString("0.#####", CultureInfo.CurrentCulture)).Append(US_DRWUQUsed).ToStringWithDelimiterBetweenAppends(" "); }
		}

		#endregion

		#region IDrawback7551ImportDocLine Members

		ZString IDrawback7551ImportDocLine.DrawbackImportEntryNumber
		{
			get { return FormattedDrawbackEntryNoOrCMDNumber; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackPortCode
		{
			get { return US_DRWPort; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackImportDate
		{
			get { return US_DRWEntryDate.ToString("MM/dd/yyyy", CultureInfo.CurrentCulture); }
		}

		ZString IDrawback7551ImportDocLine.DrawbackCDIndicator
		{
			get { return US_DRWCMCDIndicator == "D" ? "Y" : string.Empty; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackReceivedDates
		{
			get
			{
				var result = ZString.Empty;
				if (!US_DRWDateRcvFrom.IsEmpty)
				{
					result = US_DRWDateRcvFrom.ToString("MMddyy", CultureInfo.CurrentCulture);
					if (!US_DRWDateRcvTo.IsEmpty)
					{
						result += "/" + US_DRWDateRcvTo.ToString("MMddyy", CultureInfo.CurrentCulture);
					}
				}
				return result;
			}
		}

		ZString IDrawback7551ImportDocLine.DrawbackUsedDates
		{
			get
			{
				var result = ZString.Empty;
				if (!US_DRWDateUsedFrom.IsEmpty)
				{
					result = US_DRWDateUsedFrom.ToString("MMddyy", CultureInfo.CurrentCulture);
					if (!US_DRWDateUsedTo.IsEmpty)
					{
						result += "/" + US_DRWDateUsedTo.ToString("MMddyy", CultureInfo.CurrentCulture);
					}
				}
				return result;
			}
		}

		ZString IDrawback7551ImportDocLine.DrawbackHTSUSNo
		{
			get { return JI_Tariff; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackDocDescription
		{
			get { return DrawbackDocDescription; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackImportQtyAndUnit
		{
			get
			{
				var quantity = US_DRWQuantityUsed > 0 ? US_DRWQuantityUsed : DRWImportQuantity;
				return new ZStringBuilder().Append(quantity.ToString("#.#####", CultureInfo.CurrentCulture)).Append(DRWImportUQ).ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		ZString IDrawback7551ImportDocLine.DrawbackImportUQ
		{
			get { return US_DRWQuantityUsed > 0 ? US_DRWUQUsed : DRWImportUQ; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackEnteredValuePer
		{
			get
			{
				return IsACEDrawback ? !SubstitutedValuePerUnit.IsEmpty ? SubstitutedValuePerUnit.ToString("0.####", CultureInfo.CurrentCulture) : !DRWGoodsValuePerUQ.IsEmpty ? DRWGoodsValuePerUQ.ToString("0.####", CultureInfo.CurrentCulture) : string.Empty :
							  !DutyPerUnit.IsEmpty ? DutyPerUnit.ToString("0.#####", CultureInfo.CurrentCulture) : string.Empty;
			}
		}

		ZString IDrawback7551ImportDocLine.DrawbackDutyRate
		{
			get { return LineDutyRateDesc; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackBox29SectionIIUQ
		{
			get { return US_DRWQuantityUsed > 0 ? US_DRWUQUsed : DRWExportUQ; }
		}

		#endregion

		#region IDrawback7551ExportDocLine Members

		ZDateTime IDrawbackExportDocLine.DrawbackExportDate
		{
			get { return US_DRWExportDate; }
		}

		ZString IDrawbackExportDocLine.DrawbackExportAction
		{
			get { return US_DRWExportAction; }
		}

		ZString IDrawbackExportDocLine.DrawbackExportID
		{
			get { return US_DRWExportID; }
		}

		ZString IDrawbackExportDocLine.DrawbackExporterName
		{
			get { return ExporterOrDestroyer.E2_CompanyNameTruncated; }
		}

		ZString IDrawbackExportDocLine.DrawbackExpDocDescription
		{
			get { return DrawbackDocDescription; }
		}

		ZString IDrawbackExportDocLine.DrawbackExportDest
		{
			get { return US_DRWExportDest; }
		}

		ZString IDrawbackExportDocLine.DrawbackExportTariff
		{
			get { return US_ExportTariff; }
		}

		#endregion

		#region IACEDrawbackImportClaim Members

		ZString IACEDrawbackImportClaim.ActionIndicator
		{
			get { return US_DRWImpActInd; }
		}

		ZString IACEDrawbackImportClaim.EntryFilerCode
		{
			get { return US_ImportEntryNo.Left(3); }
		}

		ZString IACEDrawbackImportClaim.EntryNumber
		{
			get { return US_ImportEntryNo.SubstringSafe(3, 8); }
		}

		ZString IACEDrawbackImportClaim.CBPESLine
		{
			get { return US_DRWImportEntryLine.ToString(); }
		}

		ZString IACEDrawbackImportClaim.CertificateofDeliveryIndicator
		{
			get { return US_DRWCDInd ? "X" : string.Empty; }
		}

		ZString IACEDrawbackImportClaim.ManufacturerRulingNumber
		{
			get { return US_DRWImpManufRuleNo; }
		}

		ZString IACEDrawbackImportClaim.BasisOfClaim
		{
			get { return US_DRWClaimBasis; }
		}

		ZDateTime IACEDrawbackImportClaim.ManufDateReceived
		{
			get { return US_DRWDateRcvFrom; }
		}

		ZDateTime IACEDrawbackImportClaim.ManufDateUsed
		{
			get { return US_DRWDateUsedFrom; }
		}

		ZString IACEDrawbackImportClaim.TrackingIdentificationNumber
		{
			get { return US_DRWImpTrkID; }
		}

		ZString IACEDrawbackImportClaim.DrawbackAccountingMethodCode
		{
			get { return US_DRWAccMethod; }
		}

		IEnumerable<IACEDrawbackImportClassification> IACEDrawbackImportClaim.ImportClassifications
		{
			get
			{
				var chapter98Tariffs = new List<ACEDrawbackImportClassification>();
				var chapter99Tariffs = new List<ACEDrawbackImportClassification>();
				var otherTariffs = new List<ACEDrawbackImportClassification>();

				foreach (DrawbackAdditionalImportTariffNumber additionalTariff in DrawbackAdditionalImportTariffNumbers.OfType<DrawbackAdditionalImportTariffNumber>().OrderBy(x => x.US_LineNo))
				{
					var aCEDrawbackImportClassification = new ACEDrawbackImportClassification(additionalTariff.US_Tariff, additionalTariff.US_Description, additionalTariff.US_Quantity1, DRWExportUQ, additionalTariff.US_AllowableQty1, additionalTariff.US_ValuePerUnit1, additionalTariff.US_SubstitutedValue1);
					if (Chapter98Helper.Is98Tariff(additionalTariff.US_Tariff))
					{
						chapter98Tariffs.Add(aCEDrawbackImportClassification);
					}
					else if (Chapter98Helper.Is99Tariff(additionalTariff.US_Tariff))
					{
						chapter99Tariffs.Add(aCEDrawbackImportClassification);
					}
					else
					{
						otherTariffs.Add(aCEDrawbackImportClassification);
					}
				}

				foreach (ACEDrawbackImportClassification aCEDrawbackImportClassification in chapter98Tariffs)
				{
					yield return aCEDrawbackImportClassification;
				}

				foreach (ACEDrawbackImportClassification aCEDrawbackImportClassification in chapter99Tariffs)
				{
					yield return aCEDrawbackImportClassification;
				}

				yield return new ACEDrawbackImportClassification(JI_Tariff, DrawbackDocDescription, DRWExportQuantity, DRWExportUQ, DRWAllowableQTY, DRWGoodsValuePerUQ, SubstitutedValuePerUnit);

				foreach (ACEDrawbackImportClassification aCEDrawbackImportClassification in otherTariffs)
				{
					yield return aCEDrawbackImportClassification;
				}
			}
		}

		IEnumerable<IACEDrawbackRevenueClaimed> IACEDrawbackImportClaim.RevenueAmounts
		{
			get
			{
				if (_99ClaimedDuty > 0m)
				{
					yield return new ACEDrawbackRevenueClaimed(DrawbackFeeTypesList.Codes.DrawbackDuty, _99ClaimedDuty, CalculatedDuty, AdjClaimDuty, ZString.Empty);
				}

				if (_99ClaimedTax > 0m)
				{
					yield return new ACEDrawbackRevenueClaimed(DrawbackFeeTypesList.Codes.DrawbackTaxes, _99ClaimedTax, CalculatedTax, AdjClaimTax, ZString.Empty);
				}

				if (_99ClaimedHMF > 0m)
				{
					var qualifier = US_DRWQuarterlyHMF ? (ZString)"01" : ZString.Empty;
					yield return new ACEDrawbackRevenueClaimed(DrawbackFeeTypesList.Codes.DrawbackHMF, _99ClaimedHMF, CalculatedHMF, AdjClaimHMF, qualifier);
				}

				if (_99ClaimedMPF > 0m)
				{
					yield return new ACEDrawbackRevenueClaimed(DrawbackFeeTypesList.Codes.DrawbackMPF, _99ClaimedMPF, CalculatedMPF, AdjClaimMPF, ZString.Empty);
				}

				foreach (DrawbackOtherFee otherFee in DrawbackOtherFees)
				{
					if (otherFee._99ClaimedAmount > 0m)
					{
						yield return new ACEDrawbackRevenueClaimed(otherFee.US_FeeType, otherFee._99ClaimedAmount, otherFee.CalculatedAmount, otherFee.AdjClaimAmount, ZString.Empty);
					}
				}
			}
		}

		#endregion

		#region IACEDrawbackManufactureClaim Members

		ZString IACEDrawbackManufactureClaim.ActionIndicator
		{
			get { return US_DRWMafActInd; }
		}

		ZString IACEDrawbackManufactureClaim.ImportManufactureRulingNumber
		{
			get { return US_DRWImpManufRuleNo; }
		}

		ZString IACEDrawbackManufactureClaim.HTSNumber
		{
			get { return JI_Tariff; }
		}

		ZDecimal IACEDrawbackManufactureClaim.Quantity
		{
			get { return US_DRWQuantityUsed; }
		}

		ZString IACEDrawbackManufactureClaim.UnitOfMeasure
		{
			get { return US_DRWUQUsed; }
		}

		ZDateTime IACEDrawbackManufactureClaim.ProductionDate
		{
			get { return US_DRWDateOfManufacture; }
		}

		ZString IACEDrawbackManufactureClaim.FactoryLocation
		{
			get { return US_DRWFactoryLocation; }
		}

		ZString IACEDrawbackManufactureClaim.DescriptionText
		{
			get { return US_DRWDescrManufactured; }
		}

		ZString IACEDrawbackManufactureClaim.ManufactureRulingNumber
		{
			get { return US_DRWManufRuleNo; }
		}

		ZString IACEDrawbackManufactureClaim.ImportTrackingID
		{
			get { return US_DRWImpTrkID; }
		}

		ZString IACEDrawbackManufactureClaim.ManufacturedTrackingID
		{
			get { return US_DRWMafTrkID; }
		}

		#endregion

		#region IACEDrawbackExportClaim Members

		ZString IACEDrawbackExportClaim.ExportDestroyIndicator
		{
			get { return US_DRWExportAction; }
		}

		ZString IACEDrawbackExportClaim.HTSNumber
		{
			get { return US_ExportTariff; }
		}

		ZDecimal IACEDrawbackExportClaim.Quantity
		{
			get { return US_DRWExportQuantity; }
		}

		ZString IACEDrawbackExportClaim.UnitOfMeasure
		{
			get { return US_DRWExportUQ; }
		}

		ZDateTime IACEDrawbackExportClaim.ExportDate
		{
			get { return US_DRWExportDate; }
		}

		ZString IACEDrawbackExportClaim.NoticeOfIntentIndicator
		{
			get { return US_DRWExpNoticeInd ? "Y" : string.Empty; }
		}

		ZString IACEDrawbackExportClaim.WaiverToDrawbackIndicator
		{
			get { return US_DRWExpWavInd ? "Y" : string.Empty; }
		}

		ZString IACEDrawbackExportClaim.NameOfExporter
		{
			get { return ExporterOrDestroyer != null ? ExporterOrDestroyer.E2_CompanyName.Left(ExporterNameMaxLength) : ZString.Empty; }
		}
		internal const int ExporterNameMaxLength = 30;

		ZString IACEDrawbackExportClaim.CountryOfUltimateDestination
		{
			get { return US_DRWExportDest; }
		}

		ZString IACEDrawbackExportClaim.BOLIndicator
		{
			get { return US_DRWExpBOLInd ? "Y" : string.Empty; }
		}

		ZString IACEDrawbackExportClaim.BOLCarrierCode
		{
			get { return US_DRWExpBOLCarrier; }
		}

		ZString IACEDrawbackExportClaim.DescriptionText
		{
			get { return DrawbackDocDescription.Left(50); }
		}

		ZString IACEDrawbackExportClaim.UniqueIdentifierNumber
		{
			get { return US_DRWExportID; }
		}

		ZString IACEDrawbackExportClaim.ImportTrackingNumber
		{
			get
			{
				return US_DRWIsForManufacturerSection ? ZString.Empty : US_DRWImpTrkID;
			}
		}

		ZString IACEDrawbackExportClaim.ManufacturedTrackingNumber
		{
			get { return US_DRWMafTrkID; }
		}

		#endregion

		#region IACEDrawbackTFTEAClaim Members

		ZString IACEDrawbackTFTEAClaim.ScheduleBCode
		{
			get { return US_TariffType == TariffTypeList.Codes.ScheduleB ? "X" : string.Empty; }
		}

		#endregion

		#region IACEDrawbackTrackingNumber Members
		internal const int MaxImpTrackingNumber = 50000;
		ZInt IACEDrawbackTrackingNumberLine.ArrangeTrackingNumbers(ZInt startIndex)
		{
			var trackingNumberRequired = Declaration is JobDeclaration declaration && !declaration.US_EntryType.IsEmpty && ACEDrawbackProvisionsList.IsTFTEAExcept57(declaration.US_EntryType);
			if (trackingNumberRequired && US_DRWIsForImportSection)
			{
				startIndex++;
				US_DRWImpTrkID = startIndex.ToString("D5", CultureInfo.InvariantCulture);
			}
			else
			{
				US_DRWImpTrkID = ZString.Empty;
			}

			if (trackingNumberRequired && US_DRWIsForManufacturerSection)
			{
				var manufacturerIndex = startIndex + MaxImpTrackingNumber;
				US_DRWMafTrkID = manufacturerIndex.ToString("D5", CultureInfo.InvariantCulture);
			}
			else
			{
				US_DRWMafTrkID = ZString.Empty;
			}

			return startIndex;
		}

		#endregion
	}
}
