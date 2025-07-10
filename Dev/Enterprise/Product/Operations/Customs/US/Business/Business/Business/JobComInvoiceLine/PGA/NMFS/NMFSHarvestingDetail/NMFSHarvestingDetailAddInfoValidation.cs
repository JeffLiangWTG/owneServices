using System.Globalization;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class NMFSHarvestingDetailAddInfoValidation : USNMFSHarvestingDetailAddInfoValidation
	{
		public NMFSHarvestingDetailAddInfoValidation(USNMFSHarvestingDetailAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_GearType()
		{
			base.CheckUS_GearType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_GearTypeInfo, Parent.Lookups.GearTypeList);
			if (!IsSIMProgramTypeAndHBASourceType && (Is370orHMSPgaRequiredValidation || (IsSIMPOrCOAPgaRequiredValidation && NMFSLine.RequiresFullData)))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_GearTypeInfo);
			}
		}

		protected override void CheckUS_HarvestedCountry()
		{
			base.CheckUS_HarvestedCountry();
			if (IsCOARequiredValidation)
			{
				if (Parent.US_HarvestedCountry == NMFSConstants.InternationalWaters || Parent.US_HarvestedCountry == NMFSConstants.InternationalWatersInstallations)
				{
					Parent.US_HarvestedCountryInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_HarvestedCountryInfo, Parent.Lookups.Countries);

					if (NMFSLine.RequiresFullData)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.US_HarvestedCountryInfo);
					}

					if (IsOriginCountrySameAsHarvestCountry)
					{
						var message = Res.GetString("1405BCDC-D337-4006-B1C5-422A8A7BF02D", "The Harvesting Country cannot be the same as the Origin Country.");
						Parent.US_HarvestedCountryInfo.AddMessageError(message);
					}
				}
			}
			else if (Parent.US_HarvestedCountry != NMFSConstants.InternationalWaters)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_HarvestedCountryInfo, Parent.Lookups.Countries);

				if (Is370orHMSPgaRequiredValidation || (IsSIMPOrCOAPgaRequiredValidation && NMFSLine.RequiresFullData) || IsExportNMFSRequired)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_HarvestedCountryInfo);
				}
			}
		}

		protected override void CheckUS_OceanAreaOfCatch()
		{
			base.CheckUS_OceanAreaOfCatch();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_OceanAreaOfCatchInfo, Parent.Lookups.OceanAreaCodeList);

			if (Parent.US_HarvestedCountry == NMFSConstants.InternationalWaters && (Is370orHMSPgaRequiredValidation || (IsSIMPOrCOAPgaRequiredValidation && NMFSLine.RequiresFullData) || IsExportNMFSRequired))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OceanAreaOfCatchInfo);
			}
		}

		protected override void CheckUS_VesselCountry()
		{
			base.CheckUS_VesselCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_VesselCountryInfo, Parent.Lookups.Countries);

			if (Is370orHMSPgaRequiredValidation || IsExportNMFSRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_VesselCountryInfo);
			}
		}

		protected override void CheckUS_GearStartDate()
		{
			base.CheckUS_GearStartDate();

			if (IsSIMPOrCOAPgaRequiredValidation && NMFSLine.RequiresFullData)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_GearStartDateInfo);
			}
		}

		protected override void CheckUS_GearDescription()
		{
			base.CheckUS_GearDescription();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_GearDescriptionInfo, Parent.Lookups.GearDescriptions);

			if (IsSIMPOrCOAPgaRequiredValidation && NMFSLine.RequiresFullData)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_GearDescriptionInfo);
			}
		}

		protected override void CheckUS_ContactPartyType()
		{
			base.CheckUS_ContactPartyType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ContactPartyTypeInfo, Parent.Lookups.ContactPartyTypes);

			if (IsSIMPOrCOAPgaRequiredValidation && NMFSLine.RequiresFullData)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ContactPartyTypeInfo);
			}
		}

		protected override void CheckUS_OA_ContactParty()
		{
			base.CheckUS_OA_ContactParty();
			var contactParty = HarvestingDetail?.ContactParty;
			if (IsSIMPOrCOAPgaRequiredValidation)
			{
				if (NMFSLine.RequiresFullData)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OA_ContactPartyInfo);
				}

				if (contactParty != null)
				{
					ValidatePGAContact(Parent.US_OA_ContactPartyInfo, OrgHeaderWrapper.New(contactParty));
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_ContactPartyInfo, contactParty);
					OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.US_OA_ContactPartyInfo, contactParty);
				}
			}
		}

		void ValidatePGAContact(ZPropertyInfo propertyInfo, IPGAContactDetails wrapper)
		{
			if (string.IsNullOrWhiteSpace(wrapper.Name))
			{
				propertyInfo.AddMessageError(string.Format(CultureInfo.InvariantCulture, OrganisationValidation.ContactInformationForCustomsIsMissing, "Name"));
			}

			if (string.IsNullOrWhiteSpace(wrapper.PhoneNumber) && string.IsNullOrWhiteSpace(wrapper.EmailAddress) && string.IsNullOrWhiteSpace(wrapper.Fax))
			{
				var message = Res.GetString("8143763a-75cf-4701-9677-b49b30619e5e", "Either the work phone number or the email/fax is required for PGA reporting. Please add at least one data as USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact.");
				propertyInfo.AddMessageError(message);
			}
		}

		protected override void CheckUS_NoSmallVessels()
		{
			base.CheckUS_NoSmallVessels();

			if (IsSIMPOrCOAPgaRequiredValidation && NMFSLine.RequiresFullData)
			{
				if (!HarvestingDetail.US_NoSmallVessels_ReadOnly)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NoSmallVesselsInfo);
				}
			}
		}

		protected override void CheckUS_OceanAreaOfCatchDesc()
		{
			base.CheckUS_OceanAreaOfCatchDesc();
			if (!HarvestingDetail.US_OceanAreaOfCatchDesc_ReadOnly && !NMFSLine.Is370ProgramType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OceanAreaOfCatchDescInfo);
			}
		}

		protected new USNMFSHarvestingDetailAddInfo Parent
		{
			get { return (USNMFSHarvestingDetailAddInfo)base.Parent; }
		}

		protected NMFSHarvestingDetail HarvestingDetail
		{
			get { return (NMFSHarvestingDetail)Parent.Parent; }
		}

		protected NMFSLine NMFSLine
		{
			get
			{
				var harvestingDetail = HarvestingDetail;
				return harvestingDetail == null ? null : harvestingDetail.Parent;
			}
		}

		bool IsOriginCountrySameAsHarvestCountry
		{
			get
			{
				var result = false;
				var nmfsLine = NMFSLine;
				if (nmfsLine != null)
				{
					var invoiceLine = nmfsLine.InvoiceLine;
					if (invoiceLine != null)
					{
						result = invoiceLine.US_UC_NKCountryOfOrigin == Parent.US_HarvestedCountry;
					}
				}
				return result;
			}
		}

		bool Is370orHMSPgaRequiredValidation
		{
			get
			{
				var result = false;

				var nmfsLine = NMFSLine;
				if (nmfsLine != null && (nmfsLine.Is370ProgramType || nmfsLine.IsHMSProgramType))
				{
					var invoiceLine = nmfsLine.InvoiceLine;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}

		bool IsSIMProgramTypeAndHBASourceType
		{
			get
			{
				var nmfsLine = NMFSLine;
				return nmfsLine != null && (nmfsLine.IsSIMProgramType && nmfsLine.IsHBASourceType);
			}
		}

		bool IsSIMPOrCOAPgaRequiredValidation
		{
			get
			{
				var result = false;

				var nmfsLine = NMFSLine;
				if (nmfsLine != null && (nmfsLine.IsSIMProgramType || nmfsLine.IsCOAProgramType))
				{
					var invoiceLine = nmfsLine.InvoiceLine;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}

		bool IsCOARequiredValidation
		{
			get
			{
				var result = false;

				var nmfsLine = NMFSLine;
				if (nmfsLine != null && nmfsLine.IsCOAProgramType)
				{
					var invoiceLine = nmfsLine.InvoiceLine;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}

		bool IsExportNMFSRequired
		{
			get
			{
				var invoiceLine = NMFSLine != null ? NMFSLine.InvoiceLine : null;
				return invoiceLine != null && invoiceLine.IsExport && OGAIndicatorList.IsToBeDeclared(invoiceLine.US_NMFSHMSInd) && NMFSLine.US_ProgramType == NMFSProgramCodeList.Codes.HMS;
			}
		}
	}
}
