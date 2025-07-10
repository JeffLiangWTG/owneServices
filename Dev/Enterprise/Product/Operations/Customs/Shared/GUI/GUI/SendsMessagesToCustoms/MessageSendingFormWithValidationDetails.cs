using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class MessageSendingFormWithValidationDetails : MessageSendingObjectForm
	{
		[Obsolete("Do not call. Only for designer use.")]
		public MessageSendingFormWithValidationDetails()
		{
		}

		public MessageSendingFormWithValidationDetails(BaseMessageSendingObjectParent messageSendingObjectParent)
			: base(messageSendingObjectParent)
		{
			BindingSource.DataSourceType = typeof(BaseMessageSendingObjectParent);
			actionParent = messageSendingObjectParent;

			if (!messageSendingObjectParent.SecurityCheckpointToSendWithMessageError.IsAllowed)
			{
				SendWithValidationErrorsCheckBox.Enabled = false;
				SendWithValidationErrorsCheckBox.CaptionResourceString = Res.GetData("1413f5b4-9d47-4d9f-a872-8f160709b41a", "You don't have security rights to send with message errors");
			}

			actionParent.SelectedSendingObjectsChanged += OnSelectedSendingActionsChanged;

			SendWithValidationErrorsCheckBox.CheckedChanged += SendWithValidationErrorsCheckBox_CheckedChanged;
			SendWithAdditionalWarningCheckBox.CheckedChanged += SendWithAdditionalWarningCheckBox_CheckedChanged;

			messageSendingObjectParent.SelectedSendingObjectsChanged += MessageSendingObjectParent_SelectedSendingObjectsChanged;
			Controls.Remove(messageSendingObjectsGroupBox);
			SplitContainer.Panel1.Controls.Add(messageSendingObjectsGroupBox);
			messageSendingObjectsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
		}

		readonly BaseMessageSendingObjectParent actionParent;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeGridColumns();
			AddUserControlToBottomSection();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (MessageSendingObjectParent != null)
			{
				PreviewMessageCheckBox.Visible = PreviewMessageCheckboxVisible && GlbStaff.CurrentUser.GS_IsDeveloper;
				OnSelectedSendingActionsChanged(null, null);
				MessageSendingObjectParent_SelectedSendingObjectsChanged(null, null);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (actionParent != null)
			{
				actionParent.SelectedSendingObjectsChanged -= OnSelectedSendingActionsChanged;
			}

			if (MessageSendingObjectParent != null)
			{
				MessageSendingObjectParent.SelectedSendingObjectsChanged -= MessageSendingObjectParent_SelectedSendingObjectsChanged;
			}

			base.Dispose(disposing);
		}

		void MessageSendingObjectParent_SelectedSendingObjectsChanged(object sender, EventArgs e)
		{
			ChangeSendWithValidationErrorsCheckBoxAvailability();
			ChangeSendWithAdditionalWarningCheckBoxAvailability();
			ChangeSendButtonAvailability();
		}

		void SendWithAdditionalWarningCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			ChangeSendButtonAvailability();
		}

		void SendWithValidationErrorsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			ChangeSendButtonAvailability();
		}

		protected virtual void ChangeSendButtonAvailability()
		{
			if (MessageSendingObjectParent != null)
			{
				var sendButton = GetEffectiveSendButton();
				sendButton.Enabled = MessageSendingObjectParent.SelectedSendingObjects.Any()
					&& (!SendWithValidationErrorsCheckBox.Visible || SendWithValidationErrorsCheckBox.Checked)
					&& (!SendWithAdditionalWarningCheckBox.Visible || SendWithAdditionalWarningCheckBox.Checked);
			}
		}

		protected void ChangeSendWithValidationErrorsCheckBoxAvailability()
		{
			SendWithValidationErrorsCheckBox.Visible = SendWithValidationErrorsCheckBoxVisible;
		}

		protected void ChangeValidationErrorsTextBoxAvailability()
		{
			ValidationErrorsTextBox.Visible = SendWithValidationErrorsTextBoxVisible;
		}

		protected virtual bool SendWithValidationErrorsTextBoxVisible => true;

		protected virtual bool SendWithValidationErrorsCheckBoxVisible => MessageSendingObjectParent == null || !MessageSendingObjectParent.BizObjValidationMessageErrors.IsEmpty;

		protected void ChangeSendWithAdditionalWarningCheckBoxAvailability()
		{
			SendWithAdditionalWarningCheckBox.Visible = SendWithAdditionalWarningCheckBoxVisible;
		}

		protected virtual bool SendWithAdditionalWarningCheckBoxVisible => MessageSendingObjectParent == null || !MessageSendingObjectParent.AdditionalWarnings.IsEmpty;

		public BaseMessageSendingObjectParent MessageSendingObjectParent => BusinessEntity;

		void InitializeGridColumns()
		{
			if (MessageSendingGridColumnLayoutProvider != null)
			{
				MessageSendingObjectsGrid.ApplyGridColumnLayout(MessageSendingGridColumnLayoutProvider);
				return;
			}

			InitializeGridColumnsUsingMessageSendingObject();
		}

		void InitializeGridColumnsUsingMessageSendingObject()
		{
			var sendingParent = MessageSendingObjectParent;
			if (sendingParent != null)
			{
				var typeofElements = sendingParent.SendingObjectsCollection.TypeOfElements;
				foreach (var property in sendingParent.MessageSendingObjectProperties)
				{
					var column = CreateColumnInfo(typeofElements.GetProperty(property.PropertyName));
					column.ColumnName = property.PropertyName;
					column.IsMandatory = property.IsMandatory;
					column.IsVisible = property.IsVisible;
					column.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(property.ColumnWidth);
					column.CaptionResourceString = property.ResourceString;
					MessageSendingObjectsGrid.ColumnStyles.Add(column);
				}
			}
		}

		ZGridColumnInfo CreateColumnInfo(PropertyInfo propertyInfo)
		{
			if (IsListAttributeAttached(propertyInfo))
			{
				return new ZDropEditColumnStyleInfo();
			}
			else if (propertyInfo.PropertyType == typeof(ZBool))
			{
				return new ZCheckBoxColumnStyleInfo();
			}
			else if (propertyInfo.PropertyType == typeof(ZDateTime))
			{
				return new ZDateEditColumnStyleInfo();
			}
			else if (propertyInfo.PropertyType == typeof(ZDate))
			{
				return new ZDateEditColumnStyleInfo() { DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short };
			}

			return new ZTextBoxColumnStyleInfo();
		}

		static bool IsListAttributeAttached(PropertyInfo propertyInfo)
		{
			return propertyInfo?.GetCustomAttribute<ListAttribute>() != null;
		}

		protected virtual void AddUserControlToBottomSection()
		{
			var control = GetBottomSectionUserControl();
			control.Dock = DockStyle.Fill;
			WarningSplitContainer.Panel2.Controls.Add(control);
		}

		protected virtual ZUserControl GetBottomSectionUserControl() => new MessageSendingFormBottomSectionUserControl();

		protected virtual bool PreviewMessageCheckboxVisible => false;

		protected override void SendButton_ClickCore()
		{
			base.SendButton_ClickCore();

			var shouldSendActions = actionParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>().Where(action => action.ShouldSend).Take(2).ToArray();
			if (shouldSendActions.Length == 1 || ShouldSendSingleMessageForMultipleObjects && shouldSendActions.Length > 0)
			{
				var sendingAction = shouldSendActions[0];
				sendingAction.PreviewMessage -= PreviewMessage;
				sendingAction.PreviewMessage += PreviewMessage;
			}
		}

		protected void PreviewMessage(MessageEventArgs args)
		{
			if (PreviewMessageCheckBox.CheckState == CheckState.Checked)
			{
				using (var editForm = new MessageEditForm())
				{
					args.MessageText = editForm.EditMessage(args.MessageText);
				}
			}
		}

		protected virtual IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider { get; }

		protected virtual bool ShouldSendSingleMessageForMultipleObjects => false;

		void OnSelectedSendingActionsChanged(object sender, EventArgs e)
		{
			var shouldSendActionCount = actionParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>().Count(action => action.ShouldSend);
			var shouldEnableAndAutoChecked = PreviewMessageCheckBox.Visible && (shouldSendActionCount == 1 || ShouldSendSingleMessageForMultipleObjects && shouldSendActionCount > 0);
			PreviewMessageCheckBox.Enabled = shouldEnableAndAutoChecked;
			PreviewMessageCheckBox.Checked = shouldEnableAndAutoChecked;
		}
	}
}
