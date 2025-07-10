using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class ExtraOrderLineDetailsTest : TestCase
	{
		#region TestProperties

		public void TestProperties()
		{
			var allocationInfos = new[] { Mock.Of<IWarehouseCustomsLineAllocationInfo>() };
			var extraOrderLineDetails = new ExtraOrderLineDetails(
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { Address1 = "Extra st" },
				new ZString[] { "Extra Detail" },
				new[] { new ExtraOrderLineDetails.CustomsDetail("TP1", "WHO=BOB THE BUILDER"), new ExtraOrderLineDetails.CustomsDetail("TP2", "WHO=WENDY THE DESTROYER") },
				"New P",
				"New PA1",
				"New PA2",
				"New PA3",
				"New SN",
				"AllocationKey");

			AssertEquals("Extra st", extraOrderLineDetails.SupplierAddress.Address1);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Extra Detail" }, extraOrderLineDetails.ExtraClassificationDetails);
			var extraCustomsDetails = extraOrderLineDetails.ExtraCustomsDetails.ToArray();
			AssertEquals("extraCustomsDetails.Length", 2, extraCustomsDetails.Length);
			AssertEquals("extraCustomsDetails[0].Type", "TP1", extraCustomsDetails[0].Type);
			AssertEquals("extraCustomsDetails[0].Data", "WHO=BOB THE BUILDER", extraCustomsDetails[0].Data);
			AssertEquals("extraCustomsDetails[1].Type", "TP2", extraCustomsDetails[1].Type);
			AssertEquals("extraCustomsDetails[1].Data", "WHO=WENDY THE DESTROYER", extraCustomsDetails[1].Data);
			AssertEquals("New P", extraOrderLineDetails.NewProductCode);
			AssertEquals("New PA1", extraOrderLineDetails.NewPartAttribute1);
			AssertEquals("New PA2", extraOrderLineDetails.NewPartAttribute2);
			AssertEquals("New PA3", extraOrderLineDetails.NewPartAttribute3);
			AssertEquals("New SN", extraOrderLineDetails.NewSerialNumber);
			AssertEquals("AllocationKey", extraOrderLineDetails.AllocationKey);
		}

		#endregion
	}
}
