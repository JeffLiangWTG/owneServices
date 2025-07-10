using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Freight.Integration.CFS;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(VoyageRelatedJobCollection))]
	sealed class VoyageRelatedJobCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();

			origin.FillWithValidTestData();
			origin.JA_RL_NKPortOfLoading = "AUBNE";

			destination.FillWithValidTestData();
			destination.JB_RL_NKPortOfDischarge = "DEHAM";

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			var shipment = (CommonShipment)Factory.New<ICFSShipment>();
			shipment.FillWithValidTestData();
			shipment.JS_JX = sailing.PK;
			shipment.JS_UniqueConsignRef = "VRJC0001";

			Factory.Save();

			var collection = new VoyageRelatedJobCollection(voyage);
			return collection;
		}

		#endregion
	}
}
