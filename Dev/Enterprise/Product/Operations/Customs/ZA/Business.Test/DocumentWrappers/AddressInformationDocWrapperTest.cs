using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.MessageBuilders.Testing;
using Enterprise.Edifact.D96B.Messages.CUSDEC;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(AddressInformationDocWrapper))]
	sealed class AddressInformationDocWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstruction_SG6()
		{
			var messageBody = @"UNH+123+CUSDEC:D:96B:UN:ZZZ01'
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
NAD+IM+70707070++TACSPO DISTRIBUTING PTY LTD+980 LYTTON ROAD MURARRIE QLD 4172 A:USTRALIA+MURARRIE++4172'
RFF+VA:123321'
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
UNT+111+123'
";
			var testMessage = Factory.NewWithValidTestData<EDIMessage>();
			testMessage.EM_MessageText = messageBody.Replace("\r", "").Replace("\n", "");
			var d96bMessageFactory = new Edifact.D96B.EdifactD96BMessageFactory();
			var zaCharSet = new ZACharacterSet();
			CUSDECMessage cusdecMessage = testMessage.GetAutoEdifactMessageUsingNamedFactory(d96bMessageFactory, zaCharSet) as CUSDECMessage;
			CombineAssertions(() =>
			{
				var tester = new AddressInformationDocWrapper(cusdecMessage.Group6[0]);
				AssertEquals("70707070", tester.OrganizationCode);
				AssertEquals("", tester.OrganizationCodeQualifier);
				AssertEquals("TACSPO DISTRIBUTING PTY LTD", tester.Name);
				AssertEquals("980 LYTTON ROAD MURARRIE QLD 4172 AUSTRALIA", tester.Address);
				AssertEquals("MURARRIE", tester.City);
				AssertEquals("4172", tester.PostCode);
				AssertEquals("123321", tester.VATRegistrationNo);
			});
		}

		public void TestFallbackNullNull()
		{
			var wrapper = new AddressInformationDocWrapper(null, null);
			AssertEquals("", wrapper.OrganizationCode);
			AssertEquals("", wrapper.OrganizationCodeQualifier);
			AssertEquals("", wrapper.Name);
			AssertEquals("", wrapper.Address);
			AssertEquals("", wrapper.City);
			AssertEquals("", wrapper.PostCode);
			AssertEquals("", wrapper.VATRegistrationNo);
		}

		public void TestFallbackInputOnly()
		{
			var input = new AddressInformationForTest();
			input.OrganizationCode = "A";
			input.OrganizationCodeQualifier = "B";
			input.Name = "C";
			input.Address = "D";
			input.City = "E";
			input.PostCode = "F";
			input.VATRegistrationNo = "G";
			var wrapper = new AddressInformationDocWrapper(input, null);
			AssertEquals("A", wrapper.OrganizationCode);
			AssertEquals("B", wrapper.OrganizationCodeQualifier);
			AssertEquals("C", wrapper.Name);
			AssertEquals("D", wrapper.Address);
			AssertEquals("E", wrapper.City);
			AssertEquals("F", wrapper.PostCode);
			AssertEquals("G", wrapper.VATRegistrationNo);
			wrapper = new AddressInformationDocWrapper(input, new AddressInformationForTest());
			AssertEquals("A", wrapper.OrganizationCode);
			AssertEquals("B", wrapper.OrganizationCodeQualifier);
			AssertEquals("C", wrapper.Name);
			AssertEquals("D", wrapper.Address);
			AssertEquals("E", wrapper.City);
			AssertEquals("F", wrapper.PostCode);
			AssertEquals("G", wrapper.VATRegistrationNo);
		}

		public void TestFallbackFalbackOnly()
		{
			var fallbackInput = new AddressInformationForTest();
			fallbackInput.OrganizationCode = "A";
			fallbackInput.OrganizationCodeQualifier = "B";
			fallbackInput.Name = "C";
			fallbackInput.Address = "D";
			fallbackInput.City = "E";
			fallbackInput.PostCode = "F";
			fallbackInput.VATRegistrationNo = "G";
			var wrapper = new AddressInformationDocWrapper(null, fallbackInput);
			AssertEquals("A", wrapper.OrganizationCode);
			AssertEquals("B", wrapper.OrganizationCodeQualifier);
			AssertEquals("C", wrapper.Name);
			AssertEquals("D", wrapper.Address);
			AssertEquals("E", wrapper.City);
			AssertEquals("F", wrapper.PostCode);
			AssertEquals("G", wrapper.VATRegistrationNo);
			wrapper = new AddressInformationDocWrapper(new AddressInformationForTest(), fallbackInput);
			AssertEquals("A", wrapper.OrganizationCode);
			AssertEquals("B", wrapper.OrganizationCodeQualifier);
			AssertEquals("C", wrapper.Name);
			AssertEquals("D", wrapper.Address);
			AssertEquals("E", wrapper.City);
			AssertEquals("F", wrapper.PostCode);
			AssertEquals("G", wrapper.VATRegistrationNo);
		}

		public void TestFallback()
		{
			var input = new AddressInformationForTest();
			input.OrganizationCode = "A";
			input.OrganizationCodeQualifier = "B";
			input.Name = "C";
			input.Address = "D";
			input.City = "E";
			input.PostCode = "F";
			input.VATRegistrationNo = "G";
			var fallbackInput = new AddressInformationForTest();
			fallbackInput.OrganizationCode = "1";
			fallbackInput.OrganizationCodeQualifier = "2";
			fallbackInput.Name = "3";
			fallbackInput.Address = "4";
			fallbackInput.City = "5";
			fallbackInput.PostCode = "6";
			fallbackInput.VATRegistrationNo = "7";
			// input has priority
			var wrapper = new AddressInformationDocWrapper(input, fallbackInput);
			AssertEquals("A", wrapper.OrganizationCode);
			AssertEquals("B", wrapper.OrganizationCodeQualifier);
			AssertEquals("C", wrapper.Name);
			AssertEquals("D", wrapper.Address);
			AssertEquals("E", wrapper.City);
			AssertEquals("F", wrapper.PostCode);
			AssertEquals("G", wrapper.VATRegistrationNo);
			input.OrganizationCode = "";
			input.OrganizationCodeQualifier = "";
			input.Name = "";
			input.Address = "";
			input.City = "";
			input.PostCode = "";
			input.VATRegistrationNo = "";
			// fallback for empty values in input
			wrapper = new AddressInformationDocWrapper(input, fallbackInput);
			AssertEquals("1", wrapper.OrganizationCode);
			AssertEquals("2", wrapper.OrganizationCodeQualifier);
			AssertEquals("3", wrapper.Name);
			AssertEquals("4", wrapper.Address);
			AssertEquals("5", wrapper.City);
			AssertEquals("6", wrapper.PostCode);
			AssertEquals("7", wrapper.VATRegistrationNo);
			input.OrganizationCode = "";
			input.OrganizationCodeQualifier = "";
			input.Name = "";
			input.Address = "";
			input.City = "X";
			input.PostCode = "";
			input.VATRegistrationNo = "";
			// address properties cannot patially fallback
			wrapper = new AddressInformationDocWrapper(input, fallbackInput);
			AssertEquals("1", wrapper.OrganizationCode);
			AssertEquals("2", wrapper.OrganizationCodeQualifier);
			AssertEquals("3", wrapper.Name);
			AssertEquals("", wrapper.Address);
			AssertEquals("X", wrapper.City);
			AssertEquals("", wrapper.PostCode);
			AssertEquals("7", wrapper.VATRegistrationNo);
		}

		protected override BusinessObject GetNewBusinessObject() => new AddressInformationDocWrapper(null as AddressInformationDocWrapper);
	}
}
