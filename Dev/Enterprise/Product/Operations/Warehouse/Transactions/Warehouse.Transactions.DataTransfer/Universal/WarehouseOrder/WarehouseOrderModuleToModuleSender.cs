using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using static Enterprise.Integration.Forwarding;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WarehouseOrderModuleToModuleSender : ModuleToModuleSender<WhsOrder>, IModuleToModuleSender
	{
		public WarehouseOrderModuleToModuleSender()
		{
		}

		public PublishToUniversalResult CreateForwardingShipmentFromOrder(WhsOrder order)
		{
			using (order?.Factory.AddDisposableService())
			{
				var result = new WarehouseOrderModuleToModuleSender().CreateNewEntityFromParent(order);
				if (order?.ConsigneeDocAddress != null && order.ConsigneeDocAddress.ValidationStatus != AddressValidationStatus.ToBeVerified && result.FindJobIfExists() is IForwardingShipment shipment && shipment.ConsigneeDocumentaryAddress != null)
				{
					shipment.ConsigneeDocumentaryAddress.E2_AddressMap = order.ConsigneeDocAddress.AddressMap;
					shipment.ConsigneeDocumentaryAddress.E2_GeoLocation = order.ConsigneeDocAddress.GeoLocation;
					shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = order.ConsigneeDocAddress.E2_AddressOverride;
					if (shipment.ConsigneeDocumentaryAddress.E2_AddressOverride)
					{
						shipment.ConsigneeDocumentaryAddress.E2_ValidationStatus = order.ConsigneeDocAddress.ValidationStatus;
					}

#if DEBUG
					if (OnSaveActionForTest != null)
					{
						shipment.ConsigneeDocumentaryAddress.Factory.SaveInTransactionActions.Add(OnSaveActionForTest((JobDocAddress)shipment.ConsigneeDocumentaryAddress));
					}
#endif

					ZExceptionReporting.ProcessWithSaveExceptionHandling(shipment.ConsigneeDocumentaryAddress.Factory.Save, null);
				}
				return result;
			}
		}

		protected override ZString ErrorPrefix
		{
			get { return Res.GetString("0f8e564b-7e57-4baa-948d-83db6fcca542", "Failed to create Shipment:"); }
		}

		protected override UniversalEvent[] GetUniversalEvents(WhsOrder order)
		{
			var factory = new BusinessObjectFactory();
			UniversalEvent[] events;
			using (factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, ((IModuleToModule)order).RecipientOrganisation as OrgHeader, new RecipientRoleType[] { RecipientRoleType.FOR }, order).ToArray();
				factory.Save();
			}
			return events;
		}

		protected override DataContextType EntityTypeToLoad
		{
			get { return DataContextType.ForwardingShipment; }
		}

		#region IModuleToModuleSender Members

		public PublishToUniversalResult CreateJob<T>(T businessEntity)
			where T : BusinessObject, IWorkflowProvider, IModuleToModule
		{
			return CreateForwardingShipmentFromOrder(businessEntity as WhsOrder);
		}

		#endregion

#if DEBUG
		public Func<BusinessObject, SaveInTransactionAction> OnSaveActionForTest { set; get; }
#endif
	}
}
