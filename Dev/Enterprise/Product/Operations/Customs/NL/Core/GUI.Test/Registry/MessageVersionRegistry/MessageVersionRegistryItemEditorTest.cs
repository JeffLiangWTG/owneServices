using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(MessageVersionRegistryItemEditor))]
sealed class MessageVersionRegistryItemEditorTest : RegistryItemEditorTestCase
{
	protected override RegistryItemEditor GetEditor() => new MessageVersionRegistryItemEditor(new MessageVersionRegistryDataType(), new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), new BusinessObjectFactory());

	protected override bool GetEditorPaneEnabledState(Control editorPane)
	{
		var control = (MessageVersionRegistryUserControl)editorPane;
		var messageVersionGrid = control.FindSingle<ZArchitecture.ZGrid>("MessageVersionGrid");
		return !messageVersionGrid.ReadOnly;
	}

	protected override Type GetExpectedEditorPaneType() => typeof(MessageVersionRegistryUserControl);

	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new MessageVersionRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, MessageVersionRegistryCollection.DefaultCollection);

	protected override object[] GetValidRegistryValues() => new object[] { new MessageVersionRegistryCollection { new MessageVersionRegistry { DomainCode = MessageVersionRegistry.NCTSP5DomainCode, TargetSystemName = MessageVersionRegistry.NCTSP5DefaultTarget } } };

	protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeftRight;
}
