using System.Text;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(OrderManagerRequestMappingRegistryItem))]
	sealed class OrderManagerRequestMappingRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<OrderManagerRequestMappingCollection>
	{
		protected override StronglyTypedRegistryItem<OrderManagerRequestMappingCollection, OrderManagerRequestMappingCollection> GetNewRegistryItem()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new OrderManagerRequestMappingCollection(fallbackLevel, Factory);

			var item = new OrderManagerRequestMappingRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, collection);

			return item;
		}
	}

	[TestedType(typeof(OrderManagerRequestMappingDataType))]
	class OrderManagerRequestMappingDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<OrderManagerRequestMappingDataType>
	{
		protected override OrderManagerRequestMappingDataType GetNewDataType() => new OrderManagerRequestMappingDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var first = new OrderManagerRequestMappingCollection();

			var request1 = new OrderManagerRequestMapping();
			first.Add(request1);
			request1.Request = RequestMappingCodes.CargoDateVsShipmentWindow;
			request1.RequestType = "AAA";

			var request2 = new OrderManagerRequestMapping();
			first.Add(request2);
			request2.Request = RequestMappingCodes.CargoDateVsExWorksDate;
			request2.RequestType = "AAB";

			var firstSerialised =
				@"<?xml version=""1.0"" encoding=""utf-16""?><OrderManagerRequestMappings xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><OrderManagerRequestMapping><RequestType>AAA</RequestType><Request>CDS</Request></OrderManagerRequestMapping><OrderManagerRequestMapping><RequestType>AAB</RequestType><Request>CDE</Request></OrderManagerRequestMapping></OrderManagerRequestMappings>";

			var second = new OrderManagerRequestMappingCollection();

			var request3 = new OrderManagerRequestMapping();
			second.Add(request3);
			request3.Request = RequestMappingCodes.CargoDateVsRequiredInStoreDate;
			request3.RequestType = "AAC";

			var request4 = new OrderManagerRequestMapping();
			second.Add(request4);
			request4.Request = RequestMappingCodes.BookedQuantity;

			var secondSerialised =
				@"<?xml version=""1.0"" encoding=""utf-16""?><OrderManagerRequestMappings xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><OrderManagerRequestMapping><RequestType>AAC</RequestType><Request>CDR</Request></OrderManagerRequestMapping><OrderManagerRequestMapping><RequestType /><Request>BKQ</Request></OrderManagerRequestMapping></OrderManagerRequestMappings>";

			return
			[
				new ValidSampleAndBinaryValueInDB(first, Encoding.Unicode.GetBytes(firstSerialised)),
				new ValidSampleAndBinaryValueInDB(second, Encoding.Unicode.GetBytes(secondSerialised)),
			];
		}

		protected override string ExpectedEditorName => "OrderManagerRequestMappingRegistryItemEditor";
	}
}
