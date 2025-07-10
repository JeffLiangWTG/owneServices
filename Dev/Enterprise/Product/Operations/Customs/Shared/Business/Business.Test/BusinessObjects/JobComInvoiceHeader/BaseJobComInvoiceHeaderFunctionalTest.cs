using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OrdersBusiniess = Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobComInvoiceHeaderFunctionalTest : TestCaseWithFactory
	{
		public void TestCastToRightTransportSupporterType_WhenAttachedToDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var invoice = Factory.New<BaseJobComInvoiceHeader>();
				invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
				invoice.Transports.AddNew();
				AssertNoExceptionThrown(() =>
				{
					invoice.JZ_JE = Factory.New<BaseJobDeclaration>().PK;
				});
			}
		}

		public void TestInvoiceExchangeRateIsNotReadOnlyAtTheRightTime()
		{
			BaseJobComInvoiceHeader invoiceHeader1 = Factory.New<BaseJobComInvoiceHeader>();
			AssertEquals("InvoiceHeader1.JZ_InvoiceCurrExRateInfo.ReadOnly", true, invoiceHeader1.JZ_InvoiceCurrExRateInfo.ReadOnly);
			NonROExRateJobComInvoiceHeader invoiceHeader2 = Factory.New<NonROExRateJobComInvoiceHeader>();
			AssertEquals("InvoiceHeader2.JZ_InvoiceCurrExRateInfo.ReadOnly", false, invoiceHeader2.JZ_InvoiceCurrExRateInfo.ReadOnly);
		}

		class NonROExRateJobComInvoiceHeader : BaseJobComInvoiceHeader
		{
			public NonROExRateJobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZBool IsJZ_InvoiceCurrExRateUserEnterableCore
			{
				get { return true; }
			}
		}

		public void TestIDeclarationProvider()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader header = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			AssertEquals(header.JobDeclaration, ((IDeclarationProvider)header).Declaration);
		}

		public void TestJZ_Calc_GroupInvoice()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader topGroup = dec.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			subGroup.JZ_InvoiceNumber = "Sub Group";

			BaseJobComInvoiceHeader invoice = subGroup.JobComInvoiceHeaders.AddNew();
			invoice.JZ_Calc_GroupInvoice = subGroup.JZ_InvoiceNumber.ToUpper();
			AssertEquals("JZ_JZ is set", subGroup.PK, invoice.JZ_JZ_GroupInvoiceFK);
			AssertEquals("Group Header is SubGroup", subGroup, invoice.GroupHeader);
			AssertEquals("JZ_Calc_GroupInvoice", subGroup.JZ_InvoiceNumber, invoice.JZ_Calc_GroupInvoice);
		}

		public void TestGroupHeader_HandleDeleted()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var topGroup = declaration.TopGroupInvoice;
			var subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JZ_GroupInvoiceFK = subGroup.PK;
			Factory.Save();
			AssertSame("invoice.GroupHeader", subGroup, invoice.GroupHeader);
			((IBusinessObjectInternals)subGroup).MarkAsDeleted();
			AssertNull("invoice.GroupHeader", invoice.GroupHeader);
		}

		public void TestInvoiceCountry()
		{
			GlbCompany company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			GlbBranch branch1 = company1.Branches.AddNew();
			branch1.GB_RL_NKHomePort = "NZAKL";

			GlbCompany company2 = Factory.New<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			GlbBranch branch2 = company2.Branches.AddNew();
			branch2.GB_RL_NKHomePort = "AUMEL";

			var testDec = BaseJobDeclaration.New(Factory);
			var group = testDec.JobComInvoiceGroupHeaders[0];
			var invoice = testDec.Invoices.AddNew();
			var standaloneInvoice = Factory.New<BaseJobComInvoiceHeader>();
			testDec.JE_GB = ZGuid.Empty;
			invoice.JZ_GB = ZGuid.Empty;
			standaloneInvoice.JZ_GB = ZGuid.Empty;

			AssertEquals("No country", null, invoice.InvoiceCountry);
			AssertEquals("No country", null, standaloneInvoice.InvoiceCountry);
			testDec.JE_GB = branch1.PK;
			AssertEquals("Country From Dec", Core.Constants.CountryGuids.NewZealand, invoice.InvoiceCountry.PK);
			AssertEquals("No country", null, standaloneInvoice.InvoiceCountry);
			invoice.JZ_GB = branch2.PK;
			standaloneInvoice.JZ_GB = branch2.PK;
			AssertEquals("Country Is Still taken from Dec", Core.Constants.CountryGuids.NewZealand, invoice.InvoiceCountry.PK);
			AssertEquals("Country from Invoice Header", Core.Constants.CountryGuids.Australia, standaloneInvoice.InvoiceCountry.PK);
		}

		public void TestICommonInvoiceInvoiceLines()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			BaseJobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();

			AssertEquals("ICommonInvoice.InvoiceLines", true, ((ICommonInvoice)invoice1).InvoiceLines.Contains(line1));
			AssertEquals("ICommonInvoice.InvoiceLines", false, ((ICommonInvoice)invoice1).InvoiceLines.Contains(line2));

			AssertEquals("ICommonInvoice.InvoiceLines", false, ((ICommonInvoice)invoice2).InvoiceLines.Contains(line1));
			AssertEquals("ICommonInvoice.InvoiceLines", true, ((ICommonInvoice)invoice2).InvoiceLines.Contains(line2));
		}

		public virtual void TestILandedCostChargeHolder()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();

			BaseInvoiceCharge validCharge = invoice.Charges.AddNew();
			validCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			validCharge.J7_Amount = 100m;
			validCharge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			BaseInvoiceCharge chargeWithoutDescription = invoice.Charges.AddNew();
			chargeWithoutDescription.J7_ChargeType = "";
			chargeWithoutDescription.J7_Amount = 200m;
			chargeWithoutDescription.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			BaseInvoiceCharge chargeWithoutAmount = invoice.Charges.AddNew();
			chargeWithoutAmount.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			chargeWithoutAmount.J7_Amount = 0m;
			chargeWithoutAmount.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			BaseInvoiceCharge chargeWithoutCurrency = invoice.Charges.AddNew();
			chargeWithoutCurrency.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			chargeWithoutCurrency.J7_Amount = 300m;
			chargeWithoutCurrency.J7_RX_NKCurrency = "";

			BaseInvoiceCharge chargeIncludedInLines = invoice.Charges.AddNew();
			chargeIncludedInLines.J7_ChargeType = CustomsChargeTypeList.Codes.PackingCost;
			chargeIncludedInLines.J7_Amount = 300m;
			chargeIncludedInLines.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			chargeIncludedInLines.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();
			AssertEquals("There should be only one charge", 1, new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)invoice).ChargesToImportForLandedCosting).Count);
			AssertEquals("It should the valid charge", OFTChargeDescription + " from Entry", new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)invoice).ChargesToImportForLandedCosting)[0].ChargeDescription);
		}

		protected virtual ZString OFTChargeDescription => CustomsChargeTypeList.Descriptions.OverseasFreight.ToString().ToUpper();

		public void TestILandedCostExchangeRateHolder()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			AssertEquals("CurrencyCode", testDec.LocalCurrencyCode, ((ILandedCostExchangeRateHolder)invoice).CurrencyCode);
			AssertEquals("DefaultLCExRate", 1m, ((ILandedCostExchangeRateHolder)invoice).LandedCostExchangeRateDefault);

			invoice.JZ_InvoiceNumber = "1";
			AssertEquals("ILandedCostExchangeRateHolder.ReferenceNumber", ((ILandedCostDistributeTo)invoice).Description, ((ILandedCostExchangeRateHolder)invoice).ReferenceNumber);

			invoice.JZ_InvoiceCurrLandedCostExRate = 0.7m;
			AssertEquals("ILandedCostExchangeRateHolder", 0.7m, ((ILandedCostExchangeRateHolder)invoice).LandedCostExchangeRate);

			invoice.JZ_InvoiceCurrExRate = 0.8m;
			AssertEquals("DefaultLCExRate", 0.8m, ((ILandedCostExchangeRateHolder)invoice).LandedCostExchangeRateDefault);

			((ILandedCostExchangeRateHolder)invoice).LandedCostExchangeRate = 0.58m;
			AssertEquals("ILandedCostExchangeRateHolder", 0.58m, ((ILandedCostExchangeRateHolder)invoice).LandedCostExchangeRate);
		}

		public void TestILandedCostDistributeTo()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			AssertEquals("ILandedCostInput.PK", invoice.PK, ((ILandedCostDistributeTo)invoice).PK);

			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			invoice.JZ_InvoiceNumber = "Invoice1";
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			AssertEquals("ILandedCostInput.UniqueCode", "INVOICE " + invoice.JZ_InvoiceNumber + ":" + supplier.OH_Code, ((ILandedCostDistributeTo)invoice).UniqueCode);
			AssertEquals("ILandedCostInput.Description", BaseJobComInvoiceHeader.InvoiceConstString + invoice.JZ_InvoiceNumber + ":" + supplier.OH_FullName, ((ILandedCostDistributeTo)invoice).Description);
			AssertEquals("ILandedCostInput.TableCode", JobComInvoiceHeaderSchema.Constants.Prefix, ((ILandedCostDistributeTo)invoice).TableCode);
			AssertEquals("ILandedCostInput.UltimateDistributee", 2, new List<IUltimateDistributee>(((ILandedCostDistributeTo)invoice).UltimateDistributees).Count);
		}

		[ExpectNoExceptions]
		public void TestDeleteDoestThrowException()
		{
			BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader header = groupHeader.JobComInvoiceHeaders.AddNew();

			header.JZ_InvoiceAmount = 1000m;
			header.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			header.JZ_IncoTerm = "FOB";

			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Delete();
		}

		public virtual void TestIsIncludedInLinesForApportionedCharge()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
				BaseJobComInvoiceHeader header = groupHeader.JobComInvoiceHeaders.AddNew();

				header.JZ_InvoiceAmount = 1000m;
				header.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				header.JZ_IncoTerm = "FOB";

				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 100, declaration.LocalCurrencyCode);
				declaration.ResumeApportionment();
				AssertEquals("GroupCharge not included in ITOT", false, header.GroupCharges[0].J7_IsIncludedInITOT);
				AssertEquals("FOB", 1100m, header.JZ_Calc_FOBAmount);
			}
		}

		public void TestReapportionGroupChargeWhenItsOwnChargeDeleted()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
				BaseJobComInvoiceHeader invoice = groupHeader.JobComInvoiceHeaders.AddNew();
				invoice.JZ_InvoiceNumber = "INV1";
				invoice.JZ_InvoiceAmount = 180848.58m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000, declaration.LocalCurrencyCode);
				declaration.ResumeApportionment();
				AssertEquals("One apportioned Charge", 1, invoice.GroupCharges.Count);

				invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000);
				declaration.ResumeApportionment();
				AssertEquals("no apportioned Charge", 0, invoice.GroupCharges.Count);

				invoice.Charges.RemoveAndDeleteAll();
				declaration.ResumeApportionment();
				AssertEquals("One apportioned Charge", 1, invoice.GroupCharges.Count);
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteInvoiceHeader()
		{
			BaseJobComInvoiceGroupHeader topGroup = declaration.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_Calc_GroupInvoice = topGroup.JZ_InvoiceNumber;
			invoice.JZ_InvoiceNumber = "Invoice1";

			BaseJobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.JI_Calc_Invoice = invoice.JZ_InvoiceNumber;
			BaseJobComInvoiceLine invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine2.JI_Calc_Invoice = invoice.JZ_InvoiceNumber;
			AssertEquals("JobComInvoiceLines are populated", 2, invoice.JobComInvoiceLines.Count);

			invoice.Delete();
			AssertEquals("InvoiceLines are deleted", 0, declaration.InvoiceLines.Count);
			AssertEquals("InvoiceLines are deleted", 0, declaration.FilteredInvoiceLines.Count);
		}

		public virtual void TestCalculateFOB_CIFNonDutiablePreFOB()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var currencyCode = declaration.LocalCurrencyCode;
				var currency = RefCurrency.LoadFromCurrencyCode(Factory, currencyCode);
				if (currency != null)
				{
					if (currency.RX_SubUnitRatio == 1)
					{
						currency.RX_SubUnitRatio = 100; // If the local country currency has 1 for the sub unit, then this test will fail (out by 10 cents) because of rounding to the nearest whole unit.  For the purpose of this test, pretend that minor units are allowed.  DJC.
					}
				}
				else
				{
					Assert("Can't run this test when currency code " + currencyCode + " is not in RefCurrency", false);
				}
				BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV1";
				invoice.JZ_InvoiceAmount = 180848.58m;
				invoice.JZ_RX_NKInvoice_Currency = currencyCode;
				invoice.JZ_IncoTerm = invoice.IncotermEquivalentToCFRForTesting;

				BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
				oFT.J7_Amount = 9280m;

				BaseJobComInvHeaderCharge nonDutiableFIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight);
				nonDutiableFIFT.J7_Amount = 2755.90m;
				nonDutiableFIFT.J7_IsDutiable = false;
				nonDutiableFIFT.J7_IsGSTApplicable = true;
				nonDutiableFIFT.J7_IsIncludedInITOT = true;

				BaseJobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
				invoiceLine.JI_Calc_Invoice = invoice.JZ_InvoiceNumber;
				invoiceLine.JI_LinePrice = 171568.58m;

				ZDecimal fOBExpected = invoice.JZ_InvoiceAmount - oFT.J7_Amount - nonDutiableFIFT.J7_Amount;
				declaration.ResumeApportionment();
				AssertEquals("FOB value", fOBExpected, invoice.JZ_Calc_FOBAmount);
				AssertEquals("FOB line", fOBExpected, invoiceLine.JI_Calc_FOB);
			}
		}

		public void TestBalancePrice()
		{
			// Test Multiple Countries?
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_JE = declaration.PK;
			BaseJobComInvoiceLine line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 100m;
			BaseJobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 200m;
			BaseJobComInvoiceLine line3 = invoiceHeader.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 300m;
			BaseJobComInvoiceLine line4 = invoiceHeader.JobComInvoiceLines.AddNew();
			line4.JI_LinePrice = 400m;
			invoiceHeader.BalancePrice();
			AssertEquals("JZ_InvoiceAmount", 1000m, invoiceHeader.JZ_InvoiceAmount);
		}

		public void TestIncoTermReadOnly()
		{
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_JE = declaration.PK;
			AssertEquals("IncoTerm is for summary display and should be read only", true, invoiceHeader.IncoTermInfo.ReadOnly);
			invoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("IncoTerm should be the same as IncoTerm", Enterprise.Core.Constants.IncoTerms.FreeOnBoard, invoiceHeader.IncoTerm);
			invoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.CostInsuranceAndFreight;
			AssertEquals("IncoTerm should be the same as IncoTerm", Enterprise.Core.Constants.IncoTerms.CostInsuranceAndFreight, invoiceHeader.IncoTerm);
		}

		public void TestInvoiceLineTotalCurrencyReadOnly()
		{
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_JE = declaration.PK;
			invoiceHeader.JZ_InvoiceAmount = 3000m;
			AssertEquals("InvoiceLineTotal ReadOnly", true, invoiceHeader.InvoiceLineTotalInfo.ReadOnly);
			AssertEquals("InvoiceLineTotal Value", 3000m, invoiceHeader.InvoiceLineTotal);
		}

		public void TestInvoiceLineTotalReadOnly()
		{
			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			AssertEquals("InvoiceLineTotalCurrency ReadOnly", true, invoiceHeader.InvoiceLineTotalCurrencyInfo.ReadOnly);
			RefCurrency currency = Factory.Load<RefCurrency>(invoiceHeader.InvoiceLineTotalCurrency);
			AssertEquals("InvoiceLineTotalCurrency Value", declaration.LocalCurrencyCode, currency != null ? currency.RX_Code : ZString.Empty);
		}

		public virtual void TestExceptionInSettingExchangeRate()
		{
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_ExportDate = ZDateTime.Invalid;
			AssertEquals("DateForRate", ZDateTime.Today, invoiceHeader.CurrencyConverter.DateForRate);

			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Invalid;
			AssertEquals("DateForRate", ZDateTime.Today, invoiceHeader.CurrencyConverter.DateForRate);
		}

		public void TestLocalCurrency()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea);
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			AssertEquals("Local currecny for Eritrea should be ERN", Core.Constants.CurrencyCodes.Eritrea, invoice.LocalCurrencyCode);

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Kenya;
			newCompany.GC_Code = "QW!";
			newCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "WE!";
			newBranch.GB_RL_NKHomePort = "KENBO";
			invoice.JZ_GB = newBranch.PK;
			AssertEquals("Local currecny for Kenya", Core.Constants.CurrencyCodes.Kenya, invoice.LocalCurrencyCode);
		}

		public void TestExchngeRateGetsSetOnValuationDateSet()
		{
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceHeader.JZ_InvoiceCurrExRate = 999m;
			AssertEquals(999m, invoiceHeader.JZ_InvoiceCurrExRate);
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Now;
			AssertEquals(1m, invoiceHeader.JZ_InvoiceCurrExRate);
		}

		public void TestAttachedOrdersWithInvoice()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var order = Factory.NewWithValidTestData<OrdersBusiniess.Order>();

			var orderPivot = Factory.New<GenPivot>();
			orderPivot.XX_Relation1ID = invoice.PK;
			orderPivot.XX_Relation2ID = order.PK;

			AssertEquals("Should have orders attached from invoice", 1, invoice.AttachedOrders.Count);
		}

		#region Implementation
		protected BaseJobDeclaration declaration;

		protected virtual BaseJobDeclaration GetNewDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = GetNewDeclaration();
		}
		#endregion
	}
}
