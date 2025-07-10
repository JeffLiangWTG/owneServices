using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class EditContainerForTest : EditContainer
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public EditContainerForTest()
		{
			IsCreateNewAppInstanceIfNullForTest = true;
			SiteUser.LoginSupportForTest("EDICUS");

			UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl();
			SaveContainer = new Button();
			NotFoundError = new System.Web.UI.HtmlControls.HtmlGenericControl();
			this.ContainerContents = new System.Web.UI.HtmlControls.HtmlGenericControl();
			base.OrdersPanel = new ZCollapsablePanel();
			base.OrdersGrid = new ZDataGrid();

			base.Sequence = new ZNumericTextBox();
			base.RequiredDelivery = new ZDateEdit();
			base.EmptyReady = new ZDateEdit();
			base.ConfirmedDelivery = new ZDateEdit();
			base.EmptyPickup = new ZDateEdit();
			base.ActualDelivery = new ZDateEdit();
			base.ActualDehire = new ZDateEdit();
		}

		public new void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		public void SetupAuthorisedContentForTest()
		{
			SetupAuthorisedContent(true);
		}

		public void SetupOrdersGridForTest()
		{
			SetupOrdersGrid(OrdersGrid);
		}

		public new ZDataGrid OrdersGrid
		{
			get { return base.OrdersGrid; }
		}

		public new ZNumericTextBox Sequence
		{
			get { return base.Sequence; }
		}

		public new ZDateEdit RequiredDelivery
		{
			get { return base.RequiredDelivery; }
		}

		public new ZDateEdit EmptyReady
		{
			get { return base.EmptyReady; }
		}

		public new ZDateEdit ConfirmedDelivery
		{
			get { return base.ConfirmedDelivery; }
		}

		public new ZDateEdit EmptyPickup
		{
			get { return base.EmptyPickup; }
		}

		public new ZDateEdit ActualDelivery
		{
			get { return base.ActualDelivery; }
		}

		public new ZDateEdit ActualDehire
		{
			get { return base.ActualDehire; }
		}

		public new ZCollapsablePanel OrdersPanel => base.OrdersPanel;

		protected override BusinessObject GetNewDataSource()
		{
			return ContainerForTest;
		}

		public TrackingContainer ContainerForTest
		{
			get
			{
				if (containerForTest == null)
				{
					containerForTest = Factory.NewWithValidTestData<TrackingContainer>();
					Factory.Save();
				}
				return containerForTest;
			}
		}
		TrackingContainer containerForTest;
	}
}
