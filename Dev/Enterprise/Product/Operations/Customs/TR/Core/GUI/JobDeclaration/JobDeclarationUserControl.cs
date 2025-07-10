using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class JobDeclarationUserControl : EUJobDeclarationUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public JobDeclarationUserControl()
		{
			InitializeComponent();
			HideUnnecessaryControls();
			SetCaptions();
		}

		void SetCaptions()
		{
			PortOfLoadingFindBox.CaptionResourceString = Res.GetData("A147BCE6-58C8-40DF-87DB-05AD48FAB94E", "Load Port");
			ExportDeclarationNumberBoundTextBox.CaptionResourceString = Res.GetData("A9962571-98B0-49A0-8FB7-9FC2D2772CB1", "Registration No");
			JE_ValuationDateDateEdit.CaptionResourceString = Res.GetData("EFCABCBF-BE89-4638-BE6E-A884C728FB29", "Valuation Date");
		}

		void HideUnnecessaryControls()
		{
			CTStatusIDDropEdit.Visible = false;
			PortOfFirstArrivalFindBox.Visible = false;
			JE_UCRTextBox.Visible = false;
			ZG_AgreedPlaceCodeDropEdit.Visible = false;
			JE_DateOfFirstArrivalBoundDateEdit.Visible = false;
		}

		protected override Type GetCustomsOfficesUserControlType() => typeof(CustomsOfficesUserControl);

		protected override Type GetOrganizationImportUserControlType() => typeof(ImportOrganizationUserControl);

		protected override Type GetOrganizationExportUserControlType() => typeof(ExportOrganizationUserControl);

		protected override void SetRightTabControlSelectTab()
		{
			RightTabControl.SelectedTab = OrganisationsTabPage;
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			var isImport = JobDeclaration.IsImport;
			var isExport = JobDeclaration.IsExport;

			JE_CustomsDischargePortDropEdit.Visible = isImport && JobDeclaration.TransportMode == Core.Constants.TransportModes.Sea;
			JE_CustomsLoadPortDropEdit.Visible = isExport && JobDeclaration.TransportMode == Core.Constants.TransportModes.Sea;

			TradeTypeDropEdit.Visible = isExport;

			var customsOffice = (ZCodeFindBox)CustomsOfficesUserControl.Controls.Find("EntryCustomsFindBox", true).FirstOrDefault();

			if (customsOffice != null)
			{
				customsOffice.CaptionResourceString = isImport
					? Res.GetData("B5BE61BB-8AE8-420B-B828-D2B9C68BB19A", "[29] Entry Customs")
					: Res.GetData("B1ABE144-B79E-43D1-A6E9-0D61EFF8AC49", "[29] Exit Customs");
				customsOffice.UpdateCaption();
			}
		}

		public override bool ShowIsHighValueOvrdCheckBox(bool isImport) => false;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (JobDeclaration != null)
			{
				JobDeclaration.JE_EntryStatusInfo.ValueChanged += JE_EntryStatusInfo_ValueChanged;
				JobDeclaration.JE_EntryStatusInfo.RefreshBinding();
			}

			this.FindSingle<Control>("GoodsAtCustomsAreaCheckBox").AllowOutsideOfParent();
			this.FindSingle<Control>("OverTimePaymentCompletedCheckBox").AllowOutsideOfParent();
		}

		void JE_EntryStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			SetStatusTextBoxColor(JobDeclaration.JE_EntryStatus);
		}

		public void SetStatusTextBoxColor(string status)
		{
			var bgColor = JobDeclarationColorHelper.GetColorForEntryStatus(status);

			StatusTextBox.ColorChanger.ForceBackColor(bgColor);
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control.Name == "DeclarantOfficeAddressControl" && previousControl.Name == "NotifyOrganisationControl")
				   || (control.Name == "NotifyOrganisationControl" && previousControl.Name == "ExternalBrokerGuidFindBox");
		}
	}
}
