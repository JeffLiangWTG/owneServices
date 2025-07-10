using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.GUI.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(PLDefaultCommunicationChannelNCTSP5Control))]
class PLDefaultCommunicationChannelNCTSP5ControlTest : RegistryZUserControlTestCase
{
	protected override IBusiness GetNewBusinessEntity() => new PLDefaultCommunicationChannelNCTSP5 { IsEmailChannel = true };

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;

	public void TestEmailChannelRadioButton()
	{
		using (var control = new PLDefaultCommunicationChannelNCTSP5Control())
		{
			var emailChannelRadioButtonFindButton = control.FindSingle<ZRadioButton>("emailChannelRadioButton1");

			CombineAssertions("EmailChannelRadioButton1: ", () =>
			{
				control.ReadOnly = true;
				AssertEquals("readOnly", true, emailChannelRadioButtonFindButton.ReadOnly);

				control.ReadOnly = false;
				AssertEquals("readOnly", false, emailChannelRadioButtonFindButton.ReadOnly);
			});
		}
	}

	public void TestSeapIDRadioButton()
	{
		using (var control = new PLDefaultCommunicationChannelNCTSP5Control())
		{
			var seapIDRadioButtonFindButton = control.FindSingle<ZRadioButton>("seapIDRadioButton2");

			CombineAssertions("seapIDRadioButton2: ", () =>
			{
				control.ReadOnly = true;
				AssertEquals("readOnly", true, seapIDRadioButtonFindButton.ReadOnly);

				control.ReadOnly = false;
				AssertEquals("readOnly", false, seapIDRadioButtonFindButton.ReadOnly);
			});
		}
	}
}
