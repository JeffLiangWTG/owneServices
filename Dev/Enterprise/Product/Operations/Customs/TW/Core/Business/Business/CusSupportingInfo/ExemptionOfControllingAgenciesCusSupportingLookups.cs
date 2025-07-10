using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class ExemptionOfControllingAgenciesCusSupportingLookups : CusSupportingInfoLookups
	{
		public ExemptionOfControllingAgenciesCusSupportingLookups(ExemptionOfControllingAgenciesCusSupporting parent)
			: base(parent)
		{
		}

		public IBusinessObjectCollection SpecialCodeList => ExemptionOfControllingAgenciesCusSupporting.GetSpecialCodeList(Factory);
	}
}
