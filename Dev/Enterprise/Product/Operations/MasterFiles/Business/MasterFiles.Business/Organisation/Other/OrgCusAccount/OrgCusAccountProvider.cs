using System.Collections;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCusAccountProvider
	{
		public OrgCusAccountProvider(ZString countryCode)
		{
			CountryCode = countryCode;
		}

		public ZString CountryCode { get; }
		public static OrgCusAccountProvider GetByCountryCode(ZString countryCode)
		{
			OrgCusAccountProvider result = null;
			if (!countryCode.IsEmpty)
			{
				var types = ObjectFactory.Get<Hashtable>("OrgCusAccountProviders");
				var objectHandle = (ObjectHandle)types[countryCode.ToString()];
				result = (OrgCusAccountProvider)objectHandle?.GetObject();
			}
			if (result == null)
			{
				result = new OrgCusAccountProvider(countryCode);
			}
			return result;
		}
		public virtual OrgCusAccountValidation GetNewValidation(OrgCusAccount cusAccount) => new OrgCusAccountValidation(cusAccount);
		public virtual OrgCusAccountLookups GetNewLookups(OrgCusAccount cusAccount) => new OrgCusAccountLookups(cusAccount);
		public virtual void SetDefaultValues(OrgCusAccount cusAccount) { }
		public virtual ZString CZ_IssuerFieldType => nameof(FieldType.TextDropEdit);
		public virtual ZBool ShouldDefaultTypeWhenAble => false;
	}
}
