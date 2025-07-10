using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be SG", Core.Constants.CountryCodes.Singapore, InvoiceLine.CustomsCountryCode);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.Singapore, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestDutyRateSelectionCriteria()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_SecondaryPreference = "01";
			invoiceLine.JI_ConcessionOrder = "TestOrder";
			var rateSelectionCriteria = invoiceLine.DutyRateSelectionCriteria;
			AssertEquals("EffectiveDate", ZDate.Today, rateSelectionCriteria.EffectiveDate);
			AssertEquals("CountryOfOrigin", "US", rateSelectionCriteria.TradeGroupCountry);
			AssertEquals("DataGrouping", GlbCompany.CurrentCompany.Country.Code, rateSelectionCriteria.DataGrouping);
			AssertEquals("PrimaryPreference", "STD", rateSelectionCriteria.PrimaryPreference);
			AssertEquals("AdditionalCodes count", 1, rateSelectionCriteria.AdditionalCodes.Count);
			Assert("AdditionalCodes", rateSelectionCriteria.AdditionalCodes.Contains(""));
			AssertEquals("ConcessionOrder", "TestOrder", rateSelectionCriteria.ConcessionOrder);
			AssertEquals("RateType", Constants.RateTypes.Duty, rateSelectionCriteria.RateType);
			AssertEquals("RateCode", Constants.RateTypes.Duty, rateSelectionCriteria.RateCode);
		}

		public void TestJI_OutwardMAWB()
		{
			var info = InvoiceLine.JI_OutwardMAWBInfo;
			var expectedMaxLength = SGAddInfoSchema.SG_OutwardMAWB.MaxLength;
			AssertEquals("Should equals the max length of SG_OutwardMAWB.", expectedMaxLength, info.MaxLength);
		}

		public void TestSynchroniseTariffToHarmonisedCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = invoice.InvoiceLines.AddNew();
			invoiceline.JI_Tariff = "12345678";
			Factory.Save();
			AssertEquals("12345678", packLine.JL_HarmonisedCode);
			packLine.JL_HarmonisedCode = "";
			var invoiceline1 = invoice.InvoiceLines.AddNew();
			invoiceline1.JI_Tariff = "87654321";
			Factory.Save();
			AssertEquals("", packLine.JL_HarmonisedCode);
			invoiceline1.JI_Tariff = "12345678";
			Factory.Save();
			AssertEquals("12345678", packLine.JL_HarmonisedCode);
			packLine.JL_HarmonisedCode = "";
			invoiceline1.JI_Tariff = "12345679";
			Factory.Save();
			AssertEquals("123456", packLine.JL_HarmonisedCode);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals("OFT, ONS, OPT, OTH", chargeTypeList1.CodesAsString);
		}

		public void TestPartType()
		{
			AssertEquals(typeof(OrgSupplierPart), InvoiceLine.TypeOfPartUsed);
		}

		public void TestTypeDecider()
		{
			Assert(Factory.New<BaseJobComInvoiceLine>() is JobComInvoiceLine);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			ICusCodeDataTypeSupporter supporter = invoiceLine;
			supporter.AssertType(typeof(CASCCode1), CusCodeDataTypeList.Codes.CASCode1);
			supporter.AssertType(typeof(CASCCode2), CusCodeDataTypeList.Codes.CASCode2);
			supporter.AssertType(typeof(CASCCode3), CusCodeDataTypeList.Codes.CASCode3);
			supporter.AssertType(null, "ZZ!");
			var casCode1 = invoiceLine.CASCCode1s.AddNew();
			casCode1.CY_Data = "1";
			var casCode2 = invoiceLine.CASCCode2s.AddNew();
			casCode2.CY_Data = "1";
			var casCode3 = invoiceLine.CASCCode3s.AddNew();
			casCode3.CY_Data = "1";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(casCode1.PK);
			AssertEquals(typeof(CASCCode1), codeData.GetType());
			codeData = newFactory.Load<CusCodeData>(casCode2.PK);
			AssertEquals(typeof(CASCCode2), codeData.GetType());
			codeData = newFactory.Load<CusCodeData>(casCode3.PK);
			AssertEquals(typeof(CASCCode3), codeData.GetType());
		}

		public void TestSG_TotalDutiableWGTVOLQTY_Updated()
		{
			AssertEquals(0m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.JI_Tariff = "22089060";
			AssertEquals(0m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.SG_OuterPackQuantity = 2;
			InvoiceLine.SG_InPackQuantity = 3;
			InvoiceLine.SG_InnerPackQuantity = 4;
			InvoiceLine.SG_InmostPackQuantity = 5;
			InvoiceLine.SG_UnitDutiableWGTVOLQTY = 0.22;
			AssertEquals(26.4m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.SG_TobaccoMultiplier = 3;
			AssertEquals(360m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.SG_InmostPackQuantityUnit = UnitOfQuantityCodeList.Codes.STK;
			InvoiceLine.JI_Tariff = "24021000";
			AssertEquals(0.0264m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.JI_Tariff = "24022090";
			InvoiceLine.SG_UnitDutiableWGTVOLQTY = 1.33;
			AssertEquals(240m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.JI_Tariff = "24021000";
			InvoiceLine.SG_InmostPackQuantityUnit = UnitOfQuantityCodeList.Codes.KGM;
			AssertEquals(159.6m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
		}

		public void TestSG_TotalDutiableWGTVOLQTY_IsNotCalculatedOnDataImport()
		{
			InvoiceLine.SG_TotalDutiableWGTVOLQTY = 37.5m;
			InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit = "STK";
			((ISupportDataImporting)InvoiceLine).IsImportingData = true;
			AssertEquals("Pre-condition: All processing below should not change the values above when this is done via Data Import", 37.5m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.JI_Tariff = "22089060";
			AssertEquals("Field is unchanged", 37.5m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.SG_OuterPackQuantity = 2;
			InvoiceLine.SG_InPackQuantity = 3;
			InvoiceLine.SG_InnerPackQuantity = 4;
			InvoiceLine.SG_InmostPackQuantity = 5;
			InvoiceLine.SG_UnitDutiableWGTVOLQTY = 0.22;
			AssertEquals("Field is unchanged", 37.5m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.SG_TobaccoMultiplier = 3;
			AssertEquals("Field is unchanged", 37.5m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.SG_InmostPackQuantityUnit = UnitOfQuantityCodeList.Codes.STK;
			InvoiceLine.JI_Tariff = "24021000";
			AssertEquals("Field is unchanged", 37.5m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.JI_Tariff = "24022090";
			InvoiceLine.SG_UnitDutiableWGTVOLQTY = 1.33;
			AssertEquals("Field is unchanged", 37.5m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.JI_Tariff = "24021000";
			InvoiceLine.SG_InmostPackQuantityUnit = UnitOfQuantityCodeList.Codes.KGM;
			AssertEquals("Field is unchanged", 37.5m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			AssertEquals("Field is unchanged", "STK", InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit);
		}

		[TestDate(2012, 1, 1)]
		public void TestPerGramOrPartThereOfTariffs()
		{
			InvoiceLine.JI_Tariff = "24022020";
			InvoiceLine.SG_OuterPackQuantity = 2;
			InvoiceLine.SG_InPackQuantity = 3;
			InvoiceLine.SG_InnerPackQuantity = 4;
			InvoiceLine.SG_InmostPackQuantity = 5;
			InvoiceLine.SG_UnitDutiableWGTVOLQTY = 0.22;
			AssertEquals(120m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.JI_Tariff = "24029020";
			InvoiceLine.SG_UnitDutiableWGTVOLQTY = 1.22;
			AssertEquals(240m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
			InvoiceLine.JI_Tariff = "24022090";
			InvoiceLine.SG_UnitDutiableWGTVOLQTY = 1.99;
			AssertEquals(240m, InvoiceLine.SG_TotalDutiableWGTVOLQTY);
		}

		public void TestTotalQuantity()
		{
			InvoiceLine.SG_OuterPackQuantity = 0;
			AssertEquals("0", InvoiceLine.TotalQuantity);
			InvoiceLine.SG_OuterPackQuantity = 2;
			InvoiceLine.SG_InPackQuantity = 3;
			InvoiceLine.SG_InnerPackQuantity = 4;
			InvoiceLine.SG_InmostPackQuantity = 5;
			AssertEquals("120", InvoiceLine.TotalQuantity);
		}

		public void TestSG_UnitDutiableWGTVOLQTY()
		{
			InvoiceLine.JI_Tariff = "24022090";
			InvoiceLine.SG_UnitDutiableWGTVOLQTY = 1.3m;
			AssertEquals(2, InvoiceLine.SG_TobaccoMultiplier);
			InvoiceLine.JI_Tariff = "";
			AssertEquals(0, InvoiceLine.SG_TobaccoMultiplier);
			InvoiceLine.JI_Tariff = "24029020";
			AssertEquals(2, InvoiceLine.SG_TobaccoMultiplier);
		}

		public void TestSG_TotalDutiableWGTVOLQTYUnit_Updated()
		{
			InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.LTR;
			AssertEquals(UnitOfQuantityCodeList.Codes.LTR, InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit);
			AssertEquals(InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit, InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit);
		}

		[TestDate(2013, 02, 24)]
		public void TestTariffRate()
		{
			SetupExciseUnitRate("24011010", 347m, new ZDateTime(2012, 2, 17), new ZDateTime(2013, 2, 25));
			InvoiceLine.JI_Tariff = "24011010";
			AssertEquals("Excise rate for this tariff prior to 25-Feb-2013", 347m, InvoiceLine.SG_ExciseUnitRate);
		}

		[TestDate(2018, 01, 01)]
		public void TestNewTariffRate()
		{
			SetupExciseUnitRate("24011010", 352, new ZDateTime(2013, 2, 25), new ZDateTime(2018, 2, 19));
			InvoiceLine.JI_Tariff = "24011010";
			AssertEquals("Excise rate for this tariff prior to 19-Feb-2018", 352m, InvoiceLine.SG_ExciseUnitRate);
		}

		[TestDate(2018, 02, 20)]
		public void TestTariffRateChange()
		{
			SetupExciseUnitRate("24011010", 388m, new ZDateTime(2018, 2, 19), new ZDateTime(2018, 6, 23));
			InvoiceLine.JI_Tariff = "24011010";
			AssertEquals("Excise rate for this tariff post the 19-Feb-2018 change", 388m, InvoiceLine.SG_ExciseUnitRate);
		}

		[TestDate(2018, 02, 26)]
		public void TestTariffRateChange2()
		{
			SetupExciseUnitRate("24039190", 427m, new ZDateTime(2018, 2, 19), new ZDateTime(2018, 6, 23));
			InvoiceLine.JI_Tariff = "24039190";
			AssertEquals("Excise rate for this tariff has changed to $427 per kg", 427m, InvoiceLine.SG_ExciseUnitRate);
		}

		void SetupExciseUnitRate(string tariffCode, decimal unitRate, ZDateTime startDateTime, ZDateTime endDateTime)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, tariffCode, startDateTime, endDateTime);
			helper.CreateTariffExciseRate(tariff, unitRate, UnitOfQuantityCodeList.Codes.KGM);
			Factory.Save();
		}

		public void TestJI_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "87032341", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffExciseRate(tariff, 20m);
			Factory.Save();
			InvoiceLine.JI_Tariff = "87032341";
			AssertEquals(20m, InvoiceLine.SG_ExcisePercentageRate);
			InvoiceLine.JI_Tariff = "69141000";
			AssertEquals(ZString.Empty, InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit);
			AssertEquals(ZString.Empty, InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit);
			AssertEquals(0m, InvoiceLine.SG_ExciseUnitRate);
			AssertEquals(0m, InvoiceLine.SG_ExcisePercentageRate);
			AssertEquals(0m, InvoiceLine.SG_DutyUnitRate);
			AssertEquals(0m, InvoiceLine.SG_DutyPercentageRate);
		}

		[TestDate(2014, 05, 01)]
		public void TestJI_Tariff2()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore, "Singapore");
			Factory.Save();
			var stdPreference = helper.CreatePreferenceForCountryAndGrouping(PreferentialIndicatorCodeList.Codes.STD, "STANDARD", Core.Constants.CountryCodes.Singapore, Core.Constants.CountryCodes.Singapore);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			var dtyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Singapore, Constants.RateTypes.Duty, "Duty");
			helper.LoadOrCreateNewCusRateCode(Factory, Constants.RateTypes.Duty, dtyRateType.PK);
			Factory.Save();
			helper.CreateTariffExciseRate(tariff5, 88.0m, SGConstants.LPA);
			Factory.Save();
			InvoiceLine.JI_Tariff = "21069061";
			AssertEquals(UnitOfQuantityCodeList.Codes.LTR, InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit);
			AssertEquals(UnitOfQuantityCodeList.Codes.LTR, InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit);
			AssertEquals(88m, InvoiceLine.SG_ExciseUnitRate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "22030010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateDutyRate(tariff2, stdPreference, 16.0m, SGConstants.LPA);
			helper.CreateTariffExciseRate(tariff2, 60.0m, SGConstants.LPA);
			Factory.Save();
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Singapore;
			InvoiceLine.JI_Tariff = "22030010";
			AssertEquals(UnitOfQuantityCodeList.Codes.LTR, InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit);
			AssertEquals(UnitOfQuantityCodeList.Codes.LTR, InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit);
			AssertEquals(16m, InvoiceLine.SG_DutyUnitRate);
			AssertEquals(0m, InvoiceLine.SG_DutyPercentageRate);
			InvoiceLine.SG_DutyUnitRate = 1m;
			InvoiceLine.SG_DutyPercentageRate = 2m;
			InvoiceLine.SG_ExciseUnitRate = 3m;
			InvoiceLine.SG_ExcisePercentageRate = 4m;
			InvoiceLine.JI_Tariff = "22030010"; //no change in tariff
			AssertEquals(1m, InvoiceLine.SG_DutyUnitRate);
			AssertEquals(2m, InvoiceLine.SG_DutyPercentageRate);
			AssertEquals(3m, InvoiceLine.SG_ExciseUnitRate);
			AssertEquals(4m, InvoiceLine.SG_ExcisePercentageRate);
			InvoiceLine.JI_Tariff = "";
			AssertEquals(0m, InvoiceLine.SG_DutyUnitRate);
			AssertEquals(0m, InvoiceLine.SG_DutyPercentageRate);
			AssertEquals(0m, InvoiceLine.SG_ExciseUnitRate);
			AssertEquals(0m, InvoiceLine.SG_ExcisePercentageRate);
			AssertEquals("", InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit);
			AssertEquals("", InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit);
			InvoiceLine.JI_Tariff = "22030010";
			AssertEquals(UnitOfQuantityCodeList.Codes.LTR, InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit);
			AssertEquals(UnitOfQuantityCodeList.Codes.LTR, InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit);
			InvoiceLine.JI_Tariff = "69141000";
			AssertEquals("", InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit);
			AssertEquals("", InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit);
			AssertEquals(0m, InvoiceLine.SG_ExciseUnitRate);
			AssertEquals(0m, InvoiceLine.SG_ExcisePercentageRate);
			AssertEquals(0m, InvoiceLine.SG_DutyUnitRate);
			AssertEquals(0m, InvoiceLine.SG_DutyPercentageRate);
		}

		public void TestTariffCommodityType()
		{
			InvoiceLine.JI_Tariff = "69141000";
			AssertEquals("Commodity Type s/b blank", "", InvoiceLine.SG_TariffCommodityType);
			InvoiceLine.JI_Tariff = "22083000";
			AssertEquals("S/b Alcohol Commodity Type", CommodityTypeList.Codes.Alcohol, InvoiceLine.SG_TariffCommodityType);
			InvoiceLine.JI_Tariff = "24031920";
			AssertEquals("S/b Tobacco Commodity Type", CommodityTypeList.Codes.Tobacco, InvoiceLine.SG_TariffCommodityType);
			InvoiceLine.JI_Tariff = "27101212";
			AssertEquals("S/b Petroleum Commodity Type", CommodityTypeList.Codes.Petroleum, InvoiceLine.SG_TariffCommodityType);
			InvoiceLine.JI_Tariff = "87032351";
			AssertEquals("S/b Motor Vehicle Commodity Type", CommodityTypeList.Codes.Vehicle, InvoiceLine.SG_TariffCommodityType);
		}

		public void TestTariff()
		{
			InvoiceLine.JI_Tariff = "22030011";
			AssertEquals("22030011", InvoiceLine.UniversalTariff.ZZ1_TariffCode);
		}

		public void TestIsStrategicDefaulting()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals(false, InvoiceLine.SG_IsStrategicInfo.HasWarnings());
			InvoiceLine.SG_IsStrategic = true;
			AssertEquals(true, InvoiceLine.SG_IsStrategicInfo.HasWarnings());
			InvoiceLine.SG_IsStrategic = false;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals(false, InvoiceLine.SG_IsStrategicInfo.HasWarnings());
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			InvoiceLine.SG_IsStrategic = true;
			AssertEquals(true, InvoiceLine.SG_IsStrategicInfo.HasMessageError("Strategic goods indicator is only valid for OUT & TNP declarations"));
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals(true, InvoiceLine.SG_IsStrategicInfo.HasMessageError("Strategic goods indicator is only valid for OUT & TNP declarations"));
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals(true, InvoiceLine.SG_IsStrategicInfo.HasMessageError("Strategic goods indicator is only valid for OUT & TNP declarations"));
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			InvoiceLine.SG_IsStrategic = false;
			InvoiceLine.SG_IsStrategic = true;
			AssertEquals(false, InvoiceLine.SG_IsStrategicInfo.HasMessageError("Strategic goods indicator is only valid for OUT & TNP declarations"));
		}

		public void TestCustomsUQDefaulting()
		{
			InvoiceLine.JI_Tariff = "21069061";
			InvoiceLine.JI_InvoiceUQ = UnitOfQuantityCodeList.Codes.KGM;
			AssertEquals("LTR", InvoiceLine.JI_CustomsUnitQty);
			InvoiceLine.JI_Tariff = "39269041";
			InvoiceLine.JI_InvoiceUQ = UnitOfQuantityCodeList.Codes.NMB;
			AssertEquals(string.Empty, InvoiceLine.JI_CustomsUnitQty);
			InvoiceLine.JI_Tariff = "21069061";
			AssertEquals("LTR", InvoiceLine.JI_CustomsUnitQty);
		}

		[TestDate(2011, 11, 01)]
		public void TestJI_CustomsUnitQtyInfoNotReadOnly()
		{
			AssertEquals(false, InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
		}

		public void TestESNDPIndicatorDefault()
		{
			InvoiceLine.JI_Tariff = "12119011";
			AssertEquals("ESNDPIndicator should be Empty", "", InvoiceLine.SG_ESNDPIndicator);
			InvoiceLine.JI_Tariff = "24021000";
			AssertEquals("ESNDPIndicator should be HW", MarkingCodeList.Codes.HW, InvoiceLine.SG_ESNDPIndicator);
		}

		public void TestCustomsValue()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
				AssertEquals(0m, InvoiceLine.JI_CustomsValue);
				InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
				InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				InvoiceLine.JI_LinePrice = 1000m;
				AssertEquals(1000m, InvoiceLine.JI_CustomsValue);
				InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.SouthAfrica);
				Declaration.ResumeApportionment();
				AssertEquals(1055.87m, InvoiceLine.JI_CustomsValue);
				InvoiceLine.SG_LastSellingPrice = 900m;
				AssertEquals(900m, InvoiceLine.JI_CustomsValue);
				InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
				InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Eritrea;
				InvoiceLine.SG_LastSellingPrice = 0m;
				InvoiceLine.JI_LinePrice = 1000m;
				AssertEquals(1097.53m, InvoiceLine.JI_CustomsValue);
			}
		}

		public void TestJI_Calc_ExciseAmount()
		{
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceLine.JI_LinePrice = 1000m;
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 500m;
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();
			CusEntryLine cusEntryLine = (CusEntryLine)Declaration.ActiveEntryHeaders[0].MergedLines[0];
			cusEntryLine.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Excise, 150m);
			AssertEquals(100m, InvoiceLine.JI_Calc_ExciseAmount);
			AssertEquals(50m, invoiceLine1.JI_Calc_ExciseAmount);
		}

		public void TestJI_Calc_OtherCharges()
		{
			InvoiceLine.ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.SouthAfrica);
			AssertEquals(55.87m, InvoiceLine.JI_Calc_OtherAmount);
		}

		public void TestJI_Calc_OtherChargesByPercentage()
		{
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var othCharge = InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges);
			othCharge.J7_Percentage = 2m;
			InvoiceLine.JI_LinePrice = 1000m;
			InvoiceLine.ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 1000m, Core.Constants.CurrencyCodes.Singapore);
			InvoiceLine.ApportionedCharges[0].J7_Percentage = 2m;
			Declaration.ResumeApportionment();
			AssertEquals(20m, InvoiceLine.JI_Calc_OtherAmount);
		}

		public void TestJI_Calc_OFTChargesByPercentage()
		{
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var oftCharge = InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight);
			oftCharge.J7_Percentage = 2m;
			InvoiceLine.JI_LinePrice = 1000m;
			InvoiceLine.ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight);
			InvoiceLine.ApportionedCharges[0].J7_Percentage = 2m;
			Declaration.ResumeApportionment();
			AssertEquals(20m, InvoiceLine.JI_Calc_FreightInInvoiceCurr);
		}

		public void TestJI_Calc_OtherChargesInMultipleCurrencies()
		{
			var refCurrencyRateUSD = Factory.New<RefExchangeRate>();
			refCurrencyRateUSD.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			refCurrencyRateUSD.RE_StartDate = ZDateTime.Today.AddDays(-1);
			refCurrencyRateUSD.RE_ExpiryDate = ZDateTime.Today.AddDays(6);
			refCurrencyRateUSD.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			refCurrencyRateUSD.RE_SellRate = 0.801925m;
			var refCurrencyRateZAR = Factory.New<RefExchangeRate>();
			refCurrencyRateZAR.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			refCurrencyRateZAR.RE_StartDate = ZDateTime.Today.AddDays(-1);
			refCurrencyRateZAR.RE_ExpiryDate = ZDateTime.Today.AddDays(6);
			refCurrencyRateZAR.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			refCurrencyRateZAR.RE_SellRate = 6.043757m;
			var refCurrencyRateAUD = Factory.New<RefExchangeRate>();
			refCurrencyRateAUD.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Australia;
			refCurrencyRateAUD.RE_StartDate = ZDateTime.Today.AddDays(-1);
			refCurrencyRateAUD.RE_ExpiryDate = ZDateTime.Today.AddDays(6);
			refCurrencyRateAUD.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			refCurrencyRateAUD.RE_SellRate = 0.744602m;
			var refCurrencyRateHKD = Factory.New<RefExchangeRate>();
			refCurrencyRateHKD.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.HongKong;
			refCurrencyRateHKD.RE_StartDate = ZDateTime.Today.AddDays(-1);
			refCurrencyRateHKD.RE_ExpiryDate = ZDateTime.Today.AddDays(6);
			refCurrencyRateHKD.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			refCurrencyRateHKD.RE_SellRate = 6.222001m;
			var refCurrencyRateNZD = Factory.New<RefExchangeRate>();
			refCurrencyRateNZD.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.NewZealand;
			refCurrencyRateNZD.RE_StartDate = ZDateTime.Today.AddDays(-1);
			refCurrencyRateNZD.RE_ExpiryDate = ZDateTime.Today.AddDays(6);
			refCurrencyRateNZD.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			refCurrencyRateNZD.RE_SellRate = 0.961538m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			InvoiceLine.ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.SouthAfrica);
			//753.66
			InvoiceLine.ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 200m, Core.Constants.CurrencyCodes.Australia);
			//185.70
			InvoiceLine.ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 300m, Core.Constants.CurrencyCodes.HongKong);
			//2327.65
			InvoiceLine.ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 400m, Core.Constants.CurrencyCodes.NewZealand);
			//479.62
			AssertEquals("All other charges should be converted into invoice currency (USD) & totalled.", 3746.62m, InvoiceLine.JI_Calc_OtherAmount);
		}

		public void TestCASCCode1s()
		{
			AssertNotNull(InvoiceLine.CASCCode1s);
			CASCCode cASCCode = InvoiceLine.CASCCode1s.AddNew();
			AssertEquals(CusCodeDataTypeList.Codes.CASCode1, cASCCode.CY_Type);
			AssertEquals(InvoiceLine.PK, cASCCode.CY_ParentID);
		}

		public void TestCASCCode2s()
		{
			AssertNotNull(InvoiceLine.CASCCode2s);
			CASCCode cASCCode = InvoiceLine.CASCCode2s.AddNew();
			AssertEquals(CusCodeDataTypeList.Codes.CASCode2, cASCCode.CY_Type);
			AssertEquals(InvoiceLine.PK, cASCCode.CY_ParentID);
		}

		public void TestCASCCode3s()
		{
			AssertNotNull(InvoiceLine.CASCCode3s);
			CASCCode cASCCode = InvoiceLine.CASCCode3s.AddNew();
			AssertEquals(CusCodeDataTypeList.Codes.CASCode3, cASCCode.CY_Type);
			AssertEquals(InvoiceLine.PK, cASCCode.CY_ParentID);
		}

		public void TestProductCodes()
		{
			var productCodes = InvoiceLine.ProductCodes;
			AssertNotNull(productCodes);
			AssertSame(productCodes, ((IAdditionalLineTariffDetailParent)InvoiceLine).CusLineTariffDetails);
		}

		public void TestMarksAndNumbers()
		{
			Assert(!InvoiceLine.HasChanges);
			var query = new ZQuery(StmNoteSchema.ST_ParentID, InvoiceLine.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, InvoiceLine.TableName);
			query.FetchOnlyFromLocalCache = true;
			AssertEquals(0, Factory.Load<StmNote>(query).Length);
			Assert(InvoiceLine.MarksAndNumbers.IsEmpty);
			InvoiceLine.MarksAndNumbers = "TEST";
			Assert(InvoiceLine.HasChanges);
			AssertEquals("TEST", InvoiceLine.MarksAndNumbers);
			AssertEquals(1, Factory.Load<StmNote>(query).Length);
		}

		public void TestMarksAndNumbersMaxLength()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals("Marks & Nos maxlength for NON OUT with CO & COO decs", 512, InvoiceLine.MarksAndNumbersInfo.MaxLength);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals("Marks & Nos maxlength for OUT", 512, InvoiceLine.MarksAndNumbersInfo.MaxLength);
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NH;
			AssertEquals("Marks & Nos maxlength for OUT with CO", 170, InvoiceLine.MarksAndNumbersInfo.MaxLength);
			Declaration.SG_ApplicationProductType = "";
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals("Marks & Nos maxlength for COO decs", 170, InvoiceLine.MarksAndNumbersInfo.MaxLength);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals("Marks & Nos maxlength for NON OUT with CO & COO decs", 512, InvoiceLine.MarksAndNumbersInfo.MaxLength);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals("Marks & Nos maxlength for NON OUT with CO & COO decs", 512, InvoiceLine.MarksAndNumbersInfo.MaxLength);
		}

		public void TestGoodsDescriptionMaxLength()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			AssertEquals("Goods Description maxlength", 175, InvoiceLine.JI_DescriptionInfo.MaxLength);
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			AssertEquals("TradeNet 4.1 Goods Description maxlength has increased to 512 chars", 512, InvoiceLine.JI_DescriptionInfo.MaxLength);
		}

		public void TestCertItemDescription()
		{
			Assert(!InvoiceLine.HasChanges);
			var query = new ZQuery(StmNoteSchema.ST_ParentID, InvoiceLine.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, InvoiceLine.TableName);
			query.FetchOnlyFromLocalCache = true;
			AssertEquals(0, Factory.Load<StmNote>(query).Length);
			Assert(InvoiceLine.CertItemDescription.IsEmpty);
			InvoiceLine.CertItemDescription = "TEST";
			Assert(InvoiceLine.HasChanges);
			AssertEquals("TEST", InvoiceLine.CertItemDescription);
			AssertEquals(1, Factory.Load<StmNote>(query).Length);
			AssertEquals(1750, InvoiceLine.CertItemDescriptionInfo.MaxLength);
		}

		public void TestCertItemDescriptionMaxLength()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals("Cert Item Description maxlength for OUT", 1750, InvoiceLine.CertItemDescriptionInfo.MaxLength);
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NH;
			AssertEquals("Cert Item Description maxlength for OUT with CO", 1750, InvoiceLine.CertItemDescriptionInfo.MaxLength);
			Declaration.SG_ApplicationProductType = "";
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals("Cert Item Description maxlength for COO decs", 1750, InvoiceLine.CertItemDescriptionInfo.MaxLength);
		}

		public void TestOriginCriterion()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			InvoiceLine.SG_CertOriginCriterion1 = OriginCriterionCodeList.Codes.AANZFTAFormAANZ_CTH;
			AssertEquals("CTH", InvoiceLine.SG_CertOriginCriterion1);
			AssertEquals("", InvoiceLine.SG_CertOriginCriterion2);
			AssertEquals("", InvoiceLine.SG_CertOriginCriterion3);
			InvoiceLine.SG_CertOriginCriterion1 = OriginCriterionCodeList.Codes.ACFTAFormE_ACC;
			AssertEquals("ACFTA", InvoiceLine.SG_CertOriginCriterion1);
			AssertEquals("CUMULATIVE", InvoiceLine.SG_CertOriginCriterion2);
			AssertEquals("CONTENT", InvoiceLine.SG_CertOriginCriterion3);
			InvoiceLine.SG_CertOriginCriterion1 = "ASEAN";
			AssertEquals("Overwriting defaulted/selected value of OC1", "ASEAN", InvoiceLine.SG_CertOriginCriterion1);
			AssertEquals("", InvoiceLine.SG_CertOriginCriterion2);
			AssertEquals("", InvoiceLine.SG_CertOriginCriterion3);
			InvoiceLine.SG_CertOriginCriterion1 = OriginCriterionCodeList.Codes.ACFTAFormE_ACC;
			AssertEquals("ACFTA", InvoiceLine.SG_CertOriginCriterion1);
			AssertEquals("CUMULATIVE", InvoiceLine.SG_CertOriginCriterion2);
			AssertEquals("CONTENT", InvoiceLine.SG_CertOriginCriterion3);
			InvoiceLine.SG_CertOriginCriterion1 = OriginCriterionCodeList.Codes.AJCEPFormAJ_CTHDMI;
			AssertEquals("Selecting another value from the drop down list should override all three OC values", "CTH", InvoiceLine.SG_CertOriginCriterion1);
			AssertEquals("DMI", InvoiceLine.SG_CertOriginCriterion2);
			AssertEquals("", InvoiceLine.SG_CertOriginCriterion3);
		}

		public void TestValidation()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Assert(InvoiceLine.Validation is JobComInvoiceLineValidation_IPT);
			Declaration.JE_MessageType = "";
			Assert(InvoiceLine.Validation is JobComInvoiceLineValidation);
		}

		public void TestClone_()
		{
			InvoiceLine.MarksAndNumbers = "TEST";
			InvoiceLine.CertItemDescription = "TEST";
			InvoiceLine.SG_InwardHAWB = "TEST";
			InvoiceLine.SG_InwardMAWB = "TEST";
			InvoiceLine.SG_OutwardHAWB = "TEST";
			InvoiceLine.JI_OutwardMAWB = "TEST";
			InvoiceLine.SG_RefundForItemCustomsDutyAmount = 10m;
			InvoiceLine.SG_RefundForItemExciseAmount = 20m;
			InvoiceLine.SG_RefundForItemGSTAmount = 30m;
			CusLineTariffDetail productCode = InvoiceLine.ProductCodes.AddNew();
			productCode.BZ_Tariff = "TEST";
			productCode.BZ_UQ1 = UnitOfQuantityCodeList.Codes.NMB;
			productCode.BZ_Value = 100;
			CASCCode cascCode = InvoiceLine.CASCCode1s.AddNew();
			cascCode.CY_Code = "TE1";
			cascCode = InvoiceLine.CASCCode2s.AddNew();
			cascCode.CY_Code = "TE2";
			cascCode = InvoiceLine.CASCCode3s.AddNew();
			cascCode.CY_Code = "TE3";
			JobComInvoiceLine newInvoiceLine = (JobComInvoiceLine)InvoiceLine.Clone();
			AssertEquals("", newInvoiceLine.SG_InwardHAWB);
			AssertEquals("", newInvoiceLine.SG_InwardMAWB);
			AssertEquals("", newInvoiceLine.SG_OutwardHAWB);
			AssertEquals("", newInvoiceLine.JI_OutwardMAWB);
			AssertEquals(0m, newInvoiceLine.SG_RefundForItemCustomsDutyAmount);
			AssertEquals(0m, newInvoiceLine.SG_RefundForItemExciseAmount);
			AssertEquals(0m, newInvoiceLine.SG_RefundForItemGSTAmount);
			AssertEquals("TEST", newInvoiceLine.MarksAndNumbers);
			AssertEquals("TEST", newInvoiceLine.CertItemDescription);
			AssertEquals("TEST", newInvoiceLine.ProductCodes[0].BZ_Tariff);
			AssertEquals("NMB", newInvoiceLine.ProductCodes[0].BZ_UQ1);
			AssertEquals(100m, newInvoiceLine.ProductCodes[0].BZ_Value);
			AssertEquals("TE1", newInvoiceLine.CASCCode1s[0].CY_Code);
			AssertEquals("TE2", newInvoiceLine.CASCCode2s[0].CY_Code);
			AssertEquals("TE3", newInvoiceLine.CASCCode3s[0].CY_Code);
		}

		public void TestPartDefaulting()
		{
			OrgHeader supplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader owner = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			Declaration.JE_OH_Supplier = supplier.PK;
			Declaration.JE_OH_Importer = owner.PK;
			InvoiceHeader.JZ_JE = Declaration.PK;
			JobComInvoiceLine invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			Classification beer = Factory.New<Classification>();
			beer.CC_PercAlcohol = 4.7m;
			beer.CC_TariffNum = "22030090";
			ProductCode commodityCode = beer.ProductCodes.AddNew();
			commodityCode.CY_Data = "ZBP0AAA00CS";
			OrgSupplierPart millers = Factory.New<OrgSupplierPart>();
			millers.OP_PartNum = "Millers";
			millers.OP_Desc = "Miller Light";
			millers.OP_Brand = "MLB";
			millers.OP_Model = "ML-9428";
			OrgPartRelation sup = Factory.New<OrgPartRelation>();
			sup.OU_OH = supplier.PK;
			sup.OU_Relationship = "SUP";
			sup.OU_OP = millers.PK;
			OrgPartRelation own = Factory.New<OrgPartRelation>();
			own.OU_OH = owner.PK;
			own.OU_Relationship = "OWN";
			own.OU_OP = millers.PK;
			millers.RelatedOrganisations.Add(sup);
			millers.RelatedOrganisations.Add(own);
			var pivot = millers.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTB;
			pivot.CI_CC = beer.PK;
			invoiceLine.JI_PartNo = "Millers";
			CombineAssertions(() =>
			{
				AssertEquals("Tariff should default from Part/Classification", "22030090", invoiceLine.JI_Tariff);
				AssertEquals("Line Description default from Part/Classification", "MILLER LIGHT", invoiceLine.JI_Description);
				AssertEquals("Model should default from Part", "ML-9428", invoiceLine.JI_Model);
				AssertEquals("Brand should default from Part", "MLB", invoiceLine.JI_BrandName);
				AssertEquals("Alcohol percentage should default from Part/Classification", 4.7m, invoiceLine.SG_PercAlcohol);
				AssertEquals("Commodity codes should default from Part/Classification", 1, invoiceLine.ProductCodes.Count);
				AssertEquals("Commodity codes should default from Part/Classification", "ZBP0AAA00CS", invoiceLine.ProductCodes[0].BZ_Tariff);
			}

			);
			invoiceLine.JI_PartNo = "";
			invoiceLine.JI_Tariff = "";
			invoiceLine.SG_PercAlcohol = 0.0m;
			invoiceLine.ProductCodes.RemoveAndDeleteAll();
			pivot.CI_CC = ZGuid.Empty;
			pivot.CI_TariffNum = "22030123";
			pivot.CI_PercAlcohol = 4.7m;
			var pivotCommodityCode = pivot.ProductCodes.AddNew();
			pivotCommodityCode.CY_Data = "ZBP0AAA00ZZ";
			invoiceLine.JI_PartNo = "Millers";
			CombineAssertions(() =>
			{
				AssertEquals("Tariff should default from Part/Pivot", "22030123", invoiceLine.JI_Tariff);
				AssertEquals("Line Description default from Part/Pivot", "MILLER LIGHT", invoiceLine.JI_Description);
				AssertEquals("Brand should default from Part", "MLB", invoiceLine.JI_BrandName);
				AssertEquals("Model should default from Part", "ML-9428", invoiceLine.JI_Model);
				AssertEquals("Alcohol percentage should default from Part/Pivot", 4.7m, invoiceLine.SG_PercAlcohol);
				AssertEquals("Commodity codes should default from Part/Pivot", 1, invoiceLine.ProductCodes.Count);
				AssertEquals("Commodity codes should default from Part/Pivot", "ZBP0AAA00ZZ", invoiceLine.ProductCodes[0].BZ_Tariff);
			}

			);
		}

		public void TestClassificationDefaulting()
		{
			Classification beer = Factory.New<Classification>();
			beer.CC_PercAlcohol = 4.7m;
			beer.CC_TariffNum = "22030090";
			ProductCode commodityCode = beer.ProductCodes.AddNew();
			commodityCode.CY_Data = "ZBP0AAA00CS";
			InvoiceLine.JI_CC = beer.PK;
			AssertEquals("Tariff should default from Classification", "22030090", InvoiceLine.JI_Tariff);
			AssertEquals("Alcohol percentage should default from Classification", 4.7m, InvoiceLine.SG_PercAlcohol);
			AssertEquals("Commodity codes should default from Classification", 1, InvoiceLine.ProductCodes.Count);
		}

		[ExpectNoExceptions()]
		public void TestProductCodesRemovedAndDeletedWhenNewClassificationEntered()
		{
			InvoiceLine.JI_Tariff = "22089060";
			var productCode = InvoiceLine.ProductCodes.AddNew();
			productCode.BZ_Tariff = "TEST";
			productCode.BZ_UQ1 = UnitOfQuantityCodeList.Codes.NMB;
			productCode.BZ_Value = 100;
			var beer = Factory.New<Classification>();
			beer.CC_LookupCode = "Fancy Beer";
			beer.CC_PercAlcohol = 4.7m;
			beer.CC_TariffNum = "22030090";
			var commodityCode = beer.ProductCodes.AddNew();
			commodityCode.CY_Data = "ZBP0AAA00CS";
			InvoiceLine.JI_CC = beer.PK;
			AssertEquals("Commodity codes should default from Classification & previous codes be RemovedAndDeleted", 1, InvoiceLine.ProductCodes.Count);
			AssertEquals("Product code should default from Classificaton", "ZBP0AAA00CS", InvoiceLine.ProductCodes[0].BZ_Tariff);
			Factory.Save();
		}

		public void TestJI_CCWhenCopying()
		{
			Classification beer = Factory.New<Classification>();
			beer.CC_PercAlcohol = 4.7m;
			beer.CC_TariffNum = "22030090";
			ProductCode commodityCode = beer.ProductCodes.AddNew();
			commodityCode.CY_Data = "ZBP0AAA00CS";
			InvoiceLine.JI_CC = beer.PK;
			InvoiceLine.SG_PercAlcohol = 8.5m;
			InvoiceLine.ProductCodes[0].BZ_Tariff = "ZBP0ACF01CS";
			InvoiceLine.ProductCodes[0].BZ_UQ1 = "LTR";
			InvoiceLine.ProductCodes[0].BZ_Value = 5000;
			JobComInvoiceLine copiedInvoiceLine = (JobComInvoiceLine)InvoiceLine.Clone();
			AssertEquals("Tariff should default from Inv Line", "22030090", copiedInvoiceLine.JI_Tariff);
			AssertEquals("Alcohol percentage should default from Copied Inv Line not classification", 8.5m, InvoiceLine.SG_PercAlcohol);
			AssertEquals("Commodity codes should default from Copied Inv Line not classification", 1, InvoiceLine.ProductCodes.Count);
			AssertEquals("Product code should be from copied line not class", "ZBP0ACF01CS", InvoiceLine.ProductCodes[0].BZ_Tariff);
			AssertEquals("Product code uq from inv line", "LTR", InvoiceLine.ProductCodes[0].BZ_UQ1);
			AssertEquals("Product code value from inv line", 5000m, InvoiceLine.ProductCodes[0].BZ_Value);
		}

		public void TestJI_Description()
		{
			InvoiceLine.JI_Description = "Invoice line goods";
			AssertEquals("Invoice line goods", InvoiceLine.JI_Description);
			AssertEquals("Pre-Condition", "", InvoiceLine.CertItemDescription);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			InvoiceLine.JI_Description = "gun powder for fireworks";
			AssertEquals("gun powder for fireworks", InvoiceLine.CertItemDescription);
			InvoiceLine.JI_Description = "perfumes and deoderants";
			AssertEquals("Certificate Item Description should not be overridden if it already has a value entered.", "gun powder for fireworks", InvoiceLine.CertItemDescription);
			InvoiceLine.CertItemDescription = ZString.Empty;
			InvoiceLine.JI_Description = "perfume";
			AssertEquals("perfume", InvoiceLine.CertItemDescription);
			InvoiceLine.JI_Description = "toys";
			InvoiceLine.CertItemDescription = "";
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKO;
			AssertEquals("", InvoiceLine.CertItemDescription);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			InvoiceLine.JI_Description = "computers";
			AssertEquals("", InvoiceLine.CertItemDescription);
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			InvoiceLine.JI_Description = "computer games";
			AssertEquals("computer games", InvoiceLine.CertItemDescription);
		}

		public void TestIsStrategic()
		{
			InvoiceLine.SG_CategoryCode = "CATCODE";
			InvoiceLine.SG_EndUseDescription = "ENDUSEDESC";
			InvoiceLine.SG_IsStrategic = true;
			AssertEquals("CATCODE", InvoiceLine.SG_CategoryCode);
			AssertEquals(CA_SC1CodeList.Codes.NMU, InvoiceLine.SG_EndUseCode1);
			AssertEquals(CA_SC2CodeList.Codes.NGU, InvoiceLine.SG_EndUseCode2);
			AssertEquals(CA_SC3CodeList.Codes.NMD, InvoiceLine.SG_EndUseCode3);
			AssertEquals("ENDUSEDESC", InvoiceLine.SG_EndUseDescription);
			InvoiceLine.SG_IsStrategic = false;
			AssertEquals("", InvoiceLine.SG_CategoryCode);
			AssertEquals("", InvoiceLine.SG_EndUseCode1);
			AssertEquals("", InvoiceLine.SG_EndUseCode2);
			AssertEquals("", InvoiceLine.SG_EndUseCode3);
			AssertEquals("", InvoiceLine.SG_EndUseDescription);
		}

		public void TestPrimaryPreferenceMaxLength()
		{
			AssertEquals(3, InvoiceLine.JI_PrimaryPreferenceInfo.MaxLength);
		}

		public void TestBrandNameMaxLength()
		{
			AssertEquals(35, InvoiceLine.JI_BrandNameInfo.MaxLength);
		}

		public void TestModelMaxLength()
		{
			AssertEquals(35, InvoiceLine.JI_ModelInfo.MaxLength);
		}

		public void TestPreferenceRateApplies()
		{
			InvoiceLine.JI_PrimaryPreference = "";
			Assert("Preference Rate doesn't apply when JI_PrimaryPreference is empty.", !InvoiceLine.PreferenceRateApplies);
			InvoiceLine.JI_PrimaryPreference = "STD";
			Assert("Preference Rate doesn't apply when JI_PrimaryPreference is STD.", !InvoiceLine.PreferenceRateApplies);
			InvoiceLine.JI_PrimaryPreference = "ABC";
			Assert("Preference Rate doesn't apply when JI_PrimaryPreference is a random value.", !InvoiceLine.PreferenceRateApplies);
			InvoiceLine.JI_PrimaryPreference = "PRF";
			Assert("Preference Rate applies when JI_PrimaryPreference is PRF.", InvoiceLine.PreferenceRateApplies);
			InvoiceLine.JI_PrimaryPreference = "PRI";
			Assert("Preference Rate applies when JI_PrimaryPreference is PRI.", InvoiceLine.PreferenceRateApplies);
		}

		public void TestDelete()
		{
			InvoiceLine.MarksAndNumbers = "HERE ARE SOME LOVELY MARKS AND NUMBERS IN A NOTE";
			InvoiceLine.CertItemDescription = "JUST A CERTIFICATE DESCRIPTION NOTHING TO SEE HERE";
			Factory.Save();
			var query = new ZQuery(StmNoteSchema.ST_ParentID, InvoiceLine.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, InvoiceLine.TableName);
			query.AddToFilter(StmNoteSchema.ST_NoteType, nameof(CargoWise.Definitions.StmNoteVisibility.DOC));
			AssertEquals(2, Factory.GetDatabaseCount(typeof(StmNote), query));
			InvoiceLine.Delete();
			Factory.Save();
			AssertEquals(0, Factory.GetDatabaseCount(typeof(StmNote), query));
		}

		#region Override Base Tests
		public override void TestMakeCustomsQuantityReadOnly()
		{
			Assert(true);
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			Assert(true);
		}

		public override void TestCustomsQtyCalculatedWhenInvoiceQtySet()
		{
			InvoiceLine.JI_Tariff = "06012020";
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_CustomsQuantity);
			InvoiceLine.JI_InvoiceUQ = "DOZ";
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_CustomsQuantity);
			InvoiceLine.JI_InvoiceQuantity = 18m;
			AssertEquals(18m, InvoiceLine.JI_CustomsQuantity);
		}

		public override void TestCustomsQtyCalculatedByNetWeightOfProductWhenInvoiceQtySet()
		{
			Assert(true);
		}

		public override void TestWipeNKTaxType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			Assert(!invoiceLine.ShouldWipeNKTaxType);
		}

		#endregion
		#region Implementation
		protected override ZString DeclarationImportMessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.IPT;
			}
		}

		protected override ZString DeclarationExportMessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.OUT;
			}
		}

		protected new JobDeclaration Declaration
		{
			get
			{
				base.Declaration.JE_MergeBy = "TRF";
				return (JobDeclaration)base.Declaration;
			}
		}

		protected new JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return (JobComInvoiceHeader)base.InvoiceHeader;
			}
		}

		protected new JobComInvoiceLine InvoiceLine
		{
			get
			{
				return (JobComInvoiceLine)base.InvoiceLine;
			}
		}

		protected override bool RatesAreReciprocal
		{
			get
			{
				return true;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var refCurrecny = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.SouthAfrica);
			refCurrecny.SetCustomsRate(new ZDateTime(2000, 1, 1), ZDateTime.MaxSmallDateTimeValue, 0.558659m);
			refCurrecny = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Eritrea);
			refCurrecny.SetCustomsRate(new ZDateTime(2000, 1, 1), ZDateTime.MaxSmallDateTimeValue, 1.041667m);
			helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			helper.LoadOrCreateNewTariff(tariffType, "22089060");
			var tariff0 = helper.LoadOrCreateNewTariff(tariffType, "24021000");
			helper.LoadOrCreateNewTariff(tariffType, "24022090");
			helper.LoadOrCreateNewTariff(tariffType, "24022020");
			helper.LoadOrCreateNewTariff(tariffType, "24029020");
			var tariff1 = helper.LoadOrCreateNewTariff(tariffType, "22083000");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Alcohol, tariff1);
			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "24031920");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Tobacco, tariff2);
			var tariff3 = helper.LoadOrCreateNewTariff(tariffType, "27101212");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Petroleum, tariff3);
			var tariff4 = helper.LoadOrCreateNewTariff(tariffType, "87032351");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Vehicle, tariff4);
			helper.LoadOrCreateNewTariff(tariffType, "22030011");
			tariff5 = helper.LoadOrCreateNewTariff(tariffType, "21069061");
			helper.CreateTariffUOM(tariff5, Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.LTR);
			helper.LoadOrCreateNewTariff(tariffType, "64069100");
			var tariff6 = helper.LoadOrCreateNewTariff(tariffType, "48173000");
			helper.CreateTariffUOM(tariff6, Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.NMB);
			helper.LoadOrCreateNewTariff(tariffType, "39269041");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Tobacco, tariff0);
			var tariff8 = helper.LoadOrCreateNewTariff(tariffType, "06012020");
			helper.CreateTariffUOM(tariff8, Constants.UnitOfMeasureTypes.StatisticalUOMType, "DOZ");
			Factory.Save();
		}

		UniversalReferenceTestDataHelper helper;
		TariffView tariff5;
		#endregion

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);
	}
}
