using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface INX201_07 : INXDeclaration
	{
		ZDateTime IssueDateTime { get; }

		IDeclarationAdditionalDocument AdditionalDocument { get; }

		IAdditionalInformation AdditionalInformation { get; }
	}
}
