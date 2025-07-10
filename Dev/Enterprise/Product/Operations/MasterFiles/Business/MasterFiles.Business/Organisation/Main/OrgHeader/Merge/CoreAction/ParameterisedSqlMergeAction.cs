using System.Data;
using System.Linq;

namespace Enterprise.MasterFiles.Business
{
	class ParameterisedSqlMergeAction : CoreMergeAction
	{
		public ParameterisedSqlMergeAction(OrganisationMergeData mergeData) : base(mergeData)
		{
		}

		public override void Merge(OrgHeader oldOrg, OrgHeader newOrg, OrganisationMergerActionOnSave action)
		{
			var sqlActions = MergeHelper.GetAllParameterisedSqlProviders().Select(a => a.FullSqlText);
			var sqlActionsText = string.Join(System.Environment.NewLine, sqlActions);
			if (!string.IsNullOrEmpty(sqlActionsText))
			{
				using (var cmd = GetCommandOnMainConnection(sqlActionsText))
				{
					cmd.AddParameter(OrgMergeParameterisedSqlProvider.NewPkParameter, SqlDbType.UniqueIdentifier, MergeData.NewOrganisation.ToGuid());
					cmd.AddParameter(OrgMergeParameterisedSqlProvider.OldPkParameter, SqlDbType.UniqueIdentifier, MergeData.OldOrganisation.ToGuid());
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
