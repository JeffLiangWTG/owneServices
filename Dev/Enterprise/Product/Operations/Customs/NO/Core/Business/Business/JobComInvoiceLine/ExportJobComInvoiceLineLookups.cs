using CargoWise.Integration;

namespace Enterprise.Customs.NO.Business;

sealed class ExportJobComInvoiceLineLookups(JobComInvoiceLine parent) : JobComInvoiceLineLookups(parent)
{
	public override ICodeDescriptionPairList PrimaryPreferenceList => Factory.GetCachedValue("F3039B0D-E7D1-42EC-B8CD-AA0FD05E4D5E", () =>
	{
		var list = new PrimaryPreferenceCodeList();
		list.RemoveCode(PrimaryPreferenceCodeList.Codes.J);
		return list;
	});
}
