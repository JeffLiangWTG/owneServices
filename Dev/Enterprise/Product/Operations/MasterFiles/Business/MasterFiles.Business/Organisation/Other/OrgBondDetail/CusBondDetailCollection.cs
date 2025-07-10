using CargoWise.EntityFramework;
using CargoWise.Schema;

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CusBondDetailCollection : DependentBusinessObjectCollection<CusBondDetail, OrgHeader>
	{
		public CusBondDetailCollection(OrgHeader parentOrganisation)
			: base(parentOrganisation)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusBondDetailSchema.PW_ParentID; }
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			CusBondDetail child = (CusBondDetail)dependent;
			child.Parent = Master;
		}
	}
}
