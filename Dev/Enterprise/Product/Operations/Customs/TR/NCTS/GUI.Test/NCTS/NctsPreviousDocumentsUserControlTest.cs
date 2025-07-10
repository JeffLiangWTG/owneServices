using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	class NctsPreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestPreviousDocumentFields()
		{
			using (var control = new NctsPreviousDocumentsUserControl())
			{
				{
					var referenceTextBox = control.Controls.Find("PrevDocsReferenceTextBox", true).FirstOrDefault();
					AssertType<ZTextBox>(referenceTextBox);
					AssertEquals("PrevDocsReferenceTextBox", true, referenceTextBox.Visible);

					var declerationItemNoTextBox = control.Controls.Find("DeclerationItemNoTextBox", true).FirstOrDefault();
					AssertType<ZCalcEdit>(declerationItemNoTextBox);
					AssertEquals("DeclerationItemNoTextBox", true, declerationItemNoTextBox.Visible);

					var amountCalcDropEdit = control.Controls.Find("AmountCalcDropEdit", true).FirstOrDefault();
					AssertType<ZCalcDropEdit>(amountCalcDropEdit);
					AssertEquals("AmountCalcDropEdit", true, amountCalcDropEdit.Visible);

					var boxQuantityCalcEdit = control.Controls.Find("BoxQuantityCalcEdit", true).FirstOrDefault();
					AssertType<ZCalcEdit>(boxQuantityCalcEdit);
					AssertEquals("BoxQuantityCalcEdit", true, boxQuantityCalcEdit.Visible);

					var grossWeightrCalcDropEdit = control.Controls.Find("GrossWeightrCalcDropEdit", true).FirstOrDefault();
					AssertType<ZCalcDropEdit>(grossWeightrCalcDropEdit);
					AssertEquals("GrossWeightrCalcDropEdit", true, grossWeightrCalcDropEdit.Visible);

					var netWeightCalcDropEdit = control.Controls.Find("NetWeightCalcDropEdit", true).FirstOrDefault();
					AssertType<ZCalcDropEdit>(netWeightCalcDropEdit);
					AssertEquals("NetWeightCalcDropEdit", true, netWeightCalcDropEdit.Visible);

					var prevDocsTypeDropEdit = control.Controls.Find("PrevDocsTypeDropEdit", true).FirstOrDefault();
					AssertType<ZDropEdit>(prevDocsTypeDropEdit);
					AssertEquals("PrevDocsTypeDropEdit", true, prevDocsTypeDropEdit.Visible);

					var paymentTypeDropEdit = control.Controls.Find("PaymentTypeDropEdit", true).FirstOrDefault();
					AssertType<ZDropEdit>(paymentTypeDropEdit);
					AssertEquals("PaymentTypeDropEdit", true, paymentTypeDropEdit.Visible);

					var natureOfBussinessDropEdit = control.Controls.Find("NatureOfBussinessDropEdit", true).FirstOrDefault();
					AssertType<ZDropEdit>(natureOfBussinessDropEdit);
					AssertEquals("NatureOfBussinessDropEdit", true, natureOfBussinessDropEdit.Visible);

					var countryCodeFindBox = control.Controls.Find("CountryCodeFindBox", true).FirstOrDefault();
					AssertType<ZCodeFindBox>(countryCodeFindBox);
					AssertEquals("CountryCodeFindBox", true, countryCodeFindBox.Visible);

					var explanationTextBox = control.Controls.Find("ExplanationTextBox", true).FirstOrDefault();
					AssertType<ZTextBox>(explanationTextBox);
					AssertEquals("ExplanationTextBox", true, explanationTextBox.Visible);

					var prevTypeyCodeFindBox = control.Controls.Find("prevTypeyCodeFindBox", true).FirstOrDefault();
					AssertType<ZCodeFindBox>(prevTypeyCodeFindBox);
					AssertEquals("PrevTypeDropEdits", true, prevTypeyCodeFindBox.Visible);
				}
			}
		}

		public void TestAvaialbleColumnsForPreviousDocuments()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var detail = header.MovementHeader.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				using (var form = new ZForm(detail))
				using (var control = new NctsPreviousDocumentsUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var previousDocumentsGrid = (ZGrid)control.Controls.Find("PreviousDocumentsGrid", true).Single();
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.CSI_Code));
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.CSI_ReferenceNumber));
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.CSI_LineNo));
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.CSI_Value));
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.CSI_RX_NKCurrency));
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.CSI_Quantity));
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.CSI_Quantity2));
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.CSI_UnitOfQuantity2));
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.CSI_Quantity3));
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.CSI_UnitOfQuantity3));
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.Incoterm));
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.CSI_SubType));
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.CSI_Procedure));
					AssertNotNull(FindColumnByName(previousDocumentsGrid, NctsPreviousDocument.Schema.CSI_RN_NKCountryCode));
				}
			});
		}

		ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName) => grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
	}
}
