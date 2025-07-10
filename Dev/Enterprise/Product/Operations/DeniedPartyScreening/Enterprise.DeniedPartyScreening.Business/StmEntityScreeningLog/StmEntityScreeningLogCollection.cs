using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using IStmEntityScreeningLog = Enterprise.DeniedPartyScreening.Integration.IStmEntityScreeningLog;
using IStmEntityScreeningLogCollection = Enterprise.DeniedPartyScreening.Integration.IStmEntityScreeningLogCollection;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class StmEntityScreeningLogCollection : ActiveBusinessObjectCollection<StmEntityScreeningLog>, IStmEntityScreeningLogCollection
	{
		public StmEntityScreeningLogCollection(BusinessObject master)
			: base(master.Factory, master, new ZQuery(), StmEntityScreeningLogSchema.PJ_ParentID)
		{
			SetReadOnlyIncludingChildren(true);
		}

		#region Implementation

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		#region IStmEntityScreeningLogCollection

		ZString[] NoNeedAddInvalidateByLocalDataChangesStatus { get; } = new ZString[]
		{
			ScreeningStatusesList.Codes.NotScreened,
			ScreeningStatusesList.Codes.Unknown,
			ScreeningStatusesList.Codes.PermanentClear
		};

		bool HasToInvalidateScreeningStatus => (Relationship.Master is IScreeningStatusProvider statusProvider)
			&& !NoNeedAddInvalidateByLocalDataChangesStatus.Contains(statusProvider.ScreeningStatus)
			&& Relationship.Master.IsInDatabase;

		bool HasNewInvalidatedByLocalDataChangesLogInCache => this.Any(u => u.PJ_Sequence == 0 && u.PJ_Status == DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges);

		public void InvalidateByLocalDataChanges()
		{
			if (HasToInvalidateScreeningStatus && !HasNewInvalidatedByLocalDataChangesLogInCache)
			{
				AddStatus(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges);
			}
		}

		public string MostRecentStatus
		{
			get { return Count > 0 ? this.OrderByDescending(u => u.PJ_Sequence).First().PJ_Status.ToString() : ""; }
		}

		IStmEntityScreeningLog IStmEntityScreeningLogCollection.this[int i]
		{
			get { return base[i]; }
		}

		void AddStatus(string statusCode, string clearedReason = null)
		{
			var status = AddNew();
			status.PJ_Status = statusCode;
			status.PJ_SourceTableCode = status.PJ_ParentTableCode;
			status.PJ_SourceID = status.PJ_ParentID;

			if (!string.IsNullOrWhiteSpace(clearedReason))
			{
				status.PJ_ClearedReason = clearedReason;
			}
		}

		public void AddUpdateRelatedJobScreeningStatusLog()
		{
			AddStatus(DeniedPartyConstants.LogsScreeningStatus.UpdateRelatedJobs);
		}

		public void AddMarkAsJobClearLog(string clearReason)
		{
			AddStatus(DeniedPartyConstants.LogsScreeningStatus.JobCleared, clearReason);
		}

		public void AddRemoveOrInsertPartyScreeningLog(List<ScreeningPartiesSnapshot> orglist, List<ScreeningPartiesSnapshot> curlist)
		{
			var itemToDelete = this.Where(x => x != null && !x.IsInDatabase && (x.PJ_Status == DeniedPartyConstants.LogsScreeningStatus.AddPartyScreen || x.PJ_Status == DeniedPartyConstants.LogsScreeningStatus.RemovePartyScreen)).ToArray();

			foreach (var item in itemToDelete)
			{
				this.RemoveFromRelationship(item);
				item.Delete();
			}

			var listInsert = curlist.Where(b => !orglist.Any(a => a.Key == b.Key)).ToList();
			if (listInsert.Count > 0)
			{
				AddStatus(DeniedPartyConstants.LogsScreeningStatus.AddPartyScreen, Res.GetString("1C12D34E-D580-4386-A526-F1C8E3121082", "Parties Info:{0}", string.Join(System.Environment.NewLine, listInsert.Select(i => i.Description))));
			}

			var listDelete = orglist.Where(a => curlist.All(b => b.Key != a.Key)).ToList();
			if (listDelete.Count > 0)
			{
				AddStatus(DeniedPartyConstants.LogsScreeningStatus.RemovePartyScreen, Res.GetString("1C12D34E-D580-4386-A526-F1C8E3121082", "Parties Info:{0}", string.Join(System.Environment.NewLine, listDelete.Select(i => i.Description))));
			}
		}

		#endregion
	}
}
