using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.V902.Elements;
using Enterprise.Edifact.V902.Segments;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EDIFACTMessageSegmentBuilder))]
sealed class EDIFACTMessageSegmentBuilderTest : TestCaseWithFactory
{
	public void TestAddNewLOCSegment() => CombineAssertions(() =>
	{
		var locationQualifier = PlaceLocationQualifierList.CountryOfExportationDespatch;
		const string locationIdentification = "SE";
		var responsibleAgency = CodeListResponsibleAgencyCodedList.GetFromString("NO1");

		var locSection1 = new LOCSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewLOCSegment(locSection1, null, locationIdentification, responsibleAgency);
		AssertEquals("When no locationQualifier, Count", expected: 0, locSection1.Count);
		AssertEquals("When no locationQualifier, ToString", expected: string.Empty, locSection1.ToString(characterSet));

		var locSection2 = new LOCSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewLOCSegment(locSection2, locationQualifier, ZString.Empty, responsibleAgency);
		AssertEquals("When no locationIdentification, Count", expected: 0, locSection2.Count);
		AssertEquals("When no locationIdentification, ToString", string.Empty, locSection2.ToString(characterSet));

		var locSection3 = new LOCSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewLOCSegment(locSection3, locationQualifier, locationIdentification);
		AssertEquals("When no responsibleAgency, Count", expected: 1, locSection3.Count);
		AssertEquals("When no responsibleAgency, ToString", "LOC+35:SE'", locSection3.ToString(characterSet));

		var locSection4 = new LOCSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewLOCSegment(locSection4, locationQualifier, locationIdentification, responsibleAgency);
		AssertEquals("When all values, Count", expected: 1, locSection4.Count);
		AssertEquals("When all values, ToString", "LOC+35:SE::NO1'", locSection4.ToString(characterSet));
	});

	public void TestAddNewMEASegment() => CombineAssertions(() =>
	{
		var measurementDimension = MeasurementDimensionCodedList.GrossWeight;
		const string measurementUnit = NOCustomsFormulaUnitCodeList.Codes.KGM;
		const string measurementValue = "110";

		var meaSection1 = new MEASegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewMEASegment(meaSection1, measurementDimension, measurementUnit, ZString.Empty);
		AssertEquals("With no measurementValue", ZString.Empty, meaSection1.ToString(characterSet));

		var meaSection2 = new MEASegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewMEASegment(meaSection2, measurementDimension, measurementUnit, measurementValue);
		AssertEquals("With all values", "MEA+AAF+G+KGM:110'", meaSection2.ToString(characterSet));
	});

