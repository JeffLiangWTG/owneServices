using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(SellRatesDecimalsRegistryItemEditor))]
	public class SellRatesDecimalsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new SellRatesDecimalsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((SellRatesDecimalsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(SellRatesDecimalsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new SellRatesDecimalsRegistryItem("", null, null, null, RegistryStorageFlags.System, new SellRatesDecimalsCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { SellRatesDecimalsCollection.GetDefault() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
