using System;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OSMGBulkUpdater : NonPersistentBusinessObject
	{
		#region Schema

		public abstract class Schema
		{
			public const string BulkOrgSecurityGroup = "BulkOrgSecurityGroup";
		}

		#endregion

		public OSMGBulkUpdater()
		{
		}

		public ZInt BulkUpdateOSMG()
		{
			var totalCount = 0;
			var cancel = false;

			for (; ; )
			{
				var updatedCount = BulkUpdateOSMGCore();
				totalCount += updatedCount;

				if (BulkUpdateEvent != null)
				{
					var args = new BulkUpdateEventArgs() { ProcessedCount = totalCount };
					BulkUpdateEvent(this, args);
					cancel = args.Cancel;
				}

				if (updatedCount == 0 || cancel)
				{
					return totalCount;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Not required with sql string")]
		ZInt BulkUpdateOSMGCore()
		{
			ZInt orgsUpdatedCount = 0;

			var updateSql =
$@"
DECLARE @OSMGGroupCode VARCHAR({GlbGroupSchema.GG_Code.MaxLength}) = (SELECT GG_Code FROM dbo.GlbGroup WHERE GG_PK = '{BulkOrgSecurityGroup}')
DECLARE @OrgMiscServToUpdate TABLE (OM_PK UNIQUEIDENTIFIER PRIMARY KEY)

INSERT INTO dbo.OrgMiscServ (OM_PK, OM_OH)
SELECT NEWID(), OH_PK
FROM dbo.OrgHeader
WHERE OH_PK NOT IN (SELECT OM_OH FROM dbo.OrgMiscServ)

INSERT INTO @OrgMiscServToUpdate
SELECT TOP ({MaxRowCount}) OM_PK
FROM dbo.OrgMiscServ
WHERE OM_GG_OrgSecurityGroup IS NULL

UPDATE dbo.OrgMiscServ
SET OM_GG_OrgSecurityGroup = '{BulkOrgSecurityGroup}'
WHERE OM_PK IN (SELECT OM_PK FROM @OrgMiscServToUpdate)

INSERT INTO dbo.StmALog (SL_PK, SL_Parent, SL_Table, SL_Reference, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser, SL_GB_NKBranch, SL_GE_NKDepartment)" +    // setting SL_SE_NKEvent for insert
$@"SELECT
	NEWID(),
	OM_PK,
	'{OrgMiscServSchema.Constants.TableName}',
	'Bulk updated OSMG: ' + @OSMGGroupCode,
	GETDATE(),
	'{Events.EditedARecord}',
	'{GlbStaff.CurrentUser.GS_Code}',
	'{GlbBranch.CurrentBranch.GB_Code}',
	'{GlbDepartment.CurrentDepartment.GE_Code}'
FROM
	@OrgMiscServToUpdate

SELECT @@ROWCOUNT
";

			using (var transactionManager = Db.Connection.BeginTransactionWithManager()) // need direct SQL here
			using (var command = Db.Connection.Command(updateSql)) // uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				orgsUpdatedCount = (int)command.ExecuteScalar();
				transactionManager.CommitTransaction();
			}

			return orgsUpdatedCount;
		}

		#region Properties

		#region OrgSecurityGroup

		[List("BulkOrgSecurityGroups")]
		public ZGuid BulkOrgSecurityGroup
		{
			get { return bulkOrgSecurityGroup; }
			set
			{
				SetNonPersistentPropertyValue(BulkOrgSecurityGroupInfo, ref bulkOrgSecurityGroup, value);
			}
		}
		ZGuid bulkOrgSecurityGroup;

		public ZPropertyInfo BulkOrgSecurityGroupInfo
		{
			get { return GetZPropertyInfo(Schema.BulkOrgSecurityGroup); }
		}

		public GlbGroupCollection BulkOrgSecurityGroups
		{
			get { return groupCollection ?? (groupCollection = new GlbGroupCollection(new BusinessObjectFactory())); }
		}
		GlbGroupCollection groupCollection;

		#endregion

		#endregion

		protected virtual int MaxRowCount => 1000;

		public EventHandler<BulkUpdateEventArgs> BulkUpdateEvent;

		public class BulkUpdateEventArgs : EventArgs
		{
			public int ProcessedCount { get; set; }
			public bool Cancel { get; set; }
		}
	}
}
