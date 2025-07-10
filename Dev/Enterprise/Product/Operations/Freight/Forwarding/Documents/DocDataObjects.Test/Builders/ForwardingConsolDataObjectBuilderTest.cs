using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.Builders.Testing
{
	sealed class ForwardingConsolDataObjectBuilderTest : TestCaseWithFactory
	{
		public void TestBuilderPopulatesDataObjectCorrectly()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = CreateSydneyOrgAddress("ye olde addresse", org1.PK);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress2 = CreateSydneyOrgAddress("ye seconde olde addresse", org2.PK);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "D0VAKH11N";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_PackDepotAddress = orgAddress1.PK;
			consol.JK_OA_UnpackDepotAddress = orgAddress2.PK;

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "CNSHA";
			transport2.JW_Vessel = "Black Hole";
			transport2.JW_VoyageFlight = "222";

			Factory.Save();

			var builder = new ForwardingConsolDataObjectBuilder();
			var dataObject = builder.Build(consol);

			CombineAssertions(() =>
			{
				AssertEquals("Identifier", consol.PK, dataObject.Identifier);
				AssertEquals("ConsolNumber", consol.JK_UniqueConsignRef, dataObject.ConsolNumber);
				AssertEquals("TransportMode.Code", consol.JK_TransportMode, dataObject.TransportMode.Code);
				AssertEquals("LoadPort.Code", consol.JK_RL_NKLoadPort, dataObject.LoadPort.Code);
				AssertEquals("DischargePort.Code", consol.JK_RL_NKDischargePort, dataObject.DischargePort.Code);
				AssertEquals("DepartureCFS.AddressLine1", consol.GetDepartureCFSDocAddress.Address1, dataObject.DepartureCFS.AddressLine1);
				AssertEquals("dataObject.ArrivalCFS.AddressLine1", consol.GetArrivalCFSDocAddress.Address1, dataObject.ArrivalCFS.AddressLine1);

				AssertEquals("PortOfFirstLoading.Code", "AUSYD", dataObject.PortOfFirstLoading.Code);
				AssertEquals("PortOfLastDischarge.Code", "CNSHA", dataObject.PortOfLastDischarge.Code);
				AssertEquals("PlaceOfReceipt.Code", "AUSYD", dataObject.PlaceOfReceipt.Code);
				AssertEquals("PlaceOfDelivery.Code", "CNSHA", dataObject.PlaceOfDelivery.Code);
				AssertEquals("FirstVoyageFlight", "111", dataObject.FirstVoyageFlightNumber);

				AssertEquals("Transports.Count", 2, dataObject.Transports.Count);
				AssertEquals("Transports[0].PortOfLoading", "AUSYD", dataObject.Transports.ToList()[0].PortOfLoading.Code);
				AssertEquals("Transports[0].PortOfDischarge", "SGSIN", dataObject.Transports.ToList()[0].PortOfDischarge.Code);
				AssertEquals("Transports[1].PortOfLoading", "SGSIN", dataObject.Transports.ToList()[1].PortOfLoading.Code);
				AssertEquals("Transports[1].PortOfDischarge", "CNSHA", dataObject.Transports.ToList()[1].PortOfDischarge.Code);
			});
		}

		#region Implementation

		OrgAddress CreateSydneyOrgAddress(string address, ZGuid orgPK)
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Address1 = address;
			orgAddress.OA_City = "Sydney";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_PostCode = "2154";
			orgAddress.OA_RN_NKCountryCode = "AU";
			orgAddress.OA_Phone = "696942069";
			orgAddress.OA_Email = "bob@thebuilder.com";
			orgAddress.OA_Language = "EN";
			orgAddress.OA_IsActive = true;
			orgAddress.OA_OH = orgPK;

			Factory.Save();

			return orgAddress;
		}

		#endregion
	}
}
