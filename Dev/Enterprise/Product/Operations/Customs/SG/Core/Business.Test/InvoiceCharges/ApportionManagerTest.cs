using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	sealed class ApportionManagerTest : TestCaseWithFactory
	{
		public void TestInsurancePercent()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
				invoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.EXW;
				invoiceHeader.JZ_InvoiceAmount = 1000;
				invoiceHeader.JZ_RX_NKInvoice_Currency = SGD.RX_Code;

				InvoiceGroupHeader.Charges.RemoveAndDeleteAll();
				invoiceHeader.Charges.RemoveAndDeleteAll();

				//group charges
				addNewCharge(InvoiceGroupHeader.Charges, Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD.RX_Code, 200m, 0m);
				addNewCharge(InvoiceGroupHeader.Charges, Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD.RX_Code, 0m, 1m);

				//invoice header charges
				addNewCharge(invoiceHeader.Charges, Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, SGD.RX_Code, 100m, 0m);

				//invoice line amounts
				JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_LinePrice = 400;

				JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 500;

				Declaration.ResumeApportionment();

				AssertEquals(2, invoiceHeader.GroupCharges.Count);
				AssertEquals(12m, invoiceHeader.GroupCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(200m, invoiceHeader.GroupCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));

				AssertEquals(3, invoiceLine1.ApportionedCharges.Count);
				AssertEquals(44.44m, invoiceLine1.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, SGD));
				AssertEquals(5.33m, invoiceLine1.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(88.89m, invoiceLine1.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));

				AssertEquals(3, invoiceLine2.ApportionedCharges.Count);
				AssertEquals(55.56m, invoiceLine2.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, SGD));
				AssertEquals(6.67m, invoiceLine2.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(111.11m, invoiceLine2.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));
			}
		}

		public void TestInsurancePercent1()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
				invoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CFR;
				invoiceHeader.JZ_InvoiceAmount = 1000;
				invoiceHeader.JZ_RX_NKInvoice_Currency = SGD.RX_Code;

				InvoiceGroupHeader.Charges.RemoveAndDeleteAll();
				invoiceHeader.Charges.RemoveAndDeleteAll();

				//group charges
				addNewCharge(InvoiceGroupHeader.Charges, Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD.RX_Code, 0m, 1m);

				//invoice header charges
				addNewCharge(invoiceHeader.Charges, Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, SGD.RX_Code, 100m, 0m);
				addNewCharge(invoiceHeader.Charges, Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD.RX_Code, 200m, 0m);

				//invoice line amounts
				JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_LinePrice = 400;

				JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 500;

				Declaration.ResumeApportionment();

				AssertEquals(1, invoiceHeader.GroupCharges.Count);
				AssertEquals(12m, invoiceHeader.GroupCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));

				AssertEquals(3, invoiceLine1.ApportionedCharges.Count);
				AssertEquals(44.44m, invoiceLine1.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, SGD));
				AssertEquals(5.33m, invoiceLine1.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(88.89m, invoiceLine1.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));

				AssertEquals(3, invoiceLine2.ApportionedCharges.Count);
				AssertEquals(55.56m, invoiceLine2.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, SGD));
				AssertEquals(6.67m, invoiceLine2.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(111.11m, invoiceLine2.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));
			}
		}

		public void TestInsurancePercent2()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				JobComInvoiceHeader invoiceHeader1 = Declaration.Invoices.AddNew();
				invoiceHeader1.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.EXW;
				invoiceHeader1.JZ_InvoiceAmount = 1000;
				invoiceHeader1.JZ_RX_NKInvoice_Currency = SGD.RX_Code;

				JobComInvoiceHeader invoiceHeader2 = Declaration.Invoices.AddNew();
				invoiceHeader2.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
				invoiceHeader2.JZ_InvoiceAmount = 2000;
				invoiceHeader2.JZ_RX_NKInvoice_Currency = SGD.RX_Code;

				InvoiceGroupHeader.Charges.RemoveAndDeleteAll();
				invoiceHeader1.Charges.RemoveAndDeleteAll();
				invoiceHeader2.Charges.RemoveAndDeleteAll();

				//group charges
				addNewCharge(InvoiceGroupHeader.Charges, Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD.RX_Code, 0m, 1m);
				addNewCharge(InvoiceGroupHeader.Charges, Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD.RX_Code, 400m, 0m);

				//invoice header charges
				addNewCharge(invoiceHeader1.Charges, Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, SGD.RX_Code, 100m, 0m);

				//invoice line amounts
				JobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_LinePrice = 400;

				JobComInvoiceLine invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 500;

				JobComInvoiceLine invoiceLine3 = invoiceHeader2.JobComInvoiceLines.AddNew();
				invoiceLine3.JI_LinePrice = 900;

				JobComInvoiceLine invoiceLine4 = invoiceHeader2.JobComInvoiceLines.AddNew();
				invoiceLine4.JI_LinePrice = 1100;

				Declaration.ResumeApportionment();

				AssertEquals(2, invoiceHeader1.GroupCharges.Count);
				AssertEquals(11.25m, invoiceHeader1.GroupCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(124.14m, invoiceHeader1.GroupCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));

				AssertEquals(2, invoiceHeader2.GroupCharges.Count);
				AssertEquals(19.80m, invoiceHeader2.GroupCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(275.86m, invoiceHeader2.GroupCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));

				AssertEquals(3, invoiceLine1.ApportionedCharges.Count);
				AssertEquals(44.44m, invoiceLine1.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, SGD));
				AssertEquals(5m, invoiceLine1.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(55.17m, invoiceLine1.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));

				AssertEquals(3, invoiceLine2.ApportionedCharges.Count);
				AssertEquals(55.56m, invoiceLine2.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, SGD));
				AssertEquals(6.25m, invoiceLine2.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(68.97m, invoiceLine2.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));

				AssertEquals(2, invoiceLine3.ApportionedCharges.Count);
				AssertEquals(8.91m, invoiceLine3.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(124.14m, invoiceLine3.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));

				AssertEquals(2, invoiceLine4.ApportionedCharges.Count);
				AssertEquals(10.89m, invoiceLine4.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(151.72m, invoiceLine4.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));
			}
		}

		public void TestInsurancePercentAndFreightPercent()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
				invoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.EXW;
				invoiceHeader.JZ_InvoiceAmount = 1000;
				invoiceHeader.JZ_RX_NKInvoice_Currency = SGD.RX_Code;

				InvoiceGroupHeader.Charges.RemoveAndDeleteAll();
				invoiceHeader.Charges.RemoveAndDeleteAll();

				//group charges
				addNewCharge(InvoiceGroupHeader.Charges, Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD.RX_Code, 0m, 20m);
				addNewCharge(InvoiceGroupHeader.Charges, Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD.RX_Code, 0m, 1m);

				//invoice header charges
				addNewCharge(invoiceHeader.Charges, Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, SGD.RX_Code, 100m, 0m);

				//invoice line amounts
				JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_LinePrice = 400;

				JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 500;

				Declaration.ResumeApportionment();

				AssertEquals(2, invoiceHeader.GroupCharges.Count);
				AssertEquals(12m, invoiceHeader.GroupCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(200m, invoiceHeader.GroupCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));

				AssertEquals(3, invoiceLine1.ApportionedCharges.Count);
				AssertEquals(44.44m, invoiceLine1.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, SGD));
				AssertEquals(5.33m, invoiceLine1.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(88.89m, invoiceLine1.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));

				AssertEquals(3, invoiceLine2.ApportionedCharges.Count);
				AssertEquals(55.56m, invoiceLine2.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, SGD));
				AssertEquals(6.67m, invoiceLine2.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, SGD));
				AssertEquals(111.11m, invoiceLine2.ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, SGD));
			}
		}

		void addNewCharge(IJobComInvChargeCollection<JobComInvCharge> charges, string chargeType, string chargeCurrency, ZDecimal chargeAmount, ZDecimal chargePercent)
		{
			JobComInvCharge charge = charges.AddNew();
			charge.J7_ChargeType = chargeType;
			charge.J7_RX_NKCurrency = chargeCurrency;

			if (chargeAmount > 0)
			{
				charge.J7_Amount = chargeAmount;
			}

			if (chargePercent > 0)
			{
				charge.J7_Percentage = chargePercent;
			}
		}

		#region Declaration

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion

		#region Invoice Group Header

		JobComInvoiceGroupHeader InvoiceGroupHeader
		{
			get { return Declaration.JobComInvoiceGroupHeaders[0]; }
		}

		#endregion

		#region Currencies

		RefCurrency SGD
		{
			get { return sgd ?? (sgd = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Singapore)); }
		}
		RefCurrency sgd;

		#endregion
	}
}
