using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateNatureOfBusiness();
			ValidateExemptionCode1();
			ValidateExemptionCode2();
			ValidateGuaranteeType();
			ValidateGuaranteeAmount();
			ValidateGuaranteeRefNo();
			ValidateDepartureCountry();
			ValidateTradeCountry();
			ValidateExportCountry();
			ValidateArrivalCountry();
			ValidatePaymentMethod();
			ValidateAccountantName();
			ValidateAccountantVAT();
			ValidateContainerNumber();
			ValidateSupplementaryDeclarationRegNoIdNo();
			ValidateSupplementaryDeclarationDeliveryDate();
		}
		protected override void CheckABL_SpecialCargoCode()
		{
			base.CheckABL_SpecialCargoCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_SpecialCargoCodeInfo);
		}

		public void ValidateContainerNumber()
		{
			ValidateCalculatedProperty(Parent.ContainerNumberInfo);
		}

		protected void CheckContainerNumber()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ContainerNumberInfo);
		}

		public void ValidateSupplementaryDeclarationRegNoIdNo()
		{
			ValidateCalculatedProperty(Parent.SupplementaryDeclarationRegNoIdNoInfo);
		}

		protected void CheckSupplementaryDeclarationRegNoIdNo()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SupplementaryDeclarationRegNoIdNoInfo);
		}

		public void ValidateSupplementaryDeclarationDeliveryDate()
		{
			ValidateCalculatedProperty(Parent.SupplementaryDeclarationDeliveryDateInfo);
		}

		protected void CheckSupplementaryDeclarationDeliveryDate()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SupplementaryDeclarationDeliveryDateInfo);
		}

		public void ValidateGuaranteeType()
		{
			ValidateCalculatedProperty(Parent.GuaranteeTypeInfo);
		}

		protected void CheckGuaranteeType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.GuaranteeTypeInfo);
		}

		public void ValidateGuaranteeRefNo()
		{
			ValidateCalculatedProperty(Parent.GuaranteeRefNoInfo);
		}

		protected void CheckGuaranteeRefNo()
		{
			if (!Parent.GuaranteeType.IsEmpty && Parent.GuaranteeRefNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.GuaranteeRefNoInfo);
			}
		}

		public void ValidateGuaranteeAmount()
		{
			ValidateCalculatedProperty(Parent.GuaranteeAmountInfo);
		}

		protected void CheckGuaranteeAmount()
		{
			if (!Parent.GuaranteeType.IsEmpty && Parent.GuaranteeAmount == ZDecimal.Zero)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.GuaranteeAmountInfo);
			}

			if (Parent.GuaranteeAmount < 0)
			{
				Parent.GuaranteeAmountInfo.AddMessageError(Res.GetString("97B67B89-3FC8-405D-93A6-BD986EB77E93", "Guarantee Amount must be greater or equal to zero."));
			}
		}

		public void ValidateNatureOfBusiness()
		{
			ValidateCalculatedProperty(Parent.NatureOfBusinessInfo);
		}

		protected void CheckNatureOfBusiness()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.NatureOfBusinessInfo);
		}

		public void ValidateExemptionCode1()
		{
			ValidateCalculatedProperty(Parent.ExemptionCode1Info);
		}

		protected void CheckExemptionCode1()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ExemptionCode1Info);
			CheckIfSameExemptionCodes(Parent.ExemptionCode1Info);
		}

		public void ValidateExemptionCode2()
		{
			ValidateCalculatedProperty(Parent.ExemptionCode2Info);
		}

		protected void CheckExemptionCode2()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ExemptionCode2Info);
			CheckIfSameExemptionCodes(Parent.ExemptionCode2Info);
		}
		void CheckIfSameExemptionCodes(ZPropertyInfo exemptionCodeInfo)
		{
			if (Parent.ExemptionCode1 == Parent.ExemptionCode2 && !exemptionCodeInfo.Value.IsEmpty)
			{
				exemptionCodeInfo.AddMessageError(Res.GetString("06395F00-F0B6-45E8-8970-1922CA11927C", "Exemption Code 1 and Exemption Code 2 cannot be same."));
			}
			ValidateExportCountry();
		}

		public void ValidateDepartureCountry()
		{
			ValidateCalculatedProperty(Parent.DepartureCountryInfo);
		}

		protected void CheckDepartureCountry()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.DepartureCountryInfo);
		}

		protected override void CheckABL_Procedure()
		{
			base.CheckABL_Procedure();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_ProcedureInfo);
		}

		public void ValidateTradeCountry()
		{
			ValidateCalculatedProperty(Parent.TradeCountryInfo);
		}

		protected void CheckTradeCountry()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.TradeCountryInfo);
		}

		public void ValidateExportCountry()
		{
			ValidateCalculatedProperty(Parent.ExportCountryInfo);
		}

		protected void CheckExportCountry()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ExportCountryInfo);
		}

		public void ValidateArrivalCountry()
		{
			ValidateCalculatedProperty(Parent.ArrivalCountryInfo);
		}

		protected void CheckArrivalCountry()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ArrivalCountryInfo);
		}

		protected override void CheckABL_Incoterm()
		{
			base.CheckABL_Incoterm();
			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_IncotermInfo);
		}

		public void ValidatePaymentMethod()
		{
			ValidateCalculatedProperty(Parent.PaymentMethodInfo);
		}

		protected void CheckPaymentMethod()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.PaymentMethodInfo);
		}

		public void ValidateAccountantName()
		{
			ValidateCalculatedProperty(Parent.AccountantNameInfo);
		}

		public void ValidateAccountantVAT()
		{
			ValidateCalculatedProperty(Parent.AccountantVATInfo);
		}

		protected override void CheckABL_OtherValue()
		{
			base.CheckABL_OtherValue();
			if (Parent.ABL_OtherValue < 0)
			{
				Parent.ABL_OtherValueInfo.AddError(Res.GetString("3F01BDAB-7CE6-4E2B-AD2D-70F9C9D7CDE8", "Domestic Expenditure Value must be greater or equal to zero."));
			}
		}

		protected override void CheckABL_GrossWeight()
		{
			base.CheckABL_GrossWeight();
			var header = Parent.Header;
			var maxImportGrossWeight = header.IsImport ? Parent.MaxImportGrossWeight : Parent.MaxExportGrossWeight;

			if (!maxImportGrossWeight.IsEmpty && Parent.GrossWeightInKG > maxImportGrossWeight)
			{
				Parent.ABL_GrossWeightInfo.AddMessageError(Res.GetString("49FE22D6-F704-4ADB-8217-282C3DF68A2A", "Gross Weight value must be lower or equal to {0:f2} Kg.", maxImportGrossWeight));
			}
		}

		protected override void CheckABL_CustomsValue()
		{
			base.CheckABL_CustomsValue();
			var header = Parent.Header;
			var maxImportCustomsValue = header.IsImport ? Parent.MaxImportCustomsValue : Parent.MaxExportCustomsValue;

			if (!maxImportCustomsValue.IsEmpty && Parent.ABL_CustomsValue > maxImportCustomsValue)
			{
				Parent.ABL_CustomsValueInfo.AddMessageError(Res.GetString("08B94B57-5513-4613-B4D2-8221C3487A31", "'Customs Value must be lower or equal to {0:f2} EUR.", maxImportCustomsValue));
			}
		}

		protected override void CheckABL_GoodsValue()
		{
			base.CheckABL_GoodsValue();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_GoodsValueInfo);

			var totalGoodsValueFromPackedItems = Parent.Packs.Cast<AsycudaPack>().Sum(x => x.PackedItem?.API_GoodsValue ?? ZDecimal.Zero); // API_RX_NKGoodsValueCurrency(s) are all the same, and are same as ABL_RX_NKGoodsValueCurrency
			if (Parent.ABL_GoodsValue != totalGoodsValueFromPackedItems)
			{
				Parent.ABL_GoodsValueInfo.AddWarning(Res.GetString("90D1B1B6-15D4-481E-8768-FF4708247F55", "Out of balance, total goods value on packs is not equal goods value on bill."));
			}
		}

		protected override void CheckABL_RX_NKGoodsValueCurrency()
		{
			base.CheckABL_RX_NKGoodsValueCurrency();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RX_NKGoodsValueCurrencyInfo);
		}

		protected override void CheckABL_OA_NotifyParty()
		{
			base.CheckABL_OA_NotifyParty();
			if (Parent.ABL_OA_NotifyParty.IsEmpty)
			{
				Parent.ABL_OA_NotifyPartyInfo.AddMessageError(Res.GetString("B4756C94-1E6E-4945-9FAE-30D9A1E375E7", "Please fill in the Market Place name."));
			}
		}

		protected override INotificationType NotificationTypeForDuplicateBillNumber => CargoWise.EntityFramework.NotificationType.MessageError;
	}
}


