using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Shared;
using WTG.Foundation.Cryptography;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Web.WebService.Common
{
	public static class LoginHelper
	{
		#region Validate

		public static void Validate(WebServiceResponse response, SecuritySOAPHeader securityHeader, bool requireValidateConcurencyLogin = true)
		{
			ZString errorMessage;

			var immutableSecurityHeader = new ImmutableSecuritySOAPHeader(securityHeader);
			if (!string.IsNullOrEmpty(immutableSecurityHeader.SecurityHeader.BranchCode) && !string.IsNullOrEmpty(immutableSecurityHeader.SecurityHeader.DepartmentCode) && !string.IsNullOrEmpty(securityHeader.UserName) && !string.IsNullOrEmpty(immutableSecurityHeader.SecurityHeader.Password))
			{
				errorMessage = ValidateAndSetUserContext(immutableSecurityHeader);
				if (errorMessage.IsEmpty)
				{
					errorMessage = ValidateUserLogin(response, immutableSecurityHeader, requireValidateConcurencyLogin);
				}
			}
			else
			{
				errorMessage = Res.GetString("cf21fb29-5942-415c-a88e-604d933eb4e6", "Please provide login credentials to use this service.");
			}

			if (!errorMessage.IsEmpty)
			{
				response.ErrorMessage = errorMessage;
				response.Error = ErrorTypes.LoginFailed;
			}
			else
			{
				LoadUserPreferredLanguage(securityHeader.IsAndroidDevice);
			}
		}

		static void LoadUserPreferredLanguage(bool isAndroidDevice)
		{
			var currentUserLanguage = Env.CurrentUser?.Language ?? string.Empty;
			var language = isAndroidDevice || WinCESupportedLanguages.List.Contains(currentUserLanguage) ? currentUserLanguage : Res.DefaultLanguage;
			ObjectFactory.Get<IResourceStrings>().CurrentLanguage = language;
		}

		#region ValidateAndSetUserContext

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "It is user log in.")]
		static ZString ValidateAndSetUserContext(ImmutableSecuritySOAPHeader securityHeader)
		{
			ZString errorMessage;
			object branchPK;
			object departmentPK;

			using (Db.DisposableActionForDbConnection())
			using (var reader = GetReaderForBranchAndDepartment(securityHeader.SecurityHeader.BranchCode, securityHeader.SecurityHeader.DepartmentCode))
			{
				// read in first row (will only have one)
				reader.Read();

				branchPK = reader[GlbBranchSchema.Constants.PK];
				departmentPK = reader[GlbDepartmentSchema.Constants.PK];

				if (branchPK == DBNull.Value || departmentPK == DBNull.Value)
				{
					errorMessage = Res.GetString("0425b938-3e45-4f14-82fb-c6c3fe1ee954", "Issue loading context for the warehouse. Please reload the application.");
				}
			}

			if (errorMessage.IsEmpty)
			{
				var userContext = new UserContext(securityHeader.SecurityHeader.UserName, (Guid)branchPK, (Guid)departmentPK, reportWrongUser: false);
				using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
				{
					Env.SetUserContext(userContext); // It is user log in.
				}
			}

			return errorMessage;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static IDataReader GetReaderForBranchAndDepartment(string branchCode, string departmentCode)
		{
			var sql = @"
			SELECT
				(SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = @BranchCode) as GB_PK,
				(SELECT GE_PK FROM dbo.GlbDepartment WHERE GE_Code = @DepartmentCode) as GE_PK";

			using (var command = Db.Connection.Command(sql)) // It is cheaper and quicker to bypass using a BusinessObjectFactory here
			{
				command.AddParameterBasedOnDbColumn("@BranchCode", branchCode, GlbBranchSchema.GB_Code);
				command.AddParameterBasedOnDbColumn("@DepartmentCode", departmentCode, GlbDepartmentSchema.GE_Code);

				return command.ExecuteReader();
			}
		}

		#endregion

		#region ValidateUserLogin

		static ZString ValidateUserLogin(WebServiceResponse response, ImmutableSecuritySOAPHeader immutableSecurityHeader, bool requireValidateConcurencyLogin)
		{
			var errorMessage = IsLoginDetailsValid(immutableSecurityHeader.SecurityHeader.UserName, immutableSecurityHeader.SecurityHeader.Password);
			if (errorMessage.IsNullOrEmpty())
			{
				var loginError = ErrorTypes.None;
				if (requireValidateConcurencyLogin)
				{
					if (Env.CurrentUser == null)
					{
						loginError = ErrorTypes.LoginFailed;
						errorMessage = Res.GetString("9e6f99c7-205e-4412-84da-2c6c1fb76853", "Issue loading the user context. Please try again.");
					}
					else
					{
						loginError = ValidateConcurencyLogin(immutableSecurityHeader);
					}
				}

				if (loginError == ErrorTypes.None)
				{
					response.SecurityKey = CreateLicenceLogAndReturnSecurityKey(immutableSecurityHeader);
				}
				else
				{
					response.Error = loginError;
				}
			}

			return errorMessage;
		}

		static RFLoginSemaphoreType SemaphoreType { get; } = new RFLoginSemaphoreType();

		static ErrorTypes ValidateConcurencyLogin(ImmutableSecuritySOAPHeader securityHeader)
		{
			var loginError = ErrorTypes.None;
			var isUserLoggedIn = false;

			var semaphoreProvider = new RFSemaphoreProvider(securityHeader.SecurityHeader);
			var currentActiveSemaphores = ((ISemaphoreProvider)semaphoreProvider).GetActiveSemaphoreHandles(SemaphoreType);
			var currentUserPK = Env.CurrentUser?.PK ?? Guid.Empty;

			var owners = currentActiveSemaphores
				.Select(semaphore => semaphore.OwnerSession)
				.Where(ownerSession => ownerSession.UserPk == currentUserPK)
				.OrderBy(ownerSession => ownerSession.HostName);

			foreach (var owner in owners)
			{
				if (owner.HostName.Equals(securityHeader.SecurityHeader.DeviceID))
				{
					isUserLoggedIn = true;
					break;
				}
				else if (WarehouseDataRegistry.Instance.ValidateConcurrentRFLogins.Value)
				{
					loginError = string.IsNullOrEmpty(securityHeader.SecurityHeader.SecurityKey)
						? ErrorTypes.UserLoggedInFromAnotherDevice
						: ErrorTypes.UserWasRemotelyLoggedOut;
					break;
				}
			}

			if (loginError == ErrorTypes.None && !isUserLoggedIn)
			{
				var key = new SemaphoreHandleKey(securityHeader.SecurityHeader);
				ActiveSemaphoreHandlers.AddActiveSemaphore(semaphoreProvider, key);
			}

			return loginError;
		}

		#region ActiveSemaphoreHandlers

		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		readonly static SemaphoreDictionary ActiveSemaphoreHandlers = new SemaphoreDictionary();

		#endregion

		#region class SemaphoreDictionary

		partial class SemaphoreDictionary
		{
			public void AddActiveSemaphore(RFSemaphoreProvider semaphoreProvider, SemaphoreHandleKey key)
			{
				lock (ActiveSemaphoreHandlersLock)
				{
					if (!ActiveSemaphoreHandlers.ContainsKey(key))
					{
						var semaphoreHandle = SemaphoreProvider.CreateSemaphoreHandle(semaphoreProvider, SemaphoreType);
						ActiveSemaphoreHandlers.Add(key, semaphoreHandle);
					}
				}
			}

			public void RemoveSemaphoresWithSameUserName(SecuritySOAPHeader securityHeader)
			{
				lock (ActiveSemaphoreHandlersLock)
				{
					foreach (var item in ActiveSemaphoreHandlers.Where(k => k.Key.UserName == securityHeader.UserName && k.Key.DeviceID != securityHeader.DeviceID).ToArray())
					{
						item.Value.Dispose();
						ActiveSemaphoreHandlers.Remove(item.Key);
					}
				}
			}

			public void RemoveSemaphore(SemaphoreHandleKey key)
			{
				lock (ActiveSemaphoreHandlersLock)
				{
					if (ActiveSemaphoreHandlers.TryGetValue(key, out var semaphoreHandler))
					{
						ActiveSemaphoreHandlers.Remove(key);
						semaphoreHandler.Dispose();
					}
				}
			}

			readonly Dictionary<SemaphoreHandleKey, ISemaphoreHandle> ActiveSemaphoreHandlers = new Dictionary<SemaphoreHandleKey, ISemaphoreHandle>();
			readonly object ActiveSemaphoreHandlersLock = new object();
		}

		#endregion

		#endregion

		#region CreateLicenceLogAndReturnSecurityKey

		static string CreateLicenceLogAndReturnSecurityKey(ImmutableSecuritySOAPHeader securityHeader)
		{
			string result;

			var newSecurityKey = Utilities.GenerateSecurityToken(securityHeader.SecurityHeader.UserName, securityHeader.SecurityHeader.Password, securityHeader.SecurityHeader.BranchCode, securityHeader.SecurityHeader.DepartmentCode);
			if (securityHeader.SecurityHeader.SecurityKey != newSecurityKey)
			{
				CreateLicenceUsageLogEntry(Env.Instance.Licence.RFScannerManager, securityHeader);
				result = newSecurityKey;
			}
			else
			{
				result = securityHeader.SecurityHeader.SecurityKey;
			}

			return result;
		}

		#region IsLoginDetailsValid

		static string IsLoginDetailsValid(string loginName, string password)
		{
			var controller = new UserLoginController();

			var decrypter = new AESCryptographicProvider(CryptographyKey.Value, CryptographyIv.Value);
			var decodedPassword = Encoding.Unicode.GetString(decrypter.Decrypt(Convert.FromBase64String(password)));

			return controller.ValidateUserLoginAndPassword(loginName, decodedPassword, isDeviceUser: true).FailureMessage;
		}
		[ThreadSafe]
		static readonly internal Lazy<byte[]> CryptographyKey = new Lazy<byte[]>(() => Encoding.ASCII.GetBytes("6052D90C81D64D5B8F5677AA055CAE23"));
		[ThreadSafe]
		static readonly internal Lazy<byte[]> CryptographyIv = new Lazy<byte[]>(() => Encoding.ASCII.GetBytes("acaffc349fc97109"));

		#endregion

		#region CreateLicenceUsageLogEntry

		static void CreateLicenceUsageLogEntry(LicenceCheckpoint checkpoint, ImmutableSecuritySOAPHeader securityHeader)
		{
			var connectionTypeSupporter = ObjectFactory.Get<IHttpRequestManager>();
			var httpContextWrapper = connectionTypeSupporter.GetHttpContextBase();
			var connectionType = (httpContextWrapper?.Request).DetermineConnectionType();

			var logger = ObjectFactory.Get<ILicenceConsumptionLogCreator>();
			logger.CreateLog(
				checkpoint,
				Env.CurrentUser.IsSupportUser ? string.Empty : securityHeader.SecurityHeader.DeviceID,
				Env.CurrentUser.IsSupportUser ? string.Empty : securityHeader.SecurityHeader.DeviceModelDetails,
				(int)connectionType);
		}

		#endregion

		#endregion

		#endregion

		#region LogoutUser

		public static void LogoutUser(SecuritySOAPHeader securityHeader, bool isRemoteLogoff)
		{
			var immutableSecurityHeader = new ImmutableSecuritySOAPHeader(securityHeader);
			ValidateAndSetUserContext(immutableSecurityHeader);

			if (isRemoteLogoff)
			{
				RemoteLogoff(immutableSecurityHeader);
			}
			else
			{
				LocalLogoff(immutableSecurityHeader);
			}
		}

		#region RemoteLogoff

		static void RemoteLogoff(ImmutableSecuritySOAPHeader securityHeader)
		{
			ActiveSemaphoreHandlers.RemoveSemaphoresWithSameUserName(securityHeader.SecurityHeader);
			if (Env.CurrentUser != null)
			{
				SemaphoreProvider.RemoteLogoff(Env.CurrentUser.PK, securityHeader.SecurityHeader.DeviceID, HeartbeatTypes.WarehouseRF, string.Empty);
			}
		}

		#endregion

		#region LocalLogoff

		static void LocalLogoff(ImmutableSecuritySOAPHeader securityHeader)
		{
			var key = new SemaphoreHandleKey(securityHeader.SecurityHeader);
			ActiveSemaphoreHandlers.RemoveSemaphore(key);
		}

		#endregion

		#endregion

		#region Testing

#if DEBUG
		partial class SemaphoreDictionary
		{
			internal Dictionary<SemaphoreHandleKey, ISemaphoreHandle> ExposedDictionaryForTesting
			{
				get { return ActiveSemaphoreHandlers; }
			}
		}

		#region ActiveSemaphoreHandlersForTesting

		public static Dictionary<SemaphoreHandleKey, ISemaphoreHandle> ActiveSemaphoreHandlersForTesting
		{
			get { return ActiveSemaphoreHandlers.ExposedDictionaryForTesting; }
		}

		#endregion

		public static void ClearAllActiveSemaphoreHandlers_ForTesting()
		{
			foreach (var pair in ActiveSemaphoreHandlersForTesting)
			{
				pair.Value.Dispose();
			}
			ActiveSemaphoreHandlersForTesting.Clear();
		}
#endif

		#endregion
	}

	#region ImmutableSecuritySOAPHeader

	class ImmutableSecuritySOAPHeader
	{
		public ImmutableSecuritySOAPHeader(SecuritySOAPHeader securityHeader)
		{
			SecurityHeader = new SecuritySOAPHeader(securityHeader);
		}

		public SecuritySOAPHeader SecurityHeader { get; }
	}

	#endregion

	#region SemaphoreHandleKey struct

	public struct SemaphoreHandleKey
	{
		public SemaphoreHandleKey(SecuritySOAPHeader securityHeader)
		{
			UserName = securityHeader.UserName;
			DeviceID = securityHeader.DeviceID;
			ProcessID = securityHeader.ProcessID;
		}

		public readonly string UserName;
		public readonly string DeviceID;
		public readonly int ProcessID;

		public override bool Equals(object obj) => obj is SemaphoreHandleKey key && key == this;
		public override int GetHashCode() => UserName.GetHashCode() ^ DeviceID.GetHashCode() ^ ProcessID.GetHashCode();

		public static bool operator ==(SemaphoreHandleKey x, SemaphoreHandleKey y) => x.UserName == y.UserName && x.DeviceID == y.DeviceID && x.ProcessID == y.ProcessID;
		public static bool operator !=(SemaphoreHandleKey x, SemaphoreHandleKey y) => !(x == y);
	}

	#endregion
}
