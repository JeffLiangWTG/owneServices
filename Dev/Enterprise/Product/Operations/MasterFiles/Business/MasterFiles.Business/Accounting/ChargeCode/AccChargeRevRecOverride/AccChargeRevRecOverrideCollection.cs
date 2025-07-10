
using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeRevRecOverrideCollection : DependentBusinessObjectCollection<AccChargeRevRecOverride, AccChargeCode>, IRegistrySettingCollection
	{
		public AccChargeRevRecOverrideCollection(AccChargeCode chargeCode)
			: base(chargeCode)
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
					businessObjectCollectionSnapShotter = new BusinessObjectCollectionSnapshotManager(new AccChargeRevRecOverrideCopier(), this);
				}
				return businessObjectCollectionSnapShotter;
			}
		}

		#endregion
	}

	public class AccChargeRevRecOverrideCopier : BusinessObjectCollectionCopier
	{
		public AccChargeRevRecOverrideCopier()
			: base(
				new[] {
					AccChargeRevRecOverrideSchema.AE_BrokerType,
					AccChargeRevRecOverrideSchema.AE_Direction,
					AccChargeRevRecOverrideSchema.AE_JobType,
					AccChargeRevRecOverrideSchema.AE_Mode },
				new[] {
					AccChargeRevRecOverrideSchema.AE_RecognitionType })
		{
		}
		static readonly Lazy<AccChargeRevRecOverrideCopier> instance = new Lazy<AccChargeRevRecOverrideCopier>();
		public static AccChargeRevRecOverrideCopier Instance { get { return instance.Value; } }
	}
}
