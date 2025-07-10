using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class CFSShipmentDetailsForTest : CFSShipmentDetails
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public TrackingCFSShipment TestShipment
		{
			get { return testShipment; }
			set
			{
				testShipment = value;
				LoadOrCreateDataSource();
			}
		}
		TrackingCFSShipment testShipment;

		protected override BusinessObject GetNewDataSource()
		{
			return TestShipment ?? base.GetNewDataSource();
		}

		public void SetupTransportGridForTest()
		{
			TransportGrid = new ZGrid();
			TransportGrid.ColumnProvider = new TrackingTransportDetailsGridColumnProvider();
		}

		public ZGrid TransportGridForTest
		{
			get { return TransportGrid; }
		}

		public void SetupDeliveryInformationGridForTest()
		{
			DeliveryInformationGrid = new ZGrid();
			DeliveryInformationGrid.ColumnProvider = new CommonPickupDeliveryConfirmDetailsGridColumnProvider();
		}

		public ZGrid DeliveryInformationGridForTest
		{
			get { return DeliveryInformationGrid; }
		}

		public ZGrid PackLinesGridForTest
		{
			get { return PackLinesGrid; }
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

		public Label ClientRefLabelForTest
		{
			get { return ClientRefLabel; }
		}

		public Label ClientReftextlabelForTest
		{
			get { return ClientReftextlabel; }
		}

		public Label InterimReceiptLabelForTest
		{
			get { return InterimReceiptLabel; }
		}

		public Label InterimReceipttextlabelForTest
		{
			get { return InterimReceipttextlabel; }
		}

		public Label EntryNoLabelForTest
		{
			get { return EntryNotextlabel; }
		}

		public Label EntryNotextlabelForTest
		{
			get { return EntryNotextlabel; }
		}

		public Label WhsLocationLabelForTest
		{
			get { return WhsLocationLabel; }
		}

		public Label WhsLocationZcodefindboxlabelForTest
		{
			get { return WhsLocationZcodefindboxlabel; }
		}

		public Label MasterBillLabelForTest
		{
			get { return MasterBillLabel; }
		}

		public Label MasterBilltextLabelForTest
		{
			get { return MasterBilltextLabel; }
		}

		public void ForTest_RunOnLoad()
		{
			this.OnLoad(new EventArgs());
		}

		public void ForTest_RunOnPreBind()
		{
			this.OnPreBind();
		}

		public void SetupPageForTest()
		{
			UnauthorisedDiv = new HtmlGenericControl();
			OriginRow = new HtmlTableRow();
			DestinationRow = new HtmlTableRow();
			UnauthorisedDiv = new HtmlGenericControl();
			AuthorisedContent = new HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			NotFoundError = new HtmlGenericControl();
			ETARow = new HtmlTableRow();
			ETDRow = new HtmlTableRow();
			StorageCommencesRow = new HtmlTableRow();
			StorageCommencesParallelRow = new HtmlTableRow();
			CartageAdvisedRow = new HtmlTableRow();
			PickupCartageAdvisedRow = new HtmlTableRow();
			TransportGrid = new ZGrid();
			DeliveryInformationGrid = new ZGrid();
			PackLinesGrid = new ZGrid();
			DocumentsGrid = new ZGrid();
			AdditionalTermsRow = new HtmlTableRow();
			PayTermsRow = new HtmlTableRow();
			PayTermLabel = new Label();
			ForAuthentifiedUserOnly = new HtmlGenericControl();
			ForAuthentifiedUserOnly2 = new HtmlGenericControl();
			ForAuthentifiedUserOnly3 = new HtmlGenericControl();
			ForAuthentifiedUserOnly4 = new HtmlGenericControl();
			ClientRefLabel = new Label();
			ClientReftextlabel = new ZTextLabel();

			InterimReceiptLabel = new Label();
			InterimReceipttextlabel = new ZTextLabel();

			EntryNoLabel = new Label();
			EntryNotextlabel = new ZTextLabel();

			WhsLocationLabel = new Label();
			WhsLocationZcodefindboxlabel = new ZTextLabel();

			MasterBillLabel = new Label();
			MasterBilltextLabel = new ZTextLabel();
			ShipmentNotFoundLabel = new ZTextLabel();
		}
	}
}
