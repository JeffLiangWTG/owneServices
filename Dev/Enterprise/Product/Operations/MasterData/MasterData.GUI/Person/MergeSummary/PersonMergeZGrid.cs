using System;
using System.Drawing;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterData.GUI
{
	class PersonMergeZGrid : ZGrid
	{
		public event Func<string, Color> ForeColourDeciding;
		internal Color GetCellForeColour(string cellText)
		{
			return ForeColourDeciding?.Invoke(cellText) ?? Color.Black;
		}
	}
}
