using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class PackageDimensionsInfo : DataObjectInfo
	{
		public PackageDimensionsInfo()
			: base()
		{
			PackType = string.Empty;
			EmptyWeight = 0m;
			Weight = 0m;
			WeightUQ = string.Empty;
			Length = 0m;
			Width = 0m;
			Height = 0m;
			DimensionUQ = string.Empty;
		}

		public string PackType { get; set; }

		public decimal EmptyWeight { get; set; }

		public decimal Weight { get; set; }

		public string WeightUQ { get; set; }

		public decimal Length { get; set; }

		public decimal Width { get; set; }

		public decimal Height { get; set; }

		public string DimensionUQ { get; set; }
	}
}
