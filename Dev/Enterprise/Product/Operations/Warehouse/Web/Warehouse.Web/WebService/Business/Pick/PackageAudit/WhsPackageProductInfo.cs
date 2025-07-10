using System;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	[Serializable]
	public class WhsPackageProductInfo
	{
		public WhsPackageProductInfo()
			: this(Guid.Empty, "", "", 0m, 0m, 0m, "")
		{
		}

		public WhsPackageProductInfo(Guid productPK, string productCode, string productDescription, decimal quantity, decimal expectedQty, decimal productWeight, string productWeightUQ)
		{
			ProductPK = productPK;
			ProductCode = productCode;
			ProductDescription = productDescription;
			Quantity = quantity;
			ExpectedQty = expectedQty;
			ProductWeight = productWeight;
			ProductWeightUQ = productWeightUQ;
		}

		public Guid ProductPK { get; set; }
		public string ProductCode { get; set; }
		public string ProductDescription { get; set; }
		public decimal Quantity { get; set; }
		public decimal ExpectedQty { get; set; }
		public decimal ProductWeight { get; set; }
		public string ProductWeightUQ { get; set; }
	}
}
