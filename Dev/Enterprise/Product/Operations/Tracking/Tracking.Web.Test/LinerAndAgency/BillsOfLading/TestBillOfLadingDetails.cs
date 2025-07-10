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
	sealed class TestBillOfLadingDetails : BillOfLadingDetails
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public TrackingBillOfLading TestBillOfLading
		{
			get { return testBillOfLading; }
			set
			{
				testBillOfLading = value;
				LoadOrCreateDataSource();
			}
		}
		TrackingBillOfLading testBillOfLading;

		public void OnDuplicateBillOfLadingClickForTest() => DuplicateBillOfLading_Click(null, EventArgs.Empty);

		public void OnReverseBillOfLadingClickForTest() => ReverseBillOfLading_Click(null, EventArgs.Empty);

		protected override BusinessObject GetNewDataSource()
		{
			return TestBillOfLading ?? base.GetNewDataSource();
		}

		public ZDataGrid PacksGridGridForTest
		{
			get { return PacksGrid; }
		}

		public ZDataGrid ContainerGridForTest
		{
			get { return ContainersGrid; }
		}

		public Button DuplicateBillOfLadingForTest
		{
			get { return DuplicateBillOfLading; }
		}

		public Button ReverseBillOfLadingForTest
		{
			get { return ReverseBillOfLading; }
		}

		public Button EditButtonForTest
		{
			get { return EditButton; }
		}

		public void ForTest_RunOnLoad()
		{
			this.Page_Load(null, new EventArgs());
		}

		public void ForTest_RunOnPreBind()
		{
			this.OnPreBind();
		}

		public void SetupPageForTest()
		{
			UnauthorisedDiv = new HtmlGenericControl();
			UnauthorisedDiv = new HtmlGenericControl();
			AuthorisedContent = new HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			NotFoundError = new HtmlGenericControl();
			PacksGrid = new ZGrid();
			ContainersGrid = new ZGrid();
			DuplicateBillOfLading = new Button();
			ReverseBillOfLading = new Button();
			EditButton = new Button();
			DocsMenu = new ZDocumentsMenu();

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
			GoodsDescriptionRow = new HtmlTableRow();
			Addresses = new HtmlTable();
			ShipperCargoRow = new HtmlTableRow();
			PaymentReleaseRow = new HtmlTableRow();
			BillsRow = new HtmlTableRow();
			ConsignorLabel = new Label();
		}
	}
}
