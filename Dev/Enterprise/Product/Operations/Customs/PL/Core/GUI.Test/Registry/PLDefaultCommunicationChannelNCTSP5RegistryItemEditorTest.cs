using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.GUI.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(PLDefaultCommunicationChannelNCTSP5RegistryItemEditor))]
class PLDefaultCommunicationChannelNCTSP5RegistryItemEditorTest : RegistryItemEditorTestCase
{
	protected override RegistryItemEditor GetEditor() => new PLDefaultCommunicationChannelNCTSP5RegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

	protected override bool GetEditorPaneEnabledState(Control editorPane) => !((PLDefaultCommunicationChannelNCTSP5Control)editorPane).ReadOnly;

	protected override Type GetExpectedEditorPaneType() => typeof(PLDefaultCommunicationChannelNCTSP5Control);

	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
	{
		var nctsSettings = ObjectFactory.Get<Integration.Customs.Shared.INctsSettings>();
		var isPhase5 = nctsSettings.IsUsingPhase5(GlbCompany.CurrentCompany.Country.Code);
		return new PLDefaultCommunicationChannelNCTSP5RegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, isPhase5 ? RegistryOptions.Default : RegistryOptions.IsReadOnly, new PLDefaultCommunicationChannelNCTSP5());
	}

	protected override object[] GetValidRegistryValues()
	{
		var plDefaultCommunicationChannelNCTSP5 = new PLDefaultCommunicationChannelNCTSP5(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
		return new object[] { plDefaultCommunicationChannelNCTSP5 };
	}
}
