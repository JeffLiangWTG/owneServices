using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class PreviousPermitNoCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<PreviousPermitNoCusSupporting>
	{
		public PreviousPermitNoCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.PreviousPermitNumber)
		{
		}
	}
}
