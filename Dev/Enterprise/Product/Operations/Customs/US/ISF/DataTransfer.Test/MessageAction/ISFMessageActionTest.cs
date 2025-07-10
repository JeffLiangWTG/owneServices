using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Tasks.StandardXMLProcessor;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ISF.DataTransfer.Testing
{
	sealed class ISFMessageActionTest : TestCaseWithFactory
	{
		public void TestEndToEnd()
		{
			Assert("Ensure database is clean", Factory.Load<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_BondNumberOrHolder, "23-130634400")).IsNullOrEmpty());
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.XMS;
			message.EM_MessageType = EDIMessage.ApplicationCodes.XMS;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.ISFs;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "00000000000000000101";
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?><XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2011-02-08T13:22:30.42+11:00</Date><Source><EnterpriseCode>HYE</EnterpriseCode><CompanyCode>DAU</CompanyCode><OriginServer>KSD</OriginServer><LoginName>CWSupport</LoginName></Source><ReferenceKeys><ReferenceKey ReferenceKeyName=""BatchNumber"">3</ReferenceKey></ReferenceKeys><Target /><EDIOrganisation EDICode=""CAREDIBNE"" OwnerCode=""CAREDIBNE""><OrganisationDetails><Name>CARGOWISE EDI</Name><Location Country=""Australia"" City=""Brisbane"">AUBNE</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>10 HUTCHESON STREET</AddressLine1><AddressCode>Pick Up Address</AddressCode><CityOrSuburb>ALBION</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4010</PostCode><Language>EN</Language><Location>AUBNE</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /><AddressCapability IsMainAddress=""true"" AddressType=""PAD"" /><AddressCapability IsMainAddress=""false"" AddressType=""PIC"" /></AddressCapabilities></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>UNC</NumberType><Number>JASUNC1</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>UOC</NumberType><Number>JASUOC1</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ABN</NumberType><Number>14 001 592 650</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>14 001 592 650</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><ISFs><ISF><ImporterOfRecord><RegistrationNumber><CountryOfRegistration>US</CountryOfRegistration><NumberType>EIN</NumberType><Number>23-130634400</Number></RegistrationNumber></ImporterOfRecord><SubmissionType>ISF5</SubmissionType><ShipmentType>01</ShipmentType><TransportMode>11</TransportMode><OwnerReference>TEST</OwnerReference><BondActivityCode>01</BondActivityCode><BondType>8</BondType><BondHolder>23-130634400</BondHolder><Parties></Parties><ReferenceIDs><ReferenceID><Type>OB</Type><Number>ADSFADSFADSFASD</Number></ReferenceID></ReferenceIDs></ISF></ISFs></Payload></XmlInterchange>";
			Factory.Save();
			var processor = new StandardXMLMessageServiceTask();
			processor.ServiceLogger = new DummyLogger();
			processor.RunTask();
			Assert("ISF created from Native XML", Factory.Load<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_BondNumberOrHolder, "23-130634400")).Length > 0);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEndToEndWithSendToCustoms()
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.XMS;
			message.EM_MessageType = EDIMessage.ApplicationCodes.XMS;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.ISFs;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "00000000000000000101";
			var fileName = BaseSourcePath + @"Enterprise\Product\Operations\Customs\US\ISF\DataTransfer.Test\TestFiles\SendToCustoms.xml";
			using (var reader = new StreamReader(fileName, System.Text.Encoding.UTF8))
			{
				message.EM_MessageText = reader.ReadToEnd();
			}

			Factory.Save();
			var processor = new StandardXMLMessageServiceTask();
			processor.ServiceLogger = new DummyLogger();
			processor.RunTask();
			var query = new ZQuery(CusISFBillSchema.BB_BillNum, "MAEU953873892");
			var oceanbill = Factory.LoadTop1<CusISFBill>(query);
			AssertNotNull(oceanbill);
			AssertNotNull(oceanbill.Header);
			AssertEquals(1, oceanbill.Header.Messages.Count);
		}
	}
}
