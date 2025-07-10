using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	class TransportPackageRelationship : ICollectionRelationship
	{
		public TransportPackageRelationship(DtbTransport transport)
		{
			Transport = transport;
			HookPackageDivots();
		}

		#region Events

		void HookPackageDivots()
		{
			// if booking is deleted we should unhook the instructions collection
			// but we shouldn't need to as Booking.PackageDivots.CountChanged will never be called after the booking is deleted
			Transport.PackageDivots.CountChanged += new EventHandler(InstructionPackageDivots_CountChanged);
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
				if (!Transport.PackageDivots.Contains(packageDivot))
				{
					UnHook(packageDivot);
				}
			}

			foreach (DtbTransportInstructionPkgDivot packageDivot in Transport.PackageDivots)
			{
				if (!PackageDivots_Listening.Contains(packageDivot))
				{
					Hook(packageDivot);
				}
			}
		}

		void Hook(DtbTransportInstructionPkgDivot packageDivot)
		{
			Argument.NotNull(packageDivot, "packageDivot");

			PackageDivots_Listening.Add(packageDivot);
			packageDivot.KD_KP_PackageInfo.ValueChanged += new EventHandler(KD_KP_PackageInfo_ValueChanged);
		}

		void UnHook(DtbTransportInstructionPkgDivot packageDivot)
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

		HashSet<DtbTransportInstructionPkgDivot> PackageDivots_Listening
		{
			get { return packageDivots_Listening ?? (packageDivots_Listening = new HashSet<DtbTransportInstructionPkgDivot>()); }
		}

		HashSet<DtbTransportInstructionPkgDivot> packageDivots_Listening;

		#endregion

		#region ICollectionRelationship Members

		#region Master

		BusinessObject ICollectionRelationship.Master
		{
			get { return Transport; }
		}

		readonly DtbTransport Transport;

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

				if (Transport.PackageDivots.Count == 0)
				{
					result = ZQuery.NoResultQuery;
				}
				else
				{
					var packagePks = Array.ConvertAll(Transport.PackageDivots.Cast<DtbTransportInstructionPkgDivot>().ToArray(), d => d.KD_KP_Package);
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

		#endregion

		#region AdditionalFilter

		ICollectionRelationship ICollectionRelationship.AddFilter(ZQuery additionalFilter)
		{
			var result = (TransportPackageRelationship)MemberwiseClone();

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
			return factory.Load<PkgPackage>(new ZQuery(filter, BuildRelationshipFilter(false)));
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

		#region SupportsAddToRelationship

		bool ICollectionRelationship.SupportsAddToRelationship()
		{
			return false;
		}

		#endregion

		#region Add To / Remove From Relationship

		void ICollectionRelationship.AddToRelationship(BusinessObject businessObject)
		{
			throw new InvalidOperationException("Can't add using this relationship");
		}

		void ICollectionRelationship.RemoveFromRelationship(BusinessObject businessObject)
		{
			throw new InvalidOperationException("Can't remove using this relationship");
		}

		#endregion

		#region Clear

		void ICollectionRelationship.Clear()
		{
			throw new InvalidOperationException("Can't clear using this relationship");
		}

		#endregion

		#endregion
	}
}
