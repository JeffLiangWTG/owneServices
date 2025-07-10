using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public class PackageConfirmationRelationship : ICollectionRelationship
	{
		public PackageConfirmationRelationship(DtbBookingPackage_PackageView package_PackageView)
		{
			Package_PackageView = package_PackageView;
			HookPackageDivots();
		}

		void HookPackageDivots()
		{
			// if the Package the Package_PackageView is deleted, we should unhook the InstructionDivots collection
			// because a packageDivot.Confirmations could change?
			Package_PackageView.InstructionDivots.CountChanged += new EventHandler(InstructionDivots_CountChanged);
			HookUnhookEachPackageDivot();
		}

		void InstructionDivots_CountChanged(object sender, EventArgs e)
		{
			HookUnhookEachPackageDivot();

			if (!IsRebuilding)
			{
				OnRefreshed();
			}
		}

		void HookUnhookEachPackageDivot()
		{
			foreach (var packageDivot in InstructionDivots_Listening.ToArray())
			{
				if (!Package_PackageView.InstructionDivots.Contains(packageDivot))
				{
					UnHook(packageDivot);
				}
			}

			foreach (DtbBookingInstructionPkgDivot packageDivot in Package_PackageView.InstructionDivots)
			{
				if (!InstructionDivots_Listening.Contains(packageDivot))
				{
					Hook(packageDivot);
				}
			}
		}

		void Hook(DtbBookingInstructionPkgDivot packageDivot)
		{
			Argument.NotNull(packageDivot, "packageDivot");

			InstructionDivots_Listening.Add(packageDivot);
			packageDivot.Confirmations.CountChanged += new EventHandler(Confirmations_CountChanged);
		}

		void UnHook(DtbBookingInstructionPkgDivot packageDivot)
		{
			Argument.NotNull(packageDivot, "packageDivot");

			InstructionDivots_Listening.Remove(packageDivot);
			packageDivot.Confirmations.CountChanged -= new EventHandler(Confirmations_CountChanged);
		}

		void Confirmations_CountChanged(object sender, EventArgs e)
		{
			if (!IsRebuilding)
			{
				OnRefreshed();
			}
		}

		List<DtbBookingInstructionPkgDivot> InstructionDivots_Listening
		{
			get { return instructionDivots_Listening ?? (instructionDivots_Listening = new List<DtbBookingInstructionPkgDivot>()); }
		}

		List<DtbBookingInstructionPkgDivot> instructionDivots_Listening;

		BusinessObject ICollectionRelationship.Master
		{
			get { return Package_PackageView; }
		}

		readonly DtbBookingPackage_PackageView Package_PackageView;

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

				var confirmationPks = new List<ZGuid>();

				foreach (DtbBookingInstructionPkgDivot packageDivot in Package_PackageView.InstructionDivots)
				{
					confirmationPks.AddRange(Array.ConvertAll(packageDivot.Confirmations.ToArray(), c => c.PK));
				}

				if (confirmationPks.Count == 0)
				{
					result = ZQuery.NoResultQuery;
				}
				else
				{
					result = new ZQuery();
					result.AddToFilter(DtbBookingConfirmationSchema.PK, confirmationPks);
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
			var result = (PackageConfirmationRelationship)MemberwiseClone();

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
			return factory.Load<DtbBookingConfirmation>(new ZQuery(filter, BuildRelationshipFilter(false)));
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
			return Package_PackageView.InstructionDivots.Count > 0;
		}

		void ICollectionRelationship.AddToRelationship(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");

			var confirmation = (DtbBookingConfirmation)businessObject;
			var firstDivot = Package_PackageView.InstructionDivots[0];

			confirmation.KK_KD_BookingInstructionPkgDivot = firstDivot.PK;
		}

		void ICollectionRelationship.RemoveFromRelationship(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");

			var confirmation = (DtbBookingConfirmation)businessObject;
			confirmation.KK_KD_BookingInstructionPkgDivot = ZGuid.Empty;
		}

		void ICollectionRelationship.Clear()
		{
			throw new InvalidOperationException("Can't clear using this relationship");
		}
	}
}
