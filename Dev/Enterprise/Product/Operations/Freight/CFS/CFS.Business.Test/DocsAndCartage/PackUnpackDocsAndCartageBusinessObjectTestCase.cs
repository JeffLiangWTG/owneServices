using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(PackUnpackDocsAndCartage))]
	public class PackUnpackDocsAndCartageBusinessObjectTestCase : JobDocsAndCartageBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			PackUnpackShipment shipment = Factory.New<PackUnpackShipment>();
			return shipment.DocsAndCartage;
		}

		public override void TestJobServices()
		{
			PackUnpackDocsAndCartage docsAndCartage = Factory.New<PackUnpackDocsAndCartage>();
			AssertNotNull("DocsAndCartage should contain a job services collection", docsAndCartage.Services);
		}

		public override void TestJobServicesGetsLoadedProperly()
		{
			PackUnpackShipment shipment = Factory.New<PackUnpackShipment>();
			JobService fumigation = shipment.DocsAndCartage.Services.AddNew();
			fumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			PackUnpackShipment loadedShipment = newFactory.Load<PackUnpackShipment>(shipment.PK);
			Assert("Shipment's job services did not load correctly", loadedShipment.DocsAndCartage.Services.Count > 0);
		}

		public override void TestJobServicesIsRegisteredEditableChild()
		{
			PackUnpackShipment shipment = Factory.New<PackUnpackShipment>();
			shipment.DocsAndCartage.SetReadOnlyIncludingChildren(true);
			AssertEquals("Job services should be read only (registered editable child of JobDocsAndCartage)", true, shipment.DocsAndCartage.Services.ReadOnly);
		}
	}
}
