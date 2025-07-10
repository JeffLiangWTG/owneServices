using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class TransportEquipmentsForNctsHeaderContainerProvider : CargoWise.Customs.NL.MessageContracts.Interfaces.INCTSTransportEquipment
{
	readonly NctsDepartureHeaderContainer headerContainer;

	public TransportEquipmentsForNctsHeaderContainerProvider(NctsDepartureHeaderContainer headerContainer, int sequenceNumber)
	{
		this.headerContainer = Argument.NotNull(headerContainer, nameof(headerContainer));
		SequenceNumeric = sequenceNumber;
	}

	public int SequenceNumeric { get; }

	public string Id => headerContainer.BC_ContainerNum;

	public int? SealsAffixedQuantity => Seals.Count;

	public IReadOnlyCollection<ISeal> Seals => seals ?? (seals = GetSeals().ToArray());
	ISeal[] seals;

	IEnumerable<ISeal> GetSeals()
	{
		var idx = 1;
		var seal1 = headerContainer.BC_Seal1;
		if (!seal1.IsEmpty)
		{
			yield return new SealsProvider(seal1, idx++);
		}

		var seal2 = headerContainer.BC_Seal2;
		if (!seal2.IsEmpty)
		{
			yield return new SealsProvider(seal2, idx++);
		}

		foreach (var additionalSealNumber in headerContainer.AdditionalSeals
			.Cast<CusSeal>()
			.OrderBy(s => s.BK_SequenceNumber)
			.Select(s => s.BK_SealNumber))
		{
			yield return new SealsProvider(additionalSealNumber, idx++);
		}
	}

	public IReadOnlyCollection<IGoodsReference> GoodsReferences
	{
		get
		{
			if (goodsReferences == null)
			{
				goodsReferences = new List<IGoodsReference>();
				var index = 1;
				foreach (var bill in headerContainer.Header.Bills)
				{
					foreach (var goodsItem in bill.GoodsItems)
					{
						if (goodsItem.Packages.Cast<NctsPackage>().Any(x => x.ContainersPivot.Cast<GenPivot>().Any(p => p.XX_Relation2ID == headerContainer.PK)))
						{
							goodsReferences.Add(new GoodsReferenceProvider(goodsItem, index++));
						}
					}
				}
			}
			return goodsReferences;
		}
	}
	List<IGoodsReference> goodsReferences;
}
