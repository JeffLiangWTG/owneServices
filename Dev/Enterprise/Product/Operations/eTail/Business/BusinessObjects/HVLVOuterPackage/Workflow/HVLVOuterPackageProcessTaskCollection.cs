using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Business
{
	public class HVLVOuterPackageProcessTaskCollection : ProcessTaskCollection
	{
		public HVLVOuterPackageProcessTaskCollection(HVLVOuterPackage outerPackage)
			: base(outerPackage)
		{
		}

		public new HVLVOuterPackage Parent => (HVLVOuterPackage)base.Parent;

		public new HVLVOuterPackageProcessTask this[int index] => (HVLVOuterPackageProcessTask)Elements[index];

		public new HVLVOuterPackageProcessTask AddNew() => (HVLVOuterPackageProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new HVLVOuterPackageProcessTaskCollection(Parent);
	}
}
