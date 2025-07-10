using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(InvoiceLineChargeLookups))]
	sealed class InvoiceLineChargeLookupsBaseOnlyTest : InvoiceLineChargeLookupsAbstractTest<InvoiceLineChargeLookups>
	{
		protected override string MessageType => JobMessageTypeList.Codes.MiscellaneousCustoms;
	}
}
