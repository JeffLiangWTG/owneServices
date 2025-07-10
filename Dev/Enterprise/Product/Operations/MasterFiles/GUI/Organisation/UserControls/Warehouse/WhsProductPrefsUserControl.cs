using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class WhsProductPrefsUserControl : ZUserControl
	{
		public WhsProductPrefsUserControl()
		{
			InitializeComponent();
			UpdateReadOnly();
			HookEvents();
		}

		#region ReadOnly

		void IncludeClientInABCAnalysisCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateReadOnly();
		}

		void UpdateReadOnly()
		{
			ABCAnalysisMethodDropEdit.ReadOnly = !IncludeClientInABCAnalysisCheckBox.Checked;
			ABCAnalysisPeriodDropEdit.ReadOnly = !IncludeClientInABCAnalysisCheckBox.Checked;
		}

		#endregion

		#region Events

		void HookEvents()
		{
			IncludeClientInABCAnalysisCheckBox.CheckedChanged += new EventHandler(IncludeClientInABCAnalysisCheckBox_CheckedChanged);
		}

		void UnHookEvents()
		{
			IncludeClientInABCAnalysisCheckBox.CheckedChanged -= new EventHandler(IncludeClientInABCAnalysisCheckBox_CheckedChanged);
		}

		#endregion
	}
}
