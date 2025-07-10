using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class QuotaPermitNumberCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<QuotaPermitNumberCusSupporting>
	{
		public QuotaPermitNumberCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.TariffRateQuotaCertificate)
		{
		}
	}
}
