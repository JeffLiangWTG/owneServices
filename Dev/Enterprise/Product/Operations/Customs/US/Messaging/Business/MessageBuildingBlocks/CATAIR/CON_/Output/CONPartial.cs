namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressQueryResponse)]
	public partial class CONKA : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressQueryResponse)]
	public partial class CONKB : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressQueryResponse)]
	public partial class CONKC : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressQueryResponse)]
	public partial class CONKD : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressQueryResponse)]
	public partial class CONKE : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressQueryResponse)]
	public partial class CONKF : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		CargoWise.Types.ZString IStatusesAndErrors.LineNumber
		{
			get { return CargoWise.Types.ZString.Empty; }
		}

		CargoWise.Types.ZString IStatusesAndErrors.Code
		{
			get { return ErrorMessageIdentifierCode; }
		}

		CargoWise.Types.ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		CargoWise.Types.ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return CargoWise.Types.ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressAddResponse)]
	public partial class CONIA : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressAddResponse)]
	public partial class CONIB : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		CargoWise.Types.ZString IStatusesAndErrors.LineNumber
		{
			get { return LineItemSequenceNumber.ToString(); }
		}

		CargoWise.Types.ZString IStatusesAndErrors.Code
		{
			get { return NarrativeMessageIdentifier; }
		}

		CargoWise.Types.ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		CargoWise.Types.ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return CargoWise.Types.ZString.Empty; }
		}

		#endregion
	}
}