using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business.ImporterSecurityFiling;
using Enterprise.Tracking.Web.ImporterSecurityFiling;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ImporterSecurityFilingDetailsForTest : ImporterSecurityFilingDetails
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public void SetupPageForTesting()
		{
			ShipToGovRegNoArea = new HtmlTableRow();
			SellingPartyGovRegNoArea = new HtmlTableRow();
			BuyingPartyGovRegNoArea = new HtmlTableRow();
			StuffingLocationGovRegNoArea = new HtmlTableRow();
			ConsolidatorGovRegNoArea = new HtmlTableRow();
			BookingPartyGovRegNoArea = new HtmlTableRow();
			ISF10Details = new HtmlGenericControl();
			RoutingDetails = new HtmlGenericControl();
			ISF5Details = new HtmlGenericControl();
			SellingPartyAddressHolder = new HtmlGenericControl();
			BuyingPartyAddressHolder = new HtmlGenericControl();
			StuffingLocationHolder = new HtmlGenericControl();
			ConsolidatorAddressHolder = new HtmlGenericControl();
			BookingPartyAddressHolder = new HtmlGenericControl();
			BondDetails = new HtmlTableRow();
			ActionReasonCodeArea = new HtmlTableRow();
			BondHolder = new HtmlTableRow();
			DeleteISF = new Button();

			DocumentsGrid = new ZGrid();
			ContainersDataGrid = new ZGrid();
			AddressesDataGrid = new ZGrid();
			AdditionalShipToAddressesDataGrid = new ZGrid();
			ReferenceDataGrid = new ZGrid();
			LinesDataGrid = new ZGrid();
			ImporterFullNameInfo = new HtmlTableRow();
			ConsigneeFullNameInfo = new HtmlTableRow();
			LowValueDetails = new HtmlGenericControl();
			LoadOrCreateDataSource();
		}
		#region ControlsForTest

		public HtmlTableRow ForTest_ActionReasonCodeArea
		{
			get { return ActionReasonCodeArea; }
		}

		public HtmlTableRow ForTest_BondHolder
		{
			get { return BondHolder; }
		}

		public HtmlTableRow ForTest_BondDetails
		{
			get { return BondDetails; }
		}

		public HtmlTableRow ForTest_ShipToGovRegNoArea
		{
			get { return ShipToGovRegNoArea; }
		}
		public HtmlTableRow ForTest_SellingPartyGovRegNoArea
		{
			get { return SellingPartyGovRegNoArea; }
		}

		public HtmlGenericControl ForTest_ISF10Details
		{
			get { return ISF10Details; }
		}

		public HtmlGenericControl ForTest_RoutingDetails
		{
			get { return RoutingDetails; }
		}

		public HtmlGenericControl ForTest_ISF5Details
		{
			get { return ISF5Details; }
		}

		public HtmlGenericControl ForTest_SellingPartyAddressHolder
		{
			get { return SellingPartyAddressHolder; }
		}
		public HtmlGenericControl ForTest_BuyingPartyAddressHolder
		{
			get { return BuyingPartyAddressHolder; }
		}
		public HtmlGenericControl ForTest_StuffingLocationHolder
		{
			get { return StuffingLocationHolder; }
		}
		public HtmlGenericControl ForTest_ConsolidatorAddressHolder
		{
			get { return ConsolidatorAddressHolder; }
		}
		public HtmlGenericControl ForTest_BookingPartyAddressHolder
		{
			get { return BookingPartyAddressHolder; }
		}

		public Button ForTest_DeleteISF
		{
			get { return DeleteISF; }
		}

		#endregion

		public void ForTest_OnLoad()
		{
			OnLoad(EventArgs.Empty);
		}

		public void ForTest_DeleteISF_Click()
		{
			DeleteISF_Click(this, EventArgs.Empty);
		}

		protected override BusinessObject GetNewDataSource()
		{
			return Factory.New<TrackingCusISFHeader>();
		}

		public void SetupPageForTest()
		{
			SetUpPage();
		}
	}
}
