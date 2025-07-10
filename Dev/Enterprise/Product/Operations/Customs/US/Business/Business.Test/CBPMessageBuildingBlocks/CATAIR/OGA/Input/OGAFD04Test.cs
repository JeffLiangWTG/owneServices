using System;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class OGAFD04Test : OGABIRDUpdateTest
	{
		public void TestPersistFDAUQsCorrectly()
		{
			var messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10R888891-01319900091-013199000                 8         XJ5 7002182502891  IL 20     23                  103001081709B00152494            56   081709I299     22            8IDFS                               00000001PK         APLU       30                                  01              2082709             APLU    40001SV00000000000000000000                    000000020057020                  50 99150490  0000150900000000100000KG                               SV081709YP+ 51                                                                              60                                        GBBOOMED295LON                        62          50100001250                                                         700406100800           000000100000KG                               0000010000  OI        TEST                                                                  FD0100104BGT02   GBSLNFLJKDFSR90               GBBOOMED295LON GBBOOMED295LON    FD020000000100PK  0000004000PL  0000002000CT  0000003000BX  0000025000BV        FD03                      TEST                                                  FD040000000100KG  TOMAS TANK6309991234                                          FD05ADA08172009                                                                 FD05APA3001                                                                     FD05ATA1200                                                                     FD05CSHSV                                                                       FD05OFTI                                                                        FD05PFR12345678901                                                              FD05PFTM                                                                        FD05SA1350 W 63RD STREET                                                        FD05SACWILLOWBROOK                                                              FD05SASIL                                                                       FD05SCCUS                                                                       FD05SCNTOMAS CO                                                                 FD05SCZ60527                                                                    FD05SEMNONE                                                                     FD05SFNTOMAS CO                                                                 FD05SFTI                                                                        FD05SFX0000000000                                                               FD05SPN5555555555                                                               FD05VFT56                                                                       FD05BOLAPLU8IDFS                                                                8950100000001250                                                                9000000150900           0                       0000000125000000010000          ZZ7501000000010                                                                 ";
			#region 80-byte version
			//B018888XJ5EI                                               35039                
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
			//FD020000000100PK  0000004000PL  0000002000CT  0000003000BX  0000025000BV        
			//FD03                      TEST                                                  
			//FD040000000100KG  TOMAS TANK6309991234                                          
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
			generator.Deserialise(messageText);

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

			AssertEquals("PL", invoiceLine.FDAs[0].US_FDAMeasure5);
			AssertEquals(40m, invoiceLine.FDAs[0].US_FDAQty5);

			AssertEquals("PK", invoiceLine.FDAs[0].US_FDAMeasure6);
			AssertEquals(1m, invoiceLine.FDAs[0].US_FDAQty6);
		}

		protected override IBIRDOGALineRecord[] GetPopulatedOGARecords()
		{
			var fd04 = new OGAFD04();

			fd04.Unit6Quantity = 3m;
			fd04.Unit6Measure = "KG";

			return new IBIRDOGALineRecord[] { fd04 };
		}

		protected override void PrepareJob(JobComInvoiceLine invoiceLine, IOGALine ogaLine, IBIRDOGALineRecord ogaRecord)
		{
			base.PrepareJob(invoiceLine, ogaLine, ogaRecord);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			var fdaLine = (FDA)ogaLine;
			fdaLine.US_FDAQty1 = 4;
			fdaLine.US_FDAMeasure1 = "AA";

			fdaLine.US_FDAQty2 = 5;
			fdaLine.US_FDAMeasure2 = "BB";

			fdaLine.US_FDAQty3 = 6;
			fdaLine.US_FDAMeasure3 = "CC";

			fdaLine.US_FDAQty4 = 7;
			fdaLine.US_FDAMeasure4 = "DD";

			fdaLine.US_FDAQty5 = 8;
			fdaLine.US_FDAMeasure5 = "EE";
		}

		protected override IOGALine CreateOGALine(JobComInvoiceLine invoiceLine) => invoiceLine.FDAs.AddNew();

		protected override Type GetTypeOfMessageBlock() => typeof(OGAFD04);

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				"ValuePerBaseUnit",//This is for ACS and is not used currently
				"ContactName",//When WI00012421 is finalised, dec or invoice should have a field for this
				"ContactTelephoneNumber",//When WI00012421 is finalised, dec or invoice should have a field for this
			};
		}
	}
}
