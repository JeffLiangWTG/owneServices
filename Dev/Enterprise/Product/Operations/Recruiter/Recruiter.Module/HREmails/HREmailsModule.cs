using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Module;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using MimeKit;

namespace Enterprise.Recruiter.Module
{
	public class HREmailsModule : MailItemModule
	{
		HREmails SelectedMailItem => (Grid.SelectedElements.Length > 0) ? (HREmails)Grid.SelectedElements[0] : null;

		public override ModuleIdentifier ID => ModuleIDs.HREmails;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.HREmails;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Recruiter;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.HREmails);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new HREmailsFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new HREmailsFilterControl(GridCollection, (HREmailsFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new HREmailsCollection(Factory, new ZQuery());
		}

		public override bool AllowNew => false;

		public override bool AllowDelete => true;

		#region View

		protected override IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			if (!Env.Security.HREmailsView.IsAllowed)
			{
				Env.Security.HREmailsView.ShowError();
				return null;
			}

			try
			{
				var mail = (MailItem)selectedBusinessObject;
				ShowMailInOutlookExpress(mail);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.Show(Res.GetString("97996186-DCB8-4EC9-8328-BA29CF9074E1", "There was a problem showing this email in Microsoft Outlook Express."));
				return base.ShowViewForm(selectedBusinessObject);
			}
			return null;
		}

		#endregion

