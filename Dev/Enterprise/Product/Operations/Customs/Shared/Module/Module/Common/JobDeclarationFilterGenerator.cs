using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class JobDeclarationFilterGenerator
	{
		public JobDeclarationFilterGenerator(FilterStripBusinessObject filterBizObj)
		{
			FilterBizObj = filterBizObj;
		}

		protected readonly FilterStripBusinessObject FilterBizObj;

		public ModuleTextAndNkFilter AddFlightVoyageVesselFilter(ModuleFilterCollection filters, ZString description)
		{
			var flightVoyageVesselFilter = filters.AddTextAndNkFilter(description, GetDeclarationFlightVoyageAndVesselQuery, ModuleIDs.RefVessel, Lookups.VesselList).WithMaxLengthOf(JobDeclarationSchema.JE_VoyageFlightNo, JobDeclarationSchema.JE_VesselName);
			flightVoyageVesselFilter.Category = FilterCategories.ModesAndTypes;
			return flightVoyageVesselFilter;
		}

		public ZQuery GetDeclarationFlightVoyageAndVesselQuery(SQLComparisonOperator flightOrVoyageNoComparisonOperator, ZString flightOrVoyageNo, ZString nKVessel)
		{
			var query = new ZQuery();

			if (!flightOrVoyageNo.IsEmpty || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsBlank || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(JobDeclarationSchema.JE_VoyageFlightNo, flightOrVoyageNoComparisonOperator, flightOrVoyageNo);
			}

			if (!nKVessel.IsEmpty || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsBlank || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(JobDeclarationSchema.JE_VesselName, flightOrVoyageNoComparisonOperator, nKVessel);
			}

			return query;
		}

		public ModuleLocationFilter AddOriginDestinationFilter(ModuleFilterCollection filters, ZString description)
		{
			var originDestinationFilter = filters.AddLocationFilter(description, JobDeclarationSchema.JE_RL_NKOrigin, Lookups.LocationList, JobDeclarationSchema.JE_RL_NKFinalDestination, Lookups.LocationList);
			originDestinationFilter.SetItemDescriptions(Res.GetData("3E7E7049-AA48-4DCD-87EC-54FD4A6EF70A", "Origin"), Res.GetData("1682ABB9-4338-4298-8559-0054EC9F3A96", "Destination"));
			return originDestinationFilter;
		}

		public ModuleLocationFilter AddLoadDischargeFilter(ModuleFilterCollection filters, ZString description)
		{
			var loadDischargeFilter = filters.AddLocationFilter(description, JobDeclarationSchema.JE_RL_NKPortOfLoading, Lookups.LocationList, JobDeclarationSchema.JE_RL_NKPortOfArrival, Lookups.LocationList);
			return loadDischargeFilter;
		}

		public ModuleNumberFilter AddAgentsReferenceFilter(ModuleFilterCollection filters, ZString description)
		{
			var agentsReferenceFilter = filters.AddNumberFilter(description, JobDeclarationSchema.JE_AgentsReference);
			agentsReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			return agentsReferenceFilter;
		}

		public ReferenceNumberFilter AddAdditionalReferenceNumberFilter(ModuleFilterCollection filters, BusinessObjectFactory factory)
		{
			var additionalReferenceNumberFilter = new ReferenceNumberFilter(DeclarationFilterConstants.NumberFilterTypes.AdditionalReferenceNumber,
					new ReferenceNumberFilterHelper<BaseJobDeclaration>().GetReferenceNumberFilter,
					new RefCountryCollection(factory))
				.WithMaxLengthOf<ReferenceNumberFilter>(CusEntryNumSchema.CE_EntryNum);

			additionalReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|AdditionalReferenceNumber", DeclarationFilterConstants.NumberFilterTypes.AdditionalReferenceNumber);
			filters.AddCustomFilter(additionalReferenceNumberFilter);
			return additionalReferenceNumberFilter;
		}

		public CommonFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new CommonFilterLookups(FilterBizObj);
				}
				return lookups;
			}
		}
		CommonFilterLookups lookups;
	}
}
