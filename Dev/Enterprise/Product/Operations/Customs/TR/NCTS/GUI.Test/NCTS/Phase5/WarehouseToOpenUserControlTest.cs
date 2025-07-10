using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NctsHeader = Enterprise.Customs.TR.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	sealed class WarehouseToOpenUserControlTest : TestCaseWithFactory
	{
		public void TestFields()
		{
			using (var control = new WarehouseToOpenUserControl())
			{
				CombineAssertions(() =>
				{
					var referenceTextBox = control.Controls.Find("ReferenceTextBox", true).FirstOrDefault();
					AssertType<ZTextBox>(referenceTextBox);
					AssertEquals("ReferenceTextBox", true, referenceTextBox.Visible);

					var incoterm = control.Controls.Find("IncotermDropEdit", true).FirstOrDefault();
					AssertType<ZDropEdit>(incoterm);
					AssertEquals("IncotermDropEdit", true, incoterm.Visible);

					var paymentType = control.Controls.Find("PaymentTypeDropEdit", true).FirstOrDefault();
					AssertType<ZDropEdit>(paymentType);
					AssertEquals("PaymentTypeDropEdit", true, paymentType.Visible);

					var amountCalcDropEdit = control.Controls.Find("AmountCalcDropEdit", true).FirstOrDefault();
					AssertType<ZCalcDropEdit>(amountCalcDropEdit);
					AssertEquals("AmountCalcDropEdit", true, amountCalcDropEdit.Visible);

					var countryCodeFindBox = control.Controls.Find("CountryCodeFindBox", true).FirstOrDefault();
					AssertType<ZCodeFindBox>(countryCodeFindBox);
					AssertEquals("CountryCodeFindBox", true, countryCodeFindBox.Visible);

					var natureOfBusinessDropEdit =
						control.Controls.Find("NatureOfBusinessDropEdit", true).FirstOrDefault();
					AssertType<ZDropEdit>(natureOfBusinessDropEdit);
					AssertEquals("NatureOfBusinessDropEdit", true, natureOfBusinessDropEdit.Visible);

					var declarationItemNoTextBox =
						control.Controls.Find("DeclarationItemNoTextBox", true).FirstOrDefault();
					AssertType<ZCalcEdit>(declarationItemNoTextBox);
					AssertEquals("DeclarationItemNoTextBox", true, declarationItemNoTextBox.Visible);

					var nctsLineNoTextBox = control.Controls.Find("NctsLineNoTextBox", true).FirstOrDefault();
					AssertType<ZCalcEdit>(nctsLineNoTextBox);
					AssertEquals("NctsLineNoTextBox", true, nctsLineNoTextBox.Visible);

					var boxQuantityCalcEdit = control.Controls.Find("BoxQuantityCalcDropEdit", true).FirstOrDefault();
					AssertType<ZCalcDropEdit>(boxQuantityCalcEdit);
					AssertEquals("BoxQuantityCalcDropEdit", true, boxQuantityCalcEdit.Visible);

					var explanationTextBox = control.Controls.Find("ExplanationTextBox", true).FirstOrDefault();
					AssertType<ZTextBox>(explanationTextBox);
					AssertEquals("ExplanationTextBox", true, explanationTextBox.Visible);
				});
			}
		}
		public void TestAvailableColumnsForWarehouseToOpen()
		{
			nctsHeader.MovementHeader.WarehouseToOpenList.AddNew();

			CombineAssertions(() =>
			{
				using (var form = new ZForm(nctsHeader))
				using (var control = new WarehouseToOpenUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var warehouseToOpenGrid = (ZGrid)control.Controls.Find("WarehouseToOpenGrid", true).Single();
					AssertNotNull(FindColumnByName(warehouseToOpenGrid, NctsWarehouseToOpen.Schema.CSI_ReferenceNumber));
					AssertNotNull(FindColumnByName(warehouseToOpenGrid, NctsWarehouseToOpen.Schema.CSI_LineNo));
					AssertNotNull(FindColumnByName(warehouseToOpenGrid, NctsWarehouseToOpen.Schema.CSI_ItemNumber));
					AssertNotNull(FindColumnByName(warehouseToOpenGrid, NctsWarehouseToOpen.Schema.CSI_Value));
					AssertNotNull(FindColumnByName(warehouseToOpenGrid, NctsWarehouseToOpen.Schema.CSI_RX_NKCurrency));
					AssertNotNull(FindColumnByName(warehouseToOpenGrid, NctsWarehouseToOpen.Schema.CSI_Quantity));
					AssertNotNull(FindColumnByName(warehouseToOpenGrid, NctsWarehouseToOpen.Schema.CSI_UnitOfQuantity));
					AssertNotNull(FindColumnByName(warehouseToOpenGrid, NctsWarehouseToOpen.Schema.Incoterm));
					AssertNotNull(FindColumnByName(warehouseToOpenGrid, NctsWarehouseToOpen.Schema.CSI_SubType));
					AssertNotNull(FindColumnByName(warehouseToOpenGrid, NctsWarehouseToOpen.Schema.CSI_Procedure));
					AssertNotNull(FindColumnByName(warehouseToOpenGrid, NctsWarehouseToOpen.Schema.CSI_Description));
					AssertNotNull(FindColumnByName(warehouseToOpenGrid, NctsWarehouseToOpen.Schema.CSI_RN_NKCountryCode));
				}
			});
		}
		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}

		NctsHeader nctsHeader;
		ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName) => grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
	}
}
