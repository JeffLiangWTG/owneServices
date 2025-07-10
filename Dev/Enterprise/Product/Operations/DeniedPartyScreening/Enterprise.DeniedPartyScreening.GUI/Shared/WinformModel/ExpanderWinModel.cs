namespace Enterprise.DeniedPartyScreening.GUI
{
	public abstract class ExpanderWinModel<T>
	{
		public abstract string ExpanderTitle { get; }

		public abstract string ExpanderDescription { get; }

		public abstract bool IsExpanderEnabled { get; }

		public virtual bool IsExpanderExpanded { get; set; }
	}
}
