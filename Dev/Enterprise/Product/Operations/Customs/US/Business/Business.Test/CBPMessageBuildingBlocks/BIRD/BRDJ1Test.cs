using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.BIRD.Testing
{
	sealed class BRDJ1Test : TestCaseWithFactory
	{
		public void TestDeserialise()
		{
			string message = @"AAJI  XJ58888B00004751                  20091207204452                          J1XJ5 100020913                                                                 ZZJI  00000000101";

			InputBlockControlGenerator<BRDAA, BRDZZ> generator = new ImportInputBlockControlGenerator<BRDAA, BRDZZ>();
			AssertNoExceptionThrown(() => generator.Deserialise(BlockPadder.Pad(message)));
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

			//output generator as when a file containing 7501 is received from external broker, it is marked as a received message....
			ImportOutputBlockControlGenerator<BRDAA, BRDZZ> generator = new ImportOutputBlockControlGenerator<BRDAA, BRDZZ>();
			AssertNoExceptionThrown(() => generator.Deserialise(messageText.Replace("\r\n", "")));
		}
	}
}
