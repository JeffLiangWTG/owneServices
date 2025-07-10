using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class CusPersonFetchStrategy : ASYCUDA.Business.CusPersonFetchStrategy
	{
		public CusPersonFetchStrategy(CusPerson person) : base(person)
		{
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(typeof(CusPersonCountry), CusPersonCountrySchema.CPC_CPN_Person, BusinessObject.PK);
		}
	}
}
