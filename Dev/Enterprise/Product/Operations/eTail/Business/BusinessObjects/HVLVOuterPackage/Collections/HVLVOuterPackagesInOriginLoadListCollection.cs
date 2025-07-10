using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class HVLVOuterPackagesInOriginLoadListCollection : DependentBusinessObjectCollection<HVLVOuterPackage, HVLVOriginLoadList>, IHVLVOuterPackageCollection
	{
		public HVLVOuterPackagesInOriginLoadListCollection(HVLVOriginLoadList master)
			: base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return HVLVOuterPackageSchema.HVO_HVL_LoadList; }
		}

		IHVLVOuterPackage IHVLVOuterPackageCollection.this[int i] => (IHVLVOuterPackage)Elements[i];
	}
}
