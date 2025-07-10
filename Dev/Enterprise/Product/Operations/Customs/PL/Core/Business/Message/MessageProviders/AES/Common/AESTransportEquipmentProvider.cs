using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using CusEntryInstruction = Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.PL.Business;

public class AESTransportEquipmentProvider : ITransportEquipment
{
	public AESTransportEquipmentProvider(int sequenceNumber, CusEntryInstruction cusEntryInstruction, CusEquipment cusEquipment, bool containerIndicatorExists)
	{
		SequenceNumber = sequenceNumber;
		this.cusEntryInstruction = Argument.NotNull(cusEntryInstruction, nameof(cusEntryInstruction));
		this.cusEquipment = Argument.NotNull(cusEquipment, nameof(cusEquipment));
		this.containerIndicatorExists = containerIndicatorExists;
	}

	readonly CusEntryInstruction cusEntryInstruction;
	readonly CusEquipment cusEquipment;
	readonly bool containerIndicatorExists;

	public int SequenceNumber { get; }

	public string ContainerIdentificationNumber => CachedValueHelper.GetValue(ref containerIdentificationNumber, () => containerIndicatorExists
		? cusEquipment.CEQ_IdentificationNumber
		: null);
	CachedValue<string> containerIdentificationNumber;

	public int NumberOfSeals => CachedValueHelper.GetValue(ref numberOfSeals, () => Seals.Count);
	CachedValue<int> numberOfSeals;

	public IReadOnlyCollection<ISeal> Seals => seals ?? (seals = cusEquipment.Seals.Select((x, i) => new AESSealProvider(i + 1, x.BK_SealNumber)).ToArray<ISeal>());
	IReadOnlyCollection<ISeal> seals;

	public IReadOnlyCollection<IGoodsReference> GoodsReferences => goodsReferences ?? (goodsReferences = cusEntryInstruction.EntryHeader?.AllEntryLines
		.Cast<Declaration.CusEntryLine>()
		.Select((x, i) => new AESGoodsReferenceProvider(i + 1, x.CL_LineNumber))
		.ToArray<IGoodsReference>() ?? Array.Empty<IGoodsReference>());
	IReadOnlyCollection<IGoodsReference> goodsReferences;
}
