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
	[TestedType(typeof(AllocationMethodDefaultRegistryItemEditor))]
	sealed class AllocationMethodDefaultRegistryItemEditorTest : RegistryItemEditorTestCase
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
			return new AllocationMethodDefaultRegistryItem("", null, null, null, new AllocationMethodDefaultHeader());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new AllocationMethodDefaultRegistryItemEditor(new AllocationMethodDefaultRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AllocationMethodDefaultRegistryItemControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			AllocationMethodDefaultHeader empty = new AllocationMethodDefaultHeader(Factory);
			empty.DefaultAllocationMethod = AllocationMethodList.Codes.NotSet;

			AllocationMethodDefaultHeader nonEmpty = new AllocationMethodDefaultHeader(Factory);
			nonEmpty.DefaultAllocationMethod = AllocationMethodList.Codes.Ignore;

			AllocationMethodDefaultRule rule = nonEmpty.Rules.AddNew();
			rule.CountryCode = "AU";
			rule.AllocationMethod = AllocationMethodList.Codes.Origin;

			return new object[]
			{
				empty,
				nonEmpty,
			};
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			RegistryZUserControl control = (RegistryZUserControl)editorPane;
			return !control.ReadOnly;
		}

		#endregion
	}
}