	public void TestAddNewMOASegment() => CombineAssertions(() =>
	{
		var monetaryFunction = MonetaryAmountTypeQualifierList.AmountTargetCurrency;

		var moaSection1 = new MOASegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewMOASegment(moaSection1, monetaryFunction, []);
		AssertEquals("With no monetaryAmounts", ZString.Empty, moaSection1.ToString(characterSet));

		var moaSection2 = new MOASegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewMOASegment(moaSection2, monetaryFunction, [
			(MonetaryAmountTypeQualifierList.StatisticalValue, "111"),
			(MonetaryAmountTypeQualifierList.OtherCosts, "222"),
			(MonetaryAmountTypeQualifierList.CustomsValue, "333"),
		]);
		AssertEquals("With three monetaryAmounts", "MOA+14+123:111+160:222+40:333'", moaSection2.ToString(characterSet));

		var moaSection3 = new MOASegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewMOASegment(moaSection3, monetaryFunction, [
			(MonetaryAmountTypeQualifierList.InvoiceTotalAmount, "444", "SEK"),
			(MonetaryAmountTypeQualifierList.TransportChargesCustoms, "555"),
		]);
		AssertEquals("With two monetaryAmounts (with/without currency)", "MOA+14+39:444:SEK+144:555'", moaSection3.ToString(characterSet));

		var moaSection4 = new MOASegmentMessageSection(2);
		EDIFACTMessageSegmentBuilder.AddNewMOASegment(moaSection4, monetaryFunction, [
			(MonetaryAmountTypeQualifierList.StatisticalValue, "111"),
			(MonetaryAmountTypeQualifierList.OtherCosts, "222"),
			(MonetaryAmountTypeQualifierList.CustomsValue, "333"),
			(MonetaryAmountTypeQualifierList.AdjustedAmount, "444"),
			(MonetaryAmountTypeQualifierList.TransportChargesCustoms, "555"),
			(MonetaryAmountTypeQualifierList.AgreedCharge, "666"),
		]);
		AssertEquals("With too many monetaryAmounts", "MOA+14+123:111+160:222+40:333+5:444+144:555'MOA+14+7:666'", moaSection4.ToString(characterSet));

		var moaSection5 = new MOASegmentMessageSection(2);
		EDIFACTMessageSegmentBuilder.AddNewMOASegment(moaSection5, monetaryFunction, [
			(MonetaryAmountTypeQualifierList.StatisticalValue, "111"),
			(MonetaryAmountTypeQualifierList.OtherCosts, "0"),
			(MonetaryAmountTypeQualifierList.CustomsValue, "333"),
		]);
		AssertEquals("With three monetaryAmounts where one is zero", "MOA+14+123:111+40:333'", moaSection5.ToString(characterSet));

		var moaSection6 = new MOASegmentMessageSection(2);
		EDIFACTMessageSegmentBuilder.AddNewMOASegment(moaSection6, monetaryFunction, [
			(MonetaryAmountTypeQualifierList.StatisticalValue, "111"),
			(MonetaryAmountTypeQualifierList.OtherCosts, "0", isMandatory: true),
			(MonetaryAmountTypeQualifierList.CustomsValue, "333"),
		]);
		AssertEquals("With three monetaryAmounts where one is zero which is mandatory", "MOA+14+123:111+160:0+40:333'", moaSection6.ToString(characterSet));
	});

	public void TestAddNewTAXSegment() => CombineAssertions(() =>
	{
		const string feeAmount = "27";
		const string feeType = "FA";
		const string feeTypeSequence = "200";
		const string feeBaseValue = "10681";
		const string feeRate = "0,25";
		var responsibleAgency = CodeListResponsibleAgencyCodedList.GetFromString("NO1");

		var taxSection1 = new TAXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewTAXSegment(taxSection1, ZString.Empty, feeType, feeTypeSequence, feeBaseValue, feeRate, responsibleAgency);
		AssertEquals("With no feeAmount", ZString.Empty, taxSection1.ToString(characterSet));

		var taxSection2 = new TAXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewTAXSegment(taxSection2, feeAmount, feeType, feeTypeSequence, feeBaseValue, feeRate, responsibleAgency);
		AssertEquals("With all values", "TAX+1+161:27+FA:107:NO1+200:105:NO1+10681+0,25'", taxSection2.ToString(characterSet));
	});

	public void TestAddNewNADSegment() => CombineAssertions(() =>
	{
		var nadSection1 = new NADSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewNADSegment(nadSection1, PartyQualifierList.Importer, "SOMEONE", CodeListResponsibleAgencyCodedList.GetFromString("NO1"));
		AssertEquals("With account", "NAD+IM+SOMEONE::NO1'", nadSection1.ToString(characterSet));

		var nadSection2 = new NADSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewNADSegment(nadSection2, PartyQualifierList.Importer, ZString.Empty, CodeListResponsibleAgencyCodedList.GetFromString("NO1"));
		AssertEquals("Without account", ZString.Empty, nadSection2.ToString(characterSet));

		var nadSection3 = new NADSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewNADSegment(nadSection3, PartyQualifierList.Importer, ZString.Empty, CodeListResponsibleAgencyCodedList.GetFromString("NO1"), isRequired: true);
		AssertEquals("Without account, while segment is required", "NAD+IM+::NO1'", nadSection3.ToString(characterSet));

		var nadSection4 = new NADSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewNADSegment(nadSection4, PartyQualifierList.Seller, "SOMEONE", "ADDRESS 1", "ADDRESS 2", "ADDRESS 3");
		AssertEquals("With address", "NAD+SE++SOMEONE:ADDRESS 1:ADDRESS 2:ADDRESS 3'", nadSection4.ToString(characterSet));
	});

