using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CC507CGoodsShipmentProvider(CusExitReport exitReport) : ICC507CGoodsShipment
{
	readonly CusExitReport cusExitReport = Argument.NotNull(exitReport, nameof(exitReport));
	readonly CusExitConsignment cusExitConsignment = Argument.NotNull(exitReport.Consignment, $"{nameof(exitReport)}.{nameof(CusExitReport.Consignment)}");
	
	public IReadOnlyCollection<IDocument> AdditionalInformationPl => additionalInformationPl ??= cusExitReport.AdditionalInfos
		.Where(x => x.CSI_SubType == AdditionalInfoKindList.Codes.INF)
		.GroupBy(x => new { x.CSI_Code, x.CSI_ReferenceNumber })
		.Select((x, i) => new AESDocumentProvider(x.FirstOrDefault(), true))
		.ToArray<IDocument>();
	IReadOnlyCollection<IDocument> additionalInformationPl;

	public ICC507CConsignment Consignment => consignment ??= new CC507CConsignmentProvider(cusExitReport);
	ICC507CConsignment consignment;

	public IReadOnlyCollection<ICC507CGoodsItem> GoodsItems => goodsItems ??= cusExitConsignment
		.CusExitConsignmentItems
		.Select(x => new CC507CGoodsItemProvider(x, cusExitReport))
		.ToArray();
	IReadOnlyCollection<ICC507CGoodsItem> goodsItems;
}
