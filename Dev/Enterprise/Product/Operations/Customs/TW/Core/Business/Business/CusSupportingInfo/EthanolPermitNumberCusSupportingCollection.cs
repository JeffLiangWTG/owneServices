using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class EthanolPermitNumberCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<EthanolPermitNumberCusSupporting>
	{
		public EthanolPermitNumberCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.EthanolPermitNumber)
		{
			MaxCountValidationEnable(maxAllowed);
		}

		protected override bool AllowNewCore => Count < maxAllowed;

		const int maxAllowed = 10;
	}
}
