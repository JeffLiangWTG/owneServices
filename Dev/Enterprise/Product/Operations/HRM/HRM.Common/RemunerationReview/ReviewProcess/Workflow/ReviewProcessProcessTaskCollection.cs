using Enterprise.MasterFiles.Business;

namespace Enterprise.HRM.Common
{
	public class ReviewProcessProcessTaskCollection : ProcessTaskCollection
	{
		public ReviewProcessProcessTaskCollection(ReviewProcess parent) : base(parent)
		{
		}

		public new ReviewProcessProcessTask this[int index] => (ReviewProcessProcessTask)Elements[index];
		public new ReviewProcessProcessTask AddNew() => (ReviewProcessProcessTask)base.AddNew();
		public new ReviewProcess Parent => (ReviewProcess)base.Parent;
	}
}
