using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class DurationExclusionColumnStyleInfo : ZGuidFindBoxColumnStyleInfo
	{
		public override Type ColumnStyleType => typeof(DurationExclusionColumnStyle);
	}

	public class DurationExclusionColumnStyle(DurationExclusionColumnStyleInfo columnInfo) : ZGuidFindBoxColumnStyle(() => new DurationExclusionFindBox(), columnInfo)
	{
		protected override bool EditControlShownForReadOnlyCore => true;

		public void RefreshCodeBoxText()
		{
			IsEditing = true;
			ColumnStartedEditing(TextBox);
			parentDataGrid.BeginEdit(this, LastFocusedCell.RowNumber);
			HandleSelectFromPopup(this, EventArgs.Empty);
		}
	}
}
