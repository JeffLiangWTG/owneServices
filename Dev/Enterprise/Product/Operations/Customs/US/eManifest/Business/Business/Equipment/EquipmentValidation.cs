using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	public class EquipmentValidation : CusInBondEquipmentValidation
	{
		public EquipmentValidation(Equipment parent)
			: base(parent)
		{
		}

		new Equipment Parent
		{
			get { return (Equipment)base.Parent; }
		}

		protected override void CheckBJ_IsConveyance()
		{
			base.CheckBJ_IsConveyance();
			// Only one conveyance allowed
			if (Parent.BJ_IsConveyance && (Parent.Trip?.AllEquipmentIncludingMainConveyance.OfType<Equipment>().Any(x => x.BJ_IsConveyance && x.PK != Parent.PK) ?? false))
			{
				Parent.BJ_IsConveyanceInfo.AddMessageError("Only one record should be marked as the Conveyance");
			}
		}

		#region CheckBJ_RQ_Equipment

		protected override void CheckBJ_RQ_Equipment()
		{
			base.CheckBJ_RQ_Equipment();
			RefEquipmentValidation.Validate(Parent);
		}

		protected override void CheckBJ_RQ_EquipmentIsValidZGuid()
		{
			TypeValidation.CheckValidGuid(Parent.BJ_RQ_EquipmentInfo, Parent.HumanReadableName);
		}

		#endregion

		protected override void CheckBJ_ACEID()
		{
			base.CheckBJ_ACEID();
			if (Parent.RefEquipment == null)
			{
				RelatedObjectValidation.MaxLengthValidation(Parent.BJ_ACEID, 10, Parent.BJ_ACEIDInfo, RefEquipmentValidation.EquipmentACEIDCaption);
			}
		}

		protected override void CheckBJ_RC_RoadContainerType()
		{
			base.CheckBJ_RC_RoadContainerType();
			if (Parent.RefEquipment == null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BJ_RC_RoadContainerTypeInfo, RefEquipmentValidation.EquipmentTypeCaption);
				var refContainer = Parent.RoadContainerType;
				if (refContainer != null)
				{
					if (refContainer.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates).IsEmpty)
					{
						Parent.BJ_RC_RoadContainerTypeInfo.AddMessageError(RefEquipmentValidation.GetMissingEquipmentType(RefEquipmentValidation.EquipmentTypeCaption, refContainer.RC_Code));
					}
					else
					{
						var validTypes = Parent.BJ_IsConveyance ? Parent.Lookups.ConveyanceTypes : Parent.Lookups.EquipmentTypes;
						if (!validTypes.ContainsCode(refContainer.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates)))
						{
							Parent.BJ_RC_RoadContainerTypeInfo.AddMessageError(RefEquipmentValidation.GetInvalidEquipmentType(RefEquipmentValidation.EquipmentTypeCaption, refContainer.RC_Code).ToString());
						}
					}
				}
			}
		}

		protected override void CheckBJ_RegistrationNumber()
		{
			base.CheckBJ_RegistrationNumber();

			if (Parent.RefEquipment == null && Parent.BJ_ACEID.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BJ_RegistrationNumberInfo, RefEquipmentValidation.EquipmentNumberCaption);
			}
		}

		bool IsLicensePlateRequired()
		{
			var equipmentType = ZString.Empty;
			var roadContainerType = Parent?.RoadContainerType;
			if (roadContainerType != null)
			{
				equipmentType = roadContainerType.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
			}
			return Parent.Lookups.EquipmentTypes.ContainsCode(equipmentType) && EquipmentTypes.IsLicensePlateRequired(Parent.Factory, equipmentType);
		}

		protected override void CheckBJ_RW_NKRegistrationState()
		{
			base.CheckBJ_RW_NKRegistrationState();
			if (Parent.RefEquipment == null && Parent.BJ_ACEID.IsEmpty)
			{
				if (Parent.BJ_IsConveyance || IsLicensePlateRequired())
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BJ_RW_NKRegistrationStateInfo, RefEquipmentValidation.StateProvinceOfRegistrationCaption);
				}
			}
		}

		protected override void CheckBJ_RN_NKRegistrationCountry()
		{
			base.CheckBJ_RN_NKRegistrationCountry();
			if (Parent.RefEquipment == null && Parent.BJ_ACEID.IsEmpty)
			{
				if (Parent.BJ_IsConveyance || IsLicensePlateRequired())
				{
					RelatedObjectValidation.MaxLengthValidation(Parent.BJ_RN_NKRegistrationCountry, 2, Parent.BJ_RN_NKRegistrationCountryInfo, RefEquipmentValidation.CountryOfRegistrationCaption, true);
				}
			}
		}

		protected override void CheckBJ_VIN()
		{
			base.CheckBJ_VIN();
			if (Parent.RefEquipment == null && Parent.BJ_IsConveyance)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BJ_VINInfo, RefEquipmentValidation.VINCaption);
			}
		}

		#region CheckBJ_InsuranceAmount

		protected override void CheckBJ_InsuranceAmount()
		{
			base.CheckBJ_InsuranceAmount();
			ValidateInsuranceDetailsMandatoryIfHazmatShipment(Parent.BJ_InsuranceAmountInfo);
		}

		void ValidateInsuranceDetailsMandatoryIfHazmatShipment(ZPropertyInfo info)
		{
			if (Parent.BJ_IsConveyance)
			{
				var trip = Parent.Trip;
				if (trip != null && trip.HasHazmatShipments && info.Value.IsEmpty)
				{
					var propertyDescription = RefEquipmentValidation.GetDescriptionWithPrefix(info, true);
					info.AddMessageError(Res.GetString("5793d5cc-d790-4e2f-b6be-8e99fdabf7d5", "{0} is required for hazardous shipments", propertyDescription));
				}
			}
		}

		#endregion

		#region CheckBJ_InsuranceName

		protected override void CheckBJ_InsuranceName()
		{
			base.CheckBJ_InsuranceName();
			ValidateInsuranceDetailsMandatoryIfHazmatShipment(Parent.BJ_InsuranceNameInfo);
		}

		#endregion

		#region CheckBJ_InsurancePolicyNumber

		protected override void CheckBJ_InsurancePolicyNumber()
		{
			base.CheckBJ_InsurancePolicyNumber();
			ValidateInsuranceDetailsMandatoryIfHazmatShipment(Parent.BJ_InsurancePolicyNumberInfo);
		}

		#endregion

		#region CheckBJ_InsuranceYearPolicyIssue

		protected override void CheckBJ_InsuranceYearPolicyIssue()
		{
			base.CheckBJ_InsuranceYearPolicyIssue();
			ValidateInsuranceDetailsMandatoryIfHazmatShipment(Parent.BJ_InsuranceYearPolicyIssueInfo);
		}

		#endregion
	}
}
