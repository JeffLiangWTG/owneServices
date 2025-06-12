using System.Xml;
using eServices.Configuration.Framework;
using CargoWise.eHub.Products.TWCustoms.Orchestrations.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.TWCustoms.BizTalk.Tests.OrchestrationHelpers
{
	[TestClass]
	public class OrchestrationHelperTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetMessageCode()
		{
			var responseMessage = new XmlDocument();
			responseMessage.LoadXml(messageXml_TWCustomsForwarderManifest);
			string messageCode = OrchestrationHelper.GetMessageCode(responseMessage);
			Assert.AreEqual("N5108", messageCode);
		}

		[TestMethod]
		public void TestGetUserNameWithValue()
		{
			var userName = OrchestrationHelper.GetUserName(configXml_TWCustoms);
			Assert.AreEqual("TVCBBKTWTPE10426", userName);

			userName = OrchestrationHelper.GetUserName(configXml_TWCustomsForwarderAndLicensing);
			Assert.AreEqual("TVCBBKTWTPE10426_Manifest", userName);
		}

		[TestMethod]
		public void TestGetUserName_NullorEmpty()
		{
			const string expectedExceptionMessage = "UserName is missing from Configuration message.";

			AssertException.Throws<ConfigurationException>(() => OrchestrationHelper.GetUserName(configXml_TWCustoms_EmptyUserName), expectedExceptionMessage);
			AssertException.Throws<ConfigurationException>(() => OrchestrationHelper.GetUserName(configXml_TWCustoms_WithoutUserName), expectedExceptionMessage);
		}

		[TestMethod]
		public void TestGetWcfInsertOutboxQueries_ForwarderManifestCode()
		{
			var manifestWrapperNS = "http://cargowise.com/ehub/products/TWCustomsForwarderManifest#TWCustomsForwarderManifest";
			var inboxOutboxQueries = OrchestrationHelper.GetWcfInsertInboxOutboxQueries("SenderID", "RecipientID", "00000000-0000-0000-0000-000000000000", messageXml_TWCustomsForwarderManifest);
			var responseMessage = new XmlDocument();
			responseMessage.LoadXml(inboxOutboxQueries);
			
			var messageType = responseMessage.SelectSingleNode("//*[local-name()='MessageType']");
			Assert.AreEqual(messageType.InnerText, manifestWrapperNS);
			
			var targetMessageType = responseMessage.SelectSingleNode("//*[local-name()='TargetMessageType']");
			Assert.AreEqual(messageType.InnerText, manifestWrapperNS);
		}

		[TestMethod]
		public void TestGetWcfInsertOutboxQueries_ForwarderAndLicensingCode()
		{
			var forwarderAndLicensingWrapperNS = "http://cargowise.com/ehub/products/TWCustomsLicensing#TWCustomsLicensing";
			var inboxOutboxQueries = OrchestrationHelper.GetWcfInsertInboxOutboxQueries("SenderID", "RecipientID", "00000000-0000-0000-0000-000000000000", messageXml_TWCustomsForwarderAndLicensing);
			var responseMessage = new XmlDocument();
			responseMessage.LoadXml(inboxOutboxQueries);

			var messageType = responseMessage.SelectSingleNode("//*[local-name()='MessageType']");
			Assert.AreEqual(messageType.InnerText, forwarderAndLicensingWrapperNS);

			var targetMessageType = responseMessage.SelectSingleNode("//*[local-name()='TargetMessageType']");
			Assert.AreEqual(messageType.InnerText, forwarderAndLicensingWrapperNS);
		}

		[TestMethod]
		public void TestGetWcfInsertOutboxQueries_TWCustomsCode()
		{
			var wrapperNS = "http://cargowise.com/ehub/products/TWCustoms#TWCustomsResponse";
			var k = OrchestrationHelper.GetWcfInsertInboxOutboxQueries("SenderID", "RecipientID", "00000000-0000-0000-0000-000000000000", messageXml_TWCustoms);
			var responseMessage = new XmlDocument();
			responseMessage.LoadXml(k);

			var messageType = responseMessage.SelectSingleNode("//*[local-name()='MessageType']");
			Assert.AreEqual(messageType.InnerText, wrapperNS);

			var targetMessageType = responseMessage.SelectSingleNode("//*[local-name()='TargetMessageType']");
			Assert.AreEqual(messageType.InnerText, wrapperNS);
		}

		#region TestData
		const string configXml_TWCustoms = @"<Configuration xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"" Name=""TWCustomsSubscribers"" Version=""1.0"" TimeStamp=""2020-09-10T02:38:11"">
			  <Group Type=""System"" Reference=""D21PRD"">
				<Group Type=""Company"" Reference=""TPE"">
				  <Group Type=""Staff"" Reference=""BRK"">
					<Group Type=""MailBoxID"" Reference=""CBK0224-0"" Status=""VAL"">
					  <Item Name=""Platform"">TVA</Item>
					  <Item Name=""ReceiveAutomatically"">0</Item>
					  <Credential>
						<UserName>TVCBBKTWTPE10426</UserName>
						<Password></Password>
					  </Credential>
					  <Certificate Name=""Certificate"">
						<File></File>
						<Passphrase></Passphrase>
					  </Certificate>
					</Group>
				  </Group>
				</Group>
			  </Group>
			</Configuration>";
		readonly string configXml_TWCustoms_EmptyUserName = configXml_TWCustoms.Replace("TVCBBKTWTPE10426", string.Empty);
		readonly string configXml_TWCustoms_WithoutUserName = configXml_TWCustoms.Replace("<UserName>TVCBBKTWTPE10426</UserName>", string.Empty);

		const string configXml_TWCustomsForwarderAndLicensing = @"<Configuration xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"" Name=""TWCustomsSubscribers"" Version=""1.0"" TimeStamp=""2020-09-10T02:38:11"">
			  <Group Type=""System"" Reference=""D21PRD"">
				<Group Type=""Company"" Reference=""TPE"">
					<Group Type=""MailBoxID"" Reference=""CBK0224-0"" Status=""VAL"">
					  <Item Name=""Platform"">TVA</Item>
					  <Item Name=""ReceiveAutomatically"">0</Item>
					  <Credential>
						<UserName>TVCBBKTWTPE10426_Manifest</UserName>
						<Password></Password>
					  </Credential>
					  <Certificate Name=""Certificate"">
						<File></File>
						<Passphrase></Passphrase>
					  </Certificate>
					</Group>
				</Group>
			  </Group>
			</Configuration>";

		const string messageXml_TWCustomsForwarderManifest = @"<Response xmlns=""urn:wco:datamodel:TW:N5108:R-00-05"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""urn:wco:datamodel:TW:N5108:R-00-05 N5108.xsd"">
				<Error>
				<ValidationCode>031</ValidationCode>
				</Error>
				<Status>
				<NameCode>RE</NameCode>
				</Status>
				<Declaration>
				<BorderTransportMeans>
					<JourneyID>5X 0059</JourneyID>
					<TypeCode>4</TypeCode>
				</BorderTransportMeans>
				<Consignment>
					<BoardedQuantity>0</BoardedQuantity>
					<ConsignmentItem>
					<PreviousDocument>
						<tw_FunctionalReferenceID>11233527SW1812100001</tw_FunctionalReferenceID>
						<TypeCode>5101H</TypeCode>
					</PreviousDocument>
					</ConsignmentItem>
					<TransportContractDocument>
					<ID>406-51754603</ID>
					<TypeCode>741</TypeCode>
					</TransportContractDocument>
					<TransportContractDocument>
					<ID>5396340134</ID>
					<TypeCode>703</TypeCode>
					</TransportContractDocument>
				</Consignment>
				<GovernmentProcedure>
					<tw_TransportTypeCode>1</tw_TransportTypeCode>
				</GovernmentProcedure>
				<TransportEquipment>
					<tw_EmptyContainerQuantity>0</tw_EmptyContainerQuantity>
					<tw_FullContainerQuantity>0</tw_FullContainerQuantity>
				</TransportEquipment>
				</Declaration>
			</Response>";
		readonly string messageXml_TWCustomsForwarderAndLicensing = messageXml_TWCustomsForwarderManifest.Replace("N5108", "NX903");
		readonly string messageXml_TWCustoms = messageXml_TWCustomsForwarderManifest.Replace("N5108", "00000");
		#endregion
	}
}
