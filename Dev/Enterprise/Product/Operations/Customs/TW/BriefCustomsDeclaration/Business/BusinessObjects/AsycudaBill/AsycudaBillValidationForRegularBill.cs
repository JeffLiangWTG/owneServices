using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
			registrationNumberValidation = new ManifestRegistrationNumberValidation(parent);
		}

		readonly ManifestRegistrationNumberValidation registrationNumberValidation;

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override ZBool NeedsToShowABL_ManifestUQNotMappedMessageError => false;

		protected override void CheckABL_ShipperRegNoType()
		{
			base.CheckABL_ShipperRegNoType();
			var parent = Parent;
			if (parent.IsExport)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.ABL_ShipperRegNoTypeInfo, Res.GetString("773A069D-D8FE-4886-9658-DE638ACBE75B", "ID Type"));
			}
		}

		protected override void CheckABL_ShipperRegNo()
		{
			base.CheckABL_ShipperRegNo();
			var parent = Parent;
			var code = parent.ABL_ShipperRegNo;
			if (!code.IsEmpty)
			{
				OrgCusCodeValidation.ValidateCustomsCode(registrationNumberValidation, parent.ABL_RN_NKShipperCountry, parent.ABL_ShipperRegNoType, code, parent.ABL_ShipperRegNoInfo);
			}
			else if (parent.IsExport)
			{
				parent.ABL_ShipperRegNoInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("B60551DA-3848-487F-9D09-F4EE705235D7", "ID")));
			}
		}

		protected override void CheckABL_ManifestQtyMatchSumOfPacks()
		{
		}

		protected override void CheckABL_RL_NKFinalDestination()
		{
		}

		protected override void CheckABL_RL_NKOrigin()
		{
		}

		protected override void CheckABL_CustomsValue()
		{
			base.CheckABL_CustomsValue();
			var bill = Parent;
			var targetInfo = bill.ABL_CustomsValueInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			if (bill.ABL_CustomsValue != bill.PackedItems.Cast<AsycudaPackedItem>().Sum(x => x.API_CustomsValue))
			{
				targetInfo.AddMessageError(Res.GetString("61D0563D-151E-4A59-AD96-22D482E043FB", "Total {0} must equal to the sum of all item {1}.", bill.ABL_CustomsValueInfo.HumanReadableName, DataBoundResourceStrings.GetDataForProperty(typeof(AsycudaPackedItem), nameof(AsycudaPackedItem.API_CustomsValue)).Caption));
			}
		}

		protected override void CheckABL_GoodsValue()
		{
			base.CheckABL_GoodsValue();
			var bill = Parent;
			var targetInfo = bill.ABL_GoodsValueInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			if (bill.ABL_GoodsValue != Utilities.Round(bill.PackedItems.Cast<AsycudaPackedItem>().Sum(x => x.API_UnitPrice * x.API_CustomsQty), 2))
			{
				targetInfo.AddMessageError(Res.GetString("8453315D-BD48-4FFD-B657-36F33C0C98F5", "Total goods value must equal to the sum of all item goods values."));
			}
		}

		protected override void CheckABL_GoodsValueIsValidMoney()
		{
			TypeValidation.CheckValidMoney(Parent.ABL_GoodsValueInfo, 16, 2);
		}

		protected override void CheckMandatoryABL_OA_Consignee()
		{
		}

		protected override ZBool NeedsToCheckABL_Consignee => false;

		protected override void CheckABL_ConsigneeName()
		{
			var parent = Parent;
			var header = parent.Header;
			if (header != null && header.IsExport && header.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_ConsigneeNameInfo);
			}
		}

		protected override void CheckABL_RN_NKConsigneeCountry()
		{
			base.CheckABL_RN_NKConsigneeCountry();
			var parent = Parent;
			var header = parent.Header;
			if (header != null && header.IsExport && header.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_RN_NKConsigneeCountryInfo);
			}
		}

		protected override void CheckABL_ConsigneeRegNoType()
		{
			base.CheckABL_ConsigneeRegNoType();
			var parent = Parent;
			if (!parent.ABL_ConsigneeRegNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_ConsigneeRegNoTypeInfo);
			}
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();
			var parent = Parent;
			var code = parent.ABL_ConsigneeRegNo;
			if (!code.IsEmpty)
			{
				OrgCusCodeValidation.ValidateCustomsCode(registrationNumberValidation, parent.ABL_RN_NKConsigneeCountry, parent.ABL_ConsigneeRegNoType, code, parent.ABL_ConsigneeRegNoInfo);
			}
		}

		protected override void CheckABL_Procedure()
		{
			base.CheckABL_Procedure();
			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_ProcedureInfo);
		}

		protected override void CheckABL_Incoterm()
		{
			base.CheckABL_Incoterm();
			var parent = Parent;
			var targetInfo = parent.ABL_IncotermInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);

			var header = parent.Header;
			if (header != null && header.IsAir && header.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}

		protected override void CheckABL_RL_NKPortOfLoading()
		{
			base.CheckABL_RL_NKPortOfLoading();
			var parent = Parent;
			var targetInfo = parent.ABL_RL_NKPortOfLoadingInfo;
			if (parent.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
		}

		protected override void CheckABL_RL_NKPortOfDischarge()
		{
			base.CheckABL_RL_NKPortOfDischarge();
			var parent = Parent;
			var targetInfo = parent.ABL_RL_NKPortOfDischargeInfo;
			if (parent.IsExport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
		}

		protected override void CheckABL_RX_NKGoodsValueCurrency()
		{
			base.CheckABL_RX_NKGoodsValueCurrency();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RX_NKGoodsValueCurrencyInfo);
		}
	}
}
