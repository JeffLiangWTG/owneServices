using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders.Testing;
using Enterprise.Edifact.D96B.Messages.CUSDEC;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(CusContainerDocWrapper))]
	sealed class CusContainerDocWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstruction_EQDSegment()
		{
			var messageBody = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+DOC::ZZZ'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
EQD+CN+CONT1+:::SEAL1+++4'
EQD+CN+CONT2+:::SEAL1          SEAL2+++5'
EQD+CN+CONT3+:::               SEAL2+++7'
EQD+CN+CONT4+:::SEAL1SEALASEALB+++8'
EQD+CN+CONT5+:::SEAL1SEALASEALB'
FTX+LIN+++3:1:2:TRANBKCODE:1'
RFF+BH:HBOL01'
DTM+137:20150101:102'
RFF+ABT:ORGMRN'
RFF+AAS:MBOL01'
DTM+137:20150102:102'
RFF+ABI:FINACCNUM'
RFF+UCN:UCR'
RFF+ACD:JOBNUM'
RFF+AAV:CASENUM'
PAC+200'
PCI++MARK1'
PCI++MARK2'
PCI++MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3'
TDT+20+JQ841+4:1+++++:::VESSEL'
DOC+380+INVN1'
DTM+3:20140101:102'
DOC+380+INVN2'
DTM+3:20140102:102'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+MS+TST'
NAD+BY+BOC'
UNS+D'
CST+1+020110:108:ZZZ+100'
FTX+AAA+++THIS IS A LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG :LONG LONG LONG LONG LONG LONG LONG LONG LONG DESCRIPTION'
FTX+ACB+++TS1VALUETS1:TS2VALUETS2:TS3VALUETS3:TS4VALUETS4:TS5VALUETS5'
FTX+AAI+++TS6VALUETS6:TS7VALUETS77777777777777777'
FTX+CCI+++11:00:promea::1'
LOC+27+AU'
MEA+AAR++KG:101.00'
MEA+AAS++M3:202.00'
MEA+AAT++CQ:303.00'
MEA+AAF++KG:404'
NAD+WH+WPC'
MOA+38:200'
MOA+40:88'
RFF+WE:PTA201604198464646:0001'
TAX+1+1P1:107:ZZZ'
MOA+161:11.00'
TAX+1+12A:107:ZZZ'
MOA+161:12.10'
TAX+1+12B:107:ZZZ'
MOA+161:12.20'
TAX+1+13A:107:ZZZ'
MOA+161:13.10'
TAX+1+13B:107:ZZZ'
MOA+161:13.20'
TAX+1+13C:107:ZZZ'
MOA+161:13.30'
TAX+1+13D:107:ZZZ'
MOA+161:13.40'
TAX+1+15A:107:ZZZ'
MOA+161:15.10'
TAX+1+15B:107:ZZZ'
MOA+161:15.20'
TAX+1+1P8:107:ZZZ'
MOA+161:18.00'
TAX+1+2P1:107:ZZZ'
MOA+161:21.00'
TAX+1+2P2:107:ZZZ'
MOA+161:22.00'
TAX+1+2P3:107:ZZZ'
MOA+161:23.00'
TAX+1+VAT:107:ZZZ'
MOA+161:44.44'
TAX+1+SUR:107:ZZZ'
MOA+161:55.55'
TAX+1+PEN:107:ZZZ'
MOA+161:66.66'
TAX+1+FOR:107:ZZZ'
MOA+161:99.99'
TAX+1+DLA:107:ZZZ'
MOA+161:77.77'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+111+<<MSGNO PLACEHOLDER>>'
";
			var testMessage = Factory.NewWithValidTestData<EDIMessage>();
			testMessage.EM_MessageText = messageBody.Replace("\r", "").Replace("\n", "");
			var d96bMessageFactory = new Edifact.D96B.EdifactD96BMessageFactory();
			var zaCharSet = new ZACharacterSet();
			CUSDECMessage cusdecMessage = testMessage.GetAutoEdifactMessageUsingNamedFactory(d96bMessageFactory, zaCharSet) as CUSDECMessage;
			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();
				var tester = new CusContainerDocWrapper(cusdecMessage.EQD[0], factory);
				AssertEquals("CONT1", tester.ContainerNumber);
				AssertEquals("4", tester.ContainerMode);
				AssertEquals(Universal.ContainerModeCodeList.Descriptions.Empty, tester.ContainerModeDescription);
				AssertEquals("SEAL1", tester.FirstSealNumber);
				AssertEquals("", tester.SecondSealNumber);
				tester = new CusContainerDocWrapper(cusdecMessage.EQD[1], factory);
				AssertEquals("CONT2", tester.ContainerNumber);
				AssertEquals("5", tester.ContainerMode);
				AssertEquals(Universal.ContainerModeCodeList.Descriptions.Full, tester.ContainerModeDescription);
				AssertEquals("SEAL1", tester.FirstSealNumber);
				AssertEquals("SEAL2", tester.SecondSealNumber);
				tester = new CusContainerDocWrapper(cusdecMessage.EQD[2], factory);
				AssertEquals("CONT3", tester.ContainerNumber);
				AssertEquals("7", tester.ContainerMode);
				AssertEquals(Universal.ContainerModeCodeList.Descriptions.FullMixedConsignment, tester.ContainerModeDescription);
				AssertEquals("", tester.FirstSealNumber);
				AssertEquals("SEAL2", tester.SecondSealNumber);
			});
		}

		public void TestConstruction_IContainerInformation()
		{
			var testInput = new ContainerInformationForTest()
			{ ContainerNumber = "CONT", ContainerMode = "1", FirstSealNumber = "SEAL1", SecondSealNumber = "SEAL2", };
			var tester = new CusContainerDocWrapper(testInput, Factory);
			AssertEquals("CONT", tester.ContainerNumber);
			AssertEquals("1", tester.ContainerMode);
			AssertEquals("SEAL1", tester.FirstSealNumber);
			AssertEquals("SEAL2", tester.SecondSealNumber);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusContainerDocWrapper(null as IContainerInformation, new BusinessObjectFactory());
	}
}
