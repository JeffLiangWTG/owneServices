using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CALicenceNumberCollection : Customs.Business.CusCodeDataCollection<CALicenceNumber>
	{
		public CALicenceNumberCollection(JobDeclaration declaration)
			: base(declaration, CusCodeDataTypeList.Codes.CALicenceNumber)
		{
			MaxCountValidationEnable(5);
		}
	}
}
