using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class MSUMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2009, 12, 9, 14, 13, 15)]
		public void TestIncorrectMessage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			MSUMessageBuilder builder = new MSUMessageBuilder();
			builder.Shipment = shipment;
			NotificationBuffer buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			ZString message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,,,,,,,K
STS,,,L
", message);
			AssertEquals(@"Error: Required field empty ('Forwarder Identification' registry item)
Error: Required field empty (Registered Date)
Error: Required field empty (House Bill Number)
Error: Required field empty (Packs)
Error: Required field empty (Milestone Status Code)
Error: Required field empty (Milestone Actual Date)
", buffer.AsString);

			buffer.Clear();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			shipment.JS_HouseBill = "12345678901234567,:;";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCD");
			shipment.JS_OuterPacks = 10;
			builder.MilestoneStatusCode = "UUU";
			builder.MilestoneStatusDate = ZDateTime.Now;
			Factory.Save();

			message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,,K
STS,,,L,09DEC09 1413
", message);
			AssertEquals(@"Error: 'Forwarder Identification' registry item: value can't be formatted, Input data must consist of 3 'Upper case alphabetic characters'
Error: House Bill Number: value can't be formatted, Input data must consist of 'Text character - only comprising upper case alphabetic, numeric, hyphen, full stop, forward slash and space' with length from 0 to 20
Error: Required field empty (Milestone Status Code)
", buffer.AsString);

			buffer.Clear();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			builder.MilestoneStatusCode = CargoIMPPhase2MSUEventCodeList.Codes.DEW;
			shipment.JS_HouseBill = "12345678901234567";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");

			message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,,K
STS,DEW,,L,09DEC09 1413
", message);
			AssertEquals(@"Error: Required field empty (Weight)
Error: Required field empty (Origin)
", buffer.AsString);
		}

		[TestDate(2009, 12, 9, 14, 13, 15)]
		public void TestREW()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "12345678901234567";
			shipment.JS_OuterPacks = 10;
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			Factory.Save();

			MSUMessageBuilder builder = new MSUMessageBuilder();
			builder.Shipment = shipment;
			NotificationBuffer buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			builder.MilestoneStatusCode = CargoIMPPhase2MSUEventCodeList.Codes.REW;
			builder.MilestoneStatusDate = new ZDateTime(2009, 12, 14, 18, 6, 0);

			ZString message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,,K
STS,REW,,L,14DEC09 1806
", message);
			AssertEquals(@"Error: Required field empty (Weight)
Error: Required field empty (Origin)
", buffer.AsString);

			buffer.Clear();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);

			shipment.JS_ActualWeight = 1200.467M;
			shipment.JS_ActualVolume = 1.234M;

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "COD1";
			org1.OH_RL_NKClosestPort = "AUALX";
			shipment.JS_OA_ExportReceivingDepot = org1.MainAddress.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "COD2";
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = org2.MainAddress.PK;

			message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,1200.5,K,1.234,MC
STS,REW,AUALX,L,14DEC09 1806,,COD1,COD2
", message);
			AssertEquals(@"", buffer.AsString);
		}

		[TestDate(2009, 12, 9, 14, 13, 15)]
		public void TestDEW()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "12345678901234567";
			shipment.JS_OuterPacks = 10;
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			Factory.Save();

			MSUMessageBuilder builder = new MSUMessageBuilder();
			builder.Shipment = shipment;
			NotificationBuffer buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			builder.MilestoneStatusCode = CargoIMPPhase2MSUEventCodeList.Codes.DEW;
			builder.MilestoneStatusDate = new ZDateTime(2009, 12, 14, 18, 6, 0);

			ZString message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,,K
STS,DEW,,L,14DEC09 1806
", message);
			AssertEquals(@"Error: Required field empty (Weight)
Error: Required field empty (Origin)
", buffer.AsString);

			buffer.Clear();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);

			shipment.JS_ActualWeight = 1200.467M;
			shipment.JS_ActualVolume = 1.234M;

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "COD1";
			org1.OH_RL_NKClosestPort = "AUALX";
			shipment.JS_OA_ExportReceivingDepot = org1.MainAddress.PK;

			message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,1200.5,K,1.234,MC
STS,DEW,AUALX,L,14DEC09 1806,,COD1
", message);
			AssertEquals(@"", buffer.AsString);
		}

		[TestDate(2009, 12, 9, 14, 13, 15)]
		public void TestDOC()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "12345678901234567";
			shipment.JS_OuterPacks = 10;
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			Factory.Save();

			MSUMessageBuilder builder = new MSUMessageBuilder();
			builder.Shipment = shipment;
			NotificationBuffer buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			builder.MilestoneStatusCode = CargoIMPPhase2MSUEventCodeList.Codes.DOC;
			builder.MilestoneStatusDate = new ZDateTime(2009, 12, 14, 18, 6, 0);

			ZString message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,,K
