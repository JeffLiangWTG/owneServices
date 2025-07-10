using System;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWNCATKClientSettingRegistryDataType))]
	sealed class TWNCATKClientSettingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TWNCATKClientSettingRegistryDataType>
	{
		protected override TWNCATKClientSettingRegistryDataType GetNewDataType()
		{
			return new TWNCATKClientSettingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample1 = new TWNCATKClientSetting { MachineName = "Machine Name", SendToFolder = @"D:\Folders\SendFolder", RunningIntervalInSeconds = 60 };
			var sample2 = new TWNCATKClientSetting { MachineName = "Machine Name 2", SendToFolder = @"D:\Folders\SendFolder", RunningIntervalInSeconds = 60 };
			return new[] { new ValidSampleAndBinaryValueInDB(sample1, new TWNCATKClientSettingRegistryDataType().Serialise(sample1)), new ValidSampleAndBinaryValueInDB(sample2, new TWNCATKClientSettingRegistryDataType().Serialise(sample2)) };
		}

		protected override string ExpectedEditorName => "TWNCATKClientSettingRegistryItemEditor";
		public override void TestISDefaultImmutable()
		{
			var registryItem = new TWNCATKClientSettingRegistryItem("", null, null, null, RegistryStorageFlags.Company);
			var clientSetting = registryItem.Value;
			AssertExceptionThrown<InvalidOperationException>(() => clientSetting.RunningIntervalInSeconds = 55);
			var clone = clientSetting.Clone(clientSetting.CurrentFallbackLevel, clientSetting.Factory) as TWNCATKClientSetting;
			AssertNoExceptionThrown(() => clone.RunningIntervalInSeconds = 55);
		}
	}
}
