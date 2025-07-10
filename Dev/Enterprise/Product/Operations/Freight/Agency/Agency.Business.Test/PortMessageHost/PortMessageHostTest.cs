using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class PortMessageHostTest : BaseAgencyTest
	{
		#region TestOriginHost
		public void TestOriginHost()
		{
			ZDateTime now = ZDateTime.Now;
			Origin.JA_RL_NKPortOfLoading = HomePort;
			Origin.JA_E_DEP = now;
			AssertEquals(HomePort, OriginHost.Port);
			AssertEquals(now, originHost.EstDate);
			AssertEquals(Constants.PortDirection.Load, OriginHost.Direction);
			Origin.JA_RL_NKPortOfLoading = OverseasPort;
			Origin.JA_E_DEP = now.AddDays(1);
			AssertEquals(OverseasPort, OriginHost.Port);
			AssertEquals(now.AddDays(1), OriginHost.EstDate);
		}

		#endregion
		#region TestDestinationHost
		public void TestDestinationHost()
		{
			ZDateTime now = ZDateTime.Now;
			Destination.JB_RL_NKPortOfDischarge = HomePort;
			Destination.JB_E_ARV = now;
			AssertEquals(HomePort, DestinationHost.Port);
			AssertEquals(now, DestinationHost.EstDate);
			AssertEquals(Constants.PortDirection.Discharge, DestinationHost.Direction);
			Destination.JB_RL_NKPortOfDischarge = OverseasPort;
			Destination.JB_E_ARV = now.AddDays(1);
			AssertEquals(OverseasPort, DestinationHost.Port);
			AssertEquals(now.AddDays(1), DestinationHost.EstDate);
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
		#region Origin
		VoyageOrigin Origin
		{
			get
			{
				return origin ?? (origin = Voyage.Origins.AddNew());
			}
		}

		VoyageOrigin origin;
		#endregion
		#region OriginHost
		PortMessageHost OriginHost
		{
			get
			{
				return originHost ?? (originHost = new PortMessageHost(Origin));
			}
		}

		PortMessageHost originHost;
		#endregion
		#region Destination
		VoyageDestination Destination
		{
			get
			{
				return destination ?? (destination = Factory.New<VoyageDestination>());
			}
		}

		VoyageDestination destination;
		#endregion
		#region DestinationHost
		PortMessageHost DestinationHost
		{
			get
			{
				return destinationHost ?? (destinationHost = new PortMessageHost(Destination));
			}
		}

		PortMessageHost destinationHost;
		#endregion
		#endregion
	}
}
