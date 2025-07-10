using System;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// An enumeration for a set of styles that can be applied to custom label controls and grid columns.
	/// </summary>
	[Flags]
	public enum CustomLabelStyles
	{
		None = 0,
		UpperCase = 1 << 0,
		ShowByDefault = 1 << 1,
		AvailableByDefault = 1 << 2,
		MultiLineTextBox = 1 << 3
	}
}
