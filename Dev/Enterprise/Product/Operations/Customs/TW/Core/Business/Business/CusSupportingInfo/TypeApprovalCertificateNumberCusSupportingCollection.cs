using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class TypeApprovalCertificateNumberCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<TypeApprovalCertificateNumberCusSupporting>
	{
		public TypeApprovalCertificateNumberCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.TypeApprovalCertificateNumber)
		{
		}
	}
}
