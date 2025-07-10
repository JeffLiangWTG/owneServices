using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ControllingMessageHeaderLinkInvoiceLine))]
	sealed class ControllingMessageHeaderLinkInvoiceLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLinkReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader1.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader1.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			var messageHeader2 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader2.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			messageHeader2.TW1_CertificateType = CertificateTypeList.Codes.Code5;
			var messageHeader3 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader3.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader3.TW1_CertificateType = CertificateTypeList.Codes.Code6;
			var controllingMessageHeaderLinkInvoiceLine1 = messageHeader1.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().FirstOrDefault(x => x.InvoicelinePK == line1.PK);
			controllingMessageHeaderLinkInvoiceLine1.Link = true;
			var controllingMessageHeaderLinkInvoiceLine2 = messageHeader2.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().FirstOrDefault(x => x.InvoicelinePK == line1.PK);
			AssertEquals(false, controllingMessageHeaderLinkInvoiceLine2.LinkInfo.ReadOnly);

			var controllingMessageHeaderLinkInvoiceLine3 = messageHeader3.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().FirstOrDefault(x => x.InvoicelinePK == line1.PK);
			AssertEquals(true, controllingMessageHeaderLinkInvoiceLine3.LinkInfo.ReadOnly);

			var controllingMessageHeaderLinkInvoiceLine4 = messageHeader3.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().FirstOrDefault(x => x.InvoicelinePK == line2.PK);
			AssertEquals(false, controllingMessageHeaderLinkInvoiceLine4.LinkInfo.ReadOnly);

			controllingMessageHeaderLinkInvoiceLine1.Link = false;
			AssertEquals(false, controllingMessageHeaderLinkInvoiceLine3.LinkInfo.ReadOnly);

			messageHeader3.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_07;
			AssertEquals(true, controllingMessageHeaderLinkInvoiceLine3.LinkInfo.ReadOnly);
		}

		public void TestFieldMapping()
		{
			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var rateType = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY", "Duty");
			var rateCodeDTA = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTA, rateType.PK);
			var rateCodeDTS = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTS, rateType.PK);
			var preference = universalTestHelper.CreatePreferenceForCountry("PR1", "PR1", Core.Constants.CountryCodes.Taiwan);
			var tradeGroup = universalTestHelper.CreateTradeGroup("TW", "JP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.AddCountry(tradeGroup, "JP", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariffDTA = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090200", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTA = universalTestHelper.CreateRate(tariffDTA, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDTS = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090201", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTS = universalTestHelper.CreateRate(tariffDTS, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "150 * [LTR]", preference.PK, "150/LTR", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTS, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffBoth = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090202", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffBothRateDTS = universalTestHelper.CreateRate(tariffBoth, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "150 * [LTR]", preference.PK, "150/LTR", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(tariffBothRateDTS, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tariffBothRateDTA = universalTestHelper.CreateRate(tariffBoth, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(tariffBothRateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var buyer1 = Factory.New<OrgHeader>();
			buyer1.OH_IsConsignee = true;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART NO";
			part.OP_Desc = "Part no 1";
			var relation = part.RelatedOrganisations.AddOrganisationIfNotExist(buyer1.PK, OrgPartRelation.RelationshipTypes.Owner);
			relation.OU_LocalPartNumber = "BUYER1PARTNO";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_Importer = buyer1.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceDisplaySequence = 2;
			invoiceHeader.JZ_InvoiceNumber = "Inv Number1";
			var orderLine = Factory.New<OrderLine>();
			orderLine.JO_LineNo = 1;
			orderLine.JO_SubLineNo = 1;
			var line = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			line.JI_LineNo = 1;
			line.JI_InvoiceQuantity = 999;
			line.JI_InvoiceUQ = "PCE";
			line.JI_EnteredUnitPrice = 100;
			line.JI_DtyPymntMthd = "CAS";
			line.JI_VatPymntMthd = "DEF";
			line.JI_Tariff = "21039090200";
			line.JI_CountryOfOrigin = "JP";
			line.JI_PrimaryPreference = "PR1";
			line.JI_Weight = 1m;
			line.JI_WeightUQ = "KG";
			line.JI_Volume = 2m;
			line.JI_VolumeUQ = "M3";
			line.JI_OrderNumber = "AB";
			line.JI_JO = orderLine.PK;
			line.JI_PartAttrib1 = "AT1";
			line.JI_PartAttrib2 = "AT2";
			line.JI_PartAttrib3 = "AT3";
			line.JI_SerialNumber = "SN1";
			line.JI_PartNo = "PART NO";
			line.JI_NewOwnerPartNo = part.OP_PartNum;
			line.JI_NewPartAttribute1 = "XX1";
			line.JI_NewPartAttribute2 = "XX2";
			line.JI_NewPartAttribute3 = "XX3";
			line.JI_NewSerialNumber = "SN";
			line.JI_CustomAttrib1 = "Attribute Text 1";
			line.JI_CustomAttrib2 = "Attribute Text 2";
			line.JI_CustomAttrib3 = "Attribute Text 3";
			line.JI_CustomAttrib4 = "Attribute Text 4";
			line.JI_CustomAttrib5 = "Attribute Text 5";
			line.JI_CustomAttrib6 = "Attribute Text 6";
			line.JI_CustomTextBlob1 = "Custom Text Blob 1";
			line.JI_CustomsSupplierPartNo = "Supplier Part No";
			line.JI_NDescription = "測試";
			line.JI_CustomsQuantity = 5m;
			line.JI_CustomsUnitQty = "PCE";
			line.JI_Procedure = "44";
			line.JI_Group = "Group";
			line.JI_Description = "Description";
			line.JI_NetWeight = 10m;
			line.JI_NetWeightUQ = "G";
			line.JI_CustomsSecondQuantity = 13m;
			line.JI_CustomsSecondUnitQty = "LT";
			line.JI_MatchingKey = "MATCH004";
			line.JI_ClassUsageComment = "AAA";
			line.JI_GS_NKClassUsageCommentReviewer = "TST";
			line.JI_IsClassUsageCommentRead = true;
			line.CitesPermit = "CP";
			line.HighTechLicense = "HTL";
			line.JI_TariffAdditionalCode = "TA";
			line.JI_AlcoholPercentage = 6M;
			line.JI_CusValueConvRatio = 0.7M;
			line.JI_CarType = "A1";
			line.JI_Transmission = "Z";
			line.JI_EngineType = "CG";
			line.JI_LHD = "Y";
			line.JI_HasCatalystConverter = "E";
			line.JI_CarCondition = "X";
			line.JI_ModelYear = 2015;
			line.JI_Displacement = "3000";
			line.JI_NumberOfDoor = 4;
			line.JI_Cylinders = 3;
			line.JI_Seats = 7;
			line.JI_Gears = 5;
			line.JI_BondedGoodsCode = "YB";
			line.JI_Model = "AA";
			line.JI_BrandName = "TEST BRAND";
			line.JI_RH_NKCommodity_Code = "CC";

			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var controllingMessageHeaderLinkInvoiceLine = messageHeader.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().First();

			CombineAssertions(() =>
			{
				AssertEquals(line.PK, controllingMessageHeaderLinkInvoiceLine.InvoicelinePK);
				AssertEquals(invoiceHeader.JZ_InvoiceDisplaySequence, controllingMessageHeaderLinkInvoiceLine.InvoiceSequence);
				AssertEquals(invoiceHeader.JZ_InvoiceNumber, controllingMessageHeaderLinkInvoiceLine.InvoiceNumber);
				AssertEquals(line.JI_LineNo, controllingMessageHeaderLinkInvoiceLine.InvoiceLineSequence);
				AssertEquals(line.JI_InvoiceQuantity, controllingMessageHeaderLinkInvoiceLine.InvoiceQuantity);
				AssertEquals(line.JI_InvoiceUQ, controllingMessageHeaderLinkInvoiceLine.InvoiceQuantityUQ);
				AssertEquals(line.JI_EnteredUnitPrice, controllingMessageHeaderLinkInvoiceLine.UnitPrice);
				AssertEquals(line.JI_LinePrice, controllingMessageHeaderLinkInvoiceLine.LinePrice);
				AssertEquals(line.JI_FormattedTariff, controllingMessageHeaderLinkInvoiceLine.Tariff);
				AssertEquals(line.JI_CountryOfOrigin, controllingMessageHeaderLinkInvoiceLine.CountryOfOrigin);
				AssertEquals(line.JI_Model, controllingMessageHeaderLinkInvoiceLine.Model);
				AssertEquals(line.JI_BrandName, controllingMessageHeaderLinkInvoiceLine.BrandName);
				AssertEquals(line.JI_RH_NKCommodity_Code, controllingMessageHeaderLinkInvoiceLine.RH_NKCommodity_Code);
				AssertEquals(line.JI_Weight, controllingMessageHeaderLinkInvoiceLine.Weight);
				AssertEquals(line.JI_WeightUQ, controllingMessageHeaderLinkInvoiceLine.WeightUQ);
				AssertEquals(line.JI_Volume, controllingMessageHeaderLinkInvoiceLine.Volume);
				AssertEquals(line.JI_VolumeUQ, controllingMessageHeaderLinkInvoiceLine.VolumeUQ);
				AssertEquals(line.JI_OrderNumber, controllingMessageHeaderLinkInvoiceLine.OrderNumber);
				AssertEquals(line.JI_Calc_OrderLineNumberAndSubLine, controllingMessageHeaderLinkInvoiceLine.Calc_OrderLineNumberAndSubLine);
				AssertEquals(line.JI_PartAttrib1, controllingMessageHeaderLinkInvoiceLine.PartAttrib1);
				AssertEquals(line.JI_PartAttrib2, controllingMessageHeaderLinkInvoiceLine.PartAttrib2);
				AssertEquals(line.JI_PartAttrib3, controllingMessageHeaderLinkInvoiceLine.PartAttrib3);
				AssertEquals(line.JI_SerialNumber, controllingMessageHeaderLinkInvoiceLine.SerialNumber);
				AssertEquals(line.JI_Calc_OwnerPartNo, controllingMessageHeaderLinkInvoiceLine.CustomsOwnerPartNo);
				AssertEquals(line.JI_NewPartAttribute1, controllingMessageHeaderLinkInvoiceLine.NewPartAttribute1);
				AssertEquals(line.JI_NewPartAttribute2, controllingMessageHeaderLinkInvoiceLine.NewPartAttribute2);
				AssertEquals(line.JI_NewPartAttribute3, controllingMessageHeaderLinkInvoiceLine.NewPartAttribute3);
				AssertEquals(line.JI_NewSerialNumber, controllingMessageHeaderLinkInvoiceLine.NewSerialNumber);
				AssertEquals(line.JI_CustomAttrib1, controllingMessageHeaderLinkInvoiceLine.CustomAttrib1);
				AssertEquals(line.JI_CustomAttrib2, controllingMessageHeaderLinkInvoiceLine.CustomAttrib2);
				AssertEquals(line.JI_CustomAttrib3, controllingMessageHeaderLinkInvoiceLine.CustomAttrib3);
				AssertEquals(line.JI_CustomAttrib4, controllingMessageHeaderLinkInvoiceLine.CustomAttrib4);
				AssertEquals(line.JI_CustomAttrib5, controllingMessageHeaderLinkInvoiceLine.CustomAttrib5);
				AssertEquals(line.JI_CustomAttrib6, controllingMessageHeaderLinkInvoiceLine.CustomAttrib6);
				AssertEquals(line.JI_CustomTextBlob1, controllingMessageHeaderLinkInvoiceLine.CustomTextBlob1);
				AssertEquals(line.JI_CustomsSupplierPartNo, controllingMessageHeaderLinkInvoiceLine.CustomsSupplierPartNo);
				AssertEquals(line.JI_NDescription, controllingMessageHeaderLinkInvoiceLine.NDescription);
				AssertEquals(line.JI_PartNo, controllingMessageHeaderLinkInvoiceLine.PartNo);
				AssertEquals(line.JI_PrimaryPreference, controllingMessageHeaderLinkInvoiceLine.PrimaryPreference);
				AssertEquals(line.JI_Procedure, controllingMessageHeaderLinkInvoiceLine.Procedure);
				AssertEquals(line.JI_Group, controllingMessageHeaderLinkInvoiceLine.Group);
				AssertEquals(line.JI_Description, controllingMessageHeaderLinkInvoiceLine.Description);
				AssertEquals(line.JI_NetWeight, controllingMessageHeaderLinkInvoiceLine.NetWeight);
				AssertEquals(line.JI_NetWeightUQ, controllingMessageHeaderLinkInvoiceLine.NetWeightUQ);
				AssertEquals(line.JI_CustomsSecondQuantity, controllingMessageHeaderLinkInvoiceLine.CustomsSecondQuantity);
				AssertEquals(line.JI_CustomsSecondUnitQty, controllingMessageHeaderLinkInvoiceLine.CustomsSecondQuantityUQ);
				AssertEquals(line.JI_MatchingKey, controllingMessageHeaderLinkInvoiceLine.MatchingKey);
				AssertEquals(line.JI_GS_NKClassUsageCommentReviewer, controllingMessageHeaderLinkInvoiceLine.NKClassUsageCommentReviewer);
				AssertEquals(line.JI_ClassUsageComment, controllingMessageHeaderLinkInvoiceLine.ClassUsageComment);
				AssertEquals(line.JI_IsClassUsageCommentRead, controllingMessageHeaderLinkInvoiceLine.IsClassUsageCommentRead);
				AssertEquals(line.CitesPermit, controllingMessageHeaderLinkInvoiceLine.CitesPermit);
				AssertEquals(line.HighTechLicense, controllingMessageHeaderLinkInvoiceLine.HighTechLicense);
				AssertEquals(line.JI_TariffAdditionalCode, controllingMessageHeaderLinkInvoiceLine.TariffAdditionalCode);
				AssertEquals(line.JI_AlcoholPercentage, controllingMessageHeaderLinkInvoiceLine.AlcoholPercentage);
				AssertEquals(line.JI_CusValueConvRatio, controllingMessageHeaderLinkInvoiceLine.CusValueConvRatio);
				AssertEquals(line.JI_CarType, controllingMessageHeaderLinkInvoiceLine.CarType);
				AssertEquals(line.JI_Transmission, controllingMessageHeaderLinkInvoiceLine.Transmission);
				AssertEquals(line.JI_EngineType, controllingMessageHeaderLinkInvoiceLine.EngineType);
				AssertEquals(line.JI_LHD, controllingMessageHeaderLinkInvoiceLine.LHD);
				AssertEquals(line.JI_HasCatalystConverter, controllingMessageHeaderLinkInvoiceLine.HasCatalystConverter);
				AssertEquals(line.JI_CarCondition, controllingMessageHeaderLinkInvoiceLine.CarCondition);
				AssertEquals(line.JI_ModelYear, controllingMessageHeaderLinkInvoiceLine.ModelYear);
				AssertEquals(line.JI_Displacement, controllingMessageHeaderLinkInvoiceLine.Displacement);
				AssertEquals(line.JI_NumberOfDoor, controllingMessageHeaderLinkInvoiceLine.NumberOfDoor);
				AssertEquals(line.JI_Cylinders, controllingMessageHeaderLinkInvoiceLine.Cylinders);
				AssertEquals(line.JI_Seats, controllingMessageHeaderLinkInvoiceLine.Seats);
				AssertEquals(line.JI_Gears, controllingMessageHeaderLinkInvoiceLine.Gears);
				AssertEquals(line.JI_DtyPymntMthd, controllingMessageHeaderLinkInvoiceLine.DtyPymntMthd);
				AssertEquals(line.JI_VatPymntMthd, controllingMessageHeaderLinkInvoiceLine.VatPymntMthd);
				AssertEquals(line.FormattedAdValoremDutyRate, controllingMessageHeaderLinkInvoiceLine.FormattedAdValoremDutyRate);
				AssertEquals(line.FormattedSpecificDutyRate, controllingMessageHeaderLinkInvoiceLine.FormattedSpecificDutyRate);
				AssertEquals(line.JI_BondedGoodsCode, controllingMessageHeaderLinkInvoiceLine.BondedGoodsCode);
			});
		}

		public void TestLink()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var controllingMessageHeaderLinkInvoiceLine = messageHeader.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().First();
			var invoiceLineRelatedControllingMsgHeadersGenPivots = line.InvoiceLineRelatedControllingMsgHeadersGenPivots;
			AssertEquals(0, invoiceLineRelatedControllingMsgHeadersGenPivots.Count);

			controllingMessageHeaderLinkInvoiceLine.Link = true;
			AssertEquals(1, invoiceLineRelatedControllingMsgHeadersGenPivots.Count);

			controllingMessageHeaderLinkInvoiceLine.Link = false;
			AssertEquals(0, invoiceLineRelatedControllingMsgHeadersGenPivots.Count);
		}

		public void TestTariffDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "I02", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "I01", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportRegulations, "E04", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportRegulations, "E03", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "C06", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "C05", tariff1);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			line.JI_Tariff = tariff1.ZZ1_TariffCode;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var controllingMessageHeaderLinkInvoiceLine = messageHeader.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().First();
			AssertEquals("C05, C06, I01, I02", controllingMessageHeaderLinkInvoiceLine.TariffDescription);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("C05, C06, E03, E04", controllingMessageHeaderLinkInvoiceLine.TariffDescription);
		}

		public void TestOnSaveRollbackHasChanges()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			Factory.Save();

			var controllingMessageHeaderLinkInvoiceLine = messageHeader.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().First();
			controllingMessageHeaderLinkInvoiceLine.Link = true;
			AssertEquals(true, controllingMessageHeaderLinkInvoiceLine.HasChanges);

			Factory.Save();
			AssertEquals(false, controllingMessageHeaderLinkInvoiceLine.HasChanges);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			return messageHeader.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().Single();
		}
	}
}
