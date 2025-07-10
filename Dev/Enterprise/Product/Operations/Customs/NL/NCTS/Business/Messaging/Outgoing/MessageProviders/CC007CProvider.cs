using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC007CProvider : MessageHeaderProvider, ICC007C
{
	public CC007CProvider(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	public ITransitOperation TransitOperation => new TransitOperationProvider(nctsHeader);

	public IReadOnlyCollection<INCTSAuthorization> Authorisations => authorisations ??= nctsHeader.CusAuthorizationUsages.OrderBy(x => x.AGC_Code).ThenBy(x => x.AGC_Number).Select((x, y) => new AuthorizationProvider(x, y + 1)).ToArray();
	IReadOnlyCollection<INCTSAuthorization> authorisations;

	public string CustomsOfficeOfDestination => nctsHeader.CommonMovementHeader is NctsDepartureMovementHeader depHeader ? depHeader.DestinationCustomsOfficeCode : ((NctsArrivalMovementHeader)nctsHeader.CommonMovementHeader)?.DestinationCustomsOfficeCode;

	public string TraderIdentificationNumber => CachedValueHelper.GetValue(ref traderIdentificationNumber, () => nctsHeader.DestinationTrader.Organisation.GetIdentificationNumber());
	CachedValue<string> traderIdentificationNumber;

	public string TraderCommunicationLanguage => nctsHeader.BH_CommunicationLanguage.ToLower();

	public INCTSConsignment Consignment => consignment ??= new ConsignmentProvider(nctsHeader);
	INCTSConsignment consignment;

	public override string MessageType => NLConstants.WCoTypeCodes.ArrivalNotification;
}
