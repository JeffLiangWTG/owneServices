using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class GenericLandedCostingHelperTest : TestCaseWithFactory
	{
		public void TestGenericLandedCostingHelper()
		{
			string genericLandedCostingConfigXml = string.Format(@"
	<GenericLandedCostingConfig CountryCode='{0}'>
		<FormalEntryConfigs>
			<FormalEntryConfig EntryType='XXX' />
		</FormalEntryConfigs>
		<LCEntryCustomsDisbursementCodeMappings>
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='DTY' LCDisbursementCode='TDT' Description='Customs Duty' HeaderLevelFee='True' />
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='EXS' LCDisbursementCode='EXC' Description='Excise' JobComInvoiceLineAmountPropertyName='XX_ExciseAmount' />
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='ADD' LCDisbursementCode='OTH' Description='Antidumping Duty' HeaderLevelFee='True' />
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='SIM' LCDisbursementCode='OTH' Description='SIMA' />
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='499' LCDisbursementCode='ST1' Description='Merchandise Processing Fee' HeaderLevelFee='True' CusEntryLineApplicablePropertyName='XX_HasMPF' />
		</LCEntryCustomsDisbursementCodeMappings>
	</GenericLandedCostingConfig>", GlbCompany.CurrentCompany.Country);

			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.GenericLandedCostingConfigXml = genericLandedCostingConfigXml;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Env.CurrentCompany.LocalCurrency.Code;
			var invoiceLine1 = Factory.New<InvoiceLineForTesting>();
			invoiceLine1.JI_JZ = invoice.PK;
			invoiceLine1.JI_LinePrice = 100m;
			invoiceLine1.XX_ExciseAmount = 1m;
			var invoiceLine2 = Factory.New<InvoiceLineForTesting>();
			invoiceLine2.JI_JZ = invoice.PK;
			invoiceLine2.JI_LinePrice = 300m;
			invoiceLine2.XX_ExciseAmount = 2m;
			var invoiceLine3 = Factory.New<InvoiceLineForTesting>();
			invoiceLine3.JI_JZ = invoice.PK;
			invoiceLine3.JI_LinePrice = 100m;
			invoiceLine3.XX_ExciseAmount = 3m;
			var invoiceLine4 = Factory.New<InvoiceLineForTesting>();
			invoiceLine4.JI_JZ = invoice.PK;
			invoiceLine4.JI_LinePrice = 200m;
			invoiceLine4.XX_ExciseAmount = 4m;
			var invoiceLine5 = Factory.New<InvoiceLineForTesting>();
			invoiceLine5.JI_JZ = invoice.PK;
			invoiceLine5.JI_LinePrice = 800m;
			invoiceLine5.XX_ExciseAmount = 8m;

			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = "XXX";
			entryHeader1.Charges.AddNew("DTY", 10m);
			entryHeader1.Charges.AddNew("ADD", 20m);
			entryHeader1.Charges.AddNew("499", 30m);

			var entryLine1 = Factory.New<CusEntryLineForTesting>();
			entryHeader1.MergedLines.Add(entryLine1);
			entryLine1.CL_CustomsValue = 200m;
			entryLine1.Fees.AddOrUpdate("EXS", 3m);
			entryLine1.Fees.AddOrUpdate("SIM", 8m);
			invoiceLine1.JI_CL = entryLine1.PK;
			entryLine1.InvoiceLines.Add(invoiceLine1);
			invoiceLine2.JI_CL = entryLine1.PK;
			entryLine1.InvoiceLines.Add(invoiceLine2);
			entryLine1.XX_HasMPF = true;

			var entryLine2 = Factory.New<CusEntryLineForTesting>();
			entryHeader1.MergedLines.Add(entryLine2);
			entryLine2.CL_CustomsValue = 300m;
			invoiceLine3.JI_CL = entryLine2.PK;
			entryLine2.InvoiceLines.Add(invoiceLine3);
			invoiceLine4.JI_CL = entryLine2.PK;
			entryLine2.InvoiceLines.Add(invoiceLine4);
			entryLine2.Fees.AddOrUpdate("EXS", 7m);
			entryLine2.Fees.AddOrUpdate("SIM", 12m);
			entryLine2.XX_HasMPF = false;

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = "XXX";
			entryHeader2.Charges.AddNew("DTY", 15m);
			entryHeader2.Charges.AddNew("ADD", 20m);
			entryHeader2.Charges.AddNew("499", 30m);

			var entryLine3 = Factory.New<CusEntryLineForTesting>();
			entryHeader2.MergedLines.Add(entryLine3);
			entryLine3.CL_CustomsValue = 800m;
			invoiceLine5.JI_CL = entryLine3.PK;
			entryLine3.InvoiceLines.Add(invoiceLine5);
			entryLine3.Fees.AddOrUpdate("EXS", 8m);
			entryLine3.Fees.AddOrUpdate("SIM", 8m);
			entryLine3.XX_HasMPF = true;

			var entryHeader3 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader3.CH_MessageType = "YYY";
			entryHeader3.Charges.AddNew("DTY", 15m);
			entryHeader3.Charges.AddNew("ADD", 20m);
			entryHeader3.Charges.AddNew("499", 30m);

			var entryLine4 = Factory.New<CusEntryLineForTesting>();
			entryHeader3.MergedLines.Add(entryLine4);
			entryLine4.CL_CustomsValue = 800m;
			invoiceLine5.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine4);
			entryLine4.InvoiceLines.Add(invoiceLine5);
			entryLine4.Fees.AddOrUpdate("EXS", 8m);
			entryLine4.Fees.AddOrUpdate("SIM", 8m);
			entryLine4.XX_HasMPF = true;

			var landedCostHeader = ((ILandedCostHeader)declaration).TotalDutyTaxEntryFeeItems;
			CombineAssertions(() =>
			{
				AssertEquals("Total Duty", 25m, landedCostHeader["TDT"]);
				AssertEquals("Total Excise", 18m, landedCostHeader["EXC"]);
				AssertEquals("Total Other Duty", 68m, landedCostHeader["OTH"]);
				AssertEquals("Total MPF", 60m, landedCostHeader["ST1"]);

				AssertInvoiceLine(invoiceLine1, 1m, 1m, 4m, 7.5m);
				AssertInvoiceLine(invoiceLine2, 3m, 2m, 12m, 22.5m);
				AssertInvoiceLine(invoiceLine3, 2m, 3m, 8m, 0m);
				AssertInvoiceLine(invoiceLine4, 4m, 4m, 16m, 0m);
				AssertInvoiceLine(invoiceLine5, 15m, 8m, 28m, 30m);
			});
		}

		public void TestGenericLandedCostingHelper_AdditionalEntryLine()
		{
			string genericLandedCostingConfigXml = string.Format(@"
	<GenericLandedCostingConfig CountryCode='{0}'>
		<FormalEntryConfigs>
			<FormalEntryConfig EntryType='XXX' IsAdditionalEntryLine='Y' />
		</FormalEntryConfigs>
		<LCEntryCustomsDisbursementCodeMappings>
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='DTY' LCDisbursementCode='TDT' Description='Customs Duty' HeaderLevelFee='True' />
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='EXS' LCDisbursementCode='EXC' Description='Excise' JobComInvoiceLineAmountPropertyName='XX_ExciseAmount' />
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='ADD' LCDisbursementCode='OTH' Description='Antidumping Duty' HeaderLevelFee='True' />
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='SIM' LCDisbursementCode='OTH' Description='SIMA' />
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='499' LCDisbursementCode='ST1' Description='Merchandise Processing Fee' HeaderLevelFee='True' CusEntryLineApplicablePropertyName='XX_HasMPF' />
		</LCEntryCustomsDisbursementCodeMappings>
	</GenericLandedCostingConfig>", GlbCompany.CurrentCompany.Country);

			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.GenericLandedCostingConfigXml = genericLandedCostingConfigXml;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Env.CurrentCompany.LocalCurrency.Code;
			var invoiceLine1 = Factory.New<InvoiceLineForTesting>();
			invoiceLine1.JI_JZ = invoice.PK;
			invoiceLine1.JI_LinePrice = 100m;
			invoiceLine1.XX_ExciseAmount = 1m;
			var invoiceLine2 = Factory.New<InvoiceLineForTesting>();
			invoiceLine2.JI_JZ = invoice.PK;
			invoiceLine2.JI_LinePrice = 300m;
			invoiceLine2.XX_ExciseAmount = 2m;
			var invoiceLine3 = Factory.New<InvoiceLineForTesting>();
			invoiceLine3.JI_JZ = invoice.PK;
			invoiceLine3.JI_LinePrice = 100m;
			invoiceLine3.XX_ExciseAmount = 3m;
			var invoiceLine4 = Factory.New<InvoiceLineForTesting>();
			invoiceLine4.JI_JZ = invoice.PK;
			invoiceLine4.JI_LinePrice = 200m;
			invoiceLine4.XX_ExciseAmount = 4m;
			var invoiceLine5 = Factory.New<InvoiceLineForTesting>();
			invoiceLine5.JI_JZ = invoice.PK;
			invoiceLine5.JI_LinePrice = 800m;
			invoiceLine5.XX_ExciseAmount = 8m;

			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = "XXX";
			entryHeader1.Charges.AddNew("DTY", 10m);
			entryHeader1.Charges.AddNew("ADD", 20m);
			entryHeader1.Charges.AddNew("499", 30m);

			var entryLine1 = Factory.New<CusEntryLineForTesting>();
			entryHeader1.MergedLines.Add(entryLine1);
			entryLine1.CL_CustomsValue = 200m;
			entryLine1.Fees.AddOrUpdate("EXS", 3m);
			entryLine1.Fees.AddOrUpdate("SIM", 8m);
			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			entryLine1.InvoiceLines.Add(invoiceLine1);
			invoiceLine2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			entryLine1.InvoiceLines.Add(invoiceLine2);
			entryLine1.XX_HasMPF = true;

			var entryLine2 = Factory.New<CusEntryLineForTesting>();
			entryHeader1.MergedLines.Add(entryLine2);
			entryLine2.CL_CustomsValue = 300m;
			invoiceLine3.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
			entryLine2.InvoiceLines.Add(invoiceLine3);
			invoiceLine4.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
			entryLine2.InvoiceLines.Add(invoiceLine4);
			entryLine2.Fees.AddOrUpdate("EXS", 7m);
			entryLine2.Fees.AddOrUpdate("SIM", 12m);
			entryLine2.XX_HasMPF = false;

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = "XXX";
			entryHeader2.Charges.AddNew("DTY", 15m);
			entryHeader2.Charges.AddNew("ADD", 20m);
			entryHeader2.Charges.AddNew("499", 30m);

			var entryLine3 = Factory.New<CusEntryLineForTesting>();
			entryHeader2.MergedLines.Add(entryLine3);
			entryLine3.CL_CustomsValue = 800m;
			invoiceLine5.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine3);
			entryLine3.InvoiceLines.Add(invoiceLine5);
			entryLine3.Fees.AddOrUpdate("EXS", 8m);
			entryLine3.Fees.AddOrUpdate("SIM", 8m);
			entryLine3.XX_HasMPF = true;

			var entryHeader3 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader3.CH_MessageType = "YYY";
			entryHeader3.Charges.AddNew("DTY", 15m);
			entryHeader3.Charges.AddNew("ADD", 20m);
			entryHeader3.Charges.AddNew("499", 30m);

			var entryLine4 = Factory.New<CusEntryLineForTesting>();
			entryHeader3.MergedLines.Add(entryLine4);
			entryLine4.CL_CustomsValue = 800m;
			invoiceLine5.JI_CL = entryLine4.PK;
			entryLine4.InvoiceLines.Add(invoiceLine5);
			entryLine4.Fees.AddOrUpdate("EXS", 8m);
			entryLine4.Fees.AddOrUpdate("SIM", 8m);
			entryLine4.XX_HasMPF = true;

			var landedCostHeader = ((ILandedCostHeader)declaration).TotalDutyTaxEntryFeeItems;
			CombineAssertions(() =>
			{
				AssertEquals("Total Duty", 25m, landedCostHeader["TDT"]);
				AssertEquals("Total Excise", 18m, landedCostHeader["EXC"]);
				AssertEquals("Total Other Duty", 68m, landedCostHeader["OTH"]);
				AssertEquals("Total MPF", 60m, landedCostHeader["ST1"]);

				AssertInvoiceLine(invoiceLine1, 1m, 1m, 4m, 7.5m);
				AssertInvoiceLine(invoiceLine2, 3m, 2m, 12m, 22.5m);
				AssertInvoiceLine(invoiceLine3, 2m, 3m, 8m, 0m);
				AssertInvoiceLine(invoiceLine4, 4m, 4m, 16m, 0m);
				AssertInvoiceLine(invoiceLine5, 15m, 8m, 28m, 30m);
			});
		}

		public void TestGenericLandedCostingHelper_MultipleEntryTypes()
		{
			string genericLandedCostingConfigXml = string.Format(@"
	<GenericLandedCostingConfig CountryCode='{0}'>
		<FormalEntryConfigs>
			<FormalEntryConfig EntryType='XXX' IsAdditionalEntryLine='Y' />
			<FormalEntryConfig EntryType='YYY' IsAdditionalEntryLine='N' />
		</FormalEntryConfigs>
		<LCEntryCustomsDisbursementCodeMappings>
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='DTY' LCDisbursementCode='TDT' Description='Customs Duty' HeaderLevelFee='True' />
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='EXS' LCDisbursementCode='EXC' Description='Excise' JobComInvoiceLineAmountPropertyName='XX_ExciseAmount' />
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='ADD' LCDisbursementCode='OTH' Description='Antidumping Duty' HeaderLevelFee='True' />
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='SIM' LCDisbursementCode='OTH' Description='SIMA' />
			<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode='499' LCDisbursementCode='ST1' Description='Merchandise Processing Fee' HeaderLevelFee='True' CusEntryLineApplicablePropertyName='XX_HasMPF' />
		</LCEntryCustomsDisbursementCodeMappings>
	</GenericLandedCostingConfig>", GlbCompany.CurrentCompany.Country);

			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.GenericLandedCostingConfigXml = genericLandedCostingConfigXml;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Env.CurrentCompany.LocalCurrency.Code;
			var invoiceLine1 = Factory.New<InvoiceLineForTesting>();
			invoiceLine1.JI_JZ = invoice.PK;
			invoiceLine1.JI_LinePrice = 100m;
			invoiceLine1.XX_ExciseAmount = 1m;
			var invoiceLine2 = Factory.New<InvoiceLineForTesting>();
			invoiceLine2.JI_JZ = invoice.PK;
			invoiceLine2.JI_LinePrice = 300m;
			invoiceLine2.XX_ExciseAmount = 2m;
			var invoiceLine3 = Factory.New<InvoiceLineForTesting>();
			invoiceLine3.JI_JZ = invoice.PK;
			invoiceLine3.JI_LinePrice = 100m;
			invoiceLine3.XX_ExciseAmount = 3m;
			var invoiceLine4 = Factory.New<InvoiceLineForTesting>();
			invoiceLine4.JI_JZ = invoice.PK;
			invoiceLine4.JI_LinePrice = 200m;
			invoiceLine4.XX_ExciseAmount = 4m;
			var invoiceLine5 = Factory.New<InvoiceLineForTesting>();
			invoiceLine5.JI_JZ = invoice.PK;
			invoiceLine5.JI_LinePrice = 800m;
			invoiceLine5.XX_ExciseAmount = 8m;

			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = "XXX";
			entryHeader1.Charges.AddNew("DTY", 10m);
			entryHeader1.Charges.AddNew("ADD", 20m);
			entryHeader1.Charges.AddNew("499", 30m);

			var entryLine1 = Factory.New<CusEntryLineForTesting>();
			entryHeader1.MergedLines.Add(entryLine1);
			entryLine1.CL_CustomsValue = 200m;
			entryLine1.Fees.AddOrUpdate("EXS", 3m);
			entryLine1.Fees.AddOrUpdate("SIM", 8m);
			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			entryLine1.InvoiceLines.Add(invoiceLine1);
			invoiceLine2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			entryLine1.InvoiceLines.Add(invoiceLine2);
			entryLine1.XX_HasMPF = true;

			var entryLine2 = Factory.New<CusEntryLineForTesting>();
			entryHeader1.MergedLines.Add(entryLine2);
			entryLine2.CL_CustomsValue = 300m;
			invoiceLine3.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
			entryLine2.InvoiceLines.Add(invoiceLine3);
			invoiceLine4.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
			entryLine2.InvoiceLines.Add(invoiceLine4);
			entryLine2.Fees.AddOrUpdate("EXS", 7m);
			entryLine2.Fees.AddOrUpdate("SIM", 12m);
			entryLine2.XX_HasMPF = false;

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = "YYY";
			entryHeader2.Charges.AddNew("DTY", 15m);
			entryHeader2.Charges.AddNew("ADD", 20m);
			entryHeader2.Charges.AddNew("499", 30m);

			var entryLine3 = Factory.New<CusEntryLineForTesting>();
			entryHeader2.MergedLines.Add(entryLine3);
			entryLine3.CL_CustomsValue = 800m;
			invoiceLine5.JI_CL = entryLine3.PK;
			entryLine3.InvoiceLines.Add(invoiceLine5);
			entryLine3.Fees.AddOrUpdate("EXS", 8m);
			entryLine3.Fees.AddOrUpdate("SIM", 8m);
			entryLine3.XX_HasMPF = true;

			var landedCostHeader = ((ILandedCostHeader)declaration).TotalDutyTaxEntryFeeItems;
			CombineAssertions(() =>
			{
				AssertEquals("Total Duty", 25m, landedCostHeader["TDT"]);
				AssertEquals("Total Excise", 18m, landedCostHeader["EXC"]);
				AssertEquals("Total Other Duty", 68m, landedCostHeader["OTH"]);
				AssertEquals("Total MPF", 60m, landedCostHeader["ST1"]);

				AssertInvoiceLine(invoiceLine1, 1m, 1m, 4m, 7.5m);
				AssertInvoiceLine(invoiceLine2, 3m, 2m, 12m, 22.5m);
				AssertInvoiceLine(invoiceLine3, 2m, 3m, 8m, 0m);
				AssertInvoiceLine(invoiceLine4, 4m, 4m, 16m, 0m);
				AssertInvoiceLine(invoiceLine5, 15m, 8m, 28m, 30m);
			});
		}

		void AssertInvoiceLine(IUltimateDistributee invoiceLine, ZDecimal duty, ZDecimal excise, ZDecimal other, ZDecimal specialTax1)
		{
			AssertEquals("Duty", duty, invoiceLine.LineDutyTaxEntryFeeItems["TDT"]);
			AssertEquals("Excise", excise, invoiceLine.LineDutyTaxEntryFeeItems["EXC"]);
			AssertEquals("Other Duty", other, invoiceLine.LineDutyTaxEntryFeeItems["OTH"]);
			AssertEquals("MPF", specialTax1, invoiceLine.LineDutyTaxEntryFeeItems["ST1"]);
		}

		class JobDeclarationForTesting : BaseJobDeclaration
		{
			public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public string GenericLandedCostingConfigXml;

			protected override LandedCostingHelper GetLandedCostingHelperCore()
			{
				return new GenericLandedCostingHelperForTesting(GenericLandedCostingConfigXml);
			}
		}

		class CusEntryLineForTesting : CusEntryLine
		{
			public CusEntryLineForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public ZBool XX_HasMPF { get; set; }
			public ZPropertyInfo XX_HasMPFInfo { get { return GetZPropertyInfo(nameof(XX_HasMPF)); } }
		}

		class InvoiceLineForTesting : BaseJobComInvoiceLine
		{
			public InvoiceLineForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public ZDecimal XX_ExciseAmount { get; set; }
			public ZPropertyInfo XX_ExciseAmountInfo { get { return GetZPropertyInfo(nameof(XX_ExciseAmount)); } }
		}

		class GenericLandedCostingHelperForTesting : GenericLandedCostingHelper
		{
			public GenericLandedCostingHelperForTesting(string configXml) : base()
			{
				StringReader stringReader = null;
				try
				{
					stringReader = new StringReader(configXml);
					using (var reader = XmlReader.Create(stringReader))
					{
						stringReader = null;
						config = new GenericLandedCostingConfig(XElement.Load(reader));
					}
				}
				finally
				{
					if (stringReader != null)
					{
						stringReader.Dispose();
					}
				}
			}
		}
	}
}
