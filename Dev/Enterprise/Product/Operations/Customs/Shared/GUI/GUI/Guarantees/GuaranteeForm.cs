using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Guarantees;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class GuaranteeForm : ZTemplateForm
	{
		public GuaranteeForm(BaseCusGuaranteeHeader guaranteeHeader, GuaranteeTransactionFilterStripBusinessObject filterStripBusinessObject = null)
			: base(guaranteeHeader)
		{
			if (filterStripBusinessObject == null)
			{
				filterStripBusinessObject = new GuaranteeTransactionFilterStripBusinessObject();
			}
			guaranteeHeader.CPH_TypeInfo.ValueChanged -= CPH_TypeInfo_ValueChanged;
			guaranteeHeader.CPH_TypeInfo.ValueChanged += CPH_TypeInfo_ValueChanged;
			nPBOTransactionLine = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);
			BindingSource.DataSourceType = typeof(BaseCusGuaranteeHeader);
			InitializeComponent();
			SetProgressBar(guaranteeHeader.CPH_Calc_UsedBalance, guaranteeHeader.CPH_Calc_OpeningBalance);
			SetFilterControl(filterStripBusinessObject);
			SetNewTransactionControlsBinding();
			ShowOrHideAdditionalReferencesTab();
			ShowOrHideMessagesTab();
			AddMessagingMenuItem();
		}

		void ShowOrHideMessagesTab()
		{
			GuaranteeMessagesTabPage.TabVisible = BusinessEntity?.SupportsMessages ?? false;
		}

		void AddMessagingMenuItem()
		{
			if (BusinessEntity is BaseCusGuaranteeHeader businessEntity && businessEntity.SupportsMessages)
			{
				var messagingMenuItem = new GuaranteeMessagingMenuItem(businessEntity);
				messagingMenuItem.Name = GuaranteeMessagingMenuItemName;
				MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), messagingMenuItem);
			}
		}
		const string GuaranteeMessagingMenuItemName = nameof(GuaranteeMessagingMenuItem);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (BusinessEntity is BaseCusGuaranteeHeader businessEntity)
				{
					businessEntity.CPH_TypeInfo.ValueChanged -= CPH_TypeInfo_ValueChanged;
					businessEntity.UnlockMutex();
				}
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		void ShowOrHideAdditionalReferencesTab()
		{
			AdditionalReferencesTabPage.TabVisible = BusinessEntity?.SupportsAdditionalCustomsReferences ?? false;
		}

		void CPH_TypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideAdditionalReferencesTab();
		}

		void SetNewTransactionControlsBinding()
		{
			NewTransactionAmountZCalcEdit.SetDataBinding(nPBOTransactionLine, NPBOCusGuaranteeLineTransaction.Schema.CPL_TranValue);
			NewTransactionDateZDateEdit.SetDataBinding(nPBOTransactionLine, NPBOCusGuaranteeLineTransaction.Schema.CPL_TransactionDate);
			NewTransactionTypeZDropEdit.SetDataBinding(nPBOTransactionLine, NPBOCusGuaranteeLineTransaction.Schema.CPL_TransactionType);
			NewTransactionCommentZTextBox.SetDataBinding(nPBOTransactionLine, NPBOCusGuaranteeLineTransaction.Schema.CPL_Comment);
			NewTransactionReferenceZTextBox.SetDataBinding(nPBOTransactionLine, NPBOCusGuaranteeLineTransaction.Schema.CPL_Reference);
		}

		void SetFilterControl(GuaranteeTransactionFilterStripBusinessObject filterStripBusinessObject)
		{
			var gridCollection = BusinessEntity.CusGuaranteeLineTransactions;
			var transactionFilterControl = CreateNewGuaranteeTransactionFilterControl(gridCollection, filterStripBusinessObject);
			transactionFilterControl.Name = "GuaranteeTransactionFilterControl";
			transactionFilterControl.ShouldRunSearchOnStripsInitialized = true;
			transactionFilterControl.Dock = DockStyle.Fill;
			zPanel2.Controls.Add(transactionFilterControl);
		}

		protected virtual GuaranteeTransactionFilterControl CreateNewGuaranteeTransactionFilterControl(CusGuaranteeLineTransactionCollection cusGuaranteeLineTransactions, GuaranteeTransactionFilterStripBusinessObject guaranteeTransactionFilterStripBusinessObject) => new GuaranteeTransactionFilterControl(cusGuaranteeLineTransactions, guaranteeTransactionFilterStripBusinessObject);

		public void SetProgressBar(ZDecimal balance, ZDecimal openingBalance)
		{
			if (!openingBalance.IsEmpty)
			{
				ZDecimal percentValue = balance / openingBalance * 100;
				PercentValueCustomLabel.Text = percentValue.Truncate(2).ToString("F2", CultureInfo.CurrentCulture) + " %";
				PercentValueCustomLabel.Visible = true;
				if (percentValue >= 0 && percentValue < 50)
				{
					ProgressBar.Value = (int)percentValue;
					ProgressBar.SetForeGroundColor(System.Drawing.Color.Green);
				}
				else if (percentValue >= 50 && percentValue < 80)
				{
					ProgressBar.Value = (int)percentValue;
					ProgressBar.SetForeGroundColor(System.Drawing.Color.Orange);
				}
				else if (percentValue <= 100 && percentValue >= 80)
				{
					ProgressBar.Value = (int)percentValue;
					ProgressBar.SetForeGroundColor(System.Drawing.Color.Red);
				}
				else if (percentValue > 100)
				{
					ProgressBar.Value = 100;
					ProgressBar.SetForeGroundColor(System.Drawing.Color.Red);
					PercentValueCustomLabel.Text = Res.GetString("DCB2475B-E125-4505-8BB6-445E471067A9", "more than 100 %");
				}
				else if (percentValue < 0)
				{
					ProgressBar.Value = 0;
					ProgressBar.SetForeGroundColor(System.Drawing.Color.Green);
					PercentValueCustomLabel.Text = Res.GetString("3008E6A2-B15C-43C2-8C28-89A6A3650E18", "less than 0 %");
				}
			}
		}

		public override string FormCaption => Res.GetString("4DD83B5C-BC4C-4940-8327-333B1CC6B24B", "Guarantee {0} - {1}", BusinessEntity.PermitHolder?.OH_Code, BusinessEntity.CPH_Number);

		protected new BaseCusGuaranteeHeader BusinessEntity => (BaseCusGuaranteeHeader)base.BusinessEntity;

		protected void NewTransactionAddButton_Click(object sender, EventArgs e)
		{
			ValidateNewTransaction();
			if (!nPBOTransactionLine.HasNotifications())
			{
				var message = Res.GetString("A8055CB7-05BC-4A4F-B7A6-61AE134DABE2", "Transactions cannot be amended once saved. Do you want to continue saving the transactions?");
				var messageOBA = Res.GetString("A712E602-247B-48C4-8F8D-D37870404A49", "An opening balance adjustment transaction (OBA) is a rare edit made to the overall size of the guarantee. Its amount must be incorporated into the overall balance by the execution of the service task. The revised overall balance will not be adjusted until the service task next runs, and importantly will not be re-calculated in your user session (this form). This is to remove any risk of concurrency. Once this transaction is saved, please close the form and re-open it once the task has executed (which under normal operating conditions will be almost immediately).");
				var caption = Res.GetString("32B7FC8B-E6E7-48E2-B2A9-D3632B0C387D", "Warning: Transactions cannot be amended");

				if (Globals.Message.Show(nPBOTransactionLine.CPL_TransactionType == GuaranteeTransactionTypeList.Codes.OBA ? $"{message}{System.Environment.NewLine}{System.Environment.NewLine}{messageOBA}" : message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					BusinessEntity.DoActionWithMutexLock(() =>
					{
						var newTransaction = AddNewTransaction();
						if (newTransaction != null)
						{
							if (FireSaveButton() == ContinueWithSave.Yes)
							{
								SetProgressBar(BusinessEntity.CPH_Calc_UsedBalance, BusinessEntity.CPH_Calc_OpeningBalance);
								ClearNewTransactionValuesOnUI();
							}
							else
							{
								newTransaction.Delete();
								BusinessEntity.UnlockMutex();
							}
						}
					}, x =>
					{
						Globals.Message.ShowWarning(Res.GetString("799d3a29-b3f0-4150-83bf-bd945b30aebc", "Guarantee transactions are currently being edited by user '{0}'", BusinessEntity.Mutex.GetLockInfo()), Res.GetString("c2e1c8b4-f3c9-496a-87eb-dc7e26f71035", "Warning"));
					});
				}
			}
		}

		void ClearNewTransactionValuesOnUI()
		{
			using (nPBOTransactionLine.GetValidationSuspender())
			{
				SetNewTransactionLineValues(ZDecimal.Zero, ZString.Empty, ZString.Empty, ZString.Empty);
				nPBOTransactionLine.CPL_TransactionDate = ZDateTime.Empty;
			}
		}

		void ValidateNewTransaction()
		{
			nPBOTransactionLine.Validation.ValidateAll();
			nPBOTransactionLine.CPL_TranValueInfo.RefreshBinding();
			nPBOTransactionLine.CPL_TransactionTypeInfo.RefreshBinding();
			nPBOTransactionLine.CPL_TransactionDateInfo.RefreshBinding();
			nPBOTransactionLine.CPL_CommentInfo.RefreshBinding();
			nPBOTransactionLine.CPL_ReferenceInfo.RefreshBinding();
		}

		SharedCusPermitLineTransaction AddNewTransaction()
		{
			var newTransaction = BusinessEntity.AddTransaction(nPBOTransactionLine.CPL_Reference, nPBOTransactionLine.CPL_Comment, ZString.Empty, ZString.Empty, nPBOTransactionLine.CPL_TranValue, ZDecimal.Zero, transactionType: nPBOTransactionLine.CPL_TransactionType, transactionDate: nPBOTransactionLine.CPL_TransactionDate, notifier: ShowAddNewTransactionError);
			if (newTransaction != null)
			{
				if (nPBOTransactionLine.CPL_TransactionType == PermitTransactionTypeList.Codes.OBL)
				{
					BusinessEntity.CPH_Balance += nPBOTransactionLine.CPL_TranValue;
				}
				newTransaction.CPL_TransactionDate = nPBOTransactionLine.CPL_TransactionDate;
			}

			return newTransaction;
		}

		void ShowAddNewTransactionError(ZString message, ZDecimal balance)
		{
			var caption = Res.GetString("4093B2CA-F874-46B8-B3DF-712F8618A6FA", "Unable to add new transaction");
			Globals.Message.ShowError(message, caption);
		}

		public void SetNewTransactionLineValues(ZDecimal tranValue, ZString transactionType, ZString comment, ZString reference)
		{
			nPBOTransactionLine.CPL_TranValue = tranValue;
			nPBOTransactionLine.CPL_TransactionType = transactionType;
			nPBOTransactionLine.CPL_TransactionDate = ZDateTime.Now;
			nPBOTransactionLine.CPL_Comment = comment;
			nPBOTransactionLine.CPL_Reference = reference;
		}

		public BaseCusGuaranteeHeader GetFormGuaranteeHeader() => BusinessEntity;

		public NPBOCusGuaranteeLineTransaction GetNewTransactionLineNPBO() => nPBOTransactionLine;
		readonly NPBOCusGuaranteeLineTransaction nPBOTransactionLine;
	}
}
