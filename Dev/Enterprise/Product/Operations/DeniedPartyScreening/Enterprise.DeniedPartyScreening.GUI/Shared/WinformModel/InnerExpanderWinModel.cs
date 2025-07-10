namespace Enterprise.DeniedPartyScreening.GUI
{
	public abstract class InnerExpanderWinModel<T> : ExpanderWinModel<T> where T : class
	{
		public abstract string InnerExpanderTitle { get; }

		public bool IsInnerExpanderExpanded { get; set; }
	}
}
