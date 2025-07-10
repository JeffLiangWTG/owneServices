using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	[ModuleID(ModuleId.JobConsol)]
	public class ConsolCollection : ManyToManyBusinessObjectCollection
	{
		public ConsolCollection(CommonShipment associatedObject)
				: base(associatedObject)
		{
		}

		protected CommonShipment ParentShipment
		{
			get { return (CommonShipment)base.fAssociatedObject; }
		}

		#region Events

		public event EventHandler ConsolLoadOrDischargePortChanged;

		protected void OnConsolLoadOrDischargePortChanged(object sender, EventArgs e)
		{
			if (!IsValidationSuspended)
			{
				CheckUniqueOriginAndDestination();
			}

			if (ConsolLoadOrDischargePortChanged != null)
			{
				ConsolLoadOrDischargePortChanged(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Properties

#if DEBUG
		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
#endif
		public List<CommonContainer> AllContainers
		{
			get
			{
				List<CommonContainer> containerList = new List<CommonContainer>();

				foreach (CommonConsol consol in this)
				{
					foreach (CommonContainer container in consol.Containers)
					{
						containerList.Add(container);
					}
				}

				return containerList;
			}
		}

		#endregion

		#region Collection Functionality

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(JobConShipLink); }
		}

		public CommonConsol this[int index]
		{
			get { return (CommonConsol)Elements[index]; }
		}

		public new CommonConsol AddNew()
		{
			return (CommonConsol)base.AddNew();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newConsol = (CommonConsol)child;

			switch (ParentShipment.JS_TransportMode)
			{
				case Core.Constants.TransportModes.SeaAir:
					newConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
					break;

				case Core.Constants.TransportModes.AirSea:
					newConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
					break;

				default:
					newConsol.JK_TransportMode = ParentShipment.JS_TransportMode;
					break;
			}

			newConsol.JK_RL_NKLoadPort = ParentShipment.JS_RL_NKOrigin;
			if (ParentShipment.JS_TransportMode == Core.Constants.TransportModes.Sea &&
					 ParentShipment.JS_PackingMode == Core.Constants.ContainerModes.Other)
			{
				SetDepartureTransportAgainForSea(Core.Constants.ContainerModes.FCL, newConsol);
			}
			else if (ParentShipment.JS_TransportMode == Core.Constants.TransportModes.Sea)
			{
				SetDepartureTransportAgainForSea(ParentShipment.JS_PackingMode, newConsol);
			}
			else
			{
				newConsol.JK_ConsolMode = ParentShipment.JS_PackingMode;
			}

			newConsol.JK_RL_NKDischargePort = ParentShipment.JS_RL_NKDestination;
			DefaultPlannedCarrier(newConsol);

			var transport = newConsol.Transports[0];
			transport.JW_ETD = ParentShipment.JS_E_DEP;
			transport.JW_ETA = ParentShipment.JS_E_ARV;

			newConsol.ShipmentConsignor = ParentShipment.ConsignorPK;

			var relatedReceivingForwarderAddress = ParentShipment.FindRelatedReceivingForwarderAddressPK();
			var relatedSendingForwarderAddress = ParentShipment.FindRelatedSendingForwarderAddressPK();

			if (!relatedReceivingForwarderAddress.IsEmpty)
			{
				newConsol.JK_OA_ReceivingForwarderAddress = relatedReceivingForwarderAddress;
			}

			if (!relatedSendingForwarderAddress.IsEmpty)
			{
				newConsol.JK_OA_SendingForwarderAddress = relatedSendingForwarderAddress;
			}
		}

		void SetDepartureTransportAgainForSea(ZString defaultConsolMode, CommonConsol newConsol)
		{
			if (string.Equals(defaultConsolMode, newConsol.JK_ConsolMode))
			{
				return;
			}

			newConsol.JK_ConsolMode = defaultConsolMode;
			if (newConsol.SendingForwarder == null)
			{
				return;
			}

			OrgHeader defaultLocalTransport = newConsol.SendingForwarder.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, newConsol.JK_TransportMode, newConsol.JK_ConsolMode);
			if (defaultLocalTransport == null)
			{
				newConsol.JK_OA_DeparturePackCFSTransportAddress = ZGuid.Empty;
			}
			else
			{
				newConsol.JK_OA_DeparturePackCFSTransportAddress = defaultLocalTransport.MainAddress.PK;
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			var consol = (CommonConsol)bizOAdded;

			if (!IsLoading && !IsUpdatingByDataRefreshBus)
			{
				if (!IsValidationSuspended)
				{
					CheckUniqueOriginAndDestination();
					CheckMatchingForwardingAgentsWithShipmentRelatedParties(consol);
					new ShipmentsOnConsolLimitHelper(consol).CheckCollectionCountOnParent();
				}

				if (consol.Shipments.Contains(ParentShipment.PK))
				{
					if (!isATCEventSuppressed)
					{
						ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, ParentShipment);
					}
				}
				else
				{
					consol.Shipments.Add(ParentShipment);
				}

				if (Count == 1)
				{
					ParentShipment.JS_JS_ColoadMasterShipmentInfo.RefreshBinding();
				}

				ParentShipment.JS_JK_ConsolIDInfo.RefreshBinding();
			}

			consol.JK_RL_NKLoadPortInfo.ValueChanged += new EventHandler(OnConsolLoadOrDischargePortChanged);
			consol.JK_RL_NKDischargePortInfo.ValueChanged += new EventHandler(OnConsolLoadOrDischargePortChanged);

			base.OnAdded(consol);
		}

		internal IDisposable SuppressAddingATCEvent()
		{
			isATCEventSuppressed = true;
			return new DisposableAction(() => isATCEventSuppressed = false);
		}

		bool isATCEventSuppressed;

		#region Remove

		public override void Remove(BusinessObject elementToRemove)
		{
			base.Remove(elementToRemove);

			if (!IsValidationSuspended)
			{
				CheckUniqueOriginAndDestination();

				if (ParentShipment != null && !ParentShipment.IsDeleted && !ParentShipment.IsValidationSuspended)
				{
					ParentShipment.Validation.ValidateJS_JS_ColoadMasterShipment();
				}
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			var consolToRemove = (CommonConsol)bizO;
			base.OnRemoved(consolToRemove);

			if (!IsUpdatingByDataRefreshBus)
			{
				ConsolShipmentRelationshipHelper.ShipmentRemovedFromConsol(consolToRemove, ParentShipment);

				if (consolToRemove.Shipments.Contains(ParentShipment))
				{
					consolToRemove.Shipments.Remove(ParentShipment);
				}
			}
			else
			{
				ConsolShipmentRelationshipHelper.UnpackShipment(consolToRemove, ParentShipment);

				if (ParentShipment.OuterPackLines.CurrentConsol == consolToRemove)
				{
					ParentShipment.OuterPackLines.CurrentConsol = ParentShipment.Consols.Count > 0 ? ParentShipment.Consols[0] : null;
				}
			}

			if (Count <= 1)
			{
				if (ParentShipment != null && !ParentShipment.IsDeleted)
				{
					ParentShipment.JS_JS_ColoadMasterShipmentInfo.RefreshBinding();
				}
			}

			ParentShipment.JS_JK_ConsolIDInfo.RefreshBinding();

			consolToRemove.JK_RL_NKLoadPortInfo.ValueChanged -= new EventHandler(OnConsolLoadOrDischargePortChanged);
			consolToRemove.JK_RL_NKDischargePortInfo.ValueChanged -= new EventHandler(OnConsolLoadOrDischargePortChanged);
		}

		#endregion

		#endregion

		#region Validation

		public void CheckUniqueOriginAndDestination()
		{
			IDictionary loadingPorts = new HybridDictionary();
			IDictionary dischargePorts = new HybridDictionary();

			foreach (CommonConsol consol in this.Cast<CommonConsol>().Where(c => c.JK_RL_NKLoadPort != c.JK_RL_NKDischargePort && !c.IsDomesticRailOrRoad()))
			{
				if (!consol.IsValidationSuspended)
				{
					consol.RemoveRowError(DuplicateLoadError);
					consol.RemoveRowError(DuplicateDischargeError);
				}

				CommonConsol matchingLoadingConsol = loadingPorts[consol.JK_RL_NKLoadPort] as CommonConsol;
				if (matchingLoadingConsol != null && !matchingLoadingConsol.IsValidationSuspended && !consol.IsValidationSuspended)
				{
					string errorMessage = DuplicateLoadError;
					if (!matchingLoadingConsol.Notifications.ContainsNotificationContaining(errorMessage))
					{
						matchingLoadingConsol.AddRowError(errorMessage);
					}

					consol.AddRowError(errorMessage);
				}

				CommonConsol matchingDischargeConsol = dischargePorts[consol.JK_RL_NKDischargePort] as CommonConsol;
				if (matchingDischargeConsol != null && !matchingDischargeConsol.IsValidationSuspended && !consol.IsValidationSuspended)
				{
					string errorMessage = DuplicateDischargeError;
					if (!matchingDischargeConsol.Notifications.ContainsNotificationContaining(errorMessage))
					{
						matchingDischargeConsol.AddRowError(errorMessage);
					}

					consol.AddRowError(errorMessage);
				}

				loadingPorts[consol.JK_RL_NKLoadPort] = consol;
				dischargePorts[consol.JK_RL_NKDischargePort] = consol;
			}
		}

		string DuplicateLoadError
		{
			get { return Res.GetString("1f50dadf-b729-47ef-a56e-369834ba3871", "Multiple consols on a Shipment cannot have the same port of loading."); }
		}

		string DuplicateDischargeError
		{
			get { return Res.GetString("80928cfd-5b23-4e28-a1e2-5687751729d8", "Multiple consols on a Shipment cannot have the same port of discharge."); }
		}

		void CheckMatchingForwardingAgentsWithShipmentRelatedParties(CommonConsol consol)
		{
			if (!consol.IsValidationSuspended)
			{
				var receivingAgentsWarning = FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedReceivingAgents(new[] { ParentShipment }, new[] { consol });
				var sendingAgentsWarning = FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedSendingAgents(new[] { ParentShipment }, new[] { consol });

				if (!string.IsNullOrEmpty(receivingAgentsWarning))
				{
					consol.AddRowWarning(receivingAgentsWarning);
				}

				if (!string.IsNullOrEmpty(sendingAgentsWarning))
				{
					consol.AddRowWarning(sendingAgentsWarning);
				}
			}
		}

		#endregion

		#region Earliest/Latest consols

		public CommonConsol GetEarliestConsol()
		{
			var sortedConsols = this.Cast<CommonConsol>().ToArray();
			MovementLegComparer.SortMovementLegsByPorts(sortedConsols);
			return sortedConsols.FirstOrDefault();
		}

		public CommonConsol GetLatestConsol()
		{
			var sortedConsols = this.Cast<CommonConsol>().ToArray();
			MovementLegComparer.SortMovementLegsByPorts(sortedConsols);
			return sortedConsols.LastOrDefault();
		}

		#endregion

		#region Defaulting Behaviour

		void DefaultPlannedCarrier(CommonConsol consol)
		{
			if (consol.JK_OA_ShippingLineAddress == ZGuid.Empty)
			{
				consol.JK_OA_ShippingLineAddress = ParentShipment.JS_OA_BookedShippingLineAddress;
			}
		}

		#endregion
	}
}
