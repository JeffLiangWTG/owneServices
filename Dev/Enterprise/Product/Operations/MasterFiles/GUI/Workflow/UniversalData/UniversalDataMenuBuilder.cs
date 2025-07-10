using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class UniversalDataMenuBuilder
	{
		public UniversalDataMenuBuilder(Menus menu, Func<IEnumerable<IWorkflowProvider>> provider, IDataContextManager dataContextManager, WorkflowDescriptor workflowDescriptor = null)
		{
			this.menu = menu;
			this.provider = Argument.NotNull(provider, "provider");
			this.dataContextManager = Argument.NotNull(dataContextManager, "dataContextManager");
			this.workflowDescriptor = workflowDescriptor;
		}

		readonly Menus menu;
		readonly Func<IEnumerable<IWorkflowProvider>> provider;
		readonly IDataContextManager dataContextManager;
		readonly WorkflowDescriptor workflowDescriptor;

		public enum Menus
		{
			ManualDataExport,
			ReceipientsList
		}

		public IEnumerable<UniversalDataMenuItemDescriptor> Build()
		{
			IEnumerable<UniversalDataMenuItemDescriptor> result = null;

			switch (menu)
			{
				case Menus.ManualDataExport:
					result = BuildMenuWithPopup();
					break;

				case Menus.ReceipientsList:
					result = BuildMenuWithReceipients();
					break;
			}

			return result;
		}

		#region Menu with Receipients

		public static MenuItem BuildUniversalDataReceipientsMenu(IBulkSendUniversalDataSupportable bulkSendUniversalDataSupportable)
		{
			Argument.NotNull(bulkSendUniversalDataSupportable, "universalDataSupportable");
			Argument.NotNullOrEmpty(bulkSendUniversalDataSupportable.NameOfSingleObject, "universalDataSupportable.NameOfSingleObject");
			Argument.NotNull(bulkSendUniversalDataSupportable.TypeOfSingleObject, "universalDataSupportable.TypeOfSingleObject");
			Argument.NotNullOrEmpty(bulkSendUniversalDataSupportable.WorkflowDescriptorCode, "universalDataSupportable.WorkflowDescriptorCode");

			var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(bulkSendUniversalDataSupportable.WorkflowDescriptorCode);

			if (workflowDescriptor == null)
			{
				return null;
			}

			var dataContextManager = bulkSendUniversalDataSupportable.TypeOfSingleObject.GetUniversalDataContextManager();

			if (dataContextManager == null)
			{
				return null;
			}

			var exportMenuItem = new ZMenuItem(ResString.GetMultilingualString("06a9d392-f987-4f12-ab6f-15fe463b739f", "Export {0} To", bulkSendUniversalDataSupportable.NameOfSingleObject));

			Func<IEnumerable<IWorkflowProvider>> selectedElementsProvider = () =>
			{
				var bizObjects = bulkSendUniversalDataSupportable.GetElementsToSend() ?? Array.Empty<IWorkflowProvider>();
				if (!bizObjects.Any())
				{
					Globals.Message.ShowInformation(Res.GetString("da54f19e-f29a-45f6-a9da-bcb23fda8090", "Please select at least one {0}.", bulkSendUniversalDataSupportable.NameOfSingleObject));
				}
				return bizObjects;
			};

			var universalMenuBuilder = new UniversalDataMenuBuilder(Menus.ReceipientsList, selectedElementsProvider, dataContextManager, workflowDescriptor);

			foreach (var menuDescriptor in universalMenuBuilder.Build())
			{
				exportMenuItem.MenuItems.Add(new ZMenuItem(menuDescriptor.Caption, menuDescriptor.Handler));
			}

			return exportMenuItem;
		}

		IEnumerable<UniversalDataMenuItemDescriptor> BuildMenuWithReceipients()
		{
			Argument.NotNull(workflowDescriptor, "workflowDescriptor");

			if (dataContextManager.ManagesShipments() || dataContextManager.ManagesActivities())
			{
				var supportedParties = new MessageRecipientPartyTypeList(workflowDescriptor.SupportedMessageRecipientParties(null, null))
					.Cast<PartyTypeDescriptionPair>()
					.Where(party => SupportedPartyCodes.Contains(party.Code))
					.OrderBy(party => party.Description)
					.ToArray();

				for (int i = 0; i < supportedParties.Length; i++)
				{
					var party = supportedParties[i];
					yield return new UniversalDataMenuItemDescriptor((NoResString)party.Description, (s, e) => SendUniversalData(provider, party.Description, party.Code));
				}
			}
		}

		protected IEnumerable<string> SupportedPartyCodes
		{
			get
			{
				yield return MessageRecipientPartyTypeList.Codes.CustomsOutturnAgent;
				yield return MessageRecipientPartyTypeList.Codes.Consignee;
				yield return MessageRecipientPartyTypeList.Codes.Consignor;
				yield return MessageRecipientPartyTypeList.Codes.BillToParty;
				yield return MessageRecipientPartyTypeList.Codes.Broker;
				yield return MessageRecipientPartyTypeList.Codes.Bolero;
				yield return MessageRecipientPartyTypeList.Codes.ImportBroker;
				yield return MessageRecipientPartyTypeList.Codes.ExportBroker;
				yield return MessageRecipientPartyTypeList.Codes.PickupCartage;
				yield return MessageRecipientPartyTypeList.Codes.DeliveryCartage;
				yield return MessageRecipientPartyTypeList.Codes.ReceivingAgent;
				yield return MessageRecipientPartyTypeList.Codes.SendingAgent;
				yield return MessageRecipientPartyTypeList.Codes.ControllingCustomer;
				yield return MessageRecipientPartyTypeList.Codes.OrgProxy;
				yield return MessageRecipientPartyTypeList.Codes.Client;
				yield return MessageRecipientPartyTypeList.Codes.TransportCo;
				yield return MessageRecipientPartyTypeList.Codes.Carrier;
				yield return MessageRecipientPartyTypeList.Codes.CarrierBookingAgent;
				yield return MessageRecipientPartyTypeList.Codes.NotifyParty;
				yield return MessageRecipientPartyTypeList.Codes.DeliveryToParty;
				yield return MessageRecipientPartyTypeList.Codes.PickupParty;
				yield return MessageRecipientPartyTypeList.Codes.InvoiceDebtor;
				yield return MessageRecipientPartyTypeList.Codes.DepartureCFS;
				yield return MessageRecipientPartyTypeList.Codes.ArrivalCFS;
				yield return MessageRecipientPartyTypeList.Codes.Forwarder;
				yield return MessageRecipientPartyTypeList.Codes.ArrivalCarrier;
				yield return MessageRecipientPartyTypeList.Codes.DepartureCarrier;
				yield return MessageRecipientPartyTypeList.Codes.Principal;
				yield return MessageRecipientPartyTypeList.Codes.DepartureCTO;
				yield return MessageRecipientPartyTypeList.Codes.ArrivalCTO;
				yield return MessageRecipientPartyTypeList.Codes.ArrivalContainerYard;
				yield return MessageRecipientPartyTypeList.Codes.DepartureContainerYard;
				yield return MessageRecipientPartyTypeList.Codes.WarehouseInwards;
				yield return MessageRecipientPartyTypeList.Codes.WarehouseOutwards;
				yield return MessageRecipientPartyTypeList.Codes.WarehouseDynamicWorkOrder;
				yield return MessageRecipientPartyTypeList.Codes.WarehouseWorkOrder;
				yield return MessageRecipientPartyTypeList.Codes.HVLVAirClearanceAgent;
				yield return MessageRecipientPartyTypeList.Codes.ShippingManager;
				yield return MessageRecipientPartyTypeList.Codes.BookingParty;
				yield return MessageRecipientPartyTypeList.Codes.DeConsolidator;
				yield return MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty;
				yield return MessageRecipientPartyTypeList.Codes.PickupAgent;
				yield return MessageRecipientPartyTypeList.Codes.DeliveryAgent;
				yield return MessageRecipientPartyTypeList.Codes.HVLVSeaClearanceAgent;
				yield return MessageRecipientPartyTypeList.Codes.JapanCustomsAFR;
				yield return MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse;
				yield return MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse;
				yield return MessageRecipientPartyTypeList.Codes.Warehouse;
				yield return MessageRecipientPartyTypeList.Codes.SeaCargoResponsibleParty;
				yield return MessageRecipientPartyTypeList.Codes.ASYCUDA;
				yield return MessageRecipientPartyTypeList.Codes.IndianCustomsEDISystem;
				yield return MessageRecipientPartyTypeList.Codes.PortForExportManifest;
				yield return MessageRecipientPartyTypeList.Codes.PortForExportRelease;
				yield return MessageRecipientPartyTypeList.Codes.PortForImportManifest;
				yield return MessageRecipientPartyTypeList.Codes.PortForImportRelease;
				yield return MessageRecipientPartyTypeList.Codes.YardForExportRelease;
				yield return MessageRecipientPartyTypeList.Codes.YardForImportPreArrival;
				yield return MessageRecipientPartyTypeList.Codes.ContainerYard;
				yield return MessageRecipientPartyTypeList.Codes.CTO;
				yield return MessageRecipientPartyTypeList.Codes.ControllingAgent;
				yield return MessageRecipientPartyTypeList.Codes.CACustomsIIDD4StatusNotice;
				yield return MessageRecipientPartyTypeList.Codes.BondedWhsChangeOfOwnership;
				yield return MessageRecipientPartyTypeList.Codes.BondedWhsChangeOfRegime;
				yield return MessageRecipientPartyTypeList.Codes.BondedWarehouseInwards;
				yield return MessageRecipientPartyTypeList.Codes.BondedWarehouseOutwards;
				yield return MessageRecipientPartyTypeList.Codes.LastCompletedTaskResource;
				yield return MessageRecipientPartyTypeList.Codes.JobLevelWorkflowGroup;
				yield return MessageRecipientPartyTypeList.Codes.NotificationGroup;
				yield return MessageRecipientPartyTypeList.Codes.CreditControlledDocumentApproval;
				yield return MessageRecipientPartyTypeList.Codes.AssignedStaff;
				yield return MessageRecipientPartyTypeList.Codes.RequiredCapabilityMembers;
				yield return MessageRecipientPartyTypeList.Codes.AssignedGroupMembers;
				yield return MessageRecipientPartyTypeList.Codes.NVOCC;
				yield return MessageRecipientPartyTypeList.Codes.ExternalBroker;
				yield return MessageRecipientPartyTypeList.Codes.HVLVForwarder;
				yield return MessageRecipientPartyTypeList.Codes.CarrierMessagingDebtor;
				yield return MessageRecipientPartyTypeList.Codes.TransportJobRegistry;
				yield return MessageRecipientPartyTypeList.Codes.GlobalTradeManagement;
				yield return MessageRecipientPartyTypeList.Codes.GateManagement;
				yield return MessageRecipientPartyTypeList.Codes.PortForTransitManifest;
			}
		}

		static void SendUniversalData(Func<IEnumerable<IWorkflowProvider>> provider, string receipientDescription, string receipientType)
		{
			IWorkflowProvider[] workflowProviders = (provider() ?? Enumerable.Empty<IWorkflowProvider>()).ToArray();

			if (workflowProviders.Any())
			{
				if (workflowProviders.Cast<BusinessObject>().Any(bizObj => bizObj.HasChanges))
				{
					Globals.Message.ShowWarning(Res.GetString("b9ad8c0f-6419-4fe5-b4a8-4b0b74267287", "Please save your changes before sending XML Universal Data."), Res.GetString("dd8da5d1-496a-4004-9828-1bad4101d22e", "Send Universal XML"));
				}
				else
				{
					var factory = new BusinessObjectFactory { NameForDebugging = "The True manual data export" };
					var dataExport = new ManualDataExport(factory, workflowProviders, UniversalDataType.UniversalShipment);
					dataExport.RecipientType = receipientType;

					using (var progressForm = new ManualDataExportProgressForm())
					{
						new ManualDataExportProgressFormSendOnButtonPress(factory, dataExport).Apply(progressForm);
						ZFormModaliser.ShowDialogWithoutDispose(progressForm);
					}
				}
			}
		}

		#endregion

		#region Menu with Popup

		IEnumerable<UniversalDataMenuItemDescriptor> BuildMenuWithPopup()
		{
			if (dataContextManager.ManagesShipments())
			{
				yield return new UniversalDataMenuItemDescriptor(ResString.GetMultilingualString("beeebcba-d921-42b5-b4ed-56bba9810ce2", "Universal Shipment"),
					(s, e) => SendUniversalData(provider, UniversalDataType.UniversalShipment, dataContextManager));
			}

			if (dataContextManager.ManagesEvents())
			{
				yield return new UniversalDataMenuItemDescriptor(ResString.GetMultilingualString("541b8460-6b84-47ce-bf28-2ca7d72aa6cc", "Universal Event"),
					(s, e) => SendUniversalData(provider, UniversalDataType.UniversalEvent, dataContextManager));
			}

			if (dataContextManager.ManagesTransactions())
			{
				yield return new UniversalDataMenuItemDescriptor(ResString.GetMultilingualString("886ca0bb-02ab-436c-9284-fc49e298263e", "Universal Transaction"),
					(s, e) => SendUniversalData(provider, UniversalDataType.UniversalTransaction, dataContextManager));
			}

			if (dataContextManager.ManagesSchedules())
			{
				yield return new UniversalDataMenuItemDescriptor(ResString.GetMultilingualString("50bba809-8a45-425a-b457-05870b2673f8", "Universal Schedule"),
					(s, e) => SendUniversalData(provider, UniversalDataType.UniversalSchedule, dataContextManager));
			}

			if (dataContextManager.ManagesActivities())
			{
				yield return new UniversalDataMenuItemDescriptor(ResString.GetMultilingualString("ac141b30-7b4c-4ee7-aa4b-6b7926a02bd0", "Universal Activity"),
					(s, e) => SendUniversalData(provider, UniversalDataType.UniversalActivity, dataContextManager));
			}
		}

		static void SendUniversalData(Func<IEnumerable<IWorkflowProvider>> provider, UniversalDataType dataType, IDataContextManager dataContextManager)
		{
			IWorkflowProvider[] workflowProviders = (provider() ?? Enumerable.Empty<IWorkflowProvider>()).ToArray();

			if (workflowProviders.Any())
			{
				if (workflowProviders.Cast<BusinessObject>().Any(bizObj => bizObj.HasChanges || !bizObj.IsInDatabase))
				{
					Globals.Message.ShowWarning(Res.GetString("7a935b67-85e4-413d-b7b9-96e2b8033193", "Please save your changes before sending XML Universal Data."),
						Res.GetString("a59a973c-4a8a-496c-ab1a-d9af53821502", "Send Universal XML"));
				}
				else
				{
					var factory = new BusinessObjectFactory { NameForDebugging = "Data Export Form Factory" };
					using (factory.AddDisposableService())
					using (var exportForm = new ManualDataExportForm(factory, workflowProviders, dataType, null, dataContextManager.SchemaOverride))
					{
						ZFormModaliser.ShowDialogWithoutDispose(exportForm);
					}
				}
			}
		}

		#endregion
	}
}
