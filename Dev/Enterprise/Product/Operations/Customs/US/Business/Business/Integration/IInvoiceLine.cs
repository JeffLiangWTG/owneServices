using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IInvoiceLine : Customs.Business.IBaseInvoiceLine
	{
		ZString BaseJI_PartNo { get; }
		ZString US_SupTariff { get; }
		ZString US_SupAdditionalTariff1 { get; }
		ZString US_SupAdditionalTariff2 { get; }
		ZString US_SupAdditionalTariff3 { get; }
		ZString US_SupAdditionalTariff4 { get; }
		ZString US_SupAdditionalTariff5 { get; }
		ZDecimal US_SupQty1 { get; }
		ZString US_SupUQ1 { get; }
		ZDecimal US_SupQty2 { get; }
		ZString US_SupUQ2 { get; }
		ZDecimal US_SupQty3 { get; }
		ZString US_SupUQ3 { get; }
		ZDecimal US_SupAdditionalTariff1Qty { get; }
		ZString US_SupAdditionalTariff1UQ { get; }
		ZDecimal US_SupAdditionalTariff2Qty { get; }
		ZString US_SupAdditionalTariff2UQ { get; }
		ZDecimal US_SupAdditionalTariff3Qty { get; }
		ZString US_SupAdditionalTariff3UQ { get; }
		ZDecimal US_SupAdditionalTariff4Qty { get; }
		ZString US_SupAdditionalTariff4UQ { get; }
		ZDecimal US_SupAdditionalTariff5Qty { get; }
		ZString US_SupAdditionalTariff5UQ { get; }
		ZDecimal US_CustomsValue { get; }
		ZDecimal JI_CustomsValue { get; }
		ZDecimal TotalOriginalGoodsValueInUSD { get; }
		ZDecimal US_98GoodsValue { get; }
		ZDecimal US_98ValueInvCurr { get; }
		ZString US_ZoneStatus { get; }
		ZDateTime US_PrivilegedStatusDate { get; }
		CurrencyConverter CurrencyConverter { get; }
		ZDate EffectiveDateForDutyRate { get; }
		new IInvoiceHeader InvoiceHeader { get; }
		ZString US_SetInd { get; }
		ZString US_SecondarySPI { get; }
		ZBool IsVParentLine { get; }
		ZBool IsVChildLine { get; }
		ZBool IsSetXLine { get; }
		ZBool IsSetVLine { get; }
		IInvoiceLine ParentTariffLine { get; }
		IInvoiceLine ProductParentTariffLine { get; }
		IEnumerable<IInvoiceLine> ChildLines { get; }
		IEnumerable<IInvoiceLine> ChildVLines { get; }
		USCTariff ImportSupTariff { get; }
		RefCurrency Invoice_Currency { get; }
		ZDate FTZAdmissionEffectiveDateForDutyRate { get; }
		ZDate ImportEffectiveDateForDutyRate { get; }
		USCTariff ImportSupAdditionalTariff1 { get; }

		bool IsACE { get; }
		bool IsRecon { get; }
		bool HasDeclaration { get; }
		bool HasEmptySupTariff { get; }
	}
}
