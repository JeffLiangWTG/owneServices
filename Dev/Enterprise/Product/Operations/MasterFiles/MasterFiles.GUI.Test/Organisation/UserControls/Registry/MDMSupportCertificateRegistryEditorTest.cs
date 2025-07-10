using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Registry.GUI.RegistryItemEditor;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(MDMSupportCertificateRegistryEditor))]
	public class MDMSupportCertificateRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override EditorPaneAnchor ExpectedAnchor => EditorPaneAnchor.All;

		protected override RegistryItemEditor GetEditor()
		{
			return new MDMSupportCertificateRegistryEditor(RegistryItem.DataType as MDMSupportCertificateRegistryDataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((MDMSupportCertificateUserControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(MDMSupportCertificateUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new MDMSupportCertificateRegistryItem("Test", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", MDMProductCodes.AVS);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new [] { new SystemToSystemTrustInfo() };
		}
	}
}
