using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used later")]
public class IE141RootProvider : IIE141Root, INCTSPrettierData
{
	public IE141RootProvider(MessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		var nctsHeader = messageSendingObject.NctsHeader;
		movementHeader = Argument.NotNull(nctsHeader.MovementHeader, $"{nameof(messageSendingObject)}.{nameof(MessageSendingObject.NctsHeader)}.{nameof(NctsHeader.MovementHeader)}");
	}

	readonly NctsDepartureMovementHeader movementHeader;
	readonly MessageSendingObject messageSendingObject;

	public IChannelRepresentativeAndLocationOfGoods CountrySpecificDataPL => countrySpecificDataPL ??= new CountrySpecificDataPLProvider(movementHeader, messageSendingObject);
	IChannelRepresentativeAndLocationOfGoods countrySpecificDataPL;

	public ICC141C CC141C => cc141C ??= new CC141CProvider(movementHeader, Constants.MessageTypeCodes.IE141, messageSendingObject);
	ICC141C cc141C;

	#region Implementation of INCTSPrettierData

	void INCTSPrettierData.FillSharedFields(NCTSPrettierSharedFields sharedFields)
	{
		sharedFields.MessageType = Constants.MessageTypeCodes.IE141;
		sharedFields.MRN = CC141C.TransitOperationMRN;
	}

	IReadOnlyCollection<INCTSPrettierAdditionalBlock> INCTSPrettierData.AdditionalBlocks => Array.Empty<INCTSPrettierAdditionalBlock>();

	#endregion
}
