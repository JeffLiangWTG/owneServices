using System;
using System.Reflection;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Bookings;
using Enterprise.Tracking.Web.Bookings.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	class EditBookingForTest : EditBooking
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public void SetupPageForTesting()
		{
			UnauthorisedLabel = new ZTextLabel();
			UnauthorisedDiv = new HtmlGenericControl();
			AuthorisedContent = new HtmlGenericControl();
			ThirdPartyAddress = new ZOrgAddressControl();
			PanelThirdParyAddress = new ZDiv();
			ContainerMode = new ZDropDownList();
			notFCL = new HtmlGenericControl();
			FCL = new HtmlGenericControl();
			ContainersDataGrid = new ZDataGrid();
			Origin = new HtmlGenericControl();
			Destination = new HtmlGenericControl();
			WarehouseRecRow = new HtmlTableRow();
			CustomsEntryRow = new HtmlTableRow();
			InsuranceValueRow = new HtmlTableRow();
			PackLines = new HtmlGenericControl();
			PackLinesGrid = new ZDataGrid();
			OriginPort = new ZFindBox();
			DestinationPort = new ZFindBox();
			ScheduleChooser = new WebScheduleChooserControlForTest();
			VolumeCalculatorGridAddOn = new VolumeCalculatorDataGridAddOn
				(JobPackLinesSchema.Constants.JL_PackageCount,
				JobPackLinesSchema.Constants.JL_Length,
				JobPackLinesSchema.Constants.JL_Width,
				JobPackLinesSchema.Constants.JL_Height,
				JobPackLinesSchema.Constants.JL_UnitOfDimension,
				JobPackLinesSchema.Constants.JL_ActualVolume,
				JobPackLinesSchema.Constants.JL_ActualVolumeUQ);
			AdditionalTermsRow = new HtmlTableRow();
			PayTermLabel = new Label();
			NoBookingDiv = new ZDiv();
			NoBookingLabel = new ZTextLabel();
			AttachedOrdersGrid = new ZDataGrid();
			ReferenceGrid = new ZDataGrid();

			LoadOrCreateDataSource();
		}

		public string DefaultModeKeyForTest => DefaultModeKey;

		public void SetDefaultModeForTest() => SetDefaultMode(BookingForTest);

		public void SaveDefaultModeForTest() => SaveDefaultMode();

		public void OnLoadForTest()
		{
			OnLoad(EventArgs.Empty);
			ScheduleChooserForTest.CallOnLoad();
		}

		public void SetupGridsForTest() => SetupGrids();

		public ZDataGrid PackLinesGridForTest => PackLinesGrid;

		public WebScheduleChooserControlForTest ScheduleChooserForTest => (WebScheduleChooserControlForTest)ScheduleChooser;

		public ZOrgAddressControl ForTestThirdPartyAddress => ThirdPartyAddress;

		public ZDiv ForTestPanelThirdParyAddress => PanelThirdParyAddress;

		public HtmlGenericControl FCLForTest
		{
			get { return FCL; }
			set { FCL = value; }
		}

		public HtmlGenericControl notFCLForTest => notFCL;

		public ZDataGrid ContainersDataGridForTest => ContainersDataGrid;

		public HtmlTableRow AdditionalTermsRowForTest => AdditionalTermsRow;

		public Label PayTermLabelForTest => PayTermLabel;

		public HtmlGenericControl NoBookingDivForTest => NoBookingDiv;

		public ZTextLabel NoBookingLabelForTest => NoBookingLabel;

		public HtmlGenericControl AuthorisedContentForTest => AuthorisedContent;

		public void SetupContainersGridForTest()
		{
			ContainersDataGrid.Columns.Clear();
			SetupContainersGrid();
		}

		public void RedirectToTermsAndConditionsForTest() => RedirectToTermsAndConditionsIfNecessary();

		public TrackingBooking BookingForTest => DataSource as TrackingBooking;

		public object GetProtectedField(string fieldName) => GetType().InvokeMember(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField, null, this, null);

		public void SaveDefaultSettingsForDocAddressForTest() => SaveDefaultSettingsForDocAddress();

		public LinkPackLinesConfirmations LinkPackLinesConfirmationsForTest => (LinkPackLinesConfirmations)GetConfirmationByType(typeof(LinkPackLinesConfirmations));

		public ZDataGrid AttachedOrdersGridForTest => AttachedOrdersGrid;
	}
}
