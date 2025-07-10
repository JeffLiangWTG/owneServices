using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class MedicalInstrumentOrFoodCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<MedicalInstrumentOrFoodCusSupporting>
	{
		public MedicalInstrumentOrFoodCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.MedicalInstrumentPartyIdentifier)
		{
		}
	}
}
