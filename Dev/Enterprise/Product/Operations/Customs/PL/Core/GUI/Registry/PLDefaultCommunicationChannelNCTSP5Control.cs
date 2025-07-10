using Enterprise.Registry.GUI;

namespace Enterprise.Customs.PL.GUI.Registry;

public partial class PLDefaultCommunicationChannelNCTSP5Control : RegistryZUserControl
{
	public PLDefaultCommunicationChannelNCTSP5Control()
	{
		InitializeComponent();
	}

	protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
	{
		base.SetControlOrBusinessEntityReadOnly(readOnly);
		emailChannelRadioButton1.ReadOnly = readOnly;
		seapIDRadioButton2.ReadOnly = readOnly;
	}
}
