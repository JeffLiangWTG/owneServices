using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class GlbGroupForPluginWrapper : NonPersistentBusinessObject
	{
		public GlbGroupForPluginWrapper(GlbGroup group)
			: base(group.Factory)
		{
			this.group = group;
		}
		readonly GlbGroup group;

		public GlbExternalPassword_SGA AccessPassword
		{
			get
			{
				if (accessPassword == null || accessPassword.IsDeleted && !group.IsDeleted)
				{
					var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, GlbCompany.CurrentCompany.PK);
					query.AddToFilter(GlbExternalPasswordSchema.GP_GG, group.PK);
					query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.SGA);
					query.FetchOnlyFromLocalCache = !group.IsInDatabase;
					query.OrderBy = GlbExternalPassword.Schema.GP_SystemCreateTimeUtc;

					accessPassword = Factory.LoadTop1<GlbExternalPassword_SGA>(query);

					if (accessPassword == null)
					{
						accessPassword = Factory.New<GlbExternalPassword_SGA>();
						using (accessPassword.SuspendSettingHasChanges())
						{
							accessPassword.GP_GG = group.PK;
						}
					}

					RegisterEditableChildObject(accessPassword);
				}

				return accessPassword;
			}
		}
		GlbExternalPassword_SGA accessPassword;
	}
}
