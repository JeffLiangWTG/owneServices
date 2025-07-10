using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.ServiceTasks
{
	class HVLVConsignmentReferenceInfo : IHVLVReferenceInfo
	{
		SchemaStringColumn IHVLVReferenceInfo.IdColumn => HVLVConsignmentSchema.HVC_ConsignmentId;

		SchemaGuidColumn IHVLVReferenceInfo.PkColumn => HVLVConsignmentSchema.PK;

		SchemaBoolColumn IHVLVReferenceInfo.IsValidatedForUniquenessColumn => HVLVConsignmentSchema.HVC_IsValidatedForUniqueness;

		SchemaDateTimeColumn IHVLVReferenceInfo.CreateTimeColumn => HVLVConsignmentSchema.HVC_SystemCreateTimeUtc;

		SchemaIntColumn IHVLVReferenceInfo.ClusterKeyColumn => HVLVConsignmentSchema.HVC_ClusterKey;

		NumberFountainInfo IHVLVReferenceInfo.FallbackNumberFountainInfo => fallbackNumberFountainInfo
			?? (fallbackNumberFountainInfo = new NumberFountainInfo(Env.NumberFountains.HVLVConsignmentId, NumberFountains.HVLVConsignmentIdPrefix, NumberFountains.HVLVConsignmentIdFormatDigits));
		NumberFountainInfo fallbackNumberFountainInfo;
	}
}
