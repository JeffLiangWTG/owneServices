using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class CommonDrawbackJobDeclarationValidation : JobDeclarationValidation
	{
		public CommonDrawbackJobDeclarationValidation(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void CheckJE_MessageType()
		{
			// do not call base, JE_messageType is not user editable for a drawback

			if (Parent.US_EntryFilerCode.IsEmpty)
			{
				Parent.JE_MessageTypeInfo.AddError(EntryFilerCodeIsEmptyAndEntryNumberCannotBeGenerated);
			}
			else if (Parent.IsJE_MessageTypeChangedSinceLoading)
			{
				Parent.JE_MessageTypeInfo.AddError(CannotChangeShipmentTypeToDrawback);
			}
		}
		internal const string EntryFilerCodeIsEmptyAndEntryNumberCannotBeGenerated = "An entry filer code has not been set up for this branch or company. Without it, entry numbers cannot be generated for this Drawback Summary. Please set up one in Registry > Customs > United States of America > Import > ABI > Entry Filer Code first.";
		internal const string CannotChangeShipmentTypeToDrawback = "Shipment Type cannot be changed to Drawback.";

		protected override void CheckJE_MessageSubType()
		{
			// JE_MessageSubType is not used for US Drawbacks.
		}

		protected override void CheckJE_TransportMode()
		{
			// JE_MessageSubType is not used for US Drawbacks.
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();

			var importerInfo = Parent.JE_OH_ImporterInfo;
			MandatoryValidation.MessageErrorIfNotEntered(importerInfo);

			var claimant = Parent.Importer;
			if (claimant != null)
			{
				var customsRegNo = claimant.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber });
				if (customsRegNo.IsEmpty)
				{
					Parent.JE_OH_ImporterInfo.AddMessageError(ClaimantRegNoRequired);
				}

				new AuthorityToActValidator().Validate(Parent, Parent.Importer, importerInfo, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, "", PowerOfAttorneyValidator.ExtraMatchingConditionForImportDirectionAndPortOfEntry(ZString.Empty), PowerOfAttorneyValidator.DirectionOrPortOfEntryNotMatchForReconAndDrawback);
			}
		}

		internal const string ClaimantRegNoRequired = "This claimant does not have an EIN, SSN or CBP number configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.";

		protected override void CheckDeclarationNumber()
		{
			base.CheckDeclarationNumber();

			if (Parent.DeclarationNumber.IsEmpty)
			{
				var reasonNotToAllocate = Parent.DisallowAllocateImportEntryNumber;

				if (!string.IsNullOrEmpty(reasonNotToAllocate))
				{
					Parent.DeclarationNumberInfo.AddMessageError(reasonNotToAllocate);
				}
			}
		}

		protected override bool JE_MergeByRequired
		{
			get { return false; }
		}
	}
}
