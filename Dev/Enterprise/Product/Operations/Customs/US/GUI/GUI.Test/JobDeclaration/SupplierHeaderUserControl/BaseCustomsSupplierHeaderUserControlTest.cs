using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	sealed class BaseInvoiceHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestChangeGSTApplicableText()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var userControl = (USCustomsSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				userControl.ChargesTabControl.SelectedTab = userControl.ApportionedTabPage;
				userControl.ChargesTabControl.SelectedTab = userControl.InvoiceChargesTabPageInternal;
				AssertEquals("Charges grid GST Applicable column text", USCustomsSupplierHeaderUserControl.IsCIFComponent, userControl.InvoiceChargesGrid.Columns[JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name].ColumnStyle.HeaderText);
				AssertEquals("Charges grid GST Applicable column text", USCustomsSupplierHeaderUserControl.IsCIFComponent, userControl.ApportionedChargesGrid.Columns[JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name].ColumnStyle.HeaderText);
				AssertEquals("Charges grid GST Applicable column text", USCustomsSupplierHeaderUserControl.IsCIFComponent, userControl.BaseGroupChargesGrid.Columns[JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name].ColumnStyle.HeaderText);
				AssertEquals("JZ_Calc_TNIBoundInvoiceCurrencyControl visibility", true, userControl.JZ_Calc_TNIBoundInvoiceCurrencyControlInternal.Visible);
				AssertEquals("JZ_CIFAmountBoundCurrencyControl visibility", false, userControl.JZ_CIFAmountBoundCurrencyControlInternal.Visible);
				AssertEquals("LCExRate invisible", false, userControl.JZ_InvoiceCurrLandedCostExRateCalcEditInternal.Visible);
			}
		}

		public void TestEnteringDataForSecondChargeInGridDoesNotOverwritesFirstGridRow()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			var invoice = declaration.Invoices.AddNew();
			// 1 charge is added
			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = "OFT";
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.Customs.JobDeclaration);
				AssertNotNull(plugin);
				plugin.SelectTabPage();
				var userControl = (CustomsBrokerageUserControl)plugin.UserControl;
				userControl.MainTabControl.SelectedTab = userControl.InvoicesTabPage;
				var chargeGrid = userControl.SupplierHeaderUserControl.InvoiceChargesGrid;
				chargeGrid.Select();
				chargeGrid.Focus();
				chargeGrid.CurrentCell = new DataGridCell(1, 0);
				chargeGrid.BeginEdit(chargeGrid.Columns[0].ColumnStyle, 1);
				((ZTextBoxColumnStyle)chargeGrid.Columns[0].ColumnStyle).EditControl.Text = "ONS";
				chargeGrid.EndEdit(chargeGrid.Columns[0].ColumnStyle, 1, false);
				AssertEquals(2, invoice.Charges.Count);
				AssertEquals("OFT", invoice.Charges[0].J7_ChargeType);
				AssertEquals("ONS", invoice.Charges[1].J7_ChargeType);
			}
		}
	}
}
