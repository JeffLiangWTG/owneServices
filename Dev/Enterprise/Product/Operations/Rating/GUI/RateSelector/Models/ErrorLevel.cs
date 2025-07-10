using System;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	[Flags]
	public enum ErrorLevel
	{
		None = 0,
		Warning = 1,
		Error = 2
	}
}
