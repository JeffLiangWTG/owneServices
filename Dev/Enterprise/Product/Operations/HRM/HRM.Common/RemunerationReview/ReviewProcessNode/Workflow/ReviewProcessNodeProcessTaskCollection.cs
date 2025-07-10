using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.HRM.Common
{
	public class ReviewProcessNodeProcessTaskCollection : ProcessTaskCollection
	{
		public ReviewProcessNodeProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public new ReviewProcessNodeProcessTask this[int index] => (ReviewProcessNodeProcessTask)base[index];
		public new ReviewProcessNodeProcessTask AddNew() => (ReviewProcessNodeProcessTask)base.AddNew();
		public new ReviewProcessNode Parent => (ReviewProcessNode)base.Parent;
	}
}
