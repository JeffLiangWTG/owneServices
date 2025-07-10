namespace Enterprise.MasterFiles.Integration
{
	public struct TabPlacement
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public static class Placements
		{
			public const string TopLeft = "Left Top";
			public const string BottomLeft = "Left Bottom";
			public const string TopMiddle = "Middle Top";
			public const string BottomMiddle = "Middle Bottom";
			public const string TopRight = "Right Top";
			public const string BottomRight = "Right Bottom";
		}

		public TabPlacement(string tabPageName, string placement, int rowNumber)
		{
			this.TabPageName = tabPageName;
			this.Placement = placement;
			this.RowNumber = rowNumber;
		}

		public readonly int RowNumber;
		public readonly string TabPageName;
		public readonly string Placement;
	}
}
