using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class AffirmationCodeCollection : Customs.Business.CusCodeDataCollection<AffirmationCode>
	{
		public AffirmationCodeCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.AffirmationCode)
		{
		}
	}
}
