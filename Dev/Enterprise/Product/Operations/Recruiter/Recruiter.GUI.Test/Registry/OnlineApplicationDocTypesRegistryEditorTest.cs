using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(OnlineApplicationDocTypesRegistryEditor))]
	public class OnlineApplicationDocTypesRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new OnlineApplicationDocTypesRegistryItem("", null);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new OnlineApplicationDocTypesRegistryEditor(new OnlineApplicationDocTypesDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(OnlineApplicationDocTypesControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			OnlineApplicationDocTypeCollection oaDocTypes = new OnlineApplicationDocTypeCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			RefDocType docType1 = Factory.NewWithPrimaryKey<RefDocType>(new Guid("0A38F0A7-DCA0-4f2b-83DE-5701695EED9C"));
			docType1.RT_DocType = "AAA";
			docType1.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			docType1.RT_Desc = "Document type AAA";
			RefDocType docType2 = Factory.NewWithPrimaryKey<RefDocType>(new Guid("63C10990-43E0-4ed7-8BCA-E0A11632B805"));
			docType2.RT_DocType = "BBB";
			docType2.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			docType2.RT_Desc = "Document type BBB";
			Factory.Save();
			oaDocTypes.AddNew().RT_PK = docType1.PK;
			oaDocTypes.AddNew().RT_PK = docType2.PK;
			return new object[] { oaDocTypes };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.TopLeftRight;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((OnlineApplicationDocTypesControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
