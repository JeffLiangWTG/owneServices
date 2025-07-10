using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public abstract class GlbCompanyWrapper : NonPersistentBusinessObject, Integration.IGlbCompanyWrapper
	{
		protected GlbCompanyWrapper(GlbCompany company)
			: base(company.Factory)
		{
			Company = company;

			using (((ISingleElementListInternal)this).SuspendListChanged())
			{
				base.AddToFactoryCache();
			}

			Company.RegisterEditableChildObject(this);
		}

		public readonly GlbCompany Company;

		public static T GetWrapper<T>(GlbCompany company)
			where T : GlbCompanyWrapper
		{
			return company == null ? null :
				(T)company.Factory.GetCachedValue(company.PK.ToString() + "GlbCompanyWrapper", () =>
				{
					GlbCompanyWrapper result = null;
					var countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(company.GC_RN_NKCountryCode);
					var writers = ObjectFactory.Get<Hashtable>("GlbCompanyWrappers");
					var objectHandle = (ObjectHandle)writers[countryCode];
					if (objectHandle == null && ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(countryCode))
					{
						objectHandle = (ObjectHandle)writers[Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda];
					}
					if (objectHandle != null)
					{
						result = (GlbCompanyWrapper)objectHandle.GetObject(company);
					}
					if (result == null)
					{
						result = new GlbCompanyWrapperForOnlyICS2(company);
					}

					return result;
				});
		}

		#region Related BusinessObjects

		public T GetGlbExternalPassword<T>(ZString passwordType)
			where T : GlbExternalPassword
		{
			var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, PK);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, passwordType);
			query.AddToFilter(GlbExternalPasswordSchema.GP_GS, SQLComparisonOperator.Equal, null);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			query.OrderBy = GlbExternalPasswordSchema.Constants.GP_SystemCreateTimeUtc;

			return Factory.LoadTop1<T>(query);
		}

		public T GetGlbExternalPasswordOrCreateNew<T>(ZString passwordType)
			where T : GlbExternalPassword
		{
			var result = GetGlbExternalPassword<T>(passwordType);
			if (result == null)
			{
				result = Factory.New<T>();
				using (result.SuspendSettingHasChanges())
				{
					result.GP_GC = PK;
					result.GP_PasswordType = passwordType;
				}
			}
			return result;
		}

		public virtual ResourceStringData HumanReadableFtpCaption => Res.GetData("826c8a0a-7232-42d4-b717-bf5861c9db49", "FTP");

		#endregion

		[ChildEditable]
		public GlbCompanyCredentialICS2 GlbCompanyCredentialICS2
		{
			get
			{
				if (glbCompanyCredentialICS2 == null || glbCompanyCredentialICS2.IsDeleted)
				{
					glbCompanyCredentialICS2 = GetGlbExternalPassword<GlbCompanyCredentialICS2>(PasswordTypesList.Codes.IC2);
					RegisterEditableChildObject(glbCompanyCredentialICS2);
				}

				return glbCompanyCredentialICS2;
			}
		}
		GlbCompanyCredentialICS2 glbCompanyCredentialICS2;

		#region Override

		public sealed override bool IsInDatabase => Company.IsInDatabase;

		public sealed override string TablePrefix => GlbCompanySchema.Constants.Prefix;

		public sealed override string TableName => GlbCompanySchema.Constants.TableName;

		protected sealed override void AddToFactoryCache()
		{
			// should be called after Company is set
		}

		protected sealed override ZGuid GetPK()
		{
			return Company.PK;
		}

		protected sealed override bool SupportsCloneCore() => false;

		#endregion

		public abstract bool IsValidWrapper { get; }
	}
}
