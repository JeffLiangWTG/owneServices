using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using JobComInvoiceHeader = Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader;

namespace Enterprise.Customs.PL.Business;

public class AESDeliveryTermsProvider : IDeliveryTerms
{
	public AESDeliveryTermsProvider(JobComInvoiceHeader invoiceHeader)
	{
		this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
	}

	readonly JobComInvoiceHeader invoiceHeader;

	public string IncotermCode => invoiceHeader.JZ_IncoTerm;

	public string UNLocode => CachedValueHelper.GetValue(ref unlocode, () =>
	{
		var agreedPlaceCode = invoiceHeader.ZG_AgreedPlaceCode;

		return (agreedPlaceCode.Length == 5 && CheckIsSystemUNLocode(agreedPlaceCode))
			? agreedPlaceCode
			: null;
	});
	CachedValue<string> unlocode;

	public string Location => invoiceHeader.JZ_IncoTermPlace;

	public string Country => CachedValueHelper.GetValue(ref country, () =>
	{
		var agreedPlaceCode = invoiceHeader.ZG_AgreedPlaceCode;
		return agreedPlaceCode.Length == 2 ? agreedPlaceCode : null;
	});
	CachedValue<string> country;

	public string Text => CachedValueHelper.GetValue(ref text, () => CheckC0596(invoiceHeader.JZ_IncoTerm) ? invoiceHeader.JZ_IncoTermPlace : null);
	CachedValue<string> text;

	static bool CheckC0596(ZString incoTerm) => incoTerm == JZIncoTermList.Codes.XXX;

	bool CheckIsSystemUNLocode(ZString unlocode) => new RefUNLOCO.Loader(invoiceHeader.Factory).Load(unlocode)?.RL_IsSystem ?? false;
}
