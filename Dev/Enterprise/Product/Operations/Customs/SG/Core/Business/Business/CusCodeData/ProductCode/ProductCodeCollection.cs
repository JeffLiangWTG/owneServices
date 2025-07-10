using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business
{
	public class ProductCodeCollection : CusCodeDataCollection<ProductCode>
	{
		public ProductCodeCollection(Classification classification)
			: base(classification, CusCodeDataTypeList.Codes.ProductCode)
		{
			MaxCountValidationEnable(MaxCountForValidation);
		}

		public ProductCodeCollection(CusClassPartPivot pivot)
			: base(pivot, CusCodeDataTypeList.Codes.ProductCode)
		{
			MaxCountValidationEnable(MaxCountForValidation);
		}

		const int MaxCountForValidation = 5;
	}
}
