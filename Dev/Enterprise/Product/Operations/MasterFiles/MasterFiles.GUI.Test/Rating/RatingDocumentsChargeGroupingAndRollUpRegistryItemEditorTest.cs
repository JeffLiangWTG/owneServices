using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RatingDocumentsChargeGroupingAndRollUpRegistryItemEditor))]
	public class RatingDocumentsChargeGroupingAndRollUpRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new RatingDocRollupOrGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new RatingDocRollupOrGroupRegistryCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new RatingDocumentsChargeGroupingAndRollUpRegistryItemEditor(null, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(RatingDocumentsChargeGroupingAndRollUpControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new RatingDocRollupOrGroupRegistryCollection() };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((RatingDocumentsChargeGroupingAndRollUpControl)editorPane).ReadOnly;
		}
	}
}
