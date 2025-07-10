using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public class WhsInvoicePeriodicMultiClientInvoiceValidation : ZValidation
	{
		public WhsInvoicePeriodicMultiClientInvoiceValidation(WhsInvoicePeriodicMultiClientInvoice parent)
			: base(parent)
		{
			Parent = parent;
		}

		readonly WhsInvoicePeriodicMultiClientInvoice Parent;

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
			get { return typeof(WhsInvoicePeriodicMultiClientInvoiceValidation); }
		}

		#endregion
	}
}
