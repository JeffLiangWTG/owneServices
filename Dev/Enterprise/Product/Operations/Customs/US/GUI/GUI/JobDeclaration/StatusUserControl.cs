using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class StatusUserControl : ZUserControl
	{
		readonly MenuItem markAsClosedEntryPGAStatus;
		public StatusUserControl()
		{
			InitializeComponent();

			foreach (ZTabPage tabPage in ImportStatusTabControl.TabPages)
			{
				tabPage.ExcludeFromBindingOnSave = false;
			}
			InitializeGridLayoutCore();
			origCRLTransmitCountCaptionResourceString = CRLTransmitCountTextBox.CaptionResourceString;
			origENSTransmitCountCaptionResourceString = ENSTransmitCountTextBox.CaptionResourceString;

			markAsClosedEntryPGAStatus = new ZMenuItem("Mark as Closed", new EventHandler(ClosedPGAEntryStatus_Click));
			var actionsContextMenu = EntryPGAStatusGrid.ContextMenu;
			actionsContextMenu.MenuItems.Add(actionsContextMenu.MenuItems.Count, markAsClosedEntryPGAStatus);

			CRInformationExistsLabel.AllowOutsideOfParent();
			liquidationDetailsUserControl.AllowOutsideOfParent();
		}

		void ClosedPGAEntryStatus_Click(object sender, EventArgs e)
		{
			if (!Env.Security.USMarkAsClosedForPGAEntryLineStatus.IsAllowed)
			{
				Env.Security.USMarkAsClosedForPGAEntryLineStatus.ShowError();
			}
			else
			{
				var declaration = (JobDeclaration)this.DataSource;
				var parentForm = this.ParentForm as ZForm;

				var shouldProceed = true;
				if (declaration.HasChanges)
				{
					if (Globals.Message.Show(Res.GetString("F0128FB6-8B7D-44D7-A745-C113C7E64311", "The Job has not yet been saved. Do you want to save and proceed?"),
						Res.GetString("B9BDEFBE-360C-4E1C-B423-A4DFC114E8B6", "Save Job"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes)
					{
						if (parentForm != null)
						{
							shouldProceed = parentForm.FireSaveButton() == ContinueWithSave.Yes;
						}
					}
					else
					{
						shouldProceed = false;
					}
				}

				if (shouldProceed)
				{
					if (EntryPGAStatusGrid.SelectedElements.Length > 0)
					{
						var cusDispositions = EntryPGAStatusGrid.SelectedElements.Cast<CusDisposition>().ToArray();
						if (cusDispositions.Any(x => PGADispositionCodeList.IsFinalCode(x.CDI_Status)))
						{
							Globals.Message.ShowWarning("You have selected lines which have a disposition of either 'MC' or '07'. You cannot mark it as closed because its status is already closed.", "Mark As Closed");
							return;
						}

						if (!declaration.DispositionCodes.Cast<DispositionData>().Any(x => x.US_Code == PGADispositionCodeList.OneUSGCode))
						{
							shouldProceed = false;
							if (Globals.Message.Show(
								Res.GetString("8430E4C4-8CB4-4187-83B0-BF6F92750983",
									"ONE USG has not been issued yet. Are you sure you wish to mark as closed now?"),
								Res.GetString("BDBD46AD-A942-4FDC-AA64-AE33A02AB2B9", "Mark As Closed"), MessageBoxButtons.YesNo,
								MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes)
							{
								shouldProceed = true;
							}
						}

						if (shouldProceed)
						{
							var firstDisposition = cusDispositions.FirstOrDefault();
							if (firstDisposition != null)
							{
								using (var noteStatusForm = new ClosePGAEntryStatusForm(firstDisposition))
								{
									if (ZFormModaliser.ShowDialogWithoutDispose(noteStatusForm, this.ParentForm) == DialogResult.OK)
									{
										cusDispositions.ForEach(x =>
										{
											x.CDI_Notes = firstDisposition.CDI_Notes;
											x.CloseStatus();
										});
										parentForm?.FireSaveButton();
									}
								}
							}
						}
					}
				}
			}
		}

		readonly ResourceStringData origCRLTransmitCountCaptionResourceString;
		readonly ResourceStringData origENSTransmitCountCaptionResourceString;

		void InitializeGridLayoutCore()
		{
			DispositionGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			StatementPaymentDateDateEdit.ReadOnly = true;

			RefGrid.ContextMenu.Popup += new EventHandler(ContextMenuOnRefGrid_Popup);
		}

		void ContextMenuOnRefGrid_Popup(object sender, EventArgs e)
		{
			RefreshContextMenuOnRefGrid();
		}

		internal void RefreshContextMenuOnRefGrid()
		{
			if (writToLogPerComments != null)
			{
				MQEDIMessage message = (MQEDIMessage)GetCurrentEDIMessageForRefGrid();
				writToLogPerComments.Enabled = message != null && !message.ActionAuthorised;
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			JobDeclaration declaration = (JobDeclaration)this.DataSource;
			if (declaration != null)
			{
				declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
				declaration.US_EnableCRLInfo.ValueChanged -= IsSimplifiedEntry_ValueChanged;
				declaration.JE_ApplicationCodeInfo.ValueChanged -= IsSimplifiedEntry_ValueChanged;
				declaration.US_CargoReleaseTypeInfo.ValueChanged -= IsSimplifiedEntry_ValueChanged;
				declaration.US_PGAReplaceUpdateNeededInfo.ValueChanged -= US_PGADataReplacementUpdateRequired_ValueChanged;
			}
		}

		void IsSimplifiedEntry_ValueChanged(object sender, EventArgs e)
		{
			JobDeclaration declaration = (JobDeclaration)this.DataSource;

			ChangSimplifiedEntryControlVisibility(declaration);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
			{
				var declaration = (JobDeclaration)this.DataSource;
				if (declaration != null)
				{
					declaration.FDAMsgStatusInfo.RefreshBinding();
					if (declaration.IsACE)
					{
						var cRLTransmitCountCaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0DAF4B14-8ADF-4BC1-8FEE-4D0CF4417BFD", "CRL Sent Count", $"{Form3461NameForACE} Message Sent Count", "");
						var eNSTransmitCountCaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("062EF057-9C6C-420F-A7CF-89CDEEB734F0", "ENS Sent Count", $"{Form7501NameForACE} Messages Sent Count", "");

						CRLTabPage.Text = Form3461NameForACE + " Status";
						ENSStatusTabPage.Text = Form7501NameForACE + " Errors";
						StatusNotificationsTabPage.Text = Form7501NameForACE + " Status Notifications";
						ENSStatusGroupBox.Text = Form7501NameForACE + " Status";
						CRLTransmitCountTextBox.CaptionResourceString = cRLTransmitCountCaptionResourceString;
						CRLTransmitCountTextBox.GetExtension<ILabelCaptionRenderer>().Caption = cRLTransmitCountCaptionResourceString.ShortCaption;
						ENSTransmitCountTextBox.CaptionResourceString = eNSTransmitCountCaptionResourceString;
						ENSTransmitCountTextBox.GetExtension<ILabelCaptionRenderer>().Caption = eNSTransmitCountCaptionResourceString.ShortCaption;
						BillHodsOrExamLabel.Visible = declaration.IsACECargoCertificationMode && declaration.Bills.Cast<Business.Bill>().Any(x => x.HLDOrEXMStatus == "Y");
					}
					else
					{
						CRLTabPage.Text = Form3461Name + " Status";
						ENSStatusTabPage.Text = Form7501Name + " Errors";
						StatusNotificationsTabPage.Text = Form7501Name + " Status Notifications";
						ENSStatusGroupBox.Text = Form7501Name + " Status";
						BillHodsOrExamLabel.Visible = false;
						CRLTransmitCountTextBox.CaptionResourceString = origCRLTransmitCountCaptionResourceString;
						CRLTransmitCountTextBox.GetExtension<ILabelCaptionRenderer>().Caption = origCRLTransmitCountCaptionResourceString.ShortCaption;
						ENSTransmitCountTextBox.CaptionResourceString = origENSTransmitCountCaptionResourceString;
						ENSTransmitCountTextBox.GetExtension<ILabelCaptionRenderer>().Caption = origENSTransmitCountCaptionResourceString.ShortCaption;
					}
				}
			}
		}

		public const string Form7501NameForACE = "Entry Summary";
		public const string Form7501NameForACE_Short = "Ent Sum";
		public const string Form3461NameForACE = "Cargo Release";
		public const string Form3461NameForACE_Short = "Cargo Rel";
		public const string Form7501Name = "7501";
		public const string Form3461Name = "3461";

		void ChangSimplifiedEntryControlVisibility(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				StatusNotificationsTabPage.TabVisible = declaration.IsACE;

				var isACECargoCertificationMode = declaration.IsACECargoCertificationMode;

				if (!isACECargoCertificationMode)
				{
					BOLL7StatusGrid.RemoveFromAvailableColumns(ErrorsRecord.Schema.ActionIDNumber, ErrorsRecord.Schema.StatusDate);
					BOLL7GroupBox.Text = BLUStatusGroupBox.Text;
				}
				else
				{
					BOLL7StatusGrid.AddToAvailableColumns(ErrorsRecord.Schema.ActionIDNumber, ErrorsRecord.Schema.StatusDate);
					BOLL7GroupBox.Text = SEBOLL7StatusGridCaption;
				}

				FDAStatusTextBox.Visible = !isACECargoCertificationMode;
				FDAMsgStatusDropEdit.Visible = !isACECargoCertificationMode;
				EntryPGAStatusGroupBox.Visible = isACECargoCertificationMode;
				PGACorrectionStatusDropEdit.Visible = isACECargoCertificationMode;
				QuotaStatusDropEdit.Visible = isACECargoCertificationMode;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			RefGrid.ColourDeciding -= new EventHandler<ZArchitecture.ColourDecidingEventArgs>(RegGrid_ColourDeciding);

			if (writToLogPerComments != null)
			{
				RefGrid.ContextMenu.MenuItems.Remove(writToLogPerComments);
				writToLogPerComments = null;
			}

			if (this.DataSource != null)
			{
				JobDeclaration declaration = (JobDeclaration)this.DataSource;

				declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
				declaration.US_EnableCRLInfo.ValueChanged += IsSimplifiedEntry_ValueChanged;
				declaration.JE_ApplicationCodeInfo.ValueChanged += IsSimplifiedEntry_ValueChanged;
				declaration.US_CargoReleaseTypeInfo.ValueChanged += IsSimplifiedEntry_ValueChanged;
				declaration.US_PGAReplaceUpdateNeededInfo.ValueChanged += US_PGADataReplacementUpdateRequired_ValueChanged;

				ChangeJE_MessageTypeControlVisibility(declaration);
				ChangSimplifiedEntryControlVisibility(declaration);
				ChangePGADataReplacementUpdateRequiredLabelVisibility(declaration);

				RefGrid.ColourDeciding += new EventHandler<ZArchitecture.ColourDecidingEventArgs>(RegGrid_ColourDeciding);

				writToLogPerComments = new WriteToLogMenuItem((IStmALogParent)declaration.Shipment ?? declaration, GetCurrentEDIMessageForRefGrid, Env.Security.CustomsDeclarationEnquiryEdit, AcknowledgeActionMenuItemCaption, new ZArchitecture.Business.Internal.BusinessObjectLoggerOptions("", false, Enterprise.ZArchitecture.Business.Events.Authorised));
				writToLogPerComments.Logged += WritToLogPerComments_Logged;
				RefGrid.ContextMenu.MenuItems.Add(0, writToLogPerComments);
			}
		}

		void WritToLogPerComments_Logged(object sender, LoggedEventArgs e)
		{
			if (e.Succeeded)
			{
				(DataSource as JobDeclaration)?.ReCalculateCRLAction();
			}
		}

		void ChangeJE_MessageTypeControlVisibility(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				var isFTZ = declaration.IsFTZAdmission;

				ConsolidatedStatusTabPage.TabVisible = !isFTZ;
				CRLTabPage.TabVisible = !isFTZ;
				ENSStatusTabPage.TabVisible = !isFTZ;
				ITErrorsTabPage.TabVisible = !isFTZ;
				ElectronicInvoiceTabPage.TabVisible = !isFTZ;
				OGATabPage.TabVisible = !isFTZ;
				LiquidationsTabPage.TabVisible = !isFTZ;
				StatusNotificationsTabPage.TabVisible = !isFTZ && declaration.IsACE;
				FTZSummaryTabPage.TabVisible = isFTZ;

				EntryFilerCodeTextBox.Visible = !isFTZ;
				DashLabel.Visible = !isFTZ;
				ImportEntryNumberTextBox.Visible = !isFTZ;
				ITEntryNoTextBox.Visible = !isFTZ;
			}
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			JobDeclaration declaration = (JobDeclaration)this.DataSource;

			ChangeJE_MessageTypeControlVisibility(declaration);
		}
		internal const string SEBOLL7StatusGridCaption = "Bill of Lading Status";
		public const string AcknowledgeActionMenuItemCaption = "Acknowledge Comments";
		WriteToLogMenuItem writToLogPerComments;

		void US_PGADataReplacementUpdateRequired_ValueChanged(object sender, EventArgs e)
		{
			JobDeclaration declaration = (JobDeclaration)this.DataSource;
			ChangePGADataReplacementUpdateRequiredLabelVisibility(declaration);
		}

		void ChangePGADataReplacementUpdateRequiredLabelVisibility(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				PGADataReplacementUpdateRequiredLabel.Visible = declaration.US_PGAReplaceUpdateNeeded == YesNoList.Codes.Yes;
			}
		}

		IStmALogParent GetCurrentEDIMessageForRefGrid()
		{
			return entrySummaryStatusNotificationsUserControl.GetEDIMessage(RefGrid.ListManager);
		}

		void RegGrid_ColourDeciding(object sender, ZArchitecture.ColourDecidingEventArgs e)
		{
			e.Colour = ((ErrorsRecord)e.ObjectAtRow).IsFurtherActionRequiredOnAcknowledged ? System.Drawing.Color.Red : e.ReadOnlyColour;
		}

		void StatusNotificationsGrid_ColourDeciding(object sender, ZArchitecture.ColourDecidingEventArgs e)
		{
			e.Colour = ((ErrorsRecord)e.ObjectAtRow).IsFurtherActionRequiredButNotActionedYet ? System.Drawing.Color.Red : e.ReadOnlyColour;
		}
	}
}
