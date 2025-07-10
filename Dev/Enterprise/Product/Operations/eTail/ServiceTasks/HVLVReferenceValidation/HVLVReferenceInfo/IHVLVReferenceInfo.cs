using CargoWise.Schema;

namespace Enterprise.eTail.ServiceTasks
{
	interface IHVLVReferenceInfo
	{
		SchemaStringColumn IdColumn { get; }
		SchemaGuidColumn PkColumn { get; }
		SchemaBoolColumn IsValidatedForUniquenessColumn { get; }
		SchemaDateTimeColumn CreateTimeColumn { get; }
		SchemaIntColumn ClusterKeyColumn { get; }
		NumberFountainInfo FallbackNumberFountainInfo { get; }
	}
}
