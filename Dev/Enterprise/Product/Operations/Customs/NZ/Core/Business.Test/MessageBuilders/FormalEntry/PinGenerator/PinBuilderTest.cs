using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.PinGenerator.Testing
{
	public class PinBuilderTest : TestCaseWithFactory
	{
		public void TestSampleWithSGGSegments()
		{
			TestAgainstMessageStringFromVFP(SampleSGG);
		}

		#region SampleBulkOilWithACCAndPFMLSegments

		const string SampleSGG = @"UNH+453+CUSDEC:D:96B:UN+77111177
BGM+929+B00159814+5
CST++10:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+41+NZAKL
DTM+151:20130301:102
GIS+PDO:110:143
MEA+WT+AAD+KGM:10
FTX+AAI+++FF
RFF+ABU:9896
PAC+2++CT
TDT+20++5
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009908C:ZZZ:143
UNS+D
DMS+7+935
TOD+++FOB:106:143
CST+1+8415101021L:169:143
FTX+AAA+++GREENHOUSE
LOC+27+CN
LOC+35+AU
MEA+AAR++NMB:100.000
MEA+AAS++KNS:50.000
NAD+SU+00710841Y:ZZZ:143
MOA+14:2500.00:NZD
CUX+2++1.00
MOA+40:2500
MOA+64:150
MOA+70:15
GIS+Y:109:143
TAX+1+CST+SGG:167:143
MOA+161:294.00
TAX+1+CUD
MOA+161:125.00
TAX+1+GST
MOA+161:462.60
UNS+S
CNT+4:1
CNT+5:1
CNT+11:2
TAX+3+CST+SGG:167:143
MOA+161:294.00
TAX+3+CUD++2500
MOA+161:125.00
TAX+3+GST
MOA+161:462.60
TAX+4+TOT
MOA+161:881.60
GIS+C:134:143
AUT+MAHCLIAAMDNLGNMO+65432198B
UNT+52+453
";
		#endregion

		public void TestSampleBulkOilWithACCAndPFMLSegments()
		{
			TestAgainstMessageStringFromVFP(SampleBulkOilWithACCAndPFMLSegments);
		}
		#region SampleBulkOilWithACCAndPFMLSegments
		const string SampleBulkOilWithACCAndPFMLSegments = @"UNH+714+CUSDEC:D:96B:UN+56307688
BGM+929+B00001267+5
CST++10:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+41+NZAKL
DTM+151:20081017:102
GIS+PDO:110:143
MEA+WT+AAD+KGM:530000
FTX+AAI+++JAWOHL
RFF+BM:BULKAS OIL
PAC+1++VL
TDT+20+121+1+++++:::BUNGA DELIMA
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+PUMPIT+935
TOD+++FOB:106:143
CST+1+2710192110C:169:143
FTX+AAA+++MOTOR SPIRIT; (NOT FOR MANUF.) IN BULK IN SHIPS ETC RON LESS THAN 95:(REGULAR GRADE) NOT BLENDED WITH ETHYL ALCOHOL
LOC+27+US
LOC+35+AU
MEA+AAR++LTR:500000.000
NAD+SU+00710841Y:ZZZ:143
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:22059
MOA+70:233
GIS+N:109:143
TAX+1+CST+ACC:167:143
MOA+161:46700.00
TAX+1+CST+PFML:167:143
MOA+161:250.00
TAX+1+CUD
MOA+161:212620.00
TAX+1+GST
MOA+161:36482.75
CST+2+2710191300C:169:143
FTX+AAA+++MOTOR SPIRIT; (NOT FOR MANUF.) IN BULK IN SHIPS ETC. RON LESS THAN 95:(REGULAR GRADE) BLENDED WITH ETHYL ALCOHOL
LOC+27+AU
LOC+35+AU
MEA+AAR++LMS:10000.000
MEA+AAS++LTR:10000.000
NAD+SU+00710841Y:ZZZ:143
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:22059
MOA+70:233
GIS+N:109:143
TAX+1+CST+PFML:167:143
MOA+161:5.00
TAX+1+CUD
MOA+161:4252.40
TAX+1+GST
MOA+161:4568.68
UNS+S
CNT+4:1
CNT+5:2
CNT+11:1
TAX+3+CST+ACC:167:143
MOA+161:46700.00
TAX+3+CST+PFML:167:143
MOA+161:255.00
TAX+3+CUD++20000
MOA+161:216872.40
TAX+3+GST
MOA+161:41051.43
TAX+4+TOT
MOA+161:304878.83
GIS+B:134:143
AUT+@KEJMJ@NOMMJHFH@+65432198B
UNT+74+714
";
		#endregion

		public void TestForSealNumbers()
		{
			TestAgainstMessageStringFromVFP(SampleEDIFACTWithSealNumbers);
		}
		#region SampleEDIFACTWithSealNumbers

		const string SampleEDIFACTWithSealNumbers = @"UNH+12821+CUSDEC:D:96B:UN
BGM+830+115301+9
CST++40:105:143
LOC+9+NZAKL
LOC+11+AUSYD
LOC+41+NZAKL
LOC+28+AU
DTM+129:20040101:102
GIS+S:112:143
GIS+SEP:110:143:VERYSECURE
MEA+WT+AAD+KGM:1000
EQD+CN+OOCL0000006++++5
SEL+SEALME1
RFF+BM:MASTERTBILL1
PAC+0++PK
RFF+BM:HOUSEBILL1
RFF+AAQ:OOCL0000006
PAC+100++PK
TDT+20+898+1+++++:::BUNGA BIDARA
NAD+AL+12421356J:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
MOA+14:10000.00:NZD
CUX+2++1.00+N
UNS+S
CNT+4:0
CNT+5:1
CNT+11:100
AUT+FKOKFNAHEGDKOOAM+65432198B
UNT+33+12821
";
		#endregion

		public void TestFor2SealNumbers()
		{
			TestAgainstMessageStringFromVFP(SampleWith2SealNumbers);
		}
		#region SampleWith2SealNumbers

		const string SampleWith2SealNumbers = @"UNH+12822+CUSDEC:D:96B:UN
BGM+830+115301+9
CST++40:105:143
LOC+9+NZAKL
LOC+11+AUSYD
LOC+41+NZAKL
LOC+28+AU
DTM+129:20040101:102
GIS+S:112:143
GIS+SEP:110:143:VERYSECURE
MEA+WT+AAD+KGM:1000
EQD+CN+OOCL0000006++++5
SEL+SEALME1
EQD+CN+OOCL0000011++++5
SEL+SEALME2
RFF+BM:MASTERTBILL1
PAC+0++PK
RFF+BM:HOUSEBILL1
RFF+AAQ:OOCL0000006
PAC+50++PK
RFF+BM:HOUSEBILL1
RFF+AAQ:OOCL0000011
PAC+50++PK
TDT+20+898+1+++++:::BUNGA BIDARA
NAD+AL+12421356J:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
MOA+14:10000.00:NZD
CUX+2++1.00+N
UNS+S
CNT+4:0
CNT+5:1
CNT+11:100
AUT+KIBCEOKENMHG@HCL+65432198B
UNT+38+12822
";
		#endregion

		#region Tests for Internals of PIN Generation
		[ExpectNoExceptions()]
		public void TestGenerateBlocksFromEDIFACTMessageTextDoesntBarfOnASillyMessage()
		{
			pinBuilder.GenerateBlocksFromEDIFACTMessageText(SampleEDIFACTMessage.Replace("A", "!#^&!$%)*&#$%#$+++:::+++:::"));
		}

		public void TestGenerateBlocksFromEDIFACTMessageText()
		{
			pinBuilder.GenerateBlocksFromEDIFACTMessageText(SampleEDIFACTMessage.Replace("\r\n", "'"));
			AssertMultilineEquals("Debug Blocks Should Match", SampleEDIFACTMessageDebugBlock, pinBuilder.GetPINDebugText(), '\n');
			AssertEquals("IMFCEHFMC@CMMNDE", pinBuilder.GetMAC());
		}
		#region SampleEDIFACTMessage
		const string SampleEDIFACTMessage = @"UNH+12820+CUSDEC:D:96B:UN
BGM+929+115300+9
CST++10:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20040101:102
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL+12421356J:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
LOC+35+AU
NAD+SU+00710841Y:ZZZ:143
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:1000
MOA+70:10
GIS+N:109:143
TAX+1+CUD
MOA+161:650.00
TAX+1+GST
MOA+161:1457.50
UNS+S
CNT+4:1
CNT+5:1
CNT+11:100
TAX+3+CUD++10000
MOA+161:650.00
TAX+3+GST
MOA+161:1457.50
TAX+4+TOT
MOA+161:2107.50
GIS+B:134:143
AUT+IMFCEHFMC@CMMNDE+65432198B
UNT+46+12820
";
		#endregion
		#region SampleEDIFACTMessageDebugBlock
		const string SampleEDIFACTMessageDebugBlock = @"class entry    =[929]
client_ref     =[115300]
tran type      =[9]
entry type     =[10]
port loading   =[AUSYD]
port discharge =[NZAKL]
customs control=[1234Z]
processing port=[NZAKL]
date import    =[20040101]
other info  1  =[]
other data  1  =[]
other info  2  =[]
other data  2  =[]
other info  3  =[]
other data  3  =[]
other info  4  =[]
other data  4  =[]
other info  5  =[]
other data  5  =[]
other info  6  =[]
other data  6  =[]
other info  7  =[]
other data  7  =[]
other info  8  =[]
other data  8  =[]
other info  9  =[]
other data  9  =[]
other info 10  =[]
other data 10  =[]
total weight   =[1000]
block_a=[929115300910AUSYDNZAKL1234ZNZAKL200401011000    ]
block_b=[]
block_c number =0
reference type q   =[MB]
reference number   =[08111111111]
container type q   =[]
container number   =[]
container status   =[]
number packages    =[]
type packages      =[]
seal numbers       =[]
block_c=[MB08111111111   ]
block_c number =1
reference type q   =[HWB]
reference number   =[HOUSEBILL1]
container type q   =[]
container number   =[]
container status   =[]
number packages    =[100]
type packages      =[PK]
seal numbers       =[]
block_c=[HWBHOUSEBILL1100PK      ]
voyage_number      =[]
transport mode     =[4]
craft flight no    =[QF117]
permit auth code 1 =[]
permit auth no 1   =[]
permit auth code 2 =[]
permit auth no 2   =[]
permit auth code 3 =[]
permit auth no 3   =[]
permit auth code 4 =[]
permit auth no 4   =[]
permit auth code 5 =[]
permit auth no 5   =[]
permit auth code 6 =[]
permit auth no 6   =[]
permit auth code 7 =[]
permit auth no 7   =[]
permit auth code 8 =[]
permit auth no 8   =[]
permit auth code 9 =[]
permit auth no 9   =[]
permit auth code10 =[]
permit auth no10   =[]
block_d=[4QF117  ]
client code  =[12421356J]
client name  =[]
broker code  =[00009917B]
del auth code=[]
block_e=[12421356J00009917B      ]
invoice number =[1001001]
invoice terms  =[FOB]
block_f=[1001001FOB      ]
line_number         =[1]
tariff item         =[8301100000F]
cons code           =[]
goods descr         =[PADLOCKS]
country origin      =[AU]
country export      =[AU]
stat unit           =[]
stat qty            =[]
supp unit           =[]
supp qty            =[]
supplier code       =[00710841Y]
supplier name       =[]
value in curr       =[10000.00]
curr code           =[NZD]
value in nz         =[10000]
freight             =[1000]
insurance           =[10]
exch rate           =[1.00]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[N]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
acc levy            =[]
pfml levy           =[]
sgg levy            =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[650.00]
duty credit         =[]
gst                 =[1457.50]
pref ind            =[]
block_g=[18301100000FPADLOCKSAUAU00710841Y10000.00NZD100001000101.00N650.001457.50       ]
total_invoices      =[1]
total_lines         =[1]
total_packages      =[100]
total_alac          =[]
total_hera          =[]
total_acc           =[]
total_pfml          =[]
total_sgg           =[]
total_anti dump     =[]
total_countervailing=[]
total_tariff duty   =[650.00]
total_value in nz   =[10000]
total_gst           =[1457.50]
total_amount        =[2107.50]
total_duty credits  =[]
method of payment   =[B]
declarant code      =[65432198B]
block_h=[11100650.00100001457.502107.50B65432198B]
pbuf=[929115300910AUSYDNZAKL1234ZNZAKL200401011000    MB08111111111   HWBHOUSEBILL1100PK      4QF117  12421356J00009917B      1001001FOB      18301100000FPADLOCKSAUAU00710841Y10000.00NZD100001000101.00N650.001457.50       11100650.00100001457.502107.50B65432198B]
length of pbuf = 256
Looking up [65432198B] in database
aut_result=    [IMFCEHFMC@CMMNDE]
mes_aut_result=[IMFCEHFMC@CMMNDE]
";
		#endregion

		public void TestGetPINDebugText()
		{
			SetupTestData();
			pinBuilder.Set_MessageMAC("GDNBHBHEMOHCFGIN");
			pinBuilder.Set_DeclarantCode("65432198B");
			AssertMultilineEquals("These SHOULD match, but they.... Don't???", ExpectedPINDebugText, pinBuilder.GetPINDebugText(), '\n');
		}
		#region ExpectedPINDebugText
		const string ExpectedPINDebugText = @"class entry    =[929]
client_ref     =[111161]
tran type      =[9]
entry type     =[10]
port loading   =[AUSYD]
port discharge =[NZAKL]
customs control=[7294L]
processing port=[NZAKL]
country of dest=[]
date export    =[]
date import    =[20040621]
entry period   =[]
override ind   =[]
sold_ind       =[]
other info  1  =[]
other data  1  =[]
other info  2  =[]
other data  2  =[]
other info  3  =[]
other data  3  =[]
other info  4  =[]
other data  4  =[]
other info  5  =[]
other data  5  =[]
other info  6  =[]
other data  6  =[]
other info  7  =[]
other data  7  =[]
other info  8  =[]
other data  8  =[]
other info  9  =[]
other data  9  =[]
other info 10  =[]
other data 10  =[]
total weight   =[1000]
block_a=[929111161910AUSYDNZAKL7294LNZAKL200406211000    ]
block_b=[]
block_c number =0
reference type q    =[BM]
reference number    =[HOUSEJOHN]
container type q   =[AAQ]
container number   =[OOCL0000006]
container status   =[7]
number packages    =[100]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHOUSEJOHNAAQOOCL00000067100PK ]
block_c number =1
reference type q    =[BM]
reference number    =[HOUSEJOHN]
container type q   =[AAQ]
container number   =[OOCL0000006]
container status   =[7]
number packages    =[50]
type packages      =[BA]
seal numbers       =[]
block_c=[BMHOUSEJOHNAAQOOCL0000006750BA  ]
voyage_number      =[2828]
transport mode     =[1]
craft flight no    =[ANTWERP]
permit auth code 1 =[]
permit auth no 1   =[]
permit auth code 2 =[]
permit auth no 2   =[]
permit auth code 3 =[]
permit auth no 3   =[]
permit auth code 4 =[]
permit auth no 4   =[]
permit auth code 5 =[]
permit auth no 5   =[]
permit auth code 6 =[]
permit auth no 6   =[]
permit auth code 7 =[]
permit auth no 7   =[]
permit auth code 8 =[]
permit auth no 8   =[]
permit auth code 9 =[]
permit auth no 9   =[]
permit auth code10 =[]
permit auth no10   =[]
block_d=[28281ANTWERP    ]
client code  =[00782903F]
client name  =[]
broker code  =[00009917B]
del auth code=[00727575H]
block_e=[00782903F00009917B00727575H     ]
invoice number =[INVOICEJOHN]
invoice terms  =[FOB]
block_f=[INVOICEJOHNFOB  ]
line_number         =[1]
tariff item         =[4203101900G]
cons code           =[]
goods descr         =[LEATHER JACKETS]
country origin      =[AU]
country export      =[AU]
stat unit           =[NMB]
stat qty            =[100.000]
supp unit           =[]
supp qty            =[]
supplier code       =[00710841Y]
supplier name       =[]
value in curr       =[5000.00]
curr code           =[NZD]
value in nz         =[5000]
freight             =[50]
insurance           =[5]
exch rate           =[1.00]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[N]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[1715.00]
import duty         =[]
duty credit         =[]
gst                 =[846.25]
pref ind            =[]
block_g=[14203101900GLEATHER JACKETSAUAUNMB100.00000710841Y5000.00NZD50005051.00N1715.00846.25   ]
line_number         =[2]
tariff item         =[3402900011H]
cons code           =[]
goods descr         =[DEGREASER]
country origin      =[AU]
country export      =[AU]
stat unit           =[KGM]
stat qty            =[500.000]
supp unit           =[]
supp qty            =[]
supplier code       =[00710841Y]
supplier name       =[]
value in curr       =[5000.00]
curr code           =[NZD]
value in nz         =[5000]
freight             =[50]
insurance           =[5]
exch rate           =[1.00]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[N]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[350.00]
import duty         =[]
duty credit         =[]
gst                 =[675.63]
pref ind            =[]
block_g=[23402900011HDEGREASERAUAUKGM500.00000710841Y5000.00NZD50005051.00N350.00675.63  ]
total_invoices      =[1]
total_lines         =[2]
total_packages      =[150]
total_alac          =[]
total_hera          =[]
total_anti dump     =[]
total_countervailing=[]
total_tariff duty   =[2065.00]
total_value in nz   =[10000]
total_gst           =[1521.88]
total_amount        =[3586.88]
total_duty credits  =[]
method of payment   =[B]
declarant code      =[65432198B]
block_h=[121502065.00100001521.883586.88B65432198B       ]
pbuf=[929111161910AUSYDNZAKL7294LNZAKL200406211000    BMHOUSEJOHNAAQOOCL00000067100PK BMHOUSEJOHNAAQOOCL0000006750BA  28281ANTWERP    00782903F00009917B00727575H     INVOICEJOHNFOB  14203101900GLEATHER JACKETSAUAUNMB100.00000710841Y5000.00NZD50005051.00N1715.00846.25   23402900011HDEGREASERAUAUKGM500.00000710841Y5000.00NZD50005051.00N350.00675.63  121502065.00100001521.883586.88B65432198B       ]
length of pbuf = 392
Looking up [65432198B] in database
aut_result=    [GDNBHBHEMOHCFGIN]
mes_aut_result=[GDNBHBHEMOHCFGIN]
";
		#endregion

		public void TestGeneratePBUF()
		{
			SetupTestData();
			AssertEquals(expectedPBUF, pinBuilder.GetPBUF());
		}

		public void TestEmptyPBUFReturnsErrorString()
		{
			AssertEquals("PF.NO.PBUF", pinBuilder.GetMAC());
		}

		public void TestByteConverter()
		{
			char[] source = new char[4];
			source[0] = 'D';
			source[1] = 'C';
			source[2] = 'B';
			source[3] = 'A';
			long destLong = 0;
			for (int counter = 0; counter < 4; counter++)
			{
				destLong += Convert.ToInt16(source[counter]) * Convert.ToInt32(Math.Pow(256d, Convert.ToDouble(counter)));
			}

			long sourceLong = destLong;
			char[] dest = new char[4];
			for (int counter = 3; counter >= 0; counter--)
			{
				long powerOf256 = Convert.ToInt32(Math.Pow(256d, Convert.ToDouble(counter)));
				int fff = Convert.ToInt16(destLong / powerOf256);
				dest[counter] = Convert.ToChar(fff);
				destLong = destLong % (powerOf256);
			}

			string sourceString = Convert.ToString(source);
			string destString = Convert.ToString(dest);

			AssertEquals(sourceString, destString);
		}

		public void TestNZSignExternalDLL()
		{
			string dataBlock = "929111161910AUSYDNZAKL7294LNZAKL200406211000    BMHOUSEJOHNAAQOOCL00000067100PK BMHOUSEJOHNAAQOOCL0000006750BA  28281ANTWERP    00782903F00009917B00727575H     INVOICEJOHNFOB  14203101900GLEATHER JACKETSAUAUNMB100.00000710841Y5000.00NZD50005051.00N1715.00846.25   23402900011HDEGREASERAUAUKGM500.00000710841Y5000.00NZD50005051.00N350.00675.63  121502065.00100001521.883586.88B65432198B       ";
			string pinCode = CurrentUsersPin.TestSystemPinCode;
			string expectedResult = "GDNBHBHEMOHCFGIN";
			StringBuilder result = new StringBuilder(16);
			PinBuilder.NZSign(dataBlock, pinCode, pinCode, result, null);
			AssertEquals(expectedResult, result.ToString());
		}

		public void TestGenerateMAC()
		{
			SetupTestData();
			string expectedResult = "GDNBHBHEMOHCFGIN";
			AssertEquals(expectedResult, pinBuilder.GetMAC());
		}

		#region TestOverflowingPermitsOtherInfosAndProhibitedCodes
		public void TestOverflowingPermitsOtherInfosAndProhibitedCodes()
		{
			TestClientFailures(MessageCodeOverflow, DebugTextCodeOverflow);
		}
		#region Message
		const string MessageCodeOverflow = @"UNH+4042+CUSDEC:D:96B:UN'
BGM+929+S00076831+9'
CST++11:105:143'
LOC+9+DEBRV'
LOC+11+NZAKL'
LOC+41+NZAKL'
DTM+151:20050903:102'
GIS+NP1:110:143'
GIS+NS2:110:143'
GIS+NP3:110:143'
GIS+NS4:110:143'
GIS+NP5:110:143'
GIS+NS6:110:143'
GIS+NP7:110:143'
GIS+NS8:110:143'
GIS+NP9:110:143'
GIS+NS0:110:143'
GIS+NPA:110:143'
GIS+NSB:110:143'
MEA+WT+AAD+KGM:587'
EQD+CN+MAEU8346677++++7'
RFF+BM:HAM083503'
PAC+0++PK'
RFF+BM:55002000102'
RFF+AAQ:MAEU8346677'
PAC+2++PK'
TDT+20+517+1+++++:::NELE MAERSK'
DOC+AF1:147:143+00400186201'
DOC+AF2:147:143+00400186202'
DOC+AF3:147:143+00400186203'
DOC+AF4:147:143+00400186204'
DOC+AF5:147:143+00400186205'
DOC+AF6:147:143+00400186206'
DOC+AF7:147:143+00400186207'
DOC+AF8:147:143+00400186208'
DOC+AF9:147:143+00400186209'
DOC+AF0:147:143+00400186210'
NAD+AL+:ZZZ:143+LENE DALSGARD'
NAD+CB+00303221D:ZZZ:143'
UNS+D'
DMS+BAGGAGE+935'
TOD+++FOB:106:143'
CST+1+9805003000J:169:143'
FTX+AAA+++W1-LEGAL AUTH TO WORK IN NZ 12 MTHS OR GREATER'
LOC+27+DK'
LOC+35+DK'
NAD+SU+:ZZZ:143+LENE DALSGARD'
MOA+14:12500.00:NZD'
CUX+2++1.00'
MOA+40:12500'
MOA+64:1'
MOA+70:0'
DOC+AF1:147:143+00400186201'
DOC+AF2:147:143+00400186202'
DOC+AF3:147:143+00400186203'
DOC+AF4:147:143+00400186204'
DOC+AF5:147:143+00400186205'
DOC+AF6:147:143+00400186206'
DOC+AF7:147:143+00400186207'
GIS+Y:109:143'
GIS+PR1:118:143'
GIS+PR2:118:143'
GIS+PR3:118:143'
GIS+PR4:118:143'
GIS+PR5:118:143'
GIS+PE1:110:143'
GIS+PE2:110:143:DK'
GIS+PE3:110:143:DK'
GIS+PE4:110:143'
GIS+PE5:110:143'
GIS+PE6:110:143:100974122'
GIS+PE7:110:143:200418283'
UNS+S'
CNT+4:1'
CNT+5:1'
CNT+11:2'
TAX+3+CUD++12500'
MOA+161:0.00'
AUT+AALACGI@ABC@H@EL+40000052C'
UNT+49+4042'";
		#endregion
		#region DebugText
		const string DebugTextCodeOverflow = @"class entry    =[929]
client_ref     =[S00076831]
tran type      =[9]
entry type     =[11]
port loading   =[DEBRV]
port discharge =[NZAKL]
processing port=[NZAKL]
date import    =[20050903]
other info  1  =[NP1]
other data  1  =[]
other info  2  =[NS2]
other data  2  =[]
other info  3  =[NP3]
other data  3  =[]
other info  4  =[NS4]
other data  4  =[]
other info  5  =[NP5]
other data  5  =[]
other info  6  =[NS6]
other data  6  =[]
other info  7  =[NP7]
other data  7  =[]
other info  8  =[NS8]
other data  8  =[]
other info  9  =[NP9]
other data  9  =[]
other info 10  =[NS0]
other data 10  =[]
total weight   =[587]
block_a=[929S00076831911DEBRVNZAKLNZAKL20050903NP1NS2NP3NS4NP5NS6NP7NS8NP9NS0587 ]
block_b=[]
block_c number =0
reference type q   =[BM]
reference number   =[HAM083503]
container type q   =[]
container number   =[]
container status   =[]
number packages    =[0]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAM0835030PK  ]
block_c number =1
reference type q   =[BM]
reference number   =[55002000102]
container type q   =[AAQ]
container number   =[MAEU8346677]
container status   =[7]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BM55002000102AAQMAEU834667772PK ]
voyage_number      =[517]
transport mode     =[1]
craft flight no    =[NELE MAERSK]
permit auth code 1 =[AF1]
permit auth no 1   =[00400186201]
permit auth code 2 =[AF2]
permit auth no 2   =[00400186202]
permit auth code 3 =[AF3]
permit auth no 3   =[00400186203]
permit auth code 4 =[AF4]
permit auth no 4   =[00400186204]
permit auth code 5 =[AF5]
permit auth no 5   =[00400186205]
permit auth code 6 =[AF6]
permit auth no 6   =[00400186206]
permit auth code 7 =[AF7]
permit auth no 7   =[00400186207]
permit auth code 8 =[AF8]
permit auth no 8   =[00400186208]
permit auth code 9 =[AF9]
permit auth no 9   =[00400186209]
permit auth code10 =[AF0]
permit auth no10   =[00400186210]
block_d=[5171NELE MAERSKAF100400186201AF200400186202AF300400186203AF400400186204AF500400186205AF600400186206AF700400186207AF800400186208AF900400186209AF000400186210     ]
client code  =[]
client name  =[LENE DALSGARD]
broker code  =[00303221D]
del auth code=[]
block_e=[LENE DALSGARD00303221D  ]
invoice number =[BAGGAGE]
invoice terms  =[FOB]
block_f=[BAGGAGEFOB      ]
line_number         =[1]
tariff item         =[9805003000J]
cons code           =[]
goods descr         =[W1-LEGAL AUTH TO WORK IN NZ 12 MTHS OR GREATER]
country origin      =[DK]
country export      =[DK]
stat unit           =[]
stat qty            =[]
supp unit           =[]
supp qty            =[]
supplier code       =[]
supplier name       =[LENE DALSGARD]
value in curr       =[12500.00]
curr code           =[NZD]
value in nz         =[12500]
freight             =[1]
insurance           =[0]
exch rate           =[1.00]
exch ind            =[]
permit auth code  1 =[AF1]
permit auth no    1 =[00400186201]
permit auth code  2 =[AF2]
permit auth no    2 =[00400186202]
permit auth code  3 =[AF3]
permit auth no    3 =[00400186203]
permit auth code  4 =[AF4]
permit auth no    4 =[00400186204]
permit auth code  5 =[AF5]
permit auth no    5 =[00400186205]
relation ind        =[Y]
proh code 1         =[PR1]
proh code 2         =[PR2]
proh code 3         =[PR3]
other info  1       =[PE1]
other data  1       =[]
other info  2       =[PE2]
other data  2       =[DK]
other info  3       =[PE3]
other data  3       =[DK]
other info  4       =[PE4]
other data  4       =[]
other info  5       =[PE5]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[]
duty credit         =[]
gst                 =[]
pref ind            =[]
block_g=[19805003000JW1-LEGAL AUTH TO WORK IN NZ 12 MTHS OR GREATERDKDKLENE DALSGARD12500.00NZD12500101.00AF100400186201AF200400186202AF300400186203AF400400186204AF500400186205YPR1PR2PR3PE1PE2DKPE3DKPE4PE5    ]
total_invoices      =[1]
total_lines         =[1]
total_packages      =[2]
total_alac          =[]
total_hera          =[]
total_anti dump     =[]
total_countervailing=[]
total_tariff duty   =[0.00]
total_value in nz   =[12500]
total_gst           =[]
total_amount        =[]
total_duty credits  =[]
method of payment   =[]
declarant code      =[40000052C]
block_h=[1120.001250040000052C   ]
pbuf=[929S00076831911DEBRVNZAKLNZAKL20050903NP1NS2NP3NS4NP5NS6NP7NS8NP9NS0587 BMHAM0835030PK  BM55002000102AAQMAEU834667772PK 5171NELE MAERSKAF100400186201AF200400186202AF300400186203AF400400186204AF500400186205AF600400186206AF700400186207AF800400186208AF900400186209AF000400186210     LENE DALSGARD00303221D  BAGGAGEFOB      19805003000JW1-LEGAL AUTH TO WORK IN NZ 12 MTHS OR GREATERDKDKLENE DALSGARD12500.00NZD12500101.00AF100400186201AF200400186202AF300400186203AF400400186204AF500400186205YPR1PR2PR3PE1PE2DKPE3DKPE4PE5    1120.001250040000052C   ]
length of pbuf = 544
Looking up [40000052C] in database
aut_result=    [LO@KAE@K@KILHKFC]
mes_aut_result=[AALACGI@ABC@H@EL]";
		#endregion
		#endregion

		#endregion

		#region Tests for Failures from Clients
		#region TestPreviousFailureDTDAKL_B00001013
		public void TestPreviousFailureDTDAKL_B00001013()
		{
			TestClientFailures(MessageDTDAKL_B00001013, DebugTextDTDAKL_B00001013);
		}
		#region MessageDTDAKL_B00001013
		const string MessageDTDAKL_B00001013 = @"UNH+33+CUSDEC:D:96B:UN+04724984'
