using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BaseGroupHeaderInvoiceLineViewCollection : BusinessObjectCollectionView<BaseJobComInvoiceLine>, IAllInvoiceLines
	{
		public BaseGroupHeaderInvoiceLineViewCollection(BaseJobComInvoiceGroupHeader groupHeader, InvoiceLineCompleteCollection completeCollection) : base(completeCollection)
		{
			this.GroupHeader = groupHeader;
			Rebuild();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(BaseJobComInvoiceLine);
		}

		public bool HasApportionedCharges
		{
			get
			{
				bool result = false;
				foreach (BaseJobComInvoiceLine invoiceLine in this)
				{
					result = invoiceLine.ApportionedCharges.Count > 0;
					if (result)
					{
						break;
					}
				}
				return result;
			}
		}

		#region Implementation

		protected readonly BaseJobComInvoiceGroupHeader GroupHeader;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			bool result = false;

			if (GroupHeader != null)
			{
				BaseJobComInvoiceLine invoiceLine = (BaseJobComInvoiceLine)element;

				BaseJobComInvoiceHeader invoiceHeader = invoiceLine.InvoiceHeader;
				result = invoiceHeader != null && GroupHeader.IsThisInvoicePartOfThisGroup(invoiceHeader);
			}

			return result;
		}

		#endregion

		#region IAllInvoiceLines members

		bool IAllInvoiceLines.Contains(BaseJobComInvoiceLine invoiceLine)
		{
			return Contains(invoiceLine);
		}

		#endregion
	}
}
