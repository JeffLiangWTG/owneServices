using System;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class InvoiceLineQuantity : ICloneable
	{
		public InvoiceLineQuantity(InvoiceLineQuantity quantity)
		{
			fQuantity = quantity.Quantity;
			fUnitOfQuantity = quantity.UnitOfQuantity;
		}

		public InvoiceLineQuantity(ZDecimal quantity, ZString unitOfQuantity)
		{
			fQuantity = quantity;
			fUnitOfQuantity = unitOfQuantity;
		}

		#region ICloneable interface

		public object Clone()
		{
			return new InvoiceLineQuantity(this);
		}

		#endregion

		#region Properties

		public ZDecimal Quantity
		{
			get { return fQuantity; }
			set { fQuantity = value; }
		}

		public ZString UnitOfQuantity
		{
			get { return fUnitOfQuantity; }
		}

		#endregion

		#region Implementation

		protected ZString fUnitOfQuantity;
		protected ZDecimal fQuantity;

		#endregion
	}
}
