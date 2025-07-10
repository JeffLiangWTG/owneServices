using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ViewLocationFindBoxListProvider : FindBoxListProvider
	{
		public ViewLocationFindBoxListProvider(ViewLocationCollection collection)
			: base(collection)
		{
		}

		public new ViewLocationCollection List
		{
			get { return (ViewLocationCollection)base.List; }
		}

		protected override void AddCodeStartsWithFilter(ZQuery query, string code)
		{
			if (!string.IsNullOrWhiteSpace(code))
			{
				query.AddToFilter(ViewLocationSchema.VLO_Code, SQLComparisonOperator.StartsWith, code);
			}
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
		{
			return LocationAsEnumerable(GetBestMatchLocationByCode(code, SQLComparisonOperator.Equal, true, false));
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithoutFilter(string code)
		{
			return LocationAsEnumerable(GetBestMatchLocationByCode(code, SQLComparisonOperator.Equal, false, false));
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
		{
			return LocationAsEnumerable(GetBestMatchLocationByCode(code, SQLComparisonOperator.Equal, false, true));
		}

		IEnumerable<ViewLocation> LocationAsEnumerable(ViewLocation location)
		{
			if (location != null)
			{
				yield return location;
			}
		}

		ViewLocation GetBestMatchLocationByCode(string code, SQLComparisonOperator comparisonOperator, bool includeRelationshipFilter, bool includeCompleteFilter)
		{
			var factory = List.Factory;

			var mainFilter = new ZQuery(ViewLocationSchema.VLO_Code, comparisonOperator, code);
			if (includeRelationshipFilter)
			{
				mainFilter.AddToFilter(((IBusinessObjectCollection)List).RelationshipFilter);
			}
			if (includeCompleteFilter)
			{
				mainFilter.AddToFilter(List.CompleteFilter);
			}

			var sameCountryQuery = new ZQuery(ViewLocationSchema.VLO_CountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			sameCountryQuery.AddToFilter(mainFilter);
			sameCountryQuery.OrderBy = ViewLocationSchema.Constants.VLO_Code;
			var bestMatchForSameCountry = factory.LoadTop1<ViewLocation>(sameCountryQuery);
			if (bestMatchForSameCountry != null)
			{
				return bestMatchForSameCountry;
			}

			var differentCountryQuery = new ZQuery(ViewLocationSchema.VLO_CountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			differentCountryQuery.AddToFilter(mainFilter);
			differentCountryQuery.OrderBy = ViewLocationSchema.Constants.VLO_Code;
			var bestMatchForDifferentCountry = factory.LoadTop1<ViewLocation>(differentCountryQuery);
			if (bestMatchForDifferentCountry != null)
			{
				return bestMatchForDifferentCountry;
			}

			return null;
		}

		public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
		{
			if (explicitAutoComplete)
			{
				var nearestMatchByCode = GetBestMatchLocationByCode(code, SQLComparisonOperator.StartsWith, false, true);
				if (nearestMatchByCode != null)
				{
					return (nearestMatchByCode.VLO_Code, true);
				}
			}
			else
			{
				var exactMatchByCode = GetBestMatchLocationByCode(code, SQLComparisonOperator.Equal, false, true);
				if (exactMatchByCode != null)
				{
					return (exactMatchByCode.VLO_Code, true);
				}
				else if (code.Length == 3 && List.LocationsTypesToInclude.HasFlag(ViewLocationType.UNLOCO))
				{
					var unloco = RefUNLOCO.LoadFromIATA(List.Factory, code);
					if (unloco != null)
					{
						return (unloco.RL_Code, true);
					}
				}
			}

			return (code, false);
		}

		public override bool AutoCompleteOnCommit
		{
			get { return true; }
		}
	}
}
