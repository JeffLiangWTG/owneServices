using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	class InvoiceOrgHeaderAddressLink(ZAddress address, Func<ZGuid> headerPKGetter, Action<ZGuid> headerPKSetter, Action<ZGuid> addressPKSetter)
	{
		public ZAddress Address { get; } = address;
		public Func<ZGuid> HeaderPKGetter { get; } = headerPKGetter;
		public Action<ZGuid> HeaderPKSetter { get; } = headerPKSetter;
		public Action<ZGuid> AddressPKSetter { get; } = addressPKSetter;
	}
}
