using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(PortManifestPortRegistryItemEditor))]
	sealed class PortManifestPortRegistryItemEditorTest : RegistryItemEditorTestCase
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

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new PortManifestPortCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new PortManifestPortRegistryItemEditor(RegistryItem.DataType, NewFallbackLevel(), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PortManifestPortControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new PortManifestPortCollection();
			AddSetting(collection, "NZNPE", 1);
			AddSetting(collection, "NZAKL", 2);
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((PortManifestPortControl)editorPane).ReadOnly;
		}

		void AddSetting(PortManifestPortCollection collection, ZString port, int num)
		{
			var setting = collection.AddNew();
			setting.Port = port;
			setting.PrincipalPK = ZGuid.Empty;
			setting.SenderID = string.Format("SenderID_{0}", num);
			setting.Enabled = true;
		}

		FallbackLevel NewFallbackLevel()
		{
			return new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		#endregion
	}
}
