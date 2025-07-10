using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public abstract class GlbStaffWrapper : NonPersistentBusinessObject, Integration.IGlbStaffWrapper
	{
		protected GlbStaffWrapper(GlbStaff staff)
			: base(staff.Factory)
		{
			this.Staff = staff;
			using (((ISingleElementListInternal)this).SuspendListChanged())
			{
				base.AddToFactoryCache();
			}
			Staff.RegisterEditableChildObject(this);
		}
		public readonly GlbStaff Staff;

		public T GetGlbExternalPassword<T>(ZString passwordType, ZGuid? companyPK = null)
			where T : GlbExternalPassword
		{
			var query = new ZQuery(GlbExternalPasswordSchema.GP_GS, PK);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, passwordType);
			if (companyPK.HasValue)
			{
				query.AddToFilter(GlbExternalPasswordSchema.GP_GC, companyPK.Value);
			}
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			query.OrderBy = GlbExternalPasswordSchema.Constants.GP_SystemCreateTimeUtc;

			return Factory.LoadTop1<T>(query);
		}

		public T GetGlbExternalPasswordOrCreateNew<T>(ZString passwordType, ZGuid? companyPK = null)
			where T : GlbExternalPassword
		{
			var result = GetGlbExternalPassword<T>(passwordType, companyPK);
			if (result == null)
			{
				result = Factory.New<T>();
				using (result.SuspendSettingHasChanges())
				{
					result.GP_GS = PK;
					result.GP_PasswordType = passwordType;
					if (companyPK.HasValue)
					{
						result.GP_GC = companyPK.Value;
					}
				}
			}
			return result;
		}

		public sealed override bool IsInDatabase => Staff.IsInDatabase;
		public sealed override string TablePrefix => GlbStaffSchema.Constants.Prefix;
		public sealed override string TableName => GlbStaffSchema.Constants.TableName;

		protected sealed override void AddToFactoryCache()
		{
			// should be called after Staff is set
		}

		protected sealed override ZGuid GetPK()
		{
			return Staff.PK;
		}

		protected sealed override bool SupportsCloneCore() => false;
	}
}
