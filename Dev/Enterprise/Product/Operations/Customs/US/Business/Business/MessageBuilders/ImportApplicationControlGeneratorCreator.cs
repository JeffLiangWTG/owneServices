using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ImportApplicationControlGeneratorCreator : IApplicationControlGeneratorCreator
	{
		public ApplicationControlGenerator New(string applicationIdentifier, GlbBranch branch)
		{
			ApplicationControlGenerator result;
			if (new ACEApplicationIdentifierCodeList().ContainsCode(applicationIdentifier))
			{
				result = new ACEApplicationControlGenerator(applicationIdentifier, branch);
			}
			else
			{
				result = new ABIApplicationControlGenerator(branch);
			}
			return result;
		}
	}
}
