using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using SystemDataRegistry = Enterprise.Registry.Business.SystemDataRegistry;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public abstract class ConsolValueObjectDataAdapter<TBusinessObject, TShipment, TValueObject> : FreightValueObjectDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : CommonConsol
		where TShipment : CommonShipment
		where TValueObject : Xsd.Consol
	{
		protected ConsolValueObjectDataAdapter()
			: this(EventsWithSourceType.Empty)
		{
		}

		protected ConsolValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
		{
			this.TriggeredByEvents = triggeredByEvents;
		}
		protected readonly EventsWithSourceType TriggeredByEvents;

		#region XML Schema Overrides

		public override string RootCollectionElementName
		{
			get { return (NoResString)"Consols"; }
		}

		public override string RootElementName
		{
			get { return (NoResString)"Consol"; }
		}

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleConsolSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return FreightXmlSchemaDefinitions.Instance.ConsolsSchema; }
		}

		#endregion

		#region FindBusinessObject

		protected override TBusinessObject FindBusinessObject(TValueObject consolValue, IValueObjectImportContext context)
		{
			TBusinessObject consol = null;
			string masterBill = GetMasterBillFromValueObject(consolValue);
			string agentReference = consolValue.ConsolDetail.AgentReference;

			if (ImportConsolNoFromXml)
			{
				ZQuery filter = new ZQuery(JobConsolSchema.JK_UniqueConsignRef, agentReference);
				consol = context.Factory.LoadTop1<TBusinessObject>(filter);
			}
			else
			{
				if (masterBill != null || !string.IsNullOrEmpty(agentReference))
				{
					ZString transportMode = "";
					ZString firstLoad = "";
					ZString lastDischarge = "";
					ZDateTime eTD = ZDateTime.Empty;

					if (consolValue.ConsolDetail != null)
					{
						if (consolValue.ConsolDetail.TransportModeSpecified)
						{
							transportMode = TransportModeToXmlCodeMappings.Instance.GetEnterpriseCode(consolValue.ConsolDetail.TransportMode, Res.GetString("cd388241-8c0d-4029-9f85-26b971ac4938", "Transport mode"), context);
						}

						if (consolValue.ConsolDetail.PortOfLoading != null)
						{
							eTD = consolValue.ConsolDetail.PortOfLoading.EstimatedDateTime;
							firstLoad = consolValue.ConsolDetail.PortOfLoading.Port.Value;
						}

						if (consolValue.ConsolDetail.PortOfDischarge != null)
						{
							lastDischarge = consolValue.ConsolDetail.PortOfDischarge.Port.Value;
						}
					}

					consol = new ConsolLocator<TBusinessObject>().Find(context.Factory, masterBill, agentReference, transportMode, firstLoad, lastDischarge, eTD);
				}
			}
			return consol;
		}

		string GetMasterBillFromValueObject(Xsd.Consol consolValue)
		{
			if (consolValue.ConsolIdentifier != null)
			{
				foreach (Xsd.ConsolIdentifier identifier in consolValue.ConsolIdentifier)
				{
					if (identifier.ConsolIdentifierType == Xsd.ConsolIdentifierType.MasterWaybill)
					{
						return identifier.Value;
					}
				}
			}

			return null;
		}

		#endregion

		#region Import

		bool ImportAllElementsEvenIfNotSpecified
		{
			get { return !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value; }
		}

		protected bool ImportConsolNoFromXml
		{
			get { return SystemRegistry.ImportConsolNoFromXml.Value; }
		}

		protected override void ImportFromValueObjectCore(TBusinessObject bizObj, TValueObject valueObj, IValueObjectImportContext context)
		{
			ImportConsolAndChildObjects(bizObj, valueObj, context);
		}

		protected override void OnUserDeclinedImport(TBusinessObject bizObj, TValueObject valueObj, IValueObjectImportContext context)
		{
			if (AlwaysCheckForSailingShipmentsAndContainers)
			{
				ImportChildObjectsOnly(bizObj, valueObj, context);
			}
		}

		void ImportConsolAndChildObjects(TBusinessObject consol, TValueObject value, IValueObjectImportContext context)
		{
			if (ImportConsolNoFromXml && !value.ConsolDetail.AgentReference.IsEmpty)
			{
				context.SetPropertyInfoValue(consol.JK_UniqueConsignRefInfo, value.ConsolDetail.AgentReference, value.ConsolDetail.AgentReferenceSpecified);
			}

			ImportMasterBill(consol, value, context);

			string errorContext = Res.GetString("0e2961c6-2a71-432b-82cc-3eae2fd3448c", "Master bill {0}", consol.JK_MasterBillNum);
			StmALogValueObjectDataAdapter.New(consol, errorContext, TriggeredByEvents).FromXmlCollectionValueObject(value.Events, context);

			if (value.ConsolDetail != null)
			{
				string transportMode = TransportModeToXmlCodeMappings.Instance.GetEnterpriseCode(value.ConsolDetail.TransportMode, errorContext, context);
				if (value.ConsolDetail.TransportModeSpecified || ImportAllElementsEvenIfNotSpecified)
				{
					context.SetPropertyInfoValueIfValueNotEmpty(consol.JK_TransportModeInfo, ConsolTransportModeToXmlCodeMappings.Instance.GetEnterpriseCode(value.ConsolDetail.TransportMode, errorContext, context));
				}

				if (value.ConsolDetail.PaymentTypeSpecified || ImportAllElementsEvenIfNotSpecified)
				{
					context.SetPropertyInfoValueIfValueNotEmpty(consol.JK_PrepaidCollectInfo, PrepaidCollectToXmlCodeMappings.Instance.GetEnterpriseCode(value.ConsolDetail.PaymentType.ToString(), errorContext, context));
				}

				context.SetPropertyInfoValue(consol.JK_ReleaseTypeInfo, value.ConsolDetail.ReleaseType, value.ConsolDetail.ReleaseTypeSpecified);
				context.SetPropertyInfoValue(consol.JK_NoOriginalBillsInfo, value.ConsolDetail.NumberOfOriginalBills.ToString(), value.ConsolDetail.NumberOfOriginalBillsSpecified);
				context.SetPropertyInfoValue(consol.JK_NoCopyBillsInfo, value.ConsolDetail.NumberOfCopyBills.ToString(), value.ConsolDetail.NumberOfCopyBillsSpecified);

				if (value.ConsolDetail.MasterBillIssueDate.IsValid)
				{
					consol.JK_MasterBillIssueDate = value.ConsolDetail.MasterBillIssueDate;
				}

				ImportRoutingInformation(consol, value, context, errorContext);
				ImportConsolContainers(consol, value, context);
				ImportConsolShipmentsAndDeclarations(consol, value, context);

				if (value.ConsolDetail.ConsolTypeSpecified || ImportAllElementsEvenIfNotSpecified)
				{
					context.SetPropertyInfoValueIfValueNotEmpty(consol.JK_AgentTypeInfo, AgentTypeToXmlCodeMappings.Instance.GetEnterpriseCode(value.ConsolDetail.ConsolType, errorContext, context));
				}

				if (value.ConsolDetail.ContainerModeSpecified || ImportAllElementsEvenIfNotSpecified)
				{
					context.SetPropertyInfoValueIfValueNotEmpty(consol.JK_ConsolModeInfo, ContainerModeToXmlCodeMappings.Instance.GetEnterpriseCode(value.ConsolDetail.ContainerMode, errorContext, context));
				}

				context.SetPropertyInfoValue(consol.JK_AgentsReferenceInfo, value.ConsolDetail.AgentReference, value.ConsolDetail.AgentReferenceSpecified || ImportAllElementsEvenIfNotSpecified);
				context.SetPropertyInfoValue(consol.JK_BookingReferenceInfo, value.ConsolDetail.BookingReference, value.ConsolDetail.BookingReferenceSpecified || ImportAllElementsEvenIfNotSpecified);

				SetForwarders(consol, value, context);

				if (value.ConsolDetail.Carrier.IsSpecified || ImportAllElementsEvenIfNotSpecified)
				{
					consol.SetDefaultShippingLineAddress(context.FindOrCreateTempOrganisationPK(value.ConsolDetail.Carrier, consol, OrganisationTypes.Carrier));
				}

				if (value.ConsolDetail.Creditor.IsSpecified || ImportAllElementsEvenIfNotSpecified)
				{
					consol.CreditorPK = context.FindOrCreateTempOrganisationPK(value.ConsolDetail.Creditor, consol, OrganisationTypes.Creditor);
				}

				XsdMovement.ToPortEstimatedActualDates(value.ConsolDetail.PortFirstArrival, consol.JK_RL_NKPortOfFirstArrivalInfo, consol.JK_DatePortOfFirstArrivalInfo, consol.JK_DatePortOfFirstArrivalInfo, Res.GetString("977dfea0-aabb-4738-8db9-1e98da202fd6", "First arrival port"), context);
				XsdMovement.ToPortEstimatedActualDates(value.ConsolDetail.PortFirstForeign, consol.JK_RL_NKFirstForeignPortInfo, consol.JK_DateFirstForeignPortInfo, consol.JK_DateFirstForeignPortInfo, Res.GetString("00949173-9d5c-4f74-8c99-dd06fe6bc0d8", "First foreign port"), context);
				XsdMovement.ToPortEstimatedActualDates(value.ConsolDetail.PortLastForeign, consol.JK_RL_NKLastForeignPortInfo, consol.JK_DateLastForeignPortInfo, consol.JK_DateLastForeignPortInfo, Res.GetString("f0f1a576-374f-4c53-941a-c0d4f2ec8bbe", "Last foreign port"), context);

				XsdCustomEntryNumbersObjectHelper.ImportFromXsdCustomsEntryNumberCollection(value.ConsolDetail.CustomsEntryNumbers, consol.PK, JobConsolSchema.Constants.TableName, () => consol.CusEntryNums, context);

				ImportConsolAddresses(consol, value, context);

				ImportReferenceNumbers(consol, value, context);
			}

			ImportAddresses(consol, value, context);

			NoteValueObjectDataAdapter noteAdapter = new NoteValueObjectDataAdapter();
			noteAdapter.ImportNotesAndAttachToBusinessObjectNotes(consol.Notes, value.Notes, context);
			ImporteDocs(consol, value.Documents, context);

			AddImportEvent(consol);
		}

		protected virtual void ImportReferenceNumbers(CommonConsol consol, Xsd.Consol value, IValueObjectImportContext context)
		{
			ReferenceNumberDataAdapter.ImportReferenceNumbers(consol.Numbers, value.ConsolDetail.ReferenceNumbers, context);
		}

		static bool QueryUpdateRoutingInformation(CommonConsol consol, IValueObjectImportContext context)
		{
			QueryUserYesNoYesAllNoAllEventArgs args = new QueryUserYesNoYesAllNoAllEventArgs();
			args.Message = Res.GetString("633c21ab-a616-4da6-b67d-8f602c77162f", "Is it OK to update the routing information for {0}??", consol.HumanReadableName);

			context.QueryUser(args);

			return args.Response;
		}

		void ImportRoutingInformation(TBusinessObject consol, TValueObject value, IValueObjectImportContext context, string errorContext)
		{
			bool canUpdateRouting;

			if (!consol.IsInDatabase)
			{
				canUpdateRouting = true;
			}
			else
			{
				canUpdateRouting = Env.CurrentUser.IsBatchProcessor ? SystemRegistry.UpdateConsolsRoutingInformationDuringAutomaticImport.Value : QueryUpdateRoutingInformation(consol, context);
			}

			if (canUpdateRouting)
			{
				if (value.ConsolDetail.PlannedLegs.Count > 0 || !consol.IsInDatabase || ImportAllElementsEvenIfNotSpecified)
				{
					InferPlannedLegIfMissing(value);
					XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, value.ConsolDetail.PlannedLegs, context, errorContext);
				}

				ImportLoadDischargePorts(consol, value, context, errorContext);
			}
		}

		void ImportChildObjectsOnly(TBusinessObject consol, Xsd.Consol value, IValueObjectImportContext context)
		{
			string errorContext = Res.GetString("1aee9e58-13e0-409f-b204-65f70a2d8fb2", "Master bill {0}", consol.JK_MasterBillNum);

			bool canUpdateRouting = Env.CurrentUser.IsBatchProcessor ? SystemRegistry.UpdateConsolsRoutingInformationDuringAutomaticImport.Value : QueryUpdateRoutingInformation(consol, context);

			if (canUpdateRouting)
			{
				XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, value.ConsolDetail.PlannedLegs, context, errorContext);
			}

			ImportConsolContainers(consol, value, context);
			ImportConsolShipmentsAndDeclarations(consol, value, context);
		}

		protected virtual void SetForwarders(TBusinessObject consol, Xsd.Consol value, IValueObjectImportContext context)
		{
			var unmatchOrgRecordCriteria = new UnmatchOrgRecordCriteria();
			if (value.ConsolDetail.SendingAgent.IsSpecified || ImportAllElementsEvenIfNotSpecified)
			{
				unmatchOrgRecordCriteria.OrganisationSubType = OrganisationsSubTypeList.Descriptions.SendingForwarder;
				consol.SetDefaultSendingForwarderAddress(context.FindOrCreateTempOrganisationPK(value.ConsolDetail.SendingAgent, consol, OrganisationTypes.Forwarder, unmatchOrgRecordCriteria));
			}

			if (value.ConsolDetail.ReceivingAgent.IsSpecified || ImportAllElementsEvenIfNotSpecified)
			{
				unmatchOrgRecordCriteria.OrganisationSubType = OrganisationsSubTypeList.Descriptions.ReceivingForwarder;
				consol.SetDefaultReceivingForwarderAddress(context.FindOrCreateTempOrganisationPK(value.ConsolDetail.ReceivingAgent, consol, OrganisationTypes.Forwarder, unmatchOrgRecordCriteria));
			}
		}

		void ImportMasterBill(CommonConsol consol, Xsd.Consol consolValue, IValueObjectImportContext context)
		{
			string masterBill = GetMasterBillFromValueObject(consolValue);
			if (masterBill != null)
			{
				context.Notify(new InfoNotification(Res.GetString("8609455b-7e65-457a-97b9-272d0d796c74", "Importing consol with Master Bill '{0}'", masterBill)));
				consol.JK_IsNeutralMaster = false;
				context.SetPropertyInfoValueIfValueNotEmpty(consol.JK_MasterBillNumInfo, masterBill);
			}
		}

		void InferPlannedLegIfMissing(Xsd.Consol consolValue)
		{
			if (consolValue.ConsolDetail.PlannedLegs.Count == 0)
			{
				ZString transportMode = ConsolTransportModeToXmlCodeMappings.Instance.GetEnterpriseCode(consolValue.ConsolDetail.TransportMode, "", null);

				Xsd.PlannedLeg plannedLeg = consolValue.ConsolDetail.PlannedLegs.AddNew();
				plannedLeg.TransportMode = TransportModeToXmlCodeMappings.Instance.GetExternalCode(transportMode, "", null);
				plannedLeg.PortOfLoading = consolValue.ConsolDetail.PortOfLoading;
				plannedLeg.PortOfDischarge = consolValue.ConsolDetail.PortOfDischarge;
				plannedLeg.Item = consolValue.ConsolDetail.Item;
			}
		}

		void ImportLoadDischargePorts(CommonConsol consol, Xsd.Consol value, IValueObjectImportContext context, string errorContext)
		{
			if (value.ConsolDetail.PortOfLoading.Port.IsSpecified || ImportAllElementsEvenIfNotSpecified)
			{
				context.SetPropertyInfoValue(consol.JK_RL_NKLoadPortInfo, value.ConsolDetail.PortOfLoading.Port.Value, ForeignKeyType.PortNK);
			}

			if (value.ConsolDetail.PortOfDischarge.Port.IsSpecified || ImportAllElementsEvenIfNotSpecified)
			{
				context.SetPropertyInfoValue(consol.JK_RL_NKDischargePortInfo, value.ConsolDetail.PortOfDischarge.Port.Value, ForeignKeyType.PortNK);
			}
		}

		void ImportConsolContainers(CommonConsol consol, Xsd.Consol consolValue, IValueObjectImportContext context)
		{
			if (consolValue.ConsolDetail.Containers != null)
			{
				var adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

				foreach (Xsd.Container containerValue in consolValue.ConsolDetail.Containers)
				{
					adapter.CreateOrUpdateFromValueObject(containerValue, context);
				}
			}
		}

		protected void ImportConsolShipmentsAndDeclarations(TBusinessObject consol, Xsd.Consol consolValue, IValueObjectImportContext context)
		{
			if (consolValue.Shipments != null)
			{
				Dictionary<ZGuid, TShipment> importedShipments = new Dictionary<ZGuid, TShipment>();
				Dictionary<ZGuid, Xsd.Shipment> importedShipmentValues = new Dictionary<ZGuid, Xsd.Shipment>();

				var shipmentAdapter = GetNewShipmentValueObjectDataAdapter(consol);
				var shipmentLimitHelper = new ShipmentsOnConsolLimitHelper(consol);

				foreach (Xsd.Shipment shipmentValue in consolValue.Shipments)
				{
					if (ShouldTryToMatchAndConvertQuickBookingBeforeImportingShipment)
					{
						TShipment matchedShipment = shipmentAdapter.FindShipment(shipmentValue, context);
						if (matchedShipment == null)
						{
							FindAndConvertQuickBookingIfPossible(consol, shipmentValue, context);
						}
					}

					TShipment shipment = shipmentAdapter.CreateOrUpdateFromValueObject(shipmentValue, context);

					if (shipment != null)
					{
						CheckShipmentLimitNotExceeded(shipmentLimitHelper, context);

						if (importedShipments.ContainsKey(shipment.PK))
						{
							importedShipments[shipment.PK] = shipment;
							importedShipmentValues[shipment.PK] = shipmentValue;
						}
						else
						{
							importedShipments.Add(shipment.PK, shipment);
							importedShipmentValues.Add(shipment.PK, shipmentValue);
						}
					}
				}

				foreach (Xsd.Shipment shipmentValue in consolValue.Shipments)
				{
					ZGuid shipmentKey = (from s in importedShipmentValues
										 where s.Value == shipmentValue
										 select s.Key).FirstOrDefault();

					if (importedShipments.ContainsKey(shipmentKey))
					{
						TShipment shipment = importedShipments[shipmentKey];
						shipmentAdapter.ImportCoLoadMaster(shipment, shipmentValue, context);
					}
				}

				foreach (ZGuid pk in importedShipments.Keys)
				{
					GenerateDeclarationForShipment(importedShipmentValues[pk], importedShipments[pk], context);
				}
			}
		}

		void CheckShipmentLimitNotExceeded(ShipmentsOnConsolLimitHelper shipmentLimitHelper, IValueObjectImportContext context)
		{
			var notification = shipmentLimitHelper.CreateNotification();

			if (notification != null && notification.Type == CargoWise.ComponentModel.NotificationType.Error)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, notification.Message));
			}
		}

		protected virtual void TryToMatchAndConvertQuickBookingBeforeImportingShipment(TBusinessObject consol, ShipmentValueObjectDataAdapter<TShipment> shipmentAdapter, Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
		}

		protected virtual bool ShouldTryToMatchAndConvertQuickBookingBeforeImportingShipment
		{
			get { return true; }
		}

		protected virtual void GenerateDeclarationForShipment(Xsd.Shipment shipmentValue, CommonShipment shipment, IValueObjectImportContext context)
		{
		}

		void ImportConsolAddresses(CommonConsol consol, Xsd.Consol consolValue, IValueObjectImportContext context)
		{
			BusinessObjectFactory factory = consol.Factory;
			if (consolValue.ConsolDetail.Arrival != null)
			{
				if (consolValue.ConsolDetail.Arrival.CTO.IsSpecified || ImportAllElementsEvenIfNotSpecified)
				{
					consol.JK_OA_ArrivalCTOAddress = new AddressValueObjectHelper(Res.GetString("6405163e-1209-4d03-8a51-ad6c9dee2822", "Arrival CTO on master bill {0}", consol.JK_MasterBillNum)).FromAddressReferenceGetAddressPK(consolValue.ConsolDetail.Arrival.CTO, context);
				}

				if (consolValue.ConsolDetail.Arrival.Depot.IsSpecified || ImportAllElementsEvenIfNotSpecified)
				{
					consol.JK_OA_UnpackDepotAddress = new AddressValueObjectHelper(Res.GetString("c092a2cc-225a-422c-a8f3-ed0164b36717", "Unpack Depot on master bill {0}", consol.JK_MasterBillNum)).FromAddressReferenceGetAddressPK(consolValue.ConsolDetail.Arrival.Depot, context);
				}

				if (consolValue.ConsolDetail.Arrival.ContainerYard.IsSpecified || ImportAllElementsEvenIfNotSpecified)
				{
					consol.JK_OA_ContainerYardEmptyReturnAddress = new AddressValueObjectHelper(Res.GetString("94301a06-94bf-44d1-a806-d5a732eb1171", "Container Yard Empty Return Address on master bill {0}", consol.JK_MasterBillNum)).FromAddressReferenceGetAddressPK(consolValue.ConsolDetail.Arrival.ContainerYard, context);
				}
			}

			if (consolValue.ConsolDetail.Departure != null)
			{
				if (SystemDataRegistry.Instance.AllowDepartureCTOAddressImport.Value &&
						(consolValue.ConsolDetail.Departure.CTO.IsSpecified || ImportAllElementsEvenIfNotSpecified))
				{
					consol.JK_OA_DepartureCTOAddress = new AddressValueObjectHelper(Res.GetString("01be350a-e04b-4830-96b8-efad11430a71", "Departure CTO on master bill {0}", consol.JK_MasterBillNum)).FromAddressReferenceGetAddressPK(consolValue.ConsolDetail.Departure.CTO, context);
				}

				if (SystemDataRegistry.Instance.AllowDepartureDepotAddressImport.Value &&
						(consolValue.ConsolDetail.Departure.Depot.IsSpecified || ImportAllElementsEvenIfNotSpecified))
				{
					consol.JK_OA_PackDepotAddress = new AddressValueObjectHelper(Res.GetString("8c55f84d-266c-48e1-be15-edadff04aefa", "Pack Depot on master bill {0}", consol.JK_MasterBillNum)).FromAddressReferenceGetAddressPK(consolValue.ConsolDetail.Departure.Depot, context);
				}

				if (SystemDataRegistry.Instance.AllowDepartureContainerYardAddressImport.Value &&
						(consolValue.ConsolDetail.Departure.ContainerYard.IsSpecified || ImportAllElementsEvenIfNotSpecified))
				{
					consol.JK_OA_ContainerYardEmptyPickupAddress = new AddressValueObjectHelper(Res.GetString("33cc02c4-eeae-43ad-a232-7cd00fd07be1", "Container Yard Pickup on master bill {0}", consol.JK_MasterBillNum)).FromAddressReferenceGetAddressPK(consolValue.ConsolDetail.Departure.ContainerYard, context);
				}
			}
		}

		protected virtual void ImportAddresses(TBusinessObject consol, Xsd.Consol consolValue, IValueObjectImportContext context)
		{
		}

		#region Quick Booking Conversion

		void FindAndConvertQuickBookingIfPossible(TBusinessObject consol, Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			CommonShipment booking = FindBooking(shipmentValue, context);
			if (booking != null && IsQuickBooking(booking))
			{
				ConvertQuickBooking(consol, booking);
			}
		}

		CommonShipment FindBooking(Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			ZQuery bookingQuery = new ZQuery(JobShipmentSchema.JS_IsBooking, true);
			bookingQuery.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, false);
			bookingQuery.AddToFilter(JobShipmentSchema.JS_IsDirectBooking, false);

			var locator = new ShipmentLocator<CommonShipment>(context.Factory, bookingQuery);
			return locator.Find(shipmentValue);
		}

		bool IsQuickBooking(CommonShipment booking)
		{
			if (booking != null)
			{
				ZQuery quickBookingQuery = new ZQuery(ViewQuotedBookingSchema.VB_JS, booking.PK);
				quickBookingQuery.AddToFilter(ViewQuotedBookingSchema.VB_TH, null);

				Type viewQuotedBookingType = ObjectFactory.GetType<Integration.QuotedBooking.IViewQuotedBooking>();
				return booking.Factory.LoadTop1(viewQuotedBookingType, quickBookingQuery) != null;
			}

			return false;
		}

		void ConvertQuickBooking(TBusinessObject consol, CommonShipment booking)
		{
			BuildConsolHelper consolHelper = new BuildConsolHelper();
			consolHelper.AddBookingsToConsol(consol, new ZGuid[] { booking.PK });
		}

		#endregion

		#endregion

		#region Export

		protected virtual void ExportContainers(TBusinessObject consol, Xsd.Consol consolValue, IValueObjectExportContext context)
		{
			if (consol.Containers.Count > 0)
			{
				var adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

				consolValue.ConsolDetail.Containers = new Xsd.ContainerCollection();
				foreach (CommonContainer container in consol.Containers)
				{
					consolValue.ConsolDetail.Containers.Add(adapter.ExportToValueObject(container, context));
				}
			}
		}

		protected virtual void ExportShipments(TBusinessObject consol, Xsd.Consol consolValue, IValueObjectExportContext context)
		{
			if (consol.Shipments.Count > 0)
			{
				consolValue.Shipments = new Xsd.ShipmentCollection();
				foreach (TShipment shipment in consol.Shipments)
				{
					consolValue.Shipments.Add(GetNewShipmentValueObjectDataAdapter(null).ExportToValueObject(shipment, context));
				}
			}
		}

		protected virtual ShipmentValueObjectDataAdapter<TShipment> GetNewShipmentValueObjectDataAdapter(TBusinessObject existingConsol)
		{
			return new ShipmentValueObjectDataAdapter<TShipment>(existingConsol, TriggeredByEvents);
		}

		protected virtual void ExportAddresses(TBusinessObject consol, Xsd.Consol consolValue, IValueObjectExportContext context)
		{
		}

		protected override void ExportToValueObjectCore(TBusinessObject consol, TValueObject result, IValueObjectExportContext context)
		{
			result.ConsolDetail = new Xsd.ConsolConsolDetail();
			string errorContext = Res.GetString("a0c65081-d160-423a-8ff0-f519316baa0d", "Master bill {0}", consol.JK_MasterBillNum);

			if (consol.Logs.CreatedDateUtc.IsValid)
			{
				result.ConsolDetail.DateCreated = consol.Logs.CreatedDateUtc.ToDateTime();
			}

			if (IncludeeDocs)
			{
				result.Documents = ExportStorageDocs(consol, context);
			}

			result.Events = StmALogValueObjectDataAdapter.New(consol, errorContext, TriggeredByEvents).ToXmlCollectionValueObject(context);

			ExportMasterBill(consol, result);

			result.ConsolDetail.SendingAgent = GetNewOrganisationValueObjectDataAdapter(consol).ExportToValueObject(consol.SendingForwarder, context);
			result.ConsolDetail.ReceivingAgent = GetNewOrganisationValueObjectDataAdapter(consol).ExportToValueObject(consol.ReceivingForwarder, context);
			result.ConsolDetail.Carrier = GetNewOrganisationValueObjectDataAdapter(consol).ExportToValueObject(consol.ShippingLine, context);
			result.ConsolDetail.Creditor = GetNewOrganisationValueObjectDataAdapter(consol).ExportToValueObject(consol.Creditor, context);

			result.ConsolDetail.AgentReference = consol.JK_UniqueConsignRef;
			result.ConsolDetail.ExternalAgentReference = consol.JK_AgentsReference;
			result.ConsolDetail.BookingReference = consol.JK_BookingReference.IsEmpty ? null : consol.JK_BookingReference;

			result.ConsolDetail.ConsolType = AgentTypeToXmlCodeMappings.Instance.GetExternalCode(consol.JK_AgentType, errorContext, context);
			result.ConsolDetail.ConsolTypeSpecified = true;
			result.ConsolDetail.ContainerMode = ContainerModeToXmlCodeMappings.Instance.GetExternalCode(consol.JK_ConsolMode, errorContext, context);
			result.ConsolDetail.ContainerModeSpecified = true;
			result.ConsolDetail.TransportMode = ConsolTransportModeToXmlCodeMappings.Instance.GetExternalCode(consol.JK_TransportMode, errorContext, context);
			result.ConsolDetail.TransportModeSpecified = true;
			result.ConsolDetail.PaymentType = PrepaidCollectToXmlCodeMappings.Instance.GetExternalCode(consol.JK_PrepaidCollect, errorContext, context);
			result.ConsolDetail.PaymentTypeSpecified = true;

			if (!consol.JK_ReleaseType.IsEmpty)
			{
				result.ConsolDetail.ReleaseType = consol.JK_ReleaseType;
				result.ConsolDetail.ReleaseTypeSpecified = true;
			}

			result.ConsolDetail.NumberOfOriginalBills = consol.JK_NoOriginalBills;
			result.ConsolDetail.NumberOfOriginalBillsSpecified = true;
			result.ConsolDetail.NumberOfCopyBills = consol.JK_NoCopyBills;
			result.ConsolDetail.NumberOfCopyBillsSpecified = true;

			if (consol.JK_MasterBillIssueDate.IsValid)
			{
				result.ConsolDetail.MasterBillIssueDate = consol.JK_MasterBillIssueDate;
			}

			ExportConsolAddresses(consol, result, context, errorContext);
			ExportPortsAndDates(consol, result, context);
			result.ConsolDetail.PlannedLegs = ExportPlannedLegs(consol, context, errorContext);
			result.ConsolDetail.Item = ExportSailing(consol, context);
			ExportContainers(consol, result, context);
			ExportShipments(consol, result, context);
			ExportAddresses(consol, result, context);

			ReferenceNumberDataAdapter.ExportReferenceNumbers(consol.Numbers, result.ConsolDetail.ReferenceNumbers, context);

			OnAfterExportFromValueObjectCore(consol, result, context);

			result.ConsolDetail.CustomsEntryNumbers = XsdCustomEntryNumbersObjectHelper.ExportFromCusEntryNumCollection(consol.CusEntryNumsForAllCountries);
			result.Notes = new NoteValueObjectDataAdapter().ExportToXmlValueObjectCollection(consol.Notes, context);

			AddExportEvent(result, consol, context, ExportEventReference);
		}

		protected virtual void OnAfterExportFromValueObjectCore(TBusinessObject consol, Xsd.Consol value, IValueObjectExportContext context)
		{
		}

		protected virtual ZString ExportEventReference
		{
			get { return ZString.Empty; }
		}

		void ExportMasterBill(CommonConsol consol, Xsd.Consol consolValue)
		{
			consolValue.ConsolIdentifier = new Xsd.ConsolIdentifierCollection();
			Xsd.ConsolIdentifier identifier = consolValue.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.ConsolIdentifierTypeSpecified = true;
			identifier.Value = consol.JK_MasterBillNum;
		}

		void ExportConsolAddresses(TBusinessObject consol, Xsd.Consol consolValue, IValueObjectExportContext context, string errorContext)
		{
			consolValue.ConsolDetail.Arrival = new Xsd.PortInfo();
			consolValue.ConsolDetail.Arrival.CTO = new AddressValueObjectHelper(Res.GetString("18fa6881-bd1d-4a5f-b2ab-4e5ce50a863a", "Arrival CTO on master bill {0}", consol.JK_MasterBillNum)).ToAddressReference(consol.ArrivalCTOAddress, context);
			consolValue.ConsolDetail.Arrival.Depot = new AddressValueObjectHelper(Res.GetString("687e1945-9cfe-43c9-b8d8-730764b1c916", "Unpack Depot on master bill {0}", consol.JK_MasterBillNum)).ToAddressReference(consol.UnpackDepotAddress, context);
			consolValue.ConsolDetail.Arrival.ContainerYard = new AddressValueObjectHelper(Res.GetString("582d33f5-2260-4f8f-96ec-55d973a47622", "Container Yard Empty Return Address on master bill {0}", consol.JK_MasterBillNum)).ToAddressReference(consol.ContainerYardEmptyReturnAddress, context);

			consolValue.ConsolDetail.Departure = new Xsd.PortInfo();
			consolValue.ConsolDetail.Departure.CTO = new AddressValueObjectHelper(Res.GetString("c96dfab6-582c-44a4-8ea8-6339c52e970f", "Departure CTO on master bill {0}", consol.JK_MasterBillNum)).ToAddressReference(consol.DepartureCTOAddress, context);
			consolValue.ConsolDetail.Departure.Depot = new AddressValueObjectHelper(Res.GetString("28c03450-231c-4127-9228-0bce147184b0", "Pack Depot on master bill {0}", consol.JK_MasterBillNum)).ToAddressReference(consol.PackDepotAddress, context);
			consolValue.ConsolDetail.Departure.ContainerYard = new AddressValueObjectHelper(Res.GetString("6024199e-b9d9-471b-9cc2-ddb90c148b1d", "Container Yard Pickup on master bill {0}", consol.JK_MasterBillNum)).ToAddressReference(consol.ContainerYardEmptyPickupAddress, context);
		}

		void ExportPortsAndDates(TBusinessObject consol, Xsd.Consol consolValue, IValueObjectExportContext context)
		{
			consolValue.ConsolDetail.PortOfLoading = XsdMovement.FromPortEstimatedActualDates(consol.Factory, consol.JK_RL_NKLoadPort, consol.Transports.DepartureTransport.JW_ETD, consol.Transports.DepartureTransport.JW_ATD);
			consolValue.ConsolDetail.PortOfDischarge = XsdMovement.FromPortEstimatedActualDates(consol.Factory, consol.JK_RL_NKDischargePort, consol.Transports.ArrivalTransport.JW_ETA, consol.Transports.ArrivalTransport.JW_ATA);
			consolValue.ConsolDetail.PortFirstArrival = XsdMovement.FromPortEstimatedActualDates(consol.Factory, consol.JK_RL_NKPortOfFirstArrival, consol.JK_DatePortOfFirstArrival, consol.JK_DatePortOfFirstArrival);
			consolValue.ConsolDetail.PortFirstForeign = XsdMovement.FromPortEstimatedActualDates(consol.Factory, consol.JK_RL_NKFirstForeignPort, consol.JK_DateFirstForeignPort, consol.JK_DateFirstForeignPort);
			consolValue.ConsolDetail.PortLastForeign = XsdMovement.FromPortEstimatedActualDates(consol.Factory, consol.JK_RL_NKLastForeignPort, consol.JK_DateLastForeignPort, consol.JK_DateLastForeignPort);
		}

		Xsd.PlannedLegCollection ExportPlannedLegs(TBusinessObject consol, IValueObjectExportContext context, string errorContext)
		{
			Xsd.PlannedLegCollection result = new Xsd.PlannedLegCollection();
			foreach (Transport transport in consol.Transports)
			{
				Xsd.PlannedLeg plannedLeg = result.AddNew();
				XsdPlannedLegObjectHelper.ExportPlannedLeg(plannedLeg, transport, context, errorContext);
			}
			return result;
		}

		Xsd.SailingBase ExportSailing(TBusinessObject consol, IValueObjectExportContext context)
		{
			Transport mostInterestingTransport = consol.Transports.MostInterestingTransport;

			Xsd.SailingBase sailingValue = null;

			if (consol.JK_TransportMode == Core.Constants.TransportModes.Sea)
			{
				Xsd.SailingWithVesselVoyage vesselDetails = new Xsd.SailingWithVesselVoyage();
				sailingValue = vesselDetails;
				vesselDetails.LCLDates.IsSpecified = false;
				vesselDetails.FCLDates.IsSpecified = false;
				vesselDetails.VesselName = mostInterestingTransport.JW_Vessel.IsEmpty ? null : (string)mostInterestingTransport.JW_Vessel;
				vesselDetails.LloydsNo = (mostInterestingTransport.Vessel == null) ? null : (string)mostInterestingTransport.Vessel.RV_LloydsNumber;
				vesselDetails.CargoCarrierCode = (mostInterestingTransport.Vessel == null) ? ZString.Empty : mostInterestingTransport.Vessel.RV_CarrierCode;
				vesselDetails.VoyageNo = mostInterestingTransport.JW_VoyageFlight.IsEmpty ? null : (string)mostInterestingTransport.JW_VoyageFlight;

				if (mostInterestingTransport.JW_IsLinked && mostInterestingTransport.Carrier != null)
				{
					vesselDetails.Carrier = GetNewOrganisationValueObjectDataAdapter(consol).ExportToValueObject(mostInterestingTransport.Carrier, context);
				}
			}
			else
			{
				Xsd.FlightWithFlightNumber flightDetails = new Xsd.FlightWithFlightNumber();
				sailingValue = flightDetails;
				flightDetails.Dates = null;
				flightDetails.FlightNoJourneyNoTruckRegNo = mostInterestingTransport.JW_VoyageFlight.IsEmpty ? null : (string)mostInterestingTransport.JW_VoyageFlight;
			}

			if (mostInterestingTransport.Sailing != null)
			{
				SailingValueObjectDataAdapter.RunExport(mostInterestingTransport.Sailing, sailingValue, context);
			}
			else
			{
				sailingValue.ETD = mostInterestingTransport.JW_ETD;
				sailingValue.ETA = mostInterestingTransport.JW_ETA;
				sailingValue.ATD = mostInterestingTransport.JW_ATD;
				sailingValue.ATA = mostInterestingTransport.JW_ATA;
			}

			return sailingValue;
		}

		Xsd.DocumentCollection ExportStorageDocs(CommonConsol consol, IValueObjectExportContext context)
		{
			Xsd.DocumentCollection documents = new Xsd.DocumentCollection();

			Type adapterTypeToCreate1 = TypeDecider.GetTypeForBinding(ObjectFactory.GetType<IStorageDocsValueObjectDataAdapter>());
			Type adapterTypeToCreate2 = TypeDecider.GetTypeForBinding(ObjectFactory.GetType<IStorageFilesValueObjectDataAdapter>());
			IValueObjectDataAdapter storageDocsDataAdapter = (IValueObjectDataAdapter)Activator.CreateInstance(adapterTypeToCreate1);
			IValueObjectDataAdapter storageFilesDataAdapter = (IValueObjectDataAdapter)Activator.CreateInstance(adapterTypeToCreate2);
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			IStorageMainForPK documentFactory = (IStorageMainForPK)documentFactoryProvider.GetFactory(consol.Factory);
			IDocumentsView storageMain = documentFactory.GetStorageMain(consol.PK);

			if (storageMain != null)
			{
				foreach (BusinessObject storageDoc in storageMain.DocumentCollectionView)
				{
					documents.Add((Xsd.Document)storageDocsDataAdapter.ExportToValueObject(storageDoc, context));
				}
				foreach (BusinessObject storageDoc in storageMain.PDFFilesCollectionView)
				{
					documents.Add((Xsd.Document)storageFilesDataAdapter.ExportToValueObject(storageDoc, context));
				}
			}

			return documents;
		}

		bool IncludeeDocs
		{
			get { return SystemDataRegistry.Instance.IncludeConsoleDocs.Value; }
		}

		#endregion

		#region Use registry defaults and check child objects

		protected override bool ShouldUpdateExistingObject(TBusinessObject consol, INotifications notifications)
		{
			if (IsBatchJob)
			{
				switch (consol.JK_TransportMode)
				{
					case Core.Constants.TransportModes.Air:
						return RegistryDefaultForImportingAir;
					case Core.Constants.TransportModes.Sea:
						return RegistryDefaultForImportingSea;
					default:
						return RegistryDefaultForImporting;
				}
			}
			else
			{
				return base.ShouldUpdateExistingObject(consol, notifications);
			}
		}

		protected override bool RegistryDefaultForImporting
		{
			get { return SystemRegistry.UpdateConsolDuringAutomaticImportOther.Value; }
		}

		protected virtual bool RegistryDefaultForImportingAir
		{
			get { return SystemRegistry.UpdateConsolDuringAutomaticImportAir.Value; }
		}

		protected virtual bool RegistryDefaultForImportingSea
		{
			get { return SystemRegistry.UpdateConsolDuringAutomaticImportSea.Value; }
		}

		bool AlwaysCheckForSailingShipmentsAndContainers
		{
			get { return SystemRegistry.AlwaysCheckForConsolSailingShipmentsAndContainers.Value; }
		}

		#endregion
	}
}
