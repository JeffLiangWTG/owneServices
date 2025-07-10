using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.ServiceTasks
{
	class HVLVItemReferenceInfo : IHVLVReferenceInfo
	{
		SchemaStringColumn IHVLVReferenceInfo.IdColumn => HVLVItemSchema.HVI_ItemId;

		SchemaGuidColumn IHVLVReferenceInfo.PkColumn => HVLVItemSchema.PK;

		SchemaBoolColumn IHVLVReferenceInfo.IsValidatedForUniquenessColumn => HVLVItemSchema.HVI_IsValidatedForUniqueness;

		SchemaDateTimeColumn IHVLVReferenceInfo.CreateTimeColumn => null;

		SchemaIntColumn IHVLVReferenceInfo.ClusterKeyColumn => HVLVItemSchema.HVI_ClusterKey;

		NumberFountainInfo IHVLVReferenceInfo.FallbackNumberFountainInfo => fallbackNumberFountainInfo
			?? (fallbackNumberFountainInfo = new NumberFountainInfo(Env.NumberFountains.HVLVItemId, NumberFountains.HVLVItemIdPrefix, NumberFountains.HVLVItemIdFormatDigits));
		NumberFountainInfo fallbackNumberFountainInfo;
	}
}
