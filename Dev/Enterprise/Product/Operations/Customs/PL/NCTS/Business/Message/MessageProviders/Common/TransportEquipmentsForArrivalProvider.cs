using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class TransportEquipmentsForArrivalProvider : ITransportEquipment
{
	readonly NctsContainer container;

	public TransportEquipmentsForArrivalProvider(int sequenceNumber, NctsContainer nctsContainer)
	{
		this.container = Argument.NotNull(nctsContainer, nameof(nctsContainer));
		SequenceNumber = sequenceNumber.ToString();
	}
	public string SequenceNumber { get; }

	public string ContainerIdentificationNumber => container.ContainerNumber;

	public string NumberOfSeals => CachedValueHelper.GetValue(ref numberOfSeals, () => Seal.Count.ToString());
	CachedValue<string> numberOfSeals;

	public IReadOnlyCollection<ISeal> Seal => seals ?? (seals = GetSeals().ToArray());
	IReadOnlyCollection<ISeal> seals;

	IEnumerable<ISeal> GetSeals()
	{
		var idx = 1;
		var seal1 = container.BC_Seal1;
		if (!seal1.IsEmpty)
		{
			yield return new SealProvider(idx++, seal1);
		}

		var seal2 = container.BC_Seal2;
		if (!seal2.IsEmpty)
		{
			yield return new SealProvider(idx++, seal2);
		}

		foreach (var additionalSealNumber in container.SealsForMessaging
					.Cast<CusSeal>()
					.OrderBy(s => s.BK_SequenceNumber)
					.Select(s => s.BK_SealNumber))
		{
			yield return new SealProvider(idx++, additionalSealNumber);
		}
	}
	public IReadOnlyCollection<IGoodsReference> GoodsReference => goodsReference ?? (goodsReference = GetGoodsReference());
	IReadOnlyCollection<IGoodsReference> goodsReference;

	IReadOnlyCollection<IGoodsReference> GetGoodsReference() => container.ItemNumbers.Cast<NctsContainerItem>()
		.OrderBy(x => x.CY_Order)
		.Select((x, i) => new GoodsReferenceProvider(i + 1, x.CY_DataNumeric))
		.ToArray();
}
