using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class MessageSendingForm : MessageSendingObjectForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public MessageSendingForm()
		{
		}

		public MessageSendingForm(JobDeclarationMessageSendingObjectParent declarationWrapper)
		: base(declarationWrapper)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeNewColumns();
		}

		void InitializeNewColumns()
		{
			var actionDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			actionDropEditColumnStyleInfo.ColumnName = MessageSendingObject.TWSchema.Action;
			actionDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			actionDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			actionDropEditColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(actionDropEditColumnStyleInfo);

			var entryNumberTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			entryNumberTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.TWSchema.EntryNumber;
			entryNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			entryNumberTextBoxColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(entryNumberTextBoxColumnStyleInfo);

			var entryStatusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			entryStatusTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.Schema.EntryStatus;
			entryStatusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			MessageSendingObjectsGrid.ColumnStyles.Add(entryStatusTextBoxColumnStyleInfo);

			var statusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			statusTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.TWSchema.MessageStatus;
			statusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			MessageSendingObjectsGrid.ColumnStyles.Add(statusTextBoxColumnStyleInfo);
		}

		public new JobDeclarationMessageSendingObjectParent BusinessEntity => (JobDeclarationMessageSendingObjectParent)base.BusinessEntity;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource is JobDeclarationMessageSendingObjectParent declarationSendingObjectParent)
			{
				declarationSendingObjectParent.AllowSendWithErrorInfo.ValueChanged -= AllowSendWithError_ValueChanged;
				declarationSendingObjectParent.BizObjValidationMessageErrorsInfo.ValueChanged -= AllowSendWithError_ValueChanged;
				declarationSendingObjectParent.AllowSendWithErrorInfo.ValueChanged += AllowSendWithError_ValueChanged;
				declarationSendingObjectParent.BizObjValidationMessageErrorsInfo.ValueChanged += AllowSendWithError_ValueChanged;
				AllowSendWithError_ValueChanged(null, null);
			}
		}

		void AllowSendWithError_ValueChanged(object sender, EventArgs e)
		{
			SendButton.Enabled = BusinessEntity.SelectedSendingObjects.Any() && (BusinessEntity.BizObjValidationMessageErrorsInfo.Value.IsEmpty || BusinessEntity.AllowSendWithError);
		}

		protected override MessageSendingNotificationCollection RunPreSendValidation()
		{
			return new MessageSendingNotificationCollection();
		}

		public static class Constants
		{
			public static string TestingEnvironment => Res.GetString("29384288-2476-470F-9C32-1BC3B627AD2A", "The selected messages will be sent to a testing environment");
		}

		protected override bool CheckIsOKToSend()
		{
			bool result = true;
			if (TWCustomsDataRegistry.IsTestMode)
			{
				if (Globals.Message.Show(Constants.TestingEnvironment, Res.GetString("9F093709-AD46-4244-9DE3-CD5305DF5026", "Continue to Send"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
				{
					result = false;
				}
			}
			if (result)
			{
				result = base.CheckIsOKToSend();
			}
			return result;
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control.Name == "ContinueToSendCheckBox" && previousControl.Name == "CancelButton2") || (control.Name == "CancelButton2" && previousControl.Name == "ContinueToSendCheckBox");
		}
	}
}
