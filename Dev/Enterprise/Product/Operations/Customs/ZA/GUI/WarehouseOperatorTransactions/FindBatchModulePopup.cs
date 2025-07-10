using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.GUI
{
	public class FindBatchModulePopup : EmbeddedModulePopup
	{
		public FindBatchModulePopup(ZFilterModule module, Operation operation) : base(module)
		{
			this.operation = operation;
		}

		readonly Operation operation;

		public enum Operation { ReverseBatch, ResetBatchReceipt }

		public override string FormCaption
		{
			get
			{
				var result = string.Empty;
				switch (operation)
				{
					case Operation.ReverseBatch:
						result = Res.GetString("85AFE9A8-74DB-4EEB-A644-175C3A703D9A", "Reverse a Batch");
						break;
					case Operation.ResetBatchReceipt:
						result = Res.GetString("4CC03A1C-A46D-40E9-A536-860AFD4A53BF", "Reset Receipts for a Batch");
						break;
				}
				return result;
			}
		}

		protected override void OnCreateControl()
		{
			switch (operation)
			{
				case Operation.ResetBatchReceipt:
					Globals.Message.ShowInformation(Res.GetString("C209EF80-6606-448B-9B00-86391CF94354", "Please check allocations for BLN and EXP Orders before selecting the batch."));
					break;
			}
			base.OnCreateControl();
		}

		protected override void OnOkButtonClicked()
		{
			var selectedTransactions = GetSelectedBusinessObjects().Cast<CusWHSOperatorTransaction>().ToArray();
			if (selectedTransactions.Length == 1)
			{
				switch (operation)
				{
					case Operation.ReverseBatch:
						ReverseBatch(selectedTransactions[0]);
						break;
					case Operation.ResetBatchReceipt:
						ResetBatchReceipt(selectedTransactions[0]);
						break;
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("7C5A8F45-A4F0-4026-AF77-952F3BA56D44", "Please select only one transaction from the grid."));
			}
		}

		void ReverseBatch(CusWHSOperatorTransaction selectedTransaction)
		{
			var message = Res.GetString("16031291-6A29-400D-8422-57D1244C246A", "I understand the consequences of Reversing a Batch.");
			if (Globals.Message.ShowConfirmation(message, "Confirmation", "yes", ZMessageBoxIcon.Question) == ZDialogResult.OK)
			{
				var batchPK = selectedTransaction.WOT_WOB_CusWHSTransactionBatch.ToGuid();
				try
				{
					Business.ReverseBatch.Execute(selectedTransaction.Factory, batchPK, GlbStaff.CurrentUser.GS_Code);
					Globals.Message.ShowInformation(Res.GetString("0AD9EDAD-C3C0-47F9-8EE5-2AAC5755AE53", "Reversal completed!"));
					Close();
				}
				catch (Exception ex)
				{
					Globals.Message.ShowError(Res.GetString("EF4531D6-6248-4269-AC5E-FCF57E6F719C", "Error occurred during reversal: {0}", ex.Message), "Error");
				}
			}
		}

		void ResetBatchReceipt(CusWHSOperatorTransaction selectedTransaction)
		{
			try
			{
				var batchPK = selectedTransaction.WOT_WOB_CusWHSTransactionBatch.ToGuid();
				Business.ResetBatchReceipt.Execute(selectedTransaction.Factory, batchPK, GlbStaff.CurrentUser.GS_Code);
				Globals.Message.ShowInformation(Res.GetString("550DF5C5-68FD-4FBF-BBA8-8594AB181E81", "Script successfully executed."));
				Close();
			}
			catch (Exception ex)
			{
				Globals.Message.ShowError(Res.GetString("49638540-1855-4767-BD6A-2CDE66EB030B", "Error occurred during reset: {0}", ex.Message), "Error");
			}
		}

		public static void ShowDialog(ZFilterModule module, Operation operation)
		{
			using (module)
			{
				using (var modulePopup = new FindBatchModulePopup(module, operation))
				{
					ZFormModaliser.ShowDialogAndDispose(modulePopup);
				}
			}
		}
	}
}
