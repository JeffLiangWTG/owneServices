using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BaseInvoiceLineApportionedCharge : CommonApportionedCharge
	{
		public BaseInvoiceLineApportionedCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new static readonly TypeDecider TypeDecider = new InvoiceLineApportionedChargeTypeDecider();

		#region Related Objects

		public BaseJobComInvoiceLine InvoiceLine
		{
			get { return (BaseJobComInvoiceLine)base.Parent; }
		}

		#endregion

		protected override ZBool GetNeedCheckChargeType()
		{
			return !(InvoiceLine?.Declaration?.IsInterface ?? ZBool.False);
		}
	}
}
