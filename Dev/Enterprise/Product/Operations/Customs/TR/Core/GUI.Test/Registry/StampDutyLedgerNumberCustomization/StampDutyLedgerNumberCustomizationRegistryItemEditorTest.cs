using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(StampDutyLedgerNumberCustomizationRegistryItem))]
	internal class StampDutyLedgerNumberCustomizationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new StampDutyLedgerNumberCustomizationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new StampDutyLedgerNumberCustomizationRegistrySetting { ExpiredYear = ZDateTime.Today.Year });
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new StampDutyLedgerNumberCustomizationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(StampDutyLedgerNumberCustomizationRegistryItemUserControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new[] { new StampDutyLedgerNumberCustomizationRegistrySetting { ExpiredYear = ZDateTime.Today.Year } };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((StampDutyLedgerNumberCustomizationRegistryItemUserControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeftRight;
	}
}
