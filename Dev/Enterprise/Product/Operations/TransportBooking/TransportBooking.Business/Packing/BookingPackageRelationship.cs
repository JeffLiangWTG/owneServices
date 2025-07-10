using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public class BookingPackageRelationship : ICollectionRelationship
	{
		public BookingPackageRelationship(DtbBooking booking)
		{
			Booking = booking;
			HookPackageDivots();
		}

		void HookPackageDivots()
		{
			// if booking is deleted we should unhook the instructions collection
			// but we shouldn't need to as Booking.PackageDivots.CountChanged will never be called after the booking is deleted
			Booking.PackageDivots.CountChanged += new EventHandler(InstructionPackageDivots_CountChanged);
			HookUnhookEachPackageDivot();
		}

		void InstructionPackageDivots_CountChanged(object sender, EventArgs e)
		{
			HookUnhookEachPackageDivot();

			if (!IsRebuilding)
			{
				OnRefreshed();
			}
		}

		void HookUnhookEachPackageDivot()
		{
			foreach (var packageDivot in PackageDivots_Listening.ToArray())
			{
				if (!Booking.PackageDivots.Contains(packageDivot))
				{
					UnHook(packageDivot);
				}
			}

			foreach (DtbBookingInstructionPkgDivot packageDivot in Booking.PackageDivots)
			{
				if (!PackageDivots_Listening.Contains(packageDivot))
				{
					Hook(packageDivot);
				}
			}
		}

		void Hook(DtbBookingInstructionPkgDivot packageDivot)
		{
			Argument.NotNull(packageDivot, "packageDivot");

			PackageDivots_Listening.Add(packageDivot);
			packageDivot.KD_KP_PackageInfo.ValueChanged += new EventHandler(KD_KP_PackageInfo_ValueChanged);
		}

		void UnHook(DtbBookingInstructionPkgDivot packageDivot)
		{
			Argument.NotNull(packageDivot, "packageDivot");

			PackageDivots_Listening.Remove(packageDivot);
			packageDivot.KD_KP_PackageInfo.ValueChanged -= new EventHandler(KD_KP_PackageInfo_ValueChanged);
		}

		void KD_KP_PackageInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!IsRebuilding)
			{
				OnRefreshed();
			}
		}

		HashSet<DtbBookingInstructionPkgDivot> PackageDivots_Listening
		{
			get { return packageDivots_Listening ?? (packageDivots_Listening = new HashSet<DtbBookingInstructionPkgDivot>()); }
		}

		HashSet<DtbBookingInstructionPkgDivot> packageDivots_Listening;

		BusinessObject ICollectionRelationship.Master
		{
			get { return Booking; }
		}

		readonly DtbBooking Booking;

		ZQuery ICollectionRelationship.RelationshipFilter
		{
			get { return BuildRelationshipFilter(false); }
		}

		ZQuery BuildRelationshipFilter(bool ignoreActiveFilter)
		{
			ZQuery result = null;

			try
			{
				IsRebuilding = true;

				if (Booking.PackageDivots.Count == 0)
				{
					result = ZQuery.NoResultQuery;
				}
				else
				{
					var packagePks = Array.ConvertAll(Booking.PackageDivots.Cast<DtbBookingInstructionPkgDivot>().ToArray(), d => d.KD_KP_Package);
					result = new ZQuery(PkgPackageSchema.PK, packagePks);
					result.AddToFilter(AdditionalFilter);
					result.IgnoreActiveFilter = ignoreActiveFilter;
					result.ModificationsEnabled = false;
				}
			}
			finally
			{
				IsRebuilding = false;
			}

			return result;
		}

		bool IsRebuilding;

		bool ICollectionRelationship.MatchesRelationshipFilter(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
		{
			Argument.NotNull(businessObject, "businessObject");

			return businessObject.MatchesFilter(BuildRelationshipFilter(ignoreActiveFilter));
		}

		ICollectionRelationship ICollectionRelationship.AddFilter(ZQuery additionalFilter)
		{
			var result = (BookingPackageRelationship)MemberwiseClone();

			if (additionalFilter != null)
			{
				result.AdditionalFilter = new ZQuery(AdditionalFilter, additionalFilter);
				result.AdditionalFilter.ModificationsEnabled = false;
			}

			return result;
		}

		ZQuery AdditionalFilter;

		void OnRefreshed()
		{
			if (Refreshed != null)
			{
				Refreshed(this, EventArgs.Empty);
			}
		}

		event EventHandler ICollectionRelationship.RelationshipFilterChanged
		{
			add { Refreshed += value; }
			remove { Refreshed -= value; }
		}

		event EventHandler Refreshed;

		BusinessObject[] ICollectionRelationship.LoadBusinessObjects(BusinessObjectFactory factory, ZQuery filter)
		{
			return factory.Load<PkgPackage>(new ZQuery(filter, BuildRelationshipFilter(false)));
		}

		bool ICollectionRelationship.HasChangesIncludingRelationship(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");

			return businessObject.HasChanges;
		}

		void ICollectionRelationship.ClearHasChangesIncludingRelationship(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");

			IBusinessObjectState businessObjectState = businessObject;
			businessObjectState.ClearHasChangesIncludingChildren();
		}

		bool ICollectionRelationship.SupportsAddToRelationship()
		{
			return false;
		}

		void ICollectionRelationship.AddToRelationship(BusinessObject businessObject)
		{
			throw new InvalidOperationException("Can't add using this relationship");
		}

		void ICollectionRelationship.RemoveFromRelationship(BusinessObject businessObject)
		{
			throw new InvalidOperationException("Can't remove using this relationship");
		}

		void ICollectionRelationship.Clear()
		{
			throw new InvalidOperationException("Can't clear using this relationship");
		}
	}
}
