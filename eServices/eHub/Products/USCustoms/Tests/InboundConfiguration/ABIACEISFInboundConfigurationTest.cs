using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceBroker.Common;
using Rhino.Mocks;
using CargoWise.eServices.USCustoms.MessageHandlerABIACE;

namespace CargoWise.eServices.USCustoms.Tests
{
	[TestClass]
	public class ABIACEISFInboundMessageHandlerTest
	{
		[TestMethod]
		public void TestDebatchMessages_Succeed()
		{
			var registryRepository = MockRepository.GenerateMock<IRegistryRepository>();
			var config = new ABIACEInboundMessageHandler(registryRepository);
			var messageContent = @"A3910SV9      092915                                                            B012304791NS                                                                    1062667382074   380180107                                                       3051TFBB182548919                                   0000000020 1509290019TRSF   4062667382074      3801                                                         50BILL EXPORTED ON 150929 AT                                                    503801                                                                          60 52436         01                                                             Z3910SV9      092915                                                            ";
			string header;
			string footer;
			List<MemoryStream> bodies;

			using (var messageStream = messageContent.ToStream())
			{
				bodies = config.DebatchMessages(messageStream, out header, out footer);
			}

			var expectedHeader = @"A3910SV9      092915                                                            ";
			Assert.AreEqual(expectedHeader, header);
			var expectedFooter = @"Z3910SV9      092915                                                            ";
			Assert.AreEqual(expectedFooter, footer);

			Assert.AreEqual(1, bodies.Count);
			var expectedBody = @"B012304791NS                                                                    1062667382074   380180107                                                       3051TFBB182548919                                   0000000020 1509290019TRSF   4062667382074      3801                                                         50BILL EXPORTED ON 150929 AT                                                    503801                                                                          60 52436         01                                                             ";
			Assert.AreEqual(expectedBody, bodies[0].ContentToString());
		}

		[TestMethod]
		public void TestDebatchMessages_MessageContainsSpecialCharacter_FallbackAndSucceed()
		{
			var registryRepository = MockRepository.GenerateMock<IRegistryRepository>();
			var config = new ABIACEInboundMessageHandler(registryRepository);

			//Special character is "¯" in "STRA¯E". This character takes two bytes whose decimal values are 194 and 175.
			var messageContent = "A3910SV9      09091501   091015013041                                           B012720275$S                                               CIRLAXLAX_176430     $1DEOPTGMB18OBE                                                                 $2    DEOPTACOM GMBH & CO KG                                                    $3                              D_RRFELDER STRA¯E 18                            $4                                                   OBEREUERHEIM               $5                                            97508                             Y  2720275$S00005                                                               Z3910SV9      09091501   091015013041                                           ";

			string header;
			string footer;
			List<MemoryStream> bodies;

			using (var messageStream = messageContent.ToStream())
			{
				bodies = config.DebatchMessages(messageStream, out header, out footer);
			}

			var expectedHeader = @"A3910SV9      09091501   091015013041                                           ";
			Assert.AreEqual(expectedHeader, header);
			var expectedFooter = @"Z3910SV9      09091501   091015013041                                           ";
			Assert.AreEqual(expectedFooter, footer);

			Assert.AreEqual(1, bodies.Count);
			var expectedBody = @"B012720275$S                                               CIRLAXLAX_176430     $1DEOPTGMB18OBE                                                                 $2    DEOPTACOM GMBH & CO KG                                                    $3                              D_RRFELDER STRA¯E 18                            $4                                                   OBEREUERHEIM               $5                                            97508                             Y  2720275$S00005                                                               ";
			Assert.AreEqual(expectedBody, bodies[0].ContentToString());
		}

		[TestMethod]
		public void TestDebatchMessages_MessageContainsInvalidCharForXML_ReplaceWithSpace()
		{
			var registryRepository = MockRepository.GenerateMock<IRegistryRepository>();
			var config = new ABIACEInboundMessageHandler(registryRepository);

			//The special character is 0X19 which is EM, end of medium. This char is non-printing character and it is invalid for xml. EM exists in the messageContent.
			//In order to see EM, please copy and paste the following example in notepad++ ex> 301s
			var messageContent = "A3910SV9      080118     UC                                                     B  3802HN7UC                                  3501HN7                           E141080998080118                                  HN7  14466027     P00025784   E2  ROBERT BURANDT                  (1)6123481                                  E3The goods on line 001 of this entry were entered for consumption after 12:01 AE3M Eastern Standard Time on July 6th, 2018, and are subject to Section 301s adE3ditional import duty rate of 25% ad valorem, in addition to their general (colE3umn 1) rate of duty.   Please add tariff 9903.88.01, and the additional 25% adE3 valorem duty rate to the affected line and retransmit to the port of entry.  E3Response required within 2 working days.   Contact Robert.W.Burandt@cbp.dhs.goE3v for questions.                                                              Y  3802HN7UC00001                                                               Z3910SV9      080118                                                            ";

			string header;
			string footer;
			List<MemoryStream> bodies;

			using (var messageStream = messageContent.ToStream())
			{
				bodies = config.DebatchMessages(messageStream, out header, out footer);
			}

			var expectedHeader = @"A3910SV9      080118     UC                                                     ";
			Assert.AreEqual(expectedHeader, header);
			var expectedFooter = @"Z3910SV9      080118                                                            ";
			Assert.AreEqual(expectedFooter, footer);

			Assert.AreEqual(1, bodies.Count);
			var expectedBody = @"B  3802HN7UC                                  3501HN7                           E141080998080118                                  HN7  14466027     P00025784   E2  ROBERT BURANDT                  (1)6123481                                  E3The goods on line 001 of this entry were entered for consumption after 12:01 AE3M Eastern Standard Time on July 6th, 2018, and are subject to Section 301 s adE3ditional import duty rate of 25% ad valorem, in addition to their general (colE3umn 1) rate of duty.   Please add tariff 9903.88.01, and the additional 25% adE3 valorem duty rate to the affected line and retransmit to the port of entry.  E3Response required within 2 working days.   Contact Robert.W.Burandt@cbp.dhs.goE3v for questions.                                                              Y  3802HN7UC00001                                                               ";
			Assert.AreEqual(expectedBody, bodies[0].ContentToString());
		}
	}
}