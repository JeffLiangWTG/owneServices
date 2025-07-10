using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class TransportEquipmentWrapper : IDMSOutgoingTransportEquipment
{
	public TransportEquipmentWrapper(CusContainer container, int sequenceNumeric)
	{
		this.container = Argument.NotNull(container, nameof(container));
		SequenceNumeric = sequenceNumeric;
	}
	readonly CusContainer container;

	public int SequenceNumeric { get; }

	public string Id => container.CO_ContainerNumber;

	public int SealsAffixedQuantity
	{
		get
		{
			return Seals.Count;
		}
	}

	public IReadOnlyCollection<IGoodsReference> GoodsReferences => goodsReferences ??= container.InvoiceLinePivotCollection.Select((x, index) => new GoodsReferenceWrapper((CusContainerInvoiceLinePivot)x, index + 1)).ToArray();
	IReadOnlyCollection<IGoodsReference> goodsReferences;

	public IReadOnlyCollection<ISeal> Seals => seals ??= GetSeals().ToArray();
	IReadOnlyCollection<ISeal> seals;

	IEnumerable<ISeal> GetSeals()
	{
		var index = 1;
		var sealNo = container.CO_Seal;
		if (!sealNo.IsEmpty)
		{
			yield return new SealWrapper(sealNo, index++);
		}

		var secondSeal = container.CO_SecondSeal;
		if (!secondSeal.IsEmpty)
		{
			yield return new SealWrapper(secondSeal, index++);
		}
		foreach (var seal in container.AdditionalSeals)
		{
			yield return new SealWrapper(seal, index++);
		}
	}
}
