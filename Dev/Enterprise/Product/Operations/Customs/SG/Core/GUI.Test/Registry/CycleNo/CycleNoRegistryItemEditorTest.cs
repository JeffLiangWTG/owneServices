using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.Registry.GUI;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.SG.DataRegistry.GUI.Testing
{
	[TestedType(typeof(CycleNoRegistryItemEditor))]
	sealed class CycleNoRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new CycleNoRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((CycleNoUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(CycleNoUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
			=> new CycleNoRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System, new CycleNoCollection().GetDefaultCollection);

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CycleNoCollection();
			var cycleNo = collection.AddNew();
			cycleNo.CycleNum = 1;
			cycleNo.SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new TimeSpan(23, 40, 0));
			Factory.Save();
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
