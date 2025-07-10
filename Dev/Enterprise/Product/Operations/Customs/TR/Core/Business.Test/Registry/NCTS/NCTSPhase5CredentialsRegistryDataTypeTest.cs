using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(NCTSPhase5CredentialsRegistryItemDataType))]
	sealed class NCTSPhase5CredentialsRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<NCTSPhase5CredentialsRegistryItemDataType>
	{
		protected override string ExpectedEditorName => "NCTSPhase5CredentialsRegistryItemEditor";

		protected override NCTSPhase5CredentialsRegistryItemDataType GetNewDataType() => new NCTSPhase5CredentialsRegistryItemDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var fallbackLevel = new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
			var dataType = new NCTSPhase5CredentialsRegistryItemDataType();

			var item1 = CreateNCTSPhase5Credentials(fallbackLevel, factory, "NCTSTraderUser", "3vAT.2G98Ar!", "WiseTech", "11111111108", "12345678");
			var item2 = CreateNCTSPhase5Credentials(fallbackLevel, factory, "NCTSTraderUser2", "3vAT.2G98Ar!", "WiseTech", "11111111109", "12345679");

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(item1, dataType.Serialise(item1)),
				new ValidSampleAndBinaryValueInDB(item2, dataType.Serialise(item2))
			};
		}

		NCTSPhase5Credentials CreateNCTSPhase5Credentials(FallbackLevel fallbackLevel, BusinessObjectFactory factory, string basicAuthUsername, string basicAuthPassword, string firmID, string requestUserID, string requestPassword)
		{
			return new NCTSPhase5Credentials(fallbackLevel, factory)
			{
				BasicAuthUsername = basicAuthUsername,
				BasicAuthPassword = basicAuthPassword,
				FirmID = firmID,
				RequestUserID = requestUserID,
				RequestPassword = requestPassword,
			};
		}
	}
}