BGM+929+B00001013+5'
CST++10:105:143'
LOC+9+CNTXG'
LOC+11+NZAKL'
LOC+41+NZAKL'
DTM+151:20050606:102'
GIS+MCD:110:143:YNNNN'
GIS+ATF:110:143:8084'
MEA+WT+AAD+KGM:24100'
EQD+CN+TTNU1558525++++5'
SEL+S051611'
FTX+AAI+++RESIGNED PIN FAILURE'
RFF+BM:MISCTXG000001762'
PAC+0++PK'
RFF+BM:MISCTXG000001762'
RFF+AAQ:TTNU1558525'
PAC+21++CS'
PAC+120++CL'
TDT+20+99+1+++++:::KOTA JUTA'
NAD+AL+00310300F:ZZZ:143'
NAD+CB+40132202H:ZZZ:143'
UNS+D'
DMS+JLHC050308+935'
TOD+++CFR:106:143'
CST+1+7215900029A:169:143+800890G:184:143'
FTX+AAA+++BLACK ANNEALED WIRE CUT TO LENGTH'
LOC+27+CN'
LOC+35+CN'
MEA+AAR++KGM:18500.000'
NAD+SU+00939902Z:ZZZ:143'
MOA+14:10338.13:USD'
CUX+2++0.71'
MOA+40:14561'
MOA+64:2246'
MOA+70:24'
GIS+N:109:143'
TAX+1+GST'
MOA+161:2103.88'
GIS+Q:116:143'
CST+2+7217900019L:169:143+800890G:184:143'
FTX+AAA+++BLACK ANNEALED WIRE IN COILS, COATED OTHER THAN NAIL'
LOC+27+CN'
LOC+35+CN'
MEA+AAR++KGM:3000.000'
NAD+SU+00939902Z:ZZZ:143'
MOA+14:1481.52:USD'
CUX+2++0.71'
MOA+40:2087'
MOA+64:322'
MOA+70:3'
GIS+N:109:143'
TAX+1+GST'
MOA+161:301.50'
GIS+Q:116:143'
CST+3+7217201015H:169:143+984797K:184:143'
FTX+AAA+++ELECTROGALVANISED STEEL WIRE COLD HEADING QUALITY, 1.6MM OR MORE IN:DIAMETER'
LOC+27+CN'
LOC+35+CN'
MEA+AAR++KGM:2500.000'
NAD+SU+00939902Z:ZZZ:143'
MOA+14:1472.85:USD'
CUX+2++0.71'
MOA+40:2074'
MOA+64:320'
MOA+70:3'
GIS+N:109:143'
TAX+1+GST'
MOA+161:299.63'
GIS+Q:116:143'
UNS+S'
CNT+4:1'
CNT+5:3'
CNT+11:141'
TAX+3+CUD++18722'
MOA+161:0.00'
TAX+3+GST'
MOA+161:2705.01'
TAX+4+TOT'
MOA+161:2705.01'
GIS+D:134:143'
AUT+@LNDFHHEBHAAAJKG+40000303D'
UNT+83+33'
";
		#endregion
		#region DebugTextDTDAKL_B00001013
		const string DebugTextDTDAKL_B00001013 = @"arg1 = M0027667966
