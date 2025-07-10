using System;
using Dat.Integration.SpecialFileHandling;

namespace CargoWise.RefDbRepo.SpecialFileHandling
{
	public class StagingSchemaVersionChangeRequest : SchemaVersionChangeRequest
	{
		public StagingSchemaVersionChangeRequest(SpecialFileHandlerContext context)
			: base(context)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));
		}

		const string StagingDbSchemaTablePath = @"/Staging/Schema/StagingDb/dbo";
		const string StagingDbSchemaVersionFileServerPath = @"/Staging/Schema/DbUpgrader/StagingSchemaVersion.cs";

		protected override string SchemaTablePath => StagingDbSchemaTablePath;

		protected override string SchemaVersionFileServerPath => StagingDbSchemaVersionFileServerPath;
	}
}
