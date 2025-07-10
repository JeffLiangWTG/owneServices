using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PSCReasonCodeCollection))]
	sealed class PSCReasonCodeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PSCReasonCodeCollection>
	{
		public void TestDefaultFromLastFailedTransmission()
		{
			Entry.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Entry.Declaration.InvoiceLines.AddNew();
			Entry.Declaration.InvoiceLines.AddNew();
			Entry.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Entry.Declaration.US_PSC = true;
			Entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace;
			Entry.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryReplace;
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			//Three lines and line 1 and line 3 have reasons
			message.EM_MessageText = "B  3902SV9AE                                               6009006              10ASV9  10001954 3902B00005133   0110 X           2020711       Y               1191-01319900091-013199000                                  IL                  20AAAA3902013111A001APL EMERALD                                                 21V123W                                                                         2200000001PK                                                                    23MAAAAOBLACE27                                                                 318B 891                                                                        36THIS IS A TEST PSC EXPLANATION TEXT THIS IS A TEST PSC EXPLANATION TEXT THI   36S IS A TEST PSC EXPLANATION TEXT THIS IS A TEST PSC EXPLANATION TEXT THIS I   36S A TEST PSC EXPLANATION TEXT THIS IS A TEST PSC EXPLANATION TEXT THIS IS A   36 TEST PSC EXPLANATION TEXT  THIS IS A TEST PSC EXPLANATION TEXT               40  001 CNTW012411        0000000015602670000009000    N                        44COMMERCIAL DESCRIPTION                                                        47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 508516710020 0000185000 0000050000 000005546800NO                               6250100006250                                                                   6249900010500                                                                   63L01L04                                                                        40  002 CNTW012411        0000000017602670000009000    N                        44COMMERCIAL DESCRIPTION                                                        47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 508516710020 0000203500 0000055000 000005546800NO                               6250100006875                                                                   6249900011550                                                                   40  003 CNTW012411        0000000018602670000009000    N                        44COMMERCIAL DESCRIPTION                                                        47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 508516710020 0000222000 0000060000 000005546800NO                               6250100007500                                                                   6249900012600                                                                   63L05                                                                           895010000002062549900000034650                                                  9000000610500 00000055275 00000000000 00000000000 00000000000                   Y  3902SV9AE";
			Entry.Messages.Add(message);
			var collection = GetCollectionToTest();
			AssertEquals(2, collection.Count);
			AssertEquals(PSCLineReasonList.Codes.L01, collection[0].Reason1);
			AssertEquals(PSCLineReasonList.Codes.L04, collection[0].Reason2);
			AssertEquals(PSCLineReasonList.Codes.L05, collection[1].Reason1);
		}

		public void TestCS00199663()
		{
			Entry.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Entry.Declaration.InvoiceLines.AddNew();
			Entry.Declaration.InvoiceLines.AddNew();
			Entry.Declaration.InvoiceLines.AddNew();
			Entry.Declaration.InvoiceLines.AddNew();
			Entry.Declaration.InvoiceLines.AddNew();
			Entry.Declaration.InvoiceLines.AddNew();
			Entry.Declaration.InvoiceLines.AddNew();
			Entry.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Entry.Declaration.US_PSC = true;
			Entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace;
			Entry.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryReplace;
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			message.EM_MessageText = @"B  3310836AE                                  5501836  1   BNSBLISRL_92944      10R836  10000073 3310B00011016   0130 X Y                       Y               1175-24033380075-240333800               100512100512       TX                  20WWCI3310100512                                                                2200000016PC                                                                    23MWWCI63028625                                                                 318B 036                                                                        36UPON REVIEW OF THE ENTRY DOCUMENTATION THE IMPORTER FOUND AN ERROR IN THE C   36OUNTRY OF ORIGIN AND SUBSEQUENT CLASSIFICATION. STYLE NUMBERS DC4441, CD433   36, CD4445 AND CD439 WERE NOT MANUFACTURED IN CHINA AS STATED ON THE INVOICE    36BUT WERE MANUFACTURED BY JUSTIN BRANDS IN CARTHAGE, MO., USA. THE CORRECT C   36LASSIFICATION IS 9801.00.1095, COUNTRY OF ORIGIN IS US. WE RESPECTFULLY REQ   36UEST A REFUND OF $2244.80 FOR THE DUTIES PAID FOR THESE ITEMS.                40  001 CNCA100512        0000000051     0000000520    Y                        42XOJUSBRA1071CAL118233A           0001 0001                                    44CW974/CN; COWHIDE BOOTS, EE; 196 PRS                                          47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 506403913040 0000046845 0000009369 000000019600PRS                              6249900003245                                                                   40  002 USCA100512        0000000059     0000000595    Y                        42XOJUSBRA1071CAL118233A           0002 0002                                    44CD4441/CN; COWHIDE BOOTS, EE; 163 PRS                                         47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 509801001095 0000000000 0000010703             X                                63L01L02L03                                                                     40  003 USCA100512        0000000051     0000000521    Y                        42XOJUSBRA1071CAL118233A           0003 0003                                    44CD433/CN; COWHIDE BOOTS, EE; 140 PRS                                          47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 509801001095 0000000000 0000009383             X                                63L04L05L06                                                                     40  004 USCA100512        0000000050     0000000501    Y                        42XOJUSBRA1071CAL118233A           0004 0004                                    44CD4445/CN; COWHIDE BOOTS, EE; 140 PRS                                         47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 509801001095 0000000000 0000009024             X                                63L07L08                                                                        40  005 USCA100512        0000000071     0000000715    Y                        42XOJUSBRA1071CAL118233A           0005 0005                                    44CD439/CN; COWHIDE BOOTS, EE; 196 PRS                                          47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 509801001095 0000000000 0000012877             X                                63L08                                                                           40  006 CNCA100512        0000000053     0000000535    Y                        42XOJUSBRA1071CAL118233A           0006 0006                                    44CW4845/CN; COWHIDE BOOTS, EE; 186 PRS                                         47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 506403913040 0000048130 0000009626 000000018600PRS                              6249900003334                                                                   40  007 CNCA100512        0000000035     0000000351    Y                        42XOJUSBRA1071CAL118233A           0007 0007                                    44981/CN; COWHIDE BOOTS, EE; 249 PRS                                            47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 506403913040 0000031560 0000006312 000000024900PRS                              6249900002186                                                                   40  008 CNCA100512        0000000069     0000000699    Y                        42XOJUSBRA1071CAL118233A           0008 0008                                    44NL4035/MX; COWHIDE BOOTS, B; 228 PRS                                          47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 506403513060 0000062870 0000012574 000000022800PRS                              6249900004356                                                                   40  009 CNCA100512        0000000035     0000000350    Y                        42XOJUSBRA1071CAL118233A           0009 0009                                    44TW1061/CN; COWHIDE BOOTS, EE; 109 PRS                                         47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 506403913040 0000032285 0000006457 000000010900PRS                              6249900002237                                                                   40  010 CNCA100512        0000000016     0000000158    Y                        42XOJUSBRA1071CAL118233A           0010 0010                                    44RR1013/CN; COWHIDE BOOTS, D; 50 PRS                                           47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 506403913040 0000014225 0000002845 000000005000PRS                              6249900000986                                                                   40  011 CNCA100512        0000000043     0000000433    Y                        42XOJUSBRA1071CAL118233A           0011 0011                                    44RR1013/CN; COWHIDE BOOTS, EE; 137 PRS                                         47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 506403913040 0000038975 0000007795 000000013700PRS                              6249900002700                                                                   40  012 MXCA100512        0000000006     0000000057    Y                        42XOJUSBRA1071CAL118233A           0012 0012                                    44VF6012/MX; COWHIDE BOOTS,B; 18 PRS                                            47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 506403513060 0000005125 0000001025 000000001800PRS                              6249900000355                                                                   40  013 MXCA100512        0000000016     0000000164    Y                        42XOJUSBRA1071CAL118233A           0013 0013                                    44VF6010/MX; COWHIDE BOOTS,B; 51 PRS                                            47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 506403513060 0000014805 0000002961 000000005100PRS                              6249900001026                                                                   40  014 MXCA100512        0000000006     0000000061    Y                        42XOJUSBRA1071CAL118233A           0014 0014                                    44VF6013/MX; COWHIDE BOOTS,B; 19 PRS                                            47MXOJUSBRA1071CAL                                                              47C75-240333800                                                                 47S75-240333800                                                                 506403513060 0000005505 0000001101 000000001900PRS                              6249900000381                                                                   8949900000020806                                                                9000000300325 00000020806 00000000000 00000000000 00000000000                   Y  3310836AE";
			Entry.Messages.Add(message);
			var coll = new PSCReasonCodeCollection(Entry);
			AssertEquals(4, coll.Count);
			AssertDefaultedReasonLine(coll, 0, "002", "L01", "L02", "L03", "", "");
			AssertDefaultedReasonLine(coll, 1, "003", "L04", "L05", "L06", "", "");
			AssertDefaultedReasonLine(coll, 2, "004", "L07", "L08", "", "", "");
			AssertDefaultedReasonLine(coll, 3, "005", "L08", "", "", "", "");
		}

		void AssertDefaultedReasonLine(PSCReasonCodeCollection coll, int index, ZString lineNumber, ZString reason1, ZString reason2, ZString reason3, ZString reason4, ZString reason5)
		{
			var element = coll[index];
			AssertEquals("line number", lineNumber, element.LineNumber);
			AssertEquals("reason1", reason1, element.Reason1);
			AssertEquals("reason2", reason2, element.Reason2);
			AssertEquals("reason3", reason3, element.Reason3);
			AssertEquals("reason4", reason4, element.Reason4);
			AssertEquals("reason5", reason5, element.Reason5);
		}

		public void TestGetHeaderAndLineReasonCode()
		{
			var collection = GetCollectionToTest();
			var element = collection.AddNew();
			element.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			element.LineNumber = "002";
			var element2 = collection.AddNew();
			element2.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			element2.LineNumber = "005";
			var element3 = collection.AddNew();
			element3.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			AssertEquals("Header", element3, collection.GetHeaderReasonCode());
			AssertEquals("Line with number", element2, collection.GetLineReasonCode("005"));
		}

		public void TestSaveLastPSCReasonCodes()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
			int existingPSCReasonCodeLineCount = Factory.GetDatabaseCount(typeof(PSCReasonCusCodeData), query);
			var collection = GetCollectionToTest();
			var element = collection.AddNew();
			element.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			element.Reason1 = PSCHeaderReasonList.Codes.H01;
			var element2 = collection.AddNew();
			element2.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			element2.LineNumber = "001";
			element2.Reason1 = PSCHeaderReasonList.Codes.H01;
			Entry.MergedLines.AddNew();
			collection.SaveLastPSCReasonCodes();
			Factory.Save();
			query = new ZQuery();
			query.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
			int count = Factory.GetDatabaseCount(typeof(PSCReasonCusCodeData), query);
			AssertEquals("There are 2 more elements", existingPSCReasonCodeLineCount + 2, count);
			collection = GetCollectionToTest();
			element = collection.AddNew();
			element.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			element.LineNumber = "001";
			element.Reason1 = PSCHeaderReasonList.Codes.H01;
			collection.SaveLastPSCReasonCodes();
			Factory.Save();
			query = new ZQuery();
			query.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
			count = Factory.GetDatabaseCount(typeof(PSCReasonCusCodeData), query);
			AssertEquals("There are 1 more element", existingPSCReasonCodeLineCount + 1, count);
		}

		protected override PSCReasonCodeCollection GetCollectionToTest() => new PSCReasonCodeCollection(Entry);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new PSCReasonCode(Entry, Collection);

		CusEntryHeader entry;
		CusEntryHeader Entry
		{
			get
			{
				if (entry == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EnableENS = true;
					declaration.US_EntryFilerCode = "XJ5";
					declaration.ImportEntryNumber = "ENT32432";
					declaration.Invoices.AddNew();
					declaration.InvoiceLines.AddNew();
					declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
					entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				}

				return entry;
			}
		}
	}
}
