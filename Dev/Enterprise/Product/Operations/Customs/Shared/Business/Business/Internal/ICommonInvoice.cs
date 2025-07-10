using System.Collections;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public interface ICommonInvoice : Common.ICommonInvoice
	{
		bool IsGroup { get; }
		BaseJobDeclaration JobDeclaration { get; }
		IAllInvoiceLines InvoiceLines { get; }
		ZString UserFriendlyCode { get; }

		CodeDescriptionPairList ChargeTypeList { get; }

		CodeDescriptionPairList AllChargeTypeList { get; }
	}

	public interface IAllInvoiceLines : IEnumerable
	{
		bool Contains(BaseJobComInvoiceLine invoiceLine);
		int Count { get; }
	}
}
