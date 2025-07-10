using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFContainerRowCollection : NonPersistentBusinessObjectCollection<ISFContainerRow>
	{
		public ISFContainerRowCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ISFContainerRowCollection(ISFHeaderRow headerRow)
			: this(headerRow.Factory)
		{
			this.headerRow = headerRow;
			PopulateContainersFromHeader();
		}

		public ISFContainerRowCollection GetContainersToCopy()
		{
			ISFContainerRowCollection result = new ISFContainerRowCollection(Factory);
			foreach (ISFContainerRow containerRow in this)
			{
				if (containerRow.ShouldCopy)
				{
					result.Add(containerRow);
				}
			}

			return result;
		}

		public bool HasContainerMarkedForCopy
		{
			get
			{
				bool result = false;
				foreach (ISFContainerRow containerRow in this)
				{
					if (containerRow.ShouldCopy)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		#region Implementation
		void PopulateContainersFromHeader()
		{
			foreach (CusISFEquip container in headerRow.Header.Equipments)
			{
				if (!container.BE_ContainerNum.IsEmpty)
				{
					Add(new ISFContainerRow(container, headerRow));
				}
			}
		}

		readonly ISFHeaderRow headerRow;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new ISFContainerRowCollectionFindBoxListProvider(this); }
		}

		#endregion
	}
}
