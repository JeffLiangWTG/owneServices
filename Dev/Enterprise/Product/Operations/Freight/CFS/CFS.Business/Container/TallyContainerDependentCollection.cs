
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyContainerDependentCollection : CFSContainerCollection
	{
		public TallyContainerDependentCollection(PackUnpackLoadListConsol master, BusinessObjectFactory factory) : base(master, factory)
		{
		}

		public new TallyContainer this[int index]
		{
			get { return (TallyContainer)(Elements[index]); }
		}

		public new TallyContainer AddNew()
		{
			return (TallyContainer)base.AddNew();
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			TallyContainer container = bizOAdded as TallyContainer;
			if (container != null && !container.IsInDatabase)
			{
				if (Parent.JK_CTOAvailabilityDate.IsValid)
				{
					container.JC_FCLAvailable = Parent.JK_CTOAvailabilityDate;
				}

				if (Parent.JK_DepotAvailabilityDate.IsValid)
				{
					container.JC_LCLAvailable = Parent.JK_DepotAvailabilityDate;
				}

				if (Parent.JK_CTOStorageDate.IsValid)
				{
					container.JC_ArrivalCTOStorageStartDate = Parent.JK_CTOStorageDate;
				}

				if (Parent.JK_DepotStorageDate.IsValid)
				{
					container.JC_LCLStorageCommences = Parent.JK_DepotStorageDate;
				}
			}
		}
	}
}
