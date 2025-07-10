using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	interface IUpdateOrgPartRelationStrategy_OrgSupplierPartUpdate { }

	class UpdateOrgPartRelationStrategy_OrgSupplierPartUpdate : DeferTriggerOnUpdateConditionStrategy, IUpdateOrgPartRelationStrategy_OrgSupplierPartUpdate
	{
		UpdateOrgPartRelationStrategy_OrgSupplierPartUpdate()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
		{
			get
			{
				return new SchemaColumn[]
				{
					OrgSupplierPartSchema.OP_StockKeepingUnit
				};
			}
		}

		protected override bool ShouldInsertsBeDeferred => false;
	}
}
