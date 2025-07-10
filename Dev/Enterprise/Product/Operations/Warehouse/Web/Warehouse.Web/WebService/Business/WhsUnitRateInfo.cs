using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsUnitRateInfo : DataObjectInfo
	{
		#region Constructors

		public WhsUnitRateInfo(OrgPartUnit unit)
			: this()
		{
			if (unit != null)
			{
				this.Units = unit.OF_QuantityInParent;
				this.Package = unit.OF_PackType;
				this.Parent = unit.OF_ParentPackType;
				this.Weight = unit.OF_Weight;
				this.Cubic = unit.OF_Cubic;
				this.Width = unit.OF_Width;
				this.Depth = unit.OF_Depth;
				this.Height = unit.OF_Height;
				this.MeasurementUQ = string.Empty;
			}
		}

		public WhsUnitRateInfo()
			: this(0, string.Empty, string.Empty, 0, 0, 0, 0, 0, string.Empty)
		{
		}

		public WhsUnitRateInfo(decimal units, string package, string parent, decimal weight, decimal cubic, decimal width, decimal depth, decimal height, string measurementUQ)
		{
			this.Units = units;
			this.Package = package;
			this.Parent = parent;
			this.Weight = weight;
			this.Cubic = cubic;
			this.Width = width;
			this.Depth = depth;
			this.Height = height;
			this.MeasurementUQ = measurementUQ;
		}

		#endregion

		#region Properties

		public decimal Units { get; set; }

		public string Package { get; set; }

		public string Parent { get; set; }

		public decimal Width { get; set; }

		public decimal Depth { get; set; }

		public decimal Height { get; set; }

		public string MeasurementUQ { get; set; }

		public decimal Weight { get; set; }

		public decimal Cubic { get; set; }

		#endregion
	}
}
