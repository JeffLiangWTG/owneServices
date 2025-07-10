using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[InputBlock("H6")]
	public partial class CRLH6 : MessageBlock, IStatusesAndErrors, ICargoReleaseStatus
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return CargoWise.Types.ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return MessageIdentifierCode; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return CargoWise.Types.ZString.Empty; }
		}

		#endregion

		ZString ICargoReleaseStatus.Status
		{
			get { return this.MessageIdentifierCode; }
		}

		ZString ICargoReleaseStatus.NarrativeMessage
		{
			get { return this.NarrativeMessage; }
		}

		ZString ICargoReleaseStatus.ErrorIdentifierCode
		{
			get { return ZString.Empty; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("R1")]
	public partial class CRLR1 : MessageBlock { }
}

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("R3")]
	public partial class CRLR3 : MessageBlock, ICountryOfOriginTariffDetailsBlock
	{
		ZInt ICountryOfOriginTariffDetailsBlock.RecordControlNumber { get { return RecordControlNumber; } }
		ZString ICountryOfOriginTariffDetailsBlock.CountryOfOrigin { get { return CountryOfOrigin; } }
		ZString ICountryOfOriginTariffDetailsBlock.TariffNumber { get { return TariffNumber; } }
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("R4")]
	public partial class CRLR4 : MessageBlock { }
}