	public void TestAddNewDMSSegment() => CombineAssertions(() =>
	{
		const string documentMessageNumber = "420";
		const string documentMessageDate = "20230120";

		var dmsSection1 = new DMSSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewDMSSegment(dmsSection1, null, documentMessageDate);
		AssertEquals("When no documentMessageNumber", "DMS++137:20230120:102+380'", dmsSection1.ToString(characterSet));

		var dmsSection2 = new DMSSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewDMSSegment(dmsSection2, documentMessageNumber, null);
		AssertEquals("When no documentMessageDate", "DMS+420++380'", dmsSection2.ToString(characterSet));

		var dmsSection3 = new DMSSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewDMSSegment(dmsSection3, documentMessageNumber, documentMessageDate);
		AssertEquals("When all values", "DMS+420+137:20230120:102+380'", dmsSection3.ToString(characterSet));
	});

	public void TestAddNewCSTSegment() => CombineAssertions(() =>
	{
		const string goodsItemNumber = "1";
		const string tariffNumber = "19059034";
		const string procedureCode = "4000";
		const string preferenceCode = "A";
		const string valuationMethod = "1";
		const string reducedCustomsFlag = "S";
		var responsibleAgency = CodeListResponsibleAgencyCodedList.GetFromString("NO1");

		var dmsSection1 = new CSTSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewCSTSegment(dmsSection1, null, tariffNumber, procedureCode, preferenceCode, valuationMethod, reducedCustomsFlag, responsibleAgency);
		AssertEquals("When no goodsItemNumber", "CST++19059034:122:NO1+4000:117:1+A:116:NO1+1:109:1+S:110:NO1'", dmsSection1.ToString(characterSet));

		var dmsSection2 = new CSTSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewCSTSegment(dmsSection2, goodsItemNumber, null, procedureCode, preferenceCode, valuationMethod, reducedCustomsFlag, responsibleAgency);
		AssertEquals("When no tariffNumber", "CST+1+:122:NO1+4000:117:1+A:116:NO1+1:109:1+S:110:NO1'", dmsSection2.ToString(characterSet));

		var dmsSection3 = new CSTSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewCSTSegment(dmsSection3, goodsItemNumber, tariffNumber, null, preferenceCode, valuationMethod, reducedCustomsFlag, responsibleAgency);
		AssertEquals("When no procedureCode", "CST+1+19059034:122:NO1+:117:1+A:116:NO1+1:109:1+S:110:NO1'", dmsSection3.ToString(characterSet));

		var dmsSection4 = new CSTSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewCSTSegment(dmsSection4, goodsItemNumber, tariffNumber, procedureCode, null, valuationMethod, reducedCustomsFlag, responsibleAgency);
		AssertEquals("When no preferenceCode", "CST+1+19059034:122:NO1+4000:117:1+:116:NO1+1:109:1+S:110:NO1'", dmsSection4.ToString(characterSet));

		var dmsSection5 = new CSTSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewCSTSegment(dmsSection5, goodsItemNumber, tariffNumber, procedureCode, preferenceCode, null, reducedCustomsFlag, responsibleAgency);
		AssertEquals("When no valuationMethod", "CST+1+19059034:122:NO1+4000:117:1+A:116:NO1+:109:1+S:110:NO1'", dmsSection5.ToString(characterSet));

		var dmsSection6 = new CSTSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewCSTSegment(dmsSection6, goodsItemNumber, tariffNumber, procedureCode, preferenceCode, valuationMethod, null, responsibleAgency);
		AssertEquals("When no reducedCustomsFlag", "CST+1+19059034:122:NO1+4000:117:1+A:116:NO1+1:109:1'", dmsSection6.ToString(characterSet));

		var dmsSection7 = new CSTSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewCSTSegment(dmsSection7, goodsItemNumber, tariffNumber, procedureCode, preferenceCode, valuationMethod, reducedCustomsFlag, null);
		AssertEquals("When no responsibleAgency", "CST+1+19059034:122+4000:117:1+A:116+1:109:1+S:110'", dmsSection7.ToString(characterSet));

		var dmsSection8 = new CSTSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewCSTSegment(dmsSection8, goodsItemNumber, tariffNumber, procedureCode, preferenceCode, valuationMethod, reducedCustomsFlag, responsibleAgency);
		AssertEquals("When all values", "CST+1+19059034:122:NO1+4000:117:1+A:116:NO1+1:109:1+S:110:NO1'", dmsSection8.ToString(characterSet));
	});

