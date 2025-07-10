using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class VoyagePortsCollectionTest : TestCaseWithFactory
	{
		public void TestRemoveAndDelete()
		{
			var voyage = Factory.New<JobVoyage>();

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";

			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "USLAX";

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUMEL";

			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "NZAKL";

			voyage.GenerateSailings();
			AssertEquals("Precondition", 4, voyage.Sailings.Count);

			var referencedSailing = voyage.Sailings.Cast<JobSailing>()
				.First(sailing => sailing.JX_JA == origin1.PK && sailing.JX_JB == destination1.PK);

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_JX = referencedSailing.PK;
			AssertEquals("Precondition", true, referencedSailing.IsReferenced());

			var nonReferencedSailing = voyage.Sailings.Cast<JobSailing>()
				.First(sailing => sailing.JX_JA == origin2.PK && sailing.JX_JB == destination2.PK);
			AssertEquals("Precondition", false, nonReferencedSailing.IsReferenced());

			var collection = GetCollection(voyage);
			var port = GetPortFromSailing(referencedSailing);

			collection.RemoveAndDelete(port);
			AssertEquals("Not deleted", 4, voyage.Sailings.Count);
			AssertEquals("Not deleted", false, port.IsDeleted);

			bool cannotDeletePortHandlerCalled = false;
			((IVoyagePortsCollection)collection).CannotDeletePort += (s, e) => { cannotDeletePortHandlerCalled = true; };

			collection.RemoveAndDelete(port);
			AssertEquals("Handler was called", true, cannotDeletePortHandlerCalled);
			AssertEquals("Not deleted", 4, voyage.Sailings.Count);
			AssertEquals("Not deleted", false, port.IsDeleted);

			port = GetPortFromSailing(nonReferencedSailing);
			collection.RemoveAndDelete(port);
			AssertEquals("Related sailings has been deleted", 2, voyage.Sailings.Count);
			AssertEquals("Related sailings has been deleted", false, voyage.Sailings.Cast<JobSailing>().Any(sailing => GetPortFromSailing(sailing) == port));
			AssertEquals("Port has been deleted", true, port.IsDeleted);
		}

		public void TestSuppressSailingGeneration()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			voyage.GenerateSailings();
			AssertEquals("Precondition", 1, voyage.Sailings.Count);

			var collection = (IVoyagePortsCollection)GetCollection(voyage);
			using (collection.SuppressSailingGeneration())
			{
				collection.AddNew();
			}

			AssertEquals("Sailing generation was suppressed", 1, voyage.Sailings.Count);
			voyage.GenerateSailings();
			AssertEquals(2, voyage.Sailings.Count);

			collection.AddNew();
			AssertEquals(3, voyage.Sailings.Count);
		}

		#region Implementation

		protected abstract bool IsOriginPortsCollection { get; }

		BusinessObjectCollection GetCollection(JobVoyage voyage)
		{
			return IsOriginPortsCollection
				? voyage.Origins
				: voyage.Destinations;
		}

		BusinessObject GetPortFromSailing(JobSailing sailing)
		{
			return IsOriginPortsCollection
				? sailing.Origin
				: sailing.Destination;
		}

		#endregion
	}
}
