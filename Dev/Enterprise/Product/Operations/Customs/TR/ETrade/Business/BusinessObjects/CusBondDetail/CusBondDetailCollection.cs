using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class CusBondDetailCollection<T> : DependentBusinessObjectCollection<CusBondDetail, T>
		where T : BusinessObject
	{
		public CusBondDetailCollection(T parent)
			: base(parent)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusBondDetailSchema.PW_ParentID; }
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var child = (CusBondDetail)dependent;
			child.Parent = Master;
		}
	}
}
