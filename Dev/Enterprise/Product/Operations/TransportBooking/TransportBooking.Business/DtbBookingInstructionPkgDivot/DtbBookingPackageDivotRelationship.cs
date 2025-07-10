using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingPackageDivotRelationship : ICollectionRelationship
	{
		public DtbBookingPackageDivotRelationship(DtbBooking booking)
		{
			Booking = booking;
			HookInstructionEvents();
		}

		void HookInstructionEvents()
		{
			// if booking is deleted we should unhook the instructions collection
			// but we shouldn't need to as Booking.Instructions.CountChanged will never be called after the booking is deleted
			Booking.Instructions.CountChanged += new EventHandler(Instructions_CountChanged);
		}

		void Instructions_CountChanged(object sender, EventArgs e)
		{
			foreach (var instruction in Instructions_Listening.ToArray())
			{
				if (!Booking.Instructions.Contains(instruction))
				{
					UnHook(instruction);
				}
			}

			foreach (DtbBookingInstruction instruction in Booking.Instructions)
			{
				if (!Instructions_Listening.Contains(instruction))
				{
					Hook(instruction);
				}
			}

			if (!IsRebuilding)
			{
				OnRefreshed();
			}
		}

		void Hook(DtbBookingInstruction instruction)
		{
			Argument.NotNull(instruction, "instruction");

			Instructions_Listening.Add(instruction);
			instruction.PackageDivots.CountChanged += new EventHandler(PackageDivots_CountChanged);
		}

		void UnHook(DtbBookingInstruction instruction)
		{
			Argument.NotNull(instruction, "instruction");

			Instructions_Listening.Remove(instruction);
			instruction.PackageDivots.CountChanged -= new EventHandler(PackageDivots_CountChanged);
		}

		void PackageDivots_CountChanged(object sender, EventArgs e)
		{
			if (!IsRebuilding)
			{
				OnRefreshed();
			}
		}

		List<DtbBookingInstruction> Instructions_Listening
		{
			get { return instructions_Listening ?? (instructions_Listening = new List<DtbBookingInstruction>()); }
		}

		List<DtbBookingInstruction> instructions_Listening;

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

				if (!Booking.IsDeleted && Booking.IsSub)
				{
					var instructionPKs = Booking.MasterBooking.Instructions.Select(i => i.PK).ToArray();
					result = new ZQuery(DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction, instructionPKs);
					var packagePKFilter = new ZDBOnlyQuery(typeof(DtbBookingInstructionPkgDivot));
					var packagePKFilterSQL = @"
KD_KP_Package IN
(
	SELECT KP_PK FROM dbo.PkgPackage
	WHERE KP_KJ_ParentPackageJob = @SubBookingPackageJobPK
)";
					var parameterCollection = new ZSqlParameterCollection();
					parameterCollection.Add("@SubBookingPackageJobPK", Booking.PackageJob.PK, PkgPackageSchema.KP_KJ_ParentPackageJob);
					packagePKFilter.AddFilterAndZSQLParameterCollection(packagePKFilterSQL, parameterCollection);
					result.AddToFilter(packagePKFilter);
				}
				else
				{
					var instructionPKs = Booking.Instructions.Select(i => i.PK).ToArray();
					result = new ZQuery(DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction, instructionPKs);
				}
				result.AddToFilter(AdditionalFilter);
				result.IgnoreActiveFilter = ignoreActiveFilter;
				result.ModificationsEnabled = false;
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
			var result = (DtbBookingPackageDivotRelationship)MemberwiseClone();

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
			return factory.Load<DtbBookingInstructionPkgDivot>(new ZQuery(filter, BuildRelationshipFilter(false)));
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
