using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CDArchiveInfoTest : TestCaseWithFactory
	{
		CDArchiveInfoForTesting info;

		CDArchiveInfoForTesting Info
		{
			get
			{
				if (info == null)
				{
					OrgHeader organization = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
					info = new CDArchiveInfoForTesting(organization);
				}
				return info;
			}
		}

		public void TestAddToList()
		{
			List<ZString> list = new List<ZString>();

			CDArchiveInfo.AddToList(list, "");
			AssertEquals("Count", 0, list.Count);

			CDArchiveInfo.AddToList(list, "x");
			AssertEquals("Count", 1, list.Count);
			AssertEquals("[0]", "x", list[0]);

			CDArchiveInfo.AddToList(list, "x");
			AssertEquals("Count", 1, list.Count);

			CDArchiveInfo.AddToList(list, new ZString[] { "x", "y", "z" });
			AssertEquals("Count", 3, list.Count);
			AssertEquals("[0]", "x", list[0]);
			AssertEquals("[1]", "y", list[1]);
			AssertEquals("[2]", "z", list[2]);
		}

		public void TestGetCombinedList()
		{
			var mockInfo1 = new Mock<CDArchiveInfo>();
			var mockInfo2 = new Mock<CDArchiveInfo>();

			mockInfo1.Setup(m => m.EntryNumbersList).Returns(new ZString[] { "en1", "en2" });
			mockInfo2.Setup(m => m.EntryNumbersList).Returns(new ZString[] { "en2", "en3" });

			var mockArchive1 = new Mock<ICDArchive>();
			var mockArchive2 = new Mock<ICDArchive>();

			mockArchive1.Setup(m => m.CDArchiveInfo).Returns(mockInfo1.Object);
			mockArchive2.Setup(m => m.CDArchiveInfo).Returns(mockInfo2.Object);

			ICDArchive[] cdArchives = new ICDArchive[] { mockArchive1.Object, mockArchive2.Object };

			ZString[] entryNumbers = Info.GetCombinedList(cdArchives, delegate(CDArchiveInfo info)
			{
				return info.EntryNumbersList;
			});

			AssertEquals("Length", 3, entryNumbers.Length);
			AssertEquals("[0]", "en1", entryNumbers[0]);
			AssertEquals("[1]", "en2", entryNumbers[1]);
			AssertEquals("[2]", "en3", entryNumbers[2]);
		}

		public void TestPropertiesHashKey()
		{
			AssertEquals("Key 'Job Number' should exist", true, Info.Properties.ContainsCode("Job Number"));
			AssertEquals("Key 'Housebill' should exist", true, Info.Properties.ContainsCode("House bill"));
			AssertEquals("Key 'Masterbill' should exist", true, Info.Properties.ContainsCode("Master bill"));
			AssertEquals("Key 'Vessel' should exist", true, Info.Properties.ContainsCode("Vessel"));
			AssertEquals("Key 'Voyage' should exist", true, Info.Properties.ContainsCode("Voyage/Flight"));
			AssertEquals("Key 'ETD' should exist", true, Info.Properties.ContainsCode("ETD"));
			AssertEquals("Key 'ETA' should exist", true, Info.Properties.ContainsCode("ETA"));
			AssertEquals("Key 'Container Numbers' should exist", true, Info.Properties.ContainsCode("Container Numbers"));
			AssertEquals("Key 'Consignee Code' should exist", true, Info.Properties.ContainsCode("Consignee Code"));
			AssertEquals("Key 'Consignor Code' should exist", true, Info.Properties.ContainsCode("Consignor Code"));
			AssertEquals("Key 'Order Numbers' should exist", true, Info.Properties.ContainsCode("Order Numbers"));
			AssertEquals("Key 'Entry Number' should exist", true, Info.Properties.ContainsCode("Entry Number"));
			AssertEquals("Key 'Invoice Numbers' should exist", true, Info.Properties.ContainsCode("Invoice Numbers"));
			AssertEquals("Key 'Origin' should exist", true, Info.Properties.ContainsCode("Origin"));
			AssertEquals("Key 'Destination' should exist", true, Info.Properties.ContainsCode("Destination"));
		}

		public void TestProperties()
		{
			AssertEquals("Job Number property", "S00001000", Info.Properties.GetDescriptionFromCode("Job Number"));
			AssertEquals("Housebill property", "H12345", Info.Properties.GetDescriptionFromCode("House bill"));
			AssertEquals("Masterbill property", "M12345", Info.Properties.GetDescriptionFromCode("Master bill"));
			AssertEquals("Vessel property", "VESSEL ABC", Info.Properties.GetDescriptionFromCode("Vessel"));
			AssertEquals("Voyage property", "VOYAGE 12345", Info.Properties.GetDescriptionFromCode("Voyage/Flight"));
			AssertEquals("ETD property", new ZDateTime(2004, 12, 12).ToLongTimeString(), Info.Properties.GetDescriptionFromCode("ETD"));
			AssertEquals("ETA property", new ZDateTime(2004, 12, 24).ToLongTimeString(), Info.Properties.GetDescriptionFromCode("ETA"));
			AssertEquals("Container Numbers property", "11111, 22222, 33333, 44444, 55555", Info.Properties.GetDescriptionFromCode("Container Numbers"));
			AssertEquals("Consignee Code property", "CONSIGNEE", Info.Properties.GetDescriptionFromCode("Consignee Code"));
			AssertEquals("Consignor Code property", "CONSIGNOR", Info.Properties.GetDescriptionFromCode("Consignor Code"));
			AssertEquals("Order Numbers property", "1111, 2222, 3333, 4444, 5555", Info.Properties.GetDescriptionFromCode("Order Numbers"));
			AssertEquals("Entry Number property", "B00001000, B00001001", Info.Properties.GetDescriptionFromCode("Entry Number"));
			AssertEquals("Invoice numbers property", "111, 222, 333, 444, 555", Info.Properties.GetDescriptionFromCode("Invoice Numbers"));
			AssertEquals("Origin property", "AUSYD", Info.Properties.GetDescriptionFromCode("Origin"));
			AssertEquals("Destination property", "USLAX", Info.Properties.GetDescriptionFromCode("Destination"));
		}
	}
}
