using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CC507CConsignmentProvider(CusExitReport exitReport) : ICC507CConsignment
{
	readonly CusExitReport cusExitReport = Argument.NotNull(exitReport, nameof(exitReport));
	readonly CusExitConsignment cusExitConsignment = Argument.NotNull(exitReport.Consignment, $"{nameof(exitReport)}.{nameof(CusExitReport.Consignment)}");
	readonly CusExitHeader cusExitHeader = Argument.NotNull(exitReport.Header, $"{nameof(exitReport)}.{nameof(CusExitReport.Header)}");

	public string ModeOfTransportAtTheBorder => cusExitReport.CER_TransportType;

	public string ReferenceNumberUcr => cusExitConsignment.CXC_UniqueConsignmentReference;

	public ICC507CExitCarrier ExitCarrier => CachedValueHelper.GetValue(ref exitCarrier,
		() => CC507CExitCarrierProvider.NewOrNull(cusExitHeader.Carrier));
	CachedValue<ICC507CExitCarrier> exitCarrier;

	public IReadOnlyCollection<ITransportEquipment> TransportEquipments => transportEquipments ??= cusExitReport.CER_Calc_Discrepancies
		? GetTransportEquipments()
		: Array.Empty<ITransportEquipment>();
	IReadOnlyCollection<ITransportEquipment> transportEquipments;

	public ILocationOfGoods LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoods,
		() => new AESLocationOfGoodsProvider(exitReport.GoodsLocation));
	CachedValue<ILocationOfGoods> locationOfGoods;

	public IActiveBorderTransportMeans ActiveBorderTransportMeans =>
		activeBorderTransportMeans ??= new ActiveBorderTransportMeansProvider(cusExitReport);
	IActiveBorderTransportMeans activeBorderTransportMeans;

	public IReadOnlyCollection<IDocument> TransportDocument => transportDocument ??= cusExitReport.CER_Calc_Discrepancies
		? GetTransportDocuments()
		: Array.Empty<IDocument>();
	IReadOnlyCollection<IDocument> transportDocument;

	IReadOnlyCollection<ITransportEquipment> GetTransportEquipments() => cusExitHeader
		.CusExitContainers
		.Select((x, i) => new TransportEquipmentProvider(i + 1, x, cusExitConsignment))
		.ToArray<ITransportEquipment>();

	IReadOnlyCollection<IDocument> GetTransportDocuments() => cusExitReport
		.AdditionalInfos
		.Where(x => x.CSI_SubType == AdditionalInfoKindList.Codes.TRA)
		.Select((x, i) => new AESDocumentProvider(x, true))
		.ToArray<IDocument>();
}
