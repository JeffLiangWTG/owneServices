using System.Collections.Generic;

namespace Enterprise.MasterData.Business
{
	public interface ISupportNotifyPropertyChanged
	{
		List<PropertyChangedNotify> NotifyPropertyChanges { get; }
	}
}
