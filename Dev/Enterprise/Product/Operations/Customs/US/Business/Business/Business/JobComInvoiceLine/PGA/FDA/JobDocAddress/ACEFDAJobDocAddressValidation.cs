using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.US.Business.USACEFDAAddInfoValidation;

namespace Enterprise.Customs.US.Business
{
	public class ACEFDAJobDocAddressValidation : JobDocAddressValidation
	{
		protected readonly ACEFDA fda;

		public ACEFDAJobDocAddressValidation(AutoJobDocAddress parent, ACEFDA fda) : base(parent)
		{
			Argument.NotNull(fda, nameof(fda));
			this.fda = fda;
		}

		protected new ACEFDAJobDocAddress Parent
		{
			get { return (ACEFDAJobDocAddress)base.Parent; }
		}

		#region CheckE2_OA_Address

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.Laboratory:
					CheckE2_OA_AddressForLaboratory();
					break;
				case DocAddressTypes.Codes.Manufacturer:
					CheckE2_OA_AddressForManufacturer();
					break;
				case DocAddressTypes.Codes.GoodsDeliveredTo:
					CheckE2_OA_AddressForDeliverToParty();
					break;
				case DocAddressTypes.Codes.FSVPImporter:
					CheckE2_OA_AddressForFSVPImporter();
					break;
				case DocAddressTypes.Codes.Shipper:
					CheckE2_OA_AddressForShipper();
					break;
				case DocAddressTypes.Codes.ImporterDocumentaryAddress:
					CheckE2_OA_AddressForFDAImporter();
					break;
				case DocAddressTypes.Codes.GoodsOwner:
					CheckE2_OA_AddressForGoodsOwner();
					break;
				case DocAddressTypes.Codes.GoodsLocation:
					CheckE2_OA_AddressForGoodsLocation();
					break;
			}
		}

		protected virtual void CheckE2_OA_AddressForLaboratory()
		{
			CheckE2_OA_AddressForCommonValidations();
		}

		protected virtual void CheckE2_OA_AddressForManufacturer()
		{
			CheckE2_OA_AddressForCommonValidations();
		}

		protected virtual void CheckE2_OA_AddressForDeliverToParty()
		{
			CheckE2_OA_AddressForCommonValidations();
			var parent = Parent;
			OrganisationValidation.ValidateCountryForPGAAddress(parent.E2_OA_AddressInfo, parent, (x) => { return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(x) == Core.Constants.CountryCodes.UnitedStates; }, DeliveryToPartyShouldBeUSAddress);
		}

		protected virtual void CheckE2_OA_AddressForFSVPImporter()
		{
			var parent = Parent;
			if (parent.HasRealAddress)
			{
				if (fda.IsFSVPImpRequired)
				{
					ZipCodeValidation.ValidateForEmptyZIPForUSAddress(Parent.E2_OA_AddressInfo, parent);
					var address = parent.Address;
					var duns = address.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedStates);
					if (duns.IsEmpty)
					{
						parent.E2_OA_AddressInfo.AddMessageError(DUNSCodeRequired);
					}
					else
					{
						OrganisationValidation.ValidateACEFDAOrganisationCodes(parent.E2_OA_AddressInfo, fda.FSVPImporterNumber);
					}
				}
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.E2_OA_AddressInfo, parent.Address);
				OrganisationValidation.ValidateStateForPGAAddress(parent.E2_OA_AddressInfo, parent.Address, ShouldCheckStateForAddress);
				OrganisationValidation.ValidateCountryForPGAAddress(parent.E2_OA_AddressInfo, parent, (x) => { return x == Core.Constants.CountryCodes.UnitedStates; }, FSVPMustHaveUSAddress);
			}
		}

		protected virtual void CheckE2_OA_AddressForShipper()
		{
			CheckE2_OA_AddressForCommonValidations();
			var parent = Parent;
			if (ShouldCheckCountryForShipper && parent.HasRealAddress)
			{
				OrganisationValidation.ValidateCountryForPGAAddress(parent.E2_OA_AddressInfo, parent, (x) => { return x == Core.Constants.CountryCodes.Canada; }, DRU_804_ShouldBeCanadaAddress);
			}
		}
		bool ShouldCheckCountryForShipper => fda.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_804;

		protected virtual void CheckE2_OA_AddressForFDAImporter()
		{
			CheckE2_OA_AddressForCommonValidations();
			var parent = Parent;
			if (parent.HasRealAddress)
			{
				ValidateForZipCode(fda.Factory, parent.E2_OA_AddressInfo, parent);
			}
		}

		void ValidateForZipCode(BusinessObjectFactory factory, ZPropertyInfo propertyInfo, IAddressDetails address)
		{
			var additionalZipCodeMessage = new ZipCodeValidation().ValidateForZipCode(factory, address.PostCode, address.State, address.Country);
			if (!string.IsNullOrEmpty(additionalZipCodeMessage))
			{
				propertyInfo.AddMessageError(additionalZipCodeMessage);
			}
		}

		protected virtual void CheckE2_OA_AddressForGoodsOwner()
		{
			CheckE2_OA_AddressForCommonValidations();
		}

		void CheckE2_OA_AddressForGoodsLocation()
		{
			var parent = Parent;
			if (parent.HasRealAddress)
			{
				if (ShouldCheckCountryForGoodsLocation)
				{
					CheckE2_OA_AddressForCommonValidations(false);
					OrganisationValidation.ValidateCountryForPGAAddress(Parent.E2_OA_AddressInfo, Parent, (x) => { return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(x) == Core.Constants.CountryCodes.UnitedStates; }, GoodsLocationShouldBeUSAddress);
				}
				else
				{
					Parent.E2_OA_AddressInfo.AddMessageError(GoodsLocationNotRequired);
				}
			}
		}
		bool ShouldCheckCountryForGoodsLocation => fda.US_ProgramCode != FDAProgramCodeList.Codes.DEV;

		void CheckE2_OA_AddressForCommonValidations(bool validateOrgCode = true)
		{
			var parent = Parent;
			if (parent.HasRealAddress)
			{
				if (validateOrgCode)
				{
					OrganisationValidation.ValidateACEFDAOrganisationCodes(parent.E2_OA_AddressInfo, GetAddressNumber());
				}
				var address = Parent.Address;
				ZipCodeValidation.ValidateForEmptyZIPForUSAddress(parent.E2_OA_AddressInfo);
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.E2_OA_AddressInfo, parent.Address);
				OrganisationValidation.ValidateStateForPGAAddress(parent.E2_OA_AddressInfo, parent.Address, ShouldCheckStateForAddress);
			}
		}

		OrgCusCodeForFDA GetAddressNumber()
		{
			return OrgCusCodeForFDA.FindCustomsNumberAndID(Parent.Address, fda.US_ProgramCode);
		}

		#endregion

		protected override void CheckE2_RN_NKCountryCode()
		{
			base.CheckE2_RN_NKCountryCode();
			var parent = Parent;
			if (parent.E2_AddressOverride)
			{
				switch (parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.GoodsDeliveredTo:
						OrganisationValidation.ValidateCountryForPGAAddress(Parent.E2_RN_NKCountryCodeInfo, parent, (x) => { return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(x) == Core.Constants.CountryCodes.UnitedStates; }, DeliveryToPartyShouldBeUSAddress);
						break;
					case DocAddressTypes.Codes.FSVPImporter:
						OrganisationValidation.ValidateCountryForPGAAddress(Parent.E2_RN_NKCountryCodeInfo, parent, (x) => { return x == Core.Constants.CountryCodes.UnitedStates; }, FSVPMustHaveUSAddress);
						break;
					case DocAddressTypes.Codes.Shipper:
						if (ShouldCheckCountryForShipper)
						{
							OrganisationValidation.ValidateCountryForPGAAddress(Parent.E2_RN_NKCountryCodeInfo, parent, (x) => { return x == Core.Constants.CountryCodes.Canada; }, DRU_804_ShouldBeCanadaAddress);
						}
						break;
					case DocAddressTypes.Codes.GoodsLocation:
						if (ShouldCheckCountryForGoodsLocation)
						{
							OrganisationValidation.ValidateCountryForPGAAddress(Parent.E2_RN_NKCountryCodeInfo, parent, (x) => { return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(x) == Core.Constants.CountryCodes.UnitedStates; }, GoodsLocationShouldBeUSAddress);
						}
						break;
				}
			}
		}

		protected override void CheckE2_Postcode()
		{
			base.CheckE2_Postcode();
			var parent = Parent;
			if (parent.E2_AddressOverride)
			{
				switch (parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.ImporterDocumentaryAddress:
						ValidateForZipCode(fda.Factory, parent.E2_PostcodeInfo, parent);
						break;
				}
			}
		}

		protected override void CheckE2_GovRegNum()
		{
			base.CheckE2_GovRegNum();
			var parent = Parent;

			if (parent.E2_AddressOverride)
			{
				if (!parent.E2_GovRegNum.IsEmpty)
				{
					var error = (string)parent.E2_GovRegNumType switch
					{
						OrgCusCode.CodeTypes.FDAEstablishmentIdentifier => EstablishmentIdentifierValidator.Validate(parent.E2_GovRegNum),
						OrgCusCode.CodeTypes.DataUniversalNumberingSystem => DataUniversalNumberingSystemValidator.GetDUNSNumberError(parent.E2_GovRegNum),
						_ => null
					};

					if (!string.IsNullOrEmpty(error))
					{
						parent.E2_GovRegNumInfo.AddWarning(error);
					}
				}
				else if (!parent.E2_GovRegNumType.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.E2_GovRegNumInfo);
				}
			}
		}

		protected override void CheckE2_GovRegNumType()
		{
			base.CheckE2_GovRegNumType();
			var parent = Parent;

			if (parent.E2_AddressOverride)
			{
				if (parent.IsFSVPImporter && parent.E2_GovRegNumType.IsEmpty)
				{
					parent.E2_GovRegNumTypeInfo.AddMessageError(DUNSNumberIsRequiredMessage);
				}
				else if (!parent.E2_GovRegNumType.IsEmpty || !parent.E2_GovRegNum.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.E2_GovRegNumTypeInfo);
				}
			}
		}

		internal const string DUNSNumberIsRequiredMessage = "A DUNS Number is required for PGA reporting.";
	}
}
