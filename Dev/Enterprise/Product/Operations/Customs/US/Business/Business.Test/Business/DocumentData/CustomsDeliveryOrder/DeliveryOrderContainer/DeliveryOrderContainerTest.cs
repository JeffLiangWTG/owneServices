using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DeliveryOrderContainer))]
	sealed class DeliveryOrderContainerTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DeliveryOrderContainer>
	{
		public void TestDefault()
		{
			DeliveryOrderContainer line = Factory.New<DeliveryOrderContainer>();
			AssertEquals(CusAddInfoSchema.Constants.Prefix, line.B7_ParentTableCode);
			AssertEquals(CusAddInfoTypeAttribute.Codes.USDeliveryOrderContainer, line.B7_Type);
		}

		public void TestWeightAndUQ()
		{
			var line = Factory.New<DeliveryOrderContainer>();
			line.US_WeightUQ = "KG";
			AssertEquals("", line.WeightAndUQ);
			line.US_Weight = 1.345m;
			AssertEquals("1.345 KG", line.WeightAndUQ);
		}

		public void TestDeleteWhenEmpty()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			DeliveryOrderHeader header = declaration.DeliveryOrderHeaders.AddNew();
			DeliveryOrderContainer container = header.DeliveryOrderContainers.AddNew();
			container.US_ContainerNumber = "TURE2343251";
			Factory.Save();
			AssertEquals(false, container.IsDeleted);
			container.US_ContainerNumber = ZString.Empty;
			Factory.Save();
			AssertEquals(true, container.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var header = declaration.DeliveryOrderHeaders.AddNew();
			var container = header.DeliveryOrderContainers.AddNew();
			container.US_ContainerNumber = "TURE2343251";
			return container;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<DeliveryOrderContainer>();
		}
	}
}
