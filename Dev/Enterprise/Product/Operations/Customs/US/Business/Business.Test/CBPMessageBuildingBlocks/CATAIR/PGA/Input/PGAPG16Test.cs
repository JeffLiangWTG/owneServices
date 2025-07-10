using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class PGAPG16Test : BIRDPGAScientificRecordTest
	{
		public void TestOneSpecyWithMultipleCountries()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();

			var pgaBlocks = new List<MessageBlock>();

			DeserialiseAndAddToList(pgaBlocks, typeof(AENSOI), "OI        WOODEN DESK  PINE/OAK/BIRCH                                           ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG01), "PG01001AP                                                                       ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG04), "PG04 PINE                                                000000040000NO         ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG15), "PG15PINUS                 TAEDA                                                 ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG15), "PG15PINUS                 RIGIDA                                                ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG15), "PG15PINUS                 ECHINDA                                               ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG16), "PG16HRVCA                                                                       ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG04), "PG04 OAK                                                 000000040000NO         ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG15), "PG15QUERCUS               RUBRA                                                 ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG15), "PG15QUERCUS               ALBA                                                  ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG15), "PG15QUERCUS               ELLIPSOIDALIS                                         ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG16), "PG16HRVCA                                                                       ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG04), "PG04 BIRCH                                               000000040000NO         ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG15), "PG15BETULA                ALLEGHANIENSIS                                        ");
			DeserialiseAndAddToList(pgaBlocks, typeof(PGAPG16), "PG16HRVCA                                                                       ");

			var notifications = new NotificationCollection();
			new BIRDOGAAdapter().DoImport(invoiceLine, pgaBlocks, notifications);

			Assert(!notifications.HasNotifications());

			AssertEquals("One PGA line should have been created", 1, invoiceLine.LaceyActLines.Count);
			AssertEquals("Three constituent elements should have been created", 3, invoiceLine.LaceyActLines[0].PG04ConstituentElements.Count);

			//First ConstituentElement
			var constituentElement = invoiceLine.LaceyActLines[0].PG04ConstituentElements[0];
			AssertEquals("Three Scientific data should have created", 3, constituentElement.ScientificDataCollection.Count);

			AssertEquals("PINUS", constituentElement.ScientificDataCollection[0].US_PGAScientificGenusName);
			AssertEquals("TAEDA", constituentElement.ScientificDataCollection[0].US_PGAScientificSpeciesName);
			AssertEquals("CA", constituentElement.ScientificDataCollection[0].US_PGACountryCode);

			AssertEquals("PINUS", constituentElement.ScientificDataCollection[1].US_PGAScientificGenusName);
			AssertEquals("RIGIDA", constituentElement.ScientificDataCollection[1].US_PGAScientificSpeciesName);
			AssertEquals("CA", constituentElement.ScientificDataCollection[1].US_PGACountryCode);

			AssertEquals("PINUS", constituentElement.ScientificDataCollection[2].US_PGAScientificGenusName);
			AssertEquals("ECHINDA", constituentElement.ScientificDataCollection[2].US_PGAScientificSpeciesName);
			AssertEquals("CA", constituentElement.ScientificDataCollection[2].US_PGACountryCode);

			//Second ConstituentElement
			constituentElement = invoiceLine.LaceyActLines[0].PG04ConstituentElements[1];
			AssertEquals("Three Scientific data should have created", 3, constituentElement.ScientificDataCollection.Count);

			AssertEquals("QUERCUS", constituentElement.ScientificDataCollection[0].US_PGAScientificGenusName);
			AssertEquals("RUBRA", constituentElement.ScientificDataCollection[0].US_PGAScientificSpeciesName);
			AssertEquals("CA", constituentElement.ScientificDataCollection[0].US_PGACountryCode);

			AssertEquals("QUERCUS", constituentElement.ScientificDataCollection[1].US_PGAScientificGenusName);
			AssertEquals("ALBA", constituentElement.ScientificDataCollection[1].US_PGAScientificSpeciesName);
			AssertEquals("CA", constituentElement.ScientificDataCollection[1].US_PGACountryCode);

			AssertEquals("QUERCUS", constituentElement.ScientificDataCollection[2].US_PGAScientificGenusName);
			AssertEquals("ELLIPSOIDALIS", constituentElement.ScientificDataCollection[2].US_PGAScientificSpeciesName);
			AssertEquals("CA", constituentElement.ScientificDataCollection[2].US_PGACountryCode);

			//Third ConstituentElement
			constituentElement = invoiceLine.LaceyActLines[0].PG04ConstituentElements[2];
			AssertEquals("Three Scientific data should have created", 1, constituentElement.ScientificDataCollection.Count);

			AssertEquals("BETULA", constituentElement.ScientificDataCollection[0].US_PGAScientificGenusName);
			AssertEquals("ALLEGHANIENSIS", constituentElement.ScientificDataCollection[0].US_PGAScientificSpeciesName);
			AssertEquals("CA", constituentElement.ScientificDataCollection[0].US_PGACountryCode);
		}

		protected override IBIRDPGAScientificRecord[] GetPopulatedRecords()
		{
			var pg16 = new PGAPG16();
			pg16.SourceTypeCode = "HRV";
			pg16.CountryCode = "AU";
			return new IBIRDPGAScientificRecord[] { pg16 };
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

		protected override void PrepareData(JobDeclaration declaration, JobComInvoiceLine invoiceLine, ConstituentElement constituentElement, IBIRDPGAScientificRecord lineRecord)
		{
			base.PrepareData(declaration, invoiceLine, constituentElement, lineRecord);

			var pga = constituentElement.Parent as PGA;

			pga.PG04ConstituentElements.AddNew();
		}

		protected override Type GetTypeOfMessageBlock() => typeof(PGAPG16);

		void DeserialiseAndAddToList(List<MessageBlock> pgaBlocks, Type type, string message)
		{
			var block = (MessageBlock)Activator.CreateInstance(type);
			block.Deserialise(message);
			pgaBlocks.Add(block);
		}
	}
}
