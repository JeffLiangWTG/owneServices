using System;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(DelayAlertDeliveryRegistryItemEditor))]
	sealed class DelayAlertDeliveryRegistryItemEditorTest : RegistryItemEditorTestCase
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
			return !((DelayAlertDeliveryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DelayAlertDeliveryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DelayAlertDeliveryRegistryItem("", null, null, null, RegistryStorageFlags.System, new DelayAlertDeliveryRuleCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DelayAlertDeliveryRegistryItemEditor(new DelayAlertDeliveryRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override object[] GetValidRegistryValues()
		{
			DelayAlertDeliveryRuleCollection rules = new DelayAlertDeliveryRuleCollection();
			rules.Add(new DelayAlertDeliveryRule());

			return new object[] { rules };
		}

		#endregion
	}
}
