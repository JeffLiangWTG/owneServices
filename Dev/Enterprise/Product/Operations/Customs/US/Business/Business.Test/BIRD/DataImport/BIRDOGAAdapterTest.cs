using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BIRDOGAImporterTest : TestCaseWithFactory
	{
		public void TestProcessDOT()
		{
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 7002195701891  AL 20     23                  113901082509B00152514            56   082509J593     22            HIWER890    387000796006            00000001PK         APLUAPLU   30                                  01              2090309             APLU    40001AU00000600000000000000                    000000250060267                  50 8703240054          000000000300NO                               AU082509NAU OI        O VH,>4<=6CYL,IN VL>2.8<=3                                            DT010012AY                          V                                           DT02HYUNDAI        GETZ           2009IEWR87EWRKJWER87                          DT010022AY                          V                                           DT02HYUNDAI        GETZ           2008YOUIES70EWHJKDF07                         OI        O VH,>4<=6CYL,IN VL>2.8<=3                                            DT010032AY                          TABCABCD                                    51                                                                              60                                        ZABHPBIL1001RAN                       62          50100007500                                                         8950100000007500                                                                90                      0                       0000000750000000060000          ZZ7501000000010                                                                 ";
			//B018888XJ5EI                                               35110                
			//10A888891-01319900091-013199000                 8         XJ5 7002195701891  AL 
			//20     23                  113901082509B00152514            56   082509J593     
			//22            HIWER890    387000796006            00000001PK         APLUAPLU   
			//30                                  01              2090309             APLU    
			//40001AU00000600000000000000                    000000250060267                  
			//50 8703240054          000000000300NO                               AU082509NAU 
			//OI        O VH,>4<=6CYL,IN VL>2.8<=3                                            
			//DT010012AY                          V                                           
			//DT02HYUNDAI        GETZ           2009IEWR87EWRKJWER87                          
			//DT010022AY                          V                                           
			//DT02HYUNDAI        GETZ           2008YOUIES70EWHJKDF07                         
			//OI        O VH,>4<=6CYL,IN VL>2.8<=3                                            
			//DT010032AY                          TABCABCD                                    
			//51                                                                              
			//60                                        ZABHPBIL1001RAN                       
			//62          50100007500                                                         
			//8950100000007500                                                                
			//90                      0                       0000000750000000060000          
			//Y  8888XJ5EI00018

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(1, declaration.InvoiceLines.Count);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];

			AssertEquals("three DOT lines", 3, invoiceLine.DOTs.Count);

			DOT dot = invoiceLine.DOTs[0];

			AssertEquals("Box number", "2A", dot.US_DOTBoxNo);
			AssertEquals("Commercial Desc", "O VH,>4<=6CYL,IN VL>2.8<=3", dot.US_DOTCommercialDesc);
			AssertEquals("Clarification", "V", dot.US_DOTClarCode);
			AssertEquals("One vehicle detail", 1, dot.DOTVINs.Count);
			AssertEquals("Year", 2009, dot.DOTVINs[0].US_DOTYear);
			AssertEquals("VIN", "IEWR87EWRKJWER87", dot.DOTVINs[0].US_DOTVIN);

			dot = invoiceLine.DOTs[1];
			AssertEquals("Box number", "2A", dot.US_DOTBoxNo);
			AssertEquals("Commercial Desc", "O VH,>4<=6CYL,IN VL>2.8<=3", dot.US_DOTCommercialDesc);
			AssertEquals("Clarification", "V", dot.US_DOTClarCode);
			AssertEquals("One vehicle detail", 1, dot.DOTVINs.Count);
			AssertEquals("Year", 2008, dot.DOTVINs[0].US_DOTYear);
			AssertEquals("VIN", "YOUIES70EWHJKDF07", dot.DOTVINs[0].US_DOTVIN);

			dot = invoiceLine.DOTs[2];
			AssertEquals("Box number", "2A", dot.US_DOTBoxNo);
			AssertEquals("Commercial Desc", "O VH,>4<=6CYL,IN VL>2.8<=3", dot.US_DOTCommercialDesc);
			AssertEquals("Clarification", "T", dot.US_DOTClarCode);
			AssertEquals("No vehicle details", 0, dot.DOTVINs.Count);
			AssertEquals("Tire ID", "ABC", dot.US_DOTTireID);
			AssertEquals("Tire Brand Name", "ABCD", dot.US_DOTTireBrandName);
		}

		public void TestProcessFCC()
		{
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 7002194001891  AK 20     23                  102809082509B00152513            56   082509H572     22            IF7JISDF                            00000001PK         APLU       30                                  01              2090409             APLU    40001AU00000100000000000000                    000000005060267                  50 9027205050          000000520000NO                               AU082509YAU OA  FD0                                                                         OI        DESC1                                                                 FC0102 001                 ABC                           J90394-1               FC02000000000050                                                                OI        2                                                                     FC0102 002                 DEF                           J90394-2               FC02000000000020                                                                51                                                                              60                                        ZABHPBIL1001RAN                       62          50100001250                                                         8950100000001250                                                                90                      0                       0000000125000000010000          ZZ7501000000010                                                                 ";
			//B018888XJ5EI                                               35101                
			//10A888891-01319900091-013199000                 8         XJ5 7002194001891  AK 
			//20     23                  102809082509B00152513            56   082509H572     
			//22            IF7JISDF                            00000001PK         APLU       
			//30                                  01              2090409             APLU    
			//40001AU00000100000000000000                    000000005060267                  
			//50 9027205050          000000520000NO                               AU082509YAU 
			//OA  FD0                                                                         
			//OI        DESC1                                                                 
			//FC0102 001                 ABC                           J90394-1               
			//FC02000000000050                                                                
			//OI        2                                                                     
			//FC0102 002                 DEF                           J90394-2               
			//FC02000000000020                                                                
			//51                                                                              
			//60                                        ZABHPBIL1001RAN                       
			//62          50100001250                                                         
			//8950100000001250                                                                
			//90                      0                       0000000125000000010000          
			//Y  8888XJ5EI00018    

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(1, declaration.InvoiceLines.Count);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];

			FCC fcc = invoiceLine.FCCs[0];
			AssertEquals("Line number", 1, fcc.US_FCCLineNo);
			AssertEquals("Commercial Desc", "DESC1", fcc.US_FCCCommercialDesc);
			AssertEquals("Import Condition", "02", fcc.US_FCCImpCondNo);
			AssertEquals("model", "J90394-1", fcc.US_FCCModel);
			AssertEquals("Quantity", 50m, fcc.US_FCCQty);
			AssertEquals("Trade Name", "ABC", fcc.US_FCCTradeName);

			fcc = invoiceLine.FCCs[1];
			AssertEquals("Line number", 2, fcc.US_FCCLineNo);
			AssertEquals("Commercial Desc", "2", fcc.US_FCCCommercialDesc);
			AssertEquals("Import Condition", "02", fcc.US_FCCImpCondNo);
			AssertEquals("model", "J90394-2", fcc.US_FCCModel);
			AssertEquals("Quantity", 20m, fcc.US_FCCQty);
			AssertEquals("Trade Name", "DEF", fcc.US_FCCTradeName);
		}

		public void TestProcessPGAs()
		{
			string messageText = @"
AA7501XJ58888B00001001                  200901011212120100  6004772             
H1A    XJ5 <E#PLCH>                    01                           01          
H2                          0000000000                                          
H5001                                       0000000000                          
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
ZZ7501000000010                                                                 ";

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText.Replace("\r\n", ""));

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(1, declaration.InvoiceLines.Count);

			AssertEquals("Containers", 6, declaration.CusContainers.Count);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];
			AssertEquals("Three PGA lines", 3, invoiceLine.LaceyActLines.Count);

			#region Assertion for first PGA line

			PGA pga = invoiceLine.LaceyActLines[0];
			AssertEquals("Commercial Desc", "SOFTWOOD PULPWOOD LOG LENGTH", pga.US_PGACommercialDescription);
			AssertEquals("Line number", 1, pga.US_PGALineItemNumber);
			AssertEquals("Value", 10000m, pga.US_PGALineValue);

			AssertEquals("One component", 1, pga.PG04ConstituentElements.Count);

			ConstituentElement constituent = pga.PG04ConstituentElements[0];
			AssertEquals("Component Name", "PINE", constituent.US_PGANameOfTheConstituentElement);
			AssertEquals("Component Qty", 100m, constituent.US_PGAQuantityOfConstituentElement);
			AssertEquals("Component Unit", LaceyActUnitsOfMeasureList.Codes.CubicMeters, constituent.US_PGAUnitOfMeasure);

			AssertEquals("Scientific Data", 1, constituent.ScientificDataCollection.Count);

			AssertScientificData(constituent.ScientificDataCollection[0], "PINUS", "TAEDA", "CA");
			AssertContainerRelated(invoiceLine, pga, "MAEUXXXX");

			#endregion

			#region Assertion for second PGA line
			pga = invoiceLine.LaceyActLines[1];
			AssertEquals("Commercial Desc", "SOFTWOOD PULPWOOD 4 FOOT LENGTH", pga.US_PGACommercialDescription);
			AssertEquals("Line number", 2, pga.US_PGALineItemNumber);
			AssertEquals("Value", 20000m, pga.US_PGALineValue);

			AssertEquals("One component", 1, pga.PG04ConstituentElements.Count);

			constituent = pga.PG04ConstituentElements[0];
			AssertEquals("Component Name", "PINE", constituent.US_PGANameOfTheConstituentElement);
			AssertEquals("Component Qty", 200m, constituent.US_PGAQuantityOfConstituentElement);
			AssertEquals("Component Unit", LaceyActUnitsOfMeasureList.Codes.CubicMeters, constituent.US_PGAUnitOfMeasure);

			AssertEquals("Scientific Data", 3, constituent.ScientificDataCollection.Count);

			AssertScientificData(constituent.ScientificDataCollection[0], "PINUS", "TAEDA", "GB");
			AssertScientificData(constituent.ScientificDataCollection[1], "PINUS", "TAEDA", "FR");
			AssertScientificData(constituent.ScientificDataCollection[2], "PINUS", "TAEDA", "DE");

			AssertContainerRelated(invoiceLine, pga, "MAEUTTTT");

			#endregion

			#region Assertion for third PGA line

			pga = invoiceLine.LaceyActLines[2];
			AssertEquals("Commercial Desc", "SOFTWOOD PULPWOOD SPLIT", pga.US_PGACommercialDescription);
			AssertEquals("Line number", 3, pga.US_PGALineItemNumber);
			AssertEquals("Value", 30000m, pga.US_PGALineValue);

			AssertEquals("One component", 1, pga.PG04ConstituentElements.Count);

			constituent = pga.PG04ConstituentElements[0];
			AssertEquals("Component Name", "PINE", constituent.US_PGANameOfTheConstituentElement);
			AssertEquals("Component Qty", 300m, constituent.US_PGAQuantityOfConstituentElement);
			AssertEquals("Component Unit", LaceyActUnitsOfMeasureList.Codes.CubicMeters, constituent.US_PGAUnitOfMeasure);

			AssertEquals("Scientific Data", 6, constituent.ScientificDataCollection.Count);

			AssertScientificData(constituent.ScientificDataCollection[0], "PINUS", "TAEDA", "GB");
			AssertScientificData(constituent.ScientificDataCollection[1], "PINUS", "RIGIDA", "GB");
			AssertScientificData(constituent.ScientificDataCollection[2], "PINUS", "ECHINADA", "GB");

			AssertScientificData(constituent.ScientificDataCollection[3], "PINUS", "TAEDA", "FR");
			AssertScientificData(constituent.ScientificDataCollection[4], "PINUS", "RIGIDA", "FR");
			AssertScientificData(constituent.ScientificDataCollection[5], "PINUS", "ECHINADA", "FR");

			AssertContainerRelated(invoiceLine, pga, "MAEUTTTT");
			AssertContainerRelated(invoiceLine, pga, "MAEUHHHHHH");
			AssertContainerRelated(invoiceLine, pga, "MAEUIIIIII");
			AssertContainerRelated(invoiceLine, pga, "MAEUVVVVV");
			AssertContainerRelated(invoiceLine, pga, "MAEYKKKKKK");

			#endregion

		}

		void AssertScientificData(ScientificData scientificData, ZString geniusName, ZString speciesName, ZString countryCode)
		{
			AssertEquals("Genius Name", geniusName, scientificData.US_PGAScientificGenusName);
			AssertEquals("Species Name", speciesName, scientificData.US_PGAScientificSpeciesName);
			AssertEquals("Country Code", countryCode, scientificData.US_PGACountryCode);
		}

		void AssertContainerRelated(JobComInvoiceLine invoiceLine, PGA pga, ZString containerNumber)
		{
			Assert(invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerNumber).IsForInvoiceLine);
			Assert(pga.ContainersForInvoiceLine.FindByContainerNumber(containerNumber).IsForPGALine);
		}
	}
}
