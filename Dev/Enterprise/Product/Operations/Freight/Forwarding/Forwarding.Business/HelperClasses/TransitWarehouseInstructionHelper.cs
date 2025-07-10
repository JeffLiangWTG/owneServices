using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using EventConstants = CargoWise.EventReference.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface ITransitWarehouseInstructionSupporter : IWorkflowProvider, IFactoryProvider, IBusinessObjectState
	{
		OrgAddress PickupTransitWarehouse { get; }
		OrgAddress DeliveryTransitWarehouse { get; }

		ZDateTime PickupReceiptRequestedDate { get; set; }
		ZDateTime PickupDispatchRequestedDate { get; set; }
		ZDateTime DeliveryReceiptRequestedDate { get; set; }
		ZDateTime DeliveryDispatchRequestedDate { get; set; }

		ZString PickupDescription { get; }
		ZString DeliveryDescription { get; }
		ZString TransitWarehouseDescription { get; }
	}

	#region Helper

	public class TransitWarehouseInstructionHelper
	{
		public TransitWarehouseInstructionHelper(ITransitWarehouseInstructionSupporter supporter, INotifications notificationsParent)
		{
			this.supporter = supporter;
			this.notificationsParent = notificationsParent;
		}

		public TransitWarehouseInstructionHelper(
			ITransitWarehouseInstructionSupporter supporter,
			INotifications notificationsParent,
			Func<BusinessObjectFactory, ManualDataExport> getManualDataExportForPrepareDispatchInstruction,
			ITransitWarehouseInstructionSupporter[] supportersForLogs
		)
		{
			this.supporter = supporter;
			this.notificationsParent = notificationsParent;
			this.getManualDataExportForPrepareDispatchInstruction = getManualDataExportForPrepareDispatchInstruction;
			this.supportersForLogs = supportersForLogs;
		}

		readonly ITransitWarehouseInstructionSupporter supporter;

		readonly ITransitWarehouseInstructionSupporter[] supportersForLogs;

		readonly INotifications notificationsParent;

		readonly Func<BusinessObjectFactory, ManualDataExport> getManualDataExportForPrepareDispatchInstruction;

		#region Enums

		public enum Direction
		{
			Pickup,
			Delivery,
			Both
		}

		public enum ServiceRequest
		{
			Receipt,
			Dispatch,
			ReceiveAndDispatch,
			PrepareDispatch,
		}

		public enum SupporterType
		{
			Shipment,
			Consol
		}

		public enum TransitUniversalServiceResult
		{
			Succeeded,
			Failed,
			Queued
		}

		#endregion

		#region SendTransitWarehouseInstruction

		public void SendTransitWarehouseInstruction(Direction direction, ServiceRequest serviceRequest)
		{
			// Disable data refresh to improve performance when Sending Transit Warehouse Instruction 
			var factory = new BusinessObjectFactory { NameForDebugging = "Send Transit Warehouse Instruction", RefreshEnabled = false };
			using (factory.AddDisposableService())
			using (var notifier = new TransitWarehouseInstructionNotifier(notificationsParent))
			{
				var supporterBOInNewFactory = factory.ImportFromAnotherFactory(supporter as BusinessObject) as ITransitWarehouseInstructionSupporter;
				var supportersForLogsBOInNewFactory = supportersForLogs?
					.Select(s => factory.ImportFromAnotherFactory(s as BusinessObject) as ITransitWarehouseInstructionSupporter)
					.ToArray();
				if (SendTransitWarehouseInstructionCore(notifier, direction, serviceRequest, factory))
				{
					var instructionDate = ZDateTimeOffset.Now;
					SetInstructionDateRequested(direction, serviceRequest, instructionDate.ToZDateTime(), supporter);
					ProcessReceiptDispatchLog(direction, serviceRequest, instructionDate, supportersForLogsBOInNewFactory, supporter);

					factory.Save();
				}
			}
			(supporter as BusinessObject).Factory.Save();
		}

		public void SendTransitWarehouseInstructionInLogSubscriber(Direction direction, ServiceRequest serviceRequest, BusinessObjectFactory factoryInLogSubscriber, TimeSpan offset)
		{
			using (var notifier = new TransitWarehouseInstructionNotifier(notificationsParent))
			{
				if (SendTransitWarehouseInstructionCore(notifier, direction, serviceRequest, factoryInLogSubscriber))
				{
					var instructionDate = DateTimeOffset.Now.ToOffset(offset);
					SetInstructionDateRequested(direction, serviceRequest, instructionDate.DateTime, supporter);
					ProcessReceiptDispatchLog(direction, serviceRequest, instructionDate, supportersForLogs, supporter);

					factoryInLogSubscriber.Save();
				}
			}
			(supporter as BusinessObject).Factory.Save();
		}

		static void ProcessReceiptDispatchLog(Direction direction, ServiceRequest serviceRequest, ZDateTimeOffset instructionDate, IEnumerable<ITransitWarehouseInstructionSupporter> supportersForLogs, ITransitWarehouseInstructionSupporter supporter)
		{
			string[] logTypes;
			Direction[] directions;

			switch (serviceRequest)
			{
				case ServiceRequest.Dispatch:
					logTypes = new string[] { nameof(ServiceRequest.Dispatch) };
					break;

				case ServiceRequest.Receipt:
					logTypes = new string[] { nameof(ServiceRequest.Receipt) };
					break;

				case ServiceRequest.ReceiveAndDispatch:
					logTypes = new string[] { nameof(ServiceRequest.Receipt), nameof(ServiceRequest.Dispatch) };
					break;

				case ServiceRequest.PrepareDispatch:
					logTypes = new string[] { (NoResString)"Prepare Dispatch" };
					break;

				default:
					logTypes = Array.Empty<string>();
					break;
			}

			switch (direction)
			{
				case Direction.Both:
					directions = new[] { Direction.Pickup, Direction.Delivery };
					break;

				case Direction.Delivery:
				case Direction.Pickup:
				default:
					directions = new[] { direction };
					break;
			}

			foreach (var type in logTypes)
			{
				foreach (var directionType in directions)
				{
					GenerateLog(directionType, serviceRequest, instructionDate, type, supportersForLogs, supporter);
				}
			}
		}

		static void GenerateLog(Direction direction, ServiceRequest serviceRequest, ZDateTimeOffset instructionDate, string type, IEnumerable<ITransitWarehouseInstructionSupporter> supportersForLogs, ITransitWarehouseInstructionSupporter supporter)
		{
			var parameters = serviceRequest == ServiceRequest.PrepareDispatch
				? GetParametersForPrepareDispatch(type, direction, supporter)
				: GetParametersForReceiveDispatch(type, direction, supporter);

			foreach (var log in GetReceiptServiceLogs(instructionDate, supportersForLogs, supporter))
			{
				using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
				{
					foreach (var parameter in parameters)
					{
						log.Parameters[parameter.Key] = parameter.Value;
					}
				}
			}
		}

		static IEnumerable<StmALog> GetReceiptServiceLogs(ZDateTimeOffset instructionDate, IEnumerable<ITransitWarehouseInstructionSupporter> supportersForLogs, ITransitWarehouseInstructionSupporter supporter)
		{
			if (supportersForLogs != null)
			{
				return supportersForLogs.WhereNotNull().Select(child => child.Logs.AddNew(AutoEvents.ServiceRequested, instructionDate));
			}

			if (supporter != null)
			{
				return new StmALog[] { supporter.Logs.AddNew(AutoEvents.ServiceRequested, instructionDate) };
			}

			return Enumerable.Empty<StmALog>();
		}

		static Dictionary<string, string> GetParametersForReceiveDispatch(string type, Direction direction, ITransitWarehouseInstructionSupporter supporter)
		{
			var parameters = new Dictionary<string, string>();
			parameters[Params.Type] = type;
			parameters[Params.Facility] = EventConstants.Facilities.Code.Depot;
			parameters[Params.Location] = GetTransitWarehouseAddress(supporter, direction)?.OA_RL_NKRelatedPortCode ?? ZString.Empty;

			return parameters;
		}

		static Dictionary<string, string> GetParametersForPrepareDispatch(string type, Direction direction, ITransitWarehouseInstructionSupporter supporter)
		{
			var transitWarehouse = GetTransitWarehouseAddress(supporter, direction);

			var parameters = new Dictionary<string, string>();
			parameters[Params.Type] = type;
			parameters[Params.Facility] = EventConstants.Facilities.Code.Depot;
			parameters[Params.Location] = transitWarehouse?.OA_RL_NKRelatedPortCode ?? ZString.Empty;
			parameters[Params.Warehouse] = transitWarehouse?.Header?.OH_Code ?? ZString.Empty;

			return parameters;
		}

		bool SendTransitWarehouseInstructionCore(TransitWarehouseInstructionNotifier notifier, Direction direction, ServiceRequest serviceRequest, BusinessObjectFactory factory)
		{
			if (ValidateCanSendTransitWarehouseInstruction(notifier, direction, serviceRequest))
			{
				if (serviceRequest == ServiceRequest.ReceiveAndDispatch)
				{
					InnerSendTransitWarehouseInstructionCore(notifier, direction, ServiceRequest.Receipt, factory);
					InnerSendTransitWarehouseInstructionCore(notifier, direction, ServiceRequest.Dispatch, factory);
				}
				else if (serviceRequest == ServiceRequest.PrepareDispatch && direction == Direction.Both)
				{
					InnerSendTransitWarehouseInstructionCore(notifier, Direction.Pickup, serviceRequest, factory);
					InnerSendTransitWarehouseInstructionCore(notifier, Direction.Delivery, serviceRequest, factory);
				}
				else
				{
					InnerSendTransitWarehouseInstructionCore(notifier, direction, serviceRequest, factory);
				}

				return notifier.IsSuccess;
			}

			return false;
		}

		void InnerSendTransitWarehouseInstructionCore(TransitWarehouseInstructionNotifier notifier, Direction direction, ServiceRequest serviceRequest, BusinessObjectFactory factory)
		{
			using (var dataExport = getManualDataExportForPrepareDispatchInstruction?.Invoke(factory) ?? new ManualDataExport(factory, new[] { supporter }, UniversalDataType.UniversalShipment))
			{
				dataExport.RecipientType = direction == Direction.Pickup ? MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse : MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse;
				dataExport.RecipientService = GetRecipientService(serviceRequest);

				var receiptService = ZString.Empty;

				switch (serviceRequest)
				{
					case ServiceRequest.Receipt:
						receiptService = nameof(ServiceCodeType.TWR);
						break;

					case ServiceRequest.PrepareDispatch:
						receiptService = nameof(ServiceCodeType.TWP);
						break;

					case ServiceRequest.Dispatch:
					default:
						receiptService = nameof(ServiceCodeType.TWD);
						break;
				}

				dataExport.RecipientService = receiptService;

				var xmlEvents = dataExport.SendData(notifier);

				if (xmlEvents.Any())
				{
					var transitUniversalService = ObjectFactory.Get<ITransitUniversalService>();
					var statusCode = xmlEvents.Select(e => e.Context.ProcessingStatusCode).FirstOrDefault(status => !status.IsEmpty);
					var failureReason = xmlEvents.Select(e => e.Context.FailureReason).FirstOrDefault(reason => !reason.IsEmpty);

					var notification = transitUniversalService.GetNotificationForInstruction(statusCode, failureReason);

					if (notification != null && !string.IsNullOrEmpty(notification.Message))
					{
						notifier.Add(notification);
					}
				}
			}
		}

		#endregion

		#region SendTransitWarehouseDispatchStopLoadInstruction

		public void SendTransitWarehouseDispatchStopLoadInstruction(Direction direction, bool isCancel, bool requestedIsValid)
		{
			using (supporter.Factory.AddDisposableService())
			using (var notifier = new TransitWarehouseInstructionNotifier(notificationsParent))
			{
				if (ValidateCanSendStopLoadInstruction(notifier, direction, requestedIsValid))
				{
					var eventType = isCancel ? AutoEvents.ServiceRequested : AutoEvents.ServiceSuspended;

					var referenceParams = new Dictionary<string, string>();
					referenceParams[Params.Type] = (NoResString)"Load"; // Event Parameter
					referenceParams[Params.Facility] = EventConstants.Facilities.Code.Depot;
					referenceParams[Params.Location] = GetTransitWarehouseAddress(supporter, direction).OA_RL_NKRelatedPortCode;

					if (SendTransitWarehouseDispatchStopLoadInstructionCore(notifier, direction, eventType.Code, isCancel, referenceParams, out var result))
					{
						var log = supporter.Logs.AddNew(eventType, ZDateTimeOffset.Now);
						using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
						{
							log.ReferenceFreeText = result.ToString();
							if (eventType == AutoEvents.ServiceSuspended)
							{
								var svrEvent = supporter.Factory.LoadTop1<StmEvent>(new ZQuery(StmEventSchema.SE_Code, AutoEvents.ServiceRequestedCode));
								log.Event.SE_IsRefernceFormatOverridden = true;
								log.Event.SE_OverriddenReferenceFormat = svrEvent.SE_ReferenceFormat;
							}

							foreach (var parameter in referenceParams)
							{
								log.Parameters[parameter.Key] = parameter.Value;
							}
						}

						supporter.Factory.Save();
					}
				}
			}
		}

		bool SendTransitWarehouseDispatchStopLoadInstructionCore(TransitWarehouseInstructionNotifier notifier, Direction direction, string eventCode, bool isCancel, Dictionary<string, string> referenceParams, out TransitUniversalServiceResult result)
		{
			result = TransitUniversalServiceResult.Failed;

			using (var dataExport = new ManualDataExport(supporter.Factory, new[] { supporter }, UniversalDataType.UniversalEvent))
			{
				dataExport.RecipientType = GetRecipientType(direction);
				dataExport.RecipientService = GetRecipientService(ServiceRequest.Dispatch);
				dataExport.EventCode = dataExport.TriggerDescription = eventCode;
				dataExport.EventReference = StmALog.GenerateEventReference(string.Empty, referenceParams.Select(r => new KeyValuePair<string, string>(r.Key, r.Value)).ToArray());

				result = TransitUniversalServiceResult.Queued;
				var xmlEvents = dataExport.SendData(notifier);
				if (xmlEvents.Any())
				{
					var transitUniversalService = ObjectFactory.Get<ITransitUniversalService>();
					var statusCode = xmlEvents.Select(e => e.Context.ProcessingStatusCode).FirstOrDefault(status => !status.IsEmpty);
					var notification = transitUniversalService.GetNotificationForStopLoadInstructionEvent(statusCode, isCancel);

					if (!string.IsNullOrEmpty(notification?.Message))
					{
						notifier.Add(notification);
					}

					if (notification is InfoNotification)
					{
						result = TransitUniversalServiceResult.Succeeded;
					}
					else if (notification is ErrorNotification)
					{
						result = TransitUniversalServiceResult.Failed;
					}
				}
			}

			return notifier.IsSuccess;
		}

		bool ValidateCanSendStopLoadInstruction(TransitWarehouseInstructionNotifier notifier, Direction direction, bool requestedIsValid)
		{
			var result = false;
			if (ValidateCanSendTransitWarehouseInstruction(notifier, direction, ServiceRequest.Dispatch))
			{
				if (!requestedIsValid)
				{
					notifier.AddError(Res.GetString("970504ae-146b-4c53-8907-9f4a2293cefd",
							"The {0} Dispatch Instructions must be sent before a Load can be stopped.",
							direction == Direction.Pickup ? supporter.PickupDescription : supporter.DeliveryDescription));
				}
				else
				{
					result = true;
				}
			}

			return result;
		}

		#endregion

		#region Common Functions

		bool ValidateCanSendTransitWarehouseInstruction(INotifications notifier, Direction direction, ServiceRequest serviceRequest)
		{
			if (supporter.HasChanges)
			{
				notifier.AddError(Res.GetString("2fda97f1-6ab8-4678-9d65-788ea1dd5b34", "Please save your changes before sending the Transit Warehouse Instruction."));
				return false;
			}
			else
			{
				var address = GetTransitWarehouseAddress(supporter, direction);
				if (address == null)
				{
					notifier.AddError(Res.GetString("0dffeede-2f09-493f-b68e-2fe36bb778c6",
						"The {0} {1} must be entered before the {0} TW {2} Instruction can be sent.",
						direction == Direction.Pickup ? supporter.PickupDescription : supporter.DeliveryDescription,
						supporter.TransitWarehouseDescription,
						GetServiceRequestDescription(serviceRequest)));
					return false;
				}

				if (serviceRequest == ServiceRequest.Receipt)
				{
					var packlines = supporter is ForwardingShipment shipment ? shipment.OuterPackLines.Cast<ForwardingPackLine>() : null;
					if (packlines != null && packlines.Any(p => p.BlindPackageAttached))
					{
						notifier.AddError(Res.GetString("ff20be24-7489-4e86-b159-6acd6d6e2668", "Unable to send a receive instruction for Shipment with blind packages. Users need to send either a prepare to dispatch or dispatch instruction."));

						supporter.Logs.AddNew(AutoEvents.ErrorReport, "Unable to send a receive instruction for Shipment with blind packages. Users need to send either a prepare to dispatch or dispatch instruction.");

						return false;
					}
				}
			}

			return true;
		}

		static string GetServiceRequestDescription(ServiceRequest serviceRequest)
		{
			switch (serviceRequest)
			{
				case ServiceRequest.ReceiveAndDispatch:
					return Res.GetString("0a109d79-34bf-4aed-a496-e110b15ade2d", "Receipt and Dispatch");
				case ServiceRequest.Receipt:
					return Res.GetString("06a5d224-01f9-44fb-8222-67a2fff9fd1f", "Receipt");
				case ServiceRequest.Dispatch:
					return Res.GetString("281011d9-1e24-4149-b5f5-2d04b3ff4ed3", "Dispatch");
				case ServiceRequest.PrepareDispatch:
					return Res.GetString("0356f397-c413-37ae-40e0-26a7e783c8bd", "Prepare Dispatch");
				default:
					throw new NotSupportedException();
			}
		}

		IWhsWarehouse GetTransitWarehouse(OrgAddress address)
		{
			var query = new ZQuery(ZArchitecture.Schema.WhsWarehouseSchema.WW_OA_WarehouseAddress, address?.PK ?? ZGuid.Empty);
			query.AddToFilter(ZArchitecture.Schema.WhsWarehouseSchema.WW_IsActive, true);
			query.AddToFilter(ZArchitecture.Schema.WhsWarehouseSchema.WW_WarehouseType, Warehouse.Integration.CodeLists.WarehouseTypes.Codes.Transit);
			var warehouse = supporter.Factory.LoadTop1<IWhsWarehouse>(query);
			return warehouse;
		}

		public static OrgAddress GetTransitWarehouseAddress(ITransitWarehouseInstructionSupporter supporter, Direction direction)
		{
			if (supporter == null)
			{
				return null;
			}

			return direction == Direction.Pickup ? supporter.PickupTransitWarehouse : supporter.DeliveryTransitWarehouse;
		}

		static string GetRecipientType(Direction direction)
		{
			return direction == Direction.Pickup ? MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse : MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse;
		}

		static string GetRecipientService(ServiceRequest serviceRequest)
		{
			string recipientService;

			switch (serviceRequest)
			{
				case ServiceRequest.Receipt:
					recipientService = nameof(ServiceCodeType.TWR);
					break;

				case ServiceRequest.PrepareDispatch:
					recipientService = nameof(ServiceCodeType.TWP);
					break;

				case ServiceRequest.Dispatch:
				default:
					recipientService = nameof(ServiceCodeType.TWD);
					break;
			}

			return recipientService;
		}

		static void SetInstructionDateRequested(Direction direction, ServiceRequest serviceRequest, ZDateTime date, ITransitWarehouseInstructionSupporter supporter)
		{
			if (serviceRequest == ServiceRequest.PrepareDispatch || supporter == null)
			{
				return;
			}

			if (direction == Direction.Pickup)
			{
				if (serviceRequest == ServiceRequest.Receipt)
				{
					supporter.PickupReceiptRequestedDate = date;
				}
				else if (serviceRequest == ServiceRequest.Dispatch)
				{
					supporter.PickupDispatchRequestedDate = date;
				}
				else
				{
					supporter.PickupReceiptRequestedDate = date;
					supporter.PickupDispatchRequestedDate = date;
				}
			}
			else
			{
				if (serviceRequest == ServiceRequest.Receipt)
				{
					supporter.DeliveryReceiptRequestedDate = date;
				}
				else if (serviceRequest == ServiceRequest.Dispatch)
				{
					supporter.DeliveryDispatchRequestedDate = date;
				}
				else
				{
					supporter.DeliveryReceiptRequestedDate = date;
					supporter.DeliveryDispatchRequestedDate = date;
				}
			}
		}

		#endregion

		public void GeneratePackagesWithIDs()
		{
			var direction = Direction.Pickup;
			var receiptDispatch = ServiceRequest.Receipt;

			using (supporter.Factory.AddDisposableService())
			using (var notifier = new TransitWarehouseInstructionForIDsGeneratorNotifier(notificationsParent))
			{
				var shipment = supporter as ForwardingShipment;

				if (shipment.HasChanges)
				{
					notifier.AddError(Res.GetString("0933eb0f-bbc4-488d-8421-0e7df7e1892a", "Please save your changes before generating packages with IDs."));
					return;
				}

				if (shipment.JS_HouseBill.IsEmpty)
				{
					notifier.AddError(Res.GetString("9bfcf03c-0cc7-4cc7-898f-17b22315b809", "The House Bill number must be populated in the Basic Registration."));
					return;
				}

				var forwardingPackLines = shipment.OuterPackLines.Cast<ForwardingPackLine>().ToList();

				if (forwardingPackLines.Count == 0 || forwardingPackLines.All(packLine => packLine.PkgPackageCollection.Any()))
				{
					notifier.AddError(Res.GetString("383aee33-751d-4fe4-a14c-2e1b6989024f", "Please ensure that at least one pack line is entered in the Packing tab, with no linked package IDs against it."));
					return;
				}

				var pickupTransitWarehouse = GetTransitWarehouseAddress(supporter, direction);
				if (pickupTransitWarehouse == null)
				{
					notifier.AddError(Res.GetString("a8522752-8168-40d8-92e6-c4dc58cddf4b", @"Transit Warehouse module is used to manage packages for your shipment.

Please ensure that Shipment > Pickup > CFS/Transit Warehouse organization is not blank and is set up as the proxy for a Transit Warehouse with valid EDI Communications setup (Organization > Config > Details > Config > EDI Communications)."));
					return;
				}

				if (GetTransitWarehouse(pickupTransitWarehouse) == null)
				{
					notifier.AddError(Res.GetString("7d6ff980-2029-4104-a332-2153ee8c67cf", @"Transit Warehouse module is used to manage packages for your shipment.

Please ensure that Shipment > Pickup > CFS/Transit Warehouse organization is set up as the proxy for a Transit Warehouse and has a valid EDI Communications setup.
(Organization > Config > Details > Config > EDI Communication)"));
					return;
				}

				if (!ValidateCommunicationModes(supporter.Factory, supporter, UniversalDataType.UniversalShipment, GetRecipientType(direction), GetRecipientService(receiptDispatch)))
				{
					notifier.AddError(Res.GetString("adae93bb-8255-4606-b63f-cc64a845e1f0", @"Transit Warehouse module is used to manage packages for your shipment.

Please ensure that Shipment > Pickup > CFS/Transit Warehouse organization {0} is set up as the proxy for a Transit Warehouse and has a valid EDI Communications setup.
(Organization > Config > Details > Config > EDI Communication)", pickupTransitWarehouse.Header.OH_Code));
					return;
				}

				var packages = GeneratePackagesWithIDs(forwardingPackLines);
				notifier.Add(new InfoNotification(Res.GetString("602b8049-abdc-439a-89f5-e42949c88f03", "{0} Package IDs successfully created.", packages.Count)));
				notifier.Add(new InfoNotification(System.Environment.NewLine
					+ Res.GetString("bfe25102-9948-4d02-ae1c-92c6d72e7a78", @"Use Transit Warehouse module to manage packages for this shipment (for example, manage outer and/or inner packs, references, seals, etc.)")));
				supporter.Factory.Save();

				var factory = new BusinessObjectFactory { NameForDebugging = "Send Transit Warehouse Instruction" };
				using (factory.AddDisposableService())
				using (var logNotifier = new TransitWarehouseInstructionForIDsGeneratorNotifier(new LogNotifier()))
				{
					if (SendTransitWarehouseInstructionCore(logNotifier, direction, receiptDispatch, factory))
					{
						factory.Save();
					}
					else
					{
						notifier.AddRange(logNotifier.Notifications);
					}
				}

				return;
			}
		}

		public static bool ValidateCommunicationModes(BusinessObjectFactory factory, IWorkflowProvider parent, UniversalDataType dataType, ZString recipientType, ZString recipientService)
		{
			using (var dataExport = new ManualDataExport(factory, parent, dataType))
			{
				dataExport.RecipientType = recipientType;
				dataExport.RecipientService = recipientService;

				var recipients = dataExport.GetRecipients(new LogNotifier());

				return recipients.All(r => r.CommunicationModes.Destinations.Any());
			}
		}

		static IList<Packing.Business.PkgPackage> GeneratePackagesWithIDs(List<ForwardingPackLine> forwardingPackLines)
		{
			var packages = new List<Packing.Business.PkgPackage>();
			foreach (var packLine in forwardingPackLines.Where(p => p.JL_PackageCount > 0 && !p.PkgPackageCollection.Any()).OrderBy(p => p.JL_PackLineId))
			{
				for (var i = 0; i < packLine.JL_PackageCount; i++)
				{
					packages.Add(packLine.GeneratePackageWithIDs());
				}
			}

			return packages;
		}
	}

	#endregion

	#region Notifier

	class LogNotifier : INotifications
	{
		public void Add(INotification notification)
		{
			notifications.Add(notification);
		}

		protected List<INotification> notifications = new List<INotification>();
	}

	public class TransitWarehouseInstructionNotifier : InstructionNotifier
	{
		public TransitWarehouseInstructionNotifier(INotifications parent) : base(parent)
		{
		}

		protected override string BuildInstructionMessage()
		{
			var instructionMessageBuilder = new ZStringBuilder();
			instructionMessageBuilder.Append(Res.GetString("2c9ada7b-b14e-4223-abca-38c362199443", "The Transit Warehouse Instruction has been sent."));
			foreach (var message in notifications.Select(n => n.Message))
			{
				instructionMessageBuilder.Append(message);
			}

			return instructionMessageBuilder.ToStringWithNewLineBetweenAppends();
		}
	}

	public class TransitWarehouseInstructionForIDsGeneratorNotifier : TransitWarehouseInstructionNotifier
	{
		public TransitWarehouseInstructionForIDsGeneratorNotifier(INotifications parent) : base(parent)
		{
		}

		protected override string BuildInstructionMessage()
		{
			var instructionMessageBuilder = new ZStringBuilder();
			foreach (var message in notifications.Select(n => n.Message))
			{
				instructionMessageBuilder.Append(message);
			}

			return instructionMessageBuilder.ToStringWithNewLineBetweenAppends();
		}
	}

	#endregion
}
