using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	[TestedType(typeof(InvoiceOrgHeaderAddressLink))]
	public class InvoiceOrgHeaderAddressLinkTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var invoiceHeader = Factory.CreateInvoiceHeader(shipment);
			var orgHeaderAddressLink = new InvoiceOrgHeaderAddressLink
				(
					invoiceHeader.GIH_OA_ImporterAddress_ZAddress,
					() => invoiceHeader.GIH_OH_Importer,
					x => invoiceHeader.GIH_OH_Importer = x,
					x => invoiceHeader.GIH_OA_ImporterAddress = x
				);

			AssertType<ZAddress>(orgHeaderAddressLink.Address);
			AssertType<Func<ZGuid>>(orgHeaderAddressLink.HeaderPKGetter);
			AssertType<Action<ZGuid>>(orgHeaderAddressLink.HeaderPKSetter);
			AssertType<Action<ZGuid>>(orgHeaderAddressLink.AddressPKSetter);
		}
	}
}
