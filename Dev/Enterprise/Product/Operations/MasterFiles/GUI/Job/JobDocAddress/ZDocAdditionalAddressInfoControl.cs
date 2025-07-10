using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ZDocAdditionalAddressInfoControl : ZUserControl
	{
		public ZDocAdditionalAddressInfoControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				SetCharacterCasing();
			}

#if DEBUG
			TypeDescriptor.AddAttributes(AdditionalInfoLabel, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		void SetCharacterCasing()
		{
			AdditionalAddressInfoDropEdit.CharacterCasing = Env.Registry.OrgAllowMixedCase ? CharacterCasing.Normal : CharacterCasing.Upper;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			AdditionalAddressInfoDropEdit.SetDataBinding(dataSource, dataMember);
		}
	}
}
