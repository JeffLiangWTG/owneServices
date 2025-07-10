//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSVehicleAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSVehicleAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USVehicleAddInfoValidation : AutoUSVehicleAddInfoValidation
	{
		public USVehicleAddInfoValidation(AutoUSVehicleAddInfo parent) : base(parent)
		{
		}

		new USVehicleAddInfo Parent
		{
			get { return (USVehicleAddInfo)base.Parent; }
		}

		bool IsPGAValidationOn
		{
			get
			{
				var vehicle = Parent.Parent;
				var invoiceLine = vehicle != null ? vehicle.InvoiceLine : null;
				var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
				return declaration != null && declaration.IsPGAValidationOn();
			}
		}

		protected override void CheckUS_CBPBondNumber()
		{
			base.CheckUS_CBPBondNumber();

			if (Parent.US_CBPBondNumber.IsEmpty)
			{
				if (IsForm_21 && Parent.US_ImportCode == ImportCodesForm3520_21List.Codes._24C)
				{
					Parent.US_CBPBondNumberInfo.AddMessageError(CBPBondRequiredFor3520_21);
				}
			}
		}
		internal const string CBPBondRequiredFor3520_21 = "For EPA 3520-21, CBP Bond Number is required when Import Code is '24C'.";

		protected override void CheckUS_FormType()
		{
			base.CheckUS_FormType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_FormTypeInfo, Parent.Lookups.FormTypeList);
			if (IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_FormTypeInfo);

				if ((IsForm_1 || IsForm_21) && Parent.Parent.VehicleAndEngineDetails.Count == 0)
				{
					Parent.US_FormTypeInfo.AddMessageError(AtLeastOneVehicleLineRequired);
				}
			}
		}
		internal const string AtLeastOneVehicleLineRequired = "At least one Vehicle and Engine Detail line is required.";

		protected override void CheckUS_VehicleModel()
		{
			base.CheckUS_VehicleModel();
			if (IsPGAValidationOn)
			{
				var vehicleCollection = Parent.Parent.VehicleAndEngineDetails;
				if (vehicleCollection.Cast<VehicleDetails>().Any(x => !x.IsEngineOnly()))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_VehicleModelInfo);
				}
			}
		}

		protected override void CheckUS_ImportCode()
		{
			base.CheckUS_ImportCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ImportCodeInfo, Parent.Lookups.ImportCodeList);
			if (IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ImportCodeInfo);
			}

			if (ImportCodesForm3520_21List.IsICIRequired(Parent.US_ImportCode) &&
				!Parent.US_IndustryCode.IsEmpty &&
				Parent.US_IndustryCode != IndustryCodesList.Codes.A &&
				Parent.US_IndustryCode != IndustryCodesList.Codes.D)
			{
				Parent.US_ImportCodeInfo.AddMessageError(ImportCode24);
			}

			ValidateUS_EnginePower();
			ValidateUS_EnginePowerUQ();
			ValidateUS_ExemptionRemarks();
			ValidateUS_BondExemption();
			ValidateUS_CBPBondNumber();
			ValidateUS_ModelYear();
			ValidateUS_CertOfConformity();
			ValidateUS_VNEElectronicImage();
			ValidateUS_VehicleExemptionNumber();
			ValidateUS_OA_StorageLocation();
			ValidateUS_BondPolicyNo();
			ValidateUS_NAICNo();
			ValidateUS_StateOfIssue();
			ValidateUS_EPARegNumber();
		}
		internal const string ImportCode24 = "Import codes 24A, 24B or 24C are applicable for Industry codes 'A' and 'D' only.";

		protected override void CheckUS_IndustryCode()
		{
			base.CheckUS_IndustryCode();
			if (IsForm_21 && IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IndustryCodeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.US_IndustryCodeInfo, Parent.Lookups.IndustryCodeList);
			ValidateUS_ImportCode();
		}

		protected override void CheckUS_ModelYear()
		{
			base.CheckUS_ModelYear();
			if (!Parent.US_ModelYear.IsEmpty)
			{
				if (!Parent.US_ModelYear.IsNumbersOnlyOrEmpty)
				{
					Parent.US_ModelYearInfo.AddMessageError(ModelYearFormat);
				}

				if (Parent.US_ModelYear.IsNumbersOnlyOrEmpty &&
					(ZInt.ParseSafe(Parent.US_ModelYear, 0) < 1960 || ZInt.ParseSafe(Parent.US_ModelYear, 0) > 2050))
				{
					Parent.US_ModelYearInfo.AddMessageError(ModelYearRange);
				}
			}
			else
			{
				if (IsForm_1 && ImportCodesForm3520_1List.IsModelYearRequired(Parent.US_ImportCode))
				{
					Parent.US_ModelYearInfo.AddMessageError(ModelYearRequiredFor3520_1);
				}
				else if (IsForm_21)
				{
					if (ImportCodesForm3520_21List.IsModelYearRequired(Parent.US_ImportCode))
					{
						Parent.US_ModelYearInfo.AddMessageError(ModelYearRequiredFor3520_21);
					}
					else if (Parent.US_ImportCode == ImportCodesForm3520_21List.Codes._01 && Parent.US_IndustryCode == IndustryCodesList.Codes.G)
					{
						Parent.US_ModelYearInfo.AddMessageError(ModelYearRequiredForImportCode_01);
					}
				}
			}
		}
		internal const string ModelYearRequiredFor3520_1 = "For EPA 3520-1, Model Year is required when Import Code is 'A', 'C', 'U', 'Y' or 'Z'.";
		internal const string ModelYearRequiredFor3520_21 = "For EPA 3520-21, Model Year is required when Import Code is '9', '17', '19', '22', '24A' or '24B'.";
		internal const string ModelYearRequiredForImportCode_01 = "For EPA 3520-21, Model Year is required when Import Code is '1' and Industry Code is 'G'.";
		internal const string ModelYearFormat = "Model Year should be 4 digits, in a format CCYY, where CC is century and YY is year.";
		internal const string ModelYearRange = "Model Year should be a year between 1960 and 2050.";

		protected override void CheckUS_BodyType()
		{
			base.CheckUS_BodyType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_BodyTypeInfo, Parent.Lookups.BodyTypeList);
		}

		protected override void CheckUS_BodyCode()
		{
			base.CheckUS_BodyCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_BodyCodeInfo, Parent.Lookups.BodyCodeList);
		}

		protected override void CheckUS_DrvSide()
		{
			base.CheckUS_DrvSide();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DrvSideInfo, Parent.Lookups.DrvSideList);
		}

		protected override void CheckUS_MilitaryEq()
		{
			base.CheckUS_MilitaryEq();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_MilitaryEqInfo, Parent.Lookups.YesNoList);
		}

		protected override void CheckUS_CertOfConformity()
		{
			base.CheckUS_CertOfConformity();

			if (Parent.US_CertOfConformity.IsEmpty)
			{
				if (IsForm_1 && ImportCodesForm3520_1List.IsEngineFamilyNumberRequired(Parent.US_ImportCode))
				{
					Parent.US_CertOfConformityInfo.AddMessageError(EngineFamilyNumberRequiredFor3520_1);
				}
				else if (IsForm_21 && ImportCodesForm3520_21List.IsEngineFamilyNumberRequired(Parent.US_ImportCode))
				{
					Parent.US_CertOfConformityInfo.AddMessageError(EngineFamilyNumberRequiredFor3520_21);
				}
			}
			else if (Parent.US_CertOfConformity.Length != 12)
			{
				Parent.US_CertOfConformityInfo.AddMessageError(LengthShouldBe12);
			}
		}
		internal const string EngineFamilyNumberRequiredFor3520_1 = "For EPA 3520-1, Engine Family Number is required when Import Code is 'B'or 'F'.";
		internal const string EngineFamilyNumberRequiredFor3520_21 = "For EPA 3520-21, Engine Family Number is required when Import Code is '1' or '22'.";
		internal const string LengthShouldBe12 = "Engine Family Name should be 12 characters";

		protected override void CheckUS_VehicleExemptionNumber()
		{
			base.CheckUS_VehicleExemptionNumber();

			if (Parent.US_VehicleExemptionNumber.IsEmpty)
			{
				if (IsForm_1)
				{
					if (ImportCodesForm3520_1List.IsExemptionNumberRequired(Parent.US_ImportCode))
					{
						Parent.US_VehicleExemptionNumberInfo.AddMessageError(ExemptionNumberRequiredFor3520_1);
					}
					else if (Parent.US_ImportCode == ImportCodesForm3520_1List.Codes.M)
					{
						Parent.US_VehicleExemptionNumberInfo.AddWarning(ExemptionNumberRequiredForImportCodeM);
					}
				}
				else if (IsForm_21 && ImportCodesForm3520_21List.IsExemptionNumberRequired(Parent.US_ImportCode))
				{
					Parent.US_VehicleExemptionNumberInfo.AddMessageError(ExemptionNumberRequiredFor3520_21);
				}
			}
		}
		internal const string ExemptionNumberRequiredFor3520_1 = "For EPA 3520-1, Exemption Number is required when Import Code is 'G', 'I', 'K', 'L' or 'O'.";
		internal const string ExemptionNumberRequiredFor3520_21 = "For EPA 3520-21, Exemption Number is required when Import Code is '2', '10', '11', '12' or '18'";
		internal const string ExemptionNumberRequiredForImportCodeM = "Required if Canadian vehicle received by US resident OR importer is either permanently emigrating to the US or will reside for greater than one year with a student or work visa.";

		protected override void CheckUS_OA_Owner()
		{
			base.CheckUS_OA_Owner();
			var vehicle = Parent.Parent;
			if (IsPGAValidationOn)
			{
				if (IsForm_1)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OA_OwnerInfo);
				}

				if (vehicle != null)
				{
					var ownerAddress = vehicle.OwnerAddress;
					OrganisationValidation.ValidatePGAContact(Parent.US_OA_OwnerInfo, vehicle.OwnerWrapper);
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_OwnerInfo, ownerAddress);
					OrganisationValidation.ValidateCharactorsForAddressDescription(vehicle.US_OA_OwnerInfo, ownerAddress);
				}
			}
		}

		protected override void CheckUS_OA_StorageLocation()
		{
			base.CheckUS_OA_StorageLocation();
			var vehicle = Parent.Parent;
			if (IsPGAValidationOn)
			{
				if (IsForm_21 && ImportCodesForm3520_21List.IsStorageLocationRequired(Parent.US_ImportCode) && Parent.US_OA_StorageLocation.IsEmpty)
				{
					Parent.US_OA_StorageLocationInfo.AddMessageError(StorageLocationRequired);
				}

				if (vehicle.StorageLocationAddress != null)
				{
					OrganisationValidation.ValidateCharactorsForAddressDescription(vehicle.US_OA_StorageLocationInfo, vehicle.StorageLocationAddress);
				}
			}
			if (vehicle != null)
			{
				var storageLocationAddress = vehicle.StorageLocationAddress;
				OrganisationValidation.ValidatePGAContact(Parent.US_OA_StorageLocationInfo, vehicle.StorageLocationWrapper);
				OrganisationValidation.ValidatePostCodeForAddress(Parent.US_OA_StorageLocationInfo, storageLocationAddress);
				OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_StorageLocationInfo, storageLocationAddress);
			}
		}
		internal const string StorageLocationRequired = "Storage Location is required when Import Code is '24A' or '24B'.";

		protected override void CheckUS_EPARegNumber()
		{
			base.CheckUS_EPARegNumber();
			if (IsForm_1 && ImportCodesForm3520_1List.IsImportedByICI(Parent.US_ImportCode) && Parent.US_EPARegNumber.IsEmpty)
			{
				Parent.US_EPARegNumberInfo.AddMessageError(EPARegNoRequiredForICI);
			}
		}
		internal const string EPARegNoRequiredForICI = "EPA Registration Number is required when Import Code is 'A', 'C', 'J' or 'Z'.";

		protected override void CheckUS_BondExemption()
		{
			base.CheckUS_BondExemption();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_BondExemptionInfo, Parent.Lookups.YesNoList);

			if (IsForm_21 && IsImportCode_01CombinedWithIndustryCode_GOrImportCode_22 && Parent.US_BondExemption.IsEmpty)
			{
				Parent.US_BondExemptionInfo.AddMessageError(BondExemptionRequired);
			}

			ValidateUS_BondPolicyNo();
			ValidateUS_NAICNo();
			ValidateUS_StateOfIssue();
		}
		internal const string BondExemptionRequired = "Exempt from bond indicator is required when Import Code is '1' and Industry Code is 'G' or Import Code is '22'.";

		protected override void CheckUS_BondPolicyNo()
		{
			base.CheckUS_BondPolicyNo();

			if (Parent.US_BondPolicyNo.IsEmpty && !IsExemptFromBond)
			{
				if (IsForm_1 && ImportCodesForm3520_1List.IsPolicyNumberRequired(Parent.US_ImportCode))
				{
					Parent.US_BondPolicyNoInfo.AddMessageError(BondPolicyNoRequired3520_1);
				}
				else if (IsForm_21 && IsImportCode_01CombinedWithIndustryCode_GOrImportCode_22)
				{
					Parent.US_BondPolicyNoInfo.AddMessageError(BondPolicyNoRequired3520_21);
				}
			}
		}
		internal const string BondPolicyNoRequired3520_1 = "For EPA 3520-1, Bond Policy Number is required when Import Code is 'G', 'I', 'K' or 'J'.";
		internal const string BondPolicyNoRequired3520_21 = "For EPA 3520-21, Bond Policy Number is required when Import Code is '1' and Industry Code is 'G' or Import Code is '22'.";

		protected override void CheckUS_NAICNo()
		{
			base.CheckUS_NAICNo();
			if (IsForm_21 && IsImportCode_01CombinedWithIndustryCode_GOrImportCode_22 && Parent.US_NAICNo.IsEmpty && !IsExemptFromBond && Parent.US_BondExemption != "Y")
			{
				Parent.US_NAICNoInfo.AddMessageError(NAICNoRequired);
			}
		}
		internal const string NAICNoRequired = "Bond Issuer NAIC Number is required when Import Code is '1' and Industry Code is 'G' or Import Code is '22'.";

		protected override void CheckUS_StateOfIssue()
		{
			base.CheckUS_StateOfIssue();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_StateOfIssueInfo, Parent.Lookups.USStatesList);

			if (Parent.US_StateOfIssue.IsEmpty && !IsExemptFromBond)
			{
				if (IsForm_21 && Parent.US_ImportCode == ImportCodesForm3520_21List.Codes._22)
				{
					Parent.US_StateOfIssueInfo.AddMessageError(StateRequired);
				}
			}
		}
		internal const string StateRequired = "State Of Issue is required when Import Code is '22'.";

		protected override void CheckUS_EnginePower()
		{
			base.CheckUS_EnginePower();

			if (Parent.US_EnginePower.IsEmpty)
			{
				if (IsForm_1 && Parent.US_ImportCode == ImportCodesForm3520_1List.Codes.U)
				{
					Parent.US_EnginePowerInfo.AddMessageError(EnginePowerRequiredFor3520_1);
				}
				else if (IsForm_21 && ImportCodesForm3520_21List.IsEnginePowerRequired(Parent.US_ImportCode))
				{
					Parent.US_EnginePowerInfo.AddMessageError(EnginePowerRequiredFor3520_21);
				}
			}

			ValidateUS_EnginePowerUQ();
		}
		internal const string EnginePowerRequiredFor3520_1 = "For EPA 3520-1, Engine Power is required when Import Code is 'U'.";
		internal const string EnginePowerRequiredFor3520_21 = "For EPA 3520-21, Engine Power is required when Import Code is '19', '22' or '23'.";

		protected override void CheckUS_EnginePowerUQ()
		{
			base.CheckUS_EnginePowerUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_EnginePowerUQInfo, Parent.Lookups.EnginePowerUQ);

			if (!Parent.US_EnginePower.IsEmpty && Parent.US_EnginePowerUQ.IsEmpty)
			{
				Parent.US_EnginePowerUQInfo.AddMessageError(EnginePowerUQRequired);
			}

			if (Parent.US_EnginePower.IsEmpty && !Parent.US_EnginePowerUQ.IsEmpty)
			{
				Parent.US_EnginePowerUQInfo.AddMessageError(EnginePowerUQNotRequired);
			}
		}
		internal const string EnginePowerUQRequired = "Engine Power UQ is required if Engine Power is entered.";
		internal const string EnginePowerUQNotRequired = "Engine Power UQ is not required if Engine Power is not entered.";

		protected override void CheckUS_ExemptionRemarks()
		{
			base.CheckUS_ExemptionRemarks();

			if (IsForm_21 && ImportCodesForm3520_21List.IsExemptionRemarksRequired(Parent.US_ImportCode) && Parent.US_ExemptionRemarks.IsEmpty)
			{
				Parent.US_ExemptionRemarksInfo.AddMessageError(ExemptionRemarksRequired);
			}
		}
		internal const string ExemptionRemarksRequired = "Exemption Remarks are required if Import Code is '21' or '25'.";

		protected override void CheckUS_CertifyingIndividual()
		{
			base.CheckUS_CertifyingIndividual();
			if (IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CertifyingIndividualInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CertifyingIndividualInfo, Parent.Lookups.VNECertifyingIndividualList);

			ValidateUS_ContactName();
			ValidateUS_ContactPhoneNo();
			ValidateUS_ContactEmail();
		}

		protected override void CheckUS_ContactName()
		{
			base.CheckUS_ContactName();
			if (IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ContactNameInfo);
			}
		}

		protected override void CheckUS_ContactPhoneNo()
		{
			base.CheckUS_ContactPhoneNo();

			if (IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ContactPhoneNoInfo);
			}
		}

		protected override void CheckUS_ContactEmail()
		{
			base.CheckUS_ContactEmail();

			if (IsPGAValidationOn)
			{
				if (!Parent.US_ContactEmail.IsEmpty)
				{
					if (!EmailAddressValidation.IsEmailAddressValid(Parent.US_ContactEmail))
					{
						Parent.US_ContactEmailInfo.AddWarning("Invalid email format");
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ContactEmailInfo);
				}
			}
		}

		bool IsExemptFromBond => Parent.US_BondExemption == YesNoDefaultList.Codes.Yes;

		bool IsImportCode_01CombinedWithIndustryCode_GOrImportCode_22
		{
			get
			{
				var importCode = Parent.US_ImportCode;
				return (importCode == ImportCodesForm3520_21List.Codes._01 && Parent.US_IndustryCode == IndustryCodesList.Codes.G) || importCode == ImportCodesForm3520_21List.Codes._22;
			}
		}

		bool IsForm_1
		{
			get
			{
				var vehicle = Parent.Parent;
				return vehicle != null && vehicle.IsForm_1;
			}
		}

		bool IsForm_21
		{
			get
			{
				var vehicle = Parent.Parent;
				return vehicle != null && vehicle.IsForm_21;
			}
		}
	}
}
