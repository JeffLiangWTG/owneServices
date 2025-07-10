using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class InventoryDetailsForTest : InventoryDetails
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public InventoryDetailsForTest(string orgCode)
		{
			IsCreateNewAppInstanceIfNullForTest = true;
			SiteUser.LoginSupportForTest(orgCode);

			UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			SaveInventory = new Button();
			AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl();
			ReceiptRefLink = new ZHyperlink();
			DocketRef = new ZTextLabel();
			AdditionalDetailPanel = new ZCollapsablePanel();
			CustomAttr1 = new ZTextLabel();
			CustomAttr2 = new ZTextLabel();
			CustomAttr3 = new ZTextLabel();
			SerialNumber = new ZTextLabel();
			SerialNumberLabel = new Label();
			ExpiryDate = new ZDateTimeLabel();
			PackingDate = new ZDateTimeLabel();
			CustomAttr12Row = new System.Web.UI.HtmlControls.HtmlTableRow();
			CustomAttr3Row = new System.Web.UI.HtmlControls.HtmlTableRow();

			CrossDockedOrderLinesGrid = new ZGrid { BindTo = "ReservedPickLines", AllowEdit = true, AllowDelete = true };
			Controls.Add(CrossDockedOrderLinesGrid);
			SetupCrossDockedOrderLinesGrid();
		}

		public new void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		public Control ReceiptRefLinkForTest => ReceiptRefLink;

		public Control DocketRefForTest => DocketRef;

		public void OnPreBindForTest() => OnPreBind();

		public bool IsSerialNumberEnabledForTest => SerialNumber.Visible && SerialNumberLabel.Visible && CustomAttr3Row.Visible;

		public string SerialNumberCaptionForTest => SerialNumberLabel.Text;

		public ZDataGrid CrossDockedOrderLinesGridForTest => CrossDockedOrderLinesGrid;

		protected override BusinessObject GetNewDataSource()
		{
			return InventoryForTest;
		}

		public TrackingWhsInventory InventoryForTest => fInventoryForTest ?? (fInventoryForTest = Factory.NewWithValidTestData<TrackingWhsInventory>());
		TrackingWhsInventory fInventoryForTest;
	}
}
