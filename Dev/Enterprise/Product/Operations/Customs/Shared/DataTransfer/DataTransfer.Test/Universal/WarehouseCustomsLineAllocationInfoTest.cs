using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsLineAllocationInfoTest : TestCase
	{
		public void TestAllocationKey()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No AddInfo with given key", ZString.Empty, allocationInfo.AllocationKey);

				addInfos.Add(new AddInfo
				{
					Key = "AllocationKey",
					Value = "K1"
				});
				AssertEquals("Valid AddInfo", "K1", allocationInfo.AllocationKey);
			});
		}

		public void TestQuantity()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No AddInfo with given key", ZDecimal.Zero, allocationInfo.Quantity);

				var addInfo = new AddInfo
				{
					Key = "Quantity",
					Value = "a"
				};
				addInfos.Add(addInfo);
				AssertEquals("Invalid AddInfo value", ZDecimal.Zero, allocationInfo.Quantity);

				addInfo.Value = "1.2";
				AssertEquals("Valid AddInfo", 1.2m, allocationInfo.Quantity);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			addInfos = new List<AddInfo>();
			allocationInfo = new WarehouseCustomsLineAllocationInfo(addInfos);
		}
		WarehouseCustomsLineAllocationInfo allocationInfo;
		List<AddInfo> addInfos;
	}
}
