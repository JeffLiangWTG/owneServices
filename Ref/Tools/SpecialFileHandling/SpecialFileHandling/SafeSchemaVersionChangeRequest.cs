using System;
using Dat.Integration.SpecialFileHandling;

namespace CargoWise.RefDbRepo.SpecialFileHandling
{
	public class SafeSchemaVersionChangeRequest : SchemaVersionChangeRequest
	{
		public SafeSchemaVersionChangeRequest(SpecialFileHandlerContext context)
		 : base(context)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));
		}

		const string SafeSchemaTablePath = @"/Service/SchemaManagement/SafeDb/dbo";
		const string SafeSchemaVersionFileServerPath = @"/Service/SchemaManagement/UpgradeManagerRunner/SafeSchemaVersion.cs";

		protected override string SchemaTablePath => SafeSchemaTablePath;

		protected override string SchemaVersionFileServerPath => SafeSchemaVersionFileServerPath;
	}
}
