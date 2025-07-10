using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class AviationFuelTypeCollection : Customs.Business.CusSupportingInfoCollection<AviationFuelType>
	{
		public AviationFuelTypeCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.AviationFuelType)
		{
		}
	}
}
