using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	/// <summary>
	/// Summary description for RefCountryStatesForm.
	/// </summary>
	public partial class RefCountryStatesForm : ZForm
	{
		public RefCountryStatesForm()
		{
			SetCasing();
		}

		public RefCountryStatesForm(RefCountryStates countryStates) : base(countryStates)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			IsActiveCheckBox.SelectNextControlNonTabStopNonReadOnly(IsSystemCheckBox, true, true, true);
			SetCasing();
			PlugIns.Add(ControllerIDs.Audit);
		}

		void SetCasing()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				DescriptionTextBox.CharacterCasing = Env.Registry.OrgAllowMixedCase ? CharacterCasing.Normal : CharacterCasing.Upper;
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (IsActiveCheckBox.Focused)
			{
				if ((keyData == (Keys.Shift | Keys.Tab)))
				{
					IsSystemCheckBox.Focus();
					return true;
				}
				else if (keyData == Keys.Tab)
				{
					CodeTextBox.Focus();
					return true;
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
