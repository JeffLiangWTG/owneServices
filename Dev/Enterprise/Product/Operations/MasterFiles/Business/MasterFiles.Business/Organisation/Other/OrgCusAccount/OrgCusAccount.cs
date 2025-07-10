using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[SingleObjectAroundARow]
	public sealed class OrgCusAccount : AutoOrgCusAccount
	{
		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public static OrgCusAccount[] LoadByCodeAndCountry(BusinessObjectFactory factory, ZGuid organisationPK, ZString code, ZString countryCode)
			{
				var query = new ZQuery();
				if (code.IsEmpty || !organisationPK.IsValid || countryCode.IsEmpty)
				{
					query.IsNoResultQuery = true;
				}
				else
				{
					query.AddToFilter(OrgCusAccountSchema.CZ_OH, organisationPK);
					query.AddToFilter(OrgCusAccountSchema.CZ_Code, code);
					query.AddToFilter(OrgCusAccountSchema.CZ_RN_NKCountryCode, countryCode);
				}
				return factory.Load<OrgCusAccount>(query);
			}

			public static OrgCusAccount LoadTop1ByCodeAndCountry(BusinessObjectFactory factory, ZGuid organisationPK, ZString code, ZString countryCode) => LoadByCodeAndCountry(factory, organisationPK, code, countryCode).OrderBy(x => x.PK).FirstOrDefault();

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(OrgCusAccount);
		}

		public new class Schema : AutoOrgCusAccount.Schema
		{
			public const string DecryptedPassword = nameof(OrgCusAccount.DecryptedPassword);
		}

		public OrgCusAccount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public OrgCusAccountProvider Provider
		{
			get
			{
				if (provider == null || provider.CountryCode != CZ_RN_NKCountryCode)
				{
					provider = OrgCusAccountProvider.GetByCountryCode(CZ_RN_NKCountryCode);
				}
				return provider;
			}
		}
		OrgCusAccountProvider provider;

		[List("Lookups.IssuerList")]
		public override ZString CZ_Issuer
		{
			get => base.CZ_Issuer;
			set => base.CZ_Issuer = value;
		}

		[List("Lookups.CodeList")]
		public override ZString CZ_Code
		{
			get => base.CZ_Code;
			set
			{
				base.CZ_Code = value;
				if (Provider.ShouldDefaultTypeWhenAble && Lookups.AccountTypeList.Count == 1)
				{
					CZ_Type = this.Lookups.AccountTypeList[0].Code;
				}
			}
		}

		[List("Lookups.AccountTypeList")]
		public override ZString CZ_Type
		{
			get => base.CZ_Type;
			set => base.CZ_Type = value;
		}

		[MaxLength(47)]
		[Password]
		public ZString DecryptedPassword
		{
			get { return CZ_Password.IsEmpty ? ZString.Empty : (ZString)Encoder.Decrypt(CZ_Password); }
			set
			{
				var oldValue = DecryptedPassword;
				if (oldValue != value)
				{
					CheckMaximumLength(DecryptedPasswordInfo, value);
					CZ_Password = value.IsEmpty ? ZString.Empty : (ZString)Encoder.Encrypt(value);
					DecryptedPasswordInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateDecryptedPassword();
				}
			}
		}

		public ZPropertyInfo DecryptedPasswordInfo => GetZPropertyInfo(Schema.DecryptedPassword);

		[List("Lookups.AccountList")]
		public override ZString CZ_Account { get => base.CZ_Account; set => base.CZ_Account = value; }

		[List("Lookups.ReportingPeriodList")]
		public override ZString CZ_ReportingPeriod { get => base.CZ_ReportingPeriod; set => base.CZ_ReportingPeriod = value; }

		protected override bool IsLookupsCachedInBase => false;

		protected override OrgCusAccountLookups GetNewLookups() => Provider.GetNewLookups(this);

		protected override OrgCusAccountValidation GetNewValidation() => Provider.GetNewValidation(this);

		#region Encoder
		TwoWayEncoder Encoder
		{
			get { return encoder ?? (encoder = new TwoWayEncoder(PK.ToGuid())); }
		}
		TwoWayEncoder encoder;

		#endregion
	}
}
