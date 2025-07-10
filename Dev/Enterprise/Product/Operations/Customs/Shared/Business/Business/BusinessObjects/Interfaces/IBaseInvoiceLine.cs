using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IBaseInvoiceLine
	{
		BusinessObjectFactory Factory { get; }
		ZGuid PK { get; }
		ZShort JI_LineNo { get; }
		ZString JI_PartNo { get; }
		ZString JI_Tariff { get; }
		ZDecimal JI_CustomsQuantity { get; }
		ZString JI_CustomsUnitQty { get; }
		ZString InvoiceNumber { get; }
		ZDecimal JI_CustomsSecondQuantity { get; }
		ZString JI_CustomsSecondUnitQty { get; }
		ZDecimal JI_CustomsThirdQuantity { get; }
		ZString JI_CustomsThirdUnitQty { get; }
		IBaseInvoiceHeader InvoiceHeader { get; }
	}
}
