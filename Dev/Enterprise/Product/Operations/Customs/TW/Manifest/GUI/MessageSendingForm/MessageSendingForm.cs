using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.TW.Manifest.Business;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public partial class MessageSendingForm : Customs.GUI.MessageSendingObjectForm
	{
		public MessageSendingForm()
		: base()
		{
		}

		public MessageSendingForm(MessageSendingObjectParent headerWrapper)
		: base(headerWrapper)
		{
			OnDataSourceChanged(null, EventArgs.Empty);
			DataSourceChanged += OnDataSourceChanged;
		}

		void OnDataSourceChanged(object sender, EventArgs e)
		{
			ContinueToSendCheckBox.Visible = !BusinessEntity?.BizObjValidationMessageErrorsInfo.Value.IsEmpty ?? false;
		}

		public new MessageSendingObjectParent BusinessEntity => (MessageSendingObjectParent)base.BusinessEntity;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeColumns();
		}

		void InitializeColumns()
		{
			MessageSendingObjectsGrid.SetColumnWidth(MessageSendingObject.SchemaShouldSend, 60);

			var actionDropEditColumnStyleInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			actionDropEditColumnStyleInfo.ColumnName = MessageSendingObject.Schema.Action;
			actionDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			actionDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			actionDropEditColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(actionDropEditColumnStyleInfo);

			var billNumberTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			billNumberTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.Schema.BillNumber;
			billNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			billNumberTextBoxColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(billNumberTextBoxColumnStyleInfo);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource is MessageSendingObjectParent sendingObjectParent)
			{
				sendingObjectParent.AllowSendWithErrorInfo.ValueChanged -= AllowSendWithError_ValueChanged;
				sendingObjectParent.BizObjValidationMessageErrorsInfo.ValueChanged -= AllowSendWithError_ValueChanged;
				sendingObjectParent.AllowSendWithErrorInfo.ValueChanged += AllowSendWithError_ValueChanged;
				sendingObjectParent.BizObjValidationMessageErrorsInfo.ValueChanged += AllowSendWithError_ValueChanged;
				AllowSendWithError_ValueChanged(null, null);
			}
		}

		void AllowSendWithError_ValueChanged(object sender, EventArgs e)
		{
			SendButton.Enabled = BusinessEntity.SelectedSendingObjects.Any() && (BusinessEntity.BizObjValidationMessageErrorsInfo.Value.IsEmpty || BusinessEntity.AllowSendWithError);
		}
	}
}
