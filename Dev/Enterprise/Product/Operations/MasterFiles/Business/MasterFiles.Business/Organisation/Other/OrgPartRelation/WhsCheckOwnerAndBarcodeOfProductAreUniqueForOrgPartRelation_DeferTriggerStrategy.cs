using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	#region WhsCheckOwnerAndBarcodeOfProductAreUniqueForOrgPartRelation_DeferTriggerStrategy

	interface IWhsCheckOwnerAndBarcodeOfProductAreUniqueForOrgPartRelation_DeferTriggerStrategy { }

	class WhsCheckOwnerAndBarcodeOfProductAreUniqueForOrgPartRelation_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckOwnerAndBarcodeOfProductAreUniqueForOrgPartRelation_DeferTriggerStrategy
	{
		WhsCheckOwnerAndBarcodeOfProductAreUniqueForOrgPartRelation_DeferTriggerStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
			=> new SchemaColumn[]
			{
				OrgPartRelationSchema.OU_Relationship,
				OrgPartRelationSchema.OU_OH,
				OrgPartRelationSchema.OU_OP,
			};
	}

	#endregion
}
