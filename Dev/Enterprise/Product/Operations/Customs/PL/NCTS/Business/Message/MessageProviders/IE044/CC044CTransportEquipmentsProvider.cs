using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Message.MessageProviders.IE044;

class CC044CTransportEquipmentsProvider : ITransportEquipment
{
	public CC044CTransportEquipmentsProvider(int sequenceNumber, NctsArrivalHeaderContainer nctsContainer)
	{
		container = Argument.NotNull(nctsContainer, nameof(nctsContainer));
		SequenceNumber = sequenceNumber.ToString();
	}

	readonly NctsArrivalHeaderContainer container;

	public string SequenceNumber { get; }

	public string ContainerIdentificationNumber => container.BC_ContainerNum;

	public string NumberOfSeals => Seal.Count.ToString();

	public IReadOnlyCollection<ISeal> Seal => seals ?? (seals = GetSeals());
	IReadOnlyCollection<ISeal> seals;

	public IReadOnlyCollection<IGoodsReference> GoodsReference => goodsReference ?? (goodsReference = GetGoodsReference());
	IReadOnlyCollection<IGoodsReference> goodsReference;

	IReadOnlyCollection<ISeal> GetSeals()
	{
		var result = new List<ISeal>();

		var sequenceNumber = 0;
		if (!container.BC_Seal1.IsEmpty)
		{
			result.Add(new SealProvider(++sequenceNumber, container.BC_Seal1));
		}
		if (!container.BC_Seal2.IsEmpty)
		{
			result.Add(new SealProvider(++sequenceNumber, container.BC_Seal2));
		}
		container.SealsForMessaging.Cast<CusSeal>().ForEach(x => result.Add(new SealProvider(++sequenceNumber, x.BK_SealNumber)));

		return result;
	}

	IReadOnlyCollection<IGoodsReference> GetGoodsReference() => container.NctsArrival.Bills
		.SelectMany(bill => bill.ArrivalGoodsItems.Select(x => x.BY_DeclarationGoodsItemNumber))
		.Select((x, i) => new GoodsReferenceProvider(i + 1, x))
		.ToArray();
}
