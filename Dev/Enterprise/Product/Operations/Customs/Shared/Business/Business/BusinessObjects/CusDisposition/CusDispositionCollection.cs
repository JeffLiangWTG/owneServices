using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusDispositionCollection : DependentBusinessObjectCollection<CusDisposition, BusinessObject>
	{
		public CusDispositionCollection(ICusDispositionParent cusDispositionParent)
			: base(cusDispositionParent.CollectionMaster)
		{
			this.CusDispositionParent = cusDispositionParent;
		}
		public ICusDispositionParent CusDispositionParent;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusDispositionSchema.CDI_ParentID; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newChild = child as CusDisposition;
			if (newChild != null)
			{
				newChild.CDI_Type = CusDispositionParent.Type;
			}
		}
	}
}
