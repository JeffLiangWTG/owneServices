using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using ConvertApiDotNet.Exceptions;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruitment.Module.CandidateManagement
{
	public partial class CandidateDetailsControl : ZUserControl
	{
		public CandidateDetailsControl()
		{
			InitializeComponent();
			mobileNumberControl.SetDialling(LogOnMobileCalled);
			alternatePhoneControl.SetDialling(LogOnAlternateCalled);
#if DEBUG
			TypeDescriptor.AddAttributes(nameLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(roleLabel, new SuppressFormsLocalizedTestAttribute());
#endif

			SetupPostingButtons();
			RefreshWorkflowGUI();
			RefreshPastRolesReadOnly();
		}

		void RefreshPastRolesReadOnly()
		{
			if (CurrentDataItem is Candidate candidate)
			{
				this.experienceTextBox.ReadOnly = candidate.Document == null;
			}
		}

		void LogOnMobileCalled(object sender, PhoneDiallerUserControl.DiallingEventArgs e)
		{
			CurrentDataItem?.Application.Logs.AddNew(AutoEvents.MeetingOccurred, new KeyValuePair<string, string>(nameof(EventLoggingKeys.MeetingCommunicationType), (NoResString)"Phone"));
		}

		void LogOnAlternateCalled(object sender, PhoneDiallerUserControl.DiallingEventArgs e)
		{
			CurrentDataItem?.Application.Logs.AddNew(AutoEvents.MeetingOccurred, new KeyValuePair<string, string>(nameof(EventLoggingKeys.MeetingCommunicationType), (NoResString)"Phone"));
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			RemoveSaveHooks();
			base.OnCurrentDataItemChanged(e);

			RefreshResumePreview();
			RefreshEdocs();
			RefreshPeopleAndCommunication();
			RefreshLogs();
			RefreshWorkflowGUI();
			RefreshCreateWorkItemButton();
			RefreshHiringRequestButton();
			RefreshPastRolesReadOnly();
			RefreshNotes();

			AddSaveHooks();
			UpdateSaveButtonEnabled();
		}

		protected void RefreshNotes()
		{
			notesControl.Enabled = !(CurrentDataItem?.IsNull ?? true);
		}

		protected void RefreshCreateWorkItemButton()
		{
			createWorkItemButton.Enabled = !(CurrentDataItem?.IsNull ?? true);
		}

		protected void RefreshHiringRequestButton()
		{
			HiringRequestButton.Enabled = !(CurrentDataItem?.IsNull ?? true);
		}

		protected void RefreshLogs() => logControl.SetupStmLogsControl(CurrentDataItem?.Application);

		protected void RefreshEdocs()
		{
			if (eDocsControl != null && !eDocsControl.IsDisposed)
			{
				eDocsControl.UpdateEDocs(CurrentDataItem?.Application);
			}
		}

		protected void RefreshPeopleAndCommunication()
		{
			peopleAndCommunicationControl.Enabled = !(CurrentDataItem?.IsNull ?? true);
		}

		protected void RefreshWorkflowGUI()
		{
			tasksGrid.Enabled = !(CurrentDataItem?.IsNull ?? true);
		}

		readonly HashSet<ZGuid> conversionsInProgress_OnlyUseOnMainThread = new HashSet<ZGuid>();

		void RefreshResumePreview()
		{
			var application = CurrentDataItem?.Application;
			var displayedEDoc = ShowDoc();

			if (application != null && displayedEDoc == null)
			{
				if (!conversionsInProgress_OnlyUseOnMainThread.Contains(application.PK))
				{
					ConvertAndAttachAsync(application);
				}

				if (conversionsInProgress_OnlyUseOnMainThread.Contains(application.PK))
				{
					var previewMessage = Res.GetString("9c2c3595-0126-4fb2-bf9b-71f7c2a0b6ba", "Resume is converting to correct format");
					documentPreview.ShowFile(new DummyPreviewable(previewMessage), null);
				}
				else if (!Disposing)
				{
					documentPreview.ShowFile(new DummyPreviewable(Res.GetString("df9688a7-6a90-4c71-8b8a-8a04f286cf13", "A resume in a supported format could not be found")), null);
				}
			}
			else if (CurrentDataItem?.IsNull ?? true)
			{
				documentPreview.Close();
			}
		}

		void ConvertAndAttachAsync(HRJobApplication application)
		{
			var converter = ObjectFactory.Get<IResumeConverter>(nameof(IResumeConverter), new object[] { null });
			var toConvert = converter.GetResumeToConvert(application);

			if (toConvert != null)
			{
				conversionsInProgress_OnlyUseOnMainThread.Add(application.PK);

				var convertApiResponseTask = converter.ConvertToPdfAsync(toConvert.DataType, new MemoryStream(toConvert.ImageData));
				convertApiResponseTask.ContinueWith(res =>
				{
					ExceptionHandler(res.Exception.InnerException);
					conversionsInProgress_OnlyUseOnMainThread.Remove(application.PK);
				}, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.FromCurrentSynchronizationContext());

				convertApiResponseTask.ContinueWith(res =>
				{
					AttachEdocConverApiResponse(application, res, converter, toConvert);
					_ = ShowDoc();
					conversionsInProgress_OnlyUseOnMainThread.Remove(application.PK);
				}, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		void ExceptionHandler(Exception exception)
		{
			if (exception is ConvertApiException)
			{
				documentPreview.ShowFile(new DummyPreviewable(exception.Message), null);
			}
			else if (!exception.IsCriticalException())
			{
				documentPreview.ShowFile(new DummyPreviewable(Res.GetString("0bd6bc13-5710-45a6-b6f5-02ebf1dfdebc", "Error converting resume")), null);
				ErrorReporter.ReportOnce("UnknownErrorConvertAttachmentToPDF", "An exception caught while converting to PDF", exception);
			}
		}

		protected StorageDocsBase ShowDoc()
		{
			var resume = CurrentDataItem?.Resume;
			if (resume != null)
			{
				if (!TryShowDocument(resume, out var badLoadReason))
				{
					var dummyPreviewDoc = new DummyPreviewable(badLoadReason);
					documentPreview.ShowFile(dummyPreviewDoc, null);
				}
			}

			return resume;
		}

		protected virtual void AttachEdocConverApiResponse(HRJobApplication application, Task<Stream> res, IResumeConverter converter, IeDoc toConvert)
		{
			converter.AttachEdocCore(application, Path.ChangeExtension(toConvert.FileName, converter.ConvertedFileExtension), res.Result, toConvert.DocType.ToString());
		}

		bool TryShowDocument(StorageDocsBase doc, out string badLoadReason)
		{
			IPreviewableDocument document = null;
			try
			{
				document = PreviewableDocumentHelper.GetPreviewableDocument(doc.SC_DataType, doc.SC_ImageData);
				documentPreview.ShowFile(document, doc);
				badLoadReason = null;
				return true;
			}
			catch (UnsupportedDocumentException)
			{
				document?.Dispose();
				documentPreview.Close();
				badLoadReason = Res.GetString("b05f5a7c-1041-4bfc-b0c3-edd7dc90d238", "The document has an unsupported format, please confirm Resume/CV is a PDF file.");
				return false;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				document?.Dispose();
				documentPreview.Close();
				badLoadReason = Res.GetString("14950623-aa8d-4b1b-93a4-6ee9e2e6a31a", "An unknown error occurred: {0}", ex.Message);
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		bool SaveCore()
		{
			ZFormUtilities.EnsureSelectedControlValueCommitted(FindForm());

			var item = CurrentDataItem;
			if (item != null && Validate(item))
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				_ = item.Application.Logs.AddNew(AutoEvents.EditedARecord, new KeyValuePair<string, string>(nameof(EventLoggingKeys.Info), Res.GetString("bd4a818f-daad-421f-baa8-7c0ee3473a7a", "Candidate profile was edited")));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Save(item.Factory);
				Save(PluginFactories.ToArray());
				UpdateSaveButtonEnabled();

				Saved?.Invoke(this, EventArgs.Empty);
				return true;
			}

			return item == null;
		}

		public void SaveButton_Click() => _ = SaveCore();

		public ContinueWithSave ValidateAndSave()
			=> SaveCore()
				? ContinueWithSave.Yes
				: ContinueWithSave.No;

		IEnumerable<ITransactionParticipant> PluginFactories
		{
			get
			{
				var edocs = eDocsControl.eDocsPlugIn?.BusinessEntity?.Factory;
				if (edocs != null)
				{
					yield return edocs;
				}
			}
		}

		bool Validate(BusinessObject entity)
		{
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(entity.Factory))
			{
				entity.MarkAsNeedingValidationIncludingChildren();
				entity.RunPreSaveValidation();

				TabPageNotificationsExposer.ExposeTabPageNotificationsOnIdle(this, entity);
				Refresh();

				if (entity.HasErrors)
				{
					ShowNotifications(entity);
					return false;
				}
			}

			return true;
		}

		static void ShowNotifications(BusinessObject entity)
		{
			using (var form = new ZErrorMessageBox(entity, includeIgnoreOption: false))
			{
				_ = ZFormModaliser.ShowMessageBoxWithoutDispose(form);
			}
		}

		static void Save(params ITransactionParticipant[] factories)
			=> ZExceptionReporting.ProcessWithSaveExceptionHandling(() => BusinessObjectFactory.SaveTogether(factories), null);

		void SetupPostingButtons()
		{
			postingButtons.SaveAndCloseButton.Visible = false;
			postingButtons.CloseButton.Visible = false;

			postingButtons.SaveButton.Image = Icons.GetImage(IconTypes.SaveButtonActive);
			postingButtons.SaveButton.Click += (o, e) => SaveButton_Click();
			postingButtons.SaveButton.Enabled = false;
			Hotkeys.RegisterHotKey(Keys.Control | Keys.S, SaveButton_Click, Res.GetString("5beac1aa-a55e-4018-b093-23aaf52dd242", "Save"));
		}

		void AddSaveHooks()
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.HasChangesChanged += HasChangesChanged_EnableSave;
				CurrentDataItem.Factory.Saved += FactorySaved_DisableSave;
			}
		}

		void RemoveSaveHooks()
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.HasChangesChanged -= HasChangesChanged_EnableSave;
				CurrentDataItem.Factory.Saved -= FactorySaved_DisableSave;
			}
		}

		public void DisableSaveButton()
		{
			postingButtons.SaveButton.Enabled = false;
			UpdateFromDisplayMode();
		}

		void UpdateSaveButtonEnabled()
		{
			postingButtons.SaveButton.Enabled = CurrentDataItem?.HasChanges ?? false;
			UpdateFromDisplayMode();
		}

		void UpdateFromDisplayMode()
		{
			var form = FindForm() as ZForm;
			if (form != null)
			{
				form.DisplayMode = postingButtons.SaveButton.Enabled ? ODisplayMode.Edit : ODisplayMode.Browse;
			}
		}

		void HasChangesChanged_EnableSave(object sender, EventArgs e)
			=> UpdateSaveButtonEnabled();

		void FactorySaved_DisableSave(BusinessObjectFactory sender, bool success)
			=> UpdateSaveButtonEnabled();

		public new Candidate CurrentDataItem => base.CurrentDataItem as Candidate;

		public event EventHandler<EventArgs> Saved;

		void CreateWorkItemButton_Click(object sender, EventArgs e)
		{
			var workItem = WorkItemCreator.CreateWorkItem(CurrentDataItem.Factory, CurrentDataItem.SelectedTemplate, CurrentDataItem.Applicant.HA_FullName);
			var controller = ZControllerFactory.Create(ControllerIDs.WorkItem);
			controller.SetFormsModalTo(ParentForm);
			_ = controller.ShowFormForNewEntity(workItem);
		}

		void HiringRequestButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				var baseURL = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				if (string.IsNullOrEmpty(baseURL))
				{
					var errorMessage = ResString.GetMultilingualString("7B58B60D-3860-49E0-A7B2-03F26E560EA4",
	@"The Hiring Request cannot be opened in a browser as GLOW has not been configured for this client.
Registry: ") + GlowRegistry.Instance.GlowPortalsUri.Category + "/" + GlowRegistry.Instance.GlowPortalsUri.Caption;
					Globals.Message.ShowError(errorMessage);
					return;
				}

				var jobTitle = string.Empty;
				if (CurrentDataItem?.Role != null && !CurrentDataItem.Role.HJ_JobTitle.IsEmpty)
				{
					jobTitle = CurrentDataItem.Role.HJ_JobTitle.ToString();
				}
				const string RelativePath = "goto/hiringRequest";
				var url = UrlBuilder.GenerateURL(new Uri(baseURL), RelativePath, additionalQueryStrings: new[] { ("applicantPK", CurrentDataItem.Applicant.PK.ToString()), ("jobTitle", jobTitle) });
				WebUrlLauncher.Launch(url.ToString());
			}
		}
	}
}
