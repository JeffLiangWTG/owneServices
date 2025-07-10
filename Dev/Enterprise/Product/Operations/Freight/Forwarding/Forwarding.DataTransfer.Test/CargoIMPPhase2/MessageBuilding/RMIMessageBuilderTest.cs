using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class RMIMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2009, 12, 9, 18, 6, 7)]
		public void TestMinimalMessage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			RMIMessageBuilder builder = new RMIMessageBuilder();
			builder.Shipment = shipment;
			NotificationBuffer buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			ZString message = builder.MessageText;
			AssertEquals(@"RMI,3
MSG,09DEC09 1806
SHD,,,09DEC09,,,,,K
RTG,,L
PDL,DP
REF," + shipment.JS_UniqueConsignRef + @",JOB
", message);
			AssertEquals(@"Error: Required field empty ('Forwarder Identification' registry item)
Error: Required field empty (House Bill Number)
Error: Required field empty (Origin)
Error: Required field empty (Destination)
Error: Required field empty (Packs)
Error: Required field empty (Weight)
", buffer.AsString);

			buffer.Clear();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBill = "HOUSE BILL";
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 1300.345M;
			message = builder.MessageText;
			AssertEquals(@"RMI,3
MSG,09DEC09 1806
SHD,ABC,HOUSE BILL,09DEC09,SYD,AKL,10,1300.3,K
RTG,,L,SYD,,AKL
PDL,DP,AKL,,AKL
REF," + shipment.JS_UniqueConsignRef + @",JOB
", message);
			AssertEquals("", buffer.AsString);
		}

		[TestDate(2009, 12, 9, 18, 6, 7)]
		public void TestFullMessage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBill = "HOUSE BILL";
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 1300.345M;
			shipment.JS_ActualVolume = 1.077M;

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "COD1";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = org1.PK;

			var pickupAddress = org1.Addresses.AddNew();
			pickupAddress.OA_RL_NKRelatedPortCode = "AUMAS";
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;

			shipment.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2009, 12, 10);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "COD2";
			org2.OH_RL_NKClosestPort = "AUALX";
			shipment.JS_OA_ExportReceivingDepot = org2.MainAddress.PK;

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "COD3";
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = org3.MainAddress.PK;

			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "COD4";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org4.PK;

			var deliveryAddress = org4.Addresses.AddNew();
			deliveryAddress.OA_RL_NKRelatedPortCode = "NZPOK";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;

			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2009, 12, 28);

			var org5 = Factory.New<OrgHeader>();
			org5.OH_Code = "COD5";
			org5.OH_RL_NKClosestPort = "NZGBS";
			shipment.JS_OA_ImportReleaseDepot = org5.MainAddress.PK;

			var org6 = Factory.New<OrgHeader>();
			org6.OH_Code = "COD6";
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = org6.MainAddress.PK;

			var org7 = Factory.New<OrgHeader>();
			org7.OH_Code = "COD7";
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = org7.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "027-123456";
			consol.JK_RL_NKLoadPort = "AUCES";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUCES";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "NZLYT";
			consol.Transports.MostInterestingTransport.JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2009, 12, 11);
			consol.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2009, 12, 25);

			var otherTransport = consol.Transports.AddNew();
			otherTransport.JW_RL_NKLoadPort = "NZLYT";
			otherTransport.JW_RL_NKDiscPort = "NZAKL";
			otherTransport.JW_TransportMode = Constants.TransportModes.Road;
			otherTransport.JW_ETD = new ZDateTime(2009, 12, 25);
			otherTransport.JW_ETA = new ZDateTime(2009, 12, 27, 5, 9, 1);

			shipment.Consols.Add(consol);

			RMIMessageBuilder builder = new RMIMessageBuilder();
			builder.Shipment = shipment;
			NotificationBuffer buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			ZString message = builder.MessageText;
			AssertEquals(string.Format(@"RMI,3
MSG,09DEC09 1806
SHD,ABC,HOUSE BILL,09DEC09,SYD,AKL,10,1300.3,K,1.077,MC,,,,,COD1
RTG,,L,AUALX,,GBS
PMV,AUALX,,,CES,ET
PMV,CES,,11DEC09 0000,NZLYT,,25DEC09 0000,,,,AIR,027-123456
PMV,NZLYT,,25DEC09 0000,AKL,,27DEC09 0509,,,,ROAD
PMV,LYT,IT,,GBS
PDL,DP,GBS,,AKL,,28DEC09,0000,,,,COD4,COD6
REF,EBM22Q33TU475BXH3P60,JOB
PAR,COD1,SHP
PAR,COD4,CNE
PAR,COD7,ANP
", shipment.JS_UniqueConsignRef), message);
			AssertEquals("", buffer.AsString);
		}

		[TestDate(2009, 12, 9, 18, 6, 7)]
		public void TestBrokenRelatedParty()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBill = "HOUSE BILL";
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 1300.345M;
			Factory.Save();

			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			shipment.ConsignorPickupAddress.E2_OA_Address = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			shipment.JS_OA_ExportReceivingDepot = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;

			RMIMessageBuilder builder = new RMIMessageBuilder();
			builder.Shipment = shipment;
			NotificationBuffer buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			ZString message = builder.MessageText;
			AssertEquals(@"RMI,3
MSG,09DEC09 1806
SHD,ABC,HOUSE BILL,09DEC09,SYD,AKL,10,1300.3,K
RTG,,L,SYD,,AKL
PDL,DP,AKL,,AKL
REF," + shipment.JS_UniqueConsignRef + @",JOB
", message);
			AssertEquals("", buffer.AsString);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "COD_1";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = org1.PK;

			buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			message = builder.MessageText;
			AssertEquals(@"RMI,3
MSG,09DEC09 1806
SHD,ABC,HOUSE BILL,09DEC09,SYD,AKL,10,1300.3,K,,,,,,,COD1
RTG,,L,SYD,,AKL
PDL,DP,AKL,,AKL
REF," + shipment.JS_UniqueConsignRef + @",JOB
PAR,COD1,SHP
", message);
			AssertEquals("", buffer.AsString);

			buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			shipment.JS_UniqueConsignRef = "AAA_VV11";
			message = builder.MessageText;
			AssertEquals(@"RMI,3
MSG,09DEC09 1806
SHD,ABC,HOUSE BILL,09DEC09,SYD,AKL,10,1300.3,K,,,,,,,COD1
RTG,,L,SYD,,AKL
PDL,DP,AKL,,AKL
PAR,COD1,SHP
", message);
			AssertEquals(@"Warning: Shipment ID: value can't be formatted, Input data must consist of 'Text character - only comprising upper case alphabetic, numeric, hyphen, full stop, forward slash and space' with length from 0 to 20
", buffer.AsString);
		}
	}
}
