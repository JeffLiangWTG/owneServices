namespace Enterprise.HRM.Common
{
	public class ReviewProcessNodeLookups : AutoReviewProcessNodeLookups
	{
		public ReviewProcessNodeLookups(AutoReviewProcessNode parent) : base(parent)
		{
			ReviewProcessNodes = new ReviewProcessNodeCollection(parent.Factory);
		}

		public ReviewProcessNodeCollection ReviewProcessNodes { get; }
	}
}
