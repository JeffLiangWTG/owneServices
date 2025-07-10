using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	class RailScheduleManager : BaseSailingManager
	{
		public RailScheduleManager(ISailingManaged parent)
			: base(parent)
		{
		}

		#region Implemention

		protected override bool HasSufficientInformationCore
		{
			get
			{
				if (parent.TransportMode == Constants.TransportModes.Rail
					&& !parent.Load.IsEmpty
					&& !parent.Discharge.IsEmpty
					&& !parent.Vessel.IsEmpty
					&& !parent.Voyage.IsEmpty)
				{
					return true;
				}

				return false;
			}
		}

		protected override JobVoyage GetExistingVoyage(bool ignoreActiveFilter = false)
		{
			ZQuery voyageFilter = new ZQuery(JobVoyageSchema.JV_VoyageFlight, parent.Voyage);
			voyageFilter.IgnoreActiveFilter = ignoreActiveFilter;
			voyageFilter.AddToFilter(TransportModeFilter);
			voyageFilter.AddToFilter(new ZQuery(JobVoyageSchema.JV_RV_NKVessel, parent.Vessel));

			var result = parent.Factory.LoadTop1<JobVoyage>(voyageFilter);

			return result;
		}

		protected override JobVoyage NewVoyage()
		{
			JobVoyage result = parent.Factory.New<JobVoyage>();
			result.JV_VoyageFlight = parent.Voyage;
			result.JV_RV_NKVessel = parent.Vessel;
			result.JV_AirSeaRoad = parent.TransportMode;
			result.JV_IsChartered = parent.IsCharter;

			return result;
		}

		protected override bool ShouldUpdateETD
		{
			get { return DepartureDirty && !VoyageDirty && !VesselDirty && !LoadDirty; }
		}

		protected override bool ShouldUpdateETA
		{
			get { return ArrivalDirty && !VoyageDirty && !VesselDirty && !DischargeDirty; }
		}

		protected override void UpdateISailingValues()
		{
			base.UpdateISailingValues();
			if (Sailing != null)
			{
				if (Sailing != null && Voyage != null)
				{
					if (parent.ShippingLine.IsEmpty && !Voyage.JV_OH_Line.IsEmpty)
					{
						parent.ShippingLine = Voyage.JV_OH_Line;
					}
				}

				if (VoyageDirty)
				{
					parent.ShippingLine = Voyage.JV_OH_Line;
				}
			}
		}

		#endregion
	}
}
