using CargoWise.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	class OutputMessageBlockDeserialiserTest : TestCase
	{
		public void TestVersionForCargoReleaseStatus()
		{
			var deserialiser = new OutputMessageBlockDeserialiser();
			var so70Data = "SO70".PadRight(80);
			var so71Data = "SO71".PadRight(80);
			var so72Data = "SO72".PadRight(80);
			var so70DataWithVersion01 = "SO70".PadRight(78) + "01";
			var so70DataWithVersion02 = "SO70".PadRight(78) + "02";
			var so70DataWithVersion03 = "SO70".PadRight(78) + "03";
			var so70DataWithVersion99 = "SO70".PadRight(78) + "99";
			AssertEquals(typeof(ASESSO71), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus, so71Data).GetType());
			AssertEquals(typeof(ASESSO70), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus, so70Data).GetType());
			AssertEquals(typeof(ASESSO70_01), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus, so70DataWithVersion01).GetType());
			AssertEquals(typeof(ASESSO70_02), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus, so70DataWithVersion02).GetType());
			AssertEquals(typeof(ASESSO70_03), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus, so70DataWithVersion03).GetType());
			AssertEquals("", ErrorReporter.LastKeyReported);
			AssertEquals("", ErrorReporter.LastMessageReported);
			AssertEquals(typeof(ASESSO70_03), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus, so70DataWithVersion99).GetType());
			AssertEquals("USI_SO_SO70  _99", ErrorReporter.LastKeyReported);
			AssertEquals(string.Format("The following block is not currently supported (Application Code: USI, Application ID: SO, Version: 99, Block: '{0}').", so70DataWithVersion99), ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertEquals(typeof(ASESSO71), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification, so71Data).GetType());
			AssertEquals(typeof(ASESSO70), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification, so70Data).GetType());
			AssertEquals(typeof(ASESSO70_01), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification, so70DataWithVersion01).GetType());
			AssertEquals(typeof(ASESSO70_02), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification, so70DataWithVersion02).GetType());
			AssertEquals(typeof(ASESSO70_03), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification, so70DataWithVersion03).GetType());
			AssertEquals("", ErrorReporter.LastKeyReported);
			AssertEquals("", ErrorReporter.LastMessageReported);
			AssertEquals(typeof(ASESSO70_03), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification, so70DataWithVersion99).GetType());
			AssertEquals("USI_UC_SO70  _99", ErrorReporter.LastKeyReported);
			AssertEquals(string.Format("The following block is not currently supported (Application Code: USI, Application ID: UC, Version: 99, Block: '{0}').", so70DataWithVersion99), ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestVersionForCargoManifestEntryReleaseStatusQuery()
		{
			var deserialiser = new OutputMessageBlockDeserialiser();
			var wo70Data = "WO70".PadRight(80);
			var wo71Data = "WO71".PadRight(80);
			var wo70DataWithVersion01 = "WO70".PadRight(78) + "01";
			var wo70DataWithVersion02 = "WO70".PadRight(78) + "02";
			var wo70DataWithVersion03 = "WO70".PadRight(78) + "03";
			var wo70DataWithVersion99 = "WO70".PadRight(78) + "99";
			AssertEquals(typeof(ACEQWO71), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, wo71Data).GetType());
			AssertEquals(typeof(ACEQWO70), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, wo70Data).GetType());
			AssertEquals(typeof(ACEQWO70_01), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, wo70DataWithVersion01).GetType());
			AssertEquals(typeof(ACEQWO70_02), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, wo70DataWithVersion02).GetType());
			AssertEquals(typeof(ACEQWO70_03), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, wo70DataWithVersion03).GetType());
			AssertEquals("", ErrorReporter.LastKeyReported);
			AssertEquals("", ErrorReporter.LastMessageReported);
			AssertEquals(typeof(ACEQWO70_03), deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, wo70DataWithVersion99).GetType());
			AssertEquals("USI_C1_WO70  _99", ErrorReporter.LastKeyReported);
			AssertEquals(string.Format("The following block is not currently supported (Application Code: USI, Application ID: C1, Version: 99, Block: '{0}').", wo70DataWithVersion99), ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDeserialiserContainsOutputBlock()
		{
			AssertEquals(typeof(ZZZY), new OutputMessageBlockDeserialiser().GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1, "Z¿ºY".PadRight(80)).GetType());
		}

		public void TestOutputDeserialiserContainInputBlocksThatAreUsedForOutput()
		{
			OutputMessageBlockDeserialiser deserialiser = new OutputMessageBlockDeserialiser();
			AssertNoExceptionThrown(() => deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1, "Z¿ºB".PadRight(80)));
			AssertNoExceptionThrown(() => deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting2, "Z¿ºC".PadRight(80)));
		}
	}
}
