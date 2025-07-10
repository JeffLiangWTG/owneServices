using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class EntryInstructionSupportingDocumentsFieldsControlTest : TestCaseWithFactory
{
	public void TestSupportingDocumentsGroupBox_Caption()
	{
		using (var supportingDocumentsFieldControl = new EntryInstructionSupportingDocumentsFieldsControl(Factory.New<JobDeclaration>()))
		{
			var groupBox = supportingDocumentsFieldControl.FindSingleOrDefault<ZGroupBox>();
			AssertEquals("caption set by resource string", "Supporting Documents", groupBox.CaptionResourceString.Caption);
		}
	}

	public void TestLayout()
	{
		using (var supportingDocumentsFieldControl = new EntryInstructionSupportingDocumentsFieldsControl(Factory.New<JobDeclaration>()))
		{
			AssertType<EntryInstructionSupportingDocumentsFieldsLayout>(supportingDocumentsFieldControl.GetType().GetMethod("GetLayout", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(supportingDocumentsFieldControl, null));
		}
	}
}
