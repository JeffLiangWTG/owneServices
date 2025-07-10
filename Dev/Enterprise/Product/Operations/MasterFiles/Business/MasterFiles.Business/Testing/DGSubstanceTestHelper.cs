#if DEBUG

using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class DGSubstanceTestHelper
	{
		public static void LinkDefault(this UNDGDataItem dataItem, IDGSubstance substance)
		{
			var defaultPivot = dataItem.UNDGSubstancePivotCollection.FirstOrDefault(substancePivot => substancePivot.DP_IsDefault);
			if (defaultPivot != null)
			{
				defaultPivot.DP_IsDefault = false;
				dataItem.UNDGSubstancePivotCollection.RemoveFromRelationship(defaultPivot);
				defaultPivot.Delete();
			}

			var pivot = dataItem.UNDGSubstancePivotCollection.AddNew();
			pivot.DP_UNNO = substance.UNNO;
			pivot.DP_Variant = substance.Variant;
			pivot.DP_Standard = substance.Standard;
			pivot.DP_IsDefault = true;
		}

		public static UNDGSubstance Create(string unno, string variant, string standard, Action<UNDGSubstance> additionalInitialisation = null)
		{
			var factory = new BusinessObjectFactory();

			var substance = factory.New<UNDGSubstance>();
			substance.DG_UNNO = unno;
			substance.DG_Variant = variant;
			substance.DG_Standard = standard;

			additionalInitialisation?.Invoke(substance);

			factory.Save();
			return substance;
		}

		public static void CreateIfDoesntExist(string unno, string variant, string standard, Action<UNDGSubstance> additionalInitialisation = null)
		{
			var factory = new BusinessObjectFactory();

			var query = new ZQuery();
			query.AddToFilter(UNDGSubstanceSchema.DG_UNNO, unno);

			if (!string.IsNullOrEmpty(variant))
			{
				query.AddToFilter(UNDGSubstanceSchema.DG_Variant, variant);
			}

			query.AddToFilter(UNDGSubstanceSchema.DG_Standard, standard);

			var substance = factory.LoadTop1<UNDGSubstance>(query);
			if (substance == null)
			{
				Create(unno, variant, standard, additionalInitialisation);
			}
			else
			{
				additionalInitialisation?.Invoke(substance);
			}
		}
	}
}

#endif
