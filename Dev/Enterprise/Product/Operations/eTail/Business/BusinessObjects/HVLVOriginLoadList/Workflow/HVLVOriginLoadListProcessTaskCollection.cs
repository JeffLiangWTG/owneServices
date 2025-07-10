using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Business
{
	public class HVLVOriginLoadListProcessTaskCollection : ProcessTaskCollection
	{
		public HVLVOriginLoadListProcessTaskCollection(HVLVOriginLoadList originLoadList)
			: base(originLoadList)
		{
		}

		public new HVLVOriginLoadList Parent => (HVLVOriginLoadList)base.Parent;

		public new HVLVOriginLoadListProcessTask this[int index] => (HVLVOriginLoadListProcessTask)Elements[index];

		public new HVLVOriginLoadListProcessTask AddNew() => (HVLVOriginLoadListProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new HVLVOriginLoadListProcessTaskCollection(Parent);
	}
}
