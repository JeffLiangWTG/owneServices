using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(VoyagePortCollection))]
	class VoyagePortCollectionTest : NonPersistentBusinessObjectCollectionTestCase<VoyagePortCollection>
	{
		public void TestChangesInVoyageReflectChangesInVoyagePortCollection()
		{
			var voyage = Factory.New<JobVoyage>();
			var ports = new VoyagePortCollection(voyage);
			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			AssertEquals(1, ports.Count);
			AssertEquals("AUSYD", ports[0].Port);
			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "USLAX";
			AssertEquals(2, ports.Count);
			AssertNotNull(ports.Cast<VoyagePort>().FirstOrDefault(x => x.Port == "USLAX"));
			AssertNotNull(ports.Cast<VoyagePort>().FirstOrDefault(x => x.Port == "AUSYD"));
			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUMEL";
			AssertEquals(3, ports.Count);
			AssertNotNull(ports.Cast<VoyagePort>().FirstOrDefault(x => x.Port == "AUMEL"));
			origin2.JA_RL_NKPortOfLoading = "USLAX";
			AssertEquals(2, ports.Count);
			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "CAAAB";
			AssertEquals(3, ports.Count);
			voyage.Destinations.RemoveAndDelete(destination2);
			AssertEquals(2, ports.Count);
		}

		protected override VoyagePortCollection GetCollectionToTest()
		{
			return new VoyagePortCollection(Voyage);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			return new VoyagePort(Voyage, ZString.Empty);
		}

		JobVoyage Voyage
		{
			get
			{
				return fVoyage ?? (fVoyage = Factory.New<JobVoyage>());
			}
		}

		JobVoyage fVoyage;
	}
}
