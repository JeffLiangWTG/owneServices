using System.Collections.Generic;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public interface ISupportNotifyPropertyChanged
	{
		List<NotifyPropertyChanged> NotifyPropertyChanges { get; }
	}
}
