using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	public abstract class TWXmlTestCaseWithFactory : TestCaseWithFactory
	{
		public string GetMessageText(string name, string number, string procedure, string nameCode, string borderTransportMeans, string validationCode, string releaseDateTime = null, bool assertXML = true, string dueDateTime = null)
		{
			var messageText = GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile." + name);
			xmlDocument.LoadXml(messageText);
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace("a", xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);
			if (!string.IsNullOrEmpty(number))
			{
				SetNodeValueByPath("{0}Response/{0}Declaration/{0}ID", number, nameSpace);
				SetNodeValueByPath("{0}Declaration/{0}ID", number, nameSpace);
				if (assertXML)
				{
					AssertXMLContains("<ID>" + number + "</ID>", xmlDocument.OuterXml);
				}
			}

			if (procedure != null)
			{
				SetNodeValueByPath("{0}Response/{0}Declaration/{0}GovernmentProcedure/{0}tw_TransportTypeCode", procedure, nameSpace);
				SetNodeValueByPath("{0}Declaration/{0}GovernmentProcedure/{0}tw_TransportTypeCode", procedure, nameSpace);
				if (assertXML)
				{
					AssertXMLContains("<GovernmentProcedure><tw_TransportTypeCode>" + procedure + "</tw_TransportTypeCode></GovernmentProcedure>", xmlDocument.OuterXml);
				}
			}

			if (nameCode != null)
			{
				SetNodeValueByPath("{0}Response/{0}Status/{0}NameCode", nameCode, nameSpace);
				SetNodeValueByPath("{0}Response/{0}Declaration/{0}GoodsShipment/{0}GovernmentAgencyGoodsItem/{0}Status/{0}NameCode", nameCode, nameSpace);
				if (assertXML)
				{
					AssertXMLContains("<NameCode>" + nameCode + "</NameCode>", xmlDocument.OuterXml);
				}
			}

			if (borderTransportMeans != null)
			{
				SetNodeValueByPath("{0}Response/{0}Declaration/{0}BorderTransportMeans/{0}TypeCode", borderTransportMeans, nameSpace);
				if (assertXML)
				{
					AssertXMLContains("<TypeCode>" + borderTransportMeans + "</TypeCode>", xmlDocument.OuterXml);
				}
			}

			if (validationCode != null)
			{
				SetNodeValueByPath("{0}Response/{0}Declaration/{0}GoodsShipment/{0}GovernmentAgencyGoodsItem/{0}Error/{0}ValidationCode", validationCode, nameSpace);
				if (assertXML)
				{
					AssertXMLContains("<ValidationCode>" + validationCode + "</ValidationCode>", xmlDocument.OuterXml);
				}
			}

			if (releaseDateTime != null)
			{
				SetNodeValueByPath("{0}Response/{0}Status/{0}ReleaseDateTime", releaseDateTime, nameSpace);
				if (assertXML)
				{
					AssertXMLContains("<ReleaseDateTime>" + releaseDateTime + "</ReleaseDateTime>", xmlDocument.OuterXml);
				}
			}

			if (dueDateTime != null)
			{
				SetNodeValueByPath("{0}Response/{0}Declaration/{0}DutyTaxFee/{0}Payment/{0}DueDateTime", dueDateTime, nameSpace);
				if (assertXML)
				{
					AssertXMLContains("<DueDateTime>" + dueDateTime + "</DueDateTime>", xmlDocument.OuterXml);
				}
			}

			return xmlDocument.OuterXml;
		}

		public static string GetTWNotification(string eventType, string entryNumberType, string entryNumber, string interchangeNumber, string messageType = "")
		{
			var messageText = GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.TWCustomsDeliveryNotification.xml");
			var doc = XDocument.Parse(messageText, LoadOptions.None);
			var node = doc.XPathSelectElement("//*[local-name()='Event']/*[local-name()='EventType']");
			node.Value = eventType;
			node = doc.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='EntryNumberType']/*[local-name()='Value']");
			node.Value = entryNumberType;
			node = doc.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='EntryNumber']/*[local-name()='Value']");
			node.Value = entryNumber;
			node = doc.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='InterchangeNumber']/*[local-name()='Value']");
			node.Value = interchangeNumber;
			node = doc.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='MessageType']/*[local-name()='Value']");
			node.Value = messageType;
			return doc.ToString();
		}

		void SetNodeValueByPath(string xpathFormat, string value, XmlNamespaceManager nameSpace)
		{
			var node = xmlDocument.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, xpathFormat, "a:"), nameSpace);
			if (node != null)
			{
				node.InnerText = value;
			}
		}

		public static ZString GetExpectedMessageXML(ZString path)
		{
			using (var inStream = typeof(TWMessageProcessorTest).Assembly.GetManifestResourceStream(path))
			{
				return new StreamReader(inStream).ReadToEnd();
			}
		}

		public static void AddN5110TestMessage(CusEntryHeader entryHeader, string incomingPayResponseNo, DutyTaxFeeForTest[] dutyTaxFees, string dueDateTime, decimal totalDutyTaxFeeAmount, decimal otherChargeDeductionAmount)
		{
			var testMessage = entryHeader.Messages.AddNew();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "PRS";
			testMessage.EM_MessageNum = "TWIN1";
			testMessage.EM_MessageText = GetN5110MessageText(entryHeader.EntryNumber, incomingPayResponseNo, dutyTaxFees, dueDateTime, totalDutyTaxFeeAmount, otherChargeDeductionAmount);
			testMessage.EM_MessageType = MessageTypeList.Codes.TPC;
		}

		public static void GenerateCusEntryPayInfo(CusEntryHeader entryHeader, ZString incomingPayResponseNo, ZString transactionType, ZDecimal paymentAmount, decimal otherChargeDeductionAmount = 0m)
		{
			var newEntryPayInfo = entryHeader.EntryPayInfos.AddNew();
			newEntryPayInfo.C9_PaymentAmount = paymentAmount;
			newEntryPayInfo.C9_TransactionType = transactionType;
			newEntryPayInfo.C9_IncomingPayResponseNo = incomingPayResponseNo;
			newEntryPayInfo.C9_PaymentDate = new ZDateTime(2021, 8, 3);
			newEntryPayInfo.C9_PaymentReference = "88888888";
			newEntryPayInfo.C9_PaymentParty = "X";
			newEntryPayInfo.C9_BankAccount = "3070500001";
			newEntryPayInfo.C9_PaymentReasonCode = "2";
			newEntryPayInfo.C9_ReceiptDate = ZDate.Today;
			newEntryPayInfo.OtherChargeDeductionAmount = otherChargeDeductionAmount;
		}

		public static void AddN5111TestMessage(CusEntryHeader entryHeader, string incomingPayResponseNo, string issueDateTime, decimal paymentAmount)
		{
			var testMessage = entryHeader.Messages.AddNew();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "PRS";
			testMessage.EM_MessageNum = "TWIN1";
			testMessage.EM_MessageText = GetN5111MessageText(entryHeader.EntryNumber, incomingPayResponseNo, "D10", issueDateTime, paymentAmount);
			testMessage.EM_MessageType = MessageTypeList.Codes.TAD;
		}

		public static ZString GetN5110MessageText(ZString entryNumber, ZString incomingPayResponseNo, DutyTaxFeeForTest[] dutyTaxFees, string dueDateTime = "2021-08-03", decimal totalDutyTaxFeeAmount = 99999, decimal otherChargeDeductionAmount = 6164039)
		{
			var dutyTaxFeesNodes = new ZStringBuilder();
			foreach (var fee in dutyTaxFees)
			{
				dutyTaxFeesNodes.AppendFormat(@"<DutyTaxFee><AdValoremTaxBaseAmount>{0}</AdValoremTaxBaseAmount><TypeCode>{1}</TypeCode></DutyTaxFee>", fee.AdValoremTaxBaseAmount.ToString(), fee.TypeCode);
			}

			var messageTemplate = @"<?xml version=""1.0"" encoding =""UTF -8"" ?>
<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns =""urn:wco:datamodel:TW:N5110:R-00-05"">
  <BankAccount>
    <ReferenceID>3070500001</ReferenceID>
    <ID>88888888</ID>
  </BankAccount>
  <Status>
    <NameCode>C2</NameCode>
  </Status>
  <Declaration>
    <ID>{0}</ID>
    <TotalPackageQuantity>5</TotalPackageQuantity>
    <TypeCode>F2</TypeCode>
    <Agent>
      <ID>094</ID>
      <RoleCode>CB</RoleCode>
      <tw_SubBoxID>0</tw_SubBoxID>
    </Agent>
    <BorderTransportMeans>
      <ArrivalDateTime>2021-07-20</ArrivalDateTime>
    </BorderTransportMeans>
    <DutyTaxFee>
      <tw_TotalDutyTaxFeeAmount>{4}</tw_TotalDutyTaxFeeAmount>
      <Payment>
        <DueDateTime>{3}</DueDateTime>
        <ReferenceID>{1}</ReferenceID>
        <tw_CollectionTypeCode>6AX</tw_CollectionTypeCode>
        <tw_IssueReasonCode>2</tw_IssueReasonCode>
      </Payment>
    </DutyTaxFee>
    <GoodsShipment>
      <Consignment>
        <BorderTransportMeans>
          <ID>3EYV</ID>
          <JourneyID>020</JourneyID>
        </BorderTransportMeans>
        <TransportContractDocument>
          <ID>ABF210094A2726</ID>
          <TypeCode>704</TypeCode>
        </TransportContractDocument>
      </Consignment>
      <CustomsValuation>
        <OtherChargeDeductionAmount>{5}</OtherChargeDeductionAmount>
      </CustomsValuation>
      {2}
      <GovernmentAgencyGoodsItem>
        <Commodity>
          <Classification>
            <ID>87032310005</ID>
          </Classification>
        </Commodity>
      </GovernmentAgencyGoodsItem>
    </GoodsShipment>
    <Importer>
      <ID>80327957</ID>
      <Name>HONDA TAIWAN CO., LTD.</Name>
      <tw_ChineseName>台灣本田股份有限公司</tw_ChineseName>
      <tw_TypeCode>58</tw_TypeCode>
    </Importer>
    <Packaging>
      <TypeCode>UNT</TypeCode>
    </Packaging>
    <ResponsibleGovernmentAgency>
      <ID>R9902</ID>
    </ResponsibleGovernmentAgency>
  </Declaration>
</Response>";
			return ZString.Format(messageTemplate, entryNumber, incomingPayResponseNo, dutyTaxFeesNodes.ToString(), dueDateTime, totalDutyTaxFeeAmount, otherChargeDeductionAmount);
		}

		public static ZString GetN5111MessageText(ZString entryNumber, ZString incomingPayResponseNo, ZString depositTypeCode, string issueDateTime = "2021-08-05", decimal paymentAmount = 2431279m)
		{
			var messageTemplate = @"<?xml version=""1.0"" encoding =""UTF -8"" ?>
<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns =""urn:wco:datamodel:TW:N5111:R-00-05"" >
	<IssueDateTime>{3}</IssueDateTime>
	<BankAccount>
		<ReferenceID>3070500001</ReferenceID>
		<ID>88888888</ID>
	</BankAccount>
	<Status>
		<NameCode>C2</NameCode>
	</Status>
	<Declaration>
		<DeclarationOfficeID>AB</DeclarationOfficeID>
		<ID>{0}</ID>
		<Agent>
			<ID>OF5</ID>
			<RoleCode>CB</RoleCode>
			<tw_SubBoxID>0</tw_SubBoxID>
		</Agent>
		<DutyTaxFee>
			<Payment>
				<PaymentAmount>{4}</PaymentAmount>
				<ReferenceID>{1}</ReferenceID>
				<tw_BelongDate>202108</tw_BelongDate>
				<tw_CollectionTypeCode>6AX</tw_CollectionTypeCode>
				<tw_DepositTypeCode>{2}</tw_DepositTypeCode>
				<tw_IssueReasonCode>2</tw_IssueReasonCode>
				<ObligationGuarantee>
					<ReferenceID>AB023080</ReferenceID>
					<SecurityDetailsCode>10</SecurityDetailsCode>
					<Surety>
						<ID>24413656</ID>
						<Name>VOLVO CAR TAIWAN LIMITED</Name>
						<tw_ChineseName></tw_ChineseName>
						<tw_TypeCode>58</tw_TypeCode>
					</Surety>
				</ObligationGuarantee>
			</Payment>
		</DutyTaxFee>
		<ResponsibleGovernmentAgency>
			<ID>R9902</ID>
		</ResponsibleGovernmentAgency>
	</Declaration>
</Response>";
			return ZString.Format(messageTemplate, entryNumber, incomingPayResponseNo, depositTypeCode, issueDateTime, paymentAmount);
		}

		public static ZString GetN5107MessageText(ZString issueDateTime, ZString validationCode1, ZString validationCode2)
		{
			var messageTemplate = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5107:R-00-05"" xsi:schemaLocation=""urn:wco:datamodel:TW:N5107:R-00-05 N5107.xsd"">
	<IssueDateTime>{issueDateTime}</IssueDateTime>
	<AdditionalInformation>
		<LimitDateTime>2019-01-31</LimitDateTime>
	</AdditionalInformation>
	<ContactOffice>
		<ID>*</ID>
	</ContactOffice>
	<Status>
		<NameCode>C2</NameCode>
		<ReleaseDateTime></ReleaseDateTime>
	</Status>
	<Declaration>
		<ID>AAB1082348</ID>
		<TotalPackageQuantity>22</TotalPackageQuantity>
		<TypeCode>G1</TypeCode>
		<Agent>
			<ID>207</ID>
			<RoleCode>CB</RoleCode>
			<tw_SubBoxID>0</tw_SubBoxID>
		</Agent>
		<BorderTransportMeans>
			<ArrivalDateTime>2019-01-30</ArrivalDateTime>
			<TypeCode>1</TypeCode>
		</BorderTransportMeans>
		<GoodsShipment>
			<Consignment>
				<tw_ManifestSerialNumber>0002</tw_ManifestSerialNumber>
				<BorderTransportMeans>
					<JourneyID>19002S</JourneyID>
					<tw_Registration>08F399</tw_Registration>
				</BorderTransportMeans>
				<TransportContractDocument>
					<ID>100810455882</ID>
					<TypeCode>704</TypeCode>
				</TransportContractDocument>
			</Consignment>
			<GovernmentAgencyGoodsItem>
				<SequenceNumeric>0</SequenceNumeric>
				<Error>
					<ValidationCode>{validationCode1}</ValidationCode>
				</Error>
				<Error>
					<ValidationCode>{validationCode2}</ValidationCode>
				</Error>
			</GovernmentAgencyGoodsItem>
		</GoodsShipment>
		<GovernmentProcedure>
			<tw_TransportTypeCode>1</tw_TransportTypeCode>
		</GovernmentProcedure>
		<Packaging>
			<TypeCode>PKG</TypeCode>
		</Packaging>
	</Declaration>
</Response>";
			return messageTemplate;
		}

		public static ZString GetNX5106MessageText(ZString issueDateTime, ZString statusNameCode1, ZString statusNameCode2)
		{
			var messageTemplate = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:NX5106:R-00-04"" xsi:schemaLocation=""urn:wco:datamodel:TW:NX5106:R-00-04 NX5106.xsd"">
	<Status>
		<NameCode>N</NameCode>
	</Status>
	<Declaration>
		<ID>AAB1082348</ID>
		<AdditionalInformation>
			<tw_IssueDateTime>{issueDateTime}</tw_IssueDateTime>
		</AdditionalInformation>
		<Agent>
			<ID>207</ID>
			<RoleCode>CB</RoleCode>
			<tw_SubBoxID>0</tw_SubBoxID>
		</Agent>
		<BorderTransportMeans>
			<TypeCode>1</TypeCode>
		</BorderTransportMeans>
		<GoodsShipment>
			<Consignment>
				<TransportContractDocument>
					<ID>050801301684001</ID>
					<TypeCode>704</TypeCode>
				</TransportContractDocument>
			</Consignment>
			<GovernmentAgencyGoodsItem>
				<SequenceNumeric>25</SequenceNumeric>
				<Status>
					<NameCode>{statusNameCode1}</NameCode>
				</Status>
				<Status>
					<NameCode>{statusNameCode2}</NameCode>
				</Status>
			</GovernmentAgencyGoodsItem>
		</GoodsShipment>
		<GovernmentProcedure>
			<tw_TransportTypeCode>1</tw_TransportTypeCode>
		</GovernmentProcedure>
	</Declaration>
</Response>";
			return messageTemplate;
		}

		public static ZString GetN5204MessageText(ZString releaseDateTime, ZString statementCode1, ZString statementCode2)
		{
			var messageTemplate = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5204:R-00-03"" xmlns:tsw=""urn:SingleWindow:TW"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""urn:wco:datamodel:TW:N5204:R-00-03 N5204.xsd"">
	<FunctionCode>9</FunctionCode>
	<AdditionalInformation>
		<StatementCode>{statementCode1}</StatementCode>
	</AdditionalInformation>
	<AdditionalInformation>
		<StatementCode>{statementCode2}</StatementCode>
	</AdditionalInformation>
	<Status>
		<NameCode>C1</NameCode>
		<ReleaseDateTime>{releaseDateTime}</ReleaseDateTime>
		<tw_TotalPackageQuantity>6</tw_TotalPackageQuantity>
	</Status>
	<Declaration>
		<ID>AAB1092348</ID>
		<TotalGrossMassMeasure>2401.6</TotalGrossMassMeasure>
		<TypeCode>G5</TypeCode>
		<Agent>
			<ID>207</ID>
			<RoleCode>CB</RoleCode>
			<tw_SubBoxID>0</tw_SubBoxID>
		</Agent>
		<BorderTransportMeans>
			<TypeCode>1</TypeCode>
		</BorderTransportMeans>
		<GoodsShipment>
			<Consignment>
				<tw_ShippingOrderNumber>0027</tw_ShippingOrderNumber>
				<BorderTransportMeans>
					<JourneyID>N134</JourneyID>
					<tw_CallSignID>9V7586</tw_CallSignID>
					<tw_Registration>08F328</tw_Registration>
				</BorderTransportMeans>
				<Carrier>
					<ID>1105293</ID>
				</Carrier>
				<DepartureTransportMeans>
					<Name>WAN HAI 273</Name>
				</DepartureTransportMeans>
				<GoodsLocation>
					<ID>TXG0342C</ID>
				</GoodsLocation>
				<TransportContractDocument>
					<ID>*</ID>
					<TypeCode>704</TypeCode>
				</TransportContractDocument>
				<TransportContractDocument>
					<ID>*</ID>
					<TypeCode>714</TypeCode>
				</TransportContractDocument>
				<TransportEquipment>
					<ID>WHLU0235618</ID>
				</TransportEquipment>
			</Consignment>
			<Exporter>
				<ID>11104755</ID>
				<Name>UNI AUTO PARTS MANUFACTURE CO., LTD.</Name>
				<tw_TypeCode>58</tw_TypeCode>
			</Exporter>
			<GovernmentAgencyGoodsItem>
				<Commodity>
					<Classification>
						<ID>73262000900</ID>
					</Classification>
				</Commodity>
				<Error>
					<ValidationCode></ValidationCode>
				</Error>
			</GovernmentAgencyGoodsItem>
		</GoodsShipment>
		<Packaging>
			<MarksNumbers>SANKYO MIZUSHIMA C/NO:1-6 MADE IN TAIWAN</MarksNumbers>
			<TypeCode>PLT</TypeCode>
		</Packaging>
		<BorderTranspotMeans>
			<TypeCode></TypeCode>
		</BorderTranspotMeans>
	</Declaration>
</Response>";
			return messageTemplate;
		}

		public static ZString GetN5116MessageText(ZString releaseDateTime, ZString statementCode1, ZString statementCode2)
		{
			var messageTemplate = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5116:R-00-05"" xsi:schemaLocation=""urn:wco:datamodel:TW:N5116:R-00-05 N5116.xsd"">
	<FunctionCode>9</FunctionCode>
	<AdditionalInformation>
		<StatementCode>{statementCode1}</StatementCode>
	</AdditionalInformation>
	<AdditionalInformation>
		<StatementCode>{statementCode2}</StatementCode>
	</AdditionalInformation>
	<Status>
		<NameCode>C2</NameCode>
		<ReleaseDateTime>{releaseDateTime}</ReleaseDateTime>
		<tw_ReleaseTypeCode>1</tw_ReleaseTypeCode>
		<tw_TotalPackageQuantity>22</tw_TotalPackageQuantity>
	</Status>
	<Declaration>
		<ID>AAB1082348</ID>
		<TotalGrossMassMeasure>5387</TotalGrossMassMeasure>
		<TotalPackageQuantity>22</TotalPackageQuantity>
		<TypeCode>G1</TypeCode>
		<Agent>
			<ID>207</ID>
			<RoleCode>CB</RoleCode>
			<tw_SubBoxID>0</tw_SubBoxID>
		</Agent>
		<BorderTransportMeans>
			<ArrivalDateTime>2019-01-30</ArrivalDateTime>
			<TypeCode>1</TypeCode>
		</BorderTransportMeans>
		<GoodsShipment>
			<Consignment>
				<tw_ManifestSerialNumber>0002</tw_ManifestSerialNumber>
				<BorderTransportMeans>
					<ID>C6AV9</ID>
					<JourneyID>19002S</JourneyID>
					<tw_Registration>08F399</tw_Registration>
				</BorderTransportMeans>
				<GoodsLocation>
					<ID>TXG0102C</ID>
				</GoodsLocation>
				<TransportContractDocument>
					<ID>100810455882</ID>
					<TypeCode>704</TypeCode>
				</TransportContractDocument>
				<TransportEquipment>
					<ID>TCNU4081834</ID>
				</TransportEquipment>
			</Consignment>
			<GovernmentAgencyGoodsItem>
				<Commodity>
					<Classification>
						<ID>87088090003</ID>
					</Classification>
				</Commodity>
			</GovernmentAgencyGoodsItem>
		</GoodsShipment>
		<Importer>
			<ID>03489200</ID>
			<Name>YULON MOTOR CO., LTD.</Name>
			<tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName>
			<tw_TypeCode>58</tw_TypeCode>
		</Importer>
		<Packaging>
			<MarksNumbers>N/M N/NO</MarksNumbers>
			<TypeCode>PKG</TypeCode>
		</Packaging>
	</Declaration>
</Response>";
			return messageTemplate;
		}

		readonly XmlDocument xmlDocument = new XmlDocument();
	}

	public class DutyTaxFeeForTest
	{
		public ZDecimal AdValoremTaxBaseAmount { get; set; }

		public ZString TypeCode { get; set; }
	}
}
