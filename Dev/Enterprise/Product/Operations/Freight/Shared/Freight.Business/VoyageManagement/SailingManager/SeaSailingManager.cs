using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	class SeaSailingManager : BaseSailingManager
	{
		public SeaSailingManager(ISailingManaged parent)
			: base(parent)
		{
		}

		protected override bool HasSufficientInformationCore
		{
			get
			{
				if (parent.TransportMode == Constants.TransportModes.Sea
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
			voyageFilter.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, parent.Vessel);

			JobVoyage voyage = null;

			if (!parent.ShippingLine.IsEmpty)
			{
				voyageFilter.AddToFilter(JobVoyageSchema.JV_OH_Line, parent.ShippingLine);
				voyage = parent.Factory.LoadTop1<JobVoyage>(voyageFilter);
			}
			else
			{
				var voyages = parent.Factory.Load<JobVoyage>(voyageFilter);
				voyage = voyages.FirstOrDefault(v => v.IsMainVoyage);

				if (voyage == null)
				{
					voyage = voyages.FirstOrDefault();
				}
			}

			if (voyage != null)
			{
				UpdateCharteredStatusIfNecessary(voyage);
			}

			return voyage;
		}

		void UpdateCharteredStatusIfNecessary(JobVoyage voyage)
		{
			bool shouldUpdateIsCharted = IsCharterDirty;
			if (voyage.JV_IsChartered != parent.IsCharter && shouldUpdateIsCharted)
			{
				voyage.JV_IsChartered = parent.IsCharter;
			}
		}

		protected override JobVoyage NewVoyage()
		{
			JobVoyage result = parent.Factory.New<JobVoyage>();
			result.JV_AirSeaRoad = parent.TransportMode;
			result.JV_VoyageFlight = parent.Voyage;
			result.JV_RV_NKVessel = parent.Vessel;
			result.JV_IsChartered = parent.IsCharter;
			result.JV_OH_Line = parent.ShippingLine;

			EnforceVoyageType(result);

			return result;
		}

		void EnforceVoyageType(JobVoyage voyage)
		{
			if (voyage.Vessel != null && !voyage.Vessel.RV_OH.IsEmpty && voyage.Vessel.RV_OH != voyage.JV_OH_Line)
			{
				voyage.JV_VoyageType = Constants.VoyageType.SlotVoyage;
			}
			else
			{
				voyage.TryToDefaultVoyageType(true);
			}
		}

		protected override ZQuery GetExistingMatchingVoyageQuery()
		{
			var voyageFilter = base.GetExistingMatchingVoyageQuery();
			if (!parent.ShippingLine.IsEmpty)
			{
				voyageFilter.AddToFilter(JobVoyageSchema.JV_OH_Line, parent.ShippingLine);
			}

			return voyageFilter;
		}

		protected override void SetVoyageFieldsCore()
		{
			base.SetVoyageFieldsCore();
			parent.ShippingLine = Sailing.Voyage.JV_OH_Line;
		}

		protected override void AfterOldVoyageHasBeenReleased()
		{
			if (Voyage != null && !Voyage.IsDeleted && !Voyage.IsInDatabase)
			{
				EnforceVoyageType(Voyage);
			}
		}

		protected override bool ShouldUpdateETD
		{
			get { return DepartureDirty && !VoyageDirty && !VesselDirty && !LoadDirty && !CarrierDirty; }
		}

		protected override bool ShouldUpdateETA
		{
			get { return ArrivalDirty && !VoyageDirty && !VesselDirty && !DischargeDirty && !CarrierDirty; }
		}
	}
}
