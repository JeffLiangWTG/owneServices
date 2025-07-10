using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class EntrySummaryChapter98_99WithSupplementaryTariffNumberTest : EntrySummaryMessageBuilderAbstractTest
	{
		public void TestIRTaxWith98()
		{
			invoiceLine.JI_Tariff = "2208905000";
			invoiceLine.JI_LinePrice = 28143.72m;
			invoiceLine.US_SupTariff = "9802005060";
			invoiceLine.JI_CustomsQuantity = 1821.60m;

			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var builder = new EntrySummaryMessageBuilder(invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, true);
			var message = builder.PopulateMessage();

			var messageBlock = (ENS60)message.MessageBlock.MessageBlocks.Find(x => x is ENS60);
			AssertNotNull("60 with Excise Tax is constructed", messageBlock);
			Assert("60 with Excise Tax is constructed", messageBlock.InternalRevenueServiceIRSTax > 0);
		}

		[TestDate(2009, 12, 12)]
		public void TestCottonFeeWith9819()
		{
			OrgHeader manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer.OH_FullName = "MR MANUFACTURER";
			manufacturer.OH_IsConsignor = true;
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ZAHEDCLO7DUR");
			manufacturer.OH_Code = "ZAHEDCLO7DU2";
			invoiceLine.InvoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			invoiceLine.JI_Tariff = "6204624021";
			invoiceLine.US_SupTariff = "98191124";
			invoiceLine.US_UC_NKCountryOfExport = "ZA";
			invoiceLine.US_UC_NKCountryOfOrigin = "ZA";
			invoiceLine.US_VisaNo = "8ZA121221";
			invoiceLine.JI_CustomsQuantity = 200m;
			invoiceLine.JI_CustomsSecondQuantity = 2500m;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLine.JI_LinePrice = 10000m;

			MergeAndSend("9819 CottonFee2");

			AssertEquals(10000m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals(0m, invoiceLine.CusEntryLine.ParentLine.CL_CustomsValue);
			MQEDIMessage message = (MQEDIMessage)invoiceLine.CusEntryLine.Header.Messages[0];
			ENS62 ens62 = (ENS62)message.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "62");

			AssertNotNull(ens62);
			AssertEquals("6204624021", ens62.TariffNumber);
		}

		[TestDate(2016, 03, 27)]
		public void Test9801007000()
		{
			invoiceLine.US_SupTariff = "9801007000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SPI = "AU";
			invoiceLine.JI_Tariff = "8802200015";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";

			MergeAndSend("9801007000");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test9801007000WithSPI()
		{
			invoiceLine.US_SupTariff = "9801007000";
			invoiceLine.US_SPI = "AU";
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Tariff = "8802200015";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";

			MergeAndSend("9801007000 with SPI");
			AssertEquals("SHould be duty free", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test98130005UnderTIB()
		{
			CreateNewOrGetExistingCusCodeListForFIRMSType("Z104");
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.US_US_NKLocationOfGoods = "Z104";

			invoiceLine.US_SupTariff = "9813000520";
			invoiceLine.JI_Tariff = "9801.00.70 00";
			invoiceLine.US_SPI = "AU";
			MergeAndSend("9801007000 with 98130005", false, true);
			AssertEquals("Should be duty free", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test9801008000()
		{
			invoiceLine.US_SupTariff = "9801008000";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.US_SPI = "AU";
			invoiceLine.JI_Tariff = "8701.20.00 15";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			DOT dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._01;
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Equipment;
			dot.US_DOTPassport = "PASS";

			MergeAndSend("9801008000", true, false);
			AssertEquals("Should be duty free", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);

			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			MQEDIMessage message = (MQEDIMessage)entry.Messages[0];
			List<MessageBlock> blocks = message.MessageBlock.MessageBlocks.FindAll(x => x.GetType() == typeof(AENSOI));
			AssertEquals("DOT should be declared under ENS 70", 1, blocks.Count);

			int indexOf70 = message.MessageBlock.MessageBlocks.FindIndex(x => x.GetType() == typeof(ENS70));
			int indexOfOI = message.MessageBlock.MessageBlocks.FindIndex(indexOf70, x => x.GetType() == typeof(AENSOI));

			Assert(indexOfOI > indexOf70);
		}

		[TestDate(2016, 03, 27)]
		public void Test9801008000WithSPI()
		{
			invoiceLine.US_SupTariff = "9801008000";
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "8701.20.00 15";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SPI = "AU";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			DOT dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._01;
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Equipment;
			dot.US_DOTPassport = "PASS";

			MergeAndSend("9801008000 with SPI");
			AssertEquals("SHould be duty free", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test9801008000WithSPIWithFDA()
		{
			invoiceLine.US_SupTariff = "9801008000";//FD3
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Tariff = "8512300030";// radar detector, no FDA requirement
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SPI = "AU";

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			declaration.JE_MasterBill = "FDA9801008000";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);

			invoiceLine.FDAs[0].US_FDAForcePN = true;

			MergeAndSend("9801008000 with FDA", true, false);
			AssertEquals("SHould be duty free", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);

			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			MQEDIMessage message = (MQEDIMessage)entry.Messages[0];
			List<MessageBlock> blocks = message.MessageBlock.MessageBlocks.FindAll(x => x.GetType() == typeof(AENSOI));
			AssertEquals("FDA should be declared after ENS 50", 1, blocks.Count);

			int indexOf50 = message.MessageBlock.MessageBlocks.FindIndex(x => x.GetType() == typeof(ENS50));
			int indexOfOI = message.MessageBlock.MessageBlocks.FindIndex(x => x.GetType() == typeof(AENSOI));

			Assert("FDA should be declared after ENS 50", indexOfOI > indexOf50);
		}

		[TestDate(2016, 03, 27)]
		public void Test9801ExportedAndReturned()
		{
			invoiceLine.JI_Tariff = "9801001010";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "US";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			MergeAndSend("9801");
		}

		[TestDate(2008, 1, 3)]
		public void Test9802ExportedAndReturnedAdvancedOrImproved()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_SupTariff = "9802004020";
			invoiceLine.US_98GoodsValue = 10000m;
			invoiceLine.JI_Tariff = "8407344800";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 200m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_WeightUQ = "KG";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9802");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 1, 3)]
		public void Test9803SubstantialContainersOrHolders()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "98030050";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9803");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 1, 3)]
		public void Test9804PersonalExemptionsResidents()
		{
			invoiceLine.JI_Tariff = "98040005";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9804");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 1, 3)]
		public void Test9805PersonalExemptionsEvacuees()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "98050050";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9805");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 1, 3)]
		public void Test9806PersonalExemptionsDistinguishedVisitors()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "98060005";//free
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9806");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 1, 3)]
		public void Test9807OtherPersonalExemptions()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "98070040";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9807");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 1, 3)]
		public void Test9808GovernmentImportations()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "9808001000";//free
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9808");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 1, 3)]
		public void Test9809ForeignGovernmentImportations()
		{
			invoiceLine.JI_Tariff = "98090010";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9809");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 1, 3)]
		public void Test9810ReligiousImports()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "9810.00.60 00";//free
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9810");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 1, 3)]
		public void Test9811SamplesForSolicitingOrders()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "98110020";//free
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			declaration.JE_MasterBill = "OBL9811";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9811");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 1, 3)]
		public void Test9813TemporaryExhibition()
		{
			CreateNewOrGetExistingCusCodeListForFIRMSType("L888");
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_US_NKLocationOfGoods = "L888";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_SupTariff = "98130040";
			invoiceLine.JI_Tariff = "8606.99.01 30";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_WeightUQ = "KG";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			declaration.JE_DateOfArrival = new ZDateTime(2008, 1, 3);
			MergeAndSend("9813");
		}

		[TestDate(2008, 1, 3)]
		public void Test9814TeaFreeUnderBond()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "9814005000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			declaration.JE_MasterBill = "OBL9814";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9814");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 1, 3)]
		public void Test9815AmericanFisheries()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "9815002000";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			declaration.JE_MasterBill = "OBL9815";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9815", true, false);
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			MQEDIMessage message = (MQEDIMessage)entry.Messages[0];
			List<MessageBlock> blocks = message.MessageBlock.MessageBlocks.FindAll(x => x.GetType() == typeof(AENSOI));
			AssertEquals("FDA should be declared under ENS 40", 1, blocks.Count);
		}

		[TestDate(2008, 1, 3)]
		public void Test9816LimitedValue()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "98160020";//valued in the aggregate at not over $1,000 fair retail value in the country of acquisition
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9816");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 1, 3)]
		public void Test9817SpecialProvisions()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;
			invoiceLine.JI_Tariff = "9817009800";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9817");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2007, 3, 19)]
		public void Test9819AGOATextile()
		{
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.US_UC_NKCountryOfExport = "CV";
			invoiceHeader.US_UC_NKCountryOfOrigin = "CV";
			invoiceLine.US_SupTariff = "98191115";
			invoiceLine.US_VisaNo = "6CV456789";
			invoiceLine.JI_Tariff = "6110121010";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.US_TextileCategoryNo = "445";
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "CVBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			MergeAndSend("9819");
		}

		[TestDate(2008, 1, 3)]
		public void Test9820CaribbeanBasin()
		{
			invoiceHeader.US_UC_NKCountryOfExport = "TT";
			invoiceHeader.US_UC_NKCountryOfOrigin = "TT";

			invoiceLine.US_SupTariff = "98201103";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_TextileCategoryNo = "";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_Tariff = "6102.20.00 10";//15.9%
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;//B00170052 in CMR was rejected without 62 with 056. 98201103 is no longer CFE.

			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "TTBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			MergeAndSend("9820");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals("No Cotton", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		[TestDate(2008, 1, 3)]
		public void Test9821ATPDEA()
		{
			invoiceHeader.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Ecuador;
			invoiceHeader.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Ecuador;

			invoiceLine.US_SupTariff = "9821.11.01";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_Tariff = "6102.20.00 10";//15.9%
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "ECBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			MergeAndSend("9821");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 1, 3)]
		public void Test9822TemporaryAdmission()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;

			invoiceHeader.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Chile;
			invoiceHeader.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Chile;

			invoiceLine.US_SupTariff = "9822.01.05";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.US_SPI = "CL";
			invoiceLine.JI_Tariff = "1701.11.50 00";//33.87c/kg
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 10000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsSecondQuantity = 200m;

			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9822");
			AssertEquals("No MPF", 0m, invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2007, 3, 19)]
		public void Test9901AdditionalDuties()
		{
			invoiceLine.US_SupTariff = "9901.00.52"; // 5.99 cents/litre extra
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SupQty1 = 1000m;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Tariff = "2909.19.1800"; // 5.5%
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Weight = 1000m;
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);
			MergeAndSend("9901");
			// Duty should be $609.90 (.0599 * 1000 + (5.5% * 10000) = $550 + 59.90

			AssertEquals(0m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals(550m, invoiceLine.CusEntryLine.DutyAmount);
			AssertEquals(10000m, invoiceLine.CusEntryLine.ParentLine.CL_CustomsValue);
			AssertEquals(59.90m, invoiceLine.CusEntryLine.ParentLine.DutyAmount);
			AssertEquals("Total Duty payable", 609.90m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2007, 3, 19)]
		public void Test9902ReductionInRatesOfDuty()
		{
			invoiceLine.US_SupTariff = "9902.01.21";    // 6%
			invoiceLine.JI_Tariff = "2933.19.2300";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 20000m;  // 6.5%
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 10m;
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);
			MergeAndSend("9902");
			AssertEquals(1200m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2007, 3, 19)]
		public void Test9903InLieu()
		{
			invoiceLine.US_SupTariff = "9903.02.21";    // 100%
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "0201100510";   // normally 4.4c/kg
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_LinePrice = 20000m;
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);
			MergeAndSend("9903");
			AssertEquals("Duty Amount", 20000.00m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2007, 3, 19)]
		public void Test9904AdditionalDutiesSafeguardMeasures()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			invoiceLine.US_SupTariff = "99040237";      // 8.8%
			invoiceLine.US_UC_NKCountryOfOrigin = "ES";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "0201308010";   // 26.4%
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Weight = 1000m;
			declaration.JE_MasterBill = "OBL9904";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);
			MergeAndSend("9904");
			AssertEquals("Total Duty payable", 3520m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		// SubChapter 5 deleted 

		[TestDate(2007, 3, 19)]
		public void Test9906InLieuMexico()
		{
			invoiceLine.US_SupTariff = "9906.07.19";    // 1.6%
			invoiceLine.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine.US_UC_NKCountryOfExport = "MX";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "0704904020";   // normally 20%
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.US_SPI = "MX";

			declaration.JE_MasterBill = "OBL9906";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			declaration.JE_TotalWeight = 10m;
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);
			MergeAndSend("9906");

			AssertEquals(320.00m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		// SubChapter 7 deleted 

		[TestDate(2007, 3, 19)]
		public void Test9908InLieuIsrael()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKPortOfLoading = "ILRAA";
			invoiceLine.US_SupTariff = "9908.04.01";    // Free
			invoiceLine.US_UC_NKCountryOfOrigin = "IL";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "0401307500";   // $1.646 / kg
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 10m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_SPI = "IL";

			declaration.JE_MasterBill = "OBL9908";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);
			MergeAndSend("9908");
			AssertEquals("Total Duty Amount", 0m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2007, 3, 19)]
		public void Test9909InLieuJordan()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKPortOfLoading = "JOAQJ";
			invoiceLine.US_SupTariff = "99090410";  // 49.3c / kg
			invoiceLine.JI_CustomsSecondQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "JO";
			invoiceLine.US_SPI = "JO";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "0401307500";   // $1.646 / kg
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 1000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_LinePrice = 10000m;
			declaration.JE_MasterBill = "OBL9909";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);
			MergeAndSend("9909");
			AssertEquals("Total Duty Amount", 493.00m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2007, 3, 19)]
		public void Test9910InLieuSingapore()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKPortOfLoading = "SGSIN";
			declaration.US_SchDLoading = "55900";
			declaration.JE_TotalWeight = 10m;
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);

			invoiceLine.US_SupTariff = "9910.02.10";    // 15.8%
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "0201.20.8010"; // 26.4%
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.US_SPI = "SG";
			declaration.JE_MasterBill = "OBL9910";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);

			MergeAndSend("9910");
			AssertEquals(3160m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2007, 3, 19)]
		public void Test9911InLieuChile()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKPortOfLoading = "CLCUR";
			invoiceLine.US_SupTariff = "9911.02.30";    // 13.2 c / kg
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SupQty1 = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CL";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			invoiceLine.US_SPI = "CL";
			invoiceLine.JI_Tariff = "0207.13.0000"; // 17.6 c / kg
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_LinePrice = 10000m;
			declaration.JE_TotalWeight = 1000m;
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);
			MergeAndSend("9911");
			AssertEquals(132m, declaration.CustomsEntryHeaders[0].MergedLines[0].DutyAmount);
			AssertEquals(0m, declaration.CustomsEntryHeaders[0].MergedLines[1].DutyAmount);
		}

		[TestDate(2007, 3, 19)]
		public void Test9912InLieuMorocco()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKPortOfLoading = "MAAGA";
			invoiceLine.US_SupTariff = "9912.02.10";    // 22.8%
			invoiceLine.US_UC_NKCountryOfOrigin = "MA";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "0201.10.5010"; // 26.4%
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 15000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.US_SPI = "MA";
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);
			declaration.JE_TotalWeight = 10m;

			MergeAndSend("9912");
			AssertEquals(4560m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2007, 3, 19)]
		public void Test9913InLieuAustralia()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			invoiceLine.US_SupTariff = "9913.02.05";    // Free
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "0201.10.5010"; // 26.4%
			invoiceLine.US_MiscPermitNo = "1AU456789";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_SPI = "AU";
			invoiceLine.JI_CustomsQuantity = 15000.00m;//needs for Beef Fee Calculation
			invoiceLine.JI_Weight = 1000m;
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);
			MergeAndSend("9913");
			AssertEquals("Duty Amount", 0m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2007, 3, 19)]
		public void Test9914InLieuBahrain()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKPortOfLoading = "BHBAH";
			invoiceLine.US_SupTariff = "9914.04.02"; // 61.7c/litre
			invoiceLine.JI_Weight = 100m;
			invoiceLine.US_UC_NKCountryOfOrigin = "BH";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.US_SPI = "BH";
			invoiceLine.JI_Tariff = "0401302500"; // 77.2c/litre
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Weight = 500m;
			invoiceLine.JI_LinePrice = 10000m;
			declaration.JE_MasterBill = "OBL9914";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);
			MergeAndSend("9914");
			AssertEquals("Total Duty Amount", 617.00m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2008, 1, 2)]
		public void Test9915InLieuCAFTA()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKPortOfLoading = "HNGJA";

			invoiceLine.US_SupTariff = "9915.04.06"; // $1.646/kg
			invoiceLine.US_UC_NKCountryOfOrigin = "HN";
			invoiceLine.JI_Weight = 10m;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			invoiceLine.JI_Tariff = "0401.30.7500";  // $1.646/kg
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsSecondQuantity = 1000m;//kg
			invoiceLine.JI_CustomsQuantity = 70m;
			invoiceLine.JI_Weight = 50m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_SPI = "P+";
			declaration.JE_MasterBill = "OBL9915";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 2);

			MergeAndSend("9915");
			AssertEquals(1646m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2008, 12, 31)]
		public void Test99010050FromKR()
		{
			invoiceLine.US_UC_NKCountryOfExport = "KR";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";

			invoiceLine.US_SupTariff = "99010050";
			invoiceLine.US_SupQty1 = 5000m;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			invoiceLine.JI_Tariff = "2207106000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 5000m;
			invoiceLine.JI_Weight = 1500m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			MergeAndSend("99010050 From KR");
			AssertEquals("Total Duty Amount for normal additional duty calculation", 963.5m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2008, 12, 31)]
		public void Test99010050WithSPI_A()
		{
			invoiceLine.US_UC_NKCountryOfExport = "PE";
			invoiceLine.US_UC_NKCountryOfOrigin = "PE";
			invoiceLine.CountryOfOrigin_US.UC_SPIEndDate = ZDateTime.Empty;

			invoiceLine.US_SupTariff = "99010050";
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "2207106000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 5000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.US_SPI = "J";
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;

			MergeAndSend("99010050 With primary SPI J");
			AssertEquals("Total Duty Amount for J (becomes duty free)", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2008, 12, 31)]
		public void Test99010050WithSPI_E()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			invoiceLine.US_UC_NKCountryOfExport = "DM";
			invoiceLine.US_UC_NKCountryOfOrigin = "DM";

			invoiceLine.US_SupTariff = "99010050";
			invoiceLine.JI_Weight = 10m;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			invoiceLine.JI_Tariff = "2207106000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 5000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.US_SPI = "E";
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;

			MergeAndSend("99010050 With SPI E");
			AssertEquals("Total Duty Amount for E (becomes duty free)", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2009, 12, 12)]
		public void Test99020101FromKR()
		{
			invoiceLine.US_UC_NKCountryOfExport = "KR";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";

			invoiceLine.US_SupTariff = "99020101";
			invoiceLine.JI_Tariff = "2929102000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 5000m;
			invoiceLine.JI_Weight = 1000m;

			MergeAndSend("99020101 From KR");
			AssertEquals("Additional duty calculation from KR for 99020101", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2009, 12, 12)]
		public void Test99020101FromAU()
		{
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			invoiceLine.US_SupTariff = "99020101";
			invoiceLine.US_SPI = "AU";
			invoiceLine.JI_Tariff = "2929102000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 5000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_LinePrice = 10000m;

			MergeAndSend("99020101 From AU");
			AssertEquals("Duty Amount for 99020101 from AU", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test99030221FromAustria()
		{
			invoiceLine.US_UC_NKCountryOfExport = "AT";
			invoiceLine.US_UC_NKCountryOfOrigin = "AT";

			invoiceLine.US_SupTariff = "99030221";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			invoiceLine.JI_Tariff = "0201100590";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 5000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_LinePrice = 20000m;

			MergeAndSend("99030221 From AT");
			AssertEquals("Duty Amount for 99030221 from AT", 20000m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test99034105FromJapan()
		{
			invoiceLine.US_UC_NKCountryOfExport = "JP";
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";

			invoiceLine.US_SupTariff = "99034105";
			invoiceLine.JI_Tariff = "4113103000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 5000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_LinePrice = 10000m;

			MergeAndSend("99034105 From JP");
			AssertEquals("Total Duty Amount for additional duty for a JP product", 4240m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2008, 05, 31)]
		public void Test99060719InLieuMexico()
		{
			invoiceLine.US_SupTariff = "9906.07.19";    // free
			invoiceLine.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine.US_UC_NKCountryOfExport = "MX";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "0704904020";   // normally 20%
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.US_SPI = "MX";

			declaration.JE_MasterBill = "MX99060719";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);

			MergeAndSend("9906.07.19 in-lieu MX"); // will fail with error "14K" if not send during the right period
			AssertEquals("Duty amount for MX FTA", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2008, 12, 31)]
		public void Test99080405ForIsraelFTA()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKPortOfLoading = "ILAKL";
			declaration.JE_RL_NKPortOfLoading = "ILAKL";
			invoiceLine.US_SupTariff = "9908.04.05";    // free
			invoiceLine.US_UC_NKCountryOfOrigin = "IL";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			invoiceLine.JI_Tariff = "0406102800";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_SPI = "IL";
			declaration.JE_MasterBill = "IL99080405";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);

			MergeAndSend("9908.04.05 in-lieu IL");
			AssertEquals("Duty amount for IL FTA", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2008, 12, 31)]
		public void Test99090410ForJordanFTA()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKOrigin = "";
			declaration.JE_RL_NKPortOfLoading = "";
			declaration.US_SchDLoading = "51100";
			invoiceLine.US_SupTariff = "9909.04.10";    // 32.9c/kg
			invoiceLine.US_UC_NKCountryOfOrigin = "JO";
			invoiceLine.US_UC_NKCountryOfExport = "JO";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "0401307500";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 500m;
			invoiceLine.JI_CustomsSecondQuantity = 10m;// kg
			invoiceLine.JI_Weight = 10m;
			declaration.JE_MasterBill = "JO99090410";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			invoiceLine.US_SPI = "JO";

			MergeAndSend("9909.04.10 in-lieu JO");
			AssertEquals("Duty amount for JO FTA", 3.29m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2008, 12, 31)]
		public void Test99100402ForSGFTA()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKOrigin = "SGSIN";
			declaration.JE_RL_NKPortOfLoading = "SGSIN";
			declaration.US_SchDLoading = "55900";
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			invoiceLine.US_SupTariff = "9910.04.02"; //38.6 c per kg
			invoiceLine.JI_CustomsQuantity = 70.00000m;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			invoiceLine.JI_Tariff = "0401302500";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_SPI = "SG";
			invoiceLine.JI_CustomsQuantity = 70m;
			invoiceLine.JI_Weight = 1000m;
			declaration.JE_MasterBill = "SG99100402";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);

			MergeAndSend("9910.04.02 in-lieu SG");
			AssertEquals("Duty amount for SG FTA", 27.02m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test99119651ForCLFTA()
		{
			declaration.JE_RL_NKOrigin = "CLSCL";
			declaration.JE_RL_NKPortOfLoading = "CLSCL";
			invoiceLine.US_SupTariff = "99119651";
			invoiceLine.US_UC_NKCountryOfOrigin = "CL";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.US_SPI = "CL";

			invoiceLine.JI_Tariff = "2003900010";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 30000m;
			invoiceLine.JI_Weight = 1000m;
			declaration.JE_MasterBill = "CL99119651";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);

			MergeAndSend("99119651 in-lieu CL");
			AssertEquals("Duty amount for CL FTA", 2650.00m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2008, 12, 31)]
		public void Test99120403ForMAFTA()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKOrigin = "MATNG";
			declaration.JE_RL_NKPortOfLoading = "MATNG";
			invoiceLine.US_SupTariff = "99120403";
			invoiceLine.US_UC_NKCountryOfOrigin = "MA";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "2105002000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_CustomsQuantity = 70m;
			invoiceLine.JI_CustomsSecondQuantity = 3890m;
			invoiceLine.JI_Weight = 1000m;
			declaration.JE_MasterBill = "MA99120403";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			invoiceLine.US_SPI = "MA";

			MergeAndSend("99120403 in-lieu MA");
			AssertEquals("Duty amount for MA FTA", 2748.07m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2008, 12, 31)]
		public void Test99131220ForAUFTA()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			invoiceLine.US_SupTariff = "9913.12.10";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			invoiceLine.JI_Tariff = "1202.10.80 20";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_CustomsQuantity = 10000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.US_SPI = "AU";
			declaration.JE_MasterBill = "AU99131220";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);

			MergeAndSend("9913.12.20 in-lieu AU");
			AssertEquals("Duty amount for AU FTA", 25420.00m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2008, 12, 31)]
		public void Test99140423ForBHFTA()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKOrigin = "BHADA";
			declaration.JE_RL_NKPortOfLoading = "BHADA";
			invoiceLine.US_SupTariff = "99140423";
			invoiceLine.US_UC_NKCountryOfOrigin = "BH";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "0403904500";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 70.00000m;
			invoiceLine.JI_Weight = 1000m;
			declaration.JE_MasterBill = "BH99140423";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			invoiceLine.US_SPI = "BH";

			MergeAndSend("99140423 in-lieu BH");
			AssertEquals("Duty amount for BH FTA", 42.91m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2019, 12, 31)]
		public void Test99150490ForPPlus()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.US_SchDLoading = "21199";

			invoiceLine.US_SupTariff = "9915.04.90";
			invoiceLine.US_UC_NKCountryOfExport = "SV";
			invoiceLine.US_UC_NKCountryOfOrigin = "SV";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_Tariff = "0406.10.08 00";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 70m;
			invoiceLine.JI_Weight = 1000m;
			declaration.JE_MasterBill = "PPLS99150490";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			invoiceLine.US_SPI = SpecialProgramList.Codes.PPlus;

			MergeAndSend("99150490 in-lieu P+");
			AssertEquals("Duty amount for P+ FTA", 105.63m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2009, 6, 1)]
		public void Test9802008068()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_98GoodsValue = 2300m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Tariff = "4201.00.60 00";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.JI_Weight = 2;

			MergeAndSend("9802008068");

			AssertEquals(2300m, invoiceLine.CusEntryLine.ParentLine.CL_CustomsValue);
			AssertEquals(0m, invoiceLine.CusEntryLine.ParentLine.MPFAmount);
			AssertEquals(42m, invoiceLine.CusEntryLine.MPFAmount);
			AssertEquals(20000m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals("Duty calculated", 560m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
			AssertEquals("MPF amount", 42m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
			AssertEquals(42m, invoiceLine.CusEntryLine.Header.MPFAmountForEntry);
		}

		[TestDate(2016, 03, 27)]
		public void Test9802008068WithSPI_IL()
		{
			invoiceLine.US_UC_NKCountryOfExport = "IL";
			invoiceLine.US_UC_NKCountryOfOrigin = "IL";
			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_98GoodsValue = 2300m;

			invoiceLine.US_SPI = "IL";
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Tariff = "4201.00.60 00";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 200;
			invoiceLine.JI_Weight = 200;

			MergeAndSend("9802008068 with SPI IL");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test9802008068WithSPI_MX()
		{
			invoiceLine.US_UC_NKCountryOfExport = "MX";
			invoiceLine.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.US_98GoodsValue = 2300m;
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Tariff = "4201.00.60 00";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 200;
			invoiceLine.JI_Weight = 200;
			invoiceLine.US_SPI = "MX";

			MergeAndSend("9802008068 with SPI MX");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test9802009000()
		{
			invoiceLine.US_SupTariff = "9802009000";
			invoiceLine.US_98GoodsValue = 2300m;
			invoiceLine.US_UC_NKCountryOfExport = "MX";
			invoiceLine.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Tariff = "4201.00.60 00";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 200;
			invoiceLine.JI_Weight = 200;

			MergeAndSend("9802009000");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
			AssertNull("MPF amount", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2016, 03, 27)]
		public void Test9802002000()
		{
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_Tariff = "9802002000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 2300m;
			MergeAndSend("9802.00.20 20");
			AssertEquals("Duty calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test98020040()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_SupTariff = "9802.00.40 20";
			invoiceLine.US_98GoodsValue = 2300m;
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Tariff = "8407341800";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.JI_Weight = 200;

			MergeAndSend("9802.00.40 20");
			AssertEquals("Duty Calculated", 500m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test98020040WithSPI()
		{
			invoiceLine.US_SupTariff = "9802.00.40 20";
			invoiceLine.US_98GoodsValue = 2300m;
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "AU";
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Tariff = "8407341800";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.JI_Weight = 200;

			MergeAndSend("9802.00.40 20 with SPI");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test9802005030()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_SupTariff = "9802.00.50 30";
			invoiceLine.US_98GoodsValue = 2300m;
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Tariff = "8407341800";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.JI_Weight = 200;

			MergeAndSend("9802.00.50 30");
			AssertEquals("Duty calculated against invoice line2", 500m, invoiceLine.CusEntryLine.DutyAmount);
			AssertEquals("Duty calculated", 500m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2009, 6, 1)]
		public void Test9802006000()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			invoiceLine.US_SupTariff = "9802006000";
			invoiceLine.US_98GoodsValue = 2300m;
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";

			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Tariff = "2805110000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.JI_Weight = 200;

			MergeAndSend("9802006000");
			AssertEquals("Duty Calculated", 1060m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
			AssertEquals(0m, invoiceLine.CusEntryLine.ParentLine.MPFAmount);
			AssertEquals(42m, invoiceLine.CusEntryLine.MPFAmount);
			AssertEquals("MPF Amount", 42.00m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
			AssertEquals(42m, invoiceLine.CusEntryLine.Header.MPFAmountForEntry);
		}

		[TestDate(2016, 03, 27)]
		public void Test9802006000WithSPI_BH()
		{
			declaration.JE_RL_NKPortOfLoading = "BHADA";
			declaration.JE_RL_NKOrigin = "BHADA";

			invoiceLine.US_UC_NKCountryOfExport = "BH";
			invoiceLine.US_UC_NKCountryOfOrigin = "BH";
			invoiceLine.US_SupTariff = "9802006000";
			invoiceLine.US_98GoodsValue = 2300m;
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Tariff = "2805110000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 200;
			invoiceLine.JI_Weight = 200;
			invoiceLine.US_SPI = "BH";

			MergeAndSend("9802006000 with SPI BH");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test9802006000WithSPI_AU()
		{
			invoiceLine.US_SupTariff = "9802006000";
			invoiceLine.US_98GoodsValue = 2300m;
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "AU";
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Tariff = "2805110000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 200;
			invoiceLine.JI_Weight = 200;

			MergeAndSend("9802006000 with SPI AU");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test98040060FromKR()
		{
			invoiceLine.JI_Tariff = "98040060";
			invoiceLine.US_UC_NKCountryOfExport = "KR";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			DOT dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			dot.US_DOTPassport = "PASS";
			DOTVIN dotvin = dot.DOTVINs.AddNew();
			dotvin.US_DOTMake = "HYUNDAI";
			dotvin.US_DOTModel = "LANTRA";
			dotvin.US_DOTVIN = "UER897564789RHI";
			dotvin.US_DOTYear = 1980;

			MergeAndSend("98040060 from KR");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test98040060WithIL()
		{
			invoiceLine.JI_Tariff = "98040060";
			invoiceLine.US_UC_NKCountryOfExport = "IL";
			invoiceLine.US_UC_NKCountryOfOrigin = "IL";
			invoiceLine.US_SPI = "IL";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			DOT dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			dot.US_DOTPassport = "PASS";
			DOTVIN dotvin = dot.DOTVINs.AddNew();
			dotvin.US_DOTMake = "HYUNDAI";
			dotvin.US_DOTModel = "LANTRA";
			dotvin.US_DOTVIN = "UER897564789RHI";
			dotvin.US_DOTYear = 1980;

			MergeAndSend("98040060 with IL SPI");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test9812002000FromKR()
		{
			invoiceLine.JI_Tariff = "9812002000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfExport = "KR";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			MergeAndSend("9812002000 from KR");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test9812002000FromAU()
		{
			invoiceLine.JI_Tariff = "9812002000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "AU";
			MergeAndSend("9812002000 with AU SPI");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test98120020FromKR()
		{
			invoiceLine.JI_Tariff = "9812002000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfExport = "KR";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			MergeAndSend("9812002000 from KR");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test98120020WithSPIAU()
		{
			invoiceLine.JI_Tariff = "9812002000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "AU";

			MergeAndSend("9812002000 with SPI AU");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test9814005000WithSPI_AU()
		{
			invoiceLine.JI_Tariff = "9814005000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "AU";
			declaration.JE_MasterBill = "AU9814005000";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);

			MergeAndSend("9814005000 with SPI AU");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test98160020WFromKR()
		{
			invoiceLine.JI_Tariff = "98160020";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfExport = "KR";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";

			MergeAndSend("98160020 from KR");
			AssertEquals("Duty Calculated", 300m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test98160020WWithSPI_IL()
		{
			invoiceLine.JI_Tariff = "98160020";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfExport = "IL";
			invoiceLine.US_UC_NKCountryOfOrigin = "IL";
			invoiceLine.US_SPI = "IL";

			MergeAndSend("98160020 with SPI IL");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2008, 12, 31)]
		public void Test98160020WWithSPI_CL()
		{
			invoiceLine.JI_Tariff = "98160020";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfExport = "CL";
			invoiceLine.US_UC_NKCountryOfOrigin = "CL";
			invoiceLine.US_SPI = "CL";

			MergeAndSend("98160020 with SPI CL");
			AssertEquals("Duty Calculated", 150m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test9813000520FromKR()
		{
			CreateNewOrGetExistingCusCodeListForFIRMSType("Z104");
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.JE_DateOfArrival = declaration.JE_ExportDate;
			declaration.US_US_NKLocationOfGoods = "Z104";

			invoiceLine.US_SupTariff = "9813000520";
			invoiceLine.US_UC_NKCountryOfExport = "KR";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "9001.40.00 00";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 200m;
			invoiceLine.JI_Weight = 200;

			MergeAndSend("9813000520 from KR");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void Test9813000520WithSPI_AU()
		{
			CreateNewOrGetExistingCusCodeListForFIRMSType("Z104");
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.JE_DateOfArrival = declaration.JE_ExportDate;
			declaration.US_US_NKLocationOfGoods = "Z104";

			invoiceLine.US_SupTariff = "9813000520";
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Tariff = "9001.40.00 00";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 200m;
			invoiceLine.JI_Weight = 200;
			invoiceLine.US_SPI = "AU";

			MergeAndSend("9813000520 with SPI AU");
			AssertEquals("Duty Calculated", 0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		void CreateNewOrGetExistingCusCodeListForFIRMSType(ZString code)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, code, "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}
	}
}
