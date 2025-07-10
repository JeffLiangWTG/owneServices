using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusContainerInvoiceLinePivotCollection : DependentBusinessObjectCollection<CusContainerInvoiceLinePivot, BaseCusContainer>
	{
		public CusContainerInvoiceLinePivotCollection(BaseCusContainer container, BusinessObjectFactory factory)
			: base(container, factory)
		{
		}

		protected BaseCusContainer Container
		{
			get { return Master; }
		}

		public CusContainerInvoiceLinePivot AddNew(BaseJobComInvoiceLine invoiceLine)
		{
			var newBO = Factory.New<CusContainerInvoiceLinePivot>();
			newBO.C2_JI = invoiceLine.PK;
			this.Add(newBO);
			return newBO;
		}

		public void DeletePivotLinkedTo(BaseJobComInvoiceLine invoiceLine)
		{
			foreach (CusContainerInvoiceLinePivot pivot in this.ToArray())
			{
				if (pivot.InvoiceLine == invoiceLine)
				{
					RemoveAndDelete(pivot);
					break;
				}
			}
		}

		public BaseJobComInvoiceLine[] InvoiceLinesAssociated
		{
			get
			{
				if (invoiceLinesCached == null)
				{
					invoiceLinesCached = new CachedProperty<BaseJobComInvoiceLine[]>(Factory, GetInvoiceLinesAssociated);
				}

				return invoiceLinesCached.Value;
			}
		}
		CachedProperty<BaseJobComInvoiceLine[]> invoiceLinesCached;

		BaseJobComInvoiceLine[] GetInvoiceLinesAssociated()
		{
			List<BaseJobComInvoiceLine> result = new List<BaseJobComInvoiceLine>();
			foreach (CusContainerInvoiceLinePivot pivot in this)
			{
				if (pivot.InvoiceLine != null && !result.Contains(pivot.InvoiceLine))
				{
					result.Add(pivot.InvoiceLine);
				}
			}
			return result.ToArray();
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusContainerInvoiceLinePivotSchema.C2_CO; }
		}
	}
}
