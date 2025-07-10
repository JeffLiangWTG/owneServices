using CargoWise.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface IDocumentActionReasonSelector
	{
		DocumentActionReasonModel SelectDocumentActionReason(string message, string caption, ICodeDescriptionPairList optionsList);
	}
}
