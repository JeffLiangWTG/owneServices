using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class NonPersistentCusContainerCollection : NonPersistentBusinessObjectCollection<NonPersistentCusContainer>
	{
		public NonPersistentCusContainerCollection(BaseJobComInvoiceLine invoiceLine)
			: base(invoiceLine.Factory)
		{
			this.InvoiceLine = invoiceLine;
			RebuildElements();
			if (invoiceLine?.Declaration?.CusContainers != null)
			{
				invoiceLine.Declaration.CusContainers.CountChanged += OnContainerChanged;
			}
		}

		public void OnContainerChanged(object sender, EventArgs e)
		{
			if (!InvoiceLine.IsDeleted)
			{
				RebuildElements();
			}
		}

		public void RebuildElements()
		{
			RemoveAll();
			var declaration = InvoiceLine.Declaration;
			if (declaration != null && declaration.CusContainers != null)
			{
				foreach (BaseCusContainer container in declaration.CusContainers)
				{
					NonPersistentCusContainer containerInPivot = AddNew(container);
				}
			}
		}

		protected readonly BaseJobComInvoiceLine InvoiceLine;

		#region New Methods

		public NonPersistentCusContainer AddNew(BaseCusContainer container)
		{
			NonPersistentCusContainer result = base.AddNew();
			result.Container = container;
			return result;
		}

		public void RemoveElementLinkedTo(BaseCusContainer container)
		{
			if (!container.IsDeleted)
			{
				foreach (NonPersistentCusContainer containerForDelete in ToArray())
				{
					if (containerForDelete.ContainerNumber == container.CO_ContainerNumber)
					{
						this.RemoveAndDelete(containerForDelete);
						return;
					}
				}
			}
		}

		public void SortByContainerNumber()
		{
			Sort(NonPersistentCusContainer.Schema.ContainerNumber, System.ComponentModel.ListSortDirection.Ascending);
		}

		public NonPersistentCusContainer FindByContainer(BaseCusContainer container)
		{
			foreach (NonPersistentCusContainer currentContainer in this)
			{
				if (currentContainer.Container != null && currentContainer.Container == container)
				{
					return currentContainer;
				}
			}
			return null;
		}

		public NonPersistentCusContainer FindByContainerNumber(ZString containerNumber)
		{
			NonPersistentCusContainer result = null;

			foreach (NonPersistentCusContainer currentContainer in this)
			{
				if (currentContainer.ContainerNumber == containerNumber)
				{
					result = currentContainer;
					break;
				}
			}

			return result;
		}

		#endregion

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public NonPersistentCusContainer[] AllLinkedContainerNumbers
		{
			get
			{
				ArrayList result = new ArrayList();

				foreach (NonPersistentCusContainer currentContainer in this)
				{
					if (currentContainer.IsForInvoiceLine)
					{
						result.Add(currentContainer);
					}
				}

				return (NonPersistentCusContainer[])result.ToArray(typeof(NonPersistentCusContainer));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NonPersistentCusContainer(InvoiceLine);
		}
		#endregion
	}
}
