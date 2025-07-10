using System;
using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(PortMessagingPortRegistryItemEditor))]
	internal class PortMessagingPortRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public override void TestEditorPaneLayout()
		{
			var editor = GetEditor();
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
			return new PortMessagingPortRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new PortMessagingPortRegistryItemEditor(RegistryItem.DataType, NewFallbackLevel(), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PortMessagingPortControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new PortMessagingPortCollection();
			var port1 = collection.AddNew();
			port1.Port = "NZAKL";
			port1.SenderID = "DFGH";
			port1.Enabled = true;
			var port2 = collection.AddNew();
			port2.Port = "NZTRG";
			port2.SenderID = "KIUY";
			port2.Enabled = true;
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((PortMessagingPortControl)editorPane).ReadOnly;
		}

		FallbackLevel NewFallbackLevel()
		{
			return new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
		}
		#endregion
	}
}
