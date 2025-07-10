using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class HVLVOuterPackageItemCollection : DependentBusinessObjectCollection<HVLVItem, HVLVOuterPackage>, IHVLVItemCollection
	{
		public HVLVOuterPackageItemCollection(HVLVOuterPackage master)
			: base(master)
		{
		}

		IHVLVItem IHVLVItemCollection.this[int i] => (HVLVItem)Elements[i];

		protected override SchemaGuidColumn FKSchemaColumnInDependent => HVLVItemSchema.HVI_HVO_OuterPackage;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
