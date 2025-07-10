using System;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class FZEventMessageSendingForm : ZChildForm
	{
		public FZEventMessageSendingForm(FZEventAction action)
			: base(action)
		{
			this.action = action;
			this.action.IsCancelled = true;
			action.US_ReasonCodeInfo.ValueChanged += US_ReasonCodeInfo_ValueChanged;
		}
		readonly FZEventAction action;

		public override string FormCaption
		{
			get { return "FZ Event Reporting"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			var fzEventAction = dataSource as FZEventAction;
			if (fzEventAction != null)
			{
				var isUnconcur = (fzEventAction.EventType == FZEventType.Unconcur);
				FTZActionPanel.Visible = !isUnconcur;
				FTZUnconcurrencePanel.Visible = isUnconcur;
			}
		}

		void US_ReasonCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			switch (action.US_ReasonCode)
			{
				case FTZUnconcurrenceReasonCodeList.Codes._02:
				case FTZUnconcurrenceReasonCodeList.Codes._03:
				case FTZUnconcurrenceReasonCodeList.Codes._04:
				case FTZUnconcurrenceReasonCodeList.Codes._05:
					ReasonsTextBox.Visible = true;
					ReasonsTextBox.CaptionResourceString = action.GetReasonsCaption();
					ReasonsTextBox.UpdateCaption();
					break;
				default:
					ReasonsTextBox.Visible = false;
					break;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				if (action != null)
				{
					action.US_ReasonCodeInfo.ValueChanged -= US_ReasonCodeInfo_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			action.IsCancelled = true;
			action.RunPreSaveValidation();

			if (action.HasMessageErrors)
			{
				Globals.Message.ShowError("Please fix the errors first");
			}
			else
			{
				action.IsCancelled = false;
				Close();
			}
		}

		void GiveUpButton_Click(object sender, EventArgs e)
		{
			action.IsCancelled = true;
			Close();
		}
	}
}
