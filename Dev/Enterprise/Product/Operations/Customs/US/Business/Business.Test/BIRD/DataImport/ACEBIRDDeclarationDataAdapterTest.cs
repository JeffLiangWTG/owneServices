using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDDeclarationDataAdapterTest : TestCaseWithFactory
	{
		[TestDate(2018, 11, 12)]
		public void TestImportLinePriceForCS00700710_1()
		{
			var messageText =
"B  1101SV9AE                                               HYEDUSCMT_170626     " +
"10ASV9  71020307 1101B00166124   0111 X       001 2               Y             " +
"1158-12345678958-123456789                     042816       NY                  " +
"20APLU1101      B815TITANIC                                                     " +
"2104286                                                                         " +
"2200000100PK                                                                    " +
"23MAPLUMST0428163                                                               " +
"318B 891                                                                        " +
"40  001 HKHK032916        0000000667582010000003333338 N                        " +
"47MHKGROIND1711HON                                                              " +
"47S58-123456789                                                                 " +
"509105218030 0000008430 0000000137 000000028100NO                               " +
"509105218040 0000004388 0000000636 000000028100NO                               " +
"509105218050 0000000000 0000000000 000000028100NO                               " +
"6249900000047                                                                   " +
"6249900000220                                                                   " +
"6249900000000                                                                   " +
"89056000000011215010000000375049900000010392                                    " +
"9000000591000 00000015263 00000000000 00000000000 00000000000                   " +
"Y  1101SV9AE                                                                    ";
			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(3, invoice.JobComInvoiceLines.Count);
			var invoiceLines = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().ToArray();
			var line1 = invoiceLines.FirstOrDefault(x => x.JI_Tariff == "9105218030");
			AssertNotNull(line1);
			AssertEquals(137m, line1.JI_LinePrice);

			var line2 = invoiceLines.FirstOrDefault(x => x.JI_Tariff == "9105218040");
			AssertNotNull(line2);
			AssertEquals(636m, line2.JI_LinePrice);

			var line3 = invoiceLines.FirstOrDefault(x => x.JI_Tariff == "9105218050");
			AssertNotNull(line3);
			AssertEquals(0m, line3.JI_LinePrice);
		}

		[TestDate(2018, 11, 12)]
		public void TestImportLinePriceForCS00700710_2()
		{
			var messageText =
"B  1101SV9AE                                               HYEDUSCMT_170626     " +
"10ASV9  71020307 1101B00166124   0111 X       001 2               Y             " +
"1158-12345678958-123456789                     042816       NY                  " +
"20APLU1101      B815TITANIC                                                     " +
"2104286                                                                         " +
"2200000100PK                                                                    " +
"23MAPLUMST0428163                                                               " +
"318B 891                                                                        " +
"40  001 HKHK032916        0000000667582010000003333338 N                        " +
"47MHKGROIND1711HON                                                              " +
"47S58-123456789                                                                 " +
"508211100000 0000000769 0000000126 000000000000PCS                              " +
"508211929045 0000000000 0000000000 000000000000NO                               " +
"6249900000047                                                                   " +
"6249900000220                                                                   " +
"6249900000000                                                                   " +
"89056000000011215010000000375049900000010392                                    " +
"9000000591000 00000015263 00000000000 00000000000 00000000000                   " +
"Y  1101SV9AE                                                                    ";
			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(2, invoice.JobComInvoiceLines.Count);
			var invoiceLines = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().ToArray();
			var line1 = invoiceLines.FirstOrDefault(x => x.JI_Tariff == "8211100000");
			AssertNotNull(line1);
			AssertEquals(0m, line1.JI_LinePrice);

			var line2 = invoiceLines.FirstOrDefault(x => x.JI_Tariff == "8211929045");
			AssertNotNull(line2);
			AssertEquals(126m, line2.JI_LinePrice);
		}

		[TestDate(2008, 1, 1)]
		public void TestImportCottonFee()
		{
			var messageText =
"B  1101SV9AE                                               HYEDUSCMT_170626     " +
"10ASV9  71020307 1101B00166124   0111 X       001 2               Y             " +
"1158-12345678958-123456789                     042816       NY                  " +
"20APLU1101      B815TITANIC                                                     " +
"2104286                                                                         " +
"2200000100PK                                                                    " +
"23MAPLUMST0428163                                                               " +
"318B 891                                                                        " +
"40  001 HKHK032916        0000000667582010000003333338 N                        " +
"47MHKGROIND1711HON                                                              " +
"47S58-123456789                                                                 " +
"506105100010 0000197000 0000010000 000000100000DOZ000000100000KG                " +
"6205600001121                                                                   " +
"6250100001250                                                                   " +
"6249900003464                                                                   " +
"40  002 HKHK032916        0000000667582010000003333338 N 1                      " +
"47MHKGROIND1711HON                                                              " +
"47S58-123456789                                                                 " +
"506105100011 0000197000 0000010000 000000100000DOZ000000100000KG                " +
"6250100001250                                                                   " +
"6249900003464                                                                   " +
"40  003 HKHK032916        0000000667582010000003333338 N                        " +
"47MHKGROIND1711HON                                                              " +
"47S58-123456789                                                                 " +
"506105100012 0000197000 0000010000 000000100000DOZ000000100000KG                " +
"5222123456789                                                                   " +
"6250100001250                                                                   " +
"6249900003464                                                                   " +
"89056000000011215010000000375049900000010392                                    " +
"9000000591000 00000015263 00000000000 00000000000 00000000000                   " +
"Y  1101SV9AE                                                                    ";
			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(3, invoice.JobComInvoiceLines.Count);
			var invoiceLines = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().ToArray();
			AssertInvoiceLineCotton(invoiceLines.First(x => x.JI_Tariff == "6105100010"), "6105100010", YesNoDefaultList.Codes.No, System.Array.Empty<ZString>());
			AssertInvoiceLineCotton(invoiceLines.First(x => x.JI_Tariff == "6105100011"), "6105100011", ZString.Empty, System.Array.Empty<ZString>());
			AssertInvoiceLineCotton(invoiceLines.First(x => x.JI_Tariff == "6105100012"), "6105100012", ZString.Empty, new ZString[] { "123456789" });
		}

		[TestDate(2008, 1, 1)]
		public void TestImportWatchRepair()
		{
			var messageText =
"B  1101SV9AE                                               HYEDUSCMT_170778     " +
"10ASV9  70041270 1101B00165288   2340 XA          2012016                       " +
"1113-14792700013-147927000                     010416       TX                  " +
"2004  1101      H000                                                            " +
"21001                                                                           " +
"2200000010PC                                                                    " +
"23M    RDS12312311                                                              " +
"318B 037                                                                        " +
"SE30SE A9 TECHNOLOGY NZ LTD                                                     " +
"SE3515PO BOX 217 OTAKI KAPITI COAST                                             " +
"SE36OTAKI                                       217            NZ               " +
"40  001 CH  010416        0000000000     0000000009    N                        " +
"47MCHHARWIN8PLA                                                                 " +
"47C13-147927000                                                                 " +
"47S13-147927000                                                                 " +
"SE50MF HARRY WINSTON SA                                                         " +
"SE5515CHEMIN DU TOURBILLON 8                                                    " +
"SE56PLAN LES                                    1228           CH               " +
"5098130020   0000000000 0000000000                                              " +
"509101118010 0000000000 0000000250 000000000500NO                               " +
"509101118020 0000000000 0000000250 000000000500NO                               " +
"509101118030 0000000000 0000000250 000000000500NO                               " +
"509813000520 0000000000 0000000000                                              " +
"509101118040 0000000000 0000000250 000000005000NO                               " +
"9000000000000 00000000000 00000000000 00000000000 00000000000                   " +
"Y  1101SV9AE                                                                    ";
			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			var invoiceLines = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().ToList();
			AssertEquals(4, invoiceLines.Count);
			var parentLine = invoiceLines.First(x => x.JI_ParentID.IsEmpty && x.US_IsParent);
			invoiceLines.Remove(parentLine);
			AssertInvoiceLine(parentLine, "9101118010", "98130020", ZGuid.Empty, ZGuid.Empty, true);
			var invoiceLine = invoiceLines.First(x => x.US_SupTariff == "9813000520");
			invoiceLines.Remove(invoiceLine);
			AssertInvoiceLine(invoiceLine, "9101118040", "9813000520", parentLine.PK, ZGuid.Empty, false);
			invoiceLine = invoiceLines[0].JI_Tariff == "9101118020" ? invoiceLines[0] : invoiceLines[1];
			invoiceLines.Remove(invoiceLine);
			AssertInvoiceLine(invoiceLine, "9101118020", "", parentLine.PK, ZGuid.Empty, false);
			AssertInvoiceLine(invoiceLines[0], "9101118030", "", parentLine.PK, ZGuid.Empty, false);
		}

		[TestDate(2008, 1, 1)]
		public void TestImportFDAAndFCC()
		{
			var messageText =
"B  3901SV9AE                                  1101SV9  1   HYEDUSCMT_170282     " +
"10ASV9  71019838 3901B00165979   0140 XA          2041916                       " +
"1113-14792700013-147927000                     040716       DC                  " +
"20UA  3901040716I310                                                            " +
"21016                                                                           " +
"2200000001PK                                                                    " +
"23M    01688777441                                                              " +
"318B 037                                                                        " +
"SE30SE HARRY WINSTON SA                                                         " +
"SE3515CHEMIN DU TOURBILLON 8                                                    " +
"SE36PLAN LES                                    1228           CH               " +
"40  001 CHCH040716        0000000075     0000000020    N                        " +
"47MCHHARWIN8PLA                                                                 " +
"47C13-147927000                                                                 " +
"47S13-147927000                                                                 " +
"47ECHHARWIN8PLA                                                                 " +
"SE50MF HARRY WINSTON SA                                                         " +
"SE5515CHEMIN DU TOURBILLON 8                                                    " +
"SE56PLAN LES                                    1228           CH               " +
"508527214040 0000000000 0000002500 000000000500NO                               " +
"OI        RECEIVER                                                              " +
"FC0102 001                 FCC                           12354                  " +
"FC02000000000005                                                                " +
"OI        RECEIVER                                                              " +
"PG01001FDARADREP                         180.000                                " +
"PG02PFDP 95R--FV                                                                " +
"PG0639 CH                                                                       " +
"PG07RECEIVER                                                                    " +
"PG10                   RECEIVER ASSY                                            " +
"PG19MF                   HARRY WINSTON SA                CHEMIN DU TOURBILLON 8 " +
"PG20                                     PLAN LES                CH1228         " +
"PG19DEQ                  HARRY WINSTON SA                CHEMIN DU TOURBILLON 8 " +
"PG20                                     PLAN LES                CH1228         " +
"PG19FD147 3008687281     ROCKY'S PET EMPORUIM            2300 W CHICAGO AVE     " +
"PG20                                     CHICAGO              IL US60622-472    " +
"PG21FD1ROCKY BALBOA           3125551212     ROCKYBALBOA@ROCKYS.COM             " +
"PG19DP                   ROCKY'S PET EMPORUIM            TEST ADDRESS 2         " +
"PG20                                     DC                   DC US20599        " +
"PG21DP ROCKY BALBOA           3125551212     ROCKYBALBOA@ROCKYS.COM             " +
"PG19DFP47 3008687281     ROCKY'S PET EMPORUIM            2300 W CHICAGO AVE     " +
"PG20                                     CHICAGO              IL US60622-472    " +
"PG21DFPROCKY BALBOA           3125551212     ROCKYBALBOA@ROCKYS.COM             " +
"PG19PK 16 121124412      U.S. DEMO COMPANY               184 TEST STREET        " +
"PG20                                     CHICAGO              IL US60056        " +
"PG21PK CHRISTINA RUSZCZAK     3125551212     CHRISTINA.RUSZCZAK@CARGOWISE.COM   " +
"PG23RA6                                                                         " +
"PG25                                                    000000002500            " +
"PG261000000000500PCS                                                            " +
"PG30A0407201623002   3901                                                       " +
"6249900000866                                                                   " +
"8949900000002500                                                                " +
"9000000000000 00000002500 00000000000 00000000000 00000000000                   " +
"Y  3901SV9AE                                                                    ";
			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(1, invoice.JobComInvoiceLines.Count);
			var invoiceLine = invoice.JobComInvoiceLines[0];
			AssertEquals("JI_Tariff", "8527214040", invoiceLine.JI_Tariff);

			AssertEquals("US_FDAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_FDAIndicator);
			AssertEquals("invoiceLine.ACE_FDALines.Count", 1, invoiceLine.ACE_FDALines.Count);
			var fda = invoiceLine.ACE_FDALines[0];
			AssertEquals("fda.US_ProgramCode", "RAD", fda.US_ProgramCode);
		}

		[TestDate(2008, 1, 1)]
		public void TestImportMultiLinesWithFDA()
		{
			var messageText =
"B  1101SV9AE                                               HYEDUSCMT_170066     " +
"10ASV9  71019424 1101B00165890   0111 XA          2               Y             " +
"1158-12345678958-123456789                     032416       NY                  " +
"20APLU1101032416B815TITANIC                                                     " +
"2103246                                                                         " +
"2200000100PK                                                                    " +
"23MAPLUMST032416                                                                " +
"SE17MAEU989890                                                                  " +
"318B 891                                                                        " +
"SE30SE ACE TEST SUPPLIER HK                                                     " +
"SE3515171-172 GLOUCESTER ROAD                                                   " +
"SE36HONG KONG                                                  HK               " +
"40  001XHKHK030416        0000001000582010000001000    N                        " +
"47MHKGROIND1711HON                                                              " +
"47C58-123456789                                                                 " +
"47S58-123456789                                                                 " +
"47EHKGROIND1711HON                                                              " +
"SE50MF ACE TEST SUPPLIER HK                                                     " +
"SE5515171-172 GLOUCESTER ROAD                                                   " +
"SE56HONG KONG                                                  HK               " +
"501902194000 0000032000 0000005000 000000100000KG                               " +
"6250100000625                                                                   " +
"6249900001732                                                                   " +
"40  002VHKHK030416        0000000480582010000000480    N                        " +
"47MHKGROIND1711HON                                                              " +
"47C58-123456789                                                                 " +
"47S58-123456789                                                                 " +
"47EHKGROIND1711HON                                                              " +
"SE50MF ACE TEST SUPPLIER HK                                                     " +
"SE5515171-172 GLOUCESTER ROAD                                                   " +
"SE56HONG KONG                                                  HK               " +
"501902194000 0000000000 0000002400 000000100000KG                               " +
"OI        PASTA,N/CKD/STF/PRP,NO                                                " +
"PG01001FSIFSI                                                                  B" +
"PG01001FDAFOOPRO                                                                " +
"PG02PFDP 16WYT24                                                                " +
"PG0639 HK                                                                       " +
"PG06CSHHK                                                                       " +
"PG10                   PASTA                                                    " +
"PG19FDC                  ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROAD" +
"PG20                                     HONG KONG               HK             " +
"PG21FDCJOHN SMITH             886225452937   SUPPLIERCONT@ORG.COM               " +
"PG19DEQ                  ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROAD" +
"PG20                                     HONG KONG               HK             " +
"PG21DEQJOHN SMITH             886225452937   SUPPLIERCONT@ORG.COM               " +
"PG19FD116 123456788      FDSA                            DEV ADDR 1             " +
"PG20                                     MIDDLETOWN           NY US10940        " +
"PG21FD1FIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19UC                   ACE TEST IMPORTER 1             123 MADISON AVE        " +
"PG20                                     NEW YORK             NY US10016        " +
"PG21UC FIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19PNS                  ACE TEST IMPORTER 1             1 12A NWD ENTERPRISES  " +
"PG20                                     MIQUON               PA US19445        " +
"PG21PNSFIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19PNT16 121124412      U.S. DEMO COMPANY               184 TEST STREET        " +
"PG20                                     CHICAGO              IL US60056        " +
"PG21PNTCRAIG SEELIG           2155551212     CRAIG.SEELIG@WISETECHGLOBAL.COM    " +
"PG19DFP16 123456788      FDSA                            DEV ADDR 1             " +
"PG20                                     MIDDLETOWN           NY US10940        " +
"PG21DFPFIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19LG 16 123456788      FDSA                            DEV ADDR 1             " +
"PG20                                     MIDDLETOWN           NY US10940        " +
"PG21LG FIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19PK 16 121124412      U.S. DEMO COMPANY               184 TEST STREET        " +
"PG20                                     CHICAGO              IL US60056        " +
"PG21PK CRAIG SEELIG           2155551212     CRAIG.SEELIG@WISETECHGLOBAL.COM    " +
"PG23VES  TITANIC                                                                " +
"PG23VFT  03246                                                                  " +
"PG23CFR  12345678912                                                            " +
"PG25                                                    000000002400            " +
"PG261000000100000KG                                                             " +
"PG27MAEU989890                                                                  " +
"PG30A0324201614002   1101                                                       " +
"40  003VHKHK030416        0000000260582010000000260    N                        " +
"47MHKGROIND1711HON                                                              " +
"47C58-123456789                                                                 " +
"47S58-123456789                                                                 " +
"47EHKGROIND1711HON                                                              " +
"SE50MF ACE TEST SUPPLIER HK                                                     " +
"SE5515171-172 GLOUCESTER ROAD                                                   " +
"SE56HONG KONG                                                  HK               " +
"500712311000 0000000000 0000001300 000000100000KG                               " +
"OI        MUSHROOM                                                              " +
"PG01001FDAFOOPRO                                                                " +
"PG02PFDP 25SYR04                                                                " +
"PG0639 HK                                                                       " +
"PG0630 HK                                                                       " +
"PG06CSHHK                                                                       " +
"PG10                   FDSA  FDS FDSA                                           " +
"PG19MF                   ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROAD" +
"PG20                                     HONG KONG               HK             " +
"PG21MF JOHN SMITH             886225452937   SUPPLIERCONT@ORG.COM               " +
"PG19DEQ                  ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROAD" +
"PG20                                     HONG KONG               HK             " +
"PG21DEQJOHN SMITH             886225452937   SUPPLIERCONT@ORG.COM               " +
"PG19FD116 123456788      FDSA                            DEV ADDR 1             " +
"PG20                                     MIDDLETOWN           NY US10940        " +
"PG21FD1FIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19UC                   ACE TEST IMPORTER 1             123 MADISON AVE        " +
"PG20                                     NEW YORK             NY US10016        " +
"PG21UC FIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19PNS                  ACE TEST IMPORTER 1             1 12A NWD ENTERPRISES  " +
"PG20                                     MIQUON               PA US19445        " +
"PG21PNSFIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19PNT16 121124412      U.S. DEMO COMPANY               184 TEST STREET        " +
"PG20                                     CHICAGO              IL US60056        " +
"PG21PNTCRAIG SEELIG           2155551212     CRAIG.SEELIG@WISETECHGLOBAL.COM    " +
"PG19DFP16 123456788      FDSA                            DEV ADDR 1             " +
"PG20                                     MIDDLETOWN           NY US10940        " +
"PG21DFPFIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19LG 16 123456788      FDSA                            DEV ADDR 1             " +
"PG20                                     MIDDLETOWN           NY US10940        " +
"PG21LG FIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19PK 16 121124412      U.S. DEMO COMPANY               184 TEST STREET        " +
"PG20                                     CHICAGO              IL US60056        " +
"PG21PK CRAIG SEELIG           2155551212     CRAIG.SEELIG@WISETECHGLOBAL.COM    " +
"PG23VES  TITANIC                                                                " +
"PG23VFT  03246                                                                  " +
"PG23PFR  12345678912                                                            " +
"PG25                                                    000000001300            " +
"PG261000000100000KG                                                             " +
"PG27MAEU989890                                                                  " +
"PG30A0324201614002   1101                                                       " +
"40  004VHKHK030416        0000000260582010000000260    N                        " +
"47MHKGROIND1711HON                                                              " +
"47C58-123456789                                                                 " +
"47S58-123456789                                                                 " +
"47EHKGROIND1711HON                                                              " +
"SE50MF ACE TEST SUPPLIER HK                                                     " +
"SE5515171-172 GLOUCESTER ROAD                                                   " +
"SE56HONG KONG                                                  HK               " +
"502002908020 0000000000 0000001300 000000100000KG                               " +
"OI        SAUCE                                                                 " +
"PG01001FDAFOOPRO                                                                " +
"PG02PFDP 16YDY09                                                                " +
"PG05PINUS                 TAEDA                                                 " +
"PG0630 HK                                                                       " +
"PG0639 HK                                                                       " +
"PG06CSHHK                                                                       " +
"PG10                   FSFDS  FDSA SD                                           " +
"PG19MF                   ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROAD" +
"PG20                                     HONG KONG               HK             " +
"PG21MF JOHN SMITH             886225452937   SUPPLIERCONT@ORG.COM               " +
"PG19DEQ                  ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROAD" +
"PG20                                     HONG KONG               HK             " +
"PG21DEQJOHN SMITH             886225452937   SUPPLIERCONT@ORG.COM               " +
"PG19FD116 123456788      FDSA                            DEV ADDR 1             " +
"PG20                                     MIDDLETOWN           NY US10940        " +
"PG21FD1FIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19UC                   ACE TEST IMPORTER 1             123 MADISON AVE        " +
"PG20                                     NEW YORK             NY US10016        " +
"PG21UC FIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19PNS                  ACE TEST IMPORTER 1             1 12A NWD ENTERPRISES  " +
"PG20                                     MIQUON               PA US19445        " +
"PG21PNSFIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19PNT16 121124412      U.S. DEMO COMPANY               184 TEST STREET        " +
"PG20                                     CHICAGO              IL US60056        " +
"PG21PNTCRAIG SEELIG           2155551212     CRAIG.SEELIG@WISETECHGLOBAL.COM    " +
"PG19DFP16 123456788      FDSA                            DEV ADDR 1             " +
"PG20                                     MIDDLETOWN           NY US10940        " +
"PG21DFPFIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19LG 16 123456788      FDSA                            DEV ADDR 1             " +
"PG20                                     MIDDLETOWN           NY US10940        " +
"PG21LG FIRST LASTNAME         2156661212     PGACONTACT@PGA.COM                 " +
"PG19PK 16 121124412      U.S. DEMO COMPANY               184 TEST STREET        " +
"PG20                                     CHICAGO              IL US60056        " +
"PG21PK CRAIG SEELIG           2155551212     CRAIG.SEELIG@WISETECHGLOBAL.COM    " +
"PG23VES  TITANIC                                                                " +
"PG23VFT  03246                                                                  " +
"PG23PFR  12345678912                                                            " +
"PG25                                                    000000001300            " +
"PG261000000100000KG                                                             " +
"PG27MAEU989890                                                                  " +
"PG30A0324201614002   1101                                                       " +
"895010000000062549900000002500                                                  " +
"9000000032000 00000003125 00000000000 00000000000 00000000000                   " +
"Y  1101SV9AE                                                                    ";
			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(4, invoice.JobComInvoiceLines.Count);
			var invoiceLines = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().ToArray();
			var invoiceLine1 = invoiceLines.First(x => x.JI_Tariff == "1902194000" && x.US_FDAIndicator.IsEmpty);
			AssertInvoiceLine(invoiceLine1, "1902194000", ZString.Empty, ZGuid.Empty, ZGuid.Empty, true);
			AssertEquals("invoiceLine1.JI_Description", ZString.Empty, invoiceLine1.JI_Description);
			AssertEquals("invoiceLine1.US_SetInd", SecondarySpecProgIndicatorList.Codes.X, invoiceLine1.US_SetInd);
			AssertEquals("invoiceLine1.US_FDAIndicator", ZString.Empty, invoiceLine1.US_FDAIndicator);
			AssertEquals("invoiceLine1.ACE_FDALines.Count", 0, invoiceLine1.ACE_FDALines.Count);
			var invoiceLine2 = invoiceLines.First(x => x.JI_Tariff == "1902194000" && OGAIndicatorList.IsToBeDeclared(x.US_FDAIndicator));
			AssertInvoiceLine(invoiceLine2, "1902194000", ZString.Empty, invoiceLine1.PK, ZGuid.Empty, false);
			AssertEquals("invoiceLine2.JI_Description", "PASTA,N/CKD/STF/PRP,NO", invoiceLine2.JI_Description);
			AssertEquals("invoiceLine2.US_SetInd", SecondarySpecProgIndicatorList.Codes.V, invoiceLine2.US_SetInd);
			AssertEquals("invoiceLine2.US_FDAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine2.US_FDAIndicator);
			AssertEquals("invoiceLine2.ACE_FDALines.Count", 1, invoiceLine2.ACE_FDALines.Count);
			AssertEquals("invoiceLine2.ACE_FDALines[0].US_ProgramCode", "FOO", invoiceLine2.ACE_FDALines[0].US_ProgramCode);
			AssertEquals("invoiceLine2.ACE_FDALines[0].US_SourceCountry", "", invoiceLine2.ACE_FDALines[0].US_SourceCountry);
			var invoiceLine3 = invoiceLines.First(x => x.JI_Tariff == "0712311000");
			AssertInvoiceLine(invoiceLine3, "0712311000", ZString.Empty, invoiceLine1.PK, ZGuid.Empty, false);
			AssertEquals("invoiceLine3.JI_Description", "MUSHROOM", invoiceLine3.JI_Description);
			AssertEquals("invoiceLine3.US_SetInd", SecondarySpecProgIndicatorList.Codes.V, invoiceLine3.US_SetInd);
			AssertEquals("invoiceLine3.US_FDAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine3.US_FDAIndicator);
			AssertEquals("invoiceLine3.ACE_FDALines.Count", 1, invoiceLine3.ACE_FDALines.Count);
			AssertEquals("invoiceLine3.ACE_FDALines[0].US_ProgramCode", "FOO", invoiceLine3.ACE_FDALines[0].US_ProgramCode);
			AssertEquals("invoiceLine3.ACE_FDALines[0].US_ProgramCode", "HK", invoiceLine3.ACE_FDALines[0].US_SourceCountry);
			var invoiceLine4 = invoiceLines.First(x => x.JI_Tariff == "2002908020");
			AssertInvoiceLine(invoiceLine4, "2002908020", ZString.Empty, invoiceLine1.PK, ZGuid.Empty, false);
			AssertEquals("invoiceLine4.JI_Description", "SAUCE", invoiceLine4.JI_Description);
			AssertEquals("invoiceLine4.US_SetInd", SecondarySpecProgIndicatorList.Codes.V, invoiceLine4.US_SetInd);
			AssertEquals("invoiceLine4.US_FDAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine4.US_FDAIndicator);
			AssertEquals("invoiceLine4.ACE_FDALines.Count", 1, invoiceLine4.ACE_FDALines.Count);
			AssertEquals("invoiceLine4.ACE_FDALines[0].US_ProgramCode", "FOO", invoiceLine4.ACE_FDALines[0].US_ProgramCode);
			AssertEquals("invoiceLine4.ACE_FDALines[0].US_SourceCountry", "", invoiceLine4.ACE_FDALines[0].US_SourceCountry);
		}

		[TestDate(2008, 1, 1)]
		public void TestImportNHTSA()
		{
			var messageText =
"B  1101SV9AE                                               HYEDUSCMT_170155     " +
"10ASV9  71019515 1101B00165920   0111 XA          2041116                       " +
"1113-14792700013-147927000                     033016       NY                  " +
"20AAEU1101033016    AALSMEERGRACHT                                              " +
"21A01                                                                           " +
"2200000001PC                                                                    " +
"23MAAEU0235841230                                                               " +
"318B 037                                                                        " +
"SE30SE HARRY WINSTON SA                                                         " +
"SE3515CHEMIN DU TOURBILLON 8                                                    " +
"SE36PLAN LES                                    1228           CH               " +
"40  001 CHCH032916        0000000000101000000000001    N                        " +
"47MCHHARWIN8PLA                                                                 " +
"47C13-147927000                                                                 " +
"47S13-147927000                                                                 " +
"SE50MF HARRY WINSTON SA                                                         " +
"SE5515CHEMIN DU TOURBILLON 8                                                    " +
"SE56PLAN LES                                    1228           CH               " +
"502843300000 0000000500 0000000100             KG                               " +
"OI        IAN TEST NHTSA DRIVE SIDE                                             " +
"PG01001NHTMVS   Y                                                               " +
"PG02P                                                                           " +
"PG07IAN TEST                           FDH234         012015AKG12345678903214569" +
"PG10MVSTYPMVS1 V01 N                                                            " +
"PG19CN                   KRAFT FOODS INTERNATIONAL       A NEW DELIVERY ADDRESS " +
"PG20                                     NEW YORK             NY US141200       " +
"PG21CN JOE SMITH              3127365555     JOE@FDAKRAFOO.COM                  " +
"PG19DFP                  ACE TEST SUPPLIER CA            3990 STREET            " +
"PG20                                     BURTON               BC CAA1B2C3       " +
"PG21DFPACE TEST               22222222222    ACETEST@EPA.CO                     " +
"PG19FM                   ACE TEST SUPPLIER CA            3990 STREET            " +
"PG20                                     BURTON               BC CAA1B2C3       " +
"PG21FM ACE TEST               22222222222    ACETEST@EPA.CO                     " +
"PG19IM                   KRAFT FOODS INTERNATIONAL       A NEW DELIVERY ADDRESS " +
"PG20                                     NEW YORK             NY US141200       " +
"PG21IM JOE SMITH              3127365555     JOE@FDAKRAFOO.COM                  " +
"PG341  US235961348                                                              " +
"PG22Y946    2B   CI NH1 Y03302016                                               " +
"PG22Y872         CI     Y03302016                                               " +
"PG19CI                   U.S. DEMO COMPANY               184 TEST STREET        " +
"PG20                                     CHICAGO              IL US60056        " +
"PG21CI KNZ                    0054127845                                        " +
"PG35                                  00000100                                  " +
"6250100000013                                                                   " +
"6249900000035                                                                   " +
"895010000000001349900000002500                                                  " +
"9000000000500 00000002513 00000000000 00000000000 00000000000                   " +
"Y  1101SV9AE                                                                    ";
			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(1, invoice.JobComInvoiceLines.Count);
			var invoiceLine = invoice.JobComInvoiceLines[0];
			AssertInvoiceLine(invoiceLine, "2843300000", ZString.Empty, ZGuid.Empty, ZGuid.Empty, false);
			AssertEquals("invoiceLine.JI_Description", "IAN TEST NHTSA DRIVE SIDE", invoiceLine.JI_Description);
			AssertEquals("invoiceLine.US_NHTSAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_NHTSAIndicator);
			AssertEquals("invoiceLine.NHTSALines.Count", 1, invoiceLine.NHTSALines.Count);
			AssertEquals("invoiceLine.NHTSALines[0].US_NHTProgramCode", "MVS", invoiceLine.NHTSALines[0].US_NHTProgramCode);
		}

		[TestDate(2008, 1, 1)]
		public void TestImportXAndVLines()
		{
			var messageText =
"B  1101SV9AE                                               HYEDUSCMT_119520     " +
"10ASV9  70027576 1101B00158390   0310 XA          2080712                       " +
"1113-14792700013-147927000               072612072612       TX                  " +
"20APLU1101072612A00123                                                          " +
"21Q002                                                                          " +
"2200000012BG                                                                    " +
"23MAPLUSPITEST2                                                                 " +
"318B 037                                                                        " +
"40  001XKRKR072612      KR0000000010580300000000150   MN                        " +
"47MGB0UAFOR218ROA                                                               " +
"47C13-147927000                                                                 " +
"47S13-147927000                                                                 " +
"47EGB0UAFOR218ROA                                                               " +
"508708292500 0000000000 0000005000 000000500000NO                               " +
"6250100000625                                                                   " +
"40  002VKRKR072612      KR0000000008580300000000050   MN                        " +
"47MGB0UAFOR218ROA                                                               " +
"47C13-147927000                                                                 " +
"47S13-147927000                                                                 " +
"47EGB0UAFOR218ROA                                                               " +
"508708292500 0000000000 0000004000 000000500000NO                               " +
"40  003VKRKR072612      KR0000000002580300000000100   MN                        " +
"47MGB0UAFOR218ROA                                                               " +
"47C13-147927000                                                                 " +
"47S13-147927000                                                                 " +
"47EGB0UAFOR218ROA                                                               " +
"507210490091 0000000000 0000001000 000000010000KG                               " +
"5201U9ARFB072                                                                   " +
"5202TEST1                                                                       " +
"5206TEST 2                                                                      " +
"53A580816000C00001770A  0000000000            0000017700                        " +
"53C580208000C00000115A  0000000000            0000001150                        " +
"8800000000000 00000017700 00000000000 00000001150                               " +
"8950100000000625                                                                " +
"9000000000000 00000000625 00000000000 00000017700 00000001150                   " +
"Y  1101SV9AE                                                                    ";
			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(3, invoice.JobComInvoiceLines.Count);
			var invoiceLine = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().First(x => x.US_IsParent);
			AssertInvoiceLine(invoiceLine, "8708292500", ZString.Empty, ZGuid.Empty, ZGuid.Empty, true);
			AssertEquals("invoiceLine.US_SetInd", SecondarySpecProgIndicatorList.Codes.X, invoiceLine.US_SetInd);
			var childLines = invoiceLine.ChildLines.ToArray();
			AssertEquals("childLines", 2, childLines.Length);
			var childLine1 = childLines[0];
			var childLine2 = childLines[1];
			if (childLine2.JI_Tariff == "8708292500")
			{
				childLine2 = childLines[0];
				childLine1 = childLines[1];
			}
			AssertInvoiceLine(childLine1, "8708292500", ZString.Empty, invoiceLine.PK, ZGuid.Empty, false);
			AssertEquals("childLine1.US_SetInd", SecondarySpecProgIndicatorList.Codes.V, childLine1.US_SetInd);
			AssertInvoiceLine(childLine2, "7210490091", ZString.Empty, invoiceLine.PK, ZGuid.Empty, false);
			AssertEquals("childLine2.US_SetInd", SecondarySpecProgIndicatorList.Codes.V, childLine2.US_SetInd);
		}

		[TestDate(2008, 1, 1)]
		public void TestImportParentAndSecondaryLinesWith98()
		{
			var messageText =
"B  8888XJ5AE                                               58044                " +
"10ASV9  70022510 1101B00155757   0111 XA          2052511                       " +
"1157-12345678957-123456789                     051311       PA                  " +
"20APLU1101051311B815TITANIC                                                     " +
"21Q43                                                                           " +
"2200000100CS                                                                    " +
"23MAPLUMASTERQ43                                                                " +
"318B 891                                                                        " +
"40  001 MAMA042311      MA0000001000588660000000998    N                        " +
"47MMAMOHCHE107CAS                                                               " +
"47C57-123456789                                                                 " +
"47S57-123456789                                                                 " +
"509801001010 0000000000 0000000000             X                                " +
"509404902000 0000000000 0000010000             X                                " +
"509801001010 0000000000 0000000000             X                                " +
"503401111000 0000000000 0000000020             KG                               " +
"6250100001253                                                                   " +
"8950100000001253                                                                " +
"9000000000000 00000001253 00000000000 00000000000 00000000000                   " +
"Y  8888XJ5AE                                                                    ";
			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(2, invoice.JobComInvoiceLines.Count);
			var invoiceLine = invoice.JobComInvoiceLines[0];
			if (invoiceLine.IsChildLine)
			{
				invoiceLine = invoice.JobComInvoiceLines[1];
			}
			AssertInvoiceLine(invoiceLine, "9404902000", "9801001010", ZGuid.Empty, ZGuid.Empty, true);
			var secondaryTariffLines = invoiceLine.SecondaryTariffLines.ToArray();
			AssertEquals("SecondaryTariffLines", 1, secondaryTariffLines.Length);
			AssertInvoiceLine(secondaryTariffLines[0], "3401111000", "9801001010", invoiceLine.PK, ZGuid.Empty, false);
		}

		public void TestSetCottonFeeExempt()
		{
			string messageText =
"B  1101SV9AE                                                                    " +
"10ASV9  71020976 1101B00166339   06   X           2060916                       " +
"11161101-00903161101-00903               0527160527169998776PA                  " +
"20    1101052716W235                                                            " +
"21TRIPD                                                                         " +
"318B 422                                                                        " +
"40  001XXCCA            CA0000000029     0000000462    N                        " +
"41N      0000000030                                                             " +
"501902194000 0000000000 0000002000 000000010000KG                               " +
"40  002 XCCA              0000000727     0000023121336 N                        " +
"41N      0000000050                                                             " +
"506104420010 0000575000 0000050000 000000500000DOZ000006000000KG                " +
"6205600061470                                                                   " +
"6249900017320                                                                   " +
"40  003 XCCA              0000000727     0000023121336 N 1                      " +
"41N      0000000010                                                             " +
"506104420010 0000575000 0000050000 000000500000DOZ000000600000KG                " +
"6249900017320                                                                   " +
"894990000004850005600000061470                                                  " +
"9000001783000 00000109970 00000000000 00000000000 00000000000                   " +
"Y  1101SV9AE                                                                    ";
			var generator = new ImportInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals("5 invoice line", 3, declaration.InvoiceLines.Count);
			AssertEquals("Cotton Fee Exempt code should be empty", ZString.Empty, declaration.InvoiceLines[0].US_CottonFeeExempt);
			AssertEquals("Cotton Fee Exempt code should be 'N' as ShouldCottonFeeBeIndicated() return true and Charge Amound greater than 0", YesNoDefaultList.Codes.No, declaration.InvoiceLines[1].US_CottonFeeExempt);
			AssertEquals("Cotton Fee Exempt code should be 'Y' as ShouldCottonFeeBeIndicated() return true and Fee Exemption Code in 40 record is '1'", YesNoDefaultList.Codes.Yes, declaration.InvoiceLines[2].US_CottonFeeExempt);
		}

		public void TestSetCottonFeeExemptForChildLines()
		{
			string messageText =
"B  1101SV9AE                                                                    " +
"10ASV9  71020976 1101B00166339   06   X           2060916                       " +
"11161101-00903161101-00903               0527160527169998776PA                  " +
"20    1101052716W235                                                            " +
"21TRIPD                                                                         " +
"318B 422                                                                        " +
"40  001 AUAU060216        0000000250602670000000000648 N 1                      " +
"506104420010 0000141000 0000005000                                              " +
"506104420010 0000000000 0000000000 000000520000DOZ000000056000KG                " +
"6250100000625                                                                   " +
"6249900001732                                                                   " +
"40  002 XCCA            CA0000000145     0000004624    N                        " +
"41N      0000000010                                                             " +
"506104420010 0000000000 0000010000 000000200000PCS                              " +
"506104420010 0000000000 0000000000 000001000000NO                               " +
"6205600050000                                                                   " +
"894990000004850005600000061470                                                  " +
"9000001783000 00000109970 00000000000 00000000000 00000000000                   " +
"Y  1101SV9AE                                                                    ";
			var generator = new ImportInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals("5 invoice line", 4, declaration.InvoiceLines.Count);
			AssertEquals("Cotton Fee Exempt code should be 'Y' as ShouldCottonFeeBeIndicated() return true and Fee Exemption Code in 40 record is '1'.", YesNoDefaultList.Codes.Yes, declaration.InvoiceLines[0].US_CottonFeeExempt);
			AssertEquals("Cotton Fee Exempt code should be 'Y' as this is child line and ShouldCottonFeeBeIndicated() return true and Fee Exemption Code in 40 record is '1'.", YesNoDefaultList.Codes.Yes, declaration.InvoiceLines[1].US_CottonFeeExempt);
			AssertEquals("Cotton Fee Exempt code should be 'N' as ShouldCottonFeeBeIndicated() return true and Charge Amound greater than 0.", YesNoDefaultList.Codes.No, declaration.InvoiceLines[2].US_CottonFeeExempt);
			AssertEquals("Cotton Fee Exempt code should be 'N' as this is child line and ShouldCottonFeeBeIndicated() return true and Charge Amound greater than 0.", YesNoDefaultList.Codes.No, declaration.InvoiceLines[3].US_CottonFeeExempt);
		}

		public void TestNoExceptionThrownWhenSE55BlockNotExistsBetweenSE50AndSE56()
		{
			string messageText =
"B  0906F5ZSE                                               F5Z__200472          " +
"SE10AF5Z  00000002 06EI               800399385350906                           " +
"SE11W092816A115    FTZ0370D01                                                   " +
"SE40001IT                                                                       " +
"SE41N      00000100                                                             " +
"SE50MF XYLEM SERVICE ITALIA                                                     " +
"SE56MONTECCHIO MAGGIORE                                        IT               " +
"SE50SE XYLEM SERVICE ITALIA                                                     " +
"SE5515VIA DOTT LOMBARDI 14                                                      " +
"SE56MONTECCHIO MAGGIORE                                        IT               " +
"SE6039039050000000000100                                                        " +
"Y  0906F5ZSE02408                                                               ";

			var generator = new ImportInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			AssertNoExceptionThrown(delegate
			{
				new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
				declaration.UnlockImportEntryNumberAllocationMutex();
				AssertEquals("1 invoice line", 1, declaration.InvoiceLines.Count);
				AssertEquals(true, notifications.ContainsNotificationContaining("SE55 record is missing when entity code is 'MF' and entity name is 'XYLEM SERVICE ITALIA'."));
			});
		}

		public void TestSetCountryOfOriginNotFromManufacturer()
		{
			string messageText =
"B  1803002SE                                               002__200561          " +
"SE10A002  81991372 06EI 59-373774900  800351946901803                           " +
"SE11W042620M773    FTZ0640000                                                   " +
"SE30BY Coach Services, Inc.               EI 59-373774900                       " +
"SE30CN Coach Services, Inc.               EI 59-373774900                       " +
"SE40026VN                                                                       " +
"SE41N      00000012                                                             " +
"SE50MF ADC SHANGHAI                       MIDCNCOAMAN199SHA                     " +
"SE5515SHENFEI ROAD # 199 LOGISTIC PARK                                          " +
"SE56SHANGHAI                                                   CN               " +
"SE50SE ADC SHANGHAI                       MIDCNCOAMAN199SHA                     " +
"SE5515SHENFEI ROAD # 199 LOGISTIC PARK                                          " +
"SE56SHANGHAI                                                   CN               " +
"SE6042021100300000000420                                                        " +
"Y  1803002SE07091                                                               ";

			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(1, invoice.JobComInvoiceLines.Count);
			var invoiceLine = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().First();
			AssertEquals("VN", invoiceLine.US_UC_NKCountryOfOrigin);
		}

		/// <summary>
		/// BIRD message with two groups of tariffs
		/// The tariffs in first group are 94/99, when these tariffs are imported, tariffs should be on sorted and only one invoice line should be created.
		/// The tariffs in second group are 94/98/99 with PGA information, when these tariffs are imported, tariffs should be sorted and two invoice lines should be created with parent/child set. PGA details should be on regular tariff line
		/// </summary>
		public void TestImportSEMessagesWithProvTariffs()
		{
			string messageText =
"B  1101SV9SE                                               HYEDUSCMT_213051     " +
"SE10A              06EI 091101-00624  806949774582704                           " +
"SE11W091220W384    FTZ27602                                                     " +
"SE30BY IKEA Supply AG                     EI 091101-00624                       " +
"SE30CN IKEA Distribution Services Inc. - SEI 23-254866202                       " +
"SE40001CN                                                                       " +
"SE41P      00000674                                                             " +
"SE50MF Linyi Donglong Homtextile Co Ltd   MIDCNLINDONLIN                        " +
"SE5515DONGZHAIZI VILLAGE                                                        " +
"SE56LINYI                                       276709         CN               " +
"SE50SE Linyi Donglong Homtextile Co Ltd   MIDCNLINDONLIN                        " +
"SE5515DONGZHAIZI VILLAGE                                                        " +
"SE56LINYI                                       276709         CN               " +
"SE6094019050210000013587                                                        " +
"SE6099038803  0000908994                                                        " +
"OI        OTHER WOODEN CHAIRS                                                   " +
"PG01001EPATS1                                                                  A" +
"SE40002CN                                                                       " +
"SE41P      00000001                                                             " +
"SE50MF Suzhou Meccan Imp & Exp Corp       MIDCNSUZMEC12SUZ                      " +
"SE551512 EAST CHUNSHEN LAKE RD, XIANGCHEN                                       " +
"SE56SUZHOU                                      215131         CN               " +
"SE50SE Suzhou Meccan Imp & Exp Corp       MIDCNSUZMEC12SUZ                      " +
"SE551512 EAST CHUNSHEN LAKE RD, XIANGCHEN                                       " +
"SE56SUZHOU                                      215131         CN               " +
"SE6094039070800000000047                                                        " +
"SE6098201124  0000000000                                                        " +
"SE6099038803  0000010377                                                        " +
"OI        OTHER WOODEN FURNITURE                                                " +
"PG01001EPATS1                                                                  A" +
"Y                                                                               ";

			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(3, invoice.JobComInvoiceLines.Count);
			CombineAssertions("Values on 1st invoice line", () =>
			{
				var invoiceLine = invoice.JobComInvoiceLines[0];
				AssertEquals("First line shouldn't have any child lines", 0, invoiceLine.ChildLines.Count());
				AssertEquals("Country of Origin on first line should be CN", "CN", invoiceLine.US_UC_NKCountryOfOrigin);
				AssertEquals("Sup tariff on first line should be 99038803", "99038803", invoiceLine.US_SupTariff);
				AssertEquals("Regular tariff on first line should be 9401905021", "9401905021", invoiceLine.JI_Tariff);
				AssertEquals("Line price on first line should be 922581 = (908994 + 13587)", 922581m, invoiceLine.JI_LinePrice);
				AssertEquals("TSCA on first line should be disclaimed", "C", invoiceLine.US_TSCAInd);
				AssertEquals("TSCA disclaim reason on first line should be A", "A", invoiceLine.US_TSCADisclaimReason);
				AssertEquals("Goods description on first line should come from OI block", "OTHER WOODEN CHAIRS", invoiceLine.JI_Description);
			});

			CombineAssertions("Values on 2nd invoice line", () =>
			{
				var invoiceLine = invoice.JobComInvoiceLines[1];
				AssertEquals("Second line should have a child line", 1, invoiceLine.ChildLines.Count());
				AssertEquals("Country of Origin on second line should be CN", "CN", invoiceLine.US_UC_NKCountryOfOrigin);
				AssertEquals("Sup tariff on second line should be 98201124", "98201124", invoiceLine.US_SupTariff);
				AssertEquals("Regular tariff on second line should be empty", ZString.Empty, invoiceLine.JI_Tariff);
				AssertEquals("Line price on second line should be zero", 0m, invoiceLine.JI_LinePrice);
				AssertEquals("TSCA indicator on second line should be empty", ZString.Empty, invoiceLine.US_TSCAInd);
				AssertEquals("TSCA disclaim reason on second line should be empty", ZString.Empty, invoiceLine.US_TSCADisclaimReason);
				AssertEquals("Goods description on second line should be empty", ZString.Empty, invoiceLine.JI_Description);
			});

			CombineAssertions("Values on 3rd invoice line", () =>
			{
				var invoiceLine = invoice.JobComInvoiceLines[2];
				AssertEquals("Third line shouldn't have any child lines", 0, invoiceLine.ChildLines.Count());
				AssertEquals("Country of Origin on third line should be CN", "CN", invoiceLine.US_UC_NKCountryOfOrigin);
				AssertEquals("Sup tariff on third line should be 99038803", "99038803", invoiceLine.US_SupTariff);
				AssertEquals("Regular tariff on third line should be 9403907080", "9403907080", invoiceLine.JI_Tariff);
				AssertEquals("Line price on third line should be 10424 = (10377 + 47)", 10424m, invoiceLine.JI_LinePrice);
				AssertEquals("TSCA on third line should be disclaimed", "C", invoiceLine.US_TSCAInd);
				AssertEquals("TSCA disclaim reason on third line should be A", "A", invoiceLine.US_TSCADisclaimReason);
				AssertEquals("Goods description on third line should come from OI block", "OTHER WOODEN FURNITURE", invoiceLine.JI_Description);
			});
		}

		public void TestSetProductExclusion()
		{
			var messageText =
"B  3901JJ8AE                                                                    " +
"10AJJ8  85451101 3901            06   X           3121720          F            " +
"1113-14202600013-142026000               120220      022AB01IL                  " +
"20    3901      HA68                                                            " +
"318B 913                                                                        " +
"40  002 SESE092120        0000000005     0000000063    N                        " +
"41P1016200000000063                                                             " +
"47MSEUDDAB683HAG                                                                " +
"47S13-142026000                                                                 " +
"507220110000 0000000000 0000000369 000000006300KG                               " +
"5201H6ANBK070                                                                   " +
"5401STL154696                                                                   " +
"6249900000128                                                                   " +
"8800000000000 00000000000 00000000000 00000000000                               " +
"8949900000052833                                                                " +
"9000000000000 00000052833 00000000000 00000000000 00000000000 00000000000       " +
"Y  3901JJ8AE                                                                    ";

			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(1, invoice.JobComInvoiceLines.Count);
			var invoiceLines = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().ToArray();
			var line1 = invoiceLines.FirstOrDefault();
			AssertNotNull(line1);
			AssertEquals("", line1.US_ProductExclusion);
			AssertEquals("", line1.US_ExclusionNumber);

			messageText =
"B  3901JJ8AE                                                                    " +
"10AJJ8  85451101 3901            06   X           3121720          F            " +
"1113-14202600013-142026000               120220      022AB01IL                  " +
"20    3901      HA68                                                            " +
"318B 913                                                                        " +
"40  002 SESE092120        0000000005     0000000063    N                        " +
"41P1016200000000063                                                             " +
"47MSEUDDAB683HAG                                                                " +
"47S13-142026000                                                                 " +
"507220110000 0000000000 0000000369 000000006300KG                               " +
"5201H6ANBK070                                                                   " +
"5402STL154696                                                                   " +
"6249900000128                                                                   " +
"8800000000000 00000000000 00000000000 00000000000                               " +
"8949900000052833                                                                " +
"9000000000000 00000052833 00000000000 00000000000 00000000000 00000000000       " +
"Y  3901JJ8AE                                                                    ";

			generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			declaration = Factory.New<JobDeclaration>();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			invoice = declaration.Invoices[0];
			AssertEquals(1, invoice.JobComInvoiceLines.Count);
			invoiceLines = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().ToArray();
			line1 = invoiceLines.FirstOrDefault();
			AssertNotNull(line1);
			AssertEquals("02", line1.US_ProductExclusion);
			AssertEquals("STL154696", line1.US_ExclusionNumber);

			messageText =
"B  3901JJ8AE                                                                    " +
"10AJJ8  85451101 3901            06   X           3121720          F            " +
"1113-14202600013-142026000               120220      022AB01IL                  " +
"20    3901      HA68                                                            " +
"318B 913                                                                        " +
"40  002 SESE092120        0000000005     0000000063    N                        " +
"41P1016200000000063                                                             " +
"47MSEUDDAB683HAG                                                                " +
"47S13-142026000                                                                 " +
"507220110000 0000000000 0000000369 000000006300KG                               " +
"5201H6ANBK070                                                                   " +
"5403STL154698                                                                   " +
"6249900000128                                                                   " +
"8800000000000 00000000000 00000000000 00000000000                               " +
"8949900000052833                                                                " +
"9000000000000 00000052833 00000000000 00000000000 00000000000 00000000000       " +
"Y  3901JJ8AE                                                                    ";

			generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			declaration = Factory.New<JobDeclaration>();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			declaration.UnlockImportEntryNumberAllocationMutex();
			AssertEquals(1, declaration.Invoices.Count);
			invoice = declaration.Invoices[0];
			AssertEquals(1, invoice.JobComInvoiceLines.Count);
			invoiceLines = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().ToArray();
			line1 = invoiceLines.FirstOrDefault();
			AssertNotNull(line1);
			AssertEquals("03", line1.US_ProductExclusion);
			AssertEquals("STL154698", line1.US_ExclusionNumber);
		}

		public void TestImportNewOrganisationWithNoDuplication()
		{
			var time = ZDateTime.UtcNow;
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			query.AddToFilter(OrgHeaderSchema.OH_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, time.AddHours(-1));
			query.AddToFilter(OrgHeaderSchema.OH_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, time.AddHours(1));
			var organisations = Factory.Load<OrgHeader>(query);
			string messageText =
"B  1101113SE                                                                    " +
"SE10R113  87054551 06EI 82-093207300  800974944541503  1503                     " +
"SE11W040121LB63    FTZ0930J01                                                   " +
"SE20CR                                                                          " +
"SE30CN                                    EI 82-093207300                       " +
"SE40001HN PACKING NOT SUBJECT TO FDA                                            " +
"SE41N      00006000                                                             " +
"SE50CN                                    EI 82-093207300                       " +
"SE50MF MAS ACME HONDURAS                                                        " +
"SE5515ZOLI ZIP CHOLOMA II CARRETERA A LA 15JUTOSA K M. 2                        " +
"SE56CHOLOMA                                     6695226        HN               " +
"SE50SE MAS ACME HONDURAS                                                        " +
"SE5515ZOLI ZIP CHOLOMA II CARRETERA A LA 15JUTOSA K M. 2                        " +
"SE56CHOLOMA                                     6695226        HN               " +
"SE6039232100950000006555                                                        " +
"SE40002HN PACKING NOT SUBJECT TO FDA                                            " +
"SE41N      00015000                                                             " +
"SE50CN                                    EI 82-093207300                       " +
"SE50MF MAS ACME HONDURAS                                                        " +
"SE5515ZOLI ZIP CHOLOMA II CARRETERA A LA 15JUTOSA K M. 2                        " +
"SE56CHOLOMA                                     6695226        HN               " +
"SE50SE MAS ACME HONDURAS                                                        " +
"SE5515ZOLI ZIP CHOLOMA II CARRETERA A LA 15JUTOSA K M. 2                        " +
"SE56CHOLOMA                                     6695226        HN               " +
"SE6039239000800000013674                                                        " +
"SE40003HN                                                                       " +
"SE41N      00000200                                                             " +
"SE50CN                                    EI 82-093207300                       " +
"SE50MF MAS ACME HONDURAS                                                        " +
"SE5515ZOLI ZIP CHOLOMA II CARRETERA A LA 15JUTOSA K M. 2                        " +
"SE56CHOLOMA                                     6695226        HN               " +
"SE50SE MAS ACME HONDURAS                                                        " +
"SE5515ZOLI ZIP CHOLOMA II CARRETERA A LA 15JUTOSA K M. 2                        " +
"SE56CHOLOMA                                     6695226        HN               " +
"SE6040169100000000000900                                                        " +
"SE40032HN NET BAGS NOT SUBJECT TO FDA                                           " +
"SE41N      00207120                                                             " +
"SE50CN                                    EI 82-093207300                       " +
"SE50MF MAS ACME HONDURAS                                                        " +
"SE5515ZOLI ZIP CHOLOMA II CARRETERA A LA 15JUTOSA K M. 2                        " +
"SE56CHOLOMA                                     6695226        HN               " +
"SE50SE MAS ACME HONDURAS                                                        " +
"SE5515ZOLI ZIP CHOLOMA II CARRETERA A LA 15JUTOSA K M. 2                        " +
"SE56CHOLOMA                                     6695226        HN               " +
"SE6063079098910000213942                                                        " +
"SE40033CN                                                                       " +
"SE41P      00320112                                                             " +
"SE50CN                                    EI 82-093207300                       " +
"SE50MF CHONGQING SUNRISE FOOTWEAR CO., LTDMIDCNSUZMEC12SUZ                      " +
"SE5515RM 201, UNIT 1, BUILDING 8, BLOCK C15, PALM S PRINGS JINKAI BLVD, YUBEI   " +
"SE5515DISTRICT                                                                  " +
"SE56CHONGQING JIANGBEI INTE                     401120         CN               " +
"SE50SE CHONGQING SUNRISE FOOTWEAR CO., LTDMIDCNSUZMEC12SUZ                      " +
"SE5515RM 201, UNIT 1, BUILDING 8, BLOCK C15, PALM S PRINGS JINKAI BLVD, YUBEI   " +
"SE5515DISTRICT                                                                  " +
"SE56CHONGQING JIANGBEI INTE                     401120         CN               " +
"SE6064059090600001003212                                                        " +
"Y  1101113SE                                                                    ";

			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			Factory.Save();
			var newQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			newQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			newQuery.AddToFilter(OrgHeaderSchema.OH_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, time.AddHours(-1));
			newQuery.AddToFilter(OrgHeaderSchema.OH_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, time.AddHours(1));
			var newOrgs = Factory.Load<OrgHeader>(newQuery);
			AssertEquals("3 organisation was created.", 3, newOrgs.Length - organisations.Length);

			var midOrgs = new List<OrgHeader>();
			foreach (var org in newOrgs)
			{
				var code = org.MainAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates);
				if (!code.IsEmpty)
				{
					midOrgs.Add(org);
				}
			}
			AssertEquals("1 MID organisation was created.", 1, midOrgs.Count);
		}

		public void TestImportNewOrganisationAddress()
		{
			string messageText =
"B  3901SV9SE                                                                    " +
"SE10ASV9           06EI 20 - 836834900  800003452294601                         " +
"SE11W051221FIRM FTZ04400L001                                                    " +
"SE30BY Fisher Footwear LLC                EI 20 - 836834900                     " +
"SE30CN Fisher Footwear LLC                EI 20 - 836834900                     " +
"SE40001CN                                                                       " +
"SE41N      00000646                                                             " +
"SE50MF WENZHOU QIANQI SHOES CO., LTD CNWENCRAIG2                                " +
"SE5515NO.26 FENGJIANG ROAD NEW AND  HIGH -                                      " +
"SE56WENZHOU                                     325000         CN               " +
"SE50SE RICHBURG W FOOTWEAR SERVICES          UYRICCRAIG2                        " +
"SE5515LUIS BONAVITA 1294 OFICINA 116                                            " +
"SE56MONTEVIDEO UY                                                               " +
"SE6064029140500000010653                                                        " +
"SE40002CN                                                                       " +
"SE41N      00001773                                                             " +
"SE50MF DONG GUAN INDELI DIGITAL TECHNOLOGY   CNDONCRAIG3                        " +
"SE5515NO. 2 INDUSTRIAL ZONE, BAIHAO HOU J                                       " +
"SE56DONG GUAN                                   523957         CN               " +
"SE50SE RICHBURG W FOOTWEAR SERVICES          UYRICCRAIG2                        " +
"SE5515LUIS BONAVITA 1294 OFICINA 116                                            " +
"SE56MONTEVIDEO UY                                                               " +
"SE6064029931450000008287                                                        " +
"Y  1101113SE                                                                    ";

			var generator = new ABIInputBlockControlGenerator<BRDAABIB, BRDAABIY>();
			generator.Deserialise(messageText);
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			Factory.Save();
			var time = ZDateTime.UtcNow;
			var newQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			newQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			newQuery.AddToFilter(OrgHeaderSchema.OH_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, time.AddHours(-1));
			newQuery.AddToFilter(OrgHeaderSchema.OH_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, time.AddHours(1));
			newQuery.AddToFilter(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.Equal, "DONG GUAN INDELI DIGITAL TECHNOLOGY");
			var newOrgs = Factory.Load<OrgHeader>(newQuery);
			AssertEquals("1 organisation with name DONG GUAN.", 1, newOrgs.Length);
			AssertEquals("Address 1 is correct", "NO. 2 INDUSTRIAL ZONE, BAIHAO HOU J", newOrgs[0].MainAddress.Address1);
			AssertEquals("Country is correct", "CN", newOrgs[0].MainAddress.Country.Code);
			AssertEquals("City is correct", "DONG GUAN", newOrgs[0].MainAddress.City);
			AssertEquals("Postcode is correct", "523957", newOrgs[0].MainAddress.Postcode);
			AssertEquals("UNLOCO is correct", "CN", newOrgs[0].MainAddress.ClosestPort);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCEXPCHI";
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EntryFilerCode, "XJ5");
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			org.EDICommunicationsModes.AddNew().EK_Module = EDICommunicationsMode.Modules.US_BIRD;
			Factory.Save();
		}

		void AssertInvoiceLineCotton(JobComInvoiceLine invoiceLine, ZString tariff, ZString cottonFeeExempt, ZString[] expecteExemptionCertificates)
		{
			AssertEquals("invoiceLine.JI_Tariff", tariff, invoiceLine.JI_Tariff);
			AssertEquals("invoiceLine.US_CottonFeeExempt", cottonFeeExempt, invoiceLine.US_CottonFeeExempt);
			AssertContainsExactElementsInAnyOrder(expecteExemptionCertificates, invoiceLine.LicenceAndPermits.GetElementsHaving(LicencePermitTypeList.Codes._22).Select(x => x.CY_Data));
		}

		void AssertInvoiceLine(JobComInvoiceLine invoiceLine, ZString tariff, ZString supTariff, ZGuid parentPK, ZGuid parentProductPK, bool isParent)
		{
			AssertEquals("JI_Tariff", tariff, invoiceLine.JI_Tariff);
			AssertEquals("US_SupTariff", supTariff, invoiceLine.US_SupTariff);
			AssertEquals("JI_ParentID", parentPK, invoiceLine.JI_ParentID);
			AssertEquals("US_JI_ParentProduct", parentProductPK, invoiceLine.US_JI_ParentProduct);
			AssertEquals("US_IsParent", isParent, invoiceLine.US_IsParent);
		}
	}
}
