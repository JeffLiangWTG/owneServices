using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class CitesPermitCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<CitesPermitCusSupporting>
	{
		public CitesPermitCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.CitesImportPermit)
		{
		}
	}
}
