using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public partial class ContainerSummary : BasePageWithAuthorisation
	{
		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser != null && SiteUser.CanViewTrackingContainers; }
		}

		protected override void OnPreBind()
		{
			base.OnPreBind();

			SetupSummaryGrid(SelectedContainersGrid);
		}

		protected void SetupSummaryGrid(ZDataGrid ordersGrid)
		{
			ordersGrid.AutoGenerateColumns = false;

			ordersGrid.Columns.Add(new ZTextEditColumn(Res.GetString("2f2d7936-0cb7-47c8-8a19-591b74cca91c", "Description"), ContainerSummaryRow.Schema.StatusDescription));
			ordersGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("b9ac443c-8945-4b04-a6f2-5026c23708a8", "Containers"), ContainerSummaryRow.Schema.Containers));
			ordersGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("3ab8c95f-caeb-4b66-a322-0e22b5691c56", "Shipments"), ContainerSummaryRow.Schema.Shipments));
			ordersGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("923105e0-70f0-405e-9b1d-455884f6a61d", "Orders"), ContainerSummaryRow.Schema.Orders));
			ordersGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("279cf624-9387-4aa6-b92e-d79c351da909", "Packages"), ContainerSummaryRow.Schema.Packages));
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ContainerSummaryPage;
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.ContainerSummary;
		}

		#region BusinessObject

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			ZString key = GetStringFromParameter("Ref");

			var summary = (ContainerBatchSummary)HttpContext.Current.Session[key] ?? new ContainerBatchSummary();

			return summary;
		}

		protected ContainerBatchSummary ContainerBatchSummary
		{
			get { return DataSource as ContainerBatchSummary; }
		}

		protected void RedirectToContainerModule()
		{
			Response.Redirect(string.Format("{0}", AppInstance.ContainersPage));
		}

		#endregion
	}
}
