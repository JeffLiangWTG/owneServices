using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class TransportEquipmentProvider(int sequenceNumber, CusExitContainer exitContainer, CusExitConsignment exitConsignment) : ITransportEquipment
{
	readonly CusExitContainer exitContainer = Argument.NotNull(exitContainer, nameof(exitContainer));
	readonly CusExitConsignment cusExitConsignment = Argument.NotNull(exitConsignment, nameof(exitConsignment));

	public int SequenceNumber => sequenceNumber;

	public string ContainerIdentificationNumber => exitContainer.CXN_ContainerNumber;

	public int NumberOfSeals => Seals.Count;

	public IReadOnlyCollection<ISeal> Seals => seals ??= exitContainer.AllSealNumbers
		.Cast<CusExitSeal>()
		.Select((x, i) => new AESSealProvider(i + 1, x.BK_SealNumber))
		.ToArray();
	IReadOnlyCollection<ISeal> seals;

	public IReadOnlyCollection<IGoodsReference> GoodsReferences => goodsReferences ??= cusExitConsignment
		.CusExitConsignmentItems
		.Where(x => x.CusExitConsignmentPivots
			.Any(pivot => pivot.CNP_CXN_Container == exitContainer.PK))
		.Select(item => new AESGoodsReferenceProvider(exitContainer.CXN_Sequence, item.CCI_LineNumber))
		.ToArray<IGoodsReference>();
	IReadOnlyCollection<IGoodsReference> goodsReferences;
}
