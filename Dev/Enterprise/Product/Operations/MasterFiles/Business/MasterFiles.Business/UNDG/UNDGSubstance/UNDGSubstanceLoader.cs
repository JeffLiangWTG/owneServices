using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class UNDGSubstanceLoader
	{
		public static UNDGSubstance LoadSubstance(BusinessObjectFactory factory, UNDGSubstancePivot pivot)
		{
			if (pivot == null)
			{
				return null;
			}

			return LoadSubstances(factory, pivot.DP_UNNO, pivot.DP_Variant, pivot.DP_Standard).FirstOrDefault();
		}

		public static ZQuery BuildSubstanceQuery(string unno, string variant = "", string standard = "")
		{
			var query = new ZQuery();
			query.AddToFilter(new ZQuery(UNDGSubstanceSchema.DG_UNNO, unno));

			if (!string.IsNullOrWhiteSpace(variant))
			{
				query.AddToFilter(new ZQuery(UNDGSubstanceSchema.DG_Variant, variant));
			}

			if (!string.IsNullOrEmpty(standard))
			{
				query.AddToFilter(new ZQuery(UNDGSubstanceSchema.DG_Standard, standard));
			}

			return query;
		}

		public static IEnumerable<UNDGSubstance> LoadSubstances(BusinessObjectFactory factory, string unno, string variant = "", string standard = "")
		{
			if (string.IsNullOrWhiteSpace(unno))
			{
				return Enumerable.Empty<UNDGSubstance>();
			}

			var query = BuildSubstanceQuery(unno, variant, standard);
			return factory.Load<UNDGSubstance>(query);
		}
	}
}
