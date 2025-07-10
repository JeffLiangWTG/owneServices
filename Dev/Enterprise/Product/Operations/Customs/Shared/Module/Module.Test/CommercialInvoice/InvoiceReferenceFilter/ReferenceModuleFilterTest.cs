using System;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(ReferenceModuleFilter))]
	[CountrySpecificTest("AU")]
	sealed class ReferenceModuleFilterTest : ModuleTextFilterTest
	{
		public void TestReferenceFilter()
		{
			CommercialInvoiceFilterBusinessObject filterBizObj = (CommercialInvoiceFilterBusinessObject)Activator.CreateInstance(typeof(CommercialInvoiceFilterBusinessObject));
			BaseJobComInvoiceHeader comInvoice1 = Factory.New<BaseJobComInvoiceHeader>();
			JobComInvoiceHeaderRefs ref1 = comInvoice1.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceNumber = "MB001";
			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.MB;
			BaseJobComInvoiceHeader comInvoice2 = Factory.New<BaseJobComInvoiceHeader>();
			JobComInvoiceHeaderRefs ref2 = comInvoice2.InvoiceHeaderRefs.AddNew();
			ref2.J2_ReferenceNumber = "CN900800";
			ref2.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
			BaseJobComInvoiceHeader comInvoice3 = Factory.New<BaseJobComInvoiceHeader>();
			Factory.Save();
			ReferenceModuleFilter filter = (ReferenceModuleFilter)filterBizObj[CommercialInvoiceFilterConstants.References];
			filter.IsActive = true;
			filter.Property = "MB001";
			BaseJobComInvoiceHeader[] filteredCommercialInvoices = Factory.Load<BaseJobComInvoiceHeader>(filterBizObj.Filter);
			AssertEquals(1, filteredCommercialInvoices.Length);
			AssertEquals(comInvoice1, filteredCommercialInvoices[0]);
			filter.Property = "CN900800";
			filter.ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
			filteredCommercialInvoices = Factory.Load<BaseJobComInvoiceHeader>(filterBizObj.Filter);
			AssertEquals(1, filteredCommercialInvoices.Length);
			AssertEquals(comInvoice2, filteredCommercialInvoices[0]);
			filter.Property = "ABC";
			filter.ComparisonOperator = ReferenceModuleFilter.ComparisonConstants.NotFound;
			filter.ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
			AssertEquals(true, filter.PropertyInfo.ReadOnly);
			filteredCommercialInvoices = Factory.Load<BaseJobComInvoiceHeader>(filterBizObj.Filter);
			AssertEquals(2, filteredCommercialInvoices.Length);
			AssertEquals(true, filteredCommercialInvoices.Contains(comInvoice1));
			AssertEquals(true, filteredCommercialInvoices.Contains(comInvoice3));
		}

		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new ReferenceModuleFilter("moo");
		}
	}
}
