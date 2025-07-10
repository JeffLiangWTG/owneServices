using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ScheduleDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWriteToDataObject_SEA()
		{
			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001");
			voyage.JV_OH_Line = GetCarrier("MAERSK AUSTRALIA", "MAELIN").PK;
			AddPorts(voyage);
			AssertWriteToDataObject(voyage, "JobVoyage_UniversalSchedule_SEA.xml");
		}

		public void TestWriteToDataObject_AIR()
		{
			var voyage = UniversalTestHelper.CreateAirVoyage(Factory, "QF001", false);
			voyage.JV_OH_Line = GetCarrier("QANTAS AIR", "QAIR").PK;
			AddPorts(voyage);

			AssertWriteToDataObject(voyage, "JobVoyage_UniversalSchedule_AIR.xml");
		}

		public void TestWriteToDataObject_RAI()
		{
			var voyage = UniversalTestHelper.CreateRailVoyage(Factory, "ZZZZ", "A-10");
			voyage.JV_OH_Line = GetCarrier("RAILCORP", "RCRP").PK;
			AddPorts(voyage);

			AssertWriteToDataObject(voyage, "JobVoyage_UniversalSchedule_RAI.xml");
		}

		public void TestWriteToDataObject_ROA()
		{
			var voyage = UniversalTestHelper.CreateRoadVoyage(Factory, "ZIL 130");
			voyage.JV_OH_Line = GetCarrier("LINFORX TRUCKING PTY LTD", "LINFOX").PK;
			AddPorts(voyage);

			AssertWriteToDataObject(voyage, "JobVoyage_UniversalSchedule_ROA.xml");
		}

		void AssertWriteToDataObject(JobVoyage voyage, string embeddedResourceFileName)
		{
			var writer = new ScheduleDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, voyage)));
			var dataObject = writer.GetDataObject(voyage);

			var actualXml = UniversalTestHelper.GetXml(dataObject).Trim();
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var expectedXml = resourceRetriever.GetString("Enterprise.Freight.DataTransfer.Test.Freight.Universal.JobVoyage.TestFiles." + embeddedResourceFileName);
				AssertMultilineASCIIEquals("UniversalSchedule", expectedXml, actualXml);
			}
		}

		OrgHeader GetCarrier(string name, string code)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = name;
			carrier.OH_Code = code;
			carrier.MainAddress.OA_Address1 = "1 CARGO LN";
			carrier.MainAddress.OA_PostCode = "2000";
			carrier.MainAddress.OA_State = "NSW";
			carrier.OH_RL_NKClosestPort = "AUSYD";

			return carrier;
		}

		void AddPorts(JobVoyage voyage)
		{
			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";

			var depCTO = voyage.Factory.New<OrgHeader>();
			depCTO.OH_Code = "DEPCTO";
			depCTO.MainAddress.OA_Address1 = "1 Departure St";

			origin1.JA_OA_DepartureCTOAddress = depCTO.MainAddress.PK;

			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "NZAKL";

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "NZAKL";

			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "SGSIN";

			var arvCTO = voyage.Factory.New<OrgHeader>();
			arvCTO.OH_Code = "ARVCTO";
			arvCTO.MainAddress.OA_Address1 = "1 Arrival Pde";

			destination2.JB_OA_ArrivalCTOAddress = arvCTO.MainAddress.PK;
		}
	}
}
