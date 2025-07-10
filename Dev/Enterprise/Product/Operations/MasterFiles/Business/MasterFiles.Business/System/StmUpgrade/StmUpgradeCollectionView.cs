using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class StmUpgradeCollectionView : BusinessObjectCollectionView<StmUpgrade>
	{
		public bool ShowNoStatus;
		public bool ShowReady;
		public bool ShowApplied;
		public bool ShowNotApplied;
		public bool ShowDeleted;
		public bool ShowObsolete;

		public StmUpgradeCollectionView(BusinessObjectCollection collection) : base(collection)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			StmUpgrade upgrade = (StmUpgrade)element;

			return
				(upgrade.IsCurrentVersion) ||
				(ShowNoStatus && upgrade.SZ_Status.IsEmpty) ||
				(ShowReady && upgrade.SZ_Status == StmUpgrade.StmUpgradeStatus.Ready) ||
				(ShowApplied && upgrade.SZ_Status == StmUpgrade.StmUpgradeStatus.Applied) ||
				(ShowNotApplied && upgrade.SZ_Status == StmUpgrade.StmUpgradeStatus.NotApplied) ||
				(ShowDeleted && upgrade.SZ_Status == StmUpgrade.StmUpgradeStatus.Deleted) ||
				(ShowObsolete && upgrade.SZ_Status == StmUpgrade.StmUpgradeStatus.Obsolete);
		}
	}
}
