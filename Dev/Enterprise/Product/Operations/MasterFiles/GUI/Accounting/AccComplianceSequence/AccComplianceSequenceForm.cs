using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccComplianceSequenceForm : ZForm
	{
		public AccComplianceSequenceForm()
		{
			InitializeComponent();
		}

		public AccComplianceSequenceForm(AccComplianceSequence bO) : base(bO)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			WorkflowTabPage.Initialize(bO);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.Audit);
			CreateMenuItems();
		}

		#region Menu Items

		void CreateMenuItems()
		{
			SplitMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("C4533262-CC95-4B54-8DBE-F70C9DE96752", "Split Compliance Sequence Book"), HandleSplitComplianceSequenceBook);
		}

		void UpdateMenuItems()
		{
			var bizO = BusinessEntity as AccComplianceSequence;
			if (bizO == null)
			{
				return;
			}

			SplitMenuItem.Enabled = bizO.XD_IsActive && bizO.IsInDatabase;

			if (!AccountingMasterFilesRegistry.Instance.EnablePaperStockOptionsToPrintComplianceDocuments.Value)
			{
				MenuGuidFindBox.Enabled = false;
				XD_MaxChargesPerTransactionBoundText.Enabled = false;
				ComplianceRollupTypeDropEdit.Enabled = false;
				PrinterFindBox.Enabled = false;

				var toolTip = Res.GetString("4c9ccd70-1d43-461b-83c7-f7380b0bdebe", @"This field is not available in your login company because it has been disabled by an administrator.
This is controlled by the following registry:
{0}", AccountingMasterFilesRegistry.Instance.EnablePaperStockOptionsToPrintComplianceDocuments.Location());

				MenuGuidFindBox.ManuallySetCaptionToolTip(toolTip);
				XD_MaxChargesPerTransactionBoundText.ManuallySetCaptionToolTip(toolTip);
				ComplianceRollupTypeDropEdit.ManuallySetCaptionToolTip(toolTip);
				PrinterFindBox.ManuallySetCaptionToolTip(toolTip);
			}
		}

		internal ZDropEdit ComplianceNumberFormatDropEdit;
		MenuItem SplitMenuItem;

		void HandleSplitComplianceSequenceBook(object sender, EventArgs e)
		{
			if (!Env.Security.ComplianceSequencesModifySplit.IsAllowed)
			{
				Env.Security.ComplianceSequencesModifySplit.ShowError();
				return;
			}

			var bizO = BusinessEntity as AccComplianceSequence;
			if (bizO == null)
			{
				return;
			}

			if (bizO.HasChanges || !bizO.IsInDatabase)
			{
				Globals.Message.ShowError(Res.GetString("C485831A-1C11-460C-AE85-1D610F947D3D", "Please save this form before splitting compliance sequence book."));
				return;
			}

			if (!bizO.XD_IsActive)
			{
				Globals.Message.ShowError(Res.GetString("2D9C2D34-9D73-4896-ABD0-608159CDF289", "Cannot split inactive compliance sequence book."));
				return;
			}

			var viewModel = new AccComplianceSequenceSplitViewModel(bizO.Factory.CreateNewFactory(), bizO.PK);
			ZFormModaliser.Show(new AccComplianceSequenceSplitForm(viewModel), this);
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateGUIIfBookFull(false);
			UpdateMenuItems();
			UpdateCheckBoxAccessibility();
			Saved += OnSaved;
		}

		void OnSaved(object sender, EventArgs e)
		{
			UpdateMenuItems();
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				var book = (AccComplianceSequence)BusinessEntity;
				if (book.XD_Prefix.IsEmpty && !book.XD_PrefixInfo.ReadOnly)
				{
					string message = Res.GetString("6a1423ab-4ea6-4e1d-9e29-6560e43298f0", @"You have NOT assigned a Series Prefix.
Are you sure you want to save this Compliance Invoice Book?");
					string caption = Res.GetString("17a705fa-90c3-45e7-80ea-272290c5208e", "Warning Message - Prefix");
					var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
					if (dialogResult == System.Windows.Forms.DialogResult.No)
					{
						result = ContinueWithSave.No;
					}
				}

				if (result == ContinueWithSave.Yes)
				{
					if ((book.XD_Calc_EndNumberString.Equals("999999999") || book.Company.GC_RN_NKCountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode && book.XD_Calc_EndNumberString.Equals("99999999")) && !book.XD_Calc_EndNumberStringInfo.ReadOnly)
					{
						string message = Res.GetString("959d0caa-115c-48e1-a1ad-55b83fb4b5f5", @"You have chosen an End Number of {0}.
You cannot edit your End Number once your Compliance Invoice Book has been saved.
If your Compliance Number Series are governed by local Tax Authorities, please ensure you have entered your End Number correctly.
Are you sure you want {0} as your End Number?", book.XD_Calc_EndNumberString);
						string caption = Res.GetString("1851cbed-ffbb-4451-8c7c-b9958e102e72", "Warning Message - End Number");
						var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
						if (dialogResult == System.Windows.Forms.DialogResult.No)
						{
							result = ContinueWithSave.No;
						}
					}
				}

				if (result == ContinueWithSave.Yes && book.IsInFinalisedDate)
				{
					string message = Res.GetString("A7BDF31F-BA39-42DC-8600-2E6B79E2259B", @"The Valid From and Expiry Date falls in a compliance report that has been finalized.
If you proceed to save, both dates will be locked and not editable. Further, number sequence will not be allocated from this sequence book.
Are you sure you want to save this Compliance Invoice Book?");
					string caption = Res.GetString("D4191FC4-BD2A-48F9-AB2C-F77181533420", "Warning Message - Valid From Expiry Date");
					var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
					if (dialogResult == System.Windows.Forms.DialogResult.No)
					{
						result = ContinueWithSave.No;
					}
				}
			}
			return result;
		}

		public override string FormVerb
		{
			get
			{
				string verb = base.FormVerb;

				if (BusinessEntityForHasChanges != null && DisplayMode == ODisplayMode.Delete)
				{
					verb = Res.GetString("f92f5bfc-847d-4df8-a8f4-c7f970628594", "Deactivate");
				}
				return verb;
			}
		}

		public void UpdateGUIIfBookFull(bool isInvokedFromVoidingForm)
		{
			ZDecimal nextNo;
			ZDecimal endNo;
			if (ZDecimal.TryParse(NextNoTextBox.Text, out nextNo) && ZDecimal.TryParse(EndNoTextBox.Text, out endNo))
			{
				if (nextNo - 1 >= endNo)
				{
					NextNoTextBox.Visible = false;
					numberFullLabel.Visible = true;
					VoidingSequenceButton.Enabled = false;
					if (isInvokedFromVoidingForm)
					{
						XD_ExpiryDateEdit.DateTimeValue = ZDateTime.Now;
					}
				}
			}
		}

		void IsActiveCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!IsActiveCheckBox.Checked && !AccountingMasterFilesRegistry.Instance.AllowReActivationOfInactiveComplianceBook.Value)
			{
				var confirmationDialogResult = DialogResult.OK == Globals.Message.ShowConfirmation(
					Res.GetString("e37061d3-bbfd-46f6-a265-a27ae16abe4c", @"You are about to flag a compliance book as inactive. 
This action cannot be reversed. Once Inactive, a compliance book cannot be activated again and you will not be able to assign compliance numbers from this book.
This is controlled by this registry: {0}.
Do you want to proceed?", AccountingMasterFilesRegistry.Instance.AllowReActivationOfInactiveComplianceBook.Location()),
					Res.GetString("2edba787-bd67-4746-b968-9b158278204f", "Compliance book deactivation confirmation"),
					Res.GetString("a70ceac3-0668-412b-a709-8ccaf58fc0da", "Yes"),
					MessageBoxIcon.Question);

				IsActiveCheckBox.Checked = !confirmationDialogResult;
				IsActiveCheckBox.Enabled = IsActiveCheckBox.Checked;
			}
		}

		public IComplianceSequencePresentationProvider PresentationProvider => presentationProvider ?? (presentationProvider = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetComplianceSequencePresentationProvider());
		IComplianceSequencePresentationProvider presentationProvider;

		void UpdateCheckBoxAccessibility()
		{
			IsActiveCheckBox.Enabled = string.IsNullOrEmpty(PresentationProvider.CanReactivate(BusinessEntity as AccComplianceSequence)) || IsActiveCheckBox.Checked;
		}

		internal void VoidingSequenceButton_Click(object sender, EventArgs e)
		{
			if (CanNotVoiding)
			{
				Globals.Message.ShowError(Res.GetString("68A32D48-7073-48EA-9725-50B7C60990AE", "Voiding of Sequence No. is not allowed for 'TXE' and 'TCE' sub type."));
				return;
			}

			VoidingSequenceNumberBusinessObject bo = new VoidingSequenceNumberBusinessObject((AccComplianceSequence)BusinessEntity);
			VoidingSequenceForm form = new VoidingSequenceForm(bo, this);
			ZFormModaliser.Show(form, this);
		}

		ZBool CanNotVoiding
		{
			get
			{
				var electronicComplianceSubTypes = new[] { TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE };

				return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan &&
					electronicComplianceSubTypes.Contains(((AccComplianceSequence)BusinessEntity).XD_SequenceClass.ToString());
			}
		}
	}
}
