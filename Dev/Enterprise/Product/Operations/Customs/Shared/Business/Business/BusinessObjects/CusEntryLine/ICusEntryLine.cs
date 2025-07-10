using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public interface ICusEntryLine : Integration.Customs.ICusEntryLine
	{
		ZDecimal CL_CustomsValue { get; }
		ZShort CL_LineNumber { get; }

		ZString Tariff { get; }
		ZDecimal CustomsQuantity { get; }
		ZString CustomsUnitQty { get; }
		ZString Description { get; }
		ZString ExtendedCommercialDescription { get; }

		ZDecimal BondedWarehouseQuantity { get; }
		ZString BondedWarehouseUnitQuantity { get; }

		#region Required by DocWrapper
		ZString FormattedTariff { get; }
		ZDecimal TotalLinePriceInLocalCurrency { get; }
		ZDecimal CL_DutyPercent { get; }
		ZString DutyRateDescription { get; }
		ZDecimal CL_WarehouseUnitValue { get; }
		Money CustomsValue { get; }
		ZDecimal DutyAmount { get; }
		ZDecimal GSTVATAmount { get; }
		ZDecimal GSTVATDeferred { get; }
		Money TotalLinePrice { get; }

		ZString CL_ParentTrailer { get; }

		InvoiceLinesForEntryLineCollection InvoiceLines { get; }
		BaseJobComInvoiceLine RandomLine { get; }
		#endregion
	}
}
