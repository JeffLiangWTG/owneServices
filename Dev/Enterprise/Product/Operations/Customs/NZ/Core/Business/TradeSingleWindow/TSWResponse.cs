using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class BaseTSWResponse
	{
		public BaseTSWResponse(TSWMessage message)
		{
			Argument.NotNull(message, "message");
			IncomingTSWMessage = message;
			Factory = message.Factory;
			var messageText = message.EM_MessageText;

			using (var input = new StringReader(messageText))
			using (var reader = XmlReader.Create(input))
			{
				var root = FindElementByLocalName(XElement.Load(reader), ElementNames.DocumentMetadata)
					?? throw new XmlException($"Cannot find Element {ElementNames.DocumentMetadata}");
				response = FindElementByLocalName(root, ElementNames.Response);
				if (response == null)
				{
					throw new XmlException($"Cannot find Element {ElementNames.Response}");
				}
				commsMetaData = FindElementByLocalName(root, ElementNames.CommunicationMetaData);

				nSManager = new XmlNamespaceManager(reader.NameTable);
				nSManager.AddNamespace("d", root.Name.Namespace.NamespaceName);
				nSManager.AddNamespace("p", response.Name.Namespace.NamespaceName);
				if (commsMetaData != null)
				{
					nSManager.AddNamespace("c", commsMetaData.Name.Namespace.NamespaceName);
				}

				agencyDocumentName = GetElement("d:AgencyAssignedCustomizedDocumentName", root);
			}
		}
		public readonly TSWMessage IncomingTSWMessage;

		public BaseTSWResponse(BaseTSWResponse response)
		{
			Argument.NotNull(response, "response");
			Factory = response.Factory;
			agencyDocumentName = response.agencyDocumentName;
			commsMetaData = response.commsMetaData;
			this.response = response.response;
			nSManager = response.nSManager;
			IncomingTSWMessage = response.IncomingTSWMessage;
		}

		public static bool TryParse(TSWMessage message, out BaseTSWResponse response)
		{
			try
			{
				response = new BaseTSWResponse(message);
				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				response = null;
				return false;
			}
		}

		public BusinessObject LinkedObject
		{
			get { return OutgoingMessage != null ? OutgoingMessage.EM_LinkedObject : null; }
		}

		public TSWMessage OutgoingMessage
		{
			get
			{
				if (outgoingMessage == null)
				{
					try
					{
						var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.NewZealandCustoms);
						query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
						query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, SendersReference);
						query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " DESC";
						outgoingMessage = Factory.LoadTop1<TSWMessage>(query);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						outgoingMessage = null;
					}
				}

				return outgoingMessage;
			}
		}

		public string SendersReference
		{
			get { return sendersReference ?? (sendersReference = GetElementValue("p:OverallDeclaration/p:Declaration/p:FunctionalReferenceID")); }
		}

		public string RoleCode
		{
			get { return roleCode ?? (roleCode = GetElementValue("c:Recipient/c:RoleCode", commsMetaData)); }
		}

		public string ResponseType
		{
			get { return responseType ?? (responseType = GetAgencyElementValue(agencyDocumentName)); }
		}

		public string ResponsibleGovernmentAgency
		{
			get { return responsibleAgency ?? (responsibleAgency = GetElementValue("p:OverallDeclaration/p:Declaration/p:ResponsibleGovernmentAgency/p:ID")); }
		}

		public ZDateTime ResponseTime
		{
			get
			{
				if (responseDateTime == ZDateTime.Empty)
				{
					responseDateTime = GetElementDateValue("p:IssueDateTime");

					var createTimeUtc = IncomingTSWMessage.EM_SystemCreateTimeUtc;
					if (responseDateTime.IsEmpty && createTimeUtc.IsValid)
					{
						responseDateTime = EnvProxy.Instance.Time.GetLocalTimeFromUtc(createTimeUtc.ToDateTime());
					}
				}

				return responseDateTime;
			}
		}

		public string GoodsStatus
		{
			get { return goodsStatus ?? (goodsStatus = GetElementValue("p:OverallDeclaration/p:Declaration/p:Consignment/p:GoodsStatusCode")); }
		}

		public string GoodsClearanceStatus
		{
			get { return goodsClearanceStatus ?? (goodsClearanceStatus = GetElementAttributeValue("p:OverallDeclaration/p:Declaration/p:Consignment/p:GoodsStatusCode", "StatusType", "CLEARANCE")); }
		}

		public string GoodsMovementStatus
		{
			get { return goodsMovementStatus ?? (goodsMovementStatus = GetElementAttributeValue("p:OverallDeclaration/p:Declaration/p:Consignment/p:GoodsStatusCode", "StatusType", "MOVEMENT")); }
		}

		public string Status
		{
			get { return status ?? (status = GetElementValue("p:Status/p:NameCode")); }
		}

		public bool IsForSubmitter => RoleCode == RoleCodeList.Codes.TB;
		public bool IsForDepot => RoleCode == RoleCodeList.Codes.GC;
		public bool IsForTranshipmentDest => RoleCode == RoleCodeList.Codes.TT;
		public bool IsForNotificationParty => RoleCode == RoleCodeList.Codes.N2;

		public bool IsOCR => ResponseType == "RESOCR";
		public bool IsCRE => ResponseType == "RESCRE";
		public bool IsICR => ResponseType == "RESICR";
		public bool IsIM1 => ResponseType == "RESIM1";
		public bool IsEX1 => ResponseType == "RESEX1";

		public string Recipient
		{
			get
			{
				var messageIsFor = ZString.Empty;
				if (IsForSubmitter)
				{
					messageIsFor = "Submitter";
				}
				else if (IsForDepot)
				{
					messageIsFor = "Depot (Location of Goods)";
				}
				else if (IsForTranshipmentDest)
				{
					messageIsFor = "Transhipment Destination";
				}
				else if (IsForNotificationParty)
				{
					messageIsFor = "Delivery Notification Party";
				}

				return messageIsFor;
			}
		}

		#region Implementation

		public static class ElementNames
		{
			public const string Response = "Response";
			public const string DocumentMetadata = "DocumentMetadata";
			public const string CommunicationMetaData = "CommunicationMetaData";
		}

		readonly XmlNamespaceManager nSManager;
		readonly XElement response;
		readonly XElement commsMetaData;
		readonly XElement agencyDocumentName;
		protected readonly BusinessObjectFactory Factory;

		XElement FindElementByLocalName(XElement element, string localName)
		{
			if (element.Name.LocalName == localName)
			{
				return element;
			}

			foreach (var child in element.Elements())
			{
				var result = FindElementByLocalName(child, localName);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		#region Cached Fields

		TSWMessage outgoingMessage;
		string sendersReference;
		string responsibleAgency;
		ZDateTime responseDateTime;
		string goodsStatus;
		string status;
		string goodsClearanceStatus;
		string goodsMovementStatus;
		string roleCode;
		string responseType;

		#endregion Cached Fields

		#region Access to elements

		protected string GetAgencyElementValue(XElement root = null) => root?.Value ?? string.Empty;

		protected XElement GetElement(string path, XElement root = null) => (root ?? response)?.XPathSelectElement(path, nSManager);

		protected string GetElementValue(string path, XElement root = null) => GetElement(path, root)?.Value ?? string.Empty;

		protected string GetElementAttributeValue(string path, string attributeName, string attributeValue, XElement root = null)
		{
			return GetElementValue($"{path.TrimEnd('/')}[@{attributeName}='{attributeValue}']", root);
		}

		protected ZDateTime GetElementDateValue(string path, XElement root = null)
		{
			var element = GetElement(path, root);
			if (element != null)
			{
				ZString value = element.Value;
				ZDateTime responseTime;
				string dateFormatString = "yyyyMMddHHmmss";
				if (ZDateTime.TryParseExact(value, out responseTime, dateFormatString))
				{
					return responseTime;
				}

				dateFormatString = "yyyyMMdd";
				if (ZDateTime.TryParseExact(value, out responseTime, dateFormatString))
				{
					return responseTime;
				}
			}

			return ZDateTime.Empty;
		}

		protected IEnumerable<XElement> GetElements(string path, XElement root = null) => (root ?? response).XPathSelectElements(path, nSManager);

		protected IEnumerable<string> GetElementsValues(string path, XElement root = null) => GetElements(path, root).Select(e => e.Value);

		#endregion // Access to elements

		#endregion // Implementation
	}

	public abstract class TSWResponse : BaseTSWResponse
	{
		public class ValueWithPointer
		{
			public ValueWithPointer(string value, string pointer)
			{
				Value = value;
				Pointer = pointer;
			}

			public string Value
			{
				get;
				private set;
			}

			public string Pointer
			{
				get;
				private set;
			}
		}

		public IEnumerable<string> CustomsInstructions
		{
			get { return customsInstructions ?? (customsInstructions = GetInstructions("ICN").ToList()); }
		}

		public IEnumerable<string> AdditionalInstructions
		{
			get { return additionalInstructions ?? (additionalInstructions = GetInstructions("ICN", "p:AdditionalInformation").ToList()); }
		}

		internal IEnumerable<ZString> ConsignmentCustomsInstructions
		{
			get { return consignmentCustomsInstructions ?? (consignmentCustomsInstructions = GetConsignmentCustomsInstructions().ToList()); }
		}
		IEnumerable<ZString> consignmentCustomsInstructions;

		IEnumerable<ZString> GetConsignmentCustomsInstructions()
		{
			foreach (var consignment in GetElements("p:OverallDeclaration/p:Declaration/p:Consignment"))
			{
				var houseBill = IsOCR ? GetElementValue("p:AssociatedTransportDocument/p:ID", consignment) : GetElementValue("p:TransportContractDocument/p:ID", consignment);
				var instructions = string.Join(", ", GetElementsValues("p:AdditionalInformation/p:StatementDescription", consignment));
				yield return string.Join(ValueDelimiter, houseBill, instructions);
			}
		}

		public string DeclarationID
		{
			get
			{
				if (declarationID == null)
				{
					declarationID = GetElementValue("p:OverallDeclaration/p:Declaration/p:ID");
					if (declarationID == OutwardReportManifestStatus.InvalidReportNumber)
					{
						declarationID = "";
					}
				}

				return declarationID;
			}
		}

		public ZDateTime AcceptanceTime
		{
			get
			{
				if (acceptanceDateTime == ZDateTime.Empty)
				{
					acceptanceDateTime = GetElementDateValue("p:OverallDeclaration/p:Declaration/p:AcceptanceDateTime");
				}

				return acceptanceDateTime;
			}
		}

		public string DeclarationVersion
		{
			get { return declarationVersion ?? (declarationVersion = GetElementValue("p:OverallDeclaration/p:Declaration/p:VersionID")); }
		}

		public string EnterpriseStatus
		{
			get
			{
				if (statusInterpretation == null)
				{
					statusInterpretation = GetEnterpriseStatus();
				}
				return statusInterpretation;
			}
		}

		public string EnterpriseStatusDescription
		{
			get
			{
				if (statusInterpretationDescription == null)
				{
					statusInterpretationDescription = GetEnterpriseStatusDescription();
				}
				return statusInterpretationDescription;
			}
		}

		public IEnumerable<string> ErrorCodes
		{
			get { return errorCodes ?? (errorCodes = GetElementsValues("p:Error/p:ValidationCode").ToList()); }
		}

		public IEnumerable<ValueWithPointer> ErrorsWithPointers
		{
			get
			{
				return errorsWithPointers ??
						 (errorsWithPointers = (from error in GetElements("p:Error")
												let code = GetElement("p:ValidationCode[1]", error)
												where code != null
												select new ValueWithPointer(GetErrorDescription(code.Value), GetPointer(error))).ToList());
			}
		}

		public bool IsCancellation
		{
			get
			{
				return
					OutgoingMessage != null &&
					OutgoingMessage.EM_MessageSubType == NZ.TradeSingleWindow.MessageSubTypeList.Codes.Cancellation &&
					Status == StatusList.Codes.EntryCancelled;
			}
		}

		public string JobID
		{
			get { return GetJobID(); }
		}

		public string JobName
		{
			get { return GetJobName(); }
		}

		public virtual string MessageParentID => JobID;

		public virtual string MessageParentTypeName => JobName;

		public bool EntryBeingProcessedIsImportEntry
		{
			get
			{
				if (!entryBeingProcessedIsImportEntry.HasValue)
				{
					entryBeingProcessedIsImportEntry = GetIsImportEntry();
				}
				return entryBeingProcessedIsImportEntry.Value;
			}
		}
		bool? entryBeingProcessedIsImportEntry;

		public string MessageNumber
		{
			get { return messageNumber ?? (messageNumber = GetElementValue("p:FunctionalReferenceID")); }
		}

		public string MessageType
		{
			get { return messageType ?? (messageType = GetElementValue("p:FunctionCode")); }
		}

		public string MessageTypeDescription
		{
			get { return messageTypeDescription ?? (messageTypeDescription = GetTransactionTypeDescription(MessageType)); }
		}

		public string StatusDescription
		{
			get
			{
				return statusDescription ?? (statusDescription = GetStatusDescription(Status));
			}
		}

		public virtual void SetRelevantEntryStatus()
		{
		}

		string GetBillNumber(string path, params string[] billTypes)
		{
			return GetElementValue($"{path.TrimEnd('/')}[{string.Join(" or ", billTypes.Select(x => $"p:TypeCode='{x}'"))}]/p:ID");
		}

		internal string BillNumber => billNumber ?? (billNumber = GetBillNumber("p:OverallDeclaration/p:Declaration/p:Consignment/p:AssociatedTransportDocument", BillTypeList.Codes.MB));
		string billNumber;

		internal string MasterBillNumber => masterBillNumber ?? (masterBillNumber = GetBillNumber("p:OverallDeclaration/p:Declaration/p:GoodsShipment/p:Consignment/p:TransportContractDocument", BillTypeList.Codes.MB));
		string masterBillNumber;

		internal string HouseBill => houseBill ?? (houseBill = GetBillNumber("p:OverallDeclaration/p:Declaration/p:GoodsShipment/p:Consignment/p:TransportContractDocument", new string[] { BillTypeList.Codes.BM, BillTypeList.Codes.HWB }));
		string houseBill;

		internal string SubmitterName => submitterName ?? (submitterName = GetElementValue("p:OverallDeclaration/p:Declaration/p:Submitter/p:Name"));
		string submitterName;

		internal string AgentName => agentName ?? (agentName = GetElementValue("p:OverallDeclaration/p:Declaration/p:Agent/p:Name"));
		string agentName;

		internal string ConsigneeName => consigneeName ?? (consigneeName = GetElementValue("p:OverallDeclaration/p:Declaration/p:Consignment/p:Consignee/p:Name"));
		string consigneeName;

		internal string ConsignorName => consignorName ?? (consignorName = GetElementValue("p:OverallDeclaration/p:Declaration/p:Consignment/p:Consignor/p:Name"));
		string consignorName;

		internal string GoodsLocation => goodsLocation ?? (goodsLocation = GetElementValue("p:OverallDeclaration/p:Declaration/p:Consignment/p:GoodsLocation/p:Name"));
		string goodsLocation;

		internal string Carrier => carrier ?? (carrier = GetElementValue("p:OverallDeclaration/p:Declaration/p:Carrier/p:Name"));
		string carrier;

		internal string TransportType => transportType ?? (transportType = GetElementValue("p:OverallDeclaration/p:Declaration/p:BorderTransportMeans/p:TypeCode"));
		string transportType;

		internal string Vessel => vessel ?? (vessel = GetElementValue("p:OverallDeclaration/p:Declaration/p:BorderTransportMeans/p:Name"));
		string vessel;

		internal string IMONo => fIMONo ?? (fIMONo = GetElementValue("p:OverallDeclaration/p:Declaration/p:BorderTransportMeans/p:ID"));
		string fIMONo;

		internal string Voyage => voyage ?? (voyage = GetElementValue("p:OverallDeclaration/p:Declaration/p:BorderTransportMeans/p:JourneyID"));
		string voyage;

		internal string FlightNo => flightNo ?? (flightNo = GetElementValue("p:OverallDeclaration/p:Declaration/p:BorderTransportMeans/p:Name"));
		string flightNo;

		internal ZDateTime ArrivalDate => (arrivalDate ?? (arrivalDate = GetElementDateValue("p:OverallDeclaration/p:Declaration/p:BorderTransportMeans/p:ArrivalDateTime"))).Value;
		ZDateTime? arrivalDate;

		internal string PortOfArrival => portOfArrival ?? (portOfArrival = GetElementValue("p:OverallDeclaration/p:Declaration/p:BorderTransportMeans/p:FirstArrivalLocationID"));
		string portOfArrival;

		internal string ImporterName => importerName ?? (importerName = GetElementValue("p:OverallDeclaration/p:Declaration/p:Importer/p:Name"));
		string importerName;

		internal string ExporterName => exporterName ?? (exporterName = GetElementValue("p:OverallDeclaration/p:Declaration/p:Exporter/p:Name"));
		string exporterName;

		internal string PortOfDischarge => portOfDischarge ?? (portOfDischarge = GetElementValue("p:OverallDeclaration/p:Declaration/p:UnloadingLocation/p:ID"));
		string portOfDischarge;

		internal string PortOfLoading => portOfLoading ?? (portOfLoading = GetElementValue("p:OverallDeclaration/p:Declaration/p:LoadingLocation/p:ID"));
		string portOfLoading;

		internal string PortOfDeparture => portOfDeparture ?? (portOfDeparture = GetElementValue("p:OverallDeclaration/p:Declaration/p:Consignment/p:LoadingLocation/p:ID"));
		string portOfDeparture;

		internal ZDateTime DepartureDate => (departureDate ?? (departureDate = GetElementDateValue("p:OverallDeclaration/p:Declaration/p:BorderTransportMeans/p:DepartureDateTime"))).Value;
		ZDateTime? departureDate;

		internal string ExitOffice => exitOffice ?? (exitOffice = GetElementValue("p:OverallDeclaration/p:Declaration/p:ExitOffice/p:ID"));
		string exitOffice;

		void EnsureContainerDetailsLoaded()
		{
			if (containers == null || containerDetails == null)
			{
				containers = new List<ZString>();
				containerDetails = new List<ZString>();
				consignmentContainerDetails = new List<ZString>();

				foreach (var transportEquipment in GetElements("p:OverallDeclaration/p:Declaration/p:GoodsShipment/p:Consignment/p:TransportEquipment"))
				{
					var containerID = GetElementValue("p:ID", transportEquipment);
					var fullnessCode = string.IsNullOrEmpty(containerID) ? string.Empty : GetElementValue("p:FullnessCode", transportEquipment);
					var packingSequence = string.IsNullOrEmpty(containerID) ? string.Empty : GetElementValue("p:Pointer/p:SequenceNumeric", transportEquipment);
					var sealNumbers = string.IsNullOrEmpty(containerID) ? string.Empty : string.Join(", ", GetElementsValues("p:Seal/p:ID", transportEquipment));

					containers.Add(containerID);
					containerDetails.Add(string.Join(ValueDelimiter, containerID, fullnessCode, packingSequence, sealNumbers));
				}
			}
		}

		internal IEnumerable<ZString> Containers
		{
			get
			{
				EnsureContainerDetailsLoaded();
				return containers;
			}
		}
		List<ZString> containers;

		internal IEnumerable<ZString> ContainerDetails
		{
			get
			{
				EnsureContainerDetailsLoaded();
				return containerDetails;
			}
		}
		List<ZString> containerDetails;

		internal IEnumerable<ZString> PackagingDetails => packagingDetails ?? (packagingDetails = GetPackagingDetails().ToList());
		IEnumerable<ZString> packagingDetails;

		IEnumerable<ZString> GetPackagingDetails()
		{
			foreach (var packaging in GetElements("p:OverallDeclaration/p:Declaration/p:Packaging"))
			{
				var sequence = GetElementValue("p:SequenceNumeric", packaging);
				var packagingQty = GetElementValue("p:QuantityQuantity", packaging);
				var packagingType = GetElementValue("p:TypeCode", packaging);

				yield return sequence + ValueDelimiter + packagingQty + ValueDelimiter + packagingType;
			}
		}

		internal IEnumerable<ZString> ConsignmentsToNotify => consignmentsToNotify ?? (consignmentsToNotify = GetConsignmentsToNotify().ToList());
		IEnumerable<ZString> consignmentsToNotify;

		IEnumerable<ZString> GetConsignmentsToNotify()
		{
			foreach (var consignment in GetElements("p:OverallDeclaration/p:Declaration/p:Consignment"))
			{
				var masterBill = IsOCR ? GetElementValue("p:TransportContractDocument/p:ID", consignment) : GetElementValue("p:AssociatedTransportDocument/p:ID", consignment);
				var houseBill = IsOCR ? GetElementValue("p:AssociatedTransportDocument/p:ID", consignment) : GetElementValue("p:TransportContractDocument/p:ID", consignment);
				var consignee = GetElementValue("p:Consignee/p:Name", consignment);
				var consignor = GetElementValue("p:Consignor/p:Name", consignment);
				ZString clearanceStatus = GetElementAttributeValue("p:GoodsStatusCode", "StatusType", "CLEARANCE", consignment);
				if (clearanceStatus.IsEmpty)
				{
					clearanceStatus = GetElementValue("p:GoodsStatusCode", consignment);
				}

				var movementStatus = GetElementAttributeValue("p:GoodsStatusCode", "StatusType", "MOVEMENT", consignment);
				var containers = string.Join(", ", GetElementsValues("p:TransportEquipment/p:ID", consignment));
				var edo = GetElementValue("p:AdditionalDocument/p:ID", consignment);

				yield return string.Join(ValueDelimiter, masterBill, houseBill, consignee, consignor, clearanceStatus, movementStatus, containers, edo);
			}
		}

		internal IEnumerable<ZString> ConsignmentContainerDetails => consignmentContainerDetails ?? (consignmentContainerDetails = GetConsignmentContainerDetails().ToList());
		IEnumerable<ZString> consignmentContainerDetails;

		IEnumerable<ZString> GetConsignmentContainerDetails()
		{
			foreach (var transportEquipment in GetElements("p:OverallDeclaration/p:Declaration/p:Consignment/p:TransportEquipment"))
			{
				var containerID = GetElementValue("p:ID", transportEquipment);
				var fullnessCode = string.IsNullOrEmpty(containerID) ? string.Empty : GetElementValue("p:FullnessCode", transportEquipment);

				yield return string.Join(ValueDelimiter, containerID, fullnessCode);
			}
		}

		internal IEnumerable<string> DeliverInstructions
		{
			get { return deliverInstructions ?? (deliverInstructions = GetInstructions("DIN").ToList()); }
		}

		List<string> deliverInstructions;

		#region Implementation

		protected TSWResponse(BaseTSWResponse response)
			: base(response)
		{
		}

		protected abstract string GetJobID();

		protected abstract string GetJobName();

		protected abstract bool GetIsImportEntry();

		protected abstract string GetEnterpriseStatus();

		protected abstract string GetEnterpriseStatusDescription();

		protected virtual string InstructionsPath => "p:AdditionalInformation";

		protected IEnumerable<string> GetInstructions(string type, string path = "")
		{
			var instructionsPath = string.IsNullOrEmpty(path) ? InstructionsPath : path;
			return GetElementsValues($"{instructionsPath.TrimEnd('/')}[p:StatementTypeCode='{type}']/p:StatementDescription").Where(x => !string.IsNullOrEmpty(x));
		}

		#region Pointers

		protected string GetPointer(XElement element)
		{
			var pointerDetails = from pointer in GetElements("p:Pointer", element)
								 let section = GetElementValue("p:DocumentSectionCode", pointer)
								 where !string.IsNullOrEmpty(section)
								 let sequence = GetElementValue("p:SequenceNumeric", pointer)
								 let tag = GetElementValue("p:TagID", pointer)
								 select string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", WCOIDToTagName(section),
										string.IsNullOrEmpty(sequence) ? string.Empty : $"[{sequence}]",
										string.IsNullOrEmpty(tag) ? string.Empty : $"/{WCOIDToTagName(tag)}");

			return string.Join("/", pointerDetails);
		}

		string WCOIDToTagName(string id) => WCOIDList.TryGetValue(id, out var result) ? result : "Unknown";

		#endregion // Pointers

		#region Code-Description Lists

		ErrorList ErrorList
		{
			get { return Factory.GetCachedValue<ErrorList>(); }
		}

		StatusList StatusList
		{
			get { return Factory.GetCachedValue<StatusList>(); }
		}

		TransactionTypeList TransactionTypeList
		{
			get { return Factory.GetCachedValue<TransactionTypeList>(); }
		}

		protected Dictionary<string, string> WCOIDList
		{
			get
			{
				return Factory.GetCachedValue("TSWResponse.WCOIDList", () =>
				{
					return new Dictionary<string, string>()
					{
						{ "006",  "SequenceNumeric" },
						{ "016",  "ID" },
						{ "017",  "FunctionCode" },
						{ "01B",  "Producer" },
						{ "024",  "ExitDateTime" },
						{ "02A",  "AdditionalDocument" },
						{ "03A",  "AdditionalInformation" },
						{ "04A",  "Address" },
						{ "05A",  "Agent" },
						{ "062",  "ExportationCountryCode" },
						{ "063",  "CountryCode" },
						{ "064",  "RoutingCountryCode" },
						{ "066",  "RegionID" },
						{ "06A",  "Amendment" },
						{ "085",  "FirstArrivalLocationID" },
						{ "08A",  "ApprovedEstablishmentPlace" },
						{ "08B",  "ResponsibleGovernmentAgency" },
						{ "090",  "ConditionCode" },
						{ "095",  "RequestOverrideCode" },
						{ "098",  "PaymentMethodCode" },
						{ "09B",  "Seller" },
						{ "103",  "TransactionNatureCode" },
						{ "105",  "Content" },
						{ "107",  "MethodCode" },
						{ "108",  "CustomsValueAmount" },
						{ "110",  "ValueAmount" },
						{ "113",  "TypeCode" },
						{ "117",  "FreightChargeAmount" },
						{ "118",  "RateNumeric" },
						{ "11A",  "AssociatedTransportDocument" },
						{ "11B",  "Source" },
						{ "120",  "TaxAssessedAmount" },
						{ "124",  "FreightChargeApportionmentCode" },
						{ "125",  "AmountAmount" },
						{ "126",  "GrossMassMeasure" },
						{ "128",  "NetNetWeightMeasure" },
						{ "130",  "TariffQuantity" },
						{ "131",  "TotalGrossMassMeasure" },
						{ "135",  "CurrencyTypeCode" },
						{ "137",  "Description" },
						{ "138",  "CargoDescription" },
						{ "141",  "TypeCode" },
						{ "142",  "MarksNumbersID" },
						{ "144",  "QuantityQuantity" },
						{ "145",  "ID" },
						{ "147",  "ID" },
						{ "149",  "JourneyID" },
						{ "152",  "CharacteristicCode" },
						{ "154",  "FullnessCode" },
						{ "156",  "DepartureDateTime" },
						{ "159",  "ID" },
						{ "15A",  "BorderTransportMeans" },
						{ "15B",  "StowPosition" },
						{ "164",  "DutyRegimeCode" },
						{ "165",  "ID" },
						{ "16B",  "StuffingEstablishment" },
						{ "172",  "ArrivalDateTime" },
						{ "17B",  "Submitter" },
						{ "188",  "AdditionCode" },
						{ "18A",  "Carrier" },
						{ "18B",  "Supplier" },
						{ "20B",  "Temperature" },
						{ "21A",  "Classification" },
						{ "225",  "StatementDescription" },
						{ "226",  "StatementCode" },
						{ "229",  "Characteristic" },
						{ "239",  "Line" },
						{ "23A",  "Commodity" },
						{ "23B",  "TranshipmentLocation" },
						{ "240",  "ID" },
						{ "241",  "CityName" },
						{ "242",  "CountryCode" },
						{ "243",  "CountrySubDivisionName" },
						{ "245",  "PostcodeID" },
						{ "24A",  "CommodityRelatedPackaging" },
						{ "253",  "TypeID" },
						{ "257",  "CommercialCategorizationID" },
						{ "258",  "Name" },
						{ "25A",  "Communication" },
						{ "260",  "CharacteristicTypeCode" },
						{ "265",  "LotNumberID" },
						{ "266",  "ProductBestBeforeDateTime" },
						{ "269",  "ProcessDurationDateTime" },
						{ "26B",  "TransitDestination" },
						{ "274",  "JurisdictionDateTime" },
						{ "27A",  "Consignee" },
						{ "28A",  "Consignment" },
						{ "29A",  "ConsignmentItem" },
						{ "306",  "ValueAmount" },
						{ "30A",  "Consignor" },
						{ "30B",  "TransportContractDocument" },
						{ "317",  "ElementQuantity" },
						{ "31A",  "Consolidator" },
						{ "31B",  "TransportEquipment" },
						{ "320",  "ActualTemperatureMeasure" },
						{ "321",  "StorageRequirementMeasure" },
						{ "327",  "IntendedUse" },
						{ "330",  "IntendedUseCode" },
						{ "333",  "HierarchyCode" },
						{ "334",  "CharacteristicQualifierCode" },
						{ "335",  "NameQualifierCode" },
						{ "337",  "IdentificationTypeCode" },
						{ "338",  "IdentifierTypeCode" },
						{ "339",  "ProcessTypeDescription" },
						{ "33A",  "Constituent" },
						{ "340",  "ProcessTypeIdentificationCode" },
						{ "343",  "PackingMethodDescription" },
						{ "344",  "PackingMaterialDescription" },
						{ "346",  "NameCode" },
						{ "347",  "IdentityQualifierCode" },
						{ "348",  "ElementName" },
						{ "34A",  "Contact" },
						{ "354",  "ProductExpiryDateTime" },
						{ "355",  "InspectionStartDateTime" },
						{ "356",  "InspectionEndDateTime" },
						{ "35B",  "UCR" },
						{ "366",  "LevelIndicator" },
						{ "369",  "StatementTypeCode" },
						{ "36B",  "UltimateConsignee" },
						{ "375",  "DocumentSectionCode" },
						{ "381",  "TagID" },
						{ "387",  "AttachedCode" },
						{ "388",  "FlashpointMeasure" },
						{ "38B",  "UnloadingLocation" },
						{ "395",  "VolumeMeasure" },
						{ "396",  "CountryCode" },
						{ "39B",  "ValuationAdjustment" },
						{ "403",  "MinimumStorageRequirementMeasure" },
						{ "404",  "MaximumStorageRequirementMeasure" },
						{ "40A",  "CurrencyExchange" },
						{ "411",  "ExpectedArrivalDateDateTime" },
						{ "41A",  "CustomsValuation" },
						{ "42A",  "Declaration" },
						{ "43A",  "Deconsolidator" },
						{ "44A",  "DeliveryDestination" },
						{ "44B",  "Seal" },
						{ "46B",  "Product" },
						{ "47B",  "ProductName" },
						{ "48B",  "ProductCharacteristics" },
						{ "50A",  "DutyTaxFee" },
						{ "54A",  "ExaminationPlace" },
						{ "56A",  "ExitOffice" },
						{ "57A",  "Exporter" },
						{ "57B",  "Declarant" },
						{ "60B",  "CommodityProcess" },
						{ "62A",  "Freight" },
						{ "63A",  "GoodsConsignedPlace" },
						{ "64A",  "GoodsLocation" },
						{ "65A",  "GoodsMeasure" },
						{ "67A",  "GoodsShipment" },
						{ "68A",  "GovernmentAgencyGoodsItem" },
						{ "70B",  "Processor" },
						{ "72A",  "Grower" },
						{ "74A",  "Importer" },
						{ "78A",  "Invoice" },
						{ "81A",  "Itinerary" },
						{ "83A",  "LoadingLocation" },
						{ "86A",  "Manufacturer" },
						{ "89A",  "NotifyParty" },
						{ "92A",  "Origin" },
						{ "93A",  "Packaging" },
						{ "94A",  "Payment" },
						{ "97A",  "Pointer" },
						{ "99A",  "PreviousDocument" },
						{ "D005", "ID" },
						{ "D006", "TypeCode" },
						{ "D008", "ID" },
						{ "D009", "TypeCode" },
						{ "D013", "TypeCode" },
						{ "D014", "ID" },
						{ "D015", "IssueDateTime" },
						{ "D016", "ID" },
						{ "D018", "ID" },
						{ "D019", "TypeCode" },
						{ "D023", "ID" },
						{ "D024", "TypeCode" },
						{ "D026", "FunctionalReferenceID" },
						{ "D030", "ImageBinaryObject" },
						{ "D031", "CategoryCode" },
						{ "G005", "ID" },
						{ "G007", "ID" },
						{ "L008", "ID" },
						{ "L010", "ID" },
						{ "L013", "ID" },
						{ "L015", "ID" },
						{ "L017", "ID" },
						{ "L024", "ID" },
						{ "L029", "ID" },
						{ "L041", "ID" },
						{ "L058", "ID" },
						{ "L069", "Name" },
						{ "R003", "Name" },
						{ "R004", "ID" },
						{ "R005", "RoleCode" },
						{ "R011", "Name" },
						{ "R012", "ID" },
						{ "R014", "Name" },
						{ "R015", "ID" },
						{ "R018", "Name" },
						{ "R019", "ID" },
						{ "R020", "Name" },
						{ "R021", "ID" },
						{ "R022", "Name" },
						{ "R023", "ID" },
						{ "R024", "Name" },
						{ "R027", "Name" },
						{ "R032", "ID" },
						{ "R034", "Name" },
						{ "R037", "Name" },
						{ "R038", "ID" },
						{ "R042", "Name" },
						{ "R045", "Name" },
						{ "R046", "ID" },
						{ "R050", "Name" },
						{ "R052", "Name" },
						{ "R053", "ID" },
						{ "R059", "ID" },
						{ "R060", "Name" },
						{ "R083", "Name" },
						{ "R123", "ID" },
						{ "R134", "Name" },
						{ "T005", "Name" },
						{ "T006", "ID" },
						{ "T010", "TypeCode" }
					};
				});
			}
		}

		protected string GetErrorDescription(string code)
		{
			return ErrorList.GetDescriptionFromCode(code) ?? "Fatal error in TSW Gateway – contact CargoWise";
		}

		protected string GetTransactionTypeDescription(string code)
		{
			return TransactionTypeList.GetDescriptionFromCode(code) ?? "Unknown";
		}

		protected virtual string GetStatusDescription(string code)
		{
			return StatusList.GetDescriptionFromCode(code) ?? "Unknown";
		}

		#endregion // Code-Description Lists

		#region Cached Fields

		ZDateTime acceptanceDateTime;
		List<string> customsInstructions;
		List<string> additionalInstructions;
		string declarationID;
		string declarationVersion;
		List<string> errorCodes;
		List<ValueWithPointer> errorsWithPointers;
		string messageNumber;
		string messageType;
		string messageTypeDescription;
		string statusDescription;
		string statusInterpretation;
		string statusInterpretationDescription;

		#endregion // Cached Fields

		const string ValueDelimiter = "|";

		#endregion // Implementation
	}
}
