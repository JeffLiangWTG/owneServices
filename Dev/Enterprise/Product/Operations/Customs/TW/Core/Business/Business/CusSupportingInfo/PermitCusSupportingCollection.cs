using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class PermitCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<PermitCusSupporting>
	{
		public PermitCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.PermitNumber)
		{
		}

		protected override bool AllowNewCore => Count < MaxAllowed;

		public const int MaxAllowed = 5;
	}
}
