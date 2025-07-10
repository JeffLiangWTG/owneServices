using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class DeclarationOtherDetailsUserControl : BaseCustomsEntryUserControl
	{
		public DeclarationOtherDetailsUserControl()
		{
			InitializeComponent();
			LabelCaptionRenderProvider.SetLabelCaptionVisible(UCRNumberTextBox, false);
		}

		public new JobDeclaration CurrentDataItem => (JobDeclaration)base.CurrentDataItem;
		CusEntryInstruction currentEntryInstruction => CurrentDataItem?.CusEntryInstruction;

		ZBool IsImport => CurrentDataItem?.IsImport ?? ZBool.False;

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			SplitMarkCheckBox.Visible = IsImport;
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var isImport = IsImport;
			DocumentNumbersUserControl.Visible = !isImport;
			CEI_WaiverOfExemptionCheckBox.Visible = isImport;
			CEI_PrintDutyMemoCheckBox.Visible = isImport;
			CEI_DaysOfDelayedDeclarationCalcEdit.Visible = isImport;
			ExaminationDetailsGroupBox.Visible = isImport;
			ExaminationDetailsPanel.Visible = isImport;
			TW_ICIExamTimeDateEdit.Visible = isImport;
			ICIExamLocationDropEdit.Visible = isImport;
			BankAccountTextBox.Visible = isImport;
			JE_DefermentAccountNumberDropEdit.Visible = isImport;
			RORPaymentMethodLDropEdit.Visible = isImport;

			if (isImport)
			{
				JE_CustomsOfficeDropEdit.CaptionResourceString = Res.GetData("B02CF835-EFF2-4424-9151-E4F8A8641A6E", "Unlading Office", "", "Office of Unlading", "The Office of Unlading of the declaration. It's used to generate the third and fourth digits of the entry number.");
				CEI_GoodsLocationFindBox.CaptionResourceString = Res.GetData("131469f0-fdee-48ec-a24c-bc3622849848", "Receipt Location", "The receipt location of the Office of Receipt.");
			}
			else
			{
				JE_CustomsOfficeDropEdit.CaptionResourceString = Res.GetData("48B9087C-F3C3-4B3A-9884-4207E66A2280", "Lading Office", "", "Office of Lading", "The Office of Lading of the declaration. It's used to generate the third and fourth digits of the entry number.");
				CEI_GoodsLocationFindBox.CaptionResourceString = Res.GetData("c67d4bc4-afb0-4bd8-869f-bd1a968506a9", "Export Location", "The receipt location of the Office of Receipt.");
			}
			JE_CustomsOfficeDropEdit.UpdateCaption();
			CEI_GoodsLocationFindBox.UpdateCaption();

			JE_LocationOfGoodsCodeFindBox.Visible = !isImport;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible && currentEntryInstruction != null)
			{
				currentEntryInstruction.TW_ICIExamLocationInfo.RefreshBinding();
				currentEntryInstruction.TW_ICIExamTimeInfo.RefreshBinding();
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource is JobDeclaration declaration)
			{
				declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
				declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
				JE_MessageTypeInfo_ValueChanged(null, null);

				declaration.JE_DateAtFinalDestinationInfo.ValueChanged -= JE_DateAtFinalDestinationInfo_ValueChanged;
				declaration.JE_DateAtFinalDestinationInfo.ValueChanged += JE_DateAtFinalDestinationInfo_ValueChanged;

				var entryInstruction = declaration.CusEntryInstruction;
				if (entryInstruction != null)
				{
					entryInstruction.CEI_DateForDutyInfo.ValueChanged -= CEI_DateForDutyInfo_ValueChanged;
					entryInstruction.CEI_DateForDutyInfo.ValueChanged += CEI_DateForDutyInfo_ValueChanged;
					entryInstruction.CEI_RORPaymentMethodInfo.ValueChanged -= CEI_RORPaymentMethodInfo_ValueChanged;
					entryInstruction.CEI_RORPaymentMethodInfo.ValueChanged += CEI_RORPaymentMethodInfo_ValueChanged;
				}

				if (declaration.IsDataSyncFromShipment && !declaration.IsInDatabase)
				{
					CalculateDaysOfDelayedDeclarationIfRequired();
				}
			}
		}

		void CEI_DateForDutyInfo_ValueChanged(object sender, EventArgs e)
		{
			CalculateDaysOfDelayedDeclarationIfRequired();
		}

		void CEI_RORPaymentMethodInfo_ValueChanged(object sender, EventArgs e)
		{
			if (sender is CusEntryInstruction entryInstruction && entryInstruction.JobDeclaration is JobDeclaration declaration && !entryInstruction.CEI_RORPaymentMethod.IsEmpty)
			{
				var rorPaymentMethod = entryInstruction.CEI_RORPaymentMethod;
				var rorLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Where(invoiceLine => invoiceLine.IsROR).ToList();
				if (rorLines.Count > 0 && Globals.Message.Show(Res.GetString("6F97BFF2-1A8F-4A82-892A-49205B68030C", "Do you want to update all ROR lines to be use same '{0}' payment method?", rorPaymentMethod), Res.GetString("A2095BEF-CC8A-4614-86AC-F2F303166235", "'{0}' payment method", rorPaymentMethod), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					foreach (var rorLine in rorLines)
					{
						rorLine.JI_DtyPymntMthd = rorLine.JI_VatPymntMthd = rorLine.JI_TpfPymntMthd = rorPaymentMethod;
						rorLine.Taxes.Where(x => x.IsRorType).ForEach(x => x.JLT_MethodOfPayment = rorPaymentMethod);
					}
				}
			}
		}

		void JE_DateAtFinalDestinationInfo_ValueChanged(object sender, EventArgs e)
		{
			CalculateDaysOfDelayedDeclarationIfRequired();
		}

		void CalculateDaysOfDelayedDeclarationIfRequired()
		{
			if (IsImport)
			{
				var entryInstruction = currentEntryInstruction;
				if (entryInstruction != null && CurrentDataItem.HasDaysOfDelayedDeclaration && !entryInstruction.CEI_DateForDuty.IsEmpty)
				{
					var daysOfDelayedDeclaration = entryInstruction.CEI_DaysOfDelayedDeclaration;
					var daysOfDelayed = entryInstruction.DaysOfDelayed;
					if (daysOfDelayedDeclaration.IsEmpty
							|| (daysOfDelayed != daysOfDelayedDeclaration && Globals.Message.Show(Res.GetString("436C5E5D-F1FF-4FE0-8702-62BCC0A3BC30", "The entered Days of Delayed is {0}, and the calculated Days of Delayed is {1}, do you want to override with calculated Days of Delayed?", daysOfDelayedDeclaration, daysOfDelayed), Res.GetString("886204DC-4FE0-48E4-B98D-F64C80218F83", "Days Of Delayed"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes))
					{
						entryInstruction.ResetCalculateDaysOfDelayedDeclaration();
					}
				}
			}
		}
	}
}
