using System;
using Dat.Integration.SpecialFileHandling;

namespace CargoWise.RefDbRepo.SpecialFileHandling
{
	public class UpgradeScriptProviderLatestVersionChangeRequest : SchemaVersionChangeRequest
	{
		public UpgradeScriptProviderLatestVersionChangeRequest(SpecialFileHandlerContext context)
			: base(context)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));
		}

		protected override string SchemaTablePath => @"/Service/SchemaManagement/RemoteDb";

		protected override string SchemaVersionFileServerPath => @"/Service/SchemaManagement/RemoteDbManager/UpgradeScriptProviderVersion.cs";
	}
}
