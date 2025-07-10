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
	[TestedType(typeof(ContainerTranshipmentIndicatorRegistryItemEditor))]
	internal class ContainerTranshipmentIndicatorRegistryItemEditorTest : RegistryItemEditorTestCase
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
			return !((ContainerTranshipmentIndicatorControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ContainerTranshipmentIndicatorControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ContainerTranshipmentIndicatorRegistryItem("", null, null, null, RegistryStorageFlags.System, new ContainerTranshipmentIndicatorCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ContainerTranshipmentIndicatorRegistryItemEditor(new ContainerTranshipmentIndicatorRegistryDataType(ContainerTranshipmentIndicatorCollection.NewAndPopulate()), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new ContainerTranshipmentIndicatorCollection() };
		}
		#endregion
	}
}
