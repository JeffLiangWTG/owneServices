using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportJobComInvoiceHeaderLookups : JobComInvoiceHeaderLookups
{
	public ExportJobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
		: base(parent)
	{
	}

	public override ICodeDescriptionPairList ValuationCodeList => Universal.RefCusCodeListTypes.GetCachedList(Factory, GlbCompany.CurrentCompany.Country.Code,
		UniversalReferenceConstants.RefCusCodeListType.Codes.InvoiceHeaderValuationCodes, ZDateTime.Today);
}
