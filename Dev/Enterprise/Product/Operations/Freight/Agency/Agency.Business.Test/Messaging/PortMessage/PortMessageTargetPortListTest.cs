using System;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal sealed class PortMessageTargetPortListTest : BaseAgencyTest
	{
		public void TestConstructor_VoyageIsNull_ThrowArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PortMessageTargetPortList(null));
		}

		public void TestLoad()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";

			var list = new PortMessageTargetPortList(voyage);
			list.Load();

			AssertEquals("AUSYD, NZAKL, UAIEV", list.CodesAsString);
			AssertEquals("Is AUSYD origin port", false, list["AUSYD"].Directions.HasFlag(PortDirections.Load));
			AssertEquals("Is NZAKL origin port", true, list["NZAKL"].Directions.HasFlag(PortDirections.Load));
			AssertEquals("Is UAIEV origin port", true, list["UAIEV"].Directions.HasFlag(PortDirections.Load));
			AssertEquals("Is AUSYD destination port", true, list["AUSYD"].Directions.HasFlag(PortDirections.Discharge));
			AssertEquals("Is NZAKL destination port", true, list["NZAKL"].Directions.HasFlag(PortDirections.Discharge));
			AssertEquals("Is UAIEV destination port", false, list["UAIEV"].Directions.HasFlag(PortDirections.Discharge));
		}

		public void TestCodeIndexer()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "USLAX";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";

			var list = new PortMessageTargetPortList(voyage);
			list.Load();

			AssertEquals("NZAKL", list["NZAKL"].Code);
			AssertEquals("USLAX", list["USLAX"].Code);
			AssertEquals("AUMEL", list["AUMEL"].Code);
		}
	}
}
