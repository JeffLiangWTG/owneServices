using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Module
{
	public class EquipmentModuleColumnsAndFiltersProvider : IRefEquipmentModuleColumnsAndFiltersProvider
	{
		internal static class Descriptions
		{
			internal static MultilingualString eManifestStatusMultilingualDescription
			{
				get { return ResString.GetMultilingualString("f7ba884d-e65f-4acd-964a-239f6711d64a", "e-Manifest Type"); }
			}

			internal const string eManifestStatusFilterId = "e-Manifest Type";
		}

		#region Implementation of IRefEquipmentModuleColumnsAndFiltersProvider

		public void AddFilters(IModuleFilterCollection filters)
		{
			var collection = (ModuleFilterCollection)filters;
			var filter = collection.AddTextFilter(Descriptions.eManifestStatusFilterId, GetManifestStatusQuery, new EquipmentTypeFilterList());
			filter.MultilingualDescription = Descriptions.eManifestStatusMultilingualDescription;
			filter.Category = FilterCategories.TextSearch;
		}

		static ZQuery GetManifestStatusQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(RefEquipment));
			IEnumerable<ICodeDescription> list = null;
			switch (value)
			{
				case EquipmentTypeFilterList.Codes.Conveyance:
					list = new ConveyanceTypes().ToArray();

					break;
				case EquipmentTypeFilterList.Codes.Equipment:
					list = new EquipmentTypes().ToArray();
					break;
			}
			if (list != null)
			{
				var refContainerQuery = new ZDBOnlySubQuery(typeof(RefContainer), RefContainerSchema.PK);
				refContainerQuery.AddSubQuery(RefContainerSchema.PK, RefContainer.GetContainerCodeFilter(Core.Constants.CountryCodes.UnitedStates, list.Select(p => new ZString(p.Code))), JoinCondition.And);
				query.AddSubQuery(RefEquipmentSchema.RQ_RC_RoadContainerType, refContainerQuery, JoinCondition.And);
			}
			return query;
		}

		public void AddColumns(IFilterControl filterControl)
		{
		}

		#endregion
	}
}
