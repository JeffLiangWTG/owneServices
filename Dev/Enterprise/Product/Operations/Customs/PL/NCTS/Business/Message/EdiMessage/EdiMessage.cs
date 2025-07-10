using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class EDIMessage(BusinessObjectFactory factory, DataRow row) : BaseEDIMessage(factory, row), Integration.Customs.PLNCTS.IEDIMessage
{
	public const string PL_NCTS_LRN_PlaceHolder = "_PL_LRN_PLACEHOLDER_";

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_ApplicationCode = ApplicationCodes.PLCustomsNCTS;
	}

	protected override MessageNumberPrefix GetMessagePrefix()
		=> !EM_MessageSubType.IsEmpty
			? base.GetMessagePrefix() with { MessageCode5Symbols = $"IE{EM_MessageSubType}" }
			: base.GetMessagePrefix();

	protected override string SendersReferencePlaceHolderOverride => PL_NCTS_LRN_PlaceHolder;

	protected override string GetSendersReference() =>
		EM_MessageType == EUJobMessageTypeList.Codes.NctsDeparture
		&& EM_LinkedObject is NctsDepartureMovementHeader movementHeader
			? movementHeader.GetLRNAndSetIfNeeded() ?? string.Empty
			: string.Empty;
}