STS,DOC,,L,14DEC09 1806
", message);
			AssertEquals(@"Error: Required field empty (Exporting Carrier's Terminal Customer (Departure Consol CTO Address or MAWB Origin Code))
", buffer.AsString);

			buffer.Clear();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);

			shipment.JS_ActualWeight = 1200.467M;
			shipment.JS_ActualVolume = 1.234M;

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "COD1";
			org1.OH_RL_NKClosestPort = "AUALX";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_DepartureCTOAddress = org1.MainAddress.PK;
			shipment.Consols.Add(consol);

			message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,1200.5,K,1.234,MC
STS,DOC,AUALX,L,14DEC09 1806,,COD1
", message);
			AssertEquals("", buffer.AsString);
		}

		[TestDate(2009, 12, 9, 14, 13, 15)]
		public void TestRIW()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "12345678901234567";
			shipment.JS_OuterPacks = 10;
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			Factory.Save();

			MSUMessageBuilder builder = new MSUMessageBuilder();
			builder.Shipment = shipment;
			NotificationBuffer buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			builder.MilestoneStatusCode = CargoIMPPhase2MSUEventCodeList.Codes.RIW;
			builder.MilestoneStatusDate = new ZDateTime(2009, 12, 14, 18, 6, 0);

			ZString message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,,K
STS,RIW,,L,14DEC09 1806
", message);
			AssertEquals(@"Error: Required field empty (Destination)
", buffer.AsString);

			buffer.Clear();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);

			shipment.JS_ActualWeight = 1200.467M;
			shipment.JS_ActualVolume = 1.234M;

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "COD1";
			org1.OH_RL_NKClosestPort = "AUALX";
			shipment.JS_OA_ImportReleaseDepot = org1.MainAddress.PK;

			message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,1200.5,K,1.234,MC
STS,RIW,AUALX,L,14DEC09 1806,,COD1
", message);
			AssertEquals("", buffer.AsString);
		}

		[TestDate(2009, 12, 9, 14, 13, 15)]
		public void TestOFD()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "12345678901234567";
			shipment.JS_OuterPacks = 10;
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			Factory.Save();

			MSUMessageBuilder builder = new MSUMessageBuilder();
			builder.Shipment = shipment;
			NotificationBuffer buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			builder.MilestoneStatusCode = CargoIMPPhase2MSUEventCodeList.Codes.OFD;
			builder.MilestoneStatusDate = new ZDateTime(2009, 12, 14, 18, 6, 0);

			ZString message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,,K
STS,OFD,,L,14DEC09 1806
", message);
			AssertEquals(@"Error: Required field empty (Destination)
", buffer.AsString);

			buffer.Clear();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);

			shipment.JS_ActualWeight = 1200.467M;
			shipment.JS_ActualVolume = 1.234M;

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "COD1";
			org1.OH_RL_NKClosestPort = "AUALX";
			shipment.JS_OA_ImportReleaseDepot = org1.MainAddress.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "COD2";
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = org2.MainAddress.PK;

			message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,1200.5,K,1.234,MC
STS,OFD,AUALX,L,14DEC09 1806,,COD1,COD2
", message);
			AssertEquals("", buffer.AsString);
		}

		[TestDate(2009, 12, 9, 14, 13, 15)]
		public void TestPOD()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "12345678901234567";
			shipment.JS_OuterPacks = 10;
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			Factory.Save();

			MSUMessageBuilder builder = new MSUMessageBuilder();
			builder.Shipment = shipment;
			NotificationBuffer buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			builder.MilestoneStatusCode = CargoIMPPhase2MSUEventCodeList.Codes.POD;
			builder.MilestoneStatusDate = new ZDateTime(2009, 12, 14, 18, 6, 0);

			ZString message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,,K
STS,POD,,L,14DEC09 1806
", message);
			AssertEquals(@"", buffer.AsString);

			buffer.Clear();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);

			shipment.JS_ActualWeight = 1200.467M;
			shipment.JS_ActualVolume = 1.234M;

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "COD1";
			org1.OH_RL_NKClosestPort = "AUALX";
			shipment.ConsigneeDeliveryAddress.OrganisationPK = org1.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "COD2";
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = org2.MainAddress.PK;

			message = builder.MessageText;
			AssertEquals(@"MSU,3
MSG,09DEC09 1413
SHD,ABC,12345678901234567,09DEC09,,10,1200.5,K,1.234,MC
STS,POD,AUALX,L,14DEC09 1806,,COD1,COD2
", message);
			AssertEquals("", buffer.AsString);
		}
	}
}