arg2 = /appl/ediprd/ediedata/xlate/00088MUO.TL0
arg3 = T/W
arg4 = 1
arg5 = 3919
argc = 7
symbolic_dir_name=CUSMOD
class entry    =[929]
client_ref     =[B00001013]
tran type      =[5]
entry type     =[10]
port loading   =[CNTXG]
port discharge =[NZAKL]
customs control=[]
processing port=[NZAKL]
country of dest=[]
date export    =[20050606]
date import    =[]
entry period   =[]
override ind   =[]
sold_ind       =[]
other info  1  =[MCD]
other data  1  =[YNNNN]
other info  2  =[ATF]
other data  2  =[8084]
other info  3  =[]
other data  3  =[]
other info  4  =[]
other data  4  =[]
other info  5  =[]
other data  5  =[]
other info  6  =[]
other data  6  =[]
other info  7  =[]
other data  7  =[]
other info  8  =[]
other data  8  =[]
other info  9  =[]
other data  9  =[]
other info 10  =[]
other data 10  =[]
total weight   =[24100]
block_a=[929B00001013510CNTXGNZAKLNZAKL20050606MCDYNNNNATF808424100      ]
block_b=[RESIGNED PIN FAILURE    ]
block_c number =0
container type q   =[]
container number   =[]
container status   =[]
number packages    =[0]
type packages      =[PK]
seal numbers       =[]
block_c=[BMMISCTXG0000017620PK   ]
block_c number =1
container type q   =[AAQ]
container number   =[TTNU1558525]
container status   =[5]
number packages    =[21]
type packages      =[CS]
seal numbers       =[S051611]
block_c=[BMMISCTXG000001762AAQTTNU1558525521CSS051611    ]
block_c number =2
container type q   =[AAQ]
container number   =[TTNU1558525]
container status   =[5]
number packages    =[120]
type packages      =[CL]
seal numbers       =[]
block_c=[BMMISCTXG000001762AAQTTNU15585255120CL  ]
voyage_number      =[99]
transport mode     =[1]
craft flight no    =[KOTA JUTA]
permit auth code 1 =[]
permit auth no 1   =[]
permit auth code 2 =[]
permit auth no 2   =[]
permit auth code 3 =[]
permit auth no 3   =[]
permit auth code 4 =[]
permit auth no 4   =[]
permit auth code 5 =[]
permit auth no 5   =[]
permit auth code 6 =[]
permit auth no 6   =[]
permit auth code 7 =[]
permit auth no 7   =[]
permit auth code 8 =[]
permit auth no 8   =[]
permit auth code 9 =[]
permit auth no 9   =[]
permit auth code10 =[]
permit auth no10   =[]
block_d=[991KOTA JUTA    ]
client code  =[00310300F]
client name  =[]
broker code  =[40132202H]
del auth code=[]
block_e=[00310300F40132202H      ]
invoice block number 0
invoice number =[JLHC050308]
invoice terms  =[CFR]
block_f=[JLHC050308CFR   ]
line_number         =[1]
tariff item         =[7215900029A]
cons code           =[800890G]
goods descr         =[BLACK ANNEALED WIRE CUT TO LENGTH]
country origin      =[CN]
country export      =[CN]
stat unit           =[KGM]
stat qty            =[18500.000]
supp unit           =[]
supp qty            =[]
supplier code       =[00939902Z]
supplier name       =[]
value in curr       =[10338.13]
curr code           =[USD]
value in nz         =[14561]
freight             =[2246]
insurance           =[24]
exch rate           =[0.71]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[N]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[]
duty credit         =[]
gst                 =[2103.88]
pref ind            =[Q]
block_g=[17215900029A800890GBLACK ANNEALED WIRE CUT TO LENGTHCNCNKGM18500.00000939902Z10338.13USD145612246240.71N2103.88Q]
line_number         =[2]
tariff item         =[7217900019L]
cons code           =[800890G]
goods descr         =[BLACK ANNEALED WIRE IN COILS, COATED OTHER THAN NAIL]
country origin      =[CN]
country export      =[CN]
stat unit           =[KGM]
stat qty            =[3000.000]
supp unit           =[]
supp qty            =[]
supplier code       =[00939902Z]
supplier name       =[]
value in curr       =[1481.52]
curr code           =[USD]
value in nz         =[2087]
freight             =[322]
insurance           =[3]
exch rate           =[0.71]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[N]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[]
duty credit         =[]
gst                 =[301.50]
pref ind            =[Q]
block_g=[27217900019L800890GBLACK ANNEALED WIRE IN COILS, COATED OTHER THAN NAILCNCNKGM3000.00000939902Z1481.52USD208732230.71N301.50Q   ]
line_number         =[3]
tariff item         =[7217201015H]
cons code           =[984797K]
goods descr         =[ELECTROGALVANISED STEEL WIRE COLD HEADING QUALITY, 1.6MM OR MORE INDIAMETER]
country origin      =[CN]
country export      =[CN]
stat unit           =[KGM]
stat qty            =[2500.000]
supp unit           =[]
supp qty            =[]
supplier code       =[00939902Z]
supplier name       =[]
value in curr       =[1472.85]
curr code           =[USD]
value in nz         =[2074]
freight             =[320]
insurance           =[3]
exch rate           =[0.71]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[N]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[]
duty credit         =[]
gst                 =[299.63]
pref ind            =[Q]
block_g=[37217201015H984797KELECTROGALVANISED STEEL WIRE COLD HEADING QUALITY, 1.6MM OR MORE INDIAMETERCNCNKGM2500.00000939902Z1472.85USD207432030.71N299.63Q    ]
total_invoices      =[1]
total_lines         =[3]
total_packages      =[141]
total_alac          =[]
total_hera          =[]
total_anti dump     =[]
total_countervailing=[]
total_tariff duty   =[0.00]
total_value in nz   =[18722]
total_gst           =[2705.01]
total_amount        =[2705.01]
total_duty credits  =[]
method of payment   =[D]
declarant code      =[40000303D]
block_h=[131410.00187222705.012705.01D40000303D  ]
pbuf=[929B00001013510CNTXGNZAKLNZAKL20050606MCDYNNNNATF808424100      RESIGNED PIN FAILURE    BMMISCTXG0000017620PK   BMMISCTXG000001762AAQTTNU1558525521CSS051611    BMMISCTXG000001762AAQTTNU15585255120CL  991KOTA JUTA    00310300F40132202H      JLHC050308CFR   17215900029A800890GBLACK ANNEALED WIRE CUT TO LENGTHCNCNKGM18500.00000939902Z10338.13USD145612246240.71N2103.88Q27217900019L800890GBLACK ANNEALED WIRE IN COILS, COATED OTHER THAN NAILCNCNKGM3000.00000939902Z1481.52USD208732230.71N301.50Q   37217201015H984797KELECTROGALVANISED STEEL WIRE COLD HEADING QUALITY, 1.6MM OR MORE INDIAMETERCNCNKGM2500.00000939902Z1472.85USD207432030.71N299.63Q    131410.00187222705.012705.01D40000303D  ]
length of pbuf = 688
Looking up [40000303D] in database
aut_result=    [NDHKFM@GKDH@MGKJ]
mes_aut_result=[@LNDFHHEBHAAAJKG]
MAC NOT OK
";
		#endregion
		#endregion

		#region TestPreviousFailureAFIAKL_B00001035
		public void TestPreviousFailureAFIAKL_B00001035()
		{
			TestClientFailures(MessageAFIAKL_B00001035, DebugTextAFIAKL_B00001035);
		}
		#region Message
		const string MessageAFIAKL_B00001035 = @"UNH+140+CUSDEC:D:96B:UN+04747719'
BGM+830+S00001135+5'
CST++40:105:143'
LOC+9+NZAKL'
LOC+11+AUSYD'
LOC+41+NZAKL'
LOC+28+AU'
DTM+129:20050527:102'
GIS+S:112:143'
MEA+WT+AAD+KGM:700'
EQD+CN+++++7'
FTX+AAI+++ENTRY CHECKED AND RESENT'
RFF+BM:84177'
PAC+0++PK'
RFF+BM:S00001135'
RFF+AAQ'
PAC+2++PX'
TDT+20+612+1+++++:::VLADIVOSTOK'
NAD+AL+00331182B:ZZZ:143'
NAD+CB+00254888H:ZZZ:143'
UNS+D'
DMS+0519-05+935'
TOD+++FOB:106:143'
CST+1+3304990019D:169:143'
FTX+AAA+++SKIN CARE PREPARATIONS'
LOC+27+NZ'
MOA+14:2522.88:NZD'
CUX+2++1.00+N'
CST+2+3307200019B:169:143'
FTX+AAA+++DEODERANTS FOR PERSONAL USE'
LOC+27+NZ'
MOA+14:1373.04:NZD'
CUX+2++1.00+N'
CST+3+3307200008G:169:143'
FTX+AAA+++DEODORANTS & ANTI-PERSPIRANTS IN AEROSOL CONTAINERS, NOT CONTAINING:FLUOROCARBON'
LOC+27+NZ'
MEA+AAR++NMB:420.000'
MOA+14:722.40:NZD'
CUX+2++1.00+N'
CST+4+9403200011C:169:143+998951L:184:143'
FTX+AAA+++DISPLAY STANDS METAL,PORTABLE'
LOC+27+NZ'
MEA+AAR++NMB:8.000'
MOA+14:1415.52:NZD'
CUX+2++1.00+N'
UNS+S'
CNT+4:1'
CNT+5:4'
CNT+11:2'
AUT+LCNHJEM@AOGF@@MJ+40000670K'
UNT+51+140'
";
		#endregion
		#region DebugText
		const string DebugTextAFIAKL_B00001035 = @"arg1 = M0027404485
arg2 = /appl/ediprd/ediedata/xlate/00085ZZV.TL0
arg3 = T/G
arg4 = 1
arg5 = 5523
argc = 7
symbolic_dir_name=CUSMOD
class entry    =[830]
client_ref     =[S00001135]
tran type      =[5]
entry type     =[40]
port loading   =[NZAKL]
port discharge =[AUSYD]
customs control=[]
processing port=[NZAKL]
country of dest=[AU]
date export    =[]
date import    =[20050527]
entry period   =[]
override ind   =[]
sold_ind       =[S]
other info  1  =[]
other data  1  =[]
other info  2  =[]
other data  2  =[]
other info  3  =[]
other data  3  =[]
other info  4  =[]
other data  4  =[]
other info  5  =[]
other data  5  =[]
other info  6  =[]
other data  6  =[]
other info  7  =[]
other data  7  =[]
other info  8  =[]
other data  8  =[]
other info  9  =[]
other data  9  =[]
other info 10  =[]
other data 10  =[]
total weight   =[700]
block_a=[830S00001135540NZAKLAUSYDNZAKLAU20050527S700    ]
block_b=[ENTRY CHECKED AND RESENT]
block_c number =0
container type q   =[]
container number   =[]
container status   =[]
number packages    =[0]
type packages      =[PK]
seal numbers       =[]
block_c=[BM841770PK      ]
block_c number =1
container type q   =[AAQ]
container number   =[]
container status   =[]
number packages    =[2]
type packages      =[PX]
seal numbers       =[]
block_c=[BMS00001135AAQ2PX       ]
voyage_number      =[612]
transport mode     =[1]
craft flight no    =[VLADIVOSTOK]
permit auth code 1 =[]
permit auth no 1   =[]
permit auth code 2 =[]
permit auth no 2   =[]
permit auth code 3 =[]
permit auth no 3   =[]
permit auth code 4 =[]
permit auth no 4   =[]
permit auth code 5 =[]
permit auth no 5   =[]
permit auth code 6 =[]
permit auth no 6   =[]
permit auth code 7 =[]
permit auth no 7   =[]
permit auth code 8 =[]
permit auth no 8   =[]
permit auth code 9 =[]
permit auth no 9   =[]
permit auth code10 =[]
permit auth no10   =[]
block_d=[6121VLADIVOSTOK ]
client code  =[00331182B]
client name  =[]
broker code  =[00254888H]
del auth code=[]
block_e=[00331182B00254888H      ]
invoice block number 0
invoice number =[0519-05]
invoice terms  =[FOB]
block_f=[0519-05FOB      ]
line_number         =[1]
tariff item         =[3304990019D]
cons code           =[]
goods descr         =[SKIN CARE PREPARATIONS]
country origin      =[NZ]
country export      =[]
stat unit           =[]
stat qty            =[]
supp unit           =[]
supp qty            =[]
supplier code       =[]
supplier name       =[]
value in curr       =[2522.88]
curr code           =[NZD]
value in nz         =[]
freight             =[]
insurance           =[]
exch rate           =[1.00]
exch ind            =[N]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[]
duty credit         =[]
gst                 =[]
pref ind            =[]
block_g=[13304990019DSKIN CARE PREPARATIONSNZ2522.88NZD1.00N     ]
line_number         =[2]
tariff item         =[3307200019B]
cons code           =[]
goods descr         =[DEODERANTS FOR PERSONAL USE]
country origin      =[NZ]
country export      =[]
stat unit           =[]
stat qty            =[]
supp unit           =[]
supp qty            =[]
supplier code       =[]
supplier name       =[]
value in curr       =[1373.04]
curr code           =[NZD]
value in nz         =[]
freight             =[]
insurance           =[]
exch rate           =[1.00]
exch ind            =[N]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[]
duty credit         =[]
gst                 =[]
pref ind            =[]
block_g=[23307200019BDEODERANTS FOR PERSONAL USENZ1373.04NZD1.00N]
line_number         =[3]
tariff item         =[3307200008G]
cons code           =[]
goods descr         =[DEODORANTS & ANTI-PERSPIRANTS IN AEROSOL CONTAINERS, NOT CONTAININGFLUOROCARBON]
country origin      =[NZ]
country export      =[]
stat unit           =[NMB]
stat qty            =[420.000]
supp unit           =[]
supp qty            =[]
supplier code       =[]
supplier name       =[]
value in curr       =[722.40]
curr code           =[NZD]
value in nz         =[]
freight             =[]
insurance           =[]
exch rate           =[1.00]
exch ind            =[N]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[]
duty credit         =[]
gst                 =[]
pref ind            =[]
block_g=[33307200008GDEODORANTS & ANTI-PERSPIRANTS IN AEROSOL CONTAINERS, NOT CONTAININGFLUOROCARBONNZNMB420.000722.40NZD1.00N   ]
line_number         =[4]
tariff item         =[9403200011C]
cons code           =[998951L]
goods descr         =[DISPLAY STANDS METAL,PORTABLE]
country origin      =[NZ]
country export      =[]
stat unit           =[NMB]
stat qty            =[8.000]
supp unit           =[]
supp qty            =[]
supplier code       =[]
supplier name       =[]
value in curr       =[1415.52]
curr code           =[NZD]
value in nz         =[]
freight             =[]
insurance           =[]
exch rate           =[1.00]
exch ind            =[N]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[]
duty credit         =[]
gst                 =[]
pref ind            =[]
block_g=[49403200011C998951LDISPLAY STANDS METAL,PORTABLENZNMB8.0001415.52NZD1.00N       ]
total_invoices      =[1]
total_lines         =[4]
total_packages      =[2]
total_alac          =[]
total_hera          =[]
total_anti dump     =[]
total_countervailing=[]
total_tariff duty   =[]
total_value in nz   =[]
total_gst           =[]
total_amount        =[]
total_duty credits  =[]
method of payment   =[]
declarant code      =[40000670K]
block_h=[14240000670K    ]
pbuf=[830S00001135540NZAKLAUSYDNZAKLAU20050527S700    ENTRY CHECKED AND RESENTBM841770PK      BMS00001135AAQ2PX       6121VLADIVOSTOK 00331182B00254888H      0519-05FOB      13304990019DSKIN CARE PREPARATIONSNZ2522.88NZD1.00N     23307200019BDEODERANTS FOR PERSONAL USENZ1373.04NZD1.00N33307200008GDEODORANTS & ANTI-PERSPIRANTS IN AEROSOL CONTAINERS, NOT CONTAININGFLUOROCARBONNZNMB420.000722.40NZD1.00N   49403200011C998951LDISPLAY STANDS METAL,PORTABLENZNMB8.0001415.52NZD1.00N       14240000670K    ]
length of pbuf = 496
Looking up [40000670K] in database
aut_result=    [IHHOLMIACMLODBLN]
mes_aut_result=[LCNHJEM@AOGF@@MJ]
MAC NOT OK";
		#endregion
		#endregion

		#region TestPreviousFailureAFIAKL_B00001101
		public void TestPreviousFailureAFIAKL_B00001101()
		{
			TestClientFailures(MessageAFIAKL_B00001101, DebugTextAFIAKL_B00001101);
		}
		#region Message
		const string MessageAFIAKL_B00001101 = @"UNH+167+CUSDEC:D:96B:UN'
BGM+830+S00001101+9'
CST++40:105:143'
LOC+9+NZAKL'
LOC+11+CKAIT'
LOC+41+NZAKL'
LOC+28+CK'
DTM+129:20050531:102'
GIS+S:112:143'
MEA+WT+AAD+KGM:600'
EQD+CN+++++7'
RFF+BM:206364'
PAC+0++PK'
RFF+BM:S00001101'
RFF+AAQ'
PAC+2++PX'
TDT+20+062+1+++++:::CAPITAINE FEARN'
NAD+AL+40083722J:ZZZ:143'
NAD+CB+00254888H:ZZZ:143'
UNS+D'
DMS+1200+935'
TOD+++FOB:106:143'
CST+1+8507100929C:169:143'
FTX+AAA+++LEAD ACID BATTERIES FOR MOTOR VEHICLES'
LOC+27+KR'
MEA+AAR++NMB:20.000'
MOA+14:1238.04:NZD'
CUX+2++1.00+N'
GIS+NHW:118:143'
UNS+S'
CNT+4:1'
CNT+5:1'
CNT+11:2'
AUT+IFNAKFJJJEH@CAGN+40000670K'
UNT+35+167'";
		#endregion
		#region DebugText
		const string DebugTextAFIAKL_B00001101 = @"arg1 = M0027450195
