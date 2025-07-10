using Enterprise.Core;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class PortMessageHostCollectionTest : BaseAgencyTest
	{
		#region TestRetardedBindingCallsAddNew
		public void TestRetardedBindingCallsAddNew()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			PortMessageHostCollection hostCollection = new PortMessageHostCollection(voyage);
			AssertNoExceptionThrown(delegate
			{
				hostCollection.AddNew();
			});
		}

		#endregion
		#region TestTrackOrigins
		public void TestTrackOrigins()
		{
			VoyageOrigin origin1 = Voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = HomePort;
			AssertEquals(1, Collection.Count);
			AssertEquals(HomePort, Collection[0].Port);
			AssertEquals(Constants.PortDirection.Load, Collection[0].Direction);
			VoyageOrigin origin2 = Voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = AlternateHomePort;
			AssertEquals(2, Collection.Count);
			AssertEquals(AlternateHomePort, Collection[1].Port);
			AssertEquals(Constants.PortDirection.Load, Collection[1].Direction);
			origin1.Delete();
			AssertEquals(1, Collection.Count);
			AssertEquals(AlternateHomePort, Collection[0].Port);
			AssertEquals(Constants.PortDirection.Load, Collection[0].Direction);
			origin2.JA_RL_NKPortOfLoading = OverseasPort;
			AssertEquals(1, Collection.Count);
			AssertEquals(OverseasPort, Collection[0].Port);
			AssertEquals(Constants.PortDirection.Load, Collection[0].Direction);
		}

		#endregion
		#region TestAddDestination
		public void TestAddDestination()
		{
			VoyageDestination destination1 = Voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = HomePort;
			AssertEquals(1, Collection.Count);
			AssertEquals(HomePort, Collection[0].Port);
			AssertEquals(Constants.PortDirection.Discharge, Collection[0].Direction);
			VoyageDestination destination2 = Voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = AlternateHomePort;
			AssertEquals(2, Collection.Count);
			AssertEquals(AlternateHomePort, Collection[1].Port);
			AssertEquals(Constants.PortDirection.Discharge, Collection[1].Direction);
			destination1.Delete();
			AssertEquals(1, Collection.Count);
			AssertEquals(AlternateHomePort, Collection[0].Port);
			AssertEquals(Constants.PortDirection.Discharge, Collection[0].Direction);
			destination2.JB_RL_NKPortOfDischarge = OverseasPort;
			AssertEquals(1, Collection.Count);
			AssertEquals(OverseasPort, Collection[0].Port);
			AssertEquals(Constants.PortDirection.Discharge, Collection[0].Direction);
		}

		#endregion
		#region Implementation
		#region Voyage
		JobVoyage Voyage
		{
			get
			{
				return voyage ?? (voyage = Factory.New<JobVoyage>());
			}
		}

		JobVoyage voyage;
		#endregion
		#region Collection
		PortMessageHostCollection Collection
		{
			get
			{
				return collection ?? (collection = new PortMessageHostCollection(Voyage));
			}
		}

		PortMessageHostCollection collection;
		#endregion
		#endregion
	}
}
