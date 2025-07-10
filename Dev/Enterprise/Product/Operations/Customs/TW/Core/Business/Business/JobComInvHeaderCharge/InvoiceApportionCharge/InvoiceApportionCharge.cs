using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public partial class InvoiceApportionCharge : AutoInvoiceApportionCharge
	{
		public InvoiceApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZBool AllowNonWesternEuropeanCharacterForChargeDescription => true;
	}
}
