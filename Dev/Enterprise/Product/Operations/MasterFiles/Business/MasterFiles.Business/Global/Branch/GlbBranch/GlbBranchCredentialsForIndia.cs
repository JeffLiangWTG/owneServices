using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(GlbBranch), nameof(GlbBranch.BranchCredentialsIndia))]
	public class GlbBranchCredentialsForIndia : UserAndClientCredentials, IObsoleteValidation
	{
		GlbBranchCredentialsForIndia(GlbBranch branch, GlbExternalPassword credentialINT, GlbExternalPassword credentialINS)
			: base(credentialINT, credentialINS)
		{
			Branch = Argument.NotNull(branch, nameof(branch));
			if (!IsAllowed(branch))
			{
				throw new ArgumentException("Parent branch is not allowed for India.", nameof(branch));
			}
		}

		public static GlbBranchCredentialsForIndia New(GlbBranch branch)
		{
			Argument.NotNull(branch, nameof(branch));

			var credentialINT = GetOrCreateCredential<GlbBranchExternalPasswordINT>(branch.PK, PasswordTypesList.Codes.INT, branch.Factory);
			var credentialINS = GetOrCreateCredential<GlbBranchExternalPasswordINS>(branch.PK, PasswordTypesList.Codes.INS, branch.Factory);

			return new GlbBranchCredentialsForIndia(branch, credentialINT, credentialINS);
		}

		public GlbBranch Branch { get; }

		public static bool IsAllowed(GlbBranch branch) => (branch?.Company?.GC_RN_NKCountryCode ?? string.Empty) == Core.Constants.CountryCodes.India;

		static T GetOrCreateCredential<T>(ZGuid branchPK, ZString passwordType, BusinessObjectFactory factory) where T : GlbExternalPassword
		{
			var query = new ZQuery(GlbExternalPasswordSchema.GP_GB, branchPK);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, passwordType);
			var credentialRef = factory.LoadTop1<T>(query);

			if (credentialRef == null)
			{
				credentialRef = factory.New<T>();
				credentialRef.GP_GB = branchPK;
			}
			else
			{
				credentialRef.Validation.ValidateAll();
			}
			return credentialRef;
		}
	}
}
