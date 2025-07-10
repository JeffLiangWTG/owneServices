using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business
{
	public sealed class CusStorageDocPivotCollection : Customs.Business.CusStorageDocPivotCollection<CusStorageDocPivot, BusinessObject>
	{
		public CusStorageDocPivotCollection(BusinessObject master) : base(master)
		{
		}

		public new ICusStorageDocPivotParent Master => (ICusStorageDocPivotParent)base.Master;

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var child = (CusStorageDocPivot)dependent;
			child.Parent = Master as BusinessObject;
		}
	}
}
