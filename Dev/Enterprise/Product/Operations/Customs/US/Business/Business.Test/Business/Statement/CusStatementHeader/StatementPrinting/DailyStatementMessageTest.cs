using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DailyStatementMessageTest : TestCaseWithFactory
	{
		public void TestDairyFeeIsMissing()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.DailyStatement;
			message.EM_MessageSubType = "STM";
			message.EM_MessageText = "B012704221QRP27132389830826132            27                                    "
				+ "Q12704221 602061280000000000000000000000 0000000000000000000000K00020662 27   01"
				+ "Q22704221 60206128208261395-3838879000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60206128000000000000000000019880000000000000717400000000000000000000  "
				+ "Q12704221 602086030000000000000000000000 0000000000000000000000K00020907 27   01"
				+ "Q22704221 60208603208261395-2001941000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60208603000000000000000000048500000000000002811100000000000000000000  "
				+ "Q12704221 602097340000007562500000000000 0000000000000000000000K00021018 27   01"
				+ "Q22704221 60209734208261333-0932960000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60209734000000000000000000011908000000000000429700000000000000000000  "
				+ "Q12704221 602097420000007562500000000000 0000000000000000000000K00021019 27   01"
				+ "Q22704221 60209742208261333-0932960000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60209742000000000000000000011908000000000000429700000000000000000000  "
				+ "Q12704221 602097590000007562500000000000 0000000000000000000000K00021020 27   01"
				+ "Q22704221 60209759208261333-0932960000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60209759000000000000000000011908000000000000429700000000000000000000  "
				+ "Q12704221 602098170000007360000000000000 0000000000000000000000K00021026 27   01"
				+ "Q22704221 60209817208261395-1034280000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60209817000000000000000000006570000000000000237100000000000000000527  "
				+ "Q12704221 602111020000000000000000000000 0000000000000000000000K00021154 27   01"
				+ "Q22704221 60211102208261391-1290697000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60211102000000000000000000048500000000000003136400000000000000000000  "
				+ "Q12704221 602113340000000000000000000000 0000000000000000000000K00021177 27   01"
				+ "Q22704221 60211334208261394-2308641000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60211334000000000000000000000000000000000003346500000000000000000000  "
				+ "Q12704221 602114410000000000000000000000 0000000000000000000000K00021188 27   01"
				+ "Q22704221 60211441208261395-4723425000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60211441000000000000000000007429000000000000268100000000000000000000  "
				+ "Q12704221 602120680000000000000000000000 0000000000000000000000K00021249 27   01"
				+ "Q22704221 60212068208261338-3667249000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60212068000000000000000000024126000000000000870600000000000000000000  "
				+ "Q12704221 602122740000002287800000119216N0000000000000000000000K00021270 27   01"
				+ "Q22704221 60212274208261394-1118321LA0000000000000000000000Y  00000000000P      "
				+ "QA2704221 60212274000000000000000000018675000000000000673900000000000000000000  "
				+ "Q12704221 602122820000002419200000123693N0000000000000000000000K00021271 27   01"
				+ "Q22704221 60212282208261394-1118321LA0000000000000000000000Y  00000000000P      "
				+ "QA2704221 60212282000000000000000000014508000000000000523500000000000000000000  "
				+ "Q12704221 602124150000020222500000000000 0000000000000000000000K00021284 27   01"
				+ "Q22704221 60212415208261395-4807740000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60212415000000000000000000014287000000000000516100000000000000000000  "
				+ "QB2704221 6021241500000000000000000000002200000000000000000000000000000000      "
				+ "Q12704221 602125890000040455600000000000 0000000000000000000000K00021301 27   01"
				+ "Q22704221 60212589208261395-2226712000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60212589000000000000000000030568000000000001103100000000000000000000  "
				+ "Q12704221 602137690000000000000000000000 0000000000000000000000K00021418 27   01"
				+ "Q22704221 60213769208261395-3838879000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60213769000000000000000000013933000000000000502800000000000000000000  "
				+ "Q12704221 602137930000009409000000000000 0000000000000000000000K00021421 27   01"
				+ "Q22704221 60213793208261320-8789406000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60213793000000000000000000014815000000000000534600000000000000000000  "
				+ "Q12704221 602138190000013696000000000000 0000000000000000000000K00021423 27   01"
				+ "Q22704221 60213819208261302-0560316000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60213819000000000000000000002500000000000000087300000000000000000000  "
				+ "Q12704221 602138270000014112000000000000 0000000000000000000000K00021425 27   01"
				+ "Q22704221 60213827208261302-0560316000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60213827000000000000000000002500000000000000088200000000000000000000  "
				+ "Q12704221 602142540000000340000000000000 0000000000000000000000K00021468 27   01"
				+ "Q22704221 60214254208261346-1262565000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60214254000000000000000000006843000000000000246900000000000000000000  "
				+ "Q12704221 602142620000007187800000000000 0000000000000000000000K00021469 27   01"
				+ "Q22704221 60214262208261346-1262565000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60214262000000000000000000003890000000000000140400000000000000000000  "
				+ "Q12704221 602142700000004033600000000000 0000000000000000000000K00021470 27   01"
				+ "Q22704221 60214270208261346-1262565000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60214270000000000000000000007584000000000000273700000000000000000000  "
				+ "Q12704221 602143120000028909000000000000 0000000000000000000000K00021474 27   01"
				+ "Q22704221 60214312208261334-1668411000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60214312000000000000000000042592000000000001537000000000000000000000  "
				+ "Q12704221 602144600000000000000000000000 0000000000000000000000K00021488 27   01"
				+ "Q22704221 60214460208261395-3414374000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60214460000000000000000000006687000000000000241300000000000000000000  "
				+ "Q12704221 602145020000000007200000000000 0000000000000000000000K00003089 27   01"
				+ "Q22704221 60214502208261395-4620852000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60214502000000000000000000002500000000000000082800000000000000000000  "
				+ "Q12704221 602148580000000077500000000000 0000000000000000000000K00021525 27   01"
				+ "Q22704221 60214858208261326-2836553000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60214858000000000000000000048500000000000003259800000000000000000000  "
				+ "Q12704221 602150530000069157500000000000 0000000000000000000000K00003098 27   01"
				+ "Q22704221 60215053208261326-0477483000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60215053000000000000000000021999000000000000793800000000000000000000  "
				+ "Q12704221 602154260000024952500000000000 0000000000000000000000K00021581 27   01"
				+ "Q22704221 60215426208261383-0342382000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60215426000000000000000000015435000000000000557000000000000000000000  "
				+ "Q12704221 602154340000000117800000000000 0000000000000000000000K00021582 27   01"
				+ "Q22704221 60215434208261377-0557796000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60215434000000000000000000020874000000000000753300000000000000000000  "
				+ "Q12704221 602157640000000000000000000000 0000000000000000000000K00021617 27   01"
				+ "Q22704221 60215764208261395-4723425000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60215764000000000000000000000000000000000000216000000000000000000000  "
				+ "Q12704221 602162180000003767400000355120N0000000000000000000000K00021662 27   01"
				+ "Q22704221 60216218208261395-1034280000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60216218000000000000000000037643000000000001358500000000000000000000  "
				+ "Q12704221 602164240000018658000000000000 0000000000000000000000K00021683 27   01"
				+ "Q22704221 60216424208261352-2238444000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60216424000000000000000000048500000000000001794000000000000000000000  "
				+ "Q12704221 602191700000061999100000000000 0000000000000000000000K00021955 27   01"
				+ "Q22704221 60219170208261313-3476659NY0000000000000000000000Y  00000000000P      "
				+ "QA2704221 60219170000000000000000000037028000000000001336200000000000000000000  "
				+ "Q12704221 602199230000146004700000000000 0000000000000000000000K00022029 27   01"
				+ "Q22704221 60219923208261313-2559853SL0000000000000000000000Y  00000000000P      "
				+ "QA2704221 60219923000000000000000000048500000000000006864100000000000000000000  "
				+ "Q12704221 602206400000000000000000000000 0000000000000000000000K00022101 27   01"
				+ "Q22704221 60220640208261306-1312088000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60220640000000000000000000048500000000000001845900000000000000000000  "
				+ "Q12704221 602209960000000000000000000000 0000000000000000000000K00022136 27   01"
				+ "Q22704221 602209962082613043901-009130000000000000000000000Y  00000000000P      "
				+ "QA2704221 60220996000000000000000000048500000000000024555500000000000000000000  "
				+ "Q12704221 602220670000000000000000000000 0000000000000000000000K00022244 27   01"
				+ "Q22704221 60222067208261345-4583645000000000000000000000000Y  00000000000P      "
				+ "QA2704221 60222067000000000000000000048500000000000003068300000000000000000000  "
				+ "Q12704221 602146840000062494900000000000 0000000000000000000000K00021508 27   02"
				+ "Q22704221 60214684208261395-4807740000000000000000000000000Y  00000000000       "
				+ "QA2704221 60214684000000000000000000031987000000000001155400000000000000000000  "
				+ "QC2704221 602146840000000000000000000000000000000000004777                      "
				+ "Q3270422127132389830826130000560356600000598029000000000000000000000000000000000"
				+ "Q4270422100000000527000000000000000000000000000000000000077033550003700000      "
				+ "QE2704221000000000000000000000000000000828577000000000000667857000000000000     "
				+ "QF2704221000000000000000022000000000000000000000000000000000000000000000        "
				+ "QG2704221000000000000000000000000000000004777                                   "
				+ "Y  2704221QR00118                                                               ";

			var dailyHeader = new DailyStatementMessageHeader(Factory.New<CusStatementHeader>(), message);
			AssertEquals(47.77m, dailyHeader.TotalDairyFee);

			message.EM_MessageText = "B012506610QRF25132031190722132            96  3901610  1                        "
				+ "Q12506610 890376280001002995200000000000 00000000000000000000000XI000005 96   09"
				+ "Q22506610 89037628207221354-1550550000000000000000000000000Y  00000000000       "
				+ "QA2506610 89037628000000000000000000124551000000000014197500000000000000000000  "
				+ "QB2506610 8903762800372799000000000000000000000000000000000000000000000000      "
				+ "Q3250661025132031190722130001002995200000000000000000000000000000000000000000000"
				+ "Q4250661000000000000000003727990000000000000000000000000106692770000100000      "
				+ "QE2506610000000000000000000000000000000124551000000000000141975000000000000     "
				+ "QF2506610000000000000000000000000000000000000000000000000000000000000000        "
				+ "QG2506610000000000000000000000000000000004740                                   "
				+ "Q5250661025132031190722130001002995200000000000000000000000000000000000000000000"
				+ "Q6250661000000000000000003727990000000000000000000000000106692770000100000      "
				+ "QJ2506610000000000000000000000000000000124551000000000000141975000000000000     "
				+ "QK2506610000000000000000000000000000000000000000000000000000000000000000        "
				+ "QL2506610000000000000000000000000000000004640                                   "
				+ "Y  2506610QR00014                                                               ";
			dailyHeader = new DailyStatementMessageHeader(Factory.New<CusStatementHeader>(), message);
			AssertEquals(46.40m, dailyHeader.TotalDairyFee);
			AssertEquals(47.40m, dailyHeader.FinalTotalDairyFee);
		}

		public void TestFinalFiguresInPremilinaryMessage()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.DailyStatement;
			message.EM_MessageSubType = "STM";
			message.EM_MessageText =
				"B011101SV9QRP27092837721125082                                                  "
				+ "Q11101SV9 700229240000180780000000000000 0000000000000000000000*********        "
				+ "Q21101X02 00001231211250816-1494887000000000000000000000000Y  00000000000P      "
				+ "QA1101X02 00001231000017000000000000002500000000000000000000000000000000000000  "
				+ "Q11101X02 000012230000170000000000000000 0000000000000000000000*********        "
				+ "Q21101X02 00001223211250816-1494887000000000000000000000000Y  00000000000       "
				+ "QA1101X02 0000122300000000000000000004850000000000000625000000000000000000000   "
				+ "Q31101X02270928377211250800001730600000000000000000000000000000000000000000000  "
				+ "Q41101X0200000000000000000000000000000000000000000000000018441000000200000      "
				+ "QE1101X02000000000000000000000000000000051000000000000000062500000000000000     "
				+ "QF1101X02000000000000000000000000000000000000000000000000000000000000000        "
				+ "QG1101X02000000000000000000                                                     "
				+ "Y  1101X02QR00011";
			var dailyHeader = new DailyStatementMessageHeader(Factory.New<CusStatementHeader>(), message);
			AssertEquals(17306m, dailyHeader.FinalTotalDuty);
			AssertEquals(510m, dailyHeader.FinalTotalMerchandiseProcessingFee);
			AssertEquals(625m, dailyHeader.FinalTotalHarborMaintenanceFeeWaterways);
			AssertEquals(18441m, dailyHeader.FinalTotalAmountDue);
			AssertEquals("2", dailyHeader.FinalTotalNumberRevenueProducingEntriesForPrint);
		}

		public void TestMessageProcessing()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.DailyStatement;
			message.EM_MessageSubType = "STM";
			message.EM_MessageText =
				"B018888XJ5QRF11040910020502076                                                  "
				+ "Q18888XJ5 600188980000052345800000000000 00000000000000000000002013576        01"
				+ "Q28888XJ5 60018898605020795-278829700           00000000000Y  00000000000P      "
				+ "QA8888XJ5 60018898000000000000000000002678000000000000000000000000000000000000  "
				+ "Q18888XJ5 600315860000023562000000000000 00000000000000000000002013573        01"
				+ "Q28888XJ5 60031586605020795-278829700           00000000000Y  00000000000       "
				+ "QA8888XJ5 60031586000000000000000000006676000000000000000000000000000000000000  "
				+ "Q18888XJ5 600315780000016686000000000000 00000000000000000000002013574        01"
				+ "Q28888XJ5 60031578605020795-278829700           00000000000Y  00000000000       "
				+ "QA8888XJ5 60031578000000000000000000004728000000000000000000000000000000000000  "
				+ "Q38888XJ588040910020502070000559488300000000000000000000000000004263100000020351"
				+ "Q48888XJ50000000618400000000000           00000000000000059673690003900001      "
				+ "QE8888XJ5000000500000002159000009112000253093000010256000000316000000001000     "
				+ "QF8888XJ5000007321000001172000005991000004810000001640000003530000002420        "
				+ "Q58888XJ588040910020502070000600673100000000000000000000000000004633400000020351"
				+ "Q68888XJ50000000618400000000000           00000003636000064082400004400001      "
				+ "QJ8888XJ5000000500000002159000009112000274577000010256000000316000000001200     "
				+ "QK8888XJ5000007321000001172000005991000004810000001640000003530000002420        "
				+ "Q788040910028888 20135316ABI8888 20135423ABI8888 20135530CBP8888 20135647CBP    "
				+ "Q788040910028887 20135753ABI                                                    "
				+ "Y  8888XJ5QR00132";

			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_ProcessDate = ZDateTime.Now.AddDays(1);
			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryNum = "60018898";
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_BrokerReference = "2013576";
			var statementLine2 = statementHeader.StatementLines.AddNew();
			statementLine2.B3_EntryNum = "60031586";
			statementLine2.B3_EntryFilerCode = "XJ5";
			statementLine2.B3_BrokerReference = "2013573";
			var statementLine3 = statementHeader.StatementLines.AddNew();
			statementLine3.B3_EntryNum = "60031578";
			statementLine3.B3_EntryFilerCode = "XJ5";
			statementLine3.B3_BrokerReference = "2013574";

			var dailyHeader = new DailyStatementMessageHeader(statementHeader, message);
			AssertEquals("EDI CUSTOMS BROKERS", dailyHeader.StatementFor);
			AssertEquals(statementHeader.B2_ProcessDate, dailyHeader.B2_ProcessDate);
			AssertEquals(new ZDateTime(2007, 05, 02), dailyHeader.B2_PrintDate);
			AssertEquals("", dailyHeader.B2_BranchDesignation);
			AssertEquals("XJ5", dailyHeader.B2_EntryFilerCode);
			AssertEquals("8888", dailyHeader.B2_ProcessPort);
			AssertEquals("", dailyHeader.B2_ImporterCustomsID);

			AssertEquals(60067.31m, dailyHeader.TotalDuty);
			AssertEquals(463.34m, dailyHeader.TotalCVD);
			AssertEquals(203.51m, dailyHeader.TotalADD);
			AssertEquals(61.84m, dailyHeader.TotalCottonFee);
			AssertEquals(36.36m, dailyHeader.TotalSugarFee);
			AssertEquals(64082.40m, dailyHeader.TotalAmountDue);
			AssertEquals("44", dailyHeader.TotalNumberRevenueProducingEntriesForPrint);
			AssertEquals("1", dailyHeader.TotalNumberNonRevenueProducingEntriesForPrint);
			AssertEquals(5m, dailyHeader.TotalMailFee);
			AssertEquals(21.59m, dailyHeader.TotalBeefFee);
			AssertEquals(91.12m, dailyHeader.TotalPorkFee);
			AssertEquals(2745.77m, dailyHeader.TotalMerchandiseProcessingFee);
			AssertEquals(102.56m, dailyHeader.TotalHoneyFee);
			AssertEquals(3.16m, dailyHeader.TotalHarborMaintenanceFeeWaterways);
			AssertEquals(12.00m, dailyHeader.TotalInformalMerchandiseProcessingFee);
			AssertEquals(73.21m, dailyHeader.TotalRaspberryFee);
			AssertEquals(11.72m, dailyHeader.TotalPotatoFee);
			AssertEquals(59.91m, dailyHeader.TotalLimeFee);
			AssertEquals(48.10m, dailyHeader.TotalMushroomFee);
			AssertEquals(16.40m, dailyHeader.TotalWatermelonFee);
			AssertEquals(35.30m, dailyHeader.TotalSoftwoodLumberFee);
			AssertEquals(24.20m, dailyHeader.TotalBlueberryFee);

			AssertEquals(55948.83m, dailyHeader.FinalTotalDuty);
			AssertEquals(426.31m, dailyHeader.FinalTotalCVD);
			AssertEquals(203.51m, dailyHeader.FinalTotalADD);
			AssertEquals(61.84m, dailyHeader.FinalTotalCottonFee);
			AssertEquals(59673.69m, dailyHeader.FinalTotalAmountDue);
			AssertEquals("39", dailyHeader.FinalTotalNumberRevenueProducingEntriesForPrint);
			AssertEquals("1", dailyHeader.FinalTotalNumberNonRevenueProducingEntriesForPrint);
			AssertEquals(5m, dailyHeader.FinalTotalMailFee);
			AssertEquals(21.59m, dailyHeader.FinalTotalBeefFee);
			AssertEquals(91.12m, dailyHeader.FinalTotalPorkFee);
			AssertEquals(2530.93m, dailyHeader.FinalTotalMerchandiseProcessingFee);
			AssertEquals(102.56m, dailyHeader.FinalTotalHoneyFee);
			AssertEquals(3.16m, dailyHeader.FinalTotalHarborMaintenanceFeeWaterways);
			AssertEquals(10.00m, dailyHeader.FinalTotalInformalMerchandiseProcessingFee);
			AssertEquals(73.21m, dailyHeader.FinalTotalRaspberryFee);
			AssertEquals(11.72m, dailyHeader.FinalTotalPotatoFee);
			AssertEquals(59.91m, dailyHeader.FinalTotalLimeFee);
			AssertEquals(48.10m, dailyHeader.FinalTotalMushroomFee);
			AssertEquals(16.40m, dailyHeader.FinalTotalWatermelonFee);
			AssertEquals(35.30m, dailyHeader.FinalTotalSoftwoodLumberFee);
			AssertEquals(24.20m, dailyHeader.FinalTotalBlueberryFee);

			AssertEquals(3, dailyHeader.AllOrActiveLines.Count);
			var lines = dailyHeader.AllOrActiveLines.Cast<DailyStatementMessageLine>();
			var line1 = lines.FirstOrDefault(x => x.FormattedEntryNumber == "XJ5-6001889-8");
			AssertNotNull(line1);
			AssertEquals("2013576", line1.B3_BrokerReference);
			AssertEquals("01", line1.B3_EntryType);
			AssertEquals(StatementEntryStatus.Codes.Paperless, line1.B3_EntryStatus);
			AssertEquals(5234.58m, line1.EstimatedDuty);
			AssertEquals(0m, line1.EstimatedTax);
			AssertEquals(ZString.Empty, line1.IsDeferredTaxIndicator);
			AssertEquals(0m, line1.EstimatedCVD);
			AssertEquals(0m, line1.EstimatedADD);
			AssertEquals(ZString.Empty, line1.B3_EIIndicator);
			AssertEquals("6", line1.B2_PaymentType);
			AssertEquals("8888", line1.B3_EntryProcessPort);
			AssertEquals(ZString.Empty, line1.InterestForReconciliationIndicator);
			AssertEquals(26.78m, line1.UserFees);
			AssertEquals(5261.36m, line1.B3_CustomsFeesTotal);

			var line2 = lines.FirstOrDefault(x => x.FormattedEntryNumber == "XJ5-6003158-6");
			AssertNotNull(line2);
			AssertEquals("2013573", line2.B3_BrokerReference);
			AssertEquals("01", line2.B3_EntryType);
			AssertEquals(ZString.Empty, line2.B3_EntryStatus);
			AssertEquals(2356.20m, line2.EstimatedDuty);
			AssertEquals(0m, line2.EstimatedTax);
			AssertEquals(ZString.Empty, line2.IsDeferredTaxIndicator);
			AssertEquals(0m, line2.EstimatedCVD);
			AssertEquals(0m, line2.EstimatedADD);
			AssertEquals(ZString.Empty, line2.B3_EIIndicator);
			AssertEquals("6", line2.B2_PaymentType);
			AssertEquals("8888", line2.B3_EntryProcessPort);
			AssertEquals(ZString.Empty, line2.InterestForReconciliationIndicator);
			AssertEquals(66.76m, line2.UserFees);
			AssertEquals(2422.96m, line2.B3_CustomsFeesTotal);

			var line3 = lines.FirstOrDefault(x => x.FormattedEntryNumber == "XJ5-6003157-8");
			AssertNotNull(line3);
			AssertEquals("2013574", line3.B3_BrokerReference);
			AssertEquals("01", line3.B3_EntryType);
			AssertEquals(ZString.Empty, line3.B3_EntryStatus);
			AssertEquals(1668.60m, line3.EstimatedDuty);
			AssertEquals(0m, line3.EstimatedTax);
			AssertEquals(ZString.Empty, line3.IsDeferredTaxIndicator);
			AssertEquals(0m, line3.EstimatedCVD);
			AssertEquals(0m, line3.EstimatedADD);
			AssertEquals(ZString.Empty, line3.B3_EIIndicator);
			AssertEquals("6", line3.B2_PaymentType);
			AssertEquals("8888", line3.B3_EntryProcessPort);
			AssertEquals(ZString.Empty, line3.InterestForReconciliationIndicator);
			AssertEquals(47.28m, line3.UserFees);
			AssertEquals(1715.88m, line3.B3_CustomsFeesTotal);

			AssertEquals(5, dailyHeader.DailyDeletedLines.Count);
			var dLines = dailyHeader.DailyDeletedLines.Cast<StatementDeletedEntry>();
			var dLine1 = dLines.FirstOrDefault(x => x.FormattedEntryNumber == "2013531-6");
			AssertNotNull(dLine1);
			AssertEquals("ABI", dLine1.B3_DeletedByParty);
			var dLine2 = dLines.FirstOrDefault(x => x.FormattedEntryNumber == "2013542-3");
			AssertNotNull(dLine2);
			AssertEquals("ABI", dLine2.B3_DeletedByParty);
			var dLine3 = dLines.FirstOrDefault(x => x.FormattedEntryNumber == "2013553-0");
			AssertNotNull(dLine3);
			AssertEquals("CBP", dLine3.B3_DeletedByParty);
			var dLine4 = dLines.FirstOrDefault(x => x.FormattedEntryNumber == "2013564-7");
			AssertNotNull(dLine4);
			AssertEquals("CBP", dLine4.B3_DeletedByParty);
			var dLine5 = dLines.FirstOrDefault(x => x.FormattedEntryNumber == "2013575-3");
			AssertNotNull(dLine5);
			AssertEquals("ABI", dLine5.B3_DeletedByParty);
		}

		public void TestImporter()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DailyStatement;
			message.EM_MessageSubType = "STM";
			message.EM_MessageText = @"B001101SV9PFP1119204000072319358-123456789                                      Q11101SV9  73004911  58-1234567890723190000002920000000000000 B00176016      01 Q21101SV9  73004911 0000000000000000000000           3Y      17100000000        QA014990000000262250100000000125                                                Q31119204000  072319SV9  58-1234567890000002920000000000000000000000001101      Q4000000000000000000000000000031947000000000000000100000                        QE014990000000262250100000000125                                                Y  1101SV9PF00006";

			var organizaiton1 = Factory.New<OrgHeader>();
			organizaiton1.OH_Code = "ORG1";
			organizaiton1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "58-123456789");
			var organizaiton2 = Factory.New<OrgHeader>();
			organizaiton1.OH_Code = "ORG2";
			organizaiton1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "58-123456789");

			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_StatementType = StatementTypeList.Codes.ACE;

			var dailyHeader = new DailyStatementMessageHeader(statementHeader, message);
			AssertEquals(organizaiton1.PK, dailyHeader.Importer.PK);

			statementHeader.B2_OH_Importer = organizaiton2.PK;
			AssertEquals(organizaiton2.PK, dailyHeader.Importer.PK);
		}

		public void TestACEDailyStatementMessageProcessing()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DailyStatement;
			message.EM_MessageSubType = "STM";
			message.EM_MessageText =
				@"B018888XJ5PFF3901138000050207919-262203600                                      " +
				"Q18887XJ5  00000360  13-1479270000518150000023500000000000000NB00001160      01 " +
				"Q24904SV9  71009847 0001100003300002200000           2Y      453                " +
				"QA010530000001732005400000000320055000000189200560000002912005700000087320      " +
				"Q34915138000  051815SV9              0000023500000098000000000450000004904      " +
				"Q4000013000030000310000033000252320           0000100000                        " +
				"QE010790000003742009000000000740102000000149201030000002812010400000083320      " +
				"Q54915138000  051815SV9112233        0000023500000002200000000780000004904      " +
				"Q6000120000033000011000023000252320           0000100000                        " +
				"QJ011050000003142010600000000710107000000141201080000002811010900000083120      " +
				"Q78804091002XJ5  20135316ABIXJ5  20135530ABIXJ5  20135647CBP                    " +
				"Y  4904SV9PF00006                                                               ";

			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_StatementType = StatementTypeList.Codes.ACE;
			statementHeader.B2_ProcessDate = ZDateTime.Now.AddDays(1);
			statementHeader.B2_PrintDate = ZDateTime.Now.AddDays(2);
			statementHeader.B2_BranchDesignation = "BD";
			statementHeader.B2_EntryFilerCode = "XJ5";
			statementHeader.B2_ProcessPort = "8888";
			statementHeader.B2_ImporterCustomsID = "19-262203600";

			var dailyHeader = new DailyStatementMessageHeader(statementHeader, message);
			AssertEquals(statementHeader.B2_ProcessDate, dailyHeader.B2_ProcessDate);
			AssertEquals(statementHeader.B2_PrintDate, dailyHeader.B2_PrintDate);
			AssertEquals("BD", dailyHeader.B2_BranchDesignation);
			AssertEquals("XJ5", dailyHeader.B2_EntryFilerCode);
			AssertEquals("8888", dailyHeader.B2_ProcessPort);
			AssertEquals("19-262203600", dailyHeader.B2_ImporterCustomsID);

			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryNum = "00000360";
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_BrokerReference = "B00001160";
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, 12m);

			dailyHeader = new DailyStatementMessageHeader(null, message);
			AssertEquals("EDI CUSTOMS BROKERS", dailyHeader.StatementFor);
			AssertEquals(ZDateTime.Empty, dailyHeader.B2_ProcessDate);
			AssertEquals(new ZDateTime(2007, 05, 02), dailyHeader.B2_PrintDate);
			AssertEquals("", dailyHeader.B2_BranchDesignation);
			AssertEquals("XJ5", dailyHeader.B2_EntryFilerCode);
			AssertEquals("8888", dailyHeader.B2_ProcessPort);
			AssertEquals("19-262203600", dailyHeader.B2_ImporterCustomsID);

			dailyHeader = new DailyStatementMessageHeader(statementHeader, message);
			CombineAssertions(() =>
			{
				AssertEquals("TotalADD", 13000.03m, dailyHeader.TotalADD);
				AssertEquals("TotalAmountDue", 330002523.2m, dailyHeader.TotalAmountDue);
				AssertEquals("TotalBlueberryFee", 0m, dailyHeader.TotalBlueberryFee);
				AssertEquals("TotalCVD", 31000m, dailyHeader.TotalCVD);
				AssertEquals("TotalDeferredTax", 450000m, dailyHeader.TotalDeferredTax);
				AssertEquals("TotalHassAvocadoFee", 0m, dailyHeader.TotalHassAvocadoFee);
				AssertEquals("TotalLimeFee", 149.2m, dailyHeader.TotalLimeFee);
				AssertEquals("TotalMangoFee", 0m, dailyHeader.TotalMangoFee);
				AssertEquals("TotalPayableTax", 980000m, dailyHeader.TotalPayableTax);
				AssertEquals("TotalSheepFee", 0m, dailyHeader.TotalSoftwoodLumberFee);
				AssertEquals("TotalSorghumFee", 0m, dailyHeader.TotalSorghumFee);
			});

			CombineAssertions(() =>
			{
				AssertEquals("FinalTotalADD", 120000.03m, dailyHeader.FinalTotalADD);
				AssertEquals("FinalTotalLimeFee", 0m, dailyHeader.FinalTotalLimeFee);
				AssertEquals("FinalTotalBlueberryFee", 7.1m, dailyHeader.FinalTotalBlueberryFee);
				AssertEquals("FinalTotalAmountDue", 230002523.2m, dailyHeader.FinalTotalAmountDue);
				AssertEquals("FinalTotalMushroomFee", 0m, dailyHeader.FinalTotalMushroomFee);
				AssertEquals("FinalTotalPayableTax", 22000m, dailyHeader.FinalTotalPayableTax);
				AssertEquals("FinalTotalPotatoFee", 0m, dailyHeader.FinalTotalPotatoFee);
				AssertEquals("FinalTotalSheepFee", 314.2m, dailyHeader.FinalTotalSoftwoodLumberFee);
				AssertEquals("FinalTotalSorghumFee", 831.2m, dailyHeader.FinalTotalSorghumFee);
				AssertEquals("FinalTotalSugarFee", 0m, dailyHeader.FinalTotalSugarFee);
				AssertEquals("FinalTotalWatermelonFee", 0m, dailyHeader.FinalTotalWatermelonFee);
			});

			var lines = dailyHeader.AllOrActiveLines.Cast<DailyStatementMessageLine>();
			AssertEquals(1, lines.Count());
			var line1 = lines.First();

			AssertEquals("2", line1.B2_PaymentType);
			AssertEquals("B00001160", line1.B3_BrokerReference);
			AssertEquals(3892m, line1.B3_CustomsFeesTotal);
			AssertEquals(ZString.Empty, line1.B3_EIIndicator);
			AssertEquals("8887", line1.B3_EntryProcessPort);
			AssertEquals(StatementEntryStatus.Codes.Paperless, line1.B3_EntryStatus);
			AssertEquals("01", line1.B3_EntryType);
			AssertEquals("453", line1.B3_Team);
			AssertEquals("XJ5-0000036-0", line1.FormattedEntryNumber);
			AssertEquals("N", line1.IsDeferredTaxIndicator);
			AssertEquals(1542m, line1.UserFees);
			AssertEquals(110000.33m, line1.EstimatedADD);
			AssertEquals(22000m, line1.EstimatedCVD);
			AssertEquals(2350m, line1.EstimatedDuty);

			AssertEquals(3, dailyHeader.DailyDeletedLines.Count);
			var dLines = dailyHeader.DailyDeletedLines.Cast<StatementDeletedEntry>();
			var dLine1 = dLines.FirstOrDefault(x => x.FormattedEntryNumber == "2013531-6");
			AssertNotNull(dLine1);
			AssertEquals("ABI", dLine1.B3_DeletedByParty);
			var dLine2 = dLines.FirstOrDefault(x => x.FormattedEntryNumber == "2013553-0");
			AssertNotNull(dLine2);
			AssertEquals("ABI", dLine2.B3_DeletedByParty);
			var dLine3 = dLines.FirstOrDefault(x => x.FormattedEntryNumber == "2013564-7");
			AssertNotNull(dLine3);
			AssertEquals("CBP", dLine3.B3_DeletedByParty);
		}

		public void TestInterestAmountForReconciliationSummary()
		{
			var header = Factory.New<CusStatementHeader>();

			var bq1 = new DSTQ1();
			bq1.EntryFilerCode = "XXX";
			bq1.EntryNumber = "12345";
			bq1.BrokerReferenceNumber = "C12345";

			var docLine = new DailyStatementMessageLine(header, bq1);
			AssertEquals(ZDecimal.Zero, docLine.InterestAmountForReconciliationSummary);

			var line = header.ActiveLines.AddNew();
			line.B3_BrokerReference = "DEC12345";
			line.B3_EntryNum = "12345";
			line.B3_EntryFilerCode = "XXX";
			line.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, 12m);
			AssertEquals(12m, docLine.InterestAmountForReconciliationSummary);
		}

		public void TestB3_BrokerReference()
		{
			var header = Factory.New<CusStatementHeader>();
			var line = header.ActiveLines.AddNew();
			line.B3_BrokerReference = "DEC12345";
			line.B3_EntryNum = "12345";
			line.B3_EntryFilerCode = "XXX";

			var bq1 = new DSTQ1();
			bq1.EntryFilerCode = "XXX";
			bq1.EntryNumber = "12345";
			bq1.BrokerReferenceNumber = "C12345";

			var docLine = new DailyStatementMessageLine(header, bq1);
			AssertEquals("DEC12345", docLine.B3_BrokerReference);

			var acebq1 = new ADSTQ1();
			acebq1.EntryFilerCode = "XXX";
			acebq1.EntryNumber = "12345";
			acebq1.BrokerReferenceNumber = "C12345";

			docLine = new DailyStatementMessageLine(header, acebq1);
			AssertEquals("DEC12345", docLine.B3_BrokerReference);
		}

		public void TestB3_EntryStatusACE()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.DailyStatement;
			message.EM_MessageSubType = "STM";
			message.EM_MessageText = "B011904AFJQRP1914139ABE0519146                1704AFJ  1                        " +
