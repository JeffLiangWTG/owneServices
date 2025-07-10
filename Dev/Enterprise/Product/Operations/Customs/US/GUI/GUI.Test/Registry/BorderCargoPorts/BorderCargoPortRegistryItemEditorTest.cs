using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(BorderCargoPortRegistryItemEditor))]
	sealed class BorderCargoPortRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new BorderCargoPortRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((BorderCargoPortUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(BorderCargoPortUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new BorderCargoPortRegistryItem("", null, null, null, RegistryStorageFlags.All);

		protected override object[] GetValidRegistryValues() => new object[] { GetValidValue() };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		BorderCargoPortCollection GetValidValue()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var collection = new BorderCargoPortCollection();
			var element = collection.AddNew();
			element.PortCode = "3901";
			element.CRProcess = "1-Step";
			element.Location = "S";
			Factory.Save();
			return collection;
		}
	}
}
