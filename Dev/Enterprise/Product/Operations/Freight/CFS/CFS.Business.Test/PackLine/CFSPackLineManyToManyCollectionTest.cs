using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSPackLineManyToManyCollectionTest : BaseFreightTest
	{
		public void TestOnAddedAndOnRemoving()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			CFSService containerFumigation = container.Services.AddNew();
			CFSShipment shipment = Factory.New<CFSShipment>();
			containerFumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			CFSPackLine line = Factory.New<CFSPackLine>();
			line.JL_JS = shipment.PK;
			container.PackLines.Add(line);

			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_OuterPacksInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_F3_NKPackTypeInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_ActualWeightInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_UnitOfWeightInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_ActualVolumeInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_UnitOfVolumeInfo.ReadOnly);

			container.PackLines.Remove(line);

			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_OuterPacksInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_F3_NKPackTypeInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_ActualWeightInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_UnitOfWeightInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_ActualVolumeInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_UnitOfVolumeInfo.ReadOnly);
		}
	}
}
