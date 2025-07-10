using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BasePackingGroupContainerCollection : BusinessObjectCollectionView<BasePackingGroup>
	{
		public BasePackingGroupContainerCollection(BaseCusContainer container)
			: base(container.Declaration.PackingGroups)
		{
			this.Container = container;
		}

		protected readonly BaseCusContainer Container;

		public bool HasElementsToBeDeletedWhenContainerDeleted
		{
			get
			{
				foreach (BasePackingGroup packGroup in this)
				{
					if (ShouldBeDeletedWhenDeletingParentContainer(packGroup))
					{
						return true;
					}
				}
				return false;
			}
		}

		public void DeleteOrRemoveReferenceToContainer()
		{
			try
			{
				isReferenceBeingRemoved = true;
				foreach (BasePackingGroup packGroup in this.ToArray())
				{
					if (ShouldBeDeletedWhenDeletingParentContainer(packGroup))
					{
						RemoveAndDelete(packGroup);
					}
					else
					{
						packGroup.CR_CO_Container = ZGuid.Empty;
					}
				}
			}
			finally
			{
				isReferenceBeingRemoved = false;
				Rebuild();
			}
		}

		internal bool ShouldBeDeletedWhenDeletingParentContainer(BasePackingGroup packGroup)
		{
			return packGroup.Bill == null || WillBeDuplicateAfterReferenceToParentContainerIsRemovedFrom(packGroup);
		}

		internal bool WillBeDuplicateAfterReferenceToParentContainerIsRemovedFrom(BasePackingGroup packGroup)
		{
			return packGroup.Bill != null && packGroup.Bill.PackingGroups.GetElementWithNoContainerExcept(packGroup) != null;
		}

		protected override void RebuildCore()
		{
			if (!isReferenceBeingRemoved)
			{
				base.RebuildCore();
			}
		}
		bool isReferenceBeingRemoved;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return Container != null && (element as BasePackingGroup).CR_CO_Container == Container.PK;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((BasePackingGroup)child).CR_CO_Container = Container.PK;
		}
	}
}
