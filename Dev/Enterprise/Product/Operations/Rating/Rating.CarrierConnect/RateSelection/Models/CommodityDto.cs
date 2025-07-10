#nullable enable
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.CarrierConnect;

public class CommodityDto
{
	public CommodityDto() { }

	public CommodityDto(IRateEntry entry)
	{
		var factory = new BusinessObjectFactory();

		if (entry is UrsRateEntry ursEntry)
		{
			UniversalCommodityGroup = ursEntry.CommodityGroup;
			ProductClassCode = ursEntry.ProductClassCode;
			IsMultiMapped = ursEntry.RefCommodities.Count > 1;
			if (ursEntry.RefCommodities.Count == 1)
			{
				InitialiseFromRefCommodity(ursEntry.RefCommodities[0]);
			}
		}
		else if (!string.IsNullOrEmpty(entry.TI_RH_NKCommodityCode))
		{
			InitialiseFromRefCommodity(
				factory.LoadTop1<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, entry.TI_RH_NKCommodityCode))
			);
		}
	}

	void InitialiseFromRefCommodity(RefCommodityCode code)
	{
		RefCommodityCode = code.RH_Code;
		Description = code.RH_DescriptionMultilingual;
	}

	public string? UniversalCommodityGroup { get; init; }

	public string? ProductClassCode { get; init; }

	public string? RefCommodityCode { get; set; }

	public bool IsMultiMapped { get; init; }

	public string Description { get; set; } = string.Empty;

	public override string ToString() => string.Join("|", UniversalCommodityGroup, ProductClassCode, RefCommodityCode, Description);
}
