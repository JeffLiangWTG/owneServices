using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForLaceyActTest : PGABlocksCreatorTest
	{
		public void TestLaceyActDataEnteredForLine()
		{
			/*
			 An entry of softwood lumber with only one CBP line item, the lumber is made of spruce, quantity
			 100 cubic meters, is harvested in Canada, is valued at $10,000, and is all in one container.
			(One CBP Line with HTS code for softwood lumber. This example has one only species and was 
			 harvested from one country on one line.) */

			SetUpData();
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUXXXX", true);
			PGA pGA = invoiceLine.LaceyActLines.AddNew();
			pGA = DeclarationTestHelper.CreateLaceyActData(pGA);
			pGA.US_PGALineValue = 10000.10m;

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pGA, "MAEUXXXX");

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(PGABlocksCreator.GetLaceyActBlocks(entryLine, true));

			AssertEquals("OI        SOFTWOOD PULPWOOD                                                     ", block.MessageBlocks[0].Serialise());
			AssertEquals("PG01001AP                                                                       ", block.MessageBlocks[1].Serialise());
			AssertEquals("PG04 SPRUCE                                              000000010000M3   020000", block.MessageBlocks[2].Serialise());
			AssertEquals("PG05PICEA                 GLAUCA                                                ", block.MessageBlocks[3].Serialise());
			AssertEquals("PG06HRVCA                                                                       ", block.MessageBlocks[4].Serialise());
			AssertEquals("PG25                                               000000010000                 ", block.MessageBlocks[5].Serialise());
			AssertEquals("PG27MAEUXXXX                                                                    ", block.MessageBlocks[6].Serialise());
		}

		public void TestLaceyActTest2()
		{
			/*
			 Example Two is an entry of softwood lumber with only one CBP line item and has three PGA line items.
			 Each PGA line is associated to an OI record.  
				1)	First PGA line is lumber made of pine, quantity is 100 cubic meters, is harvested in Canada, 
					is valued at $10,000, and is all in one container.
				2)  Second PGA line is lumber made of pine, quantity is 200 cubic meters, has one species and 
					is harvested in United Kingdom, France and Germany, is valued at $20,000, and is all in one container.
				3)   Third PGA line is lumber made of pine, quantity is 300 cubic meters, has three species and 
					two countries of harvest United Kingdom and France, is valued at $30,000, and is shipped in five containers.

			(Note: there are multiple PG06 records in PGA line two to include the three countries of harvest.) */

			SetUpData();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUXXXX", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUYYYYY", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUTTTT", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUHHHHHH", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUIIIIII", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUZZZZZ", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUVVVVV", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEYKKKKKK", true);

			//pga line 1
			PGA pga = CreateNewLaceyActLine(invoiceLine, "Softwood Pulpwood log length", 10000m);
			ConstituentElement constituentElement = CreateNewConstituentElement(pga, "Pine", 100m, LaceyActUnitsOfMeasureList.Codes.CubicMeters);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.Canada);
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUXXXX");

			//pga line 2
			pga = CreateNewLaceyActLine(invoiceLine, "Softwood Pulpwood 4 foot length", 20000m);
			constituentElement = CreateNewConstituentElement(pga, "Pine", 200m, LaceyActUnitsOfMeasureList.Codes.CubicMeters);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.France);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.Germany);
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUTTTT");

			//pga line 3
			pga = CreateNewLaceyActLine(invoiceLine, "Softwood Pulpwood Split", 30000m);
			constituentElement = CreateNewConstituentElement(pga, "Pine", 300m, LaceyActUnitsOfMeasureList.Codes.CubicMeters);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewScientificData(constituentElement, "Pinus", "Rigida", Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewScientificData(constituentElement, "Pinus", "Echinada", Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.France);
			CreateNewScientificData(constituentElement, "Pinus", "Rigida", Core.Constants.CountryCodes.France);
			CreateNewScientificData(constituentElement, "Pinus", "Echinada", Core.Constants.CountryCodes.France);
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUTTTT");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUHHHHHH");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUIIIIII");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUVVVVV");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEYKKKKKK");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(declaration.CustomsEntryHeaders[0], UpdateActionCode.Add, true);
			MQEDIMessage message = builder.PopulateMessage();

			AssertMultilineASCIIEquals("", @"B01    XJ5HI                                               <<MSGNO PLACEHOLDER>>
H1A    XJ5 <E#PLCH>                    01                           01          
H2                          0000010000                                          
H5001                                       0000010000                          
OI        SOFTWOOD PULPWOOD LOG LENGTH                                          
PG01001AP                                                                       
PG04 PINE                                                000000010000M3         
PG05PINUS                 TAEDA                                                 
PG06HRVCA                                                                       
PG25                                               000000010000                 
PG27MAEUXXXX                                                                    
OI        SOFTWOOD PULPWOOD 4 FOOT LENGTH                                       
PG01002AP                                                                       
PG04 PINE                                                000000020000M3         
PG05PINUS                 TAEDA                                                 
PG06HRVGB                                                                       
PG06HRVFR                                                                       
PG06HRVDE                                                                       
PG25                                               000000020000                 
PG27MAEUTTTT                                                                    
OI        SOFTWOOD PULPWOOD SPLIT                                               
PG01003AP                                                                       
PG04 PINE                                                000000030000M3         
PG05PINUS                 TAEDA                                                 
PG05PINUS                 RIGIDA                                                
PG05PINUS                 ECHINADA                                              
PG06HRVGB                                                                       
PG05PINUS                 TAEDA                                                 
PG05PINUS                 RIGIDA                                                
PG05PINUS                 ECHINADA                                              
PG06HRVFR                                                                       
PG25                                               000000030000                 
PG27MAEUTTTT            MAEUHHHHHH          MAEUIIIIII                          
PG27MAEUVVVVV           MAEYKKKKKK                                              
Y      XJ5HI00033", message.EM_FormattedMessageText);
		}

		public void TestLaceyActTest3()
		{
			/*
				An entry of corrugated paper (cardboard) with only one CBP line item, 
				the cardboard’s content is  100% of recycled paper and is valued at $300.00 and weighs 1000 kilograms. 
				No container is reported. 

				(One CBP Line with HTS code for cardboard. This example has no species and no country of harvest. 
					Note: There are no PG 05, PG06 and PG27 records. ) */

			SetUpData();

			PGA pga = CreateNewLaceyActLine(invoiceLine, "Corrugated Cardboard", 300m);
			ConstituentElement constituentElement = CreateNewConstituentElement(pga, "Recycled Cardboard", 1000m, LaceyActUnitsOfMeasureList.Codes.Kilograms);
			constituentElement.US_PGAPercentOfConstituentElement = 100m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(PGABlocksCreator.GetLaceyActBlocks(entryLine, true));

			AssertEquals("OI        CORRUGATED CARDBOARD                                                  ", block.MessageBlocks[0].Serialise());
			AssertEquals("PG01001AP                                                                       ", block.MessageBlocks[1].Serialise());
			AssertEquals("PG04 RECYCLED CARDBOARD                                  000000100000KG   100000", block.MessageBlocks[2].Serialise());
			AssertEquals("PG25                                               000000000300                 ", block.MessageBlocks[3].Serialise());
		}

		public void TestLaceyActTest4()
		{
			/*
				An entry of corrugated paper (cardboard) with only one CBP line item, the cardboard’s 
				content is  100% of recycled paper and is valued at $300.00 and weighs 1000 Kilograms. 
				Filer submits a genus and species.  No country of harvest. No container is reported. 

				(One CBP Line with HTS code for cardboard. This example has a species and no country of harvest. 
				(Species and Country are not required for 100% recycled paper and paperboard products.) 
				Note: There is a PG 05, but there are no PG06 and PG27 records. ACS will accept the PG05 
				record even though it is not required.) */

			SetUpData();

			PGA pga = CreateNewLaceyActLine(invoiceLine, "Corrugated Cardboard", 300m);
			ConstituentElement constituentElement = CreateNewConstituentElement(pga, "Recycled Cardboard", 1000m, LaceyActUnitsOfMeasureList.Codes.Kilograms);
			constituentElement.US_PGAPercentOfConstituentElement = 100m;
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", "");
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(PGABlocksCreator.GetLaceyActBlocks(entryLine, true));

			AssertEquals("OI        CORRUGATED CARDBOARD                                                  ", block.MessageBlocks[0].Serialise());
			AssertEquals("PG01001AP                                                                       ", block.MessageBlocks[1].Serialise());
			AssertEquals("PG04 RECYCLED CARDBOARD                                  000000100000KG   100000", block.MessageBlocks[2].Serialise());
			AssertEquals("PG05PINUS                 TAEDA                                                 ", block.MessageBlocks[3].Serialise());
			AssertEquals("PG25                                               000000000300                 ", block.MessageBlocks[4].Serialise());
		}

		public void TestLaceyActTest5()
		{
			/*
			 * An entry of corrugated paper (cardboard) with only one CBP line item, 
			 * the cardboard’s content is  100% of recycled paper and is valued at $300.00 and 
			 * weighs 1000 Kilograms. 
			 * Filer submits a country of harvest but no species and genus.  No container is reported. 

			(One CBP Line with HTS code for cardboard. This example has a species and no country of harvest. 
			 * (Species and Country are not required for 100% recycled paper and paperboard products.) 
			 * Note: There is a PG 06, but there are no PG05 and PG27 records. 
			 * ACS will accept the PG06 record even though it is not required.) */

			SetUpData();

			PGA pga = CreateNewLaceyActLine(invoiceLine, "Corrugated Cardboard", 300m);
			ConstituentElement constituentElement = CreateNewConstituentElement(pga, "Recycled Cardboard", 100m, LaceyActUnitsOfMeasureList.Codes.CubicMeters);
			constituentElement.US_PGAPercentOfConstituentElement = 100m;
			CreateNewScientificData(constituentElement, "", "", Core.Constants.CountryCodes.Canada);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(PGABlocksCreator.GetLaceyActBlocks(entryLine, true));

			AssertEquals("OI        CORRUGATED CARDBOARD                                                  ", block.MessageBlocks[0].Serialise());
			AssertEquals("PG01001AP                                                                       ", block.MessageBlocks[1].Serialise());
			AssertEquals("PG04 RECYCLED CARDBOARD                                  000000010000M3   100000", block.MessageBlocks[2].Serialise());
			AssertEquals("PG06HRVCA                                                                       ", block.MessageBlocks[3].Serialise());
			AssertEquals("PG25                                               000000000300                 ", block.MessageBlocks[4].Serialise());
		}

		public void TestLaceyActTest6()
		{
			/*
			 An entry of wooden desks with only one CBP line item, each desk is made of pine, oak & birch.  
			 Each type of wood has many species and but come from one country of harvest.  
			 There are 400 wooden desks made of a combination of pine, oak and birch. 
			 The value of the PGA line is $10,000, and the shipment is in two containers.

			(One CBP Line with HTS code for wooden furniture. This example has three components with many species 
			 and is harvested from one country.  There are multiple PG04 records. 
			 Since there are multiple components, PG15 and PG16 records are used. The PG15 records repeat for each PG16 record. ) */

			SetUpData();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUXXXX", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUYYYYY", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUTTTT", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUHHHHHH", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUIIIIII", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUZZZZZ", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUVVVVV", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEYKKKKKK", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUTTTTTT", true);

			PGA pga = CreateNewLaceyActLine(invoiceLine, "Wooden Desk  Pine/Oak/Birch", 10000m);
			ConstituentElement constituentElement = CreateNewConstituentElement(pga, "Pine", 40m, LaceyActUnitsOfMeasureList.Codes.Kilograms);
			CreateNewScientificData(constituentElement, "Pinus ", "Taeda", Core.Constants.CountryCodes.Canada);
			CreateNewScientificData(constituentElement, "Pinus ", "Rigida", Core.Constants.CountryCodes.Canada);
			CreateNewScientificData(constituentElement, "Pinus ", "Echinda", Core.Constants.CountryCodes.Canada);

			constituentElement = CreateNewConstituentElement(pga, "Oak", 40m, LaceyActUnitsOfMeasureList.Codes.Kilograms);
			CreateNewScientificData(constituentElement, "Quercus", "Rubra", Core.Constants.CountryCodes.Canada);
			CreateNewScientificData(constituentElement, "Quercus", "Alba", Core.Constants.CountryCodes.Canada);
			CreateNewScientificData(constituentElement, "Quercus", "Ellipsoidalis", Core.Constants.CountryCodes.Canada);

			constituentElement = CreateNewConstituentElement(pga, "Birch", 40m, LaceyActUnitsOfMeasureList.Codes.Kilograms);
			CreateNewScientificData(constituentElement, "Betula", "Alleghaniensis", Core.Constants.CountryCodes.Canada);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUXXXX");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUTTTTTT");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(PGABlocksCreator.GetLaceyActBlocks(entryLine, true));

			AssertEquals("OI        WOODEN DESK  PINE/OAK/BIRCH                                           ", block.MessageBlocks[0].Serialise());
			AssertEquals("PG01001AP                                                                       ", block.MessageBlocks[1].Serialise());
			AssertEquals("PG04 PINE                                                000000004000KG         ", block.MessageBlocks[2].Serialise());
			AssertEquals("PG15PINUS                 TAEDA                                                 ", block.MessageBlocks[3].Serialise());
			AssertEquals("PG15PINUS                 RIGIDA                                                ", block.MessageBlocks[4].Serialise());
			AssertEquals("PG15PINUS                 ECHINDA                                               ", block.MessageBlocks[5].Serialise());
			AssertEquals("PG16HRVCA                                                                       ", block.MessageBlocks[6].Serialise());
			AssertEquals("PG04 OAK                                                 000000004000KG         ", block.MessageBlocks[7].Serialise());
			AssertEquals("PG15QUERCUS               RUBRA                                                 ", block.MessageBlocks[8].Serialise());
			AssertEquals("PG15QUERCUS               ALBA                                                  ", block.MessageBlocks[9].Serialise());
			AssertEquals("PG15QUERCUS               ELLIPSOIDALIS                                         ", block.MessageBlocks[10].Serialise());
			AssertEquals("PG16HRVCA                                                                       ", block.MessageBlocks[11].Serialise());
			AssertEquals("PG04 BIRCH                                               000000004000KG         ", block.MessageBlocks[12].Serialise());
			AssertEquals("PG15BETULA                ALLEGHANIENSIS                                        ", block.MessageBlocks[13].Serialise());
			AssertEquals("PG16HRVCA                                                                       ", block.MessageBlocks[14].Serialise());
			AssertEquals("PG25                                               000000010000                 ", block.MessageBlocks[15].Serialise());
			AssertEquals("PG27MAEUXXXX            MAEUTTTTTT                                              ", block.MessageBlocks[16].Serialise());
		}

		public void TestLaceyActTest7()
		{
			/*
			 An entry of wooden desks that has three CBP lines: 
				1) CBP Line one is 1000 wooden desks made of oak, with two possible species harvested 
					in United Kingdom. Value $15,000 is located in four containers.

				Note: There are two PG05 records for multiple species. There are two PG27 for more than three containers.

				2) CBP Line two is 400 wooden desks made of two components birch and pine, each type of 
					wood has one species but  possibly harvested in two countries France and Germany, 
					Value of line item is $20,000 an is located in one container.

				Note: There are two PG04 records to identify multiple components. With both PG04 records 
				there is one PG 15 record and two PG16 records (multiple countries).  
				PG15 & PG16 records are used because there are multiple reportable components.

			3) CBP Line three is 500 teak wooden desks. The teak has one species and could be harvested
				in three countries. Value of line is $ 25,000 and is located in 7 containers. */

			SetUpData();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUXXXX", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUYYYYY", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUWWWW", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUAAAAAAA", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAUECCCCC", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUTTTTT", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUHHHH", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUIIIII", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUJJJJ", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUBBBB", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUZZZZZ", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUVVVVV", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEYKKKKKK", true);

			PGA pga = CreateNewLaceyActLine(invoiceLine, "Wooden Desk - Oak", 15000m);
			ConstituentElement constituentElement = CreateNewConstituentElement(pga, "OAK", 10m, LaceyActUnitsOfMeasureList.Codes.Meter);
			CreateNewScientificData(constituentElement, "Quercus", "Rubra", Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewScientificData(constituentElement, "Quercus", "Alba", Core.Constants.CountryCodes.UnitedKingdom);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUXXXX");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUYYYYY");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUWWWW");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUAAAAAAA");

			pga = CreateNewLaceyActLine(invoiceLine, "Wooden Desk - Birch & Pine", 20000m);
			constituentElement = CreateNewConstituentElement(pga, "BIRCH", 4m, LaceyActUnitsOfMeasureList.Codes.Kilograms);
			CreateNewScientificData(constituentElement, "Betula", "Alleghaniensis", Core.Constants.CountryCodes.France);
			CreateNewScientificData(constituentElement, "Betula", "Alleghaniensis", Core.Constants.CountryCodes.Germany);

			constituentElement = CreateNewConstituentElement(pga, "PINE", 4m, LaceyActUnitsOfMeasureList.Codes.Kilograms);
			CreateNewScientificData(constituentElement, "PINUS", "Taeda", Core.Constants.CountryCodes.France);
			CreateNewScientificData(constituentElement, "PINUS", "Taeda", Core.Constants.CountryCodes.Germany);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUXXXX");

			pga = CreateNewLaceyActLine(invoiceLine, "Wooden Desk - TEAK", 25000m);
			constituentElement = CreateNewConstituentElement(pga, "Teak", 5m, LaceyActUnitsOfMeasureList.Codes.Kilograms);
			CreateNewScientificData(constituentElement, "Tectona", "Grandis", Core.Constants.CountryCodes.Philippines);
			CreateNewScientificData(constituentElement, "Tectona", "Grandis", Core.Constants.CountryCodes.Thailand);
			CreateNewScientificData(constituentElement, "Tectona", "Grandis", Core.Constants.CountryCodes.China);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUXXXX");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAUECCCCC");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUTTTTT");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUHHHH");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUIIIII");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUJJJJ");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUBBBB");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(PGABlocksCreator.GetLaceyActBlocks(entryLine, true));

			AssertEquals("OI        WOODEN DESK - OAK                                                     ", block.MessageBlocks[0].Serialise());
			AssertEquals("PG01001AP                                                                       ", block.MessageBlocks[1].Serialise());
			AssertEquals("PG04 OAK                                                 000000001000M          ", block.MessageBlocks[2].Serialise());
			AssertEquals("PG05QUERCUS               RUBRA                                                 ", block.MessageBlocks[3].Serialise());
			AssertEquals("PG05QUERCUS               ALBA                                                  ", block.MessageBlocks[4].Serialise());
			AssertEquals("PG06HRVGB                                                                       ", block.MessageBlocks[5].Serialise());
			AssertEquals("PG25                                               000000015000                 ", block.MessageBlocks[6].Serialise());
			AssertEquals("PG27MAEUXXXX            MAEUYYYYY           MAEUWWWW                            ", block.MessageBlocks[7].Serialise());
			AssertEquals("PG27MAEUAAAAAAA                                                                 ", block.MessageBlocks[8].Serialise());
			AssertEquals("OI        WOODEN DESK - BIRCH & PINE                                            ", block.MessageBlocks[9].Serialise());
			AssertEquals("PG01002AP                                                                       ", block.MessageBlocks[10].Serialise());
			AssertEquals("PG04 BIRCH                                               000000000400KG         ", block.MessageBlocks[11].Serialise());
			AssertEquals("PG15BETULA                ALLEGHANIENSIS                                        ", block.MessageBlocks[12].Serialise());
			AssertEquals("PG16HRVFR                                                                       ", block.MessageBlocks[13].Serialise());
			AssertEquals("PG16HRVDE                                                                       ", block.MessageBlocks[14].Serialise());
			AssertEquals("PG04 PINE                                                000000000400KG         ", block.MessageBlocks[15].Serialise());
			AssertEquals("PG15PINUS                 TAEDA                                                 ", block.MessageBlocks[16].Serialise());
			AssertEquals("PG16HRVFR                                                                       ", block.MessageBlocks[17].Serialise());
			AssertEquals("PG16HRVDE                                                                       ", block.MessageBlocks[18].Serialise());
			AssertEquals("PG25                                               000000020000                 ", block.MessageBlocks[19].Serialise());
			AssertEquals("PG27MAEUXXXX                                                                    ", block.MessageBlocks[20].Serialise());
			AssertEquals("OI        WOODEN DESK - TEAK                                                    ", block.MessageBlocks[21].Serialise());
			AssertEquals("PG01003AP                                                                       ", block.MessageBlocks[22].Serialise());
			AssertEquals("PG04 TEAK                                                000000000500KG         ", block.MessageBlocks[23].Serialise());
			AssertEquals("PG05TECTONA               GRANDIS                                               ", block.MessageBlocks[24].Serialise());
			AssertEquals("PG06HRVTH                                                                       ", block.MessageBlocks[25].Serialise());
			AssertEquals("PG06HRVPH                                                                       ", block.MessageBlocks[26].Serialise());
			AssertEquals("PG06HRVCN                                                                       ", block.MessageBlocks[27].Serialise());
			AssertEquals("PG25                                               000000025000                 ", block.MessageBlocks[28].Serialise());
			AssertEquals("PG27MAEUXXXX            MAUECCCCC           MAEUTTTTT                           ", block.MessageBlocks[29].Serialise());
			AssertEquals("PG27MAEUHHHH            MAEUIIIII           MAEUJJJJ                            ", block.MessageBlocks[30].Serialise());
			AssertEquals("PG27MAEUBBBB                                                                    ", block.MessageBlocks[31].Serialise());
		}

		public void TestLaceyActTest8()
		{
			/*
			 An entry of corrugated cardboard with only one CBP line item, each box is made of 50% 
			 cardboard and 50% recycled cardboard. The shipment weighs 1000 kilograms. 
			 The value of the PGA line is $10,000, and the shipment is in two containers.

			(Note:  The 50% recycled cardboard has a percentage -no species or country of harvest. 
			 The non-recycled cardboard does not have a percentage: it does have species and country of harvest) */

			SetUpData();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUXXXX", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUYYYYY", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUWWWW", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUAAAAAAA", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAUECCCCC", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUTTTTTT", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUHHHHHH", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUIIIIII", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUJJJJ", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUBBBB", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUZZZZZ", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUVVVVV", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEYKKKKKK", true);

			PGA pga = CreateNewLaceyActLine(invoiceLine, "Corrugated Cardboard Boxes", 10000m);
			ConstituentElement constituentElement = CreateNewConstituentElement(pga, "Recycled Cardboard", 500m, LaceyActUnitsOfMeasureList.Codes.Kilograms);
			constituentElement.US_PGAPercentOfConstituentElement = 50m;

			constituentElement = CreateNewConstituentElement(pga, "Cardboard", 500m, LaceyActUnitsOfMeasureList.Codes.Kilograms);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.Canada);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUXXXX");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUTTTTTT");
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(PGABlocksCreator.GetLaceyActBlocks(entryLine, true));

			AssertEquals("OI        CORRUGATED CARDBOARD BOXES                                            ", block.MessageBlocks[0].Serialise());
			AssertEquals("PG01001AP                                                                       ", block.MessageBlocks[1].Serialise());
			AssertEquals("PG04 RECYCLED CARDBOARD                                  000000050000KG   050000", block.MessageBlocks[2].Serialise());
			AssertEquals("PG04 CARDBOARD                                           000000050000KG         ", block.MessageBlocks[3].Serialise());
			AssertEquals("PG15PINUS                 TAEDA                                                 ", block.MessageBlocks[4].Serialise());
			AssertEquals("PG16HRVCA                                                                       ", block.MessageBlocks[5].Serialise());
			AssertEquals("PG25                                               000000010000                 ", block.MessageBlocks[6].Serialise());
			AssertEquals("PG27MAEUXXXX            MAEUTTTTTT                                              ", block.MessageBlocks[7].Serialise());
		}

		public void TestLaceyActTest9()
		{
			/*
			 An entry of wooden frames with of 100% recycled paperboard. The entry had only one CBP line item. 
			 The wood frames is made of pine, there are 1000 frames with one species and country 
			 of harvest with recycled paperboard for backing.  The value of the PGA line is $10,000, 
			 and the shipment is in two containers. */

			SetUpData();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUXXXX", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUYYYYY", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUTTTTTT", true);

			PGA pga = CreateNewLaceyActLine(invoiceLine, "Wooden Frames", 10000m);
			ConstituentElement constituentElement = CreateNewConstituentElement(pga, "Pine", 10m, LaceyActUnitsOfMeasureList.Codes.Meter);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.Canada);

			constituentElement = CreateNewConstituentElement(pga, "Recycled Paperboard", 1m, LaceyActUnitsOfMeasureList.Codes.Kilograms);
			constituentElement.US_PGAPercentOfConstituentElement = 100m;

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUXXXX");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUTTTTTT");
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(PGABlocksCreator.GetLaceyActBlocks(entryLine, true));

			AssertEquals("OI        WOODEN FRAMES                                                         ", block.MessageBlocks[0].Serialise());
			AssertEquals("PG01001AP                                                                       ", block.MessageBlocks[1].Serialise());
			AssertEquals("PG04 PINE                                                000000001000M          ", block.MessageBlocks[2].Serialise());
			AssertEquals("PG15PINUS                 TAEDA                                                 ", block.MessageBlocks[3].Serialise());
			AssertEquals("PG16HRVCA                                                                       ", block.MessageBlocks[4].Serialise());
			AssertEquals("PG04 RECYCLED PAPERBOARD                                 000000000100KG   100000", block.MessageBlocks[5].Serialise());
			AssertEquals("PG25                                               000000010000                 ", block.MessageBlocks[6].Serialise());
			AssertEquals("PG27MAEUXXXX            MAEUTTTTTT                                              ", block.MessageBlocks[7].Serialise());
		}

		public void TestLaceyActTest10()
		{
			/*
			Example ten is an entry of softwood lumber with only one CBP line item and has three 
			 PGA line items.  All three PGA lines are under the same OI record (same commercial description).  
				1)	First PGA line is lumber made of pine, quantity is 100 cubic meters, is 
					harvested in Canada, is valued at $10,000, and is all in one container.
				2)    Second PGA line is lumber made of pine, quantity is 200 cubic meters, is harvested in
					Canada, is valued at $20,000, and is all in one container.
				3)   Third PGA line is lumber made of pine, quantity is 300 cubic meters, 
					is harvested in Canada, is valued at $30,000, and is shipped in five containers.

				(Note: In the CATAIR on page OGA-13 - Record Identifier OI, CBP has allowed for the situation 
				where an OI record is not needed before each PG01 record if the same commercial description 
				is used. Currently, CBP receives entries with multiple FD01 records under the same
				OI record that have the same commercial description. If a different Government 
			 Application is needed for the same commercial description then separate OI record would be needed) */

			SetUpData();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUXXXX", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUYYYYY", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUWWWW", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUAAAAAAA", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAUECCCCC", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUTTTT", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUHHHHHH", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUIIIIIIIII", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUJJJJ", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUBBBB", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUZZZZZ", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUVVVVV", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUKKKKKK", true);

			PGA pga = CreateNewLaceyActLine(invoiceLine, "Softwood Pulpwood", 10000m);
			ConstituentElement constituentElement = CreateNewConstituentElement(pga, "Pine", 100m, LaceyActUnitsOfMeasureList.Codes.CubicMeters);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.Canada);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUXXXX");

			pga = CreateNewLaceyActLine(invoiceLine, "", 20000m);
			constituentElement = CreateNewConstituentElement(pga, "Pine", 200m, LaceyActUnitsOfMeasureList.Codes.CubicMeters);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.Canada);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUTTTT");

			pga = CreateNewLaceyActLine(invoiceLine, "", 30000m);
			constituentElement = CreateNewConstituentElement(pga, "Pine", 300m, LaceyActUnitsOfMeasureList.Codes.CubicMeters);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.Canada);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUTTTT");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUHHHHHH");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUIIIIIIIII");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUVVVVV");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUKKKKKK");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(PGABlocksCreator.GetLaceyActBlocks(entryLine, true));

			AssertEquals("OI        SOFTWOOD PULPWOOD                                                     ", block.MessageBlocks[0].Serialise());
			AssertEquals("PG01001AP                                                                       ", block.MessageBlocks[1].Serialise());
			AssertEquals("PG04 PINE                                                000000010000M3         ", block.MessageBlocks[2].Serialise());
			AssertEquals("PG05PINUS                 TAEDA                                                 ", block.MessageBlocks[3].Serialise());
			AssertEquals("PG06HRVCA                                                                       ", block.MessageBlocks[4].Serialise());
			AssertEquals("PG25                                               000000010000                 ", block.MessageBlocks[5].Serialise());
			AssertEquals("PG27MAEUXXXX                                                                    ", block.MessageBlocks[6].Serialise());
			AssertEquals("PG01002AP                                                                       ", block.MessageBlocks[7].Serialise());
			AssertEquals("PG04 PINE                                                000000020000M3         ", block.MessageBlocks[8].Serialise());
			AssertEquals("PG05PINUS                 TAEDA                                                 ", block.MessageBlocks[9].Serialise());
			AssertEquals("PG06HRVCA                                                                       ", block.MessageBlocks[10].Serialise());
			AssertEquals("PG25                                               000000020000                 ", block.MessageBlocks[11].Serialise());
			AssertEquals("PG27MAEUTTTT                                                                    ", block.MessageBlocks[12].Serialise());
			AssertEquals("PG01003AP                                                                       ", block.MessageBlocks[13].Serialise());
			AssertEquals("PG04 PINE                                                000000030000M3         ", block.MessageBlocks[14].Serialise());
			AssertEquals("PG05PINUS                 TAEDA                                                 ", block.MessageBlocks[15].Serialise());
			AssertEquals("PG06HRVCA                                                                       ", block.MessageBlocks[16].Serialise());
			AssertEquals("PG25                                               000000030000                 ", block.MessageBlocks[17].Serialise());
			AssertEquals("PG27MAEUTTTT            MAEUHHHHHH          MAEUIIIIIIIII                       ", block.MessageBlocks[18].Serialise());
			AssertEquals("PG27MAEUVVVVV           MAEUKKKKKK                                              ", block.MessageBlocks[19].Serialise());
		}

		public void TestLaceyActTest11()
		{
			/*
			Example Eleven is an entry of softwood lumber with only one CBP line item and has three PGA line items. 
			 All three PGA lines are under the same OI record (same commercial description).  
			1)	First PGA line is lumber made of pine, quantity is 100 cubic meters, is harvested in Canada, 
				is valued at $10,000, and is all in one container.
			2)    Second PGA line is lumber made of pine, quantity is 200 cubic meters, has three species and 
				is harvested in Canada, is valued at $20,000, and is all in one container.
			3)   Third PGA line is lumber made of pine, quantity is 300 cubic meters, has three species and
				two countries of harvest United Kingdom and France, is valued at $30,000, and is shipped in five containers.

			(Note: there are multiple PG05 records in PGA line two, multiple PG05 and PG06 records in PGA 
			 line three to identify the three species associated to two countries.) */

			SetUpData();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUXXXX", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUYYYYY", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUWWWW", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUAAAAAAA", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAUECCCCC", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUTTTT", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUHHHHHH", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUIIIIIIIII", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUJJJJ", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUBBBB", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUZZZZZ", false);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUVVVVV", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUKKKKKK", true);

			PGA pga = CreateNewLaceyActLine(invoiceLine, "Softwood Pulpwood", 10000m);
			ConstituentElement constituentElement = CreateNewConstituentElement(pga, "Pine", 100m, LaceyActUnitsOfMeasureList.Codes.CubicMeters);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.Canada);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUXXXX");

			pga = CreateNewLaceyActLine(invoiceLine, "", 20000m);
			constituentElement = CreateNewConstituentElement(pga, "Pine", 200m, LaceyActUnitsOfMeasureList.Codes.CubicMeters);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.Canada);
			CreateNewScientificData(constituentElement, "Pinus", "Rigida", Core.Constants.CountryCodes.Canada);
			CreateNewScientificData(constituentElement, "Pinus", "Echinada", Core.Constants.CountryCodes.Canada);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUTTTT");

			pga = CreateNewLaceyActLine(invoiceLine, "", 30000m);
			constituentElement = CreateNewConstituentElement(pga, "Pine", 300m, LaceyActUnitsOfMeasureList.Codes.CubicMeters);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewScientificData(constituentElement, "Pinus", "Rigida", Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewScientificData(constituentElement, "Pinus", "Echinada", Core.Constants.CountryCodes.UnitedKingdom);

			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.France);
			CreateNewScientificData(constituentElement, "Pinus", "Rigida", Core.Constants.CountryCodes.France);
			CreateNewScientificData(constituentElement, "Pinus", "Echinada", Core.Constants.CountryCodes.France);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUTTTT");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUHHHHHH");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUIIIIIIIII");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUVVVVV");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUKKKKKK");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(PGABlocksCreator.GetLaceyActBlocks(entryLine, true));

			AssertEquals("OI        SOFTWOOD PULPWOOD                                                     ", block.MessageBlocks[0].Serialise());
			AssertEquals("PG01001AP                                                                       ", block.MessageBlocks[1].Serialise());
			AssertEquals("PG04 PINE                                                000000010000M3         ", block.MessageBlocks[2].Serialise());
			AssertEquals("PG05PINUS                 TAEDA                                                 ", block.MessageBlocks[3].Serialise());
			AssertEquals("PG06HRVCA                                                                       ", block.MessageBlocks[4].Serialise());
			AssertEquals("PG25                                               000000010000                 ", block.MessageBlocks[5].Serialise());
			AssertEquals("PG27MAEUXXXX                                                                    ", block.MessageBlocks[6].Serialise());
			AssertEquals("PG01002AP                                                                       ", block.MessageBlocks[7].Serialise());
			AssertEquals("PG04 PINE                                                000000020000M3         ", block.MessageBlocks[8].Serialise());
			AssertEquals("PG05PINUS                 TAEDA                                                 ", block.MessageBlocks[9].Serialise());
			AssertEquals("PG05PINUS                 RIGIDA                                                ", block.MessageBlocks[10].Serialise());
			AssertEquals("PG05PINUS                 ECHINADA                                              ", block.MessageBlocks[11].Serialise());
			AssertEquals("PG06HRVCA                                                                       ", block.MessageBlocks[12].Serialise());
			AssertEquals("PG25                                               000000020000                 ", block.MessageBlocks[13].Serialise());
			AssertEquals("PG27MAEUTTTT                                                                    ", block.MessageBlocks[14].Serialise());
			AssertEquals("PG01003AP                                                                       ", block.MessageBlocks[15].Serialise());
			AssertEquals("PG04 PINE                                                000000030000M3         ", block.MessageBlocks[16].Serialise());
			AssertEquals("PG05PINUS                 TAEDA                                                 ", block.MessageBlocks[17].Serialise());
			AssertEquals("PG05PINUS                 RIGIDA                                                ", block.MessageBlocks[18].Serialise());
			AssertEquals("PG05PINUS                 ECHINADA                                              ", block.MessageBlocks[19].Serialise());
			AssertEquals("PG06HRVGB                                                                       ", block.MessageBlocks[20].Serialise());
			AssertEquals("PG05PINUS                 TAEDA                                                 ", block.MessageBlocks[21].Serialise());
			AssertEquals("PG05PINUS                 RIGIDA                                                ", block.MessageBlocks[22].Serialise());
			AssertEquals("PG05PINUS                 ECHINADA                                              ", block.MessageBlocks[23].Serialise());
			AssertEquals("PG06HRVFR                                                                       ", block.MessageBlocks[24].Serialise());
			AssertEquals("PG25                                               000000030000                 ", block.MessageBlocks[25].Serialise());
			AssertEquals("PG27MAEUTTTT            MAEUHHHHHH          MAEUIIIIIIIII                       ", block.MessageBlocks[26].Serialise());
			AssertEquals("PG27MAEUVVVVV           MAEUKKKKKK                                              ", block.MessageBlocks[27].Serialise());
		}

		public void TestLaceyActTest12()
		{
			/*
			Example Twelve is an entry of softwood lumber with only one CBP line item and has three PGA line items. 
			All three PGA lines are under the same OI record (same commercial description).  
			1)	First PGA line is lumber made of pine, quantity is 100 cubic meters, is harvested in Canada, 
				is valued at $10,000, and is all in one container.
			2)    Second PGA line is lumber made of pine, quantity is 200 cubic meters, has one species and 
				is harvested in United Kingdom, France and Germany, is valued at $20,000, and is all in one container.
			3)   Third PGA line is lumber made of pine, quantity is 300 cubic meters three species and two countries 
				of harvest United Kingdom and France, is valued at $30,000, and is shipped in five containers.

			(Note: there are multiple PG06 records in PGA line two to include the three countries of harvest.) */

			SetUpData();
			declaration.ActiveEntryHeaders.RemoveAndDeleteAll();

			declaration.JE_MergeBy = "TRF";
			invoiceLine.JI_Tariff = "4401100000";

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUXXXX", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUTTTT", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUHHHHHH", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUIIIIIIIII", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUVVVVV", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "MAEUKKKKKK", true);

			PGA pga = CreateNewLaceyActLine(invoiceLine, "Softwood Pulpwood", 10000m);
			ConstituentElement constituentElement = CreateNewConstituentElement(pga, "Pine", 100m, LaceyActUnitsOfMeasureList.Codes.CubicMeters);
			CreateNewScientificData(constituentElement, "Pinus", "Echinada", Core.Constants.CountryCodes.Canada);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUXXXX");

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "4401100000";
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine2, "MAEUTTTT", true);
			pga = CreateNewLaceyActLine(invoiceLine2, "", 20000m);
			constituentElement = CreateNewConstituentElement(pga, "Pine", 200m, LaceyActUnitsOfMeasureList.Codes.CubicMeters);
			CreateNewScientificData(constituentElement, "Pinus", "Echinada", Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewScientificData(constituentElement, "Pinus", "Echinada", Core.Constants.CountryCodes.France);
			CreateNewScientificData(constituentElement, "Pinus", "Echinada", Core.Constants.CountryCodes.Germany);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUTTTT");

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "4401100000";
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine3, "MAEUTTTT", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine3, "MAEUHHHHHH", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine3, "MAEUIIIIIIIII", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine3, "MAEUVVVVV", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine3, "MAEUKKKKKK", true);

			pga = CreateNewLaceyActLine(invoiceLine3, "", 30000m);
			constituentElement = CreateNewConstituentElement(pga, "Pine", 300m, LaceyActUnitsOfMeasureList.Codes.CubicMeters);
			CreateNewScientificData(constituentElement, "Pinus", "Echinada", Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewScientificData(constituentElement, "Pinus", "Rigida", Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewScientificData(constituentElement, "Pinus", "Echinada", Core.Constants.CountryCodes.France);
			CreateNewScientificData(constituentElement, "Pinus", "Taeda", Core.Constants.CountryCodes.France);
			CreateNewScientificData(constituentElement, "Pinus", "Rigida", Core.Constants.CountryCodes.France);

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUTTTT");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUHHHHHH");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUIIIIIIIII");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUVVVVV");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(pga, "MAEUKKKKKK");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(declaration.CustomsEntryHeaders[0], UpdateActionCode.Add, true);
			MQEDIMessage message = builder.PopulateMessage();

			AssertMultilineASCIIEquals("", @"B01    XJ5HI                                               <<MSGNO PLACEHOLDER>>
H1A    XJ5 <E#PLCH>                    01                           01          
H2                          0000010000                                          
H5001  4401100000                           0000010000                          
OI        SOFTWOOD PULPWOOD                                                     
PG01001AP                                                                       
PG04 PINE                                                000000010000M3         
PG05PINUS                 ECHINADA                                              
PG06HRVCA                                                                       
PG25                                               000000010000                 
PG27MAEUXXXX                                                                    
H5002  4401100000                                                               
PG01001AP                                                                       
PG04 PINE                                                000000020000M3         
PG05PINUS                 ECHINADA                                              
PG06HRVGB                                                                       
PG06HRVFR                                                                       
PG06HRVDE                                                                       
PG25                                               000000020000                 
PG27MAEUTTTT                                                                    
H5003  4401100000                                                               
PG01001AP                                                                       
PG04 PINE                                                000000030000M3         
PG05PINUS                 ECHINADA                                              
PG05PINUS                 TAEDA                                                 
PG05PINUS                 RIGIDA                                                
PG06HRVGB                                                                       
PG05PINUS                 ECHINADA                                              
PG05PINUS                 TAEDA                                                 
PG05PINUS                 RIGIDA                                                
PG06HRVFR                                                                       
PG25                                               000000030000                 
PG27MAEUTTTT            MAEUHHHHHH          MAEUIIIIIIIII                       
PG27MAEUVVVVV           MAEUKKKKKK                                              
Y      XJ5HI00033", message.EM_FormattedMessageText);
		}

		public void TestCertifyingDateIsSetToCurrentDateIfEmpty()
		{
			SetUpData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "ACE";
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = "SE";
			declaration.US_EntryFilerCode = "XJ5";

			PGA pga = CreateNewLaceyActLine(invoiceLine, "Softwood Pulpwood", 10000m);
			invoiceLine.InvoiceHeader.US_LACEYACTSignDate = ZDateTime.Empty;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var today = ZDate.Today;
			AssertContains("PG22             IM AP6 Y" + today.ToString("MMddyyyy"), message.EM_FormattedMessageText);

			invoiceLine.InvoiceHeader.US_LACEYACTSignDate = new ZDateTime(2016, 11, 29);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			action = GetAction(declaration);
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			AssertContains("PG22             IM AP6 Y11292016", message.EM_FormattedMessageText);
		}

		protected override void SetUpData()
		{
			base.SetUpData();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
		}

		PGA CreateNewLaceyActLine(JobComInvoiceLine invoiceLine, ZString commercialDescription, ZDecimal lineValue)
		{
			PGA result = invoiceLine.LaceyActLines.AddNew();

			result.US_PGACommercialDescription = commercialDescription;
			result.US_InvCurrPGAValue = lineValue;

			return result;
		}

		ConstituentElement CreateNewConstituentElement(PGA pga, ZString name, ZDecimal quantity, ZString uQ)
		{
			ConstituentElement result = pga.PG04ConstituentElements.AddNew();
			result.US_PGANameOfTheConstituentElement = name;
			result.US_PGAQuantityOfConstituentElement = quantity;
			result.US_PGAUnitOfMeasure = uQ;

			return result;
		}

		ScientificData CreateNewScientificData(ConstituentElement constituentElement, ZString genusName, ZString speciesName, ZString country)
		{
			ScientificData result = constituentElement.ScientificDataCollection.AddNew();

			result.US_PGAScientificGenusName = genusName;
			result.US_PGAScientificSpeciesName = speciesName;
			result.US_PGACountryCode = country;

			return result;
		}
	}
}
