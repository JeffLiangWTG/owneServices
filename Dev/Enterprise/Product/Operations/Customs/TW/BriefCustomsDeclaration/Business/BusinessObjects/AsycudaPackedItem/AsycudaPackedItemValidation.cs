using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaPackedItemValidation : ASYCUDA.Business.AsycudaPackedItemValidation
	{
		public AsycudaPackedItemValidation(AsycudaPackedItem parent) : base(parent)
		{
		}

		protected new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAPI_Preference();
		}

		public void ValidateAPI_Preference()
		{
			((IValidationInternals)this).Validate(Parent.API_PreferenceInfo, () => { CheckAPI_Preference(); });
		}

		protected override void CheckAPI_GoodsDescription()
		{
			base.CheckAPI_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_GoodsDescriptionInfo);
		}

		protected override void CheckAPI_Brand()
		{
			base.CheckAPI_Brand();
			if (Parent.Header is AsycudaManifestHeader header && header.IsExport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.API_BrandInfo);
			}
		}

		protected override void CheckAPI_CustomsValue()
		{
			base.CheckAPI_CustomsValue();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_CustomsValueInfo);
		}

		protected override void CheckAPI_CustomsQty()
		{
			base.CheckAPI_CustomsQty();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_CustomsQtyInfo, Res.GetString("D91DADB8-488A-4835-B083-01E132219C11", "Quantity"));
		}

		protected override void CheckAPI_CustomsQtyIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.API_CustomsQtyInfo, 16, 5);
		}

		protected override void CheckAPI_CustomsUQ()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.API_CustomsUQInfo, Res.GetString("1A43F339-B6FE-4AE0-B24A-57EE273E00BD", "Quantity Unit"));
		}

		protected override void CheckAPI_UnitPrice()
		{
			base.CheckAPI_UnitPrice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_UnitPriceInfo);
		}

		protected override void CheckAPI_UnitPriceIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.API_UnitPriceInfo, 19, 6);
		}

		protected override void CheckAPI_GoodsValue()
		{
			base.CheckAPI_GoodsValue();
			var parent = Parent;
			var goodsValue = parent.API_GoodsValue;
			var quantity = parent.API_CustomsQty;
			var unitPrice = parent.API_UnitPrice;
			if (goodsValue > 0 && quantity > 0 && unitPrice > 0 &&
				Utilities.Round(goodsValue / quantity, 6) != unitPrice &&
				Utilities.Round(unitPrice * quantity, 2) != goodsValue &&
				Utilities.Round(goodsValue / unitPrice, 5) != quantity)
			{
				parent.API_GoodsValueInfo.AddMessageError(Res.GetString("9B0A67B5-ACD7-4902-B8C6-0F644D7AD92F", "Goods Value must be the product of Unit Price and Quantity."));
			}
		}

		protected override void CheckAPI_GoodsValueIsValidMoney()
		{
			TypeValidation.CheckValidDecimal(Parent.API_GoodsValueInfo, 19, 2);
		}

		protected override void CheckAPI_Tariff()
		{
			base.CheckAPI_Tariff();
			if (!Parent.API_Tariff.IsEmpty && Parent.UniversalTariff == null)
			{
				var info = Parent.API_TariffInfo;
				info.AddMessageError(Res.GetString("0825B16A-BAC4-4FA0-AC52-F36D2D486EB7", "The entered {0} is invalid.", info.HumanReadableName));
			}
		}

		protected override void CheckAPI_NetWeight()
		{
			base.CheckAPI_NetWeight();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_NetWeightInfo, Res.GetString("9271DC01-8C5C-4533-BC49-86D0B6CBA656", "Net Weight"));
		}

		protected override void CheckAPI_NetWeightUQ()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.API_NetWeightUQInfo, Res.GetString("1DA902DB-9F6B-48F8-957E-A9DDDF75A711", "Net Weight Unit"));
		}

		protected override void CheckAPI_PreviousEntryNo()
		{
			base.CheckAPI_PreviousEntryNo();
			var parent = Parent;
			var previousEntryNo = parent.API_PreviousEntryNo;
			if (!previousEntryNo.IsEmpty && previousEntryNo.Length != 14)
			{
				parent.API_PreviousEntryNoInfo.AddMessageError(Res.GetString("4308E0E9-0C56-43FF-8162-E2199D35495A", "Previous Bonded Entry Number should be 14 characters long."));
			}
		}

		protected override void CheckAPI_PreviousEntryLineNo()
		{
			base.CheckAPI_PreviousEntryLineNo();
			var parent = Parent;
			if (!parent.API_PreviousEntryNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.API_PreviousEntryLineNoInfo);
			}
		}

		void CheckAPI_Preference()
		{
			if (Parent.Header is AsycudaManifestHeader header && header.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.API_PreferenceInfo);
			}
		}
	}
}
