using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentContainerDependentCollection : DependentBusinessObjectCollection<AgencyShipmentContainer, AgencyShipment>,
		IPackLineParentChangeNotifiable,
		ICollectionChangeTrackable<AgencyShipmentContainer>,
		ICollectionChangeTrackable<UNDGDataItem>
	{
		public AgencyShipmentContainerDependentCollection(AgencyShipment shipment, ZString purpose)
			: base(shipment, shipment.Factory)
		{
			this.purpose = Argument.NotNullOrEmpty(purpose, "purpose");
			Factory.Saved += OnFactoryOnSaved;
		}

		protected readonly ZString purpose;

		public void RefreshContainerMovements()
		{
			foreach (AgencyShipmentContainer container in this)
			{
				container.RefreshMovements();
			}
		}

		#region IPackLineParentChangeNotifiable

		bool IsImportingData
		{
			get { return ((ISupportDataImporting)Master).IsImportingData; }
		}

		public void NotifyWeightChanged(ZDecimal oldWeight, ZDecimal newWeight)
		{
			if (!IsImportingData)
			{
				EnsureCollectionHasAtLeastOneElement();
				if (Count == 1)
				{
					UpdateElements(JobContainerSchema.JC_GrossWeight, oldWeight, newWeight);
				}
			}
		}

		public void NotifyWeightUQChanged(ZString oldWeightUQ, ZString newWeightUQ)
		{
			if (!IsImportingData)
			{
				EnsureCollectionHasAtLeastOneElement();
				UpdateElements(JobContainerSchema.JC_GrossWeightUQ, oldWeightUQ, newWeightUQ);
			}
		}

		public void NotifyVolumeChanged(ZDecimal oldVolume, ZDecimal newVolume)
		{
			if (!IsImportingData)
			{
				EnsureCollectionHasAtLeastOneElement();
				if (Count == 1)
				{
					UpdateElements(JobContainerSchema.JC_GrossVolume, oldVolume, newVolume);
				}
			}
		}

		public void NotifyVolumeUQChanged(ZString oldVolumeUQ, ZString newVolumeUQ)
		{
			if (!IsImportingData)
			{
				EnsureCollectionHasAtLeastOneElement();
				UpdateElements(JobContainerSchema.JC_GrossVolumeUQ, oldVolumeUQ, newVolumeUQ);
			}
		}

		public void NotifyLoadingMetersChanged(ZDecimal oldValue, ZDecimal newValue)
		{
		}

		public void NotifyPackageCountChanged(ZInt oldCount, ZInt newCount)
		{
			if (!IsImportingData)
			{
				EnsureCollectionHasAtLeastOneElement();
				if (Count == 1)
				{
					var container = this[0];
					if ((container.JC_ContainerCount == 1 || container.JC_ContainerCount == oldCount) && newCount <= short.MaxValue)
					{
						container.JC_ContainerCount = (short)newCount;
					}
				}
			}
		}

		public void NotifyPackageTypeChanged(ZString oldType, ZString newType)
		{
			if (!IsImportingData)
			{
				if (Count == 0)
				{
					EnsureCollectionHasAtLeastOneElement();
					oldType = this[0].JC_F3_NKPackType;
				}

				if (Count == 1)
				{
					UpdateElements(JobContainerSchema.JC_F3_NKPackType, oldType, newType);
				}
			}
		}

		public void NotifyDescriptionChanged(ZString oldValue, ZString newValue)
		{
			if (!IsImportingData)
			{
				EnsureCollectionHasAtLeastOneElement();
				UpdateElements(JobContainerSchema.JC_Description, oldValue, newValue);
			}
		}

		void EnsureCollectionHasAtLeastOneElement()
		{
			if (Count == 0)
			{
				AddNew();
			}
		}

		void UpdateElements<T>(SchemaColumn schemaColumn, T oldValue, T newValue) where T : IZType
		{
			foreach (var container in this)
			{
				if (container[schemaColumn].Equals(oldValue))
				{
					container[schemaColumn] = newValue;
				}
			}
		}

		#endregion

		#region Implementation

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(AgencyShipmentContainer);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return JobContainerSchema.JC_JS_FCLBookingOnlyLink; }
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			AgencyShipmentContainer container = (AgencyShipmentContainer)dependent;
			base.SetCollectionRelationships(container);
			container.JC_Purpose = purpose;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JobContainerSchema.JC_Purpose, purpose);
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var container = (AgencyShipmentContainer)child;
			container.JC_ContainerMode = Master.JS_PackingMode;

			if (container.IsTopLevelPack)
			{
				if (!container.IsRollOnRollOff)
				{
					container.JC_F3_NKPackType = Master.JS_F3_NKPackType;
				}

				if (Master.IsBillOfLadingStage)
				{
					container.JC_TotalUnitOfMeasure = AgencyRegistry.Instance.DefaultBillDimensionUnit.Value;
				}
				else
				{
					container.JC_TotalUnitOfMeasure = AgencyRegistry.Instance.DefaultBookingDimensionUnit.Value;
				}
			}
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);

			if (!bizO.IsRefreshingByDataRefreshBus)
			{
				var container = (AgencyShipmentContainer)bizO;
				if (container != null && container.IsInDatabase && container.Booking != null)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					container.Booking.Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "{0} DELETED", container.JC_ContainerCode));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		#endregion

		#region Change Tracking

		public void ResetChangeTracking()
		{
			ResetContainerChangeTracking();
			ResetUNDGChangeTracking();
		}

		void OnFactoryOnSaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				ResetChangeTracking();
			}
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();
			ResetChangeTracking();
		}

		#region ICollectionChangeTrackable<AgencyShipmentContainer>

		readonly Dictionary<ZGuid, AgencyShipmentContainer> originalContainers = new Dictionary<ZGuid, AgencyShipmentContainer>();

		public void ResetContainerChangeTracking()
		{
			originalContainers.Clear();
			foreach (AgencyShipmentContainer bizObj in this)
			{
				originalContainers.Add(bizObj.PK, bizObj);
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (IsRefreshingByDataRefreshBus)
			{
				try
				{
					if (!originalContainers.ContainsKey(bizOAdded.PK))
					{
						originalContainers.Add(bizOAdded.PK, (AgencyShipmentContainer)bizOAdded);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var container = bizOAdded as AgencyShipmentContainer;
					var message = $@"Current container was used for BillOfLading.
Factory = {Factory._Instance},{Factory.NameForDebugging},{Factory.RefreshEnabled}
PK = {container.PK}
ContainerCode = {container.ContainerCode}
JC_SystemCreateTimeUtc = {container.JC_SystemCreateTimeUtc}
JC_SystemLastEditTimeUtc = {container.JC_SystemLastEditTimeUtc}
JC_JK = {container.JC_JK}
JC_JKInfo = {container.JC_JKInfo}
JC_JSB_SupplierBooking = {container.JC_JSB_SupplierBooking}
JC_ContainerNum = {container.JC_ContainerNum}
JC_ImportReleaseOrderStatus = {container.JC_ImportReleaseOrderStatus}
JC_Calc_ImportDetentionOverdueDays = {container.JC_Calc_ImportDetentionOverdueDays}
JC_Calc_ExportDetentionOverdueDays = {container.JC_Calc_ExportDetentionOverdueDays}
JC_Calc_ImportDetentionFreeDays = {container.JC_Calc_ImportDetentionFreeDays}
JC_Calc_ExportDetentionFreeDays = {container.JC_Calc_ExportDetentionFreeDays}
CustomsEntryNumberType = {container.CustomsEntryNumberType}
CustomsEntryNumber = {container.CustomsEntryNumber}
BillContainersEntryNumberType = {container.BillContainersEntryNumberType}
BillContainersEntryNumber = {container.BillContainersEntryNumber}
GrossWeightVerifiedByPK = {container.GrossWeightVerifiedByPK}
VerifiedByCompany = {container.VerifiedByCompany}
VerifiedByPerson = {container.VerifiedByPerson}
VerifiedByPhone = {container.VerifiedByPhone}
VerifiedByEmail = {container.VerifiedByEmail}
VerifiedMethod = {container.VerifiedMethod}
Exception = {ex.Message}
";
					ErrorReporter.ReportOnce("ContainerUsedInBillOfLadingFrom", message);
				}
			}
		}

		protected override void OnRemoved(BusinessObject bizORemoved)
		{
			base.OnRemoved(bizORemoved);

			if (IsRefreshingByDataRefreshBus)
			{
				if (originalContainers.ContainsKey(bizORemoved.PK))
				{
					originalContainers.Remove(bizORemoved.PK);
				}
			}
		}

		public IEnumerable<AgencyShipmentContainer> AddedElements
		{
			get
			{
				return this.Cast<AgencyShipmentContainer>()
					.Except(originalContainers.Values);
			}
		}

		public IEnumerable<AgencyShipmentContainer> UpdatedElements
		{
			get
			{
				return this.Cast<AgencyShipmentContainer>()
					.Where(bizObj => bizObj.IsInDatabase && bizObj.HasChanges);
			}
		}

		public IEnumerable<AgencyShipmentContainer> RemovedElements
		{
			get
			{
				return originalContainers.Values
					.Where(bizObj => bizObj.IsInDatabase)
					.Except(this.Cast<AgencyShipmentContainer>());
			}
		}

		public IEnumerable<AgencyShipmentContainer> ChangedElements
		{
			get { return AddedElements.Concat(UpdatedElements).Concat(RemovedElements); }
		}

		#endregion

		#region ICollectionChangeTrackable<UNDGDataItem>

		readonly List<UNDGDataItem> originalUNDGs = new List<UNDGDataItem>();

		void ResetUNDGChangeTracking()
		{
			originalUNDGs.Clear();
			originalUNDGs.AddRange(UNDGs);
		}

		IEnumerable<UNDGDataItem> UNDGs
		{
			get
			{
				return this.Cast<AgencyShipmentContainer>()
					.SelectMany(container => container.UNDGs);
			}
		}

		IEnumerable<UNDGDataItem> ICollectionChangeTrackable<UNDGDataItem>.AddedElements
		{
			get { return UNDGs.Except(originalUNDGs); }
		}

		IEnumerable<UNDGDataItem> ICollectionChangeTrackable<UNDGDataItem>.UpdatedElements
		{
			get { return UNDGs.Where(undg => undg.IsInDatabase && undg.HasChanges); }
		}

		IEnumerable<UNDGDataItem> ICollectionChangeTrackable<UNDGDataItem>.RemovedElements
		{
			get { return originalUNDGs.Except(UNDGs); }
		}

		IEnumerable<UNDGDataItem> ICollectionChangeTrackable<UNDGDataItem>.ChangedElements
		{
			get
			{
				ICollectionChangeTrackable<UNDGDataItem> undgsChangeTrackable = this;

				return undgsChangeTrackable.AddedElements
					.Concat(undgsChangeTrackable.UpdatedElements)
					.Concat(undgsChangeTrackable.RemovedElements);
			}
		}

		#endregion

		#endregion
	}
}




