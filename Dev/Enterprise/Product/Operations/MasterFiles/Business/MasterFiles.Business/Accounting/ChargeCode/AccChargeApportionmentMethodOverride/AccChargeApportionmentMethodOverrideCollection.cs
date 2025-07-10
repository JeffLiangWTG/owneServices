using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeApportionmentMethodOverrideCollection : DependentBusinessObjectCollection<AccChargeApportionmentMethodOverride, AccChargeCode>
	{
		public AccChargeApportionmentMethodOverrideCollection(AccChargeCode chargeCode) : base(chargeCode)
		{
		}

		#region BusinessObjectCollectionSnapShotter

		BusinessObjectCollectionSnapshotManager businessObjectCollectionSnapShotter;

		public BusinessObjectCollectionSnapshotManager SnapShotter
		{
			get
			{
				if (businessObjectCollectionSnapShotter == null)
				{
					businessObjectCollectionSnapShotter = new BusinessObjectCollectionSnapshotManager(new AccChargeApportionmentMethodOverrideCopier(), this);
				}
				return businessObjectCollectionSnapShotter;
			}
		}

		#endregion
	}

	public class AccChargeApportionmentMethodOverrideCopier : BusinessObjectCollectionCopier
	{
		public AccChargeApportionmentMethodOverrideCopier()
			: base(
				new[] {
					AccChargeApportionmentMethodOverrideSchema.AAM_ConsolType,
					AccChargeApportionmentMethodOverrideSchema.AAM_ContainerMode,
					AccChargeApportionmentMethodOverrideSchema.AAM_Direction,
					AccChargeApportionmentMethodOverrideSchema.AAM_Module,
					AccChargeApportionmentMethodOverrideSchema.AAM_TransportMode },
				new[] {
					AccChargeApportionmentMethodOverrideSchema.AAM_ApportionmentMethod })
		{
		}
	}
}
