using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.ISF.Business.MessageProcessors;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ForwardingShipmentToISFProcessorTest : TestCaseWithFactory
	{
		public void TestGetFromObjectFactory()
		{
			var processor = ObjectFactory.Get<IProcessor>("ForwardingShipmentToISFProcessor", Factory.New<ForwardingShipment>());
			AssertType<ForwardingShipmentToISFProcessor>(processor);
		}

		public void TestProcess_HVLV_CreatesISFHeader()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			Factory.Save();

			var isfHeaders = Factory.Load<CusISFHeader>(new ZQuery());
			AssertEquals("pre condition", 0, isfHeaders.Length);

			var processor = new ForwardingShipmentToISFProcessor(shipment);

			processor.Process(null);

			isfHeaders = Factory.Load<CusISFHeader>(new ZQuery());
			AssertEquals("1 ISF Header should be created", 1, isfHeaders.Length);
			AssertEquals("Header should not be saved yet (will be saved by LWK)", false, isfHeaders.Single().IsInDatabase);
		}
	}
}
