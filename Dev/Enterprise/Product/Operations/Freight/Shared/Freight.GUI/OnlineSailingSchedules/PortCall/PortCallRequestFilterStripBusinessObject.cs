using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.GUI.OnlineSailingSchedules;
using Enterprise.Freight.OnlineSailingSchedules.PortCall;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.GUI
{
	public class PortCallRequestFilterStripBusinessObject : FilterStripBusinessObject
	{
		public PortCallRequestFilterStripBusinessObject(PortCallManager manager)
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "PortCallRequestFilterStripBusinessObject";
			this.manager = manager;
			SetDefaultFilters();
			HookValidations();
		}

		public PortCallRequestFilterStripBusinessObject()
		{
		}

		readonly PortCallManager manager;

		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string Carrier = "Carrier";
			public const string Voyage = "Voyage";
			public const string Vessel = "Vessel";
			public const string Port = "Port";
			public const string IMO = "IMO";
			public const string CallSign = "CallSign";
			public const string DateRange = "DateRange";

			#endregion
		}

		public void ReBuildRequest()
		{
			manager.Request.CarrierPK = CarrierPK;
			manager.Request.VesselPK = VesselPK;
			manager.Request.Voyage = Voyage;
			manager.Request.IMO = IMO;
			manager.Request.CallSign = CallSign;
			manager.Request.Port = Port;
			manager.Request.StartEstimatedTime = StartDateTime;
			manager.Request.EndEstimatedTime = EndDateTime;
		}

		void SetDefaultFilters()
		{
			if (manager.Request.CarrierPK.IsValid)
			{
				CarrierPK = manager.Request.CarrierPK;
			}

			if (manager.Request.VesselPK.IsValid)
			{
				VesselPK = manager.Request.VesselPK;
			}

			if (manager.Request.EstimatedTime.IsValid)
			{
				StartDateTime = manager.Request.EstimatedTime;
				EndDateTime = manager.Request.EstimatedTime;
			}

			Voyage = manager.Request.Voyage;
			IMO = manager.Request.IMO;
			CallSign = manager.Request.CallSign;
			Port = manager.Request.Port;
		}

		void HookValidations()
		{
			((ModuleGuidFilter)this[Descriptions.Vessel]).PropertyValidation += ValidateVesselFilter;
			((ModuleTextFilter)this[Descriptions.IMO]).PropertyValidation += ValidateIMOFilter;
			((ModuleTextFilter)this[Descriptions.CallSign]).PropertyValidation += ValidateCallSignFilter;
			((ModuleTextFilter)this[Descriptions.Voyage]).PropertyValidation += ValidateVoyageFilter;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var carrierFilter = new PortCallModuleGuidFilter(Descriptions.Carrier, ModuleIDs.Organisation, GetEmptyZGuidQuery, new OrgHeaderCollection(Factory));
			filters.AddFilter(carrierFilter);
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|PortCallRequestFilterStripBusinessObject|Carrier", "Carrier");
			carrierFilter.Visibility = FilterVisibility.AlwaysVisible;

			var vesselFilter = new PortCallModuleGuidFilter(Descriptions.Vessel, ModuleIDs.RefVessel, GetEmptyZGuidQuery, BindingLists.RefVessel_List);
			filters.AddFilter(vesselFilter);
			vesselFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|PortCallRequestFilterStripBusinessObject|Vessel", "Vessel");
			vesselFilter.Visibility = FilterVisibility.AlwaysVisible;

			var voyageFilter = new OnlineSchedulesModuleTextFilter(Descriptions.Voyage, GetEmptyStringZQuery);
			filters.AddFilter(voyageFilter);
			voyageFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|PortCallRequestFilterStripBusinessObject|Voyage", "Voyage");
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			voyageFilter.Visibility = FilterVisibility.AlwaysVisible;

			var portFilter = filters.AddNkFilter(Descriptions.Port, GetEmptyNKZQuery, ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory));
			portFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|PortCallRequestFilterStripBusinessObject|Port", "Port");
			portFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			portFilter.Visibility = FilterVisibility.AlwaysVisible;
			portFilter.ReadOnly = true;

			var imoFilter = new OnlineSchedulesModuleTextFilter(Descriptions.IMO, GetEmptyStringZQuery);
			filters.AddFilter(imoFilter);
			imoFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|PortCallRequestFilterStripBusinessObject|IMO", "IMO");
			imoFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			imoFilter.Visibility = FilterVisibility.AlwaysVisible;

			var callSignFilter = new OnlineSchedulesModuleTextFilter(Descriptions.CallSign, GetEmptyStringZQuery);
			filters.AddFilter(callSignFilter);
			callSignFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|PortCallRequestFilterStripBusinessObject|CallSign", "Call Sign");
			callSignFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			callSignFilter.Visibility = FilterVisibility.AlwaysVisible;

			var dateRangeFilter = new OnlineSchedulesModuleDateFilter(Descriptions.DateRange, GetEmptyDateZQuery);
			filters.AddFilter(dateRangeFilter);
			dateRangeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateRangeFilter.Visibility = FilterVisibility.AlwaysVisible;
			dateRangeFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|PortCallRequestFilterStripBusinessObject|DateRange", "Date Range");

			return filters;
		}

		#region Properties

		ZGuid VesselPK
		{
			get => this[Descriptions.Vessel].IsActive ? ((ModuleGuidFilter)this[Descriptions.Vessel]).Property : ZGuid.Empty;
			set => ((ModuleGuidFilter)this[Descriptions.Vessel]).Property = value;
		}

		ZString Voyage
		{
			get => this[Descriptions.Voyage].IsActive ? ((ModuleTextFilter)this[Descriptions.Voyage]).Property : ZString.Empty;
			set => ((ModuleTextFilter)this[Descriptions.Voyage]).Property = value;
		}

		ZString CallSign
		{
			get => this[Descriptions.CallSign].IsActive ? ((ModuleTextFilter)this[Descriptions.CallSign]).Property : ZString.Empty;
			set => ((ModuleTextFilter)this[Descriptions.CallSign]).Property = value;
		}

		ZGuid CarrierPK
		{
			get => this[Descriptions.Carrier].IsActive ? ((ModuleGuidFilter)this[Descriptions.Carrier]).Property : ZGuid.Empty;
			set => ((ModuleGuidFilter)this[Descriptions.Carrier]).Property = value;
		}

		ZString IMO
		{
			get => this[Descriptions.IMO].IsActive ? ((ModuleTextFilter)this[Descriptions.IMO]).Property : ZString.Empty;
			set => ((ModuleTextFilter)this[Descriptions.IMO]).Property = value;
		}

		ZString Port
		{
			get => this[Descriptions.Port].IsActive ? ((ModuleNkFilter)this[Descriptions.Port]).Property : ZString.Empty;
			set => ((ModuleNkFilter)this[Descriptions.Port]).Property = value;
		}

		ZDateTime StartDateTime
		{
			get => this[Descriptions.DateRange].IsActive ? ((ModuleDateFilter)this[Descriptions.DateRange]).Property1 : ZDateTime.Empty;
			set => ((ModuleDateFilter)this[Descriptions.DateRange]).Property1 = value;
		}

		ZDateTime EndDateTime
		{
			get => this[Descriptions.DateRange].IsActive ? ((ModuleDateFilter)this[Descriptions.DateRange]).Property2 : ZDateTime.Empty;
			set => ((ModuleDateFilter)this[Descriptions.DateRange]).Property2 = value;
		}

		#endregion

		#region Validation

		void ValidateVoyageFilter(ZPropertyInfo info)
		{
			if (!info.Value.IsEmpty && info.Value.ToString().Length > 20)
			{
				info.AddError(Res.GetString("d3b51c94-4f8f-4009-b233-b37f70511bff", "Voyage number should consist of 0-20 characters."));
			}
		}

		void ValidateVesselFilter(ZPropertyInfo info)
		{
			try
			{
				IsValidatingVesselFilter = true;
				ValidateVesselIMOCallSign(info);

				if (!IsValidatingIMOFilter && !IsValidationgCallSignFilter)
				{
					((ModuleTextFilter)this[Descriptions.IMO]).Validation.ValidateProperty();
					((ModuleTextFilter)this[Descriptions.CallSign]).Validation.ValidateProperty();
				}
			}
			finally
			{
				IsValidatingVesselFilter = false;
			}
		}

		void ValidateIMOFilter(ZPropertyInfo info)
		{
			try
			{
				IsValidatingIMOFilter = true;
				ValidateVesselIMOCallSign(info);

				if (!info.Value.IsEmpty && info.Value.ToString().Length != 7)
				{
					info.AddError(Res.GetString("597af57b-9120-44c0-b324-1b035b19579d", "International Maritime Organization (IMO) number must consist of 7 characters."));
				}

				if (!IsValidatingVesselFilter && !IsValidationgCallSignFilter)
				{
					((ModuleGuidFilter)this[Descriptions.Vessel]).Validation.ValidateProperty();
					((ModuleTextFilter)this[Descriptions.CallSign]).Validation.ValidateProperty();
				}
			}
			finally
			{
				IsValidatingIMOFilter = false;
			}
		}

		void ValidateCallSignFilter(ZPropertyInfo info)
		{
			try
			{
				IsValidationgCallSignFilter = true;
				ValidateVesselIMOCallSign(info);

				var value = (ZString)info.Value;
				if (!value.IsEmpty && (value.Length > 7 || value.Length < 3))
				{
					info.AddError(Res.GetString("1364b4b9-ae91-4b4f-8f96-bb54339a4cf0", "Radio call sign of the vessel must consist of 3-7 characters."));
				}

				if (!IsValidatingVesselFilter && !IsValidatingIMOFilter)
				{
					((ModuleGuidFilter)this[Descriptions.Vessel]).Validation.ValidateProperty();
					((ModuleTextFilter)this[Descriptions.IMO]).Validation.ValidateProperty();
				}
			}
			finally
			{
				IsValidationgCallSignFilter = false;
			}
		}

		void ValidateVesselIMOCallSign(ZPropertyInfo info)
		{
			if (VesselPK.IsEmpty && IMO.IsEmpty && CallSign.IsEmpty)
			{
				info.AddError(Res.GetString("b67d0632-89e5-4cbe-bc9c-63226a47b5c7", "One of Vessel, IMO, Call Sign should be filled in."));
			}
		}

		bool IsValidatingVesselFilter;
		bool IsValidatingIMOFilter;
		bool IsValidationgCallSignFilter;

		#endregion

		#region Implement

		ZQuery GetEmptyZGuidQuery(ZGuid guid)
		{
			return new ZQuery();
		}

		ZQuery GetEmptyStringZQuery(SQLComparisonOperator comparisonoperator, ZString value)
		{
			return new ZQuery();
		}

		ZQuery GetEmptyDateZQuery(DateComparisonOperator comparisonoperator, ZDateTime value1, ZDateTime value2)
		{
			return new ZQuery();
		}

		ZQuery GetEmptyNKZQuery(ZString value)
		{
			return new ZQuery();
		}

		BindToLists BindingLists => BindToLists.GetCachedLists(Factory);

		#endregion
	}
}
