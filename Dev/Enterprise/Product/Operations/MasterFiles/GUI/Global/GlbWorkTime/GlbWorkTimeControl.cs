using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

#if WINZOR
using System.Threading.Tasks;
using WinzorFramework.JSInterop;
#endif

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbWorkTimeControl : ZUserControl
	{
		public GlbWorkTimeControl()
		{
			InitializeComponent();
			SetWorkHoursFont();
			PatternExistsLabel.Visible = false;

#if WINZOR
			foreach (var textBox in WorkingHoursTextBoxes)
			{
				textBox.MatchExpression = "[^* ]";
			}
#endif
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			var readOnly = (CurrentDataItem as GlbWorkTimeViewModel)?.IsReadOnly ?? true;
			PatternExistsLabel.Visible = readOnly;
			AMLabel.Visible = !readOnly;
			PMLabel.Visible = !readOnly;
		}

		#region Font

		void SetWorkHoursFont()
		{
#if WINZOR
			//Fonts rendered larger in Winzor than in Winform, reduce the font size to line up with the hour labels
			var fixedFont = new Font("Courier New", 10F);
#else
			var fixedFont = new Font("Courier New", 10.5F);
#endif

			foreach (var textBox in WorkingHoursTextBoxes)
			{
				textBox.Font = fixedFont;
			}
		}

#endregion

		#region Text Changed

		void WorkingHoursBoundText_TextChanged(object sender, EventArgs e)
		{
			var textBox = (ZTextBox)sender;
			var strippedText = Regex.Replace(textBox.Text, "[^* ]", "");

			if (strippedText != textBox.Text)
			{
				var caretPosition = Math.Max(0, Math.Min(strippedText.Length, textBox.SelectionStart - 1));
				textBox.Text = strippedText;
				textBox.SelectionStart = caretPosition;
			}
		}

#endregion

#if DEBUG
		internal
#endif
		IEnumerable<ZTextBox> WorkingHoursTextBoxes
		{
			get
			{
				return new[]
				{
					MondayWorkingHoursBoundText,
					TuesdayWorkingHoursBoundText,
					WednesdayWorkingHoursBoundText,
					ThursdayWorkingHoursBoundText,
					FridayWorkingHoursBoundText,
					SaturdayWorkingHoursBoundText,
					SundayWorkingHoursBoundText
				};
			}
		}
	}
}
