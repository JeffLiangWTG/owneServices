using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class HighTechLicenseCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<HighTechLicenseCusSupporting>
	{
		public HighTechLicenseCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.ShtcImportPermit)
		{
		}
	}
}
