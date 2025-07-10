using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;

namespace Enterprise.Customs.Business.Testing.CommonGoodsItemsIntegration
{
	public abstract class BaseCommonGoodsItemsIntegratorTest<TJobDeclaration, TCusEntryHeader, TCommonGoodsItemsIntegrator, TNctsHeaderToAttachCollection> : TestCaseWithFactory
		where TJobDeclaration : BaseJobDeclaration
		where TCusEntryHeader : CusEntryHeader
		where TCommonGoodsItemsIntegrator : ICommonGoodsItemsIntegrator
		where TNctsHeaderToAttachCollection : IBusinessObjectCollection
	{
		public void TestGetCommonGoodsItemsForIntegration()
		{
			(var entry, var line1, _) = CreateEntryWithTwoInvoiceLines(Factory);
			var collection = entry.CommonGoodsItemsIntegrator.GetCommonGoodsItemsForIntegration().ToArray();
			AssertEquals(2, collection.Length);
			AssertExpectedPropertiesOnGoodsItem(collection[0], line1);
		}

		public void TestCopyCommonGoodsItems()
		{
			var declaration = Factory.New<TJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var commonGoodsItemsIntegrator = entryHeader.CommonGoodsItemsIntegrator;
			var goodsItem = CreateCommonGoodsItem();
			commonGoodsItemsIntegrator.CopyCommonGoodsItems(new ICommonGoodsItem[] { goodsItem, new CommonGoodsItem() }, 0);
			AssertEquals(3, declaration.InvoiceLines.Count);
			AssertExpectedPropertiesOnInvoiceLine(declaration.InvoiceLines[1]);
		}

		public void TestTheOtherCollectionToAttach()
		{
			var header = Factory.New<TCusEntryHeader>();
			var commonGoodsItemsIntegrator = (TCommonGoodsItemsIntegrator)Activator.CreateInstance(typeof(TCommonGoodsItemsIntegrator), header);
			AssertType<TNctsHeaderToAttachCollection>(commonGoodsItemsIntegrator.TheOtherCollectionToAttach());
		}

		protected abstract (CusEntryHeader, BaseJobComInvoiceLine line1, BaseJobComInvoiceLine line2) CreateEntryWithTwoInvoiceLines(BusinessObjectFactory factory);
		protected abstract void AssertExpectedPropertiesOnGoodsItem(ICommonGoodsItem goodsItem, BaseJobComInvoiceLine line1);

		protected abstract ICommonGoodsItem CreateCommonGoodsItem();
		protected abstract void AssertExpectedPropertiesOnInvoiceLine(BaseJobComInvoiceLine invoiceLine);
	}
}
