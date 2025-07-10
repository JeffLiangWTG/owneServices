using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class ProductPermitCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<ProductPermitCusSupporting>
	{
		public ProductPermitCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.PermitNumber)
		{
		}
	}
}
