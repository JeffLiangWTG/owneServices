using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.eHubMessaging.Tests;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Testing
{
	class CartageBookingMessageActionTest : ImportMessageActionTest
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
			var action = new CartageBookingMessageAction(factoryProvider);
			List<ITransactionParticipant> forSave;
			Assert(action.ExecuteAction(message, buffer, out forSave));
			BusinessObjectFactory.SaveTogether(forSave.ToArray());
		}

		const string interchangeBody = @"<?xml version=""1.0"" encoding=""utf-8""?><XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2011-08-22T10:26:55.027+10:00</Date><XmlType>LightWeight</XmlType><Source><EnterpriseCode>EDI</EnterpriseCode><CompanyCode>EDI</CompanyCode><OriginServer>DAT</OriginServer><LoginName>EDISupport</LoginName></Source><EDIOrganisation EDICode=""EDICUS"" OwnerCode=""EDICUS""><OrganisationDetails><Name>EDI CUSTOMS BROKERS</Name><Location Country=""Australia"" City=""Brisbane"">AUBNE</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>10 HUTCHESON STREET</AddressLine1><AddressLine2>ALBION  QLD</AddressLine2><AddressCode>PST: 10 HUTCHESON STREET</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>4010</PostCode><Language>EN</Language><Location>AUBNE</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><CartageJobs><CartageJob><Type>CJHR</Type><JobType>ISCC</JobType><JobNumber>T00001000</JobNumber><Action>STA</Action><ActionType>ACC</ActionType><MessageDescription>blah</MessageDescription><MessageResponseAddress>pavlo.tubolets.testing@enterprisedevelopment.cargowise.com</MessageResponseAddress><MessageSystemType>ediEnterprise</MessageSystemType><BillTo EDICode=""EDICUS"" OwnerCode=""EDICUS""><OrganisationDetails><Name>EDI CUSTOMS BROKERS</Name><Location Country=""Australia"" City=""Brisbane"">AUBNE</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>10 HUTCHESON STREET</AddressLine1><AddressLine2>ALBION  QLD</AddressLine2><AddressCode>PST: 10 HUTCHESON STREET</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>4010</PostCode><Language>EN</Language><Location>AUBNE</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></BillTo><OuterPacks><OuterPacksWeight>0</OuterPacksWeight><OuterPacksVolume>0</OuterPacksVolume></OuterPacks><SailingInfo /><CustomAttributes /></CartageJob></CartageJobs></Payload></XmlInterchange>";
		const string messageBody = @"<CartageJob><Type>CJHR</Type><JobType>ISCC</JobType><JobNumber>T00001000</JobNumber><Action>STA</Action><ActionType>ACC</ActionType><MessageDescription>blah</MessageDescription><MessageResponseAddress>pavlo.tubolets.testing@enterprisedevelopment.cargowise.com</MessageResponseAddress><MessageSystemType>ediEnterprise</MessageSystemType><BillTo EDICode=""EDICUS"" OwnerCode=""EDICUS""><OrganisationDetails><Name>EDI CUSTOMS BROKERS</Name><Location Country=""Australia"" City=""Brisbane"">AUBNE</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>10 HUTCHESON STREET</AddressLine1><AddressLine2>ALBION  QLD</AddressLine2><AddressCode>PST: 10 HUTCHESON STREET</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>4010</PostCode><Language>EN</Language><Location>AUBNE</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></BillTo><OuterPacks><OuterPacksWeight>0</OuterPacksWeight><OuterPacksVolume>0</OuterPacksVolume></OuterPacks><SailingInfo /><CustomAttributes /></CartageJob>";
	}
}
