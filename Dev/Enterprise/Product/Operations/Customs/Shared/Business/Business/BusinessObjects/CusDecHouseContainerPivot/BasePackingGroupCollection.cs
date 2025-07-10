using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BasePackingGroupCollection : BusinessObjectCollectionView<BasePackingGroup>
	{
		public BasePackingGroupCollection(Bill houseBill)
			: base(houseBill.Declaration.PackingGroups)
		{
			this.HouseBill = houseBill;
		}

		protected readonly Bill HouseBill;

		public bool HasMultiplePackingGroups
		{
			get { return Count > 1; }
		}

		public BasePackingGroup AddNew(BaseCusContainer container)
		{
			BasePackingGroup packingGroup = AddNew();
			SetCR_CO_ContainerWithoutTriggeringChangesOrValidation(container, packingGroup);
			return packingGroup;
		}

		public string ContainerNumbersLinked
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				foreach (BasePackingGroup packGroup in this)
				{
					if (packGroup.Container != null)
					{
						result.Append(packGroup.Container.CO_ContainerNumber + ",");
					}
				}
				return result.ToString().Trim(',');
			}
		}

		void SetCR_CO_ContainerWithoutTriggeringChangesOrValidation(BaseCusContainer container, BasePackingGroup packingGroup)
		{
			if (container != null)
			{
				using (packingGroup.GetValidationSuspender())
				using (packingGroup.SuspendSettingHasChanges())
				{
					packingGroup.CR_CO_Container = container.PK;
				}
			}
		}

		protected override bool FetchOnlyFromLocalCache
		{
			get { return !HouseBill.Declaration.IsInDatabase; }
		}

		public BasePackingGroup GetElementWithContainer(BaseCusContainer container)
		{
			foreach (BasePackingGroup packingGroup in this)
			{
				if (container == null && packingGroup.CR_CO_Container.IsEmpty)
				{
					return packingGroup;
				}
				else if (container != null && packingGroup.CR_CO_Container == container.PK)
				{
					return packingGroup;
				}
			}
			return null;
		}

		public BasePackingGroup GetElementWithNoContainer()
		{
			return this.Cast<BasePackingGroup>().FirstOrDefault(pg => pg.CR_CO_Container.IsEmpty);
		}

		public BasePackingGroup GetElementWithNoContainerExcept(BasePackingGroup passedPackGroup)
		{
			foreach (BasePackingGroup packGroup in this)
			{
				if (packGroup.CR_CO_Container.IsEmpty && packGroup != passedPackGroup)
				{
					return packGroup;
				}
			}
			return null;
		}

		public int CountOfAllPackages()
		{
			int result = 0;
			foreach (BasePackingGroup packGroup in this)
			{
				result += packGroup.Packages.Count;
			}
			return result;
		}

		public bool HasPackageRecord
		{
			get
			{
				bool result = false;
				foreach (BasePackingGroup packingGroup in this)
				{
					if (packingGroup.Packages.Count > 0)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return HouseBill != null && (element as BasePackingGroup).CR_CU_HouseBill == HouseBill.PK;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((BasePackingGroup)child).CR_CU_HouseBill = HouseBill.PK;
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			((BasePackingGroup)child).CR_ClusterKey = HouseBill.CU_ClusterKey;
		}
	}
}
