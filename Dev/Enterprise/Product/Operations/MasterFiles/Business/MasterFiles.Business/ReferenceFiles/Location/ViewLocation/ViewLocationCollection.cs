using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Provides a collection of UNLOCOs, Countries, States, Cities, International and Domistic Zones
	/// </summary>
	[ModuleID(ModuleId.ViewLocation)]
	public class ViewLocationCollection : ActiveBusinessObjectCollection<ViewLocation>
	{
		public ViewLocationCollection(BusinessObjectFactory factory, ViewLocationType locationsTypesToInclude = ViewLocationType.All)
			: base(factory)
		{
			LocationsTypesToInclude = locationsTypesToInclude;
		}

		public ViewLocationCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public readonly ViewLocationType LocationsTypesToInclude;

		protected override object[] GetCollectionState()
		{
			return new object[] { LocationsTypesToInclude };
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(GetLocationTypesFilter());
			return result;
		}

		internal ZQuery GetLocationTypesFilter()
		{
			var result = new ZQuery();

			var tableCodesToExclude = new List<string>();

			if (!LocationsTypesToInclude.HasFlag(ViewLocationType.UNLOCO))
			{ tableCodesToExclude.Add(RefUNLOCOSchema.Constants.Prefix); }
			if (!LocationsTypesToInclude.HasFlag(ViewLocationType.Country))
			{ tableCodesToExclude.Add(RefCountrySchema.Constants.Prefix); }
			if (!LocationsTypesToInclude.HasFlag(ViewLocationType.State))
			{ tableCodesToExclude.Add(RefCountryStatesSchema.Constants.Prefix); }
			if (!LocationsTypesToInclude.HasFlag(ViewLocationType.City))
			{ tableCodesToExclude.Add(RefCityTownSchema.Constants.Prefix); }
			if (!LocationsTypesToInclude.HasFlag(ViewLocationType.InternationalZone))
			{ tableCodesToExclude.Add(RefZoneHeaderSchema.Constants.Prefix); }
			if (!LocationsTypesToInclude.HasFlag(ViewLocationType.TransportZone))
			{ tableCodesToExclude.Add(RateTransportZonesSchema.Constants.Prefix); }

			if (tableCodesToExclude.Count > 0)
			{
				result.AddToFilter(ViewLocationSchema.VLO_TableCode, SQLComparisonOperator.NotEqual, tableCodesToExclude);
			}

			return result;
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new ViewLocationFindBoxListProvider(this); }
		}
	}
}
