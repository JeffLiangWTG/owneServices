using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class EntryInstructionSupportingDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestSupportingDocumentGroupBoxCaption()
	{
		using (var supportingDocumentsUserControl = new EntryInstructionSupportingDocumentsUserControl())
		{
			var declaration = Factory.New<JobDeclaration>();
			supportingDocumentsUserControl.SetDataBinding(declaration, null);
			AssertEquals(typeof(EntryInstructionSupportingDocumentsFieldsControl), supportingDocumentsUserControl.SupportingDocumentsFieldsControl.GetType());
		}
	}

	public void TestUCC6AndExportAvailableColumnNames()
	{
		var property = typeof(EntryInstructionSupportingDocumentsUserControl).GetProperty("UCC6AndExportAvailableColumnNames", BindingFlags.NonPublic | BindingFlags.Instance);
		using (var supportingDocumentsUserControl = new EntryInstructionSupportingDocumentsUserControl())
		{
			AssertArrayEqualsByElements(new string[5] { "CSI_Code", "CSI_ReferenceNumber", "CSI_ItemNumber", "CSI_AdditionalDescription", "CSI_DateOfExpiry" }, (string[])property.GetValue(supportingDocumentsUserControl));
		}
	}

	public void TestUCC6AndImportAvailableColumnNames()
	{
		var property = typeof(EntryInstructionSupportingDocumentsUserControl).GetProperty("UCC6AndImportAvailableColumnNames", BindingFlags.NonPublic | BindingFlags.Instance);
		using (var supportingDocumentsUserControl = new EntryInstructionSupportingDocumentsUserControl())
		{
			AssertArrayEqualsByElements(new string[5] { "CSI_Code", "CSI_ReferenceNumber", "CSI_ItemNumber", "CSI_AdditionalDescription", "CSI_DateOfExpiry" }, (string[])property.GetValue(supportingDocumentsUserControl));
		}
	}
}
