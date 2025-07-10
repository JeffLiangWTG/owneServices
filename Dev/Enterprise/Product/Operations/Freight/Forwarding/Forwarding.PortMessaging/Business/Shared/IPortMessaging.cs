using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public interface IPortMessaging
	{
		ZString EntryType { get; }
		ZString MovementReferenceNumber { get; }
		ZBool MovementReferenceNumberComplete { get; }
		ZString ATBNumber { get; }
		ZString ExemptionReason { get; }
		ZString Annex30AType { get; }
		ZBool Annex30AFailureProcess { get; }
		ZString ExportDeclarationReference { get; }
		ZString MarksAndNumbers { get; }
		ZDateTime CustomsReleaseDate { get; }
	}
}
