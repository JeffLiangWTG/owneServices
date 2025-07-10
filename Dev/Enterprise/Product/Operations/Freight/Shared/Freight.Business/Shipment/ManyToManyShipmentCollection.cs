using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	[ModuleID(ModuleId.JobShipment)]
	public abstract class ManyToManyShipmentCollection : ManyToManyBusinessObjectCollection
	{
		public ManyToManyShipmentCollection(CommonConsol consol)
			: base(consol)
		{
			CountChanged += new CollectionCountChangedEventHandler(CollectionCountChanged);
		}

		protected CommonConsol ParentConsol
		{
			get { return (CommonConsol)base.fAssociatedObject; }
		}

		#region On Added

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			CommonShipment shipment = bizOAdded as CommonShipment;

			if (!IsLoading)
			{
				if (!shipment.Consols.Contains(ParentConsol.PK))
				{
					using (shipment.Consols.SuppressAddingATCEvent())
					{
						shipment.Consols.Add(ParentConsol);
					}
				}

				if (!IsAddingRelationshipToNewObject)
				{
					if (!shipment.IsValidationSuspended)
					{
						CheckForDuplicateLoadingAndDischarge(shipment);
					}
				}

				shipment.ResetDeclarationForDocuments();

				//Check for direct consol / shipment
				if (ParentConsol.IsDirect && Count > 1 && !ParentConsol.IsValidationSuspended)
				{
					ParentConsol.Validation.ValidateJK_AgentType();
				}

				if (!shipment.IsSuppressedETAETDOnAttachToConsol)
				{
					var updatedETA = false;
					var updatedETD = false;

					//Check ETD and ETA
					if (shipment.JS_E_DEP > ParentConsol.JK_JX_JA_E_DEP && !((ISupportDataImporting)ParentConsol).IsImportingData &&
						(!BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(Factory) || !shipment.JS_E_DEP.IsValid))
					{
						shipment.JS_E_DEP = ParentConsol.JK_JX_JA_E_DEP;
						updatedETD = true;
					}
					if (shipment.JS_E_ARV < ParentConsol.JK_JX_JB_E_ARV && !((ISupportDataImporting)ParentConsol).IsImportingData &&
						(!BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(Factory) || !shipment.JS_E_ARV.IsValid))
					{
						shipment.JS_E_ARV = ParentConsol.JK_JX_JB_E_ARV;
						updatedETA = true;
					}

					if (!BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(shipment.Factory) || updatedETA)
					{
						shipment.UpdateETAWithPortDefaultDeliveryTimeIfEmpty();
					}
					if (!BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(shipment.Factory) || updatedETD)
					{
						shipment.UpdateETDeliveryWithPortDefaultDeliveryTime();
					}
				}
			}

			HookShipmentForConsolTotals(shipment);
		}

		public bool IsAddingRelationshipToNewObject
		{
			get { return fIsAddingRelationshipToNewObject; }
			set { fIsAddingRelationshipToNewObject = value; }
		}

		bool fIsAddingRelationshipToNewObject;

		#endregion

		#region Remove

		public override void Remove(BusinessObject elementToRemove)
		{
			if (!IsUpdatingByDataRefreshBus)
			{
				CommonShipment shipment = (CommonShipment)elementToRemove;
				if (shipment.Consols.Contains(ParentConsol))
				{
					shipment.Consols.Remove(ParentConsol);
				}
			}

			if (!elementToRemove.IsValidationSuspended)
			{
				CheckForDuplicateLoadingAndDischarge((CommonShipment)elementToRemove);
			}

			base.Remove(elementToRemove);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			UnHookShipmentForConsolTotals((CommonShipment)bizO);

			if (ParentConsol.IsDirect && !ParentConsol.IsValidationSuspended)
			{
				ParentConsol.Validation.ValidateJK_AgentType();
			}
		}

		#endregion

		#region Update ParentConsol Totals

		void CollectionCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdateParentConsolBinding();
		}

		IDisposable SuspendParentConsolBinding() => new DisposableAction(
			() => suspendParentConsolBindingCounter++,
			() =>
			{
				suspendParentConsolBindingCounter--;
				if (suspendParentConsolBindingCounter == 0)
				{
					UpdateParentConsolBinding();
				}
			}
		);

		int suspendParentConsolBindingCounter;
		bool ShouldUpdateParentConsolBinding => suspendParentConsolBindingCounter == 0;

		protected override DisposableList GetAdditionalListChangedSuspenders()
		{
			DisposableList list = null;
			try
			{
				list = base.GetAdditionalListChangedSuspenders() ?? new DisposableList(1);
				list.Add(SuspendParentConsolBinding());
				return list;
			}
			catch
			{
				list?.Dispose();
				throw;
			}
		}

		void UpdateParentConsolBinding()
		{
			if (ShouldUpdateParentConsolBinding)
			{
				ParentConsol.JK_TotalShipmentWeightInfo.RefreshBinding();
				ParentConsol.JK_TotalShipmentWeightUnitInfo.RefreshBinding();
				ParentConsol.JK_TotalShipmentVolumeInfo.RefreshBinding();
				ParentConsol.JK_TotalShipmentVolumeUnitInfo.RefreshBinding();
				ParentConsol.JK_TotalShipmentLoadingMetersInfo.RefreshBinding();
				ParentConsol.JK_TotalShipmentChargeableInfo.RefreshBinding();
				ParentConsol.JK_ConsolChargeableInfo.RefreshBinding();
				ParentConsol.JK_CorrectedConsolVolumeInfo.RefreshBinding();
				ParentConsol.JK_CorrectedConsolWeightInfo.RefreshBinding();
				ParentConsol.JK_TotalShipmentChargeableUnitInfo.RefreshBinding();
				ParentConsol.JK_TotalShipmentQuantityInfo.RefreshBinding();
				ParentConsol.JK_CorrectedConsolWeightUnitInfo.RefreshBinding();
				ParentConsol.JK_CorrectedConsolVolumeUnitInfo.RefreshBinding();
				ParentConsol.JK_Calc_FreeSpaceInfo.RefreshBinding();
				ParentConsol.JK_Calc_ActualVolumeWeightInfo.RefreshBinding();

#if DEBUG
				ParentConsolRefreshBindingCount++;
#endif
			}
		}

