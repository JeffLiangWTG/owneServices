using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class ShipmentCollectionHelper
	{
		public ShipmentCollectionHelper(BusinessObjectCollection shipmentCollection)
			: this(shipmentCollection, null)
		{
		}

		public ShipmentCollectionHelper(BusinessObjectCollection shipmentCollection, ForwardingConsol parentConsol)
		{
			this.shipmentCollection = shipmentCollection;
			this.parentConsol = parentConsol;

			HookEvents();
		}

		readonly BusinessObjectCollection shipmentCollection;
		readonly ForwardingConsol parentConsol;

		void HookEvents()
		{
			if (this.shipmentCollection != null)
			{
				shipmentCollection.CountChanged += new CollectionCountChangedEventHandler(OnShipmentsCollectionCountChanged);

				foreach (ForwardingShipment shipment in shipmentCollection)
				{
					AddShipmentEvent(shipment);
				}
			}
		}

		public void UnhookAllEvents()
		{
			if (this.shipmentCollection != null)
			{
				shipmentCollection.CountChanged -= new CollectionCountChangedEventHandler(OnShipmentsCollectionCountChanged);

				foreach (ForwardingShipment shipment in shipmentCollection)
				{
					RemoveShipmentEvent(shipment);
				}
			}
		}

		void AddShipmentEvent(ForwardingShipment shipment)
		{
			if (shipment != null)
			{
				shipment.ValueNotSet += new EventHandler<ValueNotSetEventArgs>(OnShipmentValueNotSet);
				shipment.OnExportOrImportBrokerUpdate += new EventHandler<BrokerDefaultingEventArgs>(UpdateExportOrImportBrokerHelper.UpdateExportOrImportBroker);
				shipment.GetReasonChangingSecurityInspectionStatusEventHandler += MessagePopupHelper.PromptReasonForChangingSecurityInspectionStatusEventHandler;
				shipment.GetReasonChangingSecurityAdditionalInspectionStatusEventHandler += MessagePopupHelper.PromptReasonForChangingSecurityAdditionalInspectionStatusEventHandler;
				shipment.ReDefaultPackLineInspectionTypeCodesEventHandler += new CancelEventHandler(MessagePopupHelper.CheckRedefaultPackLineInspectionTypeCodesFromShipment);
				shipment.ReDefaultPackLineAdditionalInspectionTypeCodesEventHandler += new CancelEventHandler(MessagePopupHelper.CheckRedefaultPackLineAdditionalInspectionTypeCodesFromShipment);
				shipment.ReDefaultPackLineIsHighRiskEventHandler += new CancelEventHandler(MessagePopupHelper.CheckRedefaultPackLineIsHighRiskFromShipment);
				shipment.UpdateSubHVLShipmentsInspectionTypeEventHandler += new CancelEventHandler(MessagePopupHelper.CheckUpdateSubHVLShipmentInspectionTypeFromShipment);

				if (this.parentConsol != null)
				{
					shipment.MasterChanged += new EventHandler<MasterChangedEventArgs>(OnShipment_MasterChanged);
				}
			}
		}

		void RemoveShipmentEvent(ForwardingShipment shipment, bool alwaysUnlockJonHeader = true)
		{
			if (shipment != null)
			{
				shipment.ValueNotSet -= new EventHandler<ValueNotSetEventArgs>(OnShipmentValueNotSet);
				shipment.OnExportOrImportBrokerUpdate -= new EventHandler<BrokerDefaultingEventArgs>(UpdateExportOrImportBrokerHelper.UpdateExportOrImportBroker);
				shipment.MasterChanged -= new EventHandler<MasterChangedEventArgs>(OnShipment_MasterChanged);
				shipment.GetReasonChangingSecurityInspectionStatusEventHandler -= MessagePopupHelper.PromptReasonForChangingSecurityInspectionStatusEventHandler;
				shipment.GetReasonChangingSecurityAdditionalInspectionStatusEventHandler -= MessagePopupHelper.PromptReasonForChangingSecurityAdditionalInspectionStatusEventHandler;
				shipment.ReDefaultPackLineInspectionTypeCodesEventHandler -= new CancelEventHandler(MessagePopupHelper.CheckRedefaultPackLineInspectionTypeCodesFromShipment);
				shipment.ReDefaultPackLineAdditionalInspectionTypeCodesEventHandler -= new CancelEventHandler(MessagePopupHelper.CheckRedefaultPackLineAdditionalInspectionTypeCodesFromShipment);
				shipment.ReDefaultPackLineIsHighRiskEventHandler -= new CancelEventHandler(MessagePopupHelper.CheckRedefaultPackLineIsHighRiskFromShipment);
				shipment.UpdateSubHVLShipmentsInspectionTypeEventHandler -= new CancelEventHandler(MessagePopupHelper.CheckUpdateSubHVLShipmentInspectionTypeFromShipment);

				var needUnlock = true;
				if (parentConsol != null)
				{
					var apportionments = this.parentConsol.GetApportionments();
					if (apportionments.CostsCollectionLoaded)
					{
						var costs = apportionments.CostsCollection;
						foreach (var cost in costs)
						{
							if (cost is JobConsolCost)
							{
								var charges = (cost as JobConsolCost).ApportionmentCharges.Cast<ApportionSplitCharge>();
								if (charges.Any(c => c.JR_HouseBill == shipment.JS_HouseBill && c.JR_IsUsedForApportionment))
								{
									needUnlock = false;
								}
							}
						}
					}
				}

				if (needUnlock || alwaysUnlockJonHeader)
				{
					UnlockJobHeader(shipment);
				}
			}
		}

		static void UnlockJobHeader(ForwardingShipment shipment)
		{
			if (shipment.ShipmentJobHeader != null)
			{
				shipment.ShipmentJobHeader.DisposeAndDeleteNew();
			}
		}

		void OnShipmentsCollectionCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var shipment = (ForwardingShipment)e.BizObject;
			if (e.ItemAdded)
			{
				AddShipmentEvent(shipment);
			}
			else if (e.ItemRemoved)
			{
				RemoveShipmentEvent(shipment, false);
			}
		}

		void OnShipmentValueNotSet(object sender, ValueNotSetEventArgs e)
		{
			Globals.Message.ShowError(e.Reason);
		}

		void OnShipment_MasterChanged(object sender, MasterChangedEventArgs e)
		{
			ShipmentVsConsolGUIMessageHelper.Instance.OnShipmentMasterChanged(FreightShipmentVsConsolMessageHelper.Instance, (ForwardingShipment)sender, this.parentConsol, e);
		}
	}
}
