using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.GUI.Testing
{
	[TestedType(typeof(NodiRegistryItemEditor))]
	sealed class NodiRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new NodiRegistryItemEditor(new NodiDataType(), new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			var control = (NodiRegistryItemControl)editorPane;
			return !control.NodiDetailsGrid.ReadOnly;
		}
		protected override Type GetExpectedEditorPaneType() => typeof(NodiRegistryItemControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
			=> new NodiRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, new NodiRegistryCollection().DefaultCollection);

		protected override object[] GetValidRegistryValues() => new object[]
		{
			new NodiRegistryCollection
			{
				new NodiRegistry
				{
					SystemName = NodiRegistry.CustomsProductionSystemName,
					NodiNumber = NodiRegistry.CustomsProductionDefaultNodiNumber
				}
			}
		};

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeftRight;
	}
}
