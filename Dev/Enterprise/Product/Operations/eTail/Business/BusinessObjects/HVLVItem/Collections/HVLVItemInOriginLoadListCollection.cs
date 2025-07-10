using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class HVLVItemInOriginLoadListCollection : DependentBusinessObjectCollection<HVLVItem, HVLVOriginLoadList>, IHVLVItemCollection
	{
		public HVLVItemInOriginLoadListCollection(HVLVOriginLoadList master)
			: base(master)
		{
		}

		protected override bool AllowNewCore => false;

		public override bool ReadOnly => true;

		IHVLVItem IHVLVItemCollection.this[int i] => (HVLVItem)Elements[i];

		protected override SchemaGuidColumn FKSchemaColumnInDependent => HVLVItemSchema.HVI_HVL_LoadList;
	}
}
