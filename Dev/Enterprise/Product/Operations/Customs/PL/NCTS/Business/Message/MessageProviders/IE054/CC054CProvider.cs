using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.NCTS.Business.Message.MessageProviders.IE054;
using ICustomsOffice = CargoWise.Customs.PL.MessageContracts.Interfaces.ICustomsOffice;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC054CProvider : CCProviderBase, ICC054C
{
	public CC054CProvider(MessageSendingObject sendingObject, string messageType)
		: base(Argument.NotNull(sendingObject?.NctsHeader?.MovementHeader, $"{nameof(sendingObject)}?.{nameof(MessageSendingObject.NctsHeader)}?.{nameof(NctsHeader.MovementHeader)}"),
			Argument.NotNull(messageType, nameof(messageType)))
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		movementHeader = sendingObject.NctsHeader.MovementHeader;
	}

	readonly MessageSendingObject sendingObject;
	readonly NctsDepartureMovementHeader movementHeader;

	public ITransitOperationIE054 TransitOperation => transitOperation ?? (transitOperation = new CC054CTransitOperationProvider(sendingObject));
	ITransitOperationIE054 transitOperation;

	public ICustomsOffice CustomsOfficeOfDeparture => CachedValueHelper.GetValue(ref customsOfficeOfDeparture, () => CustomsOfficeProvider.NewOrNull(GetCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture)));
	CachedValue<ICustomsOffice> customsOfficeOfDeparture;

	public IHolderOfTheTransitProcedureWithMaxLength HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure = new HolderOfTheTransitProcedureProvider(nctsHeader.Principal, movementHeader));
	IHolderOfTheTransitProcedureWithMaxLength holderOfTheTransitProcedure;

	public PhaseID? PhaseID => null; // TODO: WI00542973

	NctsPLOfficeCode GetCustomsOfficeOfType(ZString officeCode) => nctsHeader.MovementHeader.CustomsOffices.Cast<NctsPLOfficeCode>().FirstOrDefault(x => x.CY_Code == officeCode);
}
