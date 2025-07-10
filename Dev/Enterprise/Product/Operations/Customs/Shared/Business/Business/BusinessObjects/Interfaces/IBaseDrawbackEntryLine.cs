using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IBaseDrawbackEntryLine : IBaseEntryLine
	{
		BusinessObjectFactory Factory { get; }

		ZString CL_Description { get; }
		ZString CustomsUnitQty { get; }
		ZString InvoiceUQ { get; }

		ZDecimal CustomsQuantity { get; }
		ZDecimal InvoiceQuantity { get; }
	}
}
