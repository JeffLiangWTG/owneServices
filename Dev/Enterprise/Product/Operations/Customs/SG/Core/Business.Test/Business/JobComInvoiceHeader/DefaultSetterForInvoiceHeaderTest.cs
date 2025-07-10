using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	class DefaultSetterForInvoiceHeaderTest : TestCaseWithFactory
	{
		public void TestDefaultForNewElementCore()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignor = OrgHeader.New(Factory);
			consignor.MiscServ.OM_EXDefaultIncoTerm = "321";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OH_Supplier = consignor.PK;
			AssertEquals("DO NOT Default Inco Term on declaration level.", "", declaration.JE_ShipmentIncoTerm);
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("Default Inco Term on invoice level.", "321", invoice.JZ_IncoTerm);
		}
	}
}
