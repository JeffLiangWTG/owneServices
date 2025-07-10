using System;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class OGAFD02Test : OGABIRDUpdateTest
	{
		public void TestPersistFDAUQsCorrectly()
		{
			var messagetext = "AA7501XJ58888B00001001                  200901011212120100  6004772             10R888891-01319900091-013199000                 8         XJ5 7002182502891  IL 20     23                  103001081709B00152494            56   081709I299     22            8IDFS                               00000001PK         APLU       30                                  01              2082709             APLU    40001SV00000000000000000000                    000000020057020                  50 99150490  0000150900000000100000KG                               SV081709YP+ 51                                                                              60                                        GBBOOMED295LON                        62          50100001250                                                         700406100800           000000100000KG                               0000010000  OI        TEST                                                                  FD0100104BGT02   GBSLNFLJKDFSR90               GBBOOMED295LON GBBOOMED295LON    FD020000002000CT  0000003000BX  0000025000BV  0000000100KG                      FD03                      TEST                                                  FD04              TOMAS TANK6309991234                                          FD05ADA08172009                                                                 FD05APA3001                                                                     FD05ATA1200                                                                     FD05CSHSV                                                                       FD05OFTI                                                                        FD05PFR12345678901                                                              FD05PFTM                                                                        FD05SA1350 W 63RD STREET                                                        FD05SACWILLOWBROOK                                                              FD05SASIL                                                                       FD05SCCUS                                                                       FD05SCNTOMAS CO                                                                 FD05SCZ60527                                                                    FD05SEMNONE                                                                     FD05SFNTOMAS CO                                                                 FD05SFTI                                                                        FD05SFX0000000000                                                               FD05SPN5555555555                                                               FD05VFT56                                                                       FD05BOLAPLU8IDFS                                                                8950100000001250                                                                9000000150900           0                       0000000125000000010000          ZZ7501000000010                                                                 ";

			#region 80-byte version
			//B018888XJ5EI                                               34826                
			//10R888891-01319900091-013199000                 8         XJ5 7002182502891  IL 
			//20     23                  103001081709B00152494            56   081709I299     
			//22            8IDFS                               00000001PK         APLU       
			//30                                  01              2082709             APLU    
			//40001SV00000000000000000000                    000000020057020                  
			//50 99150490  0000150900000000100000KG                               SV081709YP+ 
			//51                                                                              
			//60                                        GBBOOMED295LON                        
			//62          50100001250                                                         
			//700406100800           000000100000KG                               0000010000  
			//OI        TEST                                                                  
			//FD0100104BGT02   GBSLNFLJKDFSR90               GBBOOMED295LON GBBOOMED295LON    
			//FD020000002000CT  0000003000BX  0000025000BV  0000000100KG                      
			//FD03                      TEST                                                  
			//FD04              TOMAS TANK6309991234                                          
			//FD05ADA08172009                                                                 
			//FD05APA3001                                                                     
			//FD05ATA1200                                                                     
			//FD05CSHSV                                                                       
			//FD05OFTI                                                                        
			//FD05PFR12345678901                                                              
			//FD05PFTM                                                                        
			//FD05SA1350 W 63RD STREET                                                        
			//FD05SACWILLOWBROOK                                                              
			//FD05SASIL                                                                       
			//FD05SCCUS                                                                       
			//FD05SCNTOMAS CO                                                                 
			//FD05SCZ60527                                                                    
			//FD05SEMNONE                                                                     
			//FD05SFNTOMAS CO                                                                 
			//FD05SFTI                                                                        
			//FD05SFX0000000000                                                               
			//FD05SPN5555555555                                                               
			//FD05VFT56                                                                       
			//FD05BOLAPLU8IDFS                                                                
			//8950100000001250                                                                
			//9000000150900           0                       0000000125000000010000          
			//Y  8888XJ5EI00037000000150900
			#endregion

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messagetext);

			var notifications = new NotificationCollection();
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var fdaLine = invoiceLine.FDAs.AddNew();

			foreach (var block in generator.MessageBlocks)
			{
				var lineRecord = block as IBIRDOGALineRecord;

				if (lineRecord != null)
				{
					lineRecord.Update(fdaLine, notifications);
				}
			}

			AssertEquals("KG", invoiceLine.FDAs[0].US_FDAMeasure1);
			AssertEquals(1m, invoiceLine.FDAs[0].US_FDAQty1);

			AssertEquals("BV", invoiceLine.FDAs[0].US_FDAMeasure2);
			AssertEquals(250m, invoiceLine.FDAs[0].US_FDAQty2);

			AssertEquals("BX", invoiceLine.FDAs[0].US_FDAMeasure3);
			AssertEquals(30m, invoiceLine.FDAs[0].US_FDAQty3);

			AssertEquals("CT", invoiceLine.FDAs[0].US_FDAMeasure4);
			AssertEquals(20m, invoiceLine.FDAs[0].US_FDAQty4);
		}

		protected override IBIRDOGALineRecord[] GetPopulatedOGARecords()
		{
			var fd02 = new OGAFD02();
			fd02.Unit1Measure = "KG";
			fd02.Unit1Quantity = 100.99m;

			var fd02WithFiveQuantities = new OGAFD02();
			fd02WithFiveQuantities.Unit1Measure = "PL";
			fd02WithFiveQuantities.Unit1Quantity = 1m;

			fd02WithFiveQuantities.Unit2Measure = "CTN";
			fd02WithFiveQuantities.Unit2Quantity = 2m;

			fd02WithFiveQuantities.Unit3Measure = "BOX";
			fd02WithFiveQuantities.Unit3Quantity = 3m;

			fd02WithFiveQuantities.Unit4Measure = "BOL";
			fd02WithFiveQuantities.Unit4Quantity = 4m;

			fd02WithFiveQuantities.Unit5Measure = "KG";
			fd02WithFiveQuantities.Unit5Quantity = 5m;

			return new IBIRDOGALineRecord[] { fd02, fd02WithFiveQuantities };
		}

		protected override IOGALine CreateOGALine(JobComInvoiceLine invoiceLine) => invoiceLine.FDAs.AddNew();

		protected override void PrepareJob(JobComInvoiceLine invoiceLine, IOGALine ogaLine, IBIRDOGALineRecord ogaRecord)
		{
			base.PrepareJob(invoiceLine, ogaLine, ogaRecord);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
		}

		protected override Type GetTypeOfMessageBlock() => typeof(OGAFD02);
	}
}
