using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.ZArchitecture.Environment;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class DeclarationForm : BaseJobDeclarationForm
	{
		public DeclarationForm(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public DeclarationForm()
			: base()
		{
		}

		protected override void SetVisibleCore(bool value)
		{
			ZString errorMessage = ZString.Empty;

			if (value && ShouldInformVersionDifference && !UniversalTariffHelper.UseRefDatabaseData)
			{
				var loader = new NZCTariffVersionLoader(Declaration.Factory);
				errorMessage = loader.ErrorMessage;
			}

			if (!errorMessage.IsEmpty)
			{
				Close();
				Globals.Message.ShowError(errorMessage);
			}
			else
			{
				base.SetVisibleCore(value);
			}
		}

		protected virtual bool ShouldInformVersionDifference => true;

		#region GetNewTopLevelMenu
		protected override IEDIMenu GetNewTopLevelMenuCore()
		{
			return new NZEDIMenu();
		}
		#endregion

		#region GetBrokerageUserControl
		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl()
		{
			return new CustomsBrokerageUserControl();
		}
		#endregion

		#region Declaration

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		#endregion

		#region FormCaption
		protected override string FormCaptionCore
		{
			get { return Declaration != null && Declaration.IsECIWriteoff ? "ECI Write-Off" : base.FormCaptionCore; }
		}
		#endregion

		#region InitialiseForm
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
		#endregion

		#region Dispose
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		private readonly System.ComponentModel.Container components;
		#endregion

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var continueSave = base.ShowPreSaveDialogs();

			if (continueSave == ContinueWithSave.Yes)
			{
				var unsentMessageChangeSupport = new UnsentMessageChangeSupport();
				continueSave = unsentMessageChangeSupport.CheckForHeldMessageChangesAndPerformUserAction(Declaration, this);
			}

			if (continueSave == ContinueWithSave.Yes)
			{
				if (Declaration.HasDuplicatedActiveEntryHeader())
				{
					Globals.Message.ShowWarning(
					Res.GetString("07752239-665A-4410-B27F-A852A9CCBE97", "Another user has already created the entry header for the declaration.\r\nThe system will now try to combine your changes with those of the other user.\r\nPlease review the entry header after saved the form."),
					"Duplicated Entry Header");
				}
			}

			return continueSave;
		}
	}
}


