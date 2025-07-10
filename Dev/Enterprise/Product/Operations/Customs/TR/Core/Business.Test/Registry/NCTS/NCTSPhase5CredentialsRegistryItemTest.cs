using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(NCTSPhase5CredentialsRegistryItem))]
	sealed class NCTSPhase5CredentialsSettingsRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<NCTSPhase5Credentials>
	{
		protected override StronglyTypedRegistryItem<NCTSPhase5Credentials, NCTSPhase5Credentials> GetNewRegistryItem()
		{
			return new NCTSPhase5CredentialsRegistryItem(
				"",
				null,
				null,
				null,
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport
			);
		}

		protected override NCTSPhase5Credentials ValidValue
		{
			get
			{
				var result = new NCTSPhase5Credentials(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
				result.BasicAuthUsername = "NCTSTraderUser";
				result.BasicAuthPassword = "3vAT.2G98Ar!";
				result.FirmID = "WiseTech";
				result.RequestUserID = "11111111108";
				result.RequestPassword = "12345678";

				return result;
			}
		}
	}
}
