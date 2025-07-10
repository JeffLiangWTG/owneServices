using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(FTPSettingsRegistryItem))]
	sealed class FTPSettingsRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<FTPSettings>
	{
		protected override StronglyTypedRegistryItem<FTPSettings, FTPSettings> GetNewRegistryItem()
		{
			return new FTPSettingsRegistryItem(
				"",
				null,
				null,
				null,
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport
			);
		}

		protected override FTPSettings ValidValue
		{
			get
			{
				var result = new FTPSettings(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
				result.FTPAddress = ExportUnionFTPAddressList.Codes.FtpistanbulEbirlikNet;
				result.Port = 21;
				result.Inbox = "inbox";
				result.Outbox = "outbox";

				return result;
			}
		}
	}
}
