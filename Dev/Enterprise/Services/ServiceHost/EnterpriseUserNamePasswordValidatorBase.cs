using System;
using System.IdentityModel.Selectors;
using System.Linq;
using System.ServiceModel;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost
{
	public enum AuthenticationResult
	{
		Success,
		AccountLocked,
		AccountDeactivated,
		PasswordChangeRequired,
		UserInactive,
		LogonDetailsIncorrect,
		SessionExpired,
		SessionRequired
	}

	public abstract class EnterpriseUserNamePasswordValidatorBase : UserNamePasswordValidator
	{
		public override void Validate(string userName, string password)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (!ValidatePassword(userName, password))
				{
					throw new FaultException("Username or Password invalid.");
				}
			}
		}

		protected abstract AuthenticationResult GetAuthenticationResult(string userName, string password);

		bool ValidatePassword(string userName, string password)
		{
			return GetAuthenticationResult(userName, password) == AuthenticationResult.Success;
		}

		protected AuthenticationResult GetAuthenticationResultCore(string userName, string password, bool allowStaffOnly)
		{
			var expectedPassword = ObjectFactory.Get<IProductRegistration>().Key.Password;
			var isCorrectPassword = string.Equals(expectedPassword, password, StringComparison.Ordinal);
			if (isCorrectPassword)
			{
				ZGuid pk;
				if (ZGuid.TryParse(userName, out pk))
				{
					var user = LoadUserFromHeartBeat(pk, allowStaffOnly, loadNameOnly: false);

					if (user != null)
					{
						var isActive = (ZBool)user[GlbStaffSchema.GS_IsActive];
						if (!isActive)
						{
							return AuthenticationResult.AccountDeactivated;
						}

						var lockOutDateTime = (ZDateTime)user[GlbPersonSchema.PER_LoginDisabledUntilUtc];
						if (lockOutDateTime > ZDateTime.UtcNow)
						{
							return AuthenticationResult.AccountLocked;
						}

						return AuthenticationResult.Success;
					}
				}
			}

			return AuthenticationResult.LogonDetailsIncorrect;
		}

		public static DynamicBusinessObject LoadUserFromHeartBeat(ZGuid heartbeatPk, bool allowStaffOnly, bool loadNameOnly)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "EnterpriseGlowServiceUser" };
			var users = new DynamicBusinessObjectCollection(factory);
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@heartbeatPk", heartbeatPk, Schema.GenericGuidSchemaColumn);
			if (!allowStaffOnly)
			{
				parameters.Add("@webuser", User.WebUserName, Schema.GenericStringSchemaColumn);
			}

			var sql = SelectNameSql + (loadNameOnly ? "" : SelectOtherSql) + StaffSql + (allowStaffOnly ? "" : ContactSql) + (allowStaffOnly ? PersonStaffSql : PersonContactSql) + WhereSql;
			users.Load(sql, parameters);
			return users.SingleOrDefault();
		}

		const string SelectNameSql = @"
SELECT		GS_LoginName";

		const string SelectOtherSql = @", GS_IsActive, PER_LoginDisabledUntilUtc";

		const string StaffSql = @"
FROM dbo.StmServiceHeartbeat
JOIN		dbo.GlbStaff ON (SV_ParentTableCode = 'GS' AND SV_ParentId = GS_PK)";

		const string ContactSql = @" OR (SV_ParentTableCode = 'OC' AND GS_LoginName = @webuser)
LEFT JOIN	dbo.OrgContact ON OC_PK = SV_ParentId";

		const string PersonStaffSql = @"
LEFT JOIN	dbo.GlbPerson ON PER_PK = GS_PER";
		const string PersonContactSql = @" 
LEFT JOIN	dbo.GlbPerson ON PER_PK = OC_PER";

		const string WhereSql = @"
WHERE		SV_PK = @heartbeatPk";
	}
}
