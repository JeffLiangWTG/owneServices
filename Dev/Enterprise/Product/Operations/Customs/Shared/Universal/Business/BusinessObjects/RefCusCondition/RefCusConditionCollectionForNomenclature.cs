using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusConditionCollectionForNomenclature : ActiveBusinessObjectCollection<RefCusCondition>
	{
		public RefCusConditionCollectionForNomenclature(RefCusNomenclatureGroup parentNomenclature)
			: base(parentNomenclature.Factory, parentNomenclature, new ZQuery(), RefCusConditionSchema.ZX1_ZZ5_Nomenclature)
		{
		}

		protected override bool AllowNew => false;
	}
}
