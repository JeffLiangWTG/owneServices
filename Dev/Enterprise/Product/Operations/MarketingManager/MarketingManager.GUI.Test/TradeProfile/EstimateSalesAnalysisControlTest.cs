using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class EstimateSalesAnalysisControlTest : TestCaseWithFactory
	{
		#region Filter Control

		public void TestChangingFilterUpdatesCollection()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, salesProduct);

			using (var form = new ZForm(salesHeader))
			using (var control = new EstimateSalesAnalysisControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertType(typeof(EstimateSalesAnalysisFilter), control.FilterControl.BindingSource.DataSource);
				var filter = (EstimateSalesAnalysisFilter)control.FilterControl.BindingSource.DataSource;

				filter.Status = EstimateSalesAnalysisStatusFilterList.Codes.Traded;
				AssertEquals(OrgSalesActualsStatusList.Codes.Traded, salesHeader.FilterableEntitySalesCollection.StatusFilter);

				filter.Status = EstimateSalesAnalysisStatusFilterList.Codes.All;
				AssertEquals(ZString.Empty, salesHeader.FilterableEntitySalesCollection.StatusFilter);

				filter.ShouldMatchOnBuyerSupplier = false;
				AssertEquals(false, salesHeader.FilterableEntitySalesCollection.ShouldMatchOnBuyerSupplier);

				filter.ShouldMatchOnBuyerSupplier = true;
				AssertEquals(true, salesHeader.FilterableEntitySalesCollection.ShouldMatchOnBuyerSupplier);
			}
		}

		#endregion
	}
}
