using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class ExemptionOfControllingAgenciesCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<ExemptionOfControllingAgenciesCusSupporting>
	{
		public ExemptionOfControllingAgenciesCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.PermitExemptionCodes)
		{
		}

		protected override bool AllowNewCore => Count < MaxAllowed;

		public const int MaxAllowed = 5;
	}
}
