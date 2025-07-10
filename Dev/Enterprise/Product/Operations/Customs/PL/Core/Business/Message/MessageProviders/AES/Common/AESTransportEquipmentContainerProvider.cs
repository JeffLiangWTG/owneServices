using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public class AESTransportEquipmentContainerProvider : ITransportEquipment
{
	public AESTransportEquipmentContainerProvider(int sequenceNumber, BaseCusContainer container, bool everyInvoiceLineInSameContainer)
	{
		SequenceNumber = sequenceNumber;
		this.container = Argument.NotNull(container, nameof(container));
		this.everyInvoiceLineInSameContainer = everyInvoiceLineInSameContainer;
	}

	readonly BaseCusContainer container;
	readonly bool everyInvoiceLineInSameContainer;

	public int SequenceNumber { get; }

	public string ContainerIdentificationNumber => container.CO_ContainerNumber;

	public int NumberOfSeals => Seals.Count;

	public IReadOnlyCollection<ISeal> Seals => seals ?? (seals = GetContainerSeals());
	ISeal[] seals;

	public IReadOnlyCollection<IGoodsReference> GoodsReferences => goodsReferences ?? (goodsReferences =
		!string.IsNullOrEmpty(ContainerIdentificationNumber) && everyInvoiceLineInSameContainer
			? Array.Empty<IGoodsReference>()
			: container.InvoiceLinePivotCollection.InvoiceLinesAssociated
				.Select((x, i) => new AESGoodsReferenceProvider(i + 1, x.CusEntryLine.CL_LineNumber))
				.ToArray<IGoodsReference>());
	IGoodsReference[] goodsReferences;

	ISeal[] GetContainerSeals() => new[] { container.CO_Seal, container.CO_SecondSeal }.Where(x => !x.IsEmpty)
		.Select((x, i) => new AESSealProvider(i + 1, x)).ToArray<ISeal>();
}