		#region Actions

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewStandardMenuItems());
			result.Add(new ZMenuItem(Res.GetData("772B1DE6-1E00-48CB-8B0B-787489E7F622", "Reply"), ReplyClick));
			result.Add(new ZMenuItem(Res.GetData("3220BD6D-B661-40C7-854A-E468A1AC14D1", "Reply All"), ReplyAllClick));
			result.Add(new ZMenuItem(Res.GetData("3E89530A-F23E-49C1-8F2B-3C7BE2737F64", "Forward"), ForwardClick));

			var jobApplicationMenu = new ZMenuItem(Res.GetData("52F72508-EFFE-49EB-A22D-2644299926F9", "Job Application"), CreateJobApplication);
			jobApplicationMenu.MenuItems.Add(new ZMenuItem(Res.GetData("3CF5868A-C0CF-407E-B1F4-B23491CF6921", "Create Job Application"), CreateJobApplication));
			jobApplicationMenu.MenuItems.Add(new ZMenuItem(Res.GetData("BA9C4A27-CB18-4452-BF36-4F4B3AE22371", "Attach to Job Application"), AttachJobApplication));
			result.Add(jobApplicationMenu);

			return result.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("-"));
			result.Add(new ZMenuItem(Res.GetData("CE31AA1E-2B96-43CF-B734-3FE0792F9CD6", "Mark for Re-Processing"), MarkForReprocess));
			return result.ToArray();
		}

		protected override void AddResetStatusMenuItem(List<MenuItem> result)
		{
			// We don't need the reset status menu item
		}

		void ReplyClick(object sender, EventArgs e)
		{
			if (!Env.Security.HREmailsReply.IsAllowed)
			{
				Env.Security.HREmailsReply.ShowError();
				return;
			}

			if (SelectedMailItem != null)
			{
				var email = SetEmailAddressAndDisplayName(SelectedMailItem);
				var form = new HREmailsForm(email);

				form.Show();
				email.EmailSent += Email_EmailSent;
			}
			else
			{
				Globals.Message.Show(Res.GetString("08980F18-5DFE-4F26-97BF-D953F69AFC04", "Please select a mail item to reply to."));
			}
		}

		void ReplyAllClick(object sender, EventArgs e)
		{
			if (!Env.Security.HREmailsReply.IsAllowed)
			{
				Env.Security.HREmailsReply.ShowError();
				return;
			}

			if (SelectedMailItem != null)
			{
				var email = SetEmailAddressAndDisplayName(SelectedMailItem);

				foreach (MailRecipient mailRecipient in SelectedMailItem.MailRecipients)
				{
					if (mailRecipient.MR_RecipientMailAddress.Contains("<", StringComparison.OrdinalIgnoreCase) && mailRecipient.MR_RecipientMailAddress.Contains(">", StringComparison.OrdinalIgnoreCase))
					{
						var recipient = mailRecipient.MR_RecipientMailAddress.Substring(mailRecipient.MR_RecipientMailAddress.IndexOf("<", StringComparison.OrdinalIgnoreCase) + 1);
						recipient = recipient.Remove(recipient.Length - 1, 1);
						email.ToEmailAddress += recipient + ";";
						if (mailRecipient.MR_RecipientMailAddress.IndexOf("<", StringComparison.OrdinalIgnoreCase) > 0)
						{
							email.ToDisplayName += mailRecipient.MR_RecipientMailAddress.Substring(0, mailRecipient.MR_RecipientMailAddress.IndexOf("<", StringComparison.OrdinalIgnoreCase) - 1) + ";";
						}
						else
						{
							email.ToDisplayName += recipient + ";";
						}
					}
					else
					{
						email.ToDisplayName += mailRecipient.MR_RecipientMailAddress + ";";
						email.ToEmailAddress += mailRecipient.MR_RecipientMailAddress + ";";
					}
				}

				var form = new HREmailsForm(email);
				form.Show();
				email.EmailSent += Email_EmailSent;
			}
			else
			{
				Globals.Message.Show(Res.GetString("4D1BFE9D-BA8E-462F-9228-DADA61058C95", "Please select a mail item to reply to."));
			}
		}

		void ForwardClick(object sender, EventArgs e)
		{
			if (!Env.Security.HREmailsForward.IsAllowed)
			{
				Env.Security.HREmailsForward.ShowError();
				return;
			}

			if (SelectedMailItem != null)
			{
				ShowMailInOutlookForForward(SelectedMailItem);
			}
			else
			{
				Globals.Message.Show(Res.GetString("13DD6CEA-7730-410E-9CAF-0621B4EA56FB", "Please select a mail item to forward."));
			}
		}

		void CreateJobApplication(object sender, EventArgs e)
		{
			if (!Env.Security.HRJobApplicationNew.IsAllowed)
			{
				Env.Security.HRJobApplicationNew.ShowError();
				return;
			}

			var selectedEmails = GetSelectedEmails();
			if (selectedEmails != null)
			{
				if (selectedEmails.Length > 1)
				{
					Globals.Message.ShowWarning(Res.GetString("902A0042-F6D6-40D4-A350-3E5C9DBAE083", "Please select only one email to create new application."));
					return;
				}

				var selectedEmail = selectedEmails[0];
				using (var resumeParser = new DaxtraResumeParser())
				{
					var parser = CreateHRJobApplicationEmailParser(selectedEmail, resumeParser, Factory, true);
					parser.ParseMailItem(selectedEmail);

					if (parser.Parent == null)
					{
						Globals.Message.ShowWarning(Res.GetString("7083FFF6-D384-4183-987C-E8C103562F1F", "Email could not be processed."));
						return;
					}

					var application = parser.Parent;
					if (application.Applicant != null && !application.Applicant.IsInDatabase && (application.Applicant.HA_EmailAddress.IsEmpty || application.Applicant.HA_FullName.IsEmpty))
					{
						var controllerHRJobApplicant = ZControllerFactory.Create(ControllerIDs.HRJobApplicant);
						controllerHRJobApplicant.ShowChildrenAsDialog = true;
						controllerHRJobApplicant.ShowFormForNewEntity(application.Applicant);

						// if the new Applicant wasn't saved correctly
						application.Applicant.RunPreSaveValidation();
						if (application.Applicant.HasErrors)
						{
							var newApplicantWithError = application.Applicant;
							application.HP_HA = ZGuid.Empty;
							newApplicantWithError.Delete();
						}
					}

					var controllerJobApplication = ZControllerFactory.Create(ControllerIDs.HRJobApplication);
					var formJobApplication = (ZForm)controllerJobApplication.ShowFormForNewEntity(application);

					formJobApplication.Saved += delegate(object formSender, EventArgs formEvent)
					{
						ResetStatusToProcessed(new[] { selectedEmail });
						Globals.Message.ShowInformation(Res.GetString("5FE81D25-FEFA-4749-8936-2506A163810F", "Job application successfully created and email attached"));
					};

					formJobApplication.FormClosed += delegate(object formSender, FormClosedEventArgs ev)
					{
						if (!application.IsInDatabase)
						{
							application.Delete();
						}
					};
#if DEBUG
					if (Globals.IsTest)
					{
						CreateJobApplicationFormForTest = formJobApplication;
					}
#endif
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("CBBC4B86-F270-4558-90D3-07104B667E0F", "Please select one email to attach to the new job application."));
			}
		}

		protected virtual HRJobApplicationEmailParser CreateHRJobApplicationEmailParser(MailItem mailItem, DaxtraResumeParser daxtraResumeParser, BusinessObjectFactory factory, bool createApplicantWithEmptyEmail)
		{
			return new HRJobApplicationEmailParser(mailItem, daxtraResumeParser, factory, createApplicantWithEmptyEmail);
		}

#if DEBUG
		public ZForm CreateJobApplicationFormForTest;
