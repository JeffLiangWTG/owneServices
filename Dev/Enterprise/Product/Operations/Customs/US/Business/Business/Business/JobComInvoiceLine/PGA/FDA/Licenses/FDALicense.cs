using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class FDALicense : AutoFDALicense, IFDALicense
	{
		public FDALicense(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IFDALicense Members

		ZString IFDALicense.Issuer { get { return "POV"; } }
		ZString IFDALicense.CountryCode { get { return US_CountryCode; } }
		ZString IFDALicense.StateCode { get { return US_StateCode; } }
		ZString IFDALicense.StateDescription { get { return US_StateDescription; } }
		ZString IFDALicense.Number { get { return US_Number; } }

		#endregion
	}
}
