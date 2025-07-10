using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccEPaymentBeneficiary : AutoAccEPaymentBeneficiary
	{
		public AccEPaymentBeneficiary(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.ProviderCodeList")]
		public override ZString ABF_ProviderCode { get => base.ABF_ProviderCode; set => base.ABF_ProviderCode = value; }

		public GlbStaff CreatingUser => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ABF_SystemCreateUser);

		public AccAPAccountDetails LinkedAccountDetails
		{
			get
			{
				var query = new ZQuery(AccAPAccountDetailsSchema.A1_EPaymentBeneficiaryId, PK);
				return Factory.LoadTop1<AccAPAccountDetails>(query);
			}
		}

		public OrgHeader LinkedOrgHeader => LinkedAccountDetails?.CompanyData?.Header;

		public ZGuid LinkedOrgBankAccountPK => LinkedOrgHeader?.MiscServ?.OM_AB_APDefaultBankAccount ?? ZGuid.Empty;

		public ZString LinkedOrgAgreedPaymentMethod => LinkedOrgHeader?.CompanyData?.OB_APCreditAgreedPaymentMethod ?? ZString.Empty;

		public ZBool IsMatchedWithOrg => LinkedOrgHeader != null;

		#endregion

		public void UnMatchOrg()
		{
			if (LinkedAccountDetails != null)
			{
				LinkedAccountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;
				Factory.Save();
			}
		}

		public void MatchOrg(ZGuid creditorPK, ZString defaultPaymentReason, ZGuid defaultBankAccountPK, ZString agreedPaymentMethod, ZBool allowOverrideDefault, AccEPaymentBeneficiary beneficiary, ZString paymentReferenceType, ZString paymentReference)
		{
			var orgHeader = Factory.Load<OrgHeader>(creditorPK);
			if (orgHeader != null)
			{
				var accountDetailsCollection = orgHeader.CompanyData.AccountDetailsCollection.OfType<AccAPAccountDetails>();
				var accountDetails = accountDetailsCollection.FirstOrDefault(x => x.A1_PaymentMethod == EPaymentMethods.EPaymentViaOFX
																			&& x.A1_RX_NKAccountCurrency == ABF_RX_NKAccountCurrency
																			&& x.A1_IsDefaultAccount == ZBool.True);
				if (accountDetails == null)
				{
					accountDetails = orgHeader.CompanyData.AccountDetailsCollection.AddNew();
					accountDetails.A1_IsDefaultAccount = true;
					accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
					accountDetails.A1_RX_NKAccountCurrency = ABF_RX_NKAccountCurrency;
				}
				accountDetails.A1_EPaymentReasonCode = defaultPaymentReason;
				accountDetails.SetAccountDetailsValuesFromBeneficiary(beneficiary);
				accountDetails.A1_EPaymentReferenceType = paymentReferenceType;
				accountDetails.A1_EPaymentReference = paymentReference;
				if (allowOverrideDefault)
				{
					orgHeader.MiscServ.OM_AB_APDefaultBankAccount = defaultBankAccountPK;
					orgHeader.CompanyData.OB_APCreditAgreedPaymentMethod = agreedPaymentMethod;
				}
				Factory.Save();
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ABF_ProviderReference = PK.ToString().Substring(0, 8);
			ABF_GC_Company = GlbCompany.CurrentCompany.PK;
			ABF_ProviderCode = EPaymentProviderCodes.Codes.OFX;
		}
#endif
	}
}
