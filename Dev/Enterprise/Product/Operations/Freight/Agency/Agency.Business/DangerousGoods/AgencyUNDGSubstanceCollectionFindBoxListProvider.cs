using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyUNDGSubstanceCollectionFindBoxListProvider : FindBoxListProvider
	{
		public AgencyUNDGSubstanceCollectionFindBoxListProvider(AgencyUNDGSubstanceCollection collection) : base(collection)
		{
			this.cfrShouldBeDefaulted = collection?.CfrShouldBeDefaulted ?? false;
		}

		readonly bool cfrShouldBeDefaulted;

		public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
		{
			ZQuery filterQuery;

			if (cfrShouldBeDefaulted)
			{
				filterQuery = new ZQuery(UNDGSubstanceSchema.DG_Standard, SQLComparisonOperator.Equal, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR);
			}
			else
			{
				filterQuery = new ZQuery(UNDGSubstanceSchema.DG_Standard, SQLComparisonOperator.Equal, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			}

			AddCodeStartsWithFilter(filterQuery, code);
			AddIsActiveFilter(filterQuery, code);

			var bizObj = List.Factory.LoadTop1(GetTypeOfElements(code), filterQuery);
			return bizObj != null ? (bizObj[GetCodePropertyName(code)].ToString(), true) : base.NearestMatchCore(code, explicitAutoComplete);
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
		{
			ZQuery filterQuery;

			if (cfrShouldBeDefaulted)
			{
				filterQuery = new ZQuery(UNDGSubstanceSchema.DG_Standard, SQLComparisonOperator.Equal, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR);
			}
			else
			{
				filterQuery = new ZQuery(UNDGSubstanceSchema.DG_Standard, SQLComparisonOperator.Equal, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			}

			AddCodeEqualsFilter(filterQuery, code);
			filterQuery.AddToFilter(List.CompleteFilter);

			var imoUNDGSubstances = List.Factory.Load(GetTypeOfElements(code), filterQuery);
			if (imoUNDGSubstances.Any())
			{
				return imoUNDGSubstances;
			}

			return base.BizObjsFromCodeWithCompleteFilter(code);
		}
	}
}
