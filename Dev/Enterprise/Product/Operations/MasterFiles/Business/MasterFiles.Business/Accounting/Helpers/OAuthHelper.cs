using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Accounting.Helpers
{
	public static class OAuthHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Reason string")]
		public const string CredentialStatusReasonApplyingToken = "Applying token";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Reason string")]
		public const string CredentialStatusReasonRefreshingToken = "Refreshing token";

		public static void CreateOrUpdateEInvoicingCredential(ICompany company)
		{
			var newFactory = new BusinessObjectFactory();
			var companyInNewFactory = newFactory.Load<GlbCompany>(company.PK);
			var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(companyInNewFactory);

			var credential = wrapper.GetGlbExternalPasswordOrCreateNew<GlbCompanyEInvoicingCertificateCredential>(PasswordTypesList.Codes.EIM);
			if (credential.IsInDatabase)
			{
				if (credential.GP_StatusReason != CredentialStatusReasonApplyingToken)
				{
					credential.GP_StatusReason = CredentialStatusReasonRefreshingToken;
				}
			}
			else
			{
				credential.GP_StatusReason = CredentialStatusReasonApplyingToken;
				credential.GP_PasswordStatus = PasswordStatusList.Codes.Pending;
			}

			if (credential.HasChanges)
			{
				newFactory.Save();
			}
		}

		public static EInvoiceOAuthData CreateOAuthData(ICompany company)
		{
			var glbCompany = (GlbCompany)company;
			var query = new ZDBOnlyQuery(typeof(GlbCompanyEInvoicingCertificateCredential));
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.EIM);
			query.AddToFilter(GlbExternalPasswordSchema.GP_GC, company.PK);
			query.ReLoadExistingRows = true;

			var rawCredentials = glbCompany.Factory.Load<GlbCompanyEInvoicingCertificateCredential>(query);

			var credentials = new EInvoiceOAuthCredentialCollection();
			credentials.AddRange(rawCredentials.Select(x => new EInvoiceOAuthCredential()
			{
				Status = x.HasExpiredByUtc ? PasswordStatusList.Descriptions.Expired : PasswordStatusList.GetFullList(glbCompany.Factory).GetDescriptionFromCode(x.GP_PasswordStatus),
				AuthorizationDate = x.GP_SystemCreateTimeUtc.ToLocalBranchTime(glbCompany.Factory),
				IssueDate = x.GP_IssueDate.ToLocalBranchTime(glbCompany.Factory),
				ExpiryDate = x.GP_ExpiryDate.ToLocalBranchTime(glbCompany.Factory),
				Description = x.GP_StatusReason
			}));

			return new EInvoiceOAuthData(company, credentials);
		}
	}
}
