using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(GroupInvoiceChargeLookups))]
	sealed class GroupInvoiceChargeLookupsBaseOnlyTest : GroupInvoiceChargeLookupsAbstractTest<GroupInvoiceChargeLookups>
	{
		protected override string MessageType => JobMessageTypeList.Codes.MiscellaneousCustoms;
	}
}
