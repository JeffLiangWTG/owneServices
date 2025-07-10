using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class PreviousBondedCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<PreviousBondedCusSupporting>
	{
		public PreviousBondedCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.PreviousBondedEntryNumber)
		{
		}
	}
}
