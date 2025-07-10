using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidation
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent)
			: base(parent)
		{
			registrationNumberValidation = new TWManifestRegistrationNumberValidation(parent);
		}
		readonly TWManifestRegistrationNumberValidation registrationNumberValidation;

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateABL_SplitQuantity();
				ValidateABL_Tariff();
				ValidateABL_DG_UNNO();
			}
		}

		protected override void CheckABL_ManifestQty()
		{
			base.CheckABL_ManifestQty();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ManifestQtyInfo);
		}

		protected override void CheckABL_ManifestUQ()
		{
			base.CheckABL_ManifestUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_ManifestUQInfo);
		}

		#region ABL_SplitQuantity

		public void ValidateABL_SplitQuantity()
		{
			ValidateCalculatedProperty(Parent.ABL_SplitQuantityInfo);
		}

		protected void CheckABL_SplitQuantity()
		{
			var targetInfo = Parent.ABL_SplitQuantityInfo;
			if (Parent.ABL_SplitQuantity > Parent.ABL_ManifestQty)
			{
				targetInfo.AddMessageError(Res.GetString("92431E5A-746F-4DBC-BBC8-3C1AF8FA8909", "{0} must be lower than or equal to {1}.", targetInfo.HumanReadableName, Parent.ABL_ManifestQtyInfo.HumanReadableName));
			}
		}

		#endregion

		public void ValidateABL_Tariff()
		{
			ValidateCalculatedProperty(Parent.ABL_TariffInfo);
		}

		protected void CheckABL_Tariff()
		{
			var parent = Parent;
			if (!parent.ABL_Tariff.IsEmpty && parent.UniversalTariff == null)
			{
				parent.ABL_TariffInfo.AddMessageError(ListValidation.InvalidCodeMessage.ToString());
			}
		}

		protected override void CheckABL_GrossWeight()
		{
			base.CheckABL_GrossWeight();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_GrossWeightInfo);
		}

		protected override void CheckABL_GrossWeightUQ()
		{
			base.CheckABL_GrossWeightUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_GrossWeightUQInfo);
		}

		protected override void CheckABL_VolumeUQ()
		{
			base.CheckABL_VolumeUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_VolumeUQInfo);
		}

		protected override void CheckABL_RL_NKPortOfLoading()
		{
			var parent = Parent;
			base.CheckABL_RL_NKPortOfLoading();
			var targetInfo = parent.ABL_RL_NKPortOfLoadingInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			var portOfLoading = parent.ABL_RL_NKPortOfLoading;
			if (!portOfLoading.IsEmpty && !parent.IsZ99PortCode(portOfLoading))
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
		}

		protected override void CheckABL_LocationInformation()
		{
			base.CheckABL_LocationInformation();
			var parent = Parent;
			var portOfLoading = parent.ABL_RL_NKPortOfLoading;
			if (parent.ABL_LocationInformation.IsEmpty && parent.IsZ99PortCode(portOfLoading))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_LocationInformationInfo);
			}
		}

		void CheckSelectEnglishAddress(ZString language, ZPropertyInfo targetInfo)
		{
			if (!IsEnglishLanguageCode(language))
			{
				targetInfo.AddMessageError(SelectEnglishAddressMessage);
			}
		}

		bool IsEnglishLanguageCode(ZString languageCode) => SharedHelper.GetEnglishLanguageCodes().Contains(languageCode);

		string SelectEnglishAddressMessage => Res.GetString("3A8ED7F6-C979-4A45-9C44-AB9C29AD4212", "Please select an English address. Traditional Chinese address should be added as a translated address of the English address.");

		protected override void CheckABL_OA_Shipper()
		{
			base.CheckABL_OA_Shipper();
			var parent = Parent;
			var shipper = parent.Shipper;
			if (shipper != null)
			{
				CheckSelectEnglishAddress(shipper.OA_Language, parent.ABL_OA_ShipperInfo);
			}
		}

		protected override void CheckABL_ShipperStreet1()
		{
			base.CheckABL_ShipperStreet1();
			var parent = Parent;
			if (!parent.ABL_ShipperLocalStreet1.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_ShipperStreet1Info);
			}
		}

		protected override void CheckABL_NotifyPartyStreet1()
		{
			base.CheckABL_NotifyPartyStreet1();
			var parent = Parent;
			if (!parent.ABL_NotifyPartyLocalStreet1.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_NotifyPartyStreet1Info);
			}
		}

		protected override void CheckABL_OA_Consignee()
		{
			base.CheckABL_OA_Consignee();
			var parent = Parent;
			var consignee = parent.Consignee;
			if (consignee != null)
			{
				CheckSelectEnglishAddress(consignee.OA_Language, parent.ABL_OA_ConsigneeInfo);
			}
		}

		protected override void CheckABL_OA_NotifyParty()
		{
			base.CheckABL_OA_NotifyParty();
			var parent = Parent;
			var notifyParty = parent.NotifyParty;
			if (notifyParty != null)
			{
				CheckSelectEnglishAddress(notifyParty.OA_Language, parent.ABL_OA_NotifyPartyInfo);
			}
		}

		protected override void CheckABL_ShipperName()
		{
			base.CheckABL_ShipperName();
			var parent = Parent;
			if (!parent.ABL_ShipperStreet1.IsEmpty || !parent.ABL_ShipperRegNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_ShipperNameInfo, Res.GetString("1D712AC7-3023-4E09-9FD8-A168E3A6BBBF", "Shipper's English Name"));
			}
		}

		protected override void CheckABL_NotifyPartyName()
		{
			base.CheckABL_NotifyPartyName();
			var parent = Parent;
			if (!parent.ABL_NotifyPartyStreet1.IsEmpty || !parent.ABL_NotifyPartyRegNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_NotifyPartyNameInfo, Res.GetString("651499AD-751B-41C4-9FF5-E01F1D99303C", "Notify Party's English Name"));
			}
		}

		protected override void CheckABL_ConsigneeName()
		{
			base.CheckABL_ConsigneeName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeNameInfo, Res.GetString("ED56A786-2E27-4AE2-9C8A-5F231B9AF080", "Consignee's English Name"));
		}

		protected override void CheckABL_ShipmentType()
		{
			base.CheckABL_ShipmentType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipmentTypeInfo);
		}

		protected override void CheckABL_RL_NKFinalDestination()
		{
			base.CheckABL_RL_NKFinalDestination();
			var parent = Parent;
			var targetInfo = parent.ABL_RL_NKFinalDestinationInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			if (parent.Header?.IsAir ?? false)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}

		protected override void CheckABL_GoodsLocation()
		{
			base.CheckABL_GoodsLocation();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_GoodsLocationInfo);
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

		protected override void CheckABL_NotifyPartyRegNo()
		{
			base.CheckABL_NotifyPartyRegNo();
			var parent = Parent;
			var code = parent.ABL_NotifyPartyRegNo;
			if (!code.IsEmpty)
			{
				OrgCusCodeValidation.ValidateCustomsCode(registrationNumberValidation, parent.ABL_RN_NKNotifyPartyCountry, parent.ABL_NotifyPartyRegNoType, code, parent.ABL_NotifyPartyRegNoInfo);
			}
		}

		public void ValidateABL_DG_UNNO()
		{
			ValidateCalculatedProperty(Parent.ABL_DG_UNNOInfo);
		}

		protected void CheckABL_DG_UNNO()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_DG_UNNOInfo);
		}
	}
}
