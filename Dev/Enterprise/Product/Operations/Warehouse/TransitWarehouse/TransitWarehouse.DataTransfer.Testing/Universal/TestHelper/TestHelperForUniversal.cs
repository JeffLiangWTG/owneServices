using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class TestHelperForUniversal
	{
		public TestHelperForUniversal(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public StmALog GetMostRecentExportLog(IStmALogParent logParent, string eventCode, params ZGuid[] logPKsToExlude)
			=> logParent.Logs.Find(c => c.SL_SE_NKEvent == eventCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault(l => !logPKsToExlude.Contains(l.PK));

		public EDIMessage GetEDIMessageFromDB(IStmALogParent logParent, string eventCode)
		{
			var exportLog = logParent.Logs.Find(c => c.SL_SE_NKEvent == eventCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();
			return GetEDIMessageFromDB(exportLog);
		}

		public IEnumerable<EDIMessage> GetEDIMessagesFromDB(IStmALogParent logParent, string eventCode)
		{
			return logParent.Logs.Find(c => c.SL_SE_NKEvent == eventCode).Select(log => GetEDIMessageFromDB(log));
		}

		public EDIMessage GetEDIMessageFromDB(StmALog exportLog)
		{
			EDIMessage ediMessage = null;

			if (exportLog != null)
			{
				var query = new ZQuery();
				query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, StmALogSchema.Constants.Prefix);
				query.AddToFilter(GenPivotSchema.XX_Relation1ID, exportLog.PK);
				query.AddToFilter(GenPivotSchema.XX_RelationType, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage);

				var pivot = factory.LoadTop1<GenPivot>(query);

				ediMessage = factory.Load<EDIMessage>(pivot.XX_Relation2ID);
			}

			return ediMessage;
		}

		public string Build2012EventXMLWithEventParameters(string eventType, string dataSourceName = null, string dataSourceKey = null, string department = null, string facility = null, string customsReferenceNumber = null, string location = null, string messageType = null, string referenceNumber = null, string dataTargetName = null, string dataTargetKey = null, string reason = null, string requestNumber = null, string documentName = "Goods Received(CRESA)", string eventParameterType = null)
		{
			var dataSourceElement = dataSourceName != null && dataSourceKey != null ? new XElement("DataSourceCollection", new XElement("DataSource", new[] { new XElement("Key", dataSourceKey), new XElement("Type", dataSourceName) })).ToString() : "";
			var dataTargetElement = dataTargetName != null && dataTargetKey != null ? new XElement("DataTargetCollection", new XElement("DataTarget", new[] { new XElement("Key", dataTargetKey), new XElement("Type", dataTargetName) })).ToString() : "";
			var eventParametersElement = new XElement("EventParameters");
			if (department != null)
			{ eventParametersElement.Add(new XElement("Department", department)); }
			if (facility != null)
			{ eventParametersElement.Add(new XElement("Facility", facility)); }
			if (customsReferenceNumber != null)
			{ eventParametersElement.Add(new XElement("CustomsReferenceNumber", customsReferenceNumber)); }
			if (referenceNumber != null)
			{ eventParametersElement.Add(new XElement("ReferenceNumber", referenceNumber)); }
			if (location != null)
			{ eventParametersElement.Add(new XElement("Location", location)); }
			if (messageType != null)
			{ eventParametersElement.Add(new XElement("MessageType", messageType)); }
			if (requestNumber != null)
			{ eventParametersElement.Add(new XElement("RequestNumber", requestNumber)); }
			if (reason != null)
			{ eventParametersElement.Add(new XElement("Reason", reason)); }
			if (eventParameterType != null)
			{ eventParametersElement.Add(new XElement("Type", eventParameterType)); }

			var xml =
@"
<UniversalEvent>
	<Event>
		<DataContext>
			<DocumentaryOverride>
				<DocumentName>" + documentName + @"</DocumentName>
			</DocumentaryOverride>" + dataSourceElement + dataTargetElement +
$@"
		</DataContext>
		<EventTime>2021-05-28T10:29:00</EventTime>
		<EventType>{eventType}</EventType>" + eventParametersElement +
@"
	</Event>
</UniversalEvent>";

			return GetFormattedXMLText(xml);
		}

		public string Build2011EventXMLWithEventReference(string eventType, string dataSourceName = null, string dataSourceKey = null, string eventReference = null, string dataTargetName = null, string dataTargetKey = null, string documentName = "Goods Received(CRESA)")
		{
			var dataSourceElement = dataSourceName != null && dataSourceKey != null ? new XElement("DataSourceCollection", new XElement("DataSource", new[] { new XElement("Key", dataSourceKey), new XElement("Type", dataSourceName) })).ToString() : "";
			var dataTargetElement = dataTargetName != null && dataTargetKey != null ? new XElement("DataTargetCollection", new XElement("DataTarget", new[] { new XElement("Key", dataTargetKey), new XElement("Type", dataTargetName) })).ToString() : "";
			var eventReferenceElement = new XElement("EventReference", eventReference);

			var xml =
$@"
<UniversalEvent>
	<Event>
		<DataContext>
			<DocumentaryOverride>
				<DocumentName>{documentName}</DocumentName>
			</DocumentaryOverride>" + dataSourceElement + dataTargetElement +
$@"
		</DataContext>
		<EventTime>2021-05-28T10:29:00</EventTime>
		<EventType>{eventType}</EventType>" + eventReferenceElement +
@"
	</Event>
</UniversalEvent>";

			return GetFormattedXMLText(xml);
		}

		#region Build Gate Event XML

		#region GIN Event XUE for GVM and GGM

		public string BuildGINEventXUEForGVMAndGGM(string gateBookingNumber, string movementBookingNumber, ZDateTimeOffset gateInTime, string direction, string eventReference = null)
		{
			gateInTime = gateInTime.AddSeconds(-gateInTime.Second);

			var eventReferenceElement = new XElement("EventReference", eventReference);

			var directionContext = direction != null ? new XElement("Context", new[] { new XElement("Type", "Direction"), new XElement("Value", direction) }).ToString() : "";
			var recipientRoleContext = new XElement("RecipientRoleCollection", new XElement("RecipientRole", new XElement("Code", "ATW"))).ToString();

			var xml =
$@"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataSourceCollection>
                <DataSource>
                    <Type>GateMovement</Type>
                    <Key>{movementBookingNumber}</Key>
                </DataSource>
            </DataSourceCollection>
            {recipientRoleContext}
        </DataContext>

        <ContextCollection>
            <Context>
                <Type>VBSNotificationID</Type>
                <Value>ASDF1234</Value>
            </Context>
            <Context>
                <Type>GateBookingNumber</Type>
                <Value>{gateBookingNumber}</Value>
            </Context>
            <Context>
                <Type>MovementBookingNumber</Type>
                <Value>{movementBookingNumber}</Value>
            </Context>
			{directionContext}
        </ContextCollection>

        <EventType>GIN</EventType>
        <EventTime>{gateInTime}</EventTime>
		{eventReferenceElement}
    </Event>
</UniversalEvent>";

			return GetFormattedXMLText(xml);
		}

		#endregion

		#region CNC Event for GVM

		public string BuildCNCEventForGVM(string movementBookingNumber, string direction, bool isCancelGateIn, string gateMovementNumber = "")
		{
			var eventReferenceElement = new XElement("EventReference", isCancelGateIn ? "|EVT=GIN" : "|EVT=GOU");
			var directionContext = direction != null ? new XElement("Context", new[] { new XElement("Type", "Direction"), new XElement("Value", direction) }).ToString() : "";
			var recipientRoleContext = new XElement("RecipientRoleCollection", new XElement("RecipientRole", new XElement("Code", "ATW"))).ToString();
			gateMovementNumber = string.IsNullOrEmpty(gateMovementNumber) ? movementBookingNumber : gateMovementNumber;

			var xml =
$@"<UniversalEvent>
    <Event>
        <DataContext>
            <DataSourceCollection>
                <DataSource>
                    <Type>GateMovement</Type>
                    <Key>{gateMovementNumber}</Key>
                </DataSource>
            </DataSourceCollection>
            {recipientRoleContext}
        </DataContext>

        <ContextCollection>
            <Context>
                <Type>VBSNotificationID</Type>
                <Value>ASDF1234</Value>
            </Context>
            <Context>
                <Type>GateBookingNumber</Type>
                <Value>{gateMovementNumber}</Value>
            </Context>
            <Context>
                <Type>MovementBookingNumber</Type>
                <Value>{movementBookingNumber}</Value>
            </Context>
            {directionContext}
        </ContextCollection>
 
        <EventType>CNC</EventType>
        <EventTime>2024-05-31T09:36:16.16+10:00</EventTime>
        {eventReferenceElement}
    </Event>
</UniversalEvent>";

			return GetFormattedXMLText(xml);
		}

		#endregion

		#region GOU Event XUE for GVM and GGM

		public string BuildGOUEventXUEForGVMAndGGM(string gateBookingNumber, string movementBookingNumber, ZDateTimeOffset gateOutTime, string direction, string eventReference = null)
		{
			gateOutTime = gateOutTime.AddSeconds(-gateOutTime.Second);

			var eventReferenceElement = new XElement("EventReference", eventReference);

			var directionContext = direction != null ? new XElement("Context", new[] { new XElement("Type", "Direction"), new XElement("Value", direction) }).ToString() : "";
			var recipientRoleContext = new XElement("RecipientRoleCollection", new XElement("RecipientRole", new XElement("Code", "ATW"))).ToString();

			var xml =
$@"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataSourceCollection>
                <DataSource>
                    <Type>GateMovement</Type>
                    <Key>{movementBookingNumber}</Key>
                </DataSource>
            </DataSourceCollection>
            {recipientRoleContext}
        </DataContext>

        <ContextCollection>
            <Context>
                <Type>VBSNotificationID</Type>
                <Value>ASDF1234</Value>
            </Context>
            <Context>
                <Type>GateBookingNumber</Type>
                <Value>{gateBookingNumber}</Value>
            </Context>
            <Context>
                <Type>MovementBookingNumber</Type>
                <Value>{movementBookingNumber}</Value>
            </Context>
			{directionContext}
        </ContextCollection>

        <EventType>GOU</EventType>
        <EventTime>{gateOutTime}</EventTime>
		{eventReferenceElement}
    </Event>
</UniversalEvent>";

			return GetFormattedXMLText(xml);
		}

		#endregion

		#region BKL Event

		public string BuildGateBKLEvent(string movementBookingNumber, string direction)
		{
			var recipientRoleContext = new XElement("RecipientRoleCollection", new XElement("RecipientRole", new XElement("Code", "ATW"))).ToString();
			var xml =
$@"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>GateMovementBooking</Type>
          <Key>{movementBookingNumber}</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>DAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Your Australia Corp</Name>
      </Company>
      <DataProvider>WUTB69DAU</DataProvider>
      <EnterpriseID>WUT</EnterpriseID>
      <EventBranch>
        <Code>A01</Code>
        <Name>AU - Branch 1</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>BKL</Code>
        <Description>Booking Cancelled</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise One Support</Name>
      </EventUser>
      <ServerID>B69</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDescription>Send XUE to TRW on BKL</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
      {recipientRoleContext}
    </DataContext>

    <EventTime>2024-11-14T10:58:10.940+11:00</EventTime>
    <EventType>BKL</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>Direction</Type>
        <Value>{direction}</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			return GetFormattedXMLText(xml);
		}

		#endregion

		#endregion

		#region Master Air Way Bill Event XML

		public string MasterAirWayBillEventXML(string eventType, string messageType, string eventReference) => $@"<UniversalEvent>
	<Event>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>WarehouseCustomsEntry</Type>
					<Key>3FR33159700500064-B228887</Key>
				</DataSource>
			</DataSourceCollection>
			<Company>
				<Code>DFR</Code>
				<Country>
					<Code>FR</Code>
					<Name>France</Name>
				</Country>
				<Name>FR - FRANCE</Name>
			</Company>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>TWH</Code>
					<Description>Transit Warehouse</Description>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<EventTime>2023-10-24T08:23:20.523</EventTime>
		<EventType>{eventType}</EventType>
		<EventReference>{eventReference}</EventReference>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>113-26363466</Value>
			</Context>
			<Context>
				<Type>MAWBOriginIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>MAWBDestinationIATAAirportCode</Type>
				<Value>LIO</Value>
			</Context>
			<Context>
				<Type>MBOLOriginUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>MBOLDestinationUNLOCO</Type>
				<Value>FRLIO</Value>
			</Context>
			<Context>
				<Type>AgentsReference</Type>
				<Value>PPPWS  71P</Value>
			</Context>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>FGDSDGFGFDS</Value>
			</Context>
			<Context>
				<Type>HAWBOriginIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>HAWBDestinationIATAAirportCode</Type>
				<Value>LIO</Value>
			</Context>
			<Context>
				<Type>HBOLOriginUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>HBOLDestinationUNLOCO</Type>
				<Value>FRLIO</Value>
			</Context>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>3FR33159700500064-B228887</Value>
			</Context>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>{messageType}</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>3FR33159700500064</Value>
			</Context>
			<Context>
				<Type>OuterPackQty</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>InnerPackQty</Type>
				<Value>50</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		#region Master Bill Of Lading Event XML

		public string MasterBillOfLadingEventXML(string eventType, string messageType) => $@"<UniversalEvent>
		<Event>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>WarehouseCustomsEntry</Type>
					<Key>3FR33159700500064-B228887</Key>
				</DataSource>
			</DataSourceCollection>
			<Company>
				<Code>DFR</Code>
				<Country>
					<Code>FR</Code>
					<Name>France</Name>
				</Country>
				<Name>FR - FRANCE</Name>
			</Company>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>TWH</Code>
					<Description>Transit Warehouse</Description>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<EventTime>2023-10-24T08:23:20.523</EventTime>
		<EventType>{eventType}</EventType>
		<EventReference>|CRF=SDFGHJHGHH|TYP=FDSJKFDKHJ|QTY=1|IPQ=30</EventReference>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>FJKHGFDSJHK</Value>
			</Context>
			<Context>
				<Type>MBOLOriginUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>MBOLDestinationUNLOCO</Type>
				<Value>FRLIO</Value>
			</Context>
			<Context>
				<Type>AgentsReference</Type>
				<Value>PPPWS  71P</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>FDSJKFDKHJ</Value>
			</Context>
			<Context>
				<Type>HBOLOriginUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>HBOLDestinationUNLOCO</Type>
				<Value>FRLIO</Value>
			</Context>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>3FR33159700500064-B228887</Value>
			</Context>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>{messageType}</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>3FR33159700500064-B228887</Value>
			</Context>
			<Context>
				<Type>OuterPackQty</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>InnerPackQty</Type>
				<Value>50</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		string GetFormattedXMLText(string inputXml)
		{
			XmlDocument document = new XmlDocument();
			document.Load(new StringReader(inputXml));

			StringBuilder builder = new StringBuilder();
			using (XmlTextWriter writer = new XmlTextWriter(new StringWriter(builder)))
			{
				writer.Formatting = Formatting.Indented;
				document.Save(writer);
			}

			return builder.ToString();
		}

		public void CreateExtraPort(
			WhsWarehouse warehouse,
			string extraPortCode
		)
		{
			var extraPort = factory.New<GlbBranchExtraPorts>();
			extraPort.GY_GB = warehouse.WW_GB_RelatedCompanyBranch;
			extraPort.GY_IsValid = true;
			extraPort.GY_RL_NKAdditionalBranchRelatedPort = extraPortCode;
		}

		public OrganizationAddress CreateOrganizationAddress(string orgCode, DocAddressType addressType)
		{
			return new OrganizationAddress { AddressType = addressType.ToString(), OrganizationCode = orgCode };
		}
	}
}
