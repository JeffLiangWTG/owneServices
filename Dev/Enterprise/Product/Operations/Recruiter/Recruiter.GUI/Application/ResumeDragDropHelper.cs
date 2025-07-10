using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Interop.DataObjects;
using CargoWise.Types;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Recruiter.Business.HRJobApplicationEmailParser;

namespace Enterprise.Recruiter.GUI
{
	public static class ResumeDragDropHelper
	{
		public static InsertResumeDragDropResult ProcessResumeDragDrop(IDataObject data, ZForm form)
		{
			var shouldSuspendEDocPopup = false;
			var shouldAddToEdocs = false;

			var application = form.BusinessEntity as HRJobApplication;
			var opening = form.BusinessEntity as HRRecruitmentJobCampaign;

			if (application == null && opening != null)
			{
				application = opening.Applications.AddNew();
			}

			if (data != null && application != null)
			{
				application.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;

				using (var resumeParser = new DaxtraResumeParser())
				using (var insertableData = ZDataObject.FromData(data))
				{
					var fileName = (insertableData?.GetData("FileDrop") as string[])?.FirstOrDefault();
					PopulateFromFileResult result;
					using (var progress = new ProgressForm())
					{
						var status = Res.GetString("2FB4CA4D-F737-4101-B102-1A16EE04AEB7", "Please wait while parsing a resume...");
						progress.ShowCancelButton = false;
						progress.ShowModalTo(form);
						progress.SetStatusAndPercentComplete(status, 0);
						progress.Update();
						result = new HRJobApplicationEmailParser(application, resumeParser, true).PopulateFromFile(fileName, opening);

						progress.SetStatusAndPercentComplete(status, 100);
						if (result.SubmissionDate != DateTime.MinValue)
						{
							application.HP_SubmissionTimeUtc = result.SubmissionDate;
						}
					}

					shouldSuspendEDocPopup = !result.ParseResult.HasFlag(ParseResult.BadFormat);
					shouldAddToEdocs = result.ParseResult.HasFlag(ParseResult.IsEmail);

					var message = ProcessFileResultMessage(result);
					if (!string.IsNullOrEmpty(message))
					{
						var caption = Res.GetString("6223D91C-AE1B-4817-96A0-C8E9CD8DC22F", "Parsing notifications");
						Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
				}

				ShowApplicantFormToFixErrors(application, false);
			}

			return new InsertResumeDragDropResult(shouldAddToEdocs, shouldSuspendEDocPopup);
		}

		public static void ShowApplicantFormToFixErrors(HRJobApplication application, bool shouldBeInDatabase)
		{
			if (application != null
				&& application.Applicant != null
				&& (application.Applicant.IsInDatabase == shouldBeInDatabase)
				&& (application.Applicant.HA_EmailAddress.IsEmpty || application.Applicant.HA_FullName.IsEmpty))
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

					if (!newApplicantWithError.IsInDatabase)
					{
						newApplicantWithError.Delete();
					}
				}
			}
		}

		public static void ProcessResumeDragDropWithoutDaxtra(IDataObject data, HRJobApplication application)
		{
			if (data != null && application != null)
			{
				using (var insertableData = ZDataObject.FromData(data))
				{
					ZString fileName = (insertableData?.GetData("FileDrop") as string[])?.FirstOrDefault();
					if (TryParseSenderEmailAddress(fileName, out var senderAddress))
					{
						application.SetupReferringSource(senderAddress);
					}
				}
			}
		}

		public class InsertResumeDragDropResult
		{
			public InsertResumeDragDropResult(bool shouldAddToEdocs, bool shouldSuspendEDocPopup)
			{
				ShouldAddToEdocs = shouldAddToEdocs;
				ShouldSuspendEDocPopup = shouldSuspendEDocPopup;
			}

			public bool ShouldSuspendEDocPopup { get; }
			public bool ShouldAddToEdocs { get; }
		}
	}
}
