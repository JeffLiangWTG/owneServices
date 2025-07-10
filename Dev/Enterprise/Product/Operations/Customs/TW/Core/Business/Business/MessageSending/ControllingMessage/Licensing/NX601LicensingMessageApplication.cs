using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.Business;

namespace Enterprise.Customs.TW.Business
{
	public class NX601LicensingMessageApplication : LicensingMessageApplication
	{
		public NX601LicensingMessageApplication(CusTWControllingMessageHeader header, IAdditionalSupportingDocument addtionalDoc)
			: base(header, addtionalDoc)
		{
		}

		protected override ZString GetReturnSampleCore() => Header.TW1_ApplyForSampleReturn ? YesNoList.Codes.Yes : ZString.Empty;
	}
}
