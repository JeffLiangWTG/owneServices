using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF.Testing
{
	sealed class CALINFMessageDataTest : TestCaseWithFactory
	{
		[TestDate(2020, 05, 28, 14, 35, 0)]
		public void TestDataMapping_Sea()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			customsCode.OK_CodeType = "CCC";
			customsCode.OK_CustomsRegNo = "MSC";
			var vessel = Factory.New<RefVessel>();
			vessel.RV_CarrierCode = "MSC";
			vessel.RV_RadioCallSign = "RCSBS";
			vessel.RV_Code = "BlueSeas";
			vessel.RV_RN_NKCountryOfReg = "GB";
			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			jobVoyage.JV_RV_NKVessel = vessel.RV_FK;
			jobVoyage.JV_VoyageFlight = "VOY-999";
			jobVoyage.JV_OH_Line = orgHeader.PK;
			var dataProvider = (ICALINFMessageDataProvider)new CALINFMessageData(jobVoyage);
			AssertEquals("SCH", dataProvider.CALINFMessageType);
			AssertEquals(new ZDateTime(2020, 05, 28, 14, 35, 0), dataProvider.DocumentIssueDateTime);
			AssertEquals("MSC", dataProvider.MessageSender);
			AssertEquals(jobVoyage.Messages, dataProvider.Messages);
			AssertEquals(jobVoyage.Factory, dataProvider.Factory);
			AssertEquals(jobVoyage, dataProvider.TopLevelBusinessObject);
			var transport = dataProvider.Transport;
			var transportSea = transport as CALINFTransportSea;
			var transportAir = transport as CALINFTransportAir;
			AssertNotNull(transportSea);
			AssertNull(transportAir);
			AssertEquals("VOY-999", transport.ConveyanceNumber);
			AssertEquals("VOY-999", transport.PrincipalCarrierConveyanceNumber);
			AssertEquals("MSC", transport.CarrierCode);
			AssertEquals("RCSBS", transport.MeansOfTransportId);
			AssertEquals("BlueSeas", transport.MeansOfTransportName);
			AssertEquals("GB", transport.MeansOfTransportNationality);
		}

		[TestDate(2020, 05, 28, 14, 35, 0)]
		public void TestDataMapping_Air()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "125";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MiscServ.OM_RM_Airline = airline.PK;
			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			customsCode.OK_CodeType = "CCC";
			customsCode.OK_CustomsRegNo = "BAX";
			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			jobVoyage.JV_OH_Line = orgHeader.PK;
			jobVoyage.JV_VoyageFlight = "BA7755";
			var dataProvider = (ICALINFMessageDataProvider)new CALINFMessageData(jobVoyage);
			AssertEquals("ASC", dataProvider.CALINFMessageType);
			AssertEquals(new ZDateTime(2020, 05, 28, 14, 35, 0), dataProvider.DocumentIssueDateTime);
			AssertEquals("BAX", dataProvider.MessageSender);
			AssertEquals(jobVoyage.Messages, dataProvider.Messages);
			AssertEquals(jobVoyage.Factory, dataProvider.Factory);
			AssertEquals(jobVoyage, dataProvider.TopLevelBusinessObject);
			var transport = dataProvider.Transport;
			var transportSea = transport as CALINFTransportSea;
			var transportAir = transport as CALINFTransportAir;
			AssertNull(transportSea);
			AssertNotNull(transportAir);
			AssertEquals("BA7755", transport.ConveyanceNumber);
			AssertEquals("BA7755", transport.PrincipalCarrierConveyanceNumber);
			AssertEquals("125", transport.CarrierCode);
		}

		public void TestDocumentToBeAmended()
		{
			CALINFMessasgeBuilderTest.SetupZZRefDB(Factory);
			#region Prepare 6 EDI Messages:
			var calinfTestMessage01 = @"UNH+316+CALINF:D:16A:UN:RCG001'
BGM+96:::SCH+B9C73560F3A54797909A498BE37010B5+9'
DTM+137:201603310615:203'
NAD+MS+125::ZZZ'
TDT+20++++:172:20+++:103'
RFF+ACL:DDD4455667'
LOC+5+GBLON:139:6'
DTM+136:202009010600:203'
UNT+9+1'";
			var cusresTestMessage01 = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+B9C73560F3A54797909A498BE37010B5:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+6:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+ACD:123'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";
			var calinfTestMessage02 = calinfTestMessage01.Replace("B9C73560F3A54797909A498BE37010B5", "B9C73560F3A54797909A498BE37010B6");
			var cusresTestMessage02 = cusresTestMessage01.Replace("B9C73560F3A54797909A498BE37010B5", "B9C73560F3A54797909A498BE37010B6").Replace("GIS+6:120:ZZZ", "GIS+8:120:ZZZ");
			var calinfTestMessage03 = calinfTestMessage01.Replace("B9C73560F3A54797909A498BE37010B5", "B9C73560F3A54797909A498BE37010B7");
			var cusresTestMessage03 = cusresTestMessage01.Replace("B9C73560F3A54797909A498BE37010B5", "B9C73560F3A54797909A498BE37010B7");
			#endregion Prepare 6 EDI Messages.
			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			AddEdiMessagesToJobVoyage(jobVoyage, "123", calinfTestMessage01, cusresTestMessage01);
			AddEdiMessagesToJobVoyage(jobVoyage, "124", calinfTestMessage02, cusresTestMessage02);
			AddEdiMessagesToJobVoyage(jobVoyage, "125", calinfTestMessage03, cusresTestMessage03);
			var messageData = new CALINFMessageData(jobVoyage);
			var rffIFace = messageData as IRFF_DocumentToBeAmended;
			var docToAmend = rffIFace.DocumentToBeAmended;
			AssertEquals("Document to be Amended", "B9C73560F3A54797909A498BE37010B6", docToAmend);
		}

		void AddEdiMessagesToJobVoyage(JobVoyage jobVoyage, string calinfMessageNum, string calinfMessageText, string cusresMessageText)
		{
			var calinf = Factory.New<CALINFEDIMessage>();
			calinf.EM_MessageNum = calinfMessageNum;
			calinf.EM_MessageText = calinfMessageText.Replace("\r\n", "");
			calinf.EM_LinkUniqueID = jobVoyage.PK;
			calinf.EM_LinkTable = JobVoyage.Schema.TableName;
			var cusres = Factory.New<CUSRESEDIMessage>();
			cusres.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cusres.EM_MessageText = cusresMessageText.Replace("\r\n", "");
			cusres.EM_LinkUniqueID = jobVoyage.PK;
			cusres.EM_LinkTable = JobVoyage.Schema.TableName;
			jobVoyage.Messages.AddRange(calinf, cusres);
		}
	}
}
