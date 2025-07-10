using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class CusDV1DetailProvider : IDeclarationOfValueLines
	{
		public CusDV1DetailProvider(CusEntryLine entryline, ZInt lineNo)
		{
			CusEntryLine = Argument.NotNull(entryline, nameof(entryline));
			LineNo = lineNo;
		}
		CusEntryLine CusEntryLine { get; }
		ZInt LineNo { get; }

		public int DeclarationOfValueLineNo => LineNo;
		public int DeclarationLineNo => CusEntryLine.CL_LineNumber;
		public decimal IndirectPayment => decimal.Zero; //TO DO : For now zero value it will be defined later
		public decimal Commission => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.True, TRIncotermChargeCodeList.Codes.COM);
		public decimal BrokerageCommission => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.True, TRIncotermChargeCodeList.Codes.Observation);
		public decimal PackageValue => decimal.Zero; //TO DO : For now zero value it will be defined later
		public decimal ImportMergedGoods => decimal.Zero; //TO DO : For now zero value it will be defined later
		public decimal VehiclesManufacturedForImport => decimal.Zero; //TO DO : For now zero value it will be defined later
		public decimal ProductionAndConsumptionGoodsForImport => decimal.Zero; //TO DO : For now zero value it will be defined later
		public decimal PlanDraft => decimal.Zero; //TO DO : For now zero value it will be defined later
		public decimal RoyaltyLicence => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.True, TRIncotermChargeCodeList.Codes.ROY);
		public decimal IndirectTransition => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.True, TRIncotermChargeCodeList.Codes.INT);
		public decimal Transportation => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.True, TRIncotermChargeCodeList.Codes.OFT);
		public decimal Insurance => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.True, TRIncotermChargeCodeList.Codes.ONS);
		public decimal AfterEntryTransportation => decimal.Zero; //TO DO : For now empty zero it will be defined later
		public decimal TechnicalAssistance => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.True, TRIncotermChargeCodeList.Codes.Surveillance);
		public decimal OtherPayments => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.True, TRIncotermChargeCodeList.Codes.OTH);
		public string OtherPaymentQualification => string.Empty; //TO DO : For now empty value it will be defined later
		public decimal TaxFeesFund => decimal.Zero; //TO DO : For now zero value it will be defined later
	}
}
