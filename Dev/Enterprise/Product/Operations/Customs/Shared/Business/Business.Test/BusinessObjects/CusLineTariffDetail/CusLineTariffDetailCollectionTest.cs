using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetailCollection<CusLineTariffDetail>))]
	public class CusLineTariffDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestParentIDAndParentTableCode()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var collection = new CusLineTariffDetailCollection<CusLineTariffDetail>(pivot);
			var item = collection.AddNew();
			AssertEquals("item.BZ_ParentTableCode", CusClassPartPivotSchema.Constants.Prefix, item.BZ_ParentTableCode);
			AssertEquals("item.BZ_ParentID", pivot.PK, item.BZ_ParentID);

			var item2 = Factory.New<CusLineTariffDetail>();
			item2.BZ_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			item2.BZ_ParentID = ZGuid.Invalid;
			collection.Add(item2);
			AssertEquals("item2.BZ_ParentTableCode", CusClassPartPivotSchema.Constants.Prefix, item2.BZ_ParentTableCode);
			AssertEquals("item2.BZ_ParentID", pivot.PK, item2.BZ_ParentID);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			return new CusLineTariffDetailCollection<CusLineTariffDetail>(pivot);
		}

		public void TestAddNewWithTypeAndCode()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var collection = new CusLineTariffDetailCollection<CusLineTariffDetail>(pivot);
			var tariffDetail = collection.AddNew("12A", "1020304050");
			AssertEquals("CusLineTariffDetailCollection.Count", 1, collection.Count);
			AssertEquals("tariffDetail.BZ_Type", "12A", tariffDetail.BZ_Type);
			AssertEquals("tariffDetail.BZ_Tariff", "1020304050", tariffDetail.BZ_Tariff);
		}
	}
}
