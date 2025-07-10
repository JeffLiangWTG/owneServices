using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public interface IAllPossibleRecipientTypesGetter
	{
		CodeDescriptionPairList GetRecipientList();
		CodeDescriptionPairList GetListOfRecipientsWhichCannotReceiveUniversalXml();
	}
}
