using System;
using System.Collections;

namespace Enterprise.Customs.Business
{
	public class InvoiceLineQuantityCollection
	{
		public InvoiceLineQuantityCollection()
		{
			fQuantities = new ArrayList();
		}

		public void AddQuantity(InvoiceLineQuantity quantity)
		{
			if (quantity == null)
			{
				throw new ArgumentException("Quantity may not be null");
			}
			if (quantity.Quantity > 0)
			{
				InvoiceLineQuantity matchInArray = null;
				matchInArray = FindMatch(quantity);
				if (matchInArray == null)
				{
					fQuantities.Add(quantity.Clone());
				}
				else
				{
					matchInArray.Quantity += quantity.Quantity;
				}
			}
		}

		public InvoiceLineQuantity FindMatch(InvoiceLineQuantity quantity)
		{
			InvoiceLineQuantity result = null;
			foreach (InvoiceLineQuantity quantityFromList in fQuantities)
			{
				if (quantityFromList.UnitOfQuantity == quantity.UnitOfQuantity)
				{
					result = quantityFromList;
					break;
				}
			}
			return result;
		}

		public InvoiceLineQuantity this[int index]
		{
			get { return (InvoiceLineQuantity)fQuantities[index]; }
		}

		public int Count
		{
			get { return fQuantities.Count; }
		}

		protected CusEntryLine entryLine;
		protected ArrayList fQuantities;
	}
}
