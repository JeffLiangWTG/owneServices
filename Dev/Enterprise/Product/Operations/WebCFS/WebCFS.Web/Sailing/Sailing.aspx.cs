using CargoWise.EntityFramework;
using Enterprise.WebCFS.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.WebCFS.Web
{
	public partial class Sailing : BasePage
	{
		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			WebFilterBusinessObjectFactory filterFactory = new WebFilterBusinessObjectFactory(Factory);
			return filterFactory.New(typeof(SailingFilterBusinessObject));
		}

		#endregion

		#region Search Control setup

		protected SearchControl SearchControl;

		void SetupSearchControl()
		{
			SearchControl = GetNewSearchControl();
			SearchControl.ModuleID = WebModuleIDs.CFSSailings;
			SearchControl.PageSize = 30;
			SearchControl.MaxRows = 150;
			SearchControl.NewButtonVisible = false;
			SearchControlHolder.Controls.AddAt(0, SearchControl);
		}

		protected virtual SearchControl GetNewSearchControl()
		{
			return Page.LoadControl(SearchControlResource.FileName) as SearchControl;
		}

		#endregion
	}
}
