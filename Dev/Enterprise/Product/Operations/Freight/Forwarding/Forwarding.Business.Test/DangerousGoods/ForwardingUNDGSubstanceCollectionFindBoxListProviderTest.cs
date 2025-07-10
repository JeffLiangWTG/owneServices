using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingUNDGSubstanceCollectionFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestNearestMatchCore()
		{
			var iatSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			iatSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			iatSubstance.DG_Code = "ABC1";

			var imoSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			imoSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			imoSubstance.DG_Code = "ABC2";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var collection = new ForwardingUNDGSubstanceCollection(Factory, shipment);
			var provider = new ForwardingUNDGSubstanceCollectionFindBoxListProvider(collection, shipment);

			AssertEquals(("ABC2", true), provider.NearestMatchCore("ABC", true));

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(("ABC1", true), provider.NearestMatchCore("ABC", true));
		}

		public void TestBizObjsFromCodeWithCompleteFilter()
		{
			var iatSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			iatSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			iatSubstance.DG_Code = "ABC1";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var collection = new ForwardingUNDGSubstanceCollection(Factory, shipment);
			var provider = new ForwardingUNDGSubstanceCollectionFindBoxListProvider(collection, shipment);

			AssertContainsExactElementsInAnyOrder(new[] { iatSubstance }, provider.GetBusinessObjectsFromCode("ABC1").OfType<UNDGSubstance>().ToArray());

			var imoSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			imoSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			imoSubstance.DG_Code = "ABC1";

			AssertContainsExactElementsInAnyOrder(new[] { imoSubstance }, provider.GetBusinessObjectsFromCode("ABC1").OfType<UNDGSubstance>().ToArray());
		}
	}
}
