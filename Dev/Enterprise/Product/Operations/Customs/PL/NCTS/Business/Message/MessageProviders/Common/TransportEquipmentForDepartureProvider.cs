using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class TransportEquipmentForDepartureProvider : ITransportEquipment
{
	public TransportEquipmentForDepartureProvider(int sequenceNumber, NctsDepartureHeaderContainer container, string messageCode)
	{
		this.container = Argument.NotNull(container, nameof(container));
		SequenceNumber = sequenceNumber.ToString();
	}
	readonly NctsDepartureHeaderContainer container;

	public string SequenceNumber { get; }

	public string ContainerIdentificationNumber => CachedValueHelper.GetValue(ref containerIdentificationNumber, () => container.BC_ContainerNum);
	CachedValue<string> containerIdentificationNumber;

	public string NumberOfSeals => CachedValueHelper.GetValue(ref numberOfSeals, () => Seal.Count.ToString());
	CachedValue<string> numberOfSeals;

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
		container.AdditionalSeals.Cast<CusSeal>().ForEach(x => result.Add(new SealProvider(++sequenceNumber, x.BK_SealNumber)));

		return result;
	}

	IReadOnlyCollection<IGoodsReference> GetGoodsReference()
	{
		return container.Header.Bills
			.SelectMany(bill => bill.GoodsItems.Where(goodsItem => goodsItem.ContainersSelected.Contains(ContainerIdentificationNumber)))
			.Select((x, i) => new GoodsReferenceProvider(i + 1, x.BY_DeclarationGoodsItemNumber))
			.ToArray();
	}
}
