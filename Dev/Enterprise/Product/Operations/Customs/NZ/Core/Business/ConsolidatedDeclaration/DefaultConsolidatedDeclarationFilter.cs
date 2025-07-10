using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Business
{
	public static class DefaultConsolidatedDeclarationFilter
	{
		public static FilterBusinessObjectDefaults GetDefaultConsolidatedDeclarationFilter(JobDeclaration declaration)
		{
			var defaults = new FilterBusinessObjectDefaults
			{
				new FilterBusinessObjectDefault("Importer/Supplier", "Property1", declaration.JE_OH_Importer, false),
				new FilterBusinessObjectDefault("Transport Mode", "Property", declaration.JE_TransportMode, false),
				new FilterBusinessObjectDefault("Shipment Sub-Type", "Property", declaration.JE_MessageSubType, false),
				new FilterBusinessObjectDefault("Vessel and Flight/Voyage #", "Property", declaration.JE_VoyageFlightNo, false),
				new FilterBusinessObjectDefault("Vessel and Flight/Voyage #", "ComparisonOperator", (ZString)ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, false)
			};

			if (declaration.IsSea)
			{
				defaults.Add(new FilterBusinessObjectDefault("Vessel and Flight/Voyage #", "NkProperty", declaration.JE_VesselName, false));
			}

			if (!declaration.IsPeriodic)
			{
				defaults.Add(new FilterBusinessObjectDefault("Arrival at Discharge Port", "PropertySearch", ModuleDateFilter.SpecifiedDateRange, false));
				defaults.Add(new FilterBusinessObjectDefault("Arrival at Discharge Port", "Property1", declaration.JE_DateOfArrival, false));
				defaults.Add(new FilterBusinessObjectDefault("Arrival at Discharge Port", "Property2", declaration.JE_DateOfArrival, false));
			}
			return defaults;
		}
	}
}
