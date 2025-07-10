using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC044CTransportEquipmentProvider : CargoWise.Customs.NL.MessageContracts.Interfaces.INCTSTransportEquipment
{
	public CC044CTransportEquipmentProvider(NctsArrivalHeaderContainer container)
	{
		this.container = Argument.NotNull(container, nameof(container));
	}
	readonly NctsArrivalHeaderContainer container;

	public int SequenceNumeric => container.BC_SequenceNumber;

	public string Id => !IsContainerMissing ? container.BC_ContainerNum : string.Empty;

	public int? SealsAffixedQuantity => IsContainerMissing ? null : Seals.Count;

	public IReadOnlyCollection<ISeal> Seals => IsContainerMissing ? Array.Empty<ISeal>() : seals ??= container.Seals
		.Where(x => x.BK_UnloadingState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS }))
		.Select(x => new SealsProvider(x.BK_SealNumber, x.BK_SequenceNumber))
		.OrderBy(x => x.SequenceNumeric)
		.ToArray();
	ISeal[] seals;

	public IReadOnlyCollection<IGoodsReference> GoodsReferences => IsContainerMissing ? Array.Empty<IGoodsReference>() : goodsReferences ??= GetGoodsReferences(container);
	IGoodsReference[] goodsReferences;

	static IGoodsReference[] GetGoodsReferences(NctsArrivalHeaderContainer container)
	{
		var goodsReferences = new List<IGoodsReference>();
		var index = 1;
		var containerPK = container.PK;
		foreach (var bill in container.NctsArrival.Bills)
		{
			foreach (var goodsItem in bill.ArrivalGoodsItems)
			{
				foreach (var package in goodsItem.Packages)
				{
					if (package.B5_TypeOfDifference.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DEC })
						&& package.ContainersPivot.Cast<GenPivot>().Any(p => p.XX_Relation2ID == containerPK))
					{
						goodsReferences.Add(new GoodsReferenceProvider(goodsItem, index++));
					}
				}
			}
		}
		return goodsReferences.ToArray();
	}

	bool IsContainerMissing => container.BC_UnloadedState == NctsUnloadedStateList.Codes.MIS;
}
