using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class CertificateOfOriginCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<CertificateOfOriginCusSupporting>
	{
		public CertificateOfOriginCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.CertificateOfOriginNumber)
		{
		}
	}
}
