using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class EstimateSalesAnalysisTradeLaneGridColourSchemeManager : GridColourSchemeManager
	{
		public EstimateSalesAnalysisTradeLaneGridColourSchemeManager(ZGrid grid, EntitySalesWrapperCollectionFilterableView prospectSalesFilterableView)
			: base(grid)
		{
			Argument.NotNull(prospectSalesFilterableView, "prospectSalesFilterableView");

			this.prospectSalesFilterableView = prospectSalesFilterableView;
			this.prospectSalesFilterableView.SalesActualsInformationRefreshed += SalesActualsInformationRefreshed;

			grid.ColorContextKey += "Actuals";
		}

		#region FilterBusinessObject

		protected new OrgSalesFilterBusinessObjectWithActuals FilterBusinessObject
		{
			get { return (OrgSalesFilterBusinessObjectWithActuals)base.FilterBusinessObject; }
		}

		protected override FilterStripBusinessObject GetFilterBusinessObjectForGrid()
		{
			var filterBizObj = new OrgSalesFilterBusinessObjectWithActuals();
			filterBizObj.AllProspectSales = prospectSalesFilterableView.Cast<EntitySalesWrapper>();
			return filterBizObj;
		}

		#endregion

		#region Actuals Options

		void SalesActualsInformationRefreshed(object sender, EventArgs e)
		{
			ResetGridColours();
		}

		readonly EntitySalesWrapperCollectionFilterableView prospectSalesFilterableView;

		#endregion
	}
}
