using CargoWise.Data;

namespace Enterprise.MasterFiles.Business
{
	public abstract class CoreMergeAction : IMergeAction
	{
		protected CoreMergeAction(OrganisationMergeData mergeData)
		{
			MergeData = mergeData;
		}

		public abstract void Merge(OrgHeader oldOrg, OrgHeader newOrg, OrganisationMergerActionOnSave action);

		protected OrganisationMergeData MergeData { get; }

		protected DbCommand GetCommandOnMainConnection(string sqlText)
		{
			return GetCommandOnMainConnection(sqlText, MergeData.Connection);
		}

		protected static DbCommand GetCommandOnMainConnection(string sqlText, DbConnection connection)
		{
			var cmd = connection.Command(sqlText); // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			cmd.CommandTimeout = 900;
			return cmd;
		}
	}
}
