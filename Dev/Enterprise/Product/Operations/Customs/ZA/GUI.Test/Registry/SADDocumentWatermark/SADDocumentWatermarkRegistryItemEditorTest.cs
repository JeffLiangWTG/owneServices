using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.GUI.Testing
{
	[TestedType(typeof(SADDocumentWatermarkRegistryItemEditor))]
	sealed class SADDocumentWatermarkRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public override void TestRegistryItemAcceptsEditorValue()
		{
			var testHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1", "Release");
			testHelper.CreateCustomsStatusCusCodeEntry("4", "Somebody stop me");
			Factory.Save();
			base.TestRegistryItemAcceptsEditorValue();
		}
		protected override RegistryItemEditor GetEditor()
		{
			return new SADDocumentWatermarkRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((SADDocumentWatermarkUserControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(SADDocumentWatermarkUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new SADDocumentWatermarkRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new SADDocumentWatermarkCollection();
			var docWatermark1 = collection.AddNew();
			var docWatermark2 = collection.AddNew();
			docWatermark1.EntryStatusCode = "1";
			docWatermark1.WatermarkText = ZString.Empty;
			docWatermark2.EntryStatusCode = "4";
			docWatermark2.WatermarkText = "Watermarked";
			Factory.Save();
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}
	}
}
