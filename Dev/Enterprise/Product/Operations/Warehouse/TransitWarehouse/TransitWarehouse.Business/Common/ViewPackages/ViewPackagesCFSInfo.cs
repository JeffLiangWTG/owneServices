using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Warehouse.Transit.Business
{
	public class ViewPackagesCFSInfo : NonPersistentBusinessObject
	{
		public ViewPackagesCFSInfo(BusinessObjectFactory factory, ZGuid cfsAddressPK, ViewPackagesCFSInfoController controller) : base(factory)
		{
			CFSAddressPK = cfsAddressPK;
			Controller = controller;

			SetProperties(Controller.Manager);
		}

		void SetProperties(ViewPackagesManager manager)
		{
			if (TransitWarehouse != null)
			{
				var viewPacakgesQuery = TransitWarehouseHelper.GetViewPackagesQuery(Controller.Manager.Parent.JobNumber, Controller.Manager.Parent.Identifier, TransitWarehouse.PK);
				var packageStates = Factory.Load<WhsItemPackageState>(viewPacakgesQuery);
				var packageStatesWithPackages = packageStates.Where(p => p.WPS_KP_Package.IsValid).ToArray();
				SetWarehouseStatus(packageStatesWithPackages);
				SetTotalProperties(packageStatesWithPackages);
				SetWeightAndVolume(packageStatesWithPackages);
				SetBookingConfirmed(TransitWarehouse);
			}
		}

		void SetTotalProperties(WhsItemPackageState[] packageStatesWithPackages)
		{
			TotalPackages = packageStatesWithPackages.Sum(p => p.Package.KP_PackageQty);

			var packageStatesWithMatchingStatus = packageStatesWithPackages.Where(p => p.WPS_KP_Package.IsValid && HasMatchingStatus(p, WarehouseStatus)).ToArray();
			TotalPackagesWithMatchingStatus = packageStatesWithMatchingStatus.Sum(p => p.Package.KP_PackageQty);
		}

		void SetWeightAndVolume(WhsItemPackageState[] packageStatesWithPackages)
		{
			var packages = packageStatesWithPackages.Select(p => p.Package).ToArray();
			TotalWeight = TransitWarehouseHelper.CalculateTotalWeight(packages, TotalWeightUQ);
			TotalVolume = TransitWarehouseHelper.CalculateTotalVolume(TotalVolumeUQ, packages);

			if (WarehouseStatus != TransitWarehouseJobsStatus.Codes.Receiving
					|| WarehouseStatus != TransitWarehouseJobsStatus.Codes.Dispatching)
			{
				var packageStatesWithMatchingStatus = packageStatesWithPackages.Where(p => HasMatchingStatus(p, WarehouseStatus)).ToArray();
				var totalWithMatchingStatus = packageStatesWithMatchingStatus.Sum(p => p.Package.KP_PackageQty);
				var packagesWithMatchingStatus = packageStatesWithMatchingStatus.Select(p => p.Package).ToArray();
				TotalWeightWithMatchingStatus = TransitWarehouseHelper.CalculateTotalWeight(packagesWithMatchingStatus, TotalWeightUQ);
				TotalVolumeWithMatchingStatus = TransitWarehouseHelper.CalculateTotalVolume(TotalVolumeUQ, packagesWithMatchingStatus);
			}
		}

		void SetWarehouseStatus(WhsItemPackageState[] packageStates)
		{
			var statuses = packageStates.Select(p => p.WPS_Status.ToString()).Distinct().ToHashSet();
			var departedStatuses = new HashSet<string>(
				new[] {
						TransitWarehouseStatuses.Codes.Departed,
						TransitWarehouseStatuses.Codes.FreightLoaded,
						TransitWarehouseStatuses.Codes.Finalized
				});

			var arrivedStatuses = new HashSet<string>(
				new[] {
						TransitWarehouseStatuses.Codes.Arrived,
						TransitWarehouseStatuses.Codes.AdjustedOut,
						TransitWarehouseStatuses.Codes.Putaway,
						TransitWarehouseStatuses.Codes.Picked,
						TransitWarehouseStatuses.Codes.Committed,
						TransitWarehouseStatuses.Codes.Staged
				});

			var arrivedStatusesWithBooked = new HashSet<string>(
			new[] {
						TransitWarehouseStatuses.Codes.Booked,
						TransitWarehouseStatuses.Codes.Arrived,
						TransitWarehouseStatuses.Codes.AdjustedOut,
						TransitWarehouseStatuses.Codes.Putaway,
						TransitWarehouseStatuses.Codes.Picked,
						TransitWarehouseStatuses.Codes.Committed,
						TransitWarehouseStatuses.Codes.Staged
			});

			if (statuses.Count == 0)
			{
				WarehouseStatus = Res.GetString("a74f6176-22fd-42e3-8c1b-c3adaa94293d", "-");
			}
			else if (statuses.All(s => s == TransitWarehouseStatuses.Codes.Booked))
			{
				WarehouseStatus = TransitWarehouseJobsStatus.Descriptions.Booked;
			}
			else if (!statuses.Except(departedStatuses).Any())
			{
				WarehouseStatus = TransitWarehouseJobsStatus.Descriptions.Dispatched;
			}
			else if (!statuses.Except(arrivedStatuses).Any())
			{
				WarehouseStatus = TransitWarehouseJobsStatus.Descriptions.Received;
			}
			else if (!statuses.Except(arrivedStatusesWithBooked).Any())
			{
				WarehouseStatus = TransitWarehouseJobsStatus.Descriptions.Receiving;
			}
			else
			{
				WarehouseStatus = TransitWarehouseJobsStatus.Descriptions.Dispatching;
			}
		}

		void SetBookingConfirmed(WhsWarehouse transitWarehouse)
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, Controller.Manager.Parent.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.BookingConfirmedCode);

			var bookingConfirmedLogs = Factory.Load<StmALog>(query);
			IsRCNBookingConfirmed = bookingConfirmedLogs.Any(l => HasEventWithTypeAndLOC(l, TransitConstants.TransitReceiveJobType, transitWarehouse.WarehouseAddress.OA_City));
			IsDCNBookingConfirmed = bookingConfirmedLogs.Any(l => HasEventWithTypeAndLOC(l, TransitConstants.TransitDispatchJobType, transitWarehouse.WarehouseAddress.OA_City));
		}

		bool HasEventWithTypeAndLOC(StmALog log, string type, string city)
		{
			var parameters = StmALog.GetParametersFromReference(log.SL_Reference);
			if (parameters.TryGetValue("TYP", out string jobType) && parameters.TryGetValue("LOC", out string loc))
			{
				return jobType.ToUpperInvariant() == type.ToUpperInvariant() && city.ToUpperInvariant() == loc.ToUpperInvariant();
			}
			return false;
		}

		ZGuid CFSAddressPK { get; }
		ViewPackagesCFSInfoController Controller { get; }

		public bool IsSelected { get; set; }

		#region SelectAndDeselectEvents

		public void Deselect()
		{
			DeselectedEvent?.Invoke(this, null);
			IsSelected = false;
		}

		public void Select()
		{
			SelectedEvent?.Invoke(this, null);
			IsSelected = true;
		}

		public EventHandler DeselectedEvent;
		public EventHandler SelectedEvent;

		#endregion

		#region WarehouseName

		public ZString WarehouseName
		{
			get
			{
				if (string.IsNullOrEmpty(warehouseName))
				{
					var warehouse = TransitWarehouse;
					warehouseName = warehouse?.WW_WarehouseNameMultilingual ?? (OrgAddress?.Header?.OH_FullName ?? ZString.Empty);
				}
				return warehouseName;
			}
		}
		ZString warehouseName;

		#endregion

		#region OrgAddress

		public OrgAddress OrgAddress
		{
			get
			{
				if (orgAddress == null)
				{
					orgAddress = Factory.Load<OrgAddress>(CFSAddressPK);
				}
				return orgAddress;
			}
		}
		OrgAddress orgAddress;

		#endregion

		#region TransitWarehouse

		public WhsWarehouse TransitWarehouse
		{
			get
			{
				if (transitWarehouse == null)
				{
					transitWarehouse = TransitWarehouseHelper.GetTransitWarehouse(Factory, CFSAddressPK);
				}
				return transitWarehouse;
			}
		}
		WhsWarehouse transitWarehouse;

		#endregion

		#region Warehouse Related Properties

		public string WarehouseStatus { get; private set; }

		#region Total Packages

		public string NumberOfPackagesDisplayText
		{
			get
			{
				if (WarehouseStatus == TransitWarehouseJobsStatus.Descriptions.Receiving
					|| WarehouseStatus == TransitWarehouseJobsStatus.Descriptions.Dispatching)
				{
					return Invariant($"{TotalPackagesWithMatchingStatus} of {TotalPackages}"); // Display string on View Packages form.
				}

				return Invariant($"{TotalPackages}");
			}
		}
		public int TotalPackagesWithMatchingStatus { get; private set; }

		bool HasMatchingStatus(WhsItemPackageState p, string warehouseStatus)
		{
			return warehouseStatus == TransitWarehouseJobsStatus.Descriptions.Receiving && p.WPS_WRH_TransitReceiveHeader.IsValid
				|| warehouseStatus == TransitWarehouseJobsStatus.Descriptions.Dispatching && p.WPS_WDH_TransitDispatchHeader.IsValid;
		}

		int TotalPackages { get; set; }

		#endregion

		#region Total Weight and Volume

		decimal TotalWeight { get; set; }

		decimal TotalVolume { get; set; }

		public ZPropertyInfo TotalVolumeInfo => GetZPropertyInfo(nameof(TotalVolume));

		decimal TotalWeightWithMatchingStatus { get; set; }

		decimal TotalVolumeWithMatchingStatus { get; set; }

		ZString TotalVolumeUQ
		{
			get { return PackingRegistry.Instance.VolumeUnit.Value; }
		}

		public ZPropertyInfo TotalVolumeUQInfo => GetZPropertyInfo(nameof(TotalVolumeUQ));

		ZString TotalWeightUQ
		{
			get { return PackingRegistry.Instance.WeightUnit.Value; }
		}

		public ZPropertyInfo TotalWeightUQInfo => GetZPropertyInfo(nameof(TotalWeightUQ));

		public string TotalWeightDisplayText
		{
			get
			{
				if (WarehouseStatus == TransitWarehouseJobsStatus.Descriptions.Receiving
					|| WarehouseStatus == TransitWarehouseJobsStatus.Descriptions.Dispatching)
				{
					return Invariant($"{ZString.Format("{0:#,##0.#}", TotalWeightWithMatchingStatus)} / {ZString.Format("{0:#,##0.#}", TotalWeight)} {TotalWeightUQ}"); // Display string on View Packages form.
				}

				return Invariant($"{ZString.Format("{0:#,##0.#}", TotalWeight)} {TotalWeightUQ}"); // Display string on View Packages form.
			}
		}

		public string TotalVolumeDisplayText
		{
			get
			{
				if (WarehouseStatus == TransitWarehouseJobsStatus.Descriptions.Receiving
					|| WarehouseStatus == TransitWarehouseJobsStatus.Descriptions.Dispatching)
				{
					return Invariant($"{ZString.Format("{0:#,##0.#}", TotalVolumeWithMatchingStatus)} / {ZString.Format("{0:#,##0.#} {1}", TotalVolume, TotalVolumeUQ)}"); // Display string on View Packages form.
				}

				return Invariant($"{ZString.Format("{0:#,##0.#} {1}", TotalVolume, TotalVolumeUQ)}"); // Display string on View Packages form.
			}
		}

		#endregion

		public bool IsRCNBookingConfirmed { get; private set; }

		public bool IsDCNBookingConfirmed { get; private set; }

		#endregion

		#region SelectCFSDetails

		public void SelectCFS()
		{
			Controller.CFSAddressSelected(this);
		}

		#endregion
	}
}
