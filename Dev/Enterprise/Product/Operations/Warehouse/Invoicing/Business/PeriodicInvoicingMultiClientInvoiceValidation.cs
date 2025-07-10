using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public class PeriodicInvoicingMultiClientInvoiceValidation : ZValidation
	{
		public PeriodicInvoicingMultiClientInvoiceValidation(PeriodicInvoicingMultiClientInvoice parent)
			: base(parent)
		{
			Parent = parent;
		}

		readonly PeriodicInvoicingMultiClientInvoice Parent;

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidateInvoiceDate();
		}

		#endregion

		#region ValidateInvoiceDate

		public void ValidateInvoiceDate()
		{
			ValidateCalculatedProperty(Parent.InvoiceDateInfo);
		}

		protected void CheckInvoiceDate()
		{
			MandatoryValidation.CheckEntered(Parent.InvoiceDateInfo);
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.InvoiceDateInfo);
		}

		#endregion

		#region AutoValidationType

		public override Type AutoValidationType
		{
			get { return typeof(PeriodicInvoicingMultiClientInvoiceValidation); }
		}

		#endregion
	}
}
