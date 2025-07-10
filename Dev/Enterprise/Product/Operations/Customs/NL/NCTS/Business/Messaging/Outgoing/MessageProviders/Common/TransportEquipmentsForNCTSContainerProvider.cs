using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class TransportEquipmentsForNCTSContainerProvider : CargoWise.Customs.NL.MessageContracts.Interfaces.INCTSTransportEquipment
{
	readonly NctsContainer container;

	public TransportEquipmentsForNCTSContainerProvider(NctsContainer container, int sequence)
	{
		this.container = Argument.NotNull(container, nameof(container));
		SequenceNumeric = sequence;
	}

	public int SequenceNumeric { get; }

	public string Id => container.ContainerNumber;

	public int? SealsAffixedQuantity => Seals.Count;

	public IReadOnlyCollection<IGoodsReference> GoodsReferences => goodsReferences ??= container.ItemNumbers.GetElementsHaving(CusCodeDataTypeList.Codes.ItemNumber).Select((rf, index) => new GoodsReferenceProvider(rf, index + 1)).ToList<IGoodsReference>();
	List<IGoodsReference> goodsReferences;

	public IReadOnlyCollection<ISeal> Seals => seals ??= GetSeals().ToArray();
	ISeal[] seals;

	IEnumerable<ISeal> GetSeals()
	{
		var index = 1;
		var seal1 = container.BC_Seal1;
		if (!seal1.IsEmpty)
		{
			yield return new SealsProvider(seal1, index++);
		}

		var seal2 = container.BC_Seal1;
		if (!seal2.IsEmpty)
		{
			yield return new SealsProvider(seal2, index++);
		}

		foreach (var additionalSeal in container.SealsForMessaging.Cast<CusSeal>().OrderBy(s => s.BK_SequenceNumber).Select(s => s.BK_SealNumber))
		{
			yield return new SealsProvider(additionalSeal, index++);
		}
	}
}
