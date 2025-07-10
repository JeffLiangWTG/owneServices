using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.LicenseManager;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[DebuggerDisplay("Tariff = {CL_AdValoremTariff}")]
	[CodeProperty("EntryLineCode")]
	[DescriptionProperty("EntryLineDescription")]
	public partial class CusEntryLine : TypeSafeCusEntryLine,
		Integration.Customs.US.ICusEntryLine,
		ICusEntryLine,
		IEntryLineDutyDataProvider,
		IFDAEntryLine,
		ICargoReleaseCusEntryLine,
		IAESTIRCommodityLineItem,
		IAESTIRUsedVehicle,
		IFeeCalculationDataProvider,
		IACECusEntryLine,
		ISimplifiedEntryLine,
		IFTZLine,
		IGovernmentAgencies,
		Integration.Customs.ICusCodeDataTypeSupporter,
		IPGAGovernmentAgenciesCommon,
		IDrawbackEntryLine,
		IEntryLine,
		Customs.Business.ICusDispositionParent
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : TypeSafeCusEntryLine.Schema
		{
			public const string ExportCode = "ExportCode";
			public const string SecondCustomsQuantity = "SecondCustomsQuantity";
			public const string SecondCustomsUnitQty = "SecondCustomsUnitQty";
			public const string LicenseType = "LicenseType";
			public const string LicenseNumber = "LicenseNumber";
			public const string MarksAndNumbers = "MarksAndNumbers";
			public const string IsUsedVehicle = "IsUsedVehicle";
			public const string VehicleIDType = "VehicleIDType";
			public const string VehicleID = "VehicleID";
			public const string VehicleTitleNumber = "VehicleTitleNumber";
			public const string VehicleTitleState = "VehicleTitleState";
			public const string ECCN = "ECCN";
			public const string AESOriginIndicator = "AESOriginIndicator";
			public const string CL_Calc_EntryNumber = "CL_Calc_EntryNumber";
			public const string CL_Calc_XTN = "CL_Calc_XTN";
			public const string ThirdCustomsQuantity = "ThirdCustomsQuantity";
			public const string ThirdCustomsUnitQty = "ThirdCustomsUnitQty";
			public const string DDTCITARExemptionNumber = "DDTCITARExemptionNumber";
			public const string DDTCRegistrationNumber = "DDTCRegistrationNumber";
			public const string DDTCSignificantMilitaryEquipmentIndicator = "DDTCSignificantMilitaryEquipmentIndicator";
			public const string DDTCEligiblePartyCertificationIndicator = "DDTCEligiblePartyCertificationIndicator";
			public const string DDTCUSMLCategoryCode = "DDTCUSMLCategoryCode";
			public const string DDTCUnitOfMeasure = "DDTCUnitOfMeasure";
			public const string DDTCQuantity = "DDTCQuantity";
			public const string DDTCCommodityJurisdictionNumber = "DDTCCommodityJurisdictionNumber";
		}

		internal bool IsSupLineOrNormalLine
		{
			get
			{
				return RandomLine.HasEmptySupTariff || US_SupLine;
			}
		}

		internal bool IsACEFTZSupLine => (US_SupLine && Header.IsFTZAdmission);

		protected override Customs.Business.BaseJobComInvoiceLine GetRandomLine()
		{
			if (fRandomLine == null || fRandomLine.IsNull || fRandomLine.IsDeleted)
			{
				fRandomLine = GetRandomLineInternal();
			}

			return fRandomLine;
		}
		//Cached for the performance reason
		JobComInvoiceLine fRandomLine;

		JobComInvoiceLine GetRandomLineInternal()
		{
			return Helper.GetRandomLine() as JobComInvoiceLine ?? (JobComInvoiceLine)base.GetRandomLine();
		}

		protected ICusEntryLine iCusEntryLine
		{
			get { return this; }
		}

		protected override ZString UniqueKeyPrefix
		{
			get { return Declaration.US_EntryFilerCode; }
		}

		protected override ZString GetDutyRateDescription()
		{
			return US_DutyRateDesc;
		}

		#region Relate Objects

		public PSCReasonCusCodeData PSCReasonCodes
		{
			get { return pscReasonCodes ?? (pscReasonCodes = new PSCReasonCusCodeData.Loader(Factory).Load(this)); }
		}
		PSCReasonCusCodeData pscReasonCodes;

		/// <summary>
		/// Returns a house bill of JobComInvoiceHeader.JZ_CU_RelatedBill for Air
		/// Returns a master bill of JobComInvoiceHeader.JZ_CU_RelatedBill for other transport modes
		/// </summary>
		public Bill FTZBill
		{
			get { return RandomLine?.InvoiceHeader?.FTZBill; }
		}

		#endregion

		#region Merge

		protected override ZDecimal GetInvoiceLineCustomsValueToAggregate(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;

			var isParentLineSupAdditionalTariffLine = false;
			if (ParentLine is CusEntryLine parentLine)
			{
				isParentLineSupAdditionalTariffLine = parentLine.US_SupAdditionalLine;
			}
			else if (!US_SupAdditionalLine)
			{
				isParentLineSupAdditionalTariffLine = !invoiceLine.US_SupAdditionalTariff1.IsEmpty && invoiceLine.US_SupAdditionalTariff1 != TariffViewAsCodeDescription.NotApplicableCode;
			}

			var result = CustomsValueDeciderForInvoiceLine.GetCustomsValue(invoiceLine, US_SupLine, forSupplementaryAdditionalTariffLine: US_SupAdditionalLine, isParentLineSupAdditionalTariffLine: isParentLineSupAdditionalTariffLine && !IsSetXLine && !IsSetVLine);

			if (invoiceLine.IsSetXLine && Header != null && !Header.IsCustomsChargeToBeCalculated)
			{
				result = ZDecimal.Zero;
			}

			return result;
		}

		protected override ZString GetInvoiceLineTariffToSet(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetInvoiceLineTariffToSet(invoiceLine);
			var usInvoiceLine = (JobComInvoiceLine)invoiceLine;
			if (US_SupAdditionalLine)
			{
				result = usInvoiceLine.US_SupAdditionalTariff1;
			}
			else if (US_SupAdditionalLine2)
			{
				result = usInvoiceLine.US_SupAdditionalTariff2;
			}
			else if (US_SupAdditionalLine3)
			{
				result = usInvoiceLine.US_SupAdditionalTariff3;
			}
			else if (US_SupAdditionalLine4)
			{
				result = usInvoiceLine.US_SupAdditionalTariff4;
			}
			else if (US_SupAdditionalLine5)
			{
				result = usInvoiceLine.US_SupAdditionalTariff5;
			}
			else if (US_SupLine)
			{
				result = usInvoiceLine.US_SupTariff;
			}

			return result;
		}

		#endregion

		#region Export Details

		public ZString ExportCode
		{
			get { return RandomLine.US_ExportCode; }
		}

		public ZPropertyInfo ExportCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ExportCode); }
		}

		public ZDecimal SecondCustomsQuantity
		{
			get
			{
				if (secondCustomsQuantityCached == null)
				{
					secondCustomsQuantityCached = new CachedProperty<ZDecimal>(Factory, Helper.GetSecondCustomsQuantity);
				}
				return secondCustomsQuantityCached.Value;
			}
		}
		CachedProperty<ZDecimal> secondCustomsQuantityCached;

		public ZPropertyInfo SecondCustomsQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.SecondCustomsQuantity); }
		}

		public ZString SecondCustomsUnitQty
		{
			get
			{
				if (secondCustomsUnitQtyCached == null)
				{
					secondCustomsUnitQtyCached = new CachedProperty<ZString>(Factory, Helper.GetSecondCustomsUnitQty);
				}
				return secondCustomsUnitQtyCached.Value;
			}
		}
		CachedProperty<ZString> secondCustomsUnitQtyCached;

		public ZPropertyInfo SecondCustomsUnitQtyInfo
		{
			get { return GetZPropertyInfo(Schema.SecondCustomsUnitQty); }
		}

		public ZDecimal ThirdCustomsQuantity
		{
			get
			{
				if (thirdCustomsQuantityCached == null)
				{
					thirdCustomsQuantityCached = new CachedProperty<ZDecimal>(Factory, Helper.GetThirdCustomsQuantity);
				}
				return thirdCustomsQuantityCached.Value;
			}
		}
		CachedProperty<ZDecimal> thirdCustomsQuantityCached;

		public ZPropertyInfo ThirdCustomsQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.ThirdCustomsQuantity); }
		}

		public ZString ThirdCustomsUnitQty
		{
			get
			{
				if (thirdCustomsUnitQtyCached == null)
				{
					thirdCustomsUnitQtyCached = new CachedProperty<ZString>(Factory, Helper.GetThirdCustomsUnitQty);
				}
				return thirdCustomsUnitQtyCached.Value;
			}
		}
		CachedProperty<ZString> thirdCustomsUnitQtyCached;

		public ZPropertyInfo ThirdCustomsUnitQtyInfo
		{
			get { return GetZPropertyInfo(Schema.ThirdCustomsUnitQty); }
		}

		public ZDecimal GrossWeightInKilograms
		{
			get { return EffectiveGrossWeight.InKilogramsSafe; }
		}

		public ZDecimal GrossWeightInPounds
		{
			get { return EffectiveGrossWeight.InPoundsSafe; }
		}

		public ZDecimal VolumeInCubicMeters
		{
			get { return EffectiveVolume.InCubicMetres; }
		}

		public ZString LicenseType
		{
			get { return RandomLine.US_LicenseType; }
		}

		public ZPropertyInfo LicenseTypeInfo
		{
			get { return GetZPropertyInfo(Schema.LicenseType); }
		}

		public ZString LicenseNumber
		{
			get { return MessageBlockStringDataCorrector.KeepOnlyValidCharacters(RandomLine.US_LicenseNo, ABICharacterTypeString.Constants.Special, AutoUSAddInfo.Schema.US_LicenseNoMaxLength); }
		}

		public ZPropertyInfo LicenseNumberInfo
		{
			get { return GetZPropertyInfo(Schema.LicenseNumber); }
		}

		public ZString MarksAndNumbers
		{
			get { return MessageBlockStringDataCorrector.KeepOnlyValidCharacters(RandomLine.US_MarksAndNumbers, ABICharacterTypeString.Constants.Special, 75); }
		}

		public ZPropertyInfo MarksandNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.MarksAndNumbers); }
		}

		public ZBool IsUsedVehicle
		{
			get { return RandomLine.US_IsUsedVehicle; }
		}

		public ZPropertyInfo IsUsedVehicleInfo
		{
			get { return GetZPropertyInfo(Schema.IsUsedVehicle); }
		}

		public ZString VehicleIDType
		{
			get { return RandomLine.US_VehicleIDType; }
		}

		public ZPropertyInfo VehicleIDTypeInfo
		{
			get { return GetZPropertyInfo(Schema.VehicleIDType); }
		}

		public ZString VehicleID
		{
			get { return MessageBlockStringDataCorrector.KeepOnlyValidCharacters(RandomLine.US_VehicleID, ABICharacterTypeString.Constants.Special, 25); }
		}

		public ZPropertyInfo VehicleIDInfo
		{
			get { return GetZPropertyInfo(Schema.VehicleID); }
		}

		public ZString VehicleTitleNumber
		{
			get { return MessageBlockStringDataCorrector.KeepOnlyValidCharacters(RandomLine.US_VehicleTitleNo, ABICharacterTypeString.Constants.Special, 15); }
		}

		public ZPropertyInfo VehicleTitleNumberInfo
		{
			get { return GetZPropertyInfo(Schema.VehicleTitleNumber); }
		}

		public ZString VehicleTitleState
		{
			get { return RandomLine.US_VehicleTitleState; }
		}

		public ZPropertyInfo VehicleTitleStateInfo
		{
			get { return GetZPropertyInfo(Schema.VehicleTitleState); }
		}

		public ZString ECCN
		{
			get { return MessageBlockStringDataCorrector.KeepOnlyValidCharacters(RandomLine.US_ECCN, ABICharacterTypeString.Constants.Alphanumeric, 5); }
		}

		public ZPropertyInfo ECCNInfo
		{
			get { return GetZPropertyInfo(Schema.ECCN); }
		}

		#region DDTC

		public ZString DDTCITARExemptionNumber
		{
			get { return MessageBlockStringDataCorrector.KeepOnlyValidCharacters(RandomLine.US_DDTCITARExemptionNo, ABICharacterTypeString.Constants.Alphanumeric + ".", 12); }
		}

		public ZPropertyInfo DDTCITARExemptionNumberInfo
		{
			get { return GetZPropertyInfo(Schema.DDTCITARExemptionNumber); }
		}

		public ZString DDTCRegistrationNumber
		{
			get { return MessageBlockStringDataCorrector.KeepOnlyValidCharacters(RandomLine.US_DDTCRegistrationNo, ABICharacterTypeString.Constants.Special, 6); }
		}

		public ZPropertyInfo DDTCRegistrationNumberInfo
		{
			get { return GetZPropertyInfo(Schema.DDTCRegistrationNumber); }
		}

		public ZString DDTCSignificantMilitaryEquipmentIndicator
		{
			get { return RandomLine.US_DDTCMilitaryEquipmentIndicator; }
		}

		public ZPropertyInfo DDTCSignificantMilitaryEquipmentIndicatorInfo
		{
			get { return GetZPropertyInfo(Schema.DDTCSignificantMilitaryEquipmentIndicator); }
		}

		public ZString DDTCEligiblePartyCertificationIndicator
		{
			get { return RandomLine.US_DDTCPartyCertificationIndicator; }
		}

		public ZPropertyInfo DDTCEligiblePartyCertificationIndicatorInfo
		{
			get { return GetZPropertyInfo(Schema.DDTCEligiblePartyCertificationIndicator); }
		}

		public ZString DDTCUSMLCategoryCode
		{
			get { return RandomLine.US_DDTCUSMLCategoryCode; }
		}

		public ZPropertyInfo DDTCUSMLCategoryCodeInfo
		{
			get { return GetZPropertyInfo(Schema.DDTCUSMLCategoryCode); }
		}

		public ZString DDTCUnitOfMeasure
		{
			get { return RandomLine.US_DDTCUnit; }
		}

		public ZPropertyInfo DDTCUnitOfMeasureInfo
		{
			get { return GetZPropertyInfo(Schema.DDTCUnitOfMeasure); }
		}

		public ZDecimal DDTCQuantity => Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.US_DDTCQuantity);

		public ZPropertyInfo DDTCQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.DDTCQuantity); }
		}

		public ZString DDTCCommodityJurisdictionNumber
		{
			get { return RandomLine.US_JurisdictionNumber; }
		}

		public ZPropertyInfo DDTCCommodityJurisdictionNumberInfo
		{
			get { return GetZPropertyInfo(Schema.DDTCCommodityJurisdictionNumber); }
		}
		#endregion

		public ZString AESOriginIndicator
		{
			get { return RandomLine.US_AESOriginIndicator; }
		}

		public ZPropertyInfo AESOriginIndicatorInfo
		{
			get { return GetZPropertyInfo(Schema.AESOriginIndicator); }
		}

		public ZString CL_Calc_EntryNumber
		{
			get { return Header?.EntryNumber ?? ZString.Empty; }
		}

		public ZPropertyInfo CL_Calc_EntryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CL_Calc_EntryNumber); }
		}

		public ZString CL_Calc_XTN
		{
			get { return Header?.US_XTN ?? ZString.Empty; }
		}

		public ZPropertyInfo CL_Calc_XTNInfo
		{
			get { return GetZPropertyInfo(Schema.CL_Calc_XTN); }
		}

		public ZString LicenseNumberAndExemptionCode
		{
			get { return LicenseAndLicenseExemptionTypeManager.GetLicenseNumberOrLicenseExemptionMessage(LicenseType, LicenseNumber, Factory, Declaration.GetEffectiveDateForECR()); }
		}

		#endregion

		#region Overriden Properties

		public override ZGuid US_CL_ParentLine
		{
			get => base.US_CL_ParentLine;
			set
			{
				var oldValue = US_CL_ParentLine;
				base.US_CL_ParentLine = value;
				if (oldValue != value && !IsCopying)
				{
					if (ParentLine is CusEntryLine parentLine && parentLine.US_SupAdditionalLine)
					{
						if (US_SupAdditionalLine2 || US_SupAdditionalLine3 || US_SupAdditionalLine4 || US_SupAdditionalLine5)
						{
							CL_CustomsValue = ZDecimal.Zero;
						}
					}
				}
			}
		}

		public ZString CL_LineNumberFormatted
		{
			get { return CalculateLineNumberFormatted(CL_LineNumber); }
		}

		internal const int EntryLineNumberLength = 3;
		const string CharacterValue = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		//using 35-based numbering system
		const int MaximumOfTwoDigits = 1295;
		const int MaximumOfOneDigit = 35;
		/// <summary>
		/// From 1 to 999, it is 10-based numbering.
		/// From 1000, it is 35-character based numbering using 26 additional alphabet characters. 1000 is A00, A01,,, A09, A0A (this is equal to 1010 (1000 + 10),,, A0Z (this is equal to 1035), A10 (this is equal to 1036))
		/// 'A' in the third digit is evaluated as 10 and 'Z' is 35. Therefore '1' in the second digit is equal to 36.
		/// </summary>
		static ZString CalculateLineNumberFormatted(ZShort number)
		{
			ZString formatted = number.ToString().PadLeft(EntryLineNumberLength, '0');

			if (number > 999)
			{
				var firstDigitValue = (number - 1000).ToZInt();//1000 is base that starts this numbering, A00
				firstDigitValue = firstDigitValue / (MaximumOfTwoDigits + 1);
				if (firstDigitValue > 25)
				{
					return "---";
				}

				var firstDigitChar = CharacterValue[firstDigitValue + 10];//A to Z

				char secondDigitChar;
				var secondDigitValue = number - 1000 - (firstDigitValue * (MaximumOfTwoDigits + 1));
				if (secondDigitValue > MaximumOfTwoDigits)
				{
					return "---";
				}
				else
				{
					secondDigitValue = secondDigitValue / (MaximumOfOneDigit + 1);
					secondDigitChar = CharacterValue[secondDigitValue];
				}

				var thirdDigitValue = number - 1000 - (firstDigitValue * (MaximumOfTwoDigits + 1)) - (secondDigitValue * (MaximumOfOneDigit + 1));
				if (thirdDigitValue > MaximumOfOneDigit)
				{
					return "---";
				}

				var thirdDigitChar = CharacterValue[thirdDigitValue];

				formatted = new ZString(firstDigitChar) + new ZString(secondDigitChar) + new ZString(thirdDigitChar);
			}
			return formatted;
		}

		internal static ZShort GetNumericLineNumber(ZString formattedLineNumber)
		{
			ZShort result;
			if (formattedLineNumber.IsNumbersOnlyOrEmpty)
			{
				result = ZShort.ParseSafe(formattedLineNumber, ZShort.Zero);
			}
			else if (formattedLineNumber.Length != 3)
			{
				result = ZShort.Zero;
			}
			else
			{
				var firstDigit = formattedLineNumber[0];
				var firstDigitValue = (CharacterValue.IndexOf(firstDigit) - 10) * (MaximumOfTwoDigits + 1) + 1000;//10 is A, 1000 is the starting of this new numbering system

				var secondDigit = formattedLineNumber[1];
				var secondDigitValue = CharacterValue.IndexOf(secondDigit) * (MaximumOfOneDigit + 1);

				var thirdDigit = formattedLineNumber[2];
				var thirdDigitValue = CharacterValue.IndexOf(thirdDigit);

				result = (ZShort)(firstDigitValue + secondDigitValue + thirdDigitValue);
			}
			return result;
		}

		protected override void SortInvoiceLines(Customs.Business.InvoiceLinesForEntryLineCollection invoiceLines)
		{
			invoiceLines.Sort((IComparer)new InvoiceLineComparer());
		}

		protected override bool AddInvoiceLinesToEntryInvoiceLines
		{
			get { return !US_SupLine; }
		}

		protected override ZString DescriptionInternal
		{
			get
			{
				var declaration = Declaration;
				if (declaration != null && (declaration.IsInBond || declaration.IsACECargoRelease))
				{
					return RandomLine?.JI_Description ?? ZString.Empty;
				}
				else
				{
					return base.DescriptionInternal;
				}
			}
		}

		public override void RoundCustomsValue()
		{
			//DO NOT ROUND
		}

		public ZDecimal RoundedCustomsValue
		{
			get
			{
				if (roundedCustomsValueCached == null)
				{
					roundedCustomsValueCached = new CachedProperty<ZDecimal>(Factory, Helper.GetRoundedCustomsValue);
				}
				return roundedCustomsValueCached.Value;
			}
		}
		CachedProperty<ZDecimal> roundedCustomsValueCached;

		protected override bool CanBeLinkedUpByPivot
		{
			get
			{
				bool result = false;
				var declaration = Declaration;
				var header = Header;
				if (declaration != null && header != null)
				{
					result = declaration.US_EnableENS && !header.IsFormalEntry;
				}

				result |= US_SupLine;

				return result;
			}
		}

		protected override Customs.Business.TariffFormatter GetTariffFormatter()
		{
			return new TariffFormatter();
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusEntryLineFetchStrategy(this);
		}

		public override void Delete()
		{
			var header = Header;
			if (header != null)
			{
				var parentLine = ParentLine;
				if (parentLine != null)
				{
					parentLine.fChildLines = null;
				}

				foreach (CusEntryLine entryLine in header.MergedLines)
				{
					if (entryLine.US_CL_ParentLine == PK)
					{
						entryLine.US_CL_ParentLine = ZGuid.Empty;
					}
				}
			}

			AESCusDispositions.RemoveAndDeleteAll();

			base.Delete();
		}

		public override void ResetTotalsAndCachedValues()
		{
			base.ResetTotalsAndCachedValues();
			firstInvoiceLineAfterSortedOnInvoiceLineNoCached = null;
			fChildLines = null;
			fRandomLine = null;
			parentLineOrThis = null;
			lineWithCVDDetailsCalculated = false;
			lineWithADDDetailsCalculated = false;

			US_DutyRateDesc = ZString.Empty;
			US_SupCustomsValue = ZDecimal.Zero;
		}

		protected override void DoMergeInvoiceLine(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.DoMergeInvoiceLine(baseInvoiceLine);

			if (baseInvoiceLine is JobComInvoiceLine invoiceLine)
			{
				US_SupCustomsValue = invoiceLine.GetSupCustomsValue(US_SupLine, US_SupAdditionalLine, US_SupAdditionalLine2, US_SupAdditionalLine3, US_SupAdditionalLine4, US_SupAdditionalLine5);
			}
		}

		public override Guid RegistryBranchPK
		{
			get
			{
				if (registryBranchPKCached == null)
				{
					registryBranchPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						return GetRegistryBranchPK();
					});
				}
				return registryBranchPKCached.Value;
			}
		}
		CachedProperty<Guid> registryBranchPKCached;

		Guid GetRegistryBranchPK()
		{
			return base.RegistryBranchPK;
		}

		public override Guid RegistryCompanyPK
		{
			get
			{
				if (registryCompanyPKCached == null)
				{
					registryCompanyPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						return GetRegistryCompanyPK();
					});
				}
				return registryCompanyPKCached.Value;
			}
		}
		CachedProperty<Guid> registryCompanyPKCached;

		Guid GetRegistryCompanyPK()
		{
			return base.RegistryCompanyPK;
		}

		#endregion

		#region New Properties

		public ZBool IsRandomLineSPINotCAAndS
		{
			get
			{
				if (isRandomLineSPINotCAAndS == null)
				{
					isRandomLineSPINotCAAndS = new CachedProperty<ZBool>(Factory, () =>
					{
						var spi = RandomLine.US_SPI;
						return spi != SpecialProgramList.Codes.CA
							&& spi != SpecialProgramList.Codes.S
							&& spi != SpecialProgramList.Codes.SPlus;
					});
				}
				return isRandomLineSPINotCAAndS.Value;
			}
		}
		CachedProperty<ZBool> isRandomLineSPINotCAAndS;

		public ZDate ExportDate
		{
			get { return RandomLine.US_DateOfExport.Date; }
		}

		public ZString EntryLineCode
		{
			get
			{
				if (IsInGlobalCusEntryLineCollection)
				{
					return base.UniqueKey;
				}

				var entryLineNumber = CL_LineNumber.ToString();
				entryLineNumber += IsSecondaryTariffLine ? ", Tariff " + (US_ChildLineNum + 1) : "";

				return "Line " + entryLineNumber;
			}
		}

		public ZString EntryLineDescription
		{
			get
			{
				if (IsInGlobalCusEntryLineCollection)
				{
					return base.UniqueKey;
				}

				var tariff = new TariffFormatter().DisplayFormat(CL_AdValoremTariff);
				var description = CL_Description.IsEmpty ? "" : "(" + CL_Description + ")";
				var originCountry = " from " + ((ICusEntryLine)this).CountryOfOrigin;
				var customsValue = "valued at $" + (CL_CustomsValue > 0 ? CL_CustomsValue.ToStringTrimZeros() : (ZString)"0.00");

				var quantities = (GetQuantityAndUnitDescription(CustomsQuantity, CustomsUnitQty))
							+ (GetQuantityAndUnitDescription(SecondCustomsQuantity, SecondCustomsUnitQty))
							+ (GetQuantityAndUnitDescription(ThirdCustomsQuantity, ThirdCustomsUnitQty));

				return tariff + description + originCountry + " " + customsValue + (string.IsNullOrEmpty(quantities) ? "" : "(" + quantities.TrimEnd(',', ' ') + ")");
			}
		}

		string GetQuantityAndUnitDescription(ZDecimal quantity, ZString uq)
		{
			string result = "";
			if (!uq.IsEmpty)
			{
				string quantityString = quantity > 0 ? quantity.ToStringTrimZeros() : (ZString)"0.00";

				result = quantityString + " " + uq + ", ";
			}
			return result;
		}

		public IEnumerable<ZString> GetRequiredFees()
		{
			var importTariff = ImportTariff;
			if (Declaration != null && Declaration.US_EntryType != EntryTypeList.Codes.TemporaryImportationBond && importTariff != null && !this.TariffHasBabyFomulaAttribute(DateForDutyCalculation))
			{
				if (IsPRCoffeeFeeMandatory)
				{
					yield return Core.Constants.USCustoms.FeeCodes.Coffee;
				}
				if (!IsSetXLine)
				{
					foreach (ZString feeCode in importTariff.GetRequiredFeeCodes())
					{
						if (!CusFeeCodeConstants.IsExciseTax(feeCode) && !IsAMSFeeExempt && !IsRaspberryFeeExempt && (feeCode != Core.Constants.USCustoms.FeeCodes.Cotton || RandomLine.IsACE && !IsCottonFeeExempt))//In ACE, there is no de minimus exemption for cotton fee
						{
							yield return feeCode;
						}
					}
				}
			}
		}

		public bool IsDomesticMerchandise
		{
			get { return RandomLine?.IsDomesticMerchandise ?? false; }
		}

		public bool IsDutyOverriden
		{
			get
			{
				if (RandomLine is JobComInvoiceLine randomLine)
				{
					if (!US_SupLine)
					{
						return randomLine.US_OverrideDuty;
					}
					if (US_SupAdditionalLine)
					{
						return randomLine.US_OverrideSupAdditionalTariff1Duty;
					}
					if (US_SupAdditionalLine2)
					{
						return randomLine.US_OverrideSupAdditionalTariff2Duty;
					}
					if (US_SupAdditionalLine3)
					{
						return randomLine.US_OverrideSupAdditionalTariff3Duty;
					}
					if (US_SupAdditionalLine4)
					{
						return randomLine.US_OverrideSupAdditionalTariff4Duty;
					}
					if (US_SupAdditionalLine5)
					{
						return randomLine.US_OverrideSupAdditionalTariff5Duty;
					}
					return randomLine.US_OverrideSupDuty;
				}
				return false;
			}
		}

		public bool IsSoftwoodLumberSection804FarmBillRequirement
		{
			get
			{
				var invoiceLine = RandomLine;
				bool result = (invoiceLine != null && invoiceLine.IsSoftwoodLumberSection804FarmBillRequirement);
				if (!result)
				{
					foreach (CusEntryLine childLine in ChildLines)
					{
						if (childLine.IsSoftwoodLumberSection804FarmBillRequirement)
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
		}

		public bool IsDutyFreeSPIClaimed
		{
			get { return RandomLine?.IsDutyFreeSPIClaimed ?? false; }
		}

		public bool NoDutyRateExists
		{
			get { return Rates?.IsInvalidDutyRate() ?? false; }
		}

		IRateWrapper Rates
		{
			get { return DutyRateWrapper.GetWrapper(this); }
		}

		public ZDecimal TotalRoundedCustomsValueIncludingSecondaryLines
		{
			get
			{
				ZDecimal result = RoundedCustomsValue;

				foreach (CusEntryLine entryLine in ChildLines)
				{
					if (entryLine.IsSecondaryTariffLine)
					{
						result += entryLine.RoundedCustomsValue;
					}
				}
				return result;
			}
		}

		CachedProperty<ZDecimal> manifestQuantity;
		public ZDecimal ManifestQuantity
		{
			get
			{
				if (manifestQuantity == null)
				{
					manifestQuantity = new CachedProperty<ZDecimal>(Factory, () => Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => (decimal)x.US_ManifestQty));
				}
				return manifestQuantity.Value;
			}
		}

		public CusEntryLine ParentLine
		{
			get
			{
				if (fParentLine == null || fParentLine.IsDeleted || fParentLine.PK != US_CL_ParentLine)
				{
					fParentLine = US_CL_ParentLine.IsValid ? Factory.Load<CusEntryLine>(US_CL_ParentLine) : null;
				}

				return fParentLine;
			}
		}
		CusEntryLine fParentLine;

		public ZBool IsParentLine
		{
			get { return ChildLines.Count > 0; }
		}

		public ZBool IsChildLine
		{
			get { return ParentLine != null; }
		}

		public bool IsChildOf(ZGuid masterBill)
		{
			var result = false;
			var bill = FTZBill?.HighestParentBill;
			if (bill != null)
			{
				result = bill.IsMasterBill && bill.PK == masterBill;
			}

			return result;
		}

		public ZString DestinationState
		{
			get { return RandomLine?.US_DestinationState ?? ZString.Empty; }
		}

		public EntryLineHelper Helper => helper ?? (helper = new EntryLineHelper(this));
		EntryLineHelper helper;

		public IReadOnlyList<CusEntryLine> ChildLines
		{
			get
			{
				if (fChildLines == null)
				{
					fChildLines = Helper.GetChildLines().Cast<CusEntryLine>().ToArray();
				}

				return fChildLines;
			}
		}
		CusEntryLine[] fChildLines;

		internal void RefreshChildLines()
		{
			fChildLines = null;
		}

		public JobComInvoiceLine FirstInvoiceLineAfterSortedOnInvoiceLineNo
		{
			get
			{
				if (firstInvoiceLineAfterSortedOnInvoiceLineNoCached == null)
				{
					firstInvoiceLineAfterSortedOnInvoiceLineNoCached = new CachedValue<JobComInvoiceLine>(() =>
					{
						return (JobComInvoiceLine)Helper.GetFirstInvoiceLineAfterSortedOnInvoiceLineNo();
					});
				}
				return firstInvoiceLineAfterSortedOnInvoiceLineNoCached.Value;
			}
		}
		CachedValue<JobComInvoiceLine> firstInvoiceLineAfterSortedOnInvoiceLineNoCached;

		public ZDecimal Charges
		{
			get
			{
				var result = ZDecimal.Zero;
				if (!IsACEFTZSupLine)
				{
					var chargesUnrounded = ChargesUnrounded;

					result = chargesUnrounded.Round(0);

					if (result == 0)
					{
						if (chargesUnrounded == 0 && ParentLine == null)
						{
							var declaration = Declaration;
							if (declaration != null && declaration.IsChargeMandatory)
							{
								var invoiceHeader = RandomLine?.InvoiceHeader;
								if (invoiceHeader != null && invoiceHeader.JZ_Calc_TNI > 0m)
								{
									if (CL_CustomsValue > invoiceHeader.EnteredValueThresholdForCharges
										|| ChildSecondaryEntryLines.Any(childLine => childLine.CL_CustomsValue > invoiceHeader.EnteredValueThresholdForCharges))
									{
										result = 1m;
									}
								}
							}
						}
						else
						{
							result = Math.Ceiling(chargesUnrounded);
						}
					}
				}

				return result;
			}
		}

		ZDecimal ChargesUnrounded
		{
			get
			{
				var result = ZDecimal.Zero;

				if (IsSupLineOrNormalLine)
				{
					result = TAndIInLocalCurrency.Amount;

					foreach (var childLine in ChildLines)
					{
						if (childLine.RandomLine.PK != RandomLine.PK)
						{
							result += childLine.ChargesUnrounded;
						}
					}
				}

				return result;
			}
		}

		public IReadOnlyList<CusEntryLine> ChildSecondaryEntryLines => ChildLines.Where(x => x.IsSecondaryTariffLine).ToList();

		public ZDate DateForDutyCalculation
		{
			get { return RandomLine.EffectiveDateForDutyRate; }
		}

		public ZBool IsSecondaryTariffLine
		{
			get
			{
				if (isSecondaryTariffLineCached == null)
				{
					isSecondaryTariffLineCached = new CachedProperty<ZBool>(Factory, Helper.GetIsSecondaryTariffLine);
				}
				return isSecondaryTariffLineCached.Value;
			}
		}
		CachedProperty<ZBool> isSecondaryTariffLineCached;

		public ZDecimal CustomsValueRounded
		{
			get { return RoundedCustomsValue; }
		}

		public ZString ChargesRounded
		{
			get
			{
				ZString chargesValue = ZString.Empty;
				var charges = Charges;
				if (charges > 0 && Declaration.TransportMode != TransportTypeList.Codes.PassengerHandCarried)
				{
					chargesValue = "C" + charges.Round(0).ToString();
				}

				return chargesValue;
			}
		}

		public string USComponentsAssembledAbroadDutyRateForPrint(IDutyData dutyData)
		{
			string dutyRate = "";
			AppendixFDutyCalculator childLineCalculator = new AppendixFDutyCalculator(dutyData, Factory);

			if (childLineCalculator.DutyResult.PercentOfValue > 0)
			{
				if (!dutyData.IsCombinedLine() && ImportTariff != null && ImportTariff.Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, DateForDutyCalculation))
				{
					var adjustedCV = TotalCustomsValueIncludingSecondaryLines - dutyData.CustomsValue;
					if (adjustedCV > 0)
					{
						ZDecimal compoundRate = TotalDutyIncludingSecondaryLines / adjustedCV * 100;
						dutyRate = compoundRate.Round(2).ToString() + "%";
					}
					else
					{
						dutyRate = "0%";
					}
				}
				else
				{
					dutyRate = childLineCalculator.DutyResult.PercentOfValue.ToString(childLineCalculator.DutyResult.PercentOfValue.DecimalPlaces) + "%";
				}
			}
			else
			{
				dutyRate = IFeeCalculationDataProviderExtensionMethods.AmountPerUnit(childLineCalculator.DutyResult.PerUnitAmount.Amount, childLineCalculator.DutyResult.PerUnitUQ);
			}

			return dutyRate;
		}

		public ZString CL_DutyPercentAsString
		{
			get { return US_DutyRateDesc; }
		}

		public override ZString US_DutyRateDesc
		{
			get
			{
				if (uS_DutyRateDescCached == null)
				{
					uS_DutyRateDescCached = new CachedProperty<ZString>(Factory, Helper.GetDutyRateDesc);
				}
				return uS_DutyRateDescCached.Value;
			}
			set
			{
				base.US_DutyRateDesc = value;
			}
		}
		CachedProperty<ZString> uS_DutyRateDescCached;

		public OrgHeader UltimateConsigneeForCargoRelease
		{
			get { return RandomLine?.ConsigneeOrgAddress; }
		}

		/// <summary>
		/// Encrypted number is also accepted for the purpose of Cargo Release
		/// </summary>
		public ZString UltimateConsigneeNumberForCargoRelease
		{
			get { return OrgHeaderWrapper.GetCustomsRelatedCode(UltimateConsigneeForCargoRelease, OrgMatchedCustomsRegNoType.ECN); }
		}

		public ZDecimal ParentChildLineDuty
		{
			get
			{
				var calculator = AppendixFDutyCalculator.NewWithCombinedCustomsValue(this, ParentLine);
				return calculator.DutyResult.TotalAmount.Amount;
			}
		}

		internal bool IsRelevantForLinePriceBoundaryCheck
		{
			get
			{
				var entryHeader = Header;
				return entryHeader != null && (entryHeader.IsFormalEntry || entryHeader.IsFTZAdmission);
			}
		}

		#endregion

		#region NAFTA Duty Deferral
		ZString ICusEntryLine.ImportFTZNumber
		{
			get { return RandomLine.Declaration.US_FTZNo; }
		}

		ZString ICusEntryLine.ImportTariffCode
		{
			get { return CL_AdValoremTariff; }
		}

		ZString ICusEntryLine.ExportTariffCode
		{
			get { return RandomLine.US_ExportTariff; }
		}

		ZString ICusEntryLine.NAFTATariff
		{
			get { return RandomLine.US_NAFTATariff; }
		}

		ZDecimal ICusEntryLine.NAFTADutyRate
		{
			get { return RandomLine.US_NAFTADutyRate; }
		}

		ZDecimal ICusEntryLine.NAFTADutyFGN
		{
			get { return RandomLine.US_NAFTADutyFGN; }
		}

		ZDecimal ICusEntryLine.NAFTADutyUS
		{
			get { return RandomLine.US_NAFTADutyUS; }
		}

		#endregion

		#region ICusEntryLine Members

		ZBool ICusEntryLine.IsSupLine
		{
			get { return US_SupLine; }
		}

		ZDecimal Customs.Business.ICusEntryLine.CL_CustomsValue
		{
			get
			{
				var result = RoundedCustomsValue;
				var invoiceLine = RandomLine;
				if (invoiceLine != null)
				{
					var dutyData = this as IDutyData;
					if ((CalculateDutyForSetsHelper.IsCombinedXLine(invoiceLine) && !dutyData.IsSecondaryTariffLine) || (IsChildLine && IsSetVLine && CalculateDutyForSetsHelper.Is9903Tariff(invoiceLine.US_SupTariff)))
					{
						result = ZDecimal.Zero;
					}
				}
				return result.Round(0);
			}
		}

		ZDecimal ICusEntryLine.Charges
		{
			get { return Charges; }
		}

		IEnumerable<ISecondaryTariffLine> ICusEntryLine.SecondaryTariffLines
		{
			get
			{
				List<ISecondaryTariffLine> result = new List<ISecondaryTariffLine>();
				foreach (CusEntryLine entryLine in ChildSecondaryEntryLines)
				{
					var shouldRemoveEmpty = entryLine.CL_AdValoremTariff.IsEmpty && this.IsCombinedLine();
					if (!shouldRemoveEmpty)
					{
						result.Add(new SecondaryTariffLineWrapper(entryLine));
					}
				}
				return result;
			}
		}

		ICusEntryLine ICusEntryLine.ParentLine
		{
			get { return ParentLine; }
		}

		ZString ICusEntryLine.CountryOfOrigin
		{
			get { return RandomLine?.US_UC_NKCountryOfOrigin ?? ZString.Empty; }
		}

		bool MayRequireFDAData
		{
			get { return ImportTariff?.HasFDARequirement ?? false; }
		}

		IList<IPriorNoticeLine> IOGA.FDA
		{
			get
			{
				List<IPriorNoticeLine> result = new List<IPriorNoticeLine>();

				// do not report a FDA value for sup lines if its secondary line's FDA indicator is also D
				bool omitFDAValue = false;
				if (US_SupLine && OGAIndicatorList.IsToBeDeclared(((IOGA)this).FDAIndicator))
				{
					CusEntryLine nonSupLine = RandomLine.GetEntryLineFor(Header.CH_MessageType, false);

					omitFDAValue = nonSupLine != null && OGAIndicatorList.IsToBeDeclared(((IOGA)nonSupLine).FDAIndicator);
				}

				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					foreach (FDA fda in invoiceLine.FDAs)
					{
						result.Add(new PriorNoticeLine(fda, !omitFDAValue));
					}
				}

				return result;
			}
		}

		ZString IOGA.FDAIndicator
		{
			get
			{
				var invoiceLine = RandomLine;
				var result = invoiceLine?.US_FDAIndicator ?? ZString.Empty;

				if (!result.IsEmpty && !MayRequireFDAData)
				{
					if (OGAIndicatorList.IsToBeDisclaimed(result))
					{
						result = ZString.Empty;
					}
					else
					{
						var supTariffLine = invoiceLine.GetEntryLineFor(Header.CH_MessageType, true);

						if (US_SupLine || supTariffLine != null && supTariffLine.MayRequireFDAData)
						{
							result = ZString.Empty;
						}
					}
				}
				return result;
			}
		}

		bool MayRequireDOTData
		{
			get { return ImportTariff?.HasDOTRequirement ?? false; }
		}

		IEnumerable<IDOT> IOGA.DOT
		{
			get
			{
				var nonSupTariffLine = RandomLine?.GetEntryLineFor(Header.CH_MessageType, false);

				// VIN is sent against a non-sup tariff line if DOT is required for the line. If not required for the line, it is sent against sup line.
				bool sendVIN = !US_SupLine || (nonSupTariffLine != null && !nonSupTariffLine.MayRequireDOTData);
				bool mightNeedToSendDummyVIN = !sendVIN && US_SupLine && nonSupTariffLine != null && CL_AdValoremTariff.StartsWith("98") && nonSupTariffLine.MayRequireDOTData;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					foreach (DOT dot in invoiceLine.DOTs)
					{
						if (mightNeedToSendDummyVIN && dot.US_DOTClarCode == ClarificationCodeList.Codes.Vehicle)
						{
							yield return new DOTIDOT(dot, true, Math.Max((short)0, CL_LineNumber - 1));
						}
						else
						{
							yield return new DOTIDOT(dot, sendVIN);
						}
					}
				}
			}
		}

		ZString IOGA.DOTIndicator
		{
			get
			{
				var invoiceLine = RandomLine;
				var result = invoiceLine?.US_DOTIndicator ?? ZString.Empty;

				if (!result.IsEmpty && !MayRequireDOTData)
				{
					if (OGAIndicatorList.IsToBeDisclaimed(result))
					{
						result = ZString.Empty;
					}
					else
					{
						var supTariffLine = invoiceLine.GetEntryLineFor(Header.CH_MessageType, true);

						if (US_SupLine || supTariffLine != null && supTariffLine.MayRequireDOTData)
						{
							result = ZString.Empty;
						}
					}
				}
				return result;
			}
		}

		ZDecimal ICusEntryLine.GrossWeightInKilograms
		{
			get { return GetGrossWeightInKilograms(IsSupLineOrNormalLine); }
		}

		ZDecimal GetGrossWeightInKilograms(bool calcValue)
		{
			var result = ZDecimal.Zero;

			if (calcValue)
			{
				result += Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.GrossWeightInKG);

				if (!US_SupLine || IsParentLine && !this.IsCombinedLine())
				{
					foreach (ISecondaryTariffLine secondaryTariffLine in ((ICusEntryLine)this).SecondaryTariffLines)
					{
						result += secondaryTariffLine.GrossWeightInKilograms;
					}
					if (IsVParentLine)
					{
						foreach (var line in ChildVLines)
						{
							if (!line.US_SupLine || line.IsParentLine && !this.IsCombinedLine())
							{
								foreach (ISecondaryTariffLine secondaryTariffLine in ((ICusEntryLine)line).SecondaryTariffLines)
								{
									result += secondaryTariffLine.GrossWeightInKilograms;
								}
							}
						}
					}
				}
			}
			return result.Round(0);
		}

		public ZDecimal ValueForADD
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				var randomLine = RandomLine;
				var idutydata = this as IDutyData;
				var tariff = idutydata.Tariff;
				if ((randomLine != null && !randomLine.US_ADDCaseNo.IsEmpty && (IsSupLineOrNormalLine || randomLine.IsDerivedSetsPrentLine())) || (idutydata.IsCombinedLine() && !Chapter98Helper.Is99Tariff(tariff)))
				{
					if (randomLine.US_ADDDepositValue > 0)
					{
						result += Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.ValueForADD);
					}
					else if (!Chapter98Helper.Is98Tariff(tariff) || (randomLine.ChildLines.All(x => x.US_ADDDepositValue.IsEmpty) && (randomLine.ParentTariffLine?.US_ADDDepositValue.IsEmpty ?? true)))
					{
						result = CL_CustomsValue;
						var header = Header;
						if (US_SupLine && header != null && !idutydata.IsCombinedLine())
						{
							var normalLine = randomLine.GetEntryLineFor(header.CH_MessageType, false);
							if (normalLine != null)
							{
								result += normalLine.CL_CustomsValue;
							}
						}
					}
				}

				return result.Round(0);
			}
		}

		public ZDecimal ValueForCVD
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				var randomLine = RandomLine;
				var idutydata = this as IDutyData;
				var tariff = idutydata.Tariff;
				if ((randomLine != null && !randomLine.US_CVDCaseNo.IsEmpty && (IsSupLineOrNormalLine || randomLine.IsDerivedSetsPrentLine())) || (idutydata.IsCombinedLine() && !Chapter98Helper.Is99Tariff(tariff)))
				{
					if (randomLine.US_CVDDepositValue > 0)
					{
						result += Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.ValueForCVD);
					}
					else if (!Chapter98Helper.Is98Tariff(tariff) || (randomLine.ChildLines.All(x => x.US_CVDDepositValue.IsEmpty) && (randomLine.ParentTariffLine?.US_CVDDepositValue.IsEmpty ?? true)))
					{
						result = CL_CustomsValue;
						var header = Header;
						if (US_SupLine && header != null && !idutydata.IsCombinedLine())
						{
							var normalLine = randomLine.GetEntryLineFor(header.CH_MessageType, false);
							if (normalLine != null)
							{
								result += normalLine.CL_CustomsValue;
							}
						}
					}
				}

				return result.Round(0);
			}
		}

		public ZDecimal ADDQuantity => Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.US_ADDQty);

		public ZDecimal CVDQuantity => Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.US_CVDQty);

		/// <summary>
		/// Declared in ENS40. This should return a secondary line's value for a parent if exists
		/// </summary>
		public ZDecimal ADDSpecificDepositValue
		{
			get
			{
				ZDecimal result = 0m;

				if (InvoiceLineWithADDDetails != null)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceLineWithADDDetails.CusEntryLine.InvoiceLines)
					{
						if (invoiceLine.JI_Tariff == InvoiceLineWithADDDetails.JI_Tariff)//due to derived duty calculation
						{
							result += invoiceLine.ADDDepositValueInLocalCurrency;
						}
					}
				}

				return result > 0 && result < 1 ? 1 : result.Round(0);
			}
		}

		/// <summary>
		/// Declared in ENS40. This should return a secondary line's value for a parent if exists
		/// </summary>
		public ZDecimal CVDSpecificDepositValue
		{
			get
			{
				ZDecimal result = 0m;

				if (InvoiceLineWithCVDDetails != null)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceLineWithCVDDetails.CusEntryLine.InvoiceLines)
					{
						if (invoiceLine.JI_Tariff == InvoiceLineWithCVDDetails.JI_Tariff)//due to derived duty calculation
						{
							result += invoiceLine.CVDDepositValueInLocalCurrency;
						}
					}
				}

				return result > 0 && result < 1 ? 1 : result.Round(0);
			}
		}

		public ZString ADDCaseRateTypeQualifier
		{
			get { return InvoiceLineWithADDDetails?.US_ADDDepositRateIndicator ?? ZString.Empty; }
		}

		public ZString CVDCaseRateTypeQualifier
		{
			get { return InvoiceLineWithCVDDetails?.US_CVDDepositRateIndicator ?? ZString.Empty; }
		}

		ZString ICusEntryLine.PortOfLading
		{
			get { return RandomLine?.US_SchDLoading ?? ZString.Empty; }
		}

		ZString ICusEntryLine.ZoneStatus
		{
			get { return RandomLine?.US_ZoneStatus ?? ZString.Empty; }
		}

		ZDate ICusEntryLine.PrivilegedStatusFilingDate
		{
			get { return RandomLine?.US_PrivilegedStatusDate.Date ?? ZDate.Empty; }
		}

		ZInt ICusEntryLine.FTZLineItemQuantity
		{
			get
			{
				var result = ZInt.Zero;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result += invoiceLine.US_ManifestQty;
				}
				return result;
			}
		}

		ZBool ICusEntryLine.NAFTANetCostIndicator
		{
			get { return RandomLine?.US_IsNAFTANet ?? ZBool.False; }
		}

		public USCTariff ImportTariff
		{
			get
			{
				ZDate dateForDutyCalculation = DateForDutyCalculation;
				return Factory.GetCachedValue(CL_AdValoremTariff.PadRight(USCTariff.Schema.UE_TariffMaxLength) + dateForDutyCalculation.ToString(),
					delegate
					{
						return new USCTariff.Loader(Factory).LoadBestMatch(CL_AdValoremTariff, dateForDutyCalculation);
					});
			}
		}

		ZString ICusEntryLine.PreImportationReviewProgramRulingsType
		{
			get { return RandomLine?.US_PIRPRulingType ?? ZString.Empty; }
		}

		ZString ICusEntryLine.PreImportationReviewProgramRulingsNumber
		{
			get { return RandomLine?.US_PIRPRulingNo ?? ZString.Empty; }
		}

		IEnumerable<ZString> ICusEntryLine.CommercialDescriptions
		{
			get
			{
				Dictionary<ZString, bool> commercialDescriptions = new Dictionary<ZString, bool>();
				foreach (JobComInvoiceLine invoiceline in InvoiceLines)
				{
					AddCommercialDescriptionIfNeeded(commercialDescriptions, invoiceline);
					foreach (JobComInvoiceLine childLine in invoiceline.SecondaryTariffLines)
					{
						AddCommercialDescriptionIfNeeded(commercialDescriptions, childLine);
					}
				}
				return commercialDescriptions.Keys;
			}
		}

		void AddCommercialDescriptionIfNeeded(Dictionary<ZString, bool> commercialDescriptions, JobComInvoiceLine invoiceline)
		{
			if (invoiceline.IsValidForAII)
			{
				commercialDescriptions[invoiceline.CommercialDescription] = true;
			}
		}

		public ZString SpecialProgramsIndicatorPrimary
		{
			get
			{
				ZString result = ZString.Empty;

				var invoiceLine = RandomLine;

				if (invoiceLine != null && Factory.GetCachedValue<PrimarySpecProgramIndicatorList>().ContainsCode(invoiceLine.US_SPI))
				{
					result = invoiceLine.US_SPI;
				}

				return result;
			}
		}

		ZDecimal ICusEntryLine.Quantity1
		{
			get { return GetCustomsQuantity1Rounded(); }
		}

		ZDecimal GetCustomsQuantity1Rounded()
		{
			if (customsQuantity1RoundedCached == null)
			{
				customsQuantity1RoundedCached = new CachedProperty<ZDecimal>(Factory, () =>
				{
					var result = ZDecimal.Zero;
					result = RoundedQuantity.GetRoundedQuantity1(this, CustomsQuantity);
					if (result == ZDecimal.Zero && CustomsUnitQty == ABIUnitOfMeasureList.Codes.Dozen && CustomsQuantity > ZDecimal.Zero)
					{
						result = 1;
					}
					return result;
				});
			}

			return customsQuantity1RoundedCached.Value;
		}
		CachedProperty<ZDecimal> customsQuantity1RoundedCached;

		protected override ZDecimal GetCustomsQuantity()
		{
			return Helper.GetCustomsQuantity();
		}

		protected override ZString GetCustomsUnitQty()
		{
			if (customsUnitQtyCached == null)
			{
				customsUnitQtyCached = new CachedProperty<ZString>(Factory, Helper.GetCustomsUnitQty);
			}

			return customsUnitQtyCached.Value;
		}
		CachedProperty<ZString> customsUnitQtyCached;

		ZString ICusEntryLine.UnitOfMeasure1
		{
			get { return CustomsUnitQty; }
		}

		ZDecimal ICusEntryLine.Quantity2
		{
			get { return GetSecondCustomsQuantityRounded(); }
		}

		ZDecimal GetSecondCustomsQuantityRounded()
		{
			if (secondCustomsQuantityRoundedCached == null)
			{
				secondCustomsQuantityRoundedCached = new CachedProperty<ZDecimal>(Factory, () => RoundedQuantity.GetRoundedQuantity2(this, SecondCustomsQuantity));
			}

			return secondCustomsQuantityRoundedCached.Value;
		}
		CachedProperty<ZDecimal> secondCustomsQuantityRoundedCached;

		ZString ICusEntryLine.UnitOfMeasure2
		{
			get { return SecondCustomsUnitQty; }
		}

		ZDecimal ICusEntryLine.Quantity3
		{
			get
			{
				if (quantity3Cached == null)
				{
					quantity3Cached = new CachedProperty<ZDecimal>(Factory, () => RoundedQuantity.GetRoundedQuantity3(this, ThirdCustomsQuantity));
				}

				return quantity3Cached.Value;
			}
		}
		CachedProperty<ZDecimal> quantity3Cached;

		ZString ICusEntryLine.UnitOfMeasure3
		{
			get { return ThirdCustomsUnitQty; }
		}

		ZString ICusEntryLine.CountryOfExport
		{
			get { return RandomLine?.US_UC_NKCountryOfExport ?? ZString.Empty; }
		}

		ZDate ICusEntryLine.DateOfExportation
		{
			get { return RandomLine?.US_DateOfExport.Date ?? ZDate.Empty; }
		}

		ZBool ICusEntryLine.RelatedPartyIndicator
		{
			get { return (RandomLine?.US_TransactionsRelated ?? ZString.Empty) == YesNoDefaultList.Codes.Yes; }
		}

		public ZString SpecialProgramsIndicatorCountry
		{
			get
			{
				if (specialProgramsIndicatorCountryCached == null)
				{
					specialProgramsIndicatorCountryCached = new CachedProperty<ZString>(Factory, () =>
					{
						var result = ZString.Empty;
						var invoiceLine = RandomLine;

						if (invoiceLine != null && invoiceLine.US_SPI != SPICompleteList.MoreCodes.NotApplicable
							&& !Factory.GetCachedValue<PrimarySpecProgramIndicatorList>().ContainsCode(invoiceLine.US_SPI))
						{
							result = invoiceLine.US_SPI;
						}

						return result;
					});
				}

				return specialProgramsIndicatorCountryCached.Value;
			}
		}
		CachedProperty<ZString> specialProgramsIndicatorCountryCached;

		ZString ICusEntryLine.SpecialProgramsIndicatorSecondary
		{
			get
			{
				var invoiceLine = RandomLine;
				ZString result = invoiceLine.US_SecondarySPI;

				if (invoiceLine.IsSetXLine || invoiceLine.IsSetVLine)
				{
					if (IsSecondaryTariffLine)
					{
						result = ZString.Empty;
					}
				}
				return result;
			}
		}

		ZDate ICusEntryLine.DateOfExportationFromCountryOfOrigin
		{
			get
			{
				var invoiceLine = RandomLine;
				var result = invoiceLine?.US_DateOfExportFromCountryOfOrigin.Date ?? ZDate.Empty;

				if (result.IsEmpty)
				{
					var childSecondaryEntryLines = ChildSecondaryEntryLines;
					if (childSecondaryEntryLines.Count == 1)
					{
						result = ((ICusEntryLine)childSecondaryEntryLines[0]).DateOfExportationFromCountryOfOrigin;
					}
				}

				if (result.IsEmpty && invoiceLine.IsVParentLine)
				{
					foreach (var line in invoiceLine.ChildVLines)
					{
						if (!line.US_DateOfExportFromCountryOfOrigin.IsEmpty)
						{
							result = line.US_DateOfExportFromCountryOfOrigin.Date;
							break;
						}
					}
				}

				return result;
			}
		}

		public ZString VisaNumber
		{
			get
			{
				var invoiceLine = RandomLine;
				var result = invoiceLine?.US_VisaNo ?? ZString.Empty;

				if (result.IsEmpty)
				{
					var childSecondaryEntryLines = ChildSecondaryEntryLines;
					if (childSecondaryEntryLines.Count == 1)
					{
						result = childSecondaryEntryLines[0].VisaNumber;
					}
				}

				if (result.IsEmpty)
				{
					result = GetEffectiveFromChildLines(invoiceLine, line => line.US_VisaNo);
				}

				return result;
			}
		}

		ZString GetEffectiveFromChildLines(JobComInvoiceLine invoiceLine, Func<JobComInvoiceLine, ZString> infoGetter)
		{
			if (invoiceLine.IsVParentLine)
			{
				foreach (var line in invoiceLine.ChildVLines)
				{
					var result = infoGetter(line);
					if (!result.IsEmpty)
					{
						return result;
					}
				}
			}
			return ZString.Empty;
		}

		ZString ICusEntryLine.TextileCategoryNumber
		{
			get
			{
				var result = ZString.Empty;
				var childSecondaryEntryLines = ChildSecondaryEntryLines;
				if (childSecondaryEntryLines.Count == 1)
				{
					result = childSecondaryEntryLines[0].RandomLine.US_TextileCategoryNo;
				}

				if (result.IsEmpty)
				{
					result = RandomLine.US_TextileCategoryNo;
				}

				if (result.IsEmpty)
				{
					result = GetEffectiveFromChildLines(RandomLine, line => line.US_TextileCategoryNo);
				}
				return result;
			}
		}

		ZDecimal ICusEntryLine.VisaQuantity
		{
			get
			{
				var result = Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.US_VisaQty);

				if (result.IsEmpty)
				{
					var childSecondaryEntryLines = ChildSecondaryEntryLines;
					if (childSecondaryEntryLines.Count == 1)
					{
						result = ((ICusEntryLine)childSecondaryEntryLines[0]).VisaQuantity;
					}
				}

				return result;
			}
		}

		ZString ICusEntryLine.VisaUnitOfMeasure
		{
			get
			{
				var invoiceLine = RandomLine;
				var result = invoiceLine?.US_VisaUQ ?? ZString.Empty;

				if (result.IsEmpty)
				{
					var childSecondaryEntryLines = ChildSecondaryEntryLines;
					if (childSecondaryEntryLines.Count == 1)
					{
						result = ((ICusEntryLine)childSecondaryEntryLines[0]).VisaUnitOfMeasure;
					}
				}

				if (result.IsEmpty)
				{
					result = GetEffectiveFromChildLines(invoiceLine, line => line.US_VisaUQ);
				}

				return result;
			}
		}

		ZString ICusEntryLine.AgricultureLicenseNumber
		{
			get
			{
				var invoiceLine = RandomLine;
				var result = invoiceLine?.US_AgricultureLicNo ?? ZString.Empty;

				if (result.IsEmpty)
				{
					var childSecondaryEntryLines = ChildSecondaryEntryLines;
					if (childSecondaryEntryLines.Count == 1)
					{
						result = ((ICusEntryLine)childSecondaryEntryLines[0]).AgricultureLicenseNumber;
					}
				}

				if (result.IsEmpty)
				{
					result = GetEffectiveFromChildLines(invoiceLine, line => line.US_AgricultureLicNo);
				}

				return result;
			}
		}

		ZString ICusEntryLine.CottonCertificateNumberOrganicExemptionCertificateNumber
		{
			get
			{
				var invoiceLine = RandomLine;
				var result = invoiceLine != null ? GetCottonCertificateNoFromSingleInvoiceLine(invoiceLine) : ZString.Empty;

				if (result.IsEmpty)
				{
					var childSecondaryEntryLines = ChildSecondaryEntryLines;
					if (childSecondaryEntryLines.Count == 1)
					{
						result = ((ICusEntryLine)childSecondaryEntryLines[0]).CottonCertificateNumberOrganicExemptionCertificateNumber;
					}
				}

				if (result.IsEmpty)
				{
					result = GetEffectiveFromChildLines(invoiceLine, line => GetCottonCertificateNoFromSingleInvoiceLine(line));
				}

				return result;
			}
		}

		ZString GetCottonCertificateNoFromSingleInvoiceLine(JobComInvoiceLine line)
		{
			var cottonCertificateNo = ZString.Empty;
			if (line.ImportTariff != null && line.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton))
			{
				if (CottonAmount == 0 && new CottonFeeCalculator(false, true).CalculateFee(this).Amount >= CottonFeeCalculator.ThresholdCottonFeeAmount && ((IDutyDataLineHeader)Header).IsCottonFeeDeMinimusApplicable)
				{
					if (line.IsCottonFeeExemptIndicated)
					{
						cottonCertificateNo = CottonFeeCalculator.ExemptCottonFeeCertificate;
					}
					else
					{
						cottonCertificateNo = line.US_CottonCertificateNo;
					}
				}
			}
			else
			{
				cottonCertificateNo = line.US_CottonCertificateNo;
			}
			return cottonCertificateNo;
		}

		ZString ICusEntryLine.ChinaHongKongSWPMIndicator
		{
			get
			{
				var invoiceLine = RandomLine;
				var result = ZString.Empty;

				if (invoiceLine != null)
				{
					var effectiveCountryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin;
					if (effectiveCountryOfOrigin == Core.Constants.CountryCodes.HongKong || effectiveCountryOfOrigin == Core.Constants.CountryCodes.China)
					{
						result = invoiceLine.US_SWPMIndicator;
					}
				}

				if (result.IsEmpty)
				{
					var childSecondaryEntryLines = ChildSecondaryEntryLines;
					if (childSecondaryEntryLines.Count == 1)
					{
						result = ((ICusEntryLine)childSecondaryEntryLines[0]).ChinaHongKongSWPMIndicator;
					}
				}

				if (result.IsEmpty)
				{
					result = GetEffectiveFromChildLines(invoiceLine, line =>
					{
						var swpmIndicator = ZString.Empty;
						var effectiveCountryOfOrigin = line.US_UC_NKCountryOfOrigin;
						if (!line.US_CBTPACertificateNo.IsEmpty & (effectiveCountryOfOrigin == Core.Constants.CountryCodes.HongKong || effectiveCountryOfOrigin == Core.Constants.CountryCodes.China))
						{
							swpmIndicator = line.US_SWPMIndicator;
						}
						return swpmIndicator;
					});
				}

				return result;
			}
		}

		ZString ICusEntryLine.CanadianExportCertificateSugar
		{
			get
			{
				var invoiceLine = RandomLine;
				var result = invoiceLine?.US_CAExportCertificate ?? ZString.Empty;

				if (result.IsEmpty)
				{
					var childSecondaryEntryLines = ChildSecondaryEntryLines;
					if (childSecondaryEntryLines.Count == 1)
					{
						result = ((ICusEntryLine)childSecondaryEntryLines[0]).CanadianExportCertificateSugar;
					}
				}

				if (result.IsEmpty)
				{
					result = GetEffectiveFromChildLines(invoiceLine, line => line.US_CAExportCertificate);
				}

				return result;
			}
		}

		ZString ICusEntryLine.WoolLicense
		{
			get
			{
				var invoiceLine = RandomLine;
				var result = invoiceLine?.US_WoolLicenceNo ?? ZString.Empty;

				if (result.IsEmpty)
				{
					var childSecondaryEntryLines = ChildSecondaryEntryLines;
					if (childSecondaryEntryLines.Count == 1)
					{
						result = ((ICusEntryLine)childSecondaryEntryLines[0]).WoolLicense;
					}
				}

				if (result.IsEmpty)
				{
					result = GetEffectiveFromChildLines(invoiceLine, line => line.US_WoolLicenceNo);
				}

				return result;
			}
		}

		ZString ICusEntryLine.CBTPACertificationNumber
		{
			get
			{
				var randomLine = RandomLine;
				var result = randomLine?.US_CBTPACertificateNo ?? ZString.Empty;

				if (result.IsEmpty)
				{
					var childSecondaryEntryLines = ChildSecondaryEntryLines;
					if (childSecondaryEntryLines.Count == 1)
					{
						result = ((ICusEntryLine)childSecondaryEntryLines[0]).CBTPACertificationNumber;
					}
				}

				if (result.IsEmpty)
				{
					result = GetEffectiveFromChildLines(randomLine, line => line.US_CBTPACertificateNo);
				}

				return result;
			}
		}

		ZString ICusEntryLine.MiscellaneousPermitLicenseNumber
		{
			get
			{
				var randomLine = RandomLine;
				var result = randomLine?.US_MiscPermitNo ?? ZString.Empty;

				if (result.IsEmpty)
				{
					var childSecondaryEntryLines = ChildSecondaryEntryLines;
					if (childSecondaryEntryLines.Count == 1)
					{
						result = ((ICusEntryLine)childSecondaryEntryLines[0]).MiscellaneousPermitLicenseNumber;
					}
				}

				if (result.IsEmpty)
				{
					result = GetEffectiveFromChildLines(randomLine, line => line.US_MiscPermitNo);
				}

				return result;
			}
		}

		ZBool ICusEntryLine.IsSoftwoodLumberLine
		{
			get { return IsSoftwoodLumberSection804FarmBillRequirement; }
		}

		ZBool ICusEntryLine.IsSoftwoodLumberImporterDeclaration
		{
			get
			{
				ZBool result = ZBool.False;

				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					if (invoiceLine.US_LumberImporterDeclaration == YesNoDefaultList.Codes.Yes)
					{
						result = ZBool.True;
						break;
					}
				}

				if (!result)
				{
					foreach (ICusEntryLine entryLine in ChildSecondaryEntryLines)
					{
						if (entryLine.IsSoftwoodLumberImporterDeclaration)
						{
							result = ZBool.True;
							break;
						}
					}
				}
				return result;
			}
		}

		ZDecimal ICusEntryLine.SoftwoodLumberExportPrice
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (IsSupLineOrNormalLine)
				{
					result += Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.US_LumberExportPrice);

					foreach (ICusEntryLine entryLine in ChildSecondaryEntryLines)
					{
						if (RandomLine != null && entryLine.RandomLine != null && entryLine.RandomLine.PK != RandomLine.PK)
						{
							result += entryLine.SoftwoodLumberExportPrice;
						}
					}
				}
				return result;
			}
		}

		ZDecimal ICusEntryLine.SoftwoodLumberExportCharges
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (IsSupLineOrNormalLine)
				{
					result += Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.US_LumberExportCharges);

					foreach (ICusEntryLine entryLine in ChildSecondaryEntryLines)
					{
						if (RandomLine != null && entryLine.RandomLine != null && entryLine.RandomLine.PK != RandomLine.PK)
						{
							result += entryLine.SoftwoodLumberExportCharges;
						}
					}
				}

				return result;
			}
		}

		/// <summary>
		/// Countervailing Duty including secondary lines' amounts
		/// </summary>
		public ZDecimal CountervailingDuty
		{
			get
			{
				ZDecimal result = Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty);

				foreach (CusEntryLine secondaryLine in ChildSecondaryEntryLines)
				{
					result += secondaryLine.CountervailingDuty;
				}

				return result;
			}
		}

		/// <summary>
		/// Antidumping Duty including secondary lines' amounts
		/// </summary>
		public ZDecimal AntidumpingDuty
		{
			get
			{
				ZDecimal result = Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty);

				foreach (CusEntryLine secondaryLine in ChildSecondaryEntryLines)
				{
					result += secondaryLine.AntidumpingDuty;
				}

				return result;
			}
		}

		public CusEntryLine ParentLineOrThis
		{
			get { return parentLineOrThis ?? (parentLineOrThis = ParentLine ?? this); }
		}
		CusEntryLine parentLineOrThis;

		/// <summary>
		/// Total Duty including secondary lines amounts
		/// </summary>
		public ZDecimal TotalDutyIncludingSecondaryLines
		{
			get
			{
				if (totalDutyIncludingSecondaryLinesCached == null)
				{
					totalDutyIncludingSecondaryLinesCached = new CachedProperty<ZDecimal>(Factory, Helper.GetTotalDutyIncludingSecondaryLines);
				}
				return totalDutyIncludingSecondaryLinesCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalDutyIncludingSecondaryLinesCached;

		/// <summary>
		/// Total Customs value including secondary lines amounts
		/// </summary>
		public ZDecimal TotalCustomsValueIncludingSecondaryLines
		{
			get
			{
				if (totalCustomsValueIncludingSecondaryLinesCached == null)
				{
					totalCustomsValueIncludingSecondaryLinesCached = new CachedProperty<ZDecimal>(Factory, Helper.GetTotalCustomsValueIncludingSecondaryLines);
				}
				return totalCustomsValueIncludingSecondaryLinesCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalCustomsValueIncludingSecondaryLinesCached;

		public ZString CountervailingCaseNumber
		{
			get { return InvoiceLineWithCVDDetails?.US_CVDCaseNo ?? ZString.Empty; }
		}

		public ZString AntidumpingCaseNumber
		{
			get { return InvoiceLineWithADDDetails?.US_ADDCaseNo ?? ZString.Empty; }
		}

		/// <summary>
		/// Percentage
		/// </summary>
		public ZDecimal CVDDepositRate
		{
			get { return InvoiceLineWithCVDDetails?.US_CVDDepositRate ?? ZDecimal.Zero; }
		}

		/// <summary>
		/// Percentage
		/// </summary>
		public ZDecimal ADDDepositRate
		{
			get { return InvoiceLineWithADDDetails?.US_ADDDepositRate ?? ZDecimal.Zero; }
		}

		ZBool ICusEntryLine.BondedCountervailingDuty
		{
			get { return InvoiceLineWithCVDDetails?.US_IsBondedCVD ?? ZBool.False; }
		}

		ZBool ICusEntryLine.BondedAntidumpingDuty
		{
			get { return InvoiceLineWithADDDetails?.US_IsBondedADD ?? ZBool.False; }
		}

		/// <summary>
		/// From this invoice line, case number, deposit rate and whether bonded or not are returned and consumed by messaging layer
		/// For that reason, if CVD is entered for information only, this returns null object even though the details is entered
		/// </summary>
		JobComInvoiceLine InvoiceLineWithCVDDetails
		{
			get
			{
				if (!lineWithCVDDetailsCalculated)
				{
					lineWithCVDDetailsCalculated = true;
					invoiceLineWithCVDDetails = this.GetInvoiceLineWithADD_CVDDetailsIncludingSecondaryLines(USAddInfoSchema.Constants.US_CVDCaseNo);
				}
				return invoiceLineWithCVDDetails;
			}
		}
		JobComInvoiceLine invoiceLineWithCVDDetails;
		bool lineWithCVDDetailsCalculated;

		/// <summary>
		/// From this invoice line, case number, deposit rate and whether bonded or not are returned and consumed by messaging layer
		/// For that reason, if ADD is entered for information only, this returns null object even though the details are entered
		/// </summary>
		JobComInvoiceLine InvoiceLineWithADDDetails
		{
			get
			{
				if (!lineWithADDDetailsCalculated)
				{
					lineWithADDDetailsCalculated = true;
					invoiceLineWithADDDetails = this.GetInvoiceLineWithADD_CVDDetailsIncludingSecondaryLines(USAddInfoSchema.Constants.US_ADDCaseNo);
				}
				return invoiceLineWithADDDetails;
			}
		}
		JobComInvoiceLine invoiceLineWithADDDetails;
		bool lineWithADDDetailsCalculated;

		ZString ICusEntryLine.ManufacturerSupplierCode
		{
			get { return RandomLine?.ManufacturerFallBackToSupplierNumber ?? ZString.Empty; }
		}
		public ZDecimal ExciseTax
		{
			get
			{
				if (exciseTaxCached == null)
				{
					exciseTaxCached = new CachedProperty<ZDecimal>(Factory, Fees.GetExciseTax);
				}
				return exciseTaxCached.Value;
			}
		}
		CachedProperty<ZDecimal> exciseTaxCached;

		IEnumerable<IFee> ICusEntryLine.Fees
		{
			get
			{
				return new CusEntryLineFeesGenerator(this).Fees;
			}
		}

		ZString ICusEntryLine.SelectedRateType
		{
			get
			{
				ZString result = ZString.Empty;

				if (ImportTariff == null || ImportTariff.UE_DutyComputationCode == ComputationCodeList.Codes.SpecificSpecific)
				{
					var invoiceLine = RandomLine;

					result = invoiceLine != null ? invoiceLine.US_SelectedRateType : ZString.Empty;
				}

				return result;
			}
		}

		ZShort ICusEntryLine.InvDelimter
		{
			get
			{
				ZShort result = 0;
				var header = Header;
				if (!IsSecondaryTariffLine && header != null && header.InvoiceHeaders.Count > 1)
				{
					var nextEntryLine = header.MergedLines.FindByLineNumber(CL_LineNumber + 1);
					if (nextEntryLine == null || HasDifferentInvoice(nextEntryLine.RandomLine, RandomLine))
					{
						result = InvoiceSequence;
					}
				}

				return result;
			}
		}

		bool HasDifferentInvoice(JobComInvoiceLine nextRandomLine, JobComInvoiceLine randomLine)
		{
			return nextRandomLine != null &&
				randomLine != null &&
				nextRandomLine.JI_JZ != randomLine.JI_JZ;
		}

		ZString ICusEntryLine.FTZCurrentTariff
		{
			get { return RandomLine.US_FTZCurrentTariff; }
		}

		public ZShort InvoiceSequence
		{
			get { return RandomLine?.InvoiceHeader?.JZ_InvoiceDisplaySequence ?? ZShort.Zero; }
		}

		ZString ICusEntryLine.GetRelevantTariffForFee(string feeCode)
		{
			var lines = new List<CusEntryLine>();

			AddLineIfRelevantFor(lines, feeCode);

			foreach (CusEntryLine secondaryLine in ChildSecondaryEntryLines)
			{
				secondaryLine.AddLineIfRelevantFor(lines, feeCode);
			}

			var result = lines.Count == 0 ? ZString.Empty : lines[0].CL_AdValoremTariff;

			if (lines.Count > 1)
			{
				foreach (CusEntryLine entryLine in lines)
				{
					if (entryLine.Fees.GetAmount(feeCode) > 0)
					{
						result = entryLine.CL_AdValoremTariff;
						break;
					}
				}
			}

			return result;
		}

		void AddLineIfRelevantFor(List<CusEntryLine> lines, string feeCode)
		{
			var importTariff = ImportTariff;

			if (importTariff != null && importTariff.IsFeeApplicable(feeCode))
			{
				lines.Add(this);
			}
		}

		ZBool ICusEntryLine.IsDisclaimSanction
		{
			get { return !US_SupLine && RandomLine.US_DisclaimSanctions && (RandomLine.TariffMatchesFishingCondition || RandomLine.TariffMatchesMiningCondition); }
		}

		IEnumerable<ISanctionsAdditionalInfo> ICusEntryLine.SanctionsAdditionalInfos
		{
			get
			{
				var list = new List<ISanctionsAdditionalInfo>();
				var recordIndex = 1;
				foreach(FishingInformation fishing in RandomLine.FishingInformations)
				{
					var recordId = recordIndex.ToString().PadLeft(2, '0');
					if (!fishing.US_MethodOfHarvest.IsEmpty)
					{
						list.Add(new SanctionsAdditionalInfo(recordId, FshngInfo, MethodOfHarvest, fishing.US_MethodOfHarvest));
					}

					if (!fishing.US_VesselName.IsEmpty)
					{
						list.Add(new SanctionsAdditionalInfo(recordId, FshngInfo, VesselName, fishing.US_VesselName));
					}

					if (!fishing.US_VesselCountry.IsEmpty)
					{
						list.Add(new SanctionsAdditionalInfo(recordId, FshngInfo, VesselFlag, fishing.US_VesselCountry));
					}

					if (!fishing.US_VesselIMO.IsEmpty)
					{
						list.Add(new SanctionsAdditionalInfo(recordId, FshngInfo, VesselIMO, fishing.US_VesselIMO));
					}

					if (!fishing.US_HarvestedCountry.IsEmpty)
					{
						list.Add(new SanctionsAdditionalInfo(recordId, FshngInfo, CountryOfHarvest, fishing.US_HarvestedCountry));
					}
					recordIndex++;
				}

				recordIndex = 1;
				foreach (MiningInformation mining in RandomLine.MiningInformations)
				{
					if (!mining.CountryOfMining.IsEmpty)
					{
						list.Add(new SanctionsAdditionalInfo(recordIndex.ToString().PadLeft(2, '0'), MineInfo, CountryOfMining, mining.CountryOfMining));
					}
					recordIndex++;
				}
				return list;
			}
		}
		internal const string FshngInfo = "FSHNG INFO";
		internal const string MethodOfHarvest = "METHOD OF HARVEST";
		internal const string VesselName = "VESSEL NAME";
		internal const string VesselFlag = "VESSEL FLAG";
		internal const string VesselIMO = "VESSEL IMO";
		internal const string CountryOfHarvest = "COUNTRY OF HARVEST";

		internal const string MineInfo = "MINE INFO";
		internal const string CountryOfMining = "COUNTRY OF MINING";

		#endregion

		#region IDrawbackEntryLine Members

		ZDecimal IDrawbackEntryLine.TotalCustomsValueIncludingSecondaryLinesFromInvoiceLines
		{
			get
			{
				if (totalCustomsValueIncludingSecondaryLinesFromInvoiceLinesCached == null)
				{
					totalCustomsValueIncludingSecondaryLinesFromInvoiceLinesCached = new CachedProperty<ZDecimal>(Factory, Helper.GetTotalCustomsValueIncludingSecondaryLinesFromInvoiceLines);
				}
				return totalCustomsValueIncludingSecondaryLinesFromInvoiceLinesCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalCustomsValueIncludingSecondaryLinesFromInvoiceLinesCached;

		IDrawbackEntryLine[] IDrawbackEntryLine.AllDrawbackEntryLines => Header?.MergedLines.Cast<IDrawbackEntryLine>().ToArray() ?? Array.Empty<IDrawbackEntryLine>();

		ZDateTime IDrawbackEntryLine.EntryDate
		{
			get
			{
				var result = ZDateTime.Empty;
				var declaration = Declaration;
				if (declaration != null)
				{
					result = declaration.JE_DateOfArrival.IsEmpty ? declaration.US_EntryDate : declaration.JE_DateOfArrival;
				}
				return result;
			}
		}

		ZString IDrawbackEntryLine.EntryPort => Declaration?.US_SchDEntry ?? ZString.Empty;

		ZDecimal IDrawbackEntryLine.MPFAmountForEntry
		{
			get { return Header?.MPFAmountForEntry ?? ZDecimal.Zero; }
		}

		ZDecimal IDrawbackEntryLine.HMFAmountForEntry
		{
			get { return Header?.HMFAmountForEntry ?? ZDecimal.Zero; }
		}

		ZDecimal IDrawbackEntryLine.TotalEnteredValueForEntry
		{
			get { return Header?.TotalEnteredValue ?? ZDecimal.Zero; }
		}

		IDrawbackEntryLine IDrawbackEntryLine.ParentLine
		{
			get { return ParentLine; }
		}

		IEnumerable<IFee> IDrawbackEntryLine.Fees => Fees.Cast<IFee>();
		IEnumerable<IDrawbackEntryLine> IDrawbackEntryLine.ChildSecondaryEntryLines => ChildSecondaryEntryLines;
		ZDecimal IDrawbackEntryLine.GetFeeAmount(ZString feeTypeCode) => Fees.GetAmount(feeTypeCode);

		public ZDecimal TotalTaxIncludingSecondaryLines
		{
			get
			{
				if (totalTaxIncludingSecondaryLinesCached == null)
				{
					totalTaxIncludingSecondaryLinesCached = new CachedProperty<ZDecimal>(Factory, Helper.GetTotalTaxIncludingSecondaryLines);
				}
				return totalTaxIncludingSecondaryLinesCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalTaxIncludingSecondaryLinesCached;

		public ZDecimal TotalFeeAmountIncludingSecondaryLines
		{
			get
			{
				if (totalFeeAmountIncludingSecondaryLinesCached == null)
				{
					totalFeeAmountIncludingSecondaryLinesCached = new CachedProperty<ZDecimal>(Factory, Helper.GetTotalFeeAmountIncludingSecondaryLines);
				}
				return totalFeeAmountIncludingSecondaryLinesCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalFeeAmountIncludingSecondaryLinesCached;

		public ZDecimal TotalMPFIncludingSecondaryLines
		{
			get
			{
				if (totalMPFIncludingSecondaryLinesCached == null)
				{
					totalMPFIncludingSecondaryLinesCached = new CachedProperty<ZDecimal>(Factory, Helper.GetTotalMPFIncludingSecondaryLines);
				}
				return totalMPFIncludingSecondaryLinesCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalMPFIncludingSecondaryLinesCached;

		public ZDecimal TotalHMFIncludingSecondaryLines
		{
			get
			{
				if (totalHMFIncludingSecondaryLinesCached == null)
				{
					totalHMFIncludingSecondaryLinesCached = new CachedProperty<ZDecimal>(Factory, Helper.GetTotalHMFIncludingSecondaryLines);
				}
				return totalHMFIncludingSecondaryLinesCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalHMFIncludingSecondaryLinesCached;

		public ZDecimal TotalPayableMPFIncludingSecondaryLines
		{
			get
			{
				if (totalPayableMPFIncludingSecondaryLinesCached == null)
				{
					totalPayableMPFIncludingSecondaryLinesCached = new CachedProperty<ZDecimal>(Factory, Helper.GetTotalPayableMPFIncludingSecondaryLines);
				}
				return totalPayableMPFIncludingSecondaryLinesCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalPayableMPFIncludingSecondaryLinesCached;

		#endregion

		#region Calculated Property

		#region Other Fees

		public ZDecimal TotalFeeAmount
		{
			get
			{
				if (totalFeeAmountCached == null)
				{
					totalFeeAmountCached = new CachedProperty<ZDecimal>(Factory, () => Fees.TotalAmount);
				}
				return totalFeeAmountCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalFeeAmountCached;

		public ZDecimal PayableMPFAmount
		{
			get
			{
				if (payableMPFAmountCached == null)
				{
					payableMPFAmountCached = new CachedProperty<ZDecimal>(Factory, () => InvoiceLines.Sum(l => ((JobComInvoiceLine)l).US_PayableMPF));
				}
				return payableMPFAmountCached.Value;
			}
		}
		CachedProperty<ZDecimal> payableMPFAmountCached;

		public ZDecimal MPFAmount
		{
			get
			{
				if (mPFAmountCached == null)
				{
					mPFAmountCached = new CachedProperty<ZDecimal>(Factory, () => Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
				}
				return mPFAmountCached.Value;
			}
		}
		CachedProperty<ZDecimal> mPFAmountCached;

		public ZDecimal HMFAmount
		{
			get
			{
				if (hMFAmountCached == null)
				{
					hMFAmountCached = new CachedProperty<ZDecimal>(Factory, () => Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF));
				}
				return hMFAmountCached.Value;
			}
		}
		CachedProperty<ZDecimal> hMFAmountCached;

		public ZDecimal AvocadoAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado); }
		}

		public ZDecimal BeefAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Beef); }
		}

		public ZDecimal BlueberryAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry); }
		}

		public ZDecimal CottonAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton); }
		}

		public ZDecimal DutiableMailAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail); }
		}

		public ZDecimal HoneyAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Honey); }
		}

		public ZDecimal InformalAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal); }
		}

		public ZDecimal LimesAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes); }
		}

		public ZDecimal MangoAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Mango); }
		}

		public ZDecimal MushroomAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Mushroom); }
		}

		public ZDecimal OtherAgenciesAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherAgencies); }
		}

		public ZDecimal OtherExciseAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise); }
		}

		public ZDecimal RaspberryAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry); }
		}

		public ZDecimal PorkAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Pork); }
		}

		public ZDecimal PotatoAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Potato); }
		}

		public ZDecimal SoftwoodLumberAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber); }
		}

		public ZDecimal SpiritsAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits); }
		}

		public ZDecimal SorghumAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Sorghum); }
		}

		public ZDecimal SugarAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Sugar); }
		}

		public ZDecimal SurchargeAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge); }
		}

		public ZDecimal TobaccoAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco); }
		}

		public ZDecimal WatermelonAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Watermelon); }
		}

		public ZDecimal WinesAmount
		{
			get { return Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Wines); }
		}

		#endregion

		public ZString TransRelatedInd
		{
			get
			{
				var result = ZString.Empty;

				if (Header.HasMixedRelationshipIndicators)
				{
					result = RandomLine?.US_TransactionsRelated ?? ZString.Empty;
				}

				return result;
			}
		}

		public ZString LineLevelManufacturerIDWithLabel
		{
			get
			{
				var result = ZString.Empty;
				var header = Header;
				if (header != null && header.HasMultipleManufacturerIDs) //If so, it needs to print at line level
				{
					ZString manufacturerCode = ((ICusEntryLine)this).ManufacturerSupplierCode;
					if (!manufacturerCode.IsEmpty)
					{
						result = "MID " + manufacturerCode;
					}
				}
				return result;
			}
		}

		public ZString SPIAndOrSecondarySPI
		{
			get
			{
				var result = ZString.Empty;

				if (!iCusEntryLine.SpecialProgramsIndicatorCountry.IsEmpty)
				{
					result = iCusEntryLine.SpecialProgramsIndicatorCountry;
				}
				else if (!iCusEntryLine.SpecialProgramsIndicatorPrimary.IsEmpty)
				{
					result = iCusEntryLine.SpecialProgramsIndicatorPrimary;
				}

				if (!iCusEntryLine.SpecialProgramsIndicatorSecondary.IsEmpty)
				{
					result = result.IsEmpty ? iCusEntryLine.SpecialProgramsIndicatorSecondary.ToString() : result + "." + iCusEntryLine.SpecialProgramsIndicatorSecondary;
				}

				return result;
			}
		}

		public ZString CountryOfOriginForLine
		{
			get
			{
				var lineCountryOfOrigin = ZString.Empty;
				var header = Header;
				if (header != null && header.UniqueCountryOfOrigin == USConstants.MultipleValueIndicator)   //If so, it needs to print at line level
				{
					lineCountryOfOrigin = "O," + iCusEntryLine.CountryOfOrigin;
				}

				return lineCountryOfOrigin;
			}
		}

		public ZString CountryOfExportForLine
		{
			get
			{
				var lineCountryOfExport = ZString.Empty;
				var header = Header;
				if (header != null && header.UniqueCountryOfExport == USConstants.MultipleValueIndicator)   //If so, it needs to print at line level
				{
					lineCountryOfExport = "E," + iCusEntryLine.CountryOfExport;
				}

				return lineCountryOfExport;
			}
		}

		internal ZString PortOfLadingForLine
		{
			get
			{
				var header = Header;
				return header != null && header.UniquePortOfLading == USConstants.MultipleValueIndicator ?
					iCusEntryLine.PortOfLading : ZString.Empty;
			}
		}

		public ZString BindingRulingWithLabel
		{
			get
			{
				ZString result = ZString.Empty;

				if (Header != null)
				{
					if (iCusEntryLine.PreImportationReviewProgramRulingsType == PIRPRulingTypeList.Codes.BindingRulings)
					{
						ZString bindingRuling = iCusEntryLine.PreImportationReviewProgramRulingsNumber;
						if (!bindingRuling.IsEmpty)
						{
							result = "RLNG " + bindingRuling;
						}
					}
				}

				return result;
			}
		}

		public ZString TextileCategoryWithLabel
		{
			get
			{
				ZString result = ZString.Empty;

				if (Header != null)
				{
					var textileCategoryNumber = iCusEntryLine.TextileCategoryNumber;
					if (!textileCategoryNumber.IsEmpty)
					{
						result = "CAT " + textileCategoryNumber;
					}
				}

				return result;
			}
		}

		public ZString ADDNo
		{
			get
			{
				ZString result = ZString.Empty;
				var antidumpingCaseNumber = AntidumpingCaseNumber;
				if (!antidumpingCaseNumber.IsEmpty)
				{
					result = antidumpingCaseNumber.StartsWith("A") ? antidumpingCaseNumber.ToString() : "A" + antidumpingCaseNumber.ToString();
					result = (result.SubstringSafe(0, 4) + "-" + result.SubstringSafe(4, 3) + "-" + result.SubstringSafe(7, 3)).TrimEnd('-');
				}

				return result;
			}
		}

		public ZString CVDNo
		{
			get
			{
				ZString result = ZString.Empty;
				var countervailingCaseNumber = CountervailingCaseNumber;
				if (!countervailingCaseNumber.IsEmpty)
				{
					result = countervailingCaseNumber.StartsWith("C") ? countervailingCaseNumber.ToString() : "C" + countervailingCaseNumber.ToString();
					result = (result.SubstringSafe(0, 4) + "-" + result.SubstringSafe(4, 3) + "-" + result.SubstringSafe(7, 3)).TrimEnd('-');
				}

				return result;
			}
		}

		public ZString PartNo
		{
			get { return RandomLine.JI_PartNo; }
		}

		public ZString CustomAttrib1
		{
			get { return RandomLine.JI_CustomAttrib1; }
		}

		public ZString CustomAttrib2
		{
			get { return RandomLine.JI_CustomAttrib2; }
		}

		public ZString CustomAttrib3
		{
			get { return RandomLine.JI_CustomAttrib3; }
		}

		#endregion

		#region IDutyData Members

		ZString IDutyData.Tariff
		{
			get { return CL_AdValoremTariff; }
		}

		ZDate IDutyData.DateForDutyCalculation
		{
			get { return ((ICusEntryLine)this).DateForDutyCalculation; }
		}

		ZDecimal IDutyData.Quantity1
		{
			get { return ((ICusEntryLine)this).Quantity1; }
		}

		ZString IDutyData.UQ1
		{
			get { return ((ICusEntryLine)this).CustomsUnitQty; }
		}

		ZDecimal IDutyData.Quantity2
		{
			get { return ((ICusEntryLine)this).Quantity2; }
		}

		ZString IDutyData.UQ2
		{
			get { return ((ICusEntryLine)this).SecondCustomsUnitQty; }
		}

		ZDecimal IDutyData.Quantity3
		{
			get { return ((ICusEntryLine)this).Quantity3; }
		}

		ZString IDutyData.UQ3
		{
			get { return ((ICusEntryLine)this).ThirdCustomsUnitQty; }
		}

		ZDecimal IDutyData.CustomsValue
		{
			get { return RoundedCustomsValue; }
		}

		ZDecimal IDutyData.SupCustomsValue
		{
			get { return US_SupCustomsValue.Round(0); }
		}

		ZString IDutyData.SpecialProgramsIndicatorPrimary
		{
			get { return this.GetEffectiveSPIForDutyCalculation(((ICusEntryLine)this).SpecialProgramsIndicatorPrimary); }
		}

		ZString IDutyData.SpecialProgramsIndicatorCountry
		{
			get { return this.GetEffectiveSPIForDutyCalculation(((ICusEntryLine)this).SpecialProgramsIndicatorCountry); }
		}

		ZString IDutyData.CountryOfOrigin
		{
			get { return ((ICusEntryLine)this).CountryOfOrigin; }
		}

		ZString IDutyData.SpecialProgramsIndicatorSecondary
		{
			get { return ((ICusEntryLine)this).SpecialProgramsIndicatorSecondary; }
		}

		ZString IDutyData.SelectedRateType
		{
			get { return ((ICusEntryLine)this).SelectedRateType; }
		}

		ZString IDutyData.EntryType
		{
			get { return Header?.EntryType ?? ZString.Empty; }
		}

		bool IDutyData.IsClearedInPR
		{
			get { return ((IDutyData)RandomLine).IsClearedInPR; }
		}

		bool IDutyData.IsAMSFeeExempt
		{
			get { return IsAMSFeeExempt; }
		}

		bool IsAMSFeeExempt
		{
			get { return RandomLine?.IsAMSFeeExempt ?? false; }
		}

		bool IDutyData.IsRaspberryFeeExempt
		{
			get { return IsRaspberryFeeExempt; }
		}

		bool IsCottonFeeExempt
		{
			get { return this.IsCottonFeeExempt(false, false); }
		}

		bool IsRaspberryFeeExempt
		{
			get { return RandomLine?.IsRaspberryFeeExempt ?? false; }
		}

		ZBool IDutyData.IsSetVLine => IsSetVLine;

		ZBool IDutyData.IsSetXLine => IsSetXLine;

		bool IDutyData.IsCottonFeeExemptIndicated
		{
			get { return RandomLine?.IsCottonFeeExemptIndicated ?? false; }
		}

		bool IDutyData.HasCottonCertificate
		{
			get { return RandomLine?.HasCottonCertificate ?? false; }
		}

		IDutyData IDutyData.ParentTariffLine
		{
			get { return ParentLine; }
		}

		bool IDutyData.IsSecondaryTariffLine
		{
			get { return IsSecondaryTariffLine; }
		}

		ZDecimal IDutyData.ValueForADD
		{
			get { return ValueForADD; }
		}

		/// <summary>
		/// ICusEntryLine.ADDDepositRate returns a secondary line's one if exists for entry summary messaging purpose.
		/// However duty should be calculated on the line level so ICusEntryLine.ADDDepositRate cannot be used
		/// </summary>
		ZDecimal IDutyData.ADDDepositRate
		{
			get
			{
				var line = this.GetInvoiceLineWithADD_CVDDetails(USAddInfoSchema.Constants.US_ADDCaseNo);
				return line != null ? line.US_ADDDepositRate : ZDecimal.Zero;
			}
		}

		ZDecimal IDutyData.ValueForCVD
		{
			get { return ValueForCVD; }
		}

		/// <summary>
		/// ICusEntryLine.ADDDepositRate returns a secondary line's one if exists for entry summary messaging purpose.
		/// However duty should be calculated on the line level so ICusEntryLine.ADDDepositRate cannot be used
		/// </summary>
		ZDecimal IDutyData.CVDDepositRate
		{
			get
			{
				var line = this.GetInvoiceLineWithADD_CVDDetails(USAddInfoSchema.Constants.US_CVDCaseNo);
				return line != null ? line.US_CVDDepositRate : ZDecimal.Zero;
			}
		}

		ZDecimal IDutyData.ADDQuantity
		{
			get { return ADDQuantity; }
		}

		ZString IDutyData.ADDCaseRateTypeQualifier
		{
			get { return ADDCaseRateTypeQualifier; }
		}

		ZDecimal IDutyData.CVDQuantity
		{
			get { return CVDQuantity; }
		}

		ZString IDutyData.CVDCaseRateTypeQualifier
		{
			get { return CVDCaseRateTypeQualifier; }
		}

		ZDecimal? IDutyData.ADDutyManual
		{
			get
			{
				ZDecimal? result = null;

				var line = this.GetInvoiceLineWithADD_CVDDetails(USAddInfoSchema.Constants.US_ADDCaseNo);

				if (line != null && line.IsADDManual)
				{
					result = ZDecimal.Zero;
					result += Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.US_ADDuty);
				}

				return result;
			}
		}

		ZDecimal? IDutyData.CVDutyManual
		{
			get
			{
				ZDecimal? result = null;

				var line = this.GetInvoiceLineWithADD_CVDDetails(USAddInfoSchema.Constants.US_CVDCaseNo);

				if (line != null && line.IsCVDManual)
				{
					result = ZDecimal.Zero;
					result += Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.US_CVDuty);
				}

				return result;
			}
		}

		bool IDutyData.IsCombineSecondaryTariffLine
		{
			get { return RandomLine != null && ((IDutyData)RandomLine).IsCombineSecondaryTariffLine; }
		}

		IReadOnlyList<ZString> IDutyData.SupTariffs
		{
			get { return RandomLine != null ? ((IDutyData)RandomLine).SupTariffs : Array.Empty<ZString>(); }
		}

		IEnumerable<IDutyData> IDutyData.CombineChildLines
		{
			get { return RandomLine != null ? ((IDutyData)RandomLine).CombineChildLines : null; }
		}

		IEnumerable<IDutyData> IDutyData.CombineAllLines
		{
			get
			{
				var parentLine = ParentLine ?? this;
				var result = parentLine.ChildSecondaryEntryLines.ToList();
				if (result.Count > 0)
				{
					result.Insert(0, parentLine);
				}
				return result;
			}
		}

		IDutyData IDutyData.CombineParentLine
		{
			get
			{
				if (ParentLine is CusEntryLine parentLine && parentLine.US_SupAdditionalLine)
				{
					return RandomLine;
				}

				return RandomLine != null ? RandomLine.ParentTariffLine : null;
			}
		}

		public bool HasTextileCategoryNo
		{
			get { return !((ICusEntryLine)this).TextileCategoryNumber.IsEmpty; }
		}

		#endregion

		#region IFDAEntryLine Members

		ZShort IFDAEntryLine.EntryLineNo
		{
			get { return CL_LineNumber; }
		}

		ZString IFDAEntryLine.TariffNo
		{
			get { return CL_AdValoremTariff; }
		}

		IEnumerable<IPriorNoticeLine> IFDAEntryLine.BTALines
		{
			get
			{
				List<IPriorNoticeLine> result = new List<IPriorNoticeLine>();

				// do not report a FDA value for sup lines if its secondary line's FDA indicator is also D
				bool omitFDAValue = false;
				if (US_SupLine && OGAIndicatorList.IsToBeDeclared(((IOGA)this).FDAIndicator))
				{
					CusEntryLine nonSupLine = RandomLine.GetEntryLineFor(Header.CH_MessageType, false);

					omitFDAValue = nonSupLine != null && OGAIndicatorList.IsToBeDeclared(((IOGA)nonSupLine).FDAIndicator);
				}

				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					foreach (FDA fda in invoiceLine.FDAs)
					{
						result.Add(new PriorNoticeLine(fda, !omitFDAValue));
					}
				}

				return result;
			}
		}

		ZString IFDAEntryLine.FDAIndicator
		{
			get
			{
				var randomLine = RandomLine;
				var result = randomLine?.US_FDAIndicator ?? ZString.Empty;

				if (!result.IsEmpty && !MayRequireFDAData)
				{
					if (OGAIndicatorList.IsToBeDisclaimed(result))
					{
						result = ZString.Empty;
					}
					else
					{
						var supTariffLine = randomLine.GetEntryLineFor(Header.CH_MessageType, true);

						if (US_SupLine || supTariffLine != null && supTariffLine.MayRequireFDAData)
						{
							result = ZString.Empty;
						}
					}
				}
				return result;
			}
		}

		#endregion
		ZString ICargoReleaseCusEntryLine.UltimateConsigneeNumber
		{
			get
			{
				ZString result = ZString.Empty;
				var declaration = Declaration;
				if (declaration != null && declaration.HasLineLevelUltimateConsignees)
				{
					result = UltimateConsigneeNumberForCargoRelease;
				}

				return result;
			}
		}

		#region IGovernmentAgencies

		ZBool IGovernmentAgencies.ShouldIncludePGAInMessage(ZBool isCertified, ZString pgaCode)
		{
			var result = false;
			var declaration = Declaration;
			if (declaration != null)
			{
				var isNonWeeklyFTZ = declaration.IsConsumptionFTZ && !declaration.IsWeeklyEstimateFilingDate;
				result = GovernmentAgencyProgramCodeList.IsPGAAllowed(IsEntrySummary, IsCargoRelease, isCertified, declaration.US_PGAExpeditedRelease, declaration.IsWeeklyEstimateConsumptionFTZ, declaration.US_EntryType, pgaCode);

				if (result && IsEntrySummary && !isCertified && isNonWeeklyFTZ && pgaCode == GovernmentAgencyProgramCodeList.Codes.VNE)
				{
					var lastClearedMessageWithPGA = PGADispositionProviderExtensionMethods.GetLastCRClearedMessageWithPGA(declaration, withPGA: true);
					if (lastClearedMessageWithPGA != null)
					{
						result = !lastClearedMessageWithPGA.MessageBlock.MessageBlocks.OfType<AEPAPG01>().Any(x => x.GovernmentAgencyCode == ACEGovernmentAgenciesCodeList.Codes.EPA && x.GovernmentAgencyProgramCode == pgaCode);
					}
				}
			}

			return result;
		}

		ZBool IsCargoRelease
		{
			get
			{
				if (cachedIsCargoRelease == null)
				{
					cachedIsCargoRelease = new CachedProperty<ZBool>(Factory, delegate
					{
						return Header?.IsACECargoRelease ?? false;
					});
				}

				return cachedIsCargoRelease.Value;
			}
		}
		CachedProperty<ZBool> cachedIsCargoRelease;

		ZBool IsEntrySummary
		{
			get
			{
				if (cachedIsEntrySummary == null)
				{
					cachedIsEntrySummary = new CachedProperty<ZBool>(Factory, delegate
					{
						return Header?.IsFormalEntry ?? false;
					});
				}

				return cachedIsEntrySummary.Value;
			}
		}
		CachedProperty<ZBool> cachedIsEntrySummary;

		public IEnumerable<IPGADataCorrection> PGADataCorrections
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					foreach (IPGADataCorrection line in invoiceLine.PGADataCorrections)
					{
						yield return line;
					}
				}
			}
		}

		ZString IGovernmentAgencies.CommercialDescription
		{
			get { return RandomLine?.JI_Description.Left(70) ?? ZString.Empty; }
		}

		ITSCAData IGovernmentAgencies.EPA_TSCAData
		{
			get { return RandomLine; }
		}

		IPGADataCorrection IGovernmentAgencies.ODSDataCorrection
		{
			get { return RandomLine?.ODSDataCorrection; }
		}

		IPGADataCorrection IGovernmentAgencies.TSCADataCorrection
		{
			get { return RandomLine?.TSCADataCorrection; }
		}

		ZString IGovernmentAgenciesIndicators.TSCAIndicator
		{
			get
			{
				var result = ZString.Empty;
				if (ShouldSendPGAAtThisLevel(x => x.HasTSCARequirement))
				{
					result = RandomLine?.US_TSCAInd ?? ZString.Empty;
				}
				return result;
			}
		}

		ZString IGovernmentAgenciesIndicators.TSCADisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasTSCARequirement) ? RandomLine.US_TSCADisclaimReason : ZString.Empty; }
		}

		bool HasTSCARequirement
		{
			get { return ImportTariff?.HasTSCARequirement ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.VNEIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasVNERequirement) ? RandomLine.US_VNEInd : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.VNEDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasVNERequirement) ? RandomLine.US_VNEDisclaimReason : ZString.Empty; }
		}

		IEnumerable<IVNEData> IGovernmentAgencies.EPA_VNELines
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasVNERequirement), invoiceLine => invoiceLine.VehicleLines.OfType<IVNEData>()); }
		}

		bool HasVNERequirement
		{
			get { return ImportTariff?.HasVNERequirement ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.ODSIndicator
		{
			get
			{
				var result = ZString.Empty;
				if (ShouldSendPGAAtThisLevel(x => x.HasODSRequirement))
				{
					result = RandomLine?.US_ODSInd ?? ZString.Empty;
				}
				return result;
			}
		}

		ZString IGovernmentAgenciesIndicators.ODSDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasODSRequirement) ? RandomLine.US_ODSDisclaimReason : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.OMCIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasOMCRequirement) ? RandomLine.US_OMCInd : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.OMCDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasOMCRequirement) ? RandomLine.US_OMCDisclaimReason : ZString.Empty; }
		}

		IEnumerable<IOMCHeader> IGovernmentAgencies.OMCHeaders
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasOMCRequirement), invoiceLine => invoiceLine.OMCHeaders.OfType<IOMCHeader>()); }
		}

		bool HasOMCRequirement
		{
			get { return ImportTariff != null && ImportTariff.HasOMCRequirement; }
		}

		ZString IGovernmentAgenciesIndicators.ATFIndicator
		{
			get { return !US_SupLine ? RandomLine.US_ATFInd : ZString.Empty; }
		}

		IEnumerable<IATFData> IGovernmentAgencies.ATFLines
		{
			get { return GetAllPGALinesPerPGA(!US_SupLine, invoiceLine => invoiceLine.ATFLines.OfType<IATFData>()); }
		}

		bool HasODSRequirement
		{
			get { return ImportTariff?.HasODSRequirement ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.FSISIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasFSISRequirement) ? RandomLine.US_FSISInd : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.FSISDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasFSISRequirement) ? RandomLine.US_FSISDisclaimReason : ZString.Empty; }
		}

		IEnumerable<IFSISLine> IGovernmentAgencies.FSISLines
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasFSISRequirement), invoiceLine => invoiceLine.FSISLines.OfType<IFSISLine>()); }
		}

		bool HasFSISRequirement
		{
			get { return ImportTariff?.HasFSISRequirement ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.NMFS370Indicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasNMFS370Requirement) ? RandomLine.US_NMFS370Ind : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NMFS370DisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasNMFS370Requirement) ? RandomLine.US_NMFS370DisclaimReason : ZString.Empty; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFS370Lines
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasNMFS370Requirement), invoiceLine => invoiceLine.NMFS370Lines.OfType<INMFSLine>()); }
		}

		bool HasNMFS370Requirement
		{
			get { return ImportTariff?.HasNMFS370Requirement ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSAMRIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasNMFSAMRRequirement) ? RandomLine.US_NMFSAMRInd : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSAMRDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasNMFSAMRRequirement) ? RandomLine.US_NMFSAMRDisclaimReason : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.APHISIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasAPHISRequirement) ? RandomLine.US_APHISInd : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.APHISDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasAPHISRequirement) ? RandomLine.US_APHISDisclaimReason : ZString.Empty; }
		}

		bool HasAPHISRequirement
		{
			get { return ImportTariff?.HasAPHISRequirement ?? false; }
		}

		IEnumerable<IAPHISHeader> IGovernmentAgencies.APHISHeaders
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasAPHISRequirement), invoiceLine => invoiceLine.APHISHeaders.OfType<IAPHISHeader>()); }
		}

		ZString IGovernmentAgenciesIndicators.CPSCIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasCPSCRequirement) ? RandomLine.US_CPSCInd : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.CPSCDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasCPSCRequirement) ? RandomLine.US_CPSCDisclaimReason : ZString.Empty; }
		}

		IEnumerable<ICPSCHeader> IGovernmentAgencies.CPSCHeaders
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasCPSCRequirement), invoiceLine => invoiceLine.CPSCHeaders.OfType<ICPSCHeader>()); }
		}

		bool HasCPSCRequirement
		{
			get { return ImportTariff != null && ImportTariff.HasCPSCRequirement; }
		}

		ZString IGovernmentAgenciesIndicators.AMSIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasAMSRequirement) ? RandomLine.US_AMSInd : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.AMSDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasAMSRequirement) ? RandomLine.US_AMSDisclaimReason : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.AMSDisclaimProgram
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasAMSRequirement) ? RandomLine.US_AMSDisclaimProgram : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NOPIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasNOPRequirement) ? RandomLine.US_NOPInd : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NOPDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasNOPRequirement) ? RandomLine.US_NOPDisclaimReason : ZString.Empty; }
		}

		bool HasAMSRequirement
		{
			get { return ImportTariff?.HasAMSRequirement ?? false; }
		}

		bool HasNOPRequirement
		{
			get { return ImportTariff?.HasNOPRequirement ?? false; }
		}

		IEnumerable<IAMSData> IGovernmentAgencies.AMSLines
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasAMSRequirement || x.HasNOPRequirement), invoiceLine => invoiceLine.AMSLines.OfType<IAMSData>()); }
		}

		ZString IGovernmentAgenciesIndicators.FWSIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasFWSRequirement) ? RandomLine.US_FWSInd : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.FWSDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasFWSRequirement) ? RandomLine.US_FWSDisclaimReason : ZString.Empty; }
		}

		bool HasFWSRequirement
		{
			get { return ImportTariff?.HasFWSRequirement ?? false; }
		}

		IEnumerable<IFWSHeader> IGovernmentAgencies.FWSHeaders
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasFWSRequirement), invoiceLine => invoiceLine.FWSHeaders.OfType<IFWSHeader>()); }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSAMRLines
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasNMFSAMRRequirement), invoiceLine => invoiceLine.NMFSAMRLines.OfType<INMFSLine>()); }
		}

		bool HasNMFSAMRRequirement
		{
			get { return ImportTariff?.HasNMFSAMRRequirement ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSHMSIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasNMFSHMSRequirement) ? RandomLine.US_NMFSHMSInd : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSHMSDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasNMFSHMSRequirement) ? RandomLine.US_NMFSHMSDisclaimReason : ZString.Empty; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSHMSLines
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasNMFSHMSRequirement), invoiceLine => invoiceLine.NMFSHMSLines.OfType<INMFSLine>()); }
		}

		bool HasNMFSHMSRequirement
		{
			get { return ImportTariff?.HasNMFSHMSRequirement ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSSIMIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasNMFSSIMRequirement) ? RandomLine.US_NMFSSIMPInd : ZString.Empty; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSSIMLines
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasNMFSSIMRequirement), invoiceLine => invoiceLine.NMFSSIMPLines.OfType<INMFSLine>()); }
		}

		bool HasNMFSSIMRequirement
		{
			get { return ImportTariff?.HasNMFSSIMRequirement ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSCOAIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasNMFSCOARequirement) ? RandomLine.US_NMFSCOAInd : ZString.Empty; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSCOALines
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasNMFSCOARequirement), invoiceLine => invoiceLine.NMFSCOALines.OfType<INMFSLine>()); }
		}

		bool HasNMFSCOARequirement
		{
			get { return ImportTariff?.DoesRequireNMFSCOA(((ICusEntryLine)this).CountryOfOrigin, DateForDutyCalculation) ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.DDTCIndicator
		{
			get
			{
				var result = ZString.Empty;
				if (ShouldSendPGAForAgencyWithoutTariffApplicability())
				{
					result = RandomLine?.US_DDTCInd ?? ZString.Empty;
				}
				return result;
			}
		}

		IDDTCData IGovernmentAgencies.DDTCData
		{
			get { return RandomLine?.DDTCDataCorrection; }
		}

		bool ShouldSendPGAAtThisLevel(Func<CusEntryLine, bool> mayRequireData)
		{
			var result = true;
			var invoiceLine = RandomLine;
			if (!invoiceLine.HasEmptySupTariff)
			{
				if (US_SupLine)
				{
					result = mayRequireData(this);
				}
				else
				{
					var supLine = invoiceLine.GetEntryLineFor(Header.CH_MessageType, true);
					result = supLine != null && !mayRequireData(supLine);
				}
			}
			return result;
		}
		bool ShouldSendPGAForAgencyWithoutTariffApplicability() => RandomLine.HasEmptySupTariff || !US_SupLine;

		IEnumerable<ILaceyActCommon> IGovernmentAgencies.LaceyActData
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasLaceyActRequirement), invoiceLine => invoiceLine.LaceyActLines.OfType<ILaceyActCommon>()); }
		}

		bool HasLaceyActRequirement
		{
			get { return ImportTariff?.HasLaceyActRequirement ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.LaceyActIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasLaceyActRequirement) ? RandomLine.US_LaceyIndicator : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.LaceyActDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasLaceyActRequirement) ? RandomLine.US_LaceyDisclaimReason : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.PSTIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasPSTRequirement) ? RandomLine.US_PSTIndicator : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.PSTDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasPSTRequirement) ? RandomLine.US_PSTDisclaimReason : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.PSTDisclaimProgram
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasPSTRequirement) ? RandomLine.US_PSTDisclaimProgram : ZString.Empty; }
		}

		public IEnumerable<IPSTData> EPA_PSTLines
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasPSTRequirement), invoiceLine => invoiceLine.PSTLines.OfType<IPSTData>()); }
		}

		bool HasPSTRequirement
		{
			get { return ImportTariff?.HasPSTRequirement ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.ACEFDAIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasACEFDARequirement) && RandomLine.IsACEFDARelevant ? RandomLine.US_FDAIndicator : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.ACEFDADisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasACEFDARequirement) && RandomLine.IsACEFDARelevant ? RandomLine.US_FDADisclaimReason : ZString.Empty; }
		}

		public IEnumerable<IFDAData> FDALines
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasACEFDARequirement), invoiceLine => invoiceLine.ACE_FDALines.OfType<IFDAData>()); }
		}

		bool HasACEFDARequirement
		{
			get { return ImportTariff?.HasACEFDARequirement ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.TTBIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasTTBRequirement) ? RandomLine.US_TTBInd : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.TTBDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasTTBRequirement) ? RandomLine.US_TTBDisclaimReason : ZString.Empty; }
		}

		IEnumerable<ITTBLine> IGovernmentAgencies.TTBLines
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasTTBRequirement), invoiceLine => invoiceLine.TTBLines.OfType<ITTBLine>()); }
		}

		bool HasTTBRequirement
		{
			get { return ImportTariff?.HasTTBRequirement ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.NHTSAIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasNHTSARequirement) ? RandomLine.US_NHTSAIndicator : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NHTSADisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasNHTSARequirement) ? RandomLine.US_NHTDisclaimReason : ZString.Empty; }
		}

		IEnumerable<INHTSAHeader> IGovernmentAgencies.NHTSALines
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasNHTSARequirement), invoiceLine => invoiceLine.NHTSALines.OfType<INHTSAHeader>()); }
		}

		bool HasNHTSARequirement
		{
			get { return ImportTariff?.HasNHTSARequirement ?? false; }
		}

		ZString IGovernmentAgenciesIndicators.DEAIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasDEARequirement) ? RandomLine.US_DEAInd : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.DEADisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasDEARequirement) ? RandomLine.US_DEADisclaimReason : ZString.Empty; }
		}

		IEnumerable<IDEAHeader> IGovernmentAgencies.DEAHeaders
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasDEARequirement), invoiceLine => invoiceLine.DEAHeaders.OfType<IDEAHeader>()); }
		}

		bool HasDEARequirement
		{
			get { return ImportTariff != null && ImportTariff.HasDEARequirement; }
		}

		ZString IGovernmentAgenciesIndicators.HFCIndicator
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasHFCRequirement) ? RandomLine.US_HFCInd : ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.HFCDisclaimReason
		{
			get { return ShouldSendPGAAtThisLevel(x => x.HasHFCRequirement) ? RandomLine.US_HFCDisclaimReason : ZString.Empty; }
		}

		IEnumerable<IHFCHeader> IGovernmentAgencies.EPA_HFCHeaders
		{
			get { return GetAllPGALinesPerPGA(ShouldSendPGAAtThisLevel(x => x.HasHFCRequirement), invoiceLine => invoiceLine.USHFCHeaders.OfType<IHFCHeader>()); }
		}

		bool HasHFCRequirement
		{
			get { return ImportTariff != null && ImportTariff.HasHFCRequirement; }
		}

		IEnumerable<T> GetAllPGALinesPerPGA<T>(ZBool shouldSendPGAAtThisLevel, Func<JobComInvoiceLine, IEnumerable<T>> getPGALinesFromInvoiceLine)
		{
			var result = new List<T>();

			if (shouldSendPGAAtThisLevel)
			{
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result.AddRange(new TypedEnumerable<T>(getPGALinesFromInvoiceLine(invoiceLine)));
				}
			}

			return result;
		}

		#endregion

		#region IPGALineNumbers Members

		ZInt IPGALineNumbers.EPAStartLineNumber
		{
			get; set;
		}

		ZInt IPGALineNumbers.FSISStartLineNumber
		{
			get; set;
		}

		ZInt IPGALineNumbers.NMFSStartLineNumber
		{
			get; set;
		}

		ZInt IPGALineNumbers.FDAStartLineNumber
		{
			get; set;
		}

		ZInt IPGALineNumbers.TTBStartLineNumber
		{
			get; set;
		}

		ZInt IPGALineNumbers.NHTSAStartLineNumber
		{
			get; set;
		}

		ZInt IPGALineNumbers.AMSStartLineNumber
		{
			get; set;
		}

		ZInt IPGALineNumbers.APHStartLineNumber
		{
			get; set;
		}

		ZInt IPGALineNumbers.FWSStartLineNumber
		{
			get; set;
		}

		ZInt IPGALineNumbers.ATFStartLineNumber
		{
			get; set;
		}

		ZInt IPGALineNumbers.CPSCStartLineNumber
		{
			get; set;
		}

		ZInt IPGALineNumbers.OMCStartLineNumber
		{
			get; set;
		}

		ZInt IPGALineNumbers.DEAStartLineNumber
		{
			get; set;
		}

		void IPGALineNumbers.ClearPGALineNumbers()
		{
			((IPGALineNumbers)this).EPAStartLineNumber = 0;
			((IPGALineNumbers)this).FSISStartLineNumber = 0;
			((IPGALineNumbers)this).NMFSStartLineNumber = 0;
			((IPGALineNumbers)this).FDAStartLineNumber = 0;
			((IPGALineNumbers)this).TTBStartLineNumber = 0;
			((IPGALineNumbers)this).NHTSAStartLineNumber = 0;
			((IPGALineNumbers)this).AMSStartLineNumber = 0;
			((IPGALineNumbers)this).APHStartLineNumber = 0;
			((IPGALineNumbers)this).FWSStartLineNumber = 0;
			((IPGALineNumbers)this).ATFStartLineNumber = 0;
			((IPGALineNumbers)this).CPSCStartLineNumber = 0;
			((IPGALineNumbers)this).OMCStartLineNumber = 0;
			((IPGALineNumbers)this).DEAStartLineNumber = 0;
		}

		#endregion

		public new class Loader : Customs.Business.CusEntryLine.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusEntryLine);
			}

			/// <summary>
			/// Finds an entry line with match entry number, line number and entry filer code in the current company
			/// </summary>
			public CusEntryLine FindParentByDeclarationLineNumberAndEntryFilerCode(ZString importDeclarationNumber, ZInt importDeclarationLine, ZString entryFilerCode1)
			{
				if (importDeclarationLine < 1 || importDeclarationLine > short.MaxValue)
				{
					return null;
				}

				var possibleMatches = Factory.Load<CusEntryLine>(FindByDeclarationAndLineNumbersQuery(importDeclarationNumber, CusEntryHeaderMessageTypeList.Codes.EntrySummary, importDeclarationLine));
				var furtherMatches = possibleMatches.Where(x => !x.US_SupLine && x.Declaration.US_EntryFilerCode == entryFilerCode1).ToArray();

				CusEntryLine result = null;
				if (furtherMatches.Length > 0)
				{
					CusEntryLine resultWithoutTariff = null;
					foreach (var cusEntryLine in furtherMatches.OrderBy(x => x.US_ChildLineNum))
					{
						if (!cusEntryLine.IsChildLine)
						{
							result = cusEntryLine;
							break;
						}
						if (!cusEntryLine.CL_AdValoremTariff.IsEmpty)
						{
							result = cusEntryLine;
							break;
						}
						if (resultWithoutTariff == null)
						{
							resultWithoutTariff = cusEntryLine;
						}
					}
					result = result ?? resultWithoutTariff;
				}
				return result;
			}

			ZQuery FindByDeclarationAndLineNumbersQuery(ZString importDeclarationNumber, ZString importDeclarationType, ZInt importDeclarationLine, ZQuery query = null)
			{
				ZDBOnlyQuery entryLineQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
				entryLineQuery.AddToFilter(CusEntryLineSchema.CL_CustomsPostedStatus, Customs.Business.EntryLineStatusList.Codes.Active);
				entryLineQuery.AddToFilter(CusEntryLineSchema.CL_LineNumber, (short)importDeclarationLine);
				ZDBOnlySubQuery entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
				ZDBOnlySubQuery declarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
				ZDBOnlySubQuery cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.Equal, importDeclarationNumber);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, importDeclarationType);
				declarationQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);
				entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, importDeclarationType);
				entryHeaderQuery.AddSubQuery(declarationQuery, JoinCondition.And);
				entryLineQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
				if (query != null)
				{
					query.AddToFilter(entryLineQuery);
				}
				return query ?? entryLineQuery;
			}
			protected override Customs.Business.CusEntryLine FindByDeclarationAndLineNumbersCore(ZString importDeclarationNumber, ZString importDeclarationType, ZInt importDeclarationLine, ZQuery query)
			{
				if (importDeclarationLine < 1 || importDeclarationLine > short.MaxValue)
				{
					return null;
				}

				return Factory.LoadTop1<CusEntryLine>(FindByDeclarationAndLineNumbersQuery(importDeclarationNumber, importDeclarationType, importDeclarationLine, query));
			}
		}

		#region IAESTIRCommodityLineItem Members

		ZString IAESTIRCommodityLineItem.ExportInformationCode
		{
			get { return ExportCode; }
		}

		ZInt IAESTIRCommodityLineItem.LineNumber
		{
			get { return CL_LineNumber; }
		}

		ZString IAESTIRCommodityLineItem.CommodityDescription
		{
			get { return Description; }
		}

		ZDecimal IAESTIRCommodityLineItem.LicenseValue
		{
			get
			{
				if (fLicenseValue == null)
				{
					fLicenseValue = new CachedProperty<ZDecimal>(Factory, delegate
					{
						var result = ZDecimal.Zero;
						result += Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.US_LicenseValue);
						return result;
					});
				}
				return RoundToWholeValueWithMinimumOfOneIfValueHasSomeValue(fLicenseValue.Value);
			}
		}
		CachedProperty<ZDecimal> fLicenseValue;

		ZString IAESTIRCommodityLineItem.LicenseCodeLicenseExemptionCode
		{
			get { return LicenseType; }
		}

		ZString IAESTIRCommodityLineItem.ForeignDomesticOriginIndicator
		{
			get { return AESOriginIndicator; }
		}

		ZString IAESTIRCommodityLineItem.ScheduleBHTSNumber
		{
			get { return CL_AdValoremTariff; }
		}

		ZString IAESTIRCommodityLineItem.UnitOfMeasure1
		{
			get { return CustomsUnitQty; }
		}

		ZDecimal IAESTIRCommodityLineItem.Quantity1
		{
			get { return RoundToWholeValueWithMinimumOfOneIfValueHasSomeValue(CustomsQuantity); }
		}

		ZDecimal IAESTIRCommodityLineItem.ValueOfGoods
		{
			get { return RoundToWholeValueWithMinimumOfOneIfValueHasSomeValue(CustomsValue.Amount); }
		}

		ZString IAESTIRCommodityLineItem.UnitOfMeasure2
		{
			get { return SecondCustomsUnitQty; }
		}

		ZDecimal IAESTIRCommodityLineItem.Quantity2
		{
			get { return RoundToWholeValueWithMinimumOfOneIfValueHasSomeValue(SecondCustomsQuantity); }
		}

		ZDecimal IAESTIRCommodityLineItem.ShippingWeight
		{
			get { return RoundToWholeValueWithMinimumOfOneIfValueHasSomeValue(GrossWeightInKilograms); }
		}

		ZString IAESTIRCommodityLineItem.ExportControlClassificationNumberECCN
		{
			get { return ECCN; }
		}

		ZString IAESTIRCommodityLineItem.ExportLicenseNumberCFRCitationAuthorizationSymbolKCP
		{
			get { return LicenseAndLicenseExemptionTypeManager.GetLicenseNumberOrLicenseExemptionCode(LicenseType, LicenseNumber, Factory, Declaration.GetEffectiveDateForECR()); }
		}

		ZString IAESTIRCommodityLineItem.DDTCITARExemptionNumber
		{
			get { return DDTCITARExemptionNumber; }
		}

		ZString IAESTIRCommodityLineItem.DDTCRegistrationNumber
		{
			get { return DDTCRegistrationNumber; }
		}

		ZString IAESTIRCommodityLineItem.DDTCSignificantMilitaryEquipmentSMEIndicator
		{
			get { return DDTCSignificantMilitaryEquipmentIndicator; }
		}

		ZString IAESTIRCommodityLineItem.DDTCEligiblePartyCertificationIndicator
		{
			get { return DDTCEligiblePartyCertificationIndicator; }
		}

		ZString IAESTIRCommodityLineItem.DDTCUSMLCategoryCode
		{
			get { return DDTCUSMLCategoryCode; }
		}

		ZString IAESTIRCommodityLineItem.DDTCUnitOfMeasureCode
		{
			get { return DDTCUnitOfMeasure; }
		}

		ZDecimal IAESTIRCommodityLineItem.DDTCQuantity
		{
			get { return RoundToWholeValueWithMinimumOfOneIfValueHasSomeValue(DDTCQuantity); }
		}

		ZString IAESTIRCommodityLineItem.DDTCCommodityJurisdictionNumber
		{
			get { return DDTCCommodityJurisdictionNumber; }
		}

		IEnumerable<IAESTIRUsedVehicle> IAESTIRCommodityLineItem.UsedVehicles
		{
			get
			{
				if (IsUsedVehicle)
				{
					yield return this;
				}
			}
		}

		ZDecimal RoundToWholeValueWithMinimumOfOneIfValueHasSomeValue(ZDecimal fValue)
		{
			return fValue > 0 && fValue < 1 ? 1 : fValue.Round(0);
		}

		ZString IAESTIRCommodityLineItem.AMSIndicator
		{
			get { return RandomLine != null ? RandomLine.US_AMSInd : ZString.Empty; }
		}

		ZString IAESTIRCommodityLineItem.EPAIndicator
		{
			get { return RandomLine != null ? RandomLine.US_PSTIndicator : ZString.Empty; }
		}

		ZString IAESTIRCommodityLineItem.NMFSIndicator
		{
			get { return RandomLine != null ? RandomLine.US_NMFSHMSInd : ZString.Empty; }
		}

		ZString IAESTIRCommodityLineItem.ATFIndicator
		{
			get { return RandomLine != null ? RandomLine.US_ATFInd : ZString.Empty; }
		}

		ZString IAESTIRCommodityLineItem.DEAIndicator
		{
			get { return RandomLine != null ? RandomLine.US_DEAInd : ZString.Empty; }
		}

		ZString IAESTIRCommodityLineItem.FWSIndicator
		{
			get { return RandomLine != null ? RandomLine.US_FWSInd : ZString.Empty; }
		}

		ZString IAESTIRCommodityLineItem.TTBIndicator
		{
			get { return RandomLine != null ? RandomLine.US_TTBInd : ZString.Empty; }
		}

		IAESAMS IAESTIRCommodityLineItem.ExportAMS
		{
			get { return RandomLine; }
		}

		IAESEPA IAESTIRCommodityLineItem.ExportEPA
		{
			get { return RandomLine; }
		}

		IAESATF IAESTIRCommodityLineItem.ExportATF
		{
			get { return RandomLine?.ExportATF; }
		}

		IAESFWS IAESTIRCommodityLineItem.ExportFWS
		{
			get { return RandomLine?.ExportFWS; }
		}

		IEnumerable<IAESNMFS> IAESTIRCommodityLineItem.ExportNMFSLines
		{
			get
			{
				var result = new List<IAESNMFS>();
				if (RandomLine != null)
				{
					result.AddRange(new TypedEnumerable<IAESNMFS>(RandomLine.NMFSLines));
				}
				return result;
			}
		}

		IEnumerable<IAESDEA> IAESTIRCommodityLineItem.ExportDEALines
		{
			get
			{
				var result = new List<IAESDEA>();
				if (RandomLine != null)
				{
					result.AddRange(new TypedEnumerable<IAESDEA>(RandomLine.DEAHeaders));
				}
				return result;
			}
		}

		IEnumerable<IAESTTB> IAESTIRCommodityLineItem.ExportTTBLines
		{
			get
			{
				var result = new List<IAESTTB>();
				if (RandomLine != null)
				{
					result.AddRange(new TypedEnumerable<IAESTTB>(RandomLine.TTBLines));
				}
				return result;
			}
		}

		#endregion

		#region IAESTIRUsedVehicle Members

		ZString IAESTIRUsedVehicle.VehicleIdentificationNumberVINProductID
		{
			get { return VehicleID; }
		}

		ZString IAESTIRUsedVehicle.VehicleIDQualifier
		{
			get { return VehicleIDType; }
		}

		ZString IAESTIRUsedVehicle.VehicleTitleNumber
		{
			get { return VehicleTitleNumber; }
		}

		ZString IAESTIRUsedVehicle.VehicleTitleStateCode
		{
			get { return VehicleTitleState; }
		}

		#endregion

		#region IFeeCalculationDataProvider Members

		bool IFeeCalculationDataProvider.IsACS
		{
			get => ((IFeeCalculationDataProvider)RandomLine).IsACS;
		}

		bool IFeeCalculationDataProvider.IsFeeOverriden(string feeCode)
		{
			return ((IFeeCalculationDataProvider)RandomLine).IsFeeOverriden(feeCode);
		}

		void IFeeCalculationDataProvider.SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			Fees.UpdateOrAddCharge(feeCode, amount);
		}

		ZString IFeeCalculationDataProvider.GetSelectedRateType(string feeCode)
		{
			return ((IFeeCalculationDataProvider)RandomLine).GetSelectedRateType(feeCode);
		}

		public ZDateTime DateForMPFCalculation
		{
			get { return RandomLine?.DateForMPFCalculation ?? ZDateTime.Today; }
		}

		ZDecimal? IFeeCalculationDataProvider.OverriddenTaxRate
		{
			get { return US_SupLine ? null : ((IFeeCalculationDataProvider)RandomLine).OverriddenTaxRate; }
		}

		ZString IFeeCalculationDataProvider.OverriddenTaxRateUQ
		{
			get { return US_SupLine ? ZString.Empty : ((IFeeCalculationDataProvider)RandomLine).OverriddenTaxRateUQ; }
		}

		ZString IFeeCalculationDataProvider.TaxCode
		{
			get { return US_SupLine ? ZString.Empty : ((IFeeCalculationDataProvider)RandomLine).TaxCode; }
		}

		ZString IFeeCalculationDataProvider.TaxComputationCode
		{
			get { return US_SupLine ? ZString.Empty : ((IFeeCalculationDataProvider)RandomLine).TaxComputationCode; }
		}

		ZString IFeeCalculationDataProvider.TaxRateType
		{
			get { return US_SupLine ? ZString.Empty : ((IFeeCalculationDataProvider)RandomLine).TaxRateType; }
		}

		ZDecimal IFeeCalculationDataProvider.TaxRateQuantity
		{
			get { return US_SupLine ? 0m : InvoiceLines.Cast<IFeeCalculationDataProvider>().Sum(x => x.TaxRateQuantity); }
		}

		ZDecimal IFeeCalculationDataProvider.DairyQty
		{
			get { return TotalDairyQty; }
		}

		public ZDecimal TotalDairyQty => US_SupLine ? ZDecimal.Zero : Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.DairyQty);

		ZString IFeeCalculationDataProvider.VisaNumber
		{
			get
			{
				if (RandomLine != null)
				{
					return RandomLine.US_VisaNo;
				}
				return ZString.Empty;
			}
		}

		IEnumerable<IDutyData> IFeeCalculationDataProvider.SecondaryLines
		{
			get { return ChildSecondaryEntryLines.Cast<IDutyData>(); }
		}

		#endregion

		#region IACECusEntryLine Members

		ZString IACECusEntryLine.ArticleSetIndicator
		{
			get { return !IsSecondaryTariffLine && (IsSetXLine || IsSetVLine) ? RandomLine.US_SetInd : ZString.Empty; }
		}

		ZString IACECusEntryLine.FeeExemptionCode
		{
			get { return RandomLine.IsCottonFeeExemptIndicated ? "1" : string.Empty; }
		}

		ZString IACECusEntryLine.ADDCVDNonReimbursementStatement
		{
			get
			{
				var randomLine = RandomLine;
				var result = randomLine.US_ADCVDStat;
				if (result.IsEmpty && randomLine.IsParentLine && !IsSetXLine)
				{
					result = randomLine.ChildLines.Select(x => x.US_ADCVDStat).FirstOrDefault(y => !y.IsEmpty);
				}
				return result;
			}
		}

		ZString IACECusEntryLine.SoldToPartyID
		{
			get { return RandomLine.SoldToPartyEIN; }
		}

		ZString IACECusEntryLine.ForeignExporterMID
		{
			get { return RandomLine.ForeignExporterMID; }
		}

		ZString IACECusEntryLine.DeliveredToPartyCode
		{
			get { return OrgHeaderWrapper.GetCustomsRelatedCode(RandomLine.ShipToParty, OrgMatchedCustomsRegNoType.EIN); }
		}

		ZDecimal IACECusEntryLine.SupCustomsValue
		{
			get { return US_SupCustomsValue.Round(0); }
		}

		ZString ICusEntryLine.ADDCaseRateTypeQualifier
		{
			get
			{
				var result = ZString.Empty;

				if (this.IsCombinedLine())
				{
					result = InvoiceLineWithADDDetails?.US_ADDDepositRateIndicator ?? ZString.Empty;
				}
				else
				{
					result = RandomLine.US_ADDDepositRateIndicator;
				}

				return result.Right(1);
			}
		}

		ZDecimal IACECusEntryLine.ADDQuantity => Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.US_ADDQty);

		ZString IACECusEntryLine.ADDDecID
		{
			get { return RandomLine.US_ADDDecID; }
		}

		ZString ICusEntryLine.CVDCaseRateTypeQualifier
		{
			get
			{
				var result = ZString.Empty;

				if (this.IsCombinedLine())
				{
					result = InvoiceLineWithCVDDetails?.US_CVDDepositRateIndicator ?? ZString.Empty;
				}
				else
				{
					result = RandomLine.US_CVDDepositRateIndicator;
				}

				return result.Right(1);
			}
		}

		ZDecimal IACECusEntryLine.CVDQuantity => Helper.SumupLineTotalsForEntry<JobComInvoiceLine>(x => x.US_CVDQty);

		IEnumerable<ICensusWarningOverride> IACECusEntryLine.CensusWarningOverrideCodes
		{
			get
			{
				var list = new List<ICensusWarningOverride>();

				list.AddRange(new TypedEnumerable<ICensusWarningOverride>(RandomLine.CensusWarningOverrides));

				foreach (IACECusEntryLine secondaryLine in ChildSecondaryEntryLines)
				{
					foreach (var cwo in secondaryLine.CensusWarningOverrideCodes)
					{
						if (!list.Exists(x => x.OverrideCode == cwo.OverrideCode && x.ConditionCode == cwo.ConditionCode))
						{
							list.Add(cwo);
						}
					}
				}

				return list;
			}
		}

		IEnumerable<KeyValuePair<ZString, ZString>> IACECusEntryLine.LicenceTypeAndNumbers
		{
			get
			{
				List<LicenceAndPermit> permits = RandomLine.LicenceAndPermits.GetElementsInOrder();

				foreach (LicenceAndPermit permit in permits)
				{
					if (permit.CY_Code != LicencePermitTypeList.Codes.KR)
					{
						yield return new KeyValuePair<ZString, ZString>(permit.CY_Code, permit.CY_Data);
					}
				}

				foreach (var childLine in RandomLine.ChildLines.Where(x => !x.IsSetVLine))
				{
					foreach (var childPermit in childLine.LicenceAndPermits.GetElementsInOrder())
					{
						if (childPermit.CY_Code != LicencePermitTypeList.Codes.KR)
						{
							yield return new KeyValuePair<ZString, ZString>(childPermit.CY_Code, childPermit.CY_Data);
						}
					}
				}
			}
		}

		ZBool IACECusEntryLine.IsADDBonded
		{
			get { return RandomLine.US_IsBondedADD; }
		}

		ZBool IACECusEntryLine.IsCVDBonded
		{
			get { return RandomLine.US_IsBondedCVD; }
		}

		IEnumerable<KeyValuePair<ZString, ZString>> IACECusEntryLine.AdditionalDeclarationDetails
		{
			get
			{
				var additionalDetailReportedList = new HashSet<KeyValuePair<ZShort, ZString>>();
				var randomLine = RandomLine;
				if (IsSoftwoodLumberSection804FarmBillRequirement)
				{
					var iCusEntryLine = (ICusEntryLine)this;
					var additionalDecInfo = (iCusEntryLine.IsSoftwoodLumberImporterDeclaration ? "Y" : "N") + iCusEntryLine.SoftwoodLumberExportPrice.ToString(0).PadLeft(10, '0');
					var exportCharges = iCusEntryLine.SoftwoodLumberExportCharges;
					if (exportCharges > 0m)
					{
						additionalDecInfo += exportCharges.ToString(0).PadLeft(10, '0');
					}

					yield return new KeyValuePair<ZString, ZString>(AdditionalDeclarationTypeCodeList.Codes._01, additionalDecInfo);
				}

				foreach (var additionalDetail in GetAdditionalDeclarationDetailFromRandomLine(this, randomLine))
				{
					additionalDetailReportedList.Add(new KeyValuePair<ZShort, ZString>(CL_LineNumber, additionalDetail.Key));
					yield return additionalDetail;
				}

				if (this.IsCombinedLine())
				{
					foreach (var line in ChildSecondaryEntryLines.Where(x => x.US_SupLine))
					{
						var lineRandomLine = line.RandomLine;
						foreach (var additionalDetail in GetAdditionalDeclarationDetailFromRandomLine(line, lineRandomLine))
						{
							if (additionalDetailReportedList.Add(new KeyValuePair<ZShort, ZString>(line.CL_LineNumber, additionalDetail.Key)))
							{
								yield return additionalDetail;
							}
						}
					}
				}
			}
		}

		IEnumerable<KeyValuePair<ZString, ZString>> GetAdditionalDeclarationDetailFromRandomLine(CusEntryLine entryLine, JobComInvoiceLine randomLine)
		{
			if (!randomLine.US_ProductExclusion.IsEmpty && !randomLine.US_ExclusionNumber.IsEmpty)
			{
				yield return new KeyValuePair<ZString, ZString>(randomLine.US_ProductExclusion, randomLine.US_ExclusionNumber);
			}

			if (randomLine.KRExportSteelCertificateNumber != null)
			{
				yield return new KeyValuePair<ZString, ZString>(AdditionalDeclarationTypeCodeList.Codes._04, randomLine.KRExportSteelCertificateNumber.CY_Data);
			}

			if (randomLine.IsCBMAProductClaim)
			{
				yield return new KeyValuePair<ZString, ZString>(AdditionalDeclarationTypeCodeList.Codes._05, CreateCBMAProductDetail(entryLine, randomLine));
			}

			if (randomLine.US_ADD_Cert)
			{
				yield return new KeyValuePair<ZString, ZString>(AdditionalDeclarationTypeCodeList.Codes._06, ADCVDCert);
			}

			if (randomLine.IsAluminumSmeltAndCastCountryClaimed)
			{
				yield return new KeyValuePair<ZString, ZString>(AdditionalDeclarationTypeCodeList.Codes._07, CreateAluminumSmeltAndCastCountryDetail(randomLine));
			}

			if (!randomLine.US_RN_NKMeltCtry.IsEmpty)
			{
				yield return new KeyValuePair<ZString, ZString>(AdditionalDeclarationTypeCodeList.Codes._08, CreateMeltAndPourCountryDetail(randomLine));
			}

			if (randomLine.US_SupTariff == Tariff99034529)
			{
				yield return new KeyValuePair<ZString, ZString>(AdditionalDeclarationTypeCodeList.Codes._09, _201BIFACCERT);
			}
			else if (randomLine.US_SupTariff == Tariff99039109)
			{
				yield return new KeyValuePair<ZString, ZString>(AdditionalDeclarationTypeCodeList.Codes._10, _301STSCERT);
			}
		}
		const string ADCVDCert = "ADCVD CERT";
		internal const string Tariff99034529 = "99034529";
		internal const string Tariff99039109 = "99039109";
		internal const string _201BIFACCERT = "201BIFAC CERT";
		internal const string _301STSCERT = "301STS CERT";

		public ZDecimal TotalAllocationQuantity => Factory.GetValue(ref totalAllocationQuantityCached, () => InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.US_AllocationQuantity));
		CachedProperty<ZDecimal> totalAllocationQuantityCached;

		ZString CreateCBMAProductDetail(CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
		{
			var result = new ZStringBuilder();
			var isCBMA23Effective = invoiceLine.IsCBMA23Effective;
			var controlledGroupName = isCBMA23Effective ? ZString.Empty.PadLeft(AutoUSAddInfo.Schema.US_ControlledGroupNameMaxLength, 'X') : invoiceLine.US_ControlledGroupName;
			result.Append(controlledGroupName.Left(10).PadRight(10));
			result.Append(invoiceLine.US_FPI.Left(18).PadRight(18));
			result.Append(invoiceLine.ManufacturerCompanyName.Left(21).PadRight(21));
			var totalAllocationQuantity = isCBMA23Effective ? ZString.Empty : new ZString(entryLine.TotalAllocationQuantity.Truncate(4).ToString(4).Replace(".", ""));
			result.Append(totalAllocationQuantity.Length > 12 ? ZString.Empty.PadRight(12, '*') : totalAllocationQuantity.PadLeft(12, '0'));
			result.Append(invoiceLine.US_FlavorContentCreditInd ? YesNoDefaultList.Codes.Yes : " ");
			var ttbRateDesignationCode = isCBMA23Effective ? invoiceLine.US_TTBRateDesignationCode : invoiceLine.US_TaxRateS;
			result.Append(ttbRateDesignationCode.Left(6).PadLeft(6));
			var ttbTaxRate = new ZString(invoiceLine.TTBConfirmationRate.Truncate(4).ToString(4)).Replace(".", "");
			result.Append(ttbTaxRate.Length > 8 ? ZString.Empty.PadRight(8, '*') : ttbTaxRate.PadLeft(8, '0'));
			return result.ToString();
		}

		ZString CreateAluminumSmeltAndCastCountryDetail(JobComInvoiceLine invoiceLine)
		{
			var result = new ZStringBuilder();
			var isALUSMELT2024Effective = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.ALUSMELT2024, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);
			result.Append(GenerateApplicableCode(invoiceLine.US_Prim_NA));
			result.Append(invoiceLine.US_RN_NKPrimCtry.PadRight(2));
			result.Append(GenerateApplicableCode(invoiceLine.US_Sec_NA));
			result.Append(invoiceLine.US_RN_NKSecCtry.PadRight(2));
			result.Append(invoiceLine.US_RN_NKCastCtry.PadRight(2));
			return result.ToString();

			string GenerateApplicableCode(bool isTicked)
			{
				if (isALUSMELT2024Effective)
				{
					return (isTicked ? "N" : "Y").PadRight(3);
				}
				return isTicked ? "N/A" : string.Empty.PadRight(3);
			}
		}

		ZString CreateMeltAndPourCountryDetail(JobComInvoiceLine invoiceLine)
		{
			var meltCountry = invoiceLine.US_RN_NKMeltCtry;
			return meltCountry.EqualsIgnoringCase(ACEImportAddInfoJobComInvoiceLineValidation.MeltCountryOther) ? "  OTH" : meltCountry;
		}

		ZDecimal ICusEntryLine.ExciseTax
		{
			get
			{
				var lines = GetAllEntryLinesPerEntrySummary40Record();
				return lines.Sum(x => x.ExciseTax);
			}
		}

		ZString IACECusEntryLine.IRTaxCode
		{
			get
			{
				var lines = GetAllEntryLinesPerEntrySummary40Record();
				foreach (CusEntryLine entryLine in lines)
				{
					var taxCode = GetIRTaxCode(entryLine);
					if (!taxCode.IsEmpty)
					{
						return taxCode;
					}
				}
				return ZString.Empty;
			}
		}

		static ZString GetIRTaxCode(CusEntryLine entryLine)
		{
			var invoiceLine = entryLine.RandomLine;
			var importTariff = entryLine.ImportTariff;

			return !invoiceLine.US_TaxCode.IsEmpty ? invoiceLine.US_TaxCode : (importTariff != null ? importTariff.GetUniqueTaxCode() : ZString.Empty);
		}

		ZBool IACECusEntryLine.IRTaxMandatory
		{
			get { return GetAllEntryLinesPerEntrySummary40Record().Any(x => IsIRTaxRequired(x)); }
		}

		public bool IsPRCoffeeFeeMandatory
		{
			get
			{
				var importTariff = ImportTariff;
				return importTariff != null && importTariff.PRCoffeeFeeMightBeRequired && ((IDutyData)this).IsClearedInPR && !IsSetVLine;
			}
		}

		static bool IsIRTaxRequired(CusEntryLine entryLine)
		{
			return entryLine.ImportTariff != null && entryLine.ImportTariff.IsTaxRequired;
		}

		/// <summary>
		/// Parent line and its secondary lines
		/// </summary>
		/// <returns></returns>
		IEnumerable<CusEntryLine> GetAllEntryLinesPerEntrySummary40Record()
		{
			var result = (IEnumerable<CusEntryLine>)new CusEntryLine[] { this };
			result = result.Concat(ChildSecondaryEntryLines);
			return result;
		}

		#endregion

		#region ISimplifiedEntryLine Members

		IEnumerable<ISimplifiedEntryOrganisationDetails> ISimplifiedEntryLine.Entities
		{
			get
			{
				var result = new List<ISimplifiedEntryOrganisationDetails>();
				var header = Header;
				if (header != null)
				{
					var invoiceLine = RandomLine;
					if (invoiceLine.ManufacturerAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, invoiceLine.JI_OA_ManufacturerAddressInfo, EntityCodeList.Codes.ManufacturerSupplier);
					}
					if (invoiceLine.ConsigneeAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, invoiceLine.JI_OA_ConsigneeAddressInfo, EntityCodeList.Codes.Consignee);
					}
					if (invoiceLine.SoldToPartyAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, invoiceLine.JI_OA_SoldToPartyAddressInfo, EntityCodeList.Codes.BuyingParty);
					}
					if (invoiceLine.SellerAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, invoiceLine.JI_OA_SellerInfo, EntityCodeList.Codes.SellingParty);
					}
					if (invoiceLine.ShipToPartyAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, invoiceLine.JI_OA_ShipToPartyAddressInfo, EntityCodeList.Codes.ShipToParty);
					}

					if (invoiceLine.InvoiceHeader is JobComInvoiceHeader invoice)
					{
						if (invoice.ExporterAddress != null)
						{
							ACECargoReleaseData.AddEntity(result, invoice.JZ_OA_ExporterAddressInfo, EntityCodeList.Codes.Exporter);
						}
						if (invoice.ShipperAddress != null)
						{
							ACECargoReleaseData.AddEntity(result, invoice.JZ_OA_ShipperAddressInfo, EntityCodeList.Codes.Shipper);
						}
						if (invoice.DistributorAddress != null)
						{
							ACECargoReleaseData.AddEntity(result, invoice.JZ_OA_DistributorAddressInfo, EntityCodeList.Codes.Distributor);
						}
						if (invoice.PackagerAddress != null)
						{
							ACECargoReleaseData.AddEntity(result, invoice.JZ_OA_PackagerAddressInfo, EntityCodeList.Codes.Packager);
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region IFTZLine Members

		ZString IFTZLine.Tariff
		{
			get { return CL_AdValoremTariff; }
		}

		ZInt IFTZLine.LineNumber
		{
			get { return CL_LineNumber; }
		}

		ZString IFTZLine.SecondarySPI
		{
			get { return RandomLine.US_SecondarySPI; }
		}

		ZString IFTZLine.CountryOfOrigin
		{
			get { return RandomLine.US_UC_NKCountryOfOrigin; }
		}

		ZDecimal IFTZLine.Quantity1
		{
			get { return CustomsUnitQty == ABIUnitOfMeasureList.Codes.NoUnitRequired ? ManifestQuantity : GetCustomsQuantity1Rounded(); }
		}

		ZString IFTZLine.UQ1
		{
			get { return CustomsUnitQty; }
		}

		ZDecimal IFTZLine.Quantity2
		{
			get { return GetSecondCustomsQuantityRounded(); }
		}

		ZString IFTZLine.UQ2
		{
			get { return SecondCustomsUnitQty; }
		}

		ZString IFTZLine.QuotaCategory
		{
			get { return RandomLine.US_TextileCategoryNo; }
		}

		ZString IFTZLine.PNDisclaimer
		{
			get { return RandomLine.US_F_PNDisclaimer; }
		}

		ZDecimal IFTZLine.Weight
		{
			get { return IsACEFTZSupLine ? ZDecimal.Zero : GetGrossWeightInKilograms(true); }
		}

		ZDecimal IFTZLine.Value
		{
			get { return (IsACEFTZSupLine ? ZDecimal.Zero : CL_CustomsValue); }
		}

		ZString IFTZLine.ZoneStatus
		{
			get { return (IsACEFTZSupLine ? (ZString)"0" : RandomLine.US_ZoneStatus); }
		}

		ZDecimal IFTZLine.HMF
		{
			get { return (IsACEFTZSupLine ? ZDecimal.Zero : HMFAmount); }
		}

		ZString IFTZLine.ImporterReferenceID
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.ImporterOfRecordNumber : ZString.Empty;
			}
		}

		ZString IFTZLine.ManufacturerReferenceID
		{
			get
			{
				var invoiceLine = RandomLine;
				return invoiceLine.ManufacturerAddress != null ? OrgHeaderWrapper.GetCustomsCodeFromAddress(invoiceLine.ManufacturerAddress, OrgCusCode.USACodeTypes.ManufacturerID) : ZString.Empty;
			}
		}

		ZString IFTZLine.MiscPermitQualifer
		{
			get { return RandomLine.US_LicenseType; }
		}

		ZString IFTZLine.MiscPermitNumber
		{
			get { return RandomLine.US_MiscPermitNo; }
		}

		ZString IFTZLine.Remarks
		{
			get { return RandomLine.JI_Description; }
		}

		ZString IFTZLine.LongSPICode
		{
			get
			{
				var invoiceLine = RandomLine;
				return invoiceLine.US_SPI.Length > 1 && IsInSPIList ? invoiceLine.US_SPI : ZString.Empty;
			}
		}

		bool IsInSPIList
		{
			get
			{
				var spiList = SPICompleteList.GetCachedListWithoutNotApplicable(Factory);
				return spiList.ContainsCode(RandomLine.US_SPI);
			}
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.PSCReasonCodes, typeof(PSCReasonCusCodeData));
			return result;
		}

		#endregion

		#region FTZ 214 Print Members

		IForeignRegionalDistrictPort FTZForeignPortOfLading
		{
			get { return CountryNameCalculator.GetForeignOrRegionalPort(((ICusEntryLine)this).PortOfLading, Factory); }
		}

		public ZString FTZForeignPortOfLadingCode
		{
			get { return FTZForeignPortOfLading?.PortCode ?? ZString.Empty; }
		}

		public ZString FTZForeignPortOfLadingName
		{
			get { return FTZForeignPortOfLading?.PortName ?? ZString.Empty; }
		}

		public ZString FTZTariff
		{
			get
			{
				ZString result = FormattedTariff;
				if (Header.FTZPrintZones)
				{
					result += " (Zone " + ((IFTZLine)this).ZoneStatus + ")";
				}
				return result;
			}
		}

		public ZString FTZQuantityAndUnit
		{
			get { return ZString.Format("{0} {1}", CustomsQuantity.ToString(2), CustomsUnitQty); }
		}

		public ZString FTZSecondQuantityAndUnit
		{
			get
			{
				ZDecimal secondQty = SecondCustomsQuantity;
				return secondQty > 0.0m ? ZString.Format("{0} {1}", secondQty.ToString(2), SecondCustomsUnitQty) : ZString.Empty;
			}
		}

		public ZString FTZWhsPkgConvertedInvoiceUnit => RandomLine.ConvertedInvoiceUnit;

		public ZDecimal FTZWhsPkgQty
		{
			get
			{
				var whsPkgQty = ZDecimal.Zero;

				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					whsPkgQty += line.JI_Calc_BondedWhsQuantity;
				}
				return whsPkgQty;
			}
		}

		public ZString FTZWarehousePackageQtyAndUnit => FTZWhsPkgQty > 0.0m ? ZString.Format("{0} {1}", FTZWhsPkgQty.ToString(0), FTZWhsPkgConvertedInvoiceUnit) : ZString.Empty;

		public ZString FTZWeightAndUnit
		{
			get
			{
				var weight = EffectiveGrossWeight;
				return ZString.Format("{0} {1}", weight.Amount.Round(0).ToString(0), weight.Unit);
			}
		}

		public ZDecimal FTZCustomsValue
		{
			get { return !IsSetXLine ? CL_CustomsValue : ZDecimal.Zero; }
		}

		public ZString FTZDescription
		{
			get { return CL_Description.Replace(System.Environment.NewLine, " "); }
		}

		ZInt IPGAGovernmentAgenciesCommon.LineNumber
		{
			get { return CL_LineNumber; }
		}

		ZString IPGAGovernmentAgenciesCommon.Tariff
		{
			get { return CL_AdValoremTariff; }
		}

		#endregion

		#region IEntryLineDutyDataProvider Members
		IFees IEntryLineDutyDataProvider.Fees => Fees;
		IInvoiceLineDutyDataProvider[] IEntryLineDutyDataProvider.InvoiceLines => InvoiceLines.Cast<IInvoiceLineDutyDataProvider>().ToArray();
		IEntryLineDutyDataProvider IEntryLineDutyDataProvider.ParentLine => ParentLine;
		#endregion

		#region IEntryLine Members
		bool IEntryLine.IsFTZAdmission => Declaration?.IsFTZAdmission ?? false;
		IEntryLine IEntryLine.ParentLine => ParentLine;
		IInvoiceLine IEntryLine.RandomLine => RandomLine;
		IInvoiceLine IEntryLine.FirstInvoiceLineAfterSortedOnInvoiceLineNo => FirstInvoiceLineAfterSortedOnInvoiceLineNo;
		IEnumerable<IEntryLine> IEntryLine.AllRelatedEntryLines => Header?.MergedLines.Cast<IEntryLine>() ?? Enumerable.Empty<IEntryLine>();
		IEnumerable<IInvoiceLine> IEntryLine.InvoiceLines => InvoiceLines.Cast<IInvoiceLine>();
		bool IEntryLine.IsACE => Declaration?.IsACE ?? false;
		bool IEntryLine.IsCustomsChargeToBeCalculated => Header?.IsCustomsChargeToBeCalculated ?? true;
		bool IEntryLine.IsConsumptionFTZ => Declaration?.IsConsumptionFTZ ?? false;
		ZString IEntryLine.BaseDutyRateDesc => base.US_DutyRateDesc;
		IEnumerable<IEntryLine> IEntryLine.ChildSecondaryEntryLines => ChildSecondaryEntryLines;
		#endregion

		#region ICusDispositionParent
		public QuotaCusDispositionCollection QuotaDispositions
		{
			get
			{
				if (fQuotaDisposition == null)
				{
					fQuotaDisposition = new QuotaCusDispositionCollection(this);
					fQuotaDisposition.Load();
				}
				return fQuotaDisposition;
			}
		}
		QuotaCusDispositionCollection fQuotaDisposition;

		ZString Customs.Business.ICusDispositionParent.GetStatusDescription(ZString quotaStatusCode)
		{
			return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, quotaStatusCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USQuotaDispositions);
		}

		ZString Customs.Business.ICusDispositionParent.Type => Customs.Business.CusDispositionTypeCodeList.Codes.USQuotaLineStatus;

		ZString Customs.Business.ICusDispositionParent.ParentTableCode => CusEntryLineSchema.Constants.Prefix;

		BusinessObject Customs.Business.ICusDispositionParent.CollectionMaster => this;

		#endregion

		#region AESCusDispositions

		public AESCusDispositionCollection AESCusDispositions
		{
			get
			{
				if (aesCusDispositionCollection == null)
				{
					aesCusDispositionCollection = new AESCusDispositionCollection(this);
					aesCusDispositionCollection.Load();
				}
				return aesCusDispositionCollection;
			}
		}
		AESCusDispositionCollection aesCusDispositionCollection;

		#endregion
	}
}
