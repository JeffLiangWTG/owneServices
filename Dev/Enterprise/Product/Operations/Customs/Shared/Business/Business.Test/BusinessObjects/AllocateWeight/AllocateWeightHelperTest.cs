using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AllocateWeightHelperTest : TestCaseWithFactory
	{
		public void TestAllocateNetWeightWithSameInvoiceHeader()
		{
			AllocateNetWeight(Declaration.Invoices[0].JobComInvoiceLines.Cast<BaseJobComInvoiceLine>(), 100, Core.Constants.Weight.Hectograms, AllocateWeightMethodList.Codes.Price, true);

			AssertEquals(33.334m, Declaration.Invoices[0].JobComInvoiceLines[0].JI_NetWeight);
			AssertEquals(33.333m, Declaration.Invoices[0].JobComInvoiceLines[1].JI_NetWeight);
			AssertEquals(33.333m, Declaration.Invoices[0].JobComInvoiceLines[2].JI_NetWeight);
			AssertEquals(Core.Constants.Weight.Hectograms, Declaration.Invoices[0].JobComInvoiceLines[0].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Hectograms, Declaration.Invoices[0].JobComInvoiceLines[1].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Hectograms, Declaration.Invoices[0].JobComInvoiceLines[2].JI_NetWeightUQ);

			AllocateNetWeight(Declaration.Invoices[0].JobComInvoiceLines.Cast<BaseJobComInvoiceLine>(), 100, Core.Constants.Weight.Decitons, AllocateWeightMethodList.Codes.Quantity, true);

			AssertEquals(25m, Declaration.Invoices[0].JobComInvoiceLines[0].JI_NetWeight);
			AssertEquals(62.5m, Declaration.Invoices[0].JobComInvoiceLines[1].JI_NetWeight);
			AssertEquals(12.5m, Declaration.Invoices[0].JobComInvoiceLines[2].JI_NetWeight);
			AssertEquals(Core.Constants.Weight.Decitons, Declaration.Invoices[0].JobComInvoiceLines[0].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Decitons, Declaration.Invoices[0].JobComInvoiceLines[1].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Decitons, Declaration.Invoices[0].JobComInvoiceLines[2].JI_NetWeightUQ);

			Declaration.Invoices[0].JobComInvoiceLines[0].JI_NetWeight = 0;
			Declaration.Invoices[0].JobComInvoiceLines[1].JI_NetWeight = 0;

			AllocateNetWeight(Declaration.Invoices[0].JobComInvoiceLines.Cast<BaseJobComInvoiceLine>(), 100, Core.Constants.Weight.Kilograms, AllocateWeightMethodList.Codes.Quantity, false);

			AssertEquals(28.571m, Declaration.Invoices[0].JobComInvoiceLines[0].JI_NetWeight);
			AssertEquals(71.429m, Declaration.Invoices[0].JobComInvoiceLines[1].JI_NetWeight);
			AssertEquals(12.5m, Declaration.Invoices[0].JobComInvoiceLines[2].JI_NetWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.Invoices[0].JobComInvoiceLines[0].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.Invoices[0].JobComInvoiceLines[1].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Decitons, Declaration.Invoices[0].JobComInvoiceLines[2].JI_NetWeightUQ);
		}

		public void TestAllocateNetWeightWithDifferenceInvoiceHeader()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;

			AllocateNetWeight(Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>(), 100, Core.Constants.Weight.Hectograms, AllocateWeightMethodList.Codes.Price, true);

			AssertEquals(33.334m, Declaration.InvoiceLines[0].JI_NetWeight);
			AssertEquals(33.333m, Declaration.InvoiceLines[1].JI_NetWeight);
			AssertEquals(33.333m, Declaration.InvoiceLines[2].JI_NetWeight);
			AssertEquals(0m, Declaration.InvoiceLines[3].JI_NetWeight);
			AssertEquals(Core.Constants.Weight.Hectograms, Declaration.InvoiceLines[0].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Hectograms, Declaration.InvoiceLines[1].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Hectograms, Declaration.InvoiceLines[2].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.InvoiceLines[3].JI_NetWeightUQ);

			Declaration.InvoiceLines[3].JI_LinePrice = 25;
			Declaration.InvoiceLines[3].JI_InvoiceQuantity = 2;

			AllocateNetWeight(Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>(), 100, Core.Constants.Weight.Hectograms, AllocateWeightMethodList.Codes.Price, true);

			AssertEquals(25m, Declaration.InvoiceLines[0].JI_NetWeight);
			AssertEquals(25m, Declaration.InvoiceLines[1].JI_NetWeight);
			AssertEquals(25m, Declaration.InvoiceLines[2].JI_NetWeight);
			AssertEquals(25m, Declaration.InvoiceLines[3].JI_NetWeight);
			AssertEquals(Core.Constants.Weight.Hectograms, Declaration.InvoiceLines[0].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Hectograms, Declaration.InvoiceLines[1].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Hectograms, Declaration.InvoiceLines[2].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Hectograms, Declaration.InvoiceLines[3].JI_NetWeightUQ);

			AllocateNetWeight(Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>(), 100, Core.Constants.Weight.Decitons, AllocateWeightMethodList.Codes.Quantity, true);

			AssertEquals(20m, Declaration.InvoiceLines[0].JI_NetWeight);
			AssertEquals(50m, Declaration.InvoiceLines[1].JI_NetWeight);
			AssertEquals(10m, Declaration.InvoiceLines[2].JI_NetWeight);
			AssertEquals(20m, Declaration.InvoiceLines[3].JI_NetWeight);
			AssertEquals(Core.Constants.Weight.Decitons, Declaration.InvoiceLines[0].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Decitons, Declaration.InvoiceLines[1].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Decitons, Declaration.InvoiceLines[2].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Decitons, Declaration.InvoiceLines[3].JI_NetWeightUQ);

			Declaration.InvoiceLines[0].JI_NetWeight = 0;
			Declaration.InvoiceLines[1].JI_NetWeight = 0;

			AllocateNetWeight(Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>(), 100, Core.Constants.Weight.Kilograms, AllocateWeightMethodList.Codes.Quantity, false);

			AssertEquals(28.571m, Declaration.InvoiceLines[0].JI_NetWeight);
			AssertEquals(71.429m, Declaration.InvoiceLines[1].JI_NetWeight);
			AssertEquals(10m, Declaration.InvoiceLines[2].JI_NetWeight);
			AssertEquals(20m, Declaration.InvoiceLines[3].JI_NetWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.InvoiceLines[0].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.InvoiceLines[1].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Decitons, Declaration.InvoiceLines[2].JI_NetWeightUQ);
			AssertEquals(Core.Constants.Weight.Decitons, Declaration.InvoiceLines[3].JI_NetWeightUQ);
		}

		public void TestAllocateGrossWeightWithSameInvoiceHeader()
		{
			AllocateGrossWeight(Declaration.Invoices[0].JobComInvoiceLines.Cast<BaseJobComInvoiceLine>(), 101, Core.Constants.Weight.Kilograms, AllocateWeightMethodList.Codes.Price, true);
			AssertEquals(33.666m, Declaration.Invoices[0].JobComInvoiceLines[0].JI_Weight);
			AssertEquals(33.667m, Declaration.Invoices[0].JobComInvoiceLines[1].JI_Weight);
			AssertEquals(33.667m, Declaration.Invoices[0].JobComInvoiceLines[2].JI_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.Invoices[0].JobComInvoiceLines[0].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.Invoices[0].JobComInvoiceLines[1].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.Invoices[0].JobComInvoiceLines[2].JI_WeightUQ);

			AllocateGrossWeight(Declaration.Invoices[0].JobComInvoiceLines.Cast<BaseJobComInvoiceLine>(), 101, Core.Constants.Weight.Kilotonnes, AllocateWeightMethodList.Codes.Quantity, true);
			AssertEquals(25.25m, Declaration.Invoices[0].JobComInvoiceLines[0].JI_Weight);
			AssertEquals(63.125m, Declaration.Invoices[0].JobComInvoiceLines[1].JI_Weight);
			AssertEquals(12.625m, Declaration.Invoices[0].JobComInvoiceLines[2].JI_Weight);
			AssertEquals(Core.Constants.Weight.Kilotonnes, Declaration.Invoices[0].JobComInvoiceLines[0].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilotonnes, Declaration.Invoices[0].JobComInvoiceLines[1].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilotonnes, Declaration.Invoices[0].JobComInvoiceLines[2].JI_WeightUQ);

			Declaration.Invoices[0].JobComInvoiceLines[0].JI_Weight = 0;
			Declaration.Invoices[0].JobComInvoiceLines[1].JI_Weight = 0;

			AllocateGrossWeight(Declaration.Invoices[0].JobComInvoiceLines.Cast<BaseJobComInvoiceLine>(), 101, Core.Constants.Weight.Kilograms, AllocateWeightMethodList.Codes.Quantity, false);
			AssertEquals(28.857m, Declaration.Invoices[0].JobComInvoiceLines[0].JI_Weight);
			AssertEquals(72.143m, Declaration.Invoices[0].JobComInvoiceLines[1].JI_Weight);
			AssertEquals(12.625m, Declaration.Invoices[0].JobComInvoiceLines[2].JI_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.Invoices[0].JobComInvoiceLines[0].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.Invoices[0].JobComInvoiceLines[1].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilotonnes, Declaration.Invoices[0].JobComInvoiceLines[2].JI_WeightUQ);
		}

		public void TestAllocateGrossWeightWithDifferenceInvoiceHeader()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			AllocateGrossWeight(Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>(), 101, Core.Constants.Weight.Hectograms, AllocateWeightMethodList.Codes.Price, true);
			AssertEquals(33.666m, Declaration.InvoiceLines[0].JI_Weight);
			AssertEquals(33.667m, Declaration.InvoiceLines[1].JI_Weight);
			AssertEquals(33.667m, Declaration.InvoiceLines[2].JI_Weight);
			AssertEquals(0m, Declaration.InvoiceLines[3].JI_Weight);
			AssertEquals(Core.Constants.Weight.Hectograms, Declaration.InvoiceLines[0].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Hectograms, Declaration.InvoiceLines[1].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Hectograms, Declaration.InvoiceLines[2].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.InvoiceLines[3].JI_WeightUQ);

			Declaration.InvoiceLines[3].JI_LinePrice = 25;
			Declaration.InvoiceLines[3].JI_InvoiceQuantity = 2;

			AllocateGrossWeight(Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>(), 101, Core.Constants.Weight.Kilograms, AllocateWeightMethodList.Codes.Price, true);
			AssertEquals(25.25m, Declaration.InvoiceLines[0].JI_Weight);
			AssertEquals(25.25m, Declaration.InvoiceLines[1].JI_Weight);
			AssertEquals(25.25m, Declaration.InvoiceLines[2].JI_Weight);
			AssertEquals(25.25m, Declaration.InvoiceLines[3].JI_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.InvoiceLines[0].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.InvoiceLines[1].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.InvoiceLines[2].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.InvoiceLines[3].JI_WeightUQ);

			AllocateGrossWeight(Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>(), 101, Core.Constants.Weight.Kilotonnes, AllocateWeightMethodList.Codes.Quantity, true);
			AssertEquals(20.2m, Declaration.InvoiceLines[0].JI_Weight);
			AssertEquals(50.5m, Declaration.InvoiceLines[1].JI_Weight);
			AssertEquals(10.1m, Declaration.InvoiceLines[2].JI_Weight);
			AssertEquals(20.2m, Declaration.InvoiceLines[3].JI_Weight);
			AssertEquals(Core.Constants.Weight.Kilotonnes, Declaration.InvoiceLines[0].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilotonnes, Declaration.InvoiceLines[1].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilotonnes, Declaration.InvoiceLines[2].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilotonnes, Declaration.InvoiceLines[3].JI_WeightUQ);

			Declaration.InvoiceLines[0].JI_Weight = 0;
			Declaration.InvoiceLines[1].JI_Weight = 0;

			AllocateGrossWeight(Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>(), 101, Core.Constants.Weight.Kilograms, AllocateWeightMethodList.Codes.Quantity, false);
			AssertEquals(28.857m, Declaration.InvoiceLines[0].JI_Weight);
			AssertEquals(72.143m, Declaration.InvoiceLines[1].JI_Weight);
			AssertEquals(10.1m, Declaration.InvoiceLines[2].JI_Weight);
			AssertEquals(20.2m, Declaration.InvoiceLines[3].JI_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.InvoiceLines[0].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilograms, Declaration.InvoiceLines[1].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilotonnes, Declaration.InvoiceLines[2].JI_WeightUQ);
			AssertEquals(Core.Constants.Weight.Kilotonnes, Declaration.InvoiceLines[3].JI_WeightUQ);
		}

		void AllocateNetWeight(IEnumerable<BaseJobComInvoiceLine> baseJobComInvoiceLines, ZDecimal netWeight, ZString netWeightUnit, ZString method, ZBool overrideExisting)
		{
			var allocateWeight = GetAllocateWeight(baseJobComInvoiceLines, netWeight, netWeightUnit, 0, ZString.Empty, method, overrideExisting);
			AllocateWeightHelper.AllocateNetWeight(allocateWeight, baseJobComInvoiceLines);
		}

		void AllocateGrossWeight(IEnumerable<BaseJobComInvoiceLine> baseJobComInvoiceLines, ZDecimal grossWeight, ZString grossWeightUnit, ZString method, ZBool overrideExisting)
		{
			var allocateWeight = GetAllocateWeight(baseJobComInvoiceLines, 0, ZString.Empty, grossWeight, grossWeightUnit, method, overrideExisting);
			AllocateWeightHelper.AllocateGrossWeight(allocateWeight, baseJobComInvoiceLines);
		}

		AllocateWeight GetAllocateWeight(IEnumerable<BaseJobComInvoiceLine> baseJobComInvoiceLines, ZDecimal netWeight, ZString netWeightUnit, ZDecimal grossWeight, ZString grossWeightUnit, ZString method, ZBool overrideExisting)
		{
			var allocateWeight = new AllocateWeight(Declaration.Factory, baseJobComInvoiceLines);
			if (netWeight > 0)
			{
				allocateWeight.NetWeight = netWeight;
				allocateWeight.NetWeightUnit = netWeightUnit;
			}
			if (grossWeight > 0)
			{
				allocateWeight.GrossWeight = grossWeight;
				allocateWeight.GrossWeightUnit = grossWeightUnit;
			}
			allocateWeight.AllocateWeightMethod = method;
			allocateWeight.OverrideExisting = overrideExisting;
			return allocateWeight;
		}

		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
					var invoiceHeader1 = declaration.Invoices.AddNew();
					invoiceHeader1.JZ_RX_NKInvoice_Currency = invoiceHeader1.LocalCurrencyCode;
					invoiceHeader1.JZ_InvoiceCurrExRate = 1;

					var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
					invoiceLine1.JI_LinePrice = 1;
					invoiceLine1.JI_InvoiceQuantity = 2;

					var invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
					invoiceLine2.JI_LinePrice = 1;
					invoiceLine2.JI_InvoiceQuantity = 5;

					var invoiceLine3 = invoiceHeader1.JobComInvoiceLines.AddNew();
					invoiceLine3.JI_LinePrice = 1;
					invoiceLine3.JI_InvoiceQuantity = 1;

					var invoiceHeader2 = declaration.Invoices.AddNew();
					invoiceHeader2.JZ_RX_NKInvoice_Currency = "TWD";
					invoiceHeader2.IsJZ_InvoiceCurrExRateUserEnterable = true;
					invoiceHeader2.JZ_InvoiceCurrExRate = 0.04m;

					var invoiceLine4 = invoiceHeader2.JobComInvoiceLines.AddNew();
					invoiceLine4.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
					invoiceLine4.JI_WeightUQ = Core.Constants.Weight.Kilograms;
					invoiceLine4.JI_LinePrice = 0;
					invoiceLine4.JI_InvoiceQuantity = 0;

					Factory.Save();
				}
				return declaration;
			}
		}

		BaseJobDeclaration declaration;
	}
}
