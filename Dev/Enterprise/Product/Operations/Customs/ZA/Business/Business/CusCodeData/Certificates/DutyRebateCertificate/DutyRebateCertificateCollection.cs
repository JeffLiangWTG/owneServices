using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class DutyRebateCertificateCollection : CusCodeDataCollection<DutyRebateCertificate>
	{
		public DutyRebateCertificateCollection(CusEntryInstruction master)
			: base(master, CusCodeDataTypeList.Codes.DRC)
		{
		}
	}
}
