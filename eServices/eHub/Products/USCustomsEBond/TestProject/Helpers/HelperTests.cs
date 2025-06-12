using System;
using CargoWise.eHub.Products.USCustoms.eBond.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.USCustoms.eBond.Tests.Helpers
{
	[TestClass]
	public class HelperTests
	{
		[TestMethod]
		public void TestRemoveNamespace()
		{
			var xmlStringWithNamespace = @"<SuretyToBrokerMessage xmlns=""urn:us:csec:ebond:SuretyToBrokerMessage""></SuretyToBrokerMessage>";
			var expectedXmlStringWithoutNamespace = @"<SuretyToBrokerMessage></SuretyToBrokerMessage>";

			Assert.AreEqual(expectedXmlStringWithoutNamespace, TransformHelper.RemoveNamespace(xmlStringWithNamespace));

			Assert.AreEqual(expectedXmlStringWithoutNamespace, TransformHelper.RemoveNamespace(expectedXmlStringWithoutNamespace));

			var xmlStringWithNamespaceAlias = 
@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
	<soapenv:Header xmlns:wsa=""http://www.w3.org/2005/08/addressing"">
		<wsa:From>
			<wsa:Address>urn:abi:SV9</wsa:Address>
		</wsa:From>
	</soapenv:Header>
</soapenv:Envelope>";

			var xmlStringWithoutNamespaceAlias = 
@"<Envelope>
	<Header>
		<From>
			<Address>urn:abi:SV9</Address>
		</From>
	</Header>
</Envelope>";
			Assert.AreEqual(xmlStringWithoutNamespaceAlias, TransformHelper.RemoveNamespace(xmlStringWithNamespaceAlias));
		}

		[TestMethod]
		public void TestInsertXmlElement()
		{
			var eHubeBondMessage = new System.Xml.XmlDocument();

			eHubeBondMessage.LoadXml(@"<SuretyToBrokerMessage xmlns:ns0=""http://cargowise.com/ehub/core/2011/02"">
	<MessageLevelResult>
		<DispositionCode>E</DispositionCode>
		<SuretyResponseCode>B01</SuretyResponseCode>
		<SuretyResponseDescription>Data Errors see attached errors</SuretyResponseDescription>
		<BondAmount>0</BondAmount>
		<MessageLevelReasonCodes>
		<ReasonCode> 999 </ReasonCode>
		<ReasonDescription>The 'ImporterAddressState' element is invalid - The value ' ' is invalid according to its datatype 'String' - The actual length is greater than the MaxLength value.</ReasonDescription>
		</MessageLevelReasonCodes>
	</MessageLevelResult>
</SuretyToBrokerMessage>");

			var eHubeBondMessageExpected = new System.Xml.XmlDocument();
			eHubeBondMessageExpected.LoadXml(@"<SuretyToBrokerMessage xmlns:ns0=""http://cargowise.com/ehub/core/2011/02"">
	<MessageLevelResult>
		<TransactionIDTypeCode>ABC</TransactionIDTypeCode>
		<DispositionCode>E</DispositionCode>
		<SuretyResponseCode>B01</SuretyResponseCode>
		<SuretyResponseDescription>Data Errors see attached errors</SuretyResponseDescription>
		<BondAmount>0</BondAmount>
		<MessageLevelReasonCodes>
		<ReasonCode> 999 </ReasonCode>
		<ReasonDescription>The 'ImporterAddressState' element is invalid - The value ' ' is invalid according to its datatype 'String' - The actual length is greater than the MaxLength value.</ReasonDescription>
		</MessageLevelReasonCodes>
	</MessageLevelResult>
</SuretyToBrokerMessage>");

			TransformHelper.InsertOrUpdateXmlElement(eHubeBondMessage, "TransactionIDTypeCode", "ABC", "//*[local-name()='MessageLevelResult']");

			Assert.AreEqual(eHubeBondMessageExpected.OuterXml, eHubeBondMessage.OuterXml);
		}

		[TestMethod]
		public void TestUpdateXmlElement()
		{
			var eHubeBondMessage = new System.Xml.XmlDocument();

			eHubeBondMessage.LoadXml(@"<SuretyToBrokerMessage xmlns:ns0=""http://cargowise.com/ehub/core/2011/02"">
	<MessageLevelResult>
		<TransactionIDTypeCode/>
		<DispositionCode>E</DispositionCode>
		<SuretyResponseCode>B01</SuretyResponseCode>
		<SuretyResponseDescription>Data Errors see attached errors</SuretyResponseDescription>
		<BondAmount>0</BondAmount>
		<MessageLevelReasonCodes>
		<ReasonCode> 999 </ReasonCode>
		<ReasonDescription>The 'ImporterAddressState' element is invalid - The value ' ' is invalid according to its datatype 'String' - The actual length is greater than the MaxLength value.</ReasonDescription>
		</MessageLevelReasonCodes>
	</MessageLevelResult>
</SuretyToBrokerMessage>");

			var eHubeBondMessageExpected = new System.Xml.XmlDocument();
			eHubeBondMessageExpected.LoadXml(@"<SuretyToBrokerMessage xmlns:ns0=""http://cargowise.com/ehub/core/2011/02"">
	<MessageLevelResult>
		<TransactionIDTypeCode>ABC</TransactionIDTypeCode>
		<DispositionCode>E</DispositionCode>
		<SuretyResponseCode>B01</SuretyResponseCode>
		<SuretyResponseDescription>Data Errors see attached errors</SuretyResponseDescription>
		<BondAmount>0</BondAmount>
		<MessageLevelReasonCodes>
		<ReasonCode> 999 </ReasonCode>
		<ReasonDescription>The 'ImporterAddressState' element is invalid - The value ' ' is invalid according to its datatype 'String' - The actual length is greater than the MaxLength value.</ReasonDescription>
		</MessageLevelReasonCodes>
	</MessageLevelResult>
</SuretyToBrokerMessage>");

			TransformHelper.InsertOrUpdateXmlElement(eHubeBondMessage, "TransactionIDTypeCode", "ABC", "//*[local-name()='MessageLevelResult']");

			Assert.AreEqual(eHubeBondMessageExpected.OuterXml, eHubeBondMessage.OuterXml);
		}

		[TestMethod]
		public void TestInsertOrUpdateXmlElement_ExceptionOnInvalidXpath()
		{
			var eHubeBondMessage = new System.Xml.XmlDocument();

			eHubeBondMessage.LoadXml(@"<SuretyToBrokerMessage xmlns:ns0=""http://cargowise.com/ehub/core/2011/02"">
	<MessageLevelResult>
		<DispositionCode>E</DispositionCode>
		<SuretyResponseCode>B01</SuretyResponseCode>
		<SuretyResponseDescription>Data Errors see attached errors</SuretyResponseDescription>
		<BondAmount>0</BondAmount>
		<MessageLevelReasonCodes>
		<ReasonCode> 999 </ReasonCode>
		<ReasonDescription>The 'ImporterAddressState' element is invalid - The value ' ' is invalid according to its datatype 'String' - The actual length is greater than the MaxLength value.</ReasonDescription>
		</MessageLevelReasonCodes>
	</MessageLevelResult>
</SuretyToBrokerMessage>");

			try
			{
				TransformHelper.InsertOrUpdateXmlElement(eHubeBondMessage, "TransactionIDTypeCode", "ABC", "//*[local-name()='rubbish']");
			}
			catch(Exception ex)
			{
				Assert.AreEqual("Invalid xpath: //*[local-name()='rubbish']", ex.Message);
			}
			
		}
	}
}
