using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	public partial class CusPermitForm : ZTemplateForm
	{
		public CusPermitForm()
			: base()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				throw new InvalidOperationException("This constructor is only for the designer. Please use the one that takes a business object.");
			}
		}

		public CusPermitForm(BaseCusPermitHeader permitHeader)
			: base(permitHeader)
		{
			InitializeComponent();
			InitializeGridLayout();
			SetInitialVisiblity();
		}

		void SetInitialVisiblity()
		{
			splitContainer1.Panel1Collapsed = !HasPermitRuleCodes;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			HookControlChangeEvents();
		}

		bool HasPermitRuleCodes
		{
			get
			{
				if (hasPermitRuleCodes == null)
				{
					var ruleCodes = BusinessEntity.CountrySpecificInstruction.GetRuleCodeList(BusinessEntity.CPH_Type, BusinessEntity.CPH_SubType);
					hasPermitRuleCodes = ruleCodes == null || ruleCodes.Count != 0;
				}

				return hasPermitRuleCodes.Value;
			}
		}
		bool? hasPermitRuleCodes;

		public override string FormCaption
		{
			get { return Res.GetString("D19E9E95-B7D3-4CFC-A2AA-DED6AAA2120A", "Permit {0} - {1}", BusinessEntity?.PermitHolder?.OH_Code ?? ZString.Empty, BusinessEntity.CPH_Number); }
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			HandleUnitOfMeasureReadOnly(true);
			PermitNumberZTextBox.ReadOnly = true;
			PermitRuleValueFromText.ReadOnly = true;
			PermitRuleValueToZTextBox.ReadOnly = true;
			MainTabPage.SetReadOnlyIncludingChildren();
		}

		protected new BaseCusPermitHeader BusinessEntity
		{
			get { return (BaseCusPermitHeader)base.BusinessEntity; }
		}

		void InitializeGridLayout()
		{
			PermitRuleGrid.AfterBind += PermitRuleGrid_AfterBind;
			PermitTransactionsGrid.AfterBind += PermitTransactionsGrid_AfterBind;

			using (PermitTransactionsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				PermitTransactionsGrid.ReOrderColumns(new string[] { CusPermitLineTransactionSchema.Constants.CPL_TransactionDate });
			}
		}

		void HookControlChangeEvents()
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.TransactionCategoryInfo.ValueChanged += TransactionCategoryInfo_ValueChanged;
				TransactionCategoryInfo_ValueChanged(this, null);

				BusinessEntity.CPH_QtyValIndicatorInfo.ValueChanged += CPH_QtyValIndicatorInfo_ValueChanged;
				CPH_QtyValIndicatorInfo_ValueChanged(this, null);

				BusinessEntity.CPH_IsClosedInfo.ValueChanged += CPH_IsClosedInfo_ValueChanged;
				CPH_IsClosedInfo_ValueChanged(this, null);

				BusinessEntity.CPH_TypeInfo.ValueChanged += CPH_TypeInfo_ValueChanged;
				CPH_TypeInfo_ValueChanged(this, null);
			}
		}

		void UnhookControlChangeEvents()
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.TransactionCategoryInfo.ValueChanged -= TransactionCategoryInfo_ValueChanged;
				BusinessEntity.CPH_QtyValIndicatorInfo.ValueChanged -= CPH_QtyValIndicatorInfo_ValueChanged;
				BusinessEntity.CPH_IsClosedInfo.ValueChanged -= CPH_IsClosedInfo_ValueChanged;
				BusinessEntity.CPH_TypeInfo.ValueChanged -= CPH_TypeInfo_ValueChanged;
			}
		}

		void TransactionCategoryInfo_ValueChanged(object sender, EventArgs e)
		{
			var category = BusinessEntity.TransactionCategory;
			BusinessEntity.CusPermitLineTransactions.AddTransactionCategoryFilter(category);
			var isCUM = BusinessEntity.IsCUM;
			ValueBalanceZCalcEdit.Visible = isCUM && BusinessEntity.IsVAL;
			QuantityBalanceZCalcEdit.Visible = isCUM && BusinessEntity.IsQTY;
		}

		void CPH_QtyValIndicatorInfo_ValueChanged(object sender, EventArgs e)
		{
			var isCUM = BusinessEntity.IsCUM;
			var isVAL = BusinessEntity.IsVAL;
			var isQTY = BusinessEntity.IsQTY;
			ValueBalanceZCalcEdit.Visible = isCUM && isVAL;
			QuantityBalanceZCalcEdit.Visible = isCUM && isQTY;
			CPH_QtyValIndicator_GridColumnVisibility(isVAL, isQTY);
			PositionQuantityBalanceEdit();

			var isTransactionsApplicable = BusinessEntity.IsTransactionsApplicable();
			PermitTransactionsGroupBox.Visible = isTransactionsApplicable;
			HandleUnitOfMeasureVisibility(isTransactionsApplicable && isQTY);
		}

		void CPH_QtyValIndicator_GridColumnVisibility(bool isVAL, bool isQTY)
		{
			PermitTransactionsGrid.SetColumnVisible(isVAL, CusPermitLineTransactionSchema.Constants.CPL_TranValue);
			PermitTransactionsGrid.SetColumnVisible(isQTY, CusPermitLineTransactionSchema.Constants.CPL_TranQty);
		}

		void CPH_IsClosedInfo_ValueChanged(object sender, EventArgs e)
		{
			if (BusinessEntity.CPH_IsClosed)
			{
				SetReadOnlyIncludingChildren();
			}
		}

		void CPH_TypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var permitType = BusinessEntity.CPH_Type;
			var isTransactionsApplicable = BusinessEntity.IsTransactionsApplicable();
			PermitTransactionsGroupBox.Visible = isTransactionsApplicable;

			HandleUnitOfMeasureVisibility(isTransactionsApplicable && BusinessEntity.IsQTY);
			QtyValIndicatorZDropEdit.Visible = BusinessEntity.Lookups.PermitQtyValIndicators.Count > 0;

			var hasACollectionForCodeFindBox = BusinessEntity.Lookups.PermitNumberCollection != null;
			PermitNumberZTextBox.Visible = !hasACollectionForCodeFindBox;
			PermitNumberZCodeFindBox.Visible = hasACollectionForCodeFindBox;

			SetPermitNumberCustomLabelVisibility(permitType);
		}

		void SetPermitNumberCustomLabelVisibility(string permitType)
		{
			var permitNumberCustomLabel = BusinessEntity.CountrySpecificInstruction.GetCustomLabelForPermitNumber(permitType);
			PermitNumberCustomLabel.Visible = !string.IsNullOrEmpty(permitNumberCustomLabel);
			if (PermitNumberCustomLabel.Visible)
			{
				PermitNumberCustomLabel.GetExtension<ILabelCaptionRenderer>().Caption = ZString.Format("({0})", permitNumberCustomLabel);
			}
		}

		void PermitTransactionsGrid_AfterBind(object sender, EventArgs e)
		{
			CPH_QtyValIndicator_GridColumnVisibility(BusinessEntity.IsVAL, BusinessEntity.IsQTY);
		}

		void PositionQuantityBalanceEdit()
		{
			var indicator = BusinessEntity.CPH_QtyValIndicator;
			var quantityBalanceXOffset = 640;
			if (indicator == PermitQtyValIndicatorList.Codes.QTY)
			{
				quantityBalanceXOffset = 420;
			}
			QuantityBalanceZCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(quantityBalanceXOffset, 5, true);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes && BusinessEntity.CusPermitLineTransactions.OfType<BaseCusPermitLineTransaction>().Any(x => !x.IsInDatabase))
			{
				var message = Res.GetString("A8055CB7-05BC-4A4F-B7A6-61AE134DABE2", "Transactions cannot be amended once saved. Do you want to continue saving the transactions?");
				var caption = Res.GetString("32B7FC8B-E6E7-48E2-B2A9-D3632B0C387D", "Warning: Transactions cannot be amended");
				var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				result = dialogResult == DialogResult.Yes ? ContinueWithSave.Yes : ContinueWithSave.No;
			}

			return result;
		}

		void PermitRuleGrid_AfterBind(object sender, EventArgs e)
		{
			PermitRuleGrid.ListManager.PositionChanged += new EventHandler(PermitRuleGridListManager_PositionChanged);
			PermitRuleGridListManager_PositionChanged(null, null);
		}

		void PermitRuleGridListManager_PositionChanged(object sender, EventArgs e)
		{
			UpdateCurrentPermitRule();
			ChangeControlsVisibility();
		}

		void UpdateCurrentPermitRule()
		{
			BaseCusPermitRule permitRule = null;
			var listManager = PermitRuleGrid.ListManager;
			if (listManager != null)
			{
				permitRule = (BaseCusPermitRule)listManager.GetCurrent();
				if (permitRule != null && permitRule.IsDeleted)
				{
					permitRule = null;
				}
			}

			if (currentPermitRule != permitRule)
			{
				UnHookPartPivotEvents(currentPermitRule);
				currentPermitRule = permitRule;
				HookPartPivotEvents(permitRule);
			}
		}

		protected virtual void HookPartPivotEvents(BaseCusPermitRule permitRule)
		{
			if (permitRule != null)
			{
				permitRule.CPR_RuleCodeInfo.ValueChanged += CPR_RuleCode_ValueChanged;
			}
			CPR_RuleCode_ValueChanged(null, null);
		}

		protected virtual void UnHookPartPivotEvents(BaseCusPermitRule permitRule)
		{
			if (permitRule != null)
			{
				permitRule.CPR_RuleCodeInfo.ValueChanged -= CPR_RuleCode_ValueChanged;
			}
		}

		void CPR_RuleCode_ValueChanged(object sender, EventArgs e)
		{
			ChangeControlsVisibility();
		}

		protected void ChangeControlsVisibility()
		{
			var controlsToHide = RuleDetailPanel.Controls.OfType<Control>().Where(x => x.Name.StartsWith("PermitRuleValueFrom", StringComparison.Ordinal));
			foreach (var controlToHide in controlsToHide)
			{
				controlToHide.Visible = false;
			}

			var foundControlToShow = false;
			if (currentPermitRule != null)
			{
				var controlToShow = RuleDetailPanel.Controls.Find("PermitRuleValueFrom" + currentPermitRule.CPR_ValueFrom_FieldType, true)?.FirstOrDefault();
				if (controlToShow != null)
				{
					controlToShow.Visible = true;
					foundControlToShow = true;
				}
			}

			if (!foundControlToShow)
			{
				PermitRuleValueFromText.Visible = true;
			}

			var isRuleExceptionApplicable = currentPermitRule?.PermitHeader?.CountrySpecificInstruction?.IsRuleExceptionApplicable(currentPermitRule?.CPR_RuleCode ?? ZString.Empty) ?? true;
			PermitRuleExceptionGroupBox.Visible = isRuleExceptionApplicable;

			var isSingleValueRule = currentPermitRule?.IsSingleValueRule ?? false;
			PermitRuleValueToZTextBox.Visible = !isSingleValueRule;
			PermitRuleValueFromTextCodeFindBox.ShowDescriptionBox = isSingleValueRule;
			PermitRuleValueFromTextDropEdit.ShowDescriptionBox = isSingleValueRule;
			if (isSingleValueRule)
			{
				PermitRuleValueFromTextCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
				PermitRuleValueFromTextDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			}
			else
			{
				PermitRuleValueFromTextDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 17, true);
				PermitRuleValueFromTextCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 17, true);
			}
		}

		void HandleUnitOfMeasureVisibility(bool visible)
		{
			unitOfMeasureControl.Visible = visible;
		}

		void HandleUnitOfMeasureReadOnly(bool readOnly)
		{
			unitOfMeasureControl.SetReadOnly(readOnly);
		}

		protected virtual Control unitOfMeasureControl => UnitOfMeasureZTextBox;

		protected BaseCusPermitRule currentPermitRule;
	}
}
