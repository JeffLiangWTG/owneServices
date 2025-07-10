using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BillDeepCloneStrategy : CustomsBusinessObjectCloneStrategy
	{
		public BillDeepCloneStrategy(Bill bill, Dictionary<ZGuid, ZGuid> containerPKPairs, CloneType cloneType, BaseJobDeclaration clonedDeclaration)
			: base(bill, cloneType, clonedDeclaration.Factory)
		{
			this.clonedDeclaration = clonedDeclaration;
			this.containerPKPairs = containerPKPairs;
		}

		Bill BillToClone
		{
			get { return (Bill)base.bizObjToClone; }
		}

		readonly BaseJobDeclaration clonedDeclaration;
		readonly Dictionary<ZGuid, ZGuid> containerPKPairs;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			Bill result = (Bill)base.CloneInternal(args);

			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				result.CU_JE = clonedDeclaration.PK;
				clonedDeclaration.Bills.Add(result);
			}

			DeepCopyPackingGroups(result);

			return result;
		}

		void DeepCopyPackingGroups(Bill clonedResult)
		{
			var isSea = BillToClone.Declaration.IsSea;
			var shouldCloneContainersEvenNotLinked = BillToClone.Declaration.ShouldCloneContainersEvenNotLinked;
			foreach (BasePackingGroup packingGroup in BillToClone.PackingGroups)
			{
				var hasLinkedContainer = !packingGroup.CR_CO_Container.IsEmpty && containerPKPairs.ContainsKey(packingGroup.CR_CO_Container);
				if (shouldCloneContainersEvenNotLinked || !isSea || hasLinkedContainer)
				{
					BasePackingGroup cloned = (BasePackingGroup)new PackingGroupDeepCopyStrategy(packingGroup, cloneType, clonedResult).Clone();

					using (cloned.GetValidationSuspender())
					using (cloned.SuspendSettingHasChanges())
					{
						if (hasLinkedContainer)
						{
							cloned.CR_CO_Container = containerPKPairs[packingGroup.CR_CO_Container];
						}

						clonedResult.PackingGroups.Add(cloned);
					}
				}
			}
		}
	}
}
