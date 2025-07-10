using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCusAccountCollection : DependentBusinessObjectCollection<OrgCusAccount, OrgHeader>
	{
		public OrgCusAccountCollection(OrgHeader parentOrganisation, string countryCode)
			: base(parentOrganisation, new ZQuery(OrgCusAccountSchema.CZ_RN_NKCountryCode, countryCode))
		{
			CountryCode = countryCode;
		}

		public string CountryCode { get; }

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var cusAccount = (OrgCusAccount)child;
			cusAccount.CZ_RN_NKCountryCode = CountryCode;
			cusAccount.Provider.SetDefaultValues(cusAccount);
		}

		public OrgCusAccount GetOrgCusAccountForCode(ZString code)
		{
			return this.Cast<OrgCusAccount>().FirstOrDefault(acc => acc.CZ_Code == code);
		}
	}
}
