using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobDeclarationValidation_OUT : AddInfoCUSDECValidation
	{
		public AddInfoJobDeclarationValidation_OUT(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckSG_EntryYear()
		{
			base.CheckSG_EntryYear();
			if (Parent.SG_EntryYear > 0)
			{
				if (!(SGCertificatesCodeList.IsEntryYearRequired(Certificate1Type) || SGCertificatesCodeList.IsEntryYearRequired(Certificate2Type)))
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.SG_EntryYearInfo, "Entry Year");
				}
			}
		}

		ZZRefCusCodeListCombined Certificate1Type => Parent.SG_Cert1Type.IsEmpty ? null : SGCertificatesCodeList.GetCurrentOrMatchingCertificate(Parent.Factory, Parent.SG_Cert1Type);
		ZZRefCusCodeListCombined Certificate2Type => Parent.SG_Cert2Type.IsEmpty ? null : SGCertificatesCodeList.GetCurrentOrMatchingCertificate(Parent.Factory, Parent.SG_Cert2Type);

		protected override void CheckSG_OutwardTransportMode()
		{
			base.CheckSG_OutwardTransportMode();
			var placeOfStorage = Declaration.PlaceOfStorage;
			bool storageInFTZorBWYC = placeOfStorage.IsFTZ() || placeOfStorage.IsBWCY();
			if (!storageInFTZorBWYC)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_OutwardTransportModeInfo, "Outward Transport Mode");
			}
		}

		protected override void CheckSG_RN_NKFinalDestination()
		{
			base.CheckSG_RN_NKFinalDestination();
			if (!Parent.Declaration.IsSeaStore)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_RN_NKFinalDestinationInfo, "Country/Region of Final Destination");
			}
		}

		protected override void CheckSG_OutwardMAWB()
		{
			if (Parent.SG_US_NKPlaceOfStorage == "")
			{
				base.CheckSG_OutwardMAWB();
			}
		}

		protected override void CheckSG_IsSeaStore()
		{
			base.CheckSG_IsSeaStore();
			if (Parent.Declaration.IsSeaStore && Parent.Declaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.APS)
			{
				Parent.SG_IsSeaStoreInfo.AddMessageError("For export of Sea Stores, the declaration type should be 'OUT / APS");
			}
		}
	}
}
