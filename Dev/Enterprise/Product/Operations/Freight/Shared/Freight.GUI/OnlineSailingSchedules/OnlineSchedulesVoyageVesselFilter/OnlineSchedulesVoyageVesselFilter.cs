using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.GUI
{
	public class OnlineSchedulesVoyageVesselFilter : ModuleTextBaseFilter, IModuleFilterWithModuleID
	{
		public delegate ZQuery GetVoyageVesselQueryDelegate(SQLComparisonOperator voyageFlightComparisonOperator, ZString voyageFlight, ZString vessel);

		public OnlineSchedulesVoyageVesselFilter(ZString description, GetVoyageVesselQueryDelegate queryDelegate, RefVesselCollection vesselsList) : base(description, queryDelegate)
		{
			this.vesselsList = Argument.NotNull(vesselsList, "vessels");
		}

		readonly RefVesselCollection vesselsList;

		public ZString VoyageFlightNo
		{
			get => Property;
			set
			{
				Property = value;
				VoyageFlightNoInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo VoyageFlightNoInfo => GetZPropertyInfo(nameof(VoyageFlightNo));

		[List("Vessels")]
		public ZString Vessel
		{
			get => vessel;
			set
			{
				if (vessel != value)
				{
					vessel = value;
					VesselInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo VesselInfo => GetZPropertyInfo(nameof(Vessel));

		ZString vessel;

		public RefVesselCollection Vessels => vesselsList;

		public ModuleIdentifier ID => ModuleIDs.RefVessel;

		public override bool HasComparisonOperator => false;

		protected override bool IsEmptyCore => base.IsEmptyCore && VoyageFlightNo.IsEmpty && Vessel.IsEmpty;

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection collection) => throw new NotSupportedException();

		protected override void ClearCore()
		{
			base.ClearCore();
			Vessel = ZString.Empty;
		}
	}
}
