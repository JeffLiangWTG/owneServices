using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class FDARelatedContainersCollection : NonPersistentBusinessObjectCollection<FDARelatedContainer>
	{
		public FDARelatedContainersCollection(IFDARelatedContainer fda)
			: base(fda.Factory)
		{
			this.fda = fda;

			RebuildElements();
		}
		readonly IFDARelatedContainer fda;

		void RebuildElements()
		{
			RemoveAll();

			if (fda.InvoiceLine != null && fda.InvoiceLine.ContainersPivot != null)
			{
				foreach (CusContainerInvoiceLinePivot containerPivot in fda.InvoiceLine.ContainersPivot)
				{
					AddNew(containerPivot);
				}
			}

			RefreshBinding();
		}

		#region New Methods

		public FDARelatedContainer AddNew(CusContainerInvoiceLinePivot containerInvoiceLinePivot)
		{
			FDARelatedContainer result = base.AddNew();
			result.SetContainerInvoiceLinePivot(containerInvoiceLinePivot);
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FDARelatedContainer(fda);
		}

		public FDARelatedContainer FindByContainerNumber(ZString containerNumber)
		{
			foreach (FDARelatedContainer relatedContainer in this)
			{
				if (relatedContainer.ContainerNumber == containerNumber)
				{
					return relatedContainer;
				}
			}

			return null;
		}

		#endregion

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}
