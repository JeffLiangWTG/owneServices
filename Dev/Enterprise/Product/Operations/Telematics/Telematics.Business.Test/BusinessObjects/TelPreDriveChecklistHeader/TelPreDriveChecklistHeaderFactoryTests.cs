using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test.BusinessObjects.TelPreDriveChecklistHeader
{
	class TelPreDriveChecklistHeaderFactoryTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestHeadersWithDifferentTypeDoNotCollideOnTime()
		{
			CombineAssertions(() =>
			{
				Test(new ZDateTime(2020, 1, 2, 3, 4, 5));
				Test(new ZDateTime(2016, 5, 4, 3, 2, 1));
			});

			void Test(ZDateTime time)
			{
				// Arrange
				var header1 = Factory.New<Business.TelPreDriveChecklistHeader>();
				header1.TPH_GS_NKDriver = "ASD";
				header1.TPH_SystemCreateUser = "ASD";
				header1.TPH_SystemCreateTimeUtc = time;
				header1.TPH_ChecklistCreateTimeUtc = time;
				header1.TPH_Type = TelPreDriveChecklistHeaderTypes.Codes.FTD;

				var header2 = Factory.New<Business.TelPreDriveChecklistHeader>();
				header2.TPH_GS_NKDriver = "ASD";
				header2.TPH_SystemCreateUser = "ASD";
				header2.TPH_SystemCreateTimeUtc = time;
				header2.TPH_ChecklistCreateTimeUtc = time;
				header2.TPH_Type = TelPreDriveChecklistHeaderTypes.Codes.PDR;

				// Act
				// Assert
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		public void TestCompletionLocalTime()
		{
			CombineAssertions(() =>
			{
				Test(new ZDateTime(2021, 1, 2, 3, 4, 5, 6), "TLV", new ZDateTime(2021, 1, 2, 5, 4, 5, 6, DateTimeKind.Local));
				Test(new ZDateTime(2021, 1, 2, 3, 4, 5, 6), "JFK", new ZDateTime(2021, 1, 1, 22, 4, 5, 6, DateTimeKind.Local));
			});

			void Test(ZDateTime baseTime, string portCode, ZDateTime expectedDateTime)
			{
				// Arrange
				GlbBranch.GetCurrentBranch(Factory).GB_RL_NKHomePort = RefUNLOCO.LoadFromIATA(Factory, portCode).Code;

				var header = Factory.New<Business.TelPreDriveChecklistHeader>();
				header.TPH_GS_NKDriver = "ASD";
				header.TPH_SystemCreateUser = "ASD";
				header.TPH_SystemCreateTimeUtc = baseTime;
				header.TPH_ChecklistCreateTimeUtc = baseTime;
				header.TPH_Type = TelPreDriveChecklistHeaderTypes.Codes.FTD;

				// Act
				var result = header.CompletionTimeLocal;

				// Assert
				AssertEquals(expectedDateTime, result);
			}
		}
	}
}
