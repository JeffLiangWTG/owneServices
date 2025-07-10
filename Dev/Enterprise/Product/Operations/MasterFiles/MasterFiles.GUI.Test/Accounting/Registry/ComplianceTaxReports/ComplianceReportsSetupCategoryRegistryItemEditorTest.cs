using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ComplianceReportsSetupCategoryRegistryItemEditor))]
	sealed class ComplianceReportsSetupCategoryRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ComplianceReportsSetupCategoryRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ComplianceReportsSetupCategoryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ComplianceReportsSetupCategoryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ComplianceReportsSetupRegistryItem(string.Empty, null, null, null, new ComplianceReportTypeCollection(), RegistryOptions.Default);
		}
		protected override object[] GetValidRegistryValues()
		{
			ComplianceReportTypeCollection collection = new ComplianceReportTypeCollection("Test01");
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
