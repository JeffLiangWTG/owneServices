using System;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsCartonSizeInfo : DataObjectInfo
	{
		#region Constructor

		public WhsCartonSizeInfo()
			: base()
		{
			Code = "";
			WeightUQ = string.Empty;
			DimensionUQ = string.Empty;
			Length = 0m;
			Width = 0m;
			Height = 0m;
			EmptyWeight = 0m;
		}

		public WhsCartonSizeInfo(WhsCartonSize cartonSize)
			: this()
		{
			if (cartonSize == null)
			{
				throw new ArgumentNullException(nameof(cartonSize), "CartonSize is required");
			}

			Code = cartonSize.WCS_Code;
			WeightUQ = cartonSize.WCS_WeightUQ;
			DimensionUQ = cartonSize.WCS_DimensionUQ;
			Length = cartonSize.WCS_Length;
			Width = cartonSize.WCS_Width;
			Height = cartonSize.WCS_Height;
			EmptyWeight = cartonSize.WCS_EmptyWeight;
		}

		#endregion

		#region Properties

		public string Code { get; set; }

		public string WeightUQ { get; set; }

		public decimal Length { get; set; }

		public decimal Width { get; set; }

		public decimal Height { get; set; }

		public string DimensionUQ { get; set; }

		public decimal EmptyWeight { get; set; }

		#endregion
	}
}
