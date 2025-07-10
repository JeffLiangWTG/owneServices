using Enterprise.Customs.GUI;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class STATACREQDOCSendingForm : MessageSendingObjectForm
	{
		public STATACREQDOCSendingForm()
		{
		}

		public STATACREQDOCSendingForm(STATACREQDOCSendingObjectParent messageParent)
			: base(messageParent)
		{
		}

		public override string FormHeading
		{
			get { return Res.GetString("4ee95b59-c35c-43a2-b2ca-7b6fcdf29213", "Customs Statement Request (REQDOC)"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeNewColumns();
		}

		void InitializeNewColumns()
		{
			ZTextBoxColumnStyleInfo organizationTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			organizationTextBoxColumnStyleInfo.ColumnName = STATACREQDOCSendingObject.Schema.OrganizationCode;
			organizationTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			organizationTextBoxColumnStyleInfo.IsMandatory = true;
			this.MessageSendingObjectsGrid.ColumnStyles.Add(organizationTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo customsOfficeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			customsOfficeTextBoxColumnStyleInfo.ColumnName = STATACREQDOCSendingObject.Schema.CustomsOfficeCode;
			customsOfficeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			customsOfficeTextBoxColumnStyleInfo.IsMandatory = true;
			this.MessageSendingObjectsGrid.ColumnStyles.Add(customsOfficeTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo fANTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			fANTextBoxColumnStyleInfo.ColumnName = STATACREQDOCSendingObject.Schema.FinancialAccountNumber;
			fANTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			fANTextBoxColumnStyleInfo.IsMandatory = true;
			this.MessageSendingObjectsGrid.ColumnStyles.Add(fANTextBoxColumnStyleInfo);

			ZDateEditColumnStyleInfo startDateDateEditColumnStyleInfo = new ZDateEditColumnStyleInfo();
			startDateDateEditColumnStyleInfo.DateTimeFormat = ZDateTimePickerFormat.Short;
			startDateDateEditColumnStyleInfo.ColumnName = STATACREQDOCSendingObject.Schema.StartDate;
			startDateDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			startDateDateEditColumnStyleInfo.IsMandatory = true;
			this.MessageSendingObjectsGrid.ColumnStyles.Add(startDateDateEditColumnStyleInfo);

			ZDateEditColumnStyleInfo endDateDateEditColumnStyleInfo = new ZDateEditColumnStyleInfo();
			endDateDateEditColumnStyleInfo.DateTimeFormat = ZDateTimePickerFormat.Short;
			endDateDateEditColumnStyleInfo.ColumnName = STATACREQDOCSendingObject.Schema.EndDate;
			endDateDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			endDateDateEditColumnStyleInfo.IsMandatory = true;
			this.MessageSendingObjectsGrid.ColumnStyles.Add(endDateDateEditColumnStyleInfo);
		}

		protected override string NothingSelectedMessage
		{
			get { return Res.GetString("57c248ca-1c75-456d-8a95-1475d87fd36e", "No FAN numbers selected for Customs Statement Request (REQDOC)"); }
		}
	}
}

