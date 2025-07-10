using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using Enterprise.Customs.NZ.Business.Declaration;
	using NUnit.Framework;

	[TestedType(typeof(ItemPackagingData))]
	public class ItemPackagingDataTest : Customs.Business.Testing.CusCodeDataTest<ItemPackagingData>
	{
		public void TestHumanReadableNameForCY_Data()
		{
			var itemPackaging = Factory.New<ItemPackagingData>();
			itemPackaging.CY_Code = "AAA";
			AssertEquals("TSW (AAA)", itemPackaging.CY_DataInfo.HumanReadableName);

			itemPackaging.CY_Code = "";
			AssertEquals("ItemPackaging", itemPackaging.CY_DataInfo.HumanReadableName);

			itemPackaging.CY_Code = "BBB";
			AssertEquals("TSW (BBB)", itemPackaging.CY_DataInfo.HumanReadableName);

			var copyCode = Factory.New<ItemPackagingData>();
			copyCode.CopyPersistentValuesFrom(itemPackaging);
			AssertEquals("TSW (BBB)", copyCode.CY_DataInfo.HumanReadableName);

			copyCode = (ItemPackagingData)itemPackaging.Clone();
			AssertEquals("TSW (BBB)", copyCode.CY_DataInfo.HumanReadableName);
		}

		public void TestCY_Data()
		{
			var itemPackaging = Factory.New<ItemPackagingData>();
			itemPackaging.CY_Data = "dsfsdf";
			AssertEquals("DSFSDF", itemPackaging.CY_Data);
		}

		public void TestSetDefaultValues()
		{
			var itemPackaging = Factory.New<ItemPackagingData>();
			AssertEquals("NZItemPackaging Type", CusAddInfoTypeAttribute.Codes.NZItemPackaging, itemPackaging.CY_Type);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ItemPackages.AddNew().ItemPackages.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return ItemPackages.AddNew();
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					var invoice = Declaration.Invoices.AddNew();
					fInvoiceLine = invoice.JobComInvoiceLines.AddNew();
					fInvoiceLine.CommodityProducts.AddNew();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		ItemPackagingDataCollection ItemPackages
		{
			get
			{
				if (fItemPackages == null)
				{
					fItemPackages = new ItemPackagingDataCollection(InvoiceLine.ItemPackages.AddNew());
				}
				return fItemPackages;
			}
		}
		ItemPackagingDataCollection fItemPackages;
	}
}
