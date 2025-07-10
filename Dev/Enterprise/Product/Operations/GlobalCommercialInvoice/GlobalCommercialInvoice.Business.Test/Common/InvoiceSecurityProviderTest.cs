using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	[TestedType(typeof(InvoiceSecurityProvider))]
	public class InvoiceSecurityProviderTest : TestCaseWithFactory
	{
		public void TestIsReadOnly()
		{
			var securityCore = Factory.CreateSecurityCore();
			securityCore.ShipmentsCommercialInvoiceEdit.IsAllowed = false;
			securityCore.BookingsCommercialInvoiceEdit.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert(InvoiceSecurityProvider.IsReadOnly(Factory, Factory.CreateNewShipment().PK));
				Assert(InvoiceSecurityProvider.IsReadOnly(Factory, Factory.CreateNewBookingQuick().ViewPK));
				Assert(InvoiceSecurityProvider.IsReadOnly(Factory, Factory.CreateNewBookingWithQuote().ViewPK));
				Assert(InvoiceSecurityProvider.IsReadOnly(Factory, Factory.CreateNewBookingSpotQuote().ViewPK));
				Assert(!InvoiceSecurityProvider.IsReadOnly(Factory, Factory.New<DummyBusinessObject>().PK));
				Assert(!InvoiceSecurityProvider.IsReadOnly(Factory, ZGuid.Empty));
			}
		}
	}
}
