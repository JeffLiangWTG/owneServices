using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USFSISLot))]
	class USFSISLotTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<USFSISLot>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return FSISLine.Lots.AddNew();
		}

		protected override IEnumerable<USFSISLot> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().FSISLines.AddNew().Lots.AddNew();
		}

		USInvoiceLineFSISLine FSISLine
		{
			get { return fFSISLine ?? (fFSISLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().FSISLines.AddNew()); }
		}
		USInvoiceLineFSISLine fFSISLine;

		public void TestNetWeightInPounds()
		{
			var lot = FSISLine.Lots.AddNew();
			lot.US_NetWeight = 1.0m;
			lot.US_WeightUQ = "KG";
			AssertEquals(2.2m, lot.NetWeightInPounds);
		}

		public void TestProductCategory()
		{
			var lot = FSISLine.Lots.AddNew();
			lot.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.EEP;
			lot.US_ProductCharacteristicQualifier = EEPCharacteristicList.Codes._5D;
			AssertEquals("5D EPUnPast (blends of whole egg, egg whites and or yolks with or without added ingredients)", lot.ProductCategory);
			AssertEquals(ZString.Empty, lot.ProductGroup);

			lot.US_ProductCharacteristicQualifier = EEPCharacteristicList.Codes._4C;
			AssertEquals("4C EPDried – Pasteurized (Frozen or Liquid)", lot.ProductCategory);
			AssertEquals("Yolks (with or without added ingredients)", lot.ProductGroup);
		}

		public void TestProductCategoryAndProcessCategory()
		{
			var lot = FSISLine.Lots.AddNew();
			lot.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.EEP;
			lot.US_ProductCharacteristicQualifier = EEPCharacteristicList.Codes._5D;
			AssertEquals("5D EPUnPast (blends of whole egg, egg whites and or yolks with or without added ingredients)", lot.ProductCategory);
			AssertEquals(ZString.Empty, lot.ProductGroup);

			lot.US_ProductCharacteristicQualifier = EEPCharacteristicList.Codes._4C;
			AssertEquals("4C EPDried – Pasteurized (Frozen or Liquid)", lot.ProductCategory);
			AssertEquals("Yolks (with or without added ingredients)", lot.ProductGroup);

			lot.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.FCNS;
			AssertEquals("Fully Cooked \u2013 Not Shelf Stable", lot.ProcessCategory);
		}

		public void TestProductDescription()
		{
			FSISLine.US_CommercialDescription = ZString.Empty;
			var lot = FSISLine.Lots.AddNew();
			AssertEquals(ZString.Empty, lot.US_ProductDescription);

			lot.US_ProductDescription = "testing1";
			AssertEquals("testing1", lot.US_ProductDescription);

			FSISLine.US_CommercialDescription = "testing2";
			AssertEquals("testing1", lot.US_ProductDescription);

			lot.US_ProductDescription = ZString.Empty;
			AssertEquals("testing2", lot.US_ProductDescription);
		}
	}
}
