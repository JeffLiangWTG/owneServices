using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobDeclarationValidation_INP : AddInfoCUSDECValidation
	{
		public AddInfoJobDeclarationValidation_INP(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckSG_RemovalStartDate()
		{
			base.CheckSG_RemovalStartDate();
			if (Declaration.IsTradeNet4Point1)
			{
				if (Declaration.IsTemporaryConsignment)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_RemovalStartDateInfo, "Start Date of Exhibition/Temporary Import Period.\r\nFor temporary consignments, (except declaration type 'TCI'), it is mandatory to specify the start and end dates of the Exhibition/Temporary Import Period.");
				}
				else if (Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.BKN)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_RemovalStartDateInfo, "Start Date.\r\nFor blanket imports, it is mandatory to specify the Start Date.");
				}
			}
		}

		protected override void CheckSG_EndDateTempImport()
		{
			base.CheckSG_EndDateTempImport();
			if (Declaration.IsTemporaryConsignment)
			{
				ZString errorMessage = Declaration.IsTradeNet4Point1 ? "End Date of Temporary Import Period.\r\nFor temporary consignments, (except declaration type 'TCI'), it is mandatory to specify the start and end dates of the Exhibition/Temporary Import Period." : "End Date of Temporary Import Period.";
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_EndDateTempImportInfo, errorMessage);
			}
		}

		protected override void CheckSG_OutwardTransportMode()
		{
			base.CheckSG_OutwardTransportMode();
			if (Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.REX)
			{
				var placeOfStorage = Declaration.PlaceOfStorage;
				var storageInFTZorBWYC = placeOfStorage.IsFTZ() || placeOfStorage.IsBWCY();
				if (!storageInFTZorBWYC)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_OutwardTransportModeInfo, "Outward Transport Mode");
				}
			}
		}

		protected override void CheckSG_ClaimantName()
		{
			if (Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.GTR || Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.BKT)
			{
				if (Parent.SG_ClaimantName.IsEmpty && Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.GTR)
				{
					Parent.SG_ClaimantNameInfo.AddMessageError(ClaimantRequiredForGTR);
				}
				else
				{
					base.CheckSG_ClaimantName();
				}
			}
			else if (!Parent.SG_ClaimantName.IsEmpty)
			{
				Parent.SG_ClaimantNameInfo.AddMessageError(ClaimantDetailsMessage);
			}
		}

		protected override void CheckSG_ClaimantCode()
		{
			if (Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.GTR || Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.BKT)
			{
				if (Parent.SG_ClaimantCode.IsEmpty && Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.GTR)
				{
					Parent.SG_ClaimantCodeInfo.AddMessageError(ClaimantRequiredForGTR);
				}
				else
				{
					base.CheckSG_ClaimantCode();
				}
			}
			else if (!Parent.SG_ClaimantCode.IsEmpty)
			{
				Parent.SG_ClaimantCodeInfo.AddMessageError(ClaimantDetailsMessage);
			}
		}

		protected override void CheckSG_RN_NKFinalDestination()
		{
			base.CheckSG_RN_NKFinalDestination();
			if (Parent.Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.REX && !Parent.Declaration.IsSeaStore)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_RN_NKFinalDestinationInfo, "Country/Region of Final Destination. This is required for outward transport when declaration type is 'REX' (& not seastore).");
			}
		}

		internal const string ClaimantRequiredForGTR = "Claimant details are mandatory if the declaration type is GTR.";
		internal const string ClaimantDetailsMessage = "Claimant details are only required if the declaration type is GTR, (mandatory), or BKT, (if required).";
	}
}
