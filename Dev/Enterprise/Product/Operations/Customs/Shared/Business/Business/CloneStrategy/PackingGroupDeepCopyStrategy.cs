using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Copies a packing group and its packages
	/// </summary>
	public class PackingGroupDeepCopyStrategy : CustomsBusinessObjectCloneStrategy
	{
		public PackingGroupDeepCopyStrategy(BasePackingGroup packGroupToClone, CloneType cloneType, Bill clonedBill)
			: base(packGroupToClone, cloneType, clonedBill.Factory)
		{
			this.clonedBill = clonedBill;
		}

		BasePackingGroup PackGroupToClone
		{
			get { return (BasePackingGroup)base.bizObjToClone; }
		}
		readonly Bill clonedBill;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			BasePackingGroup clonedResult = (BasePackingGroup)base.CloneInternal(args);

			using (clonedResult.GetValidationSuspender())
			using (clonedResult.SuspendSettingHasChanges())
			{
				clonedResult.CR_CU_HouseBill = clonedBill.PK;
			}

			foreach (BasePackage package in PackGroupToClone.Packages)
			{
				BasePackage clonedPackage = (BasePackage)new CustomsBusinessObjectCloneStrategy(package, cloneType, alternativeFactoryToInstantiateCloneIn).Clone();

				using (clonedPackage.GetValidationSuspender())
				using (clonedPackage.SuspendSettingHasChanges())
				{
					clonedPackage.CW_CR_HouseContainer = clonedResult.PK;
					clonedResult.Packages.Add(clonedPackage);
				}
			}

			return clonedResult;
		}
	}
}
