using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgAddressAdditionalInfoUserControl : ZUserControl
	{
		public OrgAddressAdditionalInfoUserControl()
		{
			InitializeComponent();

			if (!this.IsDesignMode())
			{
				SetCharacterCasing();
			}
		}

		void SetCharacterCasing()
		{
			foreach (var styleInfo in AdditionalInfoGrid.ColumnStyles)
			{
				if (styleInfo is ZTextBoxColumnStyleInfo info)
				{
					info.CharacterCasing = RequiredCasing;
				}
			}
		}

		CharacterCasing RequiredCasing => Env.Registry.OrgAllowMixedCase ? CharacterCasing.Normal : CharacterCasing.Upper;
	}
}
