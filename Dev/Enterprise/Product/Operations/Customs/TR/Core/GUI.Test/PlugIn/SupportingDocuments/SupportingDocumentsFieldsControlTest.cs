using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class SupportingDocumentsFieldsControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(SupportingDocumentsFieldsControl), typeof(Business.Declaration.JobDeclaration));
		}

		public void TestGroupBox()
		{
			SupportingDocumentsControlTestHelper.AssertGroupBox(typeof(SupportingDocumentsFieldsControl));
		}

		public void TestFields()
		{
			SupportingDocumentsControlTestHelper.AssertFields(typeof(SupportingDocumentsFieldsControl), FieldsDetails);
		}

		public void TestFieldsProperties()
		{
			using (var control = new SupportingDocumentsFieldsControl())
			{
				var groupBox = SupportingDocumentsControlTestHelper.GetFieldsGroupBox(control);
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_CodeCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("CSI_ReferenceNumberTextBox: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("CSI_ReferenceNumberTextBox").CharacterCasing);
					AssertEquals("CSI_Status: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZDropEdit>("CSI_StatusDropEdit").CharacterCasing);
				});
			}
		}

		IEnumerable<(string ControlName, int TabIndex, Type ControlType)> FieldsDetails => new (string, int, Type)[]
		{
			("CSI_CodeCodeFindBox", 0, typeof(ZCodeFindBox)),
			("CSI_ReferenceNumberTextBox", 1, typeof(ZTextBox)),
			("CSI_StatusDropEdit", 2, typeof(ZDropEdit)),
			("CSI_DateOfIssueDateEdit", 3, typeof(ZDateEdit)),
			("CSI_DateOfExpiryDateEdit", 4, typeof(ZDateEdit))
		};
	}
}
