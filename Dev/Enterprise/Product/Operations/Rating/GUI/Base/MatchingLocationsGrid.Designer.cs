using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class MatchingLocationsGrid
	{
		void InitializeComponent()
		{
			// LocationSourceOption column
			CompileTimeCheckBindingMember.Check(((RateEntryLocation)(null)).LocationSourceOption);
			var locationSourceOptionColumn =
				new ZDropEditColumnStyleInfo(nameof(RateEntryLocation.LocationSourceOption), 50)
				{
					CharacterCasing = CharacterCasing.Upper,
				};

			// Location column
			CompileTimeCheckBindingMember.Check(((RateEntryLocation)(null)).Location);
			var locationColumn = new ZCodeFindBoxColumnStyleInfo()
			{
				ColumnName = nameof(RateEntryLocation.Location), CharacterCasing = CharacterCasing.Upper,
			};
			ControlDpiScalingHelper.SetWidth(ref locationColumn, 60, true);

			BeginInit();
			SuspendLayout();

			RemoveAction = RemoveAction.RemoveAndDelete;
			DisableImportDataMenuItem = true;
			AllowReadOnlyRowsToBeDeleted = true;
			AllowNavigation = false;
			Dock = DockStyle.Fill;
			GridId = "26711112-246B-4ADE-8867-D2F45C11E69F";
			HeaderForeColor = SystemColors.ControlText;
			LayoutKey = "TemplateRateEntryLocationsGrid";
			Name = "TemplateRateEntryLocationsGrid";
			Columns.Add(locationSourceOptionColumn);
			Columns.Add(locationColumn);
			CaptionText = "Matching Locations";
			CaptionVisible = false;
			IsCustomiseMenuVisible = false;

			EndInit();
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
