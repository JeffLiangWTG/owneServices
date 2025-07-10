using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	public partial class SupportingDocSendingForm : MessageSendingObjectForm
	{
		public SupportingDocSendingForm()
		{
		}

		public SupportingDocSendingForm(BaseMessageSendingObjectParent messageSendingObjectParent)
			: base(messageSendingObjectParent)
		{
		}

		public override string FormHeading
		{
			get { return Res.GetString("0ED25BAC-496D-45A5-84E1-1F3469188B21", "Send Supporting Documents to Customs"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeNewColumns();
		}

		void InitializeNewColumns()
		{
			while (this.MessageSendingObjectsGrid.ColumnStyles.Count > 0)
			{
				this.MessageSendingObjectsGrid.ColumnStyles.RemoveAt(0);
			}

			ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo eDocZGuidDropEditColumnStyleInfo = new ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			eDocZGuidDropEditColumnStyleInfo.ColumnName = SupportingDocSendingObject.Schema.EDoc;
			eDocZGuidDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(310);
			eDocZGuidDropEditColumnStyleInfo.IsMandatory = true;
			this.MessageSendingObjectsGrid.ColumnStyles.Add(eDocZGuidDropEditColumnStyleInfo);

			ZArchitecture.GUI.ZDropEditColumnStyleInfo zADocumentTypeDropEditColumnStyleInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			zADocumentTypeDropEditColumnStyleInfo.ColumnName = SupportingDocSendingObject.Schema.DocumentType;
			zADocumentTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zADocumentTypeDropEditColumnStyleInfo.IsMandatory = true;
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zADocumentTypeDropEditColumnStyleInfo);

			if (LRNColumnVisible)
			{
				ZArchitecture.GUI.ZDropEditColumnStyleInfo lRNDropEditColumnStyleInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
				lRNDropEditColumnStyleInfo.ColumnName = SupportingDocSendingObject.Schema.LocalReferenceNumber;
				lRNDropEditColumnStyleInfo.Caption = LRNColumnName;
				lRNDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(198);
				lRNDropEditColumnStyleInfo.IsMandatory = true;

				this.MessageSendingObjectsGrid.ColumnStyles.Add(lRNDropEditColumnStyleInfo);
			}

			if (CaseNumberColumnVisible)
			{
				caseNumberDropEditColumnStyleInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
				caseNumberDropEditColumnStyleInfo.ColumnName = Business.SupportingDocSendingObject.Schema.CaseNumber;
				caseNumberDropEditColumnStyleInfo.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
				caseNumberDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
				caseNumberDropEditColumnStyleInfo.IsMandatory = true;
				this.MessageSendingObjectsGrid.ColumnStyles.Add(caseNumberDropEditColumnStyleInfo);
			}
		}

		protected virtual ZString LRNColumnName => Res.GetString("BB36167F-F0F6-4EE2-9A01-B5515DD3FB6E", "Entry (MRN or functional reference)");

		protected virtual ZBool LRNColumnVisible => ZBool.True;

		protected virtual ZBool CaseNumberColumnVisible => ZBool.False;

		protected ZArchitecture.GUI.ZDropEditColumnStyleInfo caseNumberDropEditColumnStyleInfo;
	}
}
