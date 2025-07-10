using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Declaration;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TestShipmentDetails : ShipmentDetails
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public TrackingShipment TestShipment
		{
			get { return testShipment; }
			set
			{
				testShipment = value;
				LoadOrCreateDataSource();
			}
		}
		TrackingShipment testShipment;

		public bool SetupStatusControlForTesting()
		{
			return SetUpStatusControl(null, null);
		}

		protected override BaseStatusControl LoadStatusControl(string statusControlPath)
		{
			if (LoadStatusControlSubstitute != null)
			{
				return LoadStatusControlSubstitute();
			}

			return base.LoadStatusControl(statusControlPath);
		}

		public Func<BaseStatusControl> LoadStatusControlSubstitute { get; set; }

		protected override BusinessObject GetNewDataSource()
		{
			return TestShipment ?? base.GetNewDataSource();
		}

		public void SetupTransportGridForTest()
		{
			TransportGrid = new ZGrid();
			TransportGrid.ColumnProvider = new TrackingTransportDetailsGridColumnProvider();
		}

		public void SetupOrdersGridForTest()
		{
			OrdersGrid = new ZGrid();
			OrdersGrid.ColumnProvider = new TrackingOrderColumnProvider(false);
		}

		public ZGrid LocalChargesGridForTest
		{
			get { return LocalChargesGrid; }
		}

		public ZGrid ChargesGridForTest
		{
			get { return ChargesGrid; }
		}

		public ZGrid TransportGridForTest
		{
			get { return TransportGrid; }
		}

		public ZGrid OrdersGridForTest
		{
			get { return OrdersGrid; }
		}

		public ZDateTimeLabel ETDForTest => ETD;

		public ZDateTimeLabel ETAForTest => ETA;

		public ZGrid ContainerGridForTest
		{
			get { return ContainerGrid; }
		}

		public ZGrid ReferenceDataGridForTest
		{
			get { return ReferenceDataGrid; }
		}

		public ZGrid CustomsEntriesDataGridForTest
		{
			get { return CustomsEntriesDataGrid; }
		}

		public ZGrid PackLinesGridForTest
		{
			get { return PackLinesGrid; }
		}

		public ZGrid RelatedShipmentsForTest
		{
			get { return RelatedShipments; }
		}

		public ZDataGrid DeliveryGridForTest
		{
			get { return DeliveryGrid; }
		}

		public Control DeliveryPanelForTest
		{
			get { return DeliveryGrid; }
		}

		public Button SaveConfirmationsForTest
		{
			get { return SaveConfirmations; }
		}

		public HtmlTableRow PayTermsRowForTest
		{
			get { return PayTermsRow; }
		}

		public HtmlTableRow AdditionalTermsRowForTest
		{
			get { return AdditionalTermsRow; }
		}

		public Label PayTermLabelForTest
		{
			get { return PayTermLabel; }
		}

		public HtmlTableRow LoadingMetersRowForTest
		{
			get { return LoadingMetersRow; }
		}

		public HtmlGenericControl ForTest_MasterShipmentArea
		{
			get { return MasterShipmentArea; }
		}

		public Control ForTest_RelatedShipmentsArea
		{
			get { return RelatedShipments; }
		}

		public Button DuplicateShipmentForTest
		{
			get { return DuplicateShipment; }
		}

		public Button ReverseShipmentForTest
		{
			get { return ReverseShipment; }
		}

		public void ForTest_RunOnLoad()
		{
			this.OnLoad(new EventArgs());
		}

		public void ForTest_RunInitializeCulture()
		{
			this.InitializeCulture();
		}

		protected override Uri RequestUrl => new Uri("http://www.test.com/WebTracker/");

		public void ForTest_RunOnPreBind()
		{
			this.OnPreBind();
		}

		public HtmlGenericControl AuthorisedContentForTest => AuthorisedContent;

		public void SetupPageForTest()
		{
			MasterShipmentArea = new HtmlGenericControl();
			RelatedShipments = new ZGrid();
			UnauthorisedDiv = new HtmlGenericControl();
			OriginRow = new HtmlTableRow();
			DestinationRow = new HtmlTableRow();
			CustomsEntriesDataGrid = new ZGrid();
			ChargesGrid = new ZGrid();
			LocalChargesGrid = new ZGrid();
			CustomsEntriesDataGrid = new ZGrid();
			UnauthorisedDiv = new HtmlGenericControl();
			AuthorisedContent = new HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			NotFoundError = new HtmlGenericControl();
			ETARow = new HtmlTableRow();
			ETDRow = new HtmlTableRow();
			ETD = new ZDateTimeLabel();
			ETA = new ZDateTimeLabel();
			StorageCommencesRow = new HtmlTableRow();
			StorageCommencesParallelRow = new HtmlTableRow();
			CartageAdvisedRow = new HtmlTableRow();
			PickupCartageAdvisedRow = new HtmlTableRow();
			TransportGrid = new ZGrid();
			PackLinesGrid = new ZGrid();
			RelatedShipments = new ZGrid();
			OrdersGrid = new ZGrid();
			ContainerGrid = new ZGrid();
			DocumentsGrid = new ZGrid();
			MasterLink = new ZHyperlink();
			AdditionalTermsRow = new HtmlTableRow();
			PayTermsRow = new HtmlTableRow();
			LoadingMetersRow = new HtmlTableRow();
			PayTermLabel = new Label();
			ForAuthentifiedUserOnly = new HtmlGenericControl();
			ForAuthentifiedUserOnly2 = new HtmlGenericControl();
			ForAuthentifiedUserOnly3 = new HtmlGenericControl();
			ForAuthentifiedUserOnly4 = new HtmlGenericControl();
			DuplicateShipment = new Button();
			ReverseShipment = new Button();
			SetupDeliveryGridForTest();
			SetupReferenceDataGridForTest();
		}

		public void SetupDeliveryGridForTest()
		{
			DeliveryGrid = new ZGrid();
			SaveConfirmations = new Button();
			SetupDeliveryGrid();
		}

		public void SetupReferenceDataGridForTest()
		{
			ReferenceDataGrid = new ZGrid();
			SetupReferenceDataGrid();
		}

		public void OnDuplicateShipmentClickForTest() => DuplicateShipment_Click(null, EventArgs.Empty);

		public void OnReverseShipmentClickForTest() => ReverseShipment_Click(null, EventArgs.Empty);

		public void SetupAuthorisedContentForTesting(bool isAuthorized)
		{
			SetupAuthorisedContent(isAuthorized);
		}
	}
}
