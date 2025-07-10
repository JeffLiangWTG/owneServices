using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(N5101HBorderTransportMeans))]
	sealed class N5101HBorderTransportMeansTest : TestCaseWithFactory
	{
		public void TestTypeCode()
		{
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Type Code for transport mode : SEA", Constants.TransportModeTypeCodes.Sea, transportMeans.TypeCode);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Type Code for transport mode : AIR", Constants.TransportModeTypeCodes.Air, transportMeans.TypeCode);

			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("Type Code for transport mode : ROA", ZString.Empty, transportMeans.TypeCode);
		}

		public void TestID()
		{
			AssertEquals("Transport Means ID", "LLL", transportMeans.ID);
		}

		public void TestJournetID()
		{
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Transport Means Journey ID", "VVV", transportMeans.JourneyID);

			header.AMA_Voyage = "5X 0061";
			AssertEquals("Transport Means Journey ID", "5X 0061", transportMeans.JourneyID);

			header.AMA_Voyage = "5X0061";
			AssertEquals("Transport Means Journey ID", "5X 0061", transportMeans.JourneyID);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Transport Means Journey ID", "5X0061", transportMeans.JourneyID);
		}

		public void TestRegistration()
		{
			AssertEquals("Transport Means Registration", "RRR", transportMeans.Registration);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_LloydsNumber = "LLL";
			header.AMA_Voyage = "VVV";
			header.AMA_VehicleRegistration = "RRR";
			transportMeans = new N5101HBorderTransportMeans(header);
		}

		ITransportMeans transportMeans;
		AsycudaManifestHeader header;
	}
}
