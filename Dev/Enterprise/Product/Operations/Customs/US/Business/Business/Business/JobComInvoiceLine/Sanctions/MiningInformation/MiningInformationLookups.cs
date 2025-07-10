using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class MiningInformationLookups : Customs.Business.CusCodeDataLookups
	{
		public MiningInformationLookups(MiningInformation parent)
			: base(parent)
		{
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}
	}
}
