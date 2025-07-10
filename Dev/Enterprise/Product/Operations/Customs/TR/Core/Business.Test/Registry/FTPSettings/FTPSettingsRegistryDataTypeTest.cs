using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(FTPSettingsRegistryItemDataType))]
	sealed class FTPSettingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<FTPSettingsRegistryItemDataType>
	{
		protected override string ExpectedEditorName => "FTPSettingsRegistryItemEditor";

		protected override FTPSettingsRegistryItemDataType GetNewDataType() => new FTPSettingsRegistryItemDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var fallbackLevel = new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
			var dataType = new FTPSettingsRegistryItemDataType();

			var item1 = CreateFTPSettings(fallbackLevel, factory, ExportUnionFTPAddressList.Codes.FtpistanbulEbirlikNet, 21, "testinbox1", "testoutbox1");
			var item2 = CreateFTPSettings(fallbackLevel, factory, ExportUnionFTPAddressList.Codes.FtpankaraEbirlikNet, 22, "testinbox1", "testoutbox2");

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(item1, dataType.Serialise(item1)),
				new ValidSampleAndBinaryValueInDB(item2, dataType.Serialise(item2))
			};
		}

		FTPSettings CreateFTPSettings(FallbackLevel fallbackLevel, BusinessObjectFactory factory, string address, int port, string inbox, string outbox)
		{
			return new FTPSettings(fallbackLevel, factory)
			{
				FTPAddress = address,
				Port = port,
				Inbox = inbox,
				Outbox = outbox
			};
		}
	}
}
