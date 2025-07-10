using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CertificateTypeRegistryItemEditor))]
	sealed class CertificateTypeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public override void TestEditorPaneLayout()
		{
			RegistryItemEditor editor = GetEditor();
			using (Control control = editor.NewWinFormsEditorPane())
			{
				editor.SetEditorPaneLayout(control, 666, 333);
				AssertEquals("should have the correct width", 666, control.Width);
				AssertEquals("should have the correct height", 333, control.Height);
				AssertEquals("should be anchored", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, control.Anchor);
			}
		}

		#region Implementation

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CertificateTypeRegistryItemControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CertificateTypeRegistryItemControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CertificateTypeRegistryItem("", null, null, null, RegistryStorageFlags.System, new CertificateTypeCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new CertificateTypeRegistryItemEditor(new CertificateTypeRegistryDataType(new CertificateTypeCollection()), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CertificateTypeCollection() };
		}

		#endregion
	}
}
