using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DeliveryOrderLine))]
	sealed class DeliveryOrderLineTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DeliveryOrderLine>
	{
		public void TestDefault()
		{
			DeliveryOrderLine line = Factory.New<DeliveryOrderLine>();
			AssertEquals(CusAddInfoSchema.Constants.Prefix, line.B7_ParentTableCode);
			AssertEquals(CusAddInfoTypeAttribute.Codes.USDeliveryOrderLine, line.B7_Type);
		}

		public void TestDeleteWhenEmpty()
		{
			DeliveryOrderLine line = GetNewBusinessObjectForDeleteTest(Factory) as DeliveryOrderLine;
			line.US_GoodsDescription = "Goods";
			Factory.Save();
			AssertEquals(false, line.IsDeleted);
			line.US_GoodsDescription = ZString.Empty;
			Factory.Save();
			AssertEquals(true, line.IsDeleted);
		}

		public void TestUS_WeightInKilograms()
		{
			DeliveryOrderLine line = Factory.New<DeliveryOrderLine>();
			line.US_WeightInKilograms = 500.54123M;
			AssertEquals(500.54M, line.US_WeightInKilograms);
			line.US_WeightInKilograms = 100.1M;
			AssertEquals(100.1M, line.US_WeightInKilograms);
			line.US_WeightInKilograms = 90M;
			AssertEquals(90M, line.US_WeightInKilograms);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var header = declaration.DeliveryOrderHeaders.AddNew();
			var result = header.DeliveryOrderLines.AddNew();
			result.US_GoodsDescription = "HELLO";
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<DeliveryOrderLine>();
		}
	}
}
