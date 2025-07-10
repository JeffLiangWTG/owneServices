using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StmUpgradeCollectionContainer : NonPersistentBusinessObject, IStmALogParent, IObsoleteValidation
	{
		public StmUpgradeCollectionContainer(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region CMRRefFileLog

		public CMRReferenceFileUpdateLog CMRRefFileLog
		{
			get
			{
				if (fCMRRefFileLog == null)
				{
					fCMRRefFileLog = new CMRReferenceFileUpdateLog(Factory);
				}
				return fCMRRefFileLog;
			}
		}
		CMRReferenceFileUpdateLog fCMRRefFileLog;

		public bool IsCmrManualTestFileUpdateAllowed
		{
			get
			{
				if (isCmrManualTestFileUpdateAllowed == null)
				{
					var dbConnected = (Factory as CargoWise.Data.IDbConnected);
					var dbLocator = (dbConnected.Connection as CargoWise.Data.IPhysicalRefDbLocation);
					string cmrDbName = dbLocator.GetReferenceDatabaseName(CargoWise.Data.RefDbTypeEnum.Customs, Core.Constants.CountryCodes.Australia);
					isCmrManualTestFileUpdateAllowed = !CargoWise.Data.RefDbTableNameResolver.IsSharedDatabase(cmrDbName);
				}

				return isCmrManualTestFileUpdateAllowed.Value;
			}
		}
		bool? isCmrManualTestFileUpdateAllowed;

		#endregion

		#region Setting Upgrade Status

		public void DeleteUpgrades(StmUpgrade[] upgrades)
		{
			if (upgrades.Length > 0)
			{
				foreach (StmUpgrade upgrade in upgrades)
				{
					// Avoid loading the old blob and running out of memory
					upgrade.SetSZ_UpgradeData_CompressedSource(new ByteArrayStreamSource(System.Array.Empty<byte>()));

					upgrade.SZ_UpgradeData_Compressed = ZBlob.Empty;
					upgrade.SetStatus(StmUpgrade.StmUpgradeStatus.Deleted, Res.GetString("93e8e5ec-c46b-4654-b4c3-680d60758a1a", "Deleted by {0}", Env.CurrentUser.LoginName));
				}

				Factory.Save();
			}
		}

		#endregion

		#region Updating the Full Upgrades View

		public void UpdateFullUpgradesView(bool showReady, bool showApplied, bool showNotApplied, bool showDeleted, bool showObsolete)
		{
			SortInfo currentSortInfo = FullUpgradesView.SortInformation;

			FullUpgradesView.ShowReady = showReady;
			FullUpgradesView.ShowApplied = showApplied;
			FullUpgradesView.ShowNotApplied = showNotApplied;
			FullUpgradesView.ShowDeleted = showDeleted;
			FullUpgradesView.ShowObsolete = showObsolete;

			FullUpgradesView.Rebuild();

			if (currentSortInfo != null)
			{
				FullUpgradesView.Sort(currentSortInfo);
			}
		}

		#endregion

		#region Full Upgrades

		public StmUpgradeCollection FullUpgrades
		{
			get
			{
				if (fFullUpgrades == null)
				{
					var localFullUpgrades = new StmUpgradeCollection(Factory, EDPFilter);
					localFullUpgrades.Load();
					fFullUpgrades = localFullUpgrades;
					fFullUpgrades.SetReadOnlyIncludingChildren(true);
				}
				return fFullUpgrades;
			}
		}
		StmUpgradeCollection fFullUpgrades;

		public StmUpgradeCollectionView FullUpgradesView
		{
			get
			{
				if (fFullUpgradesView == null)
				{
					fFullUpgradesView = new StmUpgradeCollectionView(FullUpgrades);
				}
				return fFullUpgradesView;
			}
		}
		StmUpgradeCollectionView fFullUpgradesView;

		ZQuery EDPFilter
		{
			get
			{
				if (fEDPFilter == null)
				{
					fEDPFilter = new ZQuery(StmUpgradeSchema.SZ_Type, StmUpgrade.StmUpgradeType.EDP);
				}
				return fEDPFilter;
			}
		}
		ZQuery fEDPFilter;

		#endregion

		#region IStmALogParent

		public Logs Logs
		{
			get { return logs ?? (logs = new Logs(this)); }
		}
		Logs logs;

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return PK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return TableName; }
		}

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get { return Factory; }
		}

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
		{
			get
			{
				BusinessObject[] upgrades = FullUpgrades.ToArray();
				BusinessObject[] result = new BusinessObject[upgrades.Length + 1];

				if (upgrades.Length > 0)
				{
					upgrades.CopyTo(result, 0);
				}

				result[result.Length - 1] = CMRRefFileLog;

				return result;
			}
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion
	}
}
