using CargoWise.Types;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class SupportingDocSendingForm : Customs.GUI.SupportingDocSendingForm
	{
		public SupportingDocSendingForm()
		{
		}

		public SupportingDocSendingForm(Customs.Business.JobDeclarationSupportingDocSendingObjectParent declarationWrapper)
			: base(declarationWrapper)
		{
		}

		public override string FormHeading
		{
			get { return Res.GetString("032c2bc2-0d9a-4b1c-a48e-2bae5ca16117", "Send Supporting Documents to Customs"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeNewColumns();
		}

		void InitializeNewColumns()
		{
			if (caseNumberDropEditColumnStyleInfo != null)
			{
				caseNumberDropEditColumnStyleInfo.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			}
			else
			{
				caseNumberDropEditColumnStyleInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
				caseNumberDropEditColumnStyleInfo.ColumnName = Business.SupportingDocSendingObject.Schema.CaseNumber;
				caseNumberDropEditColumnStyleInfo.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
				caseNumberDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
				caseNumberDropEditColumnStyleInfo.IsMandatory = true;
				this.MessageSendingObjectsGrid.ColumnStyles.Add(caseNumberDropEditColumnStyleInfo);
			}

			ZArchitecture.ZCalcEditColumnStyleInfo eDocFileSizeInMBInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			eDocFileSizeInMBInfo.ColumnName = Business.SupportingDocSendingObject.Schema.EDocFileSizeInMB;
			eDocFileSizeInMBInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			eDocFileSizeInMBInfo.Decimals = 2;
			eDocFileSizeInMBInfo.IsMandatory = true;
			this.MessageSendingObjectsGrid.ColumnStyles.Add(eDocFileSizeInMBInfo);
		}

		protected override ZString LRNColumnName => ResString.GetMultilingualString("98BFD8C3-B11B-4E98-871A-2FB20B16BA32", "Entry (LRN)");

		protected override ZBool CaseNumberColumnVisible => ZBool.True;

		void MessageSendingObjectsGrid_Navigate(object sender, System.Windows.Forms.NavigateEventArgs ne)
		{
		}
	}
}

