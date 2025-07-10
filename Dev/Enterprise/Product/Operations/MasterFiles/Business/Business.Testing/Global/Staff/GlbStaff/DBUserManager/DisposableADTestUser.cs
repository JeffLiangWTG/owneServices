using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using CargoWise.ActiveDirectory;
using static System.FormattableString;
using ADTestAdmin = CargoWise.ActiveDirectory.TestFramework.TestConstants.ADTestAdminAccount;
using ADTests = CargoWise.ActiveDirectory.TestFramework.TestConstants;

namespace Enterprise.MasterFiles.Business.Testing.Global.Staff.GlbStaff.DBUserManager
{
	class DisposableAdTestUser : IDisposable
	{
		public DisposableAdTestUser()
		{
			var shortName = Path.GetRandomFileName();
			UserName = GetUniqueNameShorterThan20Chars();
			LoginName = Invariant($"{UserName}@{DomainName}");
			Password = ADTestAdmin.Password;

			using var impersonator = new WindowsIdentityImpersonator(ADTestAdmin.NameWithDomainPreWindows2000, ADTestAdmin.Password, () =>
			{
				var searcher = new DirectorySearcherWrapper();
				var ou = searcher.FindOrganisationalUnit(ADTests.ValidOU);
				var newUser = (DirectoryEntryWrapper)ou.CreateNewChild(UserName, DirectoryObjectType.User, searcher);

				newUser.DirectoryEntry.Properties[ADAttributes.UserPrincipalName].Add(LoginName);
				newUser.DirectoryEntry.Properties[ADAttributes.DisplayName].Value = UserName;
				newUser.DirectoryEntry.Properties[ADAttributes.Description].Value = nameof(DisposableAdTestUser);

				((IUserDirectoryEntry)newUser).SetPassword(Password);
				newUser.IsActive = true;
				newUser.PasswordMustChangeAtNextLogon = false;
				newUser.HasChanges = true;
				newUser.CommitChanges();
				newUser.RefreshCache();

				Sid = newUser.DirectoryEntry.Properties["objectSid"]?.Value as byte[];
				ObjectGuid = newUser.Guid;
				SAMAccountName = ((IUserDirectoryEntry)newUser).Win2KName;
			});

			WaitAndVerifySamAccountName();
		}

		void WaitAndVerifySamAccountName()
		{
			var maxWaitTimeSpan = TimeSpan.FromSeconds(30);
			var cancellationTokenSource = new CancellationTokenSource(maxWaitTimeSpan);
			var domainHasPersistedChanges = false;

			using var impersonator = new WindowsIdentityImpersonator(ADTestAdmin.NameWithDomainPreWindows2000, ADTestAdmin.Password, () =>
			{
				var searcher = new DirectorySearcherWrapper();
				while (!cancellationTokenSource.IsCancellationRequested)
				{
					using (var user = searcher.FindUser(ObjectGuid, ADTests.ValidOU))
					{
						if (user != null && Equals(((DirectoryEntryWrapper)user).DirectoryEntry.Properties[ADAttributes.SAMAccountName].Value, SAMAccountName))
						{
							domainHasPersistedChanges = true;
							break;
						}
					}

					Thread.Sleep(100);
				}
			});

			if (!domainHasPersistedChanges)
			{
				throw new NotSupportedException($"{ADTestAdmin.NameWithDomainPreWindows2000} has not persisted our change yet within {maxWaitTimeSpan.TotalSeconds} seconds.");
			}
		}

		static string GetUniqueNameShorterThan20Chars()
		{
			var hostName = IPGlobalProperties.GetIPGlobalProperties().HostName.Replace("-", string.Empty);
			if (hostName.Length > 6)
			{
				hostName = hostName.Substring(6);
			}

			var uniqueName = Invariant($"{hostName}{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}");
			return uniqueName.Length > 20 ? uniqueName.Substring(0, 20) : uniqueName;
		}

		public string ChangeSamAccountName()
		{
			using var impersonator = new WindowsIdentityImpersonator(ADTestAdmin.NameWithDomainPreWindows2000, ADTestAdmin.Password, () =>
			{
				var searcher = new DirectorySearcherWrapper();
				using (var user = searcher.FindUser(ObjectGuid, ADTests.ValidOU))
				{
					((DirectoryEntryWrapper)user).DirectoryEntry.Properties[ADAttributes.SAMAccountName].Value = GetUniqueNameShorterThan20Chars();
					user.HasChanges = true;
					user.CommitChanges();
					user.RefreshCache();

					SAMAccountName = user.Win2KName;
				}
			});

			WaitAndVerifySamAccountName();

			return SAMAccountName;
		}

		public string NameWithDomainPreWindows2000 => Invariant($"{DomainNetBiosName}\\{SAMAccountName}");

		public string SAMAccountName { get; private set; }

		public string LoginName { get; }

		public string UserName { get; }

		public string Password { get; }

		public Guid ObjectGuid { get; private set; }

		public byte[] Sid { get; private set; }

		public void Dispose()
		{
			using var impersonator = new WindowsIdentityImpersonator(ADTestAdmin.NameWithDomainPreWindows2000, ADTestAdmin.Password, () =>
			{
				var searcher = new DirectorySearcherWrapper();

				using (var user = searcher.FindUser(ObjectGuid, ADTests.ValidOU))
				{
					try
					{
						user?.Delete();
					}
					catch
					{
						// ignore
					}
				}
			});
		}

		public const string OU = ADTests.ValidOU;
		public const string DomainName = ADTests.Domain;
		public const string DomainNetBiosName = ADTests.DomainPreWin2000;
	}
}