	public void TestAddNewPACSegment() => CombineAssertions(() =>
	{
		const string numberOfPackages = "1";
		const string packageType = "PK";

		var pacSection1 = new PACSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewPACSegment(pacSection1, null, packageType);
		AssertEquals("With no numberOfPackages", "PAC+++PK'", pacSection1.ToString(characterSet));

		var pacSection2 = new PACSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewPACSegment(pacSection2, numberOfPackages, null);
		AssertEquals("With no packageType", "PAC+1'", pacSection2.ToString(characterSet));

		var pacSection3 = new PACSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewPACSegment(pacSection3, numberOfPackages, packageType);
		AssertEquals("With all values", "PAC+1++PK'", pacSection3.ToString(characterSet));
	});

	public void TestAddNewPCISegment() => CombineAssertions(() =>
	{
		var pciSection1 = new PCISegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewPCISegment(pciSection1, "ADR");
		AssertEquals("With generalGoodsMarks", "PCI++ADR'", pciSection1.ToString(characterSet));

		var pciSection2 = new PCISegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewPCISegment(pciSection2, ReferenceQualifierList.MotorVehicleIdentificationNumber, "ME3DJELT5NV007653");
		AssertEquals("With VT with chassis number", "PCI+++VT:ME3DJELT5NV007653'", pciSection2.ToString(characterSet));

		var pciSection3 = new PCISegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewPCISegment(pciSection3, ReferenceQualifierList.MotorVehicleIdentificationNumber, ZString.Empty);
		AssertEquals("With VT without chassis number", ZString.Empty, pciSection3.ToString(characterSet));
	});

	public void TestAddNewDTMSegment() => CombineAssertions(() =>
	{
		DTMSegmentMessageSection dtmSection = new(9);
		DateTimePeriodFormatQualifierList dateTimeFormat = DateTimePeriodFormatQualifierList.Ccyymmdd;
		DateTimePeriodQualifierList dateTimeType = DateTimePeriodQualifierList.DeliveryDateTimeRequested;
		var beforeCount = dtmSection.Count;
		EDIFACTMessageSegmentBuilder.AddNewDTMSegment(dtmSection, ZString.Empty, dateTimeType, dateTimeFormat);
		AssertEquals("DTM with blank date", expected: beforeCount, dtmSection.Count);

		EDIFACTMessageSegmentBuilder.AddNewDTMSegment(dtmSection, "20241014", dateTimeType, dateTimeFormat);
		AssertEquals("DTM with non-blank date", expected: beforeCount + 1, dtmSection.Count);
	});

	public void TestAddNewGISSegment() => CombineAssertions(() =>
	{
		GISSegmentMessageSection gisSection = new(9);
		CodeListResponsibleAgencyCodedList respAgency = CodeListResponsibleAgencyCodedList.AssignedByBuyerOrBuyersAgent;
		EDIFACTMessageSegmentBuilder.AddNewGISSegment(gisSection, indicatorCode: false, CodeListQualifierList.CustomsIndicator, respAgency);
		AssertEquals("GIS with Qualifier and no Code", ProcessingIndicatorCodedList.GetFromString("0"), gisSection[0].ProcessingIndicator.ProcessingIndicatorCoded);
		EDIFACTMessageSegmentBuilder.AddNewGISSegment(gisSection, indicatorCode: true, CodeListQualifierList.CustomsIndicator, respAgency);
		AssertEquals("GIS with Qualifier and Code", ProcessingIndicatorCodedList.MessageContentAccepted, gisSection[1].ProcessingIndicator.ProcessingIndicatorCoded);

		var beforeCount = gisSection.Count;
		EDIFACTMessageSegmentBuilder.AddNewGISSegment(gisSection, indicatorCode: false, CodeListQualifierList.TelephoneDirectory, respAgency);
		AssertEquals("GIS with no Qualifier and no Code", beforeCount, gisSection.Count);

		EDIFACTMessageSegmentBuilder.AddNewGISSegment(gisSection, indicatorCode: true, CodeListQualifierList.TelephoneDirectory, respAgency);
		AssertEquals("GIS with no Qualifier and Code", beforeCount + 1, gisSection.Count);
		AssertEquals("GIS with no Qualifier and Code - CodeList Value", CodeListResponsibleAgencyCodedList.AssignedByBuyerOrBuyersAgent, gisSection[2].ProcessingIndicator.CodeListResponsibleAgencyCoded);
		AssertEquals("GIS with no Qualifier and Code - Qualifier Value", CodeListQualifierList.TelephoneDirectory, gisSection[2].ProcessingIndicator.CodeListQualifier);
	});

