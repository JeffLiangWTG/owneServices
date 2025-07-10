using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses;
using NUnit.Framework;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	public class SARSFormulaHelperTests
	{
		[TestCase("001", "1001", "12A", "FREE", "0", "", "(C0001)")]
		[TestCase("002", "1001", "6P2", "FULL DUTY", "12A+12B", "", "(C0002)")]
		[TestCase("003", "1216", "12A", "3,817C/LI", "0.03817 * [LI]", "LI", "(C0005)")]
		[TestCase("004", "1216", "12A", "R115.08/LI AA", "115.08 * [LA]", "LA", "(C0005)")]
		[TestCase("005", "1216", "12A", "R7.05/10STICKS", "0.705 * [NO]", "NO", "(C0005)")]
		[TestCase("006", "1216", "12A", "R9.39/10CIGARETTES", "0.939 * [NO]", "NO", "(C0005)")]
		[TestCase("007", "1216", "2", "103 736C/U", "1037.36 * [NO]", "NO", "(C0005)")]
		[TestCase("008", "1216", "3", "FULL DUTY LESS 0,091C/LI", "MAX(1P1 - (0.00091 * [LI]), 0)", "LI", "(C0003)")]
		[TestCase("009", "1216", "6P3", "17,466C/LI", "0.17466 * [LI]", "LI", "(C0005)")]
		[TestCase("010", "1302", "2", "113,25%", "1.1325 * VFD", "", "(C0007)")]
		[TestCase("011", "1302", "3", "FULL DUTY LESS 10%", "MAX(1P1 - (0.1 * VFD), 0)", "", "(C0006)")]
		[TestCase("012", "1352", "3", "NOT EXCEEDING 15%", "MIN(1P1, 0.15 * VFD)", "", "(C0008)")]
		[TestCase("013", "1354", "4", "FULL DUTY IN PART 1 OF SCHEDULE NO. 1 LESS 20%", "MAX((1P1 - 0.2 * VFD), 0)", "", "(C0009)")]
		[TestCase("014", "1354", "4", "THE DUTY IN PART 1 OF SCHEDULE NO. 1 LESS 119,4%", "MAX((1P1 - 1.194 * VFD), 0)", "", "(C0009)")]
		[TestCase("015", "1556", "3", "FULL DUTY LESS THE DUTY IN SECTION B OF PART 2 OF SCHEDULE NO. 1", "1P1", "", "(C0010)")]
		[TestCase("016", "1564", "4", "FULL DUTY LESS THE DUTY IN SECTION A OF PART 2 OF SCHEDULE NO. 1", "1P1+0+12B+13A+13B+13C+13D+13E+15A+15B", "", "(C0011)")]
		[TestCase("017", "1600", "13F", "R120.00 /T CO<SUB>2</SUB>E EMISSIONS", "Carbon Emissions for ZA Factories, paid on eFiling only", "", "(C0012)")]
		[TestCase("018", "3408", "1P1", "15% OR 130C/KG", "MAX(0.15 * VFD, 1.3 * [KG])", "KG", "(C0013)")]
		[TestCase("019", "3410", "1P1", "10% OR 55C/KG LESS 90%", "MAX(0.1 * VFD, MAX((0.55 * [KG] - 0.9 * VFD), 0))", "KG", "(C0014)")]
		[TestCase("020", "3423", "1P1", "860C/KG LESS 85% WITH A MAXIMUM OF 44%", "MIN(MAX((8.6 * [KG] - 0.85 * VFD), 0), 0.44 * VFD)", "KG", "(C0015)")]
		[TestCase("021", "3425", "1P1", "0,08C/LI WITH A MAXIMUM OF 6,4%", "MIN(0.0008 * [LI], 0.064 * VFD)", "LI", "(C0016)")]
		[TestCase("022", "3436", "12B", "(SEE NOTE 1 TO THIS PART)", "MIN(MAX((ROUND(0.00003 * VFD, 3) - 0.75), 0) * VFD/100, 0.3 * VFD)", "", "(C0017)")]
		[TestCase("023", "3440", "13D", "R120.00 PER G/KM CO? EMISSIONS EXCEEDING 95G/KM", "120 * MAX(([GK] - 95), 0)", "GK", "(C0019)")]
		[TestCase("024", "4555", "4", "FULL DUTY LESS THE AMOUNT OF ANY REBATE, REFUND AND DRAWBACK GRANTED PREVIOUSLY AND LESS THE DUTY ON THE COST OF PROCESSING OR REPAIR", "{\"Rebate Amount\"}", "", "(C0020)")]
		[TestCase("025", "4558", "4", "NOT EXCEEDING THE DUTY IN SECTION B OF PART 2 OF SCHEDULE NO. 1", "{\"Rebate Amount\"}", "", "(C0021)")]
		[TestCase("026", "4559", "61G", "FULL DUTY LESS THE DUTY PAID ON ENTRY", "{\"Rebate Amount\"}", "", "(C0022)")]
		[TestCase("027", "4560", "4", "FULL DUTY LESS THE AMOUNT OF ANY REBATE, REFUND AND DRAWBACK GRANTED PREVIOUSLY", "{\"Rebate Amount\"}", "", "(C0023)")]
		[TestCase("028", "4567", "4", "FULL DUTY IN PART 1 OF SCHEDULE NO. 1", "1P1", "", "(C0024)")]
		[TestCase("029", "4567", "4", "FULL DUTY IN PART 2 OF SCHEDULE NO. 3", "3P2", "", "(C0024)")]
		[TestCase("030", "4570", "17A", "2,21C/GRAM OF THE SUGAR CONTENT THAT EXCEEDS 4G/100ML", "[LI] * 10 * (0.0221 * MAX(([GJ] - 4), 0))", "GJ", "(C0025)")]
		[TestCase("031", "9999", "1P8", "THE RATE OF DUTY REFERRED TO IN RESPECT OF VEHICLES OF HEADING 87.03 IN PART 1 AND 2 OF SCHEDULE NO. 1", "1P1+12A+12B", "", "(C0027)")]
		[TestCase("032", "9999", "1P8", "THE RATE OF DUTY SPECIFIED IN RESPECT OF THOSE GOODS IN PARTS 1 AND 2OF SCHEDULE NO. 1", "1P1+12A+12B", "", "(C0026)")]
		[TestCase("033", "9999", "3", "FULL ANTI-DUMPING DUTY", "2P1+2P2+2P3", "", "(C0028)")]
		[TestCase("034", "9999", "3", "FULL DUTY LESS THE DUTY PAYABLE ON THE VALUE CALCULATED IN TERMS OF NOTE 8.1", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("035", "9999", "3", "THE FULL ANTI-DUMPING DUTY", "2P1+2P2+2P3", "", "(C0028)")]
		[TestCase("036", "9999", "4", "(A)  IN RESPECT OF A MOTOR VEHICLE DESCRIBED IN PARAGRAPH (I)(A):FULLDUTY; OR(B)  IN RESPECT OF A MOTOR VEHICLE DECRIBED IN PARAGRAPH (I)(B):FULL DUTY LESS THE DUTY CALCULATED PRO RATA ON A DAILY BAS", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("037", "9999", "4", "FULL DUTY IN SCHEDULE NO. 1 AND SCHEDULE NO. 2", "1P1+2P1+2P2+2P3", "", "(C0035)")]
		[TestCase("038", "9999", "4", "FULL FUEL LEVY AND ROAD ACCIDENT FUND LEVY", "15A+15B", "", "(C0036)")]
		[TestCase("039", "9999", "4", "FULL SAFEGUARD DUTY", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("040", "9999", "4", "NOT EXCEEDING THE DUTIES CALCULATED IN TERMS OF THE NOTES TO THIS REBATE ITEM", "{\"Rebate Amount\"}", "", "(C0030)")]
		[TestCase("041", "9999", "4", "NOT EXCEEDING THE DUTY APPLICABLE TO SUCH GOODS IN PART 1 OF SCHEDULENO. 1 CALCULATED ON THE VALUE REFLECTED ON THE PRCCNO. 1 CALCULATED ON THE VALUE REFLECTED ON THE PRCC", "{\"Rebate Amount\"}", "", "(C0033)")]
		[TestCase("042", "9999", "4", "NOT EXCEEDING THE DUTY APPLICABLE TO SUCH GOODS IN PART 1 OF SCHEDULENO. 1 REDUCED TO THE EXTENT OF THE AMOUNT REFLECTED ON THE PRCNO. 1 REDUCED TO THE EXTENT OF THE AMOUNT REFLECTED ON THE PRC", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("043", "9999", "4", "NOT EXCEEDING THE DUTY IN EXCESS OF THE AMOUNT OF DUTY THAT WOULD HAVE BEEN DUE HAD THE GOODS BEEN IMPORTED IN A SINGLE CONSIGNMENT", "{\"Rebate Amount\"}", "", "(C0034)")]
		[TestCase("044", "9999", "4", "THE DUTY IN PART 2A OF SCHEDULE NO. 1", "12A", "", "(C0037)")]
		[TestCase("045", "9999", "4", "THE FULL ANTI-DUMPING DUTY", "2P1+2P2+2P3", "", "(C0028)")]
		[TestCase("046", "9999", "5", "AS DETERMINED AND APPROVED BY THE DIRECTOR-GENERAL: DEPARTMENT OF INTERNATIONAL RELATIONS AND CO-OPERATION", "{\"Drawback Amount\"}", "", "(C0042)")]
		[TestCase("047", "9999", "5", "NOT EXCEEDING DUTY PAYABLE PER QUARTER FOR EXCISE DUTY PURPOSE", "MIN((1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B), {\"Duty payable per quarter for Excise duty purposes\"})", "", "(C0038)")]
		[TestCase("048", "9999", "5", "NOT EXCEEDING THE DUTIES CALCULATED IN TERMS OF THE NOTES TO THIS REBATE ITEM", "{\"Rebate Amount\"}", "", "(C0030)")]
		[TestCase("049", "9999", "5", "NOT EXCEEDING THE DUTY APPLICABLE TO SUCH GOODS IN PART 1 OF SCHEDULENO. 1 CALCULATED ON THE VALUE REFLECTED ON ANY PRCC ISSUED IN THE NAME OF THE IMPORTER", "{\"Rebate Amount\"}", "", "(C0032)")]
		[TestCase("050", "9999", "5", "NOT EXCEEDING THE DUTY APPLICABLE TO SUCH GOODS IN PART 1 OF SCHEDULENO. 1 REDUCED TO THE EXTENT OF THE AMOUNT REFLECTED ON THE PRC ISSUEDIN THE NAME OF THE IMPORTER", "{\"Drawback Amount\"}", "", "(C0042)")]
		[TestCase("051", "9999", "5", "NOT EXCEEDING THE DUTY IN PART 1 OF SCHEDULE NO. 1 CALCULATED ON THE VALUE REFLECTED ON THE PRCC ISSUED IN THE NAME OF THE IMPORTER AND SUBJECT TO THE NOTE TO THIS ITEM", "{\"Rebate Amount\"}", "", "(C0031)")]
		[TestCase("052", "9999", "61D", "AS PROVIDED IN NOTE 8 TO THIS SECTION", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("053", "9999", "61E", "AS PROVIDED IN NOTE 4 TO THIS SECTION", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("054", "9999", "61F", "1,209C/LI SPIRITS IN THE MIXTURE", "0.01209 * [LI]", "LI", "(C0005/C0039)")]
		[TestCase("055", "9999", "61F", "1,409C/LI SPIRITS IN THE MIXTURE", "0.01409 * [LI]", "LI", "(C0005/C0039)")]
		[TestCase("056", "9999", "61F", "AS DETERMINED AND APPROVED BY THE DIRECTOR-GENERAL: DEPARTMENT OF INTERNATIONAL RELATIONS AND CO-OPERATION", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("057", "9999", "61F", "AS PROVIDED IN THE NOTES HERETO", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("058", "9999", "61G", "FULL DUTY NOT REBATED", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("059", "9999", "6P3", "AS DETERMINED AND APPROVED BY THE DIRECTOR-GENERAL: DEPARTMENT OF INTERNATIONAL RELATIONS AND CO-OPERATION", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("060", "9999", "6P3", "AS PROVIDED IN NOTE 10 READ WITH NOTE 13", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("061", "9999", "6P3", "AS PROVIDED IN NOTE 6 HERETO", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("062", "9999", "6P3", "AS PROVIDED IN NOTE 7 READ WITH NOTE 13", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("063", "9999", "6P3", "FULL FUEL LEVY AND ROAD ACCIDENT FUND LEVY SUBJECT TO NOTE 13", "15A+15B", "", "(C0036)")]
		[TestCase("064", "9999", "6P3", "FULL FUEL LEVY AND ROAD ACCIDENT FUND LEVY", "15A+15B", "", "(C0036)")]
		[TestCase("065", "9999", "1P1", "NOT-GOING-TO-MATCH", "NOT-GOING-TO-MATCH", "", "(C0045)")]
		[TestCase("067", "3440", "13D", "R132.00 PER G/KM CO� EMISSIONS EXCEEDING 95G/KM", "132 * MAX(([GK] - 95), 0)", "GK", "(C0019)")]
		[TestCase("068", "3440", "13D", "R195.00 PER G/KM CO<SUB>2</SUB> EMISSIONS EXCEEDING 175G/KM", "195 * MAX(([GK] - 175), 0)", "GK", "(C0019)")]

		// Original Tests
		[TestCase("102", "1001", "S1P1", "FREE", "0", "", "(C0001)")]
		[TestCase("103", "1001", "61A", "FULL DUTY", "12A+12B", "", "(C0002)")]
		[TestCase("104", "1001", "S6P2", "FULL DUTY", "12A+12B", "", "(C0002)")]
		[TestCase("105", "1001", "S6P4", "FULL DUTY", "12A+12B", "", "(C0002)")]
		[TestCase("106", "1001", "S4P5", "FULL DUTY", "13A+13B+13C+13D+13E", "", "(C0002)")]
		[TestCase("107", "1001", "S6P3", "FULL DUTY", "15A+15B", "", "(C0002)")]
		[TestCase("108", "1001", "S3P1", "FULL DUTY", "1P1", "", "(C0002)")]
		[TestCase("109", "1001", "S3P2", "FULL DUTY", "1P1", "", "(C0002)")]
		[TestCase("110", "1001", "S4P1", "FULL DUTY", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", "", "(C0002)")]
		[TestCase("111", "1001", "S4P2", "FULL DUTY", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", "", "(C0002)")]
		[TestCase("112", "1001", "S4P3", "FULL DUTY", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", "", "(C0002)")]
		[TestCase("113", "1001", "S4P4", "FULL DUTY", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", "", "(C0002)")]
		[TestCase("114", "1001", "S4P6", "FULL DUTY", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", "", "(C0002)")]
		[TestCase("115", "1216", "S1P1", "0,091C/LI", "0.00091 * [LI]", "LI", "(C0005)")]
		[TestCase("116", "1216", "S1P1", "3,5C/KW.H", "0.035 * [KW]", "KW", "(C0005)")]
		[TestCase("117", "1216", "S1P1", "5,5C/KG", "0.055 * [KG]", "KG", "(C0005)")]
		[TestCase("118", "1216", "S1P3A", "6C/BAG", "0.06 * [NO]", "NO", "(C0005)")]
		[TestCase("119", "1216", "S1P1", "8C/KG", "0.08 * [KG]", "KG", "(C0005)")]
		[TestCase("120", "1216", "S1P1", "24C/KG NET", "0.24 * [KN]", "KN", "(C0005)")]
		[TestCase("121", "1216", "S1P1", "50C/U", "0.5 * [NO]", "NO", "(C0005)")]
		[TestCase("122", "1216", "S1P1", "R6.21/10CIGARETTES", "0.621 * [NO]", "NO", "(C0005)")]
		[TestCase("123", "1216", "S1P1", "R7.05/10STICKS", "0.705 * [NO]", "NO", "(C0005)")]
		[TestCase("124", "1216", "S1P1", "R6.21/8KG", "0.77625 * [KG]", "KG", "(C0005)")]
		[TestCase("125", "1216", "S1P1", "R145.20/KG NET", "145.2 * [KN]", "KN", "(C0005)")]
		[TestCase("126", "1216", "S1P1", "R2.87/LI", "2.87 * [LI]", "LI", "(C0005)")]
		[TestCase("127", "1216", "S1P1", "2 843C/KG", "28.43 * [KG]", "KG", "(C0005)")]
		[TestCase("128", "1216", "S1P1", "R28.43/KG", "28.43 * [KG]", "KG", "(C0005)")]
		[TestCase("129", "1216", "S1P1", "317C/LA", "3.17 * [LA]", "LA", "(C0005)")]
		[TestCase("130", "1216", "S1P1", "R3 012.17/KG NET", "3012.17 * [KN]", "KN", "(C0005)")]
		[TestCase("131", "1216", "S1P1", "400c/LAMP", "4 * [NO]", "NO", "(C0005)")]
		[TestCase("132", "1216", "S1P3A", "400C/LAMP", "4 * [NO]", "NO", "(C0005)")]
		[TestCase("133", "1216", "S1P3A", "400C/M?", "4 * [SM]", "SM", "(C0005)")]
		[TestCase("134", "1216", "S1P1", "562C/M", "5.62 * [ME]", "ME", "(C0005)")]
		[TestCase("135", "1216", "S1P1", "R5.83C/KG", "5.83 * [KG]", "KG", "(C0005)")]
		[TestCase("136", "1216", "S1P1", "R79.26/LI AA", "79.26 * [LA]", "LA", "(C0005)")]
		[TestCase("137", "1216", "S3P1", "FULL DUTY LESS 56C/KG", "MAX(1P1 - (0.56 * [KG]), 0)", "KG", "(C0003)")]
		[TestCase("138", "1216", "S4P1", "FULL DUTY LESS 56C/KG", "MAX(1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B - (0.56 * [KG]), 0)", "KG", "(C0003)")]
		[TestCase("139", "1216", "S6P3", "FULL FUEL LEVY LESS 7,5 C/LI AND FULL ROAD ACCIDENT FUND LEVY", "MAX(MAX(15A - (0.075 * [LI]), 0) + 15B, 0)", "LI", "(C0004)")]
		[TestCase("140", "1302", "12B", "9,4%", "0.094 * VFD", "", "(C0007)")]
		[TestCase("141", "1302", "S1P1", "9,4%", "0.094 * VFD", "", "(C0007)")]
		[TestCase("142", "1302", "S1P1", "31%", "0.31 * VFD", "", "(C0007)")]
		[TestCase("143", "1302", "S3P1", "FULL DUTY LESS 15%", "MAX(1P1 - (0.15 * VFD), 0)", "", "(C0006)")]
		[TestCase("144", "1302", "S4P1", "FULL DUTY LESS 13,5%", "MAX(1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B - (0.135 * VFD), 0)", "", "(C0006)")]
		[TestCase("145", "1352", "S3P1", "NOT EXCEEDING 15%", "MIN(1P1, 0.15 * VFD)", "", "(C0008)")]
		[TestCase("146", "1354", "S4P1", "THE DUTY IN PART 1 OF SCHEDULE NO. 1 LESS 14,6%", "MAX((1P1 - 0.146 * VFD), 0)", "", "(C0009)")]
		[TestCase("147", "1354", "S4P1", "THE DUTY IN PART 1 OF SCHEDULE NO. 1 LESS 119,4%", "MAX((1P1 - 1.194 * VFD), 0)", "", "(C0009)")]
		[TestCase("148", "1354", "S4P1", "THE DUTY IN PART 2 OF SCHEDULE NO. 1 LESS 55,4%", "MAX((1P2 - 0.554 * VFD), 0)", "", "(C0009)")]
		[TestCase("149", "1556", "S5P1", "FULL DUTY LESS THE DUTY IN SECTION B OF PART 2 OF SCHEDULE NO. 1", "1P1+12A+0+13A+13B+13C+13D+13E+15A+15B", "", "(C0010)")]
		[TestCase("150", "1556", "S4P1", "FULL DUTY LESS THE DUTY IN SECTION A OF PART 3 OF SCHEDULE NO. 1", "1P1+12A+12B+0+13B+13C+13D+13E+15A+15B", "", "(C0010)")]
		[TestCase("151", "1564", "S3P1", "FULL DUTY LESS THE DUTY IN SECTION A OF PART 2 OF SCHEDULE NO. 1", "1P1", "", "(C0011)")]
		[TestCase("152", "1600", "S1P3", "R127.00 /T CO<SUB>2</SUB>E EMISSIONS", "Carbon Emissions for ZA Factories, paid on eFiling only", "", "(C0012)")]
		[TestCase("153", "3408", "S1P1", "7,5% OR 7,25C/2U", "MAX(0.075 * VFD, 0.0725 * [PR])", "PR", "(C0013)")]
		[TestCase("154", "3408", "S1P1", "30% OR 2 500C/2U", "MAX(0.3 * VFD, 25 * [PR])", "PR", "(C0013)")]
		[TestCase("155", "3408", "S1P1", "40% OR 240C/KG", "MAX(0.4 * VFD, 2.4 * [KG])", "KG", "(C0013)")]
		[TestCase("156", "3410", "S1P1", "7,5% OR 7,25C/2U LESS 85%", "MAX(0.075 * VFD, MAX((0.0725 * [PR] - 0.85 * VFD), 0))", "PR", "(C0014)")]
		[TestCase("157", "3410", "S1P1", "25% OR R7.20C/LI LESS 7,5%", "MAX(0.25 * VFD, MAX((7.2 * [LI] - 0.075 * VFD), 0))", "LI", "(C0014)")]
		[TestCase("158", "3423", "S1P1", "110C/KG LESS 80% WITH A MAXIMUM OF 37%", "MIN(MAX((1.1 * [KG] - 0.8 * VFD), 0), 0.37 * VFD)", "KG", "(C0015)")]
		[TestCase("159", "3425", "S1P1", "450C/KG WITH A MAXIMUM OF 96%", "MIN(4.5 * [KG], 0.96 * VFD)", "KG", "(C0016)")]
		[TestCase("160", "3425", "S1P1", "R8.51C/KG WITH A MAXIMUM OF 7,5%", "MIN(8.51 * [KG], 0.075 * VFD)", "KG", "(C0016)")]
		[TestCase("161", "3436", "12A", "{(0,00003 X A) - 0,75}% WITH A MAXIMUM OF 30% (SEE NOTE 1 TO THIS PART", "MIN(MAX((ROUND(0.00003 * VFD, 3) - 0.75), 0) * VFD/100, 0.3 * VFD)", "", "(C0018)")]
		[TestCase("162", "3436", "12B", "{(0,00003 X B) - 0,75}% WITH A MAXIMUM OF 30% (SEE NOTE 2 TO THIS PART", "MIN(MAX((ROUND(0.00003 * VFD, 3) - 0.75), 0) * VFD/100, 0.3 * VFD)", "", "(C0018)")]
		[TestCase("163", "3440", "S1P3", "R90.00 PER G/KM CO  EMISSIONS EXCEEDING 120G/KM", "90 * MAX(([GK] - 120), 0)", "GK", "(C0019)")]
		[TestCase("164", "3440", "S1P3", "R90.00 PER G/KM CO?  EMISSIONS EXCEEDING 120G/KM", "90 * MAX(([GK] - 120), 0)", "GK", "(C0019)")]
		[TestCase("165", "4555", "S4P1", "FULL DUTY LESS THE AMOUNT OF ANY REBATE, REFUND AND DRAWBACK GRANTED PREVIOUSLY AND LESS THE DUTY ON THE COST OF PROCESSING OR REPAIR", "{\"Rebate Amount\"}", "", "(C0020)")]
		[TestCase("166", "4558", "S4P1", "NOT EXCEEDING THE DUTY IN SECTION B OF PART 2 OF SCHEDULE NO. 1", "{\"Rebate Amount\"}", "", "(C0021)")]
		[TestCase("167", "4559", "S4P1", "FULL DUTY LESS THE DUTY PAID ON ENTRY", "{\"Rebate Amount\"}", "", "(C0022)")]
		[TestCase("168", "4559", "S6P2", "FULL DUTY LESS THE DUTY PAID ON ENTRY", "{\"Rebate Amount\"}", "", "(C0022)")]
		[TestCase("169", "4560", "S4P1", "FULL DUTY LESS THE AMOUNT OF ANY REBATE, REFUND AND DRAWBACK GRANTED PREVIOUSLY", "{\"Rebate Amount\"}", "", "(C0023)")]
		[TestCase("170", "4567", "S1P1", "FULL DUTY IN PART 1 OF SCHEDULE NO. 1", "1P1", "", "(C0024)")]
		[TestCase("171", "4570", "17A", "5.5c/GRAM OF THE SUGAR CONTENT THAT EXCEEDS 4G/100ML", "[LI] * 10 * (0.055 * MAX(([GJ] - 4), 0))", "GJ", "(C0025)")]
		[TestCase("172", "9999", "5P1", "AS DETERMINED AND APPROVED BY THE DIRECTOR-GENERAL  DEPARTMENT OF INTERNATIONAL RELATIONS AND CO-OPERATION", "{\"Drawback Amount\"}", "", "(C0042)")]
		[TestCase("173", "9999", "6P4", "AS PROVIDED IN THE NOTES HERETO", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("174", "9999", "3", "FULL DUTY LESS THE DUTY PAYABLE ON THE VALUE CALCULATED IN TERMS OF NOTE 29", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("175", "9999", "3", "FULL DUTY LESS THE GREATER OF 25% OR 23C/M", "{\"Rebate Amount\"}", "", "(C0029)")]
		[TestCase("176", "9999", "61G", "FULL DUTY NOT REBATED", "{\"Rebate Amount\"}", "", "(C0041)")]
		[TestCase("177", "9999", "4", "NOT EXCEEDING THE DUTIES CALCULATED IN TERMS OF THE NOTES TO THIS REBATE ITEM", "{\"Rebate Amount\"}", "", "(C0030)")]
		[TestCase("178", "9999", "5P1", "NOT EXCEEDING THE DUTY APPLICABLE TO SUCH GOODS IN PART 1 OF SCHEDULENO. 1 CALCULATED ON THE VALUE REFLECTED ON ANY PRCC ISSUED IN THE NAMEOFTHE IMPORTER", "{\"Rebate Amount\"}", "", "(C0032)")]
		[TestCase("179", "9999", "4", "NOT EXCEEDING THE DUTY APPLICABLE TO SUCH GOODS IN PART 1 OF SCHEDULENO. 1 CALCULATED ON THE VALUE REFLECTED ON THE PRCC", "{\"Rebate Amount\"}", "", "(C0033)")]
		[TestCase("180", "9999", "4", "NOT EXCEEDING THE DUTY AS CALCULATED IN TERMS OF THE NOTES TO THIS REBATE ITEM", "{\"Rebate Amount\"}", "", "(C0030)")]
		[TestCase("181", "9999", "4", "NOT EXCEEDING THE DUTY IN EXCESS OF THE AMOUNT OF DUTY THAT WOULD HAVEBEEN DUE HAD THE GOODS BEEN IMPORTED IN A SINGLE CONSIGNMENT", "{\"Rebate Amount\"}", "", "(C0034)")]
		[TestCase("182", "9999", "4", "NOT EXCEEDING THE DUTY IN PART 1 OF SCHEDULE NO. 1 CALCULATED ON THE VALUE REFLECTED ON THE IMPORT REBATE CREDIT CERTIFICATES ISSUED IN THENAME OF THE IMPORTER AND SUBJECT TO THE NOTE TO THIS ITEM", "{\"Rebate Amount\"}", "", "(C0031)")]
		[TestCase("183", "9999", "5P1", "NOT EXCEEDING THE DUTY IN PART 1 OF SCHEDULE NO. 1 CALCULATED ON THE VALUE REFLECTED ON THE IMPORT REBATE CREDIT CERTIFICATES ISSUED IN THENAME OF THE IMPORTER AND SUBJECT TO THE NOTE TO THIS ITEM", "{\"Rebate Amount\"}", "", "(C0031)")]
		[TestCase("184", "9999", "5P1", "NOT EXCEEDING THE DUTY IN PART 1 OF SCHEDULE NO. 1 CALCULATED ON THE VALUE REFLECTED ON THE PRCC ISSUED IN THE NAME OF THE IMPORTER AND SUBJECT TO THE NOTE TO THIS ITEM", "{\"Rebate Amount\"}", "", "(C0031)")]
		[TestCase("185", "9999", "5P2", "AS DETERMINED AND APPROVED BY THE DIRECTOR-GENERAL  DEPARTMENT OF INTERNATIONAL RELATIONS AND CO-OPERATION", "{\"Refund Amount\"}", "", "(C0043)")]
		[TestCase("186", "9999", "5P4", "AS DETERMINED AND APPROVED BY THE DIRECTOR-GENERAL  DEPARTMENT OF INTERNATIONAL RELATIONS AND CO-OPERATION", "{\"Refund Amount\"}", "", "(C0043)")]
		[TestCase("187", "9999", "5P6", "AS DETERMINED AND APPROVED BY THE DIRECTOR-GENERAL  DEPARTMENT OF INTERNATIONAL RELATIONS AND CO-OPERATION", "{\"Refund or Drawback Amount\"}", "", "(C0044)")]
		[TestCase("188", "9999", "5P5", "AS DETERMINED AND APPROVED BY THE DIRECTOR-GENERAL  DEPARTMENT OF INTERNATIONAL RELATIONS AND CO-OPERATION", "{\"Refund or Drawback Amount\"}", "", "(C0044)")]
		[TestCase("189", "9999", "61F", "1,209C/LI SPIRITS IN THE MIXTURE", "0.01209 * [LI]", "LI", "(C0005/C0039)")]
		[TestCase("190", "9999", "4", "THE DUTY IN PART 2A OF SCHEDULE NO. 1", "12A", "", "(C0037)")]
		[TestCase("191", "9999", "6", "THE DUTY IN PART 2A OF SCHEDULE NO. 1", "12A", "", "(C0037)")]
		[TestCase("192", "9999", "4", "FULL FUEL LEVY AND ROAD ACCIDENT FUND LEVY SUBJECT TO NOTE 13", "15A+15B", "", "(C0036)")]
		[TestCase("193", "9999", "61G", "FULL FUEL LEVY AND ROAD ACCIDENT FUND LEVY", "15A+15B", "", "(C0036)")]
		[TestCase("194", "9999", "1P8", "THE RATE OF DUTY REFERRED TO IN RESPECT OF VEHICLES OF HEADING 87.03 IN PART 1 AND 2 OF SCHEDULE NO. 1", "1P1+12A+12B", "", "(C0027)")]
		[TestCase("195", "9999", "1P8", "THE RATE OF DUTY SPECIFIED IN RESPECT OF THOSE GOODS IN PARTS 1 AND 2OF SCHEDULE NO. 1", "1P1+12A+12B", "", "(C0026)")]
		[TestCase("196", "9999", "4", "FULL DUTY IN SCHEDULE NO. 1 AND SCHEDULE NO. 2", "1P1+2P1+2P2+2P3", "", "(C0035)")]
		[TestCase("197", "9999", "S3", "THE FULL ANTI-DUMPING DUTY", "2P1+2P2+2P3", "", "(C0028)")]
		[TestCase("198", "9999", "5P1", "NOT EXCEEDING DUTY PAYABLE PER QUARTER FOR EXCISE DUTY PURPOSE", "MIN((1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B), {\"Duty payable per quarter for Excise duty purposes\"})", "", "(C0038)")]
		[TestCase("199", "9999", "12B", "(SEE NOTE 1 TO THIS PART)", "MIN(MAX((ROUND(0.00003 * VFD, 3) - 0.75), 0) * VFD/100, 0.3 * VFD)", "", "(C0040)")]
		[TestCase("200", "9999", "1P1", "xcvxcvxds", "XCVXCVXDS", "", "(C0045)")]
		public void GetFormulaData(string testId, string formulaCode, string scheduleType, string description, string expectedFormula, string expectedUOM, string expectedDescriptionPrefix)
		{
			if (scheduleType.Length == 1)
			{
				scheduleType += "P1";
			}

			if (scheduleType.StartsWith("S"))
			{
				scheduleType = scheduleType.Substring(1);
			}

			var tariff = new TariffData { Schedule = SARSSchedule.Get(scheduleType, "") };
			var rate = new Rate { RateType = RateTypes.Standard, FormulaCode = formulaCode, Description = description };

			var matched = SARSFormulaHelper.PopulateFormulaData(tariff, rate);

			Assert.That(rate.Formula, Is.EqualTo(expectedFormula), $"{testId} - Formula");
			Assert.That(rate.UnitOfMeasureConverted, Is.EqualTo(expectedUOM), $"{testId} - UnitOfMeasure");
			Assert.That(rate.Description, Is.EqualTo(description), $"{testId} - Description");
			Assert.That(matched, Is.EqualTo(true), $"{testId} - Not matched");
		}

		[Test]
		public void UniqueFormulaCodes()
		{
			var formulas = SARSFormulaHelperForTest.GetAllFormulas();

			var uniqueCodes = formulas.GroupBy(x => new { x.regCode }).Select(g => new { Code = g.Key, Count = g.Count(), Data = g.ToList() }).ToList();

			var duplicates = uniqueCodes.Where(x => x.Count > 1).ToList();

			var errorMessage = string.Join("\r\n", duplicates.Select(x => $"Code: {x.Code.regCode}:\r\n\t{string.Join("\r\n\t", x.Data.Select(d => $"{d.methodName}:{d.regEx}"))}]"));

			Assert.That(duplicates.Any(), Is.EqualTo(false), errorMessage);
		}

		[Test]
		public void AllFormulasAreTested()
		{
			var formulas = SARSFormulaHelperForTest.GetAllFormulas();
			var tested = formulas.ToDictionary(x => x.regCode, x => false);

			var type = System.Reflection.Assembly.GetExecutingAssembly().GetType("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.SARSFormulaHelperTests", true);
			var method = type.GetMethod("GetFormulaData", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			var attribs = method.GetCustomAttributes(typeof(TestCaseAttribute), false);

			foreach (TestCaseAttribute attrib in attribs)
			{
				if (attrib.Arguments.Length == 7)
				{
					var codesParam = (string)attrib.Arguments[6];

					codesParam = codesParam.Replace("(", "").Replace(")", "").Replace("C", "");
					var codes = codesParam.Split('/').Select(x => Convert.ToInt32(x, CultureInfo.InvariantCulture));

					foreach (var code in codes)
					{
						tested[code] = true;
					}
				}
			}

			Assert.That(tested.Any(x => !x.Value), Is.EqualTo(false), "The follow codes have not been tested: " + string.Join(", ", tested.Where(x => !x.Value).Select(x => $"{x.Key}")));
		}

		[Test]
		public void AllFormulasHaveRegexStartAndEndTags()
		{
			var formulas = SARSFormulaHelperForTest.GetAllFormulas();

			var notValid = formulas.Where(x => !x.regEx.StartsWith("^") || !x.regEx.EndsWith("$")).ToList();

			Assert.That(notValid.Any(), Is.EqualTo(false), $"The following formulas need ^ or $: \n {string.Join(", ", notValid.Select(x => $"{x.regCode}"))}");
		}
	}
}
