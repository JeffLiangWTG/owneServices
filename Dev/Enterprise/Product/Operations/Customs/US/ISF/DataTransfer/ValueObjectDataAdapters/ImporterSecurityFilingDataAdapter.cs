using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	public class ImporterSecurityFilingDataAdapter : ValueObjectDataAdapter<CusISFHeader, Xsd.ISF>
	{
		public ImporterSecurityFilingDataAdapter()
			: this(EventsWithSourceType.Empty)
		{
		}

		public ImporterSecurityFilingDataAdapter(EventsWithSourceType triggeredByEvents)
		{
			this.TriggeredByEvents = triggeredByEvents;
		}
		protected readonly EventsWithSourceType TriggeredByEvents;

		public override string RootCollectionElementName
		{
			get { return "ISFs"; }
		}

		public override string RootElementName
		{
			get { return "ISF"; }
		}

		public override XmlSchema Schema
		{
			get { return XmlSchemaDefinitions.Instance.SingleImporterSecurityFilingSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return XmlSchemaDefinitions.Instance.ImporterSecurityFilingsSchema; }
		}

		public AttachmentDef ExportAsAXmlAttachment(CusISFHeader bizObj, ZString attachmentFileName, IValueObjectExportContext context)
		{
			Xsd.XmlInterchange interchange = Xsd.XmlInterchange.NewPopulatedInterchange(bizObj.Factory);
			interchange.IsSpecified = true;
			interchange.Payload.Data = new BusinessObject[] { bizObj };
			interchange.Payload.DataAdapter = this;
			interchange.Payload.Context = context;

			MemoryStream result = new MemoryStream();
			XmlValueObjectSerializer xmlSerialiser = new XmlValueObjectSerializer(typeof(Xsd.XmlInterchange));
			xmlSerialiser.Serialize(result, interchange);
			AttachmentDef attachment = new AttachmentDef(attachmentFileName, result.ToArray());
			return attachment;
		}

		protected override Type ValueObjectCollectionType
		{
			get { return typeof(Xsd.ISFCollection); }
		}

		#region Export

		protected override void ExportToValueObjectCore(CusISFHeader bizObj, Xsd.ISF constructedValueObject, IValueObjectExportContext context)
		{
			constructedValueObject.Action.IsSpecified = false;
			constructedValueObject.BondHolder = bizObj.BF_BondNumberOrHolder;
			constructedValueObject.OwnerReference = bizObj.BF_OwnerReference;
			if (!bizObj.BF_ActionReasonCode.IsEmpty)
			{
				constructedValueObject.Action.IsSpecified = true;
				constructedValueObject.Action.ReasonCode = ActionReasonCodeToXmlCodeMappings.Instance.GetExternalCode(bizObj.BF_ActionReasonCode, "ISF Action Reason Code", context);
			}
			if (!bizObj.BF_BondActivityCode.IsEmpty)
			{
				constructedValueObject.BondActivityCode = BondActivityCodeToXmlCodeMappings.Instance.GetExternalCode(bizObj.BF_BondActivityCode, "ISF Bond Activity Code", context);
			}
			if (!bizObj.BF_BondType.IsEmpty)
			{
				constructedValueObject.BondType = BondTypeToXmlCodeMappings.Instance.GetExternalCode(bizObj.BF_BondType, "ISF Bond Type", context);
				constructedValueObject.BondTypeSpecified = true;
			}
			constructedValueObject.CarrierSCAC = bizObj.BF_SCAC;
			constructedValueObject.PlaceOfDelivery = Xsd.UNLOCO.FromPort(bizObj.PlaceOfDelivery);
			constructedValueObject.ForeignPortOfUnlading = Xsd.UNLOCO.FromPort(bizObj.PortOfUnload);
			if (!bizObj.BF_ShipmentType.IsEmpty)
			{
				constructedValueObject.ShipmentType = ShipmentTypeToXmlCodeMappings.Instance.GetExternalCode(bizObj.BF_ShipmentType, "Shipment Type", context);
				if (constructedValueObject.ShipmentType == Xsd.ISFShipmentType.Item11)
				{
					if (!bizObj.BF_ShipmentSubType.IsEmpty)
					{
						constructedValueObject.ShipmentSubType = ShipmentSubTypeToXmlCodeMappings.Instance.GetExternalCode(bizObj.BF_ShipmentSubType, "Shipment Sub Type", context);
					}
					constructedValueObject.EstimatedValue = bizObj.BF_EstimatedValue;
					constructedValueObject.EstimatedQuantity = Xsd.DimensionValue.FromAmountAndUnit(bizObj.BF_EstimatedQuantity, bizObj.BF_EstimatedQuantityUQ);
					constructedValueObject.EstimatedWeight = Xsd.DimensionValue.FromAmountAndUnit(bizObj.BF_EstimatedWeight, bizObj.BF_EstimatedWeightUQ);
				}
			}
			if (!bizObj.BF_EntryType.IsEmpty)
			{
				constructedValueObject.SubmissionType = EntryTypeToXmlCodeMappings.Instance.GetExternalCode(bizObj.BF_EntryType, "Entry Type", context);
			}
			if (!bizObj.BF_TransportMode.IsEmpty)
			{
				constructedValueObject.TransportMode = TransportModeToXmlCodeMappings.Instance.GetExternalCode(bizObj.BF_TransportMode, "Transport Mode", context);
			}
			ExportImporterOfRecords(bizObj, constructedValueObject, context);
			ExportBills(bizObj, constructedValueObject, context);
			ExportContainers(bizObj, constructedValueObject, context);
			Dictionary<ZGuid, ZString> manufacturerList = new Dictionary<ZGuid, ZString>();
			ExportParties(bizObj, constructedValueObject, context, manufacturerList);
			ExportRoutings(bizObj, constructedValueObject, context);
			ExportLines(bizObj, constructedValueObject, context, manufacturerList);
			constructedValueObject.TransactionNumber = bizObj.BF_CustomsReference;
			constructedValueObject.JobReference = bizObj.BF_JobReference;
			ExportStatus(bizObj, constructedValueObject, context);
			constructedValueObject.Workflow.IsSpecified = false;
			constructedValueObject.Events = StmALogValueObjectDataAdapter.New(bizObj, bizObj.BF_JobReference, TriggeredByEvents).ToXmlCollectionValueObject(context);
			AddExportEvent(constructedValueObject, bizObj, context);
		}

		protected override bool ShouldExportCustomValue(IPropertyValue dynamicPropertyValue, BusinessObject bizObj)
		{
			var type = bizObj.GetType();
			if (typeof(CusISFLine).IsAssignableFrom(type))
			{
				return dynamicPropertyValue.PropertyName == CusISFLine.Schema.CustomAttribute1 || dynamicPropertyValue.PropertyName == CusISFLine.Schema.CustomAttribute2;
			}
			else if (typeof(CusISFHeader).IsAssignableFrom(type))
			{
				return dynamicPropertyValue.PropertyName == CusISFHeader.Schema.CustomAttribute1 || dynamicPropertyValue.PropertyName == CusISFHeader.Schema.CustomAttribute2;
			}
			else
			{
				return false;
			}
		}

		void ExportRoutings(CusISFHeader bizObj, Xsd.ISF constructedValueObject, IValueObjectExportContext context)
		{
			Xsd.PlannedLegCollection routings = constructedValueObject.Routings;
			routings.IsSpecified = false;
			foreach (Transport transport in bizObj.Transports)
			{
				Xsd.PlannedLeg plannedLeg = routings.AddNew();
				XsdPlannedLegObjectHelper.ExportPlannedLeg(plannedLeg, transport, context, "Routings");
				routings.IsSpecified = true;
			}
		}

		void ExportStatus(CusISFHeader bizObj, Xsd.ISF constructedValueObject, IValueObjectExportContext context)
		{
			Xsd.ISFStatus xmlStatus = constructedValueObject.Status;
			xmlStatus.IsSpecified = true;
			xmlStatus.MessageStatus.Value = bizObj.BF_CustomsStatus;
			xmlStatus.MessageStatus.Description = bizObj.BF_CustomsStatusDescription;
			xmlStatus.FirstAcceptedDate = bizObj.BF_FirstAcceptedDate.Date;
			xmlStatus.LastAcceptedDate = bizObj.BF_LastAcceptedDate.Date;
			ExportCustomsResponse(bizObj, xmlStatus, context);
			ExportValidationResponse(bizObj, xmlStatus);
		}

		void ExportValidationResponse(CusISFHeader bizObj, Xsd.ISFStatus xmlStatus)
		{
			bizObj.Validation.ValidateAll();
			ZNotificationCollector collector = new ZNotificationCollector(bizObj, true, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
			List<string> list = new List<string>(collector.GetUniqueMessageList());
			list.Sort();
			StringCollectionX collection = new StringCollectionX(list.ToArray());
			string errors = collection.ToString();
			if (!string.IsNullOrEmpty(errors))
			{
				xmlStatus.ValidationResponse = errors;
			}
		}

		void ExportCustomsResponse(CusISFHeader bizObj, Xsd.ISFStatus xmlStatus, IValueObjectExportContext context)
		{
			Xsd.ISFStatusCustomsResponse xmlCustomsResponse = xmlStatus.CustomsResponse;
			xmlCustomsResponse.IsSpecified = false;
			LastestCustomsResponseParser responseParser = new LastestCustomsResponseParser(bizObj);
			US.Business.MQEDIMessage message = responseParser.LastestResponse;
			if (message != null)
			{
				xmlCustomsResponse.IsSpecified = true;
				ZString messageTypeCode = responseParser.LatestMessageTypeCode;
				if (!messageTypeCode.IsEmpty)
				{
					xmlCustomsResponse.MessageTypeCode = MessageTypeCodeToXmlCodeMappings.Instance.GetExternalCode(messageTypeCode, "Message Type Code", context);
					xmlCustomsResponse.MessageTypeCodeSpecified = true;
				}
				xmlCustomsResponse.RawFormat = message.EM_MessageText;
				ExportMessageInterpretation(message, xmlCustomsResponse, context);
			}

			Xsd.ISFStatusCustomsResponseBillCollection xmlBills = xmlCustomsResponse.Bills;
			xmlBills.IsSpecified = false;
			foreach (CusISFBill bill in bizObj.ReferenceDatas)
			{
				if (!bill.BB_CustomsStatus.IsEmpty)
				{
					xmlBills.IsSpecified = true;
					Xsd.ISFStatusCustomsResponseBill xmlBill = xmlBills.AddNew();
					xmlBill.Number = bill.BB_BillNum;
					xmlBill.DispositionCode.Code = DispositionCodeToXmlCodeMappings.Instance.GetExternalCode(bill.BB_CustomsStatus, "Bill Status", context);
					xmlBill.DispositionCode.CodeSpecified = true;
					xmlBill.DispositionCode.Value = bill.BB_CustomsStatusDescription;
					xmlBill.DispositionCode.IsSpecified = true;
					xmlBill.MatchedDate = bill.BB_MatchDate.Date;
					xmlBill.IsSpecified = true;
				}
			}
		}

		void ExportMessageInterpretation(US.Business.MQEDIMessage ediMessage, Xsd.ISFStatusCustomsResponse xmlCustomsResponse, IValueObjectExportContext context)
		{
			foreach (MessageBlock block in ediMessage.GetMessageBlocks<MessageBlock>())
			{
				Xsd.ISFStatusCustomsResponseMessageBlock messageBlock = xmlCustomsResponse.InterpretedFormat.AddNew();
				messageBlock.Name = block.GetType().Name;
				string messageBlockAsString = block.Serialise();
				bool isISFSF10 = block is ISFSF10;
				bool isISFSF30 = block is ISFSF30;
				foreach (MessageBlock.AttributeFieldInfo attributeFieldInfo in block.GetAttributeFieldInfos())
				{
					Xsd.ISFStatusCustomsResponseMessageBlockMessageField field = messageBlock.MessageField.AddNew();
					field.Name = attributeFieldInfo.FieldInfo.Name;
					field.ShouldCreateElementForEmptyValue = true;
					field.Value = attributeFieldInfo.Attribute.DeSerialise(messageBlockAsString).ToString();
				}
			}
		}

		void ExportParties(CusISFHeader bizObj, Xsd.ISF constructedValueObject, IValueObjectExportContext context, Dictionary<ZGuid, ZString> manufacturerList)
		{
			if (bizObj.IsISF10Entry)
			{
				ExportISF10Parties(bizObj, constructedValueObject, context, manufacturerList);
			}
			else if (bizObj.IsISF5Entry)
			{
				ExportISF5Parties(bizObj, constructedValueObject, context);
			}
		}

		void ExportISF5Parties(CusISFHeader bizObj, Xsd.ISF constructedValueObject, IValueObjectExportContext context)
		{
			Xsd.ISF5 isf5 = new Xsd.ISF5();
			isf5.IsSpecified = false;
			foreach (ISFDocAddress docAddress in bizObj.DocAddresses)
			{
				switch (docAddress.DocAddressType)
				{
					case DocAddressType.BookingPartyDocumentaryAddress:
						isf5.IsSpecified |= ExportParty(docAddress, isf5.BookingParties, "Booking Party", context);
						break;
					case DocAddressType.ShipToParty:
						isf5.IsSpecified |= ExportParty(docAddress, isf5.ShipToParties, "Ship To Party", context);
						break;
				}
			}
			constructedValueObject.Parties.Item = isf5;
		}

		void ExportISF10Parties(CusISFHeader bizObj, Xsd.ISF constructedValueObject, IValueObjectExportContext context, Dictionary<ZGuid, ZString> manufacturerList)
		{
			Xsd.ISF10 isf10 = new Xsd.ISF10();
			isf10.IsSpecified = false;

			foreach (ISFDocAddress docAddress in bizObj.DocAddresses)
			{
				switch (docAddress.DocAddressType)
				{
					case DocAddressType.SellingParty:
						isf10.IsSpecified |= ExportParty(docAddress, isf10.SellerOwners, "Selling Party", context);
						break;
					case DocAddressType.BuyingParty:
						isf10.IsSpecified |= ExportParty(docAddress, isf10.BuyerOwners, "Buying Party", context);
						break;
					case DocAddressType.ShipToParty:
						isf10.IsSpecified |= ExportParty(docAddress, isf10.ShipToParties, "Ship To Party", context);
						break;
					case DocAddressType.Consolidator:
						isf10.IsSpecified |= ExportParty(docAddress, isf10.Consolidators, "Consolidator", context);
						break;
					case DocAddressType.ScheduledContainerStuffingLocation:
						isf10.IsSpecified |= ExportParty(docAddress, isf10.StuffingLocations, "Stuffing Location", context);
						break;
				}
			}

			if (bizObj.ManufacturerAddresses.Count > 0)
			{
				DocAddressValueObjectHelper manufacturerHelper = new DocAddressValueObjectHelper("Manufacturer");
				int id = 0;
				foreach (ISFDocAddress manufacturer in bizObj.ManufacturerAddresses)
				{
					Xsd.ISFManufacturer xmlManufacturer = manufacturerHelper.ExportToValueObject<Xsd.ISFManufacturer>(manufacturer, context);
					ZString organisationID = ManufacturerIDPrefix + (++id).ToString();
					xmlManufacturer.ManufacturerID = organisationID;
					isf10.Manufacturers.Add(xmlManufacturer);
					manufacturerList.Add(manufacturer.PK, organisationID);
				}
				isf10.Manufacturers.IsSpecified = true;
				isf10.IsSpecified = true;
			}

			isf10.ConsigneePassportDetails.IsSpecified = false;
			if (!bizObj.BF_ConsigneeCode.IsEmpty)
			{
				isf10.Consignee.Number = bizObj.BF_ConsigneeCode;
				isf10.Consignee.NumberType = OrgCusCodeXmlMappings.Instance.GetExternalCode(bizObj.BF_ConsigneeCodeType, "Consignee Code Type", context);
				isf10.Consignee.CountryOfRegistration = Core.Constants.CountryCodes.UnitedStates;
				isf10.IsSpecified = true;
				if (bizObj.BF_ConsigneeCodeType == OrgCusCode.CodeTypes.PassportID)
				{
					isf10.ConsigneePassportDetails.CountryOfIssue = bizObj.BF_ConsigneeCountryOfIssue;
					isf10.ConsigneePassportDetails.DateOfBirth = bizObj.BF_ConsigneeDateOfBirth.Date;
					isf10.ConsigneePassportDetails.IsSpecified = true;
				}
				isf10.ConsigneeDateOfBirth = bizObj.BF_ConsigneeDateOfBirth.Date;

				if (bizObj.BF_ConsigneeCodeType == OrgCusCode.CodeTypes.PassportID || bizObj.BF_ConsigneeCodeType == OrgCusCode.USACodeTypes.SocialSecurityNumber)
				{
					isf10.ConsigneeFullLegalName = bizObj.BF_ConsigneeFullName;
				}
			}
			constructedValueObject.Parties.Item = isf10;
		}

		bool ExportParty(ISFDocAddress docAddress, Xsd.DocAddressCollection docAddressCollection, string errorContext, IValueObjectExportContext context)
		{
			if (docAddress != null && !docAddress.IsEmpty)
			{
				DocAddressValueObjectHelper docAddressHelper = new DocAddressValueObjectHelper(errorContext);
				Xsd.DocAddress xmlDocAddress = docAddressHelper.ExportToValueObject(docAddress, context);
				if (xmlDocAddress.IsSpecified)
				{
					docAddressCollection.Add(xmlDocAddress);
					docAddressCollection.IsSpecified = true;
				}
			}

			return docAddressCollection.IsSpecified;
		}

		const string ManufacturerIDPrefix = "MANUFACTURER_ID_";

		void ExportLines(CusISFHeader bizObj, Xsd.ISF constructedValueObject, IValueObjectExportContext context, Dictionary<ZGuid, ZString> manufacturerList)
		{
			if (bizObj.Lines.Count > 0)
			{
				DocAddressValueObjectHelper helper = new DocAddressValueObjectHelper("Manufacturer");
				foreach (CusISFLine line in bizObj.Lines)
				{
					Xsd.ISFLine isfLine = constructedValueObject.Lines.AddNew();
					isfLine.CountryOfOrigin = line.BL_RN_NKGoodsOrigin;
					isfLine.HTSNumber = line.BL_HarmonisedNum;
					ISFDocAddress manufacturerDocAddress = line.ManufacturerDocAddress;

					if (manufacturerDocAddress != null && manufacturerList.ContainsKey(manufacturerDocAddress.PK))
					{
						isfLine.ISF10.ManufacturerID = manufacturerList[manufacturerDocAddress.PK];
						isfLine.ISF10.IsSpecified = true;
					}

					if (!line.BL_TextProductCode.IsEmpty)
					{
						isfLine.ProductCode = line.BL_TextProductCode;
						isfLine.ProductCodeSpecified = true;
					}
					ExportCustomValues(line, isfLine, context);
				}
			}
		}

		void ExportContainers(CusISFHeader bizObj, Xsd.ISF constructedValueObject, IValueObjectExportContext context)
		{
			if (bizObj.Equipments.Count > 0)
			{
				foreach (CusISFEquip container in bizObj.Equipments)
				{
					Xsd.ISFContainer isfContainer = constructedValueObject.Containers.AddNew();
					isfContainer.ContainerNumber = container.BE_ContainerNum;
					isfContainer.DescriptionCode = container.BE_EquipCode;
					isfContainer.ISOType = container.BE_ContainerISO;
				}
			}
		}

		void ExportImporterOfRecords(CusISFHeader bizObj, Xsd.ISF constructedValueObject, IValueObjectExportContext context)
		{
			Xsd.ISFImporterOfRecord importerOfRecord = constructedValueObject.ImporterOfRecord;
			importerOfRecord.IsSpecified = false;

			if (!bizObj.BF_ImporterCode.IsEmpty)
			{
				importerOfRecord.IsSpecified = true;
				Xsd.RegistrationNumber registrationNumber = new Xsd.RegistrationNumber();
				registrationNumber.IsSpecified = true;
				registrationNumber.Number = bizObj.BF_ImporterCode;
				registrationNumber.NumberType = OrgCusCodeXmlMappings.Instance.GetExternalCode(bizObj.BF_ImporterCodeType, "Importer Of Record Registration Type", context);
				registrationNumber.CountryOfRegistration = Core.Constants.CountryCodes.UnitedStates;
				importerOfRecord.Item = registrationNumber;

				if (bizObj.BF_ImporterCodeType == OrgCusCode.CodeTypes.PassportID)
				{
					importerOfRecord.CountryOfIssue = bizObj.BF_CountryOfIssue;
					registrationNumber.CountryOfRegistration = bizObj.BF_CountryOfIssue;
				}

				if (bizObj.BF_ImporterCodeType == OrgCusCode.CodeTypes.PassportID || bizObj.BF_ImporterCodeType == OrgCusCode.USACodeTypes.SocialSecurityNumber)
				{
					importerOfRecord.DateOfBirth = bizObj.BF_DateOfBirth.Date;
					importerOfRecord.FullLegalName = bizObj.BF_ImporterFullName;
				}
			}
		}

		void ExportBills(CusISFHeader bizObj, Xsd.ISF constructedValueObject, IValueObjectExportContext context)
		{
			if (bizObj.ReferenceDatas.Count > 0)
			{
				foreach (CusISFBill bill in bizObj.ReferenceDatas)
				{
					if (!bill.BB_BillNum.IsEmpty && !bill.BB_BillType.IsEmpty)
					{
						Xsd.ISFReferenceID referenceID = constructedValueObject.ReferenceIDs.AddNew();
						referenceID.Number = bill.BB_BillNum;
						referenceID.Type = BillTypeToXmlCodeMappings.Instance.GetExternalCode(bill.BB_BillType, "Bill Type", context);
					}
				}
			}
		}

		#endregion

		#region Import

		internal Dictionary<ZGuid, Xsd.ISFActionType> AutoSendToCustomsList;

		bool hasMatchingError;
		public override CusISFHeader CreateOrUpdateFromValueObject(Xsd.ISF value, IValueObjectImportContext context)
		{
			CusISFHeader result = null;
			hasMatchingError = false;
			if (value != null)
			{
				result = FindBusinessObject(value, context);
				if (!hasMatchingError)
				{
					bool shouldImportFromValueObject = true;
					if (result == null)
					{
						result = NewBusinessObject(value, context);
					}
					else
					{
						shouldImportFromValueObject = ShouldUpdateExistingObject(result, context);
					}

					if (shouldImportFromValueObject)
					{
						ImportFromValueObject(result, value, context);
					}
					else
					{
						OnUserDeclinedImport(result, value, context);
					}
					AddEDIMessage(result, value, context);
				}
			}
			return result;
		}

		protected override void AfterImportFromValueObject(CusISFHeader bizObj, Xsd.ISF value, IValueObjectImportContext context)
		{
			base.AfterImportFromValueObject(bizObj, value, context);
			ZString reference = "Add";
			if (value.Action.IsSpecified)
			{
				Xsd.ISFActionType actionType = Xsd.ISFActionType.Add;
				if (value.Action.TypeSpecified)
				{
					actionType = value.Action.Type;
					reference = actionType.ToString();
				}

				if (AutoSendToCustomsList != null && value.Action.Messaging.IsSpecified && value.Action.Messaging.SendToCustoms)
				{
					AutoSendToCustomsList.Add(bizObj.PK, actionType);
					reference += " With Send To Customs";
				}
			}

			AddImportEvent(bizObj, reference);
		}

		protected override void AddImportEvent(CusISFHeader bizObj, ZString reference)
		{
			if (bizObj != null)
			{
				bizObj.Logs.AddNew(Events.DataImport, reference);
			}
		}

		protected override CusISFHeader FindBusinessObject(Xsd.ISF value, IValueObjectImportContext context)
		{
			string errorMessage = "";
			Xsd.ISFActionType actionType = Xsd.ISFActionType.Add;
			if (value.Action.IsSpecified && value.Action.TypeSpecified)
			{
				actionType = value.Action.Type;
			}
			CusISFHeader result = FindBusinessObject(value, context, actionType, ref errorMessage);

			if (result != null)
			{
				errorMessage = "";
				switch (actionType)
				{
					case Xsd.ISFActionType.Add:
						if (result.BF_CustomsStatus != MessageStatusList.Codes.ClearISFDelete && result.BF_CustomsStatus != MessageStatusList.Codes.ClearWithWarningISFDelete)
						{
							errorMessage = ActiveISFMatchingEitherHouseBillOrOreanBillExist;
							result = null;
						}
						break;
					case Xsd.ISFActionType.Replace:
					case Xsd.ISFActionType.Delete:
						if (result.IsWaitingForResponse)
						{
							errorMessage = string.Format(PendingCustomsResponseJobExist, actionType.ToString());
							result = null;
						}
						else if (result.BF_CustomsStatus == MessageStatusList.Codes.ClearISFDelete || result.BF_CustomsStatus == MessageStatusList.Codes.ClearWithWarningISFDelete)
						{
							errorMessage = string.Format(CustomsDeletedJobExist, actionType.ToString());
							result = null;
						}
						break;
				}
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				context.Add(ErrorType.DataErrorPreventSave, errorMessage);
				hasMatchingError = true;
			}
			return result;
		}
		internal const string ActiveISFMatchingEitherHouseBillOrOreanBillExist = "Cannot add a new ISF Job as there is an existing active ISF Job matching the Importer Of Record and either the House Bill of Lading or the Ocean Bill of Lading or the Owner Reference.";
		internal const string PendingCustomsResponseJobExist = "{0} failed. ISF Job is still waiting on a response from Customs.";
		internal const string CustomsDeletedJobExist = "{0} failed. ISF Job has already been deleted at Customs.";

		CusISFHeader FindBusinessObject(Xsd.ISF value, IValueObjectImportContext context, Xsd.ISFActionType actionType, ref string errorMessage)
		{
			CusISFHeader result = null;
			bool isAnAdd = actionType == Xsd.ISFActionType.Add;
			if (!isAnAdd)
			{
				errorMessage = string.Format(NoMatchingKeysSpecified, actionType.ToString());
			}
			if (!value.TransactionNumber.IsEmpty)
			{
				if (isAnAdd)
				{
					errorMessage = CannotAddNewIfTransactionNumberIsKnown;
				}
				else
				{
					MatchUsingTransactionNumber(value, actionType, context, ref result, ref errorMessage);
				}
			}
			else
			{
				if (!value.JobReference.IsEmpty)
				{
					if (isAnAdd)
					{
						errorMessage = CannotAddNewIfJobReferenceIsKnown;
					}
					else
					{
						MatchUsingJobReference(value, actionType, context, ref result, ref errorMessage);
					}
				}
				else
				{
					MatchUsingImporterAndBills(value, actionType, context, ref result, ref errorMessage);
				}
			}
			return result;
		}
		internal const string CannotAddNewIfJobReferenceIsKnown = "Action should be Replace if the Job Reference is known.";
		internal const string CannotAddNewIfTransactionNumberIsKnown = "Action should be Replace if the Transaction Number is known.";
		internal const string NoMatchingKeysSpecified = "Cannot {0} the existing data as matching data is not specified.\r\n\r\nMatching is attempted on the following elements in order.\r\nTransaction Number\r\nJob Reference Number\r\nImporter Of Record and at least a House Bill of Lading or an Ocean Bill of Lading.\r\nImporter Of Record and Owner Reference";

		void MatchUsingImporterAndBills(Xsd.ISF value, Xsd.ISFActionType actionType, IValueObjectImportContext context, ref CusISFHeader result, ref string errorMessage)
		{
			if (value.ImporterOfRecord.IsSpecified && (value.ReferenceIDs.IsSpecified || !value.OwnerReference.IsEmpty))
			{
				ZQuery importerQuery = null;
				Xsd.RegistrationNumber registrationNumber = value.ImporterOfRecord.Item as Xsd.RegistrationNumber;
				if (registrationNumber != null && registrationNumber.IsSpecified)
				{
					ZString importerCodeType = OrgCusCodeXmlMappings.Instance.GetEnterpriseCode(registrationNumber.NumberType, "ISF Importer Code", context);
					importerQuery = new ZQuery(CusISFHeaderSchema.BF_ImporterCode, registrationNumber.Number);
					importerQuery.AddToFilter(CusISFHeaderSchema.BF_ImporterCodeType, importerCodeType);
				}
				else
				{
					Xsd.Organisation xmlOrganisation = value.ImporterOfRecord.Item as Xsd.Organisation;
					if (xmlOrganisation != null && xmlOrganisation.IsSpecified)
					{
						ZGuid importerPK = context.FindOrganisationPK(xmlOrganisation, null, OrganisationTypes.None);
						if (importerPK.IsValid)
						{
							importerQuery = new ZQuery(CusISFHeaderSchema.BF_OH_Importer, importerPK);
						}
					}
				}

				bool isAnAdd = actionType == Xsd.ISFActionType.Add;
				if (!isAnAdd)
				{
					errorMessage = string.Format(NoImporterOfRecordSpecified, actionType.ToString());
				}
				if (importerQuery != null)
				{
					bool? isMatchedOnBills = null;
					if (value.ReferenceIDs.IsSpecified)
					{
						ZQuery houseBillOfLadingQuery = null;
						ZQuery oceanBillOfLadingQuery = null;
						foreach (Xsd.ISFReferenceID referenceID in value.ReferenceIDs)
						{
							if (referenceID.IsSpecified && !referenceID.Number.IsEmpty)
							{
								switch (referenceID.Type)
								{
									case Xsd.ISFReferenceIDType.BM:
										AddBillFilter(referenceID.Number, ref houseBillOfLadingQuery);
										break;
									case Xsd.ISFReferenceIDType.OB:
										AddBillFilter(referenceID.Number, ref oceanBillOfLadingQuery);
										break;
								}
							}
						}

						if (houseBillOfLadingQuery != null || oceanBillOfLadingQuery != null)
						{
							ZDBOnlyQuery billQuery = new ZDBOnlyQuery(typeof(CusISFHeader));
							if (houseBillOfLadingQuery != null)
							{
								ZDBOnlySubQuery houseBillOfLadingSubQuery = new ZDBOnlySubQuery(typeof(CusISFBill), CusISFBillSchema.BB_BF);
								houseBillOfLadingSubQuery.AddToFilter(CusISFBillSchema.BB_BillType, Common.US.ISF.BillTypeList.Codes.HouseBillOfLading);
								houseBillOfLadingSubQuery.AddToFilter(houseBillOfLadingQuery);
								billQuery.AddSubQuery(houseBillOfLadingSubQuery, JoinCondition.And);
							}
							if (oceanBillOfLadingQuery != null)
							{
								ZDBOnlySubQuery oceanBillOfLadingSubQuery = new ZDBOnlySubQuery(typeof(CusISFBill), CusISFBillSchema.BB_BF);
								oceanBillOfLadingSubQuery.AddToFilter(CusISFBillSchema.BB_BillType, Common.US.ISF.BillTypeList.Codes.OceanBillOfLading);
								oceanBillOfLadingSubQuery.AddToFilter(oceanBillOfLadingQuery);
								billQuery.AddSubQuery(oceanBillOfLadingSubQuery, JoinCondition.And);
							}

							if (!isAnAdd)
							{
								errorMessage = string.Format(NoISFMatchingEitherHouseBillOrOreanBill, actionType.ToString());
							}

							ZQuery query = new ZQuery(GetBillDateFilter());
							query.AddToFilter(importerQuery);
							query.AddToFilter(billQuery);

							CusISFHeader[] headers = context.Factory.Load<CusISFHeader>(query);
							isMatchedOnBills = headers.Length != 0;
							if (headers.Length > 1)
							{
								errorMessage = string.Format(MultipleISFMatchingEitherHouseBillOrOreanBill, isAnAdd ? "add a new ISF Job" : actionType.ToString() + " the existing data");
							}
							else if (headers.Length == 1)
							{
								result = headers[0];
							}
						}
					}

					if ((!isMatchedOnBills.HasValue || !isMatchedOnBills.Value) && !value.OwnerReference.IsEmpty)
					{
						if (!isAnAdd)
						{
							errorMessage = string.Format(isMatchedOnBills.HasValue ? NoISFMatchingEitherHouseBillOrOreanBillOrOwnerReference : NoISFMatchingOwnerReference, actionType.ToString());
						}

						ZQuery query = new ZQuery(GetBillDateFilter());
						query.AddToFilter(CusISFHeaderSchema.BF_OwnerReference, value.OwnerReference);
						query.AddToFilter(importerQuery);
						CusISFHeader[] headers = context.Factory.Load<CusISFHeader>(query);
						if (headers.Length == 1)
						{
							result = headers[0];
						}
						else if (headers.Length > 1)
						{
							errorMessage = string.Format(MultipleISFMatchingOwnerReference, isAnAdd ? "add a new ISF Job" : actionType.ToString() + " the existing data");
						}
					}
				}
			}
		}
		internal const string NoImporterOfRecordSpecified = "Cannot {0} the existing data as the Importer Of Record is not specified.";
		internal const string NoISFMatchingEitherHouseBillOrOreanBill = "Cannot {0} the existing data as there is no ISF Job matching either the House Bill of Lading or the Ocean Bill of Lading specified.";
		internal const string NoISFMatchingEitherHouseBillOrOreanBillOrOwnerReference = "Cannot {0} the existing data as there is no ISF Job matching either the House Bill of Lading or the Ocean Bill of Lading or the Owner Reference specified.";
		internal const string NoISFMatchingOwnerReference = "Cannot {0} the existing data as there is no ISF Job matching the Owner Reference specified.";
		internal const string MultipleISFMatchingEitherHouseBillOrOreanBill = "Cannot {0} as there are more than one ISF Job matching the Importer Of Record and either the House Bill of Lading or the Ocean Bill of Lading.";
		internal const string MultipleISFMatchingOwnerReference = "Cannot {0} as there are more than one ISF Job matching the Owner Reference.";

		ZQuery GetBillDateFilter()
		{
			ZQuery dateMatchQuery = new ZQuery(CusISFHeaderSchema.BF_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddDays(NoOfDaysOldAllowedInMatchingBill));
			dateMatchQuery.AddToFilter(JoinCondition.Or, CusISFHeaderSchema.BF_SystemCreateTimeUtc, ZDateTime.Empty);
			return dateMatchQuery;
		}
		public const int NoOfDaysOldAllowedInMatchingBill = -360;

		void MatchUsingJobReference(Xsd.ISF value, Xsd.ISFActionType actionType, IValueObjectImportContext context, ref CusISFHeader result, ref string errorMessage)
		{
			result = context.Factory.LoadFromNaturalKey<CusISFHeader>(CusISFHeaderSchema.BF_JobReference, value.JobReference);
			if (result == null)
			{
				errorMessage = string.Format(NoISFMatchingJobReference, actionType.ToString());
			}
		}
		internal const string NoISFMatchingJobReference = "Cannot {0} the existing data as there is no ISF Job matching the Job Reference.";

		void MatchUsingTransactionNumber(Xsd.ISF value, Xsd.ISFActionType actionType, IValueObjectImportContext context, ref CusISFHeader result, ref string errorMessage)
		{
			errorMessage = string.Format(NoISFMatchingTransactionNumber, actionType.ToString());
			CusISFHeader[] headers = context.Factory.Load<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_CustomsReference, value.TransactionNumber));
			if (headers.Length > 1)
			{
				errorMessage = string.Format(MultipleISFMatchingTransactionNumber, actionType.ToString());
			}
			else if (headers.Length == 1)
			{
				result = headers[0];
			}
		}
		internal const string NoISFMatchingTransactionNumber = "Cannot {0} the existing data as there is no ISF Job matching the Transaction Number.";
		internal const string MultipleISFMatchingTransactionNumber = "Cannot {0} the existing data as there are more than one ISF Jobs matching the Transaction Number.";

		void AddBillFilter(ZString billNumber, ref ZQuery query)
		{
			if (query == null)
			{
				query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.Or;
			}
			query.AddToFilter(CusISFBillSchema.BB_BillNum, billNumber);
		}

		protected void ImportFromInterchangeDetails(CusISFHeader bizObj, IValueObjectImportContext context)
		{
			Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;

			if (interchange.IsSpecified && interchange.InterchangeInfo.IsSpecified && interchange.InterchangeInfo.Target.IsSpecified && !interchange.InterchangeInfo.Target.BranchCode.IsEmpty)
			{
				GlbBranch branch = context.Factory.LoadFromUniqueKey<GlbBranch>(GlbBranchSchema.GB_Code, interchange.InterchangeInfo.Target.BranchCode);

				if (branch != null)
				{
					bizObj.BF_GB = branch.PK;
				}
				else
				{
					context.Notify(new WarningNotification(Res.GetString("3978b2db-bd7a-414e-836b-a517d660a63f", "Branch Code not found: {0}", interchange.InterchangeInfo.Target.BranchCode)));
				}
			}
		}

		protected override void ImportFromValueObjectCore(CusISFHeader bizObj, Xsd.ISF xmlISF, IValueObjectImportContext context)
		{
			ImportFromInterchangeDetails(bizObj, context);
			Xsd.ISFActionType actionType = Xsd.ISFActionType.Add;
			if (xmlISF.Action.IsSpecified && xmlISF.Action.TypeSpecified)
			{
				actionType = xmlISF.Action.Type;
			}
			switch (actionType)
			{
				case Xsd.ISFActionType.Delete:
					ImportDeleteAction(bizObj, xmlISF, context);
					break;
				case Xsd.ISFActionType.Add:
					bizObj.BF_CustomsReference = ZString.Empty;
					bizObj.BF_CustomsStatus = MessageStatusList.Codes.NotSentISF;
					ImportISFData(bizObj, xmlISF, context);
					break;
				default:
					ImportISFData(bizObj, xmlISF, context);
					break;
			}
		}

		void ImportDeleteAction(CusISFHeader bizObj, Xsd.ISF xmlISF, IValueObjectImportContext context)
		{
			string uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ImporterSecurityFiling, bizObj.PK.ToGuid());
			HtmlResponseEmailGenerator emailGenerator = new HtmlResponseEmailGenerator();
			EmailDef email;
			if (emailGenerator.TryGenerateEmail(Res.GetString("7234600f-64e0-4701-a84c-0da0ad3f68c3", "ISF XML Import with Delete Action: {0}", bizObj.HumanReadableName), "Delete Request for <a href=\"" + uri + "\">" + bizObj.HumanReadableName + "</a>", "This ISF Job has been deleted by the client.<br />Please send a 'Delete' message to Customs if needed.", "", "", out email))
			{
				Env.OutgoingCustomsMailManager.Create(bizObj.Factory, email, ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup.GetFallBackValueAtAllLevels(bizObj.RegistryCompanyPK, bizObj.RegistryBranchPK, Guid.Empty),
					GroupSourceLocator.GetFromRegistryItem(ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup));
			}
		}

		void ImportISFData(CusISFHeader bizObj, Xsd.ISF xmlISF, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(bizObj.BF_EntryTypeInfo, xmlISF.SubmissionTypeSpecified ? EntryTypeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlISF.SubmissionType, "ISF Submission Type", context) : "");
			context.SetPropertyInfoValue(bizObj.BF_ShipmentTypeInfo, xmlISF.ShipmentTypeSpecified ? ShipmentTypeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlISF.ShipmentType, "ISF Shipment Type", context) : "");
			if (xmlISF.TransportModeSpecified)
			{
				context.SetPropertyInfoValue(bizObj.BF_TransportModeInfo, TransportModeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlISF.TransportMode, "ISF Transport Mode", context));
			}
			if (bizObj.BF_ShipmentType == ShipmentTypeList.Codes.Informal)
			{
				context.SetPropertyInfoValue(bizObj.BF_ShipmentSubTypeInfo, xmlISF.ShipmentSubTypeSpecified ? ShipmentSubTypeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlISF.ShipmentSubType, "ISF Shipment Sub Type", context) : "");
				bizObj.BF_EstimatedValue = xmlISF.EstimatedValue;
				if (xmlISF.EstimatedQuantity.IsSpecified)
				{
					bizObj.BF_EstimatedQuantity = xmlISF.EstimatedQuantity.Value.Round(0).ToZInt();
					context.SetPropertyInfoValue(bizObj.BF_EstimatedQuantityUQInfo, xmlISF.EstimatedQuantity.DimensionType, xmlISF.EstimatedQuantity.DimensionTypeSpecified, "Estimated Quantity");
				}
				bizObj.BF_EstimatedWeight = xmlISF.EstimatedWeight.Value.Round(0).ToZInt();
				bizObj.BF_EstimatedWeightUQ = xmlISF.EstimatedWeight.DimensionType;
			}
			context.SetPropertyInfoValue(bizObj.BF_SCACInfo, xmlISF.CarrierSCAC);
			List<CusISFBill> existingReferenceIDs = new List<CusISFBill>(bizObj.ReferenceDatas);
			ImportImporterOfRecords(bizObj, xmlISF, context);
			List<CusISFBill> newlyAddedReferenceIDsFromImporterDefault = new List<CusISFBill>();
			if (existingReferenceIDs.Count != bizObj.ReferenceDatas.Count)
			{
				foreach (CusISFBill referenceData in bizObj.ReferenceDatas)
				{
					if (!existingReferenceIDs.Contains(referenceData))
					{
						newlyAddedReferenceIDsFromImporterDefault.Add(referenceData);
					}
				}
			}
			context.SetPropertyInfoValue(bizObj.BF_OwnerReferenceInfo, xmlISF.OwnerReference);
			if (xmlISF.Action.IsSpecified)
			{
				if (xmlISF.Action.Messaging.IsSpecified)
				{
					if (xmlISF.Action.Messaging.NoOfHTSDigitsToSendSpecified)
					{
						bizObj.BF_NumOfHarmChars = NoOfHTSDigitsToSendToXmlCodeMappings.Instance.GetEnterpriseCode(xmlISF.Action.Messaging.NoOfHTSDigitsToSend, "No. Of HTS Digits To Send", context);
					}
					if (xmlISF.Action.Messaging.LineMergeStyleSpecified)
					{
						bizObj.BF_LineMergeStyle = MergeStyleToXmlCodeMappings.Instance.GetEnterpriseCode(xmlISF.Action.Messaging.LineMergeStyle, "Merge Style", context);
					}
					if (xmlISF.Action.Messaging.SendContainerDetailsToCustomsSpecified)
					{
						bizObj.BF_SendEquipment = SendContainerDetailsToCustomsXmlCodeMappings.Instance.GetEnterpriseCode(xmlISF.Action.Messaging.SendContainerDetailsToCustoms, "Send Container Details To Customs", context);
					}
				}
				context.SetPropertyInfoValue(bizObj.BF_ActionReasonCodeInfo, ActionReasonCodeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlISF.Action.ReasonCode, "Action Reason Code", context), xmlISF.Action.ReasonCodeSpecified);
			}
			context.SetPropertyInfoValue(bizObj.BF_RL_NKPortOfUnloadInfo, xmlISF.ForeignPortOfUnlading.IsSpecified ? xmlISF.ForeignPortOfUnlading.Value : ZString.Empty, ForeignKeyType.PortNK, "Port Or Unload");
			context.SetPropertyInfoValue(bizObj.BF_RL_NKPlaceOfDeliveryInfo, xmlISF.PlaceOfDelivery.IsSpecified ? xmlISF.PlaceOfDelivery.Value : ZString.Empty, ForeignKeyType.PortNK, "Place Of Delivery");

			if (xmlISF.BondHolderSpecified || (bizObj.BF_OH_Importer.IsEmpty && bizObj.BF_BondNumberOrHolder != bizObj.BF_ImporterCode))
			{
				context.SetPropertyInfoValue(bizObj.BF_BondNumberOrHolderInfo, xmlISF.BondHolder);
			}
			if (xmlISF.BondActivityCodeSpecified || bizObj.BF_OH_Importer.IsEmpty)
			{
				context.SetPropertyInfoValue(bizObj.BF_BondActivityCodeInfo, xmlISF.BondActivityCodeSpecified ? BondActivityCodeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlISF.BondActivityCode, "Bond Activity Code", context) : "");
			}
			if (xmlISF.BondTypeSpecified || bizObj.BF_OH_Importer.IsEmpty)
			{
				context.SetPropertyInfoValue(bizObj.BF_BondTypeInfo, xmlISF.BondTypeSpecified ? BondTypeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlISF.BondType, "Bond Type", context) : "");
			}

			Dictionary<ZString, ZGuid> manufacturerList = new Dictionary<ZString, ZGuid>();
			ImportParties(bizObj, xmlISF.Parties, context, manufacturerList);
			ImportContainers(bizObj, xmlISF.Containers, context);
			ImportReferenceIDs(bizObj, xmlISF.ReferenceIDs, newlyAddedReferenceIDsFromImporterDefault, context);
			XsdPlannedLegObjectHelper.ImportPlannedLegs(bizObj.Transports, xmlISF.Routings, context, "Routings");
			ImportLines(bizObj, xmlISF.Lines, context, manufacturerList);
			ImportWorkflow(bizObj, xmlISF.Workflow, context);
		}

		void ImportWorkflow(CusISFHeader bizObj, Xsd.Workflow xmlWorkflow, IValueObjectImportContext context)
		{
			if (xmlWorkflow != null && xmlWorkflow.IsSpecified && xmlWorkflow.Triggers != null && xmlWorkflow.IsSpecified)
			{
				var workflowItems = bizObj.WorkflowItems;
				// WorkflowValueObjectHelper will only import trigger so we should not touch milestone
				if (xmlWorkflow.Triggers.Count > 0)
				{
					workflowItems.Triggers.RemoveAndDeleteAll();
				}
				else
				{
					// only triggers that were not added by template; no point in deleting template trigger as it get added only saving.
					workflowItems.Triggers.Cast<ProcessTask>().Where(x => x.P9_ParentTemplateID.IsEmpty).DeleteAll();
				}
				WorkflowValueObjectHelper helper = new WorkflowValueObjectHelper();
				helper.ImportFromValueObject(xmlWorkflow, workflowItems, context);
			}
		}

		void ImportParties(CusISFHeader bizObj, Xsd.ISFParties xmlParties, IValueObjectImportContext context, Dictionary<ZString, ZGuid> manufacturerList)
		{
			List<ISFDocAddress> existingDocAddresses = new List<ISFDocAddress>(bizObj.DocAddresses);
			if (xmlParties.IsSpecified)
			{
				Xsd.ISF10 xmlISF10Parties = xmlParties.Item as Xsd.ISF10;
				if (xmlISF10Parties != null)
				{
					ImportISF10Parties(bizObj, xmlISF10Parties, context, manufacturerList, existingDocAddresses);
				}
				else
				{
					Xsd.ISF5 xmlISF5Parties = xmlParties.Item as Xsd.ISF5;
					if (xmlISF5Parties != null)
					{
						ImportISF5Parties(bizObj, xmlISF5Parties, context, existingDocAddresses);
					}
				}
			}
			foreach (ISFDocAddress docAddressNotInXML in existingDocAddresses)
			{
				docAddressNotInXML.Delete();
			}
		}

		void ImportISF5Parties(CusISFHeader bizObj, Xsd.ISF5 xmlISF5Parties, IValueObjectImportContext context, List<ISFDocAddress> existingDocAddresses)
		{
			ImportParty(bizObj, xmlISF5Parties.BookingParties, bizObj.ISFDocAddressRequirementProvider.GetOtherPartyISFDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress, SubmissionTypeList.Codes.ISF5, false), "Booking Party", context, existingDocAddresses);
			ImportParty(bizObj, xmlISF5Parties.ShipToParties, bizObj.ISFDocAddressRequirementProvider.ShipToPartyDocAddressRequirement, "Ship To Party", context, existingDocAddresses);
		}

		void ImportISF10Parties(CusISFHeader bizObj, Xsd.ISF10 xmlISF10Parties, IValueObjectImportContext context, Dictionary<ZString, ZGuid> manufacturerList, List<ISFDocAddress> existingDocAddresses)
		{
			if (xmlISF10Parties.Consignee.IsSpecified && !xmlISF10Parties.Consignee.Number.IsEmpty)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.BF_ConsigneeCodeTypeInfo, OrgCusCodeXmlMappings.Instance.GetEnterpriseCode(xmlISF10Parties.Consignee.NumberType, "ISF Consignee Code", context));
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.BF_ConsigneeCodeInfo, xmlISF10Parties.Consignee.Number);
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.BF_ConsigneeDateOfBirthInfo, xmlISF10Parties.ConsigneeDateOfBirth);
				if (xmlISF10Parties.ConsigneePassportDetails.IsSpecified)
				{
					context.SetPropertyInfoValueIfValueNotEmpty(bizObj.BF_ConsigneeCountryOfIssueInfo, xmlISF10Parties.ConsigneePassportDetails.CountryOfIssue);
					context.SetPropertyInfoValueIfValueNotEmpty(bizObj.BF_ConsigneeDateOfBirthInfo, xmlISF10Parties.ConsigneePassportDetails.DateOfBirth);
				}
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.BF_ConsigneeFullNameInfo, xmlISF10Parties.ConsigneeFullLegalName);
			}

			ImportParty(bizObj, xmlISF10Parties.BuyerOwners, bizObj.ISFDocAddressRequirementProvider.GetOtherPartyISFDocAddressRequirement(DocAddressType.BuyingParty, SubmissionTypeList.Codes.ISF10, true), "Buyer Owner", context, existingDocAddresses);
			ImportParty(bizObj, xmlISF10Parties.SellerOwners, bizObj.ISFDocAddressRequirementProvider.GetOtherPartyISFDocAddressRequirement(DocAddressType.SellingParty, SubmissionTypeList.Codes.ISF10, true), "Seller Owner", context, existingDocAddresses);
			ImportParty(bizObj, xmlISF10Parties.ShipToParties, bizObj.ISFDocAddressRequirementProvider.ShipToPartyDocAddressRequirement, "Ship To Party", context, existingDocAddresses);
			ImportParty(bizObj, xmlISF10Parties.StuffingLocations, bizObj.ISFDocAddressRequirementProvider.GetOtherPartyISFDocAddressRequirement(DocAddressType.ScheduledContainerStuffingLocation, SubmissionTypeList.Codes.ISF10, false), "Stuffing Location", context, existingDocAddresses);
			ImportParty(bizObj, xmlISF10Parties.Consolidators, bizObj.ISFDocAddressRequirementProvider.GetOtherPartyISFDocAddressRequirement(DocAddressType.Consolidator, SubmissionTypeList.Codes.ISF10, false), "Consolidator", context, existingDocAddresses);

			if (xmlISF10Parties.Manufacturers.IsSpecified)
			{
				int sequence = -1;
				DocAddressValueObjectHelper helper = new DocAddressValueObjectHelper("Manufacturers");
				ISFDocAddressRequirement requiment = bizObj.ISFDocAddressRequirementProvider.ManufacturerDocAddressRequirement;
				foreach (Xsd.ISFManufacturer xmlManufacturer in xmlISF10Parties.Manufacturers)
				{
					if (xmlManufacturer.IsSpecified)
					{
						ISFDocAddress manufacturer = bizObj.DocAddresses.FindOrCreateWithRequirement(requiment, ++sequence);
						helper.ImportFromValueObject(xmlManufacturer, manufacturer, context);
						if (!xmlManufacturer.ManufacturerID.IsEmpty)
						{
							if (!manufacturerList.ContainsKey(xmlManufacturer.ManufacturerID))
							{
								manufacturerList.Add(xmlManufacturer.ManufacturerID, manufacturer.PK);
							}
						}
						if (existingDocAddresses.Contains(manufacturer))
						{
							existingDocAddresses.Remove(manufacturer);
						}

						if (manufacturer.E2_AddressOverride && manufacturer.E2_ValidationStatus == AddressValidationStatus.ToBeVerified)
						{
							manufacturer.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
						}
					}
				}
			}
		}

		void ImportParty(CusISFHeader bizObj, Xsd.DocAddressCollection xmlParties, ISFDocAddressRequirement requirement, string errorContext, IValueObjectImportContext context, List<ISFDocAddress> existingDocAddresses)
		{
			if (xmlParties.IsSpecified)
			{
				int sequence = -1;
				DocAddressValueObjectHelper helper = new DocAddressValueObjectHelper(errorContext);
				foreach (Xsd.DocAddress xmlParty in xmlParties)
				{
					if (xmlParty.IsSpecified)
					{
						ISFDocAddress docAddress = bizObj.DocAddresses.FindOrCreateWithRequirement(requirement, ++sequence);
						helper.ImportFromValueObject(xmlParty, docAddress, context);
						if (existingDocAddresses.Contains(docAddress))
						{
							existingDocAddresses.Remove(docAddress);
						}

						if (docAddress.E2_AddressOverride && docAddress.E2_ValidationStatus == AddressValidationStatus.ToBeVerified)
						{
							docAddress.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
						}
					}
				}
			}
		}

		void ImportImporterOfRecords(CusISFHeader bizObj, Xsd.ISF xmlISF, IValueObjectImportContext context)
		{
			if (xmlISF.ImporterOfRecord.IsSpecified)
			{
				bizObj.BF_ImporterCode = ZString.Empty;
				bizObj.BF_ImporterCodeType = ZString.Empty;
				bizObj.BF_DateOfBirth = ZDateTime.Empty;
				bizObj.BF_CountryOfIssue = ZString.Empty;
				bizObj.BF_OH_Importer = ZGuid.Empty;
				Xsd.RegistrationNumber registrationNumber = xmlISF.ImporterOfRecord.Item as Xsd.RegistrationNumber;
				if (registrationNumber != null)
				{
					ZString importerCodeType = OrgCusCodeXmlMappings.Instance.GetEnterpriseCode(registrationNumber.NumberType, "ISF Importer Code", context);
					context.SetPropertyInfoValue(bizObj.BF_ImporterCodeTypeInfo, importerCodeType);
					context.SetPropertyInfoValue(bizObj.BF_ImporterCodeInfo, registrationNumber.Number);

					if (!bizObj.BF_ImporterCode.IsEmpty && xmlISF.Action.IsSpecified && xmlISF.Action.Type == Xsd.ISFActionType.Add)
					{
						OrgHeader importer = bizObj.Importer; // the BF_OH_Importer should have been set from the BF_ImporterCode
						if (importer != null && importer.Branch != null)
						{
							bizObj.BF_GB = importer.Branch.PK;
						}
					}
				}
				else
				{
					Xsd.Organisation xmlOrganisation = xmlISF.ImporterOfRecord.Item as Xsd.Organisation;
					if (xmlOrganisation != null)
					{
						bizObj.BF_OH_Importer = context.FindOrCreateTempOrganisationPK(xmlOrganisation, bizObj, OrganisationTypes.None);
						if (xmlISF.Action.IsSpecified && xmlISF.Action.Type == Xsd.ISFActionType.Add)
						{
							OrgHeader importer = bizObj.Importer;
							if (importer != null && importer.Branch != null)
							{
								bizObj.BF_GB = importer.Branch.PK;
							}
						}
					}
				}

				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.BF_DateOfBirthInfo, xmlISF.ImporterOfRecord.DateOfBirth);
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.BF_CountryOfIssueInfo, xmlISF.ImporterOfRecord.CountryOfIssue);
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.BF_ImporterFullNameInfo, xmlISF.ImporterOfRecord.FullLegalName);
			}
		}

		void ImportLines(CusISFHeader bizObj, Xsd.ISFLineCollection xmlLines, IValueObjectImportContext context, Dictionary<ZString, ZGuid> manufacturerList)
		{
			bizObj.Lines.DeleteAll();
			if (xmlLines.IsSpecified)
			{
				foreach (Xsd.ISFLine xmlLine in xmlLines)
				{
					ImportLine(bizObj, xmlLine, context, manufacturerList);
				}
			}
		}

		void ImportLine(CusISFHeader bizObj, Xsd.ISFLine xmlLine, IValueObjectImportContext context, Dictionary<ZString, ZGuid> manufacturerList)
		{
			if (xmlLine.IsSpecified)
			{
				CusISFLine line = bizObj.Lines.AddNew();
				if (xmlLine.ISF10.IsSpecified && !xmlLine.ISF10.ManufacturerID.IsEmpty && manufacturerList.ContainsKey(xmlLine.ISF10.ManufacturerID))
				{
					line.BL_ManufacturerDocAddressPK = manufacturerList[xmlLine.ISF10.ManufacturerID];
				}
				ImportProductIfPossible(line, context, xmlLine);
				if (!xmlLine.CountryOfOrigin.IsEmpty)
				{
					context.SetPropertyInfoValue(line.BL_RN_NKGoodsOriginInfo, xmlLine.CountryOfOrigin, ForeignKeyType.CountryNK);
				}
				context.SetPropertyInfoValueIfValueNotEmpty(line.BL_HarmonisedNumInfo, xmlLine.HTSNumber);
				ImportCustomValues(line, xmlLine, context);
			}
		}

		void ImportProductIfPossible(CusISFLine line, IValueObjectImportContext context, Xsd.ISFLine xmlLine)
		{
			if (xmlLine.ProductCodeSpecified)
			{
				context.SetPropertyInfoValue(line.BL_TextProductCodeInfo, xmlLine.ProductCode);
				OrgSupplierPart part = line.SupplierPart;
				if (part == null)
				{
					context.Notify(new WarningNotification("Product Code not found for the Importer: " + xmlLine.ProductCode));
				}
			}
		}

		void ImportContainers(CusISFHeader bizObj, Xsd.ISFContainerCollection xmlContainers, IValueObjectImportContext context)
		{
			List<CusISFEquip> existingEquipments = new List<CusISFEquip>(bizObj.Equipments);
			foreach (Xsd.ISFContainer xmlContainer in xmlContainers)
			{
				if (xmlContainer.IsSpecified)
				{
					CusISFEquip container = bizObj.Equipments[xmlContainer.ContainerNumber];
					if (container == null)
					{
						container = bizObj.Equipments.AddNew();
					}
					else
					{
						if (existingEquipments.Contains(container))
						{
							existingEquipments.Remove(container);
						}
					}
					context.SetPropertyInfoValueIfValueNotEmpty(container.BE_ContainerNumInfo, xmlContainer.ContainerNumber);
					context.SetPropertyInfoValueIfValueNotEmpty(container.BE_ContainerISOInfo, xmlContainer.ISOType);
					context.SetPropertyInfoValueIfValueNotEmpty(container.BE_EquipCodeInfo, xmlContainer.DescriptionCode);
				}
			}
			foreach (CusISFEquip equipment in existingEquipments)
			{
				equipment.Delete();
			}
		}

		void ImportReferenceIDs(CusISFHeader bizObj, Xsd.ISFReferenceIDCollection xmlReferenceIDs, List<CusISFBill> newlyAddedReferenceIDsFromImporterDefault, IValueObjectImportContext context)
		{
			List<CusISFBill> existingReferenceDatas = new List<CusISFBill>(bizObj.ReferenceDatas);
			foreach (Xsd.ISFReferenceID xmlReferenceID in xmlReferenceIDs)
			{
				if (xmlReferenceID.IsSpecified)
				{
					ZString billType = BillTypeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlReferenceID.Type, "ISF Reference ID Type", context);
					CusISFBill bill = bizObj.ReferenceDatas[billType, xmlReferenceID.Number] ?? newlyAddedReferenceIDsFromImporterDefault.Find(new Predicate<CusISFBill>(x => x.BB_BillType == billType));
					if (bill == null)
					{
						bill = bizObj.ReferenceDatas.AddNew();
					}
					else
					{
						if (existingReferenceDatas.Contains(bill))
						{
							existingReferenceDatas.Remove(bill);
						}
					}
					context.SetPropertyInfoValue(bill.BB_BillNumInfo, xmlReferenceID.Number);
					context.SetPropertyInfoValue(bill.BB_BillTypeInfo, billType);
				}
			}
			foreach (CusISFBill referenceData in existingReferenceDatas)
			{
				if (!newlyAddedReferenceIDsFromImporterDefault.Contains(referenceData))
				{
					referenceData.Delete();
				}
			}
		}

		#endregion
	}
}

#region Overrides for base test
#endregion
