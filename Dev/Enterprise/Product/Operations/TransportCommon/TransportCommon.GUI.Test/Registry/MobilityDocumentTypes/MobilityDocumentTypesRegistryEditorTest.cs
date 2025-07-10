using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.TransportCommon.GUI.Registry;
using Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.TransportCommon.GUI.Testing
{
	[TestedType(typeof(MobilityDocumentTypesRegistryEditor))]
	class MobilityDocumentTypesRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new MobilityDocumentTypesRegistryEditor(new MobilityDocumentTypesDataType(), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((MobilityDocumentTypesControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(MobilityDocumentTypesControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new MobilityDocumentTypesRegistryItem("", null, new MobilityDocumentTypeCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			MobilityDocumentTypeCollection testDocTypes = new MobilityDocumentTypeCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			RefDocType docType1 = Factory.NewWithPrimaryKey<RefDocType>(new Guid("88eb2a5d-809d-4f20-b92a-520f287a5d52"));
			docType1.RT_DocType = "AAA";
			docType1.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			docType1.RT_Desc = "Document type AAA";
			RefDocType docType2 = Factory.NewWithPrimaryKey<RefDocType>(new Guid("e55e5b81-ec70-49bb-8c2f-bcf58e287f74"));
			docType2.RT_DocType = "BBB";
			docType2.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			docType2.RT_Desc = "Document type BBB";
			Factory.Save();
			testDocTypes.AddNew().RT_PK = docType1.PK;
			testDocTypes.AddNew().RT_PK = docType2.PK;
			return new object[] { testDocTypes };
		}
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}
	}
}
