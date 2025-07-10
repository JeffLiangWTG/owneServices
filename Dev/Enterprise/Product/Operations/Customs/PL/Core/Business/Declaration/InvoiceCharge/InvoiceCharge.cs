using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.PL.Business.Declaration;

public class InvoiceCharge : EU.Business.Declaration.InvoiceCharge, Integration.Customs.PL.IInvoiceCharge
{
	public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override bool ShouldResetDefaultIsIncludedInAmount(ZString incoTerm, ICustomsChargeCode charge) => true;
}
