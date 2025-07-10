using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class CusPersonCountry : ASYCUDA.Business.CusPersonCountry
		, Integration.Customs.ASYCUDA.ZAManifest.ICusPersonCountry
	{
		public CusPersonCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusPerson ParentPerson => (CusPerson)base.ParentPerson;
		public new CusPersonCountryLookups Lookups => (CusPersonCountryLookups)base.Lookups;
		protected override Customs.Business.CusPersonCountryLookups GetNewLookups() => new CusPersonCountryLookups(this);
	}
}
