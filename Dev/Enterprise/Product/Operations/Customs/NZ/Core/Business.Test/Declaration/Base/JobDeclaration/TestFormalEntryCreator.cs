using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class TestFormalEntryCreator : TestDeclarationCreator
	{
		public TestFormalEntryCreator(JobDeclaration declaration)
			: base(declaration)
		{
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			fCurrentInvoiceGroup = declaration.JobComInvoiceGroupHeaders[0];
		}

		public JobComInvoiceGroupHeader CurrentInvoiceGroup
		{
			get { return fCurrentInvoiceGroup; }
		}
		readonly JobComInvoiceGroupHeader fCurrentInvoiceGroup;

		public JobComInvoiceHeader CurrentInvoiceHeader
		{
			get { return fCurrentInvoiceHeader; }
		}
		JobComInvoiceHeader fCurrentInvoiceHeader;

		public JobComInvoiceLine CurrentInvoiceLine
		{
			get { return fCurrentInvoiceLine; }
		}
		JobComInvoiceLine fCurrentInvoiceLine;

		#region SetupTestForExportToAU
		public override void SetupTestForExportToAU()
		{
			base.SetupTestForExportToAU();
			Declaration.JE_SoldOrConsigned = TermsOfSaleList.Codes.Sold;
		}
		#endregion

		#region SetupTestConsignmentDetails
		public override void SetupTestConsignmentDetails()
		{
			Declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;

			Declaration.WarehouseDocAddress.E2_OA_Address = BondStoreAddress.PK;

			Declaration.JE_RL_NKProcessingPort = "NZAKL";
			Declaration.JE_PaymentMethod = PaymentMethodList.Codes.BrokerDeferred;

			base.SetupTestConsignmentDetails();

			Declaration.JE_TotalWeight = 1000;
		}
		#endregion

		#region BondStoreAddress
		protected OrgAddress BondStoreAddress
		{
			get
			{
				if (fBondStoreAddress == null)
				{
					OrgHeader bondStore = OrgHeader.New(factory);
					bondStore.OH_Code = "ZZAKBOND";
					bondStore.OH_FullName = "AUCKLAND BOND STORE";
					bondStore.MainAddress.OA_Address1 = "TEST CODE FOR NZ CUSTOMS";
					bondStore.MainAddress.OA_Address2 = "LOCATED IN NZAKL";
					bondStore.OH_RL_NKClosestPort = "NZAKL";

					fBondStoreAddress = bondStore.MainAddress;

					OrgCusCode cusCode = bondStore.CustomsCodes.AddNew();
					cusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
					cusCode.OK_OA_PremisesAddress = fBondStoreAddress.PK;
					cusCode.OK_CustomsRegNo = "1234Z";
					cusCode.OK_RN_NKCodeCountry = "NZ";
				}
				return fBondStoreAddress;
			}
		}
		OrgAddress fBondStoreAddress;
		#endregion

		#region SetupInvoiceGroup
		public void SetupInvoiceGroup(ZDecimal freightAmount, ZString freightCurrency, ZDecimal insuranceAmount, ZString insuranceCurrency)
		{
			BaseGroupInvoiceCharge freightCharge = fCurrentInvoiceGroup.Charges.AddNew();
			freightCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			freightCharge.J7_Amount = freightAmount;
			freightCharge.J7_IsGSTApplicable = true;
			freightCharge.J7_IsIncludedInITOT = false;
			freightCharge.J7_RX_NKCurrency = freightCurrency;

			BaseGroupInvoiceCharge insuranceCharge = fCurrentInvoiceGroup.Charges.AddNew();
			insuranceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			insuranceCharge.J7_Amount = insuranceAmount;
			insuranceCharge.J7_IsGSTApplicable = true;
			insuranceCharge.J7_IsIncludedInITOT = false;
			insuranceCharge.J7_RX_NKCurrency = insuranceCurrency;
		}
		#endregion

		#region SetupInvoiceHeader
		public void SetupImportInvoiceHeader(ZString invoiceNumber, ZString incoTerm, ZString currency, ZDecimal invoiceAmount)
		{
			SetupImportInvoiceHeader(invoiceNumber, incoTerm, currency, invoiceAmount, "", "", "");
		}

		public void SetupImportInvoiceHeader(ZString invoiceNumber, ZString incoTerm, ZString currency, ZDecimal invoiceAmount, ZString defaultCountryOfOrigin, ZString defaultCountryOfExport, ZString defaultQualForPrefDuty)
		{
			fCurrentInvoiceHeader = fCurrentInvoiceGroup.JobComInvoiceHeaders.AddNew();
			fCurrentInvoiceHeader.JZ_InvoiceNumber = invoiceNumber;
			fCurrentInvoiceHeader.JZ_IncoTerm = incoTerm;
			RefCurrency invCurr = RefCurrency.LoadFromCurrencyCode(factory, currency);
			if (invCurr != null)
			{
				fCurrentInvoiceHeader.JZ_RX_NKInvoice_Currency = invCurr.RX_Code;
			}
			fCurrentInvoiceHeader.JZ_InvoiceAmount = invoiceAmount;
			fCurrentInvoiceHeader.JZ_RelationshipIndicator = RelationshipIndicatorList.Codes.NotRelated;
			fCurrentInvoiceHeader.JZ_RN_NKDefaultOrigin = defaultCountryOfOrigin;
			fCurrentInvoiceHeader.JZ_RN_NKDefaultExport = defaultCountryOfExport;
			fCurrentInvoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = defaultQualForPrefDuty;
		}

		public void SetupExportInvoiceHeader(ZString invoiceNumber, ZString incoTerm, ZString currency, ZDecimal invoiceAmount, ZString exchangeRateIndicator)
		{
			SetupImportInvoiceHeader(invoiceNumber, incoTerm, currency, invoiceAmount);
			fCurrentInvoiceHeader.JZ_ExchangeRateIndicator = exchangeRateIndicator;
		}
		#endregion

		#region SetupInvoiceLine
		protected void SetupInvoiceLine(ZString tariffCode, ZString description, ZString countryOfOrigin, ZString countryOfExport, ZString qualForPrefDuty, ZDecimal linePrice, ZDecimal statisticalQty, ZString statisticalUnit)
		{
			fCurrentInvoiceLine = fCurrentInvoiceHeader.JobComInvoiceLines.AddNew();
			fCurrentInvoiceLine.JI_Tariff = tariffCode;
			fCurrentInvoiceLine.JI_Description = description;
			fCurrentInvoiceLine.JI_CountryOfOrigin = countryOfOrigin;
			fCurrentInvoiceLine.JI_RN_NKCountryOfExport = countryOfExport;
			fCurrentInvoiceLine.JI_QualifiesForPreferentialDuty = qualForPrefDuty;
			fCurrentInvoiceLine.JI_LinePrice = linePrice;
			fCurrentInvoiceLine.JI_CustomsQuantity = statisticalQty;
			fCurrentInvoiceLine.JI_CustomsUnitQty = statisticalUnit;
		}

		public void SetupImportInvoiceLine(ZString tariffCode, ZString description, ZString countryOfOrigin, ZString countryOfExport, ZString qualForPrefDuty, ZDecimal linePrice, ZDecimal statisticalQty, ZString statisticalUnit)
		{
			SetupInvoiceLine(tariffCode, description, countryOfOrigin, countryOfExport, qualForPrefDuty, linePrice, statisticalQty, statisticalUnit);
		}

		public void SetupImportInvoiceLine(ZString tariffCode, ZString description, ZString countryOfOrigin, ZString countryOfExport, ZString qualForPrefDuty, ZDecimal linePrice)
		{
			SetupInvoiceLine(tariffCode, description, countryOfOrigin, countryOfExport, qualForPrefDuty, linePrice, 0m, "");
		}

		public void SetupExportInvoiceLine(ZString tariffCode, ZString description, ZString countryOfOrigin, ZDecimal linePrice, ZDecimal statisticalQty, ZString statisticalUnit)
		{
			SetupInvoiceLine(tariffCode, description, countryOfOrigin, "", "", linePrice, statisticalQty, statisticalUnit);
		}

		public void SetupExportInvoiceLine(ZString tariffCode, ZString description, ZString countryOfOrigin, ZDecimal linePrice, ZDecimal statisticalQty, ZString statisticalUnit, ZDecimal dutyCredit, ZDecimal levyCredit)
		{
			SetupExportInvoiceLine(tariffCode, description, countryOfOrigin, linePrice, statisticalQty, statisticalUnit);
			fCurrentInvoiceLine.JI_DutyCreditAmount = dutyCredit;
			fCurrentInvoiceLine.JI_LevyCreditAmount = levyCredit;
		}

		public void SetupExportInvoiceLine(ZString tariffCode, ZString description, ZString countryOfOrigin, ZDecimal linePrice)
		{
			SetupExportInvoiceLine(tariffCode, description, countryOfOrigin, linePrice, 0m, "");
		}
		#endregion

		#region AddHouseBillWithPackingDetails
		public void AddHouseBillWithPackingDetails(ZString houseBillNumber, ZInt packQty, ZString packType)
		{
			Bill houseBill = Declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = houseBillNumber;

			PackingGroup packGroup = houseBill.PackingGroups.AddNew();
			Package package = packGroup.Packages.AddNew();
			package.CW_PackQty = packQty;
			package.CW_PackType = packType;
		}
		#endregion

		#region AddHouseBillAndContainerWithPackingDetailsAgainstContainerOnly
		public void AddHouseBillAndContainerWithPackingDetailsAgainstContainerOnly(ZString houseBillNumber, ZString containerNumber, ZString containerMode, ZInt packQty, ZString packType)
		{
			Bill houseBill = Declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = houseBillNumber;

			PackingGroup packGroup1 = houseBill.PackingGroups.AddNew();

			PackingGroup packGroup2 = houseBill.PackingGroups.AddNew();
			CusContainer container = Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			container.CO_FCL_LCL_AIR = containerMode;
			packGroup2.CR_CO_Container = container.PK;

			Package package = packGroup2.Packages.AddNew();
			package.CW_PackQty = packQty;
			package.CW_PackType = packType;
		}
		#endregion

		#region MergeDeclaration
		public void MergeDeclaration()
		{
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			try
			{
				Declaration.DoMerge(); // Will Throw Exception if there's a Merge Problem
			}
			catch
			{
				Declaration.UnlockDoMergeMutex();
				throw;
			}
			Declaration.CusEntryHeader.CH_TotalPaid = 0;//DutyAndTax will be added manually
		}
		#endregion

		#region MergeLineSetDutyAndTax
		public void MergeLineSetDutyAndTax(ZInt lineIndex, ZDecimal duty, ZDecimal tax)
		{
			CusEntryLine entryLine = Declaration.CusEntryHeader.MergedLines[lineIndex];
			entryLine.DutyAmount = duty;
			entryLine.GSTAmount = tax;
			entryLine.EntryHeader.CH_TotalPaid += duty + tax;
		}
		#endregion
	}

	public class TestFormalEntryCreatorTest : TestDeclarationCreatorTest
	{
		[ExpectNoExceptions]
		public void TestSetupTestConsignmentDetailsImport()
		{
			DecCreator.SetupTestConsignmentDetails();
			DecCreator.SetupTestForAir();
			DecCreator.SetupTestForImportFromAU();
			DecCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			DecCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			DecCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			DecCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Assert(DecCreator.Declaration.SaveHandlingSaveExceptions());
			DecCreator.MergeDeclaration();
			DecCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
		}

		[ExpectException(typeof(Exception))]
		public void TestInvalidMergeThrowsException()
		{
			DecCreator.SetupTestConsignmentDetails();
			DecCreator.MergeDeclaration();
		}

		#region Implementation
		protected override TestDeclarationCreator GetNewTestDeclarationCreator()
		{
			return new TestFormalEntryCreator(Declaration);
		}

		protected new TestFormalEntryCreator DecCreator
		{
			get { return (TestFormalEntryCreator)base.DecCreator; }
		}
		#endregion
	}
}
