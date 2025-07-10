using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class MessageUserControl : EU.GUI.MessageUserControl
{
	public MessageUserControl()
	{
		InitializeComponent();
	}

	protected override EU.GUI.EntryLineAdditionalDataUserControl GetEntryLineAdditionalData() => new EntryLineAdditionalDataUserControl();

	protected override IPanelLayoutProvider GetNewEntryDetailsPanelLayout() => new EntryDetailsLayouts();

	protected override ZBool DynamicLayoutApplied => ZBool.True;
}
