using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;
		protected AsycudaManifestHeader Header => Parent.Header;

		protected override ZBool NeedsToShowABL_ManifestUQNotEnteredMessageError => false;

		protected override ZBool NeedsToCheckABL_ConsigneePostcode => false;

		protected override ZBool NeedsToCheckABL_ShipperPostcode => false;

		protected override ZBool NeedsToShowABL_ManifestUQNotMappedMessageError => false;

		protected override ZBool NeedsToCheckABL_ManifestUQ => false;

		protected override INotificationType NotificationTypeForDuplicateBillNumber => CargoWise.EntityFramework.NotificationType.Error;

		public override void ValidateAll()
		{
			base.ValidateAll();

			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateABL_Tariff();
				ValidateGoodsOrigin();
			}
		}

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();
			var header = Header;
			if (header != null && header.AMA_ManifestType == ACEManifestTypes.Codes.IAM && Parent.ABL_BillNumber.Length > 12)
			{
				Parent.ABL_BillNumberInfo.AddError(ValidationConstants.BillNumberIsWrongLength);
			}
		}

		protected override void CheckABL_GoodsValue()
		{
			base.CheckABL_GoodsValue();
			ValidateABL_RX_NKGoodsValueCurrency();
		}

		protected override void CheckABL_GoodsDescription()
		{
			base.CheckABL_GoodsDescription();
			if (Parent.ABL_GoodsDescription.Length > CargoDescription_MaxLengthInMessage)
			{
				Parent.ABL_GoodsDescriptionInfo.AddWarning($"Description is limited to {CargoDescription_MaxLengthInMessage} characters. Only the first {CargoDescription_MaxLengthInMessage} characters will be sent in the message.");
			}
		}

		const int CargoDescription_MaxLengthInMessage = 35;

		protected override void CheckABL_ManifestQtyMatchSumOfPacks()
		{
		}

		#region ABL_Tariff

		public void ValidateABL_Tariff()
		{
			ValidateCalculatedProperty(Parent.ABL_TariffInfo);
		}

		protected void CheckABL_Tariff()
		{
			var parent = Parent;
			var tariff = parent.ABL_Tariff;
			if (!tariff.IsEmpty)
			{
				var tariffView = new TariffView.Loader(parent.Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem, new TariffFormatter().Format(tariff), ZDateTime.Today);
				if (tariffView == null)
				{
					parent.ABL_TariffInfo.AddMessageError(ListValidation.InvalidCodeMessage.ToString());
				}
			}
		}

		#endregion

		#region GoodsOrigin

		public void ValidateGoodsOrigin()
		{
			ValidateCalculatedProperty(Parent.GoodsOriginInfo);
		}

		protected void CheckGoodsOrigin()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.GoodsOriginInfo);
		}

		#endregion

		protected override void CheckCustomsEntryNumberType()
		{
			base.CheckCustomsEntryNumberType();
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.CustomsEntryNumberTypeInfo, Parent.CustomsEntryNumberInfo);
		}

		protected override void CheckCustomsEntryNumber()
		{
			base.CheckCustomsEntryNumber();
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(Parent.CustomsEntryNumberInfo, Parent.CustomsEntryNumberTypeInfo,
				new IZType[] { (ZString)ACEManifestBillEntryNumberTypes.Codes.Informal, (ZString)ACEManifestBillEntryNumberTypes.Codes.Sec321a, (ZString)ACEManifestBillEntryNumberTypes.Codes.GoodsReturned });
		}
	}
}
