using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ComplianceDocumentImageCollectionRegistryItemEditor))]
	sealed class ComplianceDocumentImageCollectionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		[ExpectNoExceptions]
		public override void TestRegistryItemAcceptsEditorValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				base.TestRegistryItemAcceptsEditorValue();
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ComplianceDocumentImageCollectionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ComplianceDocumentImageCollectionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ComplianceDocumentImageCollectionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ComplianceDocumentImageCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new ComplianceDocumentImageCollection();
			var image = collection.AddNew();
			image.Country = Core.Constants.CountryCodes.Taiwan;
			image.ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			image.Remark = "Remark";
			image.Image = new Bitmap(1, 1);
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
