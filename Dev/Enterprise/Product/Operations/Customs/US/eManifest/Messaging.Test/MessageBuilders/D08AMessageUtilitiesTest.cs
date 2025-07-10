using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.D08A.Elements;
using Enterprise.Edifact.D08A.Messages.CUSRES;
using Enterprise.Edifact.D08A.Segments;
using Converter = Enterprise.Customs.US.eManifest.Messaging.CompleteManifestDataConverter;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	sealed class D08AMessageUtilitiesPopulateDetailsTest : TestCaseWithFactory
	{
		public void TestPopulateUNH()
		{
			var unh = new UNHSegment();
			D08AMessageUtilities.PopulateUNH(unh, "123456", "CUSCAR", "D", "03B", "UN");
			AssertEquals("UNH", "UNH+123456+CUSCAR:D:03B:UN'", unh.ToString(characterSet));
		}

		public void TestPopulateBGM()
		{
			var bgm = new BGMSegment();
			D08AMessageUtilities.PopulateBGM(bgm, DocumentNameCodeList.CustomsManifest, "STANDARD", "1234567890", MessageFunctionCodeList.Original);
			AssertEquals("BGM", "BGM+85:::STANDARD+1234567890+9'", bgm.ToString(characterSet));
		}

		public void TestPopulateDTM()
		{
			var dtm = new DTMSegment();
			D08AMessageUtilities.PopulateDTM(dtm, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated, new ZDateTime(2011, 09, 08, 11, 17, 0));
			AssertEquals("DTM", "DTM+132:201109081117:203'", dtm.ToString(characterSet));

			D08AMessageUtilities.PopulateDTM(dtm, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated, new ZDate(2011, 09, 08));
			AssertEquals("DTM", "DTM+132:20110908:102'", dtm.ToString(characterSet));

			D08AMessageUtilities.PopulateDTM(dtm, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated, new ZDate(2011, 09, 08), invertedFormat: true);
			AssertEquals("DTM", "DTM+132:08092011:4'", dtm.ToString(characterSet));

			D08AMessageUtilities.PopulateDTM(dtm, DateOrTimeOrPeriodFunctionCodeQualifierList.CoverageDuration, 2011);
			AssertEquals("DTM", "DTM+429:2011:602'", dtm.ToString(characterSet));
		}

		public void TestPopulateLOC()
		{
			var loc = new LOCSegment();
			D08AMessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.PlaceOfArrival, "1234", "77");
			AssertEquals("LOC", "LOC+60+1234:77'", loc.ToString(characterSet));

			loc = new LOCSegment();
			D08AMessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.PlaceOfArrival, "1234", "77", "1234", "77");
			AssertEquals("LOC", "LOC+60+:77::1234+:77::1234'", loc.ToString(characterSet));

			loc = new LOCSegment();
			D08AMessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.PlaceOfArrival, "1234", "77", "1234", "77", "1234", "77");
			AssertEquals("LOC", "LOC+60+:77::1234+:77::1234+:77::1234'", loc.ToString(characterSet));
		}

		public void TestPopulateRFF()
		{
			var rff = new RFFSegment();
			D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.OriginatorsReference, "123456");
			AssertEquals("RFF", "RFF+ABO:123456'", rff.ToString(characterSet));

			D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.OriginatorsReference);
			AssertEquals("RFF", "RFF+ABO'", rff.ToString(characterSet));
		}

		public void TestPopulateNAD()
		{
			var nad = new NADSegment();
			D08AMessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.Consignee, "123456", "109");
			AssertEquals("NAD", "NAD+CN+123456:109'", nad.ToString(characterSet));

			D08AMessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.Consignee, "123456", "109", "789456", "LONG PARTY NAME THAT SHOULD BE SPLIT INTO TWO ELEMENTS");
			AssertEquals("NAD", "NAD+CN+123456:109+789456+LONG PARTY NAME THAT SHOULD BE SPLI:T INTO TWO ELEMENTS'", nad.ToString(characterSet));

			D08AMessageUtilities.PopulateNAD(nad, PartyNameFormatCodeList.NameComponentsInSequenceAsDefinedInDescriptionBelow, "TURNER", "BILL", "BOOTSTRAP");
			AssertEquals("NAD", "NAD+CN+123456:109+789456+TURNER:BILL:BOOTSTRAP:::1'", nad.ToString(characterSet));

			nad = new NADSegment();
			D08AMessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.Consignee, "123456", "109", "789456", "LONG PÄRTY NÄME");
			AssertEquals("NAD", "NAD+CN+123456:109+789456+LONG PARTY NAME'", nad.ToString(characterSet));

			D08AMessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.Consignee, "123456", "109", "789456", "测试NAME");
			AssertEquals("NAD", "NAD+CN+123456:109+789456'", nad.ToString(characterSet));
		}

		public void TestPopulateATT()
		{
			var att = new ATTSegment();
			D08AMessageUtilities.PopulateATT(att, AttributeFunctionCodeQualifierList.Person, "M");
			AssertEquals("ATT", "ATT+2++M'", att.ToString(characterSet));
		}

		public void TestPopulateEMP()
		{
			var emp = new EMPSegment();
			D08AMessageUtilities.PopulateEMP(emp, EmploymentDetailsCodeQualifierList.Profession, "8456");
			AssertEquals("EMP", "EMP+4+++1:::8456'", emp.ToString(characterSet));
		}

		public void TestPopulateNAT()
		{
			var nat = new NATSegment();
			D08AMessageUtilities.PopulateNAT(nat, NationalityCodeQualifierList.CurrentNationality, "US", CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization);
			AssertEquals("NAT", "NAT+2+US::5'", nat.ToString(characterSet));

			D08AMessageUtilities.PopulateNAT(nat, NationalityCodeQualifierList.CurrentNationality, "US");
			AssertEquals("NAT", "NAT+2+US'", nat.ToString(characterSet));
		}

		public void TestPopulateCTA()
		{
			var cta = new CTASegment();
			D08AMessageUtilities.PopulateCTA(cta, ContactFunctionCodeList.InformationContact);
			AssertEquals("CTA", "CTA+IC'", cta.ToString(characterSet));

			D08AMessageUtilities.PopulateCTA(cta, ContactFunctionCodeList.InformationContact, "Contact Name");
			AssertEquals("CTA", "CTA+IC+:CONTACT NAME'", cta.ToString(characterSet));

			D08AMessageUtilities.PopulateCTA(cta, ContactFunctionCodeList.InformationContact, "Contact Näme");
			AssertEquals("CTA", "CTA+IC+:CONTACT NAME'", cta.ToString(characterSet));

			D08AMessageUtilities.PopulateCTA(cta, ContactFunctionCodeList.InformationContact, "测试NAME");
			AssertEquals("CTA", "CTA+IC'", cta.ToString(characterSet));
		}

		public void TestPopulateCOM()
		{
			var com = new COMSegment();
			D08AMessageUtilities.PopulateCOM(com, CommunicationMeansTypeCodeList.ElectronicMail, "test@test.com");
			AssertEquals("COM", "COM+TEST@TEST.COM:EM'", com.ToString(characterSet));
		}

		public void TestPopulateFTX()
		{
			var ftx = new FTXSegment();
			D08AMessageUtilities.PopulateFTX(ftx, TextSubjectCodeQualifierList.SpecialInstructions, new ZString[] { "1", "2", "3", "4", "5" });
			AssertEquals("FTX", "FTX+SIN+++1:2:3:4:5'", ftx.ToString(characterSet));
		}

		public void TestPopulateTDT()
		{
			var tdt = new TDTSegment();
			D08AMessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.AtBorder, "03", "TR", "I", "109", "123456", "US");
			AssertEquals("TDT", "TDT+11++03+:::TR++I++:109::123456:US'", tdt.ToString(characterSet));

			D08AMessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.AtBorder);
			AssertEquals("TDT", "TDT+11'", tdt.ToString(characterSet));

			D08AMessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.AtBorder, "03", "LOCK", "172");
			AssertEquals("TDT", "TDT+11++03++LOCK:172'", tdt.ToString(characterSet));
		}

		public void TestPopulateEQD()
		{
			var eqd = new EQDSegment();
			D08AMessageUtilities.PopulateEQD(eqd, EquipmentTypeCodeQualifierList.Container, "123456", "109");
			AssertEquals("EQD", "EQD+CN+123456:109'", eqd.ToString(characterSet));
		}

		public void TestPopulateSEL()
		{
			var sel = new SELSegment();
			D08AMessageUtilities.PopulateSEL(sel, "123456");
			AssertEquals("SEL", "SEL+123456'", sel.ToString(characterSet));
		}

		public void TestPopulateCNI()
		{
			var cni = new CNISegment();
			D08AMessageUtilities.PopulateCNI(cni, "1", DocumentStatusCodeList.Status0);
			AssertEquals("CNI", "CNI+1+:22'", cni.ToString(characterSet));
		}

		public void TestPopulateDOC()
		{
			var doc = new DOCSegment();
			D08AMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.HouseBillOfLading, "34", "123456", "7894");
			AssertEquals("DOC", "DOC+714:::34+123456::7894'", doc.ToString(characterSet));

			doc = new DOCSegment();
			D08AMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.HouseBillOfLading, string.Empty, string.Empty, "7894", DocumentStatusCodeList.Status0);
			AssertEquals("DOC", "DOC+714+:22'", doc.ToString(characterSet));

			doc = new DOCSegment();
			D08AMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.HouseBillOfLading, "34", "123456");
			AssertEquals("DOC", "DOC+714:::34+123456'", doc.ToString(characterSet));

			doc = new DOCSegment();
			D08AMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.HouseBillOfLading, "34", "123456", "7894", DocumentStatusCodeList.Status0);
			AssertEquals("DOC", "DOC+714:::34+123456:22:7894'", doc.ToString(characterSet));
		}

		public void TestPopulateCNT()
		{
			var cnt = new CNTSegment();
			D08AMessageUtilities.PopulateCNT(cnt, ControlTotalTypeCodeQualifierList.DaysUnderCustomsTransitControl, 15);
			AssertEquals("CNT", "CNT+58:15'", cnt.ToString(characterSet));
		}

		public void TestPopulateQTY()
		{
			var qty = new QTYSegment();
			D08AMessageUtilities.PopulateQTY(qty, QuantityTypeCodeQualifierList.SplitQuantity, 15);
			AssertEquals("QTY", "QTY+11:15'", qty.ToString(characterSet));
		}

		public void TestPopulateGEI()
		{
			var gei = new GEISegment();
			D08AMessageUtilities.PopulateGEI(gei, ProcessingInformationCodeQualifierList.GetFromString("7"), "134");
			AssertEquals("GEI", "GEI+7+134'", gei.ToString(characterSet));
		}

		public void TestPopulateTSR()
		{
			var tsr = new TSRSegment();
			D08AMessageUtilities.PopulateTSR(tsr, ContractAndCarriageConditionCodeList.DoorToDoor);
			AssertEquals("TSR", "TSR+27'", tsr.ToString(characterSet));
		}

		public void TestPopulateGID()
		{
			var gid = new GIDSegment();
			D08AMessageUtilities.PopulateGID(gid, 2);
			AssertEquals("GID", "GID+2'", gid.ToString(characterSet));
		}

		public void TestPopulatePAC()
		{
			var pac = new PACSegment();
			D08AMessageUtilities.PopulatePAC(pac, 200, "BOX");
			AssertEquals("PAC", "PAC+200++BOX'", pac.ToString(characterSet));
		}

		public void TestPopulateMEA()
		{
			var mea = new MEASegment();
			D08AMessageUtilities.PopulateMEA(mea, MeasurementPurposeCodeQualifierList.ItemWeight, 1234.5678m, "K");
			AssertEquals("MEA", "MEA+AAI++K:1234.5678'", mea.ToString(characterSet));

			D08AMessageUtilities.PopulateMEA(mea, MeasurementPurposeCodeQualifierList.ItemWeight, 1.000000m, "K");
			AssertEquals("MEA", "MEA+AAI++K:1'", mea.ToString(characterSet));
		}

		public void TestPopulateMOA()
		{
			var moa = new MOASegment();
			D08AMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.GoodsItemForCustomsDeclaredValueAmount, 1234.5m, 2, "USD");
			AssertEquals("MOA", "MOA+40:1234.50:USD'", moa.ToString(characterSet));
		}

		public void TestPopulateSGP()
		{
			var sgp = new SGPSegment();
			D08AMessageUtilities.PopulateSGP(sgp, "123456", "109");
			AssertEquals("SGP", "SGP+123456:109'", sgp.ToString(characterSet));
		}

		public void TestPopulateDGS()
		{
			var dgs = new DGSSegment();
			D08AMessageUtilities.PopulateDGS(dgs, "UN1234");
			AssertEquals("DGS", "DGS+++UN1234'", dgs.ToString(characterSet));
		}

		public void TestPopulatePCI()
		{
			var pci = new PCISegment();
			D08AMessageUtilities.PopulatePCI(pci, new ZString[] { "1" });
			AssertEquals("PCI", "PCI++1'", pci.ToString(characterSet));

			D08AMessageUtilities.PopulatePCI(pci, new ZString[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" });
			AssertEquals("PCI", "PCI++1:2:3:4:5:6:7:8:9:10'", pci.ToString(characterSet));
		}

		public void TestPopulateCST()
		{
			var cst = new CSTSegment();
			D08AMessageUtilities.PopulateCST(cst, new ZString[] { "1", "2", "3", "4", "5" }, "117");
			AssertEquals("CST", "CST++1:117+2:117+3:117+4:117+5:117'", cst.ToString(characterSet));
		}

		public void TestPopulateUNT()
		{
			var unt = new UNTSegment();
			D08AMessageUtilities.PopulateUNT(unt, "10", "20");
			AssertEquals("UNT", "UNT+10+20'", unt.ToString(characterSet));
		}

		public void TestPopulateTAX()
		{
			var tax = new TAXSegment();
			D08AMessageUtilities.PopulateTAX(tax, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty);
			AssertEquals("TAX", "TAX+5'", tax.ToString(characterSet));
		}

		public void TestPopulateFII()
		{
			var fii = new FIISegment();
			D08AMessageUtilities.PopulateFII(fii, PartyFunctionCodeQualifierList.Surety, "Insurance Company");
			AssertEquals("FII", "FII+SY++::::::INSURANCE COMPANY'", fii.ToString(characterSet));
		}

		public void TestPopulatePNA()
		{
			var pna = new PNASegment();

			D08AMessageUtilities.PopulatePNA(
				pna,
				PartyFunctionCodeQualifierList.CrewMember,
				"FirstName",
				NameComponentTypeCodeQualifierList.ChristianName,
				"MiddleName",
				NameComponentTypeCodeQualifierList.OfficialSecondChristianName,
				"LastName",
				NameComponentTypeCodeQualifierList.Surname);

			AssertEquals("PNA", "PNA+FM+++++2:FIRSTNAME+7:MIDDLENAME+1:LASTNAME'", pna.ToString(characterSet));
		}

		public void TestPopulateGIS()
		{
			var gis = new GISSegment();
			D08AMessageUtilities.PopulateGIS(gis, ProcessingIndicatorDescriptionCodeList.Import, "1");
			AssertEquals("GIS", "GIS+23:::1'", gis.ToString(characterSet));
		}

		public void TestPopulatePDI()
		{
			var pdi = new PDISegment();
			D08AMessageUtilities.PopulatePDI(pdi, Core.Constants.Genders.Man);
			AssertEquals("PDI", "PDI+M'", pdi.ToString(characterSet));
		}

		public void TestPopulateIHC()
		{
			var ihc = new IHCSegment();
			D08AMessageUtilities.PopulateIHC(ihc, PersonCharacteristicCodeQualifierList.SkinColour, "AR");
			AssertEquals("IHC", "IHC+1+:::AR'", ihc.ToString(characterSet));
		}

		readonly UNOCCharacterSet characterSet = new UNOCCharacterSet();
	}

	sealed class D08AMessageUtilitiesGetDetailsTest : TestCaseWithFactory
	{
		public void TestGetAmount()
		{
			var group9Section = new SegmentGroup9MessageSection(1);
			var moaSection = group9Section.InstantiateAChildAndAddItToChildrenCollection().MOA;
			var moa = moaSection.InstantiateAChildAndAddItToChildrenCollection();
			moa.Parse(characterSet, "MOA+40:123.40");
			AssertEquals(123.4m, D08AMessageUtilities.GetAmount(group9Section, MonetaryAmountTypeCodeQualifierList.GoodsItemForCustomsDeclaredValueAmount));
		}

		public void TestGetContact()
		{
			var group8Section = new SegmentGroup8MessageSection(1);
			var comSection = group8Section.InstantiateAChildAndAddItToChildrenCollection().COM;
			var com = comSection.InstantiateAChildAndAddItToChildrenCollection();
			com.Parse(characterSet, "COM+8005551212:TE");
			AssertEquals("8005551212", D08AMessageUtilities.GetContact(group8Section, CommunicationMeansTypeCodeList.Telephone));
		}

		public void TestGetCodes()
		{
			var group12Section = new SegmentGroup12MessageSection(1);
			var cstSection = group12Section.InstantiateAChildAndAddItToChildrenCollection().CST;
			cstSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "CST++13213213:117");
			cstSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "CST++21646544:117");
			AssertEquals("13213213, 21646544", D08AMessageUtilities.GetCodes(group12Section, Converter.IdentificationCodes.C4Code).ToStringDelimited(", "));
		}

		public void TestGetLocation()
		{
			var locSection = new LOCSegmentMessageSection(1);
			locSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "LOC+89+US:162");
			locSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "LOC+89+IL:163");
			AssertEquals("US", D08AMessageUtilities.GetLocation(locSection, LocationFunctionCodeQualifierList.PlaceOfRegistration));
			AssertEquals("IL", D08AMessageUtilities.GetLocation(locSection, LocationFunctionCodeQualifierList.PlaceOfRegistration, Converter.IdentificationCodes.CountrySubEntity));
		}

		public void TestGetProcessingIndicator()
		{
			var geiSection = new GEISegmentMessageSection(1);
			geiSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "GEI+7+135");
			AssertEquals("135", D08AMessageUtilities.GetProcessingIndicator(geiSection, ProcessingInformationCodeQualifierList.GetFromString("7")));
		}

		public void TestGetShippingMarks()
		{
			var pciSection = new PCISegmentMessageSection(1);
			pciSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "PCI++MARKS LINE 1");
			pciSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "PCI++MARKS LINE 2");
			AssertEquals("MARKS LINE 1MARKS LINE 2", D08AMessageUtilities.GetShippingMarks(pciSection));
		}

		public void TestGetErrorCodesAndRejectComments()
		{
			const string expected = "060 - XXXX Bill Rejected XXXX; 033 - Invalid DDPP";
			var segments = new[] { "ERP+2", "ERC+060", "FTX+AAO+++XXXX Bill Rejected XXXX", "ERP+2", "ERC+033", "FTX+AAO+++Invalid DDPP" };

			var group4Section = new SegmentGroup4MessageSection(2);
			group4Section.PopulateFromMessage(characterSet, new MessageDataStream(segments));
			AssertEquals(expected, D08AMessageUtilities.GetErrorCodesAndRejectComments(group4Section).Select(x => x.ToStringDelimited(" - ")).ToStringDelimited("; "));

			var group14Section = new SegmentGroup14MessageSection(2);
			group14Section.PopulateFromMessage(characterSet, new MessageDataStream(segments));
			AssertEquals(expected, D08AMessageUtilities.GetErrorCodesAndRejectComments(group14Section).Select(x => x.ToStringDelimited(" - ")).ToStringDelimited("; "));
		}

		public void TestGetFreeText()
		{
			var segments = new[] { "CST++13213213:117", "FTX+AAC+++UNDG132:NAMEBIG BOSS:TELE31264846516", "FTX+AAC+++UNDG456:NAMEBIG BOSS:TELE31264846516" };
			var group12Section = new SegmentGroup12MessageSection(2);
			group12Section.PopulateFromMessage(characterSet, new MessageDataStream(segments));
			AssertEquals("UNDG132\r\nNAMEBIG BOSS\r\nTELE31264846516", D08AMessageUtilities.GetFreeText(group12Section[0].FTX, TextSubjectCodeQualifierList.DangerousGoodsAdditionalInformation));
			AssertEquals("UNDG132\r\nNAMEBIG BOSS\r\nTELE31264846516; UNDG456\r\nNAMEBIG BOSS\r\nTELE31264846516", D08AMessageUtilities.GetFreeTextGroupBySegment(group12Section, TextSubjectCodeQualifierList.DangerousGoodsAdditionalInformation).ToStringDelimited("; "));
			AssertEquals("UNDG132\r\nNAMEBIG BOSS\r\nTELE31264846516; UNDG456\r\nNAMEBIG BOSS\r\nTELE31264846516", D08AMessageUtilities.GetFreeTextGroupBySegment(group12Section[0].FTX, TextSubjectCodeQualifierList.DangerousGoodsAdditionalInformation).ToStringDelimited("; "));
			AssertEquals("UNDG132; NAMEBIG BOSS; TELE31264846516; UNDG456; NAMEBIG BOSS; TELE31264846516", D08AMessageUtilities.GetFreeTextAsEnumerable(group12Section, TextSubjectCodeQualifierList.DangerousGoodsAdditionalInformation).ToStringDelimited("; "));
		}

		public void TestGetQuantityAndUnits()
		{
			var meaSection = new MEASegmentMessageSection(1);
			meaSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "MEA+AAI++K:2650");
			AssertEquals(2650m, D08AMessageUtilities.GetQuantity(meaSection, MeasurementPurposeCodeQualifierList.ItemWeight));
			AssertEquals("K", D08AMessageUtilities.GetUnitOfMeasure(meaSection, MeasurementPurposeCodeQualifierList.ItemWeight));

			var pacSection = new PACSegmentMessageSection(1);
			pacSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "PAC+100++BOX");
			AssertEquals(100m, D08AMessageUtilities.GetQuantity(pacSection));
			AssertEquals("BOX", D08AMessageUtilities.GetUnitOfMeasure(pacSection));
		}

		public void TestGetDocReferences()
		{
			var docSection = new DOCSegmentMessageSection(1);
			docSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "DOC+929:::63+654984654");
			AssertEquals("63", D08AMessageUtilities.GetDocType(docSection, DocumentNameCodeList.GoodsDeclarationForImportation));
			AssertEquals("654984654", D08AMessageUtilities.GetDocReference(docSection, DocumentNameCodeList.GoodsDeclarationForImportation));
		}

		public void TestGetMessageReferences()
		{
			var bgmSection = new BGMSegmentMessageSection(1);
			bgmSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "BGM+34:::ST+LOCKMAN0000001+22");
			AssertEquals("34", D08AMessageUtilities.GetMessageCode(bgmSection).ToString());
			AssertEquals("LOCKMAN0000001", D08AMessageUtilities.GetMessageReference(bgmSection));
			AssertEquals("22", D08AMessageUtilities.GetMessageFunction(bgmSection).ToString());
		}

		public void TestGetPartyReference()
		{
			var group7Section = new SegmentGroup7MessageSection(1);
			var nadSection = group7Section.InstantiateAChildAndAddItToChildrenCollection().NAD;
			nadSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "NAD+CA+456734654:109");

			var group1Section = new SegmentGroup1MessageSection(1);
			nadSection = group1Section.InstantiateAChildAndAddItToChildrenCollection().NAD;
			nadSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "NAD+CA+456734654:109");

			AssertEquals("456734654", D08AMessageUtilities.GetPartyReference(group7Section, PartyFunctionCodeQualifierList.Carrier));
			AssertEquals("456734654", D08AMessageUtilities.GetPartyReference(group1Section, PartyFunctionCodeQualifierList.Carrier));
		}

		public void TestGetReference()
		{
			var segments = new[] { "RFF+RFA:03", "RFF+SN:12345", "RFF+SN:45613", "RFF+IIT:EC", "RFF+IIT:MC" };
			var group3Section = new SegmentGroup3MessageSection(5);
			group3Section.PopulateFromMessage(characterSet, new MessageDataStream(segments));

			AssertEquals("03", D08AMessageUtilities.GetReference(group3Section[0].RFF, ReferenceCodeQualifierList.GetFromString("RFA")));
			AssertEquals("03", D08AMessageUtilities.GetReference(group3Section, ReferenceCodeQualifierList.GetFromString("RFA")));
			AssertEquals("12345, 45613", D08AMessageUtilities.GetReferences(group3Section, ReferenceCodeQualifierList.TransportEquipmentSealIdentifier).ToStringDelimited(", "));

			var groups3 = D08AMessageUtilities.GetReferenceGroups(group3Section, ReferenceCodeQualifierList.GetFromString("IIT")).ToList();
			AssertEquals("EC, MC", D08AMessageUtilities.GetReferences(groups3, ReferenceCodeQualifierList.GetFromString("IIT")).ToStringDelimited(", "));
			AssertEquals(string.Empty, D08AMessageUtilities.GetReferences(groups3, ReferenceCodeQualifierList.TransportEquipmentSealIdentifier).ToStringDelimited(", "));
		}

		public void TestGetTransportDetails()
		{
			var tdtSection = new TDTSegmentMessageSection(1);
			tdtSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "TDT+11++03+:::BT+LOCK+I++:109::10000324");
			AssertEquals(tdtSection[0], D08AMessageUtilities.GetTransportDetails(tdtSection, Converter.IdentificationCodes.ACEId));
			AssertEquals("10000324", D08AMessageUtilities.GetTransportIdentifier(tdtSection, Converter.IdentificationCodes.ACEId));
		}

		public void TestGetDate()
		{
			var dtmSection = new DTMSegmentMessageSection(1);
			dtmSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "DTM+132:200412301200:203");
			AssertEquals(new ZDateTime(2004, 12, 30, 12, 0, 0), D08AMessageUtilities.GetDateTime(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated));

			dtmSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "DTM+329:19901230:203");
			AssertEquals(new ZDateTime(1990, 12, 30), D08AMessageUtilities.GetDateTime(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.PersonBirthDateTime));

			dtmSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "DTM+329:30121990:203");
			AssertEquals(new ZDateTime(1990, 12, 30), D08AMessageUtilities.GetDateTime(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.PersonBirthDateTime));
		}

		protected override void SetUp()
		{
			base.SetUp();
			characterSet = new UNOACharacterSet();
		}

		UNOACharacterSet characterSet;
	}
}
