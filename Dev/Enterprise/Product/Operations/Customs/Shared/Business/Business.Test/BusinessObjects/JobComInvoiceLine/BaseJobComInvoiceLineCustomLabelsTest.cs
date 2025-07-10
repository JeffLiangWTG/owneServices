using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobComInvoiceLine))]
	class BaseJobComInvoiceLineCustomLabelsTest : BusinessObjectWithCustomLabelsTestCase
	{
		public void TestCustomLabelsList_SerialNumber()
		{
			var newDec = Factory.New<BaseJobDeclaration>();
			var provider = new BaseJobComInvoiceLine.CustomLabelsProvider(newDec);
			var list = provider.GetCustomFields(null, Factory);
			AssertEquals("Should have 16 items", 16, list.Count);

			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_OH_Supplier = GlbBranch.CurrentBranch.OrgProxy.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.Invoices.AddNew();
			var line = dec.Invoices[0].JobComInvoiceLines.AddNew();

			provider = new BaseJobComInvoiceLine.CustomLabelsProvider(line.Declaration);
			list = provider.GetCustomFields(GlbBranch.CurrentBranch.OrgProxy, Factory);
			AssertEquals("Should have the correct number of items", 20, list.Count);
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomAttrib1));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomAttrib2));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomAttrib3));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomAttrib4));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomAttrib5));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomAttrib6));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomDecimal1));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomDecimal2));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomDecimal3));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomDate1));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomDate2));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomDate3));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomFlag1));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomFlag2));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomFlag3));
			Assert(list.Contains(AutoJobComInvoiceLine.Schema.JI_CustomTextBlob1));
			Assert(list.Contains(provider.ConfigOrgProvider.PartAttribute1));
			Assert(list.Contains(provider.ConfigOrgProvider.PartAttribute2));
			Assert(list.Contains(provider.ConfigOrgProvider.PartAttribute3));
			Assert(list.Contains(provider.ConfigOrgProvider.SerialNumber));
		}

		public void TestCustomLabelsList_SerialNumberCaption()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_OH_Supplier = GlbBranch.CurrentBranch.OrgProxy.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.Invoices.AddNew();
			var line = dec.Invoices[0].JobComInvoiceLines.AddNew();

			var provider = new BaseJobComInvoiceLine.CustomLabelsProvider(line.Declaration);
			var list = provider.GetCustomFields(GlbBranch.CurrentBranch.OrgProxy, Factory);
			Assert(list.Contains(provider.ConfigOrgProvider.SerialNumber));
			var serialNumberCustomField = list.GetFieldByPropertyName(provider.ConfigOrgProvider.SerialNumber);
			AssertEquals("Serial Number", serialNumberCustomField.Caption);
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			if (GetType() == typeof(BaseJobComInvoiceLineCustomLabelsTest))
			{
				Assert($"Covered by {nameof(FetchStrategies.Testing.JobComInvoiceLineFetchStrategyTest)}.", true);
				return;
			}

			base.TestCalcPropertiesWithDbHitsUseFetchHints();
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			return invoiceLine;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<BaseJobDeclaration>();

			var importer = factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			var result = invoiceHeader.InvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();

			var link = (result.AdditionalEntryLineLinks.Count > 0 ? result.AdditionalEntryLineLinks[0] : null)
				?? result.AdditionalEntryLineLinks.AddNew();
			link.BU_CL = entryLine.PK;
			link.BU_JI = result.PK;

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var invoiceLine = (BaseJobComInvoiceLine)base.GetNewBusinessObjectForSettingValueCallsRefreshBindingTest();
			invoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			return invoiceLine;
		}

		protected override System.Collections.Generic.Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				System.Collections.Generic.Dictionary<string, IZType> result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result.Add(BaseJobComInvoiceLine.Schema.JI_LineNo, (ZShort)1);
				return result;
			}
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO) => new BaseJobComInvoiceLine.CustomLabelsProvider(((BaseJobComInvoiceLine)bO).Declaration);
	}
}
