using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsReleaseCapturedInfo : DataObjectInfo
	{
		#region Constructor

		public WhsReleaseCapturedInfo()
		{
			Attribute1 = "";
			Attribute2 = "";
			Attribute3 = "";
			SerialNumber = "";
		}

		public WhsReleaseCapturedInfo(string attribute1, string attribute2, string attribute3, string serialNumber, decimal quantity)
		{
			if (quantity <= 0)
			{
				throw new ArgumentException("Quantity must be greater than 0.", nameof(quantity));
			}
			if (string.IsNullOrEmpty(attribute1) && string.IsNullOrEmpty(attribute2) && string.IsNullOrEmpty(attribute3) && string.IsNullOrEmpty(serialNumber))
			{
				throw new ArgumentException("at least one of release captured attributes must be non empty");
			}
			Quantity = quantity;
			Attribute1 = attribute1;
			Attribute2 = attribute2;
			Attribute3 = attribute3;
			SerialNumber = serialNumber;
		}

		#endregion

		#region Properties

		public bool IsCaptured { get; set; }
		public string Attribute1 { get; set; }
		public string Attribute2 { get; set; }
		public string Attribute3 { get; set; }
		public string SerialNumber { get; set; }
		public decimal Quantity { get; set; }

		#endregion
	}
}
