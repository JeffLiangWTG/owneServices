#nullable enable
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Rating.Business;

public struct UrsContainer
{
	public string Code { get; set; }
	public decimal? PivotWeight { get; set; }
	public decimal? PayloadWeight { get; set; }
	public decimal? PayloadVolume { get; set; }
	public ZGuid[] ContainerPKs { get; set; }
	public readonly bool IsMapped => !ContainerPKs.IsNullOrEmpty();
}
