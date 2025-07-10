using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(TelPreDriveChecklistHeader))]
	class TelPreDriveChecklistHeaderTests : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var o = (TelPreDriveChecklistHeader)base.GetNewBusinessObjectForDeleteTest(factory);
			o.TPH_Type = TelPreDriveChecklistHeaderTypes.Codes.PDR;
			o.TPH_IsProcessed = true;
			return o;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var o = (TelPreDriveChecklistHeader)base.GetBusinessObjectForFetchForLoad();
			o.TPH_Type = TelPreDriveChecklistHeaderTypes.Codes.PDR;
			o.TPH_IsProcessed = true;
			return o;
		}
	}

	class TelPreDriveChecklistHeaderPropertiesTest : TestCaseWithFactory
	{
		public void TestAllEntriesCompleted()
		{
			CombineAssertions(() =>
			{
				Test(
					new ZDateTime(2020, 1, 1, DateTimeKind.Utc),
					"ASD",
					new[]
					{
						(1, "Y", "ASD"),
						(2, "Y", "ASD"),
						(3, "Y", "ASD"),
						(4, "Y", "ASD"),
					},
					true);
				Test(
					new ZDateTime(2016, 1, 1, DateTimeKind.Utc),
					"DSA",
					new[]
					{
						(1, "Y", "THIng"),
						(2, "N", "THIng"),
						(3, "N", "THIng"),
						(4, "N", "THIng"),
					},
					false);
				Test(
					new ZDateTime(2020, 2, 3, DateTimeKind.Utc),
					"ASD",
					new[]
					{
						(1, "Y", "Lorem Ipsum"),
						(2, "", "Lorem Ipsum"),
						(3, "", "Lorem Ipsum"),
						(4, "", "Lorem Ipsum"),
					},
					false);
				Test(
					new ZDateTime(2020, 1, 1, DateTimeKind.Utc),
					"bob",
					new[]
					{
						(1, "Y", "Random gibberish"),
						(2, "N", "Random gibberish"),
						(3, "", "Random gibberish"),
						(4, "Y", "Random gibberish"),
					},
					false);
			});

			void Test(ZDateTime time, string driver, IEnumerable<(int index, string agreed, string description)> entriesToCreate, bool expectedResult)
			{
				// Arrange
				var header = Factory.New<TelPreDriveChecklistHeader>();
				header.TPH_SystemCreateTimeUtc = time;
				header.TPH_SystemCreateUser = driver;
				header.TPH_GS_NKDriver = driver;

				var entries = entriesToCreate.Select(entry => CreateEntry(header, (ZShort)entry.index, entry.agreed, entry.description));

				// Act
				// Assert
				AssertEquals(true, header.AllEntriesCompleted);
			}
		}

		TelPreDriveChecklistEntry CreateEntry(TelPreDriveChecklistHeader header, ZShort index, string agreed, string description)
		{
			var entry = Factory.New<TelPreDriveChecklistEntry>();
			entry.TPE_TPH_ChecklistHeader = header.PK;
			entry.TPE_Index = index;
			entry.TPE_IsAgreed = agreed;
			entry.TPE_Description = description;

			return entry;
		}
	}
}
