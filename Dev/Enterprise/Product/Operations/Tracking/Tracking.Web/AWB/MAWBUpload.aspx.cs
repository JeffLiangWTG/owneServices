using System;
using System.IO;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.TNT;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Web
{
	public partial class MAWBUpload : BasePageWithAuthorisation
	{
		const string CurrentStepKey = "CurrentStep";
		const string DataKey = "ImportedFile";

		#region Overrides

		protected override bool CanAccessAuthorisedContent
		{
			get { return true; }
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisabledSubmitButtons.Add(NextButton.ClientID);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!IsPostBack)
			{
				InitializeValues();
			}
		}

		void InitializeValues()
		{
			DataFile = null;
			CurrentStepValue = WebTracker.Upload.UploadStep.ValidateFile;
			ValidationErrors.Visible = ValidationWarnings.Visible = BasicInfo.Visible = ValidationInfo.Visible = false;
		}

		#endregion

		#region Events

		protected void Next_Click(object sender, EventArgs e)
		{
			switch (CurrentStepValue)
			{
				case WebTracker.Upload.UploadStep.ValidateFile:
					if (ValidateFile())
					{
						SwitchStep(WebTracker.Upload.UploadStep.ImportFile);
					}
					break;
				case WebTracker.Upload.UploadStep.ImportFile:
					if (ImportFile())
					{
						SwitchStep(WebTracker.Upload.UploadStep.ShowResults);
					}
					else
					{
						SwitchStep(WebTracker.Upload.UploadStep.ValidateFile);
					}
					break;
				default:
					break;
			}
		}

		#endregion

		#region DataSource

		byte[] DataFile
		{
			get { return (byte[])Session[DataKey]; }
			set { Session[DataKey] = value; }
		}

		ZGuid ImportedPK { get; set; }

		#endregion

		#region Steps

		WebTracker.Upload.UploadStep CurrentStepValue
		{
			get
			{
				var currentValue = ViewState[CurrentStepKey];
				var result = currentValue == null ? 1 : (int)currentValue;

				return (WebTracker.Upload.UploadStep)result;
			}
			set
			{
				ViewState[CurrentStepKey] = (int)value;
			}
		}

		bool ImportFile()
		{
			if (DataFile != null)
			{
				var importer = new AWBImporter();
				var logger = new WebImportLogger();
				var awb = importer.Import(SiteUser.LoggedInUser, new MemoryStream(DataFile), logger);

				if (!awb.MAWBRecordPK.IsEmpty)
				{
					ImportedPK = awb.MAWBRecordPK;
					return true;
				}
				else
				{
					ShowErrors(logger);
					return false;
				}
			}

			return false;
		}

		bool ValidateFile()
		{
			if (InputFile.HasFile && InputFile.FileContent.Length > 0)
			{
				using (InputFile.FileContent)
				{
					byte[] data = new byte[InputFile.FileContent.Length];
					int offset = 0;
					int remaining = (int)InputFile.FileContent.Length;
					while (remaining > 0)
					{
						int read = InputFile.FileContent.Read(data, offset, remaining);
						remaining -= read;
						offset += read;
					}
					return ProcessUploadedFile(data, Path.GetFileName(InputFile.FileName));
				}
			}

			return false;
		}

		bool ShowErrors(WebImportLogger logger)
		{
			var isErrorFree = logger.Errors.Count == 0;

			ValidationErrors.Visible = !isErrorFree;
			ImportValidationError.InnerHtml = String.Join("<BR>", logger.Errors.ToArray());

			ValidationInfo.Visible = logger.Infos.Count > 0;
			ImportValidationInfo.InnerHtml = String.Join("<BR>", logger.Infos.ToArray());

			ValidationWarnings.Visible = logger.Warnings.Count > 0;
			ImportValidationWarning.InnerHtml = String.Join("<BR>", logger.Warnings.ToArray());

			return isErrorFree;
		}

		void ShowBasicInfo(IAWBProcessedInfo awb)
		{
			if (awb != null)
			{
				BasicInfo.Visible = !string.IsNullOrEmpty(awb.MAWBNumber);

				if (BasicInfo.Visible)
				{
					BasicMAWBNumber.Text = awb.MAWBNumber;
					BasicFlightDate.Text = awb.FlightDate;
					BasicDestination.Text = awb.DestinationPort;
					BasicHAWBNumbers.Text = String.Join(", ", awb.HAWBNumbers);
				}
			}
		}

		bool ProcessUploadedFile(byte[] contents, string fileName)
		{
			DataFile = contents;

			var importer = new AWBImporter();
			var logger = new WebImportLogger();
			var awb = importer.Validate(SiteUser.LoggedInUser, new MemoryStream(DataFile), logger);

			ShowBasicInfo(awb);

			return ShowErrors(logger);
		}

		void SwitchStep(WebTracker.Upload.UploadStep nextStep)
		{
			UploadSection.Visible = nextStep == WebTracker.Upload.UploadStep.ValidateFile;

			switch (nextStep)
			{
				case WebTracker.Upload.UploadStep.ValidateFile:
					NextButton.Text = Res.GetString("3ad31eb3-361f-45c6-ab9a-4b477cc23714", "Validate");
					break;
				case WebTracker.Upload.UploadStep.ImportFile:
					NextButton.Text = Res.GetString("d80d7a82-67c1-475a-8519-714ba6dc1ef1", "Import");
					break;
				case WebTracker.Upload.UploadStep.ShowResults:
					if (!ImportedPK.IsEmpty)
					{
						ZClientScript.RegisterStartupScript(GetType(), RedirectScriptKey, String.Format(RedirectScript, AppInstance.MAWBDetailsPage, ImportedPK));
					}
					break;
				default:
					break;
			}

			CurrentStepValue = nextStep;
		}

		const string RedirectScriptKey = "RedirectScript";

		string RedirectScript
		{
			get
			{
				return (NoResString)@"<SCRIPT TYPE=""text/javascript"">
					window.opener.location='{0}?Ref={1}';
					window.close();
				</SCRIPT>"; // Javascript segment
			}
		}

		#endregion
	}
}
