using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class EInvoicingBranchRegisterValidation : ZValidation
	{
		public EInvoicingBranchRegisterValidation(EInvoicingBranchRegister parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly EInvoicingBranchRegister Parent;

		public override Type AutoValidationType => GetType();

		public override void ValidateAll()
		{
			ValidateOTP();
			ValidateDebtor();
		}

		public void ValidateOTP() => ValidateCalculatedProperty(Parent.OTPInfo);
		public void ValidateDebtor() => ValidateCalculatedProperty(Parent.DebtorPKInfo);

		protected void CheckOTP()
		{
			MandatoryValidation.CheckEntered(Parent.OTPInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Property description")]
		protected void CheckDebtorPK()
		{
			MandatoryValidation.CheckEntered(Parent.DebtorPKInfo, "Debtor");
			ListValidation.ErrorIfInvalidPK(Parent.DebtorPKInfo, Parent.Debtors);
			if (Parent.Debtor == null)
			{
				return;
			}
			if (Parent.Debtor.Country.Code != Parent.Branch.GB_RN_NKCountryCode)
			{
				Parent.DebtorPKInfo.AddError(Res.GetString("2A3E46B2-61AA-4408-9EE7-61DEB28224E3", "You must select a Debtor from the same country as the branch ({0}).", Parent.Branch.Country.Description));
			}
			if (Parent.Debtor.CompanyData.OB_ARVATConfig == AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code)
			{
				Parent.DebtorPKInfo.AddError(Res.GetString("2307FFA0-4B0E-44B2-9F4D-1A6BC02159F2", "You must select a Debtor that has an applicable VAT recognition (it cannot be '{0}').", AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.CodeAndDescription));
			}
		}
	}
}