	public void TestAddNewFTXSegment() => CombineAssertions(() =>
	{
		ZString pinkBiscuit = "PINK BISCUIT";
		ZString brownCookie = "BROWN COOKIE";
		ZString longText = "Long text description that we need to split up into several lines when converting to EDIFACT.";

		var ftxSection1 = new FTXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewFTXSegment(ftxSection1, null, [pinkBiscuit]);
		AssertEquals("With no qualifier, for array", expected: 0, ftxSection1.Count);
		var ftxSection2 = new FTXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewFTXSegment(ftxSection2, null, pinkBiscuit, fieldSplitLength: 31);
		AssertEquals("With no qualifier, for split", expected: 0, ftxSection2.Count);

		var ftxSection3 = new FTXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewFTXSegment(ftxSection3, TextSubjectQualifierList.GoodsDescription, [ZString.Empty]);
		AssertEquals("With no freeText, for array", expected: 0, ftxSection3.Count);
		var ftxSection4 = new FTXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewFTXSegment(ftxSection4, TextSubjectQualifierList.GoodsDescription, null);
		AssertEquals("With null freeText, for array", expected: 0, ftxSection4.Count);
		var ftxSection5 = new FTXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewFTXSegment(ftxSection5, TextSubjectQualifierList.GoodsDescription, ZString.Empty, fieldSplitLength: 31);
		AssertEquals("With no freeText, for split", expected: 0, ftxSection5.Count);

		var ftxSection6 = new FTXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewFTXSegment(ftxSection6, TextSubjectQualifierList.GoodsDescription, [pinkBiscuit]);
		AssertEquals("With one freeTexts value", "FTX+AAA+++PINK BISCUIT'", ftxSection6.ToString(characterSet));

		var ftxSection7 = new FTXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewFTXSegment(ftxSection7, TextSubjectQualifierList.GoodsDescription, [pinkBiscuit, brownCookie]);
		AssertEquals("With two freeTexts values", "FTX+AAA+++PINK BISCUIT:BROWN COOKIE'", ftxSection7.ToString(characterSet));

		var ftxSection8 = new FTXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewFTXSegment(ftxSection8, TextSubjectQualifierList.GoodsDescription, longText, fieldSplitLength: 31);
		AssertEquals("With split text", "FTX+AAA+++Long text description that we n:eed to split up into several li:nes when converting to EDIFACT.'", ftxSection8.ToString(characterSet));
	});

	public void TestAddNewRFFSegment() => CombineAssertions(() =>
	{
		RFFSegmentMessageSection rffSection = new(9);
		var beforeCount = rffSection.Count;
		EDIFACTMessageSegmentBuilder.AddNewRFFSegment(null, null, null);
		AssertEquals("RFF with null rffSection", expected: beforeCount, rffSection.Count);
		EDIFACTMessageSegmentBuilder.AddNewRFFSegment(rffSection, null, null);
		AssertEquals("RFF with null reftype", expected: beforeCount, rffSection.Count);
		EDIFACTMessageSegmentBuilder.AddNewRFFSegment(rffSection, ReferenceQualifierList.AccountNumber, null);
		AssertEquals("RFF with null refnumber", expected: beforeCount, rffSection.Count);
		EDIFACTMessageSegmentBuilder.AddNewRFFSegment(rffSection, ReferenceQualifierList.AccountNumber, ZString.Empty);
		AssertEquals("RFF with empty refnumber", expected: beforeCount, rffSection.Count);
		EDIFACTMessageSegmentBuilder.AddNewRFFSegment(rffSection, ReferenceQualifierList.AccountNumber, "12345");
		AssertEquals("RFF with all values", expected: beforeCount + 1, rffSection.Count);
		AssertEquals("RFF values - ReferenceType", ReferenceQualifierList.AccountNumber, rffSection[0].Reference.ReferenceQualifier);
		AssertEquals("RFF values - ReferenceValue", "12345", rffSection[0].Reference.ReferenceNumber);
	});

