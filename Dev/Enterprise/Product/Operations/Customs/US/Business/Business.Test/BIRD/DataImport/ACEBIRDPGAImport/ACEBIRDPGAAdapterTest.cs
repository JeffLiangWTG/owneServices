using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDPGAAdapterTest : TestCaseWithFactory
	{
		public void TestSupportedPGAViaACEBIRD()
		{
			var messageText =
"B  1101SV9SE                                               HYEDUSCMT_168564     " +
"SE10ASV9  71019382 01               41000000145003901  1101                     " +
"SE11       1234                        3431                                     " +
"SE40001US LAMBORGINI AVENTADOR                                                  " +
"SE6096031005000000014500                                                        " +
"OI        LAMBORGINI AVENTADOR                                                  " +
"PG01001FDA                                                                      " +
"PG01001FSI                                                                      " +
"PG01001NHT                                                                      " +
"PG01001EPAODS                                                                   " +
"PG01001EPATS1                                                                   " +
"PG01001EPAVNE                                                                   " +
"PG01001EPAPS1                                                                   " +
"PG01001EPAHFC                                                                   " +
"PG01001NMF370                                                                   " +
"PG01001NMFAMR                                                                   " +
"PG01001NMFHMS                                                                   " +
"PG01001DTC                                                                      " +
"PG01001TTB                                                                      " +
"PG01001AMS                                                                      " +
"PG01001AMSOR 1                                                                  " +
"PG01001APHAVS                                                                   " +
"PG01001APHAPL                                                                   " +
"PG01001FWS                                                                      " +
"PG01001ATF                                                                      " +
"PG01001OMC                                                                      " +
"PG01001DEA                                                                      " +
"PG01001CPS                                                                      " +
"Y  1101SV9SE                                                                    ";

			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			Assert("FSIS is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "FSI import via BIRD is not supported yet."));
			Assert("ODS is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "EPA - ODS import via BIRD is not supported yet."));
			Assert("TSCA is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "EPA - TS1 import via BIRD is not supported yet."));
			Assert("VNE is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "EPA - VNE import via BIRD is not supported yet."));
			Assert("PST is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "EPA - PS1 import via BIRD is not supported yet."));
			Assert("HFC is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "EPA - HFC import via BIRD is not supported yet."));
			Assert("NMFS 370 is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "NMF - 370 import via BIRD is not supported yet."));
			Assert("NMFS AMR is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "NMF - AMR import via BIRD is not supported yet."));
			Assert("NMFS HMS is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "NMF - HMS import via BIRD is not supported yet."));
			Assert("DDTC is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "DTC import via BIRD is not supported yet."));
			Assert("TTB is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "TTB import via BIRD is not supported yet."));
			Assert("AMS is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "AMS import via BIRD is not supported yet."));
			Assert("AMSOR is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "AMS - OR import via BIRD is not supported yet."));
			Assert("APHIS is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "APH - AVS import via BIRD is not supported yet."));
			Assert("FWS is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "FWS import via BIRD is not supported yet."));
			Assert("ATF is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "ATF import via BIRD is not supported yet."));
			Assert("FDA is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "FDA import via BIRD is not supported yet."));
			Assert("NHTSA is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "NHT import via BIRD is not supported yet."));
			Assert("Lacey is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "APH - APL import via BIRD is not supported yet."));
			Assert("OMC is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "OMC import via BIRD is not supported yet."));
			Assert("DEA is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "DEA import via BIRD is not supported yet."));
			Assert("CPSC is supported in ACE Bird now", !notifications.GetWarnings().Any(x => x.Message == "CPSC import via BIRD is not supported yet."));
		}
	}
}
