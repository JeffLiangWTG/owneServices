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

public class CC507CGoodsItemProvider(CusExitConsignmentItem exitConsignmentItem, CusExitReport exitReport) : ICC507CGoodsItem
{
	readonly CusExitConsignmentItem cusExitConsignmentItem = Argument.NotNull(exitConsignmentItem, nameof(exitConsignmentItem));
	readonly CusExitReport cusExitReport = Argument.NotNull(exitReport, nameof(exitReport));

	public int DeclarationGoodsItemNumber => cusExitConsignmentItem.CCI_LineNumber;

	public string ReferenceNumberUCR => cusExitConsignmentItem.CCI_UniqueConsignmentReference;

	public IReadOnlyCollection<IAuthorisationNumber> Authorisations => Array.Empty<IAuthorisationNumber>();

	public ICommodityBase Commodity => CachedValueHelper.GetValue(ref commodity,
		() => cusExitReport.CER_Calc_Discrepancies
			? new CommodityProvider(cusExitConsignmentItem)
			: null);
	CachedValue<ICommodityBase> commodity;

	public IReadOnlyCollection<IPackaging> Packaging => packaging ??= cusExitReport.CER_Calc_Discrepancies
		? GetPackages()
		: Array.Empty<IPackaging>();
	IReadOnlyCollection<IPackaging> packaging;

	public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ??= cusExitReport.CER_Calc_Discrepancies
		? GetTransportDocuments()
		: Array.Empty<IDocument>();
	IReadOnlyCollection<IDocument> transportDocuments;

	public IReadOnlyCollection<IDocument> AdditionalReferences => Array.Empty<IDocument>();

	IReadOnlyCollection<IPackaging> GetPackages() => cusExitConsignmentItem
		.CusExitConsignmentPackagePivots
		.Select(x => new PackagingProvider(x.Package))
		.ToArray();

	IReadOnlyCollection<IDocument> GetTransportDocuments() => cusExitConsignmentItem
		.AdditionalInfos
		.Where(x => x.CSI_SubType == AdditionalInfoKindList.Codes.TRA)
		.Select((x, i) => new AESDocumentProvider(x, true))
		.ToArray<IDocument>();
}
