using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ConsolShipmentCollection : ManyToManyShipmentCollection
	{
		public ConsolShipmentCollection(CommonConsol parent)
			: base(parent)
		{
			limitHelper = new ShipmentsOnConsolLimitHelper(ParentConsol);
			limitHelper.SetCollectionLimit(this);
		}

		readonly ShipmentsOnConsolLimitHelper limitHelper;

		public CommonShipment this[int index]
		{
			get { return (CommonShipment)Elements[index]; }
		}

		#region Add

		public override void Add(BusinessObject businessObject)
		{
			CommonShipment shipment = (CommonShipment)businessObject;
			base.Add(shipment);
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			CommonShipment shipment = (CommonShipment)bizOAdded;
			base.OnAdded(shipment);
			if (IsUpdatingByDataRefreshBus && !IsValidationSuspended)
			{
				limitHelper.CheckCollectionCountOnParent();
			}

			if (!IsLoading && !IsUpdatingByDataRefreshBus)
			{
				if (!IsValidationSuspended)
				{
					CheckMatchingForwardingAgentsWithShipmentRelatedParties(shipment);
				}

				if (shipment.Consols.Contains(ParentConsol))
				{
					ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(ParentConsol, shipment);
				}
				else
				{
					shipment.Consols.Add(ParentConsol);
				}
			}
		}

		#region Validation

		void CheckMatchingForwardingAgentsWithShipmentRelatedParties(CommonShipment shipment)
		{
			if (!shipment.IsValidationSuspended)
			{
				var receivingAgentsWarning = FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedReceivingAgents(new[] { shipment }, new[] { ParentConsol });
				var sendingAgentsWarning = FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedSendingAgents(new[] { shipment }, new[] { ParentConsol });

				if (!string.IsNullOrEmpty(receivingAgentsWarning))
				{
					shipment.AddRowWarning(receivingAgentsWarning);
				}

				if (!string.IsNullOrEmpty(sendingAgentsWarning))
				{
					shipment.AddRowWarning(sendingAgentsWarning);
				}
			}
		}

		#endregion

		public new CommonShipment AddNew()
		{
			return (CommonShipment)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return AllowAddNew && base.AllowNewCore; }
		}

		public bool AllowAddNew
		{
			get { return allowAddNew; }
			set { allowAddNew = value; }
		}
		bool allowAddNew = true;

		#endregion

		#region Remove

		public override void Remove(BusinessObject elementToRemove)
		{
			var shipmentToRemove = (CommonShipment)elementToRemove;

			if (!removingShipments.Contains(shipmentToRemove))
			{
				try
				{
					removingShipments.Add(shipmentToRemove);

					if (!shipmentToRemove.IsInDatabase && ChildEditableService.GetState(Factory) == ChildEditableServiceStates.Consol)
					{
						// If detaching a new shipment, make sure it is deleted so it does not get saved to the database.
						RemoveAndDelete(shipmentToRemove);
					}
					else
					{
						base.Remove(shipmentToRemove);
					}
				}
				finally
				{
					removingShipments.Remove(shipmentToRemove);

					if (removingShipments.Count == 0)
					{
						ParentConsol.GridShipments.Rebuild();
					}
				}
			}
		}
		readonly List<CommonShipment> removingShipments = new List<CommonShipment>();

		protected override void OnRemoving(BusinessObject bizO)
		{
			CommonShipment shipment = (CommonShipment)bizO;
			base.OnRemoving(shipment);

			if (!IsUpdatingByDataRefreshBus)
			{
				ConsolShipmentRelationshipHelper.ShipmentRemovedFromConsol(ParentConsol, shipment);
			}
		}

		#endregion

		#region Notify Changed

		public void NotifyETDChanged(ZDateTime previousValue, ZDateTime newValue)
		{
			if (previousValue != newValue)
			{
				foreach (CommonShipment shipment in this)
				{
					if (shipment.JS_E_DEP.IsEmpty || shipment.JS_E_DEP > newValue)
					{
						shipment.JS_E_DEP = newValue;
					}

					if (!shipment.IsValidationSuspended)
					{
						shipment.Validation.ValidateJS_InspectionTypeCode();
					}
				}
			}
		}

		public void NotifyETAChanged(ZDateTime previousValue, ZDateTime newValue)
		{
			if (previousValue != newValue)
			{
				foreach (CommonShipment shipment in this)
				{
					if (shipment.JS_E_ARV.IsEmpty || shipment.JS_E_ARV < newValue)
					{
						shipment.JS_E_ARV = newValue;
					}
				}
			}
		}

		public void NotifyFCLAvailabilityDateChanged(ZDateTime previousValue, ZDateTime newValue)
		{
			foreach (CommonShipment shipment in this)
			{
				if (shipment.DocsAndCartage.JP_FCLAvailable < newValue)
				{
					shipment.DocsAndCartage.JP_FCLAvailable = newValue;
				}
			}
		}

		public void NotifyFCLStorageCommencesDateChanged(ZDateTime previousValue, ZDateTime newValue)
		{
			foreach (CommonShipment shipment in this)
			{
				if (shipment.DocsAndCartage.JP_FCLStorageCommences < newValue)
				{
					shipment.DocsAndCartage.JP_FCLStorageCommences = newValue;
				}
			}
		}

		#endregion

		#region Update ETA / ETD

		public ZString UpdateShipmentsETD(ZDateTime newDate, ZDateTime oldDate)
		{
			return UpdateShipmentsEstimateTime(JobShipmentSchema.JS_E_DEP.Name, newDate, oldDate, x => x.Consols.GetEarliestConsol() == ParentConsol, Res.GetString("325adbf9-1901-488a-b753-88f01eda223d", "ETD updated"));
		}

		public ZString UpdateShipmentsETA(ZDateTime newDate, ZDateTime oldDate)
		{
			return UpdateShipmentsEstimateTime(JobShipmentSchema.JS_E_ARV.Name, newDate, oldDate, x => x.Consols.GetLatestConsol() == ParentConsol, Res.GetString("7c072d06-1a87-498f-894e-482b5d4a5058", "ETA updated"));
		}

		ZString UpdateShipmentsEstimateTime(string fieldToUpdate, ZDateTime newDate, ZDateTime oldDate, Func<CommonShipment, bool> doUpdate, string message)
		{
			var result = ZString.Empty;
			var builder = new ZStringBuilder();

			foreach (CommonShipment shipment in this)
			{
				using (shipment.SuspendColoadMasterFromUpdatingEtaEtdFromLogs())
				{
					var fieldInfo = shipment.ZPropertyInfoHash[fieldToUpdate];

					if (newDate.IsValid && doUpdate(shipment))
					{
						if (oldDate.IsValid && fieldInfo.Value.IsValid)
						{
							var newFieldDate = ((ZDateTime)fieldInfo.Value).AddTicks(newDate.Ticks - oldDate.Ticks);

							if (newFieldDate.IsValidSmallDateTime)
							{
								fieldInfo.Value = newFieldDate;
								builder.Append(string.Format("{0} " + message, shipment.JS_UniqueConsignRef));
							}
						}
						else if (fieldInfo.Value.IsEmpty
							|| (fieldToUpdate == JobShipmentSchema.JS_E_DEP.Name && fieldInfo.Value.IsValid && (ZDateTime)fieldInfo.Value > newDate)
							|| (fieldToUpdate == JobShipmentSchema.JS_E_ARV.Name && fieldInfo.Value.IsValid && (ZDateTime)fieldInfo.Value < newDate))
						{
							fieldInfo.Value = newDate;
							builder.Append(string.Format("{0} " + message, shipment.JS_UniqueConsignRef));
						}

						result = builder.ToStringWithNewLineBetweenAppends();
					}
				}
			}

			return result;
		}

		#endregion
	}
}
