using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(GlbCompany), nameof(GlbCompany.PhilippinesEInvoicingCredentials))]
	public sealed class GlbCompanyExternalPasswordForPhilippines : UserAndClientCredentials
	{
		GlbCompanyExternalPasswordForPhilippines(GlbCompany company, GlbExternalPassword userCredential, GlbExternalPassword clientCredential)
			: base(userCredential, clientCredential)
		{
			Company = Argument.NotNull(company, nameof(company));
			if (!IsAllowed(Company))
			{
				throw new InvalidOperationException($"Parent company country should be PH, not {Company.GC_RN_NKCountryCode}.");
			}
		}

		public static GlbCompanyExternalPasswordForPhilippines New(GlbCompany company)
		{
			Argument.NotNull(company, nameof(company));

			var credentialPHA = GetOrCreateCredential<GlbCompanyExternalPasswordPHA>(company.PK, PasswordTypesList.Codes.PHA, company.Factory);
			var credentialPHU = GetOrCreateCredential<GlbCompanyExternalPasswordPHU>(company.PK, PasswordTypesList.Codes.PHU, company.Factory);

			return new GlbCompanyExternalPasswordForPhilippines(company, credentialPHU, credentialPHA);
		}

		public GlbCompany Company { get; }

		static T GetOrCreateCredential<T>(ZGuid companyPK, ZString passwordType, BusinessObjectFactory factory) where T : GlbExternalPassword
		{
			var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, companyPK);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, passwordType);

			var credentialRef = factory.LoadTop1<T>(query);
			if (credentialRef == null)
			{
				credentialRef = factory.New<T>();
				credentialRef.GP_GC = companyPK;
			}
			else
			{
				credentialRef.Validation.ValidateAll();
			}

			return credentialRef;
		}

		public static bool IsAllowed(GlbCompany company) => (company?.GC_RN_NKCountryCode ?? ZString.Empty) == CountryCodes.Philippines;

		[ResourceStringData("f48105f9-190d-449c-bc9c-65a51db8d0c6r", Caption = "Application Id")]
		public override ZString ClientId
		{
			get => base.ClientId;
			set => base.ClientId = value;
		}

		[ResourceStringData("d280c818-44cf-4564-b17a-8159111e2169", Caption = "Accreditation Id")]
		public override ZString ClientSecret
		{
			get => base.ClientSecret;
			set => base.ClientSecret = value;
		}
	}
}
