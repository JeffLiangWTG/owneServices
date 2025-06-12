using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.TL.BT.Helpers;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.TL.BT.Tests.Helpers
{
	[TestClass]
	public class OrchestrationHelpersTests
	{
		[TestInitialize]
		public void Initialize()
		{
			OrchestrationHelpers.Now = () => DateTime.UtcNow;
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_GBCBResponse_Success()
		{
			var originalRecipient = "GBCustoms-CTCGB";

			#region Input XML
			var responseXml = @"<ns:SuccessResponse xmlns:ns=""http://www.hmrc.gov.uk/successresponse/2"" xmlns=""http://www.govtalk.gov.uk/enforcement/ICS/responsedata/7"">
   <ns:ResponseData>
       <CorrelationId>0JRF7UncK0t004</CorrelationId>
       <WhateverId>111</WhateverId>
   </ns:ResponseData>
</ns:SuccessResponse>";
			#endregion

			#region Expected XML
			var expectedXml = @"<CompositeOperation>
	<Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage""><Rows><eHubInboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
		<EI_PK>00000000-0000-0000-0000-000000000000</EI_PK>
		<EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID>
		<EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
		<EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender>
		<EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
		<EI_MessageType>http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse</EI_MessageType>
		<EI_IsFlatFile>false</EI_IsFlatFile>
		<EI_ApplicationCode>CDS</EI_ApplicationCode>
		<EI_Status>2</EI_Status>
		<EI_Content>H4sIAAAAAAAEAHWSW0+DMBTH3038DqQfYAeMyZKl8ADTOS+JgS36SsrZIIzT2RZw397KBo6pbz3/S/rrhS/CqNZGVjqsdUGodYx6L0ljcH3lOLyfHjDNUDmvSjaFXfgsWkWLkHWhLvZRozbLeVBJUQ4Thx/jlExQNYXAGDeokAR2hUuRw69YRwNjnDFiKLODE0kySGZ12KPP3l+emXNHQmYFbX1GknAgJj1LaiHODux8VjurkvZZbsx+BtC27SSvlJhsZTOpS9DHgjoV4IYdO6OCzZp0V/YdpI1UAisLBcsogb6cpSaFaY9zBOpJ5tYbDGtFUincpaaQtMwC9zG+n65JPLnGdW85jN2z2lueGmxQWdXzPA5n87Ar/L1tp1/cz8UTfF+3lTj8/4O+AAigWPFdAgAA</EI_Content>
	</eHubInboxMessage>
</Rows>
</Insert>
<Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent""><Rows><eHubInboxXmlContent xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
	<EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox>
	<EX_XmlContent>
	&lt;GBCustomsBusinessResponse&gt;
	&lt;ResponseHeaderProvider=""CTCGB""&gt;
		&lt;RequestID&gt;mockRequestID&lt;/RequestID&gt;
		&lt;ServiceReference&gt;mockServiceReference&lt;/ServiceReference&gt;
	&lt;/ResponseHeader&gt;
	&lt;ResponseBodyContentType=""XML""Encoding=""none""&gt;
		&lt;ns:SuccessResponsexmlns:ns=""http://www.hmrc.gov.uk/successresponse/2"" xmlns=""http://www.govtalk.gov.uk/enforcement/ICS/responsedata/7""&gt;
			&lt;ns:ResponseData&gt;
				&lt;CorrelationId&gt;0JRF7UncK0t004&lt;/CorrelationId&gt;
				&lt;WhateverId&gt;111&lt;/WhateverId&gt;
			&lt;/ns:ResponseData&gt;
		&lt;/ns:SuccessResponse&gt;
	&lt;/ResponseBody&gt;
&lt;/GBCustomsBusinessResponse&gt;
	</EX_XmlContent>
	<EX_DT_Source>12F1150E-4F3F-410F-990C-BAAB866EDC29</EX_DT_Source>
	<EX_UncompressedLength>605</EX_UncompressedLength>
</eHubInboxXmlContent>
</Rows>
</Insert>
<Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage""><Rows><eHubOutboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
	<OI_PK>00000000-0000-0000-0000-000000000000</OI_PK>
	<OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID>
	<OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK>
	<OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender>
	<OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
	<OI_DT_Target>12F1150E-4F3F-410F-990C-BAAB866EDC29</OI_DT_Target>
	<OI_Status>0</OI_Status>
	<OI_Content>H4sIAAAAAAAEAHWSW0+DMBTH3038DqQfYAeMyZKl8ADTOS+JgS36SsrZIIzT2RZw397KBo6pbz3/S/rrhS/CqNZGVjqsdUGodYx6L0ljcH3lOLyfHjDNUDmvSjaFXfgsWkWLkHWhLvZRozbLeVBJUQ4Thx/jlExQNYXAGDeokAR2hUuRw69YRwNjnDFiKLODE0kySGZ12KPP3l+emXNHQmYFbX1GknAgJj1LaiHODux8VjurkvZZbsx+BtC27SSvlJhsZTOpS9DHgjoV4IYdO6OCzZp0V/YdpI1UAisLBcsogb6cpSaFaY9zBOpJ5tYbDGtFUincpaaQtMwC9zG+n65JPLnGdW85jN2z2lueGmxQWdXzPA5n87Ar/L1tp1/cz8UTfF+3lTj8/4O+AAigWPFdAgAA</OI_Content>
	<OI_XmlContent>
	&lt;GBCustomsBusinessResponse&gt;
	&lt;ResponseHeaderProvider=""CTCGB""&gt;
		&lt;RequestID&gt;mockRequestID&lt;/RequestID&gt;
		&lt;ServiceReference&gt;mockServiceReference&lt;/ServiceReference&gt;
	&lt;/ResponseHeader&gt;
	&lt;ResponseBodyContentType=""XML""Encoding=""none""&gt;
		&lt;ns:SuccessResponsexmlns:ns=""http://www.hmrc.gov.uk/successresponse/2"" xmlns=""http://www.govtalk.gov.uk/enforcement/ICS/responsedata/7""&gt;
			&lt;ns:ResponseData&gt;
				&lt;CorrelationId&gt;0JRF7UncK0t004&lt;/CorrelationId&gt;
				&lt;WhateverId&gt;111&lt;/WhateverId&gt;
			&lt;/ns:ResponseData&gt;
		&lt;/ns:SuccessResponse&gt;
	&lt;/ResponseBody&gt;
&lt;/GBCustomsBusinessResponse&gt;
	</OI_XmlContent>
</eHubOutboxMessage>
</Rows>
</Insert>
</CompositeOperation>";
			#endregion

			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "GetPushData", "WTLDTWJLI", responseXml, null,
				gbCustomsXml, true, "77777777-7777-7777-7777-777777777777", "mockServiceReference", "mockRequestID");

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_CTCGetPushDataResponse_Success()
		{
			var originalRecipient = "GBCustoms-CTCGB";

			#region Input XML
			var responseXml = @"{
	""messageType"":""IE008"",
	""requestId"":""/customs/transits/movements/arrivals/775"",
	""arrivalId"":""775"",
	""messageId"":""1"",
	""received"":""2021-07-01T14:01:00"",
	""body"":""<CC008A>blahblah</CC008A>""
}";
			#endregion

			#region Expected XML
			var expectedXml = @"<CompositeOperation>
	<Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage""><Rows><eHubInboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
		<EI_PK>00000000-0000-0000-0000-000000000000</EI_PK>
		<EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID>
		<EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
		<EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender>
		<EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
		<EI_MessageType>http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse</EI_MessageType>
		<EI_IsFlatFile>false</EI_IsFlatFile>
		<EI_ApplicationCode>CDS</EI_ApplicationCode>
		<EI_Status>2</EI_Status>
		<EI_Content>H4sIAAAAAAAEAHVQwQqCQBC9B/3Dsgev6zFwFXILCwrCPHQ1nUzSWXNXwb9Pl0wsOgzMe/PezGN44ItGaVkqv1E5glIhqEqiAm+5IISPaAdxCjU51bLN+8alIhKBT43IyJ4NKL3feKVMHh/E2TR4K89Qt3kCIdygBkzAGL5Jzn5kJg2bx5lH9GXaESFRA+qoq8Cll+OBki0mMs0xcylKhDGxVWhHCNtera1MO9civg81sGyi5zeH/b2bs/8vewEelFJ/TgEAAA==</EI_Content>
	</eHubInboxMessage>
</Rows>
</Insert>
<Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent""><Rows><eHubInboxXmlContent xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
	<EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox>
	<EX_XmlContent>
	&lt;GBCustomsBusinessResponse&gt;
		&lt;ResponseHeaderProvider=""CTCGB""&gt;
			&lt;RequestID&gt;mockRequestID&lt;/RequestID&gt;
			&lt;ServiceReference&gt;mockServiceReference&lt;/ServiceReference&gt;
		&lt;/ResponseHeader&gt;
		&lt;ResponseBodyContentType=""XML""Encoding=""none""&gt;
			&amp;lt;CC008A&amp;gt;blahblah&amp;lt;/CC008A&amp;gt;
		&lt;/ResponseBody&gt;
	&lt;/GBCustomsBusinessResponse&gt;
	</EX_XmlContent>
	<EX_DT_Source>12F1150E-4F3F-410F-990C-BAAB866EDC29</EX_DT_Source>
	<EX_UncompressedLength>334</EX_UncompressedLength>
</eHubInboxXmlContent>
</Rows>
</Insert>
<Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage""><Rows><eHubOutboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
	<OI_PK>00000000-0000-0000-0000-000000000000</OI_PK>
	<OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID>
	<OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK>
	<OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender>
	<OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
	<OI_DT_Target>12F1150E-4F3F-410F-990C-BAAB866EDC29</OI_DT_Target>
	<OI_Status>0</OI_Status>
	<OI_Content>H4sIAAAAAAAEAHVQwQqCQBC9B/3Dsgev6zFwFXILCwrCPHQ1nUzSWXNXwb9Pl0wsOgzMe/PezGN44ItGaVkqv1E5glIhqEqiAm+5IISPaAdxCjU51bLN+8alIhKBT43IyJ4NKL3feKVMHh/E2TR4K89Qt3kCIdygBkzAGL5Jzn5kJg2bx5lH9GXaESFRA+qoq8Cll+OBki0mMs0xcylKhDGxVWhHCNtera1MO9civg81sGyi5zeH/b2bs/8vewEelFJ/TgEAAA==</OI_Content>
	<OI_XmlContent>
	&lt;GBCustomsBusinessResponse&gt;
		&lt;ResponseHeaderProvider=""CTCGB""&gt;
			&lt;RequestID&gt;mockRequestID&lt;/RequestID&gt;
			&lt;ServiceReference&gt;mockServiceReference&lt;/ServiceReference&gt;
		&lt;/ResponseHeader&gt;
		&lt;ResponseBodyContentType=""XML""Encoding=""none""&gt;
			&amp;lt;CC008A&amp;gt;blahblah&amp;lt;/CC008A&amp;gt;
		&lt;/ResponseBody&gt;
	&lt;/GBCustomsBusinessResponse&gt;
	</OI_XmlContent>
</eHubOutboxMessage>
</Rows>
</Insert>
</CompositeOperation>";
			#endregion

			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "GetPushData", "WTLDTWJLI", responseXml, null,
				gbCustomsXml, true, "77777777-7777-7777-7777-777777777777", "mockServiceReference", "mockRequestID");

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_EmcsUnsolicitedResponse_Success()
		{
			var originalRecipient = "GBCustoms-EMCS";

			#region Input XML
			var responseXml = @"<ie818:IE818 xmlns=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Excise/MovementForTraderData/3"" xmlns:doc=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:DOC:V3.13"" xmlns:emcs=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:EMCS:V3.13"" xmlns:euc=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Excise/EmcsUkCodes/3"" xmlns:ie0=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE880:V3.13"" xmlns:ie1=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE825:V3.13"" xmlns:ie2=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE717:V3.13"" xmlns:ie3=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13"" xmlns:ie=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE934:V3.13"" xmlns:ie704uk=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Excise/ie704uk/3"" xmlns:ie801=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE801:V3.13"" xmlns:ie802=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE802:V3.13"" xmlns:ie803=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE803:V3.13"" xmlns:ie807=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE807:V3.13"" xmlns:ie810=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE810:V3.13"" xmlns:ie813=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE813:V3.13"" xmlns:ie818=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE818:V3.13"" xmlns:ie819=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE819:V3.13"" xmlns:ie829=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE829:V3.13"" xmlns:ie837=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE837:V3.13"" xmlns:ie839=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE839:V3.13"" xmlns:ie840=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE840:V3.13"" xmlns:ie871=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE871:V3.13"" xmlns:ie881=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE881:V3.13"" xmlns:ie905=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE905:V3.13"" xmlns:tcl=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TCL:V3.13"" xmlns:tms=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13"" xmlns:tns4=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Common/ControlDocument"" xmlns:tns5=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Excise/MovementForTraderData/3"" xmlns:tns=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Excise/NewMessagesData/3"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<ie818:Header>
		<tms:MessageSender>NDEA.XI</tms:MessageSender>
		<tms:MessageRecipient>NDEA.XI</tms:MessageRecipient>
		<tms:DateOfPreparation>2006-08-04</tms:DateOfPreparation>
		<tms:TimeOfPreparation>09:43:40</tms:TimeOfPreparation>
		<tms:MessageIdentifier>XI0000015</tms:MessageIdentifier>
		<tms:CorrelationIdentifier>87</tms:CorrelationIdentifier>
	</ie818:Header>
	<ie818:Body>
		<ie818:AcceptedOrRejectedReportOfReceiptExport>
			<ie818:Attributes>
				<ie818:DateAndTimeOfValidationOfReportOfReceiptExport>2001-01-03T11:25:01</ie818:DateAndTimeOfValidationOfReportOfReceiptExport>
			</ie818:Attributes>
			<ie818:ConsigneeTrader language=""to"">
				<ie818:Traderid>GBWK002901025</ie818:Traderid>
				<ie818:TraderName>token</ie818:TraderName>
				<ie818:StreetName>token</ie818:StreetName>
				<ie818:StreetNumber>token</ie818:StreetNumber>
				<ie818:Postcode>token</ie818:Postcode>
				<ie818:City>token</ie818:City>
				<ie818:EoriNumber>token</ie818:EoriNumber>
			</ie818:ConsigneeTrader>
			<ie818:ExciseMovement>
				<ie818:AdministrativeReferenceCode>23XI00000000000000014</ie818:AdministrativeReferenceCode>
				<ie818:SequenceNumber>1</ie818:SequenceNumber>
			</ie818:ExciseMovement>
			<ie818:DeliveryPlaceTrader language=""to"">
				<ie818:Traderid>token</ie818:Traderid>
				<ie818:TraderName>token</ie818:TraderName>
				<ie818:StreetName>token</ie818:StreetName>
				<ie818:StreetNumber>token</ie818:StreetNumber>
				<ie818:Postcode>token</ie818:Postcode>
				<ie818:City>token</ie818:City>
			</ie818:DeliveryPlaceTrader>
			<ie818:DestinationOffice>
				<ie818:ReferenceNumber>GB005045</ie818:ReferenceNumber>
			</ie818:DestinationOffice>
			<ie818:ReportOfReceiptExport>
				<ie818:DateOfArrivalOfExciseProducts>2014-01-10</ie818:DateOfArrivalOfExciseProducts>
				<ie818:GlobalConclusionOfReceipt>22</ie818:GlobalConclusionOfReceipt>
				<ie818:ComplementaryInformation language=""to"">token</ie818:ComplementaryInformation>
			</ie818:ReportOfReceiptExport>
			<ie818:BodyReportOfReceiptExport>
				<ie818:BodyRecordUniqueReference>123</ie818:BodyRecordUniqueReference>
				<ie818:IndicatorOfShortageOrExcess>S</ie818:IndicatorOfShortageOrExcess>
				<ie818:ObservedShortageOrExcess>1000.0</ie818:ObservedShortageOrExcess>
				<ie818:ExciseProductCode>toke</ie818:ExciseProductCode>
				<ie818:RefusedQuantity>1000.0</ie818:RefusedQuantity>
				<ie818:UnsatisfactoryReason>
					<ie818:UnsatisfactoryReasonCode>12</ie818:UnsatisfactoryReasonCode>
					<ie818:ComplementaryInformation language=""to"">token</ie818:ComplementaryInformation>
				</ie818:UnsatisfactoryReason>
			</ie818:BodyReportOfReceiptExport>
		</ie818:AcceptedOrRejectedReportOfReceiptExport>
	</ie818:Body>
</ie818:IE818>";
			#endregion

			#region Expected XML
			var expectedXml = @"<CompositeOperation>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage"">
    <Rows>
      <eHubInboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EI_PK>00000000-0000-0000-0000-000000000000</EI_PK>
        <EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID>
        <EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
        <EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender>
        <EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
        <EI_MessageType>http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse</EI_MessageType>
        <EI_IsFlatFile>false</EI_IsFlatFile>
        <EI_ApplicationCode>CDS</EI_ApplicationCode>
        <EI_Status>2</EI_Status>
        <EI_Content>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</EI_Content>
      </eHubInboxMessage>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent"">
    <Rows>
      <eHubInboxXmlContent xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox>
        <EX_XmlContent>&lt;GBCustomsBusinessResponse&gt;
  &lt;ResponseHeader Provider=""EMCS""&gt;
    &lt;eHubTrackingID&gt;77777777-7777-7777-7777-777777777777&lt;/eHubTrackingID&gt;
    &lt;JobNumber&gt;mockRequestID&lt;/JobNumber&gt;
    &lt;ServiceReference&gt;mockServiceReference&lt;/ServiceReference&gt;
  &lt;/ResponseHeader&gt;
  &lt;ResponseBody ContentType=""XML"" Encoding=""Base64""&gt;
    PGllODE4OklFODE4IHhtbG5zPSJodHRwOi8vd3d3LmdvdnRhbGsuZ292LnVrL3RheGF0aW9uL0ludGVybmF0aW9uYWxUcmFkZS9FeGNpc2UvTW92ZW1lbnRGb3JUcmFkZXJEYXRhLzMiIHhtbG5zOmRvYz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpET0M6VjMuMTMiIHhtbG5zOmVtY3M9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6RU1DUzpWMy4xMyIgeG1sbnM6ZXVjPSJodHRwOi8vd3d3LmdvdnRhbGsuZ292LnVrL3RheGF0aW9uL0ludGVybmF0aW9uYWxUcmFkZS9FeGNpc2UvRW1jc1VrQ29kZXMvMyIgeG1sbnM6aWUwPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODgwOlYzLjEzIiB4bWxuczppZTE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MjU6VjMuMTMiIHhtbG5zOmllMj0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTcxNzpWMy4xMyIgeG1sbnM6aWUzPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODE1OlYzLjEzIiB4bWxuczppZT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTkzNDpWMy4xMyIgeG1sbnM6aWU3MDR1az0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvRXhjaXNlL2llNzA0dWsvMyIgeG1sbnM6aWU4MDE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MDE6VjMuMTMiIHhtbG5zOmllODAyPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODAyOlYzLjEzIiB4bWxuczppZTgwMz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgwMzpWMy4xMyIgeG1sbnM6aWU4MDc9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MDc6VjMuMTMiIHhtbG5zOmllODEwPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODEwOlYzLjEzIiB4bWxuczppZTgxMz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgxMzpWMy4xMyIgeG1sbnM6aWU4MTg9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MTg6VjMuMTMiIHhtbG5zOmllODE5PSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODE5OlYzLjEzIiB4bWxuczppZTgyOT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgyOTpWMy4xMyIgeG1sbnM6aWU4Mzc9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4Mzc6VjMuMTMiIHhtbG5zOmllODM5PSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODM5OlYzLjEzIiB4bWxuczppZTg0MD0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTg0MDpWMy4xMyIgeG1sbnM6aWU4NzE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4NzE6VjMuMTMiIHhtbG5zOmllODgxPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODgxOlYzLjEzIiB4bWxuczppZTkwNT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTkwNTpWMy4xMyIgeG1sbnM6dGNsPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OlRDTDpWMy4xMyIgeG1sbnM6dG1zPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OlRNUzpWMy4xMyIgeG1sbnM6dG5zND0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvQ29tbW9uL0NvbnRyb2xEb2N1bWVudCIgeG1sbnM6dG5zNT0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvRXhjaXNlL01vdmVtZW50Rm9yVHJhZGVyRGF0YS8zIiB4bWxuczp0bnM9Imh0dHA6Ly93d3cuZ292dGFsay5nb3YudWsvdGF4YXRpb24vSW50ZXJuYXRpb25hbFRyYWRlL0V4Y2lzZS9OZXdNZXNzYWdlc0RhdGEvMyIgeG1sbnM6eHM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIj4NCgk8aWU4MTg6SGVhZGVyPg0KCQk8dG1zOk1lc3NhZ2VTZW5kZXI+TkRFQS5YSTwvdG1zOk1lc3NhZ2VTZW5kZXI+DQoJCTx0bXM6TWVzc2FnZVJlY2lwaWVudD5OREVBLlhJPC90bXM6TWVzc2FnZVJlY2lwaWVudD4NCgkJPHRtczpEYXRlT2ZQcmVwYXJhdGlvbj4yMDA2LTA4LTA0PC90bXM6RGF0ZU9mUHJlcGFyYXRpb24+DQoJCTx0bXM6VGltZU9mUHJlcGFyYXRpb24+MDk6NDM6NDA8L3RtczpUaW1lT2ZQcmVwYXJhdGlvbj4NCgkJPHRtczpNZXNzYWdlSWRlbnRpZmllcj5YSTAwMDAwMTU8L3RtczpNZXNzYWdlSWRlbnRpZmllcj4NCgkJPHRtczpDb3JyZWxhdGlvbklkZW50aWZpZXI+ODc8L3RtczpDb3JyZWxhdGlvbklkZW50aWZpZXI+DQoJPC9pZTgxODpIZWFkZXI+DQoJPGllODE4OkJvZHk+DQoJCTxpZTgxODpBY2NlcHRlZE9yUmVqZWN0ZWRSZXBvcnRPZlJlY2VpcHRFeHBvcnQ+DQoJCQk8aWU4MTg6QXR0cmlidXRlcz4NCgkJCQk8aWU4MTg6RGF0ZUFuZFRpbWVPZlZhbGlkYXRpb25PZlJlcG9ydE9mUmVjZWlwdEV4cG9ydD4yMDAxLTAxLTAzVDExOjI1OjAxPC9pZTgxODpEYXRlQW5kVGltZU9mVmFsaWRhdGlvbk9mUmVwb3J0T2ZSZWNlaXB0RXhwb3J0Pg0KCQkJPC9pZTgxODpBdHRyaWJ1dGVzPg0KCQkJPGllODE4OkNvbnNpZ25lZVRyYWRlciBsYW5ndWFnZT0idG8iPg0KCQkJCTxpZTgxODpUcmFkZXJpZD5HQldLMDAyOTAxMDI1PC9pZTgxODpUcmFkZXJpZD4NCgkJCQk8aWU4MTg6VHJhZGVyTmFtZT50b2tlbjwvaWU4MTg6VHJhZGVyTmFtZT4NCgkJCQk8aWU4MTg6U3RyZWV0TmFtZT50b2tlbjwvaWU4MTg6U3RyZWV0TmFtZT4NCgkJCQk8aWU4MTg6U3RyZWV0TnVtYmVyPnRva2VuPC9pZTgxODpTdHJlZXROdW1iZXI+DQoJCQkJPGllODE4OlBvc3Rjb2RlPnRva2VuPC9pZTgxODpQb3N0Y29kZT4NCgkJCQk8aWU4MTg6Q2l0eT50b2tlbjwvaWU4MTg6Q2l0eT4NCgkJCQk8aWU4MTg6RW9yaU51bWJlcj50b2tlbjwvaWU4MTg6RW9yaU51bWJlcj4NCgkJCTwvaWU4MTg6Q29uc2lnbmVlVHJhZGVyPg0KCQkJPGllODE4OkV4Y2lzZU1vdmVtZW50Pg0KCQkJCTxpZTgxODpBZG1pbmlzdHJhdGl2ZVJlZmVyZW5jZUNvZGU+MjNYSTAwMDAwMDAwMDAwMDAwMDE0PC9pZTgxODpBZG1pbmlzdHJhdGl2ZVJlZmVyZW5jZUNvZGU+DQoJCQkJPGllODE4OlNlcXVlbmNlTnVtYmVyPjE8L2llODE4OlNlcXVlbmNlTnVtYmVyPg0KCQkJPC9pZTgxODpFeGNpc2VNb3ZlbWVudD4NCgkJCTxpZTgxODpEZWxpdmVyeVBsYWNlVHJhZGVyIGxhbmd1YWdlPSJ0byI+DQoJCQkJPGllODE4OlRyYWRlcmlkPnRva2VuPC9pZTgxODpUcmFkZXJpZD4NCgkJCQk8aWU4MTg6VHJhZGVyTmFtZT50b2tlbjwvaWU4MTg6VHJhZGVyTmFtZT4NCgkJCQk8aWU4MTg6U3RyZWV0TmFtZT50b2tlbjwvaWU4MTg6U3RyZWV0TmFtZT4NCgkJCQk8aWU4MTg6U3RyZWV0TnVtYmVyPnRva2VuPC9pZTgxODpTdHJlZXROdW1iZXI+DQoJCQkJPGllODE4OlBvc3Rjb2RlPnRva2VuPC9pZTgxODpQb3N0Y29kZT4NCgkJCQk8aWU4MTg6Q2l0eT50b2tlbjwvaWU4MTg6Q2l0eT4NCgkJCTwvaWU4MTg6RGVsaXZlcnlQbGFjZVRyYWRlcj4NCgkJCTxpZTgxODpEZXN0aW5hdGlvbk9mZmljZT4NCgkJCQk8aWU4MTg6UmVmZXJlbmNlTnVtYmVyPkdCMDA1MDQ1PC9pZTgxODpSZWZlcmVuY2VOdW1iZXI+DQoJCQk8L2llODE4OkRlc3RpbmF0aW9uT2ZmaWNlPg0KCQkJPGllODE4OlJlcG9ydE9mUmVjZWlwdEV4cG9ydD4NCgkJCQk8aWU4MTg6RGF0ZU9mQXJyaXZhbE9mRXhjaXNlUHJvZHVjdHM+MjAxNC0wMS0xMDwvaWU4MTg6RGF0ZU9mQXJyaXZhbE9mRXhjaXNlUHJvZHVjdHM+DQoJCQkJPGllODE4Okdsb2JhbENvbmNsdXNpb25PZlJlY2VpcHQ+MjI8L2llODE4Okdsb2JhbENvbmNsdXNpb25PZlJlY2VpcHQ+DQoJCQkJPGllODE4OkNvbXBsZW1lbnRhcnlJbmZvcm1hdGlvbiBsYW5ndWFnZT0idG8iPnRva2VuPC9pZTgxODpDb21wbGVtZW50YXJ5SW5mb3JtYXRpb24+DQoJCQk8L2llODE4OlJlcG9ydE9mUmVjZWlwdEV4cG9ydD4NCgkJCTxpZTgxODpCb2R5UmVwb3J0T2ZSZWNlaXB0RXhwb3J0Pg0KCQkJCTxpZTgxODpCb2R5UmVjb3JkVW5pcXVlUmVmZXJlbmNlPjEyMzwvaWU4MTg6Qm9keVJlY29yZFVuaXF1ZVJlZmVyZW5jZT4NCgkJCQk8aWU4MTg6SW5kaWNhdG9yT2ZTaG9ydGFnZU9yRXhjZXNzPlM8L2llODE4OkluZGljYXRvck9mU2hvcnRhZ2VPckV4Y2Vzcz4NCgkJCQk8aWU4MTg6T2JzZXJ2ZWRTaG9ydGFnZU9yRXhjZXNzPjEwMDAuMDwvaWU4MTg6T2JzZXJ2ZWRTaG9ydGFnZU9yRXhjZXNzPg0KCQkJCTxpZTgxODpFeGNpc2VQcm9kdWN0Q29kZT50b2tlPC9pZTgxODpFeGNpc2VQcm9kdWN0Q29kZT4NCgkJCQk8aWU4MTg6UmVmdXNlZFF1YW50aXR5PjEwMDAuMDwvaWU4MTg6UmVmdXNlZFF1YW50aXR5Pg0KCQkJCTxpZTgxODpVbnNhdGlzZmFjdG9yeVJlYXNvbj4NCgkJCQkJPGllODE4OlVuc2F0aXNmYWN0b3J5UmVhc29uQ29kZT4xMjwvaWU4MTg6VW5zYXRpc2ZhY3RvcnlSZWFzb25Db2RlPg0KCQkJCQk8aWU4MTg6Q29tcGxlbWVudGFyeUluZm9ybWF0aW9uIGxhbmd1YWdlPSJ0byI+dG9rZW48L2llODE4OkNvbXBsZW1lbnRhcnlJbmZvcm1hdGlvbj4NCgkJCQk8L2llODE4OlVuc2F0aXNmYWN0b3J5UmVhc29uPg0KCQkJPC9pZTgxODpCb2R5UmVwb3J0T2ZSZWNlaXB0RXhwb3J0Pg0KCQk8L2llODE4OkFjY2VwdGVkT3JSZWplY3RlZFJlcG9ydE9mUmVjZWlwdEV4cG9ydD4NCgk8L2llODE4OkJvZHk+DQo8L2llODE4OklFODE4Pg==
  &lt;/ResponseBody&gt;
&lt;/GBCustomsBusinessResponse&gt;</EX_XmlContent>
        <EX_DT_Source>12F1150E-4F3F-410F-990C-BAAB866EDC29</EX_DT_Source>
        <EX_UncompressedLength>6969</EX_UncompressedLength>
      </eHubInboxXmlContent>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage"">
    <Rows>
      <eHubOutboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <OI_PK>00000000-0000-0000-0000-000000000000</OI_PK>
        <OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID>
        <OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK>
        <OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender>
        <OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
        <OI_DT_Target>12F1150E-4F3F-410F-990C-BAAB866EDC29</OI_DT_Target>
        <OI_Status>0</OI_Status>
        <OI_Content>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</OI_Content>
        <OI_XmlContent>&lt;GBCustomsBusinessResponse&gt;
  &lt;ResponseHeader Provider=""EMCS""&gt;
    &lt;eHubTrackingID&gt;77777777-7777-7777-7777-777777777777&lt;/eHubTrackingID&gt;
    &lt;JobNumber&gt;mockRequestID&lt;/JobNumber&gt;
    &lt;ServiceReference&gt;mockServiceReference&lt;/ServiceReference&gt;
  &lt;/ResponseHeader&gt;
  &lt;ResponseBody ContentType=""XML"" Encoding=""Base64""&gt;
    PGllODE4OklFODE4IHhtbG5zPSJodHRwOi8vd3d3LmdvdnRhbGsuZ292LnVrL3RheGF0aW9uL0ludGVybmF0aW9uYWxUcmFkZS9FeGNpc2UvTW92ZW1lbnRGb3JUcmFkZXJEYXRhLzMiIHhtbG5zOmRvYz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpET0M6VjMuMTMiIHhtbG5zOmVtY3M9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6RU1DUzpWMy4xMyIgeG1sbnM6ZXVjPSJodHRwOi8vd3d3LmdvdnRhbGsuZ292LnVrL3RheGF0aW9uL0ludGVybmF0aW9uYWxUcmFkZS9FeGNpc2UvRW1jc1VrQ29kZXMvMyIgeG1sbnM6aWUwPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODgwOlYzLjEzIiB4bWxuczppZTE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MjU6VjMuMTMiIHhtbG5zOmllMj0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTcxNzpWMy4xMyIgeG1sbnM6aWUzPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODE1OlYzLjEzIiB4bWxuczppZT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTkzNDpWMy4xMyIgeG1sbnM6aWU3MDR1az0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvRXhjaXNlL2llNzA0dWsvMyIgeG1sbnM6aWU4MDE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MDE6VjMuMTMiIHhtbG5zOmllODAyPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODAyOlYzLjEzIiB4bWxuczppZTgwMz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgwMzpWMy4xMyIgeG1sbnM6aWU4MDc9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MDc6VjMuMTMiIHhtbG5zOmllODEwPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODEwOlYzLjEzIiB4bWxuczppZTgxMz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgxMzpWMy4xMyIgeG1sbnM6aWU4MTg9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MTg6VjMuMTMiIHhtbG5zOmllODE5PSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODE5OlYzLjEzIiB4bWxuczppZTgyOT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgyOTpWMy4xMyIgeG1sbnM6aWU4Mzc9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4Mzc6VjMuMTMiIHhtbG5zOmllODM5PSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODM5OlYzLjEzIiB4bWxuczppZTg0MD0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTg0MDpWMy4xMyIgeG1sbnM6aWU4NzE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4NzE6VjMuMTMiIHhtbG5zOmllODgxPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODgxOlYzLjEzIiB4bWxuczppZTkwNT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTkwNTpWMy4xMyIgeG1sbnM6dGNsPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OlRDTDpWMy4xMyIgeG1sbnM6dG1zPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OlRNUzpWMy4xMyIgeG1sbnM6dG5zND0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvQ29tbW9uL0NvbnRyb2xEb2N1bWVudCIgeG1sbnM6dG5zNT0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvRXhjaXNlL01vdmVtZW50Rm9yVHJhZGVyRGF0YS8zIiB4bWxuczp0bnM9Imh0dHA6Ly93d3cuZ292dGFsay5nb3YudWsvdGF4YXRpb24vSW50ZXJuYXRpb25hbFRyYWRlL0V4Y2lzZS9OZXdNZXNzYWdlc0RhdGEvMyIgeG1sbnM6eHM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIj4NCgk8aWU4MTg6SGVhZGVyPg0KCQk8dG1zOk1lc3NhZ2VTZW5kZXI+TkRFQS5YSTwvdG1zOk1lc3NhZ2VTZW5kZXI+DQoJCTx0bXM6TWVzc2FnZVJlY2lwaWVudD5OREVBLlhJPC90bXM6TWVzc2FnZVJlY2lwaWVudD4NCgkJPHRtczpEYXRlT2ZQcmVwYXJhdGlvbj4yMDA2LTA4LTA0PC90bXM6RGF0ZU9mUHJlcGFyYXRpb24+DQoJCTx0bXM6VGltZU9mUHJlcGFyYXRpb24+MDk6NDM6NDA8L3RtczpUaW1lT2ZQcmVwYXJhdGlvbj4NCgkJPHRtczpNZXNzYWdlSWRlbnRpZmllcj5YSTAwMDAwMTU8L3RtczpNZXNzYWdlSWRlbnRpZmllcj4NCgkJPHRtczpDb3JyZWxhdGlvbklkZW50aWZpZXI+ODc8L3RtczpDb3JyZWxhdGlvbklkZW50aWZpZXI+DQoJPC9pZTgxODpIZWFkZXI+DQoJPGllODE4OkJvZHk+DQoJCTxpZTgxODpBY2NlcHRlZE9yUmVqZWN0ZWRSZXBvcnRPZlJlY2VpcHRFeHBvcnQ+DQoJCQk8aWU4MTg6QXR0cmlidXRlcz4NCgkJCQk8aWU4MTg6RGF0ZUFuZFRpbWVPZlZhbGlkYXRpb25PZlJlcG9ydE9mUmVjZWlwdEV4cG9ydD4yMDAxLTAxLTAzVDExOjI1OjAxPC9pZTgxODpEYXRlQW5kVGltZU9mVmFsaWRhdGlvbk9mUmVwb3J0T2ZSZWNlaXB0RXhwb3J0Pg0KCQkJPC9pZTgxODpBdHRyaWJ1dGVzPg0KCQkJPGllODE4OkNvbnNpZ25lZVRyYWRlciBsYW5ndWFnZT0idG8iPg0KCQkJCTxpZTgxODpUcmFkZXJpZD5HQldLMDAyOTAxMDI1PC9pZTgxODpUcmFkZXJpZD4NCgkJCQk8aWU4MTg6VHJhZGVyTmFtZT50b2tlbjwvaWU4MTg6VHJhZGVyTmFtZT4NCgkJCQk8aWU4MTg6U3RyZWV0TmFtZT50b2tlbjwvaWU4MTg6U3RyZWV0TmFtZT4NCgkJCQk8aWU4MTg6U3RyZWV0TnVtYmVyPnRva2VuPC9pZTgxODpTdHJlZXROdW1iZXI+DQoJCQkJPGllODE4OlBvc3Rjb2RlPnRva2VuPC9pZTgxODpQb3N0Y29kZT4NCgkJCQk8aWU4MTg6Q2l0eT50b2tlbjwvaWU4MTg6Q2l0eT4NCgkJCQk8aWU4MTg6RW9yaU51bWJlcj50b2tlbjwvaWU4MTg6RW9yaU51bWJlcj4NCgkJCTwvaWU4MTg6Q29uc2lnbmVlVHJhZGVyPg0KCQkJPGllODE4OkV4Y2lzZU1vdmVtZW50Pg0KCQkJCTxpZTgxODpBZG1pbmlzdHJhdGl2ZVJlZmVyZW5jZUNvZGU+MjNYSTAwMDAwMDAwMDAwMDAwMDE0PC9pZTgxODpBZG1pbmlzdHJhdGl2ZVJlZmVyZW5jZUNvZGU+DQoJCQkJPGllODE4OlNlcXVlbmNlTnVtYmVyPjE8L2llODE4OlNlcXVlbmNlTnVtYmVyPg0KCQkJPC9pZTgxODpFeGNpc2VNb3ZlbWVudD4NCgkJCTxpZTgxODpEZWxpdmVyeVBsYWNlVHJhZGVyIGxhbmd1YWdlPSJ0byI+DQoJCQkJPGllODE4OlRyYWRlcmlkPnRva2VuPC9pZTgxODpUcmFkZXJpZD4NCgkJCQk8aWU4MTg6VHJhZGVyTmFtZT50b2tlbjwvaWU4MTg6VHJhZGVyTmFtZT4NCgkJCQk8aWU4MTg6U3RyZWV0TmFtZT50b2tlbjwvaWU4MTg6U3RyZWV0TmFtZT4NCgkJCQk8aWU4MTg6U3RyZWV0TnVtYmVyPnRva2VuPC9pZTgxODpTdHJlZXROdW1iZXI+DQoJCQkJPGllODE4OlBvc3Rjb2RlPnRva2VuPC9pZTgxODpQb3N0Y29kZT4NCgkJCQk8aWU4MTg6Q2l0eT50b2tlbjwvaWU4MTg6Q2l0eT4NCgkJCTwvaWU4MTg6RGVsaXZlcnlQbGFjZVRyYWRlcj4NCgkJCTxpZTgxODpEZXN0aW5hdGlvbk9mZmljZT4NCgkJCQk8aWU4MTg6UmVmZXJlbmNlTnVtYmVyPkdCMDA1MDQ1PC9pZTgxODpSZWZlcmVuY2VOdW1iZXI+DQoJCQk8L2llODE4OkRlc3RpbmF0aW9uT2ZmaWNlPg0KCQkJPGllODE4OlJlcG9ydE9mUmVjZWlwdEV4cG9ydD4NCgkJCQk8aWU4MTg6RGF0ZU9mQXJyaXZhbE9mRXhjaXNlUHJvZHVjdHM+MjAxNC0wMS0xMDwvaWU4MTg6RGF0ZU9mQXJyaXZhbE9mRXhjaXNlUHJvZHVjdHM+DQoJCQkJPGllODE4Okdsb2JhbENvbmNsdXNpb25PZlJlY2VpcHQ+MjI8L2llODE4Okdsb2JhbENvbmNsdXNpb25PZlJlY2VpcHQ+DQoJCQkJPGllODE4OkNvbXBsZW1lbnRhcnlJbmZvcm1hdGlvbiBsYW5ndWFnZT0idG8iPnRva2VuPC9pZTgxODpDb21wbGVtZW50YXJ5SW5mb3JtYXRpb24+DQoJCQk8L2llODE4OlJlcG9ydE9mUmVjZWlwdEV4cG9ydD4NCgkJCTxpZTgxODpCb2R5UmVwb3J0T2ZSZWNlaXB0RXhwb3J0Pg0KCQkJCTxpZTgxODpCb2R5UmVjb3JkVW5pcXVlUmVmZXJlbmNlPjEyMzwvaWU4MTg6Qm9keVJlY29yZFVuaXF1ZVJlZmVyZW5jZT4NCgkJCQk8aWU4MTg6SW5kaWNhdG9yT2ZTaG9ydGFnZU9yRXhjZXNzPlM8L2llODE4OkluZGljYXRvck9mU2hvcnRhZ2VPckV4Y2Vzcz4NCgkJCQk8aWU4MTg6T2JzZXJ2ZWRTaG9ydGFnZU9yRXhjZXNzPjEwMDAuMDwvaWU4MTg6T2JzZXJ2ZWRTaG9ydGFnZU9yRXhjZXNzPg0KCQkJCTxpZTgxODpFeGNpc2VQcm9kdWN0Q29kZT50b2tlPC9pZTgxODpFeGNpc2VQcm9kdWN0Q29kZT4NCgkJCQk8aWU4MTg6UmVmdXNlZFF1YW50aXR5PjEwMDAuMDwvaWU4MTg6UmVmdXNlZFF1YW50aXR5Pg0KCQkJCTxpZTgxODpVbnNhdGlzZmFjdG9yeVJlYXNvbj4NCgkJCQkJPGllODE4OlVuc2F0aXNmYWN0b3J5UmVhc29uQ29kZT4xMjwvaWU4MTg6VW5zYXRpc2ZhY3RvcnlSZWFzb25Db2RlPg0KCQkJCQk8aWU4MTg6Q29tcGxlbWVudGFyeUluZm9ybWF0aW9uIGxhbmd1YWdlPSJ0byI+dG9rZW48L2llODE4OkNvbXBsZW1lbnRhcnlJbmZvcm1hdGlvbj4NCgkJCQk8L2llODE4OlVuc2F0aXNmYWN0b3J5UmVhc29uPg0KCQkJPC9pZTgxODpCb2R5UmVwb3J0T2ZSZWNlaXB0RXhwb3J0Pg0KCQk8L2llODE4OkFjY2VwdGVkT3JSZWplY3RlZFJlcG9ydE9mUmVjZWlwdEV4cG9ydD4NCgk8L2llODE4OkJvZHk+DQo8L2llODE4OklFODE4Pg==
  &lt;/ResponseBody&gt;
&lt;/GBCustomsBusinessResponse&gt;</OI_XmlContent>
      </eHubOutboxMessage>
    </Rows>
  </Insert>
</CompositeOperation>";
			#endregion

			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "Unsolicited", "WTLDTWJLI", responseXml, null,
				gbCustomsXmlEmcs, true, "77777777-7777-7777-7777-777777777777", "mockServiceReference", "mockRequestID");

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_EmcsPollingResponse_Success()
		{
			var originalRecipient = "GBCustoms-EMCS";

			#region Input XML
			var responseXml = "PGllODAxOklFODAxIHhtbG5zPSJodHRwOi8vd3d3LmdvdnRhbGsuZ292LnVrL3RheGF0aW9uL0ludGVybmF0aW9uYWxUcmFkZS9FeGNpc2UvTW92ZW1lbnRGb3JUcmFkZXJEYXRhLzMiIHhtbG5zOmRvYz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpET0M6VjMuMTMiIHhtbG5zOmVtY3M9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6RU1DUzpWMy4xMyIgeG1sbnM6ZXVjPSJodHRwOi8vd3d3LmdvdnRhbGsuZ292LnVrL3RheGF0aW9uL0ludGVybmF0aW9uYWxUcmFkZS9FeGNpc2UvRW1jc1VrQ29kZXMvMyIgeG1sbnM6aWUwPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODgwOlYzLjEzIiB4bWxuczppZTE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MjU6VjMuMTMiIHhtbG5zOmllMj0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTcxNzpWMy4xMyIgeG1sbnM6aWUzPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODE1OlYzLjEzIiB4bWxuczppZT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTkzNDpWMy4xMyIgeG1sbnM6aWU3MDR1az0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvRXhjaXNlL2llNzA0dWsvMyIgeG1sbnM6aWU4MDE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MDE6VjMuMTMiIHhtbG5zOmllODAyPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODAyOlYzLjEzIiB4bWxuczppZTgwMz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgwMzpWMy4xMyIgeG1sbnM6aWU4MDc9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MDc6VjMuMTMiIHhtbG5zOmllODEwPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODEwOlYzLjEzIiB4bWxuczppZTgxMz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgxMzpWMy4xMyIgeG1sbnM6aWU4MTg9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MTg6VjMuMTMiIHhtbG5zOmllODE5PSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODE5OlYzLjEzIiB4bWxuczppZTgyOT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgyOTpWMy4xMyIgeG1sbnM6aWU4Mzc9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4Mzc6VjMuMTMiIHhtbG5zOmllODM5PSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODM5OlYzLjEzIiB4bWxuczppZTg0MD0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTg0MDpWMy4xMyIgeG1sbnM6aWU4NzE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4NzE6VjMuMTMiIHhtbG5zOmllODgxPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODgxOlYzLjEzIiB4bWxuczppZTkwNT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTkwNTpWMy4xMyIgeG1sbnM6dGNsPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OlRDTDpWMy4xMyIgeG1sbnM6dG1zPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OlRNUzpWMy4xMyIgeG1sbnM6dG5zND0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvQ29tbW9uL0NvbnRyb2xEb2N1bWVudCIgeG1sbnM6dG5zNT0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvRXhjaXNlL01vdmVtZW50Rm9yVHJhZGVyRGF0YS8zIiB4bWxuczp0bnM9Imh0dHA6Ly93d3cuZ292dGFsay5nb3YudWsvdGF4YXRpb24vSW50ZXJuYXRpb25hbFRyYWRlL0V4Y2lzZS9OZXdNZXNzYWdlc0RhdGEvMyIgeG1sbnM6eHM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIj48aWU4MDE6SGVhZGVyPjx0bXM6TWVzc2FnZVNlbmRlcj5OREVBLlhJPC90bXM6TWVzc2FnZVNlbmRlcj48dG1zOk1lc3NhZ2VSZWNpcGllbnQ+TkRFQS5BVDwvdG1zOk1lc3NhZ2VSZWNpcGllbnQ+PHRtczpEYXRlT2ZQcmVwYXJhdGlvbj4yMDIzLTA2LTIyPC90bXM6RGF0ZU9mUHJlcGFyYXRpb24+PHRtczpUaW1lT2ZQcmVwYXJhdGlvbj4xMjozNzowOC43NTU8L3RtczpUaW1lT2ZQcmVwYXJhdGlvbj48dG1zOk1lc3NhZ2VJZGVudGlmaWVyPlhJMDAwMDAzPC90bXM6TWVzc2FnZUlkZW50aWZpZXI+PHRtczpDb3JyZWxhdGlvbklkZW50aWZpZXI+ODc8L3RtczpDb3JyZWxhdGlvbklkZW50aWZpZXI+PC9pZTgwMTpIZWFkZXI+PGllODAxOkJvZHk+PGllODAxOkVBREVTQURDb250YWluZXI+PGllODAxOkNvbnNpZ25lZVRyYWRlciBsYW5ndWFnZT0iZW4iPjxpZTgwMTpUcmFkZXJpZD5BVDAwMDAwNjAyMDgwPC9pZTgwMTpUcmFkZXJpZD48aWU4MDE6VHJhZGVyTmFtZT5BRk9SIEtBTEUgTFREPC9pZTgwMTpUcmFkZXJOYW1lPjxpZTgwMTpTdHJlZXROYW1lPlRoZSBTdHJlZXQ8L2llODAxOlN0cmVldE5hbWU+PGllODAxOlBvc3Rjb2RlPkFUMTIzPC9pZTgwMTpQb3N0Y29kZT48aWU4MDE6Q2l0eT5UaGUgQ2l0eTwvaWU4MDE6Q2l0eT48L2llODAxOkNvbnNpZ25lZVRyYWRlcj48aWU4MDE6RXhjaXNlTW92ZW1lbnQ+PGllODAxOkFkbWluaXN0cmF0aXZlUmVmZXJlbmNlQ29kZT4yM1hJMDAwMDAwMDAwMDAwMDAwMTQ8L2llODAxOkFkbWluaXN0cmF0aXZlUmVmZXJlbmNlQ29kZT48aWU4MDE6RGF0ZUFuZFRpbWVPZlZhbGlkYXRpb25PZkVhZEVzYWQ+MjAyMy0wNi0yMlQxMTozNzoxMC4zNDU3MzkzOTY8L2llODAxOkRhdGVBbmRUaW1lT2ZWYWxpZGF0aW9uT2ZFYWRFc2FkPjwvaWU4MDE6RXhjaXNlTW92ZW1lbnQ+PGllODAxOkNvbnNpZ25vclRyYWRlciBsYW5ndWFnZT0iZW4iPjxpZTgwMTpUcmFkZXJFeGNpc2VOdW1iZXI+R0JXSzA4MDk3OTAwMDwvaWU4MDE6VHJhZGVyRXhjaXNlTnVtYmVyPjxpZTgwMTpUcmFkZXJOYW1lPkNsYXJreXMgRWFnbGVzPC9pZTgwMTpUcmFkZXJOYW1lPjxpZTgwMTpTdHJlZXROYW1lPkhhcHB5IFN0cmVldDwvaWU4MDE6U3RyZWV0TmFtZT48aWU4MDE6UG9zdGNvZGU+QlQxIDFCRzwvaWU4MDE6UG9zdGNvZGU+PGllODAxOkNpdHk+VGhlIENpdHk8L2llODAxOkNpdHk+PC9pZTgwMTpDb25zaWdub3JUcmFkZXI+PGllODAxOlBsYWNlT2ZEaXNwYXRjaFRyYWRlciBsYW5ndWFnZT0iZW4iPjxpZTgwMTpSZWZlcmVuY2VPZlRheFdhcmVob3VzZT5YSTAwMDAwNDY3MDE0PC9pZTgwMTpSZWZlcmVuY2VPZlRheFdhcmVob3VzZT48L2llODAxOlBsYWNlT2ZEaXNwYXRjaFRyYWRlcj48aWU4MDE6RGVsaXZlcnlQbGFjZVRyYWRlciBsYW5ndWFnZT0iZW4iPjxpZTgwMTpUcmFkZXJpZD5BVDAwMDAwNjAyMDc4PC9pZTgwMTpUcmFkZXJpZD48aWU4MDE6VHJhZGVyTmFtZT5NRVRFU1QgQk9ORCBTVFRTVEdFPC9pZTgwMTpUcmFkZXJOYW1lPjxpZTgwMTpTdHJlZXROYW1lPldISVRFVEVTVCBST0FEIE1FVEVTVCBDSVRZIEVTVEFURTwvaWU4MDE6U3RyZWV0TmFtZT48aWU4MDE6UG9zdGNvZGU+Qk4yIDRLWDwvaWU4MDE6UG9zdGNvZGU+PGllODAxOkNpdHk+U1RURVNULEtFTlQ8L2llODAxOkNpdHk+PC9pZTgwMTpEZWxpdmVyeVBsYWNlVHJhZGVyPjxpZTgwMTpDb21wZXRlbnRBdXRob3JpdHlEaXNwYXRjaE9mZmljZT48aWU4MDE6UmVmZXJlbmNlTnVtYmVyPkdCMDA0MDk4PC9pZTgwMTpSZWZlcmVuY2VOdW1iZXI+PC9pZTgwMTpDb21wZXRlbnRBdXRob3JpdHlEaXNwYXRjaE9mZmljZT48aWU4MDE6RWFkRXNhZD48aWU4MDE6TG9jYWxSZWZlcmVuY2VOdW1iZXI+bHJuaWU4MTU1OTczODEyPC9pZTgwMTpMb2NhbFJlZmVyZW5jZU51bWJlcj48aWU4MDE6SW52b2ljZU51bWJlcj5JTlZPSUNFMDAxPC9pZTgwMTpJbnZvaWNlTnVtYmVyPjxpZTgwMTpJbnZvaWNlRGF0ZT4yMDE4LTA0LTA0PC9pZTgwMTpJbnZvaWNlRGF0ZT48aWU4MDE6T3JpZ2luVHlwZUNvZGU+MTwvaWU4MDE6T3JpZ2luVHlwZUNvZGU+PGllODAxOkRhdGVPZkRpc3BhdGNoPjIwMjEtMTItMDI8L2llODAxOkRhdGVPZkRpc3BhdGNoPjxpZTgwMTpUaW1lT2ZEaXNwYXRjaD4yMjozNzowMDwvaWU4MDE6VGltZU9mRGlzcGF0Y2g+PC9pZTgwMTpFYWRFc2FkPjxpZTgwMTpIZWFkZXJFYWRFc2FkPjxpZTgwMTpTZXF1ZW5jZU51bWJlcj4xPC9pZTgwMTpTZXF1ZW5jZU51bWJlcj48aWU4MDE6RGF0ZUFuZFRpbWVPZlVwZGF0ZVZhbGlkYXRpb24+MjAyMy0wNi0yMlQxMTozNzoxMC4zNDU4MDEwMjk8L2llODAxOkRhdGVBbmRUaW1lT2ZVcGRhdGVWYWxpZGF0aW9uPjxpZTgwMTpEZXN0aW5hdGlvblR5cGVDb2RlPjE8L2llODAxOkRlc3RpbmF0aW9uVHlwZUNvZGU+PGllODAxOkpvdXJuZXlUaW1lPkQwMTwvaWU4MDE6Sm91cm5leVRpbWU+PGllODAxOlRyYW5zcG9ydEFycmFuZ2VtZW50PjE8L2llODAxOlRyYW5zcG9ydEFycmFuZ2VtZW50PjwvaWU4MDE6SGVhZGVyRWFkRXNhZD48aWU4MDE6VHJhbnNwb3J0TW9kZT48aWU4MDE6VHJhbnNwb3J0TW9kZUNvZGU+MTwvaWU4MDE6VHJhbnNwb3J0TW9kZUNvZGU+PC9pZTgwMTpUcmFuc3BvcnRNb2RlPjxpZTgwMTpNb3ZlbWVudEd1YXJhbnRlZT48aWU4MDE6R3VhcmFudG9yVHlwZUNvZGU+MTwvaWU4MDE6R3VhcmFudG9yVHlwZUNvZGU+PC9pZTgwMTpNb3ZlbWVudEd1YXJhbnRlZT48aWU4MDE6Qm9keUVhZEVzYWQ+PGllODAxOkJvZHlSZWNvcmRVbmlxdWVSZWZlcmVuY2U+MTwvaWU4MDE6Qm9keVJlY29yZFVuaXF1ZVJlZmVyZW5jZT48aWU4MDE6RXhjaXNlUHJvZHVjdENvZGU+RTQxMDwvaWU4MDE6RXhjaXNlUHJvZHVjdENvZGU+PGllODAxOkNuQ29kZT4yNzEwMTIzMTwvaWU4MDE6Q25Db2RlPjxpZTgwMTpRdWFudGl0eT4xMDAuMDAwPC9pZTgwMTpRdWFudGl0eT48aWU4MDE6R3Jvc3NNYXNzPjEwMC4wMDwvaWU4MDE6R3Jvc3NNYXNzPjxpZTgwMTpOZXRNYXNzPjkwLjAwPC9pZTgwMTpOZXRNYXNzPjxpZTgwMTpEZW5zaXR5PjEwLjAwPC9pZTgwMTpEZW5zaXR5PjxpZTgwMTpQYWNrYWdlPjxpZTgwMTpLaW5kT2ZQYWNrYWdlcz5CSDwvaWU4MDE6S2luZE9mUGFja2FnZXM+PGllODAxOk51bWJlck9mUGFja2FnZXM+MjwvaWU4MDE6TnVtYmVyT2ZQYWNrYWdlcz48aWU4MDE6U2hpcHBpbmdNYXJrcz5TdWJoYXNpcyBTd2FpbjE8L2llODAxOlNoaXBwaW5nTWFya3M+PC9pZTgwMTpQYWNrYWdlPjxpZTgwMTpQYWNrYWdlPjxpZTgwMTpLaW5kT2ZQYWNrYWdlcz5CSDwvaWU4MDE6S2luZE9mUGFja2FnZXM+PGllODAxOk51bWJlck9mUGFja2FnZXM+MjwvaWU4MDE6TnVtYmVyT2ZQYWNrYWdlcz48aWU4MDE6U2hpcHBpbmdNYXJrcz5TdWJoYXNpcyBTd2FpbiAyPC9pZTgwMTpTaGlwcGluZ01hcmtzPjwvaWU4MDE6UGFja2FnZT48L2llODAxOkJvZHlFYWRFc2FkPjxpZTgwMTpUcmFuc3BvcnREZXRhaWxzPjxpZTgwMTpUcmFuc3BvcnRVbml0Q29kZT4xPC9pZTgwMTpUcmFuc3BvcnRVbml0Q29kZT48aWU4MDE6SWRlbnRpdHlPZlRyYW5zcG9ydFVuaXRzPlRyYW5zZm9ybWVycyByb2JvdHMgaW4gZGlzZ3Vpc2U8L2llODAxOklkZW50aXR5T2ZUcmFuc3BvcnRVbml0cz48L2llODAxOlRyYW5zcG9ydERldGFpbHM+PGllODAxOlRyYW5zcG9ydERldGFpbHM+PGllODAxOlRyYW5zcG9ydFVuaXRDb2RlPjI8L2llODAxOlRyYW5zcG9ydFVuaXRDb2RlPjxpZTgwMTpJZGVudGl0eU9mVHJhbnNwb3J0VW5pdHM+TUFDSElORVM8L2llODAxOklkZW50aXR5T2ZUcmFuc3BvcnRVbml0cz48L2llODAxOlRyYW5zcG9ydERldGFpbHM+PGllODAxOlRyYW5zcG9ydERldGFpbHM+PGllODAxOlRyYW5zcG9ydFVuaXRDb2RlPjM8L2llODAxOlRyYW5zcG9ydFVuaXRDb2RlPjxpZTgwMTpJZGVudGl0eU9mVHJhbnNwb3J0VW5pdHM+TU9SRSBNQUNISU5FUzwvaWU4MDE6SWRlbnRpdHlPZlRyYW5zcG9ydFVuaXRzPjwvaWU4MDE6VHJhbnNwb3J0RGV0YWlscz48L2llODAxOkVBREVTQURDb250YWluZXI+PC9pZTgwMTpCb2R5PjwvaWU4MDE6SUU4MDE+";
			#endregion

			#region Expected XML
			var expectedXml = @"<CompositeOperation>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage"">
    <Rows>
      <eHubInboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EI_PK>00000000-0000-0000-0000-000000000000</EI_PK>
        <EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID>
        <EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
        <EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender>
        <EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
        <EI_MessageType>http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse</EI_MessageType>
        <EI_IsFlatFile>false</EI_IsFlatFile>
        <EI_ApplicationCode>CDS</EI_ApplicationCode>
        <EI_Status>2</EI_Status>
        <EI_Content>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</EI_Content>
      </eHubInboxMessage>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent"">
    <Rows>
      <eHubInboxXmlContent xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox>
        <EX_XmlContent>&lt;GBCustomsBusinessResponse&gt;
  &lt;ResponseHeader Provider=""EMCS""&gt;
    &lt;eHubTrackingID&gt;77777777-7777-7777-7777-777777777777&lt;/eHubTrackingID&gt;
    &lt;JobNumber&gt;mockRequestID&lt;/JobNumber&gt;
    &lt;ServiceReference&gt;mockCorrelationID&lt;/ServiceReference&gt;
  &lt;/ResponseHeader&gt;
  &lt;ResponseBody ContentType=""XML"" Encoding=""Base64""&gt;
    PGllODAxOklFODAxIHhtbG5zPSJodHRwOi8vd3d3LmdvdnRhbGsuZ292LnVrL3RheGF0aW9uL0ludGVybmF0aW9uYWxUcmFkZS9FeGNpc2UvTW92ZW1lbnRGb3JUcmFkZXJEYXRhLzMiIHhtbG5zOmRvYz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpET0M6VjMuMTMiIHhtbG5zOmVtY3M9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6RU1DUzpWMy4xMyIgeG1sbnM6ZXVjPSJodHRwOi8vd3d3LmdvdnRhbGsuZ292LnVrL3RheGF0aW9uL0ludGVybmF0aW9uYWxUcmFkZS9FeGNpc2UvRW1jc1VrQ29kZXMvMyIgeG1sbnM6aWUwPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODgwOlYzLjEzIiB4bWxuczppZTE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MjU6VjMuMTMiIHhtbG5zOmllMj0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTcxNzpWMy4xMyIgeG1sbnM6aWUzPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODE1OlYzLjEzIiB4bWxuczppZT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTkzNDpWMy4xMyIgeG1sbnM6aWU3MDR1az0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvRXhjaXNlL2llNzA0dWsvMyIgeG1sbnM6aWU4MDE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MDE6VjMuMTMiIHhtbG5zOmllODAyPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODAyOlYzLjEzIiB4bWxuczppZTgwMz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgwMzpWMy4xMyIgeG1sbnM6aWU4MDc9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MDc6VjMuMTMiIHhtbG5zOmllODEwPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODEwOlYzLjEzIiB4bWxuczppZTgxMz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgxMzpWMy4xMyIgeG1sbnM6aWU4MTg9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MTg6VjMuMTMiIHhtbG5zOmllODE5PSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODE5OlYzLjEzIiB4bWxuczppZTgyOT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgyOTpWMy4xMyIgeG1sbnM6aWU4Mzc9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4Mzc6VjMuMTMiIHhtbG5zOmllODM5PSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODM5OlYzLjEzIiB4bWxuczppZTg0MD0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTg0MDpWMy4xMyIgeG1sbnM6aWU4NzE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4NzE6VjMuMTMiIHhtbG5zOmllODgxPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODgxOlYzLjEzIiB4bWxuczppZTkwNT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTkwNTpWMy4xMyIgeG1sbnM6dGNsPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OlRDTDpWMy4xMyIgeG1sbnM6dG1zPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OlRNUzpWMy4xMyIgeG1sbnM6dG5zND0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvQ29tbW9uL0NvbnRyb2xEb2N1bWVudCIgeG1sbnM6dG5zNT0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvRXhjaXNlL01vdmVtZW50Rm9yVHJhZGVyRGF0YS8zIiB4bWxuczp0bnM9Imh0dHA6Ly93d3cuZ292dGFsay5nb3YudWsvdGF4YXRpb24vSW50ZXJuYXRpb25hbFRyYWRlL0V4Y2lzZS9OZXdNZXNzYWdlc0RhdGEvMyIgeG1sbnM6eHM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIj48aWU4MDE6SGVhZGVyPjx0bXM6TWVzc2FnZVNlbmRlcj5OREVBLlhJPC90bXM6TWVzc2FnZVNlbmRlcj48dG1zOk1lc3NhZ2VSZWNpcGllbnQ+TkRFQS5BVDwvdG1zOk1lc3NhZ2VSZWNpcGllbnQ+PHRtczpEYXRlT2ZQcmVwYXJhdGlvbj4yMDIzLTA2LTIyPC90bXM6RGF0ZU9mUHJlcGFyYXRpb24+PHRtczpUaW1lT2ZQcmVwYXJhdGlvbj4xMjozNzowOC43NTU8L3RtczpUaW1lT2ZQcmVwYXJhdGlvbj48dG1zOk1lc3NhZ2VJZGVudGlmaWVyPlhJMDAwMDAzPC90bXM6TWVzc2FnZUlkZW50aWZpZXI+PHRtczpDb3JyZWxhdGlvbklkZW50aWZpZXI+ODc8L3RtczpDb3JyZWxhdGlvbklkZW50aWZpZXI+PC9pZTgwMTpIZWFkZXI+PGllODAxOkJvZHk+PGllODAxOkVBREVTQURDb250YWluZXI+PGllODAxOkNvbnNpZ25lZVRyYWRlciBsYW5ndWFnZT0iZW4iPjxpZTgwMTpUcmFkZXJpZD5BVDAwMDAwNjAyMDgwPC9pZTgwMTpUcmFkZXJpZD48aWU4MDE6VHJhZGVyTmFtZT5BRk9SIEtBTEUgTFREPC9pZTgwMTpUcmFkZXJOYW1lPjxpZTgwMTpTdHJlZXROYW1lPlRoZSBTdHJlZXQ8L2llODAxOlN0cmVldE5hbWU+PGllODAxOlBvc3Rjb2RlPkFUMTIzPC9pZTgwMTpQb3N0Y29kZT48aWU4MDE6Q2l0eT5UaGUgQ2l0eTwvaWU4MDE6Q2l0eT48L2llODAxOkNvbnNpZ25lZVRyYWRlcj48aWU4MDE6RXhjaXNlTW92ZW1lbnQ+PGllODAxOkFkbWluaXN0cmF0aXZlUmVmZXJlbmNlQ29kZT4yM1hJMDAwMDAwMDAwMDAwMDAwMTQ8L2llODAxOkFkbWluaXN0cmF0aXZlUmVmZXJlbmNlQ29kZT48aWU4MDE6RGF0ZUFuZFRpbWVPZlZhbGlkYXRpb25PZkVhZEVzYWQ+MjAyMy0wNi0yMlQxMTozNzoxMC4zNDU3MzkzOTY8L2llODAxOkRhdGVBbmRUaW1lT2ZWYWxpZGF0aW9uT2ZFYWRFc2FkPjwvaWU4MDE6RXhjaXNlTW92ZW1lbnQ+PGllODAxOkNvbnNpZ25vclRyYWRlciBsYW5ndWFnZT0iZW4iPjxpZTgwMTpUcmFkZXJFeGNpc2VOdW1iZXI+R0JXSzA4MDk3OTAwMDwvaWU4MDE6VHJhZGVyRXhjaXNlTnVtYmVyPjxpZTgwMTpUcmFkZXJOYW1lPkNsYXJreXMgRWFnbGVzPC9pZTgwMTpUcmFkZXJOYW1lPjxpZTgwMTpTdHJlZXROYW1lPkhhcHB5IFN0cmVldDwvaWU4MDE6U3RyZWV0TmFtZT48aWU4MDE6UG9zdGNvZGU+QlQxIDFCRzwvaWU4MDE6UG9zdGNvZGU+PGllODAxOkNpdHk+VGhlIENpdHk8L2llODAxOkNpdHk+PC9pZTgwMTpDb25zaWdub3JUcmFkZXI+PGllODAxOlBsYWNlT2ZEaXNwYXRjaFRyYWRlciBsYW5ndWFnZT0iZW4iPjxpZTgwMTpSZWZlcmVuY2VPZlRheFdhcmVob3VzZT5YSTAwMDAwNDY3MDE0PC9pZTgwMTpSZWZlcmVuY2VPZlRheFdhcmVob3VzZT48L2llODAxOlBsYWNlT2ZEaXNwYXRjaFRyYWRlcj48aWU4MDE6RGVsaXZlcnlQbGFjZVRyYWRlciBsYW5ndWFnZT0iZW4iPjxpZTgwMTpUcmFkZXJpZD5BVDAwMDAwNjAyMDc4PC9pZTgwMTpUcmFkZXJpZD48aWU4MDE6VHJhZGVyTmFtZT5NRVRFU1QgQk9ORCBTVFRTVEdFPC9pZTgwMTpUcmFkZXJOYW1lPjxpZTgwMTpTdHJlZXROYW1lPldISVRFVEVTVCBST0FEIE1FVEVTVCBDSVRZIEVTVEFURTwvaWU4MDE6U3RyZWV0TmFtZT48aWU4MDE6UG9zdGNvZGU+Qk4yIDRLWDwvaWU4MDE6UG9zdGNvZGU+PGllODAxOkNpdHk+U1RURVNULEtFTlQ8L2llODAxOkNpdHk+PC9pZTgwMTpEZWxpdmVyeVBsYWNlVHJhZGVyPjxpZTgwMTpDb21wZXRlbnRBdXRob3JpdHlEaXNwYXRjaE9mZmljZT48aWU4MDE6UmVmZXJlbmNlTnVtYmVyPkdCMDA0MDk4PC9pZTgwMTpSZWZlcmVuY2VOdW1iZXI+PC9pZTgwMTpDb21wZXRlbnRBdXRob3JpdHlEaXNwYXRjaE9mZmljZT48aWU4MDE6RWFkRXNhZD48aWU4MDE6TG9jYWxSZWZlcmVuY2VOdW1iZXI+bHJuaWU4MTU1OTczODEyPC9pZTgwMTpMb2NhbFJlZmVyZW5jZU51bWJlcj48aWU4MDE6SW52b2ljZU51bWJlcj5JTlZPSUNFMDAxPC9pZTgwMTpJbnZvaWNlTnVtYmVyPjxpZTgwMTpJbnZvaWNlRGF0ZT4yMDE4LTA0LTA0PC9pZTgwMTpJbnZvaWNlRGF0ZT48aWU4MDE6T3JpZ2luVHlwZUNvZGU+MTwvaWU4MDE6T3JpZ2luVHlwZUNvZGU+PGllODAxOkRhdGVPZkRpc3BhdGNoPjIwMjEtMTItMDI8L2llODAxOkRhdGVPZkRpc3BhdGNoPjxpZTgwMTpUaW1lT2ZEaXNwYXRjaD4yMjozNzowMDwvaWU4MDE6VGltZU9mRGlzcGF0Y2g+PC9pZTgwMTpFYWRFc2FkPjxpZTgwMTpIZWFkZXJFYWRFc2FkPjxpZTgwMTpTZXF1ZW5jZU51bWJlcj4xPC9pZTgwMTpTZXF1ZW5jZU51bWJlcj48aWU4MDE6RGF0ZUFuZFRpbWVPZlVwZGF0ZVZhbGlkYXRpb24+MjAyMy0wNi0yMlQxMTozNzoxMC4zNDU4MDEwMjk8L2llODAxOkRhdGVBbmRUaW1lT2ZVcGRhdGVWYWxpZGF0aW9uPjxpZTgwMTpEZXN0aW5hdGlvblR5cGVDb2RlPjE8L2llODAxOkRlc3RpbmF0aW9uVHlwZUNvZGU+PGllODAxOkpvdXJuZXlUaW1lPkQwMTwvaWU4MDE6Sm91cm5leVRpbWU+PGllODAxOlRyYW5zcG9ydEFycmFuZ2VtZW50PjE8L2llODAxOlRyYW5zcG9ydEFycmFuZ2VtZW50PjwvaWU4MDE6SGVhZGVyRWFkRXNhZD48aWU4MDE6VHJhbnNwb3J0TW9kZT48aWU4MDE6VHJhbnNwb3J0TW9kZUNvZGU+MTwvaWU4MDE6VHJhbnNwb3J0TW9kZUNvZGU+PC9pZTgwMTpUcmFuc3BvcnRNb2RlPjxpZTgwMTpNb3ZlbWVudEd1YXJhbnRlZT48aWU4MDE6R3VhcmFudG9yVHlwZUNvZGU+MTwvaWU4MDE6R3VhcmFudG9yVHlwZUNvZGU+PC9pZTgwMTpNb3ZlbWVudEd1YXJhbnRlZT48aWU4MDE6Qm9keUVhZEVzYWQ+PGllODAxOkJvZHlSZWNvcmRVbmlxdWVSZWZlcmVuY2U+MTwvaWU4MDE6Qm9keVJlY29yZFVuaXF1ZVJlZmVyZW5jZT48aWU4MDE6RXhjaXNlUHJvZHVjdENvZGU+RTQxMDwvaWU4MDE6RXhjaXNlUHJvZHVjdENvZGU+PGllODAxOkNuQ29kZT4yNzEwMTIzMTwvaWU4MDE6Q25Db2RlPjxpZTgwMTpRdWFudGl0eT4xMDAuMDAwPC9pZTgwMTpRdWFudGl0eT48aWU4MDE6R3Jvc3NNYXNzPjEwMC4wMDwvaWU4MDE6R3Jvc3NNYXNzPjxpZTgwMTpOZXRNYXNzPjkwLjAwPC9pZTgwMTpOZXRNYXNzPjxpZTgwMTpEZW5zaXR5PjEwLjAwPC9pZTgwMTpEZW5zaXR5PjxpZTgwMTpQYWNrYWdlPjxpZTgwMTpLaW5kT2ZQYWNrYWdlcz5CSDwvaWU4MDE6S2luZE9mUGFja2FnZXM+PGllODAxOk51bWJlck9mUGFja2FnZXM+MjwvaWU4MDE6TnVtYmVyT2ZQYWNrYWdlcz48aWU4MDE6U2hpcHBpbmdNYXJrcz5TdWJoYXNpcyBTd2FpbjE8L2llODAxOlNoaXBwaW5nTWFya3M+PC9pZTgwMTpQYWNrYWdlPjxpZTgwMTpQYWNrYWdlPjxpZTgwMTpLaW5kT2ZQYWNrYWdlcz5CSDwvaWU4MDE6S2luZE9mUGFja2FnZXM+PGllODAxOk51bWJlck9mUGFja2FnZXM+MjwvaWU4MDE6TnVtYmVyT2ZQYWNrYWdlcz48aWU4MDE6U2hpcHBpbmdNYXJrcz5TdWJoYXNpcyBTd2FpbiAyPC9pZTgwMTpTaGlwcGluZ01hcmtzPjwvaWU4MDE6UGFja2FnZT48L2llODAxOkJvZHlFYWRFc2FkPjxpZTgwMTpUcmFuc3BvcnREZXRhaWxzPjxpZTgwMTpUcmFuc3BvcnRVbml0Q29kZT4xPC9pZTgwMTpUcmFuc3BvcnRVbml0Q29kZT48aWU4MDE6SWRlbnRpdHlPZlRyYW5zcG9ydFVuaXRzPlRyYW5zZm9ybWVycyByb2JvdHMgaW4gZGlzZ3Vpc2U8L2llODAxOklkZW50aXR5T2ZUcmFuc3BvcnRVbml0cz48L2llODAxOlRyYW5zcG9ydERldGFpbHM+PGllODAxOlRyYW5zcG9ydERldGFpbHM+PGllODAxOlRyYW5zcG9ydFVuaXRDb2RlPjI8L2llODAxOlRyYW5zcG9ydFVuaXRDb2RlPjxpZTgwMTpJZGVudGl0eU9mVHJhbnNwb3J0VW5pdHM+TUFDSElORVM8L2llODAxOklkZW50aXR5T2ZUcmFuc3BvcnRVbml0cz48L2llODAxOlRyYW5zcG9ydERldGFpbHM+PGllODAxOlRyYW5zcG9ydERldGFpbHM+PGllODAxOlRyYW5zcG9ydFVuaXRDb2RlPjM8L2llODAxOlRyYW5zcG9ydFVuaXRDb2RlPjxpZTgwMTpJZGVudGl0eU9mVHJhbnNwb3J0VW5pdHM+TU9SRSBNQUNISU5FUzwvaWU4MDE6SWRlbnRpdHlPZlRyYW5zcG9ydFVuaXRzPjwvaWU4MDE6VHJhbnNwb3J0RGV0YWlscz48L2llODAxOkVBREVTQURDb250YWluZXI+PC9pZTgwMTpCb2R5PjwvaWU4MDE6SUU4MDE+
  &lt;/ResponseBody&gt;
&lt;/GBCustomsBusinessResponse&gt;</EX_XmlContent>
        <EX_DT_Source>12F1150E-4F3F-410F-990C-BAAB866EDC29</EX_DT_Source>
        <EX_UncompressedLength>8658</EX_UncompressedLength>
      </eHubInboxXmlContent>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage"">
    <Rows>
      <eHubOutboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <OI_PK>00000000-0000-0000-0000-000000000000</OI_PK>
        <OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID>
        <OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK>
        <OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender>
        <OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
        <OI_DT_Target>12F1150E-4F3F-410F-990C-BAAB866EDC29</OI_DT_Target>
        <OI_Status>0</OI_Status>
        <OI_Content>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</OI_Content>
        <OI_XmlContent>&lt;GBCustomsBusinessResponse&gt;
  &lt;ResponseHeader Provider=""EMCS""&gt;
    &lt;eHubTrackingID&gt;77777777-7777-7777-7777-777777777777&lt;/eHubTrackingID&gt;
    &lt;JobNumber&gt;mockRequestID&lt;/JobNumber&gt;
    &lt;ServiceReference&gt;mockCorrelationID&lt;/ServiceReference&gt;
  &lt;/ResponseHeader&gt;
  &lt;ResponseBody ContentType=""XML"" Encoding=""Base64""&gt;
    PGllODAxOklFODAxIHhtbG5zPSJodHRwOi8vd3d3LmdvdnRhbGsuZ292LnVrL3RheGF0aW9uL0ludGVybmF0aW9uYWxUcmFkZS9FeGNpc2UvTW92ZW1lbnRGb3JUcmFkZXJEYXRhLzMiIHhtbG5zOmRvYz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpET0M6VjMuMTMiIHhtbG5zOmVtY3M9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6RU1DUzpWMy4xMyIgeG1sbnM6ZXVjPSJodHRwOi8vd3d3LmdvdnRhbGsuZ292LnVrL3RheGF0aW9uL0ludGVybmF0aW9uYWxUcmFkZS9FeGNpc2UvRW1jc1VrQ29kZXMvMyIgeG1sbnM6aWUwPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODgwOlYzLjEzIiB4bWxuczppZTE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MjU6VjMuMTMiIHhtbG5zOmllMj0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTcxNzpWMy4xMyIgeG1sbnM6aWUzPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODE1OlYzLjEzIiB4bWxuczppZT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTkzNDpWMy4xMyIgeG1sbnM6aWU3MDR1az0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvRXhjaXNlL2llNzA0dWsvMyIgeG1sbnM6aWU4MDE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MDE6VjMuMTMiIHhtbG5zOmllODAyPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODAyOlYzLjEzIiB4bWxuczppZTgwMz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgwMzpWMy4xMyIgeG1sbnM6aWU4MDc9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MDc6VjMuMTMiIHhtbG5zOmllODEwPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODEwOlYzLjEzIiB4bWxuczppZTgxMz0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgxMzpWMy4xMyIgeG1sbnM6aWU4MTg9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4MTg6VjMuMTMiIHhtbG5zOmllODE5PSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODE5OlYzLjEzIiB4bWxuczppZTgyOT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgyOTpWMy4xMyIgeG1sbnM6aWU4Mzc9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4Mzc6VjMuMTMiIHhtbG5zOmllODM5PSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODM5OlYzLjEzIiB4bWxuczppZTg0MD0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTg0MDpWMy4xMyIgeG1sbnM6aWU4NzE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4NzE6VjMuMTMiIHhtbG5zOmllODgxPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODgxOlYzLjEzIiB4bWxuczppZTkwNT0idXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTkwNTpWMy4xMyIgeG1sbnM6dGNsPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OlRDTDpWMy4xMyIgeG1sbnM6dG1zPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OlRNUzpWMy4xMyIgeG1sbnM6dG5zND0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvQ29tbW9uL0NvbnRyb2xEb2N1bWVudCIgeG1sbnM6dG5zNT0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvRXhjaXNlL01vdmVtZW50Rm9yVHJhZGVyRGF0YS8zIiB4bWxuczp0bnM9Imh0dHA6Ly93d3cuZ292dGFsay5nb3YudWsvdGF4YXRpb24vSW50ZXJuYXRpb25hbFRyYWRlL0V4Y2lzZS9OZXdNZXNzYWdlc0RhdGEvMyIgeG1sbnM6eHM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIj48aWU4MDE6SGVhZGVyPjx0bXM6TWVzc2FnZVNlbmRlcj5OREVBLlhJPC90bXM6TWVzc2FnZVNlbmRlcj48dG1zOk1lc3NhZ2VSZWNpcGllbnQ+TkRFQS5BVDwvdG1zOk1lc3NhZ2VSZWNpcGllbnQ+PHRtczpEYXRlT2ZQcmVwYXJhdGlvbj4yMDIzLTA2LTIyPC90bXM6RGF0ZU9mUHJlcGFyYXRpb24+PHRtczpUaW1lT2ZQcmVwYXJhdGlvbj4xMjozNzowOC43NTU8L3RtczpUaW1lT2ZQcmVwYXJhdGlvbj48dG1zOk1lc3NhZ2VJZGVudGlmaWVyPlhJMDAwMDAzPC90bXM6TWVzc2FnZUlkZW50aWZpZXI+PHRtczpDb3JyZWxhdGlvbklkZW50aWZpZXI+ODc8L3RtczpDb3JyZWxhdGlvbklkZW50aWZpZXI+PC9pZTgwMTpIZWFkZXI+PGllODAxOkJvZHk+PGllODAxOkVBREVTQURDb250YWluZXI+PGllODAxOkNvbnNpZ25lZVRyYWRlciBsYW5ndWFnZT0iZW4iPjxpZTgwMTpUcmFkZXJpZD5BVDAwMDAwNjAyMDgwPC9pZTgwMTpUcmFkZXJpZD48aWU4MDE6VHJhZGVyTmFtZT5BRk9SIEtBTEUgTFREPC9pZTgwMTpUcmFkZXJOYW1lPjxpZTgwMTpTdHJlZXROYW1lPlRoZSBTdHJlZXQ8L2llODAxOlN0cmVldE5hbWU+PGllODAxOlBvc3Rjb2RlPkFUMTIzPC9pZTgwMTpQb3N0Y29kZT48aWU4MDE6Q2l0eT5UaGUgQ2l0eTwvaWU4MDE6Q2l0eT48L2llODAxOkNvbnNpZ25lZVRyYWRlcj48aWU4MDE6RXhjaXNlTW92ZW1lbnQ+PGllODAxOkFkbWluaXN0cmF0aXZlUmVmZXJlbmNlQ29kZT4yM1hJMDAwMDAwMDAwMDAwMDAwMTQ8L2llODAxOkFkbWluaXN0cmF0aXZlUmVmZXJlbmNlQ29kZT48aWU4MDE6RGF0ZUFuZFRpbWVPZlZhbGlkYXRpb25PZkVhZEVzYWQ+MjAyMy0wNi0yMlQxMTozNzoxMC4zNDU3MzkzOTY8L2llODAxOkRhdGVBbmRUaW1lT2ZWYWxpZGF0aW9uT2ZFYWRFc2FkPjwvaWU4MDE6RXhjaXNlTW92ZW1lbnQ+PGllODAxOkNvbnNpZ25vclRyYWRlciBsYW5ndWFnZT0iZW4iPjxpZTgwMTpUcmFkZXJFeGNpc2VOdW1iZXI+R0JXSzA4MDk3OTAwMDwvaWU4MDE6VHJhZGVyRXhjaXNlTnVtYmVyPjxpZTgwMTpUcmFkZXJOYW1lPkNsYXJreXMgRWFnbGVzPC9pZTgwMTpUcmFkZXJOYW1lPjxpZTgwMTpTdHJlZXROYW1lPkhhcHB5IFN0cmVldDwvaWU4MDE6U3RyZWV0TmFtZT48aWU4MDE6UG9zdGNvZGU+QlQxIDFCRzwvaWU4MDE6UG9zdGNvZGU+PGllODAxOkNpdHk+VGhlIENpdHk8L2llODAxOkNpdHk+PC9pZTgwMTpDb25zaWdub3JUcmFkZXI+PGllODAxOlBsYWNlT2ZEaXNwYXRjaFRyYWRlciBsYW5ndWFnZT0iZW4iPjxpZTgwMTpSZWZlcmVuY2VPZlRheFdhcmVob3VzZT5YSTAwMDAwNDY3MDE0PC9pZTgwMTpSZWZlcmVuY2VPZlRheFdhcmVob3VzZT48L2llODAxOlBsYWNlT2ZEaXNwYXRjaFRyYWRlcj48aWU4MDE6RGVsaXZlcnlQbGFjZVRyYWRlciBsYW5ndWFnZT0iZW4iPjxpZTgwMTpUcmFkZXJpZD5BVDAwMDAwNjAyMDc4PC9pZTgwMTpUcmFkZXJpZD48aWU4MDE6VHJhZGVyTmFtZT5NRVRFU1QgQk9ORCBTVFRTVEdFPC9pZTgwMTpUcmFkZXJOYW1lPjxpZTgwMTpTdHJlZXROYW1lPldISVRFVEVTVCBST0FEIE1FVEVTVCBDSVRZIEVTVEFURTwvaWU4MDE6U3RyZWV0TmFtZT48aWU4MDE6UG9zdGNvZGU+Qk4yIDRLWDwvaWU4MDE6UG9zdGNvZGU+PGllODAxOkNpdHk+U1RURVNULEtFTlQ8L2llODAxOkNpdHk+PC9pZTgwMTpEZWxpdmVyeVBsYWNlVHJhZGVyPjxpZTgwMTpDb21wZXRlbnRBdXRob3JpdHlEaXNwYXRjaE9mZmljZT48aWU4MDE6UmVmZXJlbmNlTnVtYmVyPkdCMDA0MDk4PC9pZTgwMTpSZWZlcmVuY2VOdW1iZXI+PC9pZTgwMTpDb21wZXRlbnRBdXRob3JpdHlEaXNwYXRjaE9mZmljZT48aWU4MDE6RWFkRXNhZD48aWU4MDE6TG9jYWxSZWZlcmVuY2VOdW1iZXI+bHJuaWU4MTU1OTczODEyPC9pZTgwMTpMb2NhbFJlZmVyZW5jZU51bWJlcj48aWU4MDE6SW52b2ljZU51bWJlcj5JTlZPSUNFMDAxPC9pZTgwMTpJbnZvaWNlTnVtYmVyPjxpZTgwMTpJbnZvaWNlRGF0ZT4yMDE4LTA0LTA0PC9pZTgwMTpJbnZvaWNlRGF0ZT48aWU4MDE6T3JpZ2luVHlwZUNvZGU+MTwvaWU4MDE6T3JpZ2luVHlwZUNvZGU+PGllODAxOkRhdGVPZkRpc3BhdGNoPjIwMjEtMTItMDI8L2llODAxOkRhdGVPZkRpc3BhdGNoPjxpZTgwMTpUaW1lT2ZEaXNwYXRjaD4yMjozNzowMDwvaWU4MDE6VGltZU9mRGlzcGF0Y2g+PC9pZTgwMTpFYWRFc2FkPjxpZTgwMTpIZWFkZXJFYWRFc2FkPjxpZTgwMTpTZXF1ZW5jZU51bWJlcj4xPC9pZTgwMTpTZXF1ZW5jZU51bWJlcj48aWU4MDE6RGF0ZUFuZFRpbWVPZlVwZGF0ZVZhbGlkYXRpb24+MjAyMy0wNi0yMlQxMTozNzoxMC4zNDU4MDEwMjk8L2llODAxOkRhdGVBbmRUaW1lT2ZVcGRhdGVWYWxpZGF0aW9uPjxpZTgwMTpEZXN0aW5hdGlvblR5cGVDb2RlPjE8L2llODAxOkRlc3RpbmF0aW9uVHlwZUNvZGU+PGllODAxOkpvdXJuZXlUaW1lPkQwMTwvaWU4MDE6Sm91cm5leVRpbWU+PGllODAxOlRyYW5zcG9ydEFycmFuZ2VtZW50PjE8L2llODAxOlRyYW5zcG9ydEFycmFuZ2VtZW50PjwvaWU4MDE6SGVhZGVyRWFkRXNhZD48aWU4MDE6VHJhbnNwb3J0TW9kZT48aWU4MDE6VHJhbnNwb3J0TW9kZUNvZGU+MTwvaWU4MDE6VHJhbnNwb3J0TW9kZUNvZGU+PC9pZTgwMTpUcmFuc3BvcnRNb2RlPjxpZTgwMTpNb3ZlbWVudEd1YXJhbnRlZT48aWU4MDE6R3VhcmFudG9yVHlwZUNvZGU+MTwvaWU4MDE6R3VhcmFudG9yVHlwZUNvZGU+PC9pZTgwMTpNb3ZlbWVudEd1YXJhbnRlZT48aWU4MDE6Qm9keUVhZEVzYWQ+PGllODAxOkJvZHlSZWNvcmRVbmlxdWVSZWZlcmVuY2U+MTwvaWU4MDE6Qm9keVJlY29yZFVuaXF1ZVJlZmVyZW5jZT48aWU4MDE6RXhjaXNlUHJvZHVjdENvZGU+RTQxMDwvaWU4MDE6RXhjaXNlUHJvZHVjdENvZGU+PGllODAxOkNuQ29kZT4yNzEwMTIzMTwvaWU4MDE6Q25Db2RlPjxpZTgwMTpRdWFudGl0eT4xMDAuMDAwPC9pZTgwMTpRdWFudGl0eT48aWU4MDE6R3Jvc3NNYXNzPjEwMC4wMDwvaWU4MDE6R3Jvc3NNYXNzPjxpZTgwMTpOZXRNYXNzPjkwLjAwPC9pZTgwMTpOZXRNYXNzPjxpZTgwMTpEZW5zaXR5PjEwLjAwPC9pZTgwMTpEZW5zaXR5PjxpZTgwMTpQYWNrYWdlPjxpZTgwMTpLaW5kT2ZQYWNrYWdlcz5CSDwvaWU4MDE6S2luZE9mUGFja2FnZXM+PGllODAxOk51bWJlck9mUGFja2FnZXM+MjwvaWU4MDE6TnVtYmVyT2ZQYWNrYWdlcz48aWU4MDE6U2hpcHBpbmdNYXJrcz5TdWJoYXNpcyBTd2FpbjE8L2llODAxOlNoaXBwaW5nTWFya3M+PC9pZTgwMTpQYWNrYWdlPjxpZTgwMTpQYWNrYWdlPjxpZTgwMTpLaW5kT2ZQYWNrYWdlcz5CSDwvaWU4MDE6S2luZE9mUGFja2FnZXM+PGllODAxOk51bWJlck9mUGFja2FnZXM+MjwvaWU4MDE6TnVtYmVyT2ZQYWNrYWdlcz48aWU4MDE6U2hpcHBpbmdNYXJrcz5TdWJoYXNpcyBTd2FpbiAyPC9pZTgwMTpTaGlwcGluZ01hcmtzPjwvaWU4MDE6UGFja2FnZT48L2llODAxOkJvZHlFYWRFc2FkPjxpZTgwMTpUcmFuc3BvcnREZXRhaWxzPjxpZTgwMTpUcmFuc3BvcnRVbml0Q29kZT4xPC9pZTgwMTpUcmFuc3BvcnRVbml0Q29kZT48aWU4MDE6SWRlbnRpdHlPZlRyYW5zcG9ydFVuaXRzPlRyYW5zZm9ybWVycyByb2JvdHMgaW4gZGlzZ3Vpc2U8L2llODAxOklkZW50aXR5T2ZUcmFuc3BvcnRVbml0cz48L2llODAxOlRyYW5zcG9ydERldGFpbHM+PGllODAxOlRyYW5zcG9ydERldGFpbHM+PGllODAxOlRyYW5zcG9ydFVuaXRDb2RlPjI8L2llODAxOlRyYW5zcG9ydFVuaXRDb2RlPjxpZTgwMTpJZGVudGl0eU9mVHJhbnNwb3J0VW5pdHM+TUFDSElORVM8L2llODAxOklkZW50aXR5T2ZUcmFuc3BvcnRVbml0cz48L2llODAxOlRyYW5zcG9ydERldGFpbHM+PGllODAxOlRyYW5zcG9ydERldGFpbHM+PGllODAxOlRyYW5zcG9ydFVuaXRDb2RlPjM8L2llODAxOlRyYW5zcG9ydFVuaXRDb2RlPjxpZTgwMTpJZGVudGl0eU9mVHJhbnNwb3J0VW5pdHM+TU9SRSBNQUNISU5FUzwvaWU4MDE6SWRlbnRpdHlPZlRyYW5zcG9ydFVuaXRzPjwvaWU4MDE6VHJhbnNwb3J0RGV0YWlscz48L2llODAxOkVBREVTQURDb250YWluZXI+PC9pZTgwMTpCb2R5PjwvaWU4MDE6SUU4MDE+
  &lt;/ResponseBody&gt;
&lt;/GBCustomsBusinessResponse&gt;</OI_XmlContent>
      </eHubOutboxMessage>
    </Rows>
  </Insert>
</CompositeOperation>";
			#endregion

			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "Polling", "WTLDTWJLI", responseXml, null,
				null, true, "77777777-7777-7777-7777-777777777777", "mockServiceReference", "mockRequestID", "mockCorrelationID");

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_GBCBResponse_Error()
		{
			var originalRecipient = "GBCustoms-CTCGB";

			#region Input XML
			var responseXml = @"<ns0:ErrorResponse SchemaVersion=""SchemaVersion_0"" xmlns:ns0=""http://www.govtalk.gov.uk/CM/errorresponse"">
  <ns0:Application anyAttr=""anyAttrContents"">
    <any0>anyContents0</any0>
    <any1>anyContents1</any1>
    <any2>anyContents2</any2>
  </ns0:Application>
  <ns0:Error>
    <ns0:RaisedBy>RaisedBy_0</ns0:RaisedBy>
    <ns0:Number>100</ns0:Number>
    <ns0:Type>ErrorType_0</ns0:Type>
    <ns0:Text>Text_0_0</ns0:Text>
    <ns0:Text>Text_0_1</ns0:Text>
    <ns0:Text>Text_0_2</ns0:Text>
    <ns0:Location>Location_0</ns0:Location>
    <ns0:Application anyAttr=""anyAttrContents"">
      <any0>anyContents0</any0>
      <any1>anyContents1</any1>
      <any2>anyContents2</any2>
    </ns0:Application>
  </ns0:Error>
  <ns0:Error>
    <ns0:RaisedBy>RaisedBy_0</ns0:RaisedBy>
    <ns0:Number>100</ns0:Number>
    <ns0:Type>ErrorType_1</ns0:Type>
    <ns0:Text>Text_1_0</ns0:Text>
    <ns0:Text>Text_1_1</ns0:Text>
    <ns0:Text>Text_1_2</ns0:Text>
    <ns0:Location>Location_0</ns0:Location>
    <ns0:Application anyAttr=""anyAttrContents"">
      <any0>anyContents0</any0>
      <any1>anyContents1</any1>
      <any2>anyContents2</any2>
    </ns0:Application>
  </ns0:Error>
  <ns0:ResponseText>
    <ns0:Text>
        {
            ""code"": ""JSON_SCHEMA_ERROR"",
            ""message"": ""The JSON payload is not conformant with the JSON schema"",
            ""errors"": [
                {
                    ""code"": ""ONEOF"",
                    ""message"": ""Instance does not match any schema."",
                    ""path"": ""/direction""
                }
            ]
        }
    </ns0:Text>
  </ns0:ResponseText>
</ns0:ErrorResponse>";
			#endregion

			#region Expected XML

			var expectedXml = @"<CompositeOperation>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage"">
    <Rows>
      <eHubInboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EI_PK>00000000-0000-0000-0000-000000000000</EI_PK>
        <EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID>
        <EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
        <EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender>
        <EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
        <EI_MessageType>http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange</EI_MessageType>
        <EI_IsFlatFile>false</EI_IsFlatFile>
        <EI_ApplicationCode>UDM</EI_ApplicationCode>
        <EI_Status>2</EI_Status>
        <EI_Content>H4sIAAAAAAAEAK1VbW+iQBD+fsn9B9Ovd+2+9OpFS00ELUJbWkFF/WJ42cAq7BpArP76Q0CqpzV3SUmWzDwz+8wzEzIILIbNIaMpiWIrUFhCIse3mEdq72HA4mYWfrjyk2TZBGC9Xt84VuTxNY3JjcNDYDg+Ca0YVAQAQ4QAQlet799qNaFHLJdEuZ15BmGZp3RqYI/oxKFLSlhSgQI4uCOI3N3sc6sa3TS7UMj7X2m4kpZT5kyVuwcGNCQtDDG8Rvga1QcIN29/Ne/qPxBqQiiAj6STm5slab3o6j5n5x7kdKzEkng24vfDomXA4KvIIUd4GXmLeEp3I5EGkiwK4Ag74gFniQr4tLBQQhIPAuIklLNjtnNSMzjvivRW9iCynAVlntIRwF+t5okjK1iR1u/yuT7z2j8CKHKPmzk7qkuiulHEo0ta8oRdeAbvax8O+sr6xioMrWhzScYgI5jBGfxZKy1UWfi+sFAVRVUUzfAXCdVJvOQsJjvaS0Il2vYUqThkzSu7PHSCG4upodQVSX0awsbrGGkdozvS+qPG4zBQ34ZUiQ85iqMm07G2nZhuoMwzzkDnU0PM7ys90Sfmezox+541fvHssAEVWUvtcJo6IfJt1vdcHEBLEqElDz0j0N4GVNxOsB/YZpc+t081Tsfqxr5Vt7ta5mkPn/V2qHduY73Qumi86sOGrND10+f5bWqbo62DH1k5G9VmGpyYd/Osz4WNR1tFvktdSUwmY32e9ZL1tfAcrPGpifznszM7OEz0Xdmjr7RNn7G+dMLR3JWD1KbKeU09eIKP+/w03n54+NfPq8LOrI5y9+23Njhe2+WOL/e6AD77+7T+AMvhQL+YBgAA</EI_Content>
      </eHubInboxMessage>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent"">
    <Rows>
      <eHubInboxXmlContent xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox>
        <EX_XmlContent>&lt;ns0:UniversalInterchange xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;
  &lt;Header&gt;
    &lt;SenderID /&gt;
    &lt;RecipientID /&gt;
  &lt;/Header&gt;
  &lt;Body&gt;
    &lt;UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;
	  &lt;Event&gt;
        &lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;
        &lt;EventType&gt;MRJ&lt;/EventType&gt;
		&lt;DataContext&gt;
          &lt;DataSource&gt;
            &lt;DataProvider&gt;CTCGB&lt;/DataProvider&gt;
          &lt;/DataSource&gt;
        &lt;/DataContext&gt;
        &lt;ContextCollection&gt;
          &lt;Context&gt;
            &lt;Type&gt;eHubTrackingID&lt;/Type&gt;
            &lt;Value&gt;77777777-7777-7777-7777-777777777777&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;Error&lt;/Type&gt;
            &lt;Value&gt;ErrorType_0; ErrorType_1&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;ErrorSummary&lt;/Type&gt;
            &lt;Value&gt;Text_0_0, Text_0_1, Text_0_2; Text_1_0, Text_1_1, Text_1_2&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;ResponseText&lt;/Type&gt;
            &lt;Value&gt;CiAgICAgICAgewogICAgICAgICAgICAiY29kZSI6ICJKU09OX1NDSEVNQV9FUlJPUiIsCiAgICAgICAgICAgICJtZXNzYWdlIjogIlRoZSBKU09OIHBheWxvYWQgaXMgbm90IGNvbmZvcm1hbnQgd2l0aCB0aGUgSlNPTiBzY2hlbWEiLAogICAgICAgICAgICAiZXJyb3JzIjogWwogICAgICAgICAgICAgICAgewogICAgICAgICAgICAgICAgICAgICJjb2RlIjogIk9ORU9GIiwKICAgICAgICAgICAgICAgICAgICAibWVzc2FnZSI6ICJJbnN0YW5jZSBkb2VzIG5vdCBtYXRjaCBhbnkgc2NoZW1hLiIsCiAgICAgICAgICAgICAgICAgICAgInBhdGgiOiAiL2RpcmVjdGlvbiIKICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgXQogICAgICAgIH0KICAgIA==&lt;/Value&gt;
          &lt;/Context&gt;
        &lt;/ContextCollection&gt;
      &lt;/Event&gt;
    &lt;/UniversalEvent&gt;
  &lt;/Body&gt;
&lt;/ns0:UniversalInterchange&gt;</EX_XmlContent>
        <EX_DT_Source>33333333-3333-3333-3333-333333333333</EX_DT_Source>
        <EX_UncompressedLength>1688</EX_UncompressedLength>
      </eHubInboxXmlContent>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage"">
    <Rows>
      <eHubOutboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <OI_PK>00000000-0000-0000-0000-000000000000</OI_PK>
        <OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID>
        <OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK>
        <OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender>
        <OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
        <OI_DT_Target>33333333-3333-3333-3333-333333333333</OI_DT_Target>
        <OI_Status>0</OI_Status>
        <OI_Content>H4sIAAAAAAAEAK1VbW+iQBD+fsn9B9Ovd+2+9OpFS00ELUJbWkFF/WJ42cAq7BpArP76Q0CqpzV3SUmWzDwz+8wzEzIILIbNIaMpiWIrUFhCIse3mEdq72HA4mYWfrjyk2TZBGC9Xt84VuTxNY3JjcNDYDg+Ca0YVAQAQ4QAQlet799qNaFHLJdEuZ15BmGZp3RqYI/oxKFLSlhSgQI4uCOI3N3sc6sa3TS7UMj7X2m4kpZT5kyVuwcGNCQtDDG8Rvga1QcIN29/Ne/qPxBqQiiAj6STm5slab3o6j5n5x7kdKzEkng24vfDomXA4KvIIUd4GXmLeEp3I5EGkiwK4Ag74gFniQr4tLBQQhIPAuIklLNjtnNSMzjvivRW9iCynAVlntIRwF+t5okjK1iR1u/yuT7z2j8CKHKPmzk7qkuiulHEo0ta8oRdeAbvax8O+sr6xioMrWhzScYgI5jBGfxZKy1UWfi+sFAVRVUUzfAXCdVJvOQsJjvaS0Il2vYUqThkzSu7PHSCG4upodQVSX0awsbrGGkdozvS+qPG4zBQ34ZUiQ85iqMm07G2nZhuoMwzzkDnU0PM7ys90Sfmezox+541fvHssAEVWUvtcJo6IfJt1vdcHEBLEqElDz0j0N4GVNxOsB/YZpc+t081Tsfqxr5Vt7ta5mkPn/V2qHduY73Qumi86sOGrND10+f5bWqbo62DH1k5G9VmGpyYd/Osz4WNR1tFvktdSUwmY32e9ZL1tfAcrPGpifznszM7OEz0Xdmjr7RNn7G+dMLR3JWD1KbKeU09eIKP+/w03n54+NfPq8LOrI5y9+23Njhe2+WOL/e6AD77+7T+AMvhQL+YBgAA</OI_Content>
        <OI_XmlContent>&lt;ns0:UniversalInterchange xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;
  &lt;Header&gt;
    &lt;SenderID /&gt;
    &lt;RecipientID /&gt;
  &lt;/Header&gt;
  &lt;Body&gt;
    &lt;UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;
	  &lt;Event&gt;
        &lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;
        &lt;EventType&gt;MRJ&lt;/EventType&gt;
		&lt;DataContext&gt;
          &lt;DataSource&gt;
            &lt;DataProvider&gt;CTCGB&lt;/DataProvider&gt;
          &lt;/DataSource&gt;
        &lt;/DataContext&gt;
        &lt;ContextCollection&gt;
          &lt;Context&gt;
            &lt;Type&gt;eHubTrackingID&lt;/Type&gt;
            &lt;Value&gt;77777777-7777-7777-7777-777777777777&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;Error&lt;/Type&gt;
            &lt;Value&gt;ErrorType_0; ErrorType_1&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;ErrorSummary&lt;/Type&gt;
            &lt;Value&gt;Text_0_0, Text_0_1, Text_0_2; Text_1_0, Text_1_1, Text_1_2&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;ResponseText&lt;/Type&gt;
            &lt;Value&gt;CiAgICAgICAgewogICAgICAgICAgICAiY29kZSI6ICJKU09OX1NDSEVNQV9FUlJPUiIsCiAgICAgICAgICAgICJtZXNzYWdlIjogIlRoZSBKU09OIHBheWxvYWQgaXMgbm90IGNvbmZvcm1hbnQgd2l0aCB0aGUgSlNPTiBzY2hlbWEiLAogICAgICAgICAgICAiZXJyb3JzIjogWwogICAgICAgICAgICAgICAgewogICAgICAgICAgICAgICAgICAgICJjb2RlIjogIk9ORU9GIiwKICAgICAgICAgICAgICAgICAgICAibWVzc2FnZSI6ICJJbnN0YW5jZSBkb2VzIG5vdCBtYXRjaCBhbnkgc2NoZW1hLiIsCiAgICAgICAgICAgICAgICAgICAgInBhdGgiOiAiL2RpcmVjdGlvbiIKICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgXQogICAgICAgIH0KICAgIA==&lt;/Value&gt;
          &lt;/Context&gt;
        &lt;/ContextCollection&gt;
      &lt;/Event&gt;
    &lt;/UniversalEvent&gt;
  &lt;/Body&gt;
&lt;/ns0:UniversalInterchange&gt;</OI_XmlContent>
      </eHubOutboxMessage>
    </Rows>
  </Insert>
</CompositeOperation>";

			#endregion

			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");
			OrchestrationHelpers.Now = () => new DateTime(2020, 12, 16, 12, 34, 56);

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "GetPushData", "WTLDTWJLI", responseXml, null,
				gbCustomsXml, false, "77777777-7777-7777-7777-777777777777", "mockServiceReference", "mockRequestID");

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_UniEvent_Success()
		{
			var originalRecipient = "GBCustoms-ICSGB";

			var responseXml = @"<ns:SuccessResponse xmlns:ns=""http://www.hmrc.gov.uk/successresponse/2"" xmlns=""http://www.govtalk.gov.uk/enforcement/ICS/responsedata/7"">
   <ns:ResponseData>
       <CorrelationId>0JRF7UncK0t004</CorrelationId>
       <WhateverId>666</WhateverId>
   </ns:ResponseData>
</ns:SuccessResponse>";

			var mockClientRegistrationAccessor = MockClientRegistrationAccessor(originalRecipient);
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GBCTID", originalRecipient, "WTLDTWJLI", "0JRF7UncK0t004", gbCustomsXml, null)).Repeat.Once();

			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;
			OrchestrationHelpers.DataModelAccessor = () => mockDataModelAccessor;
			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.Now = () => new DateTime(2020, 12, 16, 12, 34, 56);
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "Create", "WTLDTWJLI", responseXml, null,
				gbCustomsXml, true, "66666666-6666-6666-6666-666666666666", "", "");

			#region Expected XML

			var expectedXml = @"<CompositeOperation><Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage""><Rows><eHubInboxMessagexmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo""><EI_PK>00000000-0000-0000-0000-000000000000</EI_PK><EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID><EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID><EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender><EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient><EI_MessageType>http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange</EI_MessageType><EI_IsFlatFile>false</EI_IsFlatFile><EI_ApplicationCode>UDM</EI_ApplicationCode><EI_Status>2</EI_Status><EI_Content>H4sIAAAAAAAEAK1UbU/CMBD+buJ/IHw12HYgJktdgsMXNBrjpt9rd0LjaElXQP693auMDdTEfljW5567e+6uOSoT7L5IsQKdsHgiDWg+Y3IKnc95LBPXmi+6M2MWLkLr9fqUMz1Va5HAKVdzFPAZzFmCqgDIwYQgQrre8VGnQ2+BRaCzf3sLQNrbZNxBJfIMXCwESFOBFG350EsVbUpuleNqZR1yeX+V5lTSspBZpOpqgTEzzFe2CZ/bcGEIbXwwvopj4EYoWWPUODsWaws3C/BGyYYvI/bApHiHxFCUoQ3uPWy8Rz/EGPfJgPQpSoGdXGhPsm1Dq9Kc0Kwx70Uo5uA52ME94vTIMCSO2x+4Z8MTQlyMKfomNTzTUh6Cx5JTr+xwYwO11Hy3EZnlSauVSF/DxA9uLnPtFdasuxFob7UFtGeYtE1qOUW4Xb6FmvEPIaeTcesQ6SuLl+ANi9Nr+ZSHopxbL6a1VYdE+UpriFlayWFN+O75+vxF8ntsMB78U/ZRFIk0tV0gPzXktxkrrGVExRsrFwOqb4ZijRSrg6J9C877AkAv4Df7BAAA</EI_Content></eHubInboxMessage></Rows></Insert><Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent""><Rows><eHubInboxXmlContentxmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo""><EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox><EX_XmlContent>&lt;ns0:UniversalInterchangexmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;&lt;Header&gt;&lt;SenderID/&gt;&lt;RecipientID/&gt;&lt;/Header&gt;&lt;Body&gt;&lt;UniversalEventxmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;&lt;Event&gt;&lt;DataContext&gt;&lt;DataTargetCollection&gt;&lt;DataTarget&gt;&lt;Type&gt;AsycudaManifest&lt;/Type&gt;&lt;Key&gt;NCT00031413&lt;/Key&gt;&lt;/DataTarget&gt;&lt;/DataTargetCollection&gt;&lt;/DataContext&gt;&lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;&lt;EventType&gt;MSN&lt;/EventType&gt;&lt;DataContext&gt;&lt;DataSource&gt;&lt;DataProvider&gt;ICSGB&lt;/DataProvider&gt;&lt;/DataSource&gt;&lt;/DataContext&gt;&lt;ContextCollection&gt;&lt;Context&gt;&lt;Type&gt;eHubTrackingID&lt;/Type&gt;&lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;CorrelationID&lt;/Type&gt;&lt;Value&gt;0JRF7UncK0t004&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;AdditionalID&lt;/Type&gt;&lt;Value&gt;666&lt;/Value&gt;&lt;/Context&gt;&lt;/ContextCollection&gt;&lt;/Event&gt;&lt;/UniversalEvent&gt;&lt;/Body&gt;&lt;/ns0:UniversalInterchange&gt;</EX_XmlContent><EX_DT_Source>33333333-3333-3333-3333-333333333333</EX_DT_Source><EX_UncompressedLength>1275</EX_UncompressedLength></eHubInboxXmlContent></Rows></Insert><Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage""><Rows><eHubOutboxMessagexmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo""><OI_PK>00000000-0000-0000-0000-000000000000</OI_PK><OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID><OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK><OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender><OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient><OI_DT_Target>33333333-3333-3333-3333-333333333333</OI_DT_Target><OI_Status>0</OI_Status><OI_Content>H4sIAAAAAAAEAK1UbU/CMBD+buJ/IHw12HYgJktdgsMXNBrjpt9rd0LjaElXQP693auMDdTEfljW5567e+6uOSoT7L5IsQKdsHgiDWg+Y3IKnc95LBPXmi+6M2MWLkLr9fqUMz1Va5HAKVdzFPAZzFmCqgDIwYQgQrre8VGnQ2+BRaCzf3sLQNrbZNxBJfIMXCwESFOBFG350EsVbUpuleNqZR1yeX+V5lTSspBZpOpqgTEzzFe2CZ/bcGEIbXwwvopj4EYoWWPUODsWaws3C/BGyYYvI/bApHiHxFCUoQ3uPWy8Rz/EGPfJgPQpSoGdXGhPsm1Dq9Kc0Kwx70Uo5uA52ME94vTIMCSO2x+4Z8MTQlyMKfomNTzTUh6Cx5JTr+xwYwO11Hy3EZnlSauVSF/DxA9uLnPtFdasuxFob7UFtGeYtE1qOUW4Xb6FmvEPIaeTcesQ6SuLl+ANi9Nr+ZSHopxbL6a1VYdE+UpriFlayWFN+O75+vxF8ntsMB78U/ZRFIk0tV0gPzXktxkrrGVExRsrFwOqb4ZijRSrg6J9C877AkAv4Df7BAAA</OI_Content><OI_XmlContent>&lt;ns0:UniversalInterchangexmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;&lt;Header&gt;&lt;SenderID/&gt;&lt;RecipientID/&gt;&lt;/Header&gt;&lt;Body&gt;&lt;UniversalEventxmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;&lt;Event&gt;&lt;DataContext&gt;&lt;DataTargetCollection&gt;&lt;DataTarget&gt;&lt;Type&gt;AsycudaManifest&lt;/Type&gt;&lt;Key&gt;NCT00031413&lt;/Key&gt;&lt;/DataTarget&gt;&lt;/DataTargetCollection&gt;&lt;/DataContext&gt;&lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;&lt;EventType&gt;MSN&lt;/EventType&gt;&lt;DataContext&gt;&lt;DataSource&gt;&lt;DataProvider&gt;ICSGB&lt;/DataProvider&gt;&lt;/DataSource&gt;&lt;/DataContext&gt;&lt;ContextCollection&gt;&lt;Context&gt;&lt;Type&gt;eHubTrackingID&lt;/Type&gt;&lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;CorrelationID&lt;/Type&gt;&lt;Value&gt;0JRF7UncK0t004&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;AdditionalID&lt;/Type&gt;&lt;Value&gt;666&lt;/Value&gt;&lt;/Context&gt;&lt;/ContextCollection&gt;&lt;/Event&gt;&lt;/UniversalEvent&gt;&lt;/Body&gt;&lt;/ns0:UniversalInterchange&gt;</OI_XmlContent></eHubOutboxMessage></Rows></Insert></CompositeOperation>";

			#endregion

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_UniEvent_Error()
		{
			var originalRecipient = "GBCustomsTest-CTCGB";

			#region ErrorResponse XML

			var responseXml = @"<ns0:ErrorResponse SchemaVersion=""SchemaVersion_0"" xmlns:ns0=""http://www.govtalk.gov.uk/CM/errorresponse"">
  <ns0:Application anyAttr=""anyAttrContents"">
    <any0>anyContents0</any0>
    <any1>anyContents1</any1>
    <any2>anyContents2</any2>
  </ns0:Application>
  <ns0:Error>
    <ns0:RaisedBy>RaisedBy_0</ns0:RaisedBy>
    <ns0:Number>100</ns0:Number>
    <ns0:Type>ErrorType_0</ns0:Type>
    <ns0:Text>Text_0_0</ns0:Text>
    <ns0:Text>Text_0_1</ns0:Text>
    <ns0:Text>Text_0_2</ns0:Text>
    <ns0:Location>Location_0</ns0:Location>
    <ns0:Application anyAttr=""anyAttrContents"">
      <any0>anyContents0</any0>
      <any1>anyContents1</any1>
      <any2>anyContents2</any2>
    </ns0:Application>
  </ns0:Error>
  <ns0:Error>
    <ns0:RaisedBy>RaisedBy_0</ns0:RaisedBy>
    <ns0:Number>100</ns0:Number>
    <ns0:Type>ErrorType_1</ns0:Type>
    <ns0:Text>Text_1_0</ns0:Text>
    <ns0:Text>Text_1_1</ns0:Text>
    <ns0:Text>Text_1_2</ns0:Text>
    <ns0:Location>Location_0</ns0:Location>
    <ns0:Application anyAttr=""anyAttrContents"">
      <any0>anyContents0</any0>
      <any1>anyContents1</any1>
      <any2>anyContents2</any2>
    </ns0:Application>
  </ns0:Error>
  <ns0:ResponseText>
    <ns0:Text>
        {
            ""code"": ""JSON_SCHEMA_ERROR"",
            ""message"": ""The JSON payload is not conformant with the JSON schema"",
            ""errors"": [
                {
                    ""code"": ""ONEOF"",
                    ""message"": ""Instance does not match any schema."",
                    ""path"": ""/direction""
                }
            ]
        }
    </ns0:Text>
  </ns0:ResponseText>
</ns0:ErrorResponse>";

			#endregion

			var mockClientRegistrationAccessor = MockClientRegistrationAccessor(originalRecipient);


			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;
			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.Now = () => new DateTime(2020, 12, 16, 12, 34, 56);
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "Create", "WTLDTWJLI", responseXml, null,
				gbCustomsXml, false, "66666666-6666-6666-6666-666666666666", "", "");

			#region Expected XML

			var expectedXml = @"<CompositeOperation>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage"">
    <Rows>
      <eHubInboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EI_PK>00000000-0000-0000-0000-000000000000</EI_PK>
        <EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID>
        <EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
        <EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender>
        <EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
        <EI_MessageType>http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange</EI_MessageType>
        <EI_IsFlatFile>false</EI_IsFlatFile>
        <EI_ApplicationCode>UDM</EI_ApplicationCode>
        <EI_Status>2</EI_Status>
        <EI_Content>H4sIAAAAAAAEAK1VW2+iQBR+32T/g+nrbjsXtyZaaiJoEdrSCijqi+EygVEuBhCrv34RkGpBs5uUZMiZ85055/vOTGYYP4KdsU8TEka6K/gxCU1H923S+PBcP+qk8OONE8frDgDb7fbO1EM72NKI3JmBBxTTIZ4egTIBwBAhgNBN9+ePRoMZEt0iYWanM4X46UzoN8DRIxOTrinx49LJgJM1DBtYu2NsWWOQpAtyev9LDZfUspRZpnKaOvp6rHNB2oSPU3cBqGl+EnOB6xIzpoF/FnEW8wVJMXW3Jl3JjKNcHQMyRyXsmey6EqdCCJvoD2oy4OD4UgZcqHMK1JLMA6ry8jao1CNdDDG8RfgWtVSEO80/nfvWL4Q6EDLgM6iy8iDlVRaPMefKrvdUCTah+bURGfIeBgk9HARO5Xg25176qroriS6qLVwX9pGpo3rcQDLcGGqomyvq20K/dhOZie5uSLdVfLc1v+PHgDz2XExtq66RGoRhUH+gCi5ZwAFewIfG5wR9Z31l43l6uLtGQ00TLOAC/m4UFiot/JBbqERRiaIF/iaiMonWgR+RQ9prRDnaswUuH2QblHYx6Ay3V3NFaAmc+DyG7bcpkvrKYCKNJu2nsSu+j6kQnebIhxjPp9J+plmusExzunIwV9hsvTBkHaJ9JDNtZOvTV9vw2lDgpcTw5onpIcfwR7aFXahzLNT5sa240rtK2f0MO66hDehLr8pxPhV3RlPcH2ppVQ2XtJ3yXRpYzrmu2m/yuM0LdPt8Ob5HDW2yN/GTX/RGNHwJzrT7ZapzZeDJXuDvE4tj49lUXqZaUl0r28RSMNeQ81Lbs5Phs47F2/SN9ugLltemN1lavJsYVKjnNIQV/3QUVPHe4+O/Hq/SV3N1FHff8a0C549V8bIVrxkDLr253b8T3GvbjgcAAA==</EI_Content>
      </eHubInboxMessage>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent"">
    <Rows>
      <eHubInboxXmlContent xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox>
        <EX_XmlContent>&lt;ns0:UniversalInterchange xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;
  &lt;Header&gt;
    &lt;SenderID /&gt;
    &lt;RecipientID /&gt;
  &lt;/Header&gt;
  &lt;Body&gt;
    &lt;UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;
	  &lt;Event&gt;
        &lt;DataContext&gt;
          &lt;DataTargetCollection&gt;
            &lt;DataTarget&gt;
              &lt;Type&gt;NctsHeader&lt;/Type&gt;
              &lt;Key&gt;NCT00031413&lt;/Key&gt;
			&lt;/DataTarget&gt;
          &lt;/DataTargetCollection&gt;
        &lt;/DataContext&gt;
        &lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;
        &lt;EventType&gt;MRJ&lt;/EventType&gt;
		&lt;DataContext&gt;
          &lt;DataSource&gt;
            &lt;DataProvider&gt;CTCGB&lt;/DataProvider&gt;
          &lt;/DataSource&gt;
        &lt;/DataContext&gt;
        &lt;ContextCollection&gt;
          &lt;Context&gt;
            &lt;Type&gt;eHubTrackingID&lt;/Type&gt;
            &lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;Error&lt;/Type&gt;
            &lt;Value&gt;ErrorType_0; ErrorType_1&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;ErrorSummary&lt;/Type&gt;
            &lt;Value&gt;Text_0_0, Text_0_1, Text_0_2; Text_1_0, Text_1_1, Text_1_2&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;ResponseText&lt;/Type&gt;
            &lt;Value&gt;CiAgICAgICAgewogICAgICAgICAgICAiY29kZSI6ICJKU09OX1NDSEVNQV9FUlJPUiIsCiAgICAgICAgICAgICJtZXNzYWdlIjogIlRoZSBKU09OIHBheWxvYWQgaXMgbm90IGNvbmZvcm1hbnQgd2l0aCB0aGUgSlNPTiBzY2hlbWEiLAogICAgICAgICAgICAiZXJyb3JzIjogWwogICAgICAgICAgICAgICAgewogICAgICAgICAgICAgICAgICAgICJjb2RlIjogIk9ORU9GIiwKICAgICAgICAgICAgICAgICAgICAibWVzc2FnZSI6ICJJbnN0YW5jZSBkb2VzIG5vdCBtYXRjaCBhbnkgc2NoZW1hLiIsCiAgICAgICAgICAgICAgICAgICAgInBhdGgiOiAiL2RpcmVjdGlvbiIKICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgXQogICAgICAgIH0KICAgIA==&lt;/Value&gt;
          &lt;/Context&gt;
        &lt;/ContextCollection&gt;
      &lt;/Event&gt;
    &lt;/UniversalEvent&gt;
  &lt;/Body&gt;
&lt;/ns0:UniversalInterchange&gt;</EX_XmlContent>
        <EX_DT_Source>33333333-3333-3333-3333-333333333333</EX_DT_Source>
        <EX_UncompressedLength>1934</EX_UncompressedLength>
      </eHubInboxXmlContent>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage"">
    <Rows>
      <eHubOutboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <OI_PK>00000000-0000-0000-0000-000000000000</OI_PK>
        <OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID>
        <OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK>
        <OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender>
        <OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
        <OI_DT_Target>33333333-3333-3333-3333-333333333333</OI_DT_Target>
        <OI_Status>0</OI_Status>
        <OI_Content>H4sIAAAAAAAEAK1VW2+iQBR+32T/g+nrbjsXtyZaaiJoEdrSCijqi+EygVEuBhCrv34RkGpBs5uUZMiZ85055/vOTGYYP4KdsU8TEka6K/gxCU1H923S+PBcP+qk8OONE8frDgDb7fbO1EM72NKI3JmBBxTTIZ4egTIBwBAhgNBN9+ePRoMZEt0iYWanM4X46UzoN8DRIxOTrinx49LJgJM1DBtYu2NsWWOQpAtyev9LDZfUspRZpnKaOvp6rHNB2oSPU3cBqGl+EnOB6xIzpoF/FnEW8wVJMXW3Jl3JjKNcHQMyRyXsmey6EqdCCJvoD2oy4OD4UgZcqHMK1JLMA6ry8jao1CNdDDG8RfgWtVSEO80/nfvWL4Q6EDLgM6iy8iDlVRaPMefKrvdUCTah+bURGfIeBgk9HARO5Xg25176qroriS6qLVwX9pGpo3rcQDLcGGqomyvq20K/dhOZie5uSLdVfLc1v+PHgDz2XExtq66RGoRhUH+gCi5ZwAFewIfG5wR9Z31l43l6uLtGQ00TLOAC/m4UFiot/JBbqERRiaIF/iaiMonWgR+RQ9prRDnaswUuH2QblHYx6Ay3V3NFaAmc+DyG7bcpkvrKYCKNJu2nsSu+j6kQnebIhxjPp9J+plmusExzunIwV9hsvTBkHaJ9JDNtZOvTV9vw2lDgpcTw5onpIcfwR7aFXahzLNT5sa240rtK2f0MO66hDehLr8pxPhV3RlPcH2ppVQ2XtJ3yXRpYzrmu2m/yuM0LdPt8Ob5HDW2yN/GTX/RGNHwJzrT7ZapzZeDJXuDvE4tj49lUXqZaUl0r28RSMNeQ81Lbs5Phs47F2/SN9ugLltemN1lavJsYVKjnNIQV/3QUVPHe4+O/Hq/SV3N1FHff8a0C549V8bIVrxkDLr253b8T3GvbjgcAAA==</OI_Content>
        <OI_XmlContent>&lt;ns0:UniversalInterchange xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;
  &lt;Header&gt;
    &lt;SenderID /&gt;
    &lt;RecipientID /&gt;
  &lt;/Header&gt;
  &lt;Body&gt;
    &lt;UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;
	  &lt;Event&gt;
        &lt;DataContext&gt;
          &lt;DataTargetCollection&gt;
            &lt;DataTarget&gt;
              &lt;Type&gt;NctsHeader&lt;/Type&gt;
              &lt;Key&gt;NCT00031413&lt;/Key&gt;
			&lt;/DataTarget&gt;
          &lt;/DataTargetCollection&gt;
        &lt;/DataContext&gt;
        &lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;
        &lt;EventType&gt;MRJ&lt;/EventType&gt;
		&lt;DataContext&gt;
          &lt;DataSource&gt;
            &lt;DataProvider&gt;CTCGB&lt;/DataProvider&gt;
          &lt;/DataSource&gt;
        &lt;/DataContext&gt;
        &lt;ContextCollection&gt;
          &lt;Context&gt;
            &lt;Type&gt;eHubTrackingID&lt;/Type&gt;
            &lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;Error&lt;/Type&gt;
            &lt;Value&gt;ErrorType_0; ErrorType_1&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;ErrorSummary&lt;/Type&gt;
            &lt;Value&gt;Text_0_0, Text_0_1, Text_0_2; Text_1_0, Text_1_1, Text_1_2&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;ResponseText&lt;/Type&gt;
            &lt;Value&gt;CiAgICAgICAgewogICAgICAgICAgICAiY29kZSI6ICJKU09OX1NDSEVNQV9FUlJPUiIsCiAgICAgICAgICAgICJtZXNzYWdlIjogIlRoZSBKU09OIHBheWxvYWQgaXMgbm90IGNvbmZvcm1hbnQgd2l0aCB0aGUgSlNPTiBzY2hlbWEiLAogICAgICAgICAgICAiZXJyb3JzIjogWwogICAgICAgICAgICAgICAgewogICAgICAgICAgICAgICAgICAgICJjb2RlIjogIk9ORU9GIiwKICAgICAgICAgICAgICAgICAgICAibWVzc2FnZSI6ICJJbnN0YW5jZSBkb2VzIG5vdCBtYXRjaCBhbnkgc2NoZW1hLiIsCiAgICAgICAgICAgICAgICAgICAgInBhdGgiOiAiL2RpcmVjdGlvbiIKICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgXQogICAgICAgIH0KICAgIA==&lt;/Value&gt;
          &lt;/Context&gt;
        &lt;/ContextCollection&gt;
      &lt;/Event&gt;
    &lt;/UniversalEvent&gt;
  &lt;/Body&gt;
&lt;/ns0:UniversalInterchange&gt;</OI_XmlContent>
      </eHubOutboxMessage>
    </Rows>
  </Insert>
</CompositeOperation>";

			#endregion

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_UniEvent_GatewayError()
		{
			var originalRecipient = "GBCustomsTest-CTCGB";

			#region ErrorResponse XML

			var responseXml = @"<ErrorResponse>
  <Error>
      <Text>
          HTTP Status (502): BadGateway
      </Text>
  </Error>
  <ResponseText>
      <Text>
          <!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">
<HTML><HEAD><META HTTP-EQUIV=""Content-Type"" CONTENT=""text/html; charset=iso-8859-1"">
<TITLE>ERROR: The request could not be satisfied</TITLE>
</HEAD><BODY>
<H1>502 ERROR</H1>
<H2>The request could not be satisfied.</H2>
<HR noshade size=""1px"">
The origin closed the connection.
We can't connect to the server for this app or website at this time. There might be too much traffic or a configuration error. Try again later, or contact the app or website owner.
<BR clear=""all"">
If you provide content to customers through CloudFront, you can find steps to troubleshoot and help prevent this error by reviewing the CloudFront documentation.
<BR clear=""all"">
<HR noshade size=""1px"">
<PRE>
Generated by cloudfront (CloudFront)
Request ID: 06Ro1u6mQx3wTGbATQFMN0ZuB_wwczPEJcpfEoLd61jbCl8Uf-BYsQ==
</PRE>
<ADDRESS>
</ADDRESS>
</BODY></HTML>
      </Text>
  </ResponseText>
</ErrorResponse>";

			#endregion

			var mockClientRegistrationAccessor = MockClientRegistrationAccessor(originalRecipient);


			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;
			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.Now = () => new DateTime(2020, 12, 16, 12, 34, 56);
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "Create", "WTLDTWJLI", responseXml, null,
				gbCustomsXml, false, "66666666-6666-6666-6666-666666666666", "", "");

			#region Expected XML

			var expectedXml = @"<CompositeOperation>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage"">
    <Rows>
      <eHubInboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EI_PK>00000000-0000-0000-0000-000000000000</EI_PK>
        <EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID>
        <EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
        <EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender>
        <EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
        <EI_MessageType>http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange</EI_MessageType>
        <EI_IsFlatFile>false</EI_IsFlatFile>
        <EI_ApplicationCode>UDM</EI_ApplicationCode>
        <EI_Status>2</EI_Status>
        <EI_Content>H4sIAAAAAAAEAK1W226jSBB9X2n/IZqnXUUzNBBHIfJEGq5ux2ZMAw3mjZsAuwHLxhf89VtgO5O7ZqWxlJiuqq46deo07mG1QfduVezS9SZkuGrSdZyHVZZeHUpWbe7B/f1L3jSre47b7/ff4nCd1ftik36L65Kz4zwtww33lIATEM9zPP/l4e+/rq6GozRM0nX/DCs7rWCF1SvuYiFpXKyKtGqejEPu2Z6hXCftJfaphraDDSd4/xea8AStT9lnelqCQQ2bUKmBhMNz89nhQP60UWrG0rgp6upFxIuYVx7wOe0qfTDjZnPqbsj1hjdhj2n7YCoOQkjkb3hxyHWGV2W4D+o8d7wL8hTwtr0TDU5Rpg8CEtBXXvjK3zq8cC/e3A9ur3n+HqEh9yvozc6ulSkZX2JedvY5p3a9Xcevieg9s3W9KzohKI5iyCfsT7a3fb9J9GG3Z9MHcxy+B/UywHS0jZx1GC+LKsPqu0Mc0pBt04fb8+frO/8unyF3in3ZzLtUfQZKW6/r9wU1fJu/q/DnqtrbsgzX7WfFn9lGjjO7spuw2W6u/hkg4d/7KzlMjLBJ92F7OZJ/Bh1JN6u62qQOBHyGbqbomoNM16OyjrXcddx95upUcTSmYgU3k1byp2i6myDiEkXGVOOnWLW2U1XL6GicR5W5Sgy2i0p9A7G6U+AMlzlKRj9uYS0mYrxNjtNtJI6rCU/siZCjyNujiXDYRaLJJiVBgYKvVau+s3ViOurNna1RmcC341LXsqGmTqyJSwl1mTezx2okDFDgDdCEkkFsuAXWzJnDiO4wS8IVYenI2oUj0kTqJpsLeR5XJktUfhULd81PNeN/2uiAFzemsji4NiVT4gx0l41n7qLOqJEzPBqz2KcsFi3YL/GRYWVRKSFsjMFn5tDvMSgZC9T9jmrMdTS3xz9BuW651vVMG8+Ivjz1pGrXpvOjxRq13aVkzxQJT52+Np4uBm5ouFlc0kPi0WOiyItIpJtAkYEvqF2CT9BR6Jtl6NHlZLHf2eqFK9xhOoaGvgxs+Rj6Kzaz8SEeZcUsQ499H4bUhl6yigrIa0jHwLOypLebMK8Bm4tkFQk3W9Va+ZBjMfduqh6DMNgGnonwiOzgr+7yB/5YCHycBaXUdrbQn2ZzX953NfAoYfNeBy7Y+hqruJVR6PFsUsjQI20hRwNYashfwDOKhLss8ukiVGQUl3oJfC76XIbWcb6FdZX447zXViGzuBrv4uKm09wAG3o199gWGwfw03aiyODrcaP5CXeHOY9HP7JIxFki0CIWGALbLhEGDPJ0/CtuzwvN4wUq5t5h02tiyUo8YrvElvdxKQmhR058ga6iquvtLpuLMCtDaoCTY1cL4vhAyDILNJ14xID1NlH2WepJPOzNAT/Mb7AE7aDAl49nXluYdQH1YYbSDnjJo9LKgKtNrHS1qdBp/MI11Gq7XubVstML4KIi5KzOvaowXz7QgrbjABtkBxibbv/cv8y4vrMY7BcObO5jCcN5jZTf09JMl23SaXaZsKikLeRkoNEitXtd8YFxrqvkL3A8WujR/aXtMVHrbKoGdnTUebPkSaqaItWSwqKEkCX/c6qveIvBO0Mwb12NPs5FuSSeNA3U+SEs4dyrOQ0KXvF8k8zgpaXAeXD1sd5j1HSNwLPLT89n8fX6dCa789e9w2bZ9++/+6p9sr3zi3n+yb9c0biXd7Tzhe58iRtyH101H/4DHOvLhoUKAAA=</EI_Content>
      </eHubInboxMessage>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent"">
    <Rows>
      <eHubInboxXmlContent xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox>
        <EX_XmlContent>&lt;ns0:UniversalInterchange xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;
  &lt;Header&gt;
    &lt;SenderID /&gt;
    &lt;RecipientID /&gt;
  &lt;/Header&gt;
  &lt;Body&gt;
    &lt;UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;
	  &lt;Event&gt;
        &lt;DataContext&gt;
          &lt;DataTargetCollection&gt;
            &lt;DataTarget&gt;
              &lt;Type&gt;NctsHeader&lt;/Type&gt;
              &lt;Key&gt;NCT00031413&lt;/Key&gt;
			&lt;/DataTarget&gt;
          &lt;/DataTargetCollection&gt;
        &lt;/DataContext&gt;
        &lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;
        &lt;EventType&gt;MRJ&lt;/EventType&gt;
		&lt;DataContext&gt;
          &lt;DataSource&gt;
            &lt;DataProvider&gt;CTCGB&lt;/DataProvider&gt;
          &lt;/DataSource&gt;
        &lt;/DataContext&gt;
        &lt;ContextCollection&gt;
          &lt;Context&gt;
            &lt;Type&gt;eHubTrackingID&lt;/Type&gt;
            &lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;Error&lt;/Type&gt;
            &lt;Value&gt;&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;ErrorSummary&lt;/Type&gt;
            &lt;Value&gt;HTTP Status (502): BadGateway&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;ResponseText&lt;/Type&gt;
            &lt;Value&gt;PCFET0NUWVBFIEhUTUwgUFVCTElDICItLy9XM0MvL0RURCBIVE1MIDQuMDEgVHJhbnNpdGlvbmFsLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL1RSL2h0bWw0L2xvb3NlLmR0ZCI+DQo8SFRNTD48SEVBRD48TUVUQSBIVFRQLUVRVUlWPSJDb250ZW50LVR5cGUiIENPTlRFTlQ9InRleHQvaHRtbDsgY2hhcnNldD1pc28tODg1OS0xIj4NCjxUSVRMRT5FUlJPUjogVGhlIHJlcXVlc3QgY291bGQgbm90IGJlIHNhdGlzZmllZDwvVElUTEU+DQo8L0hFQUQ+PEJPRFk+DQo8SDE+NTAyIEVSUk9SPC9IMT4NCjxIMj5UaGUgcmVxdWVzdCBjb3VsZCBub3QgYmUgc2F0aXNmaWVkLjwvSDI+DQo8SFIgbm9zaGFkZSBzaXplPSIxcHgiPg0KVGhlIG9yaWdpbiBjbG9zZWQgdGhlIGNvbm5lY3Rpb24uDQpXZSBjYW4ndCBjb25uZWN0IHRvIHRoZSBzZXJ2ZXIgZm9yIHRoaXMgYXBwIG9yIHdlYnNpdGUgYXQgdGhpcyB0aW1lLiBUaGVyZSBtaWdodCBiZSB0b28gbXVjaCB0cmFmZmljIG9yIGEgY29uZmlndXJhdGlvbiBlcnJvci4gVHJ5IGFnYWluIGxhdGVyLCBvciBjb250YWN0IHRoZSBhcHAgb3Igd2Vic2l0ZSBvd25lci4NCjxCUiBjbGVhcj0iYWxsIj4NCklmIHlvdSBwcm92aWRlIGNvbnRlbnQgdG8gY3VzdG9tZXJzIHRocm91Z2ggQ2xvdWRGcm9udCwgeW91IGNhbiBmaW5kIHN0ZXBzIHRvIHRyb3VibGVzaG9vdCBhbmQgaGVscCBwcmV2ZW50IHRoaXMgZXJyb3IgYnkgcmV2aWV3aW5nIHRoZSBDbG91ZEZyb250IGRvY3VtZW50YXRpb24uDQo8QlIgY2xlYXI9ImFsbCI+DQo8SFIgbm9zaGFkZSBzaXplPSIxcHgiPg0KPFBSRT4NCkdlbmVyYXRlZCBieSBjbG91ZGZyb250IChDbG91ZEZyb250KQ0KUmVxdWVzdCBJRDogMDZSbzF1Nm1ReDN3VEdiQVRRRk1OMFp1Ql93d2N6UEVKY3BmRW9MZDYxamJDbDhVZi1CWXNRPT0NCjwvUFJFPg0KPEFERFJFU1M+DQo8L0FERFJFU1M+DQo8L0JPRFk+PC9IVE1MPg==&lt;/Value&gt;
          &lt;/Context&gt;
        &lt;/ContextCollection&gt;
      &lt;/Event&gt;
    &lt;/UniversalEvent&gt;
  &lt;/Body&gt;
&lt;/ns0:UniversalInterchange&gt;</EX_XmlContent>
        <EX_DT_Source>33333333-3333-3333-3333-333333333333</EX_DT_Source>
        <EX_UncompressedLength>2693</EX_UncompressedLength>
      </eHubInboxXmlContent>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage"">
    <Rows>
      <eHubOutboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <OI_PK>00000000-0000-0000-0000-000000000000</OI_PK>
        <OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID>
        <OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK>
        <OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender>
        <OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
        <OI_DT_Target>33333333-3333-3333-3333-333333333333</OI_DT_Target>
        <OI_Status>0</OI_Status>
        <OI_Content>H4sIAAAAAAAEAK1W226jSBB9X2n/IZqnXUUzNBBHIfJEGq5ux2ZMAw3mjZsAuwHLxhf89VtgO5O7ZqWxlJiuqq46deo07mG1QfduVezS9SZkuGrSdZyHVZZeHUpWbe7B/f1L3jSre47b7/ff4nCd1ftik36L65Kz4zwtww33lIATEM9zPP/l4e+/rq6GozRM0nX/DCs7rWCF1SvuYiFpXKyKtGqejEPu2Z6hXCftJfaphraDDSd4/xea8AStT9lnelqCQQ2bUKmBhMNz89nhQP60UWrG0rgp6upFxIuYVx7wOe0qfTDjZnPqbsj1hjdhj2n7YCoOQkjkb3hxyHWGV2W4D+o8d7wL8hTwtr0TDU5Rpg8CEtBXXvjK3zq8cC/e3A9ur3n+HqEh9yvozc6ulSkZX2JedvY5p3a9Xcevieg9s3W9KzohKI5iyCfsT7a3fb9J9GG3Z9MHcxy+B/UywHS0jZx1GC+LKsPqu0Mc0pBt04fb8+frO/8unyF3in3ZzLtUfQZKW6/r9wU1fJu/q/DnqtrbsgzX7WfFn9lGjjO7spuw2W6u/hkg4d/7KzlMjLBJ92F7OZJ/Bh1JN6u62qQOBHyGbqbomoNM16OyjrXcddx95upUcTSmYgU3k1byp2i6myDiEkXGVOOnWLW2U1XL6GicR5W5Sgy2i0p9A7G6U+AMlzlKRj9uYS0mYrxNjtNtJI6rCU/siZCjyNujiXDYRaLJJiVBgYKvVau+s3ViOurNna1RmcC341LXsqGmTqyJSwl1mTezx2okDFDgDdCEkkFsuAXWzJnDiO4wS8IVYenI2oUj0kTqJpsLeR5XJktUfhULd81PNeN/2uiAFzemsji4NiVT4gx0l41n7qLOqJEzPBqz2KcsFi3YL/GRYWVRKSFsjMFn5tDvMSgZC9T9jmrMdTS3xz9BuW651vVMG8+Ivjz1pGrXpvOjxRq13aVkzxQJT52+Np4uBm5ouFlc0kPi0WOiyItIpJtAkYEvqF2CT9BR6Jtl6NHlZLHf2eqFK9xhOoaGvgxs+Rj6Kzaz8SEeZcUsQ499H4bUhl6yigrIa0jHwLOypLebMK8Bm4tkFQk3W9Va+ZBjMfduqh6DMNgGnonwiOzgr+7yB/5YCHycBaXUdrbQn2ZzX953NfAoYfNeBy7Y+hqruJVR6PFsUsjQI20hRwNYashfwDOKhLss8ukiVGQUl3oJfC76XIbWcb6FdZX447zXViGzuBrv4uKm09wAG3o199gWGwfw03aiyODrcaP5CXeHOY9HP7JIxFki0CIWGALbLhEGDPJ0/CtuzwvN4wUq5t5h02tiyUo8YrvElvdxKQmhR058ga6iquvtLpuLMCtDaoCTY1cL4vhAyDILNJ14xID1NlH2WepJPOzNAT/Mb7AE7aDAl49nXluYdQH1YYbSDnjJo9LKgKtNrHS1qdBp/MI11Gq7XubVstML4KIi5KzOvaowXz7QgrbjABtkBxibbv/cv8y4vrMY7BcObO5jCcN5jZTf09JMl23SaXaZsKikLeRkoNEitXtd8YFxrqvkL3A8WujR/aXtMVHrbKoGdnTUebPkSaqaItWSwqKEkCX/c6qveIvBO0Mwb12NPs5FuSSeNA3U+SEs4dyrOQ0KXvF8k8zgpaXAeXD1sd5j1HSNwLPLT89n8fX6dCa789e9w2bZ9++/+6p9sr3zi3n+yb9c0biXd7Tzhe58iRtyH101H/4DHOvLhoUKAAA=</OI_Content>
        <OI_XmlContent>&lt;ns0:UniversalInterchange xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;
  &lt;Header&gt;
    &lt;SenderID /&gt;
    &lt;RecipientID /&gt;
  &lt;/Header&gt;
  &lt;Body&gt;
    &lt;UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;
	  &lt;Event&gt;
        &lt;DataContext&gt;
          &lt;DataTargetCollection&gt;
            &lt;DataTarget&gt;
              &lt;Type&gt;NctsHeader&lt;/Type&gt;
              &lt;Key&gt;NCT00031413&lt;/Key&gt;
			&lt;/DataTarget&gt;
          &lt;/DataTargetCollection&gt;
        &lt;/DataContext&gt;
        &lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;
        &lt;EventType&gt;MRJ&lt;/EventType&gt;
		&lt;DataContext&gt;
          &lt;DataSource&gt;
            &lt;DataProvider&gt;CTCGB&lt;/DataProvider&gt;
          &lt;/DataSource&gt;
        &lt;/DataContext&gt;
        &lt;ContextCollection&gt;
          &lt;Context&gt;
            &lt;Type&gt;eHubTrackingID&lt;/Type&gt;
            &lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;Error&lt;/Type&gt;
            &lt;Value&gt;&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;ErrorSummary&lt;/Type&gt;
            &lt;Value&gt;HTTP Status (502): BadGateway&lt;/Value&gt;
          &lt;/Context&gt;
          &lt;Context&gt;
            &lt;Type&gt;ResponseText&lt;/Type&gt;
            &lt;Value&gt;PCFET0NUWVBFIEhUTUwgUFVCTElDICItLy9XM0MvL0RURCBIVE1MIDQuMDEgVHJhbnNpdGlvbmFsLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL1RSL2h0bWw0L2xvb3NlLmR0ZCI+DQo8SFRNTD48SEVBRD48TUVUQSBIVFRQLUVRVUlWPSJDb250ZW50LVR5cGUiIENPTlRFTlQ9InRleHQvaHRtbDsgY2hhcnNldD1pc28tODg1OS0xIj4NCjxUSVRMRT5FUlJPUjogVGhlIHJlcXVlc3QgY291bGQgbm90IGJlIHNhdGlzZmllZDwvVElUTEU+DQo8L0hFQUQ+PEJPRFk+DQo8SDE+NTAyIEVSUk9SPC9IMT4NCjxIMj5UaGUgcmVxdWVzdCBjb3VsZCBub3QgYmUgc2F0aXNmaWVkLjwvSDI+DQo8SFIgbm9zaGFkZSBzaXplPSIxcHgiPg0KVGhlIG9yaWdpbiBjbG9zZWQgdGhlIGNvbm5lY3Rpb24uDQpXZSBjYW4ndCBjb25uZWN0IHRvIHRoZSBzZXJ2ZXIgZm9yIHRoaXMgYXBwIG9yIHdlYnNpdGUgYXQgdGhpcyB0aW1lLiBUaGVyZSBtaWdodCBiZSB0b28gbXVjaCB0cmFmZmljIG9yIGEgY29uZmlndXJhdGlvbiBlcnJvci4gVHJ5IGFnYWluIGxhdGVyLCBvciBjb250YWN0IHRoZSBhcHAgb3Igd2Vic2l0ZSBvd25lci4NCjxCUiBjbGVhcj0iYWxsIj4NCklmIHlvdSBwcm92aWRlIGNvbnRlbnQgdG8gY3VzdG9tZXJzIHRocm91Z2ggQ2xvdWRGcm9udCwgeW91IGNhbiBmaW5kIHN0ZXBzIHRvIHRyb3VibGVzaG9vdCBhbmQgaGVscCBwcmV2ZW50IHRoaXMgZXJyb3IgYnkgcmV2aWV3aW5nIHRoZSBDbG91ZEZyb250IGRvY3VtZW50YXRpb24uDQo8QlIgY2xlYXI9ImFsbCI+DQo8SFIgbm9zaGFkZSBzaXplPSIxcHgiPg0KPFBSRT4NCkdlbmVyYXRlZCBieSBjbG91ZGZyb250IChDbG91ZEZyb250KQ0KUmVxdWVzdCBJRDogMDZSbzF1Nm1ReDN3VEdiQVRRRk1OMFp1Ql93d2N6UEVKY3BmRW9MZDYxamJDbDhVZi1CWXNRPT0NCjwvUFJFPg0KPEFERFJFU1M+DQo8L0FERFJFU1M+DQo8L0JPRFk+PC9IVE1MPg==&lt;/Value&gt;
          &lt;/Context&gt;
        &lt;/ContextCollection&gt;
      &lt;/Event&gt;
    &lt;/UniversalEvent&gt;
  &lt;/Body&gt;
&lt;/ns0:UniversalInterchange&gt;</OI_XmlContent>
      </eHubOutboxMessage>
    </Rows>
  </Insert>
</CompositeOperation>";

			#endregion

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_UniEvent_Error_NoType()
		{
			var originalRecipient = "GBCustomsTest-CTCGB";

			#region ErrorResponse XML

			var responseXml = @"<ErrorResponse>
                                    <Error>
                                        <Text>
                                            HTTP Status (400): BadRequest
                                        </Text>
                                    </Error>
                                    <ResponseText>
                                        <Text>
                                            The request has failed schema validation. Please review the required message structure as specified by the XSD file 'cc015b.xsd'. Detailed error below:
cvc-complex-type.2.4.a: Invalid content was found starting with element 'TINCO159'. One of '{NamCO17}' is expected.
                                        </Text>
                                    </ResponseText>
                                </ErrorResponse>";

			#endregion

			var mockClientRegistrationAccessor = MockClientRegistrationAccessor(originalRecipient);


			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;
			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.Now = () => new DateTime(2020, 12, 16, 12, 34, 56);
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "Create", "WTLDTWJLI", responseXml, null,
				gbCustomsXml, false, "66666666-6666-6666-6666-666666666666", "", "");

			#region Expected XML

			var expectedXml = @"<CompositeOperation>
	<Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage""><Rows><eHubInboxMessagexmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
		<EI_PK>00000000-0000-0000-0000-000000000000</EI_PK>
		<EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID>
		<EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
		<EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender>
		<EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
		<EI_MessageType>http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange</EI_MessageType>
		<EI_IsFlatFile>false</EI_IsFlatFile>
		<EI_ApplicationCode>UDM</EI_ApplicationCode>
		<EI_Status>2</EI_Status>
		<EI_Content>H4sIAAAAAAAEAK1VW2+bMBR+n7T/EPVp09RiIKnWiEYqJCFkTdIQAoE3YzxDY0zGJbdfP+e65qquqiUQ/s53Lt+xxVFYBqpDFk1xmkFqsBynKISM4NI8piyrcvPjTZjnk6ogzGazOwRTksyiDN+hJBYGKMQxzIR9AEECoiiI4k3t65dSSWlhGOB0/c13A8z4zqiXhB1iYhRNIszyPagIb3wUNQkWO+4+R2PKHTbl/W9p0r60dch1pP2WA3WYQy3hTZi/hbcGi8fHuZZQilEeJeyAccA5snCbtZjgWhfl2UadIqyBE9ovvKh1NQsAIItlUVaEFXCURriQ563hbJEbwqm8TRusKMY1CUjgVpRuxXtLlKpyuVq5/yGKVQAU4R/pxHMlpWO2d5xDZdd7OkiKFB03Ym15SZNptLoImqXp6qb2PXaq+yTQRbVb6MI5KudK3R0gbhW+lUI0jhgx6mcPUbEhLXDtfrtuz7x2SxE23EMxZ1t1rahGmibnL5RyGn+V4fOyDoo4huniWvKjG359tSzrpTTIYV5kpW9lAL5XSyoMTPynwFn+7kifpNDE2SRhGbY44ZpCLXoihvaxx9ZDarTaFI1siuQ+gXpzaeheCJ059TR16Uoh9Z0GCeJmBh0zDHQ69aMyGepz6o66a98gpjRYqADqQ4Jiex44dOE5feI79hJJTeYN1GXQaouubIooHhJ31CFIVqkr0Rg69tjQ2xWjZSac5w5Bn3gxzbzBE3Olzqxj2dEzC5eehgqjYdJAb058feVjL1D8sOC+1Ncf5B6ZvAZxJ3elhxzx2rAmAjxS6fOrUXS1StiL1LbPvNDXKfftTn1mUp/1SSCt9U4DpzI2Wl3gjtoAOhVmtIJJoBPi8T5wHUWgPTG7QXt98HPetcbsOVJffK7Fl1zSlvOe64h1a9mQfw8QgVyfNwpnntMFvA/FB8/n8b3XaI+d+aNsf4m7ESYczrDtwNsOOUW4NIprfwEYnUK4pQcAAA==</EI_Content>
	</eHubInboxMessage>
</Rows>
</Insert>
<Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent""><Rows><eHubInboxXmlContentxmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
	<EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox>
	<EX_XmlContent>&lt;ns0:UniversalInterchangexmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;&lt;Header&gt;&lt;SenderID/&gt;&lt;RecipientID/&gt;&lt;/Header&gt;&lt;Body&gt;&lt;UniversalEventxmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;&lt;Event&gt;&lt;DataContext&gt;&lt;DataTargetCollection&gt;&lt;DataTarget&gt;&lt;Type&gt;NctsHeader&lt;/Type&gt;&lt;Key&gt;NCT00031413&lt;/Key&gt;&lt;/DataTarget&gt;&lt;/DataTargetCollection&gt;&lt;/DataContext&gt;&lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;&lt;EventType&gt;MRJ&lt;/EventType&gt;&lt;DataContext&gt;&lt;DataSource&gt;&lt;DataProvider&gt;CTCGB&lt;/DataProvider&gt;&lt;/DataSource&gt;&lt;/DataContext&gt;&lt;ContextCollection&gt;&lt;Context&gt;&lt;Type&gt;eHubTrackingID&lt;/Type&gt;&lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;Error&lt;/Type&gt;&lt;Value&gt;&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ErrorSummary&lt;/Type&gt;&lt;Value&gt;HTTPStatus(400):BadRequest&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ResponseText&lt;/Type&gt;&lt;Value&gt;CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgVGhlIHJlcXVlc3QgaGFzIGZhaWxlZCBzY2hlbWEgdmFsaWRhdGlvbi4gUGxlYXNlIHJldmlldyB0aGUgcmVxdWlyZWQgbWVzc2FnZSBzdHJ1Y3R1cmUgYXMgc3BlY2lmaWVkIGJ5IHRoZSBYU0QgZmlsZSAnY2MwMTViLnhzZCcuIERldGFpbGVkIGVycm9yIGJlbG93OgpjdmMtY29tcGxleC10eXBlLjIuNC5hOiBJbnZhbGlkIGNvbnRlbnQgd2FzIGZvdW5kIHN0YXJ0aW5nIHdpdGggZWxlbWVudCAnVElOQ08xNTknLiBPbmUgb2YgJ3tOYW1DTzE3fScgaXMgZXhwZWN0ZWQuCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA=&lt;/Value&gt;&lt;/Context&gt;&lt;/ContextCollection&gt;&lt;/Event&gt;&lt;/UniversalEvent&gt;&lt;/Body&gt;&lt;/ns0:UniversalInterchange&gt;</EX_XmlContent>
	<EX_DT_Source>33333333-3333-3333-3333-333333333333</EX_DT_Source>
	<EX_UncompressedLength>1957</EX_UncompressedLength>
</eHubInboxXmlContent>
</Rows>
</Insert>
<Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage""><Rows><eHubOutboxMessagexmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
	<OI_PK>00000000-0000-0000-0000-000000000000</OI_PK>
	<OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID>
	<OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK>
	<OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender>
	<OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
	<OI_DT_Target>33333333-3333-3333-3333-333333333333</OI_DT_Target>
	<OI_Status>0</OI_Status>
	<OI_Content>H4sIAAAAAAAEAK1VW2+bMBR+n7T/EPVp09RiIKnWiEYqJCFkTdIQAoE3YzxDY0zGJbdfP+e65qquqiUQ/s53Lt+xxVFYBqpDFk1xmkFqsBynKISM4NI8piyrcvPjTZjnk6ogzGazOwRTksyiDN+hJBYGKMQxzIR9AEECoiiI4k3t65dSSWlhGOB0/c13A8z4zqiXhB1iYhRNIszyPagIb3wUNQkWO+4+R2PKHTbl/W9p0r60dch1pP2WA3WYQy3hTZi/hbcGi8fHuZZQilEeJeyAccA5snCbtZjgWhfl2UadIqyBE9ovvKh1NQsAIItlUVaEFXCURriQ563hbJEbwqm8TRusKMY1CUjgVpRuxXtLlKpyuVq5/yGKVQAU4R/pxHMlpWO2d5xDZdd7OkiKFB03Ym15SZNptLoImqXp6qb2PXaq+yTQRbVb6MI5KudK3R0gbhW+lUI0jhgx6mcPUbEhLXDtfrtuz7x2SxE23EMxZ1t1rahGmibnL5RyGn+V4fOyDoo4huniWvKjG359tSzrpTTIYV5kpW9lAL5XSyoMTPynwFn+7kifpNDE2SRhGbY44ZpCLXoihvaxx9ZDarTaFI1siuQ+gXpzaeheCJ059TR16Uoh9Z0GCeJmBh0zDHQ69aMyGepz6o66a98gpjRYqADqQ4Jiex44dOE5feI79hJJTeYN1GXQaouubIooHhJ31CFIVqkr0Rg69tjQ2xWjZSac5w5Bn3gxzbzBE3Olzqxj2dEzC5eehgqjYdJAb058feVjL1D8sOC+1Ncf5B6ZvAZxJ3elhxzx2rAmAjxS6fOrUXS1StiL1LbPvNDXKfftTn1mUp/1SSCt9U4DpzI2Wl3gjtoAOhVmtIJJoBPi8T5wHUWgPTG7QXt98HPetcbsOVJffK7Fl1zSlvOe64h1a9mQfw8QgVyfNwpnntMFvA/FB8/n8b3XaI+d+aNsf4m7ESYczrDtwNsOOUW4NIprfwEYnUK4pQcAAA==</OI_Content>
	<OI_XmlContent>&lt;ns0:UniversalInterchangexmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;&lt;Header&gt;&lt;SenderID/&gt;&lt;RecipientID/&gt;&lt;/Header&gt;&lt;Body&gt;&lt;UniversalEventxmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;&lt;Event&gt;&lt;DataContext&gt;&lt;DataTargetCollection&gt;&lt;DataTarget&gt;&lt;Type&gt;NctsHeader&lt;/Type&gt;&lt;Key&gt;NCT00031413&lt;/Key&gt;&lt;/DataTarget&gt;&lt;/DataTargetCollection&gt;&lt;/DataContext&gt;&lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;&lt;EventType&gt;MRJ&lt;/EventType&gt;&lt;DataContext&gt;&lt;DataSource&gt;&lt;DataProvider&gt;CTCGB&lt;/DataProvider&gt;&lt;/DataSource&gt;&lt;/DataContext&gt;&lt;ContextCollection&gt;&lt;Context&gt;&lt;Type&gt;eHubTrackingID&lt;/Type&gt;&lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;Error&lt;/Type&gt;&lt;Value&gt;&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ErrorSummary&lt;/Type&gt;&lt;Value&gt;HTTPStatus(400):BadRequest&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ResponseText&lt;/Type&gt;&lt;Value&gt;CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgVGhlIHJlcXVlc3QgaGFzIGZhaWxlZCBzY2hlbWEgdmFsaWRhdGlvbi4gUGxlYXNlIHJldmlldyB0aGUgcmVxdWlyZWQgbWVzc2FnZSBzdHJ1Y3R1cmUgYXMgc3BlY2lmaWVkIGJ5IHRoZSBYU0QgZmlsZSAnY2MwMTViLnhzZCcuIERldGFpbGVkIGVycm9yIGJlbG93OgpjdmMtY29tcGxleC10eXBlLjIuNC5hOiBJbnZhbGlkIGNvbnRlbnQgd2FzIGZvdW5kIHN0YXJ0aW5nIHdpdGggZWxlbWVudCAnVElOQ08xNTknLiBPbmUgb2YgJ3tOYW1DTzE3fScgaXMgZXhwZWN0ZWQuCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA=&lt;/Value&gt;&lt;/Context&gt;&lt;/ContextCollection&gt;&lt;/Event&gt;&lt;/UniversalEvent&gt;&lt;/Body&gt;&lt;/ns0:UniversalInterchange&gt;</OI_XmlContent>
</eHubOutboxMessage>
</Rows>
</Insert>
</CompositeOperation>";

			#endregion

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_UniEvent_Error_NoCorrelation()
		{
			var originalRecipient = "GBCustomsTest-CTCGB";

			#region ErrorResponse XML

			var responseXml = @"<ns:SuccessResponse xmlns:ns=""http://www.hmrc.gov.uk/successresponse/2"" xmlns=""http://www.govtalk.gov.uk/enforcement/ICS/responsedata/7"">
   <ns:ResponseData>
       Do you still hear the lambs, Clarice?
   </ns:ResponseData>
</ns:SuccessResponse>";

			#endregion

			#region ClientRegistration Mock

			var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
			var mockClientRegistrations = new List<Dictionary<string, object>>()
			{
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", originalRecipient } ,
					{ "CX_Qualifier", "Create"},
					{ "CX_Code", "XMLBody:/*/*[local-name()='ResponseData']/*[local-name()='CorrelationId']/text()" },
					{ "CX_Flag1", "Synchronous"},
					{ "CX_Flag2", "Subscribed" },
					{ "CX_Attr1", "CorrelationID" }
				}
			};
			mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations(originalRecipient, "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

			#endregion

			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;
			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.Now = () => new DateTime(2020, 12, 16, 12, 34, 56);
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "Create", "WTLDTWJLI", responseXml, null,
				gbCustomsXml, true, "66666666-6666-6666-6666-666666666666", "", "");

			#region Expected XML

			var expectedXml = @"<CompositeOperation>
	<Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage""><Rows><eHubInboxMessagexmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
		<EI_PK>00000000-0000-0000-0000-000000000000</EI_PK>
		<EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID>
		<EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
		<EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender>
		<EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
		<EI_MessageType>http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange</EI_MessageType>
		<EI_IsFlatFile>false</EI_IsFlatFile>
		<EI_ApplicationCode>UDM</EI_ApplicationCode>
		<EI_Status>2</EI_Status>
		<EI_Content>H4sIAAAAAAAEAK1VXW+iQBR932T/A+lr0w4DaqqhJhUtYpVtAWvlDWZmdVxgDF+Kv76jiNaKze6mkwxhzr1z7zn3wowSxmJrHNKMRLHr62FCIjR3wxkR1oEfxi1uvr+aJ8myBcBqtbpFbjRjKxqTW8QCYKE5CdwYHAIASYQQQHjV/vlDEJQ+cTGJdu98ZZGQr/SuAErEJIguKQmTA6iAD3uUDsN56XvI0cv4hoLev1KTDtR2IXeRDksOdN3EVRkvwvojvDfYPD5JVOb7BCWUhSceJz6fLNxm50vSNlASF+oUsAPO3J5I3jZUWxRFGdagrIAt8CkNuJDno6GSZOFwLq8og00D0pZESbyB0g1s2FBqybVWvXENYUsUFXB0Otu5lTIyB6XPqbKva2qxNEKfC7GzPEcso9sPQbVVrVNwP2Dnus8CXVS7hy70UamiWjaQ9FPPjlz0h4YzvVvZROXV9VPSbuzHTcWjHAoofE/FVJbqK1K9KGLVH9Sei8EElUUR8d2tWkHHvEn0N+U/xXcysNIgcKP8/4gIiKU+Fjwi0BLE30TOJPGShTGxucNX5J61+uaXb8CpZPhINiznzVh5Un3jWJ2aN1mnaLPkU6Ru3xRRl2VDGcs4rzPvbbAYBjjDtA7dvLnBE2PB925Q8LpBWjNF0jgbUX1GNBh74aipB3MR9x8aw7zJI6DUkZoS1h5jN6+HnjxN8STOnEk98OTBwplA3wtfMmts2EN5wHl1Mi80fEd7FKfWnawvak+6+jAruB/t5tZuF7bd7JmZ3vczbHU2WPNjT+0wZ/KY632TcX3xdAIpylezF2k9R4G/cOy7Iq7a3Oo+1mJszrHWu1YXK55n1BjLr4upxHXCY+7n2f3937bugFX8jPvTpDz9wenxv78r9veDAi7dYu138ugzaeAGAAA=</EI_Content>
	</eHubInboxMessage>
</Rows>
</Insert>
<Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent""><Rows><eHubInboxXmlContentxmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
	<EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox>
	<EX_XmlContent>&lt;ns0:UniversalInterchangexmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;&lt;Header&gt;&lt;SenderID/&gt;&lt;RecipientID/&gt;&lt;/Header&gt;&lt;Body&gt;&lt;UniversalEventxmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;&lt;Event&gt;&lt;DataContext&gt;&lt;DataTargetCollection&gt;&lt;DataTarget&gt;&lt;Type&gt;NctsHeader&lt;/Type&gt;&lt;Key&gt;NCT00031413&lt;/Key&gt;&lt;/DataTarget&gt;&lt;/DataTargetCollection&gt;&lt;/DataContext&gt;&lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;&lt;EventType&gt;MRJ&lt;/EventType&gt;&lt;DataContext&gt;&lt;DataSource&gt;&lt;DataProvider&gt;CTCGB&lt;/DataProvider&gt;&lt;/DataSource&gt;&lt;/DataContext&gt;&lt;ContextCollection&gt;&lt;Context&gt;&lt;Type&gt;eHubTrackingID&lt;/Type&gt;&lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;Error&lt;/Type&gt;&lt;Value&gt;NoCorrelationIdentifiers&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ErrorSummary&lt;/Type&gt;&lt;Value&gt;NoCorrelationIdentifierscouldbeidentified&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ResponseText&lt;/Type&gt;&lt;Value&gt;PG5zOlN1Y2Nlc3NSZXNwb25zZSB4bWxuczpucz0iaHR0cDovL3d3dy5obXJjLmdvdi51ay9zdWNjZXNzcmVzcG9uc2UvMiIgeG1sbnM9Imh0dHA6Ly93d3cuZ292dGFsay5nb3YudWsvZW5mb3JjZW1lbnQvSUNTL3Jlc3BvbnNlZGF0YS83Ij4KICAgPG5zOlJlc3BvbnNlRGF0YT4KICAgICAgIERvIHlvdSBzdGlsbCBoZWFyIHRoZSBsYW1icywgQ2xhcmljZT8KICAgPC9uczpSZXNwb25zZURhdGE+CjwvbnM6U3VjY2Vzc1Jlc3BvbnNlPg==&lt;/Value&gt;&lt;/Context&gt;&lt;/ContextCollection&gt;&lt;/Event&gt;&lt;/UniversalEvent&gt;&lt;/Body&gt;&lt;/ns0:UniversalInterchange&gt;</EX_XmlContent>
	< EX_DT_Source>33333333-3333-3333-3333-333333333333</EX_DT_Source>
	<EX_UncompressedLength>1760</EX_UncompressedLength>
</eHubInboxXmlContent>
</Rows>
</Insert>
<Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage""><Rows><eHubOutboxMessagexmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
	<OI_PK>00000000-0000-0000-0000-000000000000</OI_PK>
	<OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID>
	<OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK>
	<OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender>
	<OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
	<OI_DT_Target>33333333-3333-3333-3333-333333333333</OI_DT_Target>
	<OI_Status>0</OI_Status>
	<OI_Content>H4sIAAAAAAAEAK1VXW+iQBR932T/A+lr0w4DaqqhJhUtYpVtAWvlDWZmdVxgDF+Kv76jiNaKze6mkwxhzr1z7zn3wowSxmJrHNKMRLHr62FCIjR3wxkR1oEfxi1uvr+aJ8myBcBqtbpFbjRjKxqTW8QCYKE5CdwYHAIASYQQQHjV/vlDEJQ+cTGJdu98ZZGQr/SuAErEJIguKQmTA6iAD3uUDsN56XvI0cv4hoLev1KTDtR2IXeRDksOdN3EVRkvwvojvDfYPD5JVOb7BCWUhSceJz6fLNxm50vSNlASF+oUsAPO3J5I3jZUWxRFGdagrIAt8CkNuJDno6GSZOFwLq8og00D0pZESbyB0g1s2FBqybVWvXENYUsUFXB0Otu5lTIyB6XPqbKva2qxNEKfC7GzPEcso9sPQbVVrVNwP2Dnus8CXVS7hy70UamiWjaQ9FPPjlz0h4YzvVvZROXV9VPSbuzHTcWjHAoofE/FVJbqK1K9KGLVH9Sei8EElUUR8d2tWkHHvEn0N+U/xXcysNIgcKP8/4gIiKU+Fjwi0BLE30TOJPGShTGxucNX5J61+uaXb8CpZPhINiznzVh5Un3jWJ2aN1mnaLPkU6Ru3xRRl2VDGcs4rzPvbbAYBjjDtA7dvLnBE2PB925Q8LpBWjNF0jgbUX1GNBh74aipB3MR9x8aw7zJI6DUkZoS1h5jN6+HnjxN8STOnEk98OTBwplA3wtfMmts2EN5wHl1Mi80fEd7FKfWnawvak+6+jAruB/t5tZuF7bd7JmZ3vczbHU2WPNjT+0wZ/KY632TcX3xdAIpylezF2k9R4G/cOy7Iq7a3Oo+1mJszrHWu1YXK55n1BjLr4upxHXCY+7n2f3937bugFX8jPvTpDz9wenxv78r9veDAi7dYu138ugzaeAGAAA=</OI_Content>
	<OI_XmlContent>&lt;ns0:UniversalInterchangexmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;&lt;Header&gt;&lt;SenderID/&gt;&lt;RecipientID/&gt;&lt;/Header&gt;&lt;Body&gt;&lt;UniversalEventxmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;&lt;Event&gt;&lt;DataContext&gt;&lt;DataTargetCollection&gt;&lt;DataTarget&gt;&lt;Type&gt;NctsHeader&lt;/Type&gt;&lt;Key&gt;NCT00031413&lt;/Key&gt;&lt;/DataTarget&gt;&lt;/DataTargetCollection&gt;&lt;/DataContext&gt;&lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;&lt;EventType&gt;MRJ&lt;/EventType&gt;&lt;DataContext&gt;&lt;DataSource&gt;&lt;DataProvider&gt;CTCGB&lt;/DataProvider&gt;&lt;/DataSource&gt;&lt;/DataContext&gt;&lt;ContextCollection&gt;&lt;Context&gt;&lt;Type&gt;eHubTrackingID&lt;/Type&gt;&lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;Error&lt;/Type&gt;&lt;Value&gt;NoCorrelationIdentifiers&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ErrorSummary&lt;/Type&gt;&lt;Value&gt;NoCorrelationIdentifierscouldbeidentified&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ResponseText&lt;/Type&gt;&lt;Value&gt;PG5zOlN1Y2Nlc3NSZXNwb25zZSB4bWxuczpucz0iaHR0cDovL3d3dy5obXJjLmdvdi51ay9zdWNjZXNzcmVzcG9uc2UvMiIgeG1sbnM9Imh0dHA6Ly93d3cuZ292dGFsay5nb3YudWsvZW5mb3JjZW1lbnQvSUNTL3Jlc3BvbnNlZGF0YS83Ij4KICAgPG5zOlJlc3BvbnNlRGF0YT4KICAgICAgIERvIHlvdSBzdGlsbCBoZWFyIHRoZSBsYW1icywgQ2xhcmljZT8KICAgPC9uczpSZXNwb25zZURhdGE+CjwvbnM6U3VjY2Vzc1Jlc3BvbnNlPg==&lt;/Value&gt;&lt;/Context&gt;&lt;/ContextCollection&gt;&lt;/Event&gt;&lt;/UniversalEvent&gt;&lt;/Body&gt;&lt;/ns0:UniversalInterchange&gt;</OI_XmlContent>
</eHubOutboxMessage>
</Rows>
</Insert>
</CompositeOperation>";

			#endregion

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_GBCBResponse_Outcomes_ICSGB_Polling_Success()
		{
			var originalRecipient = "GBCustoms-ICSGB";

			#region Input XML
			var responseXml = @"<outcomeResponse xmlns:cc3=""http://ics.dgtaxud.ec/CC328A"">
  <response>
    <cc3:CC328A>
      <MesSenMES3>GB000012340003/1234567890</MesSenMES3>
      <MesRecMES6>GB000012340003/1234567890</MesRecMES6>
      <DatOfPreMES9>190114</DatOfPreMES9>
      <TimOfPreMES10>0945</TimOfPreMES10>
      <MesIdeMES19>MSUI11235227</MesIdeMES19>
      <MesTypMES20>CC328A</MesTypMES20>
      <CorIdeMES25>0JRF7UncK0t004</CorIdeMES25>
      <HEAHEA>
        <RefNumHEA4>Preeti_315A_TC001</RefNumHEA4>
        <DocNumHEA5>10GB08I01234567891</DocNumHEA5>
        <DecRegDatTimHEA115>201901140945</DecRegDatTimHEA115>
      </HEAHEA>
      <CUSOFFLON>
        <RefNumCOL1>ES000055</RefNumCOL1>
      </CUSOFFLON>
      <PERLODSUMDEC>
        <TINPLD1>GB000012340002</TINPLD1>
      </PERLODSUMDEC>
      <CUSOFFFENT730>
        <RefNumCUSOFFFENT731>GB000011</RefNumCUSOFFFENT731>
      </CUSOFFFENT730>
    </cc3:CC328A>
  </response>
  <acknowledgement method='DELETE' href='/customs/imports/outcomes/0JRF7UncK0t004'/>
</outcomeResponse>";
			#endregion

			#region Expected XML
			var expectedXml = @"<CompositeOperation>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage"">
    <Rows>
      <eHubInboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EI_PK>00000000-0000-0000-0000-000000000000</EI_PK>
        <EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID>
        <EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
        <EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender>
        <EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
        <EI_MessageType>http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse</EI_MessageType>
        <EI_IsFlatFile>false</EI_IsFlatFile>
        <EI_ApplicationCode>CDS</EI_ApplicationCode>
        <EI_Status>2</EI_Status>
        <EI_Content>H4sIAAAAAAAEAIVUW4+iMBR+32T/A+F9pwVkvKSQKKDDrrcIJvs2cUtVMtIaqDsz/36PIMQymG2M4nc57Tk9HDKbeJdCiqyYXIqUs6LYsOIseMHc7980jdT/XtguYbm2zsXfFB4cPfSi2UQvRSBjL5c/cb6jbyk/hL7bv60fHV/1IqhluoXyRJ6z006mggOYCfqmIASpgvKUSD2mevSJSD41T3DJuIw/z8zRfy/muhZwKhLY2dG54KzJRFwkFRmrzdpHduLFiFLL0Y9SnkcIpbR4Sg5y93FJnhhFnmeZg3Hthwj5fQFvGPhHlfAOBXzBiojxRRBZ7myCYRmm1YMfC10f7Of+YIgJulO13RtGAX/+j7tWKW5/J1f7dc6AGbrGEBtGjyAFVORxmtWMgV087NkEqVj7bGFSEkN3EW1DA45km2a/PE/DtC1wP0CY2K2KVYobTBFDF1RRTNvFPzfT/pbTX1hi3Cs7pOEU00swho+ClY2yX14yIHou5MJk+moZ9vg19qCg19Zq2JbPF7RibNfAcAGDEDd1B+Md3TYyumEHKDXUD3jDsF0TVzdQ1bVDoOSBOhIh3jZaTafz1bI7P281N9wguraJbddplaAaujsMWQeb+cqPtgs/8NobxOFyPfcNtQlNaI8brm7wMNItg2mwjPsWfpDFnaTZsLklle3K62twgjpeToK+vMUEBhUX7yeWHFgGo0TLmDyKxNH9YB7Ega4dc7Z3dESrcYrS7CxyWaDbQCmQ2qW6huqRg1ozpzXTrvMLIIIej+p/2UenZsYFAAA=</EI_Content>
      </eHubInboxMessage>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent"">
    <Rows>
      <eHubInboxXmlContent xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox>
        <EX_XmlContent>&lt;GBCustomsBusinessResponse&gt;
  &lt;ResponseHeader Provider=""ICSGB""&gt;
    &lt;eHubTrackingID&gt;77777777-7777-7777-7777-777777777777&lt;/eHubTrackingID&gt;
    &lt;CorrelationID&gt;mockCorrelationID&lt;/CorrelationID&gt;
  &lt;/ResponseHeader&gt;
  &lt;ResponseBody ContentType=""XML"" Encoding=""none""&gt;
    &lt;outcomeResponse xmlns:cc3=""http://ics.dgtaxud.ec/CC328A""&gt;
  &lt;response&gt;
    &lt;cc3:CC328A&gt;
      &lt;MesSenMES3&gt;GB000012340003/1234567890&lt;/MesSenMES3&gt;
      &lt;MesRecMES6&gt;GB000012340003/1234567890&lt;/MesRecMES6&gt;
      &lt;DatOfPreMES9&gt;190114&lt;/DatOfPreMES9&gt;
      &lt;TimOfPreMES10&gt;0945&lt;/TimOfPreMES10&gt;
      &lt;MesIdeMES19&gt;MSUI11235227&lt;/MesIdeMES19&gt;
      &lt;MesTypMES20&gt;CC328A&lt;/MesTypMES20&gt;
      &lt;CorIdeMES25&gt;0JRF7UncK0t004&lt;/CorIdeMES25&gt;
      &lt;HEAHEA&gt;
        &lt;RefNumHEA4&gt;Preeti_315A_TC001&lt;/RefNumHEA4&gt;
        &lt;DocNumHEA5&gt;10GB08I01234567891&lt;/DocNumHEA5&gt;
        &lt;DecRegDatTimHEA115&gt;201901140945&lt;/DecRegDatTimHEA115&gt;
      &lt;/HEAHEA&gt;
      &lt;CUSOFFLON&gt;
        &lt;RefNumCOL1&gt;ES000055&lt;/RefNumCOL1&gt;
      &lt;/CUSOFFLON&gt;
      &lt;PERLODSUMDEC&gt;
        &lt;TINPLD1&gt;GB000012340002&lt;/TINPLD1&gt;
      &lt;/PERLODSUMDEC&gt;
      &lt;CUSOFFFENT730&gt;
        &lt;RefNumCUSOFFFENT731&gt;GB000011&lt;/RefNumCUSOFFFENT731&gt;
      &lt;/CUSOFFFENT730&gt;
    &lt;/cc3:CC328A&gt;
  &lt;/response&gt;
  &lt;acknowledgement method='DELETE' href='/customs/imports/outcomes/0JRF7UncK0t004'/&gt;
&lt;/outcomeResponse&gt;
  &lt;/ResponseBody&gt;
&lt;/GBCustomsBusinessResponse&gt;</EX_XmlContent>
        <EX_DT_Source>12F1150E-4F3F-410F-990C-BAAB866EDC29</EX_DT_Source>
        <EX_UncompressedLength>1478</EX_UncompressedLength>
      </eHubInboxXmlContent>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage"">
    <Rows>
      <eHubOutboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <OI_PK>00000000-0000-0000-0000-000000000000</OI_PK>
        <OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID>
        <OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK>
        <OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender>
        <OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
        <OI_DT_Target>12F1150E-4F3F-410F-990C-BAAB866EDC29</OI_DT_Target>
        <OI_Status>0</OI_Status>
        <OI_Content>H4sIAAAAAAAEAIVUW4+iMBR+32T/A+F9pwVkvKSQKKDDrrcIJvs2cUtVMtIaqDsz/36PIMQymG2M4nc57Tk9HDKbeJdCiqyYXIqUs6LYsOIseMHc7980jdT/XtguYbm2zsXfFB4cPfSi2UQvRSBjL5c/cb6jbyk/hL7bv60fHV/1IqhluoXyRJ6z006mggOYCfqmIASpgvKUSD2mevSJSD41T3DJuIw/z8zRfy/muhZwKhLY2dG54KzJRFwkFRmrzdpHduLFiFLL0Y9SnkcIpbR4Sg5y93FJnhhFnmeZg3Hthwj5fQFvGPhHlfAOBXzBiojxRRBZ7myCYRmm1YMfC10f7Of+YIgJulO13RtGAX/+j7tWKW5/J1f7dc6AGbrGEBtGjyAFVORxmtWMgV087NkEqVj7bGFSEkN3EW1DA45km2a/PE/DtC1wP0CY2K2KVYobTBFDF1RRTNvFPzfT/pbTX1hi3Cs7pOEU00swho+ClY2yX14yIHou5MJk+moZ9vg19qCg19Zq2JbPF7RibNfAcAGDEDd1B+Md3TYyumEHKDXUD3jDsF0TVzdQ1bVDoOSBOhIh3jZaTafz1bI7P281N9wguraJbddplaAaujsMWQeb+cqPtgs/8NobxOFyPfcNtQlNaI8brm7wMNItg2mwjPsWfpDFnaTZsLklle3K62twgjpeToK+vMUEBhUX7yeWHFgGo0TLmDyKxNH9YB7Ega4dc7Z3dESrcYrS7CxyWaDbQCmQ2qW6huqRg1ozpzXTrvMLIIIej+p/2UenZsYFAAA=</OI_Content>
        <OI_XmlContent>&lt;GBCustomsBusinessResponse&gt;
  &lt;ResponseHeader Provider=""ICSGB""&gt;
    &lt;eHubTrackingID&gt;77777777-7777-7777-7777-777777777777&lt;/eHubTrackingID&gt;
    &lt;CorrelationID&gt;mockCorrelationID&lt;/CorrelationID&gt;
  &lt;/ResponseHeader&gt;
  &lt;ResponseBody ContentType=""XML"" Encoding=""none""&gt;
    &lt;outcomeResponse xmlns:cc3=""http://ics.dgtaxud.ec/CC328A""&gt;
  &lt;response&gt;
    &lt;cc3:CC328A&gt;
      &lt;MesSenMES3&gt;GB000012340003/1234567890&lt;/MesSenMES3&gt;
      &lt;MesRecMES6&gt;GB000012340003/1234567890&lt;/MesRecMES6&gt;
      &lt;DatOfPreMES9&gt;190114&lt;/DatOfPreMES9&gt;
      &lt;TimOfPreMES10&gt;0945&lt;/TimOfPreMES10&gt;
      &lt;MesIdeMES19&gt;MSUI11235227&lt;/MesIdeMES19&gt;
      &lt;MesTypMES20&gt;CC328A&lt;/MesTypMES20&gt;
      &lt;CorIdeMES25&gt;0JRF7UncK0t004&lt;/CorIdeMES25&gt;
      &lt;HEAHEA&gt;
        &lt;RefNumHEA4&gt;Preeti_315A_TC001&lt;/RefNumHEA4&gt;
        &lt;DocNumHEA5&gt;10GB08I01234567891&lt;/DocNumHEA5&gt;
        &lt;DecRegDatTimHEA115&gt;201901140945&lt;/DecRegDatTimHEA115&gt;
      &lt;/HEAHEA&gt;
      &lt;CUSOFFLON&gt;
        &lt;RefNumCOL1&gt;ES000055&lt;/RefNumCOL1&gt;
      &lt;/CUSOFFLON&gt;
      &lt;PERLODSUMDEC&gt;
        &lt;TINPLD1&gt;GB000012340002&lt;/TINPLD1&gt;
      &lt;/PERLODSUMDEC&gt;
      &lt;CUSOFFFENT730&gt;
        &lt;RefNumCUSOFFFENT731&gt;GB000011&lt;/RefNumCUSOFFFENT731&gt;
      &lt;/CUSOFFFENT730&gt;
    &lt;/cc3:CC328A&gt;
  &lt;/response&gt;
  &lt;acknowledgement method='DELETE' href='/customs/imports/outcomes/0JRF7UncK0t004'/&gt;
&lt;/outcomeResponse&gt;
  &lt;/ResponseBody&gt;
&lt;/GBCustomsBusinessResponse&gt;</OI_XmlContent>
      </eHubOutboxMessage>
    </Rows>
  </Insert>
</CompositeOperation>";
			#endregion

			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "Polling", "WTLDTWJLI", responseXml, null,
				gbCustomsXml, true, "77777777-7777-7777-7777-777777777777", "mockServiceReference", "mockRequestID", "mockCorrelationID");

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_GBCBResponse_Notification_ICSGB_Polling_Success()
		{
			var originalRecipient = "GBCustoms-ICSGB";

			#region Input XML
			var responseXml = @"<notificationResponse xmlns:cc3=""http://ics.dgtaxud.ec/CC351A"">
  <response>
    <cc3:CC351A>
      <MesSenMES3>GBCD1234/1234567890</MesSenMES3>
      <MesRecMES6>GBC123</MesRecMES6>
      <DatOfPreMES9>030211</DatOfPreMES9>
      <TimOfPreMES10>0123</TimOfPreMES10>
      <MesIdeMES19>ABC123</MesIdeMES19>
      <MesTypMES20>CC313A</MesTypMES20>
      <CorIdeMES25>ABC123</CorIdeMES25>
      <HEAHEA>
        <RefNumHEA4>ABCD1234</RefNumHEA4>
        <DocNumHEA5>12AB3C4D5E6F7G8H90</DocNumHEA5>
        <TraModAtBorHEA76>4</TraModAtBorHEA76>
        <NatHEA001>GB</NatHEA001>
        <IdeOfMeaOfTraCroHEA85>ABC123</IdeOfMeaOfTraCroHEA85>
        <TotNumOfIteHEA305>42</TotNumOfIteHEA305>
        <ComRefNumHEA>ABC123</ComRefNumHEA>
        <ConRefNumHEA>ABC123</ConRefNumHEA>
        <NotDatTimHEA104>200302111234</NotDatTimHEA104>
        <DecRegDatTimHEA115>200302111234</DecRegDatTimHEA115>
        <DecSubDatTimHEA118>200302111234</DecSubDatTimHEA118>
      </HEAHEA>
      <GOOITEGDS>…</GOOITEGDS> <!-- content of this element removed -->
      <CUSOFFLON>
        <RefNumCOL1>ES000055</RefNumCOL1>
      </CUSOFFLON>
      <TRAREP>…</TRAREP> <!-- content of this element removed -->
      <PERLODSUMDEC>…</PERLODSUMDEC> <!-- content of this element removed -->
      <CUSOFFFENT730>…</CUSOFFFENT730> <!-- content of this element removed -->
      <TRACARENT601>…</TRACARENT601> <!-- content of this element removed -->
      <CUSINT632>…</CUSINT632> <!-- content of this element removed -->
    </cc3:CC351A>
  </response>
  <acknowledgement method='DELETE' href='/customs/imports/notifications/0JRF7UncK0t004'/>
</notificationResponse>";
			#endregion

			#region Expected XML
			var expectedXml = @"<CompositeOperation>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage"">
    <Rows>
      <eHubInboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EI_PK>00000000-0000-0000-0000-000000000000</EI_PK>
        <EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID>
        <EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
        <EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender>
        <EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
        <EI_MessageType>http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse</EI_MessageType>
        <EI_IsFlatFile>false</EI_IsFlatFile>
        <EI_ApplicationCode>CDS</EI_ApplicationCode>
        <EI_Status>2</EI_Status>
        <EI_Content>H4sIAAAAAAAEAK1WwY6iQBC9b7L/wHJ3uwFRZ4IkCui4q2KESfbqQKNkpNtAOztz26/ZD9sv2RJE6ZY5zRBD6FevXhdV1RbWdOwcC86yYnwsUkqKYk2KA6MFsb9+URSrXj2QTUxyZZWzlxQehurMCaZjtSQBjTwcn8J8Ez2ndDtz7f756rTc6stCktNZymF5TvYbnjIKYMaiZwGxkEgoo0RimGLoYxa/KQ6jnFAevh3IUP21mKuKRyMWw85DlTJKLm9CGU+TNCrlawXlNdvT4j6KjKG64/xwj1AaFd/jLd+8HuPvJEKOY5jaqBYBmbyZxTMG/vcVsYECviBFQOjCCwwbquFqutFFp5vZ6w/usIUadtlvTSLAeyc/8CipNSRQ3Q33k1VOwHJnYwPrmmYhARToYZrVFg3buJQWMTmQWVwa7uzRNZILJpOhCmDQsQ3Z0IxRSb5gAhlqXano5kW5iQnkB28EPwEr2yBZHjMwdE8KZXZP/XIBJbrLospi2po+GhtO1zW93qQ/HTycatEwS47QyQsWj/iY5WDu92zY5gaTfJYbDjjGGhTQQteVRIPX9ZMF2fgJCDo5A9bgmo92qxwd4xC3n8w4AbuBTburQ3w3qOTmsOySqkYBGuCNA21zoO86LBmHPoTmApuGu7aOq/as6iRb5WKRaE22V4ZmSv4thFuJ4PjUYAxuJWSC0HWope2sqe/PQm/qBva/P38tdF0KrG+djhJVf0wKSxS+SwuF7El2WuckYy8kVjod6UQ8Bv5kMveX7X3u+HPN9gIMl2nWfV6CYtDtMla4Hq29VRX0+fnDEa+89dx3g8eF6zmVsoB8UkYm3jLsG7jaQIQ+vANkwoFcLMMenM06N1fkM95gBlKGfon+vPyYsoVaJo6FbkaTBSOYst97Em8rtYzwHYuHquvNvdBTlV1OkqGKoupDAaXZgeW8QM1RWSD8Yz3pP9LoJ+YYd1UF1RMVtY1UaW6fZjRAcFLe/Rz5D5ZbeUuqCAAA</EI_Content>
      </eHubInboxMessage>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent"">
    <Rows>
      <eHubInboxXmlContent xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox>
        <EX_XmlContent>&lt;GBCustomsBusinessResponse&gt;
  &lt;ResponseHeader Provider=""ICSGB""&gt;
    &lt;eHubTrackingID&gt;77777777-7777-7777-7777-777777777777&lt;/eHubTrackingID&gt;
    &lt;CorrelationID&gt;mockCorrelationID&lt;/CorrelationID&gt;
  &lt;/ResponseHeader&gt;
  &lt;ResponseBody ContentType=""XML"" Encoding=""none""&gt;
    &lt;notificationResponse xmlns:cc3=""http://ics.dgtaxud.ec/CC351A""&gt;
  &lt;response&gt;
    &lt;cc3:CC351A&gt;
      &lt;MesSenMES3&gt;GBCD1234/1234567890&lt;/MesSenMES3&gt;
      &lt;MesRecMES6&gt;GBC123&lt;/MesRecMES6&gt;
      &lt;DatOfPreMES9&gt;030211&lt;/DatOfPreMES9&gt;
      &lt;TimOfPreMES10&gt;0123&lt;/TimOfPreMES10&gt;
      &lt;MesIdeMES19&gt;ABC123&lt;/MesIdeMES19&gt;
      &lt;MesTypMES20&gt;CC313A&lt;/MesTypMES20&gt;
      &lt;CorIdeMES25&gt;ABC123&lt;/CorIdeMES25&gt;
      &lt;HEAHEA&gt;
        &lt;RefNumHEA4&gt;ABCD1234&lt;/RefNumHEA4&gt;
        &lt;DocNumHEA5&gt;12AB3C4D5E6F7G8H90&lt;/DocNumHEA5&gt;
        &lt;TraModAtBorHEA76&gt;4&lt;/TraModAtBorHEA76&gt;
        &lt;NatHEA001&gt;GB&lt;/NatHEA001&gt;
        &lt;IdeOfMeaOfTraCroHEA85&gt;ABC123&lt;/IdeOfMeaOfTraCroHEA85&gt;
        &lt;TotNumOfIteHEA305&gt;42&lt;/TotNumOfIteHEA305&gt;
        &lt;ComRefNumHEA&gt;ABC123&lt;/ComRefNumHEA&gt;
        &lt;ConRefNumHEA&gt;ABC123&lt;/ConRefNumHEA&gt;
        &lt;NotDatTimHEA104&gt;200302111234&lt;/NotDatTimHEA104&gt;
        &lt;DecRegDatTimHEA115&gt;200302111234&lt;/DecRegDatTimHEA115&gt;
        &lt;DecSubDatTimHEA118&gt;200302111234&lt;/DecSubDatTimHEA118&gt;
      &lt;/HEAHEA&gt;
      &lt;GOOITEGDS&gt;…&lt;/GOOITEGDS&gt; &lt;!-- content of this element removed --&gt;
      &lt;CUSOFFLON&gt;
        &lt;RefNumCOL1&gt;ES000055&lt;/RefNumCOL1&gt;
      &lt;/CUSOFFLON&gt;
      &lt;TRAREP&gt;…&lt;/TRAREP&gt; &lt;!-- content of this element removed --&gt;
      &lt;PERLODSUMDEC&gt;…&lt;/PERLODSUMDEC&gt; &lt;!-- content of this element removed --&gt;
      &lt;CUSOFFFENT730&gt;…&lt;/CUSOFFFENT730&gt; &lt;!-- content of this element removed --&gt;
      &lt;TRACARENT601&gt;…&lt;/TRACARENT601&gt; &lt;!-- content of this element removed --&gt;
      &lt;CUSINT632&gt;…&lt;/CUSINT632&gt; &lt;!-- content of this element removed --&gt;
    &lt;/cc3:CC351A&gt;
  &lt;/response&gt;
  &lt;acknowledgement method='DELETE' href='/customs/imports/notifications/0JRF7UncK0t004'/&gt;
&lt;/notificationResponse&gt;
  &lt;/ResponseBody&gt;
&lt;/GBCustomsBusinessResponse&gt;</EX_XmlContent>
        <EX_DT_Source>12F1150E-4F3F-410F-990C-BAAB866EDC29</EX_DT_Source>
        <EX_UncompressedLength>2218</EX_UncompressedLength>
      </eHubInboxXmlContent>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage"">
    <Rows>
      <eHubOutboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <OI_PK>00000000-0000-0000-0000-000000000000</OI_PK>
        <OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID>
        <OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK>
        <OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender>
        <OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
        <OI_DT_Target>12F1150E-4F3F-410F-990C-BAAB866EDC29</OI_DT_Target>
        <OI_Status>0</OI_Status>
        <OI_Content>H4sIAAAAAAAEAK1WwY6iQBC9b7L/wHJ3uwFRZ4IkCui4q2KESfbqQKNkpNtAOztz26/ZD9sv2RJE6ZY5zRBD6FevXhdV1RbWdOwcC86yYnwsUkqKYk2KA6MFsb9+URSrXj2QTUxyZZWzlxQehurMCaZjtSQBjTwcn8J8Ez2ndDtz7f756rTc6stCktNZymF5TvYbnjIKYMaiZwGxkEgoo0RimGLoYxa/KQ6jnFAevh3IUP21mKuKRyMWw85DlTJKLm9CGU+TNCrlawXlNdvT4j6KjKG64/xwj1AaFd/jLd+8HuPvJEKOY5jaqBYBmbyZxTMG/vcVsYECviBFQOjCCwwbquFqutFFp5vZ6w/usIUadtlvTSLAeyc/8CipNSRQ3Q33k1VOwHJnYwPrmmYhARToYZrVFg3buJQWMTmQWVwa7uzRNZILJpOhCmDQsQ3Z0IxRSb5gAhlqXano5kW5iQnkB28EPwEr2yBZHjMwdE8KZXZP/XIBJbrLospi2po+GhtO1zW93qQ/HTycatEwS47QyQsWj/iY5WDu92zY5gaTfJYbDjjGGhTQQteVRIPX9ZMF2fgJCDo5A9bgmo92qxwd4xC3n8w4AbuBTburQ3w3qOTmsOySqkYBGuCNA21zoO86LBmHPoTmApuGu7aOq/as6iRb5WKRaE22V4ZmSv4thFuJ4PjUYAxuJWSC0HWope2sqe/PQm/qBva/P38tdF0KrG+djhJVf0wKSxS+SwuF7El2WuckYy8kVjod6UQ8Bv5kMveX7X3u+HPN9gIMl2nWfV6CYtDtMla4Hq29VRX0+fnDEa+89dx3g8eF6zmVsoB8UkYm3jLsG7jaQIQ+vANkwoFcLMMenM06N1fkM95gBlKGfon+vPyYsoVaJo6FbkaTBSOYst97Em8rtYzwHYuHquvNvdBTlV1OkqGKoupDAaXZgeW8QM1RWSD8Yz3pP9LoJ+YYd1UF1RMVtY1UaW6fZjRAcFLe/Rz5D5ZbeUuqCAAA</OI_Content>
        <OI_XmlContent>&lt;GBCustomsBusinessResponse&gt;
  &lt;ResponseHeader Provider=""ICSGB""&gt;
    &lt;eHubTrackingID&gt;77777777-7777-7777-7777-777777777777&lt;/eHubTrackingID&gt;
    &lt;CorrelationID&gt;mockCorrelationID&lt;/CorrelationID&gt;
  &lt;/ResponseHeader&gt;
  &lt;ResponseBody ContentType=""XML"" Encoding=""none""&gt;
    &lt;notificationResponse xmlns:cc3=""http://ics.dgtaxud.ec/CC351A""&gt;
  &lt;response&gt;
    &lt;cc3:CC351A&gt;
      &lt;MesSenMES3&gt;GBCD1234/1234567890&lt;/MesSenMES3&gt;
      &lt;MesRecMES6&gt;GBC123&lt;/MesRecMES6&gt;
      &lt;DatOfPreMES9&gt;030211&lt;/DatOfPreMES9&gt;
      &lt;TimOfPreMES10&gt;0123&lt;/TimOfPreMES10&gt;
      &lt;MesIdeMES19&gt;ABC123&lt;/MesIdeMES19&gt;
      &lt;MesTypMES20&gt;CC313A&lt;/MesTypMES20&gt;
      &lt;CorIdeMES25&gt;ABC123&lt;/CorIdeMES25&gt;
      &lt;HEAHEA&gt;
        &lt;RefNumHEA4&gt;ABCD1234&lt;/RefNumHEA4&gt;
        &lt;DocNumHEA5&gt;12AB3C4D5E6F7G8H90&lt;/DocNumHEA5&gt;
        &lt;TraModAtBorHEA76&gt;4&lt;/TraModAtBorHEA76&gt;
        &lt;NatHEA001&gt;GB&lt;/NatHEA001&gt;
        &lt;IdeOfMeaOfTraCroHEA85&gt;ABC123&lt;/IdeOfMeaOfTraCroHEA85&gt;
        &lt;TotNumOfIteHEA305&gt;42&lt;/TotNumOfIteHEA305&gt;
        &lt;ComRefNumHEA&gt;ABC123&lt;/ComRefNumHEA&gt;
        &lt;ConRefNumHEA&gt;ABC123&lt;/ConRefNumHEA&gt;
        &lt;NotDatTimHEA104&gt;200302111234&lt;/NotDatTimHEA104&gt;
        &lt;DecRegDatTimHEA115&gt;200302111234&lt;/DecRegDatTimHEA115&gt;
        &lt;DecSubDatTimHEA118&gt;200302111234&lt;/DecSubDatTimHEA118&gt;
      &lt;/HEAHEA&gt;
      &lt;GOOITEGDS&gt;…&lt;/GOOITEGDS&gt; &lt;!-- content of this element removed --&gt;
      &lt;CUSOFFLON&gt;
        &lt;RefNumCOL1&gt;ES000055&lt;/RefNumCOL1&gt;
      &lt;/CUSOFFLON&gt;
      &lt;TRAREP&gt;…&lt;/TRAREP&gt; &lt;!-- content of this element removed --&gt;
      &lt;PERLODSUMDEC&gt;…&lt;/PERLODSUMDEC&gt; &lt;!-- content of this element removed --&gt;
      &lt;CUSOFFFENT730&gt;…&lt;/CUSOFFFENT730&gt; &lt;!-- content of this element removed --&gt;
      &lt;TRACARENT601&gt;…&lt;/TRACARENT601&gt; &lt;!-- content of this element removed --&gt;
      &lt;CUSINT632&gt;…&lt;/CUSINT632&gt; &lt;!-- content of this element removed --&gt;
    &lt;/cc3:CC351A&gt;
  &lt;/response&gt;
  &lt;acknowledgement method='DELETE' href='/customs/imports/notifications/0JRF7UncK0t004'/&gt;
&lt;/notificationResponse&gt;
  &lt;/ResponseBody&gt;
&lt;/GBCustomsBusinessResponse&gt;</OI_XmlContent>
      </eHubOutboxMessage>
    </Rows>
  </Insert>
</CompositeOperation>";
			#endregion

			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "Polling", "WTLDTWJLI", responseXml, null,
				gbCustomsXml, true, "77777777-7777-7777-7777-777777777777", "mockServiceReference", "mockRequestID", "mockCorrelationID");

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_CTCLargeFile_ExcludeBodyFromSubscription()
		{
			var originalRecipient = "GBCustoms-CTCGB";

			#region Input XML
			var responseXml = @"<ns:SuccessResponse xmlns:ns=""http://www.hmrc.gov.uk/successresponse/2"" xmlns=""http://www.govtalk.gov.uk/enforcement/ICS/responsedata/7"">
   <ns:ResponseData>
       <CorrelationId>0JRF7UncK0t004</CorrelationId>
       <WhateverId>666</WhateverId>
   </ns:ResponseData>
</ns:SuccessResponse>";

			var outboundXml = @"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
    <Header>
        <GBCustomsRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
            xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
            <Provider>CTCGB</Provider>
            <Service>Depart</Service>
            <Credentials key=""HYEMIK.GB048834222514.WPR""></Credentials>
            <JobNumber>NCT00009891</JobNumber>
            <ServiceReference></ServiceReference>
            <Version>2.0</Version>
            <ContentType>XML</ContentType>
            <Accept>application/vnd.hmrc.2.0+json</Accept>
            <LargeFile>true</LargeFile>
        </GBCustomsRequest>
    </Header>
    <q1:cc015c xmlns:q1=""http://ncts.dgtaxud.ec"">
        <messagesender>GB945390992000</messagesender>
        <messagerecipient>NTA.GB</messagerecipient>
        <preparationdateandtime>2023-10-12T12:57:11</preparationdateandtime>
        <messageidentification>
            <SENDERSREFERENCEPLACEHOLDER/>
        </messageidentification>
        <messagetype>CC015C</messagetype>
        <correlationidentifier>
            <SENDERSREFERENCEPLACEHOLDER/>
        </correlationidentifier>
        <transitoperation>
            <lrn>239453909920BB00000013</lrn>
            <declarationtype>T1</declarationtype>
            <additionaldeclarationtype>A</additionaldeclarationtype>
            <security>0</security>
            <reduceddatasetindicator>0</reduceddatasetindicator>
            <communicationlanguageatdeparture>en</communicationlanguageatdeparture>
            <bindingitinerary>0</bindingitinerary>
        </transitoperation>
        <customsofficeofdeparture>
            <referencenumber>GB000060</referencenumber>
        </customsofficeofdeparture>
        <customsofficeofdestinationdeclared>
            <referencenumber>GB000060</referencenumber>
        </customsofficeofdestinationdeclared>
        <customsofficeoftransitdeclared>
            <sequencenumber>1</sequencenumber>
            <referencenumber>GB000060</referencenumber>
        </customsofficeoftransitdeclared>
        <holderofthetransitprocedure>
            <identificationnumber>GB953574106000</identificationnumber>
        </holderofthetransitprocedure>
        <representative>
            <identificationnumber>GB953574106000</identificationnumber>
            <status>2</status>
        </representative>
        <guarantee>
            <sequencenumber>1</sequencenumber>
            <guaranteetype>0</guaranteetype>
            <guaranteereference>
                <sequencenumber>1</sequencenumber>
                <grn>23GB0000010000058</grn>
                <accesscode>AC01</accesscode>
                <amounttobecovered>100</amounttobecovered>
                <currency>GBP</currency>
            </guaranteereference>
        </guarantee>
        <consignment>
            <countryofdestination>GB</countryofdestination>
            <containerindicator>1</containerindicator>
            <inlandmodeoftransport>3</inlandmodeoftransport>
            <modeoftransportattheborder>3</modeoftransportattheborder>
            <grossmass>35</grossmass>
            <consignor>
                <identificationnumber>GB954131533000</identificationnumber>
            </consignor>
            <consignee>
                <identificationnumber>GB954131533000</identificationnumber>
            </consignee>
            <transportequipment>
                <sequencenumber>1</sequencenumber>
                <containeridentificationnumber>DANU1234565</containeridentificationnumber>
                <numberofseals>2</numberofseals>
                <seal>
                    <sequencenumber>1</sequencenumber>
                    <identifier>s1</identifier>
                </seal>
                <seal>
                    <sequencenumber>2</sequencenumber>
                    <identifier>s2</identifier>
                </seal>
            </transportequipment>
            <activebordertransportmeans>
                <sequencenumber>1</sequencenumber>
                <customsofficeatborderreferencenumber>BE101000</customsofficeatborderreferencenumber>
                <typeofidentification>40</typeofidentification>
                <identificationnumber>IATA0000</identificationnumber>
                <nationality>BE</nationality>
                <conveyancereferencenumber>ffffffffffff</conveyancereferencenumber>
            </activebordertransportmeans>
            <houseconsignment>
                <sequencenumber>1</sequencenumber>
                <grossmass>35</grossmass>
                <consignmentitem>
                    <goodsitemnumber>1</goodsitemnumber>
                    <declarationgoodsitemnumber>1</declarationgoodsitemnumber>
                    <commodity>
                        <descriptionofgoods>sssss</descriptionofgoods>
                        <commoditycode>
                            <harmonizedsystemsubheadingcode>610711</harmonizedsystemsubheadingcode>
                        </commoditycode>
                        <goodsmeasure>
                            <grossmass>30</grossmass>
                            <netmass>30</netmass>
                        </goodsmeasure>
                    </commodity>
                    <packaging>
                        <sequencenumber>1</sequencenumber>
                        <typeofpackages>BX</typeofpackages>
                        <numberofpackages>10</numberofpackages>
                        <shippingmarks>red</shippingmarks>
                    </packaging>
                </consignmentitem>
            </houseconsignment>
        </consignment>
    </q1:cc015c>
</ns0:GBCustoms>";
			#endregion

			var mockContext = GenerateMockContext(originalRecipient);
			var mockClientRegistrationAccessor = MockClientRegistrationAccessor(originalRecipient);
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(
				"GBCTID",
				"GBCustoms-CTCGB",
				"WTLDTWJLI",
				"0JRF7UncK0t004",
				@"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms""><Header><GBCustomsRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><Provider>CTCGB</Provider><Service>Depart</Service><Credentials key=""HYEMIK.GB048834222514.WPR""></Credentials><JobNumber>NCT00009891</JobNumber><ServiceReference></ServiceReference><Version>2.0</Version><ContentType>XML</ContentType><Accept>application/vnd.hmrc.2.0+json</Accept><LargeFile>true</LargeFile></GBCustomsRequest></Header></ns0:GBCustoms>",
				"Depart")).Repeat.Once();

			OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;
			OrchestrationHelpers.DataModelAccessor = () => mockDataModelAccessor;
			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			_ = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(
				MockRepository.GenerateStub<ILog>(),
				originalRecipient,
				"Create",
				"WTLDTWJLI",
				responseXml,
				null,
				outboundXml,
				true,
				"77777777-7777-7777-7777-777777777777",
				"mockServiceReference",
				"mockRequestID",
				"mockCorrelationID");
		}

		[TestMethod]
		public void TestExtractCorrelationIDs_JSON()
		{
			var originalRecipient = "GBCustomsTest-GVMS";

			var responseJSON = @"{
  ""notificationId"": ""1ed5f407-8096-40d1-87ef-9a2a103eeb85"",
  ""boxId"": ""50dca3fc-c37c-4f03-b719-63571333624c"",
  ""messageContentType"": ""application/json"",
  ""message"": ""{\""key\"":\""value\""}"",
  ""status"": ""RECEIVED"",
  ""createdDateTime"": ""2020-06-01T10:20:23.160+0000""}";

			TestExtractCorrelationIDs(originalRecipient, responseJSON, "JSONBody:notificationId", "1ed5f407-8096-40d1-87ef-9a2a103eeb85");
		}

		[TestMethod]
		public void TestExtractCorrelationIDs_HttpHeaders()
		{
			var originalRecipient = "GBCustomsTest-GVMS";

			var responseJSON = @"{
  ""boxId"": ""50dca3fc-c37c-4f03-b719-63571333624c"",
  ""messageContentType"": ""application/json"",
  ""message"": ""{\""key\"":\""value\""}"",
  ""status"": ""RECEIVED"",
  ""createdDateTime"": ""2020-06-01T10:20:23.160+0000""}";

			HttpHeader[] headers = new HttpHeader[2];
			headers[0] = HttpHeader.Create("Content-Type", "application/json; charset=utf-8");
			headers[1] = HttpHeader.Create("NotificationId", "1ed5f407-8096-40d1-87ef-9a2a103eeb85");


			TestExtractCorrelationIDs(originalRecipient, responseJSON, "Header:NotificationId", "1ed5f407-8096-40d1-87ef-9a2a103eeb85", headers);
		}

		[TestMethod]
		public void TestExtractCorrelationIDs_ParameterInHeaders()
		{
			var originalRecipient = "GBCustomsTest-CTCGB";

			HttpHeader[] headers = new HttpHeader[2];
			headers[0] = HttpHeader.Create("Content-Type", "application/json; charset=utf-8");
			headers[1] = HttpHeader.Create("Location", "/customs/transits/movements/arrivals/123456/messages/1");


			TestExtractCorrelationIDs(originalRecipient, "", @"Parameter:/customs/transits/movements/arrivals/(?<CorrelationId>\d+)/messages/1", "123456", headers);
		}

		[TestMethod]
		public void TestUpdateSubscription_New_Polling()
		{
			var subscriptionReferenceTypeValues = new Dictionary<Guid, string>();
			var recipient = "GBCustomsTest_CTCGB";
			var mockContext = GenerateMockContext(recipient);
			var mockLogger = MockRepository.GenerateMock<ILog>();

			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.Now = () => new DateTime(2020, 01, 01, 00, 00, 00);

			OrchestrationHelpers.UpdateSubscription(mockLogger, recipient, "WTLDTWJLI", new DateTime(2020, 01, 01, 00, 00, 00), false);

			var subscriptions = mockContext.eHubSubscriptionValues;
			foreach (var subscription in subscriptions)
			{
				subscriptionReferenceTypeValues.Add(subscription.SV_PK, subscription.SV_ReferenceType);
			}

			Assert.AreEqual("New|2020-01-01T00:00:00", subscriptionReferenceTypeValues[Guid.Parse("55555555-5555-5555-5555-555555555555")]);
			Assert.AreEqual("Blah", subscriptionReferenceTypeValues[Guid.Parse("66666666-6666-6666-6666-666666666666")]);
			Assert.AreEqual("New|2020-01-01T00:00:00", subscriptionReferenceTypeValues[Guid.Parse("77777777-7777-7777-7777-777777777777")]);
			Assert.AreEqual("New|2020-01-01T00:00:00", subscriptionReferenceTypeValues[Guid.Parse("88888888-8888-8888-8888-888888888888")]);
			Assert.AreEqual("Polling|2020-01-01T00:00:00", subscriptionReferenceTypeValues[Guid.Parse("99999999-9999-9999-9999-999999999999")]);
			Assert.AreEqual("Polling|2020-01-01T00:00:00", subscriptionReferenceTypeValues[Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA")]);
			Assert.AreEqual("ReceivedLaLaLa", subscriptionReferenceTypeValues[Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB")]);
		}

		[TestMethod]
		public void TestUpdateSubscription_Received()
		{
			var subscriptionReferenceTypeValues = new Dictionary<Guid, string>();
			var recipient = "GBCustomsTest_CTCGB";
			var mockContext = GenerateMockContext(recipient);
			var mockLogger = MockRepository.GenerateMock<ILog>();

			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.Now = () => new DateTime(2020, 01, 01, 00, 00, 00);

			OrchestrationHelpers.UpdateSubscription(mockLogger, recipient, "WTLDTWJLI", new DateTime(2020, 01, 01, 00, 00, 00), true);

			var subscriptions = mockContext.eHubSubscriptionValues;
			foreach (var subscription in subscriptions)
			{
				subscriptionReferenceTypeValues.Add(subscription.SV_PK, subscription.SV_ReferenceType);
			}

			Assert.AreEqual("Received|2020-01-01T00:00:00", subscriptionReferenceTypeValues[Guid.Parse("55555555-5555-5555-5555-555555555555")]);
			Assert.AreEqual("Blah", subscriptionReferenceTypeValues[Guid.Parse("66666666-6666-6666-6666-666666666666")]);
			Assert.AreEqual("Received|2020-01-01T00:00:00", subscriptionReferenceTypeValues[Guid.Parse("77777777-7777-7777-7777-777777777777")]);
			Assert.AreEqual("Received|2020-01-01T00:00:00", subscriptionReferenceTypeValues[Guid.Parse("88888888-8888-8888-8888-888888888888")]);
			Assert.AreEqual("Received|2020-01-01T00:00:00", subscriptionReferenceTypeValues[Guid.Parse("99999999-9999-9999-9999-999999999999")]);
			Assert.AreEqual("Received|2020-01-01T00:00:00", subscriptionReferenceTypeValues[Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA")]);
			Assert.AreEqual("ReceivedLaLaLa", subscriptionReferenceTypeValues[Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB")]);
		}

		[TestMethod]
		public void TestUpdateSubscription_AfterPollingStart()
		{
			var subscriptionReferenceTypeValues = new Dictionary<Guid, string>();
			var recipient = "GBCustomsTest_CTCGB";
			var mockContext = GenerateMockContext(recipient);
			var mockLogger = MockRepository.GenerateMock<ILog>();

			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.Now = () => new DateTime(2020, 01, 01, 00, 00, 00);

			OrchestrationHelpers.UpdateSubscription(mockLogger, recipient, "WTLDTWJLI", new DateTime(2018, 01, 01, 00, 00, 00), false);

			var subscriptions = mockContext.eHubSubscriptionValues;
			foreach (var subscription in subscriptions)
			{
				subscriptionReferenceTypeValues.Add(subscription.SV_PK, subscription.SV_ReferenceType);
			}

			Assert.AreEqual("Blah", subscriptionReferenceTypeValues[Guid.Parse("55555555-5555-5555-5555-555555555555")]);
			Assert.AreEqual("Blah", subscriptionReferenceTypeValues[Guid.Parse("66666666-6666-6666-6666-666666666666")]);
			Assert.AreEqual(null, subscriptionReferenceTypeValues[Guid.Parse("77777777-7777-7777-7777-777777777777")]);
			Assert.AreEqual("Blah", subscriptionReferenceTypeValues[Guid.Parse("88888888-8888-8888-8888-888888888888")]);
			Assert.AreEqual("NewLaLaLa", subscriptionReferenceTypeValues[Guid.Parse("99999999-9999-9999-9999-999999999999")]);
			Assert.AreEqual("PollingLaLaLa", subscriptionReferenceTypeValues[Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA")]);
			Assert.AreEqual("ReceivedLaLaLa", subscriptionReferenceTypeValues[Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB")]);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException), "GBCustoms TL UpdateSubscription: Supplied parameters are invalid - originalRecipient [] originalSender [Sender])")]
		public void TestUpdateSubscription_ParameterError_Recipient()
		{
			var mockLogger = MockRepository.GenerateStrictMock<ILog>();
			mockLogger.Expect(x => x.Error("GBCustoms TL UpdateSubscription: Supplied parameters are invalid - originalRecipient [] originalSender [Sender]"));

			OrchestrationHelpers.UpdateSubscription(mockLogger, "", "Sender", new DateTime(2020, 01, 01, 00, 00, 00), false);

			mockLogger.VerifyAllExpectations();
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException), "GBCustoms TL UpdateSubscription: Supplied parameters are invalid - originalRecipient [Recipient] originalSender [])")]
		public void TestUpdateSubscription_ParameterError_Sender()
		{
			var mockLogger = MockRepository.GenerateStrictMock<ILog>();
			mockLogger.Expect(x => x.Error("GBCustoms TL UpdateSubscription: Supplied parameters are invalid - originalRecipient [Recipient] originalSender []"));

			OrchestrationHelpers.UpdateSubscription(mockLogger, "Recipient", "", new DateTime(2020, 01, 01, 00, 00, 00), false);

			mockLogger.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_GVMS()
		{
			var originalRecipient = "GBCustoms-GVMS";

			var responseJSON = @"{
  ""notificationId"": ""1ed5f407-8096-40d1-87ef-9a2a103eeb85"",
  ""boxId"": ""50dca3fc-c37c-4f03-b719-63571333624c"",
  ""messageContentType"": ""application/json"",
  ""message"": ""{\""key\"":\""value\""}"",
  ""status"": ""RECEIVED"",
  ""createdDateTime"": ""2020-06-01T10:20:23.160+0000""}";

			var mockClientRegistrationAccessor = MockClientRegistrationAccessor(originalRecipient);

			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;
			OrchestrationHelpers.DataModelAccessor = () => mockDataModelAccessor;
			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.Now = () => new DateTime(2020, 12, 16, 12, 34, 56);
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "Create", "WTLDTWJLI", responseJSON, null,
				gbCustomsXml, true, "66666666-6666-6666-6666-666666666666", "", "");

			#region Expected XML

			var expectedXml = @"<CompositeOperation>
	<Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage"">
		<Rows>
			<eHubInboxMessagexmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
				<EI_PK>00000000-0000-0000-0000-000000000000</EI_PK>
				<EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID>
				<EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
				<EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender>
				<EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
				<EI_MessageType>http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange</EI_MessageType>
				<EI_IsFlatFile>false</EI_IsFlatFile>
				<EI_ApplicationCode>UDM</EI_ApplicationCode>
				<EI_Status>2</EI_Status>
				<EI_Content>H4sIAAAAAAAEAK1VXY+aQBR9b9L/QPa12R0GdZMa1oQFZbEOVkAU3kaY1WH5MHyI+Os7Kth11U3blATi3HvmzDn3gleMM747jemGpBkOtTgnqbfC8ZJw2yiMsy5LP92t8nzdBaAsywcPp8ukpBl58JIImN6KRDgDJwIg8BACCO96X79wnPhCsE/Sw2+2MknMVprCgSZiEI+uKYnzU1AE7/aIz4lfNdjTGf0N23CU97fShJO0A+WB6bRkAQXnWE5YEbbvw3XCYvwkl5MwJF5Ok/gMcYb5kGE5q1qTnrqJMimrvMLHCMf0lWT50awIDvmLXT9I1dNli+f5FmzDlgj2gQ+nghvHvk9c1XwEXLo9VsWiEekJvMDfQ+EePlpQ6Lba3c7jNwi7PC+C36CLnXsryBg2mHNnn5fYTIrU+1iIQ+Znmmzo/r1QbWQepZ9Cl7YveG6arUM3uipeU9q0k7wUCyvF3huNl5pytYeijcOC9B7r6/7Ko7lEcMSem7laqc9E9dM0uf4+1Vr0hJOTNCUh3rvlNJ/1iL5S9on8TwVmEUU4rf5NCOclRehzC8LRJuj/J3EGydZJnBGLAT4TR8pkqcnDYtEy1m4UBs7cWC+EztCVtUdN1rbubALdYFLqFd9Gypswsialq/TzseKHLuU7jjVcIUvauTObjpUpHUkHPrporRoOiFQjcCw9cioYoJ0fjCwjQgrKncDbjk1eQLtpC1loh3ZOpauo4cjdub5zZn44Eb4XvmoX/sDoeOqUjqlEnflzuVAbve0NjvXNgmqZTKWlFsHQa+krVzhiCSwpFuzOnOmZy0PBmW2ha5f01azxsc4zHuhVe71D05jqA9N2B4bc8OmVOxvw7szoM1xoq2HumgdvFVK0cmRJrC7S1lb65TjQ9vduFPQFJGclUqRSi/mnP23rKXblQ63/aJo5Ac4HRT1V6kkiglvzrvcLALRa2woHAAA=</EI_Content>
				</eHubInboxMessage>
		</Rows>
		</Insert>
		<Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent"">
			<Rows>
				<eHubInboxXmlContentxmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
					<EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox>
					<EX_XmlContent>&lt;ns0:UniversalInterchangexmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;&lt;Header&gt;&lt;SenderID/&gt;&lt;RecipientID/&gt;&lt;/Header&gt;&lt;Body&gt;&lt;UniversalEventxmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;&lt;Event&gt;&lt;DataContext&gt;&lt;DataTargetCollection&gt;&lt;DataTarget&gt;&lt;Type&gt;GvmsAsycudaManifestHeader&lt;/Type&gt;&lt;Key&gt;NCT00031413&lt;/Key&gt;&lt;/DataTarget&gt;&lt;/DataTargetCollection&gt;&lt;/DataContext&gt;&lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;&lt;EventType&gt;MRJ&lt;/EventType&gt;&lt;DataContext&gt;&lt;DataSource&gt;&lt;DataProvider&gt;GVMS&lt;/DataProvider&gt;&lt;/DataSource&gt;&lt;/DataContext&gt;&lt;ContextCollection&gt;&lt;Context&gt;&lt;Type&gt;eHubTrackingID&lt;/Type&gt;&lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;Error&lt;/Type&gt;&lt;Value&gt;NoCorrelationIdentifiers&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ErrorSummary&lt;/Type&gt;&lt;Value&gt;NoCorrelationIdentifierscouldbeidentified&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ResponseText&lt;/Type&gt;&lt;Value&gt;ewogICJub3RpZmljYXRpb25JZCI6ICIxZWQ1ZjQwNy04MDk2LTQwZDEtODdlZi05YTJhMTAzZWViODUiLAogICJib3hJZCI6ICI1MGRjYTNmYy1jMzdjLTRmMDMtYjcxOS02MzU3MTMzMzYyNGMiLAogICJtZXNzYWdlQ29udGVudFR5cGUiOiAiYXBwbGljYXRpb24vanNvbiIsCiAgIm1lc3NhZ2UiOiAie1wia2V5XCI6XCJ2YWx1ZVwifSIsCiAgInN0YXR1cyI6ICJSRUNFSVZFRCIsCiAgImNyZWF0ZWREYXRlVGltZSI6ICIyMDIwLTA2LTAxVDEwOjIwOjIzLjE2MCswMDAwIn0=&lt;/Value&gt;&lt;/Context&gt;&lt;/ContextCollection&gt;&lt;/Event&gt;&lt;/UniversalEvent&gt;&lt;/Body&gt;&lt;/ns0:UniversalInterchange&gt;</EX_XmlContent>
					<EX_DT_Source>33333333-3333-3333-3333-333333333333</EX_DT_Source>
					<EX_UncompressedLength>1802</EX_UncompressedLength>
					</eHubInboxXmlContent>
			</Rows>
			</Insert>
			<Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage"">
				<Rows>
					<eHubOutboxMessagexmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
						<OI_PK>00000000-0000-0000-0000-000000000000</OI_PK>
						<OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID>
						<OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK>
						<OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender>
						<OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
						<OI_DT_Target>33333333-3333-3333-3333-333333333333</OI_DT_Target>
						<OI_Status>0</OI_Status>
						<OI_Content>H4sIAAAAAAAEAK1VXY+aQBR9b9L/QPa12R0GdZMa1oQFZbEOVkAU3kaY1WH5MHyI+Os7Kth11U3blATi3HvmzDn3gleMM747jemGpBkOtTgnqbfC8ZJw2yiMsy5LP92t8nzdBaAsywcPp8ukpBl58JIImN6KRDgDJwIg8BACCO96X79wnPhCsE/Sw2+2MknMVprCgSZiEI+uKYnzU1AE7/aIz4lfNdjTGf0N23CU97fShJO0A+WB6bRkAQXnWE5YEbbvw3XCYvwkl5MwJF5Ok/gMcYb5kGE5q1qTnrqJMimrvMLHCMf0lWT50awIDvmLXT9I1dNli+f5FmzDlgj2gQ+nghvHvk9c1XwEXLo9VsWiEekJvMDfQ+EePlpQ6Lba3c7jNwi7PC+C36CLnXsryBg2mHNnn5fYTIrU+1iIQ+Znmmzo/r1QbWQepZ9Cl7YveG6arUM3uipeU9q0k7wUCyvF3huNl5pytYeijcOC9B7r6/7Ko7lEcMSem7laqc9E9dM0uf4+1Vr0hJOTNCUh3rvlNJ/1iL5S9on8TwVmEUU4rf5NCOclRehzC8LRJuj/J3EGydZJnBGLAT4TR8pkqcnDYtEy1m4UBs7cWC+EztCVtUdN1rbubALdYFLqFd9Gypswsialq/TzseKHLuU7jjVcIUvauTObjpUpHUkHPrporRoOiFQjcCw9cioYoJ0fjCwjQgrKncDbjk1eQLtpC1loh3ZOpauo4cjdub5zZn44Eb4XvmoX/sDoeOqUjqlEnflzuVAbve0NjvXNgmqZTKWlFsHQa+krVzhiCSwpFuzOnOmZy0PBmW2ha5f01azxsc4zHuhVe71D05jqA9N2B4bc8OmVOxvw7szoM1xoq2HumgdvFVK0cmRJrC7S1lb65TjQ9vduFPQFJGclUqRSi/mnP23rKXblQ63/aJo5Ac4HRT1V6kkiglvzrvcLALRa2woHAAA=</OI_Content>
						<OI_XmlContent>&lt;ns0:UniversalInterchangexmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;&lt;Header&gt;&lt;SenderID/&gt;&lt;RecipientID/&gt;&lt;/Header&gt;&lt;Body&gt;&lt;UniversalEventxmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;&lt;Event&gt;&lt;DataContext&gt;&lt;DataTargetCollection&gt;&lt;DataTarget&gt;&lt;Type&gt;GvmsAsycudaManifestHeader&lt;/Type&gt;&lt;Key&gt;NCT00031413&lt;/Key&gt;&lt;/DataTarget&gt;&lt;/DataTargetCollection&gt;&lt;/DataContext&gt;&lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;&lt;EventType&gt;MRJ&lt;/EventType&gt;&lt;DataContext&gt;&lt;DataSource&gt;&lt;DataProvider&gt;GVMS&lt;/DataProvider&gt;&lt;/DataSource&gt;&lt;/DataContext&gt;&lt;ContextCollection&gt;&lt;Context&gt;&lt;Type&gt;eHubTrackingID&lt;/Type&gt;&lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;Error&lt;/Type&gt;&lt;Value&gt;NoCorrelationIdentifiers&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ErrorSummary&lt;/Type&gt;&lt;Value&gt;NoCorrelationIdentifierscouldbeidentified&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ResponseText&lt;/Type&gt;&lt;Value&gt;ewogICJub3RpZmljYXRpb25JZCI6ICIxZWQ1ZjQwNy04MDk2LTQwZDEtODdlZi05YTJhMTAzZWViODUiLAogICJib3hJZCI6ICI1MGRjYTNmYy1jMzdjLTRmMDMtYjcxOS02MzU3MTMzMzYyNGMiLAogICJtZXNzYWdlQ29udGVudFR5cGUiOiAiYXBwbGljYXRpb24vanNvbiIsCiAgIm1lc3NhZ2UiOiAie1wia2V5XCI6XCJ2YWx1ZVwifSIsCiAgInN0YXR1cyI6ICJSRUNFSVZFRCIsCiAgImNyZWF0ZWREYXRlVGltZSI6ICIyMDIwLTA2LTAxVDEwOjIwOjIzLjE2MCswMDAwIn0=&lt;/Value&gt;&lt;/Context&gt;&lt;/ContextCollection&gt;&lt;/Event&gt;&lt;/UniversalEvent&gt;&lt;/Body&gt;&lt;/ns0:UniversalInterchange&gt;</OI_XmlContent>
						</eHubOutboxMessage>
				</Rows>
				</Insert>
</CompositeOperation>";

			#endregion

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_CNS()
		{
			var originalRecipient = "GBCustoms-CNS";

			var responseJSON = @"{
  ""notificationId"": ""1ed5f407-8096-40d1-87ef-9a2a103eeb85"",
  ""boxId"": ""50dca3fc-c37c-4f03-b719-63571333624c"",
  ""messageContentType"": ""application/json"",
  ""message"": ""{\""key\"":\""value\""}"",
  ""status"": ""RECEIVED"",
  ""createdDateTime"": ""2020-06-01T10:20:23.160+0000""}";

			var mockClientRegistrationAccessor = MockClientRegistrationAccessor(originalRecipient);

			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;
			OrchestrationHelpers.DataModelAccessor = () => mockDataModelAccessor;
			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.Now = () => new DateTime(2020, 12, 16, 12, 34, 56);
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "Create", "WTLDTWJLI", responseJSON, null,
				gbCustomsXml, true, "66666666-6666-6666-6666-666666666666", "", "");

			#region Expected XML

			var expectedXml = @"<CompositeOperation>
	<Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage"">
		<Rows>
			<eHubInboxMessagexmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
				<EI_PK>00000000-0000-0000-0000-000000000000</EI_PK>
				<EI_MessageTrackingID>00000000-0000-0000-0000-000000000000</EI_MessageTrackingID>
				<EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
				<EI_CC_Sender>22222222-2222-2222-2222-222222222222</EI_CC_Sender>
				<EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
				<EI_MessageType>http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange</EI_MessageType>
				<EI_IsFlatFile>false</EI_IsFlatFile>
				<EI_ApplicationCode>UDM</EI_ApplicationCode>
				<EI_Status>2</EI_Status>
				<EI_Content>H4sIAAAAAAAEAK1UXY+iMBR932T/A5nXzUwpjiZrGBMG1MFYXAFR+obQ0TJADR8i/PpFBFd33MnuZppAuKenp+felitGCd9fRHRP4sQJ1Cglsbt1og3hDmEQJf1q+ulum6a7PgB5nj+4TrxhOU3Ig8tCYLhbEjoJOAsAgYcQQHg3+PqF48QX4ngkrr+ryCBRFakKB1pEJy7dURKlZ1AEF2vEZ+YVLfe8x3BfLTjZ+1drwtlaLVkrncMWMGlIBgIv8PdQuIc9Ewr9zmO/2/sGYZ/nRfCL9G5lsSMDpE9azjG84ChO6sisKvHhctNmwmBZ7JIrvJn5EbM9PZZE1gwRXCFXKuCmzAl+v63YQDILAuKmlEXXareMVnCdE3nJ1mbsuG802qiKCH5LtCZaTpCRQa8Z9zde7RDBiXudzM1CfWRqGMcs/siLxjiZxTEJnGO2nOpVR0RfaXU5PtOBkYWhExf/Z4RzWRZ43JpwtAW9TzKnk2THooSYFeEjcyRnG1WeZOuOvsNh4NsrfbcWuhMsqz1VVg94OYfYn+dawT8i5U2YmvMcK8N0pngBpnzXNidbZEolXlp0pizoVKr16LqzbTUgGuu+bWqhXUAflZ4/NfUQKSi1ffcwM3gBlYsOMlGJSrvQxqjVSPFKK+2lF8yF75k3tjJvpHfd8YLOqETt1XO+Hrd+H/dOpO3XVE1kKm3UEAZuR9ti4cQlMKeOYHVXlZ+VPBHs5QFiK6evRsOPNL7SgW5x9Dsx9IU2Miw80uVWTyvwcsTjpT6seIE1DlJs1LkVSFHzqSlVdZEOljLMZ756fMqpPxSQnORIkXI14p/+9ljP2I0ftekzbYcE1y2y6adNDxXBnzr94CeBF0wVBAYAAA==</EI_Content>
				</eHubInboxMessage>
		</Rows>
		</Insert>
		<Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent"">
			<Rows>
				<eHubInboxXmlContentxmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
					<EX_EI_Inbox>00000000-0000-0000-0000-000000000000</EX_EI_Inbox>
					<EX_XmlContent>&lt;ns0:UniversalInterchangexmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;&lt;Header&gt;&lt;SenderID/&gt;&lt;RecipientID/&gt;&lt;/Header&gt;&lt;Body&gt;&lt;UniversalEventxmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;&lt;Event&gt;&lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;&lt;EventType&gt;MRJ&lt;/EventType&gt;&lt;DataContext&gt;&lt;DataSource&gt;&lt;DataProvider&gt;CNS&lt;/DataProvider&gt;&lt;/DataSource&gt;&lt;/DataContext&gt;&lt;ContextCollection&gt;&lt;Context&gt;&lt;Type&gt;eHubTrackingID&lt;/Type&gt;&lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;Error&lt;/Type&gt;&lt;Value&gt;NoCorrelationIdentifiers&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ErrorSummary&lt;/Type&gt;&lt;Value&gt;NoCorrelationIdentifierscouldbeidentified&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ResponseText&lt;/Type&gt;&lt;Value&gt;ewogICJub3RpZmljYXRpb25JZCI6ICIxZWQ1ZjQwNy04MDk2LTQwZDEtODdlZi05YTJhMTAzZWViODUiLAogICJib3hJZCI6ICI1MGRjYTNmYy1jMzdjLTRmMDMtYjcxOS02MzU3MTMzMzYyNGMiLAogICJtZXNzYWdlQ29udGVudFR5cGUiOiAiYXBwbGljYXRpb24vanNvbiIsCiAgIm1lc3NhZ2UiOiAie1wia2V5XCI6XCJ2YWx1ZVwifSIsCiAgInN0YXR1cyI6ICJSRUNFSVZFRCIsCiAgImNyZWF0ZWREYXRlVGltZSI6ICIyMDIwLTA2LTAxVDEwOjIwOjIzLjE2MCswMDAwIn0=&lt;/Value&gt;&lt;/Context&gt;&lt;/ContextCollection&gt;&lt;/Event&gt;&lt;/UniversalEvent&gt;&lt;/Body&gt;&lt;/ns0:UniversalInterchange&gt;</EX_XmlContent>
					<EX_DT_Source>33333333-3333-3333-3333-333333333333</EX_DT_Source>
					<EX_UncompressedLength>1540</EX_UncompressedLength>
					</eHubInboxXmlContent>
			</Rows>
			</Insert>
			<Insertxmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage"">
				<Rows>
					<eHubOutboxMessagexmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
						<OI_PK>00000000-0000-0000-0000-000000000000</OI_PK>
						<OI_MessageTrackingID>00000000-0000-0000-0000-000000000000</OI_MessageTrackingID>
						<OI_EI_InboxPK>00000000-0000-0000-0000-000000000000</OI_EI_InboxPK>
						<OI_CC_Sender>22222222-2222-2222-2222-222222222222</OI_CC_Sender>
						<OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
						<OI_DT_Target>33333333-3333-3333-3333-333333333333</OI_DT_Target>
						<OI_Status>0</OI_Status>
						<OI_Content>H4sIAAAAAAAEAK1UXY+iMBR932T/A5nXzUwpjiZrGBMG1MFYXAFR+obQ0TJADR8i/PpFBFd33MnuZppAuKenp+felitGCd9fRHRP4sQJ1Cglsbt1og3hDmEQJf1q+ulum6a7PgB5nj+4TrxhOU3Ig8tCYLhbEjoJOAsAgYcQQHg3+PqF48QX4ngkrr+ryCBRFakKB1pEJy7dURKlZ1AEF2vEZ+YVLfe8x3BfLTjZ+1drwtlaLVkrncMWMGlIBgIv8PdQuIc9Ewr9zmO/2/sGYZ/nRfCL9G5lsSMDpE9azjG84ChO6sisKvHhctNmwmBZ7JIrvJn5EbM9PZZE1gwRXCFXKuCmzAl+v63YQDILAuKmlEXXareMVnCdE3nJ1mbsuG802qiKCH5LtCZaTpCRQa8Z9zde7RDBiXudzM1CfWRqGMcs/siLxjiZxTEJnGO2nOpVR0RfaXU5PtOBkYWhExf/Z4RzWRZ43JpwtAW9TzKnk2THooSYFeEjcyRnG1WeZOuOvsNh4NsrfbcWuhMsqz1VVg94OYfYn+dawT8i5U2YmvMcK8N0pngBpnzXNidbZEolXlp0pizoVKr16LqzbTUgGuu+bWqhXUAflZ4/NfUQKSi1ffcwM3gBlYsOMlGJSrvQxqjVSPFKK+2lF8yF75k3tjJvpHfd8YLOqETt1XO+Hrd+H/dOpO3XVE1kKm3UEAZuR9ti4cQlMKeOYHVXlZ+VPBHs5QFiK6evRsOPNL7SgW5x9Dsx9IU2Miw80uVWTyvwcsTjpT6seIE1DlJs1LkVSFHzqSlVdZEOljLMZ756fMqpPxSQnORIkXI14p/+9ljP2I0ftekzbYcE1y2y6adNDxXBnzr94CeBF0wVBAYAAA==</OI_Content>
						<OI_XmlContent>&lt;ns0:UniversalInterchangexmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11""&gt;&lt;Header&gt;&lt;SenderID/&gt;&lt;RecipientID/&gt;&lt;/Header&gt;&lt;Body&gt;&lt;UniversalEventxmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""&gt;&lt;Event&gt;&lt;EventTime&gt;2020-12-16T12:34:56+11:00&lt;/EventTime&gt;&lt;EventType&gt;MRJ&lt;/EventType&gt;&lt;DataContext&gt;&lt;DataSource&gt;&lt;DataProvider&gt;CNS&lt;/DataProvider&gt;&lt;/DataSource&gt;&lt;/DataContext&gt;&lt;ContextCollection&gt;&lt;Context&gt;&lt;Type&gt;eHubTrackingID&lt;/Type&gt;&lt;Value&gt;66666666-6666-6666-6666-666666666666&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;Error&lt;/Type&gt;&lt;Value&gt;NoCorrelationIdentifiers&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ErrorSummary&lt;/Type&gt;&lt;Value&gt;NoCorrelationIdentifierscouldbeidentified&lt;/Value&gt;&lt;/Context&gt;&lt;Context&gt;&lt;Type&gt;ResponseText&lt;/Type&gt;&lt;Value&gt;ewogICJub3RpZmljYXRpb25JZCI6ICIxZWQ1ZjQwNy04MDk2LTQwZDEtODdlZi05YTJhMTAzZWViODUiLAogICJib3hJZCI6ICI1MGRjYTNmYy1jMzdjLTRmMDMtYjcxOS02MzU3MTMzMzYyNGMiLAogICJtZXNzYWdlQ29udGVudFR5cGUiOiAiYXBwbGljYXRpb24vanNvbiIsCiAgIm1lc3NhZ2UiOiAie1wia2V5XCI6XCJ2YWx1ZVwifSIsCiAgInN0YXR1cyI6ICJSRUNFSVZFRCIsCiAgImNyZWF0ZWREYXRlVGltZSI6ICIyMDIwLTA2LTAxVDEwOjIwOjIzLjE2MCswMDAwIn0=&lt;/Value&gt;&lt;/Context&gt;&lt;/ContextCollection&gt;&lt;/Event&gt;&lt;/UniversalEvent&gt;&lt;/Body&gt;&lt;/ns0:UniversalInterchange&gt;</OI_XmlContent>
						</eHubOutboxMessage>
				</Rows>
				</Insert>
</CompositeOperation>";

			#endregion

			Assert.AreEqual(Regex.Replace(expectedXml.Replace("\r\n", string.Empty), @"\s+", ""),
				Regex.Replace(output.Replace("\r\n", string.Empty), @"\s+", ""));
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestProcessHttpResponseAndReturnMessage_GBCBResponse_CheckDirectionHasValue()
		{
            var originalRecipient = "GBCustoms-CTCGB";

            var responseXml = @"<ns:SuccessResponse xmlns:ns=""http://www.hmrc.gov.uk/successresponse/2"" xmlns=""http://www.govtalk.gov.uk/enforcement/ICS/responsedata/7"">
   <ns:ResponseData>
       <CorrelationId>0JRF7UncK0t004</CorrelationId>
       <WhateverId>666</WhateverId>
   </ns:ResponseData>
</ns:SuccessResponse>";

            var mockClientRegistrationAccessor = MockClientRegistrationAccessor(originalRecipient);
            var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
            mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GBCTID", originalRecipient, "WTLDTWJLI", "0JRF7UncK0t004", gbCustomsXml, "Depart")).Repeat.Once();

            var mockContext = GenerateMockContext(originalRecipient);

            OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;
            OrchestrationHelpers.DataModelAccessor = () => mockDataModelAccessor;
            OrchestrationHelpers.ContextFactory = () => mockContext;
            OrchestrationHelpers.Now = () => new DateTime(2020, 12, 16, 12, 34, 56);
            OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ProcessHttpResponseAndReturnMessage(MockRepository.GenerateStub<ILog>(), originalRecipient, "Create", "WTLDTWJLI", responseXml, null,
				gbCustomsXml, true, "66666666-6666-6666-6666-666666666666", "", "");

			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestSplitStatus()
		{
			Assert.AreEqual("New", OrchestrationHelpers.SplitStatus(MockRepository.GenerateStub<ILog>(), "New|2021-12-01T10:01:01Z")[0]);
			Assert.AreEqual("2021-12-01T10:01:01Z", OrchestrationHelpers.SplitStatus(MockRepository.GenerateStub<ILog>(), "New |2021-12-01T10:01:01Z")[1]);
			Assert.AreEqual("New", OrchestrationHelpers.SplitStatus(MockRepository.GenerateStub<ILog>(), null)[0]);
		}

		[TestMethod]
		public void TestGetStatusType()
		{
			Assert.AreEqual("New", OrchestrationHelpers.GetStatusType(MockRepository.GenerateStub<ILog>(), "New|2021-12-01T10:01:01Z"));
			Assert.AreEqual("Polling", OrchestrationHelpers.GetStatusType(MockRepository.GenerateStub<ILog>(), "Polling|2021-12-01T10:01:01Z"));
			Assert.AreEqual("New", OrchestrationHelpers.GetStatusType(MockRepository.GenerateStub<ILog>(), null));
		}

		[TestMethod]
		public void TestGetStatusDate()
		{
			Assert.AreEqual("2021-12-01T10:01:01", OrchestrationHelpers.GetStatusDate(MockRepository.GenerateStub<ILog>(), "New|2021-12-01T10:01:01").ToString("yyyy-MM-ddTHH:mm:ss"));
			Assert.AreEqual(DateTime.UtcNow.ToString("yyyy-MM-ddTHH"), OrchestrationHelpers.GetStatusDate(MockRepository.GenerateStub<ILog>(), null).ToString("yyyy-MM-ddTHH"));
		}

		[TestMethod]
		public void TestHasTimeLapsed()
		{
			Assert.AreEqual(false, OrchestrationHelpers.HasTimeLapsed(MockRepository.GenerateStub<ILog>(), DateTime.UtcNow, 1));
			Assert.AreEqual(true, OrchestrationHelpers.HasTimeLapsed(MockRepository.GenerateStub<ILog>(), DateTime.UtcNow.AddMinutes(-2), 1));
		}

		[TestMethod]
		public void TestGetXmlMessageCount()
		{
			Assert.AreEqual(2, OrchestrationHelpers.GetXmlMessageCount(MockRepository.GenerateStub<ILog>(), xmlStringOutcomesResponse, "//*[local-name()='entryDeclarationResponses']/*[local-name()='response']"));
		}

		[TestMethod]
		public void TestGetXmlMessageValue()
		{
			Assert.AreEqual("/customs/imports/outcomes/0JRF7UncK0t004", OrchestrationHelpers.GetXmlMessageValue(MockRepository.GenerateStub<ILog>(), xmlStringOutcomesResponse, "//*[local-name()='entryDeclarationResponses']/*[local-name()='response']/*[local-name()='link']", 1));
			Assert.AreEqual("/customs/imports/outcomes/0987654321", OrchestrationHelpers.GetXmlMessageValue(MockRepository.GenerateStub<ILog>(), xmlStringOutcomesResponse, "//*[local-name()='entryDeclarationResponses']/*[local-name()='response']/*[local-name()='link']", 2));
		}

		[TestMethod]
		public void TestGetXmlMessageAttribute()
		{
			Assert.AreEqual("DELETE", OrchestrationHelpers.GetXmlMessageAttribute(MockRepository.GenerateStub<ILog>(), xmlStringOutcomeResponse, "//*[local-name()='outcomeResponse']/*[local-name()='acknowledgement']", "method"));
			Assert.AreEqual("/customs/imports/outcomes/0JRF7UncK0t004", OrchestrationHelpers.GetXmlMessageAttribute(MockRepository.GenerateStub<ILog>(), xmlStringOutcomeResponse, "//*[local-name()='outcomeResponse']/*[local-name()='acknowledgement']", "href"));
		}

		private void TestExtractCorrelationIDs(string originalRecipient, string response, string typeAndSearchPath, string expectedValue, HttpHeader[] headers = null)
		{
			#region ClientRegistration Mock

			var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
			var mockClientRegistrations = new List<Dictionary<string, object>>()
			{
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", originalRecipient } ,
					{ "CX_Qualifier", "Create"},
					{ "CX_Code", typeAndSearchPath },
					{ "CX_Flag1", "Synchronous"},
					{ "CX_Flag2", "Subscribed" },
					{ "CX_Attr1", "CorrelationID" }
				}
			};
			mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations(originalRecipient, "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

			#endregion

			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;
			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.Now = () => new DateTime(2020, 12, 16, 12, 34, 56);
			OrchestrationHelpers.NewGuid = () => Guid.Parse("00000000-0000-0000-0000-000000000000");

			var output = OrchestrationHelpers.ExtractCorrelationIDs(MockRepository.GenerateStub<ILog>(), originalRecipient, "Create", response, headers);

			Assert.AreEqual(expectedValue, output[0].Value);
		}

        private static EMCSConfig gbCustomsEmcsConfig()
        {
			var expectedOutput = new EMCSConfig()
			{
				Service = "IE815",
				Recipient = "GBCustoms-EMCS",
                ErrorCount = 0,
                OutboundDate = new DateTime(2020, 12, 16, 12, 34, 56),
				LastPollingDate = new DateTime(2020, 12, 16, 12, 34, 56),
				CredentialsKey = "HYEMIK.GB896458895023.D01",
				RequestHeaders = new HttpHeader[] { new HttpHeader() { Key = "RequestKey", Value = "RequestValue" } },
				ResponseHeaders = new HttpHeader[] { new HttpHeader() { Key = "ResponseKey", Value = "ResponseValue" } }
		};
			return expectedOutput;
        }

        [TestMethod]
        public void SaveConfigurationEMCS_Insert()
        {
            var expectedOutput = gbCustomsEmcsConfig();
            var mockClientRegistrationAccessor = MockRepository.GenerateStrictMock<IClientRegistrationAccessor>();
            OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;

            mockClientRegistrationAccessor.Expect(x => x.Exists("a7259690-55cb-4764-9b18-6c0a18c79a35", "GBCustoms-EMCS", "")).Return(false);
            mockClientRegistrationAccessor.Expect(x => x.Insert("a7259690-55cb-4764-9b18-6c0a18c79a35", "GBCustoms-EMCS", "", string.Empty, 1, ConfigXml, string.Empty, string.Empty));

            OrchestrationHelpers.SaveConfiguration(expectedOutput, "a7259690-55cb-4764-9b18-6c0a18c79a35");
            mockClientRegistrationAccessor.VerifyAllExpectations();
        }
        
        [TestMethod] 
        public void SaveConfigurationEMCS_Update() 
        {
			var expectedOutput = gbCustomsEmcsConfig();
			var mockClientRegistrationAccessor = MockRepository.GenerateStrictMock<IClientRegistrationAccessor>();
            OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;

            mockClientRegistrationAccessor.Expect(x => x.Exists("a7259690-55cb-4764-9b18-6c0a18c79a28", "GBCustoms-EMCS", "")).Return(true); 
            mockClientRegistrationAccessor.Expect(x => x.UpdateFlag1AndConfigXml("a7259690-55cb-4764-9b18-6c0a18c79a28", "GBCustoms-EMCS", "", 1, ConfigXml)); 
            
            OrchestrationHelpers.SaveConfiguration(expectedOutput, "a7259690-55cb-4764-9b18-6c0a18c79a28");
            mockClientRegistrationAccessor.VerifyAllExpectations();
        }

        private static IClientRegistrationAccessor MockClientRegistrationAccessor(string originalRecipient)
		{
			var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
			var mockClientRegistrations = new List<Dictionary<string, object>>()
			{
				new Dictionary<string, object>
				{
					{"CX_CC_ID", originalRecipient},
					{"CX_Qualifier", "Create"},
					{"CX_Code", "XMLBody:/*/*[local-name()='ResponseData']/*[local-name()='CorrelationId']/text()"},
					{"CX_Flag1", "Synchronous"},
					{"CX_Flag2", "Subscribed"},
					{"CX_Attr1", "CorrelationID"}
				},
				new Dictionary<string, object>
				{
					{"CX_CC_ID", originalRecipient},
					{"CX_Qualifier", "Create"},
					{"CX_Code", "XMLBody:/*/*[local-name()='ResponseData']/*[local-name()='WhateverId']/text()"},
					{"CX_Flag1", "Synchronous"},
					{"CX_Flag2", "Additional"},
					{"CX_Attr1", "AdditionalID"}
				},
				new Dictionary<string, object>
				{
					{"CX_CC_ID", originalRecipient},
					{"CX_Qualifier", "Create"},
					{"CX_Code", "XMLBody:/*/*[local-name()='ResponseData']/*[local-name()='NA']/text()"},
					{"CX_Flag1", "Synchronous"},
					{"CX_Flag2", "ShouldNotOutput"},
					{"CX_Attr1", "ShouldNotOutputID"}
				}
			};
			mockClientRegistrationAccessor
				.Stub(_ => _.ReadRegistrations(originalRecipient, "GBCustoms-Transport", qualifier: "Create%", flag1: 0,
					useLike: true)).Return(mockClientRegistrations);
			return mockClientRegistrationAccessor;
		}

        private eHubTransactionsContext GenerateMockContext(string originalRecipient)
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockContext.Stub(mock => mock.eHubClients).Return(new TestDbSet<eHubClient>()
			{
				new eHubClient
				{
					CC_PK = Guid.Parse("11111111-1111-1111-1111-111111111111"),
					CC_ID = "WTLDTWJLI"
				},
				new eHubClient
				{
					CC_PK = Guid.Parse("22222222-2222-2222-2222-222222222222"),
					CC_ID = originalRecipient
				}
			});
			mockContext.Stub(mock => mock.eHubMessageTypes).Return(new TestDbSet<eHubMessageType>()
			{
				new eHubMessageType
				{
					DT_PK = Guid.Parse("33333333-3333-3333-3333-333333333333"),
					DT_Code = "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange"
				}
			});
			mockContext.Stub(mock => mock.eHubSubscriptionTypes).Return(new TestDbSet<eHubSubscriptionType>()
			{
				new eHubSubscriptionType
				{
					ST_PK = Guid.Parse("44444444-4444-4444-4444-444444444444"),
					ST_ID = "GBCTID"
				}
			});
			mockContext.Stub(mock => mock.eHubSubscriptionValues).Return(new TestDbSet<eHubSubscriptionValue>()
			{
				new eHubSubscriptionValue
				{
					SV_PK = Guid.Parse("55555555-5555-5555-5555-555555555555"),
					SV_CC_Recipient = Guid.Parse("22222222-2222-2222-2222-222222222222"),
					SV_CC_Sender = Guid.Parse("11111111-1111-1111-1111-111111111111"),
					SV_SubscribedUTC = new DateTime(2019, 01, 01, 00, 00, 00),
					SV_ExpiryUTC = new DateTime(2021, 01, 01, 00, 00, 00),
					SV_ReferenceType = "Blah",
					SV_ST = Guid.Parse("44444444-4444-4444-4444-444444444444")
				},
				new eHubSubscriptionValue
				{
					SV_PK = Guid.Parse("66666666-6666-6666-6666-666666666666"),
					SV_CC_Recipient = Guid.Parse("22222222-2222-2222-2222-222222222222"),
					SV_CC_Sender = Guid.Parse("11111111-1111-1111-1111-111111111111"),
					SV_SubscribedUTC = new DateTime(2019, 01, 01, 00, 00, 00),
					SV_ExpiryUTC = new DateTime(2020, 01, 01, 00, 00, 00),
					SV_ReferenceType = "Blah",
					SV_ST = Guid.Parse("44444444-4444-4444-4444-444444444444")
				},
				new eHubSubscriptionValue
				{
					SV_PK = Guid.Parse("77777777-7777-7777-7777-777777777777"),
					SV_CC_Recipient = Guid.Parse("22222222-2222-2222-2222-222222222222"),
					SV_CC_Sender = Guid.Parse("11111111-1111-1111-1111-111111111111"),
					SV_SubscribedUTC = new DateTime(2019, 01, 01, 00, 00, 00),
					SV_ExpiryUTC = new DateTime(2021, 01, 01, 00, 00, 00),
					SV_ST = Guid.Parse("44444444-4444-4444-4444-444444444444")
				},
				new eHubSubscriptionValue
				{
					SV_PK = Guid.Parse("88888888-8888-8888-8888-888888888888"),
					SV_CC_Recipient = Guid.Parse("22222222-2222-2222-2222-222222222222"),
					SV_CC_Sender = Guid.Parse("11111111-1111-1111-1111-111111111111"),
					SV_SubscribedUTC = new DateTime(2019, 01, 01, 00, 00, 00),
					SV_ReferenceType = "Blah",
					SV_ST = Guid.Parse("44444444-4444-4444-4444-444444444444")
				},
				new eHubSubscriptionValue
				{
					SV_PK = Guid.Parse("99999999-9999-9999-9999-999999999999"),
					SV_CC_Recipient = Guid.Parse("22222222-2222-2222-2222-222222222222"),
					SV_CC_Sender = Guid.Parse("11111111-1111-1111-1111-111111111111"),
					SV_SubscribedUTC = new DateTime(2019, 01, 01, 00, 00, 00),
					SV_ExpiryUTC = new DateTime(2021, 01, 01, 00, 00, 00),
					SV_ReferenceType = "NewLaLaLa",
					SV_ST = Guid.Parse("44444444-4444-4444-4444-444444444444")
				},
				new eHubSubscriptionValue
				{
					SV_PK = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
					SV_CC_Recipient = Guid.Parse("22222222-2222-2222-2222-222222222222"),
					SV_CC_Sender = Guid.Parse("11111111-1111-1111-1111-111111111111"),
					SV_SubscribedUTC = new DateTime(2019, 01, 01, 00, 00, 00),
					SV_ExpiryUTC = new DateTime(2021, 01, 01, 00, 00, 00),
					SV_ReferenceType = "PollingLaLaLa",
					SV_ST = Guid.Parse("44444444-4444-4444-4444-444444444444")
				},
				new eHubSubscriptionValue
				{
					SV_PK = Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
					SV_CC_Recipient = Guid.Parse("22222222-2222-2222-2222-222222222222"),
					SV_CC_Sender = Guid.Parse("11111111-1111-1111-1111-111111111111"),
					SV_SubscribedUTC = new DateTime(2019, 01, 01, 00, 00, 00),
					SV_ExpiryUTC = new DateTime(2021, 01, 01, 00, 00, 00),
					SV_ReferenceType = "ReceivedLaLaLa",
					SV_ST = Guid.Parse("44444444-4444-4444-4444-444444444444")
				},
			});

            return mockContext;
		}

		[TestMethod]
		public void CreateConfig_Success()
		{
			#region Input XML
			var messageXml = @"<GBCustomsRequest>
	    <Provider>EMCS</Provider>
	    <Service>IE815</Service> 
	    <Credentials Key=""HYEMIK.GB896458895023.D01"" />
	    <ServiceReference>123456</ServiceReference>
	    <JobNumber>B00001000</JobNumber>
		<Recipient>GBCustoms-EMCS</Recipient>
	    <ContentType>XML</ContentType>
	    <Version>3</Version>
    </GBCustomsRequest>";
			#endregion

			var requestHeaders = new HttpHeader[] { new HttpHeader() { Key = "RequestKey", Value = "RequestValue" } };
			var responseHeaders = new HttpHeader[] { new HttpHeader() { Key = "ResponseKey", Value = "ResponseValue" } };
			var timeStamp = new DateTime(2024, 5, 13, 7, 5, 8);

			var expectedOutput = new EMCSConfig()
			{
				Service = "IE815",
				Recipient = "GBCustoms-EMCS",
				ErrorCount = 0,
				OutboundDate = DateTime.UtcNow,
				CredentialsKey = "HYEMIK.GB896458895023.D01",
				RequestHeaders = requestHeaders,
				ResponseHeaders = responseHeaders,
				ContentType = "ContentType",
				LastPollingDate = timeStamp.AddYears(-1),
			};

			var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
			mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-EMCS", "GBCustoms-EMCS", qualifier: "B00001000")).Return(null);
			OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;
			OrchestrationHelpers.Now = () => timeStamp;

			var output = OrchestrationHelpers.CreateConfig(MockRepository.GenerateMock<ILog>(), messageXml, "GBCustoms-EMCS", MessageType.GBCustomsRequest, requestHeaders, responseHeaders, "HYEMIK.GB896458895023.D01", "ContentType");

			Assert.AreEqual(expectedOutput.Service, output.Service);
			Assert.AreEqual(expectedOutput.ServiceReference, output.ServiceReference);
			Assert.AreEqual(expectedOutput.JobNumber, output.JobNumber);
			Assert.AreEqual(expectedOutput.Recipient, output.Recipient);
			Assert.AreEqual(expectedOutput.CredentialsKey, output.CredentialsKey);
			Assert.AreEqual(expectedOutput.RequestHeaders, output.RequestHeaders);
			Assert.AreEqual(expectedOutput.ResponseHeaders, output.ResponseHeaders);
			Assert.AreEqual(expectedOutput.ContentType, output.ContentType);
			Assert.AreEqual(expectedOutput.LastPollingDate, output.LastPollingDate);
		}

		[TestMethod]
		public void CreateConfig_Success_LastPollingExists()
		{
			#region Input XML
			var messageXml = @"<GBCustomsRequest>
	    <Provider>EMCS</Provider>
	    <Service>IE815</Service> 
	    <Credentials Key=""HYEMIK.GB896458895023.D01"" />
	    <ServiceReference>123456</ServiceReference>
	    <JobNumber>B00001000</JobNumber>
		<Recipient>GBCustoms-EMCS</Recipient>
	    <ContentType>XML</ContentType>
	    <Version>3</Version>
    </GBCustomsRequest>";
			#endregion

			var requestHeaders = new HttpHeader[] { new HttpHeader() { Key = "RequestKey", Value = "RequestValue" } };
			var responseHeaders = new HttpHeader[] { new HttpHeader() { Key = "ResponseKey", Value = "ResponseValue" } };

			var expectedOutput = new EMCSConfig()
			{
				Service = "IE815",
				Recipient = "GBCustoms-EMCS",
				ErrorCount = 0,
				OutboundDate = DateTime.UtcNow,
				CredentialsKey = "HYEMIK.GB896458895023.D01",
				RequestHeaders = requestHeaders,
				ResponseHeaders = responseHeaders,
				ContentType = "ContentType",
				LastPollingDate = new DateTime(2024, 7, 26, 7, 22, 57),
			};

			#region ClientRegistration Mock
			var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
			var mockClientRegistrations = new List<Dictionary<string, object>>()
			{
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", "AAABBBCCC" } ,
					{ "CX_Qualifier", ""},
					{ "CX_ConfigXml", @"<EMCSConfig>
	<Service>Consignor</Service>
	<ServiceReference/>
	<JobNumber>EMCSJobNumber</JobNumber>
	<Recipient>GBCustomsTest-EMCS</Recipient>
	<ErrorCount>0</ErrorCount>
	<OutboundDate>2024-07-26T07:22:57Z</OutboundDate>
	<LastPollingDate>2024-07-26T07:22:57</LastPollingDate>
</EMCSConfig>"
					}
				}
			};
			mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-EMCS", "GBCustoms-EMCS", qualifier: string.Empty)).Return(mockClientRegistrations);
			#endregion

			OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;

			var output = OrchestrationHelpers.CreateConfig(MockRepository.GenerateMock<ILog>(), messageXml, "GBCustoms-EMCS", MessageType.GBCustomsRequest, requestHeaders, responseHeaders, "HYEMIK.GB896458895023.D01", "ContentType", "GBCustoms-EMCS");

			Assert.AreEqual(expectedOutput.Service, output.Service);
			Assert.AreEqual(expectedOutput.ServiceReference, output.ServiceReference);
			Assert.AreEqual(expectedOutput.JobNumber, output.JobNumber);
			Assert.AreEqual(expectedOutput.Recipient, output.Recipient);
			Assert.AreEqual(expectedOutput.CredentialsKey, output.CredentialsKey);
			Assert.AreEqual(expectedOutput.RequestHeaders, output.RequestHeaders);
			Assert.AreEqual(expectedOutput.ResponseHeaders, output.ResponseHeaders);
			Assert.AreEqual(expectedOutput.ContentType, output.ContentType);
			Assert.AreEqual(expectedOutput.LastPollingDate, output.LastPollingDate);
		}

		[TestMethod]
		[ExpectedException(typeof(XmlException), "Invalid XML format")]
		public void CreateConfig_InvalidXML()
		{
			var messageXml = @"test/test";
			OrchestrationHelpers.CreateConfig(MockRepository.GenerateMock<ILog>(), messageXml, "GBCustoms-EMCS", MessageType.GBCustomsRequest, null, null, string.Empty, string.Empty);
		}

		[TestMethod]
		public void TestSetTokenHeader()
		{
			var headers = new HttpHeader[]
			{
				new HttpHeader() { Key = "Accept", Value = "blah" },
				new HttpHeader() { Key = "Authorization", Value = "Bearer 83626c340c69ee7f9bdbe91dcb166c4f" }
			};

			var output = OrchestrationHelpers.SetTokenHeader(headers, "NewToken");

			Assert.AreEqual(2, output.Count());
			Assert.AreEqual(output[1].Value, "Bearer NewToken");
		}

		[TestMethod]
		public void TestGetQueryString()
		{
			var input = new EMCSConfig()
			{
				LastPollingDate = new DateTime(2023, 12, 1, 15, 6, 32)
			};

			var output = OrchestrationHelpers.GetQueryString(input);

			Assert.AreEqual("?traderType=consignee&updatedSince=2023-12-01T15:06:32Z", output);
		}

		[TestMethod]
		public void TestGetDirection_Success_CTCGB()
        {
			var expectedDirection = "Depart";

			var output = OrchestrationHelpers.GetDirection(gbCustomsXml, "CTCGB");

			Assert.AreEqual(expectedDirection, output);
        }

		[TestMethod]
		public void TestGetDirection_Success_EMCS()
		{
			var expectedDirection = "Depart";

			var output = OrchestrationHelpers.GetDirection(gbCustomsXml, "EMCS");

			Assert.AreEqual(expectedDirection, output);
		}

		[TestMethod]
		public void TestGetDirection_InvalidXML()
		{
			var messageXml = @"test/test";

			var output = OrchestrationHelpers.GetDirection(messageXml, "CTCGB");

			Assert.IsNull(output);
		}

		[TestMethod]
		public void TestExtractBodyForIrMark()
        {
			#region Input XML
			var inputXML = @"<IE815 xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.01"">
      <Header>
        <MessageSender xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">NDEA.GB</MessageSender>
        <MessageRecipient xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">NDEA.GB</MessageRecipient>
        <DateOfPreparation xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">2023-02-23</DateOfPreparation>
        <TimeOfPreparation xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">14:38:02.5797324</TimeOfPreparation>
        <MessageIdentifier xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">00000016304</MessageIdentifier>
      </Header>
      <Body>
        <SubmittedDraftOfEADESAD>
          <Attributes>
            <SubmissionMessageType>1</SubmissionMessageType>
            <DeferredSubmissionFlag>1</DeferredSubmissionFlag>
          </Attributes>
          <ConsigneeTrader language=""en"">
            <Traderid>GBWK000097101</Traderid>
            <TraderName>00200</TraderName>
            <StreetName>Some address</StreetName>
            <Postcode>PO123</Postcode>
            <City>City</City>
          </ConsigneeTrader>
          <ConsignorTrader language=""en"">
            <TraderExciseNumber>GBWK895164000</TraderExciseNumber>
            <TraderName>Sabic UK Petrochemicals Limited</TraderName>
            <StreetName>Wilton Centre  Po Box 99</StreetName>
            <Postcode>TS10 4YA</Postcode>
            <City>Middlesbrough</City>
          </ConsignorTrader>
          <PlaceOfDispatchTrader language=""en"">
            <ReferenceOfTaxWarehouse>GB0000A004554</ReferenceOfTaxWarehouse>
            <TraderName>Sabic UK Petrochemicals Limited</TraderName>
            <StreetName>Wilton Centre, Po Box 99</StreetName>
            <Postcode>TS90 9JE</Postcode>
            <City>Middlesbrough</City>
          </PlaceOfDispatchTrader>
          <DeliveryPlaceTrader language=""en"">
            <Traderid>GB00000097101</Traderid>
            <TraderName>Evonik Oxeno Antwerpen Nv</TraderName>
            <StreetName>Tijsmanstunnel West</StreetName>
            <Postcode>NA</Postcode>
            <City>Antwerp</City>
          </DeliveryPlaceTrader>
          <CompetentAuthorityDispatchOffice>
            <ReferenceNumber>GB004098</ReferenceNumber>
          </CompetentAuthorityDispatchOffice>
          <FirstTransporterTrader language=""en"">
            <TraderName>Chemgas Shipping Bv</TraderName>
            <StreetName>Gedempte Zalmhaven 4G</StreetName>
            <Postcode>3011BT</Postcode>
            <City>Rotterdam</City>
          </FirstTransporterTrader>
          <HeaderEadEsad>
            <DestinationTypeCode>1</DestinationTypeCode>
            <JourneyTime>D01</JourneyTime>
            <TransportArrangement>1</TransportArrangement>
          </HeaderEadEsad>
          <TransportMode>
            <TransportModeCode>1</TransportModeCode>
          </TransportMode>
          <MovementGuarantee>
            <GuarantorTypeCode>0</GuarantorTypeCode>
          </MovementGuarantee>
          <BodyEadEsad>
            <BodyRecordUniqueReference>1</BodyRecordUniqueReference>
            <ExciseProductCode>B000</ExciseProductCode>
            <CnCode>27111900</CnCode>
            <Quantity>1783691.000</Quantity>
            <GrossMass>1783693.000</GrossMass>
            <NetMass>1783692.000</NetMass>
            <CommercialDescription language=""en"">C4 Raffinate 1</CommercialDescription>
            <MaturationPeriodOrAgeOfProducts language=""en"">Maturation 123</MaturationPeriodOrAgeOfProducts>
            <Package>
              <KindOfPackages>VL</KindOfPackages>
            </Package>
          </BodyEadEsad>
          <EadEsadDraft>
            <LocalReferenceNumber>EMC0000000008371</LocalReferenceNumber>
            <InvoiceNumber>000000001</InvoiceNumber>
            <InvoiceDate>2021-09-01</InvoiceDate>
            <OriginTypeCode>1</OriginTypeCode>
            <DateOfDispatch>2021-09-02</DateOfDispatch>
            <TimeOfDispatch>00:00:00</TimeOfDispatch>
          </EadEsadDraft>
          <TransportDetails>
            <TransportUnitCode>1</TransportUnitCode>
            <IdentityOfTransportUnits>ABC</IdentityOfTransportUnits>
            <ComplementaryInformation language=""en"">Some Info</ComplementaryInformation>
          </TransportDetails>
        </SubmittedDraftOfEADESAD>
      </Body>
    </IE815>";
			var inputXmlDocument = new XmlDocument();
			inputXmlDocument.LoadXml(inputXML);
			#endregion

			#region Expected XML
			var expectedXML = @"<s:Body xmlns:s=""http://www.w3.org/2003/05/soap-envelope"">
    <IE815 xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.01"">
	  <Header>
        <MessageSender xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">NDEA.GB</MessageSender>
        <MessageRecipient xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">NDEA.GB</MessageRecipient>
        <DateOfPreparation xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">2023-02-23</DateOfPreparation>
        <TimeOfPreparation xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">14:38:02.5797324</TimeOfPreparation>
        <MessageIdentifier xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">00000016304</MessageIdentifier>
      </Header>
      <Body>
        <SubmittedDraftOfEADESAD>
          <Attributes>
            <SubmissionMessageType>1</SubmissionMessageType>
            <DeferredSubmissionFlag>1</DeferredSubmissionFlag>
          </Attributes>
          <ConsigneeTrader language=""en"">
            <Traderid>GBWK000097101</Traderid>
            <TraderName>00200</TraderName>
            <StreetName>Some address</StreetName>
            <Postcode>PO123</Postcode>
            <City>City</City>
          </ConsigneeTrader>
          <ConsignorTrader language=""en"">
            <TraderExciseNumber>GBWK895164000</TraderExciseNumber>
            <TraderName>Sabic UK Petrochemicals Limited</TraderName>
            <StreetName>Wilton Centre  Po Box 99</StreetName>
            <Postcode>TS10 4YA</Postcode>
            <City>Middlesbrough</City>
          </ConsignorTrader>
          <PlaceOfDispatchTrader language=""en"">
            <ReferenceOfTaxWarehouse>GB0000A004554</ReferenceOfTaxWarehouse>
            <TraderName>Sabic UK Petrochemicals Limited</TraderName>
            <StreetName>Wilton Centre, Po Box 99</StreetName>
            <Postcode>TS90 9JE</Postcode>
            <City>Middlesbrough</City>
          </PlaceOfDispatchTrader>
          <DeliveryPlaceTrader language=""en"">
            <Traderid>GB00000097101</Traderid>
            <TraderName>Evonik Oxeno Antwerpen Nv</TraderName>
            <StreetName>Tijsmanstunnel West</StreetName>
            <Postcode>NA</Postcode>
            <City>Antwerp</City>
          </DeliveryPlaceTrader>
          <CompetentAuthorityDispatchOffice>
            <ReferenceNumber>GB004098</ReferenceNumber>
          </CompetentAuthorityDispatchOffice>
          <FirstTransporterTrader language=""en"">
            <TraderName>Chemgas Shipping Bv</TraderName>
            <StreetName>Gedempte Zalmhaven 4G</StreetName>
            <Postcode>3011BT</Postcode>
            <City>Rotterdam</City>
          </FirstTransporterTrader>
          <HeaderEadEsad>
            <DestinationTypeCode>1</DestinationTypeCode>
            <JourneyTime>D01</JourneyTime>
            <TransportArrangement>1</TransportArrangement>
          </HeaderEadEsad>
          <TransportMode>
            <TransportModeCode>1</TransportModeCode>
          </TransportMode>
          <MovementGuarantee>
            <GuarantorTypeCode>0</GuarantorTypeCode>
          </MovementGuarantee>
          <BodyEadEsad>
            <BodyRecordUniqueReference>1</BodyRecordUniqueReference>
            <ExciseProductCode>B000</ExciseProductCode>
            <CnCode>27111900</CnCode>
            <Quantity>1783691.000</Quantity>
            <GrossMass>1783693.000</GrossMass>
            <NetMass>1783692.000</NetMass>
            <CommercialDescription language=""en"">C4 Raffinate 1</CommercialDescription>
            <MaturationPeriodOrAgeOfProducts language=""en"">Maturation 123</MaturationPeriodOrAgeOfProducts>
            <Package>
              <KindOfPackages>VL</KindOfPackages>
            </Package>
          </BodyEadEsad>
          <EadEsadDraft>
            <LocalReferenceNumber>EMC0000000008371</LocalReferenceNumber>
            <InvoiceNumber>000000001</InvoiceNumber>
            <InvoiceDate>2021-09-01</InvoiceDate>
            <OriginTypeCode>1</OriginTypeCode>
            <DateOfDispatch>2021-09-02</DateOfDispatch>
            <TimeOfDispatch>00:00:00</TimeOfDispatch>
          </EadEsadDraft>
          <TransportDetails>
            <TransportUnitCode>1</TransportUnitCode>
            <IdentityOfTransportUnits>ABC</IdentityOfTransportUnits>
            <ComplementaryInformation language=""en"">Some Info</ComplementaryInformation>
          </TransportDetails>
        </SubmittedDraftOfEADESAD>
      </Body>
    </IE815>
  </s:Body>";
			var expectedXmlDocument = new XmlDocument();
			expectedXmlDocument.LoadXml(expectedXML);
			#endregion

			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");

			var actualXmlDocument = OrchestrationHelpers.ExtractBodyForIrMark(inputXmlDocument, namespaceManager);

			Assert.AreEqual(Regex.Replace(expectedXmlDocument.OuterXml.Replace("\r\n", string.Empty), @"\s+", ""), Regex.Replace(actualXmlDocument.OuterXml.Replace("\r\n", string.Empty), @"\s+", ""));
		}

		[TestMethod]
		public void TestCreateIRValue()
		{
			#region Input XML
			var inputXML = @"<s:Body xmlns:s=""http://www.w3.org/2003/05/soap-envelope"">
    <IE815 xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.01"">
	  <Header>
        <MessageSender xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">NDEA.GB</MessageSender>
        <MessageRecipient xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">NDEA.GB</MessageRecipient>
        <DateOfPreparation xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">2023-02-23</DateOfPreparation>
        <TimeOfPreparation xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">14:38:02.5797324</TimeOfPreparation>
        <MessageIdentifier xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.01"">00000016304</MessageIdentifier>
      </Header>
      <Body>
        <SubmittedDraftOfEADESAD>
          <Attributes>
            <SubmissionMessageType>1</SubmissionMessageType>
            <DeferredSubmissionFlag>1</DeferredSubmissionFlag>
          </Attributes>
          <ConsigneeTrader language=""en"">
            <Traderid>GBWK000097101</Traderid>
            <TraderName>00200</TraderName>
            <StreetName>Some address</StreetName>
            <Postcode>PO123</Postcode>
            <City>City</City>
          </ConsigneeTrader>
          <ConsignorTrader language=""en"">
            <TraderExciseNumber>GBWK895164000</TraderExciseNumber>
            <TraderName>Sabic UK Petrochemicals Limited</TraderName>
            <StreetName>Wilton Centre  Po Box 99</StreetName>
            <Postcode>TS10 4YA</Postcode>
            <City>Middlesbrough</City>
          </ConsignorTrader>
          <PlaceOfDispatchTrader language=""en"">
            <ReferenceOfTaxWarehouse>GB0000A004554</ReferenceOfTaxWarehouse>
            <TraderName>Sabic UK Petrochemicals Limited</TraderName>
            <StreetName>Wilton Centre, Po Box 99</StreetName>
            <Postcode>TS90 9JE</Postcode>
            <City>Middlesbrough</City>
          </PlaceOfDispatchTrader>
          <DeliveryPlaceTrader language=""en"">
            <Traderid>GB00000097101</Traderid>
            <TraderName>Evonik Oxeno Antwerpen Nv</TraderName>
            <StreetName>Tijsmanstunnel West</StreetName>
            <Postcode>NA</Postcode>
            <City>Antwerp</City>
          </DeliveryPlaceTrader>
          <CompetentAuthorityDispatchOffice>
            <ReferenceNumber>GB004098</ReferenceNumber>
          </CompetentAuthorityDispatchOffice>
          <FirstTransporterTrader language=""en"">
            <TraderName>Chemgas Shipping Bv</TraderName>
            <StreetName>Gedempte Zalmhaven 4G</StreetName>
            <Postcode>3011BT</Postcode>
            <City>Rotterdam</City>
          </FirstTransporterTrader>
          <HeaderEadEsad>
            <DestinationTypeCode>1</DestinationTypeCode>
            <JourneyTime>D01</JourneyTime>
            <TransportArrangement>1</TransportArrangement>
          </HeaderEadEsad>
          <TransportMode>
            <TransportModeCode>1</TransportModeCode>
          </TransportMode>
          <MovementGuarantee>
            <GuarantorTypeCode>0</GuarantorTypeCode>
          </MovementGuarantee>
          <BodyEadEsad>
            <BodyRecordUniqueReference>1</BodyRecordUniqueReference>
            <ExciseProductCode>B000</ExciseProductCode>
            <CnCode>27111900</CnCode>
            <Quantity>1783691.000</Quantity>
            <GrossMass>1783693.000</GrossMass>
            <NetMass>1783692.000</NetMass>
            <CommercialDescription language=""en"">C4 Raffinate 1</CommercialDescription>
            <MaturationPeriodOrAgeOfProducts language=""en"">Maturation 123</MaturationPeriodOrAgeOfProducts>
            <Package>
              <KindOfPackages>VL</KindOfPackages>
            </Package>
          </BodyEadEsad>
          <EadEsadDraft>
            <LocalReferenceNumber>EMC0000000008371</LocalReferenceNumber>
            <InvoiceNumber>000000001</InvoiceNumber>
            <InvoiceDate>2021-09-01</InvoiceDate>
            <OriginTypeCode>1</OriginTypeCode>
            <DateOfDispatch>2021-09-02</DateOfDispatch>
            <TimeOfDispatch>00:00:00</TimeOfDispatch>
          </EadEsadDraft>
          <TransportDetails>
            <TransportUnitCode>1</TransportUnitCode>
            <IdentityOfTransportUnits>ABC</IdentityOfTransportUnits>
            <ComplementaryInformation language=""en"">Some Info</ComplementaryInformation>
          </TransportDetails>
        </SubmittedDraftOfEADESAD>
      </Body>
    </IE815>
  </s:Body>";
			var inputXmlDocument = new XmlDocument();
			inputXmlDocument.LoadXml(inputXML);
			#endregion

			var expectedIrValue = "ttV9uo/Jvw+x1EeI6X3gWIUPfBg=";

			var actualIrValue = OrchestrationHelpers.CreateIRValue(inputXmlDocument);

			Assert.AreEqual(actualIrValue, expectedIrValue);
		}

		[TestMethod]
		public void TestGetMessageCount()
		{
			#region Input XML
			var inputXML = @"<soapenv:Envelope xmlns:soapenv=""http://www.w3.org/2003/05/soap-envelope"">
	<soapenv:Body>
		<GetNewMessagesResponse xmlns=""http://www.govtalk.gov.uk/taxation/EMCS/GetNewMessagesResponse/3"">
			<NewMessagesDataResponse xmlns=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Excise/NewMessagesData/3"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
				<Messages/>
				<CountOfMessagesAvailable>5</CountOfMessagesAvailable>
			</NewMessagesDataResponse>
		</GetNewMessagesResponse>
	</soapenv:Body>
</soapenv:Envelope>
";
			#endregion

			Assert.AreEqual(5, OrchestrationHelpers.GetMessageCount(inputXML));
		}

		[TestMethod]
		public void TestDeleteConfiguration()
		{
			var config = gbCustomsEmcsConfig();
			var mockClientRegistrationAccessor = MockRepository.GenerateStrictMock<IClientRegistrationAccessor>();
			OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;

			mockClientRegistrationAccessor.Expect(x => x.UpdateFlag1("a7259690-55cb-4764-9b18-6c0a18c79a35", "GBCustoms-EMCS", "", string.Empty, 0));

			OrchestrationHelpers.MarkConfigurationAsCompleted(config, "a7259690-55cb-4764-9b18-6c0a18c79a35");
			mockClientRegistrationAccessor.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestSubscribePreSubmissionCorrelations_OneSubscription()
		{
			var originalRecipient = "GBCustoms-ICSGB";

			var outboundXmlCC015B = @"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
	<Header>
		<GBCustomsRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
			<Provider>CTCGB</Provider>
			<Service>Depart</Service>
			<Credentials Key=""HYECM2.GB896458895023.CTC""/>
			<JobNumber>NCT00009891</JobNumber>
			<ServiceReference/>
			<Version>1.0</Version>
			<ContentType>XML</ContentType>
		</GBCustomsRequest>
	</Header>
	<Body>
		<CC015B>
			<SynIdeMES1>UNOC</SynIdeMES1>
			<SynVerNumMES2>3</SynVerNumMES2>
			<MesRecMES6>NCTS</MesRecMES6>
			<DatOfPreMES9>210916</DatOfPreMES9>
			<TimOfPreMES10>1209</TimOfPreMES10>
			<IntConRefMES11>1145</IntConRefMES11>
			<AppRefMES14>NCT00000891</AppRefMES14>
			<TesIndMES18>1</TesIndMES18>
			<MesIdeMES19>1145</MesIdeMES19>
			<MesTypMES20>CC015B</MesTypMES20>
			<ComAccRefMES21>CC015BValue</ComAccRefMES21>
			<HEAHEA>
				<RefNumHEA4>NCT00000891</RefNumHEA4>
			</HEAHEA>
		</CC015B>
	</Body>
</ns0:GBCustoms>";

			var outboundXmlCC045A = @"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
	<Header>
		<GBCustomsRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
			<Provider>CTCGB</Provider>
			<Service>Depart</Service>
			<Credentials Key=""HYECM2.GB896458895023.CTC""/>
			<JobNumber>NCT00009891</JobNumber>
			<ServiceReference/>
			<Version>1.0</Version>
			<ContentType>XML</ContentType>
		</GBCustomsRequest>
	</Header>
	<Body>
		<CC045A>
			<SynIdeMES1>UNOC</SynIdeMES1>
			<SynVerNumMES2>3</SynVerNumMES2>
			<MesRecMES6>NCTS</MesRecMES6>
			<DatOfPreMES9>210916</DatOfPreMES9>
			<TimOfPreMES10>1209</TimOfPreMES10>
			<IntConRefMES11>1145</IntConRefMES11>
			<AppRefMES14>NCT00000891</AppRefMES14>
			<TesIndMES18>1</TesIndMES18>
			<MesIdeMES19>1145</MesIdeMES19>
			<MesTypMES20>CC015B</MesTypMES20>
			<ComAccRefMES21>CC045AValue</ComAccRefMES21>
			<HEAHEA>
				<RefNumHEA4>NCT00000891</RefNumHEA4>
			</HEAHEA>
		</CC045A>
	</Body>
</ns0:GBCustoms>";

			var mockClientRegistrationAccessor = MockClientRegistrationAccessor(originalRecipient);
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GBCTID", originalRecipient, "WTLDTWJLI", "CC015BValue", outboundXmlCC015B, null)).Repeat.Once();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GBCTID", originalRecipient, "WTLDTWJLI", "CC045AValue", outboundXmlCC045A, null)).Repeat.Once();

			var mockContext = GenerateMockContext(originalRecipient);

			OrchestrationHelpers.ClientRegistrationAccessor = () => mockClientRegistrationAccessor;
			OrchestrationHelpers.ContextFactory = () => mockContext;
			OrchestrationHelpers.DataModelAccessor = () => mockDataModelAccessor;

			OrchestrationHelpers.SubscribePreSubmissionCorrelations(MockRepository.GenerateStub<ILog>(), outboundXmlCC015B, originalRecipient, "WTLDTWJLI");
			OrchestrationHelpers.SubscribePreSubmissionCorrelations(MockRepository.GenerateStub<ILog>(), outboundXmlCC045A, originalRecipient, "WTLDTWJLI");

			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestSubscribePreSubmissionCorrelations_NoSubscription()
		{
			var originalRecipient = "GBCustoms-ICSGB";

			var outboundXml = @"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
	<Header>
		<GBCustomsRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
			<Provider>CTCGB</Provider>
			<Service>Depart</Service>
			<Credentials Key=""HYECM2.GB896458895023.CTC""/>
			<JobNumber>NCT00009891</JobNumber>
			<ServiceReference/>
			<Version>1.0</Version>
			<ContentType>XML</ContentType>
		</GBCustomsRequest>
	</Header>
	<Body>
	</Body>
</ns0:GBCustoms>";

			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Repeat.Never();

			OrchestrationHelpers.SubscribePreSubmissionCorrelations(MockRepository.GenerateStub<ILog>(), outboundXml, originalRecipient, "WTLDTWJLI");

			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestSplitFormData()
		{
			#region Response Data
			var responseData = @"{
""x-amz-credential"": ""AKIAWBITZ3CPLGEV22M6/20231024/eu-west-2/s3/aws4_request"",
""x-amz-meta-upscan-initiate-response"": ""2023-10-24T09:55:37.967399Z"",
""x-amz-meta-original-filename"": ""${filename}"",
""x-amz-algorithm"": ""AWS4-HMAC-SHA256"",
""x-amz-signature"": ""f8ea928f02179d6479a781e09a514781de3c787545d69bd2172830899c26e37e"",
}";
			#endregion

			#region Expected Result
			var expectedResult = new Dictionary<string, string>()
			{
				{ "x-amz-credential", "AKIAWBITZ3CPLGEV22M6/20231024/eu-west-2/s3/aws4_request" },
				{ "x-amz-meta-upscan-initiate-response", "2023-10-24T09:55:37.967399Z" },
				{ "x-amz-meta-original-filename", "${filename}" },
				{ "x-amz-algorithm", "AWS4-HMAC-SHA256" },
				{ "x-amz-signature", "f8ea928f02179d6479a781e09a514781de3c787545d69bd2172830899c26e37e" },
			};
			#endregion

			var actualResult = OrchestrationHelpers.SplitFormData(responseData);

			CollectionAssert.AreEquivalent(expectedResult, actualResult);
		}

		[TestMethod]
		public void TestCreateLargeFileMessage()
		{
			#region Fields Value
			var fields = @"{
""x-amz-credential"": ""AKIAWBITZ3CPLGEV22M6/20231024/eu-west-2/s3/aws4_request"",
""x-amz-meta-upscan-initiate-response"": ""2023-10-24T09:55:37.967399Z"",
""x-amz-meta-original-filename"": ""${filename}"",
""x-amz-algorithm"": ""AWS4-HMAC-SHA256"",
""x-amz-signature"": ""f8ea928f02179d6479a781e09a514781de3c787545d69bd2172830899c26e37e"",
}";
			#endregion

			#region Expected Result
			var expectedResult = @"Message-ID: <UniqueBoundry>
MIME-Version: 1.0
Content-Type: multipart/form-data; boundary=UniqueBoundry

--UniqueBoundry
Content-Type: text/xml
Content-Disposition: form-data; name=x-amz-credential

AKIAWBITZ3CPLGEV22M6/20231024/eu-west-2/s3/aws4_request
--UniqueBoundry
Content-Type: text/xml
Content-Disposition: form-data; name=x-amz-meta-upscan-initiate-response

2023-10-24T09:55:37.967399Z
--UniqueBoundry
Content-Type: text/xml
Content-Disposition: form-data; name=x-amz-meta-original-filename

${filename}
--UniqueBoundry
Content-Type: text/xml
Content-Disposition: form-data; name=x-amz-algorithm

AWS4-HMAC-SHA256
--UniqueBoundry
Content-Type: text/xml
Content-Disposition: form-data; name=x-amz-signature

f8ea928f02179d6479a781e09a514781de3c787545d69bd2172830899c26e37e
--UniqueBoundry
Content-Type: application/xml
Content-Disposition: form-data; name=file

LargeFileContent
--UniqueBoundry--
";
			#endregion

			var requestMessage = (MemoryStream)OrchestrationHelpers.CreateLargeFileMessage(MockRepository.GenerateStub<ILog>(), fields, "multipart/form-data", "UniqueBoundry", "LargeFileContent");
			Assert.IsNotNull(requestMessage);
			Assert.IsTrue(requestMessage.Length > 0);

			requestMessage.Position = 0;

			var result = new StreamReader(requestMessage).ReadToEnd();

			Assert.AreEqual(expectedResult, result);
		}

		[TestMethod]
		public void TestGetCodeMappingService()
		{
			var result = string.Empty;
			var mockLogger = MockRepository.GenerateMock<ILog>();

			result = OrchestrationHelpers.GetCodeMappingService(mockLogger, "GBCTC", "Arrive", "SomeSeviceReference");
			Assert.AreEqual("Arrive", result);

			result = OrchestrationHelpers.GetCodeMappingService(mockLogger, "GBCTC", "Arrive", "");
			Assert.AreEqual("Arrive", result);

			result = OrchestrationHelpers.GetCodeMappingService(mockLogger, "EMCS", "Consignor", "SomeSeviceReference");
			Assert.AreEqual("Consignor", result);

			result = OrchestrationHelpers.GetCodeMappingService(mockLogger, "EMCS", "Consignor", "");
			Assert.AreEqual("FirstMessage", result);
		}

		[TestMethod]
		public void TestDeserializeICSParameters()
		{
			const string inputXml = @"<TypedPolling xmlns=""http://schemas.microsoft.com/Sql/2008/05/TypedPolling/GBCTLPolling"">
	<TypedPollingResultSet0>
		<TypedPollingResultSet0>
			<Provider>GBCustomsTest-ICSGB</Provider>
			<Subscriber>HYEDUKCM2</Subscriber>
			<CredentialsKey>HYECM2.GB696969696969.DJC</CredentialsKey>
		</TypedPollingResultSet0>
		<TypedPollingResultSet0>
			<Provider>GBCustomsTest-ICSGB</Provider>
			<Subscriber>HYETSTTST</Subscriber>
			<CredentialsKey>HYECM2.GB696969696969.DJC</CredentialsKey>
		</TypedPollingResultSet0>
		<TypedPollingResultSet0>
			<Provider>GBCustomsTest-ICSGB</Provider>
			<Subscriber>HYEDUKCM2</Subscriber>
			<CredentialsKey>HYECM2.GB945390992000.CDS</CredentialsKey>
		</TypedPollingResultSet0>
		<TypedPollingResultSet0>
			<Provider>GBCustomsTest-ICSGB</Provider>
			<Subscriber>HYEDUKCM2</Subscriber>
			<CredentialsKey>HYECM2.GB945390992000.CTC</CredentialsKey>
		</TypedPollingResultSet0>
		<TypedPollingResultSet0>
			<Provider>GBCustomsTest-ICSGB</Provider>
			<Subscriber>HYETSTTST</Subscriber>
			<CredentialsKey>HYETST.GB048834222514.WP1</CredentialsKey>
		</TypedPollingResultSet0>
		<TypedPollingResultSet0>
			<Provider>GBCustomsTest-ICSGB</Provider>
			<Subscriber>YASYUKTRN</Subscriber>
			<CredentialsKey>YASTRN.GB593411144000.CDS</CredentialsKey>
		</TypedPollingResultSet0>
	</TypedPollingResultSet0>
</TypedPolling>";

			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(inputXml);

			var result = OrchestrationHelpers.DeserializeICSParameters(xmlDoc);

			Assert.AreEqual(6, result.PollingResults.Count());
			Assert.AreEqual("HYECM2.GB696969696969.DJC", result.PollingResults[0].CredentialsKey);
			Assert.AreEqual("HYEDUKCM2", result.PollingResults[0].Subscriber);
			Assert.AreEqual("GBCustomsTest-ICSGB", result.PollingResults[0].Provider);
		}

		#region configxml
		private static readonly string ConfigXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<EMCSConfig xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Service>IE815</Service>
  <Recipient>GBCustoms-EMCS</Recipient>
  <ErrorCount>0</ErrorCount>
  <OutboundDate>2020-12-16T12:34:56</OutboundDate>
  <LastPollingDate>2020-12-16T12:34:56</LastPollingDate>
  <RequestHeaders>
    <HttpHeader>
      <Key>RequestKey</Key>
      <Value>RequestValue</Value>
    </HttpHeader>
  </RequestHeaders>
  <ResponseHeaders>
    <HttpHeader>
      <Key>ResponseKey</Key>
      <Value>ResponseValue</Value>
    </HttpHeader>
  </ResponseHeaders>
  <CredentialsKey>HYEMIK.GB896458895023.D01</CredentialsKey>
</EMCSConfig>";
        #endregion

		#region gbCustomsXml

		string gbCustomsXml = @"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
	<Header>
		<GBCustomsRequest>
			<Provider>CTCGB</Provider>
			<Service>Depart</Service>
			<Credentials Key=""HYEMIK.GB896458895023.D01"" />
			<JobNumber>NCT00031413</JobNumber>
			<ServiceReference>123456</ServiceReference>
			<Version>1.0</Version>
			<ContentType>xml</ContentType>  
			<RiskingSimulation>
				<Result>reject</Result>
				<Reason>nonUniqueLRN</Reason>
				<Latency>1000</Latency>
				<Intervention>true</Intervention>
				<InterventionLatency>2000</InterventionLatency>
			</RiskingSimulation>
		</GBCustomsRequest>
	</Header>
	<Body>
		<ie:CC315A xmlns:ie=""http://ics.dgtaxud.ec/CC315A"">
			<MesSenMES3>GB1234231/1234567890</MesSenMES3>
			<MesRecMES6>ABCD1234</MesRecMES6>
			<DatOfPreMES9>091231</DatOfPreMES9>
			<TimOfPreMES10>2359</TimOfPreMES10>
			<MesIdeMES19>ABCD1234</MesIdeMES19>
			<MesTypMES20>CC315A</MesTypMES20>
			<HEAHEA>
				<RefNumHEA4>ABCD1234</RefNumHEA4>
				<TraModAtBorHEA76>4</TraModAtBorHEA76>
				<TotNumOfIteHEA305>1</TotNumOfIteHEA305>
				<TotNumOfPacHEA306>1</TotNumOfPacHEA306>
				<TotGroMasHEA307>1.000</TotGroMasHEA307>
				<DecPlaHEA394>EXAMPLE</DecPlaHEA394>
				<ComRefNumHEA>ABC1234D</ComRefNumHEA>
				<ConRefNumHEA>ABC1234D</ConRefNumHEA>
				<PlaLoaGOOITE334>EXAMPLE</PlaLoaGOOITE334>
				<PlaUnlGOOITE334>EXAMPLE</PlaUnlGOOITE334>
				<DecDatTimHEA114>200912312359</DecDatTimHEA114>
			</HEAHEA>
			<TRACONCE1>
				<TINCE159>GBab12</TINCE159>
			</TRACONCE1>
			<GOOITEGDS>
				<IteNumGDS7>1</IteNumGDS7>
				<PRODOCDC2>
					<DocTypDC21>AB12</DocTypDC21>
					<DocRefDC23>ABCDEF123456</DocRefDC23>
				</PRODOCDC2>
				<COMCODGODITM>
					<ComNomCMD1>1234</ComNomCMD1>
				</COMCODGODITM>
				<PACGS2>
					<KinOfPacGS23>VR</KinOfPacGS23>
				</PACGS2>
			</GOOITEGDS>
			<ITI>
				<CouOfRouCodITI1>AB</CouOfRouCodITI1>
			</ITI>
			<ITI>
				<CouOfRouCodITI1>AB</CouOfRouCodITI1>
			</ITI>
			<PERLODSUMDEC>
				<TINPLD1>GBCD12345EFG</TINPLD1>
			</PERLODSUMDEC>
			<CUSOFFFENT730>
				<RefNumCUSOFFFENT731>ABCD1234</RefNumCUSOFFFENT731>
				<ExpDatOfArrFIRENT733>200912312359</ExpDatOfArrFIRENT733>
			</CUSOFFFENT730>
		</ie:CC315A>
	</Body>
</ns0:GBCustoms>
";

		string gbCustomsXmlEmcs = @"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
  <Header>
    <GBCustomsRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
      <Provider>EMCS</Provider>
      <Service>Unsolicited</Service>
      <Credentials Key=""HYECM2.GB945390992000.EM2"" />
      <JobNumber>Job123</JobNumber>
      <Version>1.0</Version>
      <ContentType>xml</ContentType>
      <ServiceReference>3f757fe2-6185-42f6-be24-cfca61e5f82a</ServiceReference>
    </GBCustomsRequest>
  </Header>
  <Body>
    <PushData>
      <MessageUri>/movements/4d543312-c043-4257-b617-2ac52594001d/messages/GB100000000305228</MessageUri>
      <RequestId></RequestId>
    </PushData>
  </Body>
</ns0:GBCustoms>";

		#endregion

		#region ICS XML
		const string xmlStringOutcomesResponse = @"<entryDeclarationResponses>
  <response>
    <correlationId>0JRF7UncK0t004</correlationId>
    <link>/customs/imports/outcomes/0JRF7UncK0t004</link>
    <MRN>10GB08I01234567891</MRN>
  </response>
  <response>
    <correlationId>0JRF7UncAqr004</correlationId>
    <link>/customs/imports/outcomes/0987654321</link>
  </response>
</entryDeclarationResponses>
";

		const string xmlStringOutcomeResponse = @"<outcomeResponse xmlns:cc3=""http://ics.dgtaxud.ec/CC328A"">
  <response>
    <cc3:CC328A>
      <MesSenMES3>GB000012340003/1234567890</MesSenMES3>
      <MesRecMES6>GB000012340003/1234567890</MesRecMES6>
      <DatOfPreMES9>190114</DatOfPreMES9>
      <TimOfPreMES10>0945</TimOfPreMES10>
      <MesIdeMES19>MSUI11235227</MesIdeMES19>
      <MesTypMES20>CC328A</MesTypMES20>
      <CorIdeMES25>0JRF7UncK0t004</CorIdeMES25>
      <HEAHEA>
        <RefNumHEA4>Preeti_315A_TC001</RefNumHEA4>
        <DocNumHEA5>10GB08I01234567891</DocNumHEA5>
        <DecRegDatTimHEA115>201901140945</DecRegDatTimHEA115>
      </HEAHEA>
      <CUSOFFLON>
        <RefNumCOL1>ES000055</RefNumCOL1>
      </CUSOFFLON>
      <PERLODSUMDEC>
        <TINPLD1>GB000012340002</TINPLD1>
      </PERLODSUMDEC>
      <CUSOFFFENT730>
        <RefNumCUSOFFFENT731>GB000011</RefNumCUSOFFFENT731>
      </CUSOFFFENT730>
    </cc3:CC328A>
  </response>
  <acknowledgement method = 'DELETE' href='/customs/imports/outcomes/0JRF7UncK0t004'/>
</outcomeResponse>
";
		#endregion

		#region EMCS Test Xml
		const string EMCS_RequestXml = @"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
	<Header>
		<GBCustomsRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
	        <Provider>EMCS</Provider>
	        <Service>IE815</Service> 
	        <Credentials Key=""HYEMIK.GB896458895023.D01"" />
	        <ServiceReference>123456</ServiceReference>
	        <JobNumber>B00001000</JobNumber>
	        <ContentType>XML</ContentType>
	        <Version>3</Version>
        </GBCustomsRequest>
	</Header>
	<Body>
		<IE815>TestData</IE815>
	</Body>
</ns0:GBCustoms>
";

		const string EMCS_ExpectedXml = @"<?xmlversion=""1.0""encoding=""utf-8""?><s:Envelope xmlns:s=""http://www.w3.org/2003/05/soap-envelope"">
  <s:Header>
    <Action a:mustUnderstand=""1"" xmlns=""http://schemas.microsoft.com/ws/2005/05/addressing/none"" xmlns:a=""http://www.w3.org/2003/05/soap-envelope"">http://www.hmrc.gov.uk/emcs/submitdraftmovement</Action>
    <h:Security xmlns:h=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
      <h:BinarySecurityToken ValueType=""http://www.hmrc.gov.uk#MarkToken"">OVfKGIONu1uVppbFmxWKaV4FyhY=</h:BinarySecurityToken>
      <h:UsernameToken>
        <h:Username>ASTEST</h:Username>
        <h:Password>123</h:Password>
      </h:UsernameToken>
    </h:Security>
    <h:EMCSInfo xmlns:h=""http://www.hmrc.gov.uk/ws/emcs-info-header/1"" xmlns=""http://www.hmrc.gov.uk/ws/emcs-info-header/1"">
      <h:ConsignorId>GBWK000000000</h:ConsignorId>
    </h:EMCSInfo>
    <h:Info xmlns:h=""http://www.hmrc.gov.uk/ws/info-header/1"" xmlns=""http://www.hmrc.gov.uk/ws/info-header/1"">
      <h:VendorName URI=""www.wisetechglobal.com"">WiseTech Global</h:VendorName>
      <h:VendorID>1601</h:VendorID>
      <h:VendorProduct Version=""1.0"">CargoWise</h:VendorProduct>
      <h:ServiceMessageType>HMRC-EMCS-IE815-DIRECT</h:ServiceMessageType>
    </h:Info>
  </s:Header>
  <s:Body>
    <IE815>TestData</IE815>
  </s:Body>
</s:Envelope>";
		#endregion
	}
}
