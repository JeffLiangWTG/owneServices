using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobComInvoiceHeaderRoutingSupportTest : TestCaseWithFactory
	{
		public void TestITransportParentMembers()
		{
			BaseJobComInvoiceHeader header = Factory.New<BaseJobComInvoiceHeader>();
			header.JZ_InvoiceNumber = "078345";
			ITransportParent transportParent = header;
			TransportSupporter supporter = transportParent.TransportSupporter;

			AssertEquals(ZString.Empty, supporter.BillOfLading);
			AssertEquals("078345", supporter.ConsignmentRef);
			AssertEquals("078345", supporter.Description);
			AssertEquals(ZString.Empty, supporter.TransportMode);
			AssertEquals(ZString.Empty, supporter.ContainerMode);
			AssertEquals("Transports", header.Transports, transportParent.Transports);
			AssertEquals(Core.Constants.TransportParentTypes.CommercialInvoice, transportParent.TypeCode);
		}

		public void TestIRoutingSupportMembers()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			BaseJobComInvoiceHeader header = declaration.Invoices.AddNew();
			IRoutingSupport routingSupport = header;
			AssertEquals("Transport Mode", Core.Constants.TransportModes.Sea, routingSupport.TransportMode);
		}
	}
}
