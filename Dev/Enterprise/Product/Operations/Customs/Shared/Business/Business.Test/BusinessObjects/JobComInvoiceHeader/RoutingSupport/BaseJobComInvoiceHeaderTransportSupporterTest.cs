using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Security;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobComInvoiceHeaderTransportSupporterTest : TransportSupporterTestCase<BaseJobComInvoiceHeaderTransportSupporter>
	{
		public void TestITransportParentMembers()
		{
			var header = Factory.New<BaseJobComInvoiceHeader>();
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

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
		}

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent parent = Factory.New<BaseJobComInvoiceHeader>();
			return parent.TransportSupporter;
		}

		protected override ZString TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}
	}
}
