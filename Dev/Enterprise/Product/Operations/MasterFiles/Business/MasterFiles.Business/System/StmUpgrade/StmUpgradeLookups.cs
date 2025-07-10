//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmUpgradeLookups
//
//    This class should be used for overriding collections in AutoStmUpgradeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class StmUpgradeLookups : AutoStmUpgradeLookups
	{
		public StmUpgradeLookups(AutoStmUpgrade parent) : base(parent)
		{
		}

		#region Statuses

		public CodeDescriptionPairList Statuses
		{
			get
			{
				if (fStatuses == null)
				{
					fStatuses = new CodeDescriptionPairList();
					fStatuses.AddPair(StmUpgrade.StmUpgradeStatus.CurrentVersion, Res.GetString("6ecb05b8-f5fc-4f66-a015-7a23a8f7fa19", "Current Version"));
					fStatuses.AddPair(StmUpgrade.StmUpgradeStatus.Ready, Res.GetString("321ea6f3-3d8e-4415-8cc2-0c5d25daeef8", "Ready"));
					fStatuses.AddPair(StmUpgrade.StmUpgradeStatus.Applied, Res.GetString("7ff34c03-24e3-4817-8033-6fe153a9ea90", "Applied"));
					fStatuses.AddPair(StmUpgrade.StmUpgradeStatus.NotApplied, Res.GetString("f66d53eb-4416-4448-9fab-8641d44b9c01", "Not Applied"));
					fStatuses.AddPair(StmUpgrade.StmUpgradeStatus.Deleted, Res.GetString("051fcd83-feb9-4a6d-b99d-5b3664915563", "Deleted"));
					fStatuses.AddPair(StmUpgrade.StmUpgradeStatus.Obsolete, Res.GetString("704e0818-cda8-418b-a113-deae0a9b35b7", "Obsolete"));
				}

				return fStatuses;
			}
		}

		CodeDescriptionPairList fStatuses;

		#endregion
	}
}
