using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public partial class CartageDetails : BasePageWithAuthorisation
	{
		protected override string GetPageName()
		{
			return WebTracker.Pages.CartageDetails;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.CartageDetailsPage;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewCartage; }
		}

		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			return TrackingCartage.FromPKFilteredBySiteUser(Factory, GetGuidFromParameter("Ref"), SiteUser);
		}

		TrackingCartage CurrentCartage
		{
			get { return DataSource as TrackingCartage; }
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
			SetupContainerised();
			SetupVisibleForAddressControls();
			MainJobPanel.Visible = CurrentCartage.HasParent;
			SetupParentPanel();
			LegsDataGrid.Visible = CurrentCartage.FilteredCartageLegs.Count > 0;
		}

		void SetupParentPanel()
		{
			MainJobPanel.Visible = CurrentCartage.HasParent;
			if (CurrentCartage.CartageParent != null)
			{
				AddressesLinkedToJobLinkLabel.Text = CurrentCartage.CartageParent.UniqueConsignmentID;

				var controller = ZControllerFactory.Create(CurrentCartage.CartageParent.ControllerID);
				var webModule = WebModuleIDs.GetWebModuleIDFromModuleID(controller.ModuleID);
				var parentKey = ((BusinessObject)CurrentCartage.CartageParent).PK.ToString();
				AddressesLinkedToJobLinkLabel.NavigateUrl = WebController.GetTargetUrl(webModule, parentKey);
			}
		}

		void SetupVisibleForAddressControls()
		{
			FirstDocAddressControl.Visible = CurrentCartage.FirstDocAddress != null && !CurrentCartage.FirstDocAddress.IsEmpty;
			SecondDocAddressControl.Visible = CurrentCartage.SecondDocAddress != null && !CurrentCartage.SecondDocAddress.IsEmpty;
			ThirdDocAddressControl.Visible = CurrentCartage.ThirdDocAddress != null && !CurrentCartage.ThirdDocAddress.IsEmpty;
			FourthDocAddressControl.Visible = CurrentCartage.FourthDocAddress != null && !CurrentCartage.FourthDocAddress.IsEmpty;
		}

		void SetupContainerised()
		{
			ContainersDataGrid.Visible = CurrentCartage.IsContainerised;
			LooseBookingGrid.Visible = false; //CurrentCartage.IsLoose;
			AreaGrossWeight.Visible = CurrentCartage.IsContainerised;
		}

		protected override void SetupGrids()
		{
			base.SetupGrids();
			SetupLooseBookingGrid(LooseBookingGrid);
			SetupContainersGrid(ContainersDataGrid);
			SetupDocumentsGrid(DocumentsGrid);
			SetupLegsGrid(LegsDataGrid);
		}

		void SetupLooseBookingGrid(ZGrid looseBookingGrid)
		{
			looseBookingGrid.ColumnProvider = new CartageLooseBookingColumnProvider();
		}

		void SetupLegsGrid(ZGrid legsDataGrid)
		{
			legsDataGrid.ColumnProvider = new CartageLegColumnProvider();
		}

		protected void SetupContainersGrid(ZGrid containersGrid)
		{
			containersGrid.ColumnProvider = new CartageContainerColumnProvider();
		}
	}
}
