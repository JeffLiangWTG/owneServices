using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class JobVoyageShipmentDataObjectWriterTest : UniversalShipmentDataObjectWriterTest
	{
		readonly ZDateTime JS_HouseBillIssueDate = ZDateTime.Today;
		protected override ITopLevelDataObjectWriter GetWriter(IDataWritingManager manager)
		{
			return new JobVoyageShipmentDataObjectWriter(manager);
		}

		protected override BusinessObject GetShipmentBusinessObject()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			carrier.MainAddress.OA_Address1 = "Address 1";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "9999999";
			vessel.RV_Name = "Vessel";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_AirSeaRoad = "SEA";
			voyage.JV_VoyageFlight = "12PW";
			voyage.JV_IsCargoOnly = true;
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_SendersMessageReference = "J000000001";
			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = new ZDateTime(2015, 12, 1);
			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "SGSIN";
			origin2.JA_E_DEP = new ZDateTime(2015, 12, 10);
			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "NZAKL";
			destination1.JB_E_ARV = new ZDateTime(2015, 12, 20);

			var sailing1 = voyage.Sailings[0];
			sailing1.JX_UniqueReference = "SA00000001";
			var sailing2 = voyage.Sailings[1];
			sailing2.JX_UniqueReference = "SA00000002";

			var billOfLading1 = Factory.NewWithValidTestData<CommonShipment>();
			billOfLading1.JS_UniqueConsignRef = "V00000001";
			billOfLading1.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			billOfLading1.JS_IsShipping = true;
			billOfLading1.JS_JX = sailing1.PK;
			var container1 = (CommonContainer)Factory.NewWithValidTestData(ObjectFactory.GetType<Integration.Agency.IBillOfLadingContainer>());
			container1.JC_JS_FCLBookingOnlyLink = billOfLading1.PK;
			container1.JC_ContainerNum = "CON00000001";
			var billOfLading2 = Factory.NewWithValidTestData<CommonShipment>();
			billOfLading2.JS_UniqueConsignRef = "V00000002";
			billOfLading2.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			billOfLading2.JS_IsShipping = true;
			billOfLading2.JS_JX = sailing1.PK;
			var container2 = (CommonContainer)Factory.NewWithValidTestData(ObjectFactory.GetType<Integration.Agency.IBillOfLadingContainer>());
			container2.JC_JS_FCLBookingOnlyLink = billOfLading2.PK;
			container2.JC_ContainerNum = "CON00000002";
			var billOfLading3 = Factory.NewWithValidTestData<CommonShipment>();
			billOfLading3.JS_UniqueConsignRef = "V00000003";
			billOfLading3.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			billOfLading3.JS_IsShipping = true;
			billOfLading3.JS_JX = sailing2.PK;
			var container3 = (CommonContainer)Factory.NewWithValidTestData(ObjectFactory.GetType<Integration.Agency.IBillOfLadingContainer>());
			container3.JC_JS_FCLBookingOnlyLink = billOfLading3.PK;
			container3.JC_ContainerNum = "CON00000003";

			billOfLading1.JS_HouseBillIssueDate = JS_HouseBillIssueDate;
			billOfLading2.JS_HouseBillIssueDate = JS_HouseBillIssueDate;
			billOfLading3.JS_HouseBillIssueDate = JS_HouseBillIssueDate;

			return voyage;
		}

		protected override string GetExpectedDataObjectXml()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				return resourceRetriever.GetString("Enterprise.Freight.DataTransfer.Test.Freight.Universal.JobVoyage.TestFiles.JobVoyage_UniversalShipment.xml")
										.Replace("houseBillIssueDatePlaceholder", JS_HouseBillIssueDate.ToString("yyyy-MM-ddTHH:mm:ss"));
			}
		}
	}
}
