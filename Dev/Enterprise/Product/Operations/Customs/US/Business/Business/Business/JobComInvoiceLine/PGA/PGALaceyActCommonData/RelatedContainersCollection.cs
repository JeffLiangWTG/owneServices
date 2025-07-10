using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class RelatedContainersCollection : NonPersistentBusinessObjectCollection<RelatedContainer>
	{
		public RelatedContainersCollection(PGA pga)
			: base(pga.Factory)
		{
			this.pga = pga;

			RebuildElements();
		}
		readonly PGA pga;

		#region New Methods

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RelatedContainer(pga);
		}

		public RelatedContainer FindByContainerNumber(ZString containerNumber)
		{
			foreach (RelatedContainer relatedContainer in this)
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

		void RebuildElements()
		{
			RemoveAll();

			var invoiceLine = pga.InvoiceLine;
			if (invoiceLine != null)
			{
				foreach (CusContainerInvoiceLinePivot containerPivot in invoiceLine.ContainersPivot)
				{
					var container = base.AddNew();
					container.SetContainerInvoiceLinePivot(containerPivot);
				}
			}
			RefreshBinding();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}
