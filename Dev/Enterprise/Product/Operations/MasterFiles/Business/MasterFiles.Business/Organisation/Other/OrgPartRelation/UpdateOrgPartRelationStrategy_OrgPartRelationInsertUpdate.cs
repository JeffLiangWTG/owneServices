using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	interface IUpdateOrgPartRelationStrategy_OrgPartRelationInsertUpdate { }

	class UpdateOrgPartRelationStrategy_OrgPartRelationInsertUpdate : DeferTriggerOnUpdateConditionStrategy, IUpdateOrgPartRelationStrategy_OrgPartRelationInsertUpdate
	{
		UpdateOrgPartRelationStrategy_OrgPartRelationInsertUpdate()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
		{
			get
			{
				return new SchemaColumn[]
				{
					OrgPartRelationSchema.OU_ClientUQ
				};
			}
		}
	}
}
