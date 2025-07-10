using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobDeclarationValidation_INP : JobDeclarationValidation_Inward
	{
		public JobDeclarationValidation_INP(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override ICodeDescriptionPairList MessageSubTypeList
		{
			get { return Parent.Lookups.INPMessageSubTypeList; }
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			if (!InwardTransportOptional)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TransportModeInfo, "Inward Transport Mode");
			}
		}

		bool InwardTransportOptional
		{
			get
			{
				var placeOfRelease = Declaration.PlaceOfRelease;
				return placeOfRelease.IsLicencedPremise() && !placeOfRelease.IsBWCY()
					|| Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.BKT
					|| Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.SHO
					|| Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.APS
					|| Declaration.SG_GoodsPreviouslyExemptedFromDuties;
			}
		}

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();
			var placeOfReceipt = Declaration.PlaceOfReceipt;
			if (placeOfReceipt.IsLicencedPremise()
				&& Declaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.APS
				&& Declaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.SHO)
			{
				Declaration.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("CF038119-39B2-48CE-AAD3-C1E4C4CE2369", "When goods are receipted into a licensed or bonded warehouse, Declaration Type should be APS or SHO"));
			}
			if (Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.SHO && !Declaration.TradersRemarks.Any())
			{
				Declaration.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("03104910-F756-4F9A-8734-6EE5CF434218", "Traders Remarks are required for INP SHO declarations"));
			}
		}

		protected override void CheckJE_OH_Consignee()
		{
			base.CheckJE_OH_Consignee();
			if (Parent.JE_OH_Consignee.IsEmpty && Parent.JE_MessageSubType == DeclarationTypeCodeList.Codes.SFZ)
			{
				bool meantForStorageOrSeastores = Parent.PlaceOfStorage != null || Parent.IsSeaStore;
				if (!meantForStorageOrSeastores)
				{
					Parent.JE_OH_ConsigneeInfo.AddMessageError("Consignee is required when the declaration type is SFZ unless the goods are sea stores or the goods are meant for storage.");
				}
			}
		}

		protected override void CheckJE_OH_Exporter()
		{
			base.CheckJE_OH_Exporter();

			if (Parent.JE_MessageSubType == DeclarationTypeCodeList.Codes.REX)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ExporterInfo, "Exporter");
			}
		}

		public override void ValidateClaimantAddress(JobDocAddressValidation validation)
		{
			var claimant = validation.Parent;

			if (Parent.JE_MessageSubType == DeclarationTypeCodeList.Codes.GTR || Parent.JE_MessageSubType == DeclarationTypeCodeList.Codes.BKT)
			{
				if (Parent.JE_MessageSubType == DeclarationTypeCodeList.Codes.GTR && !claimant.OrganisationPK.IsValid)
				{
					claimant.OrganisationPKInfo.AddMessageError(AddInfoJobDeclarationValidation_INP.ClaimantRequiredForGTR);
				}
				else
				{
					base.ValidateClaimantAddress(validation);
				}
			}
			else if (claimant.OrganisationPK.IsValid)
			{
				claimant.OrganisationPKInfo.AddMessageError(AddInfoJobDeclarationValidation_INP.ClaimantDetailsMessage);
			}
		}

		protected override bool IsInwardCarrierAgentMandatory => Parent.JE_MessageSubType != DeclarationTypeCodeList.Codes.BKT && TransportModeCodeList.TransportIsSeaOrAir(Parent.JE_TransportMode);

		protected override bool IsOutwardShippingLineForwarderMandatory => Parent.JE_MessageSubType == DeclarationTypeCodeList.Codes.REX && TransportModeCodeList.TransportIsSeaOrAir(Parent.SG_OutwardTransportMode)
																		&& !(Parent.PlaceOfStorage?.IsFTZ() ?? false);
	}
}