arg2 = /appl/ediprd/ediedata/xlate/00086GFL.TL0
arg3 = T/G
arg4 = 1
arg5 = 19123
argc = 7
symbolic_dir_name=CUSMOD
class entry    =[830]
client_ref     =[S00001101]
tran type      =[9]
entry type     =[40]
port loading   =[NZAKL]
port discharge =[CKAIT]
customs control=[]
processing port=[NZAKL]
country of dest=[CK]
date export    =[]
date import    =[20050531]
entry period   =[]
override ind   =[]
sold_ind       =[S]
other info  1  =[]
other data  1  =[]
other info  2  =[]
other data  2  =[]
other info  3  =[]
other data  3  =[]
other info  4  =[]
other data  4  =[]
other info  5  =[]
other data  5  =[]
other info  6  =[]
other data  6  =[]
other info  7  =[]
other data  7  =[]
other info  8  =[]
other data  8  =[]
other info  9  =[]
other data  9  =[]
other info 10  =[]
other data 10  =[]
total weight   =[600]
block_a=[830S00001101940NZAKLCKAITNZAKLCK20050531S600    ]
block_c number =0
container type q   =[]
container number   =[]
container status   =[]
number packages    =[0]
type packages      =[PK]
seal numbers       =[]
block_c=[BM2063640PK     ]
block_c number =1
container type q   =[AAQ]
container number   =[]
container status   =[]
number packages    =[2]
type packages      =[PX]
seal numbers       =[]
block_c=[BMS00001101AAQ2PX       ]
voyage_number      =[062]
transport mode     =[1]
craft flight no    =[CAPITAINE FEARN]
permit auth code 1 =[]
permit auth no 1   =[]
permit auth code 2 =[]
permit auth no 2   =[]
permit auth code 3 =[]
permit auth no 3   =[]
permit auth code 4 =[]
permit auth no 4   =[]
permit auth code 5 =[]
permit auth no 5   =[]
permit auth code 6 =[]
permit auth no 6   =[]
permit auth code 7 =[]
permit auth no 7   =[]
permit auth code 8 =[]
permit auth no 8   =[]
permit auth code 9 =[]
permit auth no 9   =[]
permit auth code10 =[]
permit auth no10   =[]
block_d=[0621CAPITAINE FEARN     ]
client code  =[40083722J]
client name  =[]
broker code  =[00254888H]
del auth code=[]
block_e=[40083722J00254888H      ]
invoice block number 0
invoice number =[1200]
invoice terms  =[FOB]
block_f=[1200FOB ]
line_number         =[1]
tariff item         =[8507100929C]
cons code           =[]
goods descr         =[LEAD ACID BATTERIES FOR MOTOR VEHICLES]
country origin      =[KR]
country export      =[]
stat unit           =[NMB]
stat qty            =[20.000]
supp unit           =[]
supp qty            =[]
supplier code       =[]
supplier name       =[]
value in curr       =[1238.04]
curr code           =[NZD]
value in nz         =[]
freight             =[]
insurance           =[]
exch rate           =[1.00]
exch ind            =[N]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[]
proh code 1         =[NHW]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[]
duty credit         =[]
gst                 =[]
pref ind            =[]
block_g=[18507100929CLEAD ACID BATTERIES FOR MOTOR VEHICLESKRNMB20.0001238.04NZD1.00NNHW ]
total_invoices      =[1]
total_lines         =[1]
total_packages      =[2]
total_alac          =[]
total_hera          =[]
total_anti dump     =[]
total_countervailing=[]
total_tariff duty   =[]
total_value in nz   =[]
total_gst           =[]
total_amount        =[]
total_duty credits  =[]
method of payment   =[]
declarant code      =[40000670K]
block_h=[11240000670K    ]
pbuf=[830S00001101940NZAKLCKAITNZAKLCK20050531S600    BM2063640PK     BMS00001101AAQ2PX       0621CAPITAINE FEARN     40083722J00254888H      1200FOB 18507100929CLEAD ACID BATTERIES FOR MOTOR VEHICLESKRNMB20.0001238.04NZD1.00NNHW 11240000670K    ]
length of pbuf = 240
Looking up [40000670K] in database
aut_result=    [CBKOGOBKJOBNEKMN]
mes_aut_result=[IFNAKFJJJEH@CAGN]
MAC NOT OK
";
		#endregion
		#endregion

		#region TestPreviousFailureEDISYD_B00001040
		public void TestPreviousFailureEDISYD_B00001040()
		{
			TestClientFailures(MessageEDISYD_B00001040, DebugTextEDISYD_B00001040);
		}
		#region Message
		const string MessageEDISYD_B00001040 = @"UNH+255+CUSDEC:D:96B:UN+84491210'
BGM+929+B00001040+5'
CST++10:105:143'
LOC+9+AUSYD'
LOC+11+NZAKL'
LOC+41+NZAKL'
DTM+151:20050609:102'
MEA+WT+AAD+KGM:400'
EQD+CN+TRLU5183832++++7'
SEL+SEAL1'
EQD+CN+SUDU3699185++++7'
SEL+SEAL2'
FTX+AAI+++FIXED THE DAMN CONTAINER NUMBERS.'
RFF+BM:OCEANBILL'
PAC+0++PK'
RFF+BM:HOUSEBILL'
RFF+AAQ:TRLU5183832'
PAC+50++BA'
PAC+30++KG'
RFF+BM:HOUSEBILL'
RFF+AAQ:SUDU3699185'
PAC+5++QH'
PAC+8++HC'
PAC+3++CB'
RFF+BM:HOUSEBILL2'
RFF+AAQ:TRLU5183832'
PAC+20++PH'
PAC+7++YR'
RFF+BM:HOUSEBILL2'
RFF+AAQ:SUDU3699185'
PAC+47++BA'
PAC+30++BG'
TDT+20+109+1+++++:::BUNGA TERASEK'
NAD+AL+00782903F:ZZZ:143'
NAD+CB+00009917B:ZZZ:143'
UNS+D'
DMS+FRED+935'
TOD+++FOB:106:143'
CST+1+0301100000B:169:143'
FTX+AAA+++ORNAMENTAL FISH (CUCKOO SQEAKERS)'
LOC+27+AU'
LOC+35+AU'
MEA+AAR++KGM:400.000'
NAD+SU+00710841Y:ZZZ:143'
MOA+14:100000.00:NZD'
CUX+2++1.00'
MOA+40:100000'
MOA+64:340'
MOA+70:12000'
GIS+Y:109:143'
TAX+1+GST'
MOA+161:14042.50'
GIS+Q:116:143'
UNS+S'
CNT+4:1'
CNT+5:1'
CNT+11:200'
TAX+3+CUD++100000'
MOA+161:0.00'
TAX+3+GST'
MOA+161:14042.50'
TAX+4+TOT'
MOA+161:14042.50'
GIS+B:134:143'
AUT+@N@OMNCELLCEOCEK+65432198B'
UNT+66+255'
";
		#endregion
		#region DebugText
		const string DebugTextEDISYD_B00001040 = @"symbolic_dir_name=CUSSWT
class entry    =[929]
client_ref     =[B00001040]
tran type      =[5]
entry type     =[10]
port loading   =[AUSYD]
port discharge =[NZAKL]
customs control=[]
processing port=[NZAKL]
country of dest=[]
date export    =[20050609]
date import    =[]
entry period   =[]
override ind   =[]
sold_ind       =[]
other info  1  =[]
other data  1  =[]
other info  2  =[]
other data  2  =[]
other info  3  =[]
other data  3  =[]
other info  4  =[]
other data  4  =[]
other info  5  =[]
other data  5  =[]
other info  6  =[]
other data  6  =[]
other info  7  =[]
other data  7  =[]
other info  8  =[]
other data  8  =[]
other info  9  =[]
other data  9  =[]
other info 10  =[]
other data 10  =[]
total weight   =[400]
block_a=[929B00001040510AUSYDNZAKLNZAKL20050609400       ]
block_b=[FIXED THE DAMN CONTAINER NUMBERS.       ]
block_c number =0
container type q   =[]
container number   =[]
container status   =[]
number packages    =[0]
type packages      =[PK]
seal numbers       =[]
block_c=[BMOCEANBILL0PK  ]
block_c number =1
container type q   =[AAQ]
container number   =[TRLU5183832]
container status   =[7]
number packages    =[50]
type packages      =[BA]
seal numbers       =[SEAL1]
block_c=[BMHOUSEBILLAAQTRLU5183832750BASEAL1     ]
block_c number =2
container type q   =[AAQ]
container number   =[TRLU5183832]
container status   =[7]
number packages    =[30]
type packages      =[KG]
seal numbers       =[]
block_c=[BMHOUSEBILLAAQTRLU5183832730KG  ]
block_c number =3
container type q   =[AAQ]
container number   =[SUDU3699185]
container status   =[7]
number packages    =[5]
type packages      =[QH]
seal numbers       =[SEAL2]
block_c=[BMHOUSEBILLAAQSUDU369918575QHSEAL2      ]
block_c number =4
container type q   =[AAQ]
container number   =[SUDU3699185]
container status   =[7]
number packages    =[8]
type packages      =[HC]
seal numbers       =[]
block_c=[BMHOUSEBILLAAQSUDU369918578HC   ]
block_c number =5
container type q   =[AAQ]
container number   =[SUDU3699185]
container status   =[7]
number packages    =[3]
type packages      =[CB]
seal numbers       =[]
block_c=[BMHOUSEBILLAAQSUDU369918573CB   ]
block_c number =6
container type q   =[AAQ]
container number   =[TRLU5183832]
container status   =[7]
number packages    =[20]
type packages      =[PH]
seal numbers       =[SEAL1]
block_c=[BMHOUSEBILL2AAQTRLU5183832720PHSEAL1    ]
block_c number =7
container type q   =[AAQ]
container number   =[TRLU5183832]
container status   =[7]
number packages    =[7]
type packages      =[YR]
seal numbers       =[]
block_c=[BMHOUSEBILL2AAQTRLU518383277YR  ]
block_c number =8
container type q   =[AAQ]
container number   =[SUDU3699185]
container status   =[7]
number packages    =[47]
type packages      =[BA]
seal numbers       =[SEAL2]
block_c=[BMHOUSEBILL2AAQSUDU3699185747BASEAL2    ]
block_c number =9
container type q   =[AAQ]
container number   =[SUDU3699185]
container status   =[7]
number packages    =[30]
type packages      =[BG]
seal numbers       =[]
block_c=[BMHOUSEBILL2AAQSUDU3699185730BG ]
voyage_number      =[109]
transport mode     =[1]
craft flight no    =[BUNGA TERASEK]
permit auth code 1 =[]
permit auth no 1   =[]
permit auth code 2 =[]
permit auth no 2   =[]
permit auth code 3 =[]
permit auth no 3   =[]
permit auth code 4 =[]
permit auth no 4   =[]
permit auth code 5 =[]
permit auth no 5   =[]
permit auth code 6 =[]
permit auth no 6   =[]
permit auth code 7 =[]
permit auth no 7   =[]
permit auth code 8 =[]
permit auth no 8   =[]
permit auth code 9 =[]
permit auth no 9   =[]
permit auth code10 =[]
permit auth no10   =[]
block_d=[1091BUNGA TERASEK       ]
client code  =[00782903F]
client name  =[]
broker code  =[00009917B]
del auth code=[]
block_e=[00782903F00009917B      ]
invoice block number 0
invoice number =[FRED]
invoice terms  =[FOB]
block_f=[FREDFOB ]
line_number         =[1]
tariff item         =[0301100000B]
cons code           =[]
goods descr         =[ORNAMENTAL FISH (CUCKOO SQEAKERS)]
country origin      =[AU]
country export      =[AU]
stat unit           =[KGM]
stat qty            =[400.000]
supp unit           =[]
supp qty            =[]
supplier code       =[00710841Y]
supplier name       =[]
value in curr       =[100000.00]
curr code           =[NZD]
value in nz         =[100000]
freight             =[340]
insurance           =[12000]
exch rate           =[1.00]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[Y]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[]
duty credit         =[]
gst                 =[14042.50]
pref ind            =[Q]
block_g=[10301100000BORNAMENTAL FISH (CUCKOO SQEAKERS)AUAUKGM400.00000710841Y100000.00NZD100000340120001.00Y14042.50Q    ]
total_invoices      =[1]
total_lines         =[1]
total_packages      =[200]
total_alac          =[]
total_hera          =[]
total_anti dump     =[]
total_countervailing=[]
total_tariff duty   =[0.00]
total_value in nz   =[100000]
total_gst           =[14042.50]
total_amount        =[14042.50]
total_duty credits  =[]
method of payment   =[B]
declarant code      =[65432198B]
block_h=[112000.0010000014042.5014042.50B65432198B       ]
pbuf=[929B00001040510AUSYDNZAKLNZAKL20050609400       FIXED THE DAMN CONTAINER NUMBERS.       BMOCEANBILL0PK  BMHOUSEBILLAAQTRLU5183832750BASEAL1     BMHOUSEBILLAAQTRLU5183832730KG  BMHOUSEBILLAAQSUDU369918575QHSEAL2      BMHOUSEBILLAAQSUDU369918578HC   BMHOUSEBILLAAQSUDU369918573CB   BMHOUSEBILL2AAQTRLU5183832720PHSEAL1    BMHOUSEBILL2AAQTRLU518383277YR  BMHOUSEBILL2AAQSUDU3699185747BASEAL2    BMHOUSEBILL2AAQSUDU3699185730BG 1091BUNGA TERASEK       00782903F00009917B      FREDFOB 10301100000BORNAMENTAL FISH (CUCKOO SQEAKERS)AUAUKGM400.00000710841Y100000.00NZD100000340120001.00Y14042.50Q    112000.0010000014042.5014042.50B65432198B       ]
length of pbuf = 640
Looking up [65432198B] in test file
aut_result=    [GIMMFJAMIIKAFBHC]
mes_aut_result=[@N@OMNCELLCEOCEK]
MAC NOT OK
";
		#endregion
		#endregion

		#region TestPreviousFailureTLLCHC_B00001039
		public void TestPreviousFailureTLLCHC_B00001039()
		{
			TestClientFailures(MessageTLLCHC_B00001039, DebugTextTLLCHC_B00001039);
		}
		#region Message
		const string MessageTLLCHC_B00001039 = @"UNH+134+CUSDEC:D:96B:UN+51166367'
BGM+929+B00001039+5'
CST++10:105:143'
LOC+9+SGSIN'
LOC+11+NZPOE'
LOC+41+NZCHC'
DTM+151:20050611:102'
GIS+ATF:110:143:7566'
GIS+MCD:110:143:YNYYY'
MEA+WT+AAD+KGM:21080'
EQD+CN+PONU0880880++++5'
FTX+AAI+++ERROR CORRECTED'
RFF+BM:PONLMER22001095'
PAC+0++PK'
RFF+BM:PONLMER22001095'
RFF+AAQ:PONU0880880'
PAC+20++07'
TDT+20+5018+1+++++:::CONTSHIP AURORA'
NAD+AL+00364128H:ZZZ:143'
NAD+CB+00387259K:ZZZ:143'
UNS+D'
DMS+ KR-496+935'
TOD+++CIF:106:143'
CST+1+3202900100F:169:143'
FTX+AAA+++TANNING SUBSTANCES BASED ON CHROMIUM SALTS'
LOC+27+TR'
LOC+35+TR'
NAD+SU+00310163M:ZZZ:143'
MOA+14:14375.09:USD'
CUX+2++0.71'
MOA+40:20247'
MOA+64:3044'
MOA+70:90'
GIS+N:109:143'
TAX+1+GST'
MOA+161:2922.63'
UNS+S'
CNT+4:1'
CNT+5:1'
CNT+11:20'
TAX+3+CUD++20247'
MOA+161:0.00'
TAX+3+GST'
MOA+161:2922.63'
TAX+4+TOT'
MOA+161:2922.63'
GIS+D:134:143'
AUT+FDAEDGONAFKMDJJO+40060241H'
UNT+49+134'";
		#endregion
		#region DebugText
		const string DebugTextTLLCHC_B00001039 = @"arg1 = M0027721896
