using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.DIS.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.DIS.GUI
{
	public partial class DISForm : ZChildForm
	{
		public DISForm(DISHostWrapper disWrapper)
			: base(disWrapper)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CancelOrCloseButton, SaveButton);
			SendButton.Enabled = Env.Security.CustomsDISEdit.IsAllowed;
			disUserControl1.DocumentsGrid.RowsDeleted += DocumentsGrid_RowsDeleted;
		}

		void DocumentsGrid_RowsDeleted(object sender, ZArchitecture.RowsDeletingEventArgs e)
		{
			UpdateButtonStatus();
		}

		new DISHostWrapper BusinessEntity
		{
			get { return (DISHostWrapper)base.BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public override string FormCaption
		{
			get { return "Document Image System"; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (BusinessEntity != null)
			{
				BusinessEntity.HasChangesChanged += BusinessEntity_HasChangesChanged;

				UpdateButtonStatus();
			}
		}

		void BusinessEntity_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			UpdateButtonStatus();
		}

		void UpdateButtonStatus()
		{
			if (BusinessEntity != null)
			{
				if (BusinessEntity.HasChanges)
				{
					CancelOrCloseButton.Text = "&Cancel";
					SendButton.Text = "Save && &Send";
					SaveButton.Enabled = true;
				}
				else
				{
					CancelOrCloseButton.Text = "&Close";
					SendButton.Text = "&Send";
					SaveButton.Enabled = false;
				}
			}
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			base.ZForm_Closing(sender, e);
			if (!e.Cancel)
			{
				BusinessEntity.IsActive = false; //by this time, save should be complete
			}
		}

		protected override void HandleSaveWhileClosing(CancelEventArgs e)
		{
			BusinessEntity.IsActive = true; //just in case save is not yet finished (i.e. BusinessEntity recycled)
			base.HandleSaveWhileClosing(e);
		}

		bool ValidateAndSaveData()
		{
			return FireSaveButton() == ContinueWithSave.Yes;
		}

		protected override void Dispose(bool disposing)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.ReleaseObjects();
				BusinessEntity.HasChangesChanged -= BusinessEntity_HasChangesChanged;
			}
			base.Dispose(disposing);
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.DISDocuments.Count == 0)
			{
				Globals.Message.ShowError("Please enter at least one DIS document first.");
			}
			else
			{
				if (!Env.Security.CustomsDISSendMessage.IsAllowed)
				{
					Env.Security.CustomsDISSendMessage.ShowError();
				}
				else if (ValidateAndSaveData())
				{
					var proceed = true;
					if (BusinessEntity.HasMessageErrors())
					{
						var validation = MessageSendingValidation.New(BusinessEntity, new CustomsNotificationCollector(BusinessEntity, true, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName));
						var notifications = validation.CheckBusinessObjectLevelValidation();

						if (notifications.ContainsError())
						{
							proceed = false;
							Globals.Message.ShowError(notifications.NotificationsAsString());
						}
						else if (notifications.ContainsWarning())
						{
							if (!Env.Security.CustomsDISSendWithMessageErrors.IsAllowed)
							{
								proceed = false;
								Globals.Message.ShowError(Customs.Business.SingleMessageManager.MessageErrorsExistWithNoSecurityRight + " " + Env.Security.CustomsDISSendWithMessageErrors.DisplayTextPathToSecurityRight);
							}
							else if (Globals.Message.Show(notifications.NotificationsAsString(), "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
							{
								proceed = false;
							}
						}
					}

					if (proceed)
					{
						var coll = new MessageSendingActionCollection(BusinessEntity);
						using (MessageSendingForm sendForm = new MessageSendingForm(coll))
						{
							ZFormModaliser.ShowDialogWithoutDispose(sendForm);

							if (sendForm.ProceedWithSend)
							{
								coll.SendMessages();

								try
								{
									coll.Factory.Save();
									Globals.Message.ShowInformation("Messages Sent.");
									Close();
								}
								catch (ZSaveException exception)
								{
									ZExceptionReporting.HandleSaveException(exception);
								}
							}
						}
					}
				}
			}
		}
	}
}
