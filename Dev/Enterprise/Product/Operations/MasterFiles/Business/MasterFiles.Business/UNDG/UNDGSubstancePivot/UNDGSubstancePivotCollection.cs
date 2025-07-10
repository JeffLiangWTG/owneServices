using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstancePivotCollection : ActiveBusinessObjectCollection<UNDGSubstancePivot>
	{
		public UNDGSubstancePivotCollection(BusinessObjectFactory factory, IUNDGSubstancePivotParent parent, string tablePrefix)
			: base(factory, (BusinessObject)parent, new ZQuery(UNDGSubstancePivotSchema.DP_ParentTableCode, tablePrefix), UNDGSubstancePivotSchema.DP_ParentId)
		{
		}

		public UNDGSubstance DefaultSubstance
		{
			get
			{
				var defaultPivot = this.FirstOrDefault(pivot => pivot.DP_IsDefault);
				return UNDGSubstanceLoader.LoadSubstance(Factory, defaultPivot);
			}
		}

		public void UpdateDefaultPivot(UNDGSubstance substance)
		{
			DeleteAll();

			if (substance == null || substance.DG_UNNO.IsEmpty)
			{
				return;
			}

			var pivot = AddPivotFromSubstance(substance);
			pivot.DP_IsDefault = true;
		}

		public UNDGSubstancePivot AddPivotFromSubstance(UNDGSubstance substance)
		{
			var pivot = AddNew();
			pivot.DP_UNNO = substance.DG_UNNO;
			pivot.DP_Variant = substance.DG_Variant;
			pivot.DP_Standard = substance.DG_Standard;

			return pivot;
		}
	}
}
