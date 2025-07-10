using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVClearanceProcessTaskCollection : ProcessTaskCollection
	{
		public CusUSLVClearanceProcessTaskCollection(BusinessObject parent)
			: base(parent) { }

		new CusUSLVClearance Parent
		{
			get { return (CusUSLVClearance)base.Parent; }
		}

		public new CusUSLVClearanceProcessTask this[int index] => (CusUSLVClearanceProcessTask)Elements[index];

		public new CusUSLVClearanceProcessTask AddNew()
		{
			return (CusUSLVClearanceProcessTask)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return new CusUSLVClearanceProcessTaskCollection(Parent);
		}
	}
}
