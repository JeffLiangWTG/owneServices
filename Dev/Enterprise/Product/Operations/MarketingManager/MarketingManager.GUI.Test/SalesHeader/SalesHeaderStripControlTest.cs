using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class SalesHeaderStripControlTest : TestCaseWithFactory
	{
		#region Properties

		public void TestIsMandatory()
		{
			using (var control = new SalesHeaderStripControl())
			{
				control.IsMandatory = true;
				AssertEquals(false, control.DeleteButton.Visible);

				control.IsMandatory = false;
				AssertEquals(true, control.DeleteButton.Visible);
			}
		}

		#endregion

		#region Delete Button

		public void TestDeleteButton()
		{
			var product = Factory.New<OrgSalesProduct>();
			product.MP_Name = "Custom Product";
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, product);

			using (var form = new ZForm(salesHeader))
			using (var control = new SalesHeaderStripControl())
			{
				form.Controls.Add(control);
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.DeleteButton.PerformClick();

				AssertEquals("Caption", "Delete all Custom Product Estimate Values", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Text", "Are you sure you want to delete all estimate values for Custom Product?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, salesHeader.IsDeleted);
				AssertNotNull(control.Parent);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				control.DeleteButton.PerformClick();

				AssertEquals(true, salesHeader.IsDeleted);
				AssertNull(control.Parent);
			}
		}

		#endregion

		#region ReadOnly

		public void TestReadOnly()
		{
			using (var control = new SalesHeaderStripControl())
			{
				control.ReadOnly = true;
				AssertEquals(false, control.DeleteButton.Visible);

				control.ReadOnly = false;
				AssertEquals(true, control.DeleteButton.Visible);
			}
		}

		#endregion

		#region Focus

		public void TestFocus_OnNewRow()
		{
			var shipment = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, shipment);

			using (var form = new ZForm(salesHeader))
			using (var control = new SalesHeaderStripControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.Focus(true);
				var tradeLaneWithDetailsControl = (TradeLaneWithDetailsControl)control.Controls.Find("tradeLaneWithDetailsControl", true)[0];

				AssertEquals(true, tradeLaneWithDetailsControl.TradeLanesGrid.ContainsFocus);
				AssertEquals("Should be editing", true, ((IEditableControl)tradeLaneWithDetailsControl.TradeLanesGrid).IsEditing);
				AssertEquals("First column should be selected", 0, tradeLaneWithDetailsControl.TradeLanesGrid.CurrentCell.ColumnNumber);
			}
		}

		public void TestFocus_Sales()
		{
			var shipment = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesHeader = new SalesHeader(org, shipment);
			var sales1 = salesHeader.EntitySalesCollectionProductView.AddNew();
			var sales2 = salesHeader.EntitySalesCollectionProductView.AddNew();

			var opp = org.SalesOpportunities.AddNew();
			opp.AssociatedTradeLanesPivots.AddPivotFor(sales1);
			opp.AssociatedTradeLanesPivots.AddPivotFor(sales2);

			Factory.Save();

			using (var form = new ZForm(salesHeader))
			using (var control = new SalesHeaderStripControl())
			{
				form.Controls.Add(control);
				salesHeader.CompanyFilter = Env.CurrentCompany.PK;

				form.Show();
				var tradeLaneWithDetailsControl = (TradeLaneWithDetailsControl)control.Controls.Find("tradeLaneWithDetailsControl", true)[0];

				AssertEquals("Precondition", 2, tradeLaneWithDetailsControl.TradeLanesGrid.ListManager.Count);

				control.Focus(sales2);

				AssertEquals(true, tradeLaneWithDetailsControl.TradeLanesGrid.ContainsFocus);
				AssertEquals("Should have selected the focused sales", sales2.PK, ((OrgSales)tradeLaneWithDetailsControl.TradeLanesGrid.ListManager.GetCurrent()).PK);
			}
		}

		#endregion
	}
}