"Q11904AFJ 191617310000000279000000000000 00000000000000000000004IA004404      11" +
"Q21904AFJ 19161731605191463-1175468000000000000000000000000Y  00000000000P Y    " +
"QA1904AFJ 19161731000000000000000000000000000000000000000000000000020000000000  " +
"Q31904AFJ1914139ABE0519140000000279000000000000000000000000000000000000000000000" +
"Q41904AFJ00000000000000000000000000000000000000000000000000029900000100000      " +
"QE1904AFJ000000000000000000000000000000000000000000000000000000000000000200     " +
"QF1904AFJ000000000000000000000000000000000000000000000000000000000000000        " +
"QG1904AFJ000000000000000000000000000000000000                                   " +
"Y  1904AFJQR00008";

			var dailyHeader = new DailyStatementMessageHeader(Factory.New<CusStatementHeader>(), message);

			var lines = dailyHeader.AllOrActiveLines.Cast<DailyStatementMessageLine>();
			var line1 = lines.FirstOrDefault(x => x.FormattedEntryNumber == "AFJ-1916173-1");
			AssertNotNull(line1);

			AssertEquals("Should print ACE", "A", line1.B3_EntryStatus);
		}

		public void TestB3_CustomsFeesTotalIncludesEstimatedTaxIfNotDeferred()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.DailyStatement;
			message.EM_MessageSubType = "STM";
			message.EM_MessageText = "B014601286QRF46130875280328132            12  0401286  1                        " +
				"Q14601286 665253090000001786100000080135N0000000000000000000000OS0001901 12   01" +
				"Q24601286 66525309203281304-3148236000000000000000000000000Y  00000000000P   126" +
				"QA4601286 66525309000000000000000000004301000000000000155200000000000000000000  " +
				"Q14601286 665255980000004314200000204272N0000000000000000000000OS0001928 12   01" +
				"Q24601286 66525598203281304-3148236000000000000000000000000Y  00000000000P      " +
				"QA4601286 66525598000000000000000000009976000000000000360000000000000000000000  " +
				"Q3460128646130875280328130000006100300000284407000000000000000000000000000000000" +
				"Q4460128600000000000000000000000000000000000000000000000003648390000200000      " +
				"QE4601286000000000000000000000000000000014277000000000000005152000000000000     " +
				"QF4601286000000000000000000000000000000000000000000000000000000000000000        " +
				"QG4601286000000000000000000000000000000000000                                   " +
				"Q5460128646130875280328130000006100300000284407000000000000000000000000000000000" +
				"Q6460128600000000000000000000000000000000000000000000000003648390000200000      " +
				"QJ4601286000000000000000000000000000000014277000000000000005152000000000000     " +
				"QK4601286000000000000000000000000000000000000000000000000000000000000000        " +
				"QL4601286000000000000000000000000000000000000                                   " +
				"Y  4601286QR00016";

			var statementHeader = Factory.New<CusStatementHeader>();
			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryNum = "66525309";
			statementLine.B3_EntryFilerCode = "286";
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, 12m);
			var statementLine2 = statementHeader.StatementLines.AddNew();
			statementLine2.B3_EntryNum = "66525598";
			statementLine2.B3_EntryFilerCode = "286";
			statementLine2.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, 12m);

			var dailyHeader = new DailyStatementMessageHeader(statementHeader, message);
			var lines = dailyHeader.AllOrActiveLines.Cast<DailyStatementMessageLine>();
			var line1 = lines.FirstOrDefault(x => x.FormattedEntryNumber == "286-6652530-9");
			AssertNotNull(line1);

			AssertEquals("01", line1.B3_EntryType);
			AssertEquals(StatementEntryStatus.Codes.Paperless, line1.B3_EntryStatus);
			AssertEquals(178.61m, line1.EstimatedDuty);
			AssertEquals(801.35m, line1.EstimatedTax);
			AssertEquals("N", line1.IsDeferredTaxIndicator);
			AssertEquals(0m, line1.EstimatedCVD);
			AssertEquals(0m, line1.EstimatedADD);
			AssertEquals(ZString.Empty, line1.B3_EIIndicator);
			AssertEquals("I", line1.InterestForReconciliationIndicator);
			AssertEquals(70.53m, line1.UserFees);
			AssertEquals(1050.49m, line1.B3_CustomsFeesTotal);
			AssertEquals("126", line1.B3_Team);

			var line2 = lines.FirstOrDefault(x => x.FormattedEntryNumber == "286-6652559-8");
			AssertNotNull(line2);
			AssertEquals("01", line2.B3_EntryType);
			AssertEquals(431.42m, line2.EstimatedDuty);
			AssertEquals(2042.72m, line2.EstimatedTax);
			AssertEquals("N", line2.IsDeferredTaxIndicator);
			AssertEquals(0m, line2.EstimatedCVD);
			AssertEquals(0m, line2.EstimatedADD);
			AssertEquals(147.76m, line2.UserFees);
			AssertEquals(2621.90m, line2.B3_CustomsFeesTotal);
		}
	}
}
