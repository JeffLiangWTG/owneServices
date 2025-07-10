using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class CusStorageDocPivotCollection<TPivot, TParent> : DependentBusinessObjectCollection<TPivot, TParent>
		where TPivot : BaseCusStorageDocPivot
		where TParent : BusinessObject
	{
		protected CusStorageDocPivotCollection(TParent master) : base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusStorageDocPivotSchema.CSD_ParentID;
	}
}