arg2 = /appl/ediprd/ediedata/xlate/000894SW.TL0
arg3 = T/W
arg4 = 1
arg5 = 24473
argc = 7
symbolic_dir_name=CUSMOD
class entry    =[929]
client_ref     =[B00001039]
tran type      =[5]
entry type     =[10]
port loading   =[SGSIN]
port discharge =[NZPOE]
customs control=[]
processing port=[NZCHC]
country of dest=[]
date export    =[20050611]
date import    =[]
entry period   =[]
override ind   =[]
sold_ind       =[]
other info  1  =[ATF]
other data  1  =[7566]
other info  2  =[MCD]
other data  2  =[YNYYY]
other info  3  =[]
other data  3  =[]
other info  4  =[]
other data  4  =[]
other info  5  =[]
other data  5  =[]
other info  6  =[]
other data  6  =[]
other info  7  =[]
other data  7  =[]
other info  8  =[]
other data  8  =[]
other info  9  =[]
other data  9  =[]
other info 10  =[]
other data 10  =[]
total weight   =[21080]
block_a=[929B00001039510SGSINNZPOENZCHC20050611ATF7566MCDYNYYY21080      ]
block_b=[ERROR CORRECTED ]
block_c number =0
container type q   =[]
container number   =[]
container status   =[]
number packages    =[0]
type packages      =[PK]
seal numbers       =[]
block_c=[BMPONLMER220010950PK    ]
block_c number =1
container type q   =[AAQ]
container number   =[PONU0880880]
container status   =[5]
number packages    =[20]
type packages      =[07]
seal numbers       =[]
block_c=[BMPONLMER22001095AAQPONU088088052007    ]
voyage_number      =[5018]
transport mode     =[1]
craft flight no    =[CONTSHIP AURORA]
permit auth code 1 =[]
permit auth no 1   =[]
permit auth code 2 =[]
permit auth no 2   =[]
permit auth code 3 =[]
permit auth no 3   =[]
permit auth code 4 =[]
permit auth no 4   =[]
permit auth code 5 =[]
permit auth no 5   =[]
permit auth code 6 =[]
permit auth no 6   =[]
permit auth code 7 =[]
permit auth no 7   =[]
permit auth code 8 =[]
permit auth no 8   =[]
permit auth code 9 =[]
permit auth no 9   =[]
permit auth code10 =[]
permit auth no10   =[]
block_d=[50181CONTSHIP AURORA    ]
client code  =[00364128H]
client name  =[]
broker code  =[00387259K]
del auth code=[]
block_e=[00364128H00387259K      ]
invoice block number 0
invoice number =[ KR-496]
invoice terms  =[CIF]
block_f=[ KR-496CIF      ]
line_number         =[1]
tariff item         =[3202900100F]
cons code           =[]
goods descr         =[TANNING SUBSTANCES BASED ON CHROMIUM SALTS]
country origin      =[TR]
country export      =[TR]
stat unit           =[]
stat qty            =[]
supp unit           =[]
supp qty            =[]
supplier code       =[00310163M]
supplier name       =[]
value in curr       =[14375.09]
curr code           =[USD]
value in nz         =[20247]
freight             =[3044]
insurance           =[90]
exch rate           =[0.71]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[N]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[]
duty credit         =[]
gst                 =[2922.63]
pref ind            =[]
block_g=[13202900100FTANNING SUBSTANCES BASED ON CHROMIUM SALTSTRTR00310163M14375.09USD202473044900.71N2922.63   ]
total_invoices      =[1]
total_lines         =[1]
total_packages      =[20]
total_alac          =[]
total_hera          =[]
total_anti dump     =[]
total_countervailing=[]
total_tariff duty   =[0.00]
total_value in nz   =[20247]
total_gst           =[2922.63]
total_amount        =[2922.63]
total_duty credits  =[]
method of payment   =[D]
declarant code      =[40060241H]
block_h=[11200.00202472922.632922.63D40060241H   ]
pbuf=[929B00001039510SGSINNZPOENZCHC20050611ATF7566MCDYNYYY21080      ERROR CORRECTED BMPONLMER220010950PK    BMPONLMER22001095AAQPONU088088052007    50181CONTSHIP AURORA    00364128H00387259K       KR-496CIF      13202900100FTANNING SUBSTANCES BASED ON CHROMIUM SALTSTRTR00310163M14375.09USD202473044900.71N2922.63   11200.00202472922.632922.63D40060241H   ]
length of pbuf = 352
Looking up [40060241H] in database
aut_result=    [GFHHA@@EDGEEAJJM]
mes_aut_result=[FDAEDGONAFKMDJJO]
MAC NOT OK
";
		#endregion
		#endregion

		#region TestPreviousFailureROHAKL_S00076230
		public void TestPreviousFailureROHAKL_S00076230()
		{
			TestClientFailures(MessageROHAKL_S00076230, DebugTextROHAKL_S00076230);
		}
		#region Message
		const string MessageROHAKL_S00076230 = @"UNH+3466+CUSDEC:D:96B:UN+08421154'
BGM+929+S00076230+5'
CST++10:105:143'
LOC+9+DEBRE'
LOC+11+NZAKL'
LOC+41+NZAKL'
DTM+151:20050827:102'
GIS+MCD:110:143:YNYYY'
MEA+WT+AAD+KGM:214071'
EQD+CN+APMU8050752++++5'
EQD+CN+CAXU9233856++++5'
EQD+CN+CAXU9598113++++5'
EQD+CN+CAXU9678857++++5'
EQD+CN+CLHU4180010++++5'
EQD+CN+CLHU8068668++++5'
EQD+CN+CLHU8625505++++5'
EQD+CN+FSCU6651713++++5'
EQD+CN+GATU4068076++++5'
EQD+CN+GATU8483990++++5'
EQD+CN+GESU4455189++++5'
EQD+CN+INBU5056043++++5'
EQD+CN+INKU2606508++++5'
EQD+CN+MAEU3314492++++5'
EQD+CN+MAEU3315858++++5'
EQD+CN+MAEU3316366++++5'
EQD+CN+MAEU3320936++++5'
EQD+CN+MAEU3322435++++5'
EQD+CN+MAEU6003508++++5'
EQD+CN+MAEU6194132++++5'
EQD+CN+MAEU7341135++++5'
EQD+CN+MAEU8155198++++5'
EQD+CN+MAEU8166124++++5'
EQD+CN+MAEU8387526++++5'
EQD+CN+MSKU6104420++++5'
EQD+CN+MSKU6301558++++5'
EQD+CN+MSKU6335645++++5'
EQD+CN+MSKU6527817++++5'
EQD+CN+MSKU6595494++++5'
EQD+CN+MSKU8191110++++5'
EQD+CN+MSKU8610047++++5'
EQD+CN+MSKU8954232++++5'
EQD+CN+MSKU9002885++++5'
EQD+CN+SAMU4011903++++5'
EQD+CN+SEAU7858253++++5'
EQD+CN+SEAU8168674++++5'
EQD+CN+SEAU8470961++++5'
EQD+CN+TEXU7054245++++5'
EQD+CN+TORU5001847++++5'
EQD+CN+TRIU9190959++++5'
EQD+CN+TTNU9406642++++5'
EQD+CN+GESU4224282++++5'
EQD+CN+MAEU6125044++++5'
EQD+CN+MSKU6257408++++5'
EQD+CN+MAEU7407720++++5'
EQD+CN+MSKU8029665++++5'
EQD+CN+MSKU8390301++++5'
EQD+CN+TRIU9794835++++5'
EQD+CN+UESU4571304++++5'
EQD+CN+MSKU9022156++++5'
EQD+CN+CAXU9167219++++5'
EQD+CN+TCNU9311035++++5'
EQD+CN+MSKU8595148++++5'
EQD+CN+TTNU9058038++++5'
EQD+CN+TCKU9700114++++5'
FTX+AAI+++PIN FAILURE'
RFF+BM:HAMF08259'
PAC+0++PK'
RFF+BM:HAMF08251'
RFF+AAQ:APMU8050752'
PAC+3++PK'
RFF+BM:HAMF08251'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU8954232'
PAC+12++PK'
RFF+BM:HAMF08251'
RFF+AAQ:TEXU7054245'
PAC+5++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MAEU6003508'
PAC+5++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MAEU8166124'
PAC+5++PK'
RFF+BM:HAMF08251'
RFF+AAQ:TRIU9190959'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MAEU8155198'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU8191110'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MAEU8387526'
PAC+7++PK'
RFF+BM:HAMF08251'
RFF+AAQ:GESU4455189'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:SEAU8168674'
PAC+7++PK'
RFF+BM:HAMF08251'
RFF+AAQ:TTNU9406642'
PAC+6++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MAEU6194132'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:SEAU8470961'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU6527817'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:CAXU9233856'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:CAXU9598113'
PAC+10++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MAEU3315858'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MAEU3322435'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:GATU8483990'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:SEAU7858253'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MAEU3320936'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU6335645'
PAC+11++PK'
RFF+BM:HAMF08251'
RFF+AAQ:FSCU6651713'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MAEU7341135'
PAC+11++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MAEU3314492'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU6104420'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:SAMU4011903'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:INBU5056043'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:CLHU8625505'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU6595494'
PAC+9++PK'
RFF+BM:HAMF08251'
RFF+AAQ:CLHU4180010'
PAC+28++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU6301558'
PAC+4++PK'
RFF+BM:HAMF08251'
RFF+AAQ:CAXU9678857'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:GATU4068076'
PAC+4++PK'
RFF+BM:HAMF08251'
RFF+AAQ:INKU2606508'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU9002885'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU8610047'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:CLHU8068668'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MAEU3316366'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:TORU5001847'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:GESU4224282'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:TCNU9311035'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU6257408'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU8029665'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU9022156'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:CAXU9167219'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:TTNU9058038'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:UESU4571304'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MAEU6125044'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU8595148'
PAC+3++PK'
RFF+BM:HAMF08251'
RFF+AAQ:TRIU9794835'
PAC+2++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MAEU7407720'
PAC+6++PK'
RFF+BM:HAMF08251'
RFF+AAQ:TCKU9700114'
PAC+7++PK'
RFF+BM:HAMF08251'
RFF+AAQ:MSKU8390301'
PAC+3++PK'
TDT+20+517+1+++++:::NICOLINE MAERSK'
NAD+AL+00288454C:ZZZ:143'
NAD+CB+00303221D:ZZZ:143'
UNS+D'
DMS+305050+935'
TOD+++CIF:106:143'
CST+1+8422300900K:169:143+994583A:184:143'
FTX+AAA+++FULLY AUTOMATED FILLING AND SEALING MACHINES'
LOC+27+DE'
LOC+35+DE'
NAD+SU+00914821Q:ZZZ:143'
MOA+14:9959978.85:EUR'
CUX+2++0.56'
MOA+40:17785677'
MOA+64:263043'
MOA+70:0'
GIS+N:109:143'
GIS+NEW:118:143'
TAX+1+GST'
MOA+161:2256090.00'
UNS+S'
CNT+4:1'
CNT+5:1'
CNT+11:235'
TAX+3+CUD++17785677'
MOA+161:0.00'
TAX+3+GST'
MOA+161:2256090.00'
TAX+4+TOT'
MOA+161:2256090.00'
GIS+D:134:143'
AUT+FHCACFICNGNAF@EG+40000052C'
UNT+267+3466'";
		#endregion
		#region DebugText
		const string DebugTextROHAKL_S00076230 = @"arg1 = M0028903554