#endif

		void AttachJobApplication(object sender, EventArgs e)
		{
			if (!Env.Security.HRJobApplicationEdit.IsAllowed)
			{
				Env.Security.HRJobApplicationEdit.ShowError();
				return;
			}

			var selectedMailItems = Grid.GetSelectedElements<MailItem>();
			if (selectedMailItems.Length > 0)
			{
				var recordChooser = new ZRecordChooser<HRJobApplication>(ModuleIDs.HRJobApplication);
				recordChooser.ShowModal(Grid.FindForm(), delegate(HRJobApplication[] selectedTasks)
				{
					if (selectedTasks.Length > 0)
					{
						var showSaveConcurrecyWarning = false;

						foreach (var selectedTask in selectedTasks)
						{
							BusinessObjectEmailAttacher.AttachEmail(selectedTask, string.Empty, selectedMailItems);

							try
							{
								selectedTask.Factory.Save();
							}
							catch (ZSaveConcurrencyException)
							{
								showSaveConcurrecyWarning = true;
								foreach (var selectedMailItem in selectedMailItems)
								{
									selectedMailItem.Reload();
								}
								if (selectedTask is BusinessObject selectedBizO)
								{
									selectedBizO.Reload();
								}
							}
						}

						ResetStatusToProcessed(selectedMailItems);
						if (showSaveConcurrecyWarning)
						{
							Globals.Message.ShowWarning(Res.GetString("293053D4-00AB-4791-9B7F-3B38B07F092D", "The selected email(s) have been modified by another user. The other user's changes have been merged with yours. Please try again."));
						}
						else
						{
							Globals.Message.ShowInformation(Res.GetString("5906C22F-B6A2-49BA-8665-AD3D43CD1F6D", "Email successfully attached to job application"));
						}
					}
				});
			}
			else
			{
				Globals.Message.Show(Res.GetString("3AE47F90-9043-4887-90E9-72E3461DA878", "Please select 1 or more mail items to attach to the job application."));
			}
		}

		void MarkForReprocess(object sender, EventArgs e)
		{
			var selectedMailItems = Grid.GetSelectedElements<MailItem>();
			if (selectedMailItems.Length > 0)
			{
				foreach (var item in selectedMailItems)
				{
					item.MI_Status = MailStatus.MarkedForReprocessing;
				}

				selectedMailItems[0].Factory.Save();
			}
			else
			{
				Globals.Message.Show(Res.GetString("04C07B33-904C-4E56-B7F2-0E6482867E19", "Please select 1 or more mail items to re-process."));
			}
		}

		void Email_EmailSent(object sender, EventArgs e)
		{
			if (SelectedMailItem != null)
			{
				SelectedMailItem.MI_Status = MailStatus.Processed;
				SelectedMailItem.Factory.Save();
			}
		}

		void ShowMailInOutlookForForward(MailItem attachedEmail)
		{
			ShowMailInOutlookExpress(attachedEmail);
		}

		protected virtual void ShowMailInOutlookExpress(MailItem mail)
		{
			var filename = Temp.GetTempFileNameWithExtension((NoResString)"eml"); // Outlook Express file extension
			var emlBytes = BusinessObjectEmailAttacher.BuildMessage(mail);

			File.WriteAllText(filename, Encoding.UTF8.GetString(emlBytes));
			FileOpener.Open(filename);
		}

		HREmailToContactBusinessObject SetEmailAddressAndDisplayName(MailItem selectedMailItem)
		{
			var result = GetNewServiceEmail(selectedMailItem);

			result.Subject = selectedMailItem.MI_Subject;

			if (!result.Subject.ToUpper().Contains("RE:", StringComparison.OrdinalIgnoreCase)) // Untranslatable reason
			{
				result.Subject = "RE: " + result.Subject; // Untranslatable reason
			}

			if (MailboxAddress.TryParse(selectedMailItem.MI_From, out var mailboxAddress))
			{
				result.ToDisplayName = (mailboxAddress.Name ?? mailboxAddress.Address) + ";";
				result.ToEmailAddress = mailboxAddress.Address + ";";
			}

			return result;
		}

		HREmailToContactBusinessObject GetNewServiceEmail(MailItem selectedMailItem)
		{
			return new HREmailToContactBusinessObject(selectedMailItem);
		}

		void ResetStatusToProcessed(BusinessObject[] selectedElements)
		{
			try
			{
				if (selectedElements.Length > 0)
				{
					var assigner = GetMailStatusAssigner(selectedElements, MailStatus.Processed);
					assigner.Assign();

					if (!assigner.HasDBChanged)
					{
						if (assigner.Errors.Length > 0)
						{
							Globals.Message.ShowError(Res.GetString("1DCBC296-DAA1-48A1-B07A-2F4198A9B2A6", "Errors were encountered trying to reset the Status of Mail Item(s).\r\n{0}", assigner.Errors));
						}
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("45812D53-2ACB-4E47-9EFD-AE095E5850A6", "One or more of the selected Emails has been changed. Please refresh the grid and try again."));
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("9E996DE8-02BA-4949-B15F-3F7B28EFCB67", "Please select one or more Emails to reset the Status."));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Exception Resetting Email Status", ex);
			}
		}

		protected virtual HREmails[] GetSelectedEmails() => Grid.SelectedElements.Length > 0 ? Grid.SelectedElements.Cast<HREmails>().ToArray() : null;

		#endregion
	}
}
