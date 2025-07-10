using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class FreeDayExclusionColumnStyleInfo : ZGuidFindBoxColumnStyleInfo
	{
		public override Type ColumnStyleType => typeof(FreeDayExclusionColumnStyle);
	}

	public class FreeDayExclusionColumnStyle(FreeDayExclusionColumnStyleInfo columnInfo) : ZGuidFindBoxColumnStyle(() => new FreeDayExclusionFindBox(), columnInfo)
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
