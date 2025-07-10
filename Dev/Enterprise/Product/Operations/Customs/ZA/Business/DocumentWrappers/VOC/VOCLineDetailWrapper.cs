using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	internal class VOCLineDetailWrapper : NonPersistentBusinessObject, IDocumentWrapper
	{
		public VOCLineDetailWrapper(ILineLevelInformation entryLineProvider, CusEntryHeader header, BusinessObjectFactory factory) : base(factory)
		{
			LineLevelInformationProvider = entryLineProvider;
			LinkedCusEntryHeader = header;
		}

		ILineLevelInformation LineLevelInformationProvider { get; }

		#region Related Object

		CusEntryHeader LinkedCusEntryHeader { get; }
		CusEntryLine ProviderAsEntryLine => LineLevelInformationProvider as CusEntryLine;
		CUSDECMessageSG30Helper ProviderAsSG30 => LineLevelInformationProvider as CUSDECMessageSG30Helper;

		IEnumerable<CusLineTariffDetail> InvoiceLineTariffDetails
		{
			get
			{
				if (invoiceLineTariffDetails == null)
				{
					CusEntryLine entryLine = ProviderAsEntryLine
						?? DocumentWrapperHelper.FindMatchingEntryLine(LinkedCusEntryHeader, LineLevelInformationProvider);

					invoiceLineTariffDetails = entryLine?.RandomLine?.CusLineTariffDetails?.OfType<CusLineTariffDetail>();
					invoiceLineTariffDetails = invoiceLineTariffDetails ?? System.Array.Empty<CusLineTariffDetail>();
				}
				return invoiceLineTariffDetails;
			}
		}
		IEnumerable<CusLineTariffDetail> invoiceLineTariffDetails;

		#endregion

		#region Properties

		public ZString PreviousProcedureMRN => LineLevelInformationProvider.PreviousProcedureMRN;
		public ZString WarehousingMRNLineNumber => LineLevelInformationProvider.WarehousingMRNLineNumber;

		#region Row Head
		public ZString LineNumber => LineLevelInformationProvider.LineNumber;

		public ZString CountryOfOrigin => LineLevelInformationProvider.CountryOfOrigin;

		#endregion

		#region Tariff Code

		public ZString TariffCode => DocumentWrapperHelper.FormatTariffCode(LineLevelInformationProvider.TariffCode);

		public ZString ProcedureMeasure => DocumentWrapperHelper.FormatNonDutyTariffCode(LineLevelInformationProvider.ProcedureMeasure);

		public ZString AntiDumpingTariffCode => DocumentWrapperHelper.FormatNonDutyTariffCode(InvoiceLineTariffDetails.WhereRateType(RateTypes.AntiDumping)?.FirstOrDefault()?.UniversalTariff?.GetTariffCodeWithCheckDigit() ?? ZString.Empty);

		public ZString AdValoremExciseTariffCode => DocumentWrapperHelper.FormatNonDutyTariffCode(InvoiceLineTariffDetails.WhereRateType(RateTypes.AdValoremExcise)?.FirstOrDefault()?.UniversalTariff?.GetTariffCodeWithCheckDigit() ?? ZString.Empty);

		#endregion

		#region Quantity and code and ProcedureCodes

		public ZDecimal CustomsQuantity => LineLevelInformationProvider?.CustomsQuantity ?? ZDecimal.Zero;

		public ZString CustomsUnitQty => LineLevelInformationProvider?.CustomsUnitQty ?? ZString.Empty;

		public ZDecimal AdditionalQuantity => LineLevelInformationProvider?.AdditionalQuantity ?? ZDecimal.Zero;

		public ZString AdditionalUnitQty => LineLevelInformationProvider?.AdditionalUnitQty ?? ZString.Empty;

		public ZDecimal ClassificationQuantity => LineLevelInformationProvider?.ClassificationQuantity ?? ZDecimal.Zero;

		public ZString ClassificationUnitQty => LineLevelInformationProvider?.ClassificationUnitQty ?? ZString.Empty;

		public ZDecimal WarehouseCountableQuantity => LineLevelInformationProvider?.WarehouseCountableQuantity.Truncate() ?? ZDecimal.Zero;

		public ZString WarehouseCountableUnitQty => LineLevelInformationProvider?.WarehouseCountableUnitQty ?? ZString.Empty;

		public ZString CustomsProcedureCode => LineLevelInformationProvider.CustomsProcedureCode;

		public ZString PreviousProcedureCode => LineLevelInformationProvider.PreviousProcedureCode;

		#endregion

		#region Amounts

		public ZDecimal CustomsValue => LineLevelInformationProvider.CustomsValue;

		CalcFeeValues LineCalcFeeValues
		{
			get
			{
				if (!lineCalcFeeValues.HasValue)
				{
					lineCalcFeeValues = ProviderAsEntryLine?.GetCalcFeeValues() ?? ProviderAsSG30?.FeeValues ?? new CalcFeeValues();
				}
				return lineCalcFeeValues.Value;
			}
		}
		CalcFeeValues? lineCalcFeeValues;

		public ZDecimal CustomsDutyExcluding12B => LineCalcFeeValues.CustomsDutyExcluding12B;

		public ZDecimal S1P2BDuty => LineCalcFeeValues.S1P2BDuty;

		public ZDecimal ValueAddedTax => LineCalcFeeValues.ValueAddedTax;

		public ZDecimal ProvisionalPaymentsAndPenalties => LineCalcFeeValues.ProvisionalPayment + LineCalcFeeValues.Penalty;

		#endregion

		public ZString GoodsDescription => LineLevelInformationProvider.GoodsDescription;

		public ZDecimal ActualPrice => LineLevelInformationProvider.ActualPrice;

		public ZString TradeStatisticsIndicator => LineLevelInformationProvider.TradeStatisticsIndicator;

		public BusinessObjectCollectionWrapper<AdditionalInformationDocWrapper> AdditionalInfos
		{
			get
			{
				if (additionalInfos == null)
				{
					additionalInfos = new BusinessObjectCollectionWrapper<AdditionalInformationDocWrapper>();
					additionalInfos.AddRange(LineLevelInformationProvider.AdditionalInformations.Select(x => new AdditionalInformationDocWrapper(x)));
				}
				return additionalInfos;
			}
		}
		BusinessObjectCollectionWrapper<AdditionalInformationDocWrapper> additionalInfos;

		#endregion
	}
}
