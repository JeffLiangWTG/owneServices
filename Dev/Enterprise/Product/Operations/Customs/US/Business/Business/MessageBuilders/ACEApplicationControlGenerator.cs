using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	sealed class ACEApplicationControlGenerator : ABIApplicationControlGenerator<AABIInputA, AABIInputZ>
	{
		public ACEApplicationControlGenerator(string applicationIdentifier, GlbBranch branch)
			: base(branch)
		{
			A.ApplicationIdentifierCode = applicationIdentifier;
		}
	}
}
