using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class NX5105Declaration_GoodsShipmentAbstractTest<TGoodsShipment> : TestCaseWithFactory
		where TGoodsShipment : IGoodsShipment
	{
		#region Exit Date Time
		[ExpectNoExceptions]
		public void TestGoodsShipment_ExitDateTime()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			var goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			entryHeader.Declaration.JE_ExportDate = new ZDateTime(2019, 01, 01);
			NUnit.Framework.Assert.That(goodsShipment.ExitDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 01, 01)), "GoodsShipment.ExitDateTime should be");
		}

		#endregion
		#region Item Charge Amount
		[TestDate(2016, 01, 01)]
		[ExpectNoExceptions]
		public void TestGoodsShipment_ItemChargeAmount()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, CurrencyCodes.UnitedStates, 10m, new ZDateTime(2016, 01, 01), ExchangeRateTypes.Code.CustomsRate);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, CurrencyCodes.EuropeanUnion, 20m, new ZDateTime(2016, 01, 01), ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = "FOB";
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_PartNo = "PARTNO1";
			invoiceLine1.JI_InvoiceQuantity = 4;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			var invoiceChargeCollection = invoice.Charges;
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_PartNo = "PARTNO1";
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			_ = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, CurrencyCodes.UnitedStates);
			_ = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 90m, CurrencyCodes.UnitedStates);
			_ = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 100m, CurrencyCodes.EuropeanUnion);
			var invCharge = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 60m, CurrencyCodes.UnitedStates);
			invCharge.J7_IsIncludedInITOT = true;
			declaration.ResumeApportionment();
			_ = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(entryHeader.CustomsValue, NUnit.Framework.Is.EqualTo(34300m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsShipment.ItemChargeAmount, NUnit.Framework.Is.EqualTo(3000m).Using(CustomComparers.TypeComparison));
		}

		#endregion
		#region Total CIF Amount
		[TestDate(2016, 01, 01)]
		[ExpectNoExceptions]
		public void TestGoodsShipment_TotalCIFAmount()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 10m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.EuropeanUnion, 20m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_InvoiceAmount = 700m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_IncoTerm = "FOB";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			entryLine.ResetTotalsAndCachedValues();
			entryLine.RefreshInvoiceLines();
			entryHeader.ResetTotalsAndCachedValues();
			entryHeader.ResetInvoiceHeadersAndLines();
			NUnit.Framework.Assert.That(goodsShipment.TotalCIFAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "GoodsShipment.TotalCIFAmount should be");
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_InvoiceQuantity = 4;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			entryLine.ResetTotalsAndCachedValues();
			entryLine.RefreshInvoiceLines();
			entryHeader.ResetTotalsAndCachedValues();
			entryHeader.ResetInvoiceHeadersAndLines();
			var invoiceChargeCollection = invoiceHeader.Charges;
			_ = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, CurrencyCodes.UnitedStates);
			_ = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 90m, CurrencyCodes.UnitedStates);
			_ = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 100m, CurrencyCodes.EuropeanUnion);
			var invCharge = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 60m, CurrencyCodes.UnitedStates);
			invCharge.J7_IsIncludedInITOT = true;
			declaration.ResumeApportionment();
			_ = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.TotalCIFAmount, NUnit.Framework.Is.EqualTo(11300M).Using(CustomComparers.TypeComparison), "GoodsShipment.TotalCIFAmount should be");
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			entryLine.ResetTotalsAndCachedValues();
			entryLine.RefreshInvoiceLines();
			entryHeader.ResetTotalsAndCachedValues();
			entryHeader.ResetInvoiceHeadersAndLines();
			goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			declaration.ResumeApportionment();
			_ = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			NUnit.Framework.Assert.That(goodsShipment.TotalCIFAmount, NUnit.Framework.Is.EqualTo(14300m).Using(CustomComparers.TypeComparison), "GoodsShipment.TotalCIFAmount should be");
		}

		#endregion
		#region Consignee 
		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignee()
		{
			SetupOrganizations();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			declaration.JE_OH_Consignee = ZGuid.Empty;
			var goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.Consignee, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			declaration.ConsigneeDocumentaryAddress.OrganisationPK = organization1.PK;
			entryInstruction.CEI_Style = ZString.Empty;
			entryInstruction.CEI_OA_Warehouse2 = organization1.MainAddress.PK;
			NUnit.Framework.Assert.That(goodsShipment.Consignee.ID, NUnit.Framework.Is.EqualTo("123465789").Using(CustomComparers.TypeComparison), "Consignee.ID should be");
			NUnit.Framework.Assert.That(goodsShipment.Consignee.Name, NUnit.Framework.Is.EqualTo("OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "Consignee.Name should be");
			NUnit.Framework.Assert.That(goodsShipment.Consignee.ChineseName, NUnit.Framework.Is.EqualTo("TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "Consignee.ChineseName should be");
			NUnit.Framework.Assert.That(goodsShipment.Consignee.Address.Line, NUnit.Framework.Is.EqualTo("ADDRESS 1 ADDRESS 2 TAIWAN").Using(CustomComparers.TypeComparison), "Consignee.Address.Line should be");
			NUnit.Framework.Assert.That(goodsShipment.Consignee.Address.ChineseLine, NUnit.Framework.Is.EqualTo("TW OTA ADDRESS 1TW OTA ADDRESS 2").Using(CustomComparers.TypeComparison), "Consignee.Address.ChineseLine should be");
			organization1.OH_RL_NKClosestPort = "US";
			NUnit.Framework.Assert.That(goodsShipment.Consignee, NUnit.Framework.Is.Not.EqualTo(default(IPartyDetails)));
		}

		OrgHeader organization1;
		OrgHeader organization2;
		OrgCusCode vatCusCode;
		OrgCusCode ccpCusCode;
		OrgCusCode pasCusCode;
		OrgCusCode pidCusCode;
		void SetupOrganizations()
		{
			organization1 = Factory.New<OrgHeader>();
			organization1.Addresses.RemoveAndDeleteAll();
			organization1.OH_Code = "Org1";
			organization1.OH_RL_NKClosestPort = "TW";
			var contact = organization1.Contacts.AddNew();
			contact.OC_ContactName = "Contact Name";
			var address1 = organization1.Addresses[0];
			address1.OA_RN_NKCountryCode = "TW";
			address1.OA_CompanyNameOverride = "OVERRIDEN COMPANY NAME";
			address1.OA_Language = "EN";
			address1.OA_Address1 = "ADDRESS 1";
			address1.OA_Address2 = "ADDRESS 2";
			address1.OA_Phone = "PHONE1";
			var entranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			entranslatedAddress1.OTA_Language = "EN";
			entranslatedAddress1.OTA_Address1 = "EN OTA ADDRESS 1";
			entranslatedAddress1.OTA_Address2 = "EN OTA ADDRESS 2";
			var zhTWtranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "TW OVERRIDEN COMPANY NAME";
			zhTWtranslatedAddress1.OTA_Address1 = "TW OTA ADDRESS 1";
			zhTWtranslatedAddress1.OTA_Address2 = "TW OTA ADDRESS 2";
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.AEO, "123465789", "TW");
			vatCusCode = organization1.CustomsCodes.AddNew();
			vatCusCode.OK_RN_NKCodeCountry = "TW";
			vatCusCode.OK_CodeType = "VAT";
			vatCusCode.OK_CustomsRegNo = "123465789";
			ccpCusCode = address1.CustomsCodes.AddNew();
			ccpCusCode.OK_RN_NKCodeCountry = "TW";
			ccpCusCode.OK_CodeType = "CCP";
			ccpCusCode.OK_CustomsRegNo = "987654321";
			pasCusCode = organization1.CustomsCodes.AddNew();
			pasCusCode.OK_RN_NKCodeCountry = "TW";
			pasCusCode.OK_CodeType = "PAS";
			pasCusCode.OK_CustomsRegNo = "PASREGNO";
			pidCusCode = organization1.CustomsCodes.AddNew();
			pidCusCode.OK_RN_NKCodeCountry = "TW";
			pidCusCode.OK_CodeType = "PID";
			pidCusCode.OK_CustomsRegNo = "PIDREGNO";
			organization2 = Factory.NewWithValidTestData<OrgHeader>();
			organization2.OH_Code = "Org2";
			organization2.OH_RL_NKClosestPort = "TW";
			Factory.Save();
		}

		#endregion
		#region Consignment
		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			var goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.Consignment.GetType(), NUnit.Framework.Is.EqualTo(typeof(NX5105GoodsShipment_Consignment)));
		}

		#endregion
		#region Consignor
		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignor()
		{
			SetupOrganizations();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			declaration.JE_OH_Exporter = ZGuid.Empty;
			var goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.Consignor, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			declaration.ConsignorDocumentaryAddress.OrganisationPK = organization2.PK;
			entryInstruction.CEI_Style = "D5";
			entryInstruction.CEI_OA_Warehouse = organization1.MainAddress.PK;
			organization2.OH_RL_NKClosestPort = "US";
			NUnit.Framework.Assert.That(goodsShipment.Consignor, NUnit.Framework.Is.Not.EqualTo(default(IPartyDetails)));
		}

		#endregion
		#region Customs Valuation
		[ExpectNoExceptions]
		public void TestGoodsShipment_CustomsValuation()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			var goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.CustomsValuation, NUnit.Framework.Is.TypeOf(typeof(NX5105GoodsShipment_CustomsValuation)));
		}

		#endregion
		#region Duty Tax Fees
		[ExpectNoExceptions]
		public void TestGoodsShipment_DutyTaxFees()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			var line = entryHeader.MergedLines.AddNew();
			var invoiceLine = line.InvoiceLines.AddNew() as JobComInvoiceLine;
			var charge = entryHeader.Charges.AddNew();
			charge.C1_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.DEF;
			charge.C1_ChargeAmount = 66M;
			charge = entryHeader.Charges.AddNew();
			charge.C1_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
			charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.DEF;
			charge.C1_ChargeAmount = 77M;
			charge = entryHeader.Charges.AddNew();
			charge.C1_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			charge.C1_ChargeAmount = 88M;
			charge = entryHeader.Charges.AddNew();
			charge.C1_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
			charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			charge.C1_ChargeAmount = 99M;
			var goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.DutyTaxFees.Cast<GoodsShipmentDutyTaxFeeWrapper>().ToList(), NUnit.Framework.Is.TypeOf(typeof(List<GoodsShipmentDutyTaxFeeWrapper>)));
			NUnit.Framework.Assert.That(goodsShipment.DutyTaxFees.Count(), NUnit.Framework.Is.EqualTo(2));
			var dutyTaxFee = goodsShipment.DutyTaxFees.SingleOrDefault(x => x.AdValoremTaxBaseAmount == 143M);
			NUnit.Framework.Assert.That(dutyTaxFee, NUnit.Framework.Is.Not.EqualTo(default(IGoodsShipmentDutyTaxFee)));
			NUnit.Framework.Assert.That(dutyTaxFee.TypeCode, NUnit.Framework.Is.EqualTo("A19").Using(CustomComparers.TypeComparison));
			dutyTaxFee = goodsShipment.DutyTaxFees.SingleOrDefault(x => x.AdValoremTaxBaseAmount == 187M);
			NUnit.Framework.Assert.That(dutyTaxFee, NUnit.Framework.Is.Not.EqualTo(default(IGoodsShipmentDutyTaxFee)));
			NUnit.Framework.Assert.That(dutyTaxFee.TypeCode, NUnit.Framework.Is.EqualTo("A10").Using(CustomComparers.TypeComparison));
			entryHeader.Charges.RemoveAndDeleteAll();
			goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.DutyTaxFees.Count(), NUnit.Framework.Is.EqualTo(1));
			dutyTaxFee = goodsShipment.DutyTaxFees.Single();
			NUnit.Framework.Assert.That(dutyTaxFee.TypeCode, NUnit.Framework.Is.EqualTo("A10").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(dutyTaxFee.AdValoremTaxBaseAmount, NUnit.Framework.Is.EqualTo(ZDecimal.Zero));
		}

		#endregion
		#region Government Agency Goods Items
		[ExpectNoExceptions]
		public virtual void TestGoodsShipment_GovernmentAgencyGoodsItems()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			CreateEntryLineWithInvoiceLine(entryHeader, 3, invoiceHeader, "22");
			CreateEntryLineWithInvoiceLine(entryHeader, 2, invoiceHeader, "11");
			CreateEntryLineWithInvoiceLine(entryHeader, 1, invoiceHeader, "11");
			CreateEntryLineWithInvoiceLine(entryHeader, 4, invoiceHeader, "22");
			IGoodsShipment goodsShipment = new NX5105Declaration_GoodsShipment(entryHeader, null, null, false);
			var goodsItems = goodsShipment.GovernmentAgencyGoodsItems.Cast<IGovernmentAgencyGoodsItem>().ToList();
			NUnit.Framework.Assert.That(goodsItems.Count, NUnit.Framework.Is.EqualTo(4));
			for (int i = 0; i < goodsItems.Count; ++i)
			{
				NUnit.Framework.Assert.That((goodsItems[i] as NX5105GoodsShipment_GovernmentAgencyGoodsItem).EntryLine.CL_LineNumber, NUnit.Framework.Is.EqualTo(i + 1).Using(CustomComparers.TypeComparison));
			}

			NUnit.Framework.Assert.That(goodsItems[0].Commodity.Description, NUnit.Framework.Is.EqualTo("11").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsItems[1].Commodity.Description, NUnit.Framework.Is.EqualTo("11").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsItems[2].Commodity.Description, NUnit.Framework.Is.EqualTo("22").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsItems[3].Commodity.Description, NUnit.Framework.Is.EqualTo("22").Using(CustomComparers.TypeComparison));
			goodsShipment = new NX5105Declaration_GoodsShipment(entryHeader, null, null, true);
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.First(), NUnit.Framework.Is.TypeOf<NX5105CMGovernmentAgencyGoodsItem>());
		}

		CusEntryLine CreateEntryLineWithInvoiceLine(CusEntryHeader entryHeader, ZShort lineNumber, JobComInvoiceHeader invoiceHeader, ZString grouping)
		{
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = lineNumber;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Group = grouping;
			invoiceLine.JI_CL = entryLine.PK;
			return entryLine;
		}

		#endregion
		#region Notify Party
		[ExpectNoExceptions]
		public void TestGoodsShipment_NotifyParty()
		{
			SetupOrganizations();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			declaration.JE_OH_NotifyParty = organization1.PK;
			var goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty.Name, NUnit.Framework.Is.EqualTo("OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "NotifyParty.Name should be");
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty.ChineseName, NUnit.Framework.Is.EqualTo("TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "NotifyParty.ChineseName should be");
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty.Address.Line, NUnit.Framework.Is.EqualTo("ADDRESS 1 ADDRESS 2 TAIWAN").Using(CustomComparers.TypeComparison), "NotifyParty.Address.Line should be");
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty.Address.ChineseLine, NUnit.Framework.Is.EqualTo("TW OTA ADDRESS 1TW OTA ADDRESS 2").Using(CustomComparers.TypeComparison), "NotifyParty.Address.ChineseLine should be");
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty.ID, NUnit.Framework.Is.EqualTo("123465789").Using(CustomComparers.TypeComparison), "NotifyParty.ID should be");
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "NotifyParty.TypeCode should be");
			organization1.MainAddress.CustomsCodes.Delete(vatCusCode);
			goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty.ID, NUnit.Framework.Is.EqualTo("NOPASREGNO").Using(CustomComparers.TypeComparison), "NotifyParty.ID should be");
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "NotifyParty.TypeCode should be");
			organization1.MainAddress.CustomsCodes.Delete(pasCusCode);
			goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty.ID, NUnit.Framework.Is.EqualTo("PIDREGNO").Using(CustomComparers.TypeComparison), "NotifyParty.ID should be");
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison), "NotifyParty.TypeCode should be");
		}

		#endregion
		#region Seller
		[ExpectNoExceptions]
		public void TestGoodsShipment_Seller()
		{
			SetupOrganizations();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "F2";
			entryInstruction.CEI_OA_Warehouse = organization1.MainAddress.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			declaration.JE_OH_Supplier = organization1.PK;
			declaration.SupplierDocumentaryAddress.OrganisationPK = organization1.PK;
			IGoodsShipment goodsShipment = new NX5105Declaration_GoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.Seller.ID, NUnit.Framework.Is.EqualTo("123465789").Using(CustomComparers.TypeComparison), "Seller.ID should be");
			NUnit.Framework.Assert.That(goodsShipment.Seller.Name, NUnit.Framework.Is.EqualTo("OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "Seller.Name should be");
			NUnit.Framework.Assert.That(goodsShipment.Seller.ChineseName, NUnit.Framework.Is.EqualTo("TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "Seller.ChineseName should be");
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2Address = organization2.Addresses.AddNew();
			organization2Address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "568", "TW");
			entryInstruction.CEI_OA_Warehouse = organization2Address.PK;
			entryInstruction.CEI_Style = "F5";
			goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.Seller.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "Seller.TypeCode should be");
			organization1.MainAddress.CustomsCodes.Delete(vatCusCode);
			goodsShipment = new NX5105Declaration_GoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.Seller.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "Seller.TypeCode should be");
			organization1.MainAddress.CustomsCodes.Delete(pasCusCode);
			goodsShipment = new NX5105Declaration_GoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.Seller.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison), "Seller.TypeCode should be");
			NUnit.Framework.Assert.That(goodsShipment.Seller.Address.CountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison), "Seller.Address.CountryCode should be");
			NUnit.Framework.Assert.That(goodsShipment.Seller.Address.Line, NUnit.Framework.Is.EqualTo("ADDRESS 1 ADDRESS 2 TAIWAN").Using(CustomComparers.TypeComparison), "Seller.Address.Line should be");
			NUnit.Framework.Assert.That(goodsShipment.Seller.Address.ChineseLine, NUnit.Framework.Is.EqualTo("TW OTA ADDRESS 1TW OTA ADDRESS 2").Using(CustomComparers.TypeComparison), "Seller.Address.ChineseLine should be");
			NUnit.Framework.Assert.That(goodsShipment.Seller.Communications.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("PHONE1").Using(CustomComparers.TypeComparison), "Seller.Communications[0].ID should be");
			NUnit.Framework.Assert.That(goodsShipment.Seller.Communications.ElementAt(0).TypeID, NUnit.Framework.Is.EqualTo("TE").Using(CustomComparers.TypeComparison), "Seller.Communications[0].TypeID should be");
			declaration.SupplierDocumentaryAddress.E2_Contact = "Glendy Guan";
			NUnit.Framework.Assert.That(goodsShipment.Seller.ContactName, NUnit.Framework.Is.EqualTo("Glendy Guan").Using(CustomComparers.TypeComparison), "Seller.ContactName should be");
			NUnit.Framework.Assert.That(goodsShipment.Seller.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-123465789").Using(CustomComparers.TypeComparison), "Seller.LPCOAuthorizedParty.ID should be");
		}

		#endregion
		#region Trade Terms Condition Code
		[ExpectNoExceptions]
		public void TestGoodsShipment_TradeTermsConditionCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryHeader.CH_DeclarationIncoterm = "CIF";
			var goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.TradeTermsConditionCode, NUnit.Framework.Is.EqualTo("CIF").Using(CustomComparers.TypeComparison), "GoodsShipment.TradeTermsConditionCode should be");
			entryHeader.CH_DeclarationIncoterm = "FOB";
			NUnit.Framework.Assert.That(goodsShipment.TradeTermsConditionCode, NUnit.Framework.Is.EqualTo("FOB").Using(CustomComparers.TypeComparison), "GoodsShipment.TradeTermsConditionCode should be");
		}

		#endregion
		#region UCR
		[ExpectNoExceptions]
		public void TestGoodsShipment_UCR()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var goodsShipment = GetGoodsShipment(entryHeader, null, null, false);
			entryInstruction.UCRNumber = ZString.Empty;
			NUnit.Framework.Assert.That(goodsShipment.UCR, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "GoodsShipment.UCR should be");
			entryInstruction.UCRNumber = "123456";
			NUnit.Framework.Assert.That(goodsShipment.UCR, NUnit.Framework.Is.EqualTo("123456").Using(CustomComparers.TypeComparison), "GoodsShipment.UCR should be");
		}

		#endregion
		#region AdditionalDocuments

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestGoodsShipment_AdditionalDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var doc1 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var doc2 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"), Core.Constants.FileFormats.XLS);
			var messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			var docLine1 = messageSendingObject.SupportingDocuments.AddNew();
			docLine1.EDoc = doc1.UniqueKey;
			var docLine2 = messageSendingObject.SupportingDocuments.AddNew();
			docLine2.EDoc = doc2.UniqueKey;
			var goodsShipment = GetGoodsShipment(entryHeader, messageSendingObject.SupportingDocuments, messageSendingObject.GetAllEDocs(), false);
			NUnit.Framework.Assert.That(goodsShipment.AdditionalDocuments, NUnit.Framework.Is.Not.EqualTo(default(IEnumerable<IAdditionalDocument>)));
			NUnit.Framework.Assert.That(goodsShipment.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(2));
		}

		#endregion
		protected CusEntryHeader GetEntryHeaderWithMinimumData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			return entryHeader;
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				GetGoodsShipment(null, null, null, false);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				GetGoodsShipment(GetEntryHeaderWithMinimumData(), null, null, false);
			}

			);
		}

		[ExpectNoExceptions]
		public void TestCheckNotApplicableProperties()
		{
			var goodsShipment = GetGoodsShipment(GetEntryHeaderWithMinimumData(), null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.AdditionalDocuments, NUnit.Framework.Is.Not.EqualTo(default(IEnumerable<IAdditionalDocument>)));
			NUnit.Framework.Assert.That(goodsShipment.DeliveryDestinationName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsShipment.Buyer, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			NUnit.Framework.Assert.That(goodsShipment.Exporter, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			NUnit.Framework.Assert.That(goodsShipment.GoodsMeasures, NUnit.Framework.Is.EqualTo(default(IEnumerable<IGoodsMeasure>)));
			NUnit.Framework.Assert.That(goodsShipment.AdditionalInformations, NUnit.Framework.Is.EqualTo(default(IEnumerable<IAdditionalInformation>)));
		}

		[ExpectNoExceptions]
		public void TestSellerCustomsControlID()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "Org1";
			org.OH_RL_NKClosestPort = "TW";
			var epzCustomsCode = org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.EPZ, "EPZ12345", "TW");
			var cbfCustomsCode = org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.CBF, "CBF54321", "TW");
			var ftzCustomsCode = org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ13579", "TW");
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.CBPCodeType = "EPZ";
			supplierDocumentaryAddress.CBPCode = "12345678";
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			IGoodsShipment goodsShipment = new NX5105Declaration_GoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.Seller.CustomsControlID, NUnit.Framework.Is.EqualTo("12345678").Using(CustomComparers.TypeComparison));
			supplierDocumentaryAddress.E2_AddressOverride = false;
			supplierDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			goodsShipment = new NX5105Declaration_GoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.Seller.CustomsControlID, NUnit.Framework.Is.EqualTo("EPZ12345").Using(CustomComparers.TypeComparison));
			org.MainAddress.CustomsCodes.Delete(epzCustomsCode);
			goodsShipment = new NX5105Declaration_GoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.Seller.CustomsControlID, NUnit.Framework.Is.EqualTo("CBF54321").Using(CustomComparers.TypeComparison));
			org.MainAddress.CustomsCodes.Delete(cbfCustomsCode);
			goodsShipment = new NX5105Declaration_GoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.Seller.CustomsControlID, NUnit.Framework.Is.EqualTo("FTZ13579").Using(CustomComparers.TypeComparison));
			org.MainAddress.CustomsCodes.Delete(ftzCustomsCode);
			goodsShipment = new NX5105Declaration_GoodsShipment(entryHeader, null, null, false);
			NUnit.Framework.Assert.That(goodsShipment.Seller.CustomsControlID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		protected abstract IGoodsShipment GetGoodsShipment(CusEntryHeader entryHeader, SupportingDocumentCollection supportingDocuments, IStorageDocsBaseCollection[] allEDocs, bool includeControllingMessageInformation);
	}
}
