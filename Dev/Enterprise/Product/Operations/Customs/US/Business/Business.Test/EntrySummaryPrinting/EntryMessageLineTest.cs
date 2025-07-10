using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryMessageLine))]
	sealed class EntryMessageLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLineMessageBlocksReturned()
		{
			AssertEquals("40001AU00000000000000009000                    000000005060267                  ", EntryMessageLine.ens40.Serialise());
			AssertEquals("ens43 message blocks expected", 2, EntryMessageLine.ens43.Count);
			AssertEquals("43123456R                                                                       ", EntryMessageLine.ens43[0].Serialise());
			AssertEquals("43      D PLASTIC SEAL                                                          ", EntryMessageLine.ens43[1].Serialise());
			AssertEquals("50 8211100000          000000400000PCS                              AU071407N   ", EntryMessageLine.ens50.Serialise());
			AssertEquals("51                                                                              ", EntryMessageLine.ens51.Serialise());
			AssertEquals("60                                        XYBEREQU6LON                          ", EntryMessageLine.ens60.Serialise());

			AssertEquals("709102111010 0000012871            NO                               0000003406  ", EntryMessageLine.ens70.Serialise());
			AssertEquals("809802004040                                                        0000001010  ", EntryMessageLine.ens80.Serialise());
			AssertEquals("ens81 message blocks expected", 2, EntryMessageLine.ens81.Count);
			AssertEquals("819102111020 0000006080            NO                               0000001609  ", EntryMessageLine.ens81[0].Serialise());
			AssertEquals("819802004040                                                                    ", EntryMessageLine.ens81[1].Serialise());
		}

		EntryMessageLine entryMessageLine;
		EntryMessageLine EntryMessageLine
		{
			get
			{
				if (entryMessageLine == null)
				{
					entryMessageLine = new EntryMessageLine(Factory);
					ENS40 ens40 = new ENS40();
					ENS43 ens43_1 = new ENS43();
					ENS43 ens43_2 = new ENS43();
					List<ENS43> line_ens43List = new List<ENS43>();
					ENS50 ens50 = new ENS50();
					ENS51 ens51 = new ENS51();
					ENS60 ens60 = new ENS60();
					ENS62 ens62 = new ENS62();
					List<ENS62> ens62ChargesList = new List<ENS62>();
					ENS70 ens70 = new ENS70();
					ENS80 ens80 = new ENS80();
					ENS81 ens81_1 = new ENS81();
					ENS81 ens81_2 = new ENS81();
					List<ENS81> ens81SecondaryTariffList = new List<ENS81>();

					ens40.Deserialise("40001AU00000000000000009000                    000000005060267                  ");
					entryMessageLine.ens40 = ens40;

					ens43_1.Deserialise("43123456R                                                                       ");
					ens43_2.Deserialise("43      D PLASTIC SEAL                                                          ");
					line_ens43List.Add(ens43_1);
					line_ens43List.Add(ens43_2);
					entryMessageLine.ens43 = line_ens43List;

					ens50.Deserialise("50 8211100000          000000400000PCS                              AU071407N   ");
					ens51.Deserialise("51                                                                              ");
					ens60.Deserialise("60                                        XYBEREQU6LON                          ");
					ens62.Deserialise("62          49900005040                                                         ");
					entryMessageLine.ens50 = ens50;
					entryMessageLine.ens51 = ens51;
					entryMessageLine.ens60 = ens60;
					ens62ChargesList.Add(ens62);
					entryMessageLine.ens62 = ens62ChargesList;

					ens70.Deserialise("709102111010 0000012871            NO                               0000003406  ");
					entryMessageLine.ens70 = ens70;
					ens80.Deserialise("809802004040                                                        0000001010  ");
					entryMessageLine.ens80 = ens80;
					ens81_1.Deserialise("819102111020 0000006080            NO                               0000001609  ");
					ens81SecondaryTariffList.Add(ens81_1);
					ens81_2.Deserialise("819802004040                                                                    ");
					ens81SecondaryTariffList.Add(ens81_2);
					entryMessageLine.ens81 = ens81SecondaryTariffList;
				}

				return entryMessageLine;
			}
		}
	}
}
