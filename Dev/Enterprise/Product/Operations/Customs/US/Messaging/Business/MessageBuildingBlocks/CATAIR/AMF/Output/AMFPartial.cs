using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAddResponse)]
	public partial class AMFDollarA : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAddResponse)]
	public partial class AMFDollar5 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAddResponse)]
	public partial class AMFDollar6 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAddResponse)]
	public partial class AMFDollar7 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ErrorMessageIdentifier; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
