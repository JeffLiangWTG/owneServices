using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Urs.Api.Integration.DTOs;

namespace Enterprise.Rating.Business.Test;

class UrsRateEntryTest : TestCaseWithFactory
{
	public void TestUrsRateEntry_HasCommodityFields()
	{
		var code = Factory.New<RefCommodityCode>();
		code.RH_Code = "Cod";
		code.RH_UniversalCommodityGroup = "Universal";
		Factory.Save();

		var entry = new UrsRateEntry(
			new TradeServiceDto
			{
				Product = new ProductDto
				{
					UniversalCode = "Universal",
					Classification = new ProductClassDto
					{
						Code = "Code",
						Name = "Name"
					},
				}
			},
			Factory);

		CombineAssertions(() =>
		{
			AssertEquals("Code", entry.ProductClassCode);
			AssertEquals("Name", entry.ProductClassName);
		});
	}
}
