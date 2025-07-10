using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.PL.GUI;

[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
public partial class SpecificDataUserControl : ZUserControl
{
	public SpecificDataUserControl()
	{
		InitializeComponent();
	}

	internal void UpdateLayout(IPanelLayoutProvider panelLayoutProvider)
	{
		DynamicSpecificDataPanel.UpdateLayout(panelLayoutProvider);
	}

	public static PropertyDescriptor[] GetPropertyDescriptors()
	{
		return new ControlPropertyDescriptorBuilder<SpecificDataUserControl>()
			.Property(nameof(IsVisibleForBinding), ZBool.False, false)
			.Result;
	}
}
