using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class FTZAddInfoBillValidation : CommonImportAddInfoBillValidation
	{
		public FTZAddInfoBillValidation(AddInfoBill addInfoHouseBill)
			: base(addInfoHouseBill)
		{
		}

		protected override void CheckUS_UC_NKCountryOfExport()
		{
			base.CheckUS_UC_NKCountryOfExport();
			ListValidation.MessageErrorIfInvalidCode(Parent.Bill.US_UC_NKCountryOfExportInfo);

			if (IsFTZAdmissionValidationMode && IsLowestBill && IsRegularAdmission)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.Bill.US_UC_NKCountryOfExportInfo, "Country of Export");
			}
		}

		protected override void CheckUS_SchDLoading()
		{
			base.CheckUS_SchDLoading();
			ListValidation.MessageErrorIfInvalidCode(Parent.Bill.US_SchDLoadingInfo);

			if (IsFTZAdmissionValidationMode
				&& IsLowestBill
				&& IsRegularAdmission
				&& (Parent.Declaration == null || Parent.Declaration.IsSea))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.Bill.US_SchDLoadingInfo, "Foreign Load Port");
			}
		}

		protected override void CheckUS_US_NKLocationOfGoods()
		{
			base.CheckUS_US_NKLocationOfGoods();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_US_NKLocationOfGoodsInfo, Parent.Lookups.FIRMSList, (NoResString)CommonImportAddInfoJobDeclarationValidation.LocationOfGoodsNotOnFile);

			if (Parent.Bill.US_US_NKLocationOfGoods.IsEmpty)
			{
				if (IsFTZPTTValidationMode || (IsFTZAdmissionValidationMode && Parent.Declaration.US_F_IncludePTT))
				{
					Parent.Bill.US_US_NKLocationOfGoodsInfo.AddMessageError(string.Format(GetErrorMessage(), "FIRMS Code"));
				}
			}
		}

		protected override void CheckUS_F_OH_PTTCarrier()
		{
			base.CheckUS_F_OH_PTTCarrier();

			if (Parent.Bill.US_F_OH_PTTCarrier.IsEmpty)
			{
				if (IsFTZPTTValidationMode || (IsFTZAdmissionValidationMode && Parent.Declaration.US_F_IncludePTT))
				{
					Parent.US_F_OH_PTTCarrierInfo.AddMessageError(string.Format(GetErrorMessage(), "Carrier"));
				}
			}
			else
			{
				OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(Parent.US_F_OH_PTTCarrierInfo, OrgMatchedCustomsRegNoType.EIN, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Carrier"), false, false);
			}
		}

		protected override void CheckUS_SESplitShip()
		{
			base.CheckUS_SESplitShip();

			if (Parent.Bill is Bill bill && bill.US_SESplitShip && bill.ITAndSplitDetails.Count == 0)
			{
				Parent.US_SESplitShipInfo.AddMessageError(AtLeastOneSplitDetailMustBeEntered);
			}
		}
		internal const string AtLeastOneSplitDetailMustBeEntered = "At least one split shipment details line must be entered.";

		string GetErrorMessage()
		{
			return IsFTZPTTValidationMode ? ValidationConstants.FTZ.DataRequired : ValidationConstants.FTZ.DataRequiredWhenPTTIncluded;
		}

		protected override bool ShouldValidateUS_UI_NKBillIssuerSCAC
		{
			get
			{
				var declaration = Parent.Declaration;
				return IsFTZAdmissionValidationMode && declaration != null && !declaration.IsODZ_AdmissionType;
			}
		}

		bool IsLowestBill
		{
			get { return Parent.Bill != null && Parent.Bill.IsFTZLowestBill; }
		}

		bool IsRegularAdmission
		{
			get
			{
				var declaration = Parent.Declaration;
				return declaration != null && declaration.IsRegularFTZAdmission;
			}
		}

		bool IsFTZAdmissionValidationMode
		{
			get
			{
				var declaration = Parent.Declaration;
				return declaration != null && declaration.IsFTZAdmissionValidationMode;
			}
		}

		bool IsFTZPTTValidationMode
		{
			get
			{
				var declaration = Parent.Declaration;
				return declaration != null && declaration.IsFTZPTTValidationMode;
			}
		}
	}
}