arg2 = /appl/ediprd/ediedata/xlate/0008KWTH.TL0
arg3 = T/E
arg4 = 1
arg5 = 17493
argc = 7
symbolic_dir_name=CUSMOD
class entry    =[929]
client_ref     =[S00076230]
tran type      =[5]
entry type     =[10]
port loading   =[DEBRE]
port discharge =[NZAKL]
customs control=[]
processing port=[NZAKL]
country of dest=[]
date export    =[20050827]
date import    =[]
entry period   =[]
override ind   =[]
sold_ind       =[]
other info  1  =[MCD]
other data  1  =[YNYYY]
other info  2  =[]
other data  2  =[]
other info  3  =[]
other data  3  =[]
other info  4  =[]
other data  4  =[]
other info  5  =[]
other data  5  =[]
other info  6  =[]
other data  6  =[]
other info  7  =[]
other data  7  =[]
other info  8  =[]
other data  8  =[]
other info  9  =[]
other data  9  =[]
other info 10  =[]
other data 10  =[]
total weight   =[214071]
block_a=[929S00076230510DEBRENZAKLNZAKL20050827MCDYNYYY214071    ]
block_b=[PIN FAILURE     ]
block_c number =0
container type q   =[]
container number   =[]
container status   =[]
number packages    =[0]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF082590PK  ]
block_c number =1
container type q   =[AAQ]
container number   =[APMU8050752]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQAPMU805075253PK   ]
block_c number =2
container type q   =[]
container number   =[]
container status   =[]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF082512PK  ]
block_c number =3
container type q   =[AAQ]
container number   =[MSKU8954232]
container status   =[5]
number packages    =[12]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU8954232512PK  ]
block_c number =4
container type q   =[AAQ]
container number   =[TEXU7054245]
container status   =[5]
number packages    =[5]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQTEXU705424555PK   ]
block_c number =5
container type q   =[AAQ]
container number   =[MAEU6003508]
container status   =[5]
number packages    =[5]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMAEU600350855PK   ]
block_c number =6
container type q   =[AAQ]
container number   =[MAEU8166124]
container status   =[5]
number packages    =[5]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMAEU816612455PK   ]
block_c number =7
container type q   =[AAQ]
container number   =[TRIU9190959]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQTRIU919095952PK   ]
block_c number =8
container type q   =[AAQ]
container number   =[MAEU8155198]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMAEU815519852PK   ]
block_c number =9
container type q   =[AAQ]
container number   =[MSKU8191110]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU819111053PK   ]
block_c number =10
container type q   =[AAQ]
container number   =[MAEU8387526]
container status   =[5]
number packages    =[7]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMAEU838752657PK   ]
block_c number =11
container type q   =[AAQ]
container number   =[GESU4455189]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQGESU445518952PK   ]
block_c number =12
container type q   =[AAQ]
container number   =[SEAU8168674]
container status   =[5]
number packages    =[7]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQSEAU816867457PK   ]
block_c number =13
container type q   =[AAQ]
container number   =[TTNU9406642]
container status   =[5]
number packages    =[6]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQTTNU940664256PK   ]
block_c number =14
container type q   =[AAQ]
container number   =[MAEU6194132]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMAEU619413253PK   ]
block_c number =15
container type q   =[AAQ]
container number   =[SEAU8470961]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQSEAU847096153PK   ]
block_c number =16
container type q   =[AAQ]
container number   =[MSKU6527817]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU652781752PK   ]
block_c number =17
container type q   =[AAQ]
container number   =[CAXU9233856]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQCAXU923385652PK   ]
block_c number =18
container type q   =[AAQ]
container number   =[CAXU9598113]
container status   =[5]
number packages    =[10]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQCAXU9598113510PK  ]
block_c number =19
container type q   =[AAQ]
container number   =[MAEU3315858]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMAEU331585852PK   ]
block_c number =20
container type q   =[AAQ]
container number   =[MAEU3322435]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMAEU332243552PK   ]
block_c number =21
container type q   =[AAQ]
container number   =[GATU8483990]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQGATU848399052PK   ]
block_c number =22
container type q   =[AAQ]
container number   =[SEAU7858253]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQSEAU785825353PK   ]
block_c number =23
container type q   =[AAQ]
container number   =[MAEU3320936]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMAEU332093652PK   ]
block_c number =24
container type q   =[AAQ]
container number   =[MSKU6335645]
container status   =[5]
number packages    =[11]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU6335645511PK  ]
block_c number =25
container type q   =[AAQ]
container number   =[FSCU6651713]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQFSCU665171352PK   ]
block_c number =26
container type q   =[AAQ]
container number   =[MAEU7341135]
container status   =[5]
number packages    =[11]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMAEU7341135511PK  ]
block_c number =27
container type q   =[AAQ]
container number   =[MAEU3314492]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMAEU331449252PK   ]
block_c number =28
container type q   =[AAQ]
container number   =[MSKU6104420]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU610442052PK   ]
block_c number =29
container type q   =[AAQ]
container number   =[SAMU4011903]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQSAMU401190353PK   ]
block_c number =30
container type q   =[AAQ]
container number   =[INBU5056043]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQINBU505604353PK   ]
block_c number =31
container type q   =[AAQ]
container number   =[CLHU8625505]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQCLHU862550553PK   ]
block_c number =32
container type q   =[AAQ]
container number   =[MSKU6595494]
container status   =[5]
number packages    =[9]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU659549459PK   ]
block_c number =33
container type q   =[AAQ]
container number   =[CLHU4180010]
container status   =[5]
number packages    =[28]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQCLHU4180010528PK  ]
block_c number =34
container type q   =[AAQ]
container number   =[MSKU6301558]
container status   =[5]
number packages    =[4]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU630155854PK   ]
block_c number =35
container type q   =[AAQ]
container number   =[CAXU9678857]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQCAXU967885752PK   ]
block_c number =36
container type q   =[AAQ]
container number   =[GATU4068076]
container status   =[5]
number packages    =[4]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQGATU406807654PK   ]
block_c number =37
container type q   =[AAQ]
container number   =[INKU2606508]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQINKU260650852PK   ]
block_c number =38
container type q   =[AAQ]
container number   =[MSKU9002885]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU900288553PK   ]
block_c number =39
container type q   =[AAQ]
container number   =[MSKU8610047]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU861004753PK   ]
block_c number =40
container type q   =[AAQ]
container number   =[CLHU8068668]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQCLHU806866853PK   ]
block_c number =41
container type q   =[AAQ]
container number   =[MAEU3316366]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMAEU331636652PK   ]
block_c number =42
container type q   =[AAQ]
container number   =[TORU5001847]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQTORU500184753PK   ]
block_c number =43
container type q   =[AAQ]
container number   =[GESU4224282]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQGESU422428253PK   ]
block_c number =44
container type q   =[AAQ]
container number   =[TCNU9311035]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQTCNU931103552PK   ]
block_c number =45
container type q   =[AAQ]
container number   =[MSKU6257408]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU625740853PK   ]
block_c number =46
container type q   =[AAQ]
container number   =[MSKU8029665]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU802966553PK   ]
block_c number =47
container type q   =[AAQ]
container number   =[MSKU9022156]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU902215652PK   ]
block_c number =48
container type q   =[AAQ]
container number   =[CAXU9167219]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQCAXU916721952PK   ]
block_c number =49
container type q   =[AAQ]
container number   =[TTNU9058038]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQTTNU905803853PK   ]
block_c number =50
container type q   =[AAQ]
container number   =[UESU4571304]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQUESU457130452PK   ]
block_c number =51
container type q   =[AAQ]
container number   =[MAEU6125044]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMAEU612504452PK   ]
block_c number =52
container type q   =[AAQ]
container number   =[MSKU8595148]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU859514853PK   ]
block_c number =53
container type q   =[AAQ]
container number   =[TRIU9794835]
container status   =[5]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQTRIU979483552PK   ]
block_c number =54
container type q   =[AAQ]
container number   =[MAEU7407720]
container status   =[5]
number packages    =[6]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMAEU740772056PK   ]
block_c number =55
container type q   =[AAQ]
container number   =[TCKU9700114]
container status   =[5]
number packages    =[7]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQTCKU970011457PK   ]
block_c number =56
container type q   =[AAQ]
container number   =[MSKU8390301]
container status   =[5]
number packages    =[3]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAMF08251AAQMSKU839030153PK   ]
voyage_number      =[517]
transport mode     =[1]
craft flight no    =[NICOLINE MAERSK]
permit auth code 1 =[]
permit auth no 1   =[]
permit auth code 2 =[]
permit auth no 2   =[]
permit auth code 3 =[]
permit auth no 3   =[]
permit auth code 4 =[]
permit auth no 4   =[]
permit auth code 5 =[]
permit auth no 5   =[]
permit auth code 6 =[]
permit auth no 6   =[]
permit auth code 7 =[]
permit auth no 7   =[]
permit auth code 8 =[]
permit auth no 8   =[]
permit auth code 9 =[]
permit auth no 9   =[]
permit auth code10 =[]
permit auth no10   =[]
block_d=[5171NICOLINE MAERSK     ]
client code  =[00288454C]
client name  =[]
broker code  =[00303221D]
del auth code=[]
block_e=[00288454C00303221D      ]
invoice block number 0
invoice number =[305050]
invoice terms  =[CIF]
block_f=[305050CIF       ]
line_number         =[1]
tariff item         =[8422300900K]
cons code           =[994583A]
goods descr         =[FULLY AUTOMATED FILLING AND SEALING MACHINES]
country origin      =[DE]
country export      =[DE]
stat unit           =[]
stat qty            =[]
supp unit           =[]
supp qty            =[]
supplier code       =[00914821Q]
supplier name       =[]
value in curr       =[9959978.85]
curr code           =[EUR]
value in nz         =[17785677]
freight             =[263043]
insurance           =[0]
exch rate           =[0.56]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[N]
proh code 1         =[NEW]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[]
duty credit         =[]
gst                 =[2256090.00]
pref ind            =[]
block_g=[18422300900K994583AFULLY AUTOMATED FILLING AND SEALING MACHINESDEDE00914821Q9959978.85EUR1778567726304300.56NNEW2256090.00      ]
total_invoices      =[1]
total_lines         =[1]
total_packages      =[235]
total_alac          =[]
total_hera          =[]
total_anti dump     =[]
total_countervailing=[]
total_tariff duty   =[0.00]
total_value in nz   =[17785677]
total_gst           =[2256090.00]
total_amount        =[2256090.00]
total_duty credits  =[]
method of payment   =[D]
declarant code      =[40000052C]
block_h=[112350.00177856772256090.002256090.00D40000052C ]
pbuf=[929S00076230510DEBRENZAKLNZAKL20050827MCDYNYYY214071    PIN FAILURE     BMHAMF082590PK  BMHAMF08251AAQAPMU805075253PK   BMHAMF082512PK  BMHAMF08251AAQMSKU8954232512PK  BMHAMF08251AAQTEXU705424555PK   BMHAMF08251AAQMAEU600350855PK   BMHAMF08251AAQMAEU816612455PK   BMHAMF08251AAQTRIU919095952PK   BMHAMF08251AAQMAEU815519852PK   BMHAMF08251AAQMSKU819111053PK   BMHAMF08251AAQMAEU838752657PK   BMHAMF08251AAQGESU445518952PK   BMHAMF08251AAQSEAU816867457PK   BMHAMF08251AAQTTNU940664256PK   BMHAMF08251AAQMAEU619413253PK   BMHAMF08251AAQSEAU847096153PK   BMHAMF08251AAQMSKU652781752PK   BMHAMF08251AAQCAXU923385652PK   BMHAMF08251AAQCAXU9598113510PK  "
			+ @"BMHAMF08251AAQMAEU331585852PK   BMHAMF08251AAQMAEU332243552PK   BMHAMF08251AAQGATU848399052PK   BMHAMF08251AAQSEAU785825353PK   BMHAMF08251AAQMAEU332093652PK   BMHAMF08251AAQMSKU6335645511PK  BMHAMF08251AAQFSCU665171352PK   BMHAMF08251AAQMAEU7341135511PK  BMHAMF08251AAQMAEU331449252PK   BMHAMF08251AAQMSKU610442052PK   BMHAMF08251AAQSAMU401190353PK   BMHAMF08251AAQINBU505604353PK   BMHAMF08251AAQCLHU862550553PK   BMHAMF08251AAQMSKU659549459PK   BMHAMF08251AAQCLHU4180010528PK  BMHAMF08251AAQMSKU630155854PK   BMHAMF08251AAQCAXU967885752PK   BMHAMF08251AAQGATU406807654PK   BMHAMF08251AAQINKU260650852PK   BMHAMF08251AAQMSKU900288553PK   BMHAMF08251AAQMSKU861004753PK   BMHAMF08251AAQCLHU806866853PK   BMHAMF08251AAQMAEU331636652PK   BMHAMF08251AAQTORU500184753PK   BMHAMF08251AAQGESU422428253PK   BMHAMF08251AAQTCNU931103552PK   BMHAMF08251AAQMSKU625740853PK   BMHAMF08251AAQMSKU802966553PK   BMHAMF08251AAQMSKU902215652PK   BMHAMF08251AAQCAXU916721952PK   BMHAMF08251AAQTTNU905803853PK   BMHAMF08251AAQUESU457130452PK   BMHAMF08251AAQMAEU612504452PK   BMHAMF08251AAQMSKU859514853PK   BMHAMF08251AAQTRIU979483552PK   BMHAMF08251AAQMAEU740772056PK   BMHAMF08251AAQTCKU970011457PK   BMHAMF08251AAQMSKU839030153PK   5171NICOLINE MAERSK     00288454C00303221D      305050CIF       18422300900K994583AFULLY AUTOMATED FILLING AND SEALING MACHINESDEDE00914821Q9959978.85EUR1778567726304300.56NNEW2256090.00      112350.00177856772256090.002256090.00D40000052C ]
length of pbuf = 2104
Looking up [40000052C] in database
aut_result=    [DJNJBEMG@HGFJDJH]
mes_aut_result=[FHCACFICNGNAF@EG]
MAC NOT OK";
		#endregion
		#endregion

		#region TestPreviousFailureROHAKL_S00076831
		public void TestPreviousFailureROHAKL_S00076831()
		{
			TestClientFailures(MessageROHAKL_S00076831, DebugTextROHAKL_S00076831);
		}
		#region Message
		const string MessageROHAKL_S00076831 = @"UNH+4042+CUSDEC:D:96B:UN'
BGM+929+S00076831+9'
CST++11:105:143'
LOC+9+DEBRV'
LOC+11+NZAKL'
LOC+41+NZAKL'
DTM+151:20050903:102'
GIS+NPG:110:143'
GIS+NSG:110:143'
MEA+WT+AAD+KGM:587'
EQD+CN+MAEU8346677++++7'
RFF+BM:HAM083503'
PAC+0++PK'
RFF+BM:55002000102'
RFF+AAQ:MAEU8346677'
PAC+2++PK'
TDT+20+517+1+++++:::NELE MAERSK'
NAD+AL+:ZZZ:143+LENE DALSGARD'
NAD+CB+00303221D:ZZZ:143'
UNS+D'
DMS+BAGGAGE+935'
TOD+++FOB:106:143'
CST+1+9805003000J:169:143'
FTX+AAA+++W1-LEGAL AUTH TO WORK IN NZ 12 MTHS OR GREATER'
LOC+27+DK'
LOC+35+DK'
NAD+SU+:ZZZ:143+LENE DALSGARD'
MOA+14:12500.00:NZD'
CUX+2++1.00'
MOA+40:12500'
MOA+64:1'
MOA+70:0'
DOC+AF4:147:143+00400186201'
GIS+Y:109:143'
GIS+AWC:110:143'
GIS+CZ1:110:143:DK'
GIS+CZ2:110:143:DK'
GIS+LCA:110:143'
GIS+NPU:110:143'
GIS+PP1:110:143:100974122'
GIS+PP2:110:143:200418283'
UNS+S'
CNT+4:1'
CNT+5:1'
CNT+11:2'
TAX+3+CUD++12500'
MOA+161:0.00'
AUT+AALACGI@ABC@H@EL+40000052C'
UNT+49+4042'";
		#endregion
		#region DebugText
		const string DebugTextROHAKL_S00076831 = @"arg1 = M0029065609
arg2 = /appl/ediprd/ediedata/xlate/0008MJOI.TL0
arg3 = T/E
arg4 = 1
arg5 = 26810
argc = 7
symbolic_dir_name=CUSMOD
class entry    =[929]
client_ref     =[S00076831]
tran type      =[9]
entry type     =[11]
port loading   =[DEBRV]
port discharge =[NZAKL]
customs control=[]
processing port=[NZAKL]
country of dest=[]
date export    =[20050903]
date import    =[]
entry period   =[]
override ind   =[]
sold_ind       =[]
other info  1  =[NPG]
other data  1  =[]
other info  2  =[NSG]
other data  2  =[]
other info  3  =[]
other data  3  =[]
other info  4  =[]
other data  4  =[]
other info  5  =[]
other data  5  =[]
other info  6  =[]
other data  6  =[]
other info  7  =[]
other data  7  =[]
other info  8  =[]
other data  8  =[]
other info  9  =[]
other data  9  =[]
other info 10  =[]
other data 10  =[]
total weight   =[587]
block_a=[929S00076831911DEBRVNZAKLNZAKL20050903NPGNSG587 ]
block_c number =0
container type q   =[]
container number   =[]
container status   =[]
number packages    =[0]
type packages      =[PK]
seal numbers       =[]
block_c=[BMHAM0835030PK  ]
block_c number =1
container type q   =[AAQ]
container number   =[MAEU8346677]
container status   =[7]
number packages    =[2]
type packages      =[PK]
seal numbers       =[]
block_c=[BM55002000102AAQMAEU834667772PK ]
voyage_number      =[517]
transport mode     =[1]
craft flight no    =[NELE MAERSK]
permit auth code 1 =[]
permit auth no 1   =[]
permit auth code 2 =[]
permit auth no 2   =[]
permit auth code 3 =[]
permit auth no 3   =[]
permit auth code 4 =[]
permit auth no 4   =[]
permit auth code 5 =[]
permit auth no 5   =[]
permit auth code 6 =[]
permit auth no 6   =[]
permit auth code 7 =[]
permit auth no 7   =[]
permit auth code 8 =[]
permit auth no 8   =[]
permit auth code 9 =[]
permit auth no 9   =[]
permit auth code10 =[]
permit auth no10   =[]
block_d=[5171NELE MAERSK ]
client code  =[]
client name  =[LENE DALSGARD]
broker code  =[00303221D]
del auth code=[]
block_e=[LENE DALSGARD00303221D  ]
invoice block number 0
invoice number =[BAGGAGE]
invoice terms  =[FOB]
block_f=[BAGGAGEFOB      ]
line_number         =[1]
tariff item         =[9805003000J]
cons code           =[]
goods descr         =[W1-LEGAL AUTH TO WORK IN NZ 12 MTHS OR GREATER]
country origin      =[DK]
country export      =[DK]
stat unit           =[]
stat qty            =[]
supp unit           =[]
supp qty            =[]
supplier code       =[]
supplier name       =[LENE DALSGARD]
value in curr       =[12500.00]
curr code           =[NZD]
value in nz         =[12500]
freight             =[1]
insurance           =[0]
exch rate           =[1.00]
exch ind            =[]
permit auth code  1 =[AF4]
permit auth no    1 =[00400186201]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[Y]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[AWC]
other data  1       =[]
other info  2       =[CZ1]
other data  2       =[DK]
other info  3       =[CZ2]
other data  3       =[DK]
other info  4       =[LCA]
other data  4       =[]
other info  5       =[NPU]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[]
duty credit         =[]
gst                 =[]
pref ind            =[]
block_g=[19805003000JW1-LEGAL AUTH TO WORK IN NZ 12 MTHS OR GREATERDKDKLENE DALSGARD12500.00NZD12500101.00AF400400186201YAWCCZ1DKCZ2DKLCANPU     ]
total_invoices      =[1]
total_lines         =[1]
total_packages      =[2]
total_alac          =[]
total_hera          =[]
total_anti dump     =[]
total_countervailing=[]
total_tariff duty   =[0.00]
total_value in nz   =[12500]
total_gst           =[]
total_amount        =[]
total_duty credits  =[]
method of payment   =[]
declarant code      =[40000052C]
block_h=[1120.001250040000052C   ]
pbuf=[929S00076831911DEBRVNZAKLNZAKL20050903NPGNSG587 BMHAM0835030PK  BM55002000102AAQMAEU834667772PK 5171NELE MAERSK LENE DALSGARD00303221D  BAGGAGEFOB      19805003000JW1-LEGAL AUTH TO WORK IN NZ 12 MTHS OR GREATERDKDKLENE DALSGARD12500.00NZD12500101.00AF400400186201YAWCCZ1DKCZ2DKLCANPU     1120.001250040000052C   ]
length of pbuf = 312
Looking up [40000052C] in database
aut_result=    [@JINKODILDBM@FFN]
mes_aut_result=[AALACGI@ABC@H@EL]
MAC NOT OK
";
		#endregion
		#endregion

		#region TestPreviousFailureEDISYD_B00001133
		public void TestPreviousFailureEDISYD_B00001133()
		{
			TestClientFailures(MessageEDISYD_B00001133, DebugTextEDISYD_B00001133);
		}
		#region Message
		const string MessageEDISYD_B00001133 = @"UNH+528+CUSDEC:D:96B:UN+24573378'
BGM+929+B00001133+5'
CST++53:105:143'
LOC+9+AUSYD'
LOC+11+NZAKL'
LOC+41+NZAKL'
DTM+324:200603:610'
MEA+WT+AAD+KGM:23000000'
FTX+AAI+++HOPE THIS FIXES IT'
RFF+BM:OCEANISWET'
PAC+0++PK'
RFF+BM:SOISWATER'
PAC+1++VL'
TDT+20+219+1+++++:::BUNGA BIDARA'
NAD+AL+00782903F:ZZZ:143'
NAD+CB+00009917B:ZZZ:143'
UNS+D'
CST+1+2710191111F:169:143'
FTX+AAA+++MOTOR SPIRIT IN BULK'
LOC+27+AU'
LOC+35+AU'
MEA+AAR++LTR:23000000.000'
MEA+AAS++GPB:1.000'
NAD+SU+00710841Y:ZZZ:143'
MOA+14:1209091.00:NZD'
CUX+2++1.00'
MOA+40:1209091'
MOA+64:0'
MOA+70:0'
GIS+Y:109:143'
TAX+1+CST+ACC:167:143'
MOA+161:1168400.00'
TAX+1+CUD'
MOA+161:8326000.00'
TAX+1+GST'
MOA+161:1337936.38'
UNS+S'
CNT+5:1'
CNT+11:1'
TAX+3+CST+ACC:167:143'
MOA+161:1168400.00'
TAX+3+CUD++1209091'
MOA+161:8326000.00'
TAX+3+GST'
MOA+161:1337936.38'
TAX+4+TOT'
MOA+161:10832336.38'
GIS+D:134:143'
AUT+COMJANIEMMABAFEK+65432198B'
UNT+50+528'";
		#endregion
		#region DebugText
		const string DebugTextEDISYD_B00001133 = @"symbolic_dir_name=CUSSWT
class entry    =[929]
client_ref     =[B00001133]
tran type      =[5]
entry type     =[53]
port loading   =[AUSYD]
port discharge =[NZAKL]
customs control=[]
processing port=[NZAKL]
country of dest=[]
date export    =[]
date import    =[]
entry period   =[200603]
override ind   =[]
sold_ind       =[]
other info  1  =[]
other data  1  =[]
other info  2  =[]
other data  2  =[]
other info  3  =[]
other data  3  =[]
other info  4  =[]
other data  4  =[]
other info  5  =[]
other data  5  =[]
other info  6  =[]
other data  6  =[]
other info  7  =[]
other data  7  =[]
other info  8  =[]
other data  8  =[]
other info  9  =[]
other data  9  =[]
other info 10  =[]
other data 10  =[]
total weight   =[23000000]
block_a=[929B00001133553AUSYDNZAKLNZAKL20060323000000    ]
block_b=[HOPE THIS FIXES IT      ]
block_c number =0
container type q   =[]
container number   =[]
container status   =[]
number packages    =[0]
type packages      =[PK]
seal numbers       =[]
block_c=[BMOCEANISWET0PK ]
block_c number =1
container type q   =[]
container number   =[]
container status   =[]
number packages    =[1]
type packages      =[VL]
seal numbers       =[]
block_c=[BMSOISWATER1VL  ]
voyage_number      =[219]
transport mode     =[1]
craft flight no    =[BUNGA BIDARA]
permit auth code 1 =[]
permit auth no 1   =[]
permit auth code 2 =[]
permit auth no 2   =[]
permit auth code 3 =[]
permit auth no 3   =[]
permit auth code 4 =[]
permit auth no 4   =[]
permit auth code 5 =[]
permit auth no 5   =[]
permit auth code 6 =[]
permit auth no 6   =[]
permit auth code 7 =[]
permit auth no 7   =[]
permit auth code 8 =[]
permit auth no 8   =[]
permit auth code 9 =[]
permit auth no 9   =[]
permit auth code10 =[]
permit auth no10   =[]
block_d=[2191BUNGA BIDARA]
client code  =[00782903F]
client name  =[]
broker code  =[00009917B]
del auth code=[]
block_e=[00782903F00009917B      ]
line_number         =[1]
tariff item         =[2710191111F]
cons code           =[]
goods descr         =[MOTOR SPIRIT IN BULK]
country origin      =[AU]
country export      =[AU]
stat unit           =[LTR]
stat qty            =[23000000.000]
supp unit           =[GPB]
supp qty            =[1.000]
supplier code       =[00710841Y]
supplier name       =[]
value in curr       =[1209091.00]
curr code           =[NZD]
value in nz         =[1209091]
freight             =[0]
insurance           =[0]
exch rate           =[1.00]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[Y]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[1168400.00]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[8326000.00]
duty credit         =[]
gst                 =[1337936.38]
pref ind            =[]
block_g=[12710191111FMOTOR SPIRIT IN BULKAUAULTR23000000.000GPB1.00000710841Y1209091.00NZD1209091001.00Y1168400.008326000.001337936.38   ]
total_invoices      =[]
total_lines         =[1]
total_packages      =[1]
total_alac          =[]
total_hera          =[1168400.00]
total_anti dump     =[]
total_countervailing=[]
total_tariff duty   =[8326000.00]
total_value in nz   =[1209091]
total_gst           =[1337936.38]
total_amount        =[10832336.38]
total_duty credits  =[]
method of payment   =[D]
declarant code      =[65432198B]
block_h=[111168400.008326000.0012090911337936.3810832336.38D65432198B    ]
pbuf=[929B00001133553AUSYDNZAKLNZAKL20060323000000    HOPE THIS FIXES IT      BMOCEANISWET0PK BMSOISWATER1VL  2191BUNGA BIDARA00782903F00009917B      12710191111FMOTOR SPIRIT IN BULKAUAULTR23000000.000GPB1.00000710841Y1209091.00NZD1209091001.00Y1168400.008326000.001337936.38   111168400.008326000.0012090911337936.3810832336.38D65432198B    ]
length of pbuf = 336
Looking up [65432198B] in test file
aut_result=    [DMBJDFFDFHF@I@DG]
mes_aut_result=[COMJANIEMMABAFEK]
MAC NOT OK
";
		#endregion
		#endregion

		#region TestPreviousFailureEDISYD_B00001134
		public void TestPreviousFailureEDISYD_B00001134()
		{
			TestClientFailures(MessageEDISYD_B00001134, DebugTextEDISYD_B00001134);
		}
		#region Message
		const string MessageEDISYD_B00001134 = @"UNH+525+CUSDEC:D:96B:UN+09260515'
BGM+929+B00001134+5'
CST++53:105:143'
LOC+9+AUSYD'
LOC+11+NZAKL'
LOC+41+NZAKL'
DTM+324:200602:610'
MEA+WT+AAD+KGM:23000000'
FTX+AAI+++LESS ERRORS'
RFF+BM:MOREWATER'
PAC+0++PK'
RFF+BM:AGAINITSWATER'
PAC+1++VL'
TDT+20+219+1+++++:::BUNGA BIDARA'
NAD+AL+00782903F:ZZZ:143'
NAD+CB+00009917B:ZZZ:143'
UNS+D'
CST+1+2710191111F:169:143'
FTX+AAA+++MOTOR SPIRIT IN BULK'
LOC+27+AU'
LOC+35+AU'
MEA+AAR++LTR:23000000.000'
MEA+AAS++:1.000'
NAD+SU+00710841Y:ZZZ:143'
MOA+14:1209091.00:NZD'
CUX+2++1.00'
MOA+40:1209091'
MOA+64:0'
MOA+70:0'
GIS+Y:109:143'
TAX+1+CST+ACC:167:143'
MOA+161:1168400.00'
TAX+1+CUD'
MOA+161:8326000.00'
TAX+1+GST'
MOA+161:1337936.38'
UNS+S'
CNT+5:1'
CNT+11:1'
TAX+3+CST+ACC:167:143'
MOA+161:1168400.00'
TAX+3+CUD++1209091'
MOA+161:8326000.00'
TAX+3+GST'
MOA+161:1337936.38'
TAX+4+TOT'
MOA+161:10832336.38'
GIS+B:134:143'
AUT+GNKBLI@FOKHHM@GO+65432198B'
UNT+50+525'";
		#endregion
		#region DebugText
		const string DebugTextEDISYD_B00001134 = @"symbolic_dir_name=CUSSWT
class entry    =[929]
client_ref     =[B00001134]
tran type      =[5]
entry type     =[53]
port loading   =[AUSYD]
port discharge =[NZAKL]
customs control=[]
processing port=[NZAKL]
country of dest=[]
date export    =[]
date import    =[]
entry period   =[200602]
override ind   =[]
sold_ind       =[]
other info  1  =[]
other data  1  =[]
other info  2  =[]
other data  2  =[]
other info  3  =[]
other data  3  =[]
other info  4  =[]
other data  4  =[]
other info  5  =[]
other data  5  =[]
other info  6  =[]
other data  6  =[]
other info  7  =[]
other data  7  =[]
other info  8  =[]
other data  8  =[]
other info  9  =[]
other data  9  =[]
other info 10  =[]
other data 10  =[]
total weight   =[23000000]
block_a=[929B00001134553AUSYDNZAKLNZAKL20060223000000    ]
block_b=[LESS ERRORS     ]
block_c number =0
container type q   =[]
container number   =[]
container status   =[]
number packages    =[0]
type packages      =[PK]
seal numbers       =[]
block_c=[BMMOREWATER0PK  ]
block_c number =1
container type q   =[]
container number   =[]
container status   =[]
number packages    =[1]
type packages      =[VL]
seal numbers       =[]
block_c=[BMAGAINITSWATER1VL      ]
voyage_number      =[219]
transport mode     =[1]
craft flight no    =[BUNGA BIDARA]
permit auth code 1 =[]
permit auth no 1   =[]
permit auth code 2 =[]
permit auth no 2   =[]
permit auth code 3 =[]
permit auth no 3   =[]
permit auth code 4 =[]
permit auth no 4   =[]
permit auth code 5 =[]
permit auth no 5   =[]
permit auth code 6 =[]
permit auth no 6   =[]
permit auth code 7 =[]
permit auth no 7   =[]
permit auth code 8 =[]
permit auth no 8   =[]
permit auth code 9 =[]
permit auth no 9   =[]
permit auth code10 =[]
permit auth no10   =[]
block_d=[2191BUNGA BIDARA]
client code  =[00782903F]
client name  =[]
broker code  =[00009917B]
del auth code=[]
block_e=[00782903F00009917B      ]
line_number         =[1]
tariff item         =[2710191111F]
cons code           =[]
goods descr         =[MOTOR SPIRIT IN BULK]
country origin      =[AU]
country export      =[AU]
stat unit           =[LTR]
stat qty            =[23000000.000]
supp unit           =[]
supp qty            =[1.000]
supplier code       =[00710841Y]
supplier name       =[]
value in curr       =[1209091.00]
curr code           =[NZD]
value in nz         =[1209091]
freight             =[0]
insurance           =[0]
exch rate           =[1.00]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[Y]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[1168400.00]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[8326000.00]
duty credit         =[]
gst                 =[1337936.38]
pref ind            =[]
block_g=[12710191111FMOTOR SPIRIT IN BULKAUAULTR23000000.0001.00000710841Y1209091.00NZD1209091001.00Y1168400.008326000.001337936.38      ]
total_invoices      =[]
total_lines         =[1]
total_packages      =[1]
total_alac          =[]
total_hera          =[1168400.00]
total_anti dump     =[]
total_countervailing=[]
total_tariff duty   =[8326000.00]
total_value in nz   =[1209091]
total_gst           =[1337936.38]
total_amount        =[10832336.38]
total_duty credits  =[]
method of payment   =[B]
declarant code      =[65432198B]
block_h=[111168400.008326000.0012090911337936.3810832336.38B65432198B    ]
pbuf=[929B00001134553AUSYDNZAKLNZAKL20060223000000    LESS ERRORS     BMMOREWATER0PK  BMAGAINITSWATER1VL      2191BUNGA BIDARA00782903F00009917B      12710191111FMOTOR SPIRIT IN BULKAUAULTR23000000.0001.00000710841Y1209091.00NZD1209091001.00Y1168400.008326000.001337936.38      111168400.008326000.0012090911337936.3810832336.38B65432198B    ]
length of pbuf = 336
Looking up [65432198B] in test file
aut_result=    [NHECM@CNNJGANC@F]
mes_aut_result=[GNKBLI@FOKHHM@GO]
MAC NOT OK
";
		#endregion
		#endregion

		#region TestPreviousFailureELAAKL_B00023132
		public void TestPreviousFailureELAAKL_B00023132()
		{
			TestClientFailures(MessageELAAKL_B00023132, DebugTextELAAKL_B00023132);
		}
		#region Message
		const string MessageELAAKL_B00023132 = @"UNH+10807+CUSDEC:D:96B:UN+53514140'
BGM+929+B00023132+5'
CST++10:105:143'
LOC+9+AUSYD'
LOC+11+NZAKL'
LOC+41+NZAKL'
DTM+151:20080503:102'
GIS+PDO:110:143'
MEA+WT+AAD+KGM:33'
FTX+AAI+++E F'
RFF+MB:12537947394'
RFF+HWB:BUD30019183'
PAC+4++PK'
TDT+20++4+++++:::QF49'
NAD+AL+40037577B:ZZZ:143'
NAD+CB+40195219F:ZZZ:143'
UNS+D'
DMS+320130+935'
TOD+++FOB:106:143'
DMS+320128+935'
TOD+++FOB:106:143'
DMS+320129+935'
TOD+++FOB:106:143'
DMS+320131+935'
TOD+++FOB:106:143'
CST+4+3923210100B:169:143'
FTX+AAA+++PRINTED PLASTIC BAGS WITH HANDLES'
LOC+27+CN'
LOC+35+HU'
NAD+SU+00166733T:ZZZ:143'
MOA+14:160.00:USD'
CUX+2++0.79'
MOA+40:203'
MOA+64:8'
MOA+70:1'
GIS+N:109:143'
TAX+1+CUD'
MOA+161:15.23'
TAX+1+GST'
MOA+161:28.40'
CST+1+6112410200F:169:143'
FTX+AAA+++WOMENS SWIMWEAR'
LOC+27+CN'
LOC+35+HU'
MEA+AAR++NMB:59.000'
NAD+SU+00166733T:ZZZ:143'
MOA+14:2274.00:USD'
CUX+2++0.79'
MOA+40:2878'
MOA+64:117'
MOA+70:7'
GIS+N:109:143'
TAX+1+CUD'
MOA+161:431.70'
TAX+1+GST'
MOA+161:429.21'
CST+2+6211430000L:169:143'
FTX+AAA+++OTHER GARMENTS WOMENS MAN-MADE'
LOC+27+CN'
LOC+35+HU'
MEA+AAR++NMB:14.000'
NAD+SU+00166733T:ZZZ:143'
MOA+14:721.00:USD'
CUX+2++0.79'
MOA+40:913'
MOA+64:37'
MOA+70:2'
GIS+N:109:143'
TAX+1+CUD'
MOA+161:136.95'
TAX+1+GST'
MOA+161:136.12'
CST+3+6211430000L:169:143'
FTX+AAA+++OTHER GARMENTS WOMENS MAN-MADE'
LOC+27+TR'
LOC+35+HU'
MEA+AAR++NMB:57.000'
NAD+SU+00166733T:ZZZ:143'
MOA+14:2381.49:USD'
CUX+2++0.79'
MOA+40:3015'
MOA+64:123'
MOA+70:8'
GIS+N:109:143'
TAX+1+CUD'
MOA+161:452.25'
TAX+1+GST'
MOA+161:449.78'
UNS+S'
CNT+4:4'
CNT+5:4'
CNT+11:4'
TAX+3+CUD++7009'
MOA+161:1036.13'
TAX+3+GST'
MOA+161:1043.51'
TAX+4+TOT'
MOA+161:2079.64'
GIS+D:134:143'
AUT+GHHAJIMOKIEOI@DL+40000641F'
UNT+101+10807'";
		#endregion
		#region DebugText
		const string DebugTextELAAKL_B00023132 = @"arg1 = M0044148309
arg2 = /appl/ediprd/ediedata/xlate/000CS8JP.TL0
arg3 = T/G
arg4 = 1
arg5 = 4047
argc = 7
symbolic_dir_name=CUSMOD
class entry    =[929]
client_ref     =[B00023132]
tran type      =[5]
entry type     =[10]
port loading   =[AUSYD]
port discharge =[NZAKL]
customs control=[]
processing port=[NZAKL]
country of dest=[]
date export    =[20080503]
date import    =[]
entry period   =[]
override ind   =[]
sold_ind       =[]
other info  1  =[PDO]
other data  1  =[]
other info  2  =[]
other data  2  =[]
other info  3  =[]
other data  3  =[]
other info  4  =[]
other data  4  =[]
other info  5  =[]
other data  5  =[]
other info  6  =[]
other data  6  =[]
other info  7  =[]
other data  7  =[]
other info  8  =[]
other data  8  =[]
other info  9  =[]
other data  9  =[]
other info 10  =[]
other data 10  =[]
total weight   =[33]
block_a=[929B00023132510AUSYDNZAKLNZAKL20080503PDO33     ]
block_b=[E F     ]
block_c number =0
container type q   =[]
container number   =[]
container status   =[]
number packages    =[]
type packages      =[]
seal numbers       =[]
block_c=[MB12537947394   ]
block_c number =1
container type q   =[]
container number   =[]
container status   =[]
number packages    =[4]
type packages      =[PK]
seal numbers       =[]
block_c=[HWBBUD300191834PK       ]
voyage_number      =[]
transport mode     =[4]
craft flight no    =[QF49]
permit auth code 1 =[]
permit auth no 1   =[]
permit auth code 2 =[]
permit auth no 2   =[]
permit auth code 3 =[]
permit auth no 3   =[]
permit auth code 4 =[]
permit auth no 4   =[]
permit auth code 5 =[]
permit auth no 5   =[]
permit auth code 6 =[]
permit auth no 6   =[]
permit auth code 7 =[]
permit auth no 7   =[]
permit auth code 8 =[]
permit auth no 8   =[]
permit auth code 9 =[]
permit auth no 9   =[]
permit auth code10 =[]
permit auth no10   =[]
block_d=[4QF49   ]
client code  =[40037577B]
client name  =[]
broker code  =[40195219F]
del auth code=[]
block_e=[40037577B40195219F      ]
invoice block number 0
invoice number =[320130]
invoice terms  =[FOB]
block_f=[320130FOB       ]
invoice block number 1
invoice number =[320128]
invoice terms  =[FOB]
block_f=[320128FOB       ]
invoice block number 2
invoice number =[320129]
invoice terms  =[FOB]
block_f=[320129FOB       ]
invoice block number 3
invoice number =[320131]
invoice terms  =[FOB]
block_f=[320131FOB       ]
line_number         =[1]
tariff item         =[6112410200F]
cons code           =[]
goods descr         =[WOMENS SWIMWEAR]
country origin      =[CN]
country export      =[HU]
stat unit           =[NMB]
stat qty            =[59.000]
supp unit           =[]
supp qty            =[]
supplier code       =[00166733T]
supplier name       =[]
value in curr       =[2274.00]
curr code           =[USD]
value in nz         =[2878]
freight             =[117]
insurance           =[7]
exch rate           =[0.79]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[N]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[431.70]
duty credit         =[]
gst                 =[429.21]
pref ind            =[]
block_g=[16112410200FWOMENS SWIMWEARCNHUNMB59.00000166733T2274.00USD287811770.79N431.70429.21    ]
line_number         =[2]
tariff item         =[6211430000L]
cons code           =[]
goods descr         =[OTHER GARMENTS WOMENS MAN-MADE]
country origin      =[CN]
country export      =[HU]
stat unit           =[NMB]
stat qty            =[14.000]
supp unit           =[]
supp qty            =[]
supplier code       =[00166733T]
supplier name       =[]
value in curr       =[721.00]
curr code           =[USD]
value in nz         =[913]
freight             =[37]
insurance           =[2]
exch rate           =[0.79]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[N]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[136.95]
duty credit         =[]
gst                 =[136.12]
pref ind            =[]
block_g=[26211430000LOTHER GARMENTS WOMENS MAN-MADECNHUNMB14.00000166733T721.00USD9133720.79N136.95136.12]
line_number         =[3]
tariff item         =[6211430000L]
cons code           =[]
goods descr         =[OTHER GARMENTS WOMENS MAN-MADE]
country origin      =[TR]
country export      =[HU]
stat unit           =[NMB]
stat qty            =[57.000]
supp unit           =[]
supp qty            =[]
supplier code       =[00166733T]
supplier name       =[]
value in curr       =[2381.49]
curr code           =[USD]
value in nz         =[3015]
freight             =[123]
insurance           =[8]
exch rate           =[0.79]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[N]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[452.25]
duty credit         =[]
gst                 =[449.78]
pref ind            =[]
block_g=[36211430000LOTHER GARMENTS WOMENS MAN-MADETRHUNMB57.00000166733T2381.49USD301512380.79N452.25449.78     ]
line_number         =[4]
tariff item         =[3923210100B]
cons code           =[]
goods descr         =[PRINTED PLASTIC BAGS WITH HANDLES]
country origin      =[CN]
country export      =[HU]
stat unit           =[]
stat qty            =[]
supp unit           =[]
supp qty            =[]
supplier code       =[00166733T]
supplier name       =[]
value in curr       =[160.00]
curr code           =[USD]
value in nz         =[203]
freight             =[8]
insurance           =[1]
exch rate           =[0.79]
exch ind            =[]
permit auth code  1 =[]
permit auth no    1 =[]
permit auth code  2 =[]
permit auth no    2 =[]
permit auth code  3 =[]
permit auth no    3 =[]
permit auth code  4 =[]
permit auth no    4 =[]
permit auth code  5 =[]
permit auth no    5 =[]
relation ind        =[N]
proh code 1         =[]
proh code 2         =[]
proh code 3         =[]
other info  1       =[]
other data  1       =[]
other info  2       =[]
other data  2       =[]
other info  3       =[]
other data  3       =[]
other info  4       =[]
other data  4       =[]
other info  5       =[]
other data  5       =[]
alac levy           =[]
hera levy           =[]
anti dump           =[]
countervailing      =[]
tariff duty         =[]
import duty         =[15.23]
duty credit         =[]
gst                 =[28.40]
pref ind            =[]
block_g=[43923210100BPRINTED PLASTIC BAGS WITH HANDLESCNHU00166733T160.00USD203810.79N15.2328.40 ]
total_invoices      =[4]
total_lines         =[4]
total_packages      =[4]
total_alac          =[]
total_hera          =[]
total_anti dump     =[]
total_countervailing=[]
total_tariff duty   =[1036.13]
total_value in nz   =[7009]
total_gst           =[1043.51]
total_amount        =[2079.64]
total_duty credits  =[]
method of payment   =[D]
declarant code      =[40000641F]
block_h=[4441036.1370091043.512079.64D40000641F  ]
pbuf=[929B00023132510AUSYDNZAKLNZAKL20080503PDO33     E F     MB12537947394   HWBBUD300191834PK       4QF49   40037577B40195219F      320130FOB       320128FOB       320129FOB       320131FOB       16112410200FWOMENS SWIMWEARCNHUNMB59.00000166733T2274.00USD287811770.79N431.70429.21    26211430000LOTHER GARMENTS WOMENS MAN-MADECNHUNMB14.00000166733T721.00USD9133720.79N136.95136.1236211430000LOTHER GARMENTS WOMENS MAN-MADETRHUNMB57.00000166733T2381.49USD301512380.79N452.25449.78     43923210100BPRINTED PLASTIC BAGS WITH HANDLESCNHU00166733T160.00USD203810.79N15.2328.40 4441036.1370091043.512079.64D40000641F  ]
length of pbuf = 608
Looking up [40000641F] in database
aut_result=    [HDIBCGMDMLNBGGAL]
mes_aut_result=[GHHAJIMOKIEOI@DL]
MAC NOT OK
";
		#endregion
		#endregion

		//            #region TestPreviousFailureXXXAKL_X00000000
		//            public void TestPreviousFailureXXXAKL_X00000000()
		//            {
		//                TestClientFailures(MessageXXXAKL_X00000000, DebugTextXXXAKL_X00000000);
		//            }
		//            #region Message
		//            const string MessageXXXAKL_X00000000 = @"UNH+20332+CUSDEC:D:96B:UN'
		//BGM+929+X00000000+9'
		//...
		//UNT+47+20332'";
		//            #endregion
		//            #region DebugText
		//            const string DebugTextXXXAKL_X00000000 = @"symbolic_dir_name=CUSMOD
		//class entry    =[929]
		//client_ref     =[X00000000]
		//tran type      =[9]
		//...
		//declarant code      =[40017856K]
		//block_h=[1110.007023933.00933.00D40017856K       ]
		//pbuf=[929S00125127910AUMELNZAKLNZAKL20060715241       MB17257646374   HWBS00125127PCE 4CV7395 00376606D00303221D      103714FOB       12106909979AVITAMIN COMPOUNDS & LIQUID VITAMIN SUPPLMENTS, AND OTHER FOOD &BEVERAGE PREPARAAUAUKGM241.00000238368Q5899.44AUD702343290.84AF100100428798NNBF933.00Q       1110.007023933.00933.00D40017856K       ]
		//length of pbuf = 336
		//Looking up [40017856K] in database
		//aut_result=    [NNFMKFFHEGADG@OH]
		//mes_aut_result=[CAKNLIEABCDC@JED]
		//MAC NOT OK
		//";
		//            #endregion
		//            #endregion

		#region Implementation
		void TestClientFailures(string message, string debugText)
		{
			pinBuilder.GenerateBlocksFromEDIFACTMessageText(message.Replace("\r\n", ""));
			string actualDebugText = pinBuilder.GetPINDebugText();
			string expected = GetDataBlockLinesOnly(debugText);
			string actual = GetDataBlockLinesOnly(actualDebugText);
			if (expected != actual)
			{
				AssertMultilineEquals("Debug Blocks Should Match.", expected + "\r\nExpected Text:\r\n" + debugText, actual + "\r\nActual Text:\r\n" + actualDebugText, '\n');
			}
			else
			{
				Assert(true);
			}
		}

		string GetDataBlockLinesOnly(string debugText)
		{
			StringBuilder result = new StringBuilder();
			string[] lines = debugText.Replace("\r\n", "\r").Split('\r');
			foreach (string line in lines)
			{
				if ((line.StartsWith("block_") && line.IndexOf("=[]") == -1) && line.Substring(7, 1) == "=" || line.StartsWith("pbuf="))
				{
					result.Append(line + "\r\n");
				}
			}
			return result.ToString();
		}
		#endregion
		#endregion

		#region Implementation
		protected string expectedPBUF;
		PinBuilder_ForTesting pinBuilder;
		protected override void SetUp()
		{
			base.SetUp();

			pinBuilder = new PinBuilder_ForTesting(CurrentUsersPin.TestSystemPinCode);
		}

		protected void TestAgainstMessageStringFromVFP(string messageString)
		{
			pinBuilder.GenerateBlocksFromEDIFACTMessageText(messageString.Replace("\r\n", "'"));
			Assert("Make sure the Message has a valid AUT segment", pinBuilder.Get_MessageMAC().Length == 16);
			AssertEquals(pinBuilder.Get_MessageMAC(), pinBuilder.GetMAC());
		}

		#region SetupTestData

		protected void SetupTestData()
		{
			pinBuilder.Get_HeaderData().AddDataField("class entry    ", "929");
			pinBuilder.Get_HeaderData().AddDataField("client_ref     ", "111161");
			pinBuilder.Get_HeaderData().AddDataField("tran type      ", "9");
			pinBuilder.Get_HeaderData().AddDataField("entry type     ", "10");
			pinBuilder.Get_HeaderData().AddDataField("port loading   ", "AUSYD");
			pinBuilder.Get_HeaderData().AddDataField("port discharge ", "NZAKL");
			pinBuilder.Get_HeaderData().AddDataField("customs control", "7294L");
			pinBuilder.Get_HeaderData().AddDataField("processing port", "NZAKL");
			pinBuilder.Get_HeaderData().AddDataField("country of dest", "");
			pinBuilder.Get_HeaderData().AddDataField("date export    ", "");
			pinBuilder.Get_HeaderData().AddDataField("date import    ", "20040621");
			pinBuilder.Get_HeaderData().AddDataField("entry period   ", "");
			pinBuilder.Get_HeaderData().AddDataField("override ind   ", "");
			pinBuilder.Get_HeaderData().AddDataField("sold_ind       ", "");
			pinBuilder.Get_HeaderData().AddDataField("other info  1  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other data  1  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other info  2  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other data  2  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other info  3  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other data  3  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other info  4  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other data  4  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other info  5  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other data  5  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other info  6  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other data  6  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other info  7  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other data  7  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other info  8  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other data  8  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other info  9  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other data  9  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other info 10  ", "");
			pinBuilder.Get_HeaderData().AddDataField("other data 10  ", "");
			pinBuilder.Get_HeaderData().AddDataField("total weight   ", "1000");
			string expectedHeader = "929111161910AUSYDNZAKL7294LNZAKL200406211000    ";
			AssertEquals(expectedHeader, pinBuilder.Get_HeaderData().GetPinGenerationBlock());

			//			PinBuilder.Remarks.AddDataField("dd", "dd");

			pinBuilder.Call_AddNewPackaging();
			// Not Shown in Debug File
			pinBuilder.Get_CurrentPackaging().AddDataField("reference type q    ", "BM");
			pinBuilder.Get_CurrentPackaging().AddDataField("reference number    ", "HOUSEJOHN");
			// Not Shown in Debug File
			pinBuilder.Get_CurrentPackaging().AddDataField("container type q   ", "AAQ");
			pinBuilder.Get_CurrentPackaging().AddDataField("container number   ", "OOCL0000006");
			pinBuilder.Get_CurrentPackaging().AddDataField("container status   ", "7");
			pinBuilder.Get_CurrentPackaging().AddDataField("number packages    ", "100");
			pinBuilder.Get_CurrentPackaging().AddDataField("type packages      ", "PK");
			pinBuilder.Get_CurrentPackaging().AddDataField("seal numbers       ", "");
			string expectedPackaging1 = "BMHOUSEJOHNAAQOOCL00000067100PK ";
			AssertEquals(expectedPackaging1, pinBuilder.Get_CurrentPackaging().GetPinGenerationBlock());

			pinBuilder.Call_AddNewPackaging();
			// Not Shown in Debug File
			pinBuilder.Get_CurrentPackaging().AddDataField("reference type q    ", "BM");
			pinBuilder.Get_CurrentPackaging().AddDataField("reference number    ", "HOUSEJOHN");
			// Not Shown in Debug File
			pinBuilder.Get_CurrentPackaging().AddDataField("container type q   ", "AAQ");
			pinBuilder.Get_CurrentPackaging().AddDataField("container number   ", "OOCL0000006");
			pinBuilder.Get_CurrentPackaging().AddDataField("container status   ", "7");
			pinBuilder.Get_CurrentPackaging().AddDataField("number packages    ", "50");
			pinBuilder.Get_CurrentPackaging().AddDataField("type packages      ", "BA");
			pinBuilder.Get_CurrentPackaging().AddDataField("seal numbers       ", "");
			string expectedPackaging2 = "BMHOUSEJOHNAAQOOCL0000006750BA  ";
			AssertEquals(expectedPackaging2, pinBuilder.Get_CurrentPackaging().GetPinGenerationBlock());

			pinBuilder.Get_TransportAndPermitDetails().AddDataField("voyage_number      ", "2828");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("transport mode     ", "1");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("craft flight no    ", "ANTWERP");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth code 1 ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth no 1   ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth code 2 ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth no 2   ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth code 3 ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth no 3   ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth code 4 ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth no 4   ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth code 5 ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth no 5   ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth code 6 ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth no 6   ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth code 7 ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth no 7   ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth code 8 ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth no 8   ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth code 9 ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth no 9   ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth code10 ", "");
			pinBuilder.Get_TransportAndPermitDetails().AddDataField("permit auth no10   ", "");
			string expectedTransport = "28281ANTWERP    ";
			AssertEquals(expectedTransport, pinBuilder.Get_TransportAndPermitDetails().GetPinGenerationBlock());

			pinBuilder.Get_PartyIdentification().AddDataField("client code  ", "00782903F");
			pinBuilder.Get_PartyIdentification().AddDataField("client name  ", "");
			pinBuilder.Get_PartyIdentification().AddDataField("broker code  ", "00009917B");
			pinBuilder.Get_PartyIdentification().AddDataField("del auth code", "00727575H");
			string expectedPartyID = "00782903F00009917B00727575H     ";
			AssertEquals(expectedPartyID, pinBuilder.Get_PartyIdentification().GetPinGenerationBlock());

			pinBuilder.Call_AddNewInvoiceHeader();
			pinBuilder.Get_CurrentInvoiceHeader().AddDataField("invoice number ", "INVOICEJOHN");
			pinBuilder.Get_CurrentInvoiceHeader().AddDataField("invoice terms  ", "FOB");
			string expectedInvoiceHeader = "INVOICEJOHNFOB  ";
			AssertEquals(expectedInvoiceHeader, pinBuilder.Get_CurrentInvoiceHeader().GetPinGenerationBlock());

			pinBuilder.Call_AddNewInvoiceLine(1);
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("line_number         ", "1");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("tariff item         ", "4203101900G");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("cons code           ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("goods descr         ", "LEATHER JACKETS");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("country origin      ", "AU");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("country export      ", "AU");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("stat unit           ", "NMB");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("stat qty            ", "100.000");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("supp unit           ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("supp qty            ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("supplier code       ", "00710841Y");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("supplier name       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("value in curr       ", "5000.00");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("curr code           ", "NZD");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("value in nz         ", "5000");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("freight             ", "50");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("insurance           ", "5");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("exch rate           ", "1.00");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("exch ind            ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth code  1 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth no    1 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth code  2 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth no    2 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth code  3 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth no    3 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth code  4 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth no    4 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth code  5 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth no    5 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("relation ind        ", "N");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("proh code 1         ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("proh code 2         ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("proh code 3         ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other info  1       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other data  1       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other info  2       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other data  2       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other info  3       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other data  3       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other info  4       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other data  4       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other info  5       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other data  5       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("alac levy           ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("hera levy           ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("anti dump           ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("countervailing      ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("tariff duty         ", "1715.00");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("import duty         ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("duty credit         ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("gst                 ", "846.25");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("pref ind            ", "");
			string expectedInvoiceLine1 = "14203101900GLEATHER JACKETSAUAUNMB100.00000710841Y5000.00NZD50005051.00N1715.00846.25   ";
			AssertEquals(expectedInvoiceLine1, pinBuilder.Get_CurrentInvoiceLine().GetPinGenerationBlock());

			pinBuilder.Call_AddNewInvoiceLine(2);
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("line_number         ", "2");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("tariff item         ", "3402900011H");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("cons code           ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("goods descr         ", "DEGREASER");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("country origin      ", "AU");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("country export      ", "AU");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("stat unit           ", "KGM");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("stat qty            ", "500.000");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("supp unit           ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("supp qty            ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("supplier code       ", "00710841Y");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("supplier name       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("value in curr       ", "5000.00");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("curr code           ", "NZD");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("value in nz         ", "5000");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("freight             ", "50");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("insurance           ", "5");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("exch rate           ", "1.00");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("exch ind            ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth code  1 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth no    1 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth code  2 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth no    2 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth code  3 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth no    3 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth code  4 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth no    4 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth code  5 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("permit auth no    5 ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("relation ind        ", "N");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("proh code 1         ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("proh code 2         ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("proh code 3         ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other info  1       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other data  1       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other info  2       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other data  2       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other info  3       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other data  3       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other info  4       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other data  4       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other info  5       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("other data  5       ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("alac levy           ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("hera levy           ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("anti dump           ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("countervailing      ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("tariff duty         ", "350.00");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("import duty         ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("duty credit         ", "");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("gst                 ", "675.63");
			pinBuilder.Get_CurrentInvoiceLine().AddDataField("pref ind            ", "");
			string expectedInvoiceLine2 = "23402900011HDEGREASERAUAUKGM500.00000710841Y5000.00NZD50005051.00N350.00675.63  ";
			AssertEquals(expectedInvoiceLine2, pinBuilder.Get_CurrentInvoiceLine().GetPinGenerationBlock());

			pinBuilder.Get_Totals().AddDataField("total_invoices      ", "1");
			pinBuilder.Get_Totals().AddDataField("total_lines         ", "2");
			pinBuilder.Get_Totals().AddDataField("total_packages      ", "150");
			pinBuilder.Get_Totals().AddDataField("total_alac          ", "");
			pinBuilder.Get_Totals().AddDataField("total_hera          ", "");
			pinBuilder.Get_Totals().AddDataField("total_anti dump     ", "");
			pinBuilder.Get_Totals().AddDataField("total_countervailing", "");
			pinBuilder.Get_Totals().AddDataField("total_tariff duty   ", "2065.00");
			pinBuilder.Get_Totals().AddDataField("total_value in nz   ", "10000");
			pinBuilder.Get_Totals().AddDataField("total_gst           ", "1521.88");
			pinBuilder.Get_Totals().AddDataField("total_amount        ", "3586.88");
			pinBuilder.Get_Totals().AddDataField("total_duty credits  ", "");
			pinBuilder.Get_Totals().AddDataField("method of payment   ", "B");
			pinBuilder.Get_Totals().AddDataField("declarant code      ", "65432198B");
			string expectedTotals = "121502065.00100001521.883586.88B65432198B       ";
			AssertEquals(expectedTotals, pinBuilder.Get_Totals().GetPinGenerationBlock());

			expectedPBUF = expectedHeader + expectedPackaging1 + expectedPackaging2 + expectedTransport + expectedPartyID + expectedInvoiceHeader + expectedInvoiceLine1 + expectedInvoiceLine2 + expectedTotals;
		}
		#endregion

		#endregion

		public class PinBuilder_ForTesting : PinBuilder
		{
			public PinBuilder_ForTesting(string pinCode) : base(pinCode)
			{
			}

			public string Get_MessageMAC() => messageMAC;
			public void Set_MessageMAC(string value)
			{
				messageMAC = value;
			}
			public string Get_DeclarantCode() => declarantCode;
			public void Set_DeclarantCode(string value)
			{
				declarantCode = value;
			}
			public DataBlock Get_HeaderData() => headerData;
			public void Call_AddNewPackaging()
			{
				AddNewPackaging();
			}
			public DataBlock Get_CurrentPackaging() => CurrentPackaging;

			public DataBlock Get_TransportAndPermitDetails() => transportAndPermitDetails;

			public DataBlock Get_PartyIdentification() => partyIdentification;

			public void Call_AddNewInvoiceHeader()
			{
				AddNewInvoiceHeader();
			}

			public DataBlock Get_CurrentInvoiceHeader() => CurrentInvoiceHeader;

			public void Call_AddNewInvoiceLine(int value)
			{
				AddNewInvoiceLine(value);
			}

			public DataBlock Get_CurrentInvoiceLine() => CurrentInvoiceLine;

			public DataBlock Get_Totals() => totals;
		}
	}
}

