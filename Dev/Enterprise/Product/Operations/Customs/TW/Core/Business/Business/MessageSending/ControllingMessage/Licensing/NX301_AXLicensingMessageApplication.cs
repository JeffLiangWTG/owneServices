using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.Business;

namespace Enterprise.Customs.TW.Business
{
	public class NX301_AXLicensingMessageApplication : LicensingMessageApplication
	{
		public NX301_AXLicensingMessageApplication(CusTWControllingMessageHeader header, IAdditionalSupportingDocument addtionalDoc)
			: base(header, addtionalDoc)
		{
		}

		protected override ZString GetProvedPaperCore() => Header.TW1_ProofOfPaper ? YesNoList.Codes.Yes : YesNoList.Codes.No;
	}
}
