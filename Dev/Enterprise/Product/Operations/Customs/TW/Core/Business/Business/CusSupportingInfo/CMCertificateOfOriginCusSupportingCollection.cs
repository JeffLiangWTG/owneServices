using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class CMCertificateOfOriginCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<CMCertificateOfOriginCusSupporting>
	{
		public CMCertificateOfOriginCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.CmCertificateOfOriginNumber)
		{
		}
	}
}
