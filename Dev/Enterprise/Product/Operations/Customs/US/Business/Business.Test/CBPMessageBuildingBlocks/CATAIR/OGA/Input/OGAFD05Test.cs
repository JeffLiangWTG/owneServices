using System;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class OGAFD05Test : OGABIRDUpdateTest
	{
		public void TestUpdateAccordingToAffirmationCodes()
		{
			var messageText = "AA3461J58888B00001001                  200901011212120100  6004772              H1A8888XJ5 7001870691-0131990004007140981                   AA  390101891       H2I317    91-013199000 150  0000004058B00152390                                 HA            00155546514                         00000002PK                    H5001GB9018908000GBBOOMED295LON             0000004058                          OI        WHAT                                                                  FD0100176E--AX   GBSLNFLJKDFSR90               GBBOOMED295LON GBBOOMED295LON    FD020000010000PCS                                                               FD030000000500                                                                  FD04              TOMAS TANK6309991234                                          FD05ADA07142009                                                                 FD05APA3901                                                                     FD05ATA1200                                                                     FD05CFR8784800                                                                  FD05CSHGB                                                                       FD05DEV1417592                                                                  FD05LSTB117902                                                                  FD05OFTI                                                                        FD05PFR12345678901                                                              FD05PFTG                                                                        FD05SA1350 W 63RD STREET                                                        FD05SACWILLOWBROOK                                                              FD05SASIL                                                                       FD05SCCUS                                                                       FD05SCNTOMAS CO                                                                 FD05SCZ60527                                                                    FD05SEMNONE                                                                     FD05SFNTOMAS CO                                                                 FD05SFTF                                                                        FD05SFX0000000000                                                               FD05SPN5555555555                                                               FD05VFT150                                                                      FD05AWB00155546514                                                              ZZ7501000000010                                                                 ";

			var eightyByteVersion = @"FD05ADA07142009                                                                 
FD05APA3901                                                                     
FD05ATA1200                                                                     
FD05CFR8784800                                                                  
FD05CSHGB                                                                       
FD05DEV1417592                                                                  
FD05LSTB117902                                                                  
FD05OFTI                                                                        
FD05PFR12345678901                                                              
FD05PFTG                                                                        
FD05SA1350 W 63RD STREET                                                        
FD05SACWILLOWBROOK                                                              
FD05SASIL                                                                       
FD05SCCUS                                                                       
FD05SCNTOMAS CO                                                                 
FD05SCZ60527                                                                    
FD05SEMNONE                                                                     
FD05SFNTOMAS CO                                                                 
FD05SFTF                                                                        
FD05SFX0000000000                                                               
FD05SPN5555555555                                                               
FD05VFT150                                                                      
FD05AWB00155546514                                                              
FD05TEMNONE                                                                     ";

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var notifications = new NotificationCollection();
			var declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(1, declaration.InvoiceLines.Count);

			var invoiceLine = declaration.InvoiceLines[0];
			var fda = invoiceLine.FDAs[0];
			fda.US_FDAForcePN = true;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(declaration.ActiveEntryHeaders.CargoReleaseEntry);

			var builder = new CargoReleaseMessageBuilder(declaration.ActiveEntryHeaders.CargoReleaseEntry, UpdateActionCode.Add, true);
			var message = builder.PopulateMessage();
			AssertContains(eightyByteVersion, message.EM_FormattedMessageText);
		}

		public void TestUpdateContainersAndRailCars()
		{
			var messageText = "AA3461J58888B00001001                  200901011212120100  6004772              H1A8888XJ5 7001870691-0131990004007140981                   AA  390101891       H2I317    91-013199000 150  0000004058B00152390                                 HA            00155546514                         00000002PK                    H5001GB9018908000GBBOOMED295LON             0000004058                          OI        WHAT                                                                  FD0100176E--AX   GBSLNFLJKDFSR90               GBBOOMED295LON GBBOOMED295LON    FD020000010000PCS                                                               FD030000000500                                                                  FD04              TOMAS TANK6309991234                                          FD05ADA07142009                                                                 FD05APA3901                                                                     FD05ATA1200                                                                     FD05CNOAPLU00000100                                                             FD05CNOAPLU00000101                                                             FD05DEV1417592                                                                  FD05LSTB117902                                                                  FD05OFTI                                                                        FD05PFR12345678901                                                              FD05PFTG                                                                        FD05RNOCWR0100                                                                  FD05RNOCWR0101                                                                  FD05RNOCWR0102                                                                  FD05SCCUS                                                                       FD05SCNTOMAS CO                                                                 FD05SCZ60527                                                                    FD05SEMNONE                                                                     FD05SFNTOMAS CO                                                                 FD05SFTF                                                                        FD05SFX0000000000                                                               FD05SPN5555555555                                                               FD05VFT150                                                                      FD05AWB00155546514                                                              ZZ7501000000010                                                                 ";

			var eightyByteVersion = @"FD020000010000PCS                                                               
FD030000000500                                                                  
FD05ADA07142009                                                                 
FD05APA3901                                                                     
FD05ATA1200                                                                     
FD05CNOAPLU00000100                                                             
FD05CNOAPLU00000101                                                             
FD05CNOCWR0100                                                                  
FD05CNOCWR0101                                                                  
FD05CNOCWR0102                                                                  
FD05DEV1417592                                                                  
FD05LSTB117902                                                                  
FD05OFTI                                                                        
FD05PFR12345678901                                                              
FD05PFTG                                                                        
FD05SCCUS                                                                       
FD05SCNTOMAS CO                                                                 
FD05SCZ60527                                                                    
FD05SEMNONE                                                                     
FD05SFNTOMAS CO                                                                 
FD05SFTF                                                                        
FD05SFX0000000000                                                               
FD05SPN5555555555                                                               
FD05VFT150                                                                      
FD05AWB00155546514                                                              
FD05TEMNONE                                                                     ";

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var notifications = new NotificationCollection();
			var declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(1, declaration.InvoiceLines.Count);

			var invoiceLine = declaration.InvoiceLines[0];
			var fda = invoiceLine.FDAs[0];
			fda.US_FDAForcePN = true;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(declaration.ActiveEntryHeaders.CargoReleaseEntry);

			var builder = new CargoReleaseMessageBuilder(declaration.ActiveEntryHeaders.CargoReleaseEntry, UpdateActionCode.Add, true);
			var message = builder.PopulateMessage();
			AssertContains(eightyByteVersion, message.EM_FormattedMessageText);
		}

		protected override void PrepareJob(JobComInvoiceLine invoiceLine, IOGALine ogaLine, IBIRDOGALineRecord ogaRecord)
		{
			base.PrepareJob(invoiceLine, ogaLine, ogaRecord);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			var fda = (FDA)ogaLine;
			fda.AffirmationCodes.AddNew(AffirmationCodeConstants.Codes.SLN, "Smith");
			fda.US_FDAForcePN = true;
		}

		protected override IOGALine CreateOGALine(JobComInvoiceLine invoiceLine) => invoiceLine.FDAs.AddNew();

		protected override Type GetTypeOfMessageBlock() => typeof(OGAFD05);

		protected override IBIRDOGALineRecord[] GetPopulatedOGARecords()
		{
			var ogafd05 = new OGAFD05();
			ogafd05.AffirmationOfComplianceCode = "CSH";
			ogafd05.AffirmationOfComplianceQualifier = "GB";

			return new IBIRDOGALineRecord[] { ogafd05 };
		}
	}
}