#if DEBUG
		public int ParentConsolRefreshBindingCount { get; set; }
#endif

		void HookShipmentForConsolTotals(CommonShipment shipment)
		{
			shipment.JS_ActualWeightInfo.ValueChanged += new EventHandler(OnShipmentJS_ActualWeightChanged);
			shipment.JS_UnitOfWeightInfo.ValueChanged += new EventHandler(OnShipmentJS_UnitOfWeightChanged);
			shipment.JS_ActualVolumeInfo.ValueChanged += new EventHandler(OnShipmentJS_ActualVolumeChanged);
			shipment.JS_UnitOfVolumeInfo.ValueChanged += new EventHandler(OnShipmentJS_UnitOfVolumeChanged);
			shipment.JS_LoadingMetersInfo.ValueChanged += new EventHandler(OnShipmentJS_LoadingMetersChanged);
			shipment.JS_ActualChargeableInfo.ValueChanged += new EventHandler(OnShipmentJS_ActualChargeableChanged);
			shipment.JS_ChargeableUnitInfo.ValueChanged += new EventHandler(OnShipmentJS_ChargeableUnitInfoChanged);
			shipment.JS_OuterPacksInfo.ValueChanged += new EventHandler(OnShipmentJS_OuterPacksChanged);
		}

		void UnHookShipmentForConsolTotals(CommonShipment shipment)
		{
			shipment.JS_ActualWeightInfo.ValueChanged -= new EventHandler(OnShipmentJS_ActualWeightChanged);
			shipment.JS_UnitOfWeightInfo.ValueChanged -= new EventHandler(OnShipmentJS_UnitOfWeightChanged);
			shipment.JS_ActualVolumeInfo.ValueChanged -= new EventHandler(OnShipmentJS_ActualVolumeChanged);
			shipment.JS_UnitOfVolumeInfo.ValueChanged -= new EventHandler(OnShipmentJS_UnitOfVolumeChanged);
			shipment.JS_LoadingMetersInfo.ValueChanged -= new EventHandler(OnShipmentJS_LoadingMetersChanged);
			shipment.JS_ActualChargeableInfo.ValueChanged -= new EventHandler(OnShipmentJS_ActualChargeableChanged);
			shipment.JS_ChargeableUnitInfo.ValueChanged -= new EventHandler(OnShipmentJS_ChargeableUnitInfoChanged);
			shipment.JS_OuterPacksInfo.ValueChanged -= new EventHandler(OnShipmentJS_OuterPacksChanged);
		}

		void OnShipmentJS_ActualWeightChanged(object sender, EventArgs e)
		{
			ParentConsol.JK_TotalShipmentWeightInfo.RefreshBinding();
			ParentConsol.JK_CorrectedConsolWeightInfo.RefreshBinding();
		}

		void OnShipmentJS_UnitOfWeightChanged(object sender, EventArgs e)
		{
			ParentConsol.JK_TotalShipmentWeightInfo.RefreshBinding();
			ParentConsol.JK_TotalShipmentWeightUnitInfo.RefreshBinding();
			ParentConsol.JK_TotalShipmentChargeableUnitInfo.RefreshBinding();
			ParentConsol.JK_CorrectedConsolWeightInfo.RefreshBinding();
			ParentConsol.JK_CorrectedConsolWeightUnitInfo.RefreshBinding();
		}

		void OnShipmentJS_ActualVolumeChanged(object sender, EventArgs e)
		{
			ParentConsol.JK_TotalShipmentVolumeInfo.RefreshBinding();
			ParentConsol.JK_CorrectedConsolVolumeInfo.RefreshBinding();
		}

		void OnShipmentJS_UnitOfVolumeChanged(object sender, EventArgs e)
		{
			ParentConsol.JK_TotalShipmentVolumeInfo.RefreshBinding();
			ParentConsol.JK_TotalShipmentVolumeUnitInfo.RefreshBinding();
			ParentConsol.JK_TotalShipmentChargeableUnitInfo.RefreshBinding();
			ParentConsol.JK_CorrectedConsolVolumeInfo.RefreshBinding();
			ParentConsol.JK_CorrectedConsolVolumeUnitInfo.RefreshBinding();
		}

		void OnShipmentJS_LoadingMetersChanged(object sender, EventArgs e)
		{
			ParentConsol.JK_TotalShipmentLoadingMetersInfo.RefreshBinding();
		}

		void OnShipmentJS_ActualChargeableChanged(object sender, EventArgs e)
		{
			ParentConsol.JK_TotalShipmentChargeableInfo.RefreshBinding();
			ParentConsol.JK_ConsolChargeableInfo.RefreshBinding();
			ParentConsol.JK_Calc_FreeSpaceInfo.RefreshBinding();
			ParentConsol.JK_Calc_ActualVolumeWeightInfo.RefreshBinding();
		}

		void OnShipmentJS_ChargeableUnitInfoChanged(object sender, EventArgs e)
		{
			ParentConsol.JK_TotalShipmentChargeableUnitInfo.RefreshBinding();
		}

		void OnShipmentJS_OuterPacksChanged(object sender, EventArgs e)
		{
			ParentConsol.JK_TotalShipmentQuantityInfo.RefreshBinding();
		}

		#endregion

		#region Default Values

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(JobConShipLink); }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			CommonShipment shipment = (CommonShipment)child;

			shipment.IsSettingDefaultValues = true;
			try
			{
				if (!ParentConsol.JK_TransportMode.IsEmpty)
				{
					shipment.JS_TransportMode = ParentConsol.JK_TransportMode;
				}

				ZString defaultPackingMode = FreightUtilities.ShipmentContainerMode(ParentConsol.JK_ConsolMode, ParentConsol.JK_TransportMode);
				if (!defaultPackingMode.IsEmpty)
				{
					shipment.JS_PackingMode = defaultPackingMode;
				}

				if (FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.Value)
				{
					shipment.JS_RL_NKOrigin = ParentConsol.JK_RL_NKLoadPort;
				}

				if (FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.Value)
				{
					shipment.JS_RL_NKDestination = ParentConsol.JK_RL_NKDischargePort;
				}

				shipment.JS_E_DEP = ParentConsol.Transports.DepartureTransport.JW_ETD;
				shipment.JS_E_ARV = ParentConsol.Transports.ArrivalTransport.JW_ETA;

				if (ParentConsol.JK_IsCFS)
				{
					shipment.JS_IsCFSRegistered = true;
				}

				shipment.UpdateETAWithPortDefaultDeliveryTime();
				shipment.UpdateETDeliveryWithPortDefaultDeliveryTime();

				if (ParentConsol.JK_TransportMode == Core.Constants.TransportModes.Sea &&
					ParentConsol.JK_ConsolMode == Core.Constants.ContainerModes.BuyersConsol &&
					shipment.ConsigneePK.IsEmpty &&
					Count > 0)
				{
					shipment.ConsigneePK = ((CommonShipment)this.Elements[0]).ConsigneePK;
				}
			}
			finally
			{
				shipment.IsSettingDefaultValues = false;
			}
		}

		#endregion

		#region Validation

		public void CheckAllShipmentsForDuplicateLoadingAndDischarge()
		{
			foreach (CommonShipment shipment in this)
			{
				if (!shipment.IsValidationSuspended)
				{
					CheckForDuplicateLoadingAndDischarge(shipment);
				}
			}
		}

		protected void CheckForDuplicateLoadingAndDischarge(CommonShipment shipment)
		{
			shipment.RemoveRowError(DuplicateLoadError);
			shipment.RemoveRowError(DuplicateDischargeError);

			if (!shipment.Consols.Contains(ParentConsol))
			{
				return;
			}

			var hasBuyersConsolLeadMaster = shipment.CoLoadMasterShipment != null
					&& shipment.CoLoadMasterShipment.IsBuyersConsolLead;
			if (shipment.IsBuyersConsolLead
				|| (hasBuyersConsolLeadMaster
					&& shipment.CoLoadMasterShipment.Consols.Contains(ParentConsol)))
			{
				return;
			}

			var consolsToCheck = shipment.Consols.ToArray();
			if (hasBuyersConsolLeadMaster)
			{
				consolsToCheck = consolsToCheck.Except(shipment.CoLoadMasterShipment.Consols).ToArray();
			}

			ZString loading = ParentConsol.JK_RL_NKLoadPort;
			ZString discharge = ParentConsol.JK_RL_NKDischargePort;
			var consolLoadPorts = new HashSet<ZString>();
			var consolDiscPorts = new HashSet<ZString>();
			bool duplicateFound = false;

			foreach (CommonConsol consol in consolsToCheck)
			{
				if (consol.PK != ParentConsol.PK)
				{
					var transportLegs = consol.Transports;

					foreach (Transport transport in transportLegs)
					{
						if (transport.JW_RL_NKLoadPort == loading && !consol.IsDomesticRailOrRoad() && !ParentConsol.IsDomesticRailOrRoad())
						{
							shipment.AddRowError(DuplicateLoadError);
							duplicateFound = true;
						}

						if (transport.JW_RL_NKDiscPort == discharge && !consol.IsDomesticRailOrRoad() && !ParentConsol.IsDomesticRailOrRoad())
						{
							shipment.AddRowError(DuplicateDischargeError);
							duplicateFound = true;
						}

						if (duplicateFound)
						{
							return;
						}
					}

					if (!consol.IsDomesticRailOrRoad() && !ParentConsol.IsDomesticRailOrRoad())
					{
						consolLoadPorts.Add(consol.JK_RL_NKLoadPort);
						consolDiscPorts.Add(consol.JK_RL_NKDischargePort);
					}
				}
			}

			foreach (Transport transport in ParentConsol.Transports)
			{
				if (consolLoadPorts.Contains(transport.JW_RL_NKLoadPort))
				{
					shipment.AddRowError(DuplicateLoadError);
					duplicateFound = true;
				}

				if (consolDiscPorts.Contains(transport.JW_RL_NKDiscPort))
				{
					shipment.AddRowError(DuplicateDischargeError);
					duplicateFound = true;
				}

				if (duplicateFound)
				{
					return;
				}
			}
		}

		string DuplicateLoadError
		{
			get { return Res.GetString("0f84e871-4aac-43f1-a7b2-87469e7d4dc9", "Shipment already on a Consol with same Port of Loading."); }
		}

		string DuplicateDischargeError
		{
			get { return Res.GetString("9e91c6d3-cef7-45c2-afa9-4332fe1a2a49", "Shipment already on a Consol with same Port of Discharge."); }
		}

		#endregion
	}
}
