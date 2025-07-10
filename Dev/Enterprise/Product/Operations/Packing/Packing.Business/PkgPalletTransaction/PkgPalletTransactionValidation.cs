using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	public class PkgPalletTransactionValidation : AutoPkgPalletTransactionValidation
	{
		public PkgPalletTransactionValidation(AutoPkgPalletTransaction parent)
			: base(parent)
		{
		}

		new PkgPalletTransaction Parent
		{
			get { return (PkgPalletTransaction)base.Parent; }
		}

		#region CheckKTR_PalletType

		protected override void CheckKTR_PalletType()
		{
			base.CheckKTR_PalletType();
			ListValidation.ErrorIfInvalidCode(Parent.KTR_PalletTypeInfo);
		}

		#endregion

		#region CheckKTR_EquipmentCode

		protected override void CheckKTR_EquipmentCode()
		{
			base.CheckKTR_EquipmentCode();
			MandatoryValidation.CheckEntered(Parent.KTR_EquipmentCodeInfo);
			if (!Parent.KTR_EquipmentCode.IsEmpty && Parent.PalletDefinition != null && Parent.PalletDefinition.EquipmentCode != Parent.KTR_EquipmentCode)
			{
				Parent.KTR_EquipmentCodeInfo.AddWarning(Res.GetString("41476141-7f92-4370-b57b-5a847f442d99", "The Pallet Type chosen ({0}) has an equipment code configured in the registry of {1}, which is different to what you have entered here. Please check that correct pallet type and equipment have been specified.", Parent.KTR_PalletType, Parent.PalletDefinition.EquipmentCode));
			}
		}

		#endregion

		#region CheckKTR_Status

		protected override void CheckKTR_Status()
		{
			base.CheckKTR_Status();
			MandatoryValidation.CheckEntered(Parent.KTR_StatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.KTR_StatusInfo);
		}

		#endregion

		#region CheckKTR_TransactionType

		protected override void CheckKTR_TransactionType()
		{
			base.CheckKTR_TransactionType();
			MandatoryValidation.CheckEntered(Parent.KTR_TransactionTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.KTR_TransactionTypeInfo);
		}

		#endregion

		#region CheckKTR_Quantity

		protected override void CheckKTR_Quantity()
		{
			base.CheckKTR_Quantity();
			MandatoryValidation.CheckNotZero(Parent.KTR_QuantityInfo);
			MandatoryValidation.CheckNotNegative(Parent.KTR_QuantityInfo);
		}

		#endregion

		#region TransferFromAndToAccountNumbers

		protected override void CheckKTR_TransferFromAccountNumber()
		{
			base.CheckKTR_TransferFromAccountNumber();
			CheckTradingAccountNumbers(Parent.KTR_TransferFromAccountNumberInfo);
		}

		protected override void CheckKTR_TransferToAccountNumber()
		{
			base.CheckKTR_TransferToAccountNumber();
			CheckTradingAccountNumbers(Parent.KTR_TransferToAccountNumberInfo);
		}

		void CheckTradingAccountNumbers(ZPropertyInfo info)
		{
			if (Parent.IsTransfer)
			{
				MandatoryValidation.CheckEntered(info);
			}
			else if (!info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("3c46a7e3-a1b5-4fd4-bf4d-b58c178f3d18", "Exchange transfers must not have a trading account number."));
			}
		}

		#endregion
	}
}
