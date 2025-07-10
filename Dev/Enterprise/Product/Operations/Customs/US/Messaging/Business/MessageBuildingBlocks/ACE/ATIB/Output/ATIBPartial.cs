using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.TIBExtensionOrClosureResponse)]
	public partial class ATIBE0 : MessageBlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.TIBExtensionOrClosureResponse)]
	public partial class ATIBE1 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors Members

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return BrokerReferenceNumber; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ConditionCode; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeText; }
		}

		#endregion
	}
}
