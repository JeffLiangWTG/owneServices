using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaPackedItemValidation : ASYCUDA.Business.AsycudaPackedItemValidation
	{
		public AsycudaPackedItemValidation(AsycudaPackedItem parent)
			: base(parent)
		{
		}
		protected new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateBanderolTariff();
		}

		protected override void CheckAPI_GoodsDescription()
		{
			base.CheckAPI_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_GoodsDescriptionInfo);
		}

		protected override void CheckAPI_RN_NKGoodsOrigin()
		{
			base.CheckAPI_RN_NKGoodsOrigin();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.API_RN_NKGoodsOriginInfo);
		}

		protected override void CheckAPI_CustomsQty()
		{
			base.CheckAPI_CustomsQty();
			if (Parent.API_CustomsQty.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.API_CustomsQtyInfo);
			}
		}

		protected override void CheckAPI_CustomsQty2()
		{
			base.CheckAPI_CustomsQty2();
			if (Parent.API_CustomsQty2 < 0)
			{
				Parent.API_CustomsQty2Info.AddError(NegativeAmountNotAllowed);
			}
		}

		protected override void CheckAPI_CustomsQty3()
		{
			base.CheckAPI_CustomsQty3();
			if (Parent.API_CustomsQty3 < 0)
			{
				Parent.API_CustomsQty3Info.AddError(NegativeAmountNotAllowed);
			}
		}

		internal string NegativeAmountNotAllowed = Res.GetString("30957079-FAB8-4850-8DFA-D37315E658FD", "Please enter a non-negative value.");

		protected override void CheckAPI_CustomsUQ2()
		{
			base.CheckAPI_CustomsUQ2();
			if (!Parent.API_CustomsQty2.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.API_CustomsUQ2Info);
			}
		}

		protected override void CheckAPI_CustomsUQ3()
		{
			base.CheckAPI_CustomsUQ3();
			if (!Parent.API_CustomsQty3.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.API_CustomsUQ3Info);
			}
		}

		protected override void CheckAPI_GoodsValue()
		{
			base.CheckAPI_GoodsValue();
			if (Parent.API_GoodsValue.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.API_GoodsValueInfo);
			}
		}

		protected override void CheckAPI_RX_NKGoodsValueCurrency()
		{
			base.CheckAPI_RX_NKGoodsValueCurrency();
			if (Parent.API_RX_NKGoodsValueCurrency.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.API_RX_NKGoodsValueCurrencyInfo);
			}
		}

		protected override void CheckAPI_Tariff()
		{
			base.CheckAPI_Tariff();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_TariffInfo);
		}

		protected override void TariffListValidationCore()
		{
			if (!Parent.API_Tariff.IsEmpty)
			{
				var length = Parent.API_Tariff.Length;

				if (length != 4 && length != 6 && length != 8 && length != 12)
				{
					Parent.API_TariffInfo.AddMessageError(Res.GetString("8AD62B1E-A148-4ECB-BB74-D5D009C4C799", "Only 4, 6, 8, 12 digits are allowed"));
				}
				else
				{
					if (Parent.NormalTariffView == null)
					{
						Parent.API_TariffInfo.AddMessageError(Res.GetString("704D6529-29F7-4243-9A77-961AC690D0FD", "The code you have selected is not in the list."));
					}
				}
			}
		}

		public void ValidateBanderolTariff()
		{
			ValidateCalculatedProperty(Parent.BanderolTariffInfo);
		}

		protected void CheckBanderolTariff()
		{
			if (Parent.Pack?.Bill?.Header?.IsImport ?? false)
			{
				var banderolTariff = Parent.BanderolTariff;

				if (banderolTariff.IsEmpty)
				{
					var normalTariff = Parent.NormalTariffView;
					if (normalTariff != null && normalTariff.HasAttribute(TaxCodeList.RelatedMiscCodes.BanderolTariffAttributes))
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.BanderolTariffInfo);
					}
				}
				else
				{
					var result = Parent.BanderolTariffView;
					if (result == null)
					{
						Parent.BanderolTariffInfo.AddMessageError(Res.GetString("18612088-0DD3-4739-BEA3-0A832944B69D", "The code you have selected is not in the list."));
					}
				}
			}
		}

		protected override void CheckAPI_ChemicalSubstanceCode()
		{
			base.CheckAPI_ChemicalSubstanceCode();
			if (!Parent.API_ChemicalSubstanceCode.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.API_ChemicalSubstanceCodeInfo);
			}
		}
	}
}
