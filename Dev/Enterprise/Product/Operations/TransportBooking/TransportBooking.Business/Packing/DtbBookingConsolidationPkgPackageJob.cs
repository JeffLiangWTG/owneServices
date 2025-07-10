using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.Packing.Business;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConsolidationPkgPackageJob : PkgPackageJob, IDtbBookingConsolidationPkgPackageJob
	{
		public DtbBookingConsolidationPkgPackageJob(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			SubBookingPKsToInclude = new List<ZGuid>();
			SubConsolidationPKsToExclude = new List<ZGuid>();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			SubBookingPKsToInclude = new List<ZGuid>();
			SubConsolidationPKsToExclude = new List<ZGuid>();
		}

		protected override PkgPackageCollection GetPackageCollectionCore()
		{
			if (DoesParentConsolidationHaveAnyAttachedMasterBookings)
			{
				var result = GetPackagesFromSubs();
				SubConsolidationPKsToExclude = new List<ZGuid>();
				return result;
			}
			else
			{
				return base.GetPackageCollectionCore();
			}
		}

		protected override PkgPackage[] GetAllPackagesOnJobCore()
		{
			if (DoesParentConsolidationHaveAnyAttachedMasterBookings)
			{
				return new MasterBookingPkgPackageCollection(this, topLevelOnly: false).ToArray();
			}
			else
			{
				return base.GetAllPackagesOnJobCore();
			}
		}

		public List<ZGuid> SubBookingPKsToInclude { get; set; }

		public List<ZGuid> SubConsolidationPKsToExclude { get; set; }

		protected override bool ContainsPackageCore(PkgPackage package)
		{
			if (DoesParentConsolidationHaveAnyAttachedMasterBookings)
			{
				var allPackages = new MasterBookingPkgPackageCollection(this, topLevelOnly: false);
				return allPackages.Contains(package);
			}

			return base.ContainsPackageCore(package);
		}

		MasterBookingPkgPackageCollection GetPackagesFromSubs()
		{
			return new MasterBookingPkgPackageCollection(this);
		}

		public override bool HasChanges
		{
			get
			{
				if (DoesParentConsolidationHaveAnyAttachedMasterBookings)
				{
					return false;
				}
				else
				{
					return base.HasChanges;
				}
			}
			set
			{
				if (DoesParentConsolidationHaveAnyAttachedMasterBookings)
				{
					base.HasChanges = false;
				}
				else
				{
					base.HasChanges = value;
				}
			}
		}

		bool DoesParentConsolidationHaveAnyAttachedMasterBookings
		{
			get
			{
				return !IsDeleted && ParentConsolidation != null && ParentConsolidation.Bookings != null && ParentConsolidation.Bookings.Any(b => b.KM_IsMaster);
			}
		}

		DtbBookingConsolidation ParentConsolidation
		{
			get
			{
				return ParentJob as DtbBookingConsolidation;
			}
		}

		public override bool ReadOnly { get => DoesParentConsolidationHaveAnyAttachedMasterBookings || base.ReadOnly; }

		public override void Delete()
		{
			if (DoesParentConsolidationHaveAnyAttachedMasterBookings)
			{
				throw new NotSupportedException();
			}
			else
			{
				base.Delete();
			}
		}
	}
}
