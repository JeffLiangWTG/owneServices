using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class SADLineDetailWrapper : NonPersistentBusinessObject
	{
		#region Ctor

		public SADLineDetailWrapper(ILineLevelInformation cusEntryLine, CusEntryHeader header) : base((cusEntryLine as BusinessObject)?.Factory ?? new BusinessObjectFactory())
		{
			CusEntryLineProvider = cusEntryLine;
			linkedCusEntryHeader = header;
		}

		public SADLineDetailWrapper(ILineLevelInformation cusEntryLine, CusEntryHeader header, CUSDECEDIMessage sourceEDIMessage) : this(cusEntryLine, header)
		{
			this.sourceEDIMessage = sourceEDIMessage;
		}

		public ILineLevelInformation CusEntryLineProvider;
		CusEntryLine ProviderAsEntryLine => CusEntryLineProvider as CusEntryLine;
		CUSDECMessageSG30Helper ProviderAsSG30 => CusEntryLineProvider as CUSDECMessageSG30Helper;

		readonly CusEntryHeader linkedCusEntryHeader;
		readonly CUSDECEDIMessage sourceEDIMessage;

		#endregion

		public ZDecimal ActualPrice => CusEntryLineProvider?.ActualPrice ?? ZDecimal.Zero;

		public ZDecimal AdditionalQuantity => CusEntryLineProvider?.AdditionalQuantity ?? ZDecimal.Zero;

		public ZString AdditionalUnitQty => CusEntryLineProvider?.AdditionalUnitQty ?? ZString.Empty;

		public ZDecimal ClassificationQuantity => CusEntryLineProvider?.ClassificationQuantity ?? ZDecimal.Zero;

		public ZString ClassificationUnitQty => CusEntryLineProvider?.ClassificationUnitQty ?? ZString.Empty;

		public ZDecimal CustomsQuantity => CusEntryLineProvider?.CustomsQuantity ?? ZDecimal.Zero;

		public ZString CustomsUnitQty => CusEntryLineProvider?.CustomsUnitQty ?? ZString.Empty;

		public ZString GoodsDescription => CusEntryLineProvider?.GoodsDescription ?? ZString.Empty;

		public ZString PreviousProcedureMRN => CusEntryLineProvider?.PreviousProcedureMRN ?? ZString.Empty;

		public ZString RebateCodeFormatted => DocumentWrapperHelper.FormatNonDutyTariffCode(CusEntryLineProvider?.ProcedureMeasure ?? ZString.Empty);

		public ZString RebateUserCode => CusEntryLineProvider?.RebateUserCode ?? ZString.Empty;

		public ZString TradeStatisticsIndicator => CusEntryLineProvider?.TradeStatisticsIndicator ?? ZString.Empty;

		public ZDecimal WarehouseCountableQuantity => CusEntryLineProvider?.WarehouseCountableQuantity.Truncate() ?? ZDecimal.Zero;

		public ZString WarehouseCountableUnitQty => CusEntryLineProvider?.WarehouseCountableUnitQty ?? ZString.Empty;

		public ZString LineNumberFormatted => (CusEntryLineProvider?.LineNumber ?? ZString.Empty).TrimStart(new char[] { '0' });

		public ZString TariffCodeFormatted => DocumentWrapperHelper.FormatTariffCode(CusEntryLineProvider?.TariffCode ?? ZString.Empty);

		public ZString CustomsProcedureCode => CusEntryLineProvider?.CustomsProcedureCode ?? ZString.Empty;

		public ZString PreviousProcedureCode => CusEntryLineProvider?.PreviousProcedureCode ?? ZString.Empty;

		public ZString PreferenceCode => CusEntryLineProvider?.PreferenceCode ?? ZString.Empty;

		public ZString CountryOfOrigin => CusEntryLineProvider?.CountryOfOrigin ?? ZString.Empty;

		public ZString WarehousingMRNLineNumber => CusEntryLineProvider?.WarehousingMRNLineNumber ?? ZString.Empty;

		public ZDecimal CustomsValue => CusEntryLineProvider?.CustomsValue ?? ZDecimal.Zero;

		public ZBool IsEmpty => CusEntryLineProvider == null;

		public AdditionalInformationDocWrapper LicenceNumberAddInfo
		{
			get
			{
				var readAddInfo = CusEntryLineProvider?.AdditionalInformations?.FirstOrDefault(input => AdditionalInfoReferenceList.FirstOrDefault(x => x.ZZD_Code == input.Code)?.HasAttribute(RefCusCodeListAttributeTypes.Codes.LicenceNumber) ?? false);
				return readAddInfo as AdditionalInformationDocWrapper ?? new AdditionalInformationDocWrapper(readAddInfo);
			}
		}

		IEnumerable<ZZRefCusCodeListCombined> AdditionalInfoReferenceList
		{
			get
			{
				if (additionalInfoReferenceList == null)
				{
					var refCodeCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, ZDateTime.Today);
					refCodeCollection.Load();
					additionalInfoReferenceList = refCodeCollection.OfType<ZZRefCusCodeListCombined>();
				}
				return additionalInfoReferenceList;
			}
		}
		IEnumerable<ZZRefCusCodeListCombined> additionalInfoReferenceList;

		public AdditionalInformationDocWrapper BondHolderCodeAddInfo
		{
			get
			{
				var readAddInfo = CusEntryLineProvider?.AdditionalInformations?.FirstOrDefault(input => input.Code == "BHR");
				return readAddInfo as AdditionalInformationDocWrapper ?? new AdditionalInformationDocWrapper(readAddInfo);
			}
		}

		public AdditionalInformationDocWrapper ProvisionalPaymentSuretyAddInfo
		{
			get
			{
				var readAddInfo = CusEntryLineProvider?.AdditionalInformations?.FirstOrDefault(input => input.Code == "PPS");
				return readAddInfo as AdditionalInformationDocWrapper ?? new AdditionalInformationDocWrapper(readAddInfo);
			}
		}

		public OrgHeader BondHolder
		{
			get
			{
				OrgHeader result = null;
				var code = BondHolderCodeAddInfo?.Value ?? ZString.Empty;
				if (!code.IsEmpty)
				{
					result = OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.BondHolderCode, code, Core.Constants.CountryCodes.SouthAfrica);
				}
				return result;
			}
		}

		public BusinessObjectCollectionWrapper<AdditionalInformationDocWrapper> GeneralAddInfos
		{
			get
			{
				if (generalAddInfos == null)
				{
					generalAddInfos = new BusinessObjectCollectionWrapper<AdditionalInformationDocWrapper>();
					var addInfosTobeAdded = (CusEntryLineProvider?.AdditionalInformations.Where(x => x.Code != LicenceNumberAddInfo.Code)
						.Select(addinfo => addinfo as AdditionalInformationDocWrapper ?? new AdditionalInformationDocWrapper(addinfo, CusEntryLineProvider.DateOfAssessment, CusEntryLineProvider.Factory)))?.ToList();
					if (addInfosTobeAdded != null)
					{
						addInfosTobeAdded.Sort(new AdditionalInformationComparer(AdditionalInfoReferenceList));
						generalAddInfos.AddRange(addInfosTobeAdded);
					}
				}
				return generalAddInfos;
			}
		}
		BusinessObjectCollectionWrapper<AdditionalInformationDocWrapper> generalAddInfos;

		public BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> CalcDutiesAndFees
		{
			get
			{
				if (calcDutiesAndFees == null)
				{
					calcDutiesAndFees = new BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper>();
					var calcFeeValues = ProviderAsEntryLine?.GetCalcFeeValues() ?? ProviderAsSG30?.FeeValues ?? new CalcFeeValues();

					if (IsNotIntoWarehouseWarehousing)
					{
						foreach (var duty in calcFeeValues.CustomsDutiesExcluding12B ?? Enumerable.Empty<IDutyFeeInformation>())
						{
							AddCalculatedDutyAndFeeIfNotEmpty(duty.Code, duty.Value);
						}

						AddCalculatedDutyAndFeeIfNotEmpty("VAT", calcFeeValues.ValueAddedTax);
					}

					AddCalculatedDutyAndFeeIfNotEmpty("12B", calcFeeValues.S1P2BDuty);
					AddCalculatedDutyAndFeeIfNotEmpty("PP's", calcFeeValues.ProvisionalPayment + calcFeeValues.Penalty);

					calcDutiesAndFees = SADDocumentWrapperHelper.ConsolidateDutiesIfNeeded(calcDutiesAndFees);
				}
				return calcDutiesAndFees;
			}
		}

		ZBool IsNotIntoWarehouseWarehousing
		{
			get
			{
				var linkedCusEntryHeader = this.linkedCusEntryHeader;
				return linkedCusEntryHeader != null && !linkedCusEntryHeader.IsIntoWarehouseWarehousing;
			}
		}

		void AddCalculatedDutyAndFeeIfNotEmpty(string type, ZDecimal amount)
		{
			if (!amount.IsEmpty)
			{
				calcDutiesAndFees.Add(new DutyFeeInformationDocWrapper(type, amount));
			}
		}

		BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> calcDutiesAndFees;

		public ZDecimal ATVAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				var applicableTaxRate = new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.SouthAfrica, "VAT", CusEntryInstruction.GetEffectiveAssessmentDate(linkedCusEntryHeader?.EntryInstruction, Factory))?.ZZF_Value ?? ZDecimal.Zero;
				if (!applicableTaxRate.IsEmpty)
				{
					var taxAmount = CalcDutiesAndFees.OfType<DutyFeeInformationDocWrapper>().FirstOrDefault(x => x.Code == "VAT")?.Value ?? ZDecimal.Zero;
					result = taxAmount / applicableTaxRate;
				}
				return result.RoundUsingCustomsValueRule();
			}
		}

		public ZDecimal VPBAmount
		{
			get
			{
				var result = ProviderAsEntryLine?.CL_VPBAmount
					?? sourceEDIMessage?.GetVPBAmountForLine(LineNumberFormatted);
				return result?.RoundUsingCustomsValueRule() ?? ZDecimal.Zero;
			}
		}

		public ZDecimal TotalDutyAndFees
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (DutyFeeInformationDocWrapper dutyFee in CalcDutiesAndFees)
				{
					result += dutyFee.Value;
				}
				return result;
			}
		}

		public ZString AdditionalCommodityCodeFormatted
		{
			get
			{
				var result = ZString.Empty;

				var entryLine = ProviderAsEntryLine
					?? DocumentWrapperHelper.FindMatchingEntryLine(linkedCusEntryHeader, CusEntryLineProvider);

				var invoiceLine = entryLine?.RandomLine;
				if (invoiceLine != null)
				{
					var lineTariff = invoiceLine.CusLineTariffDetails.Cast<CusLineTariffDetail>().WhereRateTypeIn(new ZString[] { Universal.Constants.RateTypes.Excise, Universal.Constants.RateTypes.AntiDumping, Universal.Constants.RateTypes.AdValoremExcise }).FirstOrDefault();
					if (lineTariff != null)
					{
						result = DocumentWrapperHelper.FormatNonDutyTariffCode(lineTariff.Tariff);
						var relatedTariff = lineTariff.UniversalTariff?.RelatedTariffs?.FirstOrDefault(x => (x.CusTariffType?.ZZI_TariffType ?? ZString.Empty) == UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
						if (relatedTariff != null)
						{
							result += "/" + DocumentWrapperHelper.FormatTariffCode(relatedTariff.ZZH_TariffCode);
						}
					}
				}
				return result;
			}
		}
	}
}
