using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	interface IUpdateOrgPartRelationStrategy_OrgPartUnitInsertUpdate { }

	class UpdateOrgPartRelationStrategy_OrgPartUnitInsertUpdate : DeferTriggerOnUpdateConditionStrategy, IUpdateOrgPartRelationStrategy_OrgPartUnitInsertUpdate
	{
		UpdateOrgPartRelationStrategy_OrgPartUnitInsertUpdate()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
		{
			get
			{
				return new SchemaColumn[]
				{
					OrgPartUnitSchema.OF_PackType,
					OrgPartUnitSchema.OF_ParentPackType,
					OrgPartUnitSchema.OF_QuantityInParent
				};
			}
		}
	}
 }
