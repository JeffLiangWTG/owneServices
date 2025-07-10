using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ACEAffirmationCodeCollection : Customs.Business.CusCodeDataCollection<ACEAffirmationCode>
	{
		public ACEAffirmationCodeCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.AffirmationCode)
		{
		}
	}
}
