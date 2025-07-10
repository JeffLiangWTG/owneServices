using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class SupportingDocumentsFieldsControlTest : EU.GUI.PlugIn.Testing.SupportingDocumentsFieldsControlTest
{
	protected override IEnumerable<(string ControlName, int TabIndex, Type ControlType)> FieldsDetails => new (string, int, Type)[]
	{
		("SupDocTypeCodeFindBox", 0, typeof(ZCodeFindBox)),
		("SupDocReferenceTextBox", 1, typeof(ZTextBox)),
		("SupDocReferenceCodeFindBox", 1, typeof(ZCodeFindBox)),
		("SupDocQuantityCalcEdit", 3, typeof(ZCalcEdit)),
		("CSI_UnitOfQuantityDropEdit", 4, typeof(ZDropEdit)),
		("CSI_ValueCalcEdit", 7, typeof(ZCalcEdit)),
		("CSI_RX_NKCurrencyCodeFindBox", 8, typeof(ZCodeFindBox)),
		("CSI_DateOfExpiryDateEdit", 10, typeof(ZDateEdit)),
		("CSI_ItemNumberCalcEdit", 16, typeof(ZCalcEdit))
	};

	public void TestControls_SupDocsGroupBox()
	{
		using (var control = new SupportingDocumentsFieldsControl())
		{
			var supDocGroupBox = control.FindSingle<ZGroupBox>("SupportingDocumentsGroupBox");
			CombineAssertions(() =>
			{
				AssertEquals("[UCC 2/3] Supporting documents", supDocGroupBox.CaptionResourceString.Caption);
			});
		}
	}

	protected override Type SupportingDocumentsFieldsControlType => typeof(SupportingDocumentsFieldsControl);
}
