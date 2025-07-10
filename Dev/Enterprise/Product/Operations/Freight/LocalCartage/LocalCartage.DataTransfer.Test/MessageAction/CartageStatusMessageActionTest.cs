using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.eHubMessaging.Tests;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Testing
{
	class CartageStatusMessageActionTest : ImportMessageActionTest
	{
		public void TestExecuteAction_ImportCartageJob()
		{
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);
			var buffer = new NotificationBuffer();
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_MessageText = messageBody;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.CartageJobs;
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XMS;
			interchange.EI_HeaderText = "";
			interchange.EI_FooterText = "";
			interchange.EI_BodyText = interchangeBody;
			message.EM_EI = interchange.PK;
			var action = new CartageStatusMessageAction(factoryProvider);
			List<ITransactionParticipant> forSave;
			Assert(action.ExecuteAction(message, buffer, out forSave));
			BusinessObjectFactory.SaveTogether(forSave.ToArray());
		}

		public void TestExecuteAction_UpdateShipmentLogsAndNotes()
		{
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S12345678";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);
			var buffer = new NotificationBuffer();
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_MessageText = messageBodyForWithShipmentRef;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.CartageJobs;
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XMS;
			interchange.EI_HeaderText = "";
			interchange.EI_FooterText = "";
			interchange.EI_BodyText = interchangeBody;
			message.EM_EI = interchange.PK;
			var action = new CartageStatusMessageAction(factoryProvider);
			List<ITransactionParticipant> forSave;
			Assert(action.ExecuteAction(message, buffer, out forSave));
			BusinessObjectFactory.SaveTogether(forSave.ToArray());
			var logs = shipment.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code));
			AssertEquals(1, logs.Length);
			AssertEquals(true, ((StmALog)logs[0]).SL_Reference.Contains("WKD-Work Completed"));
			var notes = shipment.Notes.FindByDescription(FreightConstants.LocalCartageNote);
			AssertEquals("One note should be created", 1, notes.Length);
		}

		const string interchangeBody = @"<?xml version=""1.0"" encoding=""utf-8""?><XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2011-08-22T10:26:55.027+10:00</Date><XmlType>LightWeight</XmlType><Source><EnterpriseCode>EDI</EnterpriseCode><CompanyCode>EDI</CompanyCode><OriginServer>DAT</OriginServer><LoginName>EDISupport</LoginName></Source><Target><Type>LocalCartageStatus</Type></Target><EDIOrganisation EDICode=""EDICUS"" OwnerCode=""EDICUS""><OrganisationDetails><Name>EDI CUSTOMS BROKERS</Name><Location Country=""Australia"" City=""Brisbane"">AUBNE</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>10 HUTCHESON STREET</AddressLine1><AddressLine2>ALBION  QLD</AddressLine2><AddressCode>PST: 10 HUTCHESON STREET</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>4010</PostCode><Language>EN</Language><Location>AUBNE</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><CartageJobs><CartageJob><Type>CJHR</Type><JobType>ISCC</JobType><JobNumber>T00001000</JobNumber><Action>STA</Action><ActionType>ACC</ActionType><MessageDescription>blah</MessageDescription><MessageResponseAddress>pavlo.tubolets.testing@enterprisedevelopment.cargowise.com</MessageResponseAddress><MessageSystemType>ediEnterprise</MessageSystemType><BillTo EDICode=""EDICUS"" OwnerCode=""EDICUS""><OrganisationDetails><Name>EDI CUSTOMS BROKERS</Name><Location Country=""Australia"" City=""Brisbane"">AUBNE</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>10 HUTCHESON STREET</AddressLine1><AddressLine2>ALBION  QLD</AddressLine2><AddressCode>PST: 10 HUTCHESON STREET</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>4010</PostCode><Language>EN</Language><Location>AUBNE</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></BillTo><OuterPacks><OuterPacksWeight>0</OuterPacksWeight><OuterPacksVolume>0</OuterPacksVolume></OuterPacks><SailingInfo /><CustomAttributes /></CartageJob></CartageJobs></Payload></XmlInterchange>";
		const string messageBody = @"<CartageJob><Type>CJHR</Type><JobType>ISCC</JobType><JobNumber>T00001000</JobNumber><Action>STA</Action><ActionType>ACC</ActionType><MessageDescription>blah</MessageDescription><MessageResponseAddress>pavlo.tubolets.testing@enterprisedevelopment.cargowise.com</MessageResponseAddress><MessageSystemType>ediEnterprise</MessageSystemType><BillTo EDICode=""EDICUS"" OwnerCode=""EDICUS""><OrganisationDetails><Name>EDI CUSTOMS BROKERS</Name><Location Country=""Australia"" City=""Brisbane"">AUBNE</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>10 HUTCHESON STREET</AddressLine1><AddressLine2>ALBION  QLD</AddressLine2><AddressCode>PST: 10 HUTCHESON STREET</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>4010</PostCode><Language>EN</Language><Location>AUBNE</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></BillTo><OuterPacks><OuterPacksWeight>0</OuterPacksWeight><OuterPacksVolume>0</OuterPacksVolume></OuterPacks><SailingInfo /><CustomAttributes /></CartageJob>";
		const string messageBodyForWithShipmentRef = @"<CartageJob><Type>CJHR</Type><JobType>ISCC</JobType><JobNumber>T00001000</JobNumber><Action>STA</Action><ActionType>WKD</ActionType><MessageDescription>blah</MessageDescription><MessageResponseAddress>pavlo.tubolets.testing@enterprisedevelopment.cargowise.com</MessageResponseAddress><MessageSystemType>ediEnterprise</MessageSystemType><ClientJobReference>S12345678</ClientJobReference><BillTo EDICode=""EDICUS"" OwnerCode=""EDICUS""><OrganisationDetails><Name>EDI CUSTOMS BROKERS</Name><Location Country=""Australia"" City=""Brisbane"">AUBNE</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>10 HUTCHESON STREET</AddressLine1><AddressLine2>ALBION  QLD</AddressLine2><AddressCode>PST: 10 HUTCHESON STREET</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>4010</PostCode><Language>EN</Language><Location>AUBNE</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></BillTo><OuterPacks><OuterPacksWeight>0</OuterPacksWeight><OuterPacksVolume>0</OuterPacksVolume></OuterPacks><SailingInfo /><CustomAttributes /></CartageJob>";
	}
}
