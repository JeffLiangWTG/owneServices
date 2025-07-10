using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.LinerAndAgency.Testing
{
	sealed class TestBookingDetails : BookingDetails
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public TrackingLinerAndAgencyBooking TestBooking
		{
			get { return testBooking; }
			set
			{
				testBooking = value;
				LoadOrCreateDataSource();
			}
		}
		TrackingLinerAndAgencyBooking testBooking;

		public void OnDuplicateBookingClickForTest() => DuplicateBooking_Click(null, EventArgs.Empty);

		public void OnReverseBookingClickForTest() => ReverseBooking_Click(null, EventArgs.Empty);

		protected override BusinessObject GetNewDataSource()
		{
			return TestBooking ?? base.GetNewDataSource();
		}

		public ZDataGrid PacksGridGridForTest
		{
			get { return PacksGrid; }
		}

		public ZDataGrid ContainerGridForTest
		{
			get { return ContainersGrid; }
		}

		public ZDataGrid BookedContainersGridForTest
		{
			get { return BookedContainersGrid; }
		}

		public Button DuplicateBookingForTest
		{
			get { return DuplicateBooking; }
		}

		public Button ReverseBookingForTest
		{
			get { return ReverseBooking; }
		}

		public Button EditBookingForTest
		{
			get { return EditBooking; }
		}

		public Button CancelBookingForTest
		{
			get { return CancelBooking; }
		}

		public LinerAndAgencyBaseWebInterfacesHelper WebInterfacesHelperForTest
		{
			get { return WebInterfacesHelper; }
		}

		public eDocAttachPopup eDocsAddNewLinkForTest
		{
			get { return eDocsAddNewLink; }
		}

		public void ForTest_RunOnLoad()
		{
			OnLoad(new EventArgs());
			this.Page_Load(null, new EventArgs());
		}

		public void ForTest_RunOnPreBind()
		{
			this.OnPreBind();
		}

		public void ForTest_SetupDocumentsGrid()
		{
			this.SetupDocumentsGrid(DocumentsGrid);
		}

		public void SetupPageForTest()
		{
			UnauthorisedDiv = new HtmlGenericControl();
			UnauthorisedDiv = new HtmlGenericControl();
			AuthorisedContent = new HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			NotFoundError = new HtmlGenericControl();
			PacksGrid = new ZGrid();
			BookedContainersGrid = new ZGrid();
			ContainersGrid = new ZGrid();
			DuplicateBooking = new Button();
			ReverseBooking = new Button();
			CancelBooking = new Button();
			EditBooking = new Button();
			ConvertBooking = new Button();
			VolumeCalculatorGridAddOn = new VolumeCalculatorDataGridAddOn
				(JobPackLinesSchema.Constants.JL_PackageCount,
				JobPackLinesSchema.Constants.JL_Length,
				JobPackLinesSchema.Constants.JL_Width,
				JobPackLinesSchema.Constants.JL_Height,
				JobPackLinesSchema.Constants.JL_UnitOfDimension,
				JobPackLinesSchema.Constants.JL_ActualVolume,
				JobPackLinesSchema.Constants.JL_ActualVolumeUQ);
			DataContent = new HtmlGenericControl();
			NotesPanel = new HtmlGenericControl();
			DocumentsGrid = new ZGrid();
			EditButtonDiv = new HtmlGenericControl();
			CancelledBookingDiv = new HtmlGenericControl();
			ShipperRef = new ZTextLabel();
			ShipperRefLabel = new Label();
			CargoTypeRow = new HtmlTableRow();
			PaymentTermRow = new HtmlTableRow();
			GoodsDescriptionRow = new HtmlTableRow();
		}
	}
}
