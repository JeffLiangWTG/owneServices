using System.Web.UI.HtmlControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business.ImporterSecurityFiling;
using Enterprise.Tracking.Web.ImporterSecurityFiling;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class EditImporterSecurityFilingDetailsForTest : EditImporterSecurityFiling
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public void SetupPageForTesting()
		{
			SellingPartyAddress = new ZDocAddressWebControl();
			BuyingPartyAddress = new ZDocAddressWebControl();
			StuffingLocation = new ZDocAddressWebControl();
			ConsolidatorAddress = new ZDocAddressWebControl();
			BookingPartyAddress = new ZDocAddressWebControl();

			ShipmentTypeDropDown = new ZDropDownList();
			TransportModeArea = new HtmlTableRow();
			ActionReasonCodeArea = new HtmlTableRow();
			ImporterFindBox = new ZGuidFindBox();
			ISF10Details = new HtmlGenericControl();
			ISF5Details = new HtmlGenericControl();
			BondISFChangesForCSMS09000148 = new HtmlGenericControl();
			BondDetails = new HtmlGenericControl();
			BondHolder = new HtmlTableRow();
			SuretyCode = new HtmlTableRow();
			SendISF = new System.Web.UI.WebControls.Button();
			ReferenceGrid = new ZDataGrid();
			LoadOrCreateDataSource();
		}
		#region ControlsForTest

		public HtmlTableRow ForTest_TransportModeArea
		{
			get { return TransportModeArea; }
		}

		public HtmlTableRow ForTest_ActionReasonCodeArea
		{
			get { return ActionReasonCodeArea; }
		}

		public HtmlGenericControl ForTest_BondISFChangesForCSMS09000148
		{
			get { return BondISFChangesForCSMS09000148; }
		}

		public HtmlTableRow ForTest_BondHolder
		{
			get { return BondHolder; }
		}

		public HtmlTableRow ForTest_SuretyCode
		{
			get { return SuretyCode; }
		}

		public System.Web.UI.WebControls.Button ForTest_SendISF
		{
			get { return SendISF; }
		}

		public ZGuidFindBox ForTest_ImporterFindBox
		{
			get { return ImporterFindBox; }
		}

		public HtmlGenericControl ForTest_ISF10Details
		{
			get { return ISF10Details; }
		}

		public HtmlGenericControl ForTest_ISF5Details
		{
			get { return ISF5Details; }
		}

		public HtmlGenericControl ForTest_BondDetails
		{
			get { return BondDetails; }
		}

		#endregion

		public ZDataGrid ReferenceTestGrid
		{
			get { return ReferenceGrid; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return Factory.New<TrackingCusISFHeader>();
		}

		public void SetSendButtonAvailabilityForTest()
		{
			SetSendButtonAvailability();
		}

		public void ForTest_SendISFMessage()
		{
			ShouldSendISF = true;
			SaveDataSourceFactoryNoValidation();
		}

		public void SetupPageForTest()
		{
			SetVisibleONEntryType();
			SetupReferenceDataGrid(ReferenceGrid);
		}
	}
}
