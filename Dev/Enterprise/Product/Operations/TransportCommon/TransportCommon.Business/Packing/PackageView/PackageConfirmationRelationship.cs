using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	public sealed class PackageConfirmationRelationship<T> : ICollectionRelationship
		where T : DtbTransportConfirmation
	{
		public PackageConfirmationRelationship(Package_PackageView package_PackageView)
		{
			Package_PackageView = package_PackageView;
			HookPackageDivots();
		}

		#region Events

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

			foreach (DtbTransportInstructionPkgDivot packageDivot in Package_PackageView.InstructionDivots)
			{
				if (!InstructionDivots_Listening.Contains(packageDivot))
				{
					Hook(packageDivot);
				}
			}
		}

		void Hook(DtbTransportInstructionPkgDivot packageDivot)
		{
			Argument.NotNull(packageDivot, "packageDivot");

			InstructionDivots_Listening.Add(packageDivot);
			packageDivot.Confirmations.CountChanged += new EventHandler(Confirmations_CountChanged);
		}

		void UnHook(DtbTransportInstructionPkgDivot packageDivot)
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

		List<DtbTransportInstructionPkgDivot> InstructionDivots_Listening
		{
			get { return instructionDivots_Listening ?? (instructionDivots_Listening = new List<DtbTransportInstructionPkgDivot>()); }
		}

		List<DtbTransportInstructionPkgDivot> instructionDivots_Listening;

		#endregion

		#region ICollectionRelationship Members

		#region Master

		BusinessObject ICollectionRelationship.Master
		{
			get { return Package_PackageView; }
		}

		readonly Package_PackageView Package_PackageView;

		#endregion

		#region RelationshipFilter

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

				foreach (DtbTransportInstructionPkgDivot packageDivot in Package_PackageView.InstructionDivots)
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

		#endregion

		#region AdditionalFilter

		ICollectionRelationship ICollectionRelationship.AddFilter(ZQuery additionalFilter)
		{
			var result = (PackageConfirmationRelationship<T>)MemberwiseClone();

			if (additionalFilter != null)
			{
				result.AdditionalFilter = new ZQuery(AdditionalFilter, additionalFilter);
				result.AdditionalFilter.ModificationsEnabled = false;
			}

			return result;
		}

		ZQuery AdditionalFilter;

		#endregion

		#region Refreshed

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

		#endregion

		#region LoadBusinessObjects

		BusinessObject[] ICollectionRelationship.LoadBusinessObjects(BusinessObjectFactory factory, ZQuery filter)
		{
			return factory.Load<T>(new ZQuery(filter, BuildRelationshipFilter(false)));
		}

		#endregion

		#region HasChangesIncludingRelationship

		bool ICollectionRelationship.HasChangesIncludingRelationship(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");

			return businessObject.HasChanges;
		}

		#endregion

		#region ClearHasChangesIncludingRelationship

		void ICollectionRelationship.ClearHasChangesIncludingRelationship(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");

			IBusinessObjectState businessObjectState = businessObject;
			businessObjectState.ClearHasChangesIncludingChildren();
		}

		#endregion

		#region Add To / Remove From Relationship

		bool ICollectionRelationship.SupportsAddToRelationship()
		{
			return Package_PackageView.InstructionDivots.Count > 0;
		}

		void ICollectionRelationship.AddToRelationship(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");

			var confirmation = (DtbTransportConfirmation)businessObject;
			var firstDivot = Package_PackageView.InstructionDivots[0];

			confirmation.KK_KD_BookingInstructionPkgDivot = firstDivot.PK;
		}

		void ICollectionRelationship.RemoveFromRelationship(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");

			var confirmation = (DtbTransportConfirmation)businessObject;
			confirmation.KK_KD_BookingInstructionPkgDivot = ZGuid.Empty;
		}

		#endregion

		#endregion

		#region ICollectionRelationship Members

		void ICollectionRelationship.Clear()
		{
			throw new InvalidOperationException("Can't clear using this relationship");
		}

		#endregion
	}
}
