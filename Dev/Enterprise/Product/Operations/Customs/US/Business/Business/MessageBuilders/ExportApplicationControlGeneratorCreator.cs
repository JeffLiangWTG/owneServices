using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ExportApplicationControlGeneratorCreator : IApplicationControlGeneratorCreator
	{
		public ApplicationControlGenerator New(string applicationIdentifier, GlbBranch branch) => new AESApplicationControlGenerator(branch);
	}
}
