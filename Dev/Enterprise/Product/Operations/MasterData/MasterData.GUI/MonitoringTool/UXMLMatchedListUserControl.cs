using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class UXMLMatchedListUserControl : ZUserControl
	{
		public UXMLMatchedListUserControl()
		{
			InitializeComponent();
		}

		public void PopulatePanel(IEnumerable<UXMLMatchingDiagnosticModel> totalsToShow)
		{
			var i = 0;
			totalsToShow.ForEach(o =>
			{
				var uxmlMatchedResultUserControl = new UXMLMatchedResultUserControl(o)
				{
					Name = "uxmlMatchedResult" + o.MatchCode,
					Dock = DockStyle.Fill,
				};

				if (i == 0)
				{
					uxmlMatchedResultUserControl.SetBackColor(CalculateMatchedResultBackColor(o));
				}

				uxmlMatchedResultUserControl.SetDataBinding(o, "");
				controlTableLayout.RowCount = i + 1;
				controlTableLayout.RowStyles.Add(new RowStyle());
				controlTableLayout.Controls.Add(uxmlMatchedResultUserControl, 0, i);
				i += 1;
			});
		}

		public void ClearPanel()
		{
			controlTableLayout.Controls.RemoveAndDisposeAll();
			controlTableLayout.RowCount = 1;
			controlTableLayout.RowStyles.Clear();
		}

		#region Implementation

		Color CalculateMatchedResultBackColor(UXMLMatchingDiagnosticModel model)
		{
			var matchResult = model.Result;
			var backColor = Colors.Default;

			if (matchResult == UXMLMatchingDiagnosticUtils.Constants.MatchAndSelected || matchResult == UXMLMatchingDiagnosticUtils.Constants.Match)
			{
				backColor = Colors.Matched;
			}
			else if (matchResult == UXMLMatchingDiagnosticUtils.Constants.NotMatch)
			{
				backColor = Colors.NotMatched;
			}

			return backColor;
		}

		public static class Colors
		{
			public readonly static Color Matched = Color.FromArgb(198, 236, 198);
			public readonly static Color NotMatched = Color.LightYellow;
			public readonly static Color Default = Color.White;
		}

		#endregion
	}
}
