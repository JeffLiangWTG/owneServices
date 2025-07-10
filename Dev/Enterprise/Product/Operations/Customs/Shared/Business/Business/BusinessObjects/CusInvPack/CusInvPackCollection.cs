using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class CusInvPackCollection<T, MasterT> : DependentBusinessObjectCollection<T, MasterT>
		where T : CusInvPack
		where MasterT : BusinessObject
	{
		protected CusInvPackCollection(MasterT parent)
			: base(parent)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusInvPackSchema.B5_ParentID; }
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			T child = (T)dependent;
			child.Parent = Master;
		}
	}
}
