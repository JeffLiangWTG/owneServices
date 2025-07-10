namespace Enterprise.MasterFiles.Business
{
	using System.Collections.Generic;
	using System.Data;
	using CargoWise.Data;
	using CargoWise.EntityFramework;

	public class ProductOrgMerger : NonPersistentBusinessObject
	{
		#region SQL

		const string OldOrganizationParameterName = "OldOrgPK";
		const string NewOrganizationParameterName = "NewOrgPK";
		const string NonOldOrgRelationsParameterName = "NonOldOrgRelations";
		const string MaxRowsParameterName = "MaxRows";
		const string UserCodeParameterName = "UserCode";

		const string WithAffectedPartsSql = @"
with
AffectedRelations as
(
	select
		OP_PK, OP_PartNum, 
		case when OU_OH=@OldOrgPK and OU_Relationship in ('OWN', 'BTH') then 1 else 0 end as OldOrgOwner,
		case when OU_OH=@NewOrgPK and OU_Relationship in ('OWN', 'BTH') then 1 else 0 end as NewOrgOwner,
		case when OU_OH<>@OldOrgPK then 1 else 0 end as NonOldOrgRelations
	from dbo.OrgPartRelation
	inner join dbo.OrgSupplierPart on OP_PK=OU_OP and OP_IsActive=1
	where OU_OP in (select OU_OP from dbo.OrgPartRelation where OU_OH in (@OldOrgPK, @NewOrgPK))
),
AffectedParts as
(
	select OP_PK, OP_PartNum, max(OldOrgOwner) as OldOrgOwner, max(NewOrgOwner) as NewOrgOwner, max(NonOldOrgRelations) as NonOldOrgRelations
	from AffectedRelations
	group by OP_PK, OP_PartNum
)
";

		const string StatsSql = WithAffectedPartsSql + @"
select OP_PartNum, NonOldOrgRelations, PartCount from
(
	select 
		OAP.OP_PartNum,
		NonOldOrgRelations,
		rank() over (partition by NonOldOrgRelations order by OP_PartNum) as PartRank, 
		count(*) over (partition by NonOldOrgRelations) as PartCount
	from AffectedParts OAP
	where OldOrgOwner=1 and exists (select * from AffectedParts NAP where OAP.OP_PartNum=NAP.OP_PartNum and NAP.NewOrgOwner=1 and NAP.OP_PK<>OAP.OP_PK)
) T where PartRank <= @MaxRows
OPTION (RECOMPILE)
";

		const string DeactivateSql = WithAffectedPartsSql + @"
select
	OAP.OP_PK
into #TempProductOrgMerger
from AffectedParts OAP
where OldOrgOwner=1 and NonOldOrgRelations=@NonOldOrgRelations and exists (select * from AffectedParts NAP where OAP.OP_PartNum=NAP.OP_PartNum and NAP.NewOrgOwner=1 and NAP.OP_PK<>OAP.OP_PK)
OPTION (RECOMPILE);

insert into dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_Reference, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser)
select newid(), 'OrgSupplierPart', OP_PK, 'Deactivated Duplicate Part (Merge organization ' + coalesce(OldOrg.OH_Code, '') + ' into '+coalesce(NewOrg.OH_Code, '')+')', getDate(), 'EDT', @UserCode
from #TempProductOrgMerger
left join dbo.OrgHeader OldOrg on OldOrg.OH_PK=@OldOrgPK
left join dbo.OrgHeader NewOrg on NewOrg.OH_PK=@NewOrgPK;

update
	dbo.OrgSupplierPart
set
	OP_IsActive=0,
	OP_SystemLastEditTimeUtc = SYSUTCDATETIME(),
	OP_SystemLastEditUser = @UserCode
where
	OP_PK in (select OP_PK from #TempProductOrgMerger);

drop table #TempProductOrgMerger;
";

		const string DeleteSql = WithAffectedPartsSql + @"
select 
	OAP.OP_PK
into #TempProductOrgMerger
from AffectedParts OAP
where OldOrgOwner=1 and NonOldOrgRelations=1 and exists (select * from AffectedParts NAP where OAP.OP_PartNum=NAP.OP_PartNum and NAP.NewOrgOwner=1 and NAP.OP_PK<>OAP.OP_PK);

insert into dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_Reference, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser)
select newid(), 'OrgSupplierPart', OP_PK, 'Remove Relations from Duplicate Part (Merge organization ' + coalesce(OldOrg.OH_Code, '') + ' into '+coalesce(NewOrg.OH_Code, '')+')', getDate(), 'EDT', @UserCode
from #TempProductOrgMerger
left join dbo.OrgHeader OldOrg on OldOrg.OH_PK=@OldOrgPK
left join dbo.OrgHeader NewOrg on NewOrg.OH_PK=@NewOrgPK;

delete from dbo.OrgPartRelation
where (1=1)
and OU_OP in
(
	select OP_PK from #TempProductOrgMerger
)
and OU_Relationship in ('BTH', 'OWN')
and OU_OH=@OldOrgPK
OPTION (RECOMPILE);

drop table #TempProductOrgMerger;
";

		#endregion

		readonly MergeOrgHeader mergeOrgHeader;

		public ProductOrgMerger(MergeOrgHeader mergeOrgHeader)
		{
			this.mergeOrgHeader = mergeOrgHeader;
			ReloadStats();
		}

		public int SingleRelationDuplicateCount { get; private set; }
		public int MultiRelationDuplicateCount { get; private set; }
		public int TotalDuplicateCount => SingleRelationDuplicateCount + MultiRelationDuplicateCount;

		public List<string> SingleRelationDuplicatePartNumbers { get; } = new List<string>();
		public List<string> MultiRelationDuplicatePartNumbers { get; } = new List<string>();

		public OrgHeader OldOrganization => mergeOrgHeader?.OldOrganisation;
		public OrgHeader NewOrganization => mergeOrgHeader?.NewOrganisation;

		public void ReloadStats()
		{
			SingleRelationDuplicateCount = 0;
			MultiRelationDuplicateCount = 0;

			SingleRelationDuplicatePartNumbers.Clear();
			MultiRelationDuplicatePartNumbers.Clear();

			if (!IsValidMerge)
			{
				return;
			}

			Db.Connection.ExecuteReader(
				StatsSql,
				command =>
				{
					command.AddParameter(OldOrganizationParameterName, SqlDbType.UniqueIdentifier, OldOrganization.PK.ToGuid());
					command.AddParameter(NewOrganizationParameterName, SqlDbType.UniqueIdentifier, NewOrganization.PK.ToGuid());
					command.AddParameter(MaxRowsParameterName, SqlDbType.Int, 16);
				},
				r =>
				{
					if ((int)r["NonOldOrgRelations"] == 1)
					{
						MultiRelationDuplicateCount = (int)r["PartCount"];
						MultiRelationDuplicatePartNumbers.Add((string)r["OP_PartNum"]);
					}
					else
					{
						SingleRelationDuplicateCount = (int)r["PartCount"];
						SingleRelationDuplicatePartNumbers.Add((string)r["OP_PartNum"]);
					}
				}
			);
		}

		bool IsValidMerge
		{
			get { return OldOrganization != null && NewOrganization != null && OldOrganization.PK != NewOrganization.PK; }
		}

		public void DeactivateSingleRelationDuplicates()
		{
			if (!IsValidMerge)
			{
				return;
			}

			using (var tx = Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery(DeactivateSql, command =>
				{
					command.AddParameter(OldOrganizationParameterName, SqlDbType.UniqueIdentifier, OldOrganization.PK.ToGuid());
					command.AddParameter(NewOrganizationParameterName, SqlDbType.UniqueIdentifier, NewOrganization.PK.ToGuid());
					command.AddParameter(NonOldOrgRelationsParameterName, SqlDbType.Bit, 0);
					command.AddParameter(UserCodeParameterName, SqlDbType.VarChar, (string)GlbStaff.CurrentUser.GS_Code);
				});
				tx.CommitTransaction();
			}
		}

		public void DeactivateMultiRelationDuplicates()
		{
			if (!IsValidMerge)
			{
				return;
			}

			using (var tx = Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery(DeactivateSql, command =>
				{
					command.AddParameter(OldOrganizationParameterName, SqlDbType.UniqueIdentifier, OldOrganization.PK.ToGuid());
					command.AddParameter(NewOrganizationParameterName, SqlDbType.UniqueIdentifier, NewOrganization.PK.ToGuid());
					command.AddParameter(NonOldOrgRelationsParameterName, SqlDbType.Bit, 1);
					command.AddParameter(UserCodeParameterName, SqlDbType.VarChar, (string)GlbStaff.CurrentUser.GS_Code);
				});
				tx.CommitTransaction();
			}
		}

		public void DeleteMultiRelationDuplicateRelations()
		{
			if (!IsValidMerge)
			{
				return;
			}

			using (var tx = Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery(DeleteSql, command =>
				{
					command.AddParameter(OldOrganizationParameterName, SqlDbType.UniqueIdentifier, OldOrganization.PK.ToGuid());
					command.AddParameter(NewOrganizationParameterName, SqlDbType.UniqueIdentifier, NewOrganization.PK.ToGuid());
					command.AddParameter(UserCodeParameterName, SqlDbType.VarChar, (string)GlbStaff.CurrentUser.GS_Code);
				});
				tx.CommitTransaction();
			}
		}
	}
}
