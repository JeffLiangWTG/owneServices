using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class FDALicenseAddInfoLookups : AutoFDALicenseAddInfoLookups
	{
		public FDALicenseAddInfoLookups(AutoFDALicenseAddInfo parent) : base(parent)
		{
		}

		protected new FDALicenseAddInfo Parent
		{
			get { return (FDALicenseAddInfo)base.Parent; }
		}

		protected FDALicense License
		{
			get { return Parent.Parent; }
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		public IBusinessObjectCollection StateList
		{
			get
			{
				IBusinessObjectCollection result = null;
				var licence = License;
				var country = licence == null ? null : licence.Country;
				if (country != null)
				{
					result = country.States;
				}
				return result ?? new RefCountryStatesCollection(Factory, ZQuery.NoResultQuery);
			}
		}
	}
}