	public void TestPopulateUNS1Segment() => CombineAssertions(() =>
	{
		UNSSegment unsSegment = new UNSSegment();
		AssertEquals("UNS1 value before populate", expected: null, unsSegment.SectionIdentification);
		EDIFACTMessageSegmentBuilder.PopulateUNS1Segment(unsSegment);
		AssertEquals("UNS1 value after populate", SectionIdentificationList.GetFromString("D"), unsSegment.SectionIdentification);
	});

	public void TestPopulateUNS2Segment() => CombineAssertions(() =>
	{
		UNSSegment unsSegment = new UNSSegment();
		AssertEquals("UNS2 value before populate", expected: null, unsSegment.SectionIdentification);
		EDIFACTMessageSegmentBuilder.PopulateUNS2Segment(unsSegment);
		AssertEquals("UNS2 value after populate", SectionIdentificationList.GetFromString("S"), unsSegment.SectionIdentification);
	});

	public void TestAddNewTAXSegment_GrandTotal() => CombineAssertions(() =>
	{
		const string grandTotalFeeAmount = "1234";

		var taxSection1 = new TAXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewTAXSegment(taxSection1, ZString.Empty);
		AssertEquals("With no feeAmount", ZString.Empty, taxSection1.ToString(characterSet));

		var taxSection2 = new TAXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewTAXSegment(taxSection2, grandTotalFeeAmount);
		AssertEquals("With all values", "TAX+4+161:1234'", taxSection2.ToString(characterSet));
	});

	public void TestAddNewTAXSegment_Total() => CombineAssertions(() =>
	{
		const string totalFeeAmount = "1234";
		const string totalFeeType = "FA";
		var responsibleAgency = CodeListResponsibleAgencyCodedList.GetFromString("NO1");

		var taxSection1 = new TAXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewTAXSegment(taxSection1, ZString.Empty, totalFeeType, responsibleAgency);
		AssertEquals("With no feeAmount", ZString.Empty, taxSection1.ToString(characterSet));

		var taxSection2 = new TAXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewTAXSegment(taxSection2, totalFeeAmount, ZString.Empty, responsibleAgency);
		AssertEquals("With no feeType", ZString.Empty, taxSection2.ToString(characterSet));

		var taxSection3 = new TAXSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewTAXSegment(taxSection3, totalFeeAmount, totalFeeType, responsibleAgency);
		AssertEquals("With all values", "TAX+3+161:1234+FA:107:NO1'", taxSection3.ToString(characterSet));
	});

	public void TestPopulateGDSSegment() => CombineAssertions(() =>
	{
		const string expected = "GDS+2'";
		var gdsSegment = new GDSSegment();
		AssertEquals("GDS value, initially blank", ZString.Empty, gdsSegment.NatureOfCargo.ToString(characterSet));

		var gdsSection = new GDSSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddnewGDSSegment(gdsSection);
		AssertEquals("GDS value after adding segment", expected, gdsSection.ToString(characterSet));
	});

	public void TestAddNewCNTSegment() => CombineAssertions(() =>
	{
		CNTSegmentMessageSection cntSection = new(9);
		var beforeCount = cntSection.Count;
		EDIFACTMessageSegmentBuilder.AddNewCNTSegment(cntSection, null, null);
		AssertEquals("CNT with null value", expected: beforeCount, cntSection.Count);
		EDIFACTMessageSegmentBuilder.AddNewCNTSegment(cntSection, ZString.Empty, ZString.Empty);
		AssertEquals("CNT with empty value", expected: beforeCount, cntSection.Count);
		EDIFACTMessageSegmentBuilder.AddNewCNTSegment(cntSection, "12345", "6789");
		AssertEquals("CNT with all values", expected: beforeCount + 1, cntSection.Count);
		AssertEquals("CNT values - Lines", "12345", cntSection[0].ControlNoLines.ControlValue);
		AssertEquals("CNT values - Packages", "6789", cntSection[0].ControlNoPackages.ControlValue);
	});

	protected override void SetUp()
	{
		base.SetUp();
		characterSet = new ();
	}

	NOCharacterSet characterSet;
}
