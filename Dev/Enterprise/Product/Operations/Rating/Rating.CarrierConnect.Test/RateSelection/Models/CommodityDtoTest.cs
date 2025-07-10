using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Urs.Api.Integration.DTOs;

namespace Enterprise.Rating.CarrierConnect.Test;

class CommodityDtoTest : TestCaseWithFactory
{
	public void TestCommodityDto_FromUrsEntry_HasCommodityFields_MultiMapped()
	{
		var code = Factory.New<RefCommodityCode>();
		code.RH_Code = "Cod";
		code.RH_Description = "Desc";
		code.RH_UniversalCommodityGroup = "Universal";

		var code2 = Factory.New<RefCommodityCode>();
		code2.RH_Code = "Co2";
		code2.RH_Description = "Desc2";
		code2.RH_UniversalCommodityGroup = "Universal";
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
		entry.CommodityGroup = "Group";
		var dto = new CommodityDto(entry);
		var expectedDto = new CommodityDto
		{
			UniversalCommodityGroup = "Group",
			ProductClassCode = "Code",
			IsMultiMapped = true,
			RefCommodityCode = "",
			Description = "",
		};

		AssertEquals(expectedDto.ToString(), dto.ToString());
	}

	public void TestCommodityDto_FromUrsEntry_HasCommodityFields_NotMultiMapped()
	{
		var code = Factory.New<RefCommodityCode>();
		code.RH_Code = "Cod";
		code.RH_Description = "Desc";
		code.RH_UniversalCommodityGroup = "Universal";
		Factory.Save();

		var entry = new UrsRateEntry(
			new TradeServiceDto
			{
				Product = new ProductDto
				{
					UniversalCode = "Universal",
					Classification = new ProductClassDto { Code = "Code", Name = "Name" }
				}
			},
			Factory
		)
		{
			CommodityGroup = "Universal"
		};

		var dto = new CommodityDto(entry);
		var expectedDto = new CommodityDto
		{
			UniversalCommodityGroup = "Universal",
			ProductClassCode = "Code",
			IsMultiMapped = false,
			RefCommodityCode = "Cod",
			Description = "Desc",
		};

		AssertEquals(expectedDto.ToString(), dto.ToString());
	}
}
