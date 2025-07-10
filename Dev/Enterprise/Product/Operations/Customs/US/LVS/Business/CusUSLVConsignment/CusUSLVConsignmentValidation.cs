//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusUSLVConsignmentValidation
//
//    This class should be used for overriding validation in AutoCusUSLVConsignmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVConsignmentValidation : AutoCusUSLVConsignmentValidation
	{
		public CusUSLVConsignmentValidation(AutoCusUSLVConsignment parent) : base(parent)
		{
		}

		protected new CusUSLVConsignment Parent => (CusUSLVConsignment)base.Parent;

		protected override void CheckULB_NumberOfPacks()
		{
			base.CheckULB_NumberOfPacks();
			var parent = Parent;
			if (parent.ULB_NumberOfPacks == 0)
			{
				parent.ULB_NumberOfPacksInfo.AddMessageError(Res.GetString("82C42262-BA2F-48BA-A62D-D11C7FA21627", "Please enter a Bill quantity."));
			}
		}

		protected override void CheckULB_ConsigneeIdentifier()
		{
			base.CheckULB_ConsigneeIdentifier();
			var parent = Parent;
			if (parent.ULB_ConsigneeIdentifier.IsEmpty && !parent.ULB_ConsigneeQualifier.IsEmpty)
			{
				parent.ULB_ConsigneeIdentifierInfo.AddMessageError(Res.GetString("5FCB7473-E1FA-4A19-A280-F3CE81118073", "Consignee Identifier is mandatory when a Consignee Qualifier has been entered."));
			}
		}

		protected override void CheckULB_ConsigneeQualifier()
		{
			base.CheckULB_ConsigneeQualifier();
			var parent = Parent;
			if (parent.ULB_ConsigneeQualifier.IsEmpty && !parent.ULB_ConsigneeIdentifier.IsEmpty)
			{
				parent.ULB_ConsigneeQualifierInfo.AddMessageError(Res.GetString("196AB18F-0754-4A3D-ACB7-F000177E4950", "Consignee Qualifier is mandatory when a Consignee Identifier has been entered."));
			}
		}

		protected void CheckULB_GoodsValue()
		{
			var deminimus = FeeCalculationHelper.GetDeminimus(Parent.Factory);
			if (Parent.ULB_GoodsValue > deminimus)
			{
				Parent.ULB_GoodsValueInfo.AddMessageError(Res.GetString("023D45B8-DAA7-4DE2-940F-BC9DEB3B5EEC", "Value cannot exceed {0:C} USD", deminimus));
			}
		}

		protected override void CheckULB_SellerName()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ULB_SellerNameInfo);
		}

		protected override void CheckULB_SellerAddress1()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ULB_SellerAddress1Info);
		}

		protected override void CheckULB_SellerCity()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ULB_SellerCityInfo);
		}

		protected override void CheckULB_ConsigneeName()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ULB_ConsigneeNameInfo);
		}

		protected override void CheckULB_ConsigneeAddress1()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ULB_ConsigneeAddress1Info);
		}

		protected override void CheckULB_ConsigneeCity()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ULB_ConsigneeCityInfo);
		}

		protected override void CheckULB_EntryType()
		{
			MandatoryValidation.CheckEntered(Parent.ULB_EntryTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ULB_EntryTypeInfo);
		}

		public void ValidateULB_GoodsValue()
		{
			ValidateCalculatedProperty(Parent.ULB_GoodsValueInfo);
		}

		protected override void CheckULB_EquipmentNumber()
		{
			base.CheckULB_EquipmentNumber();
			var parent = Parent;
			if (parent.Shipment != null && ContainerModes.IsContainerised(parent.Shipment.ULH_ContainerMode))
			{
				if (parent.ULB_EquipmentNumber.IsEmpty)
				{
					parent.ULB_EquipmentNumberInfo.AddMessageError(Res.GetString("a3e195b2-87de-49ff-b7b9-29eabcdc226f", "The container mode indicates this shipment is containerized, as yet, no containers have been entered."));
				}
				else
				{
					CheckContainerNoHasValidCharactersOnly();
					if (!parent.ULB_EquipmentNumberInfo.HasMessageErrors())
					{
						ContainerNumberValidation.WarnIfInvalid(parent.ULB_EquipmentNumberInfo);
					}
				}
			}

			if (parent.Shipment != null && parent.Shipment.ULH_TransportMode == TransportModes.Sea)
			{
				ContainerNumberValidation.WarnIfInvalid(parent.ULB_EquipmentNumberInfo);
			}
		}

		protected virtual void CheckContainerNoHasValidCharactersOnly()
		{
			if (Parent.ULB_EquipmentNumber.KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789") != Parent.ULB_EquipmentNumber)
			{
				Parent.ULB_EquipmentNumberInfo.AddMessageError(MustOnlyContainAlphaNumerics);
			}
		}
		static string MustOnlyContainAlphaNumerics
		{
			get { return Res.GetString("fb0ee5c0-70e7-4c7e-9388-c668a1348229", "Invalid Characters In Container Number - Container number must only contain alphanumeric characters."); }
		}

		protected override void CheckULB_PackType()
		{
			base.CheckULB_HouseBillIssuerSCAC();
			ListValidation.MessageErrorIfInvalidCode(Parent.ULB_PackTypeInfo, Parent.Lookups.PackTypes);
		}

		protected override void CheckULB_RN_NKConsigneeCountry()
		{
			base.CheckULB_RN_NKConsigneeCountry();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ULB_RN_NKConsigneeCountryInfo, Parent.Lookups.ConsigneeCountries);
		}

		protected override void CheckULB_RN_NKSellerCountry()
		{
			base.CheckULB_RN_NKSellerCountry();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ULB_RN_NKSellerCountryInfo, Parent.Lookups.SellerCountries);
		}

		protected override void CheckULB_HouseBillIssuerSCAC()
		{
			base.CheckULB_HouseBillIssuerSCAC();
			if (!Parent.ULB_HouseBillIssuerSCAC.IsEmpty)
			{
				if (Parent.Shipment != null)
				{
					new IssuerCarrierSCACValidator(Parent.Factory).ValidateSCACCode(Parent.ULB_HouseBillIssuerSCACInfo, Parent.Shipment.ULH_TransportMode, "Issuer", true);
				}
				CusUSLVClearanceValidation.ValidateUnknownCarrierSCAC(Parent.ULB_HouseBillIssuerSCACInfo);
			}
			else
			{
				if (Parent.Shipment != null &&
					(TransportTypeList.IsHouseBillSCACMandatory(Parent.Shipment.ULH_TransportMode) || Parent.Shipment.ULH_TransportMode == TransportTypeList.Codes.Truck))
				{
					Parent.ULB_HouseBillIssuerSCACInfo.AddMessageError(SCACCodeForHouseBillRequiredIfSeaRailOrTruck);
				}
			}
		}

		internal const string SCACCodeForHouseBillRequiredIfSeaRailOrTruck = "Standard Carrier Alpha Code (SCAC) required for House Bill (when Mode Of Transport is Sea, Rail or Truck.)";

		protected override void CheckULB_HouseBill()
		{
			base.CheckULB_HouseBill();
			var billValidator = new BillValidator();
			billValidator.CheckInvalidCharacters(Parent.ULB_HouseBillInfo, "House Bill");
		}

		#region First Item Valdation

		public void ValidateFirstCusUSLVItemProductCode()
		{
			ValidateCalculatedProperty(Parent.FirstCusUSLVItemProductCodeInfo);
		}

		protected void CheckFirstCusUSLVItemProductCode()
		{
			CusUSLVItemValidationHelper.CheckProductCode(Parent.FirstCusUSLVItem, Parent.FirstCusUSLVItemProductCodeInfo);
		}

		public void ValidateFirstCusUSLVItemLineValue()
		{
			ValidateCalculatedProperty(Parent.FirstCusUSLVItemLineValueInfo);
		}

		protected void CheckFirstCusUSLVItemLineValue()
		{
			CusUSLVItemValidationHelper.CheckLineValue(Parent.FirstCusUSLVItem, Parent.FirstCusUSLVItemLineValueInfo);
		}

		public void ValidateFirstCusUSLVItemTariff()
		{
			ValidateCalculatedProperty(Parent.FirstCusUSLVItemTariffInfo);
		}

		protected void CheckFirstCusUSLVItemTariff()
		{
			CusUSLVItemValidationHelper.CheckTariff(Parent.FirstCusUSLVItem, Parent.FirstCusUSLVItemTariffInfo);
		}

		public void ValidateFirstCusUSLVItemCountryOfOrigin()
		{
			ValidateCalculatedProperty(Parent.FirstCusUSLVItemCountryOfOriginInfo);
		}

		protected void CheckFirstCusUSLVItemCountryOfOrigin()
		{
			CusUSLVItemValidationHelper.CheckCountryOfOrigin(Parent.FirstCusUSLVItem, Parent.FirstCusUSLVItemCountryOfOriginInfo);
		}

		public void ValidateFirstCusUSLVItemCurrency()
		{
			ValidateCalculatedProperty(Parent.FirstCusUSLVItemCurrencyInfo);
		}

		protected void CheckFirstCusUSLVItemCurrency()
		{
			CusUSLVItemValidationHelper.CheckCurrency(Parent.FirstCusUSLVItem, Parent.FirstCusUSLVItemCurrencyInfo);
		}

		public void ValidateFirstCusUSLVItemAntiDumping()
		{
			ValidateCalculatedProperty(Parent.FirstCusUSLVItemAntiDumpingInfo);
		}

		protected void CheckFirstCusUSLVItemAntiDumping()
		{
			CusUSLVItemValidationHelper.CheckAntiDumping(Parent.FirstCusUSLVItem, Parent.FirstCusUSLVItemAntiDumpingInfo, Parent.FirstCusUSLVItemAntiDumping);
		}

		public void ValidateFirstCusUSLVItemCountervailing()
		{
			ValidateCalculatedProperty(Parent.FirstCusUSLVItemCountervailingInfo);
		}

		protected void CheckFirstCusUSLVItemCountervailing()
		{
			CusUSLVItemValidationHelper.CheckCountervailing(Parent.FirstCusUSLVItem, Parent.FirstCusUSLVItemCountervailingInfo, Parent.FirstCusUSLVItemCountervailing);
		}

		public void ValidateFirstCusUSLVItemGoodsDescription()
		{
			ValidateCalculatedProperty(Parent.FirstCusUSLVItemGoodsDescriptionInfo);
		}

		protected void CheckFirstCusUSLVItemGoodsDescription()
		{
			CusUSLVItemValidationHelper.CheckGoodsDescription(Parent.FirstCusUSLVItem, Parent.FirstCusUSLVItemGoodsDescriptionInfo);
		}

		#endregion

		public void ValidateITNumber()
		{
			ValidateCalculatedProperty(Parent.ITNumberInfo);
		}

		protected void CheckITNumber()
		{
			if (Parent.ITNumber != Bill.Constants.Multiple)
			{
				if (Parent.Shipment != null)
				{
					ITNumberValidator.ValidateITNumberFormat(Parent.ITNumberInfo, Parent.Shipment.IsAir);
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateULB_GoodsValue();
			ValidateFirstCusUSLVItemProductCode();
			ValidateFirstCusUSLVItemLineValue();
			ValidateFirstCusUSLVItemTariff();
			ValidateFirstCusUSLVItemCountryOfOrigin();
			ValidateFirstCusUSLVItemCurrency();
			ValidateFirstCusUSLVItemAntiDumping();
			ValidateFirstCusUSLVItemCountervailing();
			ValidateFirstCusUSLVItemGoodsDescription();
			ValidateITNumber();
		}
	}
}
