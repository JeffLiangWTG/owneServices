using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class PGAPG06Test : BIRDPGAScientificRecordTest
	{
		public void TestOneSpecyWithMultipleCountries()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();

			var pgaBlocks = new List<MessageBlock>();

			DeserialiseAndAddToList(pgaBlocks, typeof(AENSOI), "OI        SOFTWOOD PULPWOOD 4 FOOT LENGTH                                       ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG01), "PG01001AP                                                                       ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG04), "PG04 PINE                                                000000020000M3         ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG05), "PG05PINUS                 TAEDA                                                 ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG06), "PG06HRVGB                                                                       ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG06), "PG06HRVFR                                                                       ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG06), "PG06HRVDE                                                                       ");

			var notifications = new NotificationCollection();
			new BIRDOGAAdapter().DoImport(invoiceLine, pgaBlocks, notifications);

			Assert(!notifications.HasNotifications());

			AssertEquals("One PGA line should have been created", 1, invoiceLine.LaceyActLines.Count);
			AssertEquals("One constituent element should have been created", 1, invoiceLine.LaceyActLines[0].PG04ConstituentElements.Count);

			var constituentElement = invoiceLine.LaceyActLines[0].PG04ConstituentElements[0];
			AssertEquals("Three Scientific data should have created", 3, constituentElement.ScientificDataCollection.Count);

			AssertEquals("PINUS", constituentElement.ScientificDataCollection[0].US_PGAScientificGenusName);
			AssertEquals("TAEDA", constituentElement.ScientificDataCollection[0].US_PGAScientificSpeciesName);
			AssertEquals("GB", constituentElement.ScientificDataCollection[0].US_PGACountryCode);

			AssertEquals("PINUS", constituentElement.ScientificDataCollection[1].US_PGAScientificGenusName);
			AssertEquals("TAEDA", constituentElement.ScientificDataCollection[1].US_PGAScientificSpeciesName);
			AssertEquals("FR", constituentElement.ScientificDataCollection[1].US_PGACountryCode);

			AssertEquals("PINUS", constituentElement.ScientificDataCollection[2].US_PGAScientificGenusName);
			AssertEquals("TAEDA", constituentElement.ScientificDataCollection[2].US_PGAScientificSpeciesName);
			AssertEquals("DE", constituentElement.ScientificDataCollection[2].US_PGACountryCode);
		}

		protected override IBIRDPGAScientificRecord[] GetPopulatedRecords()
		{
			PGAPG06 pg06 = new PGAPG06();
			pg06.CountryCode = "AU";
			pg06.SourceTypeCode = "HRV";
			return new IBIRDPGAScientificRecord[] { pg06 };
		}

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				//These are not populated at all in message builders
				"GeographicLocation",
				"RangeOfProcessingDate",
				"ProcessingType",
				"ProcessingDescription",
			};
		}

		protected override Type GetTypeOfMessageBlock() => typeof(PGAPG06);

		void DeserialiseAndAddToList(List<MessageBlock> pgaBlocks, Type type, string message)
		{
			var block = (MessageBlock)Activator.CreateInstance(type);
			block.Deserialise(message);
			pgaBlocks.Add(block);
		}
	}
}
