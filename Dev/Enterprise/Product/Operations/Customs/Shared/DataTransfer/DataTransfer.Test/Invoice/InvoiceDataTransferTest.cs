using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	public abstract class InvoiceDataTransferTest : ValueObjectDataAdapterTest<BaseJobComInvoiceHeader, Xsd.InvoiceHeader>
	{
		public void TestImportIncoIncoterm()
		{
			NotificationBuffer notify = new NotificationBuffer();
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			interchange.InterchangeInfo.EDIOrganisation.OwnerCode = "MAPPINGOWNERCODE";

			var xmlInvoiceHeader = new Xsd.InvoiceHeader();
			xmlInvoiceHeader.Incoterm = "FB1";

			OrgHeader otherOrg = Factory.New<OrgHeader>();
			otherOrg.OH_FullName = "Importer";
			otherOrg.OH_IsConsignee = true;
			otherOrg.OH_IsConsignor = true;
			otherOrg.OH_RL_NKClosestPort = "AUSYD";
			OrgAddress address1 = otherOrg.Addresses[0];
			address1.OA_Address1 = "FAWGE3AR6PKAFVWWY42VZ7TMTNXEH7G5RX0EM2HRJ13ND7WE0J";
			otherOrg.OH_Code = "T5GISEQI2KZR";

			OrgPatternMatchOverride matchOwnerCodeToCurrentCompany = Factory.New<OrgPatternMatchOverride>();
			using (matchOwnerCodeToCurrentCompany.GetValidationSuspender())
			{
				matchOwnerCodeToCurrentCompany.OO_Relationship = "ORG";
				matchOwnerCodeToCurrentCompany.OO_OH = Env.CurrentCompany.OrganisationPK;
				matchOwnerCodeToCurrentCompany.OO_ForeignCode = "MAPPINGOWNERCODE";
				matchOwnerCodeToCurrentCompany.OO_LocalGuid = otherOrg.PK;

				var orgOverride = Factory.New<OrgPatternMatchOverride>();
				orgOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.IncoTerm;
				orgOverride.OO_ForeignCode = "FB1";
				orgOverride.OO_LocalCode = "FOB";
				orgOverride.OO_OH = otherOrg.PK;
				Factory.Save();

				var invoiceHeader = GetInvoiceHeader();
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, notify);
				invoiceDataAdapter.ImportFromValueObject(invoiceHeader, xmlInvoiceHeader, context);

				AssertEquals("Incoterm is translated", "FOB", invoiceHeader.JZ_IncoTerm);
			}
		}

		public void TestEmptyConsigneeInValueObjectDoesNotOverwriteDefaultedConsignee()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			xmlInvoiceHeader.Consignor = new Xsd.Organisation();

			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeader();
			OrgHeader supplier = Factory.New<OrgHeader>();
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			invoiceDataAdapter.ImportFromValueObject(invoiceHeader, xmlInvoiceHeader, context);

			AssertEquals(supplier.PK, invoiceHeader.JZ_OH_Supplier);
		}

		public virtual void TestImportConsigneeValue()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			xmlInvoiceHeader.Consignee = new Xsd.Organisation();
			xmlInvoiceHeader.Consignee.EDICode = "TEST";
			xmlInvoiceHeader.Consignee.OrganisationDetails = new Xsd.OrganisationDetail();
			xmlInvoiceHeader.Consignee.OrganisationDetails.Name = "CargoWise pty ltd";
			xmlInvoiceHeader.Consignee.OwnerCode = "OWNER";

			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeader();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			invoiceDataAdapter.ImportFromValueObject(invoiceHeader, xmlInvoiceHeader, context);

			AssertEquals("Consignee is imported", "CargoWise pty ltd", invoiceHeader.Buyer.OH_FullName);
		}

		public void TestProductClassificationDifferentToClassificationInXml()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<Business.OrgSupplierPart>();
			product.OP_PartNum = "PRODNUM";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = "IMP";
			classification.CC_TariffNum = "0000";
			classification.CC_LookupCode = "TARIFF";

			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = product.PK;

			Factory.Save();

			var xmlInvoiceHeader = new Xsd.InvoiceHeader();
			xmlInvoiceHeader.StandAloneInvoiceDirection = "IMP";
			xmlInvoiceHeader.Consignee = new Xsd.Organisation();
			xmlInvoiceHeader.Consignee.EDICode = importer.OH_Code;
			xmlInvoiceHeader.Consignee.OrganisationDetails = new Xsd.OrganisationDetail();
			xmlInvoiceHeader.Consignee.OrganisationDetails.Name = "CargoWise pty ltd";
			xmlInvoiceHeader.Consignee.OwnerCode = "OWNER";

			var xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();
			xmlInvoiceLine.InvoiceQty = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(20), "QTY");
			xmlInvoiceLine.LinePrice = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(15), AUD);
			xmlInvoiceLine.ProductNumber = "PRODNUM";
			xmlInvoiceLine.ProductDescription = "PRODUCT DESCRIPTION";
			xmlInvoiceLine.LineClassification = new Xsd.InvoiceLineLineClassification();
			xmlInvoiceLine.LineClassification.TariffLookup = "TARIFF";
			xmlInvoiceLine.LineClassification.TariffCode.Value = "1111";
			xmlInvoiceLine.LineClassification.OriginOfGoods = Core.Constants.CountryCodes.NewZealand;

			var invoice = GetInvoiceHeader();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, context);

			var invoiceLine = invoice.JobComInvoiceLines[0];
			AssertEquals("Tariff Number", "1111", invoiceLine.JI_Tariff);
		}

		public void TestProductClassificationDifferentToClassificationInXml2()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<Business.OrgSupplierPart>();
			product.OP_PartNum = "PRODNUM";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_OP = product.PK;

			Factory.Save();

			var xmlInvoiceHeader = new Xsd.InvoiceHeader();
			xmlInvoiceHeader.StandAloneInvoiceDirection = "IMP";
			xmlInvoiceHeader.Consignee = new Xsd.Organisation();
			xmlInvoiceHeader.Consignee.EDICode = importer.OH_Code;
			xmlInvoiceHeader.Consignee.OrganisationDetails = new Xsd.OrganisationDetail();
			xmlInvoiceHeader.Consignee.OrganisationDetails.Name = "CargoWise pty ltd";
			xmlInvoiceHeader.Consignee.OwnerCode = "OWNER";

			var xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();
			xmlInvoiceLine.InvoiceQty = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(20), "QTY");
			xmlInvoiceLine.LinePrice = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(15), AUD);
			xmlInvoiceLine.ProductNumber = "PRODNUM";
			xmlInvoiceLine.ProductDescription = "PRODUCT DESCRIPTION";
			xmlInvoiceLine.LineClassification = new Xsd.InvoiceLineLineClassification();
			xmlInvoiceLine.LineClassification.TariffLookup = "TARIFF";
			xmlInvoiceLine.LineClassification.TariffCode.Value = "1111";
			xmlInvoiceLine.LineClassification.OriginOfGoods = Core.Constants.CountryCodes.NewZealand;

			var invoice = GetInvoiceHeader();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, context);

			var invoiceLine = invoice.JobComInvoiceLines[0];
			AssertEquals("Tariff Number", "1111", invoiceLine.JI_Tariff);
		}

		public void TestImportInvoiceDirection()
		{
			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeader();

			if (invoiceHeader.IsAttachedToPersistentDeclaration)
			{
				Assert(true);
			}
			else
			{
				Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
				xmlInvoiceHeader.StandAloneInvoiceDirection = "IMP";

				xmlInvoiceHeader.Consignee = new Xsd.Organisation();
				xmlInvoiceHeader.Consignee.EDICode = "TEST";
				xmlInvoiceHeader.Consignee.OrganisationDetails = new Xsd.OrganisationDetail();
				xmlInvoiceHeader.Consignee.OrganisationDetails.Name = "CargoWise pty ltd";

				Xsd.UNLOCO consigneeUnloco = new Xsd.UNLOCO();
				consigneeUnloco.Country = "AU";
				consigneeUnloco.Value = "AUSYD";
				xmlInvoiceHeader.Consignee.OrganisationDetails.Location = consigneeUnloco;
				xmlInvoiceHeader.Consignee.OwnerCode = "OWNER";

				ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				invoiceDataAdapter.ImportFromValueObject(invoiceHeader, xmlInvoiceHeader, context);
				Factory.Save();

				AssertEquals("Invoice Direction", "IMP", invoiceHeader.JZ_StandAloneInvoiceDirection);
			}
		}

		public void TestImportFromValueObject()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			xmlInvoiceHeader.InvoiceNumber = "000001";

			invoiceDataAdapter.ImportFromValueObject(InvoiceHeader, xmlInvoiceHeader, importContext);

			AssertEquals("000001", InvoiceHeader.JZ_InvoiceNumber);
		}

		public virtual void TestImportInvoiceDetails()
		{
			#region Sample Data
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			xmlInvoiceHeader.InvoiceNumber = "000001";
			xmlInvoiceHeader.InvoiceAmount = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(10), AUD);

			xmlInvoiceHeader.Consignor = new Xsd.Organisation();
			xmlInvoiceHeader.Consignor.EDICode = "TEST";
			xmlInvoiceHeader.Consignor.OrganisationDetails = new Xsd.OrganisationDetail();
			xmlInvoiceHeader.Consignor.OrganisationDetails.Name = "Name";
			xmlInvoiceHeader.Consignor.OwnerCode = "OWNER";

			xmlInvoiceHeader.InvoiceDate = new ZDate(2005, 03, 15);
			xmlInvoiceHeader.ValuationDate = new ZDateTime(2005, 03, 15, 16, 50, 0);
			xmlInvoiceHeader.IsGroupInvoice = Xsd.TrueFalse.@false;
			xmlInvoiceHeader.IsGroupInvoiceSpecified = true;
			xmlInvoiceHeader.Incoterm = "INC";

			xmlInvoiceHeader.Weight = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(100), "KG");
			xmlInvoiceHeader.Volume = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(300), "VL");
			xmlInvoiceHeader.Packages = 10m;

			#endregion

			invoiceDataAdapter.ImportFromValueObject(InvoiceHeader, xmlInvoiceHeader, importContext);

			AssertEquals("000001", InvoiceHeader.JZ_InvoiceNumber);
			AssertEquals((ZDecimal)10, InvoiceHeader.JZ_InvoiceAmount);

			AssertEquals(new ZDateTime(2005, 03, 15), InvoiceHeader.JZ_InvoiceDate);
			AssertEquals(new ZDateTime(2005, 03, 15, 16, 50, 0), InvoiceHeader.JZ_ValuationDateOverride);

			Assert(!InvoiceHeader.JZ_GroupInvoice);
			AssertEquals("INC", InvoiceHeader.JZ_IncoTerm);

			AssertEquals((ZDecimal)100, InvoiceHeader.JZ_Weight);
			AssertEquals("KG", InvoiceHeader.JZ_WeightUQ);

			AssertEquals((ZDecimal)300, InvoiceHeader.JZ_Volume);
			AssertEquals("VL", InvoiceHeader.JZ_VolumeUQ);

			if (InvoiceHeader.IsAttachedToPersistentDeclaration)
			{
				AssertEquals((ZDecimal)10, InvoiceHeader.JZ_NoOfPacks);
			}
		}

		public void TestImportInvoiceHeader()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			xmlInvoiceHeader.InvoiceNumber = "000001";
			xmlInvoiceHeader.InvoiceAmount = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(10), AUD);

			xmlInvoiceHeader.Consignor = new Xsd.Organisation();
			xmlInvoiceHeader.Consignor.EDICode = "TEST";
			xmlInvoiceHeader.Consignor.OrganisationDetails = new Xsd.OrganisationDetail();
			xmlInvoiceHeader.Consignor.OrganisationDetails.Name = "Name";
			xmlInvoiceHeader.Consignor.OwnerCode = "OWNER";

			xmlInvoiceHeader.InvoiceDate = new ZDate(2005, 03, 15);
			xmlInvoiceHeader.ValuationDate = new ZDateTime(2005, 03, 15, 16, 50, 0);
			xmlInvoiceHeader.IsGroupInvoice = Xsd.TrueFalse.@false;
			xmlInvoiceHeader.IsGroupInvoiceSpecified = true;
			xmlInvoiceHeader.Incoterm = "INC";

			xmlInvoiceHeader.Weight = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(100), "KG");
			xmlInvoiceHeader.Volume = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(300), "VL");

			invoiceDataAdapter.ImportFromValueObject(InvoiceHeader, xmlInvoiceHeader, importContext);

			AssertEquals("000001", InvoiceHeader.JZ_InvoiceNumber);
			AssertEquals((ZDecimal)10, InvoiceHeader.JZ_InvoiceAmount);

			Assert(!InvoiceHeader.JZ_OH_Supplier.IsEmpty);

			AssertEquals(new ZDateTime(2005, 03, 15), InvoiceHeader.JZ_InvoiceDate);
			AssertEquals(new ZDateTime(2005, 03, 15, 16, 50, 0), InvoiceHeader.JZ_ValuationDateOverride);

			Assert(!InvoiceHeader.JZ_GroupInvoice);
			AssertEquals("INC", InvoiceHeader.JZ_IncoTerm);

			AssertEquals((ZDecimal)100, InvoiceHeader.JZ_Weight);
			AssertEquals("KG", InvoiceHeader.JZ_WeightUQ);

			AssertEquals((ZDecimal)300, InvoiceHeader.JZ_Volume);
			AssertEquals("VL", InvoiceHeader.JZ_VolumeUQ);
		}

		public void TestImportInvoiceChargesDetails()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();

			Xsd.InvoiceChargeCollection invoiceCharges = new Xsd.InvoiceChargeCollection();
			Xsd.InvoiceCharge charge = invoiceCharges.AddNew();
			charge.ChargeType = "CHG";
			charge.ChargeValue = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(10), AUD);
			charge.DutyApplies = Xsd.TrueFalse.@true;
			charge.DutyAppliesSpecified = true;
			charge.GstApplies = Xsd.TrueFalse.@true;
			charge.GstAppliesSpecified = true;
			charge.IsIncludedInTotal = Xsd.TrueFalse.@true;
			charge.IsIncludedInTotalSpecified = true;

			charge = invoiceCharges.AddNew();
			charge.ChargeType = "ADD";
			charge.ChargeValue = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(15), NZD);
			charge.GstApplies = Xsd.TrueFalse.@true;
			charge.GstAppliesSpecified = true;
			charge.IsIncludedInTotal = Xsd.TrueFalse.@false;
			charge.IsIncludedInTotalSpecified = true;
			charge.IsIncludedInInvoice = Xsd.TrueFalse.@false;
			charge.IsIncludedInInvoiceSpecified = true;

			xmlInvoiceHeader.InvoiceCharges = invoiceCharges;

			invoiceDataAdapter.ImportFromValueObject(InvoiceHeader, xmlInvoiceHeader, importContext);

			var charges = InvoiceHeader.Charges;

			AssertEquals(2, charges.Count);

			AssertEquals("CHG", charges[0].J7_ChargeType);
			AssertEquals((ZDecimal)10, charges[0].J7_Amount);
			AssertEquals("AUD", charges[0].J7_RX_NKCurrency);
			Assert(charges[0].J7_IsDutiable);
			Assert(charges[0].J7_IsGSTApplicable);
			Assert(charges[0].J7_IsIncludedInITOT);

			AssertEquals("ADD", charges[1].J7_ChargeType);
			AssertEquals((ZDecimal)15, charges[1].J7_Amount);
			AssertEquals("NZD", charges[1].J7_RX_NKCurrency);
			Assert("If it is not specified, it should not attempt to set a value here. The default value will stay", charges[1].J7_IsDutiable);
			Assert(charges[1].J7_IsGSTApplicable);
			Assert(!charges[1].J7_IsIncludedInITOT);
			Assert(charges[1].J7_IsNotIncludedInInvoice);
		}

		public void TestImportInvoiceLinesDetail()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();

			Xsd.InvoiceLineCollection invoiceLines = new Xsd.InvoiceLineCollection();

			Xsd.InvoiceLine invoiceLine = invoiceLines.AddNew();
			invoiceLine.OrderNumber = "000001";
			invoiceLine.ProductDescription = "Test 1";
			invoiceLine.InvoiceLineNumber = "1";

			invoiceLine = invoiceLines.AddNew();
			invoiceLine.OrderNumber = "000002";
			invoiceLine.ProductDescription = "Test 2";
			invoiceLine.InvoiceLineNumber = "2";

			xmlInvoiceHeader.InvoiceLines = invoiceLines;

			invoiceDataAdapter.ImportFromValueObject(InvoiceHeader, xmlInvoiceHeader, importContext);
			invoiceDataAdapter.ImportFromValueObject(InvoiceHeader, xmlInvoiceHeader, importContext);

			AssertEquals(2, InvoiceHeader.JobComInvoiceLines.Count);
		}

		public void TestImportInvoiceLineChargesDetails()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();

			Xsd.InvoiceChargeCollection invoiceCharges = new Xsd.InvoiceChargeCollection();
			Xsd.InvoiceCharge charge = invoiceCharges.AddNew();
			charge.ChargeType = "OFT";
			charge.ChargeValue = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(10), AUD);
			charge.DutyApplies = Xsd.TrueFalse.@true;
			charge.DutyAppliesSpecified = true;
			charge.GstApplies = Xsd.TrueFalse.@true;
			charge.GstAppliesSpecified = true;
			charge.IsIncludedInTotal = Xsd.TrueFalse.@true;
			charge.IsIncludedInTotalSpecified = true;

			charge = invoiceCharges.AddNew();
			charge.ChargeType = "ONS";
			charge.ChargeValue = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(15), NZD);
			charge.DutyApplies = Xsd.TrueFalse.@false;
			charge.DutyAppliesSpecified = true;
			charge.GstApplies = Xsd.TrueFalse.@false;
			charge.GstAppliesSpecified = true;
			charge.IsIncludedInTotal = Xsd.TrueFalse.@false;
			charge.IsIncludedInTotalSpecified = true;

			charge = invoiceCharges.AddNew();
			charge.ChargeType = "ADD";
			charge.ChargeValue = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(16), NZD);
			charge.GstApplies = Xsd.TrueFalse.@true;
			charge.GstAppliesSpecified = true;
			charge.IsIncludedInTotal = Xsd.TrueFalse.@false;
			charge.IsIncludedInTotalSpecified = true;
			charge.IsIncludedInInvoice = Xsd.TrueFalse.@false;
			charge.IsIncludedInInvoiceSpecified = true;

			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();
			xmlInvoiceLine.Charges = invoiceCharges;

			InvoiceHeader.JZ_IncoTerm = "CFR";

			invoiceDataAdapter.ImportFromValueObject(InvoiceHeader, xmlInvoiceHeader, importContext);

			AssertEquals("One invoiceline", 1, InvoiceHeader.JobComInvoiceLines.Count);

			var charges = InvoiceHeader.JobComInvoiceLines[0].Charges;

			AssertEquals(3, charges.Count);

			AssertEquals("OFT", charges[0].J7_ChargeType);
			AssertEquals((ZDecimal)10, charges[0].J7_Amount);
			AssertEquals("AUD", charges[0].J7_RX_NKCurrency);
			Assert(charges[0].J7_IsDutiable);
			Assert(charges[0].J7_IsGSTApplicable);
			Assert(charges[0].J7_IsIncludedInITOT);

			AssertEquals("ONS", charges[1].J7_ChargeType);
			AssertEquals((ZDecimal)15, charges[1].J7_Amount);
			AssertEquals("NZD", charges[1].J7_RX_NKCurrency);
			Assert(!charges[1].J7_IsDutiable);
			Assert(!charges[1].J7_IsGSTApplicable);
			Assert(!charges[1].J7_IsIncludedInITOT);

			AssertEquals("ADD", charges[2].J7_ChargeType);
			AssertEquals((ZDecimal)16, charges[2].J7_Amount);
			AssertEquals("NZD", charges[2].J7_RX_NKCurrency);
			Assert("If it is not specified, it should not attempt to set a value here. The default value will stay", charges[2].J7_IsDutiable);
			Assert(charges[2].J7_IsGSTApplicable);
			Assert(!charges[2].J7_IsIncludedInITOT);
			Assert(charges[2].J7_IsNotIncludedInInvoice);
		}

		public void TestImportInvoiceLineDetail()
		{
			CreateTestCusClassification();

			BaseJobComInvoiceHeader invoice = InvoiceHeader;
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			#region Test Data
			JobDec.JE_RL_NKPortOfArrival = "AUMEL";

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();

			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();

			xmlInvoiceLine.InvoiceQty = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(20), "QTY");

			xmlInvoiceLine.LinePrice = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(15), AUD);

			xmlInvoiceLine.ProductNumber = "PRODNUM";
			xmlInvoiceLine.ProductDescription = "PRODUCT DESCRIPTION";
			xmlInvoiceLine.ExtendedProductDescription = "EXTENDED PRODUCT DESCRIPTION";

			xmlInvoiceLine.CustomsInvoiceQty = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(101), "QTY");

			xmlInvoiceLine.OrderNumber = "ORDERNUM";

			xmlInvoiceLine.LineClassification = new Xsd.InvoiceLineLineClassification();
			xmlInvoiceLine.LineClassification.TariffLookup = "TARIFF";
			xmlInvoiceLine.LineClassification.TariffCode.Value = TariffNumber;
			xmlInvoiceLine.LineClassification.OriginOfGoods = Core.Constants.CountryCodes.NewZealand;

			xmlInvoiceLine.Volume = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(45), "QTY");
			xmlInvoiceLine.Weight = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(127), "KG");

			xmlInvoiceLine.CustomText1 = "CustomText1";
			xmlInvoiceLine.CustomText2 = "CustomText2";
			xmlInvoiceLine.CustomText3 = "CustomText3";
			xmlInvoiceLine.CustomText4 = "CustomText4";
			xmlInvoiceLine.CustomText5 = "CustomText5";
			xmlInvoiceLine.CustomText6 = "CustomText6";

			xmlInvoiceLine.CustomTextField1 = "CustomTextField1";

			xmlInvoiceLine.CustomFlag1 = false;
			xmlInvoiceLine.CustomFlag2 = true;
			xmlInvoiceLine.CustomFlag3 = false;

			xmlInvoiceLine.CustomDate1 = ZDateTime.BrettsBirthday;
			xmlInvoiceLine.CustomDate2 = ZDateTime.BrettsBirthday;
			xmlInvoiceLine.CustomDate3 = ZDateTime.BrettsBirthday;

			xmlInvoiceLine.CustomDecimal1 = 12.12;
			xmlInvoiceLine.CustomDecimal2 = 13.13;
			xmlInvoiceLine.CustomDecimal3 = 14.14;

			xmlInvoiceLine.PartAttrib1 = "PartAttrib1";
			xmlInvoiceLine.PartAttrib2 = "PartAttrib2";
			xmlInvoiceLine.PartAttrib3 = "PartAttrib3";

			xmlInvoiceLine.NetWeight.Value = 160m;
			xmlInvoiceLine.NetWeight.DimensionType = "KJ";

			#endregion

			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, importContext);

			AssertEquals("Invoice Quantity", (ZDecimal)20, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("Invoice Quantity unit of measure", "QTY", invoiceLine.JI_InvoiceUQ);

			AssertEquals("Line Price", (ZDecimal)15, invoiceLine.JI_LinePrice);

			AssertEquals("Part Number", "PRODNUM", invoiceLine.JI_PartNo);

			AssertEquals("Order Number", "ORDERNUM", invoiceLine.JI_OrderNumber);

			AssertEquals("Tariff Number", TariffNumber, invoiceLine.JI_Tariff);
			AssertEquals("PRODUCT DESCRIPTION", invoiceLine.JI_Description);
			AssertEquals(invoiceLine.IsExtendedCommercialDescriptionEnabled ? "EXTENDED PRODUCT DESCRIPTION" : "", invoiceLine.JI_ExtraInfoForClassification);

			AssertEquals("Invoice Line - Country of Origin", Core.Constants.CountryCodes.NewZealand, invoiceLine.JI_CountryOfOrigin);

			AssertEquals("Volume", (ZDecimal)45, invoiceLine.JI_Volume);
			AssertEquals("Volume unit of measure", "QT", invoiceLine.JI_VolumeUQ);

			AssertEquals("Weight", (ZDecimal)127, invoiceLine.JI_Weight);
			AssertEquals("Weight unit of measure", "KG", invoiceLine.JI_WeightUQ);

			AssertEquals(160m, invoiceLine.JI_NetWeight);
			AssertEquals("KJ", invoiceLine.JI_NetWeightUQ);

			AssertEquals("CustomText1", invoiceLine.JI_CustomAttrib1);
			AssertEquals("CustomText2", invoiceLine.JI_CustomAttrib2);
			AssertEquals("CustomText3", invoiceLine.JI_CustomAttrib3);
			AssertEquals("CustomText4", invoiceLine.JI_CustomAttrib4);
			AssertEquals("CustomText5", invoiceLine.JI_CustomAttrib5);
			AssertEquals("CustomText6", invoiceLine.JI_CustomAttrib6);

			AssertEquals("CustomTextField1", invoiceLine.JI_CustomTextBlob1);

			AssertEquals(false, invoiceLine.JI_CustomFlag1);
			AssertEquals(true, invoiceLine.JI_CustomFlag2);
			AssertEquals(false, invoiceLine.JI_CustomFlag3);

			AssertEquals(ZDateTime.BrettsBirthday, invoiceLine.JI_CustomDate1);
			AssertEquals(ZDateTime.BrettsBirthday, invoiceLine.JI_CustomDate2);
			AssertEquals(ZDateTime.BrettsBirthday, invoiceLine.JI_CustomDate3);

			AssertEquals((decimal)12.12, invoiceLine.JI_CustomDecimal1);
			AssertEquals((decimal)13.13, invoiceLine.JI_CustomDecimal2);
			AssertEquals((decimal)14.14, invoiceLine.JI_CustomDecimal3);
			AssertEquals("Part Attribute 1", "PartAttrib1", invoiceLine.JI_PartAttrib1);
			AssertEquals("Part Attribute 2", "PartAttrib2", invoiceLine.JI_PartAttrib2);
			AssertEquals("Part Attribute 3", "PartAttrib3", invoiceLine.JI_PartAttrib3);

			xmlInvoiceLine.LineClassification.TariffLookup = "";
			invoiceLine = JobDec.Invoices[0].JobComInvoiceLines.AddNew();

			xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();
			xmlInvoiceLine.CustomText1 = "CustomText1";

			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, importContext);

			AssertEquals("Custom Attribute 1 - Second Pass", "CustomText1", JobDec.Invoices[0].JobComInvoiceLines[0].JI_CustomAttrib1);
		}

		public void TestImportInvoiceLineDetail_ClassificationDescriptionIsNotOverWrittenByEmptyProductDescription()
		{
			CreateTestCusClassification();

			BaseJobComInvoiceHeader invoice = InvoiceHeader;
			BaseJobComInvoiceLine invoiceLine = JobDec.Invoices[0].JobComInvoiceLines.AddNew();
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();

			#region Test Data

			JobDec.JE_RL_NKPortOfArrival = "AUMEL";

			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();
			xmlInvoiceLine.ProductDescription = "";

			xmlInvoiceLine.LineClassification = new Xsd.InvoiceLineLineClassification();
			xmlInvoiceLine.LineClassification.TariffLookup = "TARIFF";

			#endregion

			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, importContext);

			AssertEquals("TEST FOR INVOICE XML DATA ADAPTER", invoiceLine.JI_Description);
		}

		public void TestImportInvoiceLineDetail_InvoiceQtyUQIsNotOverwrittenByAnEmptyInvoiceQty_DimensionType()
		{
			CreateTestCusClassification();

			BaseJobComInvoiceHeader invoice = InvoiceHeader;
			BaseJobComInvoiceLine invoiceLine = JobDec.Invoices[0].JobComInvoiceLines.AddNew();
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();

			#region Test Data

			JobDec.JE_RL_NKPortOfArrival = "AUMEL";
			JobDec.JE_MessageType = "IMP";

			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();
			xmlInvoiceLine.ProductDescription = "";

			xmlInvoiceLine.LineClassification = new Xsd.InvoiceLineLineClassification();
			xmlInvoiceLine.LineClassification.TariffLookup = "TARIFF";

			xmlInvoiceLine.InvoiceQty.Value = 99;
			xmlInvoiceLine.InvoiceQty.DimensionType = "";

			#endregion

			invoiceLine.JI_InvoiceUQ = "ZZ";

			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, importContext);

			AssertEquals("", "ZZ", invoiceLine.JI_InvoiceUQ);
		}

		public void TestImportInvoiceLineDetail_NoClassification()
		{
			BaseJobComInvoiceHeader invoice = InvoiceHeader;
			BaseJobComInvoiceLine invoiceLine = JobDec.Invoices[0].JobComInvoiceLines.AddNew();

			JobDec.JE_RL_NKPortOfArrival = "AUMEL";
			JobDec.JE_MessageType = "IMP";

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();

			xmlInvoiceLine.ProductDescription = "PRODUCT DESCRIPTION";
			xmlInvoiceLine.LineClassification = new Xsd.InvoiceLineLineClassification();
			xmlInvoiceLine.LineClassification.TariffLookup = "TARIFF";
			xmlInvoiceLine.LineClassification.TariffCode.Value = TariffNumber;

			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, importContext);

			AssertEquals(TariffNumber, invoiceLine.JI_Tariff);
			AssertEquals("PRODUCT DESCRIPTION", invoiceLine.JI_Description);
		}

		public void TestImportInvoiceLineDetail_CustomsInvoiceQtyDimensionTypeShouldNotBeUsed()
		{
			BaseJobComInvoiceHeader invoice = InvoiceHeader;
			BaseJobComInvoiceLine invoiceLine = JobDec.Invoices[0].JobComInvoiceLines.AddNew();

			JobDec.JE_RL_NKPortOfArrival = "AUMEL";
			JobDec.JE_MessageType = "IMP";

			BaseCusClassification classification = CreateTestCusClassification();
			invoiceLine.JI_CC = classification.PK;
			invoiceLine.JI_CustomsUnitQty = "NO";

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();

			xmlInvoiceLine.CustomsInvoiceQty.Value = 10m;
			xmlInvoiceLine.CustomsInvoiceQty.DimensionType = "KG";

			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, importContext);

			AssertEquals("NO", invoiceLine.JI_CustomsUnitQty);
			AssertEquals(10m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestImportInvoiceLineDetail_CustomsInvoiceQtyShouldOlyBeUsedIfCustomsUQHasAValue()
		{
			BaseJobComInvoiceHeader invoice = InvoiceHeader;
			BaseJobComInvoiceLine invoiceLine = JobDec.Invoices[0].JobComInvoiceLines.AddNew();

			JobDec.JE_RL_NKPortOfArrival = "AUMEL";
			JobDec.JE_MessageType = "IMP";

			BaseCusClassification classification = CreateTestCusClassification();
			invoiceLine.JI_CC = classification.PK;
			invoiceLine.JI_CustomsUnitQty = "NO";

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();

			xmlInvoiceLine.CustomsInvoiceQty.Value = 10m;
			xmlInvoiceLine.CustomsInvoiceQty.DimensionType = "KG";

			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, importContext);

			AssertEquals("NO", invoiceLine.JI_CustomsUnitQty);
			AssertEquals(10m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_CustomsUnitQty = "";
			invoiceLine.JI_CustomsQuantity = 0m;

			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, importContext);
			AssertEquals("", invoiceLine.JI_CustomsUnitQty);
			AssertEquals(0m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestInvoiceOriginMapping_IfOriginEmpty()
		{
			NotificationBuffer notify = new NotificationBuffer();
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			interchange.InterchangeInfo.EDIOrganisation.OwnerCode = "MAPPINGOWNERCODE";

			BaseJobComInvoiceHeader invoice = InvoiceHeader;
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, notify);
			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, context);

			AssertEquals(String.Empty, invoiceLine.JI_CountryOfOrigin);
		}

		public void TestInvoiceOriginMapping_IfNoOwnerCode()
		{
			NotificationBuffer notify = new NotificationBuffer();
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			interchange.InterchangeInfo.EDIOrganisation.OwnerCode = String.Empty;

			BaseJobComInvoiceHeader invoice = InvoiceHeader;
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, notify);
			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, context);

			AssertEquals(String.Empty, invoiceLine.JI_CountryOfOrigin);
		}

		public void TestInvoiceOriginMapping_IfNoCountryMappingSpecified()
		{
			NotificationBuffer notify = new NotificationBuffer();
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			interchange.InterchangeInfo.EDIOrganisation.OwnerCode = "MAPPINGOWNERCODE";

			BaseJobComInvoiceHeader invoice = InvoiceHeader;
			BaseJobComInvoiceLine invoiceLine = JobDec.InvoiceLines.AddNew();

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();

			xmlInvoiceLine.LineClassification.OriginOfGoods = "AUST";

			OrgHeader otherOrg = Factory.New<OrgHeader>();
			otherOrg.OH_FullName = "Importer";
			otherOrg.OH_IsConsignee = true;
			otherOrg.OH_IsConsignor = true;
			otherOrg.OH_RL_NKClosestPort = "AUSYD";
			OrgAddress address1 = otherOrg.Addresses[0];
			address1.OA_Address1 = "FAWGE3AR6PKAFVWWY42VZ7TMTNXEH7G5RX0EM2HRJ13ND7WE0J";
			otherOrg.OH_Code = "T5GISEQI2KZR";

			Assert(otherOrg.PK != ZGuid.Empty);

			OrgPatternMatchOverride matchOwnerCodeToCurrentCompany = Factory.New<OrgPatternMatchOverride>();
			using (matchOwnerCodeToCurrentCompany.GetValidationSuspender())
			{
				matchOwnerCodeToCurrentCompany.OO_Relationship = "ORG";
				matchOwnerCodeToCurrentCompany.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
				matchOwnerCodeToCurrentCompany.OO_ForeignCode = "MAPPINGOWNERCODE";
				matchOwnerCodeToCurrentCompany.OO_LocalGuid = otherOrg.PK;

				Factory.Save();

				ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, notify);
				invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, context);

				AssertEquals("AU", invoiceLine.JI_CountryOfOrigin);
				AssertEquals(invoiceLine.JI_CountryOfOriginInfo.MaxLength, invoiceLine.JI_CountryOfOrigin.Length);
			}
		}

		public void TestInvoiceOriginMapping_IfNoCountryMappingSpecified2()
		{
			NotificationBuffer notify = new NotificationBuffer();
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			interchange.InterchangeInfo.EDIOrganisation.OwnerCode = "MAPPINGOWNERCODE";

			BaseJobComInvoiceHeader invoice = InvoiceHeader;
			BaseJobComInvoiceLine invoiceLine = JobDec.InvoiceLines.AddNew();

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();

			xmlInvoiceLine.LineClassification.OriginOfGoods = "AUST";

			OrgHeader otherOrg = Factory.New<OrgHeader>();
			otherOrg.OH_FullName = "Importer";
			otherOrg.OH_IsConsignee = true;
			otherOrg.OH_IsConsignor = true;
			otherOrg.OH_RL_NKClosestPort = "AUSYD";
			OrgAddress address1 = otherOrg.Addresses[0];
			address1.OA_Address1 = "FAWGE3AR6PKAFVWWY42VZ7TMTNXEH7G5RX0EM2HRJ13ND7WE0J";
			otherOrg.OH_Code = "T5GISEQI2KZR";

			Assert(otherOrg.PK != ZGuid.Empty);

			Factory.Save();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, notify);
			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, context);

			AssertEquals("AU", invoiceLine.JI_CountryOfOrigin);
			AssertEquals(invoiceLine.JI_CountryOfOriginInfo.MaxLength, invoiceLine.JI_CountryOfOrigin.Length);
		}

		public void TestAddImportEvent()
		{
			Xsd.InvoiceHeader xsdInvoice = new Xsd.InvoiceHeader();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			BaseJobComInvoiceHeader importedInvoice = (BaseJobComInvoiceHeader)invoiceDataAdapter.CreateOrUpdateFromValueObject(xsdInvoice, context);
			StmALog[] dataImportEvents = importedInvoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added to new invoice", 1, dataImportEvents.Length);

			Factory.Save();

			importedInvoice = (BaseJobComInvoiceHeader)invoiceDataAdapter.CreateOrUpdateFromValueObject(xsdInvoice, context);
			dataImportEvents = importedInvoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added to NEW invoice", 1, dataImportEvents.Length);
		}

		public void TestInvoiceOriginMapping()
		{
			NotificationBuffer notify = new NotificationBuffer();
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			interchange.InterchangeInfo.EDIOrganisation.OwnerCode = "MAPPINGOWNERCODE";

			BaseJobComInvoiceLine invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();

			xmlInvoiceLine.LineClassification.OriginOfGoods = "AUST";

			OrgHeader otherOrg = Factory.New<OrgHeader>();
			otherOrg.OH_FullName = "Importer";
			otherOrg.OH_IsConsignee = true;
			otherOrg.OH_IsConsignor = true;
			otherOrg.OH_RL_NKClosestPort = "AUSYD";
			OrgAddress address1 = otherOrg.Addresses[0];
			address1.OA_Address1 = "FAWGE3AR6PKAFVWWY42VZ7TMTNXEH7G5RX0EM2HRJ13ND7WE0J";
			otherOrg.OH_Code = "T5GISEQI2KZR";

			Assert(otherOrg.PK != ZGuid.Empty);

			OrgPatternMatchOverride matchOwnerCodeToCurrentCompany = Factory.New<OrgPatternMatchOverride>();
			using (matchOwnerCodeToCurrentCompany.GetValidationSuspender())
			{
				matchOwnerCodeToCurrentCompany.OO_Relationship = "ORG";
				matchOwnerCodeToCurrentCompany.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
				matchOwnerCodeToCurrentCompany.OO_ForeignCode = "MAPPINGOWNERCODE";
				matchOwnerCodeToCurrentCompany.OO_LocalGuid = otherOrg.PK;

				OrgPatternMatchOverride mapToCountry = Factory.New<OrgPatternMatchOverride>();
				using (mapToCountry.GetValidationSuspender())
				{
					mapToCountry.OO_Relationship = "COU";
					mapToCountry.OO_ForeignCode = "AUST";
					mapToCountry.OO_OH = otherOrg.PK;
					mapToCountry.OO_LocalGuid = new ZGuid(Core.Constants.CountryGuids.Australia);

					Factory.Save();

					ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, notify);
					invoiceDataAdapter.ImportFromValueObject(InvoiceHeader, xmlInvoiceHeader, context);

					AssertEquals("AU", invoiceLine.JI_CountryOfOrigin);
				}
			}
		}

		public void TestImportParentInvoiceLineNumber()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();

			Xsd.InvoiceLineCollection xmlInvoiceLines = new Xsd.InvoiceLineCollection();

			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceLines.AddNew();
			xmlInvoiceLine.OrderNumber = "000001";
			xmlInvoiceLine.ProductDescription = "Test 1";
			xmlInvoiceLine.InvoiceLineNumber = "1";

			xmlInvoiceLine = xmlInvoiceLines.AddNew();
			xmlInvoiceLine.OrderNumber = "000002";
			xmlInvoiceLine.ProductDescription = "Test 2";
			xmlInvoiceLine.InvoiceLineNumber = "2";

			xmlInvoiceLine = xmlInvoiceLines.AddNew();
			xmlInvoiceLine.OrderNumber = "000003";
			xmlInvoiceLine.ProductDescription = "Test 3";
			xmlInvoiceLine.InvoiceLineNumber = "3";
			xmlInvoiceLine.ParentInvoiceLineNumber = (ZShort)2;
			xmlInvoiceLine.ParentInvoiceLineNumberSpecified = true;

			xmlInvoiceHeader.InvoiceLines = xmlInvoiceLines;

			invoiceDataAdapter.ImportFromValueObject(InvoiceHeader, xmlInvoiceHeader, importContext);
			invoiceDataAdapter.ImportFromValueObject(InvoiceHeader, xmlInvoiceHeader, importContext);

			AssertEquals(3, InvoiceHeader.JobComInvoiceLines.Count);
			AssertEquals(false, InvoiceHeader.JobComInvoiceLines[2].JI_ParentID.IsEmpty);
		}

		public void TestImportInvoiceReferenceDetails()
		{
			#region Sample Data
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			xmlInvoiceHeader.InvoiceNumber = "000001";
			xmlInvoiceHeader.InvoiceAmount = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(10), AUD);

			Xsd.InvoiceReference xmlRefs = xmlInvoiceHeader.References.AddNew();
			xmlRefs.Type = InvoiceHeaderRefsTypeList.Codes.CN;
			xmlRefs.Value = "CNT900800";

			xmlRefs = xmlInvoiceHeader.References.AddNew();
			xmlRefs.Type = InvoiceHeaderRefsTypeList.Codes.MB;
			xmlRefs.Value = "MB0001";

			xmlRefs = xmlInvoiceHeader.References.AddNew();
			xmlRefs.Type = InvoiceHeaderRefsTypeList.Codes.HB;
			xmlRefs.Value = "HB0002";

			xmlRefs = xmlInvoiceHeader.References.AddNew();
			xmlRefs.Type = InvoiceHeaderRefsTypeList.Codes.SH;
			xmlRefs.Value = "SH0003";

			#endregion

			invoiceDataAdapter.ImportFromValueObject(InvoiceHeader, xmlInvoiceHeader, importContext);

			AssertEquals("000001", InvoiceHeader.JZ_InvoiceNumber);
			AssertEquals((ZDecimal)10, InvoiceHeader.JZ_InvoiceAmount);
			AssertEquals(4, InvoiceHeader.InvoiceHeaderRefs.Count);
			AssertNotNull(InvoiceHeader.InvoiceHeaderRefs.Find(x => x.J2_ReferenceNumber == "CNT900800"));
			AssertNotNull(InvoiceHeader.InvoiceHeaderRefs.Find(x => x.J2_ReferenceNumber == "MB0001"));
			AssertNotNull(InvoiceHeader.InvoiceHeaderRefs.Find(x => x.J2_ReferenceNumber == "HB0002"));
			AssertNotNull(InvoiceHeader.InvoiceHeaderRefs.Find(x => x.J2_ReferenceNumber == "SH0003"));
		}

		public void TestExportConsigneeValue()
		{
			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeader();

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_FullName = "CargoWise pty ltd";

			invoiceHeader.JZ_OH_Buyer = importer.PK;

			ValueObjectExportContext context = new ValueObjectExportContext(new NotificationBuffer());
			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoiceHeader, context);

			AssertEquals(true, xmlInvoiceHeader.Consignee.IsSpecified);
			AssertEquals("Consignee is exported", "CargoWise pty ltd", xmlInvoiceHeader.Consignee.OrganisationDetails.Name);
		}

		public void TestExportInvoiceHeader_InvoiceNumber()
		{
			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeaderWithTestData();
			invoiceHeader.JZ_InvoiceNumber = "INVOICENUMBER";

			JobDec.JE_MessageType = "IMP";

			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("INVOICENUMBER", xmlInvoiceHeader.InvoiceNumber);
		}

		public void TestExportInvoiceLineCharges()
		{
			BaseJobComInvoiceHeader invHead = InvoiceHeader;
			BaseJobComInvoiceLine invLine = invHead.JobComInvoiceLines.AddNew();

			BaseJobComInvHeaderCharge invLineCharge = invLine.Charges.AddNew("OFT", 10.23m, Core.Constants.CurrencyCodes.SouthAfrica);
			invLineCharge.J7_IsDutiable = false;
			invLineCharge.J7_IsGSTApplicable = false;
			invLineCharge.J7_IsIncludedInITOT = false;
			invLineCharge.J7_IsNotIncludedInInvoice = true;

			invLineCharge = invLine.Charges.AddNew("COM", 75.45m, Core.Constants.CurrencyCodes.Lao);
			invLineCharge.J7_IsDutiable = true;
			invLineCharge.J7_IsGSTApplicable = true;
			//This flag for lines cannot be set by users
			//InvLineCharge.J7_IsIncludedInITOT = true;
			invLine = invHead.JobComInvoiceLines.AddNew();
			invLine.Charges.AddNew("XYZ", 99.99m, Core.Constants.CurrencyCodes.SolomonIslands);

			Xsd.InvoiceHeader xmlInvHead = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invHead, new ValueObjectExportContext(new NotificationBuffer()));

			Xsd.InvoiceLine xmlInvLine1 = xmlInvHead.InvoiceLines[0];
			Xsd.InvoiceLine xmlInvLine2 = xmlInvHead.InvoiceLines[1];

			AssertEquals(2, xmlInvLine1.Charges.Count);
			AssertEquals(1, xmlInvLine2.Charges.Count);

			Xsd.InvoiceCharge charge1 = xmlInvLine1.Charges[0];
			AssertEquals("OFT", charge1.ChargeType);
			AssertEquals(10.23m, charge1.ChargeValue.Value);
			AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, charge1.ChargeValue.CurrencyCode);
			AssertEquals(Xsd.TrueFalse.@false, charge1.GstApplies);
			AssertEquals(Xsd.TrueFalse.@false, charge1.DutyApplies);
			AssertEquals(Xsd.TrueFalse.@false, charge1.IsIncludedInTotal);
			AssertEquals(Xsd.TrueFalse.@false, charge1.IsIncludedInInvoice);

			Xsd.InvoiceCharge charge2 = xmlInvLine1.Charges[1];
			AssertEquals("COM", charge2.ChargeType);
			AssertEquals(75.45m, charge2.ChargeValue.Value);
			AssertEquals(Core.Constants.CurrencyCodes.Lao, charge2.ChargeValue.CurrencyCode);
			AssertEquals(true, charge2.GstAppliesSpecified);
			AssertEquals(Xsd.TrueFalse.@true, charge2.GstApplies);
			AssertEquals(true, charge2.DutyAppliesSpecified);
			AssertEquals(Xsd.TrueFalse.@true, charge2.DutyApplies);
			AssertEquals(true, charge2.IsIncludedInTotalSpecified);

			Xsd.InvoiceCharge charge3 = xmlInvLine2.Charges[0];
			AssertEquals("XYZ", charge3.ChargeType);
			AssertEquals(99.99m, charge3.ChargeValue.Value);
			AssertEquals(Core.Constants.CurrencyCodes.SolomonIslands, charge3.ChargeValue.CurrencyCode);
		}

		public void TestExportInvoiceHeaderValues()
		{
			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeaderWithTestData();

			invoiceHeader.JZ_InvoiceAmount = 100;
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2005, 3, 16, 10, 43, 0);
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2005, 3, 16, 10, 44, 0);
			invoiceHeader.JZ_GroupInvoice = false;
			invoiceHeader.JZ_IncoTerm = "Inc";
			invoiceHeader.JZ_Weight = 10;
			invoiceHeader.JZ_WeightUQ = "KG";
			invoiceHeader.JZ_Volume = 20;
			invoiceHeader.JZ_VolumeUQ = "VO";

			if (invoiceHeader.IsAttachedToPersistentDeclaration)
			{
				invoiceHeader.JZ_NoOfPacks = 10;
			}

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			invoiceDataAdapter.ExportToValueObject(invoiceHeader, xmlInvoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals((ZDecimal)100, xmlInvoiceHeader.InvoiceAmount.Value);
			AssertEquals(true, xmlInvoiceHeader.ExchangeRateSpecified);
			AssertEquals(invoiceHeader.JZ_InvoiceCurrExRate, xmlInvoiceHeader.ExchangeRate);
			AssertEquals(new DateTime(2005, 3, 16), xmlInvoiceHeader.InvoiceDate);
			Assert(xmlInvoiceHeader.InvoiceDate.IsValid);
			AssertEquals(new DateTime(2005, 3, 16, 10, 44, 0), xmlInvoiceHeader.ValuationDate);
			Assert(xmlInvoiceHeader.ValuationDate.IsValid);
			AssertEquals(Xsd.TrueFalse.@false, xmlInvoiceHeader.IsGroupInvoice);
			Assert(xmlInvoiceHeader.IsGroupInvoiceSpecified);
			AssertEquals("Inc", xmlInvoiceHeader.Incoterm);
			AssertEquals((decimal)10, xmlInvoiceHeader.Weight.Value);
			AssertEquals("KG", xmlInvoiceHeader.Weight.DimensionType);
			AssertEquals((decimal)20, xmlInvoiceHeader.Volume.Value);
			AssertEquals("VO", xmlInvoiceHeader.Volume.DimensionType);

			if (invoiceHeader.IsAttachedToPersistentDeclaration)
			{
				AssertEquals((decimal)10, xmlInvoiceHeader.Packages);
			}
		}

		public void TestExportInvoiceHeaderValues_AddCustomsDetailsNotNull()
		{
			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			invoiceDataAdapter.ExportToValueObject(invoiceHeader, xmlInvoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));

			AssertNotNull("AddCustomsDetails", xmlInvoiceHeader.AddCustomsDetails);
			AssertRightNumberOfAddCustomsDetailsAreGeneratedFromAnEmptyInvoiceHeader(xmlInvoiceHeader.AddCustomsDetails.Count);
		}

		public void TestExportInvoiceLines()
		{
			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeaderWithTestData();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines[0];
			invoiceLine.JI_Description = "Description 1";
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "Description 2";

			Xsd.InvoiceHeader xmlInvoice = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(2, xmlInvoice.InvoiceLines.Count);
		}

		public void TestExportInvoiceLineSummary()
		{
			RefCurrency aUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			RefCurrency localCurrency = testDec.LocalCurrency;
			testDec.JE_ExportDate = new ZDateTime(2005, 1, 1);
			SetExchangeRate(testDec.JE_ExportDate, testDec.JE_ExportDate, 0.5m, aUD, "CUS");
			CusEntryHeader entryHeader;
			if (testDec.CustomsEntryHeaders.Count == 0)
			{
				entryHeader = testDec.CustomsEntryHeaders.AddNew();
			}
			else
			{
				entryHeader = testDec.CustomsEntryHeaders[0];
			}
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			CusEntryLineFee duty = entryLine.Fees.AddNew();
			duty.CF_ChargeAmount = 1000m;
			duty.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;

			CusEntryLineFee duty2 = entryLine.Fees.AddNew();
			duty2.CF_ChargeAmount = 500m;
			duty2.CF_ChargeType = testDec.GSTOrVATCode;

			CusEntryLineFee duty3 = entryLine.Fees.AddNew();
			duty3.CF_ChargeAmount = 300m;
			duty3.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred;

			entryLine.CL_DutyPercent = 5.5m;

			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = aUD.RX_Code;

			entryLine.CL_CustomsValue = 20000m;

			BaseJobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_CL = entryLine.PK;
			line.JI_LinePrice = 10000m;
			DecorateLineToHaveDuty(line, 5000m);

			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine.PK;
			line2.JI_LinePrice = 10000m;
			DecorateLineToHaveDuty(line2, 5000m);

			Assert("PreCondition: Should have Duty on line", line.JI_Calc_DutyAmount != 0);

			Xsd.InvoiceHeader xmlInvoice = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoice, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.InvoiceLineSummary summary = xmlInvoice.InvoiceLines[0].Summary;

			AssertEquals(line.JI_Calc_CIF, summary.CIF.Value);
			AssertEquals(aUD.RX_Code, summary.CIF.CurrencyCode);
			ZDecimal expectedAmount = line.CurrencyConverter.ConvertExact(new Money(summary.CIF.Value, aUD), localCurrency).Amount;
			AssertEquals(expectedAmount, summary.CIFInLocalCurr.Value);

			AssertEquals(line.JI_Calc_FOB, summary.FOB.Value);
			AssertEquals(aUD.RX_Code, summary.FOB.CurrencyCode);
			expectedAmount = line.CurrencyConverter.ConvertExact(new Money(summary.FOB.Value, aUD), localCurrency).Amount;
			AssertEquals(expectedAmount, summary.FOBInLocalCurr.Value);

			AssertEquals(line.JI_Calc_FreightInInvoiceCurr, summary.Freight.Value);
			AssertEquals(aUD.RX_Code, summary.Freight.CurrencyCode);
			expectedAmount = line.CurrencyConverter.ConvertExact(new Money(summary.Freight.Value, aUD), localCurrency).Amount;
			AssertEquals(expectedAmount, summary.FreightInLocalCurr.Value);

			AssertEquals(line.JI_Calc_InsuranceInInvoiceCurr, summary.Insurance.Value);
			AssertEquals(aUD.RX_Code, summary.Insurance.CurrencyCode);
			expectedAmount = line.CurrencyConverter.ConvertExact(new Money(summary.Insurance.Value, aUD), localCurrency).Amount;
			AssertEquals(expectedAmount, summary.InsuranceInLocalCurr.Value);

			AssertEquals(line.JI_Calc_DutyAmount, summary.Duty.Value);
			AssertEquals(localCurrency.RX_Code, summary.Duty.CurrencyCode);
			AssertEquals(line.AdValoremDutyPercent, summary.DutyPercent);
			AssertEquals("DutyPercentIsSpecified", true, summary.DutyPercentSpecified);

			AssertEquals(line.JI_Calc_GSTVATAmount + line.JI_Calc_GSTVATDeferred, summary.GST.Value);
			AssertEquals(localCurrency.RX_Code, summary.GST.CurrencyCode);
		}

		public void TestExportInvoiceLines_DoesntReturnNull()
		{
			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			Xsd.InvoiceHeader xmlInvoice = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));

			AssertNotNull("InvoiceLines", xmlInvoice.InvoiceLines);
			AssertEquals("InvoiceLines.Count", 0, xmlInvoice.InvoiceLines.Count);
		}

		public void TestExportInvoiceCharges()
		{
			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeaderWithTestData();
			invoiceHeader.Charges.RemoveAll();

			Xsd.InvoiceHeader xmlInvoice = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.InvoiceChargeCollection xmlCharges = xmlInvoice.InvoiceCharges;
			AssertNull(xmlCharges);

			BaseInvoiceCharge invoiceCharge = invoiceHeader.Charges.AddNew();
			invoiceCharge.J7_Amount = 100;
			invoiceCharge.J7_ChargeType = "CHG";
			invoiceCharge = invoiceHeader.Charges.AddNew();
			invoiceCharge.J7_Amount = 110;
			invoiceCharge.J7_ChargeType = "BIL";

			xmlInvoice = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));
			xmlCharges = xmlInvoice.InvoiceCharges;
			AssertEquals(2, xmlCharges.Count);
			AssertEquals((decimal)100, xmlCharges[0].ChargeValue.Value);
			AssertEquals("CHG", xmlCharges[0].ChargeType);
			AssertEquals((decimal)110, xmlCharges[1].ChargeValue.Value);
			AssertEquals("BIL", xmlCharges[1].ChargeType);
		}

		public void TestExportInvoiceLineDetails()
		{
			BaseCusClassification classification = CreateTestCusClassification();

			BaseJobComInvoiceHeader invoice = GetInvoiceHeaderWithTestData();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines[0];

			invoiceLine.JI_CC = classification.PK;
			invoiceLine.JI_CountryOfOrigin = "NZ";
			invoiceLine.JI_CustomsUnitQty = "CUS";
			invoiceLine.JI_CustomsQuantity = 97;
			invoiceLine.JI_NetWeight = 138m;
			invoiceLine.JI_NetWeightUQ = "LB";
			invoiceLine.JI_ExtraInfoForClassification = "EXTENDED DESCRIPTION";
			invoiceLine.JI_Tariff = TariffNumber;

			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoice, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines[0];

			AssertEquals(97m, xmlInvoiceLine.CustomsInvoiceQty.Value);
			AssertEquals("CUS", xmlInvoiceLine.CustomsInvoiceQty.DimensionType);

			AssertEquals(TariffNumber, xmlInvoiceLine.LineClassification.TariffCode.Value);
			AssertEquals("CustomAttrib1", xmlInvoiceLine.CustomText1);
			AssertEquals("CustomAttrib2", xmlInvoiceLine.CustomText2);
			AssertEquals("CustomAttrib3", xmlInvoiceLine.CustomText3);
			AssertEquals("CustomAttrib4", xmlInvoiceLine.CustomText4);
			AssertEquals("CustomAttrib5", xmlInvoiceLine.CustomText5);
			AssertEquals("CustomAttrib6", xmlInvoiceLine.CustomText6);

			AssertEquals("CustomText1", xmlInvoiceLine.CustomTextField1);

			AssertEquals(false, xmlInvoiceLine.CustomFlag1);
			AssertEquals(true, xmlInvoiceLine.CustomFlag2);
			AssertEquals(false, xmlInvoiceLine.CustomFlag3);

			AssertEquals(ZDateTime.BrettsBirthday, xmlInvoiceLine.CustomDate1);
			AssertEquals(ZDateTime.BrettsBirthday, xmlInvoiceLine.CustomDate2);
			AssertEquals(ZDateTime.BrettsBirthday, xmlInvoiceLine.CustomDate3);

			AssertEquals((decimal)12.12, xmlInvoiceLine.CustomDecimal1);
			AssertEquals((decimal)13.13, xmlInvoiceLine.CustomDecimal2);
			AssertEquals((decimal)14.14, xmlInvoiceLine.CustomDecimal3);
			AssertEquals("PartAttrib1", xmlInvoiceLine.PartAttrib1);
			AssertEquals("PartAttrib2", xmlInvoiceLine.PartAttrib2);
			AssertEquals("PartAttrib3", xmlInvoiceLine.PartAttrib3);
			AssertEquals(36m, xmlInvoiceLine.InvoiceQty.Value);
			AssertEquals("Inv", xmlInvoiceLine.InvoiceQty.DimensionType);
			AssertEquals("NZ", xmlInvoiceLine.LineClassification.OriginOfGoods);
			AssertEquals(classification.CC_LookupCode, xmlInvoiceLine.LineClassification.TariffLookup);
			AssertEquals(classification.CC_TariffNum, xmlInvoiceLine.LineClassification.TariffCode.Value);
			AssertEquals(21m, xmlInvoiceLine.LinePrice.Value);
			AssertEquals("OrderNumber", xmlInvoiceLine.OrderNumber);
			AssertEquals(invoiceLine.JI_Description, xmlInvoiceLine.ProductDescription);
			AssertEquals(invoiceLine.IsExtendedCommercialDescriptionEnabled ? "EXTENDED DESCRIPTION" : "", xmlInvoiceLine.ExtendedProductDescription);
			AssertEquals("PartNo", xmlInvoiceLine.ProductNumber);
			AssertEquals(192m, xmlInvoiceLine.Volume.Value);
			AssertEquals("Vo", xmlInvoiceLine.Volume.DimensionType);
			AssertEquals(177m, xmlInvoiceLine.Weight.Value);
			AssertEquals("We", xmlInvoiceLine.Weight.DimensionType);
			AssertEquals(138m, xmlInvoiceLine.NetWeight.Value);
			AssertEquals("LB", xmlInvoiceLine.NetWeight.DimensionType);
		}

		public void TestAddExportEvent()
		{
			NotificationBuffer notification = new NotificationBuffer();
			BaseJobComInvoiceHeader invoice = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
			StmALog[] dataExportEvents = invoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("No DEX event should be added to invoice", 0, dataExportEvents.Length);

			invoiceDataAdapter.ExportToValueObject(invoice, new ValueObjectExportContext(notification));
			dataExportEvents = invoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to invoice", 1, dataExportEvents.Length);

			invoiceDataAdapter.ExportToValueObject(invoice, new ValueObjectExportContext(notification));
			dataExportEvents = invoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to invoice", 2, dataExportEvents.Length);
		}

		public void TestGetMatchedOrganisation()
		{
			Xsd.Organisation orgValue = new Xsd.Organisation();
			orgValue.OrganisationDetails = new Xsd.OrganisationDetail();
			orgValue.OrganisationDetails.Name = "ORGNAME";
			orgValue.OrganisationDetails.WebAddress = "www.input.edi.com.au";
			orgValue.EDICode = "ZZZORG";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ZGuid orgPK = new InvoiceDataTransferTool(false).GetMatchedOrganisation(orgValue, context);

			AssertEquals("ORGNAME", Factory.Load<OrgHeader>(orgPK).OH_FullName);
		}

		public void TestExportParentLineNumber()
		{
			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeaderWithTestData();
			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();

			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;

			BaseJobComInvoiceLine invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();

			Xsd.InvoiceHeader xmlInvHead = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(4, xmlInvHead.InvoiceLines.Count);

			Xsd.InvoiceLine xmlInvLine1 = xmlInvHead.InvoiceLines[0];
			Xsd.InvoiceLine xmlInvLine2 = xmlInvHead.InvoiceLines[1];
			Xsd.InvoiceLine xmlInvLine3 = xmlInvHead.InvoiceLines[2];
			Xsd.InvoiceLine xmlInvLine4 = xmlInvHead.InvoiceLines[3];

			AssertEquals((ZShort)0, xmlInvLine1.ParentInvoiceLineNumber);
			AssertEquals((ZShort)0, xmlInvLine2.ParentInvoiceLineNumber);
			AssertEquals((ZShort)2, xmlInvLine3.ParentInvoiceLineNumber);
			AssertEquals((ZShort)0, xmlInvLine4.ParentInvoiceLineNumber);
		}

		public void TestExportReferences()
		{
			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeader();
			JobComInvoiceHeaderRefs reference = invoiceHeader.InvoiceHeaderRefs.AddNew();
			reference.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
			reference.J2_ReferenceNumber = "CNT900800";

			reference = invoiceHeader.InvoiceHeaderRefs.AddNew();
			reference.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.MB;
			reference.J2_ReferenceNumber = "MB0001";

			reference = invoiceHeader.InvoiceHeaderRefs.AddNew();
			reference.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.HB;
			reference.J2_ReferenceNumber = "HB0002";

			reference = invoiceHeader.InvoiceHeaderRefs.AddNew();
			reference.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.SH;
			reference.J2_ReferenceNumber = "SH0003";

			ValueObjectExportContext context = new ValueObjectExportContext(new NotificationBuffer());
			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoiceHeader, context);

			AssertEquals(4, xmlInvoiceHeader.References.Count);
			AssertEquals(InvoiceHeaderRefsTypeList.Codes.CN, xmlInvoiceHeader.References[0].Type);
			AssertEquals("CNT900800", xmlInvoiceHeader.References[0].Value);
			AssertEquals(InvoiceHeaderRefsTypeList.Codes.MB, xmlInvoiceHeader.References[1].Type);
			AssertEquals("MB0001", xmlInvoiceHeader.References[1].Value);
			AssertEquals(InvoiceHeaderRefsTypeList.Codes.HB, xmlInvoiceHeader.References[2].Type);
			AssertEquals("HB0002", xmlInvoiceHeader.References[2].Value);
			AssertEquals(InvoiceHeaderRefsTypeList.Codes.SH, xmlInvoiceHeader.References[3].Type);
			AssertEquals("SH0003", xmlInvoiceHeader.References[3].Value);
		}

		protected virtual void AssertRightNumberOfAddCustomsDetailsAreGeneratedFromAnEmptyInvoiceHeader(int count)
		{
			AssertEquals("AddCustomsDetails.Count", 0, count);
		}

		protected virtual void DecorateLineToHaveDuty(BaseJobComInvoiceLine invoiceLine, ZDecimal dutyAmount)
		{
		}

		protected virtual ZString TariffNumber => "9999.99.99.99";

		protected RefCurrency AUD => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

		protected RefCurrency NZD => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");

		protected bool ItemInXmlAddInfoCollection(Xsd.AdditionalCustomsInformationCollection addCusInfos, string itemType)
			=> AddInfoDataTransferToolTest.ItemInXmlAddInfoCollection(addCusInfos, itemType);

		protected string GetXmlAddInfoValue(Xsd.AdditionalCustomsInformationCollection addCusInfos, string itemType)
			=> AddInfoDataTransferToolTest.GetXmlAddInfoValue(addCusInfos, itemType);

		protected virtual BaseJobComInvoiceHeader GetEmptyInvoiceHeader()
		{
			JobDec.MakeNonPersistent();

			BaseJobComInvoiceHeader invoiceHeader = JobDec.Invoices.AddNew();
			invoiceHeader.SetExchangeRateIfNotUserEntered();
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2005, 3, 14);
			invoiceHeader.JZ_IncoTerm = "FOB";

			return invoiceHeader;
		}

		protected virtual BaseJobComInvoiceHeader GetInvoiceHeaderWithTestData()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Supplier";
			organisation.OH_IsConsignee = true;
			organisation.OH_IsConsignor = true;
			organisation.OH_RL_NKClosestPort = "AUMEL";
			organisation.MiscServ.OM_LandedCostMarginPercent1 = 10m;
			organisation.MiscServ.OM_LandedCostMarginPercent1 = 20m;
			organisation.MiscServ.OM_LandedCostMarginPercent1 = 30m;

			OrgAddress address = organisation.Addresses.MainAddress;
			address.OA_Address1 = "7J1B7RFZ4WSH3LJTGUG7P55D1ABG3TMYMKFCPHQ2VSK4UDXQF0";
			organisation.OH_Code = "L1FZUT4BYOD8";

			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeader();
			ZString messageType = invoiceHeader.JZ_MessageType;

			invoiceHeader.JZ_OH_Supplier = organisation.PK;//this does change message type

			if (!invoiceHeader.IsAttachedToPersistentDeclaration)
			{
				invoiceHeader.JZ_MessageType = messageType;
			}

			if (invoiceHeader.IsAttachedToPersistentDeclaration)
			{
				invoiceHeader.JZ_NoOfPacks = 100;
			}

			invoiceHeader.JZ_InvoiceNumber = "INVOICENUMBER";
			invoiceHeader.JZ_InvoiceAmount = 57;
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(1998, 3, 4);

			invoiceHeader.JZ_IncoTerm = "Inc";
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2005, 3, 14);
			invoiceHeader.JZ_Volume = 159;
			invoiceHeader.JZ_VolumeUQ = "Vo";
			invoiceHeader.JZ_Weight = 168;
			invoiceHeader.JZ_WeightUQ = "We";

			BaseCusClassification classification = CreateTestCusClassification();

			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CC = classification.PK;
			invoiceLine.JI_PartNo = "PartNo";
			invoiceLine.JI_InvoiceQuantity = 36;
			invoiceLine.JI_InvoiceUQ = "Inv";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_LinePrice = 21;
			invoiceLine.JI_OrderNumber = "OrderNumber";
			invoiceLine.JI_Description = "Description";
			invoiceLine.JI_Volume = 192;
			invoiceLine.JI_VolumeUQ = "Vo";
			invoiceLine.JI_Weight = 177;
			invoiceLine.JI_WeightUQ = "We";
			invoiceLine.JI_CustomAttrib1 = "CustomAttrib1";
			invoiceLine.JI_CustomAttrib2 = "CustomAttrib2";
			invoiceLine.JI_CustomAttrib3 = "CustomAttrib3";
			invoiceLine.JI_CustomAttrib4 = "CustomAttrib4";
			invoiceLine.JI_CustomAttrib5 = "CustomAttrib5";
			invoiceLine.JI_CustomAttrib6 = "CustomAttrib6";
			invoiceLine.JI_CustomTextBlob1 = "CustomText1";
			invoiceLine.JI_CustomFlag1 = false;
			invoiceLine.JI_CustomFlag2 = true;
			invoiceLine.JI_CustomFlag3 = false;
			invoiceLine.JI_CustomDate1 = ZDateTime.BrettsBirthday;
			invoiceLine.JI_CustomDate2 = ZDateTime.BrettsBirthday;
			invoiceLine.JI_CustomDate3 = ZDateTime.BrettsBirthday;
			invoiceLine.JI_CustomDecimal1 = 12.12;
			invoiceLine.JI_CustomDecimal2 = 13.13;
			invoiceLine.JI_CustomDecimal3 = 14.14;
			invoiceLine.JI_PartAttrib1 = "PartAttrib1";
			invoiceLine.JI_PartAttrib2 = "PartAttrib2";
			invoiceLine.JI_PartAttrib3 = "PartAttrib3";

			BaseInvoiceCharge invoiceCharge = invoiceHeader.Charges.AddNew();
			invoiceCharge.J7_ChargeType = "Cha";
			invoiceCharge.J7_Amount = 32;
			invoiceCharge.J7_RX_NKCurrency = "AUD";
			invoiceCharge.J7_IsGSTApplicable = false;
			invoiceCharge.J7_IsDutiable = false;
			invoiceCharge.J7_IsIncludedInITOT = false;

			return invoiceHeader;
		}

		protected virtual BaseCusClassification CreateTestCusClassification()
		{
			BaseCusClassification result = Factory.New<BaseCusClassification>();

			result.CC_Description = "TEST FOR INVOICE XML DATA ADAPTER";
			result.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			result.CC_IsActive = true;
			result.CC_LookupCode = "TARIFF";
			result.CC_TariffNum = TariffNumber;

			return result;
		}

		protected BaseJobComInvoiceHeader InvoiceHeader => fInvoiceHeader ?? (fInvoiceHeader = GetInvoiceHeader());
		BaseJobComInvoiceHeader fInvoiceHeader;

		protected BaseJobDeclaration JobDec => InvoiceHeader.JobDeclaration;

		protected virtual BaseJobDeclaration GetJobDeclaration() => Factory.New<BaseJobDeclaration>();

		protected virtual BaseJobComInvoiceHeader GetInvoiceHeader() => GetJobDeclaration().Invoices.AddNew();

		protected virtual IValueObjectDataAdapter GetInvoiceValueObjectDataAdapter() => GetNewBizObjXmlDataAdapter();

		protected IValueObjectDataAdapter invoiceDataAdapter;
		protected ValueObjectImportContext importContext;

		protected override BaseJobComInvoiceHeader NewBusinessObject() => GetInvoiceHeader();

		protected override void SetUp()
		{
			base.SetUp();

			invoiceDataAdapter = GetInvoiceValueObjectDataAdapter();

			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			importContext = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
		}

		void SetExchangeRate(ZDateTime startDate, ZDateTime endDate, ZDecimal exchangeRate, RefCurrency foreignCurrency, string exRateType)
		{
			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, foreignCurrency.RX_Code);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_GC, Env.CurrentCompany.PK);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, exRateType);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThan, startDate.AddDays(1));
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);

			RefExchangeRate exchangeRateDuty = Factory.LoadTop1<RefExchangeRate>(sQLFilter);
			if (exchangeRateDuty == null)
			{
				exchangeRateDuty = Factory.New<RefExchangeRate>();
				exchangeRateDuty.RE_ExpiryDate = endDate;
				exchangeRateDuty.RE_ExRateType = exRateType;
				exchangeRateDuty.RE_GC = Env.CurrentCompany.PK;
				exchangeRateDuty.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
				exchangeRateDuty.RE_StartDate = startDate;
				exchangeRateDuty.RE_SellRate = exchangeRate;
				Factory.Save();
			}
			else
			{
				exchangeRateDuty.RE_SellRate = exchangeRate;
			}
		}
	}
}
