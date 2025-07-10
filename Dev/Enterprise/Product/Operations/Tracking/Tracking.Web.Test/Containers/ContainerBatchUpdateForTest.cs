using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ContainerBatchUpdateForTest : ContainerBatchUpdate
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public ContainerBatchUpdateForTest()
		{
			IsCreateNewAppInstanceIfNullForTest = true;
			SiteUser.LoginSupportForTest("EDICUS");

			UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl();
			this.ContainerBatchLabel = new ZTextLabel();
			SaveChanges = new Button();
			CancelChanges = new Button();
			NotFoundError = new System.Web.UI.HtmlControls.HtmlGenericControl();
			this.BatchContents = new System.Web.UI.HtmlControls.HtmlGenericControl();

			SelectedContainersGrid = new ZDataGrid();
			SelectedContainersGrid.BindTo = "Containers";
			SelectedContainersGrid.AllowEdit = true;

			base.RequiredDelivery = new ZDateEdit();
			base.ConfirmedDelivery = new ZDateEdit();
			base.ActualDelivery = new ZDateEdit();
			base.EstimatedDehire = new ZDateEdit();
			base.Pickup = new ZDateEdit();
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

		public void SetupContainerGridForTest()
		{
			SetupContainerGrid();
		}

		public ZDataGrid SelectedContainersGridForTest
		{
			get { return SelectedContainersGrid; }
		}

		public new ZDateEdit RequiredDelivery
		{
			get { return base.RequiredDelivery; }
		}

		public new ZDateEdit ConfirmedDelivery
		{
			get { return base.ConfirmedDelivery; }
		}

		public new ZDateEdit ActualDelivery
		{
			get { return base.ActualDelivery; }
		}

		public new ZDateEdit EstimatedDehire
		{
			get { return base.EstimatedDehire; }
		}

		public new ZDateEdit Pickup
		{
			get { return base.Pickup; }
		}

		public new ZDateEdit ActualDehire
		{
			get { return base.ActualDehire; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return HolderForTest;
		}

		public ContainerBatchHolder HolderForTest
		{
			get
			{
				if (fHolderForTest == null)
				{
					TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
					Factory.Save();
					fHolderForTest = new ContainerBatchHolder(Factory, container.PK);
				}
				return fHolderForTest;
			}
		}
		ContainerBatchHolder fHolderForTest;
	}
}
